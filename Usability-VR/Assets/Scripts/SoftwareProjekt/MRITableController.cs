using System;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.GraphicsBuffer;
using static ControlRoom;

public class MRITableController : MonoBehaviour
{
    public bool debugTableIn = false;
    public bool debugTableOut = false;

    [NonSerialized] public bool enableMoveToIsocentre = false;
    [NonSerialized] public bool enableMoveToHomePosition = false;
    [Range(0f, 1f)] public float manualSpeedMultiplier; // the speed multiplier set by the user
    private float maxSpeedMultiplier = 1f;
    private float snappingTolerance; // when to snap to min/max values

    [SerializeField] private ControlRoom controlRoom;
    [SerializeField] private MRI_Rotary_Knob_Interactivity MRIButtonInteractivityControllerLeft;
    [SerializeField] private MRI_Rotary_Knob_Interactivity MRIButtonInteractivityControllerRight;

    [SerializeField] private GameObject controllerUpDown;
    [SerializeField] private GameObject controllerInOut;
    [SerializeField] private GameObject tableLiftUpperPart;
    [SerializeField] private GameObject tableLiftMiddlePart;
    [SerializeField] private GameObject tableLiftLowerPart;

    [SerializeField] private float speed_UpDown;
    private float speed_InOut = 0.002184f; // 0.003361344 * 0,65 quick fix to have the original speed of the mockup
    private readonly float height_min = -0.3f;
    private readonly float height_max = 0f;
    private readonly float in_max = 1.8f;
    private readonly float in_min = 0f;
    private readonly float axisSwitchDelay_ms = 450f; // in ms; time to switch from up to in and vice versa
    private bool axisSwitchDelayEnabled = false;
    private bool isTransitionDelayNecessary = false; // only when actually switching from one movementState to the other
    private float axisSwitchDelayStartTime;
    private MovementStates movementState;
    private bool hasCalibrationLaserBeenToggled; // only allow toggling once per button press and not per frame
    private float isocentreCalibrationValue = -1.0f; // the point on the patient table to move to the iso centre of the MRI. Default (-1) is invalid state (non-calibrated).
    public bool isIsoCentreCalibrated;
    private bool isAutomaticMovementCancelled; // enable to cancel all automatic movements (move to home/iso-centre)

    public delegate void MRI_Table_ControllerEvent(DisplayConfiguration configuration);
    public event MRI_Table_ControllerEvent OnIsoCentreReached;

    enum MovementStates
    {
        UpIn,
        OutDown,
        Neutral
    }

    private void Start()
    {
        snappingTolerance = Mathf.Min(speed_InOut, speed_UpDown) / 2.0f;
        movementState = MovementStates.Neutral;

        MRIButtonInteractivityControllerLeft.OnButtonTopPress += InitiateMoveUpIn;
        MRIButtonInteractivityControllerLeft.OnButtonBottomPress += InitiateMoveOutDown;
        MRIButtonInteractivityControllerLeft.OnButtonLeftPress += InitiateMoveToHome;
        MRIButtonInteractivityControllerLeft.OnButtonRightPress += ToggleCalibrationLaser;
        MRIButtonInteractivityControllerLeft.OnButtonRightExit += AllowCalibrationLaserToggle;
        MRIButtonInteractivityControllerLeft.OnRotaryKnobLongPress += CalibrateIsocentre;
        MRIButtonInteractivityControllerLeft.OnRotaryKnobSinglePress += InitiateMoveToIsoCentre;
        MRIButtonInteractivityControllerLeft.OnRotaryKnobRotate += MoveUpInManually;

        MRIButtonInteractivityControllerRight.OnButtonTopPress += InitiateMoveUpIn;
        MRIButtonInteractivityControllerRight.OnButtonBottomPress += InitiateMoveOutDown;
        MRIButtonInteractivityControllerRight.OnButtonLeftPress += InitiateMoveToHome;
        MRIButtonInteractivityControllerRight.OnButtonRightPress += ToggleCalibrationLaser;
        MRIButtonInteractivityControllerRight.OnButtonRightExit += AllowCalibrationLaserToggle;
        MRIButtonInteractivityControllerRight.OnRotaryKnobLongPress += CalibrateIsocentre;
        MRIButtonInteractivityControllerRight.OnRotaryKnobSinglePress += InitiateMoveToIsoCentre;
    }

