using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TableDetection : MonoBehaviour
{
    public string tableTag = "Table";
    public bool hasTouchedTable = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag(tableTag))
            hasTouchedTable = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag(tableTag))
            hasTouchedTable = false;
    }
}
