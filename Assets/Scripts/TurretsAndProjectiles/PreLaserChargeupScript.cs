using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PreLaserChargeupScript : MonoBehaviour {

    private LineRenderer lr;
    private float currTime;

    public float chargetime;
    public float maxWidth;

    private Ray currentLaserRay;    //Will designate origin point and direction of the laser!
    private Vector3 hitPosition;

    public float maxRange;  // zero or -1 to indicate infinite range

    // Use this for initialization
    void Start () {
        lr = GetComponent<LineRenderer>();
        currTime = 0;
        lr.startWidth = 0.01f;
        lr.endWidth = 0.01f;
	}
	
	// Update is called once per frame
	void Update () {
        currTime += Time.deltaTime;
        float ratio = currTime / chargetime;
        float w = ratio * maxWidth;
        lr.startWidth = w;
        lr.endWidth = w;
    }

    public void setLaserRay(Vector3 origin, Vector3 direction) {
        currentLaserRay.origin = origin;
        currentLaserRay.direction = direction.normalized;
    }

    //We will perform the physics raycast and damage callback on FIXED update for damage consistency and for more accurate (?) physics positioning
    void FixedUpdate() {
        //Ray cast infinite distance..
        RaycastHit hitInfo;
        LayerMask mask = -1;
        bool rayCastRes = Physics.Raycast(currentLaserRay, out hitInfo, maxRange, mask, QueryTriggerInteraction.Ignore);

        if (rayCastRes) {
            //Hit something! we should set the 'end pos' for our laser to be the thing we hit!!
            hitPosition = hitInfo.point;
        }
        else {
            //Didn't hit anthing.. Set the end point to be 'maxRange' away from the start point in the ray direction!
            hitPosition = currentLaserRay.origin + currentLaserRay.direction * maxRange;    //Assumes the direction vector is normalised to length 1 (enforced in setter)
        }
        lr.SetPositions(new Vector3[] { currentLaserRay.origin, hitPosition });
    }
}
