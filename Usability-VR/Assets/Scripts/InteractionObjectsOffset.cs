using System;
using UnityEngine;
using static ApplicationSettings;

public class InteractionObjectsOffset : MonoBehaviour
{
    public Ablaufmanager ablaufmanagerHand;
    private Transform _standardTransform;
    [NonSerialized] public Transform currentOffset;
    public Transform tweezersOffset;
    public Transform needleOffset;

    [NonSerialized] public bool overrideNeedleRotationEnabled = false; // if enabled, prevents the needle's rotation being reset to its offset rotation when removing needle after injection. Won't be disabled afterwards unless interaction is restarted completely.

    private void Start()
    {
        _standardTransform = transform;
    }

    // Update is called once per frame
    void Update()
    {
        switch (ablaufmanagerHand.currentStep)
        {
            case Steps.Tweezers:
                currentOffset = tweezersOffset;
                break;
            case Steps.Needle:
                currentOffset = needleOffset;
                break;
            default:
                currentOffset = _standardTransform;
                break;
        }
    }
}
