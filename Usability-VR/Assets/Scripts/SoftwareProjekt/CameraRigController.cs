using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SpatialTracking;
using Valve.VR;
using static ApplicationSettings;

public class CameraRigController : MonoBehaviour
{
    public GameObject interventionPosition;
    public GameObject controlRoomPosition;
    public GameObject headPosition;
    private Vector3 defaultCameraRigPosition;
    private Quaternion defaultCameraRigRotation;

    public GameObject head;
    public GameObject cameraRig;
    public VoiceControl voiceControl;

    private bool isCameraLockedToHead;

    private void Start()
    {
        defaultCameraRigPosition = cameraRig.transform.position;
        defaultCameraRigRotation = cameraRig.transform.rotation;
        isCameraLockedToHead = false;
    }

    private void FixedUpdate()
    {
        if (isCameraLockedToHead)
        {
            GameObject camera = cameraRig.transform.GetChild(0).gameObject;
            cameraRig.transform.rotation = Quaternion.Euler(0, 0, 0);
            cameraRig.transform.position = -camera.transform.localPosition + headPosition.transform.position;
            head.SetActive(false);
        }
        else
        {
            #region just for testing / setup
            //cameraRig.transform.position = interventionPosition.transform.position;
            //cameraRig.transform.rotation = interventionPosition.transform.rotation;
            // ---- //
            //cameraRig.transform.position = controlRoomPosition.transform.position;
            //cameraRig.transform.rotation = controlRoomPosition.transform.rotation;
            #endregion

            head.SetActive(true);
        }
    }

    public void ChangeCameraTo(string keyword)
    {
        if (keyword.Equals(VoiceControlKeywords.camera_reset.ToString()))
        {
            cameraRig.transform.position = defaultCameraRigPosition;
            cameraRig.transform.rotation = defaultCameraRigRotation;
            isCameraLockedToHead = false;
        }
        else if (keyword.Equals(VoiceControlKeywords.camera_to_intervention.ToString()))
        {
            cameraRig.transform.position = interventionPosition.transform.position;
            cameraRig.transform.rotation = interventionPosition.transform.rotation;
            isCameraLockedToHead = false;
        }
        else if (keyword.Equals(VoiceControlKeywords.camera_to_control.ToString()))
        {
            cameraRig.transform.position = controlRoomPosition.transform.position;
            cameraRig.transform.rotation = controlRoomPosition.transform.rotation;
            isCameraLockedToHead = false;
        }
        else if (keyword.Equals(VoiceControlKeywords.camera_to_head.ToString()))
        {
            isCameraLockedToHead = true;
        }
        else
        {
            Debug.LogError("Invalid keyword detected for camera change!");
        }
    }
}
