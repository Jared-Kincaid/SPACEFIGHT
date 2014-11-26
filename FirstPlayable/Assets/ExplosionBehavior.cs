// Behavior of projectile explosions.
// Jared Kincaid (jared.kincaid@digipen.edu)
// Summer 2014
// All content (C) 2014 DigiPen (USA) Corporation, all rights reserved

using UnityEngine;
using System.Collections;

public class ExplosionBehavior : MonoBehaviour 
{
	public AudioClip explosionSound;

	const float minScale = 0.0f;
	const float maxScale = 0.4f;
	const float explosionDamage = 40.0f;
	float scaleRate;

	// Use this for initialization
	void Start () 
	{
		  // Play boom sound here
		SoundManager.Call_SoundEffect (explosionSound, 0.5f);
		scaleRate = 0.6f;
	}
	
	// Update is called once per frame
	void Update () 
	{
		  // Grow each frame until upper limit
		  // Once upper limit is reached, shrink until lower limit, and then destroy self
		Vector3 newScale = this.transform.localScale;
		newScale.x += (scaleRate * Time.deltaTime);
		newScale.y = newScale.x;
		this.transform.localScale = newScale;
		if(this.transform.localScale.x >= maxScale)
		{
			scaleRate = -0.4f;
		}
		if(this.transform.localScale.x <= minScale)
		{
			Destroy(gameObject);
		}
	}

	void OnTriggerStay2D(Collider2D collisionInfo)
	{		
		// Check for collision with players, deal damage by amount time collided with them
		if(collisionInfo.gameObject.tag == "Ship")
		{
			ShipBehavior otherShip = (ShipBehavior)collisionInfo.gameObject.GetComponent(typeof(ShipBehavior));
			otherShip.ApplyDamage(explosionDamage * Time.deltaTime);
		}
	}
}
