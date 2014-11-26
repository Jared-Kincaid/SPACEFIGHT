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

	void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info)
	{
		Vector3 syncPosition = Vector3.zero;
		Vector3 syncVelocity = Vector3.zero;
		if (stream.isWriting)
		{
			syncPosition = rigidbody.position;
			stream.Serialize(ref syncPosition);
			
			syncVelocity = rigidbody.velocity;
			stream.Serialize(ref syncVelocity);
		}
	}
}
