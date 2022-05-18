using System;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;

public class TwoArmRobotAgent : Agent
{
    public Transform endEffector;
    public Transform handBase;
    public Transform robotPoint;
    public GameObject cube;
    public GameObject robot;
    
    //Rotate rotateCube;
    
    RobotController robotController;
    TablePositionRandomizer tablePositionRandomizer;

    private void Awake()
    {
        //rotateCube = cube.GetComponent<Rotate>();
    }

    private void Start()
    {
        robotController = robot.GetComponent<RobotController>();
        tablePositionRandomizer = cube.GetComponent<TablePositionRandomizer>();
    }

    public override void OnEpisodeBegin()
    {
         float[] defaultRotations = { 0.0f };
         robotController.ForceJointsToRotations(defaultRotations);
         tablePositionRandomizer.Move();
         //rotateCube.UpdateSpeed();
    }

    // private void OnDrawGizmos()
    // {
    //     Gizmos.DrawLine(handBase.position, handBase.position + handBase.forward);
    //     Gizmos.DrawRay(cube.transform.position, cube.transform.position - robotPoint.transform.position);
    // }

    /*private void Update()
    {
        Vector3 forward = handBase.position + handBase.forward;//handBase.TransformDirection(handBase.forward);
        Vector3 toOther = cube.transform.position - robotPoint.transform.position;//cube.transform.position - robot.transform.position;

        var dot = Vector3.Dot(forward.normalized, toOther.normalized);
        //if (Vector3.Dot(forward, toOther) < 0)
        {
            print(dot);
        }
        //print("ideal " + (handBase.position - cube.transform.position) + " current " + (endEffector.position - cube.transform.position));
    }*/

    public override void CollectObservations(VectorSensor sensor)
    {
        if (robotController.joints[0].robotPart == null)
        {
            // No robot is present, no observation should be added
            return;
        }

        // relative cube position
        Vector3 cubePosition = cube.transform.position - robot.transform.position;
        //sensor.AddObservation(cubePosition);
        //
        // relative end position
        Vector3 endPosition = handBase.position + handBase.forward;//endEffector.transform.position - robot.transform.position;
        //sensor.AddObservation(endPosition);
        sensor.AddObservation(endPosition);
        sensor.AddObservation(cubePosition - endPosition);
    }

    public override void OnActionReceived(float[] vectorAction)
    {
        // move
        for (int jointIndex = 0; jointIndex < vectorAction.Length; jointIndex ++)
        {
            RotationDirection rotationDirection = ActionIndexToRotationDirection((int) vectorAction[jointIndex]);
            robotController.RotateJoint(jointIndex, rotationDirection, false);
        }
        
        Vector3 forward = handBase.position + handBase.forward;
        Vector3 toOther = cube.transform.position - robotPoint.transform.position;

        var dot = Vector3.Dot(forward.normalized, toOther.normalized);


        var reward = dot;
        print(reward);

        SetReward(reward);
    }

    private static RotationDirection ActionIndexToRotationDirection(int actionIndex) => 
        (RotationDirection)(actionIndex - 1);
}


