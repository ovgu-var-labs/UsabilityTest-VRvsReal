using UnityEngine;
using static ApplicationSettings.SceneDetailLevel;

public class HandModel_Controller : MonoBehaviour
{
    public Ablaufmanager ablaufmanager;
    public GameObject handModelHighRes;
    public GameObject handModelLowRes;
    public GameObject leftHandPlaceHolder_LowPoly;
    public GameObject leftHandPlaceHolder_HighPoly;
    public GameObject rightHandPlaceHolder_LowPoly;
    public GameObject rightHandPlaceHolder_HighPoly;

    void FixedUpdate()
    {
        switch (ablaufmanager.sceneDetailLevel)
        {
            case Low:
                handModelHighRes.SetActive(false);
                handModelLowRes.SetActive(true);
                break;
            case High:
                handModelHighRes.SetActive(true);
                handModelLowRes.SetActive(false);
                break;
            default:
                handModelHighRes.SetActive(false);
                handModelLowRes.SetActive(true);
                break;
        }
    }

    public GameObject GetLeftHandPlaceholder()
    {
        if (ablaufmanager.sceneDetailLevel.Equals(High))
        {
            return leftHandPlaceHolder_HighPoly;
        }
        else
        {
            return leftHandPlaceHolder_LowPoly;
        }
    }

    public GameObject GetRightHandPlaceHolder()
    {
        if (ablaufmanager.sceneDetailLevel.Equals(High))
        {
            return rightHandPlaceHolder_HighPoly;
        }
        else
        {
            return rightHandPlaceHolder_LowPoly;
        }
    }
}
