using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIndicators : MonoBehaviour
{
    [SerializeField] private SlideControlUI slideControl;

    void OnGUI()
    {
        int w = Screen.width, h = Screen.height;
        GUIStyle style = new GUIStyle();
        Rect rect = new Rect(10, 10, w, h * 2 / 100);
        style.alignment = TextAnchor.UpperLeft;
        style.fontSize = h * 2 / 50;
        if (slideControl.numberPhaseA >= 4 && slideControl.numberPhaseB >= 4)
        {
            style.normal.textColor = Color.green;
        }
        else
        {
            style.normal.textColor = Color.white;
        }
        string text = string.Format("A:{0} B:{1}", slideControl.numberPhaseA, slideControl.numberPhaseB);
        GUI.Label(rect, text, style);
    }
}
