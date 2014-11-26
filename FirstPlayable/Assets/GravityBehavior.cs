using UnityEngine;
using System.Collections;

public class GravityBehavior : MonoBehaviour 
{

	// Use this for initialization
	void Start () 
	{
	
	}

	  // Apply gravity to applicable objects
	void Gravitate(GameObject[] objectList, GameObject[] planetList)
	{
		foreach(GameObject curObject in objectList)
		{
			if(curObject == null)
			{
				continue;
			}

			  // Ships should be a little floatier than bullets.
			float objAccel = 7.0f;
			if(curObject.tag == "Ship")
			{
				objAccel = 3.0f;
			}

			float minDistance = 500000.0f;
			Vector3 finalGrav = new Vector3(0.0f, 0.0f, 0.0f);
			float numPlanets = 0.0f;
			float objectMass = curObject.GetComponent<Rigidbody2D>().mass;
			foreach(GameObject curPlanet in planetList)
			{
				numPlanets += 1.0f;
				Vector3 gravDirection = curPlanet.transform.position - curObject.transform.position;
				float gravDistance = gravDirection.magnitude;
				gravDirection.Normalize();
				  // Get radius as a function of collider radius and uniform object scale
				float planetRadius = curPlanet.transform.localScale.x * curPlanet.GetComponent<CircleCollider2D>().radius;
				gravDistance = gravDistance - planetRadius;
				
				  // Find closest planet to object
				if(minDistance > gravDistance)
				{
					minDistance = gravDistance - planetRadius;
					//gravDistance += 1;
					//float planetMass = 8.0f;//Mathf.PI * (4.0f / 3.0f) * planetRadius * planetRadius * planetRadius;
					//finalGrav = (gravDirection * planetMass * projectileMass) / Mathf.Max(gravDistance, 1.0f);
					//finalGrav += gravDirection * projectileMass;
					finalGrav = gravDirection * objectMass * objAccel;
				}
			}

			  // Use gravity from closest planet only
			curObject.rigidbody2D.AddForce(finalGrav);
		}
	}

	// Update is called once per frame
	void Update () 
	{
		// Only do this on server
		if (Network.isClient) 
		{
			return;
		}

		GameObject[] planetList = GameObject.FindGameObjectsWithTag("Planet");
		GameObject[] shipList = GameObject.FindGameObjectsWithTag("Ship");
		GameObject[] projectileList = GameObject.FindGameObjectsWithTag("Projectile");

		Gravitate(shipList, planetList);
		Gravitate (projectileList, planetList);
	}
}
