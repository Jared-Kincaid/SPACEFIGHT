// Controller of overall game flow, menus, and game object placement.
// Jared Kincaid (jared.kincaid@digipen.edu)
// Summer 2014
// All content (C) 2014 DigiPen (USA) Corporation, all rights reserved

using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour 
{
	
	public AudioClip selectSound;
	public AudioClip victorySound;
	
	public AudioClip titleTrack;
	public AudioClip gameplayTrack;
	public AudioClip victoryTrack;

	public int numPlayers;
	public int numPlanets;

	public float sparseFactor;

	public SpriteRenderer shipPrefab;
	public SpriteRenderer planetPrefab;
	public GameObject digipenSplash;
	public GameObject titleScreen;
	public float turnTimerMax;
	public float graceTimerMax;

	int playerTurn;
	float turnTimer;
	float graceTimer;

	GameObject[] shipList;
	int shipsAlive;

	string currentGameState;
	string previousGameState;

	Color[] playerColors = new []
	{
		new Color(0.0f, 171.0f / 255.0f, 1.0f), // Blue
		new Color(0.0f, 1.0f, 86.0f / 255.0f), // Green
		new Color(1.0f, 1.0f, 0.0f), // Yellow
		new Color(1.0f, 177.0f / 255.0f, 0.0f), // Orange
		new Color(1.0f, 1.0f, 1.0f), // White
		new Color(0.5f, 0.5f, 0.5f), // Grey
		new Color(146.0f / 255.0f, 0.0f, 218.0f / 255.0f), // Purple
		new Color(1.0f, 0.0f, 1.0f) // Pink
	};

	Color[] planetColors = new []
	{
		new Color(208.0f / 255.0f, 147.0f / 255.0f, 70.0f / 255.0f), // Tan
		new Color(38.0f / 255.0f, 120.0f / 255.0f, 27.0f / 255.0f), // Green
		new Color(158.0f / 255.0f, 47.0f / 255.0f, 47.0f / 255.0f), // Red
		new Color(80.0f / 255.0f, 56.0f / 255.0f, 1.0f), // Blue
		new Color(161.0f / 255.0f, 77.0f / 255.0f, 1.0f / 255.0f), // Brown
		new Color(212.0f / 255.0f, 212.0f / 255.0f, 212.0f / 255.0f) // Grey
	};

	float volumeBackup;

	void PauseGame()
	{
		Time.timeScale = 0.0f;
		Camera.main.transform.position = Camera.main.transform.position + new Vector3 (100.0f, 100.0f, 0.0f);
		currentGameState = "Pause";
		Camera.main.GetComponent<CameraBehavior> ().PauseCamera (true);
		volumeBackup = AudioListener.volume;
		AudioListener.volume = volumeBackup * 0.5f;
		this.GetComponent<GUIText>().text = "";
	}

	void UnPauseGame()
	{
		Time.timeScale = 1.0f;
		Camera.main.transform.position = Camera.main.transform.position - new Vector3 (100.0f, 100.0f, 0.0f);
		currentGameState = "Game";
		Camera.main.GetComponent<CameraBehavior> ().PauseCamera (false);
		AudioListener.volume = volumeBackup;
	}

	void SpawnPlanets()
	{
		  // Clear All Existing Planets
		GameObject[] existingPlanetList = GameObject.FindGameObjectsWithTag("Planet");
		foreach(GameObject existingPlanet in existingPlanetList)
		{
			Destroy(existingPlanet);
		}

		int planetsPlaced = 0;
		Vector3 planetPos = new Vector3(0.0f, 0.0f, 0.0f);
		float planetScale = Random.Range(0.5f, 1.0f);
		Quaternion noRotation = new Quaternion(0.0f, 0.0f, 0.0f, 1.0f);

		  // Put first planet at origin
		do 
		{
			SpriteRenderer newPlanet = Instantiate(planetPrefab, planetPos, noRotation) as SpriteRenderer;
			newPlanet.color = planetColors[Random.Range(0, 5)];
			newPlanet.transform.localScale = new Vector3(planetScale, planetScale, 0.0f);
			planetsPlaced++;
			  // Until we've placed enough planets,
			GameObject[] planetList = GameObject.FindGameObjectsWithTag("Planet");
			Vector3 nextPlanetPos = planetPos;
			int triesLeft = 50;
			//int curPlanet = planetList.Length - 1;
			do
			{
				  // Find a random rotation, scale, and distance
				float randRot = Random.Range(0.0f, 360.0f) * Mathf.Deg2Rad;
				planetScale = Random.Range(0.5f, 1.0f);
				float randDis = newPlanet.transform.localScale.x * 2.56f + Random.Range(2.0f * sparseFactor, 10.0f * sparseFactor);
				nextPlanetPos.x += Mathf.Cos(randRot) * randDis;
				nextPlanetPos.y += Mathf.Sin(randRot) * randDis;
				  // If the new position is far enough away from other planets, place the new planet.
				bool failPlacement = false;
				for(int i = 0; i < planetList.Length; i++)
				{
					float minDistance = (planetList[i].transform.localScale.x + planetScale) * 2.56f + sparseFactor * (float)numPlanets;
					if((planetList[i].transform.position - nextPlanetPos).magnitude < minDistance)
					{
						failPlacement = true;
						break;
					}
				}
				  // If planet is too far from origin, try again.
				if(nextPlanetPos.magnitude > sparseFactor * 10.0f)
				{
					failPlacement = true;
				}
				if(failPlacement == false)
				{
					planetPos = nextPlanetPos;
					break;
				}
				  // If planet is badly placed, try again.
				triesLeft--;
			}while(triesLeft > 0);
			  // If we can't place the new planet in a reasonable number of tries, give up.
			if(triesLeft == 0)
			{
				numPlanets = planetsPlaced;
			}
		} while(planetsPlaced < numPlanets);
	}

	void SpawnPlayers()
	{
		  // Clear All Existing Players
		GameObject[] existingPlayersList = GameObject.FindGameObjectsWithTag("Ship");
		foreach(GameObject existingPlayer in existingPlayersList)
		{
			Destroy(existingPlayer);
		}

		// For each player, find a planet on which to spawn them, and spawn them there.
		GameObject[] planetList = GameObject.FindGameObjectsWithTag("Planet");
		int[] planetOccupied = new int[planetList.Length];
		for(int i = 0; i < numPlayers; ++i)
		{
			// Pick a planet without too many other players on it.
			int targetPlanet;
			do
			{
				targetPlanet = Random.Range(0, numPlanets - 1);
			}while(planetOccupied[targetPlanet] > Mathf.Ceil((float)numPlayers / (float)numPlanets));
			planetOccupied[targetPlanet]++;
			
			// Pick a random direction on that planet
			float planetAngle = Random.Range(0.0f, 360.0f) * Mathf.Deg2Rad;
			CircleCollider2D planetCollider = planetList[targetPlanet].GetComponent("CircleCollider2D") as CircleCollider2D;

			// Place player
			float planetDistance = planetList[targetPlanet].transform.localScale.x * planetCollider.radius;
			Vector3 playerPosition = planetList[targetPlanet].transform.position;
			playerPosition.x += Mathf.Cos(planetAngle) * planetDistance;
			playerPosition.y += Mathf.Sin(planetAngle) * planetDistance;
			Quaternion playerRotation = Quaternion.AngleAxis(planetAngle * Mathf.Rad2Deg - 90.0f, Vector3.forward);
			
			// Pick a random color for the player.
			SpriteRenderer newShip = Instantiate(shipPrefab, playerPosition, playerRotation) as SpriteRenderer;
			ShipBehavior shipBehavior = newShip.GetComponent<ShipBehavior>();
			shipBehavior.SetColor(playerColors[i]);
			newShip.color = playerColors[i];
			shipList[i] = newShip.gameObject;
		}
	}

	public void EndTurn()
	{
		  // End current turn
		if(shipList[playerTurn] != null)
		{
			ShipBehavior curPlayer = shipList[playerTurn].GetComponent(typeof(ShipBehavior)) as ShipBehavior;
			curPlayer.TurnStatus(false);
		}
		  // Next player
		do
		{
			playerTurn++;
			if(playerTurn >= numPlayers)
			{
				playerTurn = 0;
			}
		} while (shipList[playerTurn] == null);

		ShipBehavior nextPlayer = shipList[playerTurn].GetComponent(typeof(ShipBehavior)) as ShipBehavior;
		nextPlayer.TurnStatus(true);

		CameraBehavior cBehavior = (CameraBehavior)Camera.main.gameObject.GetComponent<CameraBehavior>();
		cBehavior.ZoomTo (shipList [playerTurn].transform.position);
		  // Reset Timer
		graceTimer = graceTimerMax;
		turnTimer = turnTimerMax;
	}

	bool confirmQuit;
	bool confirmNew;

	float splashTimer;
	GameObject splashScreenActual;
	GameObject titleScreenActual;

	void SplashState()
	{
		if(splashTimer > 0.0f)
		{
			splashTimer -= Time.deltaTime;
		}
		else
		{
			Destroy(splashScreenActual);
			currentGameState = "Title";
		}
		if (splashTimer < 2.0f && Input.GetButton("Fire1"))
		{
			splashTimer = 0.0f;
		}
	}

	void TitleState()
	{
		int screenBottom = Screen.height;
		int screenRight = Screen.width;
		int numButtons = 6;
		int bSpace = 10;
		int bWidth = screenRight / 4 - 2 * bSpace;
		int bHeight = (screenBottom - (numButtons + 1) * bSpace) / numButtons;
		int bPosX = (screenRight - bWidth) / 2;
		int bPosY = bSpace;
		int bHalfWidth = (bWidth - bSpace) / 2;
		int bHalfPosX = bPosX + bHalfWidth + bSpace;

		titleScreenActual.GetComponent<SpriteRenderer>().enabled = true;
		  // Title
		//GUI.Box( new Rect(bSpace, bSpace, screenRight - 2 * bSpace, 3 * bHeight + 2 * bSpace), "SPACEFIGHT");
		
		  // New Game
		bPosY += 3 * (bHeight + bSpace);
		if(GUI.Button(new Rect(bPosX, bPosY, bWidth, bHeight), "New Game"))
		{
			SoundManager.Call_SoundEffect (selectSound, 0.5f);
			confirmQuit = false;
			titleScreenActual.GetComponent<SpriteRenderer>().enabled = false;
			currentGameState = "GameSelect";
		}

		  // Options
		bPosY += bHeight + bSpace;
		if(GUI.Button(new Rect(bPosX, bPosY, bWidth, bHeight), "Options"))
		{
			SoundManager.Call_SoundEffect (selectSound, 0.5f);
			confirmQuit = false;
			titleScreenActual.GetComponent<SpriteRenderer>().enabled = false;
			previousGameState = currentGameState;
			currentGameState = "Options";
		}

		  // Quit
		  // Quit button checks for confirmation before quitting
		bPosY += bHeight + bSpace;
		if(confirmQuit == true)
		{
			if(GUI.Button(new Rect(bPosX, bPosY, bHalfWidth, bHeight), "Quit"))
			{
				SoundManager.Call_SoundEffect (selectSound, 0.5f);
				Application.Quit();
			}
			if(GUI.Button(new Rect(bHalfPosX, bPosY, bHalfWidth, bHeight), "Don't"))
			{
				SoundManager.Call_SoundEffect (selectSound, 0.5f);
				confirmQuit = false;
			}
		}
		else
		{
			if(GUI.Button(new Rect(bPosX, bPosY, bWidth, bHeight), "Quit Game"))
			{
				SoundManager.Call_SoundEffect (selectSound, 0.5f);
				confirmNew = false;
				confirmQuit = true;
			}
		}
	}

	void GameSelectState()
	{
		int screenBottom = Screen.height;
		int screenRight = Screen.width;
		int numButtons = 4;
		int bSpace = 10;
		int bWidth = screenRight / 4 - 2 * bSpace;
		int bHeight = (screenBottom - (numButtons + 1) * bSpace) / numButtons;
		int bPosX = (screenRight - bWidth) / 2;
		int bPosY = bSpace;
		
		  // Local Hotseat Game
		if(GUI.Button(new Rect(bPosX, bPosY, bWidth, bHeight), "Hotseat Game"))
		{
			SoundManager.Call_SoundEffect (selectSound, 0.5f);
			confirmQuit = false;
			titleScreenActual.GetComponent<SpriteRenderer>().enabled = false;
			currentGameState = "Setup";
		}
		
		  // Host Online Listen Server 
		bPosY += bHeight + bSpace;
		if(GUI.Button(new Rect(bPosX, bPosY, bWidth, bHeight), "Host Game"))
		{
			SoundManager.Call_SoundEffect (selectSound, 0.5f);
			previousGameState = currentGameState;
			currentGameState = "HostGame";
		}
		
		// Join Online Server
		bPosY += bHeight + bSpace;
		if(GUI.Button(new Rect(bPosX, bPosY, bWidth, bHeight), "Join Game"))
		{
			SoundManager.Call_SoundEffect (selectSound, 0.5f);
			previousGameState = currentGameState;
			currentGameState = "JoinGame";
		}

		  // Back
		bPosY += bHeight + bSpace;
		if(GUI.Button(new Rect(bPosX, bPosY, bWidth, bHeight), "Back"))
		{
			SoundManager.Call_SoundEffect (selectSound, 0.5f);
			currentGameState = "Title";
		}
	}

	void HostGameState()
	{
		//TODO: Host Server
	}

	void JoinGameState()
	{
		//TODO: Join Server
	}

	void SetupState()
	{
		int screenBottom = Screen.height;
		int screenRight = Screen.width;
		int numButtons = 5;
		int bSpace = 10;
		int bWidth = screenRight / 4 - 2 * bSpace;
		int bHeight = (screenBottom - (numButtons + 1) * bSpace) / numButtons;
		int bPosX = (screenRight - bWidth) / 2;
		int bPosY = bSpace;
		int bHalfWidth = (bWidth - bSpace) / 2;

		GUIStyle centerText = GUI.skin.GetStyle("Button");
		//centerText.alignment = TextAnchor.MiddleCenter;

		  // Number of Players
		string playersString = "Players\n";
		playersString += numPlayers.ToString();
		GUI.Box(new Rect(bPosX, bPosY, bWidth, bHeight), playersString, centerText);
		if(GUI.Button(new Rect(bPosX - bSpace - bHalfWidth, bPosY, bHalfWidth, bHeight), "<"))
		{
			SoundManager.Call_SoundEffect (selectSound, 0.5f);
			numPlayers--;
			if(numPlayers < 2)
			{
				numPlayers = 2;
			}
		}
		if(GUI.Button(new Rect(bPosX + bWidth + bSpace, bPosY, bHalfWidth, bHeight), ">"))
		{
			SoundManager.Call_SoundEffect (selectSound, 0.5f);
			numPlayers++;
			if(numPlayers > 8)
			{
				numPlayers = 8;
			}
		}

		  // Number of Planets
		bPosY += bHeight + bSpace;
		string planetsString = "Planets\n";
		planetsString += numPlanets.ToString();
		GUI.Box(new Rect(bPosX, bPosY, bWidth, bHeight), planetsString, centerText);
		if(GUI.Button(new Rect(bPosX - bSpace - bHalfWidth, bPosY, bHalfWidth, bHeight), "<"))
		{
			SoundManager.Call_SoundEffect (selectSound, 0.5f);
			numPlanets--;
			if(numPlanets < 1)
			{
				numPlanets = 1;
			}
		}
		if(GUI.Button(new Rect(bPosX + bWidth + bSpace, bPosY, bHalfWidth, bHeight), ">"))
		{
			SoundManager.Call_SoundEffect (selectSound, 0.5f);
			numPlanets++;
			if(numPlanets > 6)
			{
				numPlanets = 6;
			}
		}


		  // Turn Time Limit
		bPosY += bHeight + bSpace;
		string timerString = "Turn Time\n";
		timerString += turnTimerMax.ToString();
		timerString += " Seconds";
		GUI.Box(new Rect(bPosX, bPosY, bWidth, bHeight), timerString, centerText);
		if(GUI.Button(new Rect(bPosX - bSpace - bHalfWidth, bPosY, bHalfWidth, bHeight), "<"))
		{
			SoundManager.Call_SoundEffect (selectSound, 0.5f);
			turnTimerMax -= 5.0f;
			if(turnTimerMax < 15.0f)
			{
				turnTimerMax = 15.0f;
			}
		}
		if(GUI.Button(new Rect(bPosX + bWidth + bSpace, bPosY, bHalfWidth, bHeight), ">"))
		{
			SoundManager.Call_SoundEffect (selectSound, 0.5f);
			turnTimerMax += 5.0f;
			if(turnTimerMax > 60.0f)
			{
				turnTimerMax = 60.0f;
			}
		}

		  // Begin Game
		bPosY += bHeight + bSpace;
		if(GUI.Button(new Rect(bPosX, bPosY, bWidth, bHeight), "Begin Game"))
		{
			SoundManager.Call_SoundEffect (selectSound, 0.5f);
			this.GetComponent<AudioSource>().Stop();
			this.GetComponent<AudioSource>().loop = true;
			this.GetComponent<AudioSource>().PlayOneShot(gameplayTrack, this.GetComponent<AudioSource>().volume);

			InitGameState();
		}

		  // Back
		bPosY += bHeight + bSpace;
		if(GUI.Button(new Rect(bPosX, bPosY, bWidth, bHeight), "Back"))
		{
			SoundManager.Call_SoundEffect (selectSound, 0.5f);
			currentGameState = previousGameState;
		}
		
	}
	
	void OptionsState()
	{
		int screenBottom = Screen.height;
		int screenRight = Screen.width;
		int numButtons = 4;
		int bSpace = 10;
		int bWidth = screenRight / 4 - 2 * bSpace;
		int bHeight = (screenBottom - (numButtons + 1) * bSpace) / numButtons;
		int bPosX = (screenRight - bWidth) / 2;
		int bPosY = bSpace;

		  // Fullscreen Toggle
		string fullscreenStatus;
		if(Screen.fullScreen)
		{
			fullscreenStatus = "Go Windowed";
		}
		else
		{
			fullscreenStatus = "Go Fullscreen";
		}
		if(GUI.Button(new Rect(bPosX, bPosY, bWidth, bHeight), fullscreenStatus))
		{
			SoundManager.Call_SoundEffect (selectSound, 0.5f);
			Screen.fullScreen = !Screen.fullScreen;
		}
		  // Mute Music Only
		string musicStatus = "";
		bPosY += bHeight + bSpace;
		if(this.GetComponent<AudioSource>().volume != 0.0f)
		{
			musicStatus = "Mute Music";
		}
		else
		{
			musicStatus = "Unmute Music";
		}
		if(GUI.Button(new Rect(bPosX, bPosY, bWidth, bHeight), musicStatus))
		{
			if(this.GetComponent<AudioSource>().volume == 0.0f)
			{
				this.GetComponent<AudioSource>().volume = 0.7f;
			}
			else
			{
				this.GetComponent<AudioSource>().volume = 0.0f;
			}
			SoundManager.Call_SoundEffect (selectSound, 0.5f);
		}

		  // Mute All Sound
		string soundStatus = "";
		bPosY += bHeight + bSpace;
		if(AudioListener.volume != 0)
		{
			soundStatus = "Mute All Sound";
		}
		else
		{
			soundStatus = "Unmute All Sound";
		}
		if(GUI.Button(new Rect(bPosX, bPosY, bWidth, bHeight), soundStatus))
		{
			if(AudioListener.volume == 0.0f)
			{
				AudioListener.volume = 1.0f;
			}
			else
			{
				AudioListener.volume = 0.0f;
			}
			SoundManager.Call_SoundEffect (selectSound, 0.5f);
		}

		  // Back
		bPosY += bHeight + bSpace;
		if(GUI.Button(new Rect(bPosX, bPosY, bWidth, bHeight), "Back"))
		{
			SoundManager.Call_SoundEffect (selectSound, 0.5f);
			currentGameState = previousGameState;
		}
	}

	void GameState()
	{
		int screenRight = Screen.width;
		int buttonScale = Screen.height / 8;
		int buttonOffset = 10;

		Time.timeScale = 1.0f;

		  // Pause Button
		if(GUI.Button(new Rect(screenRight - buttonScale - buttonOffset, buttonOffset, buttonScale, buttonScale), "Pause")
		   || Input.GetKeyDown(KeyCode.Escape))
		{
			SoundManager.Call_SoundEffect (selectSound, 0.5f);
			PauseGame();
		}
	}

	void PauseState()
	{
		int screenBottom = Screen.height;
		int screenRight = Screen.width;
		int numButtons = 6;
		int bSpace = 10;
		int bWidth = screenRight / 4 - 2 * bSpace;
		int bHeight = (screenBottom - (numButtons + 1) * bSpace) / numButtons;
		int bPosX = (screenRight - bWidth) / 2;
		int bPosY = bSpace;
		int bHalfWidth = (bWidth - bSpace) / 2;
		int bHalfPosX = bPosX + bHalfWidth + bSpace;

		  // Resume Button goes back to game in progress
		if(GUI.Button(new Rect(bPosX, bPosY, bWidth, bHeight), "Resume Game") || Input.GetKeyDown(KeyCode.Escape))
		{
			SoundManager.Call_SoundEffect (selectSound, 0.5f);
			confirmNew = false;
			confirmQuit = false;
			UnPauseGame();
		}

		  // New Game
		bPosY += bHeight + bSpace;
		if(confirmNew == true)
		{
			if(GUI.Button(new Rect(bPosX, bPosY, bHalfWidth, bHeight), "Setup"))
			{
				SoundManager.Call_SoundEffect (selectSound, 0.5f);
				UnPauseGame();
				confirmNew = false;
				currentGameState = "Setup";
			}
			if(GUI.Button(new Rect(bHalfPosX, bPosY, bHalfWidth, bHeight), "Cancel"))
			{
				SoundManager.Call_SoundEffect (selectSound, 0.5f);
				confirmNew = false;
			}
		}
		else
		{
			if(GUI.Button(new Rect(bPosX, bPosY, bWidth, bHeight), "New Game"))
			{
				SoundManager.Call_SoundEffect (selectSound, 0.5f);
				confirmQuit = false;
				confirmNew = true;
			}
		}

		  // Options
		bPosY += bHeight + bSpace;
		if(GUI.Button(new Rect(bPosX, bPosY, bWidth, bHeight), "Options"))
		{
			SoundManager.Call_SoundEffect (selectSound, 0.5f);
			confirmQuit = false;
			confirmNew = false;
			previousGameState = currentGameState;
			currentGameState = "Options";
		}

		  // Credits
		bPosY += bHeight + bSpace;
		if(GUI.Button(new Rect(bPosX, bPosY, bWidth, bHeight), "Credits"))
		{
			SoundManager.Call_SoundEffect (selectSound, 0.5f);
			confirmQuit = false;
			confirmNew = false;
			currentGameState = "Credits";
		}

		  // How To Play
		bPosY += bHeight + bSpace;
		if(GUI.Button(new Rect(bPosX, bPosY, bWidth, bHeight), "How to Play"))
		{
			SoundManager.Call_SoundEffect (selectSound, 0.5f);
			confirmQuit = false;
			confirmNew = false;
			currentGameState = "HowToPlay";
		}


		  // Quit button checks for confirmation before quitting
		bPosY += bHeight + bSpace;
		if(confirmQuit == true)
		{
			if(GUI.Button(new Rect(bPosX, bPosY, bHalfWidth, bHeight), "Quit"))
			{
				SoundManager.Call_SoundEffect (selectSound, 0.5f);
				Application.Quit();
			}
			if(GUI.Button(new Rect(bHalfPosX, bPosY, bHalfWidth, bHeight), "Don't"))
			{
				SoundManager.Call_SoundEffect (selectSound, 0.5f);
				confirmQuit = false;
			}
		}
		else
		{
			if(GUI.Button(new Rect(bPosX, bPosY, bWidth, bHeight), "Quit"))
			{
				SoundManager.Call_SoundEffect (selectSound, 0.5f);
				confirmQuit = true;
				confirmNew = false;
			}
		}
	}

	void CreditsState()
	{
		int screenBottom = Screen.height;
		int screenRight = Screen.width;
		int numButtons = 6;
		int bSpace = 10;
		int bWidth = screenRight / 4 - 2 * bSpace;
		int bHeight = (screenBottom - (numButtons + 1) * bSpace) / numButtons;
		int bPosX = (screenRight - bWidth) / 2;
		int bPosY = bSpace;

		  // Credits Text/Image
		string creditsText = "All content © 2014 DigiPen (USA) Corporation, all rights reserved\n\n";
		creditsText += "PRESIDENT:  Claude Comair\n\n";
		creditsText += "INSTRUCTOR:  Jen Sward\n\n";
		creditsText += "PRODUCER:  Jared Kincaid\n\n";
		creditsText += "DESIGNER:  Jared Kincaid\n";
		creditsText += "ARTIST:  Jared Kincaid\n";
		creditsText += "PROGRAMMER: Jared Kincaid\n";
		GUI.Button(new Rect(bSpace, bPosY, screenRight - 2 * bSpace, 5 * bHeight + 4 * bSpace), creditsText);

		  // Back Button
		bPosY += 5 * (bHeight + bSpace);
		if(GUI.Button(new Rect(bPosX, bPosY, bWidth, bHeight), "Back"))
		{
			SoundManager.Call_SoundEffect (selectSound, 0.5f);
			currentGameState = "Pause";
		}
	}

	void HowToPlayState()
	{
		int screenBottom = Screen.height;
		int screenRight = Screen.width;
		int numButtons = 6;
		int bSpace = 10;
		int bWidth = screenRight / 4 - 2 * bSpace;
		int bHeight = (screenBottom - (numButtons + 1) * bSpace) / numButtons;
		int bPosX = (screenRight - bWidth) / 2;
		int bPosY = bSpace;

		  // How-to-play Text/Image
		string howToPlayText = "To aim, drag the crosshair to the angle at which you want to fire.\n";
		howToPlayText += "The further from your ship the crosshair is, the faster the shot will go.\n";
		howToPlayText += "When you think you're aiming in the right direction, press the \"Fire\" button to fire.\n\n";
		howToPlayText += "To fly, first click the \"Flight Mode\" button.\n";
		howToPlayText += "After that, touch and hold in the direction you want to fly.\n";
		howToPlayText += "Like the crosshair, touching further away from the ship will make it fly faster.\n\n";
		howToPlayText += "Beware flying too fast.  If you collide with a planet too fast, you will take damage!\n\n";
		howToPlayText += "Last player alive wins the game!\n";
		GUI.Button(new Rect(bSpace, bPosY, screenRight - 2 * bSpace, 5 * bHeight + 4 * bSpace), howToPlayText);

		  // Back Button
		bPosY += 5 * (bHeight + bSpace);
		if(GUI.Button(new Rect(bPosX, bPosY, bWidth, bHeight), "Back"))
		{
			SoundManager.Call_SoundEffect (selectSound, 0.5f);
			currentGameState = "Pause";
		}
	}

	void VictoryState()
	{
		int screenBottom = Screen.height;
		int screenRight = Screen.width;
		int numButtons = 6;
		int bSpace = 10;
		int bWidth = screenRight / 4 - 2 * bSpace;
		int bHeight = (screenBottom - (numButtons + 1) * bSpace) / numButtons;
		int bPosX = (screenRight - bWidth) / 2;
		int bPosY = bSpace;
		int bHalfWidth = (bWidth - bSpace) / 2;
		int bHalfPosX = bPosX + bHalfWidth + bSpace;

		Time.timeScale = 0.0f;

		int winningPlayer;
		for(winningPlayer = 0; winningPlayer < numPlayers; ++winningPlayer)
		{
			if(shipList[winningPlayer] != null)
			{
				break;
			}
		}

		  // Player X Wins!
		//GUI.Box( new Rect(bSpace, bSpace, screenRight - 2 * bSpace, 4 * bHeight + 3 * bSpace), "SOMEBODY WINS");
		this.GetComponent<GUIText>().fontSize = 80;
		this.GetComponent<GUIText>().color = playerColors[winningPlayer];
		string whoWon = "Player ";
		whoWon += (winningPlayer + 1).ToString();
		whoWon += " Wins!";
		this.GetComponent<GUIText>().text = whoWon;

		  // New Game
		bPosY += 4 * (bHeight + bSpace);
		if(confirmNew == true)
		{
			if(GUI.Button(new Rect(bPosX, bPosY, bHalfWidth, bHeight), "Setup"))
			{
				SoundManager.Call_SoundEffect (selectSound, 0.5f);
				confirmNew = false;
				currentGameState = "Setup";
			}
			if(GUI.Button(new Rect(bHalfPosX, bPosY, bHalfWidth, bHeight), "Cancel"))
			{
				SoundManager.Call_SoundEffect (selectSound, 0.5f);
				confirmNew = false;
			}
		}
		else
		{
			if(GUI.Button(new Rect(bPosX, bPosY, bWidth, bHeight), "New Game"))
			{
				SoundManager.Call_SoundEffect (selectSound, 0.5f);
				confirmQuit = false;
				confirmNew = true;
			}
		}

		  // Quit
		  // Quit button checks for confirmation before quitting
		bPosY += bHeight + bSpace;
		if(confirmQuit == true)
		{
			if(GUI.Button(new Rect(bPosX, bPosY, bHalfWidth, bHeight), "Quit"))
			{
				SoundManager.Call_SoundEffect (selectSound, 0.5f);
				Application.Quit();
			}
			if(GUI.Button(new Rect(bHalfPosX, bPosY, bHalfWidth, bHeight), "Don't"))
			{
				SoundManager.Call_SoundEffect (selectSound, 0.5f);
				confirmQuit = false;
			}
		}
		else
		{
			if(GUI.Button(new Rect(bPosX, bPosY, bWidth, bHeight), "Quit Game"))
			{
				SoundManager.Call_SoundEffect (selectSound, 0.5f);
				confirmNew = false;
				confirmQuit = true;
			}
		}
	}
	

	void OnGUI () 
	{

		// Display current Menu GUI
		if(currentGameState == "Splash")
		{
			SplashState();
		}
		if(currentGameState == "Title")
		{
			TitleState();
		}
		if(currentGameState == "GameSelect")
		{
			GameSelectState();
		}
		if(currentGameState == "HostGame")
		{
			currentGameState = "Setup";
		}
		if(currentGameState == "JoinGame")
		{
			currentGameState = "Setup";
		}
		if(currentGameState == "Setup")
		{
			SetupState();
		}
		if(currentGameState == "Options")
		{
			OptionsState();
		}
		if(currentGameState == "Game")
		{
			GameState();
		}
		if(currentGameState == "Pause")
		{
			PauseState();
		}
		if(currentGameState == "Credits")
		{
			CreditsState();
		}
		if(currentGameState == "HowToPlay")
		{
			HowToPlayState();
		}
		if(currentGameState == "Victory")
		{
			VictoryState();
		}
	}

	void InitGameState()
	{
		shipList = new GameObject[numPlayers];
		shipsAlive = numPlayers;
		
		// Spawn Planets
		SpawnPlanets ();
		// Spawn players
		SpawnPlayers ();
		
		// Start Timer
		turnTimer = 0.1f;
		graceTimer = 0.0f;
		playerTurn = numPlayers - 1;
		
		currentGameState = "Game";
	}

	// Use this for initialization
	void Start ()
	{
		splashTimer = 8.0f;
		currentGameState = "Splash";

		splashScreenActual = Instantiate(digipenSplash, new Vector3(0, 0, 0), new Quaternion()) as GameObject;
		SpriteRenderer splashRend = splashScreenActual.GetComponent<SpriteRenderer>();
		Vector3 newScale = new Vector3(1, 1, 1);
		float width = splashRend.sprite.bounds.size.x;
		float height = splashRend.sprite.bounds.size.y;
		float screenHeight = Camera.main.orthographicSize * 2.0f;
		float screenWidth = screenHeight / Screen.height * Screen.width;
		newScale.x = screenWidth / width;
		newScale.y = screenHeight / height * 4.0f / 3.0f;
		splashScreenActual.transform.localScale = newScale;

		titleScreenActual = Instantiate(titleScreen, new Vector3(0, 0, 0), new Quaternion()) as GameObject;
		SpriteRenderer titleRend = titleScreenActual.GetComponent<SpriteRenderer>();
		newScale = new Vector3(1, 1, 1);
		width = titleRend.sprite.bounds.size.x;
		height = titleRend.sprite.bounds.size.y;
		screenHeight = Camera.main.orthographicSize * 2.0f;
		screenWidth = screenHeight / Screen.height * Screen.width;
		newScale.x = screenWidth / width * 0.8f;
		newScale.y = screenHeight / height / 2.0f * 0.8f;
		titleScreenActual.transform.localScale = newScale;
		titleScreenActual.transform.position = new Vector3(0.0f, screenHeight / 4.0f, 0.0f);
		titleRend.enabled = false;

		this.GetComponent<AudioSource>().volume = 0.7f;
		this.GetComponent<AudioSource>().loop = true;
		this.GetComponent<AudioSource>().PlayOneShot(titleTrack, this.GetComponent<AudioSource> ().volume);

	}
	
	// Update is called once per frame
	void Update () 
	{
		if(currentGameState != "Game")
		{
			return;
		}

		if(shipList[playerTurn] == null)
		{
			EndTurn();
		}

		  // Update Timer
			  // If grace timer is not up or no input has been used, update grace timer instead
		if(graceTimer > 0.0f)
		{
			graceTimer -= Time.deltaTime;
			  // If player has done anything, kill the grace timer
			if(Input.GetButton("Fire1"))
			{
				graceTimer = 0.0f;
			}
		}
		  // If time's up, end current turn and move on to next player.
		else if(turnTimer > 0.0f)
		{
			turnTimer -= Time.deltaTime;
		}
		else
		{
			EndTurn ();
		}

		  // Check life status of players
		for(int i = 0; i < numPlayers; i++)
		{
			if(shipList[i] == null)
			{
				continue;
			}

			if(shipList[i].GetComponent<ShipBehavior>().IsDead())
			{
				shipList[i].GetComponent<ShipBehavior>().KillShip();
				shipsAlive--;
			}
		}

		  // ToDo: If only one player is left alive, game over!
		if(shipsAlive == 1)
		{
			SoundManager.Call_SoundEffect (victorySound, 0.5f);
			this.GetComponent<AudioSource>().Stop();
			this.GetComponent<AudioSource>().loop = false;
			this.GetComponent<AudioSource>().PlayOneShot(victoryTrack, this.GetComponent<AudioSource>().volume);

			currentGameState = "Victory";
		}


		  // Display time left
		string hudString = "";
		this.GetComponent<GUIText>().fontSize = 20;

		this.GetComponent<GUIText>().pixelOffset = new Vector2((float)Screen.width / 2.0f, (float)Screen.height - 15.0f);
		if(graceTimer > 0.0f)
		{
			hudString = "Player ";
			hudString += (playerTurn + 1).ToString();
			hudString += ", Get Ready!";
			this.GetComponent<GUIText>().text = hudString;
			this.GetComponent<GUIText>().color = playerColors[playerTurn];
		}
		else
		{
			hudString = "Time Left:  ";
			hudString += Mathf.Floor(turnTimer).ToString();
			hudString += ":";
			float hundredths = (turnTimer - Mathf.Floor(turnTimer)) * 100.0f;
			hudString += Mathf.Floor(hundredths).ToString();
			this.GetComponent<GUIText>().text = hudString;
			if(turnTimer > 5.0f)
			{
				this.GetComponent<GUIText>().color = playerColors[playerTurn];
			}
			else
			{
				this.GetComponent<GUIText>().color = new Color(1.0f, 0.0f, 0.0f);
			}
		}
	}
}
