using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HomingRocket : MonoBehaviour {

    //Any homing rocket has a gameobject which it is following.
    public GameObject target;
    public GameObject explosionSourceObject;    //The object to clone in as an 'explosion' when we hit something!

    public float startSpeed;
    public float maxSpeed;      //Rocket will keep accelerating up to this speed!
    public float acceleration;
    public float maxRadianRotationPerFixedUpdate;

    public float explosionDamage;    //Amount of damage applied by rocket explosion
    public float directHitDamage;   //Amount of directly applied damage if the rocket itself hits something, on top of explosion damage

    private float speed;

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
	}
	
	// Update is called once per frame
	void FixedUpdate () {
        moveTowardsTarget();
        //now we just need to accelerate the rocket if and only if we are still below the speed cap!
        if (speed < maxSpeed) {
            speed += acceleration * Time.fixedDeltaTime;
            if (speed > maxSpeed) speed = maxSpeed;
        }
	}

    private void moveTowardsTarget() {
        Vector3 forward = this.transform.forward;
        Vector3 targetDirection = target.transform.position - this.transform.position; //Vector pointing from here to target.
        forward = Vector3.RotateTowards(forward, targetDirection, maxRadianRotationPerFixedUpdate, 0);
        this.transform.LookAt(this.transform.position + forward);
        this.transform.position += forward * speed * Time.fixedDeltaTime;
    }

    private void OnTriggerEnter(Collider other) {
        //No matter what we hit, we simply spawn an explosion system!
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
