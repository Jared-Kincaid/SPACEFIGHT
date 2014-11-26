// Camera controls, including panning, tracking, zooming, etc.
// Jared Kincaid (jared.kincaid@digipen.edu)
// Summer 2014
// All content (C) 2014 DigiPen (USA) Corporation, all rights reserved

using UnityEngine;
using System.Collections;

public class CameraBehavior : MonoBehaviour 
{

	Vector3 zoomFromPos, zoomToPos;
	bool zooming;
	float lerpTime;
	bool paused;

	GameObject tracking;
	float trackingTimer;

	// Use this for initialization
	void Start () 
	{
	
	}

	  // Immediately stops tracking current trackign target.
	public void StopTracking()
	{
		ZoomTo(tracking.transform.position);
		trackingTimer = 0.0f;
		tracking = null;
	}

	public void TrackObject(GameObject objectToTrack)
	{
		tracking = objectToTrack;
		trackingTimer = 2.0f;
		ZoomTo(objectToTrack.transform.position);
	}

	public void ZoomTo(Vector3 zoomPos)
	{
		zoomFromPos = this.transform.position;
		zoomToPos = zoomPos;
		zoomToPos.z = this.transform.position.z;
		lerpTime = 0.0f;
		zooming = true;
	}

	public void PauseCamera(bool pauseStatus)
	{
		paused = pauseStatus;
	}

	// Update is called once per frame
	void Update () 
	{
		  // Don't move the camera when the game's paused!
		if(paused)
		{
			return;
		}

		  // Tracking an object overrides everything
		if (tracking != null) 
		{
			Vector3 trackedPos = tracking.transform.position;
			trackedPos.z = this.transform.position.z;
			this.transform.position = trackedPos;
			zoomFromPos = trackedPos;

			  // Let player skip tracking with input
			if(tracking.tag == "Projectile" && Input.GetButtonDown("Fire1"))
			{
				tracking = null;
				trackingTimer = 0.0f;
			}
			return;
		}

		if(trackingTimer > 0.0f)
		{
			trackingTimer -= Time.deltaTime;
			return;
		}
		  // Zooming to a location overrides player input
		if (zooming) 
		{
			lerpTime += Time.deltaTime;
			this.transform.position = Vector3.Lerp(zoomFromPos, zoomToPos, Mathf.SmoothStep(0.0f, 1.0f, lerpTime));
			if(lerpTime >= 1.0f)
			{
				zooming = false;
			}
			return;
		}

		const float orthographicSizeMin = 1.0f;
		const float orthographicSizeMax = 15.0f;
		const float zoomSpeed = 2.5f;
		float starfieldScale = Camera.main.orthographicSize;

		  // Mouse Wheel Zoom
		if (Input.GetAxis("Mouse ScrollWheel") != 0) // scroll down to zoom out
		{
			Camera.main.orthographicSize -= Input.GetAxis("Mouse ScrollWheel") * zoomSpeed;
		}
		  // Right-click and drag to move camera
		if(Input.GetMouseButton(1))
		{
			Vector3 newPosition = this.transform.position;
			newPosition.x -= Input.GetAxis("Mouse X");
			newPosition.y -= Input.GetAxis("Mouse Y");

			this.transform.position = newPosition;
		}

		  // If there are two touches on the device...
		if (Input.touchCount == 2)
		{
			  // Store both touches.
			Touch touchZero = Input.GetTouch(0);
			Touch touchOne = Input.GetTouch(1);
			
			  // Find the position in the previous frame of each touch.
			Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
			Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;
			
			  // Find the magnitude of the vector (the distance) between the touches in each frame.
			float prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
			float touchDeltaMag = (touchZero.position - touchOne.position).magnitude;
			
			  // Find the difference in the distances between each frame.
			float deltaMagnitudeDiff = prevTouchDeltaMag - touchDeltaMag;
			
			  // ... change the orthographic size based on the change in distance between the touches.
			camera.orthographicSize += deltaMagnitudeDiff * (zoomSpeed * 2.0f);
				
			  // Move camera by the average movement of the two touches.
			Vector2 touchMovement = (touchZero.deltaPosition + touchOne.deltaPosition);
			Vector3 newPosition = this.transform.position;
			newPosition.x += touchMovement.x;
			newPosition.y += touchMovement.y;
			this.transform.position = newPosition;
		}

		Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize, orthographicSizeMin, orthographicSizeMax );
		starfieldScale = Camera.main.orthographicSize / starfieldScale;

		this.transform.GetChild(0).transform.localScale *= starfieldScale;
	}
}
