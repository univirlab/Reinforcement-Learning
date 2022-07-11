using System;
using UnityEngine;
using static MathFunctions;

public class MathTesting : MonoBehaviour
{
    [SerializeField] private Transform endposition;
    [SerializeField] private Transform Hand2;
    public RotateAxis rotateAxis1;
    public RotateAxis rotateAxis2;
    public float rotateAngle1;
    public float rotateAngle2;

    public Vector3 newPosition;
    
    [Header("OFFSET")]
    public RotateAxis offsetAxis;
    public float offsetDist;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(endposition.position, .02f);
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(newPosition, .02f);
    }

    [ContextMenu("Position Print")]
    public void GetPosition()
    {
        var radians = rotateAngle1 * Mathf.PI / 180;
        
        radians = rotateAngle2 * Mathf.PI / 180;
        newPosition = RotateAndOffset(offsetAxis, offsetDist, rotateAxis2, radians, endposition.position);
        
        radians = rotateAngle1 * Mathf.PI / 180;
        newPosition = RotateAroundAxis(newPosition, rotateAxis1, radians);
    }

    private void Update()
    {
        var angle = Hand2.transform.localRotation.eulerAngles;
        if (angle.y != 0)
            angle.x = 180 - angle.x;
        print(angle);
    }
}