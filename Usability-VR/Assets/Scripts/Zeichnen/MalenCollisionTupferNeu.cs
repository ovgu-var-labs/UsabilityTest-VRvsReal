//Script für die De-/Aktivierung des Malens
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ApplicationSettings;

public class MalenCollisionTupferNeu : MonoBehaviour
{
    public Studienmanager studienmanager;
    public bool tupferMalen;
    public StopPainting stopPainting;

    private int whichDisinfectionRound; // 0,1,2. 
    [NonSerialized] public Vector3 collisionPoint; // the position where the brush touches the painting trigger


    void Start()
    {
        tupferMalen = false;
        whichDisinfectionRound = 2; // In the current version there is only the last disinfection round. Therefore we start at 2 right away.
    }

    void OnCollisionEnter(Collision target)
    {
        // Debug.Log("Tweezers collided with " + target.gameObject.name);

        if (target.collider.CompareTag(ApplicationObjectTags.PaintingTrigger.ToString()))
        {
            if (whichDisinfectionRound == stopPainting.desinfektionszaehler) 
            {
                studienmanager.TweezersPaintDisinfection(firstTime: true); // register first stroke of disinfectant
                //Debug.Log("Disinfection started");
            }

            collisionPoint = target.transform.position;
            tupferMalen = true;
        }
    }

    void OnCollisionExit(Collision target)
    {
        if (target.collider.CompareTag(ApplicationObjectTags.PaintingTrigger.ToString()))
        {
            tupferMalen = false;
        }
    }
}
