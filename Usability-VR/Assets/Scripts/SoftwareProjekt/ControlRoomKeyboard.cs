using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ApplicationSettings;

public class ControlRoomKeyboard : MonoBehaviour
{
    public delegate void KeyboardPress();
    public event KeyboardPress OnKeyboardPressed;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag(ApplicationObjectTags.LHand_Index.ToString()) || collision.collider.CompareTag(ApplicationObjectTags.RHand_Index.ToString()))
        {
            OnKeyboardPressed?.Invoke();
        }
    }
}
