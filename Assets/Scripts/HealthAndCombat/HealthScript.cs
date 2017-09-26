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
            takeDamageEvent.AddListener(normalDamageTake);
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
            Object.Destroy(this.gameObject);     //Use the OnDestroy() call in another script to define onDestory behaviour!
        }
    }
}
