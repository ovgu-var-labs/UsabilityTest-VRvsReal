using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MRI_AirVentilationControl : MonoBehaviour
{
    [SerializeField] private MRI_Small_Controller_Interactivity controllerLeft;
    [SerializeField] private MRI_Small_Controller_Interactivity controllerRight;
    [SerializeField][Range(0,1)] private float maxVolume;

    private bool isVentilationEnabled;
    private float targetVolume;
    private bool isAnimating;
    private float animationStart;
    private float animationDuration_s = 2.5f; // seconds
    private float startVolume;

    // Start is called before the first frame update
    void Start()
    {
        isVentilationEnabled = false;
        isAnimating = false;
        controllerLeft.OnButtonRightPress += TurnVentilationOnOff;
        controllerRight.OnButtonRightPress += TurnVentilationOnOff;
    }

    private void FixedUpdate()
    {
        if (isAnimating) AnimateVolume();
    }

    void TurnVentilationOnOff()
    {
        if (isAnimating) isVentilationEnabled = !isVentilationEnabled; // if pressed again during animation, revert volume change
        isAnimating = true;
        animationStart = Time.time;
        startVolume = GetComponent<AudioSource>().volume;
    }

    void AnimateVolume()
    {
        float t = (Time.time - animationStart) / animationDuration_s;
        float newVolume;
        if (isVentilationEnabled)
        {
            newVolume = Mathf.Lerp(startVolume, 0, t); // volume down
            if (newVolume <= 0)
            {
                isVentilationEnabled = false;
                isAnimating = false;
            }
        }
        else
        {
            newVolume = Mathf.Lerp(startVolume, maxVolume, t); // volume up
            if (newVolume >= maxVolume)
            {
                isVentilationEnabled = true;
                isAnimating = false;
            }
        }

        GetComponent<AudioSource>().volume = newVolume;
    }
}
