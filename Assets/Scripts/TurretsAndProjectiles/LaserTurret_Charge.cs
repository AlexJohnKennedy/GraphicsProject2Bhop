using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserTurret_Charge : MonoBehaviour {

    public float chargeTime;    //How long the turret has to 'charge' in a fixed position before activating the damaging laser.
    public float mainLaserTime;     //How long the damaging laser fires for in a fixed position after charging.
    public float waitTime;      //How long the turret waits in between firing beofre re-locking on and charging up again.
    public float visionDistance;
    public float laserDamage;

    public LaserScript mainLaserPrefab;
    public PreLaserChargeupScript chargeBeamPrefab;

    private LaserScript mainLaserInstance;
    private PreLaserChargeupScript chargeBeamInstance;

    private float timer;

    public GameObject target;

    private Vector3 lockOnPosition;
    private bool lockedOn;
    private int state;      //0 - waiting
                            //1 - charging
                            //2 - firing

	// Use this for initialization
	void Start () {
        if (target == null) {
            target = GameObject.FindGameObjectWithTag("Player");
            if (target == null) {
                throw new System.Exception("NEED ONE PLAYER OBJECT WITH TAG == \"Player\"!");
            }
        }
        state = 0;
        timer = waitTime;
        lockedOn = false;
    }
	
	// Update is called once per frame
	void Update () {
		//Decide behaviour based on current state.
        if (state == 0) {
            wait();
        }
        else if (state == 1) {
            if (!lockedOn) {
                tryToLockOn();
            }
            if (lockedOn) {
                //We are locked on, so just do charging behaviour
                charge();
            }
        }
        else {
            //Firing main laser
            fireMainLaser();
        }
	}

    private void fireMainLaser() {
        timer -= Time.deltaTime;
        if (timer <= 0) {
            destroyMainLaser();
            timer = waitTime;
            state = 0;
            lockedOn = false;
        }
    }

    private void charge() {
        //Just decrement timer. IF we reach zero, spawn fireBeam and update timers
        timer -= Time.deltaTime;
        if (timer <= 0) {
            //Set to firing state and fire laser
            spawnMainLaser();
            destroyChargeBeam();
            timer = mainLaserTime;
            state = 2;
            lockedOn = false;
        }
    }

    private void tryToLockOn() {
        lookAtPredictive();

        //If we can see the target, activate the 'charge' beam and update state flags
        if (canSeeTarget()) {
            spawnChargeBeam();
            lockedOn = true;
        }
    }

    private void spawnMainLaser() {
        mainLaserInstance = Object.Instantiate(mainLaserPrefab);
        mainLaserInstance.setLaserRay(transform.position + transform.forward * 1, transform.forward);
        mainLaserInstance.maxRange = visionDistance;
        mainLaserInstance.laserDamagePerTick = laserDamage;
    }
    private void destroyMainLaser() {
        Object.Destroy(mainLaserInstance.gameObject);
        mainLaserInstance = null;
    }

    private void spawnChargeBeam() {
        chargeBeamInstance = Object.Instantiate(chargeBeamPrefab);
        chargeBeamInstance.chargetime = chargeTime;
        chargeBeamInstance.setLaserRay(transform.position + transform.forward * 1, transform.forward);
        chargeBeamInstance.maxRange = visionDistance;
    }
    private void destroyChargeBeam() {
        Object.Destroy(chargeBeamInstance.gameObject);
        chargeBeamInstance = null;
    }

    private void wait() {
        //All we do is look at the target object and update timers.
        //Look where target WILL BE in one 'charge times' worth (plus a tiny bit)
        lookAtPredictive();

        //update timers.
        timer -= Time.deltaTime;
        if (timer <= 0) {
            //Switch to charge state
            state = 1;
            timer = chargeTime;
        }
    }

    private void lookAtPredictive() {
        Rigidbody rb = target.GetComponent<Rigidbody>();    //Target SHOULD have a rigidbody of this will break.

        Vector3 vel = rb.velocity;
        Vector3 pos = target.transform.position;

        lockOnPosition = pos + vel * (chargeTime + 0.05f);

        transform.LookAt(lockOnPosition);
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

    private void OnDisable() {
        if (mainLaserInstance != null) this.mainLaserInstance.gameObject.SetActive(false);
        if (chargeBeamInstance != null) this.chargeBeamInstance.gameObject.SetActive(false);
    }
}
