using Unity.MLAgents.Sensors;
using UnityEngine;
using Random = UnityEngine.Random;
using static MathFunctions;

public class ThreeArmsAgent : TwoArmsRobotAgent
{
    private Vector3 anglesOld1;
    private Vector3 anglesOld2;
    private Vector3 anglesOld3;

    private string anglesArray;

    private float minMaxValue = 2;

    [Header("Testing")] [SerializeField] private Transform endposition;
    public Vector3 newPosition;

    public override void CollectObservations(VectorSensor sensor)
    {
        // old state size = 3 (cubePosition) + 3 (cubePosition - endPosition) + 3 (endPosition) +
        // 3 (jointPoint.position) + 3 * 3 (robotPart.Rotation for 3 joints) = 21

        // updated state size = 3 (cubePosition) + 3 (cubePosition - endPosition) + 3 (endPosition) +
        // 3 (robotPart.RotationFloat for 3 joints) = 12

        // The position of the cube and the upper arm (6 float numbers)
        cubePosition = cube.transform.position - robot.transform.position;
        // var cubePosNorm = new Vector3(Normalization(cubePosition.x, -minMaxValue, minMaxValue),
        //     Normalization(cubePosition.y, -minMaxValue, minMaxValue),
        //     Normalization(cubePosition.z, -minMaxValue, minMaxValue));

        sensor.AddObservation(cubePosition);

        endPosition = endEffector.transform.position - robot.transform.position;

        // The relative position between the hand and the cube (3 float numbers)
        sensor.AddObservation(cubePosition - endPosition);

        sensor.AddObservation(endPosition);

        // всегда 010 - возможно, передается только локальные значения
        //sensor.AddObservation(jointPoint.position);

        // rotation of each robohand part
        foreach (var robotPart in roboParts)
            sensor.AddObservation(robotPart.RotationFloat);
    }

    public override void OnActionReceived(float[] vectorAction)
    {
        //float[] testVector = {Random.Range(-10f, 10f), Random.Range(-10f, 10f), Random.Range(-10f, 10f)};
        _actions = vectorAction;

        anglesOld1 = roboParts[0].transform.localRotation.eulerAngles;
        anglesOld2 = roboParts[1].transform.localRotation.eulerAngles;
        anglesOld3 = roboParts[2].transform.localRotation.eulerAngles;

        for (var index = 0; index < roboParts.Length; index++)
        {
            var roboPart = roboParts[index];
            //roboPart.RotateSimple(_actions[index]);
            //roboPart.RotateAround(_actions[index]);
            roboPart.Rotate(_actions[index]);
        }

        if (touchDetector.hasTouchedTarget)
        {
            SetReward(10f);
            _curReward = 10;
            EndEpisode();
        }

        SetReward(-1);
        _curReward = -1;
        UpdateInfo();
    }

    protected virtual void UpdateInfo()
    {
        var angles1 = roboParts[0].transform.localRotation.eulerAngles;
        var angles2 = roboParts[1].transform.localRotation.eulerAngles;
        var angles3 = roboParts[2].transform.localRotation.eulerAngles;

        var angle1 = angles1.y;
        var angle2 = angles2.x;
        var angle3 = angles3.x;
        
        if (angles1.x != 0)
            angle1 = 180 - angle1;
        if (angles2.y != 0)
            angle2 = 180 - angle2;
        if (angles3.y != 0)
            angle3 = 180 - angle3;
        
        newPosition = GetPosition(RotateAxis.OY, angle1, 
                                    RotateAxis.OX, angle2, 
                                    RotateAxis.OX, angle3,
                                    RotateAxis.OY, 0.4048796f, endposition.position);
        
        cubePosition = cube.transform.position - robot.transform.position;

        anglesArray =
            $"\nAngles old {roboParts[0].gameObject.name}: {anglesOld1}\nAngles old {roboParts[1].gameObject.name}: {anglesOld2}\nAngles old {roboParts[2].gameObject.name}: {anglesOld3}" +
            $"\nAngles after action {roboParts[0].gameObject.name}: {angles1}\nAngles after action {roboParts[1].gameObject.name}: {angles2}\nAngles after action {roboParts[2].gameObject.name}: {angles3}" +
            $"\nCube position: {cubePosition}";

        var obs = string.Join("  ", GetObservations());
        var act = string.Join("  ", _actions);
        var str =
            $"Cumulative reward: {GetCumulativeReward()} \nCurrent reward: {_curReward} \nState: {obs} \nAction: {act} " +
            anglesArray;
        text.text = str;
    }

    private void OnDrawGizmos()
    {
        //Gizmos.color = Color.green;
        //Gizmos.DrawSphere(endposition.position, .02f);
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(newPosition, .02f);
    }

    private float Normalization(float value, float min, float max) =>
        (value - min) / (max - min);
}