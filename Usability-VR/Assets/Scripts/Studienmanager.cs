using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using static UnityEngine.Networking.UnityWebRequest;

public class Studienmanager : MonoBehaviour
{
    public StudyControl studyControl;
    public Ablaufmanager ablaufmanager;
    public EventManager eventManager;
    public SceneController sceneController;

    StudyResultsContainer studyResultsContainer;

    public readonly string detailedTimeStamp = "HH:mm:ss.fff"; // 23:59:59.999

    private bool _isRecordingData;
    //public delegate void SceneCreation(StudyResultsContainer.SceneData scene);
    //public event SceneCreation OnSceneDataCreated;

    private void Start()
    {
        _isRecordingData = false;
        eventManager.OnScenarioStopped += SaveRecordedData;
    }

    void Update()
    {
        // manual injection by pressing a button instead of using voice control
        if (Input.GetKeyDown(ApplicationSettings.manualInjectionKeyCode))
        {
            eventManager.RaiseOnManualInjection(ApplicationSettings.manualInjectionKeyCode.ToString().ToLower());
        }
    }

    /// <summary>
    /// When the Unity player was exited by the user, save results.
    /// </summary>
    private void OnApplicationQuit()
    {
        SaveRecordedData();
    }

    /// <summary>
    /// Save all recorded data to a file.
    /// </summary>
    void SaveRecordedData(object sender = null, EventArgs e = null)
    {
        if (!_isRecordingData) return; // don't save useless/empty data

        AddComment();
        studyResultsContainer.scenes.Last().timeOfScenarioStopped = DateTime.Now.ToString(detailedTimeStamp);
        studyResultsContainer.timeOfApplicationUsedLast = DateTime.Now.ToString(detailedTimeStamp);
        var exportPath = ExportJSONFromResultsContainer(studyResultsContainer, false); // export recordings
        Debug.Log("Successfully recorded data from scenario \"" + studyControl.scenario.ToString() + "\" at " + DateTime.Now.ToString("HH:mm:ss") + ".\nExported to " + exportPath);

        _isRecordingData = false;
    }

    /// <summary>
    /// Add a comment to the study data.
    /// </summary>
    void AddComment()
    {
        studyResultsContainer.scenes.Last().comment += studyControl.commentForStudyResults; // add a comment to the current scene
        studyControl.commentForStudyResults = "no comment"; // reset
    }

    /// <summary>
    /// Initiate this study manager.
    /// Only then will it start looking for existing data
    /// or create new skeleton data to work with.
    /// </summary>
    public void InitiateStudyManager()
    {
        _isRecordingData = true;
        sceneController.OnSceneReloadKeyPressed += SaveCurrentProgress;
        FillStudyResultsContainer();
        eventManager.RaiseOnScenarioPreparedAndStarted();
    }

    /// <summary>
    /// Get the path of the JSON file which stores the current test subject's data.
    /// </summary>
    /// <param name="userID">The ID of the current test subject.</param>
    /// <param name="addTimeStamp">Whether to add the current time to the file path.</param>
    /// <returns>The file path to the JSON file for the current test subject.</returns>
    string GetFilePathForCurrentTestSubject(string userID, bool addTimeStamp)
    {
        string currentTime = "";
        if (addTimeStamp) currentTime = DateTime.Now.ToString("yyyy-MM-dd");

        return Application.persistentDataPath + "/studyResults_at_" + currentTime + "_of_ID-" + userID + ".json";
    }

    /// <summary>
    /// Fill the studyResultsContainer with already saved data.
    /// If there is none, create a new representation.
    /// </summary>
    void FillStudyResultsContainer()
    {
        studyResultsContainer = ImportJSONAsResultsContainer(GetFilePathForCurrentTestSubject(studyControl.GetTestSubjectID(), true));
    }

    /// <summary>
    /// Takes string <paramref name="jsonData"/> and creates a .json file from it.
    /// </summary>
    /// <param name="results">The results data to include in the JSON file.</param>
    /// <param name="isSnapshot">Whether to add suffix indicating it is just a temporary snapshot/backup.</param>
    /// <returns>The path to the saved JSON file.</returns>
    string ExportJSONFromResultsContainer(StudyResultsContainer results, bool isSnapshot)
    {
        string jsonData = JsonUtility.ToJson(results);
        string suffix = "";
        if (isSnapshot) suffix = "_snapshot";
        string filePath = GetFilePathForCurrentTestSubject(results.userID, true) + suffix;
        StreamWriter writer = new StreamWriter(filePath);
        writer.Write(jsonData);
        writer.Close();
        return filePath;
    }

