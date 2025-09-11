using System.Collections;
using System;
using UnityEngine;
using Valve.VR.InteractionSystem;

public class MRI_Rotary_Knob_Interactivity : MonoBehaviour
{
    // Game Objects
    [Header("Game Objects")]
    public GameObject rotaryKnob;
    public GameObject button_top;
    public GameObject button_bottom;
    public GameObject button_left;
    public GameObject button_right;
    public GameObject TableProgramm;

    // Scripts
    [Header("Scripts")]
    public MRITableController MRI_controller;
    [SerializeField] private SlideControlUI SlideControl;

    // Public variables
    [Header("Control Variables")]
    [Tooltip("Turns the keyboard control of the table on and off")]
    public bool tableControllerOn = false; 
    [Tooltip("Time before table starts moving after press")]
    public float pressDelay = 1f;         
    [Tooltip("Time before table stops moving after release")]
    public float releaseDelay = 0.8f;             
    [Tooltip("Amount substracted to the speed multiplier every frame after release")]
    public float speedReductionOnRelease = 0.008f;      
    [NonSerialized] public float RandError;

    // Private variables
    private bool upArrowIsPressed = false;
    private bool downArrowIsPressed = false;
    private bool ignoreNextDownRelease = false;
    private bool ignoreNextUpRelease = false;
    private bool releaseDownPhase = false;
    private bool releaseUpPhase = false;
    private bool keyUp = false;
    private bool keyDown = false;
    private float _longPressDuration_ms = 1000f; // how long a button needs to be pressed to trigger press-and-hold functionality
    private float speedMult = 1f;

    // Button press and rotate events
    public delegate void ButtonPress();
    public event ButtonPress OnButtonTopPress;
    public event ButtonPress OnButtonBottomPress;
    public event ButtonPress OnButtonLeftPress;
    public event ButtonPress OnButtonRightPress;
    public event ButtonPress OnButtonRightExit;
    public event ButtonPress OnRotaryKnobSinglePress;
    public event ButtonPress OnRotaryKnobLongPress;
    public delegate void ButtonRotate(float absoluteDegree);
    public event ButtonRotate OnRotaryKnobRotate;


    private void Update()
    {
        RandError = UnityEngine.Random.value;  //Sets to a random number for error calculations

        // startet automatische Bewegung zur Home‑Position
        if (Input.GetKeyDown(KeyCode.Space))
        {
            MRI_controller.enableMoveToHomePosition = true;
            MRI_controller.ResetToHomeTransform();          // snap table instantly to home
        }
        
        // Turns the table controller on and off depending on the current phase of the color sorting program
        if (TableProgramm.activeInHierarchy || SlideControl.tutorialPhaseA) {tableControllerOn = true;}
        else{tableControllerOn = false;}

        // Activates movement for down arrow and reduces the speed multiplier after release
        if (downArrowIsPressed || releaseDownPhase)  
        {
            speedMult = releaseDownPhase ? speedMult - speedReductionOnRelease : 1f;  // The speed is only reduced if the release phase flag is active (reduction continues every frame)
            MRI_controller.manualSpeedMultiplier = speedMult;
            OnButtonTopPress?.Invoke();     // Invokes event that activates movement
        }

        // Activates movement for up arrow and reduces the speed multiplier after release
        if (upArrowIsPressed || releaseUpPhase)
        {
            speedMult = releaseUpPhase ? speedMult - speedReductionOnRelease : 1f;  // The speed is only reduced if the release phase flag is active (reduction continues every frame)
            MRI_controller.manualSpeedMultiplier = speedMult;
            OnButtonBottomPress?.Invoke();         // Invokes event that activates movement
        }

        if (tableControllerOn)
        {
            // Logic for pressing down arrow
            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                // random ignore input (only if under the 3‐ignore limit)
                if (RandError < 0.15f && SlideControl.mode == SlideControlUI.Mode.withErrors && SlideControl.falseNegRoundCounter < 3)
                {
                    SlideControl.falseNegA++;
                    SlideControl.falseNegRoundCounter++;  // count this ignore
                    ignoreNextDownRelease = true;         // Flag for ignoring key release
                }
                else
                {
                    keyDown = true;    // Flag that indicates if the key is pressed
                    StartCoroutine(ButtonDownPress());      // Starts timer
                }
            }

            //Logic for releasing down arrow
            if (Input.GetKeyUp(KeyCode.DownArrow))
            {
                if (!ignoreNextDownRelease)
                {
                    keyDown = false;    // Flag that indicates if the key is pressed
                    StartCoroutine(ButtonDownRelease());      // Starts timer
                }
                ignoreNextDownRelease = false;
            }

            // Logic for pressing up arrow
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                // random ignore input (only if under the 3‐ignore limit)
                if (RandError < 0.15f && SlideControl.mode == SlideControlUI.Mode.withErrors && SlideControl.falseNegRoundCounter < 3)
                {
                    SlideControl.falseNegA++;
                    SlideControl.falseNegRoundCounter++;  // count this ignore
                    ignoreNextUpRelease = true;           // Flag for ignoring key release
                }
                else
                {
                    keyUp = true;    // Flag that indicates if the key is pressed
                    StartCoroutine(ButtonUpPress());    // Starts timer
                }
            }

