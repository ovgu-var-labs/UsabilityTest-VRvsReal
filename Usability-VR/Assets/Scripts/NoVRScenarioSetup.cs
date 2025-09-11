using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoVRScenarioSetup : MonoBehaviour
{
    public GameObject belly_HQ;
    public GameObject belly_LQ;
    public GameObject belly_real_scenario;

    public void Setup()
    {
        belly_HQ.SetActive(false);
        belly_LQ.SetActive(false);
        belly_real_scenario.SetActive(true);
    }
}
