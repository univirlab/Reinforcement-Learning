using System;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;

public class RobotAgent : Agent
{
    public GameObject endEffector;
    public GameObject cube;
    public GameObject robot;

    RobotController robotController;
    TouchDetector touchDetector;
    TablePositionRandomizer tablePositionRandomizer;

    public Vector3 cubePosition;
    public Vector3 endPosition;
    public Vector3 difference;
    


    private void Start()
    {
        robotController = robot.GetComponent<RobotController>();
        touchDetector = cube.GetComponent<TouchDetector>();
        tablePositionRandomizer = cube.GetComponent<TablePositionRandomizer>();
    }

    public override void OnEpisodeBegin()
    {
        float[] defaultRotations = { 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f };
        robotController.ForceJointsToRotations(defaultRotations);
        touchDetector.hasTouchedTarget = false;
        tablePositionRandomizer.Move();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        if (robotController.joints[0].robotPart == null)
        {
            // No robot is present, no observation should be added
            return;
        }

        // relative cube position
        cubePosition = cube.transform.position - robot.transform.position;
        sensor.AddObservation(cubePosition);

        // relative end position
        endPosition = endEffector.transform.position - robot.transform.position;
        sensor.AddObservation(endPosition);
        sensor.AddObservation(cubePosition - endPosition);
        difference = cubePosition - endPosition;
    }
    

    public override void OnActionReceived(float[] vectorAction)
    {
        // move
        for (int jointIndex = 0; jointIndex < vectorAction.Length; jointIndex ++)
        {
            RotationDirection rotationDirection = ActionIndexToRotationDirection((int) vectorAction[jointIndex]);
            robotController.RotateJoint(jointIndex, rotationDirection, false);
        }

        // Knocked the cube off the table
        if (cube.transform.position.y < -1.0)
        {
            SetReward(-1f);
            EndEpisode();
        }

        // end episode if we touched the cube
        if (touchDetector.hasTouchedTarget)
        {
            SetReward(1f);
            EndEpisode();
        }

        //reward
        float distanceToCube = Vector3.Distance(endEffector.transform.position, cube.transform.position); // roughly 0.7f

        var jointHeight = 0f; // This is to reward the agent for keeping high up // max is roughly 3.0f
        for (int jointIndex = 0; jointIndex < robotController.joints.Length; jointIndex ++)
            jointHeight += robotController.joints[jointIndex].robotPart.transform.position.y - cube.transform.position.y;
        
        var reward = - distanceToCube + jointHeight / 100f;

        SetReward(reward * 0.1f);
    }

    private static RotationDirection ActionIndexToRotationDirection(int actionIndex) => 
        (RotationDirection)(actionIndex - 1);
}


