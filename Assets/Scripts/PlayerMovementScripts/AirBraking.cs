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

    public float boostCost;
    public float boostSpeed;
    public float bounceDirectionSpeedScaleFactor;   //When we 'boost' while over the boost speed, we can redirect all our momentum instnatly. However, we suffer a speed penalty determined by this variable.

    public GameObject shieldInstance;
    public float shieldDistanceFromPlayerModel;

    private float airBrakeMeter;

    private Rigidbody rb;
    private static int LEFT_CLICK_ID  = 0;
    private static int RIGHT_CLICK_ID = 1;

    private float t;

	// Use this for initialization
	void Start () {
        rb = GetComponent<Rigidbody>();     //Get the rigid body
        if (airBrakeMeterCapactity <= 0) { airBrakeMeterCapactity = 100; }
        if (airBrakeMeterDrainSpeed <= 0) { airBrakeMeterDrainSpeed = 60; }
        if (airBrakeMeterRefillSpeed <= 0) { airBrakeMeterRefillSpeed = 25; }
        airBrakeMeter = airBrakeMeterCapactity;     //start with full meter, of course!

        shieldInstance.SetActive(false);

        t = 0;
    }

    private void FixedUpdate() {
        //Obtain the camera, since that is the thing pointing where we are looking. Use this to position the shield instance
        Camera childCam = this.transform.GetComponentInChildren<Camera>(false);
        Vector3 lookDir = childCam.transform.forward;

        //Shield object should be rotated to match the camera's orientation.
        shieldInstance.transform.rotation = (childCam.transform.rotation);
        shieldInstance.transform.Rotate(0, 90, 0);
        shieldInstance.transform.position = this.gameObject.transform.position + lookDir * shieldDistanceFromPlayerModel;

        t += Time.fixedDeltaTime;
        while (t > 1) {
            t -= 1;
        }

        shieldInstance.gameObject.GetComponent<MeshRenderer>().material.SetFloat("_ScrollOffset", t);
    }

    // Fixed update is called once per physics engine update (fixed interval)
    // Update is called once per rendered frame
    void Update () {
        manageInitialBoost();

        manageShieldState();
	}

    private void manageInitialBoost() {
        //If the player JUST CLICKED the left mouse button, attempt to do a 'boost' action.
        if (Input.GetMouseButtonDown(LEFT_CLICK_ID)) {
            if (airBrakeMeter >= boostCost) {
                airBrakeMeter -= boostCost;

                Vector3 prevVel = new Vector3(rb.velocity.x, rb.velocity.y, rb.velocity.z);
                prevVel.y = 0;
                float mag = prevVel.magnitude;

                if (mag * bounceDirectionSpeedScaleFactor <= boostSpeed) {
                    //If we are below the boost speed, just HARD SET horizontal velocity to be 'boostSpeed' in the direction we are facing.
                    Vector3 newVel = this.transform.forward * boostSpeed;
                    newVel.y = 0;

                    rb.velocity = newVel;
                }
                else {
                    //If we are above the boost speed, keep our current speed, scaled by some penalty amount, and instnalty redirct our momentum
                    //in the direction we are facing.
                    Vector3 newVel = this.transform.forward * mag * bounceDirectionSpeedScaleFactor;
                    newVel.y = 0;

                    rb.velocity = newVel;
                }
            }
        }
    }

    private void manageShieldState() {
        //If the player is holding the right mouse button, attempt to activate the shield object.
        if (Input.GetMouseButton(RIGHT_CLICK_ID)) {

            //Only allow the air brake if the air brake meter isn't empty!
            if (airBrakeMeter > 0.1) {
                shieldInstance.SetActive(true);
            }

            //Finally, Drain the meter, since the user is holding the air brake button!
            airBrakeMeter -= airBrakeMeterDrainSpeed * Time.deltaTime;
            if (airBrakeMeter < 0) {
                airBrakeMeter = 0;
                shieldInstance.SetActive(false);
            }
        }
        else {
            shieldInstance.SetActive(false);

            //Player is NOT pressing the air brake button! Refill their meter!
            airBrakeMeter += airBrakeMeterRefillSpeed * Time.deltaTime;
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