            //Logic for releasing up arrow
            if (Input.GetKeyUp(KeyCode.UpArrow))
            {
                if (!ignoreNextUpRelease)
                {
                    keyUp = false;    // Flag that indicates if the key is pressed
                    StartCoroutine(ButtonUpRelease());     // Starts timer
                }
                ignoreNextUpRelease = false;
            }

            //Timers
            IEnumerator ButtonDownPress()
            {
                if (SlideControl.mode == SlideControlUI.Mode.withErrors) { yield return new WaitForSeconds(pressDelay + 1); }
                else { yield return new WaitForSeconds(pressDelay); }   // Selection of different delay times for mode with errors
                releaseUpPhase = false;          //Resseting flags for speed reduction
                releaseDownPhase = false;
                downArrowIsPressed = true;       //Flag to activate movement
            }

            IEnumerator ButtonDownRelease()
            {
                releaseDownPhase = true;  // set release-phase (speed reduction) flag before Delay
                if (SlideControl.mode == SlideControlUI.Mode.withErrors) { yield return new WaitForSeconds(releaseDelay + 1 - pressDelay); }
                else { yield return new WaitForSeconds(releaseDelay); }
                if (!keyDown)                      // Checks if the key is currently pressed to avoid interrupting the movement
                {                                  // (Used to avoid unwanted behaviour with quick consecutive presses)
                    releaseDownPhase = false;
                    downArrowIsPressed = false;
                }
            }

            IEnumerator ButtonUpPress()
            {
                if (SlideControl.mode == SlideControlUI.Mode.withErrors) { yield return new WaitForSeconds(pressDelay + 1); }
                else { yield return new WaitForSeconds(pressDelay); }    // Selection of different delay times for mode with errors
                releaseUpPhase = false;        //Resseting flags for speed reduction
                releaseDownPhase = false;
                upArrowIsPressed = true;       //Flag to activate movement
            }

            IEnumerator ButtonUpRelease()
            {
                releaseUpPhase = true;        // set release-phase (speed reduction) flag before Delay
                if (SlideControl.mode == SlideControlUI.Mode.withErrors) { yield return new WaitForSeconds(releaseDelay + 1 - pressDelay); }
                else { yield return new WaitForSeconds(releaseDelay); }
                if (!keyUp)                                     // Checks if the key is currently pressed to avoid interrupting the movement
                {                                               // (Used to avoid unwanted behaviour with quick consecutive presses)
                     upArrowIsPressed = false;
                     releaseUpPhase = false;
                }
               
            }
        }
    }

    public void ButtonActivity(GameObject sender, bool isButtonPressed, float activityStartTime, float rotatioAngle = -180.0f)
    {
        if (sender == rotaryKnob)
        {
            var howLongIsButtonPressed_ms = (Time.time - activityStartTime) * 1000f;

            if (!isButtonPressed && howLongIsButtonPressed_ms <= _longPressDuration_ms) // invoke single-press only when exiting the button shortly after pressing (because of additional press-and-hold functionality)
            {
                OnRotaryKnobSinglePress?.Invoke(); // move to isocentre
            }
            else if (isButtonPressed && howLongIsButtonPressed_ms > _longPressDuration_ms)
            {
                OnRotaryKnobLongPress?.Invoke(); // calibrate isocentre
            }
            else if (isButtonPressed && rotatioAngle >= -90 && rotatioAngle <= 90)
            {
                OnRotaryKnobRotate.Invoke(rotatioAngle);
            }
        }
        else if (sender == button_top)
        {
            if (isButtonPressed) OnButtonTopPress?.Invoke(); // move up / in
        }
        else if (sender == button_bottom)
        {
            if (isButtonPressed) OnButtonBottomPress?.Invoke(); // move out / down
        }
        else if (sender == button_left && isButtonPressed) // move home
        {
            OnButtonLeftPress?.Invoke();
        }
        else if (sender == button_right)
        {
            if (isButtonPressed) OnButtonRightPress?.Invoke(); // turn iso-centre calibration laser marker on / off
            else OnButtonRightExit?.Invoke();
        }
    }
}
