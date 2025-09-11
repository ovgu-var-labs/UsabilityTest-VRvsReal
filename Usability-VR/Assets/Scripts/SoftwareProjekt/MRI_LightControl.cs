using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MRI_LightControl : MonoBehaviour
{
    [SerializeField] private MRI_Small_Controller_Interactivity controllerLeft;
    [SerializeField] private MRI_Small_Controller_Interactivity controllerRight;
    [SerializeField] private bool usingEmissionOnly; // whether the "light" is actually just an object with an emitting material
    [SerializeField][Tooltip("Default: 10,000")] private float maxEmission = 10000;

    private float maxIntensity;
    private bool isLightEnabled;
    private float targetIntensity;
    private bool isAnimating;
    private float animationStart;
    //private float animationDuration_s = 0.5f; // seconds

    // Start is called before the first frame update
    void Start()
    {
        isLightEnabled = true;
        isAnimating = false;
        if (!usingEmissionOnly)
        {
            maxIntensity = GetComponent<Light>().intensity;
        }
        controllerLeft.OnButtonLeftPress += TurnLightOnOff;
        controllerRight.OnButtonLeftPress += TurnLightOnOff;
    }

    private void FixedUpdate()
    {
        if (isAnimating) AnimateLightIntensity();
    }

    void TurnLightOnOff()
    {
        if (isLightEnabled)
        {
            targetIntensity = 0;
        }
        else
        {
            if (usingEmissionOnly)
            {
                targetIntensity = maxEmission;
            }
            else
            {
                targetIntensity = maxIntensity;
            }
        }

        isAnimating = true;
    }

    void AnimateLightIntensity()
    {
        if (usingEmissionOnly)
        {
            GetComponent<Renderer>().material.SetFloat("_EmissionIntensity", targetIntensity);
        }
        else
        {
            GetComponent<Light>().intensity = targetIntensity;
        }

        isLightEnabled = !isLightEnabled;
        isAnimating = false;
    }
}
