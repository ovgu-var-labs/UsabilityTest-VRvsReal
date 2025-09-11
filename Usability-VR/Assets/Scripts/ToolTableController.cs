using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using static ApplicationSettings;
using static UnityEngine.GraphicsBuffer;
using static StudyControl;
using System;
using static UnityEngine.Rendering.DebugUI;
using UnityEngine.UI;
using System.Diagnostics;

public class ToolTableController : MonoBehaviour
{
    public Ablaufmanager ablaufmanager;
    public EventManager eventmanager;
    public StudyControl studycontrol;

    [Tooltip("The gameobject used to record where the tool table is initially placed for the first task (moving the tool table)")] public GameObject tableStart;
    [Tooltip("The tracker gameobject for the tool table.")] public GameObject tableTracker;
    private Transform toolTablePlaceholderTr;
    private bool _isOffsetSet;
    public GameObject LeftHandLow;
    public GameObject RightHandLow;
    public GameObject LeftHandHigh;
    public GameObject RightHandHigh;

    private Transform LeftHandTransform;
    private Transform RightHandTransform;

    [HideInInspector] public bool isTableGrabbed = false; // by both hands
    private bool _isTouchedByRightHand;
    private bool _isTouchedByLeftHand;

    private Vector3 _handsLastPosition; // the position in the middle of both hands
    private Vector3 _handsLastConnectingVector; // the vector connecting both hands (left to right hand)
    private float _bufferStart;
    private readonly float _bufferDuration = 0.1f; // how long to wait until all the trackers are in the right position (in seconds)
    private bool _wasBufferPassed;
    private ToolTableStopwatch _ttStopwatch;

    [HideInInspector] public bool isStartPositionSet;
    [HideInInspector] public DateTime lastTimeRemovedHandsFromTable;
    [HideInInspector] public bool recordLastTimeHandsRemoved = true;

    private void Start()
    {
        _ttStopwatch = GetComponent<ToolTableStopwatch>();
        isStartPositionSet = false;
        _wasBufferPassed = true;
        toolTablePlaceholderTr = transform.parent.GetComponent<Transform>();
        eventmanager.OnSceneDetailLevelChange += SetCorrectHands;
        eventmanager.OnScenarioPreparedAndStarted += Setup;
    }

    void Setup(object sender, EventArgs e)
    {
        // if there are no trackers used (VR-only), set the table start position right away
        if (studycontrol.scenario.Equals(Scenario.VR_without_trackers))
        {
            transform.parent.GetComponent<ParentConstraint>().enabled = false; // disable tracker parenting for placeholder (parent of this)
            tableStart.GetComponent<ParentConstraint>().enabled = false; // set start position of table
            isStartPositionSet = true;
            _ttStopwatch.EnableDistanceCheck();
        }
        else if (studycontrol.scenario.Equals(Scenario.VR_with_trackers) || studycontrol.scenario.Equals(Scenario.Real))
        {
            _bufferStart = Time.time;
            _wasBufferPassed = false;
        }
    }

    void SetCorrectHands(SceneDetailLevel detailLevel)
    {
        switch (detailLevel)
        {
            case SceneDetailLevel.Low:
                LeftHandTransform = LeftHandLow.transform;
                RightHandTransform = RightHandLow.transform;
                break;
            case SceneDetailLevel.High:
                LeftHandTransform = LeftHandHigh.transform;
                RightHandTransform = RightHandHigh.transform;
                break;
            default:
                break;
        }
    }

    private void OnCollisionEnter(Collision target)
    {
        if (target.collider.CompareTag(ApplicationObjectTags.LeftHand.ToString())) _isTouchedByLeftHand = true; // moving left hand into collider
        if (target.collider.CompareTag(ApplicationObjectTags.RightHand.ToString())) _isTouchedByRightHand = true; // moving right hand into collider
    }

    private void OnCollisionExit(Collision target)
    {
        if (target.collider.CompareTag(ApplicationObjectTags.LeftHand.ToString())) _isTouchedByLeftHand = false; // removing left hand from collider
        if (target.collider.CompareTag(ApplicationObjectTags.RightHand.ToString())) _isTouchedByRightHand = false; // removing right hand from collider
    }

    private Vector3 GetMidpoint(Vector3 p1, Vector3 p2)
    {
        return Vector3.Lerp(p1, p2, 0.5f);
    }

