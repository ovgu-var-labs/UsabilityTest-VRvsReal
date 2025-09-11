using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ApplicationSettings;

public class Eintunken : MonoBehaviour
{
    public Tweezers_Manager tweezers_manager;
    public Material Jod;
    public GameObject TexturePainter;
    public Ablaufmanager ablaufmanager;

    public delegate void TweezersDip();
    public event TweezersDip OnTweezersDipped;

    void OnCollisionEnter(Collision target)
    {
        if (!ablaufmanager.currentStep.Equals(Steps.Tweezers)) return; // cannot dip if not allowed

        //Einfärben des Tupfers beim Eintunken in die Jodflüssigkeit
        if (target.collider.CompareTag(ApplicationObjectTags.Tweezers.ToString()))
        {
            tweezers_manager.GetTupferIntakt().GetComponent<MeshRenderer>().material = Jod;
            tweezers_manager.GetTupferVerformt().GetComponent<MeshRenderer>().material = Jod;
            TexturePainter.SetActive(true);
            OnTweezersDipped?.Invoke(); // tell listeners that the tweezers were dipped in the fluid
        }
    }

    void OnCollisionExit(Collision target)
    {
        //tunken = false;
    }
}
