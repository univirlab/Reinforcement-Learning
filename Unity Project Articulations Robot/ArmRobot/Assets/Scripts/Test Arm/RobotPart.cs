﻿using System;
using UnityEngine;

public class RobotPart : MonoBehaviour
{
    [SerializeField] private Vector3[] rotationAxes;
    [SerializeField] private Vector3 startLocalPosition;

    public Vector3 Rotation => transform.rotation.eulerAngles.normalized;

    public void SetStartPositionRotation()
    {
        transform.localPosition = startLocalPosition;
        transform.rotation = Quaternion.Euler(new Vector3(0, 0, 0));
    }
    
    public void RotateAround(float[] rotateValues)
    {
        for (var index = 0; index < rotationAxes.Length; index++)
        {
            if (rotateValues.Length < index + 1)
                break;
            
            transform.RotateAround(transform.position, rotationAxes[index], rotateValues[index]);
        }
    }
}
