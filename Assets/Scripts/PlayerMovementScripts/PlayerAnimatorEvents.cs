using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/** This class is simply responsible for relaying events to the player's animator paramters,
 * such that the animator knows which animation states to play!
 */
public class PlayerAnimatorEvents : MonoBehaviour {

    Animator anim;

	// Use this for initialization
	void Start () {
        //Aquire this game object's animator!
        anim = this.GetComponent<Animator>();

        //Set crouch state to FALSE to begin with!
        anim.SetBool("isCrouched", false);
	}
	
	// Update is called once per frame
	void Update () {
		//Check if the user is holding control key, and set the crouch state accordingly
        if (Input.GetKey(KeyCode.LeftControl)) {
            anim.SetBool("isCrouched", true);
        }
        else {
            anim.SetBool("isCrouched", false);
        }
	}
}
