using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This script should help quickly identify all gameobjects that are children to this gameobject via ParentConstraint.
/// Use the list to quickly jump to the referenced objects.
/// </summary>
public class DeParenting_Helper : MonoBehaviour
{
    [Tooltip("Add objects to this list if they are children to this object via ParentConstraint. Quickly jump to these objects by clicking them in the list")]
    public GameObject[] ObjectsAffectedByThisGameobject;
}
