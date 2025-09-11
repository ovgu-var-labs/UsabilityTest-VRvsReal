using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ResetTransformScript))]
public class ResetTransformFromEditor : Editor
{
    public override void OnInspectorGUI()
    {
        ResetTransformScript reset = (ResetTransformScript)target;
        EditorGUILayout.BeginHorizontal();
        if(GUILayout.Button("Reset to default position"))
        {
            reset.ResetInOut();
        }
        EditorGUILayout.EndHorizontal();
    }
}
