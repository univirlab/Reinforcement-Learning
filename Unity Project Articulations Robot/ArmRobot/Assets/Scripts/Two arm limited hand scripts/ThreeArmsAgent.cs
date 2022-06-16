using UnityEngine;

public class ThreeArmsAgent : TwoArmsRobotAgent
{
    
    public override void OnActionReceived(float[] vectorAction)
    {
        //float[] testVector = {Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f)};
        _actions = vectorAction;
        for (var index = 0; index < roboParts.Length; index++)
        {
            var roboPart = roboParts[index];
            roboPart.RotateAround(_actions[index]);
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
}
