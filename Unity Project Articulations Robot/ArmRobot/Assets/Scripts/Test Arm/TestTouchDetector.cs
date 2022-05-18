using UnityEngine;

public class TestTouchDetector : MonoBehaviour
{
    public string touchTargetTag = "Pincher";
    public bool hasTouchedTarget = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.gameObject.CompareTag(touchTargetTag))
        {
            Debug.Log("Touch Detected!");
            hasTouchedTarget = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.transform.gameObject.CompareTag(touchTargetTag))
            hasTouchedTarget = false;
    }
}
