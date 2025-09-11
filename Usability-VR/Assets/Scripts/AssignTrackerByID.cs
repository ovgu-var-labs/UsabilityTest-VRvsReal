using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using Valve.VR;

/// <summary>
/// Assigns a tracker via ID.
/// </summary>
public class AssignTrackerByID : MonoBehaviour
{
    [Tooltip("Whether to assign this game object to its hardware tracker.")] public bool assignToTrackerDevice = false;
    [Tooltip("Match items/tools to their IDs:" +
        "\nLHR_72A542D4 --> tweezers" +
        "\nLHR_68E385F6 --> needle" +
        "\nLHR_31913AF --> patient table" +
        "\nLHR_0AF9B59F --> tool table" +
        "\nLHR_FC146A1F --> laser projector" +
        "\nLHR_BDB73DD5 --> MRI")]
    public AvailableTrackerIDs IDOfThisTracker;
    public TrackerManager trackerManager;
    [Tooltip("Send additional errors and warnings")] public bool verbose;
    private bool _wasDeviceOfflineErrorSent = false;

    private Dictionary<int, Valve.VR.SteamVR_TrackedObject.EIndex> intIndexToEIndex; // the dictionary to assign the Valve indices correctly from numeric indices
    [HideInInspector] public bool isTrackerAssignedSuccessfully; // whether the tracker is currently assigned to a tracker (i.e. tracker is online)

    /// <summary>
    /// IMPORTANT:  When adding a new tracker ID to this list, replace the dash "-" with an underscore "_".
    ///             Otherwise the ID cannot be stored as enum.
    /// </summary>
    public enum AvailableTrackerIDs
    {
        none,
        LHR_72A542D4, // tweezers
        LHR_68E385F6, // needle
        LHR_31913AF9, // patient table
        LHR_0AF9B59F, // tool table
        LHR_FC146A1F, // laser projector
        LHR_BDB73DD5, // MRI
    }

    // Start is called before the first frame update
    void Start()
    {
        // populate dict
        intIndexToEIndex = new Dictionary<int, Valve.VR.SteamVR_TrackedObject.EIndex>();
        int enumLength = Enum.GetNames(typeof(Valve.VR.SteamVR_TrackedObject.EIndex)).Length;
        for (int i = -1; i < enumLength; ++i)
        {
            // add all available device slots (EIndex) to their respective numeric index
            switch (i)
            {
                case -1:
                    intIndexToEIndex.Add(i, Valve.VR.SteamVR_TrackedObject.EIndex.None);
                    break;
                case 0:
                    intIndexToEIndex.Add(i, Valve.VR.SteamVR_TrackedObject.EIndex.Hmd);
                    break;
                case 1:
                    intIndexToEIndex.Add(i, Valve.VR.SteamVR_TrackedObject.EIndex.Device1);
                    break;
                case 2:
                    intIndexToEIndex.Add(i, Valve.VR.SteamVR_TrackedObject.EIndex.Device2);
                    break;
                case 3:
                    intIndexToEIndex.Add(i, Valve.VR.SteamVR_TrackedObject.EIndex.Device3);
                    break;
                case 4:
                    intIndexToEIndex.Add(i, Valve.VR.SteamVR_TrackedObject.EIndex.Device4);
                    break;
                case 5:
                    intIndexToEIndex.Add(i, Valve.VR.SteamVR_TrackedObject.EIndex.Device5);
                    break;
                case 6:
                    intIndexToEIndex.Add(i, Valve.VR.SteamVR_TrackedObject.EIndex.Device6);
                    break;
                case 7:
                    intIndexToEIndex.Add(i, Valve.VR.SteamVR_TrackedObject.EIndex.Device7);
                    break;
                case 8:
                    intIndexToEIndex.Add(i, Valve.VR.SteamVR_TrackedObject.EIndex.Device8);
                    break;
                case 9:
                    intIndexToEIndex.Add(i, Valve.VR.SteamVR_TrackedObject.EIndex.Device9);
                    break;
                case 10:
                    intIndexToEIndex.Add(i, Valve.VR.SteamVR_TrackedObject.EIndex.Device10);
                    break;
                case 11:
                    intIndexToEIndex.Add(i, Valve.VR.SteamVR_TrackedObject.EIndex.Device11);
                    break;
                case 12:
                    intIndexToEIndex.Add(i, Valve.VR.SteamVR_TrackedObject.EIndex.Device12);
                    break;
                case 13:
                    intIndexToEIndex.Add(i, Valve.VR.SteamVR_TrackedObject.EIndex.Device13);
                    break;
                case 14:
                    intIndexToEIndex.Add(i, Valve.VR.SteamVR_TrackedObject.EIndex.Device14);
                    break;
                case 15:
                    intIndexToEIndex.Add(i, Valve.VR.SteamVR_TrackedObject.EIndex.Device15);
                    break;
                case 16:
                    intIndexToEIndex.Add(i, Valve.VR.SteamVR_TrackedObject.EIndex.Device16);
                    break;
                default:
                    intIndexToEIndex.Add(i, Valve.VR.SteamVR_TrackedObject.EIndex.None);
                    break;
            }
        }

        if (trackerManager == null)
        {
            Debug.LogError("No tracker manager assigned!");
            return;
        }
        else
        {
            trackerManager.OnTrackerListCompleted += AssignTracker;
        }
    }

    /// <summary>
    /// Assigns this gameobject's SteamVRTrackedObject to a matching hardware tracker, if found.
    /// </summary>
    void AssignTracker()
    {
        if (!assignToTrackerDevice) return;

        string hardwareID = IDOfThisTracker.ToString().Replace('_', '-');
        var steamVRtrackedObject = transform.GetComponent<Valve.VR.SteamVR_TrackedObject>();

        // try and find the desired ID in the list of currently available devices
        if (trackerManager.trackerIndicesToID.ContainsKey(hardwareID))
        {
            if (steamVRtrackedObject != null)
            {
                int index = -1; // invalid device ("none")
                trackerManager.trackerIndicesToID.TryGetValue(hardwareID, out index);

                Valve.VR.SteamVR_TrackedObject.EIndex eindex = Valve.VR.SteamVR_TrackedObject.EIndex.None;
                if (index >= 0) intIndexToEIndex.TryGetValue(index, out eindex);

                // set the respective index at SteamVRTrackedObject 
                steamVRtrackedObject.index = eindex;
                isTrackerAssignedSuccessfully = true;

                var parentConstraint = GetComponent<ParentConstraint>();
                if (parentConstraint != null) parentConstraint.enabled = false; // disable parenting to be able to control own position
            }
            else
            {
                Debug.LogError(
                    "No SteamVRTrackedObject script found on this object! " +
                    "\nCould not add tracker hardware index.");
            }
        }
        else
        {
            // could not assign tracker
            isTrackerAssignedSuccessfully = false;
            steamVRtrackedObject.index = SteamVR_TrackedObject.EIndex.None;
            if (!_wasDeviceOfflineErrorSent && verbose)
            {
                Debug.LogError("Could not assign tracker " + IDOfThisTracker + ". Is the device online?");
                _wasDeviceOfflineErrorSent = true;
            }
        }
    }
}
