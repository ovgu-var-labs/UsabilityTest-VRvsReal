using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ApplicationSettings;

/// <summary>
/// Sync transform of this object to a target if trackers are enabled.
/// </summary>
public class SyncTransform : MonoBehaviour
{
    public GameObject syncTransformTo;
    public StudyControl studyControl;

    // Update is called once per frame
    void Update()
    {
        if (studyControl.scenario.Equals(Scenario.VR_without_trackers)) return; // don't override transforms if trackers aren't enabled

        if(syncTransformTo != null)
        {
            syncTransformTo.transform.position = transform.position;
            syncTransformTo.transform.rotation = transform.rotation;
        }        
    }
}
