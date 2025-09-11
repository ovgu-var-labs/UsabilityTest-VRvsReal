using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Valve.VR;

public class TrackerManager : MonoBehaviour
{
    public EventManager eventManager;
    [HideInInspector] public Dictionary<string, int> trackerIndicesToID; // maps a tracker's ID to the index it was given at start of the scene
    public delegate void ListComplete();
    public event ListComplete OnTrackerListCompleted;

    private void Start()
    {
        eventManager.OnScenarioSettingsApplied += LookForTrackers;
    }

    void LookForTrackers(StudyControl studyControl)
    {
        if (studyControl.scenario.Equals(ApplicationSettings.Scenario.VR_without_trackers)) return; // don't apply trackers if none are needed

        int x = 2; // seconds
        InvokeRepeating("ListDevices", 0, x); // search for trackers at startup and then every x seconds
    }

    void ListDevices()
    {
        trackerIndicesToID = new Dictionary<string, int>();

        // find all connected OpenVR tracking devices and add collect their IDs along with their device index
        for (int i = 0; i < SteamVR.connected.Length; ++i)
        {
            ETrackedPropertyError error = new ETrackedPropertyError();
            StringBuilder sb = new StringBuilder();
            OpenVR.System.GetStringTrackedDeviceProperty((uint)i, ETrackedDeviceProperty.Prop_SerialNumber_String, sb, OpenVR.k_unMaxPropertyStringSize, ref error);
            var SerialNumber = sb.ToString();

            OpenVR.System.GetStringTrackedDeviceProperty((uint)i, ETrackedDeviceProperty.Prop_ModelNumber_String, sb, OpenVR.k_unMaxPropertyStringSize, ref error);
            var ModelNumber = sb.ToString();

            bool isTrackerOnline = OpenVR.System.IsTrackedDeviceConnected((uint)i);

            if (isTrackerOnline && (SerialNumber.Length > 0 || ModelNumber.Length > 0))
            {
                // Debug.Log("Device online: Index = " + i.ToString() + " | Serial = " + SerialNumber + " | Model = " + ModelNumber);
                trackerIndicesToID.Add(SerialNumber, i);
            }
        }

        OnTrackerListCompleted?.Invoke();
    }
}
