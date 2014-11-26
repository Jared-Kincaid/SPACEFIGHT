// Ship Controller.  Includes player controls for flight and shooting.
// Jared Kincaid (jared.kincaid@digipen.edu)
// Summer 2014
// All content (C) 2014 DigiPen (USA) Corporation, all rights reserved

using UnityEngine;
using System.Collections;

public class ShipBehavior : MonoBehaviour {

	bool flightMode;
	bool currentTurn;
	public Rigidbody2D projectilePrefab;
	public Rigidbody2D exhaustPrefab;


	Color thisColor;

	float health;
	const float maxHealth = 100.0f;
	bool takingDamage;

	const float landingDistance = 1.0f;
	const float tooFast = 3.0f;
	const float impactDamage = 30.0f;
	const float tooSlow = 0.5f;

	float exhaustTimer;
	const float thrustForce = 5.0f;
	const float exhaustFrequency = 1.0f / 40.0f;

	public void SetColor(Color newColor)
	{
		thisColor = newColor;
	}

	public void KillShip()
	{
		  // Spawn explosions and destroy self.
		Destroy(gameObject);
	}

	public bool IsDead()
	{
		return health < 0.0f;
	}

	public void TurnStatus(bool turnStatus)
	{
		currentTurn = turnStatus;
		SpriteRenderer[] crosshairSprite = GetComponentsInChildren<SpriteRenderer>();
		foreach(SpriteRenderer sRenderer in crosshairSprite)
		{
			if(sRenderer.tag == "Crosshair")
			{
				SpriteRenderer curRenderer = this.GetComponent<SpriteRenderer>();
				sRenderer.enabled = turnStatus;
				sRenderer.color = curRenderer.color;
			}
		}
	}

	public void ApplyDamage(float damageAmount)
	{	
		health -= damageAmount;
		takingDamage = true;
	}

	// Use this for initialization
	void Start () 
	{
		currentTurn = false;
		flightMode = false;
		takingDamage = false;
		health = maxHealth;
		exhaustTimer = exhaustFrequency;
	}
	
	// Update is called once per frame
	void OnGUI () 
	{

		if(currentTurn == false || Time.timeScale == 0.0f)
		{
			return;
		}

//		Vector3 shipPos = Camera.main.WorldToScreenPoint(this.transform.position);
		int screenBottom = Screen.height;
		int screenRight = Screen.width;
		int buttonScale = Screen.height / 5;
		int buttonOffset = 10;
//		shipPos.z = 10.0f;
//		shipPos.y = Screen.height - shipPos.y;
//		float buttonOffsetX = 150;
		if (flightMode) 
		{
			  // Shoot Mode
			if(GUI.Button(new Rect(buttonOffset, screenBottom - buttonScale - buttonOffset, buttonScale, buttonScale), "Shoot Mode"))
			{
				flightMode = false;
				Camera.main.GetComponent<CameraBehavior>().StopTracking();
				SpriteRenderer[] crosshairSprite = GetComponentsInChildren<SpriteRenderer>();
				foreach(SpriteRenderer sRenderer in crosshairSprite)
				{
					if(sRenderer.tag == "Crosshair")
					{
						sRenderer.enabled = true;
					}
				}
			}
		}
		else
		{
			  // Flight Mode
			if(GUI.Button(new Rect(buttonOffset, screenBottom - buttonScale - buttonOffset, buttonScale, buttonScale), "Flight Mode"))
			{
				flightMode = true;
				Camera.main.GetComponent<CameraBehavior>().TrackObject(gameObject);
				SpriteRenderer[] crosshairSprite = GetComponentsInChildren<SpriteRenderer>();
				foreach(SpriteRenderer sRenderer in crosshairSprite)
				{
					if(sRenderer.tag == "Crosshair")
					{
						sRenderer.enabled = false;
					}
				}
			}
			  // Fire!
			if(GUI.Button(new Rect(screenRight - buttonScale - buttonOffset, screenBottom - buttonScale - buttonOffset, buttonScale, buttonScale), "Fire!"))
			{
				Rigidbody2D projectile = Instantiate(projectilePrefab, this.transform.position, this.transform.rotation) as Rigidbody2D;
				Physics2D.IgnoreCollision(this.collider2D, projectile.collider2D);
				CameraBehavior camBehavior = Camera.main.GetComponent<CameraBehavior>();
				camBehavior.TrackObject(projectile.gameObject);
				Transform[] childTransforms = GetComponentsInChildren<Transform>();
				foreach(Transform childTransform in childTransforms)
				{
					if(childTransform.tag == "Crosshair")
					{
						projectile.velocity = 3.5f * (childTransform.position - this.transform.position);
					}
				}
			}
		}
	}

