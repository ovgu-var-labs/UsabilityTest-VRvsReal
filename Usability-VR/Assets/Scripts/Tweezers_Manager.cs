using UnityEngine;
using static ApplicationSettings;


public class Tweezers_Manager : MonoBehaviour
{
    #region Set in Editor
    public Ablaufmanager ablaufmanager;
    public EventManager eventManager;
    public GameObject Pinzette_HighPoly;
    public GameObject TupferIntakt_highPoly;
    public GameObject TupferVerformt_highPoly;
    public GameObject Pinzette_LowPoly;
    public GameObject TupferIntakt_lowPoly;
    public GameObject TupferVerformt_lowPoly;
    #endregion

    private void Start()
    {
        eventManager.OnSceneDetailLevelChange += SetDetailLevel;
        SetDetailLevel(ablaufmanager.sceneDetailLevel);
    }

    void SetDetailLevel(SceneDetailLevel detailLevel)
    {
        bool enableHighPolyMode; // set false to enable low-poly mode

        switch (detailLevel)
        {
            case SceneDetailLevel.High:
                enableHighPolyMode = true;
                break;
            case SceneDetailLevel.Low:
                enableHighPolyMode = false;
                break;
            default:
                enableHighPolyMode = true;
                break;
        }
        
        Pinzette_HighPoly.SetActive(enableHighPolyMode);
        Pinzette_LowPoly.SetActive(!enableHighPolyMode);
        TupferIntakt_highPoly.SetActive(enableHighPolyMode);
        TupferIntakt_lowPoly.SetActive(!enableHighPolyMode);
        TupferVerformt_highPoly.SetActive(false);
        TupferVerformt_lowPoly.SetActive(false);
    }

    public GameObject GetTupferIntakt()
    {
        if (ablaufmanager.sceneDetailLevel.Equals(SceneDetailLevel.High)) return TupferIntakt_highPoly;
        else return TupferIntakt_lowPoly;
    }

    public GameObject GetTupferVerformt()
    {
        if (ablaufmanager.sceneDetailLevel.Equals(SceneDetailLevel.High)) return TupferVerformt_highPoly;
        else return TupferVerformt_lowPoly;
    }
}
