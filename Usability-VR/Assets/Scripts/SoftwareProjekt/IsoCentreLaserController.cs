using System;
using UnityEngine;

public class IsoCentreLaserController : MonoBehaviour
{
    [SerializeField] private GameObject laserMarkerProjector_parent;
    [SerializeField] private GameObject laserMarkerProjector_red;
    [SerializeField] private GameObject laserMarkerProjector_green;
    [NonSerialized] public bool laserMarkerForCalibrationEnabled = false;

    public enum ColourState
    {
        red,
        green
    }

    // Start is called before the first frame update
    void Start()
    {
        UpdateLaserColour(ColourState.red);
    }

    void FixedUpdate()
    {
        laserMarkerProjector_parent.SetActive(laserMarkerForCalibrationEnabled);
    }

    /// <summary>
    /// Set both to false to turn lasers off.
    /// </summary>
    public void UpdateLaserColour(ColourState colour)
    {
        switch (colour)
        {
            case ColourState.red:
                laserMarkerProjector_red.SetActive(true);
                laserMarkerProjector_green.SetActive(false);
                break;
            case ColourState.green:
                laserMarkerProjector_red.SetActive(false);
                laserMarkerProjector_green.SetActive(true);
                break;
            default:
                laserMarkerProjector_red.SetActive(false);
                laserMarkerProjector_green.SetActive(false);
                break;
        }
    }

    public void ToggleLaser()
    {
        laserMarkerForCalibrationEnabled = !laserMarkerForCalibrationEnabled;
    }
}