    void FixedUpdate()
    {
        if (isAutomaticMovementCancelled)
        {
            enableMoveToHomePosition = false;
            enableMoveToIsocentre = false;
        }

        CheckCalibrationState();
        if (enableMoveToIsocentre) MoveToIsocentre();
        if (enableMoveToHomePosition) MoveToHomePosition();

        if (debugTableIn) InitiateMoveUpIn();
        else if (debugTableOut) InitiateMoveOutDown();
    }

    void InitiateMoveToHome()
    {
        if (enableMoveToHomePosition || enableMoveToIsocentre) // cancel automatic movement when pressed again or other movement was ongoing
        {
            isAutomaticMovementCancelled = true;
            return;
        }

        isAutomaticMovementCancelled = false;
        enableMoveToIsocentre = false; // cancel moving to iso-centre
        enableMoveToHomePosition = true;
    }

    public void InitiateMoveToIsoCentre()
    {
        if (enableMoveToIsocentre || enableMoveToHomePosition) // cancel automatic movement when pressed again or other movement was ongoing
        {
            Debug.Log("Cancelling movement to iso");
            isAutomaticMovementCancelled = true;
            return;
        }

        isAutomaticMovementCancelled = false;
        enableMoveToHomePosition = false; // cancel moving to home position
        enableMoveToIsocentre = true;
    }

    void MoveUpInManually(float angle)
    {
        isAutomaticMovementCancelled = true;
        manualSpeedMultiplier = Mathf.Abs(angle / 90.0f);
        Debug.Log("moving in manually. Speed = " + manualSpeedMultiplier);

        if (angle > 0)
        {
            MoveTable(true, false); // move up/in
        }
        else
        {
            MoveTable(false, true); // move out/down
        }
    }

    void InitiateMoveUpIn()
    {
        isAutomaticMovementCancelled = true;
        // manualSpeedMultiplier = 1; //deactivate for slower delay
        MoveTable(true, false);
    }

    void InitiateMoveOutDown()
    {
        isAutomaticMovementCancelled = true;
        // manualSpeedMultiplier = 1; //deactivate for slower delay
        MoveTable(false, true);
    }

    void AllowCalibrationLaserToggle()
    {
        hasCalibrationLaserBeenToggled = false;
    }

    void ToggleCalibrationLaser()
    {
        if (!hasCalibrationLaserBeenToggled) GetComponent<IsoCentreLaserController>().ToggleLaser();
        hasCalibrationLaserBeenToggled = true;
    }

    void CheckCalibrationState()
    {
        // indicate that the iso centre needs to be recalibrated if patient was moved after calibration
        if (controllerInOut.transform.localPosition.x != isocentreCalibrationValue)
        {
            GetComponent<IsoCentreLaserController>().UpdateLaserColour(IsoCentreLaserController.ColourState.red);
        }
    }

    /// <summary>
    /// Animates the table controller according to the input state.
    /// </summary>
    /// <param name="upIn">Whether the button is pressed to move upwards/inwards.</param>
    /// <param name="outDown">Whether the button is pressed to move outwards/downwards</param>
    /// <param name="currentSpeedMultiplier01">Multiplies speed value by values from 0 to 1.</param>
    void MoveTable(bool upIn, bool outDown)
    {
        if (upIn == outDown) return; // cannot move two axes at the same time

        var currentSpeedMultiplier = Mathf.Clamp01(manualSpeedMultiplier / maxSpeedMultiplier);

        if (upIn)
        {
            var target = new Vector2(controllerInOut.transform.localPosition.x + speed_InOut, controllerUpDown.transform.localPosition.y + speed_UpDown);
            MoveTableToPosition(target, currentSpeedMultiplier);
        }
        else if (outDown)
        {
            var target = new Vector2(controllerInOut.transform.localPosition.x - speed_InOut, controllerUpDown.transform.localPosition.y - speed_UpDown);
            MoveTableToPosition(target, currentSpeedMultiplier);
        }
    }

    public void CalibrateIsocentre()
    {
        isAutomaticMovementCancelled = true;

        if (controllerUpDown.transform.localPosition.y == height_max)
        {
            isocentreCalibrationValue = controllerInOut.transform.localPosition.x;
            GetComponent<IsoCentreLaserController>().UpdateLaserColour(IsoCentreLaserController.ColourState.green); // change red laser to green laser to indicate calibration is done
            isIsoCentreCalibrated = true;
        }
        else
        {
            isocentreCalibrationValue = -1.0f; // set invalid state
            isIsoCentreCalibrated = false;
            Debug.Log("Can't calibrate. Table is not at max height.");
        }
    }

