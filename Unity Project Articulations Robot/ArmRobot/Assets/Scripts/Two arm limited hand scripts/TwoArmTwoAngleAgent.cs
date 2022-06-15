using Unity.MLAgents;
using UnityEngine;

public class TwoArmTwoAngleAgent : TwoArmsLimitedRobotAgent
{
    protected override void FixedUpdate()
    {
        if (roboParts[0].CanRotate && !roboParts[0].IsRotated)
        {
            var angle = new Vector3(0, _actions[0], 0);
            roboParts[0].RotateTo(angle);
        }
        else if (!roboParts[0].CanRotate && !roboParts[0].IsRotated)
        {
            if (roboParts[1].CanRotate && !roboParts[1].IsRotated)
            {
                var angle = new Vector3(_actions[1], 0, 0);
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
}
