using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*This class simply applies high resistence to the player character when they press the air brake button.
* Resistance force will be applied as through they were positioning a 'sail' in front of them: Thus, looking
* straight ahead, appiles fasted stopping and no curve. Looking 45 degrees from the velocity vector will push the player
* backwards in the direction they are looking, but at a vector component reduction.
*/
public class AirBraking : MonoBehaviour {

    public float resistAcceleration;
    public float airBrakeMeterCapactity;    //Maximum amount of airBrake 'Meter' we can have
    public float airBrakeMeterDrainSpeed;   //Amount to drain from the meter per second
    public float airBrakeMeterRefillSpeed;  //How quickly the meter recharges

    private float airBrakeMeter;

    private Rigidbody rb;
    private static int RIGHT_CLICK_ID = 1;

	// Use this for initialization
	void Start () {
        rb = GetComponent<Rigidbody>();     //Get the rigid body
        if (airBrakeMeterCapactity <= 0) { airBrakeMeterCapactity = 100; }
        if (airBrakeMeterDrainSpeed <= 0) { airBrakeMeterDrainSpeed = 60; }
        if (airBrakeMeterRefillSpeed <= 0) { airBrakeMeterRefillSpeed = 25; }
        airBrakeMeter = airBrakeMeterCapactity;     //start with full meter, of course!
    }
	
	// Fixed update is called once per physics engine update (fixed interval)
	void FixedUpdate () {
		//Simply apply resitance force appropriately if we are holding the right mouse button
        if (Input.GetMouseButton(RIGHT_CLICK_ID)) {

            //Only allow the air brake if the air brake meter isn't empty!
            if (airBrakeMeter > 0.1) {
                Vector3 prevVel = new Vector3(rb.velocity.x, rb.velocity.y, rb.velocity.z);     //Current velocity vector

                //Obtain the camera, since that is the thing pointing where we are looking
                Camera childCam = this.transform.GetComponentInChildren<Camera>(false);
                Vector3 lookDir = childCam.transform.forward;

                //Calculate what the resulting velocity of the rigid body should be after applying shield acceleration
                Vector3 newvelocity = applyWindForce(prevVel, lookDir);

                //Set rigid body to have the new velocity!
                rb.velocity = newvelocity;
            }

            //Finally, Drain the meter, since the user is holding the air brake button!
            airBrakeMeter -= airBrakeMeterDrainSpeed * Time.fixedDeltaTime;
            if (airBrakeMeter < 0) { airBrakeMeter = 0; }
        }
        else {
            //Player is NOT pressing the air brake button! Refill their meter!
            airBrakeMeter += airBrakeMeterRefillSpeed * Time.fixedDeltaTime;
            if (airBrakeMeter > airBrakeMeterCapactity) { airBrakeMeter = airBrakeMeterCapactity; }
        }
	}

    private Vector3 applyWindForce(Vector3 prevVel, Vector3 lookDir) {
        //Look dir should be a normalised vector in the direction we are looking.
        //We should apply 'acceleration' in the opposite direction, NORMAL to where we are looking (like a sail)
        //I.e acceleration direction = -lookDir

        //the magnitude of the acceleration should be relative to the dot product of the current velocity and look dir normal vector.
        //I.e FULL backwards acceleration when we are looking exactly in the direction we are travelling.

        float dotProd = Vector3.Dot(prevVel, lookDir);
        if (dotProd <= 0) {
            return prevVel;     //Apply no acceleration if the player is looking 'backwards' with respect to current velocity.
        }

        //Magnitude of velocity to add in the acceleration direction (acceleration * time). Also scaled by the proportion of airBrakeMeter that we have!
        float velocityMagnitudeToAdd = resistAcceleration * dotProd * Time.fixedDeltaTime * (airBrakeMeter / airBrakeMeterCapactity);

        //Velocity vector to add to the current veolcity vector is the magnitude * the direction unit vector..
        Vector3 resultingVelocity = prevVel + (-lookDir * velocityMagnitudeToAdd);

        return resultingVelocity;
    }

    public float getMeterPercentage() {
        //This is just the current amount / capacity normalized to be 100
        if (airBrakeMeterCapactity <= 0) return 0;  //Return zero for disabled air braking..
        return (airBrakeMeter / airBrakeMeterCapactity * 100);
    }
}