    void MoveToHomePosition()
    {
        Vector2 homePos = new Vector2(in_min, height_max);

        if (controllerInOut.transform.localPosition.x == homePos.x && controllerUpDown.transform.localPosition.y == homePos.y)
        {
            enableMoveToHomePosition = false; // done (home position reached)
        }
        else
        {
            MoveTableToPosition(homePos, 1);
        }
    }

    /// <summary>
    /// Move the patient at a given marker position to the iso centre of the MRI.
    /// </summary>
    void MoveToIsocentre()
    {
        if (isocentreCalibrationValue < in_min || isocentreCalibrationValue > in_max) // do not allow movement if no valid isocentre is calibrated
        {
            isAutomaticMovementCancelled = true;
            return;
        }

        GetComponent<IsoCentreLaserController>().laserMarkerForCalibrationEnabled = false; // deactivate lasers

        float tableOvershootLength = 0.3f; // how far does the patient table reach out of the MRI at head's end
        Vector2 targetPosition = new Vector2(Mathf.Clamp(isocentreCalibrationValue + (in_max - tableOvershootLength) / 2f, 0, in_max), height_max); // iso centre is halfway into MRI. Clamp to maxOut (0) and maxIn (inOut_max).

        if (controllerInOut.transform.localPosition.x == targetPosition.x)
        {
            enableMoveToIsocentre = false; // done (iso-centre reached)
            OnIsoCentreReached?.Invoke(DisplayConfiguration.scan_complete);

            // remove all subscribers
            Delegate[] clientList = OnIsoCentreReached.GetInvocationList();
            foreach (var d in clientList)
                OnIsoCentreReached -= (d as MRI_Table_ControllerEvent);
        }
        else
        {
            MoveTableToPosition(targetPosition, 1);
        }
    }

    /// <summary>
    /// Move the patient table to any position.
    /// </summary>
    /// <param name="targetPosition">x= horizontal target, y = vertical target</param>
    /// <param name="currentSpeedMultiplier">The wanted speed with which to move the table (0 to 1).</param>
    void MoveTableToPosition(Vector2 targetPosition, float currentSpeedMultiplier)
    {
        if (controllerUpDown.transform.localPosition.y < height_max && controllerUpDown.transform.localPosition.y < targetPosition.y && controllerInOut.transform.localPosition.x == in_min)
        {
            // move up
            MoveUpDown(targetPosition.y, currentSpeedMultiplier * 0);  // deactivated vertical movement

            axisSwitchDelayEnabled = false;
            if (movementState == MovementStates.UpIn) isTransitionDelayNecessary = true; // delay only necessary between "up" and "in" states
            movementState = MovementStates.UpIn;
        }
        else if (controllerInOut.transform.localPosition.x > in_min && controllerInOut.transform.localPosition.x > targetPosition.x && controllerUpDown.transform.localPosition.y == height_max)
        {
            // move out
            MoveInOut(targetPosition.x, -currentSpeedMultiplier);
            axisSwitchDelayEnabled = false;
            if (movementState == MovementStates.OutDown) isTransitionDelayNecessary = true; // delay only necessary between "out" and "down" states
            movementState = MovementStates.OutDown;
        }
        else if (!axisSwitchDelayEnabled)
        {
            if (isTransitionDelayNecessary)
            {
                axisSwitchDelayStartTime = Time.time;
            }
            else
            {
                // act as if delay time is already reached if delay is not necessary
                axisSwitchDelayStartTime = Time.time - axisSwitchDelay_ms;
            }

            axisSwitchDelayEnabled = true;
        }
        else if (controllerInOut.transform.localPosition.x < in_max && controllerInOut.transform.localPosition.x < targetPosition.x && controllerUpDown.transform.localPosition.y == height_max)
        {
            if (((Time.time - axisSwitchDelayStartTime) * 1000f >= axisSwitchDelay_ms) || (movementState != MovementStates.UpIn)) // wait for delay to pass or skip delay if previous movement was opposite way (in / out)
            {
                // move in
                MoveInOut(targetPosition.x, currentSpeedMultiplier);
                isTransitionDelayNecessary = false;
            }
        }
        else if (controllerUpDown.transform.localPosition.y > height_min && controllerUpDown.transform.localPosition.y > targetPosition.y && controllerInOut.transform.localPosition.x == in_min)
        {
            if (((Time.time - axisSwitchDelayStartTime) * 1000f >= axisSwitchDelay_ms) || (movementState != MovementStates.OutDown)) // wait for delay to pass or skip delay if previous movement was opposite way (up / down)
            {
                // move down
                MoveUpDown(targetPosition.y, -currentSpeedMultiplier * 0);    // deactivated vertical movement
                isTransitionDelayNecessary = false;
            }
        }

        UpdateLiftParts(controllerUpDown.transform.localPosition.y);
    }

