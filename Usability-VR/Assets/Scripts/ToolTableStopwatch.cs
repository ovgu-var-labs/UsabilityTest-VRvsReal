using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ApplicationSettings;

public class ToolTableStopwatch : MonoBehaviour
{
    public Studienmanager studienmanager;
    public EventManager eventManager;
    private const float detectionThreshold = 0.05f; // the threshold in Unity units (meters) for when to detect the tool table was initially moved from its start position
    public static readonly double timeUntilToolTableFreeze = 0.75f; // how long until the tool table will freeze when let go (in seconds)

    private ToolTableController tableController;
    private Transform tableStart; // the transform of the tool table's start position
    private bool _movementStartRecorded; // whether the time when the tool table was first moved was recorded
    private bool _isDistanceCheckEnabled; // whether to evaluate the distance between start position and current position of table


    private void Start()
    {
        _isDistanceCheckEnabled = false;
    }

    public void EnableDistanceCheck()
    {
        tableController = GetComponent<ToolTableController>();
        tableStart = tableController.tableStart.transform;

        //// testing (spawn a cube at tableStart position)
        //var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        //cube.transform.position = tableStart.transform.position;

        _isDistanceCheckEnabled = true;
        _movementStartRecorded = false;
    }

    private void FixedUpdate()
    {
        if (!_isDistanceCheckEnabled) return;

        // check distance between current table position and starting position
        float num = transform.position.x - tableStart.position.x;
        float num2 = transform.position.z - tableStart.position.z;
        var distance = Mathf.Sqrt(num * num + num2 * num2);

        // if the distance to the start was big enough, start recording movement interaction time
        if (!_movementStartRecorded && distance > detectionThreshold)
        {
            studienmanager.RecordToolTableMovementStart();
            _movementStartRecorded = true;
        }
    }

    /// <summary>
    /// When table is in final place. Send the recorded time stamp when the table was let go to the study manager.
    /// </summary>
    public void RecordTableMovementEnd(DateTime timeOfMovementEnded)
    {
        studienmanager.RecordToolTableMovementEnd(timeOfMovementEnded);
        eventManager.RaiseOnSoundEffectPlayed(eventManager.soundEffectsManager.defaultNotification);
        eventManager.RaiseOnToolTablePlacementFinished();
    }
}
