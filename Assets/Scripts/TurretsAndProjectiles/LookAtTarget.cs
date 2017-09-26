using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtTarget : MonoBehaviour {

    public float maxRadianRotationPerFixedUpdate;

    public GameObject target;

	// Use this for initialization
	void Start () {
        //If no target object was set in editor then just use player as target
        if (target == null) {
            target = GameObject.FindGameObjectWithTag("Player");
            if (target == null) {
                throw new System.Exception("NEED ONE PLAYER OBJECT WITH TAG == \"Player\"!");
            }
        }
	}
	
	// Update is called once per frame
	void Update () {
        if (maxRadianRotationPerFixedUpdate <= 0) {
            this.gameObject.transform.LookAt(target.transform.position);
        }
        else {
            Vector3 forward = this.transform.forward;
            Vector3 targetDirection = target.transform.position - this.transform.position; //Vector pointing from here to target.
            forward = Vector3.RotateTowards(forward, targetDirection, maxRadianRotationPerFixedUpdate, 0);
            this.transform.LookAt(this.transform.position + forward);
        }
    }
}
