using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefineDynamicObjects : MonoBehaviour
{
    [HideInInspector][Tooltip("Select the topmost parent for all objects in the scene. This will be used to set it and all its children to static/dynamic if desired.")] public GameObject topmostParent;
    [HideInInspector][Tooltip("When updating static/dynamic states, also update the gameobjects' children.")] public bool alsoChangeChildren;
    [HideInInspector][Tooltip("Add all objects that should be dynamic. Their children will be changed automatically if flag is set.")] public GameObject[] dynamicObjects;


    public void SetObjectsState(GameObject[] objects, bool isStatic, bool _alsoChangeChildren)
    {
        foreach (GameObject obj in objects)
        {
            obj.isStatic = isStatic;

            if (_alsoChangeChildren)
            {
                // do the same for each child of the object (even if object is inactive)
                foreach (Transform childTransform in obj.transform.GetComponentsInChildren<Transform>(true))
                {
                    childTransform.gameObject.isStatic = isStatic;
                    if (!isStatic)
                    {
                        if (childTransform.GetComponent<MeshRenderer>())
                        {
                            childTransform.GetComponent<MeshRenderer>().lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
                        }
                    }
                }
            }
        }
    }

    public GlobalDetailLevelController GetDetailLevelController()
    {
        return transform.GetComponent<GlobalDetailLevelController>();
    }
}
