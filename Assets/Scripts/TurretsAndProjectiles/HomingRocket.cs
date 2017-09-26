using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HomingRocket : MonoBehaviour {

    //Any homing rocket has a gameobject which it is following.
    public GameObject target;
    public GameObject explosionSourceObject;    //The object to clone in as an 'explosion' when we hit something!

    public GameObject spawnerObj;   //This object should be programically set by the spawning turret script to be the turret root object. 
                                    //We will NOT detect collisions with this until the rocket has reached a 1 unit distance away from it's source.

    public float startSpeed;
    public float maxSpeed;      //Rocket will keep accelerating up to this speed!
    public float acceleration;
    public float maxRadianRotationPerFixedUpdate;

    public float explosionDamage;    //Amount of damage applied by rocket explosion
    public float directHitDamage;   //Amount of directly applied damage if the rocket itself hits something, on top of explosion damage

    private float speed;
    private bool noCollideWithSpawner;

	// Use this for initialization
	void Start () {
        speed = startSpeed;

        //If we don't currently have a target, then try to acquire the player object in the world via tag.
        //If we find it, set the player as target! if not, just spawn an empty at (0,0,0) and go towards that..
        if (target == null) {
            target = GameObject.FindGameObjectWithTag("Player");

            if (target == null) {
                target = new GameObject("Empty target");
                target.transform.position = new Vector3(0, 0, 0);
            }
        }
        noCollideWithSpawner = true;
        //Debug.Log("Setting armed to FALSE in start method");
        //armed = false;
	}

	// Update is called once per frame
	void FixedUpdate () {
        moveTowardsTarget();
        //now we just need to accelerate the rocket if and only if we are still below the speed cap!
        if (speed < maxSpeed) {
            speed += acceleration * Time.fixedDeltaTime;
            if (speed > maxSpeed) speed = maxSpeed;
        }
        if (spawnerObj == null || (noCollideWithSpawner && Vector3.Distance(transform.position, spawnerObj.transform.position) > 2f)) {
            noCollideWithSpawner = false;
        } 
	}

    private void moveTowardsTarget() {
        Vector3 forward = this.transform.forward;
        Vector3 targetDirection = target.transform.position - this.transform.position; //Vector pointing from here to target.
        forward = Vector3.RotateTowards(forward, targetDirection, maxRadianRotationPerFixedUpdate, 0);
        this.transform.LookAt(this.transform.position + forward);
        this.transform.position += forward * speed * Time.fixedDeltaTime;
    }

    private bool checkForSpawningObject(GameObject hit, GameObject spawner) {
        //If the thing we hit is the spawner, or SOME parent of the spawner, then return true. Else false.
        while (spawner != null) {
            if (hit == spawner) {
                return true;
            }
            Transform par = spawner.transform.parent;
            if (par == null) {
                spawner = null;
            }
            else {
                spawner = par.gameObject;
            }
        }
        return false;
    }

    private void OnTriggerEnter(Collider other) {
        if (noCollideWithSpawner && checkForSpawningObject(other.gameObject, spawnerObj)) {
            return;     //Dont hit spawner if we aren't 'armed' yet so to speak (avoid instantly exploding)
        }

        //No matter what else we hit, we simply spawn an explosion system!
        GameObject explosion = Object.Instantiate(explosionSourceObject, transform.position, transform.rotation);  //spawn a clone of explosion object at the rocket's current pos and rotation.
        explosionScript script = explosion.GetComponent<explosionScript>();
        if (script != null) {
            //Okay, we instantiated a typical explosion object. Set the explosion damage based on what this homing rocket is set to.
            script.damage = this.explosionDamage;
        }

        //If we directly hit something with health, apply hit damamge as well! (thus, direct hits are more powerful)
        GameObject hit = other.gameObject;
        HealthScript applyDamage = hit.GetComponent<HealthScript>();
        if (applyDamage != null) {
            applyDamage.takeDamageEvent.Invoke(directHitDamage, this.gameObject);
        }

        //DESTROY THIS!
        Object.Destroy(this.gameObject);
    }

    //Can be used to manually specify a specific target by the creator gameobject.
    public void setTarget(GameObject target) {
        this.target = target;
    }
}
