using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class explosionScript : MonoBehaviour {

    public float startRadius;
    public float explosionDetectRadius;
    public float particleDampenForce;
    public float colliderExpandTime;      //How long it takes for the collider to expand to full size.
    public float destroyTime;             //How long the explosion lasts for

    public float damage;

    private SphereCollider sphereCollider;
    private float expandTimeLeft;
    private float timeLeft;
    private List<GameObject> alreadyHit;

	// Use this for initialization
	void Start () {
        sphereCollider = GetComponent<SphereCollider>();

        //Start radius for the collider is very small.
        sphereCollider.radius = startRadius;

        timeLeft = destroyTime;
        expandTimeLeft = colliderExpandTime;

        alreadyHit = new List<GameObject>();
	}
	
    //FixedUpdate for size growing and destorying
    void FixedUpdate() {
        expandTimeLeft -= Time.fixedDeltaTime;
        if (expandTimeLeft < 0) {
            expandTimeLeft = 0;
        }

        //Expand sphere collider radius based on the time elapsed..
        sphereCollider.radius = radiusAtTime(expandTimeLeft);
        //To make the physics engine respond to this collider, it needs to think it is 'moving'   
        this.transform.position = this.transform.position + Vector3.zero;   //Fake 'nudge' so the physics engine thinks it moved and responds to collisions
       
        timeLeft -= Time.fixedDeltaTime;
        if (timeLeft < 0) {
            //No time left. KILL this explosion object!
            Object.Destroy(this.gameObject);
        }
    }

    private float radiusAtTime(float t) {
        float elapsed = colliderExpandTime - expandTimeLeft;
        float ratio = elapsed / colliderExpandTime;

        return startRadius + ratio * (explosionDetectRadius - startRadius);
    }

    //When the explosion collider enters another collider, we should send DAMAGE events if and only if the other object we are colliding with
    //has the 'health' script attatched (and thus, can take damage).
    public void onTriggerEnter(Collider other) {
        GameObject hit = other.gameObject;
        if (!alreadyHit.Contains(hit)) {
            HealthScript applyDamage = hit.GetComponent<HealthScript>();

            if (applyDamage != null) {
                applyDamage.takeDamageEvent.Invoke(damage, this.gameObject);
            }

            //Okay, we don't want to hit this object more than once for the same explosion. Add it to the 'already hit' list
            alreadyHit.Add(hit);
        }
    }

    public void OnTriggerStay(Collider other) {
        //Just call the other event
        this.onTriggerEnter(other);
    }
}
