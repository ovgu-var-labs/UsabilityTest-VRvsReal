using Leap;
using Leap.HandsModule;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ApplicationSettings;
using System.Linq;
using System;

/// <summary>
/// This script controls which HandBinder to use, according to the current detail level.
/// </summary>
public class PinchDetectorController : MonoBehaviour
{
    public Ablaufmanager ablaufmanager;
    public EventManager eventManager;
    public HandBinder handBinder_LeftHandLow;
    public HandBinder handBinder_RightHandLow;
    public HandBinder handBinder_LeftHandHigh;
    public HandBinder handBinder_RightHandHigh;

    private PinchDetector pinchDetectorLeftHand;
    private PinchDetector pinchDetectorRightHand;

    // Start is called before the first frame update
    void Start()
    {
        eventManager.OnScenarioPreparedAndStarted += UpdateHandBinder;
        pinchDetectorRightHand = transform.gameObject.GetComponents<PinchDetector>().ElementAt(0);
        pinchDetectorLeftHand = transform.gameObject.GetComponents<PinchDetector>().ElementAt(1);
    }

    /// <summary>
    /// Update the HandBinders for the left and right hand according to the provided detail level.
    /// </summary>
    /// <param name="detailLevel">The current detail level of the scene.</param>
    public void UpdateHandBinder(object sender, EventArgs e)
    {
        switch (ablaufmanager.sceneDetailLevel)
        {
            case SceneDetailLevel.Low:
                SetHandBinder(pinchDetectorLeftHand, handBinder_LeftHandLow);
                SetHandBinder(pinchDetectorRightHand, handBinder_RightHandLow);
                break;
            case SceneDetailLevel.High:
                SetHandBinder(pinchDetectorLeftHand, handBinder_LeftHandHigh);
                SetHandBinder(pinchDetectorRightHand, handBinder_RightHandHigh);
                break;
            default:
                break;
        }
    }

    void SetHandBinder(PinchDetector pinchDetector, HandBinder handBinder)
    {
        // pinchDetector.HandModel = handBinder;
    }
}
