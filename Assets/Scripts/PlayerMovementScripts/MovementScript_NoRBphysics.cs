using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* This script is used to proved WASD movement to an object, if and only if it is currently colliding with a floor trigger.
 * Requires that the gameObject has a rigid body physics component, since movement will be achieved through applying force to this object.
 * Will perform a trigger check to see if we are touching the floor.
 */
public class MovementScript_NoRBphysics : MonoBehaviour {

    public float runningForce;     //Affects movement speed via changing how much force is applied by WASD keys
    public float distanceToCheckGround;
    public float jumpVelocity;

    public float airAcceleration;
    public float maxAirVelocity;

    public float groundFrictionCoeff;

    public float mouseSensitivity;

    private Rigidbody rb;   //We will be using this to apply force in order to make the camera move instead of just hardcoding transforms, so rigid body collisions work better!

    private float angleX;
    private float angleY;
    private float angleZ;

    private bool onGround;

    // Use this for initialization
    void Start() {
        angleX = 0;
        angleY = 0;
        angleZ = 0;

        onGround = false;

        //We'll just start will the cursor locked, and use ESC to unlock it
        Cursor.lockState = CursorLockMode.Locked;

        //Add we need to do is aquire a reference to this rigid body
        rb = GetComponent<Rigidbody>();

        //Since we are using half life bhop movment, we will be implementing friction (on the player) MANUALLY in our physics engine updates.
        //Thus, we will enforce a ZERO DRAG coefficient on our rigid body in this script upon startup.
        rb.drag = 0;

        //NOTE: You must set all ground mesh colliders to have zero friction as well for 'true' lossless bunny jumping, but the script cannot control this!

    }

    // FixedUpdate  a physics engine update that is called in perfect sync with the physics engine
    void FixedUpdate() {

        //First, check if we are on the ground. If we are NOT on the ground, then we should use air control physics.
        //If we ARE on the ground, use normal movement physics.
        onGround = performGroundCheckRaycast(distanceToCheckGround);

        if (onGround) {
            groundMovement();
        }
        else {
            airMovement();
        }


        //Call mouse look regardless of whether we are on the ground or in the air!
        //mouseLook();  //Call this in lateupdate instead..
    }

    //LateUpdate: called just before frame is rendered, so we'll use this to set the rotation to be bounded to the camera
    private void LateUpdate() {
        mouseLook();

        printVelocity();

        // Release cursor on escape keypress
        if (Input.GetKeyDown(KeyCode.Escape)) {
            Cursor.lockState = CursorLockMode.None;
        }
    }

    private bool performGroundCheckRaycast(float downcastDistance) {
        //This function will perform a physics racast straight downwards. It will only 'detect' gameobjects which are in the 'Ground' layer
        //I.e. we are using unity layers to filter to only ground objects. It will project only a small distance downwards so to accurately detect
        //if we are touching the ground!

        return Physics.Raycast(transform.position,  //Vector3 (xpos,ypos,zpos) - Starting point from which to raycast
                               transform.up * -1,   //Vector3 (xdir,ydir,zdir) - Direciton to cast the ray
                               downcastDistance,    //Float - Distance for the cast to go
                               1 << LayerMask.NameToLayer("Ground")     //Layer filter bitmask - used to determine which colliders we are checking
                               );
    }

    private void groundMovement() {
        //Make Y axis zeroed relative vectors. This way, we only apply horizontal force and don't fly up into the air.
        //TODO: Make the applied force somehow relative to the slop of the ground we are standing on?

        //Only apply running forces if we are on the ground.
        Vector3 forward = new Vector3(transform.forward.x, 0, transform.forward.z);
        forward.Normalize();

        Vector3 right = new Vector3(transform.right.x, 0, transform.right.z);
        right.Normalize();

        Vector3 dir = new Vector3(0, 0, 0);

        //WASD controls (Applies horizontal forces to the rigidbody of this gameobject)
        if (Input.GetKey(KeyCode.W)) {
            //rb.AddForce(forward * runningForce);
            dir += forward;
        }
        if (Input.GetKey(KeyCode.S)) {
            //rb.AddForce(-forward * runningForce);
            dir -= forward;
        }
        if (Input.GetKey(KeyCode.A)) {
            //rb.AddForce(-right * runningForce);
            dir -= right;
        }
        if (Input.GetKey(KeyCode.D)) {
            //rb.AddForce(right * runningForce);
            dir += right;
        }
        dir.Normalize();
        rb.AddForce(dir * runningForce);

        //Jumping. If the jump key is pressed then directly set vertical velocity on this rigid body
        //Holding space (bhop mode) also makes us skip friction calculations!
        if (Input.GetKey(KeyCode.Space)) {
            //rb.AddForce(transform.up * jumpForce);
            Vector3 vel = rb.velocity;
            vel.y = jumpVelocity;
            rb.velocity = vel;
        }
        else {
            //Were on the ground and not bhopping. Thus, we should apply a friction force manually to the rigid body.
            //Directly oppossing the current 
            Vector3 currVel = rb.velocity;
            float speed = currVel.magnitude;
            currVel.Normalize();

            //apply backwards acceleration scaled off friction value and current speed.
            rb.AddForce(-currVel * speed * groundFrictionCoeff);
        }

    }

