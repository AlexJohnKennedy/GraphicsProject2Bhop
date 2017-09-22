using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/* This script is passed references to all other scripts or obects which contain 'statistics' that need to be
 * displayed on the player HUD. It is responsible for gathering this information and then updating fields with the information
 */
public class StatGatherer : MonoBehaviour {

    public AirBraking airBrakingObject;  //Contains stat for current 'air brake meter'
    public Rigidbody playerRigidbody;    //Can harvest velocity and movement information about the player from it's rigid body

    public Text velocityHudText;
    public Text airBrakeMeterText;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        //Simply update everything.
        updateAirBrakeMeterText();
        updateVelocityTextObject();	
	}

    private void updateAirBrakeMeterText() {
        int perc = (int)airBrakingObject.getMeterPercentage(); //Hard cast to floor value, since we don't want to display decimals..
        airBrakeMeterText.text = perc.ToString();
    }

    private void updateVelocityTextObject() {
        int vel = (int)getHorizontalVelocity();     //Hard cast to floor the value, since we don't want to display decimal places on the hud..
        velocityHudText.text = vel.ToString();
    }

    private float getHorizontalVelocity() {
        Vector3 raw = new Vector3(playerRigidbody.velocity.x, 0, playerRigidbody.velocity.z);

        return raw.magnitude;
    }
}
