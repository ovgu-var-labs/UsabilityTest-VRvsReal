using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// This class is a representation of the collected study results of one test subject.
/// </summary>
[Serializable]
public class StudyResultsContainer
{
    public string dateOfStudy; // the date when this study was recorded
    public string userID; // ID of the person who is taking part in the study
    public string timeOfApplicationStartedFirst; // very start of the study
    public string timeOfApplicationUsedLast; // very end of the study (the last time a scenario was ended)

    public List<SceneData> scenes; // for each scene that was played (e.g. low-poly/high-poly)

    [Serializable]
    public class SceneData
    {
        public string scenarioType; // hands, trackers, reality (no headset)
        public string preferredInterventionSide; // left or right of the patient table
        public string detailLevelOfScene; // low-poly / high-poly
        public string laserAngleSetting; // which angle was selected for this scene along the projector ring
        public Vector3 laserDirectionVector; // the laser's direction vector
        public Vector3 laserPositionOnSkin; // where the laser hits the body to mark the desired injection position
        public float wantedNeedleInjectionDepthAbsolute; // the target depth in Unity units (metres).
        public string comment; // optional comment made by the instructor
        public string timeOfScenarioStarted; // when this scenario was started
        public string timeOfScenarioStopped; // when this scenario was stopped

        public string timeOfToolTableMovementStarted; // when the tool table was first moved out of its starting position
        public string timeOfToolTableMovementEnded; // when the tool table was placed in the target area
        public string totalTimeSpanOfTableMovement; // the total time between tool table movement start and end
        public string timeOfTweezersGrabbed; // when the tweezers were first grabbed by a hand
        public string timeOfTweezersDippedFirstTime; // when the tweezers were first dipped in the disinfectant
        public string timeOfTweezersPaintedOnBodyFirstTime; // when the tweezers were first used to "paint" on the body in the first round of disinfection
        public string timeOfTweezersPaintedOnBodyLastTime; // when the tweezers were last used to "paint" on the body in the first round of disinfection, i.e. the last stroke of disinfectant.
        public string timeOfTweezersDropped; // when the tweezers were released after completing the painting process
        
        public List<NeedleInteraction> needleInteractions; // for every interaction that was made with the needle

        [Serializable]
        public class NeedleInteraction
        {
            public string timeOfNeedleGrabbed; // when the needle was grabbed for this interaction
            public string timeOfNeedleReleased; // when the needle was released after this interaction

            public List<AutoNeedleInjection> automaticNeedleInjectionDetections; // all automatically detected needle injections
            public List<ManualNeedleInjection> manualNeedleInjectionDetections; // all manually added needle injections

            [Serializable]
            public class AutoNeedleInjection
            {
                public string timeOfNeedleInjected;
                public bool wasNeedleAlreadyInBodyWhenGrabbed; // whether the needle was already inside the body when it was grabbed
                public bool wasNeedleLiftedOutOfBody; // whether the needle was lifted out of the body

                public Vector3 needleInjectionPoint; // where the needle was injected
                public float differenceInjectionPointToLaserOnSkin; // how close was the final needle position to the desired position

                public Vector3 needleInjectionVector; // direction vector of the injection
                public float differenceInjectionAngleToWantedAngle; // deviation to wanted direction, in degrees

                public float needleInjectionDepth; // how far the needle was injected into the body
                public float differenceInjectionDepthToWantedDepth; // difference to how far the needle should have been injected
            }

            [Serializable]
            public class ManualNeedleInjection
            {
                public string injectionTriggerKeywordOrKeyCode; // which keyword was used for the manual injection
                public string timeOfNeedleInjected;
                public string timeOfNeedleKeywordDetected; // when the user called out the injection keyword the last time
                
                public Vector3 needleInjectionPoint; // where the needle was injected
                public float differenceInjectionPointToLaserOnSkin; // how close was the final needle position to the desired position

                public Vector3 needleInjectionVector; // direction vector of the injection
                public float differenceInjectionAngleToWantedAngle; // deviation to wanted direction, in degrees

                public float needleInjectionDepth; // how far the needle was injected into the body
                public float differenceInjectionDepthToWantedDepth; // difference to how far the needle should have been injected
            }
        }
    }
}
