using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ApplicationSettings;

public class StopPainting : MonoBehaviour
{
    public Studienmanager studienmanager;
    public Eintunken eintunkenManager;
    public Material Watte;
    public Tweezers_Manager tweezersManager;
    public GameObject fluidLevelLow;
    public GameObject fluidLevelMedium;
    public GameObject fluidLevelHigh;
    public GameObject TexturePainter;
    public Ablaufmanager ablaufmanager;
    public EventManager eventManager;

    [NonSerialized] public bool tupfer_malen;
    [NonSerialized] public int desinfektionszaehler; // Durchläufe der Desinfektion
    [NonSerialized] public bool wasDisinfectionCompleted;
    bool _wasDippedOnce;
    bool _wasNotificationSoundPlayed;

    private const int MaxBrushCountPerDisinfectionRound = 100; // how many brushes to paint per disinfection before the swab will be empty (white) again
    private int currentMaxBrushCount = 0; // the current maximum of brushes before the swab will be empty (white) again

    void Start()
    {
        tupfer_malen = true;
        _wasNotificationSoundPlayed = false;
        wasDisinfectionCompleted = false;
        eintunkenManager.OnTweezersDipped += JodfluessigkeitVerringern;
        currentMaxBrushCount += MaxBrushCountPerDisinfectionRound;
    }

    void Update()
    {
        if (ablaufmanager.tupfer_malen == true)
        {
            CheckStopDisinfection();
        }
    }

    /// <summary>
    /// Check whether to change to swab colour back to white.
    /// </summary>
    void CheckStopDisinfection()
    {
        if (desinfektionszaehler == 3) // all disinfections complete
        {
            tweezersManager.GetTupferIntakt().GetComponent<MeshRenderer>().material = Watte;
            tweezersManager.GetTupferVerformt().GetComponent<MeshRenderer>().material = Watte;

            ablaufmanager.tupfer_malen = false;
            tupfer_malen = false;

            // only in Real Scenario: play notification sound
            if (!_wasNotificationSoundPlayed /*&& ablaufmanager.studyControl.scenario.Equals(Scenario.Real)*/)
            {
                eventManager.RaiseOnSoundEffectPlayed(eventManager.soundEffectsManager.defaultNotification);
                _wasNotificationSoundPlayed = true;
            }
        }
        else if (this.transform.childCount > currentMaxBrushCount) // current disinfection complete (enough brushes painted)
        {
            tweezersManager.GetTupferIntakt().GetComponent<MeshRenderer>().material = Watte;
            tweezersManager.GetTupferVerformt().GetComponent<MeshRenderer>().material = Watte;
            TexturePainter.SetActive(false);
            currentMaxBrushCount += MaxBrushCountPerDisinfectionRound; // define next brush limit
            desinfektionszaehler++;
            studienmanager.TweezersPaintDisinfection(firstTime: false); // register last stroke of disinfectant
            wasDisinfectionCompleted = true;
            //Debug.Log("Disinfection ended");
        }
    }

    // Jodfluessigkeit nach jedem Eintunken reduzieren
    void JodfluessigkeitVerringern()
    {
        //bool zaehler = eintunkenManager.tunken;

        if (/*zaehler && */desinfektionszaehler == 0)
        {
            fluidLevelHigh.SetActive(false);
            fluidLevelMedium.SetActive(false);
            fluidLevelLow.SetActive(false);

            desinfektionszaehler = 2;

            if (!_wasDippedOnce)
            {
                if (ablaufmanager.studyControl.scenario.Equals(Scenario.Real))
                {
                    // play sound to indicate that tweezers were dipped successfully,
                    // but only when using REAL scenario since there is no visual cue for that
                    eventManager.RaiseOnSoundEffectPlayed(eventManager.soundEffectsManager.defaultNotification);
                }
                studienmanager.TweezersDipped();
                _wasDippedOnce = true;
            }

        }
        //if (/*zaehler && */desinfektionszaehler == 1)
        //{
        //    fluidLevelMedium.SetActive(false);
        //}
        //if (/*zaehler &&*/ desinfektionszaehler == 2)
        //{
        //    fluidLevelLow.SetActive(false);
        //}
    }
}
