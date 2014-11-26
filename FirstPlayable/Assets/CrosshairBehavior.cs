using UnityEngine;
using System.Collections;

public class CrosshairBehavior : MonoBehaviour {

	// Use this for initialization
	void Start () 
	{
		  // These are the vars you need to change to set your own master server
//		Debug.Log (MasterServer.ipAddress.ToString());
//		Debug.Log (Network.natFacilitatorIP.ToString());
		
		SpriteRenderer curRenderer = this.GetComponent("SpriteRenderer") as SpriteRenderer;
		curRenderer.enabled = false;
	}
	
	// Update is called once per frame
	void Update () 
	{
		  // Match color to ship
		SpriteRenderer curRenderer = this.GetComponent("SpriteRenderer") as SpriteRenderer;
		SpriteRenderer parentRenderer = this.GetComponentInParent<SpriteRenderer>() as SpriteRenderer;
		curRenderer.color = parentRenderer.color;

		  // If mouse is pressed near the cursor, start dragging it around.
		Vector3 mousePos = Input.mousePosition;
		mousePos.z = 10;
		Vector3 worldMousePos = Camera.main.ScreenToWorldPoint(mousePos);
		if (Input.GetButton("Fire1") && (worldMousePos - this.transform.position).magnitude < 1.0f) 
		{
			  // Move cursor to mouse position.
			this.transform.position = worldMousePos;

			  // Make sure crosshair is within a reasonable distance of the ship.
			if((this.transform.parent.position - this.transform.position).magnitude > 3.0)
			{
				Vector3 relativePos = this.transform.position - this.transform.parent.position;
				relativePos.Normalize();
				relativePos.x *= 3.0f;
				relativePos.y *= 3.0f;
				relativePos.z *= 3.0f;
				this.transform.position = this.transform.parent.position + relativePos;
			}
		}
	}
}
