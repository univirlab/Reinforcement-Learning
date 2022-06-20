using Unity.MLAgents.Sensors;
using UnityEngine;

public class ThreeArmsAgent : TwoArmsRobotAgent
{
    private Vector3 anglesOld1;
    private Vector3 anglesOld2;
    private Vector3 anglesOld3;

    private string anglesArray;

    [SerializeField] private Transform jointPoint;

    public override void CollectObservations(VectorSensor sensor)
    {
        // state size = 3 (cubePosition) + 3 (cubePosition - endPosition) + 3 (endPosition) +
        // 3 (jointPoint.position) + 3 * 3 (robotPart.Rotation for 3 joints) = 21
        
        // The position of the cube and the upper arm (6 float numbers)
        cubePosition = cube.transform.position - robot.transform.position;
        sensor.AddObservation(cubePosition);
        
        endPosition = endEffector.transform.position - robot.transform.position;

        // The relative position between the hand and the cube (3 float numbers)
        sensor.AddObservation(cubePosition - endPosition);

        sensor.AddObservation(endPosition);
        
        //добавить позицию сустава
        
        sensor.AddObservation(jointPoint.position);
        
        // rotation of each robohand part
        foreach (var robotPart in roboParts)
            sensor.AddObservation(robotPart.Rotation);
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
            roboPart.RotateSimple(_actions[index]);
            //roboPart.RotateAround(_actions[index]);
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

        anglesArray =
            //$"\nAngles old: {anglesOld1.y} {anglesOld2.x} {anglesOld3.x}  Angles new: {angles1.y} {angles2.x} {angles3.x}";
            $"\nAngles old {roboParts[0].gameObject.name}: {anglesOld1}\nAngles old {roboParts[1].gameObject.name}: {anglesOld2}\nAngles old {roboParts[2].gameObject.name}: {anglesOld3}" +
            $"\nAngles after action {roboParts[0].gameObject.name}: {angles1}\nAngles after action {roboParts[1].gameObject.name}: {angles2}\nAngles after action {roboParts[2].gameObject.name}: {angles3}" +
            $"\nCube position: {cube.transform.localEulerAngles}";

        var obs = string.Join("  ", GetObservations());
        var act = string.Join("  ", _actions);
        var str =
            $"Cumulative reward: {GetCumulativeReward()} \nCurrent reward: {_curReward} \nState: {obs} \nAction: {act} " +
            anglesArray;
        text.text = str;
    }
}
