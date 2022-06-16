using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using UnityEngine.UI;

public class TwoArmsLimitedRobotAgent : Agent
{
    // можно накапливать action в переменную - на какой угол в сумме мы уже повернулись по каждой оси - получаем по два числа для каждого сочленения вместо трех (rotation)
    
    [SerializeField] private Text text;
    
    public GameObject endEffector;
    public GameObject cube;
    public GameObject robot;

    public LimitedRobotPart[] roboParts;

    TablePositionRandomizer tablePositionRandomizer;
    protected TwoArmTouchDetector touchDetector;

    public Vector3 cubePosition;
    public Vector3 endPosition;

    protected float _curReward;
    protected float[] _actions;
    
    private void Start()
    {
        touchDetector = cube.GetComponent<TwoArmTouchDetector>();
        tablePositionRandomizer = cube.GetComponent<TablePositionRandomizer>();
        
        Academy.Instance.AutomaticSteppingEnabled = false;
        RequestDecision();
        Academy.Instance.EnvironmentStep();
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
        float[] testVector = {Random.Range(-89, 89f), Random.Range(-89f, 89f)};
        _actions = testVector;

        roboParts[0].CanRotate = true;
        roboParts[1].CanRotate = true;
       
        if (touchDetector.hasTouchedTarget)
        {
            SetReward(10f);
            _curReward = 10;
            EndEpisode();
        }
        SetReward(-1);
        _curReward = -1;
    }

    protected virtual void FixedUpdate()
    {
        if (roboParts[0].CanRotate && !roboParts[0].IsRotated)
        {
            var angle = new Vector3(_actions[0], _actions[1], 0);
            roboParts[0].RotateTo(angle);
        }
        else if (!roboParts[0].CanRotate && !roboParts[0].IsRotated)
        {
            if (roboParts[1].CanRotate && !roboParts[1].IsRotated)
            {
                var angle = new Vector3(_actions[2], 0, 0);
                roboParts[1].RotateTo(angle);
            }
        }

        if (!roboParts[0].CanRotate && !roboParts[1].CanRotate)
        {
            RequestDecision();
            Academy.Instance.EnvironmentStep();
        }

        
        
        UpdateInfo();
    }

    protected void UpdateInfo()
    {
        var obs = string.Join("  ", GetObservations());
        var act = string.Join("  ", _actions);
        var str =
            $"Cumulative reward: {GetCumulativeReward()} \nCurrent reward: {_curReward} \nState: {obs} \nAction: {act}";
        text.text = str;
    }
}