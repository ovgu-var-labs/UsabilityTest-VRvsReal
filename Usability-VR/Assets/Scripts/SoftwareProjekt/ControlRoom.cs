using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ApplicationSettings;

public class ControlRoom : MonoBehaviour
{
    [SerializeField] private GameObject displayLeft;
    [SerializeField] private GameObject displayRight;

    [SerializeField] private MRITableController mri_controller;
    [SerializeField] private ControlRoomKeyboard keyboard;

    [SerializeField] private Texture result_idle;
    [SerializeField] private Texture result_complete;
    [SerializeField] private Texture scan_idle;
    [SerializeField] private Texture scan_complete;
    [SerializeField] private Texture scan_started;

    public enum DisplayConfiguration
    {
        scan_idle,
        scan_started,
        scan_complete
    }

    private void Start()
    {
        keyboard.OnKeyboardPressed += InvokeRemoteScan;

        ChangeDisplayImages(DisplayConfiguration.scan_idle);
    }

    void InvokeRemoteScan()
    {
        if(!mri_controller.isIsoCentreCalibrated) return; // cannot start scan if iso-centre is not calibrated

        mri_controller.InitiateMoveToIsoCentre();
        ChangeDisplayImages(DisplayConfiguration.scan_started);
        mri_controller.OnIsoCentreReached += ChangeDisplayImages;
    }

    void ChangeDisplayImages(DisplayConfiguration configuration)
    {
        if (configuration == DisplayConfiguration.scan_idle)
        {
            displayLeft.GetComponent<Renderer>().materials[1].mainTexture = scan_idle;
            displayRight.GetComponent<Renderer>().materials[1].mainTexture = result_idle;
        }
        else if (configuration == DisplayConfiguration.scan_started)
        {
            displayLeft.GetComponent<Renderer>().materials[1].mainTexture = scan_started;
            displayRight.GetComponent<Renderer>().materials[1].mainTexture = result_idle;
        }
        else if (configuration == DisplayConfiguration.scan_complete)
        {
            displayLeft.GetComponent<Renderer>().materials[1].mainTexture = scan_complete;
            displayRight.GetComponent<Renderer>().materials[1].mainTexture = result_complete;
        }
    }
}