    /// <summary>
    /// Calculates the signed (+/-) angle between two vectors a and b.
    /// It uses the normal n to tell if the result should be clockwise or counterclockwise.
    /// Source: 
    ///     https://stackoverflow.com/questions/19675676/calculating-actual-angle-between-two-vectors-in-unity3d
    ///     https://stackoverflow.com/users/222233/jerdak
    /// </summary>
    /// <param name="a">first vector</param>
    /// <param name="b">second vector</param>
    /// <param name="n">the normal</param>
    /// <returns>The signed angle between a and b, using n as normal.</returns>
    float SignedAngleBetween(Vector3 a, Vector3 b, Vector3 n)
    {
        // angle in [0,180]
        float angle = Vector3.Angle(a, b);
        float sign = Mathf.Sign(Vector3.Dot(n, Vector3.Cross(a, b)));

        // angle in [-179,180]
        float signed_angle = angle * sign;

        return signed_angle;
    }

    private void Update()
    {
        if (!studycontrol.IsStudyResultsRecordingActive) return;

        if (Time.time - _bufferStart > _bufferDuration && !_wasBufferPassed)
        {
            _wasBufferPassed = true;

            if (studycontrol.scenario.Equals(Scenario.VR_with_trackers) || studycontrol.scenario.Equals(Scenario.Real))
            {
                // check whether tool table tracker is assigned and set start position
                if (!isStartPositionSet && tableTracker.GetComponent<AssignTrackerByID>().isTrackerAssignedSuccessfully)
                {
                    // set start position of table
                    ParentConstraint constraint = tableStart.GetComponent<ParentConstraint>();
                    if (constraint != null) constraint.enabled = false; // fix start position (remove it from being parented to the table)
                    isStartPositionSet = true;

                    _ttStopwatch.EnableDistanceCheck();
                }
            }
        }

        // control tool table position if grabbed
        // only use in VR-only scenario and only during table setup phase/task
        if (studycontrol.scenario.Equals(Scenario.VR_without_trackers) && ablaufmanager.currentStep == Steps.TableSetup)
        {
            if (_isTouchedByLeftHand && ablaufmanager.isLeftHandPinched && _isTouchedByRightHand && ablaufmanager.isRightHandPinched) // is the table grabbed by both hands?
            {
                var handsMid = GetMidpoint(LeftHandTransform.position, RightHandTransform.position);
                Vector3 diffPos = handsMid - _handsLastPosition;
                if (!isTableGrabbed) diffPos = Vector3.zero; // previous frame's position may be wrong on first try
                diffPos = new Vector3(diffPos.x, 0, diffPos.z); // don't include height difference

                if (!_isOffsetSet)
                {
                    // get vector between transform position and midpoint
                    Vector3 offsetTransformAndMidpoint = new Vector3(handsMid.x, toolTablePlaceholderTr.position.y, handsMid.z) - toolTablePlaceholderTr.position; // exclude height difference
                                                                                                                                                                   // move transform so that transform = midpoint
                    toolTablePlaceholderTr.position = new Vector3(handsMid.x, toolTablePlaceholderTr.position.y, handsMid.z);
                    // counter movement with offset
                    transform.position += new Vector3(-offsetTransformAndMidpoint.x, 0, -offsetTransformAndMidpoint.z);
                    _isOffsetSet = true;
                }

                // add movement difference
                toolTablePlaceholderTr.position += diffPos;

                Vector3 currentConnectingVector = RightHandTransform.position - LeftHandTransform.position;
                float rotationDifferenceAngle = SignedAngleBetween(_handsLastConnectingVector, currentConnectingVector, Vector3.up);
                Vector3 diffRot = new Vector3(0, rotationDifferenceAngle, 0);
                if (!isTableGrabbed) diffRot = Vector3.zero; // previous frame's rotation may be wrong on first try

                toolTablePlaceholderTr.rotation = Quaternion.Euler(diffRot) * toolTablePlaceholderTr.rotation;

                _handsLastPosition = handsMid;
                _handsLastConnectingVector = currentConnectingVector;
                isTableGrabbed = true;
                recordLastTimeHandsRemoved = true;
            }
            else
            {
                isTableGrabbed = false;
                _isOffsetSet = false;
                toolTablePlaceholderTr.position = new Vector3(transform.position.x, toolTablePlaceholderTr.position.y, transform.position.z); // reset
                transform.localPosition = Vector3.zero;
            }

            if (!isTableGrabbed && recordLastTimeHandsRemoved)
            {
                lastTimeRemovedHandsFromTable = DateTime.Now;
                recordLastTimeHandsRemoved = false; // avoid recording this time again
            }
        }
    }
}
