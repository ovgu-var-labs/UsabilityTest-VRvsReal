using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MRITableAudio : MonoBehaviour
{
    //Game objects
    [Header("Game Objects")]
    [SerializeField] private GameObject controllerInOut;
    private AudioSource moveAudio;

    //Scripts
    [Header("Scripts")]
    [SerializeField] private MRI_Rotary_Knob_Interactivity MRIButtonInteractivityControllerLeft;
    [SerializeField] private SlideControlUI SlideControl;

    //Serialized variables
    [Header("Control variables")]
    [Tooltip("Time before the sound starts after press")]
    [SerializeField] private float audioStartDelay = 1f;
    [Tooltip("Time before the sound stops after release")]
    [SerializeField] private float audioEndDelay = 1f;

    //Private variables
    private readonly float in_max = 1.8f;
    private readonly float in_min = 0f;
    private bool audioIsPlaying = false;
    private bool kDown = false;
    private bool kUp = false;
    private bool ignoreNextAudioEnd = false;
    private bool audioStartRunning = false;
    private bool audioStopRunning = false;

    void Start()
    {
        moveAudio = GetComponent<AudioSource>(); //Checks for audio sources in the object
        if (moveAudio == null) { Debug.LogError("AudioSource not found on this GameObject!"); } //Debug message if no audio source is found
    }

    void Update()
    {
        if (MRIButtonInteractivityControllerLeft.tableControllerOn) // Checks if the controller for the table is on
        {
            // DownArrow pressed to move inward
            if (Input.GetKeyDown(KeyCode.DownArrow) && controllerInOut.transform.localPosition.x < in_max)
            {
                //Logic for random input ignore
                if (MRIButtonInteractivityControllerLeft.RandError < 0.15f && SlideControl.mode == SlideControlUI.Mode.withErrors) { ignoreNextAudioEnd = true; }
                else
                {
                    kDown = true;
                    StartCoroutine(AudioStart());
                }
            }

            // UpArrow input to move outward
            if (Input.GetKeyDown(KeyCode.UpArrow) && controllerInOut.transform.localPosition.x > in_min)
            {
                //Logic for random input ignore
                if (MRIButtonInteractivityControllerLeft.RandError < 0.15f && SlideControl.mode == SlideControlUI.Mode.withErrors) { ignoreNextAudioEnd = true; }
                else
                {
                    kUp = true;
                    StartCoroutine(AudioStart());
                }
            }

            //Logic for stopping audio on release or ignoring input release after random input ignore
            if (Input.GetKeyUp(KeyCode.UpArrow))
            {
                if (!ignoreNextAudioEnd)
                {
                    kUp = false;
                    StartCoroutine(AudioStop()); // Starts countdown for audio end
                } 
                ignoreNextAudioEnd = false;
            }

            if (Input.GetKeyUp(KeyCode.DownArrow))
            {
                if (!ignoreNextAudioEnd)
                {
                    kDown = false;
                    StartCoroutine(AudioStop()); // Starts countdown for audio end
                } 
                ignoreNextAudioEnd = false;
            }
            
            // Stop audio at max and min positions
            if (
                controllerInOut.transform.localPosition.x == in_min && audioIsPlaying && kUp
                || controllerInOut.transform.localPosition.x == in_max && audioIsPlaying && kDown
            )
            {
                StopPlayback();
            }

            // Timers
            IEnumerator AudioStart()
            {
                audioStartRunning = true;
                if (!audioIsPlaying || audioStopRunning)
                {
                    if (SlideControl.mode == SlideControlUI.Mode.withErrors) { yield return new WaitForSeconds(audioStartDelay + 1); }
                    else { yield return new WaitForSeconds(audioStartDelay); }
                    StartPlayback();
                }
                audioStartRunning = false;
            }

             IEnumerator AudioStop()
            {
                audioStopRunning = true;
                if (audioIsPlaying || audioStartRunning)
                {
                    if (SlideControl.mode == SlideControlUI.Mode.withErrors) { yield return new WaitForSeconds(audioEndDelay + 1); }
                    else { yield return new WaitForSeconds(audioEndDelay); }
                    if (!kUp && !kDown)
                    {
                        StopPlayback();
                    }
                }
                audioStopRunning = false;
            }
        }
    }

    // Method to start audio playback
    private void StartPlayback()
    {
        moveAudio.Play();
        audioIsPlaying = true;
    }

    // Method to stop audio playback
    private void StopPlayback()
    {
        moveAudio.Stop();
        audioIsPlaying = false;
    }
}
