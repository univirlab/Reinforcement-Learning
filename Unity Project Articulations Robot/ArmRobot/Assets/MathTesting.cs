using System;
using UnityEngine;
using static MathFunctions;

public class MathTesting : MonoBehaviour
{
    [SerializeField] private Transform endposition;
    [SerializeField] private Transform endpositionSmall;
    [SerializeField] private Transform Hand2;
    public RotateAxis rotateAxis1;
    public RotateAxis rotateAxis2;
    public RotateAxis rotateAxis3;
    public float rotateAngle1;
    public float rotateAngle2;
    public float rotateAngle3;

    public Vector3 newPosition;
    
    [Header("OFFSET")]
    public RotateAxis offsetAxis;
    public float offsetDist;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(endpositionSmall.position, .12f);
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(newPosition, .12f);
    }

    [ContextMenu("Get Position")]
    public void GetPosition()
    {
        var radians = rotateAngle1 * Mathf.PI / 180;
        
        // Hand 2 - axis OX
        radians = rotateAngle3 * Mathf.PI / 180;
        newPosition = RotateAndOffset(offsetAxis, offsetDist, rotateAxis3, radians, endposition.position);
        
        // Hand 1 - axis OX
        radians = rotateAngle2 * Mathf.PI / 180;
        newPosition = RotateAroundAxis(newPosition, rotateAxis2, radians);
        
        // Hand 0 - axis OY
        radians = rotateAngle1 * Mathf.PI / 180;
        newPosition = RotateAroundAxis(newPosition, rotateAxis1, radians);
    }
    
    [ContextMenu("Get Position for small hand")]
    public void GetPositionSmall()
    {
        var radians = rotateAngle1 * Mathf.PI / 180;
        
        // Hand 2 - axis OX
        radians = rotateAngle3 * Mathf.PI / 180;
        newPosition = RotateAndOffset(offsetAxis, offsetDist, rotateAxis3, radians, endpositionSmall.position);
        
        // Hand 1 - axis OX
        radians = rotateAngle2 * Mathf.PI / 180;
        newPosition = RotateAroundAxis(newPosition, rotateAxis2, radians);
        
        // Hand 0 - axis OY
        radians = rotateAngle1 * Mathf.PI / 180;
        newPosition = RotateAroundAxis(newPosition, rotateAxis1, radians);

        var fullThisLength = 0.279f;
        var fullUnityLength = endposition.position.y;
        
        print($"new pos before: {newPosition}");
        
        //newPosition = NormalizeCoordinates(fullThisLength, fullUnityLength, newPosition);
        
        print($"new pos after: {newPosition}");

    }

    private void Update()
    {
        var angle = Hand2.transform.localRotation.eulerAngles;
        if (angle.y != 0)
            angle.x = 180 - angle.x;
        print(angle);
    }
}