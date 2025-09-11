using UnityEngine;
using static ApplicationSettings;

public class MRI_Small_Controller_Button : MonoBehaviour
{
    public MRI_Small_Controller_Interactivity MRISmallController;
    private float emissionIntensity = 5000;

    void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag(ApplicationObjectTags.LHand_Index.ToString()) || collision.collider.CompareTag(ApplicationObjectTags.RHand_Index.ToString()))
        {
            MRISmallController.ButtonActivity(transform.gameObject, true);
            Shine(true);
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.collider.CompareTag(ApplicationObjectTags.LHand_Index.ToString()) || collision.collider.CompareTag(ApplicationObjectTags.RHand_Index.ToString()))
        {
            Shine(true);
        }
    }

    void OnCollisionExit(Collision collision)
    {
        if (collision.collider.CompareTag(ApplicationObjectTags.LHand_Index.ToString()) || collision.collider.CompareTag(ApplicationObjectTags.RHand_Index.ToString()))
        {
            MRISmallController.ButtonActivity(transform.gameObject, false);
            Shine(false);
        }
    }

    void Shine(bool isShineEnabled)
    {
        Material buttonMaterial = GetComponent<Renderer>().material;
        if (isShineEnabled)
        {
            buttonMaterial.SetFloat("_EmissionIntensity", emissionIntensity);
        }
        else
        {
            buttonMaterial.SetFloat("_EmissionIntensity", 0);
        }
    }
}
