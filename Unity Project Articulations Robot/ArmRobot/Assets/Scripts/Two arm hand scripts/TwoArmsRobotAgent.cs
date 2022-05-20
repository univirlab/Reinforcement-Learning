using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using UnityEngine.UI;

public class TwoArmsRobotAgent : Agent
{
    // можно накапливать action в переменную - на какой угол в сумме мы уже повернулись по каждой оси - получаем по два числа для каждого сочленения вместо трех (rotation)
    
    [SerializeField] private Text text;
    
    public GameObject endEffector;
    public GameObject cube;
    public GameObject robot;

    public RobotPart[] roboParts;

    TwoArmsTablePositionRandomizer tablePositionRandomizer;
    TwoArmTouchDetector touchDetector;

    public Vector3 cubePosition;
    public Vector3 endPosition;

    private float _curReward;
    private float[] _actions;

    private void Start()
    {
        touchDetector = cube.GetComponent<TwoArmTouchDetector>();
        tablePositionRandomizer = cube.GetComponent<TwoArmsTablePositionRandomizer>();
    }

    public override void OnEpisodeBegin()
    {
        foreach (var robotPart in roboParts)
            robotPart.SetStartPositionRotation();
        
        touchDetector.hasTouchedTarget = false;
        tablePositionRandomizer.Move();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // The position of the cube and the upper arm (6 float numbers)
        cubePosition = cube.transform.position - robot.transform.position;
        sensor.AddObservation(cubePosition);
        
        endPosition = endEffector.transform.position - robot.transform.position;

        // The relative position between the hand and the cube (3 float numbers)
        sensor.AddObservation(cubePosition - endPosition);

        sensor.AddObservation(endPosition);

        // rotation of each robohand part - quaternions (4 floats for each)
        foreach (var robotPart in roboParts)
            sensor.AddObservation(robotPart.Rotation);
    }


    public override void OnActionReceived(float[] vectorAction)
    {
        //float[] testVector = {Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f)};
        _actions = vectorAction;
        int indexOfAction = 0;
        for (var index = 0; index < roboParts.Length; index++)
        {
            var roboPart = roboParts[index];
            roboPart.RotateAround(new[] {vectorAction[indexOfAction], vectorAction[indexOfAction + 1]});
            indexOfAction += 2;
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

    private void UpdateInfo()
    {
        var obs = string.Join("  ", GetObservations());
        var act = string.Join("  ", _actions);
        var str =
            $"Cumulative reward: {GetCumulativeReward()} \nCurrent reward: {_curReward} \nState: {obs} \nAction: {act}";
        text.text = str;
    }
}