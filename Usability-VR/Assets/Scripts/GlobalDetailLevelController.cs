using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalDetailLevelController : MonoBehaviour
{
    [HideInInspector] [Tooltip("If enabled, all items in the exclude lists will not be enabled.")] public bool applyExcludeLists = false;
    [HideInInspector] [Tooltip("Add all objects that are only low quality / low-poly. Their children will be enabled automatically.")] public GameObject[] lowQualityObjects;
    [HideInInspector] [Tooltip("These objects will never be enabled.")] public GameObject[] lowQualityObjectsExceptions;
    [HideInInspector] [Tooltip("Add all objects that are only high-quality / high-poly. Their children will be enabled automatically.")] public GameObject[] highQualityObjects;
    [HideInInspector] [Tooltip("These objects will never be enabled.")] public GameObject[] highQualityObjectsExceptions;

    /// <summary>
    /// Enable all high poly or all low poly objects.
    /// </summary>
    /// <param name="enableHighQuality">If true, enable all high poly objects and disable low poly objects. Does the opposite if false.</param>
    public void SetObjectsQualityState(bool enableLowQuality, bool enableHighQuality)
    {
        EnableDisableObjects(lowQualityObjects, enableLowQuality);
        EnableDisableObjects(highQualityObjects, enableHighQuality);

        // disable all objects in the exclude lists
        if (applyExcludeLists)
        {
            EnableDisableObjects(lowQualityObjectsExceptions, false);
            EnableDisableObjects(highQualityObjectsExceptions, false);
        }
    }

    /// <summary>
    /// Enable or disable a list of objects along with their children.
    /// </summary>
    /// <param name="objects">The objects to enable/disable.</param>
    /// <param name="enableGameObjects">Whether to enable or disable the objects in the list.</param>
    void EnableDisableObjects(GameObject[] objects, bool enableGameObjects)
    {
        foreach (GameObject obj in objects)
        {
            obj.SetActive(enableGameObjects);

            // do the same for each child of the object (even if object is inactive)
            foreach (Transform childTransform in obj.transform.GetComponentsInChildren<Transform>(true))
            {
                childTransform.gameObject.SetActive(enableGameObjects);
            }
        }
    }
}
