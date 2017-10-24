using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtTarget : MonoBehaviour {

    public float maxRadianRotationPerFixedUpdate;
    public float minRadianRoationPerFixedUpdate;
    public float maxRotationAngle;  //Degrees. if the target is at a higher angle than this, we will use max rotation speed (to 'catch up')
    public float minRotationAngle;  //Degrees. the angle at which we will use min rotation speed.

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
            float rotationSpeed;
            Vector3 targetDirection = target.transform.position - this.transform.position; //Vector pointing from here to target.
            float angle = Vector3.Angle(this.transform.forward, targetDirection);

            if (angle <= minRotationAngle) {
                rotationSpeed = minRadianRoationPerFixedUpdate;
            }
            else if (angle >= maxRotationAngle) {
                rotationSpeed = maxRadianRotationPerFixedUpdate;
            }
            else {
                float ratio = (angle - minRotationAngle) / (maxRotationAngle - minRotationAngle);
                rotationSpeed = minRadianRoationPerFixedUpdate + ratio * (maxRadianRotationPerFixedUpdate - minRadianRoationPerFixedUpdate);
            }

            Vector3 forward = this.transform.forward;
            forward = Vector3.RotateTowards(forward, targetDirection, rotationSpeed, 0);
            this.transform.LookAt(this.transform.position + forward);
        }
    }
}
