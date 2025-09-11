using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Editor script to enable / disable static state for selected objects.
/// </summary>

[CustomEditor(typeof(DefineDynamicObjects))]

public class Editor_DefineDynamicObjects : Editor
{
    private DefineDynamicObjects manager;

    SerializedProperty dynamicObjects;
    SerializedProperty topmostParent;
    SerializedProperty alsoChangeChildren;

    private void OnEnable()
    {
        manager = target as DefineDynamicObjects;
        if (manager == null) return;
        topmostParent = serializedObject.FindProperty("topmostParent");
        dynamicObjects = serializedObject.FindProperty("dynamicObjects");
        alsoChangeChildren = serializedObject.FindProperty("alsoChangeChildren");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.Space(10);
        EditorGUILayout.PropertyField(topmostParent);
        EditorGUILayout.Space(5);

        if (GUILayout.Button("Set all objects to static"))
        {
            GameObject[] allObjects = { manager.topmostParent };
            manager.SetObjectsState(allObjects, true, true);
        }

        if (GUILayout.Button("Set all high quality objects to static"))
        {
            GameObject[] highQualityObjects = manager.GetDetailLevelController().highQualityObjects;
            manager.SetObjectsState(highQualityObjects, true, true);
        }

        if (GUILayout.Button("Set all low quality objects to static"))
        {
            GameObject[] lowQualityObjects = manager.GetDetailLevelController().lowQualityObjects;
            manager.SetObjectsState(lowQualityObjects, true, true);
        }

        EditorGUILayout.Space(15);

        if (GUILayout.Button("Set all objects to dynamic"))
        {
            GameObject[] tempList = { manager.topmostParent };
            manager.SetObjectsState(tempList, false, true);
        }

        if (GUILayout.Button("Set all high quality objects to dynamic"))
        {
            GameObject[] highQualityObjects = manager.GetDetailLevelController().highQualityObjects;
            manager.SetObjectsState(highQualityObjects, false, true);
        }

        if (GUILayout.Button("Set all low quality objects to dynamic"))
        {
            GameObject[] lowQualityObjects = manager.GetDetailLevelController().lowQualityObjects;
            manager.SetObjectsState(lowQualityObjects, false, true);
        }

        EditorGUILayout.Space(15);
        EditorGUILayout.PropertyField(dynamicObjects);
        EditorGUILayout.PropertyField(alsoChangeChildren);
        EditorGUILayout.Space(5);

        if (GUILayout.Button("Set objects in list to dynamic"))
        {
            manager.SetObjectsState(manager.dynamicObjects, false, manager.alsoChangeChildren);
        }

        if (GUILayout.Button("Set objects in list to static"))
        {
            manager.SetObjectsState(manager.dynamicObjects, true, manager.alsoChangeChildren);
        }

        EditorGUILayout.Space(30);

        serializedObject.ApplyModifiedProperties();
    }
}