    /// <summary>
    /// Move patient table in or out.
    /// </summary>
    /// <param name="speedMultiplier">Adjust movement speed and direction (+ is in, - is out).</param>
    private void MoveInOut(float target, float speedMultiplier)
    {
        var newInPos = new Vector3(controllerInOut.transform.localPosition.x + speed_InOut * speedMultiplier, height_max, controllerInOut.transform.localPosition.z);
        var newX = newInPos.x; // separate x-position

        // Snap to min/max/target on x-axis
        if (newInPos.x < in_min + snappingTolerance) newX = in_min; // snap to in_min
        else if (newInPos.x > in_max - snappingTolerance) newX = in_max; // snap to in_max
        else if ((target - snappingTolerance) < newInPos.x && newInPos.x < (target + snappingTolerance)) newX = target;

        newInPos = new Vector3(newX, newInPos.y, newInPos.z); // integrate x-position
        controllerInOut.transform.localPosition = newInPos;
    }

    /// <summary>
    /// Move patient table up or down.
    /// </summary>
    /// <param name="speedMultiplier">Adjust movement speed and direction (+ is up, - is down).</param>
    private void MoveUpDown(float target, float speedMultiplier)
    {
        var newUpPos = new Vector3(in_min, controllerUpDown.transform.localPosition.y + speed_UpDown * speedMultiplier, controllerUpDown.transform.localPosition.z);
        var newY = newUpPos.y; // separate y-position

        // Snap to min/max/target on y-axis
        if (newUpPos.y < height_min + snappingTolerance) newY = height_min; // snap to height_min
        else if (newUpPos.y > height_max - snappingTolerance) newY = height_max; // snap to height_max
        else if ((target - snappingTolerance) < newUpPos.y && newUpPos.y < (target + snappingTolerance)) newY = target;

        newUpPos = new Vector3(newUpPos.x, newY, newUpPos.z); // integrate y-position
        controllerUpDown.transform.localPosition = newUpPos;
    }

    /// <summary>
    /// Animate the 3 parts below the table according to the table height.
    /// </summary>
    /// <param name="currentTableControllerHeight">The current y-Position of the table controller (up/down).</param>
    private void UpdateLiftParts(float currentTableControllerHeight)
    {
        /* var percentageTravelledByTable = 1 - (currentTableControllerHeight - height_min) / (height_max - height_min); // 1 if table is all the way down, 0 if table is all the way up
        tableLiftUpperPart.transform.localPosition = new Vector3(tableLiftUpperPart.transform.localPosition.x, -0.265f * percentageTravelledByTable, tableLiftUpperPart.transform.localPosition.z);
        tableLiftMiddlePart.transform.localPosition = new Vector3(tableLiftMiddlePart.transform.localPosition.x, -0.155f * percentageTravelledByTable, tableLiftMiddlePart.transform.localPosition.z);
        tableLiftLowerPart.transform.localPosition = new Vector3(tableLiftLowerPart.transform.localPosition.x, -0.044f * percentageTravelledByTable, tableLiftLowerPart.transform.localPosition.z); */
    }
    /// <summary>
    /// Immediately snap table to home position.
    /// </summary>
    public void ResetToHomeTransform()
    {
        // set horizontal (in/out)
        var inPos = controllerInOut.transform.localPosition;
        inPos.x = in_min;
        controllerInOut.transform.localPosition = inPos;

        // set vertical (up/down)
        var upPos = controllerUpDown.transform.localPosition;
        upPos.y = height_max;
        controllerUpDown.transform.localPosition = upPos;
    }
}
