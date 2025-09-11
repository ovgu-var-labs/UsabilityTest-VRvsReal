using UnityEngine;
using static ApplicationSettings;
using static UnityEngine.GraphicsBuffer;

public class MRI_Rotary_Knob_Button : MonoBehaviour
{
    public MRI_Rotary_Knob_Interactivity knob_interactivity;
    [Tooltip("Enable if this button can be rotated via a pinch-and-turn gesture.")] public bool isRotatable;
    [Tooltip("Enable if the button has a press-and-hold functionality.")] public bool hasPressAndHoldFunctionality;

    public GameObject LHandIndex;
    public GameObject LHandThumb;
    public GameObject RHandIndex;
    public GameObject RHandThumb;


    private float emissionIntensity = 5000;
    private float activityStartTime; // to measure how long the button is pressed (for press-and-hold functionality)
    private bool isRHandPinchActive;
    private bool isLHandPinchActive;
    private bool isStartPinchVectorSaved = false;
    private Vector3 startPinchVector;

    void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag(ApplicationObjectTags.LHand_Index.ToString()) || collision.collider.CompareTag(ApplicationObjectTags.RHand_Index.ToString()))
        {
            activityStartTime = Time.time;
            if (!hasPressAndHoldFunctionality)
            {
                knob_interactivity.ButtonActivity(transform.gameObject, true, activityStartTime);
                Shine(true);
            }
        }
    }

    [System.Obsolete]
    void OnCollisionStay(Collision collision)
    {
        if (!hasPressAndHoldFunctionality) return;

        bool isTouchedLeftHand = collision.collider.CompareTag(ApplicationObjectTags.LHand_Index.ToString());
        bool isTouchedRightHand = collision.collider.CompareTag(ApplicationObjectTags.RHand_Index.ToString());

        if (isRotatable && ((isTouchedLeftHand && isLHandPinchActive) || (isTouchedRightHand && isRHandPinchActive)))
        {
            Vector3 rightHandVector = RHandThumb.transform.position - RHandIndex.transform.position;
            Vector3 leftHandVector = LHandThumb.transform.position - LHandIndex.transform.position;

            if (isStartPinchVectorSaved == false)
            {
                if (isTouchedLeftHand)
                {
                    startPinchVector = leftHandVector;
                }
                else
                {
                    startPinchVector = rightHandVector;
                }

                isStartPinchVectorSaved = true;
            }

            if (isTouchedLeftHand)
            {
                ControlRotation(Vector3.Angle(startPinchVector, leftHandVector));
            }
            else
            {
                ControlRotation(Vector3.SignedAngle(startPinchVector, rightHandVector, transform.localRotation * Vector3.forward));
            }

            //Shine(true);
        }
        else if (isTouchedLeftHand || isTouchedRightHand)
        {
            knob_interactivity.ButtonActivity(transform.gameObject, true, activityStartTime);
            Shine(true);
        }
    }

    void OnCollisionExit(Collision collision)
    {
        if (collision.collider.CompareTag(ApplicationObjectTags.LHand_Index.ToString()) || collision.collider.CompareTag(ApplicationObjectTags.RHand_Index.ToString()))
        {
            knob_interactivity.ButtonActivity(transform.gameObject, false, activityStartTime);
            Shine(false);
            isStartPinchVectorSaved = false;
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

    void ControlRotation(float angle)
    {
        // clamp angle
        angle = Mathf.Clamp(angle, -90, 90); // only allow quarter rotation clockwise or anti-clockwise

        transform.localRotation = Quaternion.Euler(0, 0, angle);

        knob_interactivity.ButtonActivity(transform.gameObject, true, activityStartTime, angle);
    }

    #region used by Ultralreap Pinch Detector
    /// <summary>
    /// Activate pinch gesture for the right hand.
    /// </summary>
    public void RHand_PinchActive()
    {
        isRHandPinchActive = true;
    }

    /// <summary>
    /// Deactivate pinch gesture for the right hand.
    /// </summary>
    public void RHand_PinchInactive()
    {
        isRHandPinchActive = false;
    }

    /// <summary>
    /// Activate pinch gesture for the left hand.
    /// </summary>
    public void LHand_PinchActive()
    {
        isLHandPinchActive = true;
    }

    /// <summary>
    /// Deactivate pinch gesture for the left hand.
    /// </summary>
    public void LHand_PinchInactive()
    {
        isLHandPinchActive = false;
    }
    #endregion
}
