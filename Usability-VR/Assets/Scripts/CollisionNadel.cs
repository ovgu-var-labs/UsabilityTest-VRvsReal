using System;
using UnityEngine;
using static ApplicationSettings;

/// <summary>
/// Script to check whether hands touch needle or not.
/// Attached to needle object.
/// </summary>
public class CollisionNadel : MonoBehaviour
{
    [NonSerialized] public bool isNeedleTouchedLeftHand;
    [NonSerialized] public bool isNeedleTouchedRightHand;


    void OnCollisionEnter(Collision target)
    {
        if (target.collider.CompareTag(ApplicationObjectTags.LeftHand.ToString()))
        {
            isNeedleTouchedLeftHand = true;
        } else if (target.collider.CompareTag(ApplicationObjectTags.RightHand.ToString()))
        {
            isNeedleTouchedRightHand = true;
        }
    }

    void OnCollisionExit(Collision target)
    {
        if (target.collider.CompareTag(ApplicationObjectTags.LeftHand.ToString()))
        {
            isNeedleTouchedLeftHand = false;
        }
        else if (target.collider.CompareTag(ApplicationObjectTags.RightHand.ToString()))
        {
            isNeedleTouchedRightHand = false;
        }
    }

}
