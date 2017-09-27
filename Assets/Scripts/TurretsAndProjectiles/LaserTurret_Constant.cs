using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Behaviour for a simple laser turret which just tries to aim at the player, with a constant laser beam shooting out!
//All this script needs to to is SPAWN a laser and place it in the forward direction whenever the 'target' of the lookat script is within vision distance
//Despawn the laser if outside of the distance. The turret's 'lookat' script will handle target aiming/tracking.
public class LaserTurret_Constant : MonoBehaviour {

    public float visionDist;
    public LaserScript laserPrefab;
    private GameObject target;

    private bool laserIsActive;
    private LaserScript currentLaserInstance;

	// Use this for initialization
	void Start () {
        //Look at script should have run first according to project settings -> script execution order, thus the target should be in there.
        LookAtTarget lookScript = this.gameObject.GetComponent<LookAtTarget>();
        if (lookScript == null) {
            //The turret needs a lookAt script! Let's manually add it.
            lookScript = this.gameObject.AddComponent<LookAtTarget>();
        }
        //Okay, acquire the target from that scirpt so that we always shoot rockets which home on what we are looking at!
        target = lookScript.target;

        laserIsActive = false;
    }
	
	// Update is called once per frame
	void Update () {
		if (canSeeTarget()) {
            if (!laserIsActive) {
                //Can see the target but laser wasn't already active. We need to instantiate a laser!
                currentLaserInstance = Object.Instantiate(laserPrefab);
                currentLaserInstance.maxRange = visionDist;
                laserIsActive = true;
            }
            //Update the current laser instance.
            currentLaserInstance.setLaserRay(transform.position + transform.forward * 1, transform.forward);
        }
        else {
            if (laserIsActive) {
                //Destroy the currently active laser.
                Object.Destroy(currentLaserInstance);
                currentLaserInstance = null;
                laserIsActive = false;
            }
        }
	}

    private bool canSeeTarget() {
        return Vector3.Distance(transform.position, target.transform.position) <= visionDist;
    }
}