    /// <summary>
    /// Takes a JSON file specified in <paramref name="filePath"/> and creates a representation as StudyResultsContainer.
    /// </summary>
    /// <param name="filePath">The complete path to the JSON file.</param>
    /// <returns>The StudyResultsContainer containing the JSON data.</returns>
    public StudyResultsContainer ImportJSONAsResultsContainer(string filePath)
    {
        if (File.Exists(filePath))
        {
            string jsonString = File.ReadAllText(filePath);
            var container = JsonUtility.FromJson<StudyResultsContainer>(jsonString);
            var sceneData = GenerateSceneData();
            container.scenes.Add(sceneData); // add current scene's data to existing list
            //OnSceneDataCreated?.Invoke(sceneData);
            return container;
        }
        else
        {
            Debug.Log("No matching JSON found at " + filePath + "\n Creating new representation.");
            return CreateFreshResultsContainer();
        }
    }

    /// <summary>
    /// Save the current progress to a JSON file.
    /// </summary>
    void SaveCurrentProgress()
    {
        ExportJSONFromResultsContainer(studyResultsContainer, false);
    }

    StudyResultsContainer CreateFreshResultsContainer()
    {
        // create must-have basic data
        var container = new StudyResultsContainer
        {
            dateOfStudy = DateTime.Now.ToString("yyyy-MM-dd"),
            userID = studyControl.GetTestSubjectID(),
            timeOfApplicationStartedFirst = DateTime.Now.ToString(detailedTimeStamp),
            scenes = new List<StudyResultsContainer.SceneData>()
        };

        var sceneData = GenerateSceneData();
        container.scenes.Add(sceneData); // add current scene's data to existing list
        //OnSceneDataCreated?.Invoke(sceneData);

        return container;
    }

    /// <summary>
    /// Generate basic scene data for the current scene.
    /// </summary>
    StudyResultsContainer.SceneData GenerateSceneData()
    {
        // use current scene's data
        return new StudyResultsContainer.SceneData
        {
            scenarioType = studyControl.scenario.ToString(),
            preferredInterventionSide = studyControl.preferredInterventionSide.ToString(),
            detailLevelOfScene = studyControl.sceneDetailLevel.ToString(),
            timeOfScenarioStarted = DateTime.Now.ToString(detailedTimeStamp),
            needleInteractions = new List<StudyResultsContainer.SceneData.NeedleInteraction>(),
        };
    }

    /// <summary>
    /// Update laser values in study results.
    /// </summary>
    public void LogLaserValues()
    {
        var container = studyResultsContainer.scenes.Last();
        container.laserAngleSetting = studyControl.laserAngleInDegrees.ToString();
        container.laserPositionOnSkin = ablaufmanager.laserProjector.GetComponent<LaserCollisionDetector>().LaserTargetPositionOnBody;
        container.laserDirectionVector = ablaufmanager.laserProjector.GetComponent<LaserCollisionDetector>().LaserVector;
    }

    #region tool table data collection

    /// <summary>
    /// Records when the tool table (which holds the tweezers and needle) was moved for the first time.
    /// </summary>
    public void RecordToolTableMovementStart()
    {
        studyResultsContainer.scenes.Last().timeOfToolTableMovementStarted = DateTime.Now.ToString(detailedTimeStamp);
    }

    /// <summary>
    /// Records when the tool table (which holds the tweezers and needle) was moved for the last time.
    /// Calculates how long it took from grabbing the table for the first time to letting it go the last time.
    /// </summary>
    public void RecordToolTableMovementEnd(DateTime dt)
    {
        studyResultsContainer.scenes.Last().timeOfToolTableMovementEnded = dt.ToString(detailedTimeStamp);

        DateTime endTime = DateTime.Parse(studyResultsContainer.scenes.Last().timeOfToolTableMovementEnded);
        DateTime startTime = endTime;
        string optionalWarning = "";

        if (studyResultsContainer.scenes.Last().timeOfToolTableMovementStarted != null)
        {
            startTime = DateTime.Parse(studyResultsContainer.scenes.Last().timeOfToolTableMovementStarted);
        }
        else
        {
            // in case there was no recorded start time, subtract the freeze time as the tooltable must have been placed inside a target at the start of the scenario
            startTime = startTime.AddSeconds((-1.0) * ToolTableStopwatch.timeUntilToolTableFreeze);
            studyResultsContainer.scenes.Last().timeOfToolTableMovementStarted = startTime.ToString(detailedTimeStamp);
            optionalWarning = " - WARNING! Start time of tool table movement was not available and was merely auto-completed! " +
                "This scenario's run might be invalid! Tool table was probably already placed inside target at start of scenario.";

            Debug.LogError("!!!-+-+-+-+- " +
                "Tool table start and end time are the same! Please restart the scenario." + " -+-+-+-+-!!!" +
                "\nMake sure to position the tool table in the starting position and place all tools on the table (not in the disinfection fluid).");
        }

        studyResultsContainer.scenes.Last().totalTimeSpanOfTableMovement = endTime.Subtract(startTime).TotalSeconds.ToString() + " seconds" + optionalWarning;
    }
    #endregion

