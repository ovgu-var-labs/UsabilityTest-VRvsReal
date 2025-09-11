using UnityEngine;

/// <summary>
/// Script detects where the laser hits the body.
/// This is needed to calculate the accuracy of the needle injection.
/// </summary>
public class LaserCollisionDetector : MonoBehaviour
{
    [Tooltip("The layer mask of the injection target (e.g. body / belly).")] public LayerMask targetLayerMask;
    public Vector3 LaserTargetPositionOnBody { get; private set; }
    public Vector3 LaserVector { get; private set; }

    public Studienmanager studienmanager;
    public Ablaufmanager ablaufmanager;
    private bool wereLaserValuesLogged = false;

    void FixedUpdate()
    {
        Debug.DrawRay(transform.position, transform.forward, Color.yellow, 10.0f);

        if (Physics.Raycast(new Ray(transform.position, transform.forward), out RaycastHit hit, Mathf.Infinity, targetLayerMask))
        {
            if (hit.collider != null)
            {
                LaserTargetPositionOnBody = hit.point;
                LaserVector = Vector3.Normalize(hit.point - transform.position);

                if (!wereLaserValuesLogged && ablaufmanager.isSceneConfiguredAndRunning)
                {
                    studienmanager.LogLaserValues(); // log laser values at start of scenario
                    wereLaserValuesLogged = true;
                }
            }
        }
    }
}