	void OnCollisionEnter2D(Collision2D collisionInfo)
	{
		// If the projectile hits a planet, destroy it.
		if(collisionInfo.gameObject.tag == "Planet")
		{
			float howFast = collisionInfo.relativeVelocity.magnitude;
			if(howFast > tooFast)
			{
				ApplyDamage((howFast - tooFast) * impactDamage);
			}

			if(rigidbody2D.velocity.magnitude < tooSlow)
			{
				rigidbody2D.velocity = new Vector2(0.0f, 0.0f);
			}
		}
	}

	void Update ()
	{

		GameObject nearPlanet = null;
		float nearestPlanetDistance = 1000.0f;
		  // Change color if under attack
		if (takingDamage) 
		{
			this.GetComponent<SpriteRenderer>().color = new Color(1.0f, 0.0f, 0.0f);
			takingDamage = false;
		}
		else
		{
			this.GetComponent<SpriteRenderer>().color = thisColor;
		}

		  // Gravity
		GameObject[] planetList = GameObject.FindGameObjectsWithTag("Planet");
		foreach(GameObject planet in planetList)
		{
			Vector3 gravDirection = planet.transform.position - this.transform.position;
			float gravDistance = gravDirection.magnitude;
			gravDirection.Normalize();
			// Mass by volume
			float planetRadius = planet.transform.localScale.x * planet.GetComponent<CircleCollider2D>().radius;
			float planetMass = Mathf.PI * (4.0f / 3.0f) * planetRadius * planetRadius * planetRadius;
			float shipMass = this.GetComponent<Rigidbody2D>().mass;

			  // Find closest planet
			float distanceToPlanet = gravDistance - planetRadius;
			if(distanceToPlanet < nearestPlanetDistance)
			{
				nearestPlanetDistance = distanceToPlanet;
				nearPlanet = planet;
			}

			rigidbody2D.AddForce(gravDirection * planetMass * shipMass / (gravDistance * gravDistance * gravDistance));
		}

		  // Check to see if ship is within landing distance
		if(nearestPlanetDistance > landingDistance)
		{
			nearPlanet = null;
		}

		  // Nudge player if they get too far away from level
		if (this.transform.position.magnitude > 50.0f) 
		{
			this.GetComponent<Rigidbody2D>().AddForce(this.transform.position);
		}

		  // Don't let this ship be controlled when it's not their turn!
		if (currentTurn == false) 
		{
			return;
		}


		if(flightMode)
		{
			exhaustTimer -= Time.deltaTime;
			if (Input.GetButton("Fire1"))// && mouseDir.magnitude < 3.0f) 
			{
				  // If ship is near planet, check for good angle.
				  // If angle is bad, bounce ship away, otherwise align ship to planet surface.
				Vector3 mousePos = Input.mousePosition;
				mousePos.z = 10.0f;
				Vector3 shipDir = Camera.main.ScreenToWorldPoint(mousePos);
				if(nearPlanet == null)
				{
					shipDir = shipDir - this.transform.position;
					shipDir.Normalize();
				}
				else
				{
					shipDir = this.transform.position - nearPlanet.transform.position;
					shipDir.Normalize();
				}
				float angle = Mathf.Atan2(shipDir.y, shipDir.x) * Mathf.Rad2Deg - 90.0f;
				this.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
				rigidbody2D.AddForce(shipDir * thrustForce);

				  // While in flight, generate exhaust particles.
				if(exhaustTimer <= 0.0f)
				{
					Rigidbody2D exhaustParticle = Instantiate(exhaustPrefab, this.transform.position - (shipDir * 0.5f), this.transform.rotation) as Rigidbody2D;
					float randAngle = Random.Range(-30.0f, 30.0f);
					Quaternion randRotation = Quaternion.AngleAxis(randAngle, Vector3.forward);
					exhaustParticle.velocity = randRotation * -shipDir;
					exhaustTimer = exhaustFrequency;
				}
			}
		}

		  // If ship is near a planet and within landing angle tolerances, autocorrect angle.
		if(nearPlanet != null)
		{
			Vector3 nearPlanetHeading = this.transform.position - nearPlanet.transform.position;
			nearPlanetHeading.Normalize();
			float nearPlanetAngle = Mathf.Atan2(nearPlanetHeading.y, nearPlanetHeading.x) * Mathf.Rad2Deg - 90.0f;
			this.transform.rotation = Quaternion.AngleAxis(nearPlanetAngle, Vector3.forward);
			if(this.rigidbody2D.velocity.magnitude < 0.2f)
			{
				this.rigidbody2D.velocity = new Vector2(0.0f, 0.0f);
			}
		}
	}

}