    #region tweezers data collection
    /// <summary>
    /// When the tweezers were first grabbed from the table.
    /// </summary>
    public void TweezersGrabbed()
    {
        studyResultsContainer.scenes.Last().timeOfTweezersGrabbed = DateTime.Now.ToString(detailedTimeStamp);
    }

    /// <summary>
    /// The first time the tweezers were dipped into the disinfectant.
    /// </summary>
    public void TweezersDipped()
    {
        studyResultsContainer.scenes.Last().timeOfTweezersDippedFirstTime = DateTime.Now.ToString(detailedTimeStamp);
    }

    /// <summary>
    /// Log the time when the disinfection was started (when the soaked tweezers touch the body).
    /// </summary>
    /// <param name="firstTime">Whether the log is considering the first stroke of disinfectant (true) or the last stroke (false).</param>
    public void TweezersPaintDisinfection(bool firstTime)
    {
        var scene = studyResultsContainer.scenes.Last();
        if (firstTime)
        {
            scene.timeOfTweezersPaintedOnBodyFirstTime = DateTime.Now.ToString(detailedTimeStamp);
        }
        else
        {
            scene.timeOfTweezersPaintedOnBodyLastTime = DateTime.Now.ToString(detailedTimeStamp);
        }
    }

    /// <summary>
    /// When the tweezers were dropped onto the table.
    /// </summary>
    public void TweezersDropped()
    {
        studyResultsContainer.scenes.Last().timeOfTweezersDropped = DateTime.Now.ToString(detailedTimeStamp);
    }
    #endregion

    #region needle data collection
    /// <summary>
    /// Return all needle interactions (not injections!).
    /// </summary>
    /// <returns>List of all needle interactions made so far.</returns>
    private List<StudyResultsContainer.SceneData.NeedleInteraction> GetAllNeedleInteractions()
    {
        return studyResultsContainer.scenes.Last().needleInteractions;
    }

    /// <summary>
    /// Of all needle interactions made so far, return the last one.
    /// </summary>
    /// <returns></returns>
    private StudyResultsContainer.SceneData.NeedleInteraction GetCurrentNeedleInteraction()
    {
        if (!GetAllNeedleInteractions().Any())
        {
            GetAllNeedleInteractions().Add(new StudyResultsContainer.SceneData.NeedleInteraction());
        }
        return GetAllNeedleInteractions().Last();
    }

    private List<StudyResultsContainer.SceneData.NeedleInteraction.AutoNeedleInjection> GetAllNeedleInjections()
    {
        return GetCurrentNeedleInteraction().automaticNeedleInjectionDetections;
    }

    /// <summary>
    /// Of all the injections made so far, return the last one.
    /// </summary>
    /// <returns>The last injection made.</returns>
    private StudyResultsContainer.SceneData.NeedleInteraction.AutoNeedleInjection GetCurrentInjection()
    {
        if (GetAllNeedleInjections() == null || !GetAllNeedleInjections().Any())
        {
            return null;
        }
        else
        {
            return GetAllNeedleInjections().Last();
        }
    }

    /// <summary>
    /// Update the target depth.
    /// </summary>
    /// <param name="wantedDepthPercent">The new target depth in relation to the needle's full length.</param>
    /// <param name="wantedDepthAbsolute">The new target depth in Unity units (metres).</param>
    public void UpdateWantedInjectionDepth(float wantedDepthAbsolute)
    {
        studyResultsContainer.scenes.Last().wantedNeedleInjectionDepthAbsolute = wantedDepthAbsolute;
    }

    /// <summary>
    /// Needle is grabbed by the user.
    /// </summary>
    public void NeedleGrabbed()
    {
        GetAllNeedleInteractions()
            .Add(new StudyResultsContainer.SceneData.NeedleInteraction
            {
                timeOfNeedleGrabbed = DateTime.Now.ToString(detailedTimeStamp)
            });

        // if needle is grabbed again after it was released in the body (i.e. still being injected), fill the new interaction with a new injection
        if (ablaufmanager.isNeedleInjected)
        {
            GetCurrentNeedleInteraction().automaticNeedleInjectionDetections = new List<StudyResultsContainer.SceneData.NeedleInteraction.AutoNeedleInjection>();
            GetAllNeedleInjections().Add(new StudyResultsContainer.SceneData.NeedleInteraction.AutoNeedleInjection { timeOfNeedleInjected = DateTime.Now.ToString(detailedTimeStamp), wasNeedleAlreadyInBodyWhenGrabbed = true });
        }
    }

