// Ablaufmanager f�r die Szene mit Handeingabe
// Regelt die Interaktionsabl�ufe mit den Objekten

using Leap;
using Leap.Attributes;
using System;
using System.Linq;
using UnityEngine;
using Valve.VR;
using static ApplicationSettings;
using static StudyControl;

public class Ablaufmanager : MonoBehaviour
{
    #region Global Variables
    public EventManager eventManager;
    [Tooltip("Application will start with this step. Default is tweezers step.")]
    public Steps currentStep = Steps.TableSetup;
    [NonSerialized] public SceneDetailLevel sceneDetailLevel;

    private LaserAngles.AngleNames _laserAngleState;
    public bool isInjectionMarkerLaserEnabled;

    [NonSerialized] public CameraFade fadeCamera;
    public StudyControl studyControl;
    public GameObject pinchDetector;
    public GlobalDetailLevelController globalDetailLevelController;
    public Studienmanager studienmanager;
    public StopPainting stopPainting;
    public GameObject pinzetteTisch;
    public GameObject nadelTisch;
    public GameObject tischCol;
    public HandModel_Controller handmodelController;
    [NonSerialized] public bool tupfer_malen;

    private LaserAngles.AngleNames _previousLaserAngleState;
    [NonSerialized] public float laserAngle;
    [Tooltip("The ImageProjector GameObject.")] public GameObject laserProjector;

    enum ActiveHandState
    {
        LeftHand,
        RightHand,
        Neutral
    }

    [System.NonSerialized]
    public bool tisch;
    [System.NonSerialized]
    public bool isNeedleInjected;

    private CollisionNadel collisionnadel;
    private CollisionPinzette collisionpinzette;
    [NonSerialized] public GameObject interactionObjectPlaceholder;
    [NonSerialized] public GameObject _interactionObject;

    public bool isRightHandPinched;
    public bool isLeftHandPinched;
    private ActiveHandState _activeHandState = ActiveHandState.LeftHand;
    private bool _areTweezersTouchedLeftHand = false;
    private bool _areTweezersTouchedRightHand = false;
    private bool _isNeedleTouchedLeftHand = false;
    private bool _isNeedleTouchedRightHand = false;
    private bool _istPinzetteGegriffen = false;
    public bool _isNeedleGrabbed = false;
    private bool _wereTweezersDropped = false; // whether the Tweezers were dropped after use
    private int _tweezersFreezeDelayMilliseconds = 1000; // Set delay in milliseconds to freeze tweezers after they were dropped/released.
    public bool isSceneConfiguredAndRunning; // whether the scene was set up successfully and should now enable all functionality

    #endregion

    #region Variables Hand Tracking
    #endregion

    #region Variables Vive Tracker
    #endregion

    #region Variables Controller
    #endregion

    void Start()
    {
        isInjectionMarkerLaserEnabled = true;
        isSceneConfiguredAndRunning = false;
        //sceneDetailLevel = studyControl.sceneDetailLevel;
        //_previousSceneDetailLevel = sceneDetailLevel;
        fadeCamera = GetComponent<CameraFade>();
        //fadeCamera.OnBlackPointReached += UpdateSceneDetailLevel;
        collisionnadel = nadelTisch.GetComponent<CollisionNadel>();
        collisionpinzette = pinzetteTisch.GetComponent<CollisionPinzette>();
        eventManager.OnScenarioSettingsApplied += ResetScenario;
    }

    void ResetScenario(StudyControl studyControl)
    {
        sceneDetailLevel = studyControl.sceneDetailLevel;
        UpdateSceneDetailLevel();
        UpdateLaserAngle();
        if (studyControl.scenario.Equals(Scenario.Real)) SetupForNoVR();
        currentStep = Steps.TableSetup;
        studienmanager.InitiateStudyManager();
        isSceneConfiguredAndRunning = true;
    }

    /// <summary>
    /// Configure a specific setup just for the scenario where no head mounted device is used.
    /// </summary>
    void SetupForNoVR()
    {
        GetComponent<NoVRScenarioSetup>().Setup();
    }

    public void ChangeStep(Steps step)
    {
        currentStep = step;
        // Debug.Log("Changed step to " + step);
    }

