using UnityEngine;

public class RobotPart : MonoBehaviour
{
    [SerializeField] protected Vector3[] rotationAxes;
    [SerializeField] protected Vector3 startLocalPosition;
    public float[] Angles;
    
    public float Speed = 2;

    public Vector3 Rotation
    {
        get
        {
            var q = transform.rotation.eulerAngles.normalized;
            //var q = transform.localEulerAngles.normalized;
            return q;
        }
    }
    
    public float RotationFloat
    {
        get
        {
            var q = rotationAxes[0] == Vector3.up
                ? Normalization(transform.transform.localRotation.eulerAngles.y, 0, 360)
                : Normalization(transform.transform.localRotation.eulerAngles.x, 0, 360);
            return q;
        }
    }

    private float Normalization(float value, float min, float max) => 
        (value - min) / (max - min);

    private void Awake()
    {
        Angles = new float[rotationAxes.Length];

        for (var index = 0; index < rotationAxes.Length; index++)
        {
            Angles[index] = index == 0
                ? WrapAngle(transform.rotation.eulerAngles).y
                : WrapAngle(transform.rotation.eulerAngles).x;
        }
    }

    public virtual void SetStartPositionRotation()
    {
        transform.localRotation = Quaternion.Euler(0f, 0, 0f);
        transform.localPosition = startLocalPosition;
    }

    public virtual void RotateAround(float[] rotateValues)
    {
        for (var index = 0; index < rotationAxes.Length; index++)
        {
            if (rotateValues.Length < index + 1)
                break;
            
            transform.RotateAround(transform.position, rotationAxes[index], rotateValues[index] * Speed);
            
            Angles[index] = index == 0
                ? WrapAngle(transform.rotation.eulerAngles).y
                : WrapAngle(transform.rotation.eulerAngles).x;
        }
    }
    
    public virtual void RotateAround(float rotateValue)
    {
        transform.RotateAround(transform.position, rotationAxes[0], rotateValue * Speed);
        
        
        Angles[0] = rotationAxes[0] == Vector3.up
            ? WrapAngle(transform.rotation.eulerAngles).y
            : WrapAngle(transform.rotation.eulerAngles).x;
    }
    
    public void RotateSimple(float rotateValue)
    {
        if (rotationAxes[0] == Vector3.up)
        {
            var newValue = transform.localEulerAngles.y + rotateValue;
            transform.localRotation = Quaternion.Euler(0f, newValue, 0f);
            Angles[0] = WrapAngle(transform.rotation.eulerAngles).y;
        }
        else
        {
            var newValue = transform.localEulerAngles.x + rotateValue;
            transform.localRotation = Quaternion.Euler(newValue, 0f, 0f);
            Angles[0] = WrapAngle(transform.rotation.eulerAngles).x;
        }
    }
    
    protected Vector3 WrapAngle(Vector3 angle)
    {
        var x = angle.x;
        var y = angle.y;
        var z = angle.z;

        if (Vector3.Dot(transform.up, Vector3.up) >= 0f)
        {
            if (angle.x >= 0f && angle.x <= 90f)
                x = angle.x;
            if (angle.x >= 270f && angle.x <= 360f)
                x = angle.x - 360f;
        }

        if (Vector3.Dot(transform.up, Vector3.up) < 0f)
        {
            if (angle.x >= 0f && angle.x <= 90f)
                x = 180 - angle.x;
            if (angle.x >= 270f && angle.x <= 360f)
                x = 180 - angle.x;
        }

        if (angle.y > 180)
            y = angle.y - 360f;

        if (angle.z > 180)
            z = angle.z - 360f;

        return new Vector3(x, y, z);
    }
    
    protected float WrapAngle(float angle)
    {
        angle%=360;
        if(angle >180)
            return angle - 360;
 
        return angle;
    }
}