    private void airMovement() {
        Vector3 forward = new Vector3(transform.forward.x, 0, transform.forward.z);
        forward.Normalize();

        Vector3 right = new Vector3(transform.right.x, 0, transform.right.z);
        right.Normalize();

        Vector3 desiredMoveDir = new Vector3(0, 0, 0);

        //Desired move direction will be determined by the current orientation and the player's input keys!
        if (Input.GetKey(KeyCode.W)) {
            desiredMoveDir += forward;
        }
        if (Input.GetKey(KeyCode.S)) {
            desiredMoveDir += -forward;
        }
        if (Input.GetKey(KeyCode.A)) {
            desiredMoveDir += -right;
        }
        if (Input.GetKey(KeyCode.D)) {
            desiredMoveDir += right;
        }

        desiredMoveDir.Normalize();

        //We don't want to touch our vertical component, only horizontal!
        Vector3 preVel = new Vector3(rb.velocity.x, 0, rb.velocity.z);
        float yComponent = rb.velocity.y;   //Retain the y component so that our acceleration alg doesn't change/lock it.


        Vector3 accelerateResult = Accelerate(desiredMoveDir, preVel, airAcceleration, maxAirVelocity);

        //Directly set the RB's velocty to result vector, after restoring the y component value!
        accelerateResult.y = yComponent;
        rb.velocity = accelerateResult;
    }

    // accelDir: normalized direction that the player has requested to move (taking into account the movement keys and look direction)
    // prevVelocity: The current velocity of the player, before any additional calculations
    // accelerate: The server-defined player acceleration value
    // max_velocity: The server-defined maximum player velocity (this is not strictly adhered to due to strafejumping)
    private Vector3 Accelerate(Vector3 accelDir, Vector3 prevVelocity, float accelerate, float max_velocity) {
        float projVel = Vector3.Dot(prevVelocity, accelDir); // Vector projection of Current velocity onto accelDir.
        float accelVel = accelerate * Time.fixedDeltaTime; // Accelerated velocity in direction of movment

        // If necessary, truncate the accelerated velocity so the vector projection does not exceed max_velocity
        if (projVel + accelVel > max_velocity)
            accelVel = max_velocity - projVel;

        return prevVelocity + accelDir * accelVel;
    }



    //This script must be tied to mouseLook to not flip the object all over the place lol
    private void mouseLook() {
        Vector2 mouseDelta = new Vector2(Input.GetAxisRaw("Mouse Y"), Input.GetAxisRaw("Mouse X"));

        angleX -= mouseDelta.x * mouseSensitivity;     //Up and down rotation. We don't want to actually rotate the body object like this though. We will apply to camera local rotation
        angleY += mouseDelta.y * mouseSensitivity;     //Left and right roation. We will apply this directly to the character body object.

        //Rotate the character body object itself
        this.transform.eulerAngles = new Vector3(0, angleY + mouseDelta.y, 0);  //Only apply left/right rotation to this object!

        //Rotate the camera(s) up and down, which is a child of this object!
        Camera[] childCams = this.transform.GetComponentsInChildren<Camera>(false);
        for (int i=0; i < childCams.Length; i++) {
            childCams[i].transform.localEulerAngles = new Vector3(angleX, 0, 0);
        }
    }

    private void printVelocity() {
        Vector3 vel = (rb.velocity);
        vel.y = 0;  //Remove vertical component 
        float speed = vel.magnitude;
        //Debug.Log("Current horizontal speed = " + speed);
    }
}