    void FixedUpdate()
    {
        if (!isSceneConfiguredAndRunning) return;

        switch (currentStep)
        {
            case Steps.Tweezers:
                tupfer_malen = true;
                break;
            case Steps.Needle:
                break;
            default: break;
        }

        EnableDisableInjectionMarkerLaser(isInjectionMarkerLaserEnabled);

        if (studienmanager.studyControl.scenario.Equals(ApplicationSettings.Scenario.VR_without_trackers))
        {
            UpdatePinchDetector();
            _isNeedleTouchedLeftHand = collisionnadel.isNeedleTouchedLeftHand;
            _isNeedleTouchedRightHand = collisionnadel.isNeedleTouchedRightHand;

            _areTweezersTouchedLeftHand = collisionpinzette.areTweezersTouchedLeftHand;
            _areTweezersTouchedRightHand = collisionpinzette.areTweezersTouchedRightHand;

            TischColEinblenden();

            if (isLeftHandPinched || isRightHandPinched)
            {
                switch (currentStep)
                {
                    case Steps.Tweezers:
                        PinzetteAblauf();
                        break;
                    case Steps.Needle:
                        NadelAblauf();
                        break;
                    default: break;
                }
            }

            // wenn Pinzette gegriffen und die Interaktion mit der Pinzette noch nicht beendet
            if (_istPinzetteGegriffen && currentStep == Steps.Tweezers)
            {
                TrackInteractionObject();
            }

            // wenn Nadel gegriffen und die Interaktion mit der Nadel noch nicht beendet
            if (_isNeedleGrabbed && currentStep == Steps.Needle)
            {
                TrackInteractionObject();
            }

            // wenn die Nadel eingestochen und PinchGeste beendet ist
            if (isNeedleInjected && _isNeedleGrabbed)
            {
                if ((_activeHandState == ActiveHandState.LeftHand && !isLeftHandPinched) || (_activeHandState == ActiveHandState.RightHand && !isRightHandPinched))
                {
                    _isNeedleGrabbed = false; // freeze needle in body (can be grabbed again unless step changes)
                    studienmanager.ReleaseNeedle();
                }
            }

            // wenn im Collider �ber dem Tisch, die Pinzette gegriffen ist und PinchGeste beendet
            if (tisch && _istPinzetteGegriffen)
            {
                if ((_activeHandState == ActiveHandState.LeftHand && !isLeftHandPinched) || (_activeHandState == ActiveHandState.RightHand && !isRightHandPinched))
                {
                    PinzetteFallenlassen();
                }
            }

            // wenn Interaktion mit Pinzette beendet und Nadel-Interaktion beginnt
            if (currentStep == Steps.Needle)
            {
                PinzetteFixieren();
            }

        }

    }

    /// <summary>
    /// Adapt Ultraleaps PinchDetector to each step.
    /// Since some objects (e.g. the needle) are smaller than others (e.g. tweezers),
    /// they require a smaller activation/deactivation distance than others (and vice versa).
    /// </summary>
    void UpdatePinchDetector()
    {
        if (pinchDetector == null) return; // not existing in device tracking scene

        if (currentStep == Steps.Tweezers || currentStep == Steps.TableSetup)
        {
            float activateDistance = 0.03f;
            float deactivateDistance = 0.05f;

            pinchDetector.GetComponents<PinchDetector>().ElementAt(0).activateDistance = activateDistance;
            pinchDetector.GetComponents<PinchDetector>().ElementAt(0).deactivateDistance = deactivateDistance;

            pinchDetector.GetComponents<PinchDetector>().ElementAt(1).activateDistance = activateDistance;
            pinchDetector.GetComponents<PinchDetector>().ElementAt(1).deactivateDistance = deactivateDistance;
        }
        else if (currentStep == Steps.Needle)
        {
            float activateDistance = 0.02f;
            float deactivateDistance = 0.025f; // old was 0.03f

            pinchDetector.GetComponents<PinchDetector>().ElementAt(0).activateDistance = activateDistance;
            pinchDetector.GetComponents<PinchDetector>().ElementAt(0).deactivateDistance = deactivateDistance;

            pinchDetector.GetComponents<PinchDetector>().ElementAt(1).activateDistance = activateDistance;
            pinchDetector.GetComponents<PinchDetector>().ElementAt(1).deactivateDistance = deactivateDistance;
        }
    }

    void UpdateSceneDetailLevel()
    {
        switch (sceneDetailLevel)
        {
            case SceneDetailLevel.Low:
                globalDetailLevelController.SetObjectsQualityState(true, false);
                break;
            case SceneDetailLevel.High:
                globalDetailLevelController.SetObjectsQualityState(false, true);
                break;
        }

        eventManager.RaiseOnSceneDetailLevelChange(sceneDetailLevel);
    }

    /// <summary>
    /// Freeze Tweezers after they were dropped/released.
    /// Use delay to prevent Tweezers freezing directly after release (in the air).
    /// </summary>
    void PinzetteFixieren()
    {
        if (_tweezersFreezeDelayMilliseconds >= 0)
        {
            _tweezersFreezeDelayMilliseconds -= (int)(Time.unscaledDeltaTime * 1000f); // Subtract time in milliseconds passed since last frame.
        }
        else
        {
            pinzetteTisch.transform.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePosition | RigidbodyConstraints.FreezeRotation;
            pinzetteTisch.transform.GetComponent<BoxCollider>().enabled = false;
        }
    }

