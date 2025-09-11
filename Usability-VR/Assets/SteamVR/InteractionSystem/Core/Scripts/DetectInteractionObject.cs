using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DetectInteractionObject : MonoBehaviour
{
    public Handgesten_Controller handgesten_Controller;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnCollisionEnter(Collision target)
    {
        if (target.gameObject.name == "Nadelkopf")
        {
            handgesten_Controller.nadel = true;
        }

        if (target.gameObject.name == "Pinzette_Tupfer_VR")
        {
            handgesten_Controller.pinzette = true;
        }
    }

    void OnCollisionStay(Collision target)
    {
        if (target.gameObject.name == "Nadelkopf")
        {
            handgesten_Controller.nadel = true;
        }

        if (target.gameObject.name == "Pinzette_Tupfer_VR")
        {
            handgesten_Controller.pinzette = true;
        }
    }
   
}
