using UnityEngine;

public class ResetTransformScript : MonoBehaviour
{
    public void ResetInOut()
    {
        transform.localPosition = Vector3.zero;
        transform.GetChild(0).transform.localPosition = Vector3.zero;
    }
}