    /// <summary>
    /// Needle is injected into body.
    /// </summary>
    public void AddAutomaticNeedleInjection()
    {
        if (!studyControl.scenario.Equals(ApplicationSettings.Scenario.VR_without_trackers)) return; // only record automatic injections in no-tracker scenario

        var newInjection = new StudyResultsContainer.SceneData.NeedleInteraction.AutoNeedleInjection { timeOfNeedleInjected = DateTime.Now.ToString(detailedTimeStamp), wasNeedleAlreadyInBodyWhenGrabbed = false };
        if (GetAllNeedleInjections() == null)
        {
            GetCurrentNeedleInteraction().automaticNeedleInjectionDetections = new List<StudyResultsContainer.SceneData.NeedleInteraction.AutoNeedleInjection>();
        }

        GetAllNeedleInjections().Add(newInjection);
    }

    public void AddManualNeedleInjection(string injectionKeyword, string timeOfFirstInjection, Vector3 injectionPoint, float injectionPositionDeviation, float injectionDepth, float injectionDepthToMarkerDeviation, Vector3 injectionDirection, float injectionAngleDeviationToLaser)
    {
        if (GetCurrentNeedleInteraction().manualNeedleInjectionDetections == null) GetCurrentNeedleInteraction().manualNeedleInjectionDetections = new List<StudyResultsContainer.SceneData.NeedleInteraction.ManualNeedleInjection>();

        var manualInjection = new StudyResultsContainer.SceneData.NeedleInteraction.ManualNeedleInjection
        {
            injectionTriggerKeywordOrKeyCode = injectionKeyword,
            timeOfNeedleInjected = timeOfFirstInjection,
            timeOfNeedleKeywordDetected = DateTime.Now.ToString(detailedTimeStamp),
            needleInjectionPoint = injectionPoint,
            differenceInjectionPointToLaserOnSkin = injectionPositionDeviation,
            needleInjectionVector = injectionDirection,
            differenceInjectionAngleToWantedAngle = injectionAngleDeviationToLaser,
            needleInjectionDepth = injectionDepth,
            differenceInjectionDepthToWantedDepth = injectionDepthToMarkerDeviation
        };

        GetCurrentNeedleInteraction().manualNeedleInjectionDetections.Add(manualInjection);
    }

    /// <summary>
    /// For the current injection, update its values.
    /// </summary>
    /// <param name="injectionPoint">The injection's position on the body.</param>
    /// <param name="needleInjectionPositionDeviation">The injection point's deviation from the target (laser).</param>
    /// <param name="injectionDepth">How far the needle is inserted into the body.</param>
    /// <param name="injectionDepthToMarkerDeviation">How far the injection depth differs from the wanted depth (marker on needle).</param>
    /// <param name="injectionAngleDeviationToLaser">In degree, how far the needle vector varies from the laser vector.</param>
    public void UpdateNeedleInjection(Vector3 injectionPoint, float needleInjectionPositionDeviation, float injectionDepth, float injectionDepthToMarkerDeviation, Vector3 injectionVector, float injectionAngleDeviationToLaser)
    {
        if (!studyControl.scenario.Equals(ApplicationSettings.Scenario.VR_without_trackers)) return; // don't record automatic injections if tangibles are used

        var injection = GetCurrentInjection();
        if (injection == null) return;
        injection.needleInjectionPoint = injectionPoint;
        injection.differenceInjectionPointToLaserOnSkin = needleInjectionPositionDeviation;
        injection.needleInjectionDepth = injectionDepth;
        injection.differenceInjectionDepthToWantedDepth = injectionDepthToMarkerDeviation;
        injection.differenceInjectionAngleToWantedAngle = injectionAngleDeviationToLaser;
        injection.needleInjectionVector = injectionVector;
    }

    /// <summary>
    /// Needle is lifted out of body.
    /// </summary>
    public void LiftNeedle()
    {
        var injection = GetCurrentInjection();
        if (injection != null) injection.wasNeedleLiftedOutOfBody = true;
    }

    /// <summary>
    /// Needle is released from hand.
    /// </summary>
    public void ReleaseNeedle()
    {
        GetCurrentNeedleInteraction().timeOfNeedleReleased = DateTime.Now.ToString(detailedTimeStamp);
    }
    #endregion
}
