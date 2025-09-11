using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using static ApplicationSettings;
using UnityEditor.Rendering;

[CustomEditor(typeof(StudyControl))]

public class Editor_StudyControl : Editor
{
    private StudyControl studyControl;

    SerializedProperty testSubjectID;
    SerializedProperty commentForStudyResults;
    SerializedProperty preferredInterventionSide;
    SerializedProperty scenario;
    SerializedProperty sceneDetailLevel;
    SerializedProperty laserAngleInDegrees;
    SerializedProperty insertionMarkerDepth;

    LaserAngles.AngleNames previousAngle;
    bool wasPreviousAngleSaved;
    bool wasPreviousAngleRestored;

    private void OnEnable()
    {
        studyControl = target as StudyControl;
        if (studyControl == null) return;

        testSubjectID = serializedObject.FindProperty("testSubjectID");
        commentForStudyResults = serializedObject.FindProperty("commentForStudyResults");
        preferredInterventionSide = serializedObject.FindProperty("preferredInterventionSide");
        scenario = serializedObject.FindProperty("scenario");
        sceneDetailLevel = serializedObject.FindProperty("sceneDetailLevel");
        laserAngleInDegrees = serializedObject.FindProperty("laserAngleInDegrees");
        insertionMarkerDepth = serializedObject.FindProperty("insertionMarkerDepth");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        //EditorGUILayout.Space(35);
        EditorGUILayout.PropertyField(commentForStudyResults);

        EditorGUI.BeginDisabledGroup(studyControl.IsStudyResultsRecordingActive);
        int space = 1;

        EditorGUILayout.Space(space);
        EditorGUILayout.PropertyField(testSubjectID);
        EditorGUILayout.Space(space);
        EditorGUILayout.PropertyField(scenario);
        EditorGUILayout.Space(space);
        //EditorGUI.BeginDisabledGroup(studyControl.scenario.Equals(ApplicationSettings.Scenario.Real));
        EditorGUILayout.PropertyField(preferredInterventionSide);
        EditorGUILayout.Space(space);
        EditorGUILayout.PropertyField(sceneDetailLevel);
        EditorGUILayout.Space(space);
        //SetCorrectAngleSetting();
        //EditorGUI.EndDisabledGroup();
        EditorGUILayout.PropertyField(laserAngleInDegrees);
        EditorGUILayout.Space(space);
        EditorGUILayout.PropertyField(insertionMarkerDepth);
        EditorGUILayout.Space(space);

        EditorGUI.EndDisabledGroup();

        string buttonText = studyControl.IsStudyResultsRecordingActive ? "Stop recording results" : "Start this scenario";
        if (GUILayout.Button(buttonText))
        {
            if (studyControl.IsStudyResultsRecordingActive)
            {
                studyControl.StopRecording();
            }
            else
            {
                studyControl.SaveScenarioSettings();
                studyControl.RestartScene();
            }
        }

        EditorGUILayout.Space(30);

        serializedObject.ApplyModifiedProperties();
    }

}
