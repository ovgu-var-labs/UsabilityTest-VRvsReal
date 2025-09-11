using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows.Speech;

public class ApplicationSettings 
{
    public static ScenarioSettingsContainer scenarioSettings;

    /// <summary>
    /// Application tags need to exactly reflect the custom tags set in the Unity Editor (Inspector) to work!
    /// </summary>
    public enum ApplicationObjectTags
    {
        Tweezers,
        Needle,
        LeftHand,
        RightHand,
        PaintingTrigger,
        LHand_Index,
        RHand_Index,
        LHand_Thumb,
        RHand_Thumb,
        ToolTable
    }

    public static KeyCode manualInjectionKeyCode = KeyCode.Space;

    /// <summary>
    /// Use underscores ("_") instead of spaces. They will be swapped with spaces.
    /// </summary>
    public enum VoiceControlKeywords
    {
        invalid, // only use as placeholder
        stop, // will be overwritten by StudyControl.manualInjectionKeyword (if it's not an empty string)
        camera_reset,
        camera_to_head,
        camera_to_control,
        camera_to_intervention
    }

    /// <summary>
    /// The available steps in this application.
    /// </summary>
    public enum Steps
    {
        TableSetup,
        Tweezers,
        Needle,
        EndOfApplication
    }

    public enum PreferredInterventionSide
    {
        left,
        right
    }

    public enum Scenario
    {
        VR_without_trackers,
        VR_with_trackers,
        Real
    }

    public enum SceneDetailLevel
    {
        Low,
        High
    }

    public class NeedleMarkerPositions
    {
        public enum MarkerPositions
        {
            _4_5cm,
            _5_5cm,
            _6_5cm,
            _7_5cm,
        }

        // meters
        public static float _4_5cmValue = 0.045f;
        public static float _5_5cmValue = 0.055f;
        public static float _6_5cmValue = 0.065f;
        public static float _7_5cmValue = 0.075f;
    }

    public class LaserAngles
    {
        public enum AngleNames
        {
            _10,
            _15,
            _20,
            _25
        }

        // degree
        public static float _25degrees = 25.0f;
        public static float _20degrees = 20.0f;
        public static float _15degrees = 15.0f;
        public static float _10degrees = 10.0f;
        public static float zero = 0.0f;
    }
}

