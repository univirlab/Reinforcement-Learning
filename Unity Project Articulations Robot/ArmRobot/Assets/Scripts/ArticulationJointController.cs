using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum RotationDirection { None = 0, Positive = 1, Negative = -1 };

public class ArticulationJointController : MonoBehaviour
{
    public bool IsTestArm = false;
    public Vector2 RotateValue;
    
    public RotationDirection rotationState = RotationDirection.None;
    public float speed = 300.0f;

    private ArticulationBody articulation;


    // LIFE CYCLE

    void Awake()
    {
        articulation = GetComponent<ArticulationBody>();
    }

    void FixedUpdate() 
    {
        if (IsTestArm)
        {
            float rotationChangeX = RotateValue.x * speed * Time.fixedDeltaTime;
            float rotationChangeY = RotateValue.y * speed * Time.fixedDeltaTime;
            float rotationGoalX = CurrentPrimaryAxisRotation() + rotationChangeX;
            float rotationGoalY = CurrentPrimaryAxisRotation() + rotationChangeY;
            RotateTo(rotationGoalX, rotationGoalY);
        }
        else
        {
            if (rotationState != RotationDirection.None) {
                float rotationChange = (float)rotationState * speed * Time.fixedDeltaTime;
                float rotationGoal = CurrentPrimaryAxisRotation() + rotationChange;
                RotateTo(rotationGoal);
            }
        }
    }

    public void SetTestRotateValue(Vector2 value)
    {
        RotateValue = value;
    }

    // READ

    public float CurrentPrimaryAxisRotation()
    {
        float currentRotationRads = articulation.jointPosition[0];
        float currentRotation = Mathf.Rad2Deg * currentRotationRads;
        return currentRotation;
    }


    // CONTROL

    public void ForceToRotation(float rotation, bool istest = false)
    {
        // set target
        RotateTo(rotation);
        
        // force position
        float rotationRads = Mathf.Deg2Rad * rotation;
        ArticulationReducedSpace newPosition = new ArticulationReducedSpace(rotationRads);
        articulation.jointPosition = newPosition;

        // force velocity to zero
        ArticulationReducedSpace newVelocity = new ArticulationReducedSpace(0.0f);
        articulation.jointVelocity = newVelocity;
        
    }


    // MOVEMENT HELPERS

    void RotateTo(float primaryAxisRotation, float primaryAxisRotationY = 0)
    {
        var drive = articulation.xDrive;
        drive.target = primaryAxisRotation;
        articulation.xDrive = drive;

        if (IsTestArm)
        {
            var drivey = articulation.yDrive;
            drivey.target = primaryAxisRotationY;
            articulation.yDrive = drivey;
        }
    }




}
