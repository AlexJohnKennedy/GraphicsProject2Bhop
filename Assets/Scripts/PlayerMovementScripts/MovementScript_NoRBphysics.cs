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

    public bool easyModeBhop;

    private Rigidbody rb;   //We will be using this to apply force in order to make the camera move instead of just hardcoding transforms, so rigid body collisions work better!

    private float angleX;
    private float angleY;
    private float angleZ;

    private bool onGround;

    // Use this for initialization
    void Start() {
        //Use the statically available startGame script variables to determine movement settings, (I.e. easy mode or hard mode)
        easyModeBhop = StartGameScript.startGameWithEasyMode;

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
        
        return Physics.Raycast(transform.position,  //Vector3 (xpos,ypos,zpos) - Starting point from which to raycast. Set this to be the bottom of the collider object
                               transform.up * -1,   //Vector3 (xdir,ydir,zdir) - Direciton to cast the ray
                               downcastDistance * transform.localScale.y,    //Float - Distance for the cast to go
                               1 << LayerMask.NameToLayer("Ground")     //Layer filter bitmask - used to determine which colliders we are checking
                               );
    }

    private void groundMovement() {
        //Make Y axis zeroed relative vectors. This way, we only apply horizontal force and don't fly up into the air.
        //TODO: Make the applied force somehow relative to the slop of the ground we are standing on?
        float speed = rb.velocity.magnitude;

        //Jumping. If the jump key is pressed then directly set vertical velocity on this rigid body
        //Holding space (bhop mode) also makes us skip friction calculations!
        if (Input.GetKey(KeyCode.Space)) {
            //rb.AddForce(transform.up * jumpForce);
            Vector3 vel = rb.velocity;
            vel.y = jumpVelocity;
            rb.velocity = vel;
        }
        else if (Input.GetKey(KeyCode.LeftControl) && speed > 3.5) {
            //NOT bhopping, on the ground but holding crouch. This should be SLIDE MODE.
            //In slide mode, we will slide along the ground with very low friction, and still be able to curve via strafing.
            //However, we will not be able to 'increase' our speed. To achieve this, we will save the current speed, apply 'airmovement' control (for strafing)
            //THEN apply a very small resist speed reduction. If after the reduction we are STILL above the initial speed, we'll directly clamp the speed to whatever the previous was.

            airMovement();

            //Okay, let's simply apply a very slight resistance force for sliding
            Vector3 currVel = rb.velocity;
            currVel.Normalize();
            rb.AddForce(-currVel * speed * groundFrictionCoeff * 0.0005f);

            //If the speed has gone OVER the original speed, even after the resist force, then we need to clamp it back down to avoid crouch-slide acceleration.
            if (rb.velocity.magnitude > speed * 1.05) {
                rb.velocity = currVel * speed * 1.05f;  //CurrVel was normalized so this sets the magnitude directly to 'speed'
            }
        }
        else {
            //Running on ground
            applyGroundRunningWASDForce();

            //We're on the ground and not bhopping. Thus, we should apply a friction force manually to the rigid body.
            //Directly oppossing the current velocity
            Vector3 currVel = rb.velocity;
            currVel.Normalize();

            //apply backwards acceleration scaled off friction value and current speed.
            rb.AddForce(-currVel * speed * groundFrictionCoeff);
        }

    }
    private void applyGroundRunningWASDForce() {
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
    }

    private void airMovement() {
        Vector3 forward = new Vector3(transform.forward.x, 0, transform.forward.z);
        forward.Normalize();

        Vector3 right = new Vector3(transform.right.x, 0, transform.right.z);
        right.Normalize();

        Vector3 desiredMoveDir = new Vector3(0, 0, 0);

        //Desired move direction will be determined by the current orientation and the player's input keys! (If we are not in easy mode)
        if (!easyModeBhop) {
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
        }
        else {
            //EASY MODE (BOOOOO) determine the desiredMoveDir based on the mouse movement to supply auto strafing, to help beginner players with bhopping.
            desiredMoveDir = easyModeGetAccelVector();
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

    //This function will use the current mouse 'difference' to automatically strafe for you, and provide perfect WASD strafing for the player.
    //This will make movement easier and more accessible whil still being bohp movement. However, it also removes most of the skill and timing requirments, so in my opinion, 
    //makes it alot less fun. :(
    private Vector3 easyModeGetAccelVector() {
        //AUTO STRAFE IF AND ONLY IF YOU'RE HOLDING THE W KEY.
        if (!Input.GetKey(KeyCode.W)) {
            return new Vector3(0, 0, 0);
        }

        Vector2 mouseDelta = new Vector2(Input.GetAxisRaw("Mouse Y"), Input.GetAxisRaw("Mouse X"));

        Vector3 right = new Vector3(transform.right.x, 0, transform.right.z);
        right.Normalize();

        if (mouseDelta.y > 0) {
            //Mouse moving RIGHT. thus, automatically strafe to the right.
            return right;
        }
        else if (mouseDelta.y < 0) {
            return -right;
        }
        else {
            return new Vector3(0, 0, 0);
        }
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

        //Angle clamping to avoid overflows and weird upside down rotatations.
        if (angleY > 360) {
            angleY -= 360;
        }
        else if (angleY < 0) {
            angleY += 360;
        }
        
        if (angleX < -90) {
            //CLAMP DOWNWARDSNESS
            angleX = -90;
        }
        else if (angleX > 90) {
            //CLAMP UPWARDSNESS
            angleX = 90;
        }

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
