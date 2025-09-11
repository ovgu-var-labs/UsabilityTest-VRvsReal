using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DocumentationNote : MonoBehaviour
{
    [Header("Documentation note / hint", order = 1)]
    [Space(10, order = 2)]
    [TextArea] public string note = "Enter note here";
}
