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

    private float speed;

	// Use this for initialization
	void Start () {
        speed = startSpeed;
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
        Object.Instantiate(explosionSourceObject, transform.position, transform.rotation);  //spawn a clone of explosion object at the rocket's current pos and rotation.

        //DESTROY THIS!
        Object.Destroy(this.gameObject);
    }
}
