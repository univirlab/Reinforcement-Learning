using UnityEngine;

public class RobotPart : MonoBehaviour
{
    [SerializeField] protected Vector3[] rotationAxes;
    [SerializeField] protected Vector3 startLocalPosition;
    
    public float Speed = 2;

    public Vector3 Rotation
    {
        get
        {
            var q = transform.rotation.eulerAngles.normalized;
            return q;
        }
    }

    public void SetStartPositionRotation()
    {
        transform.localPosition = startLocalPosition;
        transform.rotation = Quaternion.Euler(new Vector3(0, 0, 0));
    }

    public virtual void RotateAround(float[] rotateValues)
    {
        for (var index = 0; index < rotationAxes.Length; index++)
        {
            if (rotateValues.Length < index + 1)
                break;

            transform.RotateAround(transform.position, rotationAxes[index], rotateValues[index] * Speed);
        }
    }
    
    public virtual void RotateAround(float rotateValue)
    {
        transform.RotateAround(transform.position, rotationAxes[0], rotateValue * Speed);
    }
}