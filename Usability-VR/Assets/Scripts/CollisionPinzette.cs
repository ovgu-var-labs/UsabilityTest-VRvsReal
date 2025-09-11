using System;
using UnityEngine;
using static ApplicationSettings;

/// <summary>
/// Script to check whether hands touch Tweezers or not.
/// Attached to Tweezers object.
/// </summary>
public class CollisionPinzette : MonoBehaviour
{
    [NonSerialized] public bool areTweezersTouchedLeftHand;
    [NonSerialized] public bool areTweezersTouchedRightHand;

    void OnCollisionEnter(Collision target)
    {
        if (target.collider.CompareTag(ApplicationObjectTags.LeftHand.ToString()))
        {
            areTweezersTouchedLeftHand = true;
        }
        else if (target.collider.CompareTag(ApplicationObjectTags.RightHand.ToString()))
        {
            areTweezersTouchedRightHand = true;
        }
    }

    void OnCollisionExit(Collision target)
    {
        if (target.collider.CompareTag(ApplicationObjectTags.LeftHand.ToString()))
        {
            areTweezersTouchedLeftHand = false;
        }
        else if (target.collider.CompareTag(ApplicationObjectTags.RightHand.ToString()))
        {
            areTweezersTouchedRightHand = false;
        }
    }

}
