﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LimitedRobotPart : RobotPart
{
    [SerializeField] private float minAngle = -120;
    [SerializeField] private float maxAngle = 120;

    public bool IsOutOfRange;
    public float[] Angles;
    public bool IsRotated;
    public bool CanRotate;

    public Vector3 Rotation
    {
        get
        {
            var q = transform.rotation.eulerAngles.normalized;
            return q;
        }
    }

    private void Awake()
    {
        Angles = new float[rotationAxes.Length];
    }

    public void SetStartPositionRotation()
    {
        transform.localPosition = startLocalPosition;
        transform.rotation = Quaternion.Euler(new Vector3(0, 0, 0));
    }

    public override void RotateAround(float[] rotateValues)
    {
        for (var index = 0; index < rotationAxes.Length; index++)
        {
            if (rotateValues.Length < index + 1)
                break;

            transform.RotateAround(transform.position, rotationAxes[index], rotateValues[index] * Speed);

            Angles[index] = index == 0
                ? WrapAngle(transform.rotation.eulerAngles).x
                : WrapAngle(transform.rotation.eulerAngles).z;
            IsOutOfRange = !IsInRanges();

            //if (!IsInRanges())
            //    transform.RotateAround(transform.position, rotationAxes[index], -rotateValues[index] * Speed);
        }
    }

    public void RotateAround(float rotateValue)
    {
        transform.RotateAround(transform.position, rotationAxes[0], rotateValue * Speed);

        Angles[0] = WrapAngle(transform.rotation.eulerAngles).x;
        IsOutOfRange = !IsInRanges();
        
        //if (!IsInRanges())
        //    transform.RotateAround(transform.position, rotationAxes[0], -rotateValue * Speed);
    }
    
    public void RotateAround(float rotateValue, Vector3 rotationAxis)
    {
        transform.RotateAround(transform.position, rotationAxis, rotateValue * Speed);

        Angles[0] = WrapAngle(transform.rotation.eulerAngles).x;
        IsOutOfRange = !IsInRanges();
    }

    public void RotateTo(Vector3 targetRotation)
    {
        IsRotated = true;

        if (rotationAxes.Length == 2)
            StartCoroutine(RotateXY(targetRotation));
        else
            StartCoroutine(RotateX(targetRotation));
    }
    
    private float _eps = 1f;
    
    private IEnumerator RotateXY(Vector3 targetRotation)
    {
        while (Math.Abs(WrapAngle(transform.rotation.eulerAngles.x) - targetRotation.x) > _eps
               || Math.Abs(WrapAngle(transform.rotation.eulerAngles.y) - targetRotation.y) > _eps)
        {
            //print(WrapAngle(transform.rotation.eulerAngles.x) + "  ---  " + targetRotation.x);
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(targetRotation), Speed * Time.deltaTime);
            yield return new WaitForFixedUpdate();
        }

        transform.rotation = Quaternion.Euler(targetRotation);
        IsRotated = false;
        CanRotate = false;
    }     
    
    private IEnumerator RotateX(Vector3 targetRotation)
    {
        while (Math.Abs(WrapAngle(transform.rotation.eulerAngles.x) - targetRotation.x) > _eps)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(targetRotation), Speed * Time.deltaTime);
            yield return new WaitForFixedUpdate();
        }

        transform.rotation = Quaternion.Euler(targetRotation);
        IsRotated = false;
        CanRotate = false;
    } 

    private bool IsInRanges()
    {
        bool res = true;
        foreach (var rotationAx in rotationAxes)
        {
            if (rotationAx == Vector3.forward)
                res = res && (WrapAngle(transform.rotation.eulerAngles).z > minAngle &&
                              WrapAngle(transform.rotation.eulerAngles).z < maxAngle);
            else if (rotationAx == Vector3.right)
                    res = res && (WrapAngle(transform.rotation.eulerAngles).x > minAngle &&
                                  WrapAngle(transform.rotation.eulerAngles).x < maxAngle);
        }

        return res;
    }

    private Vector3 WrapAngle(Vector3 angle)
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
    
    private float WrapAngle(float angle)
    {
        angle%=360;
        if(angle >180)
            return angle - 360;
 
        return angle;
    }
}