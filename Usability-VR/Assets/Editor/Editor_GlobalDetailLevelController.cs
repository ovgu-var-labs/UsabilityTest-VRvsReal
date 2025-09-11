using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GlobalDetailLevelController))]

public class Editor_GlobalDetailLevelController : Editor
{
    private GlobalDetailLevelController dtlController;

    SerializedProperty lowQualityObjects;
    SerializedProperty lowQualityObjectsExceptions;

    SerializedProperty highQualityObjects;
    SerializedProperty highQualityObjectsExceptions;

    SerializedProperty applyExcludeLists;

    private void OnEnable()
    {
        dtlController = target as GlobalDetailLevelController;
        if (dtlController == null) return;
        lowQualityObjects = serializedObject.FindProperty("lowQualityObjects");
        highQualityObjects = serializedObject.FindProperty("highQualityObjects");
        lowQualityObjectsExceptions = serializedObject.FindProperty("lowQualityObjectsExceptions");
        highQualityObjectsExceptions = serializedObject.FindProperty("highQualityObjectsExceptions");
        applyExcludeLists = serializedObject.FindProperty("applyExcludeLists");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        
        EditorGUILayout.Space(10);
        EditorGUILayout.PropertyField(applyExcludeLists);

        EditorGUILayout.Space(10);
        if (GUILayout.Button("Enable all objects"))
        {
            dtlController.SetObjectsQualityState(true, true);
        }

        EditorGUILayout.Space(10);
        EditorGUILayout.PropertyField(lowQualityObjects);
        EditorGUILayout.Space(5);
        EditorGUILayout.PropertyField(lowQualityObjectsExceptions);

        if (GUILayout.Button("Enable low quality objects only"))
        {
            dtlController.SetObjectsQualityState(true, false);
        }
        EditorGUILayout.Space(5);

        EditorGUILayout.Space(10);
        EditorGUILayout.PropertyField(highQualityObjects);
        EditorGUILayout.Space(5);
        EditorGUILayout.PropertyField(highQualityObjectsExceptions);
        if (GUILayout.Button("Enable high quality objects only"))
        {
            dtlController.SetObjectsQualityState(false, true);
        }
        EditorGUILayout.Space(30);

        serializedObject.ApplyModifiedProperties();
    }
}
