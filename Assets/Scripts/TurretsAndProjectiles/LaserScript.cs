using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserScript : MonoBehaviour {

    private Ray currentLaserRay;    //Will designate origin point and direction of the laser!
    private Vector3 hitPosition;

    private Vector2 texOffset;

    private ParticleSystem lineParticles;

    private LaserScript reflectionChild;

    public ParticleSystem impactParticles;
    public float offsetIncreaseRate;
    public float laserDamagePerTick;
    public float maxRange;  // zero or -1 to indicate infinite range
    public float ratePerUnitDistance;   //line particles emission rate

    public int numPossibleReflections;  //How many times this laser can 'reflect' off of relfective surfaces.

    //Audio source for the laser. This is tricky, since we cannot use the inbuilt spatialization since this entire object is suppossedely emitting sound.
    //What i will do is manually create a child gameobject which has it's own independent position. This object will serve as the audio source point for
    //the laser noise. Then, each update i will manually calculate the point along the laser line that is CLOSEST to the active audio listener in the scene, and then
    //explicilty place the audio source to emit from that point. THAT way, the laser shouldn't inconsistently emit sound from an arbitrary point in the middle of the laser line projection.
    private GameObject audioSourcePoint;

	// Use this for initialization
	void Start () {
        maxRange = 10000f;
        texOffset = new Vector2(0, 0);
        
        //DEBUG
        //setLaserRay(new Vector3(10, 1.2f, 0), new Vector3(0, 0, 1));

        lineParticles = GetComponent<ParticleSystem>();

        audioSourcePoint = this.gameObject.transform.GetChild(1).gameObject;

        reflectionChild = null;
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
        bool rayCastRes = Physics.Raycast(currentLaserRay, out hitInfo, maxRange, mask, QueryTriggerInteraction.Collide);   //Need to collide with triggers too, since the player sheild is a trigger.

        if (rayCastRes) {
            //Hit something! we should set the 'end pos' for our laser to be the thing we hit!!
            hitPosition = hitInfo.point;

            //Second, check if the thing we hit has health scripting attatched. If so, apply some damage! baby!
            HealthScript applyDamage = hitInfo.collider.GetComponent<HealthScript>();
            if (applyDamage != null) {
                applyDamage.takeDamageEvent.Invoke(laserDamagePerTick, this.gameObject);
            }

            //Third, we should manage our 'reflection'. If this laser can be reflected, AND we hit an object in the 'reflect' layer, THEN we should calculate the
            // raycast incident angle and spawn a reflection child laser prefab going in that direction.
            if (numPossibleReflections > 0 && (hitInfo.transform.gameObject.layer == LayerMask.NameToLayer("LaserReflective") || hitInfo.transform.gameObject.tag.Equals("Reflective"))) {
                Vector3 outgoingDirection = Vector3.Reflect(currentLaserRay.direction, hitInfo.normal);

                //Create a new laser object as our reflection child, if it is not already existing. If it is, we should just update it.
                if (reflectionChild == null) {
                    //SPAWN
                    GameObject obj = Object.Instantiate(this.gameObject);
                    reflectionChild = obj.GetComponent<LaserScript>();
                }
                //SET
                reflectionChild.setLaserRay(hitPosition, outgoingDirection);
                reflectionChild.numPossibleReflections = numPossibleReflections - 1;
            }
            else if (reflectionChild != null) {
                //No reflection available!! Lets make sure any reflection children are dead.
                Object.Destroy(reflectionChild.gameObject);
                reflectionChild = null;
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

        //Calculate the position that is closest along the line between the player/audio listener and the line.
        //then set the 'audio source' to be at that point. That way, the sound will always sound like it's coming from a point on the laser which is closest to the player!
        GameObject plr = GameObject.FindGameObjectWithTag("Player");
        if (plr == null) return;

        Vector3 playerListenerPoint = plr.transform.position;
        audioSourcePoint.transform.position = getPointOnLineSegClosestToOtherPoint(currentLaserRay.origin, hitPosition, playerListenerPoint);
    }

    private Vector3 getPointOnLineSegClosestToOtherPoint(Vector3 A, Vector3 B, Vector3 Point) {
        Vector3 AP = Point - A;       //Direcitonal vector going from start of line seg (A) to the goal point (Point)  
        Vector3 AB = B - A;           //Directional vector goign from start to end of the line segment (A -> B)  

        //X = k A + (1 - k) B
        //k_raw = (P - B).(A - B) / (A - B).(A - B)

        float numerator = Vector3.Dot(AP, AB);
        float denom = Vector3.Dot(AB, AB);

        if (denom == 0) {
            //CANT DIVIDE.
            return A;
        }

        float k = numerator / denom;

        if (k > 1) {
            k = 1;
        }
        else if (k < 0) {
            k = 0;
        }

        return (1-k) * A + (k) * B;
    }

    private void OnDisable() {
        this.OnDestroy();
    }

    private void OnDestroy() {
        if (reflectionChild != null) {
            Object.Destroy(reflectionChild);
        }
        if (lineParticles != null) {
            Object.Destroy(lineParticles.gameObject);
        }
    }

}
