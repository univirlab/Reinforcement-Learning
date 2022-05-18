using UnityEngine;

public class TouchDetector : MonoBehaviour
{
    public string touchTargetTag;
    public bool hasTouchedTarget = false;


    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.gameObject.tag == touchTargetTag)
        {
            Debug.Log("Touch Detected!");
            hasTouchedTarget = true;
        }
    }

    private void OnCollisionExit(Collision other)
    {
        if (other.transform.gameObject.tag == touchTargetTag)
        {
            hasTouchedTarget = false;
        }
    }
}
