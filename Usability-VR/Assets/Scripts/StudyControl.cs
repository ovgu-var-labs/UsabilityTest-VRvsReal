using System;
using System.Linq;
using UnityEngine;
using static ApplicationSettings;

public class StudyControl : MonoBehaviour
{
    private SceneController _sceneController;
    private EventManager _eventManager;

    #region These values can be set  by the user in the inspector to control the state of the application.
    [Tooltip("Set the ID of the current test subject. Use only [\"0-9\", \"A-Z\", \"_\", \"-\"]")] public string testSubjectID;
    [TextArea][Tooltip("This text will be added to the study results for this scene.")] public string commentForStudyResults;
    [Tooltip("Which scenario to run.")] public Scenario scenario;
    [Tooltip("Set the test subject's preferred side.")] public PreferredInterventionSide preferredInterventionSide;
    [Tooltip("The desired detail level of the scene.")] public SceneDetailLevel sceneDetailLevel;
    [Tooltip("Select the desired laser setting (in degrees).")] public LaserAngles.AngleNames laserAngleInDegrees;
    [Tooltip("The position of the insertion marker relative to the needle's length. 0 means the marker is at the tip of the needle.")] public NeedleMarkerPositions.MarkerPositions insertionMarkerDepth;
    #endregion

    [HideInInspector] public float insertionMarkerDepthValue;
    [HideInInspector] public bool IsStudyResultsRecordingActive { get; private set; } = false;

    void Start()
    {
        _sceneController = GetComponent<Studienmanager>().sceneController;
        _eventManager = GetComponent<Studienmanager>().eventManager;
        _eventManager.OnScenarioPreparedAndStarted += ActivateStudyResultsRecording;
        SearchForScenarioSettings();
    }

    private void ActivateStudyResultsRecording(object sender, System.EventArgs e)
    {
        IsStudyResultsRecordingActive = true;
    }

    #region scenario settings
    /// <summary>
    /// Save the scenario settings to runtime storage.
    /// </summary>
    public void SaveScenarioSettings()
    {
        testSubjectID = CleanID(testSubjectID);

        ScenarioSettingsContainer container = new ScenarioSettingsContainer()
        {
            testSubjectID = testSubjectID,
            comment = commentForStudyResults,
            scenario = scenario,
            interventionSide = preferredInterventionSide,
            sceneDetailLevel = sceneDetailLevel,
            laserAngle = laserAngleInDegrees,
            insertionMarkerDepth = insertionMarkerDepth
        };

        ApplicationSettings.scenarioSettings = container;
    }

    /// <summary>
    /// Restart the scene to trigger/start the configured setting.
    /// </summary>
    public void RestartScene()
    {
        _sceneController.ReloadScene();
    }

    /// <summary>
    /// Find the scenario settings and apply them if they're valid.
    /// </summary>
    void SearchForScenarioSettings()
    {
        if (ApplicationSettings.scenarioSettings != null)
        {
            ApplyScenarioSettings(ApplicationSettings.scenarioSettings);

            _eventManager.RaiseOnScenarioSettingsApplied(this);

            Debug.Log("Scenario \"" + scenario.ToString() + "\" loaded successfully at " + DateTime.Now.ToString("HH:mm:ss") + ".\nRecording study results.");
        }
        else
        {
            _eventManager.RaiseOnScenarioNotApplied();
            //Debug.Log("Recording not started.\nSet scenario settings and click \"Start scenario\" in study control.");
        }
    }

    /// <summary>
    /// Apply the scenario settings to this study controller for the user to see.
    /// </summary>
    /// <param name="scenarioSettings">The scenario settings to apply</param>
    void ApplyScenarioSettings(ScenarioSettingsContainer scenarioSettings)
    {
        testSubjectID = scenarioSettings.testSubjectID;
        commentForStudyResults = scenarioSettings.comment;
        preferredInterventionSide = scenarioSettings.interventionSide;
        scenario = scenarioSettings.scenario;
        sceneDetailLevel = scenarioSettings.sceneDetailLevel;
        laserAngleInDegrees = scenarioSettings.laserAngle;
        insertionMarkerDepth = scenarioSettings.insertionMarkerDepth;
        SetInsertionMarkerDepth();
    }

    /// <summary>
    /// Stop the recording of the scenario and allow a new scenario to be configured / started.
    /// </summary>
    public void StopRecording()
    {
        _eventManager.RaiseOnScenarioStopped(this, null);
        IsStudyResultsRecordingActive = false;
    }
    #endregion

    #region helpers
    /// <summary>
    /// Match the actual depth values to the selection.
    /// </summary>
    private void SetInsertionMarkerDepth()
    {
        switch (insertionMarkerDepth)
        {
            case NeedleMarkerPositions.MarkerPositions._4_5cm:
                insertionMarkerDepthValue = NeedleMarkerPositions._4_5cmValue;
                break;
            case NeedleMarkerPositions.MarkerPositions._5_5cm:
                insertionMarkerDepthValue = NeedleMarkerPositions._5_5cmValue;
                break;
            case NeedleMarkerPositions.MarkerPositions._6_5cm:
                insertionMarkerDepthValue = NeedleMarkerPositions._6_5cmValue;
                break;
            case NeedleMarkerPositions.MarkerPositions._7_5cm:
                insertionMarkerDepthValue = NeedleMarkerPositions._7_5cmValue;
                break;
        }
    }

    /// <summary>
    /// Retrieve the cleaned test subject ID.
    /// </summary>
    /// <returns>The cleaned string.</returns>
    public string GetTestSubjectID()
    {
        return CleanID(testSubjectID);
    }

    /// <summary>
    /// Cleans the given ID to include only allowed characters.
    /// </summary>
    /// <param name="IDToClean"></param>
    /// <returns></returns>
    private string CleanID(string IDToClean)
    {
        // only allow ["0-9", "A-Z", "_", "-"]
        return new string((from c in IDToClean
                           where char.IsNumber(c) || char.IsLetter(c) || c.Equals('_') || c.Equals('-')
                           select c
        ).ToArray());
    }
    #endregion
}
