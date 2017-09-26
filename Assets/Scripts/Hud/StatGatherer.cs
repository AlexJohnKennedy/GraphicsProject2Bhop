using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/* This script is passed references to all other scripts or obects which contain 'statistics' that need to be
 * displayed on the player HUD. It is responsible for gathering this information and then updating fields with the information
 */
public class StatGatherer : MonoBehaviour {

    public AirBraking airBrakingObject;  //Contains stat for current 'air brake meter'
    public Rigidbody playerRigidbody;    //Can harvest velocity and movement information about the player from it's rigid body
    public HealthScript healthObject;    //Contains stat for current 'health'

    public Text velocityHudText;
    public Text velocityHudTextShadow;
    public Text airBrakeMeterText;
    public Text healthHudText;

    public float velocityTextPosMaxOffset;      //Parameters used for scaling effect on velocity text.
    public float velocityTextScaleMaxOffset;
    public Color velocityTextMaxColour;
    public float maxOffsetVelocity;     //The velocity at which the text offsets are 'maximally scaled'

    private float velTextLocalYPos;
    private float baseFontSize;
    private Color basetextColour;

	// Use this for initialization
	void Start () {
        velTextLocalYPos = velocityHudText.transform.localPosition.y;
        baseFontSize = velocityHudText.fontSize;
        basetextColour = velocityHudText.color;

        if (velocityHudTextShadow != null) {
            velocityHudTextShadow.transform.localPosition = velocityHudText.transform.localPosition;
            velocityHudTextShadow.transform.SetAsFirstSibling();
        }
    }
	
	// Update is called once per frame
	void Update () {
        //Simply update everything.
        updateAirBrakeMeterText();
        updateHealthText();
        updateVelocityTextObject(velocityHudText, 1f, true);	
        if (velocityHudTextShadow != null) updateVelocityTextObject(velocityHudTextShadow, 0.5f, false);
    }

    private void updateAirBrakeMeterText() {
        int perc = (int)airBrakingObject.getMeterPercentage(); //Hard cast to floor value, since we don't want to display decimals..
        airBrakeMeterText.text = perc.ToString();
    }

    private void updateHealthText() {
        int health = (int)healthObject.getCurrHealth();
        healthHudText.text = health.ToString();
    }

    private void updateVelocityTextObject(Text velocityHudText, float scalingScaling, bool useColourInterp) {
        int vel = (int)getHorizontalVelocity();     //Hard cast to floor the value, since we don't want to display decimal places on the hud..
        velocityHudText.text = vel.ToString();

        float mag = calcVelocityScaling(vel);
        float posOffset = scalingScaling * mag * velocityTextPosMaxOffset;
        float scaling = mag * velocityTextScaleMaxOffset;

        //provide scaling effects and colour effects based on the current velocity!
        if (useColourInterp) { 
            float rColorInterpolated = basetextColour.r + (velocityTextMaxColour.r - basetextColour.r) * mag;
            float gColorInterpolated = basetextColour.g + (velocityTextMaxColour.g - basetextColour.g) * mag;
            float bColorInterpolated = basetextColour.b + (velocityTextMaxColour.b - basetextColour.b) * mag;

            //Apply colour scaling
            Color newCol = new Color(rColorInterpolated,
                                     gColorInterpolated,
                                     bColorInterpolated,
                                     basetextColour.a);
            velocityHudText.color = newCol;
        }

        //Offset vertical position downwards slightly..
        Vector3 pos = velocityHudText.transform.localPosition;
        pos.y = velTextLocalYPos - posOffset;
        velocityHudText.transform.localPosition = pos;

        //Apply font size scaling...
        velocityHudText.fontSize = (int)(baseFontSize + scaling);
    }

    private float getHorizontalVelocity() {
        Vector3 raw = new Vector3(playerRigidbody.velocity.x, 0, playerRigidbody.velocity.z);

        return raw.magnitude;
    }

    private float calcVelocityScaling(float vel) {
        //Get linear scale amount
        if (vel < maxOffsetVelocity) {
            float linear = vel / maxOffsetVelocity;
            float quadratic = linear * linear;
            return quadratic;
        }
        else {
            return 1.0f;
        }
    }
}
