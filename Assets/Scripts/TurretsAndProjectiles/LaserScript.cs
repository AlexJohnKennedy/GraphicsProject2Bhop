using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserScript : MonoBehaviour {

    private Ray currentLaserRay;    //Will designate origin point and direction of the laser!
    private Vector3 hitPosition;

    private Vector2 texOffset;

    private ParticleSystem lineParticles;

    public ParticleSystem impactParticles;
    public float offsetIncreaseRate;
    public float laserDamagePerTick;
    public float maxRange;  // zero or -1 to indicate infinite range
    public float ratePerUnitDistance;   //line particles emission rate

	// Use this for initialization
	void Start () {
        maxRange = 10000f;
        texOffset = new Vector2(0, 0);
        
        //DEBUG
        //setLaserRay(new Vector3(10, 1.2f, 0), new Vector3(0, 0, 1));

        lineParticles = GetComponent<ParticleSystem>();
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

            //Second, check if the thing we hit has health scripting attatched. If so, apply some damage! baby!
            Rigidbody rb = hitInfo.rigidbody;
            if (rb != null) {
                GameObject hit = hitInfo.rigidbody.gameObject;
                HealthScript applyDamage = hit.GetComponent<HealthScript>();
                if (applyDamage != null) {
                    applyDamage.takeDamageEvent.Invoke(laserDamagePerTick, this.gameObject);
                }
            }
        }
        else {
            //Didn't hit anthing.. Set the end point to be 'maxRange' away from the start point in the ray direction!
            hitPosition = currentLaserRay.origin + currentLaserRay.direction * maxRange;    //Assumes the direction vector is normalised to length 1 (enforced in setter)
        }
        
    }


    // Update is called once per frame
    // Will update the line renderer for this laser beam ever frame rather than every physics engine update.
    void Update () {
        //Acquire the laser 'line renderer'. the object thie script is attatched to should have one and only one line renderer to render the laser graphics.
        LineRenderer lr = GetComponent<LineRenderer>();

        lr.SetPositions(new Vector3[] {currentLaserRay.origin, hitPosition});

        //Next, we'll adjust the laser material offset to make it look like it's pulsing forwards.
        texOffset.x -= offsetIncreaseRate * Time.deltaTime;
        if (texOffset.x < 0) {
            texOffset.x += 1;
        }
        Material flickerMaterial = lr.materials[1];     //We assume that the flickery material is the SECOND material on the line renderer.
        flickerMaterial.SetTextureOffset("_MainTex", texOffset);

        //Align soft particle emission system to be in the center of the line renderer
        float x = (currentLaserRay.origin.x + hitPosition.x) / 2;
        float y = (currentLaserRay.origin.y + hitPosition.y) / 2;
        float z = (currentLaserRay.origin.z + hitPosition.z) / 2;
        lineParticles.transform.position = new Vector3(x, y, z);

        //Set particle system emmision rate to be consistent over a given distance
        float lineDistance = Vector3.Distance(currentLaserRay.origin, hitPosition);
        var linePartEmissionModule = lineParticles.emission;
        linePartEmissionModule.rateOverTimeMultiplier = ratePerUnitDistance * lineDistance;

        //Set particle system radius to align with line renderer length
        var shapeModule = lineParticles.shape;
        shapeModule.radius = lineDistance/2;

        //Rotate particle system to align with line renderer.
        Vector3 alignTo = hitPosition - currentLaserRay.origin;
        lineParticles.transform.rotation = Quaternion.FromToRotation(Vector3.right, alignTo);

        //Setup impact particles to appear at the impact point of this laser
        impactParticles.transform.position = hitPosition;
    }
}
