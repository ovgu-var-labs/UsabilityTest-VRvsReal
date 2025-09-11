using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProximityDetect : MonoBehaviour
{
    public Transform[] markings;  //Color Bars in the table
    public Transform sensorPoint;   //Target location
    [NonSerialized] public List<float> distances = new List<float>();
    private float distance;
    private int distIndex;


    //Called from Slide Control script. Only returns the distance of the current color
    public float CheckProximity(int currentIndex, int[] currentorder)
    {
        distances.Clear();
        foreach (Transform mark in markings)
        {
            distance = Vector3.Distance(mark.position, sensorPoint.position);
            distances.Add(distance);
        }
        distIndex = currentorder[currentIndex];   // This selects the color currently being evaluated
        return distances[distIndex - 1];
    }
}
