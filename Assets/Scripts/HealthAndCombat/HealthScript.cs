using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class HealthScript : MonoBehaviour {

    public class DefaultTakeDamageEvent : UnityEvent<float,GameObject> {
    }

    public float startHealth;
    public UnityEvent<float, GameObject> takeDamageEvent;

    private float currHealth;

    // Use this for initialization
    void Start () {
		if (takeDamageEvent == null) {
            //Since we were NOT passed a customised damage taken event, we'll instantiate some default behaviour
            takeDamageEvent = new DefaultTakeDamageEvent();
            if (!this.gameObject.tag.Equals("Player")) {
                takeDamageEvent.AddListener(normalDamageTake);
            }
            else {
                takeDamageEvent.AddListener(playerDamageTake);
            }
        }

        currHealth = startHealth;
	}
	
	public float getCurrHealth() {
        return this.currHealth;
    }
    public int getFlooredHealthPercentage() {
        return (int) (currHealth / startHealth);
    }

    //Default listener
    private void normalDamageTake(float dmgAmount, GameObject damager) {
        //All we will do here is just minus the damange amount from this objects health, and kill this object if we have run out of health!

        currHealth -= dmgAmount;

        if (currHealth <= 0) {
            currHealth = 0;

            //Time to die
            this.gameObject.SetActive(false);   //Deactivate instead of destorying. (safer)
            //Object.Destroy(this.gameObject);     //Use the OnDestroy() call in another script to define onDestory behaviour!
        }
    }

    //Player character listener only
    private void playerDamageTake(float dmgAmount, GameObject damager) {

        currHealth -= dmgAmount;

        if (currHealth <= 0) {
            currHealth = 0;

            //Time to die. BUT since this method is for the player character, we cannot simply delete the player just yet! Let's spawn a new camera exaclty where the player's camera
            //is at the moment. Then, we can spawn a 'gameover timer' script and add it to the new camera. This timer will automatically take the player back to the main menu after a
            //few seconds.
            Camera.main.gameObject.transform.parent = null; //DETATCH THE CAMERA FROM THE PLAYER, so that the player camera does not also get deleted when delete the player.

            Camera.main.gameObject.AddComponent<GameOverTimer>();

            this.gameObject.SetActive(false);
        }
    }
}
