// Ship Exhaust Controller
// Jared Kincaid (jared.kincaid@digipen.edu)
// Summer 2014
// All content (C) 2014 DigiPen (USA) Corporation, all rights reserved

using UnityEngine;
using System.Collections;

public class ExhaustBehavior : MonoBehaviour {


	float lifetime;
	const float baseLifetime = 1.5f;
	Vector2 originalVelocity;
	
	public AudioClip exhaustSound;

	// Use this for initialization
	void Start () 
	{
		lifetime = baseLifetime + Random.Range(-0.5f, 0.5f);
		originalVelocity = new Vector2 (0.0f, 0.0f);
		if(lifetime > baseLifetime)
		{
			SoundManager.Call_SoundEffect (exhaustSound, 0.3f);
		}
	}
	
	// Update is called once per frame
	void Update () 
	{
		if (originalVelocity.sqrMagnitude == 0.0f) 
		{
			originalVelocity = rigidbody2D.velocity;
		}
		lifetime -= Time.deltaTime;
		float colorVal = lifetime / baseLifetime;
		this.GetComponent<SpriteRenderer>().color = new Color(colorVal, colorVal, colorVal, colorVal);
		Vector2 velocityMod = (originalVelocity / (baseLifetime / 2.0f)) * Time.deltaTime;
		rigidbody2D.velocity = rigidbody2D.velocity - velocityMod;

		if(lifetime <= 0.0f)
		{
			Destroy(gameObject);
		}
	}
}