    void PinzetteFallenlassen()
    {
        // Pinzettenmodell mit Kinematik versehen damit es runter f�llt
        tischCol.SetActive(false); //Tisch Collider ausblenden, damit Nadel im Anschluss greifbar ist und Pinzette nicht durch die Luft springt
        pinzetteTisch.transform.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
        pinzetteTisch.SetActive(true);
        nadelTisch.GetComponent<BoxCollider>().enabled = true;
        tisch = false;
        studienmanager.TweezersDropped();
        _wereTweezersDropped = true;
        currentStep = Steps.Needle;
    }

    // TischCollider einblenden, damit Pinzette �ber der Tisch losgelassen werden kann, aber nur so lang bis Pinzette losgelassen wurde
    void TischColEinblenden()
    {
        if (!stopPainting.tupfer_malen && !_wereTweezersDropped)
        {
            tischCol.SetActive(true);
        }
    }

    public void EnableDisableInjectionMarkerLaser(bool enable)
    {
        laserProjector.SetActive(enable);
    }

    private void UpdateLaserAngle()
    {
        _laserAngleState = studyControl.laserAngleInDegrees;
        float side = studyControl.preferredInterventionSide.Equals(PreferredInterventionSide.left) ? 1 : -1;

        switch (_laserAngleState)
        {
            case LaserAngles.AngleNames._25:
                laserAngle = side * LaserAngles._25degrees;
                break;
            case LaserAngles.AngleNames._20:
                laserAngle = side * LaserAngles._20degrees;
                break;
            case LaserAngles.AngleNames._15:
                laserAngle = side * LaserAngles._15degrees;
                break;
            case LaserAngles.AngleNames._10:
                laserAngle = side * LaserAngles._10degrees;
                break;
        }
    }

    public void NadelAblauf()
    {
        bool isNeedleTouched = false;

        if (_isNeedleTouchedLeftHand && isLeftHandPinched)
        {
            _activeHandState = ActiveHandState.LeftHand;
            isNeedleTouched = true;
        }
        else if (_isNeedleTouchedRightHand && isRightHandPinched)
        {
            _activeHandState = ActiveHandState.RightHand;
            isNeedleTouched = true;
        }

        if (isNeedleTouched && !_isNeedleGrabbed)
        {
            studienmanager.NeedleGrabbed();

            _interactionObject = nadelTisch;
            TrackInteractionObject();
            _isNeedleGrabbed = true;
        }
    }

    public void PinzetteAblauf()
    {
        bool areTweezersTouched = false;
        if (_areTweezersTouchedLeftHand && isLeftHandPinched)
        {
            _activeHandState = ActiveHandState.LeftHand;
            areTweezersTouched = true;
        }
        else if (_areTweezersTouchedRightHand && isRightHandPinched)
        {
            _activeHandState = ActiveHandState.RightHand;
            areTweezersTouched = true;
        }

        if (areTweezersTouched && !_istPinzetteGegriffen)
        {
            studienmanager.TweezersGrabbed();
            _interactionObject = pinzetteTisch;
            TrackInteractionObject();
            _istPinzetteGegriffen = true;
        }
    }

    /// <summary>
    /// Track the interaction object to the hand's wrist position (with specific offsets for each object).
    /// </summary>
    public void TrackInteractionObject()
    {
        switch (_activeHandState)
        {
            case ActiveHandState.LeftHand: interactionObjectPlaceholder = handmodelController.GetLeftHandPlaceholder(); break;
            case ActiveHandState.RightHand: interactionObjectPlaceholder = handmodelController.GetRightHandPlaceHolder(); break;
        }

        InteractionObjectsOffset interactionObjectOffset = interactionObjectPlaceholder.GetComponent<InteractionObjectsOffset>();

        if (!isNeedleInjected)
        {
            var offsetRot = interactionObjectOffset.currentOffset.rotation;
            if (!interactionObjectOffset.overrideNeedleRotationEnabled) // prevent rotation from resetting to offset if needle was removed after being injected
            {
                offsetRot = interactionObjectOffset.currentOffset.rotation; // needle was not injected yet, so use the default position in hand
            }

            _interactionObject.transform.position = interactionObjectOffset.currentOffset.position;
            _interactionObject.transform.rotation = offsetRot;
        }
    }

    #region Ultraleap Pinch Detection
    public void RightHandPinchCheck()
    {
        Hand hand = Hands.Provider.GetHand(Chirality.Right);
        if (hand == null) return;
        isRightHandPinched = hand.IsPinching();
    }

    public void LeftHandPinchCheck()
    {
        Hand hand = Hands.Provider.GetHand(Chirality.Left);
        if (hand == null) return;
        isLeftHandPinched = hand.IsPinching();
    }
    #endregion
}
