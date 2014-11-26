// Projectile Controller.
// Jared Kincaid (jared.kincaid@digipen.edu)
// Summer 2014
// All content (C) 2014 DigiPen (USA) Corporation, all rights reserved

using UnityEngine;
using System.Collections;

public class ProjectileBehavior : MonoBehaviour 
{
	
	public AudioClip shootSound;

	public Collider2D explosionPrefab;
	float lifeTime;

	void Explode ()
	{
		Instantiate(explosionPrefab, this.transform.position, this.transform.rotation);
		Destroy(gameObject);
	}

	// Use this for initialization
	void Start () 
	{
		  // Play fire sound
		SoundManager.Call_SoundEffect(shootSound, 0.5f);
		lifeTime = 10.0f;
	}
	
	// Update is called once per frame
	void Update () 
	{
		GameObject[] planetList = GameObject.FindGameObjectsWithTag("Planet");
		float minDistance = 500000.0f;
		Vector3 finalGrav = new Vector3(0.0f, 0.0f, 0.0f);
		float numPlanets = 0.0f;
		float projectileMass = this.GetComponent<Rigidbody2D>().mass;
		foreach(GameObject planet in planetList)
		{
			numPlanets += 1.0f;
			Vector3 gravDirection = planet.transform.position - this.transform.position;
			float gravDistance = gravDirection.magnitude;
			gravDirection.Normalize();
			  // Get radius as a function of collider radius and uniform object scale
			float planetRadius = planet.transform.localScale.x * planet.GetComponent<CircleCollider2D>().radius;
			gravDistance = gravDistance - planetRadius;

			  // Find closest planet to object
			if(minDistance > gravDistance)
			{
				minDistance = gravDistance - planetRadius;
				gravDistance += 1;
				float planetMass = 8.0f;//Mathf.PI * (4.0f / 3.0f) * planetRadius * planetRadius * planetRadius;
				finalGrav = (gravDirection * planetMass * projectileMass) / Mathf.Max(gravDistance, 1.0f);
				//finalGrav += gravDirection * projectileMass;
			}
		}
		  // Use gravity from closest planet only
		rigidbody2D.AddForce(finalGrav);

		lifeTime -= Time.deltaTime;
		if(lifeTime <= 0.0f)
		{
			Explode ();
		}
	}

	void OnTriggerEnter2D(Collider2D collisionInfo)
	{
		  // If the projectile hits a planet, destroy it.
		if(collisionInfo.gameObject.tag == "Planet" || collisionInfo.gameObject.tag == "Ship")
		{
			Explode();
		}
	}
}
