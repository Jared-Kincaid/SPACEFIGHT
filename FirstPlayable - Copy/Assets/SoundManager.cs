// Sound Effect player
// Jared Kincaid (jared.kincaid@digipen.edu)
// Summer 2014
// All content (C) 2014 DigiPen (USA) Corporation, all rights reserved

using UnityEngine;
using System.Collections;

public class SoundManager : MonoBehaviour 
{

	public delegate void event_sfx(AudioClip aud, float vol);
	public static event event_sfx SUB_sfx;


	public static void Call_SoundEffect (AudioClip aud, float vol)
	{
		SUB_sfx(aud, vol);
	}

	void SoundEffect (AudioClip aud, float vol)
	{
		GetComponent<AudioSource>().PlayOneShot(aud, vol);
	}

	void OnEnable()
	{
		SUB_sfx += SoundEffect;
	}

	void OnDisable()
	{
		SUB_sfx -= SoundEffect;
	}
}
