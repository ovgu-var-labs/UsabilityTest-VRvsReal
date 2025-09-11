using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Handgesten_Controller : MonoBehaviour
{
    
   
    public GameObject Hand_neutral;
    public GameObject Hand_Pinzette;
    public GameObject PinzetteO;
    public GameObject NadelO;
    public GameObject Hand_Nadel;
    public GameObject Hand_Pinch;
    public GameObject Controller_Pos;
    public bool greifen;
    public bool pinzette;
    public bool nadel;

    public GameObject Object;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //greifen = greifencontroller.greifen;

        if (greifen)
        {
           
            if (nadel)
            {
                Hand_Nadel.SetActive(true);
                Hand_neutral.SetActive(false);
                Hand_Nadel.transform.position = Controller_Pos.transform.position;
                Hand_Nadel.transform.rotation = Controller_Pos.transform.rotation;
                Object = NadelO;

            }
            else
            {
               
                if (pinzette)
                {
                    Hand_Pinzette.SetActive(true);
                    Hand_neutral.SetActive(false);
                    Hand_Pinzette.transform.position = Controller_Pos.transform.position;
                    Hand_Pinzette.transform.rotation = Controller_Pos.transform.rotation;
                    Object = PinzetteO;
                }
                else
                {
                    
                    Hand_Pinch.SetActive(true);
                    Hand_neutral.SetActive(false);
                    Hand_Pinch.transform.position = Controller_Pos.transform.position;
                    Hand_Pinch.transform.rotation = Controller_Pos.transform.rotation;
                }
            }
            

        }
        else
        {
            if (nadel)
            {
                Hand_Nadel.SetActive(false);
                Hand_neutral.SetActive(true);
                Hand_neutral.transform.position = Controller_Pos.transform.position;
                Hand_neutral.transform.rotation = Controller_Pos.transform.rotation;
                nadel = false;
                Object = NadelO;
            }
            else
            {
                if (pinzette)
                {
                    Hand_Pinzette.SetActive(false);
                    Hand_neutral.SetActive(true);
                    Hand_neutral.transform.position = Controller_Pos.transform.position;
                    Hand_neutral.transform.rotation = Controller_Pos.transform.rotation;
                    pinzette = false;
                    Object = PinzetteO;
                }
                else
                {
                    Hand_Pinch.SetActive(false);
                    Hand_neutral.SetActive(true);
                    Hand_neutral.transform.position = Controller_Pos.transform.position;
                    Hand_neutral.transform.rotation = Controller_Pos.transform.rotation;
                }
            }

            



         Hand_neutral.transform.position = Controller_Pos.transform.position;
         Hand_neutral.transform.rotation = Controller_Pos.transform.rotation;

        }
    }
}
