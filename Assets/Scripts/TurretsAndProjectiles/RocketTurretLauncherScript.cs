using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RocketTurretLauncherScript : MonoBehaviour {

    private GameObject target;  //We will acquire this target from the 'look at' script.

    public bool overrideRocketParameters;   //Set to true if and only if we want this turret to have customized rocket properties for rockets that it shoots.
    public float rocketDirectDamage;
    public float rocketExplosionDamage;
    public float rocketStartSpeed;
    public float rocketMaxSpeed;
    public float rocketAcceleration;
    public float rocketRotationPerFixedUpdate;

    public float visionDistance;    //How far the turret can 'see' (used in raycast).
    public float fireRefreshTime;   //how long the turret must wait in between firing a rocket.
    public float initialFireDelay;  //How long the turret must INITIALLY wait before firing after 'seeing' the player.
    public bool canShootWhenLookingAway;    //if true, the turret can fire a rocket without being currently looking DIRECTLY at the player.
                                            //if false, the turrent must wait until it is looking RIGHT AT the player in order to 'lock on' and fire.
    public int maxConcurrentRockets;    //How many rockets can be airborne at the same time.

    public GameObject rocketPrefab;     //Object to instantiate when shooting, should be a homing rocket prefab. 
    private List<GameObject> activeRockets;

    private float currWaitTime;
    private float currVisionWaitTime;

	// Use this for initialization
	void Start () {
        LookAtTarget lookScript = this.gameObject.GetComponent<LookAtTarget>();
        if (lookScript == null) {
            //The turret needs a lookAt script! Let's manually add it.
            lookScript = this.gameObject.AddComponent<LookAtTarget>();
        }
        //Okay, acquire the target from that scirpt so that we always shoot rockets which home on what we are looking at!
        target = lookScript.target;

        activeRockets = new List<GameObject>();

        currVisionWaitTime = 0;
        currVisionWaitTime = initialFireDelay;
	}
	
	// Update is called once per frame
	void Update () {
        //Cull active rockets that have become inactive
        cullDeadRockets();

		//Update timers.
        if (currWaitTime > 0) {
            currWaitTime -= Time.deltaTime;
        }
        if (canSeeTarget()) {

            Debug.Log("CAN SEE TARGET: " + target.name);

            //We can see! let's count down our initial vision delay timer and fire if it's expired.
            if (currVisionWaitTime > 0) {
                //Need to wait still.
                currVisionWaitTime -= Time.deltaTime;
            }
            else {
                //Okay, no need to wait for the vision delay. If there is Also no overall cooldown, we can shoot!
                if (currWaitTime <= 0 && activeRockets.Count < maxConcurrentRockets) {
                    if (canShootWhenLookingAway || isLookingAtTarget()) {
                        shootRocket();
                    }
                }
            }
        }
        else {
            //We can't see the target, so reset the initial delay timer.
            currVisionWaitTime = initialFireDelay;
        }
	}

    private void shootRocket() {
        //This method assumes the conditions for firing a rocket have been met and simply fires one.
        //First, reset the firing timer
        currWaitTime = fireRefreshTime;

        //Now, spawn a rocket and make it target the target!
        GameObject newRocket = Object.Instantiate(rocketPrefab, transform.position + transform.forward * 0.5f, transform.rotation);

        HomingRocket homingScript = newRocket.GetComponent<HomingRocket>();
        if (homingScript != null) {
            homingScript.target = target;
            if (overrideRocketParameters) {
                homingScript.startSpeed = rocketStartSpeed;
                homingScript.acceleration = rocketAcceleration;
                homingScript.maxSpeed = rocketAcceleration;
                homingScript.directHitDamage = rocketDirectDamage;
                homingScript.explosionDamage = rocketExplosionDamage;
                homingScript.maxRadianRotationPerFixedUpdate = rocketRotationPerFixedUpdate;
            }
        }

        //Add to active rockets list so that we can keep track of how many rockets are active!
        activeRockets.Add(newRocket);
    }

    private bool canSeeTarget() {
        //Send a raycast from this object to target object. If it intercepts something else false, return false.
        //If it interects with the target, return true!
        Ray ray = new Ray(transform.position, target.transform.position - transform.position);
        RaycastHit hitInfo = new RaycastHit();
        LayerMask mask = -1;
        bool rayCastRes;
        if (visionDistance <= 0) {
            //Inifite vision
            rayCastRes = Physics.Raycast(ray, out hitInfo, Mathf.Infinity, mask, QueryTriggerInteraction.Ignore);
        }
        else {
            rayCastRes = Physics.Raycast(ray, out hitInfo, visionDistance, mask, QueryTriggerInteraction.Ignore);
        }

        //Determine wheather we hit the player!
        if (!rayCastRes) {
            //didnt' hit anything
            return false;
        }
        else {
            //We hit something! look at the hitinfo object to determine WHAT we hit!
            if (hitInfo.collider.gameObject == target) {
                //We hit the target!!
                return true;
            }
            //hit somehing that wasn't the target.
            return false;
        }
    }

    private bool isLookingAtTarget() {
        //Send a raycast from this object in direction of turrent orientation. If it intercepts something else false, return false.
        //If it interects with the target, return true!
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hitInfo = new RaycastHit();
        LayerMask mask = -1;
        bool rayCastRes;
        if (visionDistance <= 0) {
            //Inifite vision
            rayCastRes = Physics.Raycast(ray, out hitInfo, Mathf.Infinity, mask, QueryTriggerInteraction.Ignore);
        }
        else {
            rayCastRes = Physics.Raycast(ray, out hitInfo, visionDistance, mask, QueryTriggerInteraction.Ignore);
        }

        //Determine wheather we hit the player!
        if (!rayCastRes) {
            //didnt' hit anything
            return false;
        }
        else {
            //We hit something! look at the hitinfo object to determine WHAT we hit!
            if (hitInfo.collider.gameObject == target) {
                //We hit the target!!
                return true;
            }
            //hit somehing that wasn't the target.
            return false;
        }
    }

    private void cullDeadRockets() {
        for (int i = activeRockets.Count - 1; i >= 0; i--) {
            GameObject o = activeRockets[i];
            //NOTE: Unity engine overrides == operator on gameobjects such that o == null returns TRUE if the gameobject has been destroyed.
            if (o == null) {
                activeRockets.RemoveAt(i);
            }
        }
    }
}
