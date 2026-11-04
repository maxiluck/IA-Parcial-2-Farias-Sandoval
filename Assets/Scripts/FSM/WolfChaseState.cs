using UnityEngine;

public class WolfChaseState : WolfState
{
    private float retryTimer;

    public WolfChaseState(WolfAgent wolf, FiniteStateMachine stateMachine)
        : base(wolf, stateMachine) { }

    public override void Enter()
    {
        PrepareChase();
        FollowDetectedPosition();
    }

    public override void Update()
    {
        if (ReturnToPatrol()) return;
        if (LoseSightOf()) return;
        FollowDetectedPosition();
    }

    public override void Exit()
    {
        wolf.PathFollower.StopFollowing();
    }

    private void PrepareChase()
    {
        wolf.PathFollower.StopFollowing();
        retryTimer = 0f;
    }

    private bool LoseSightOf()
    {
        if (wolf.Perception.CanSeeTarget) return false;
        stateMachine.ChangeState(wolf.SearchState);
        return true;
    }

    private void FollowDetectedPosition()
    {
        wolf.Communication.ReportDetection();
        if (wolf.Combat.TryCapture())
        {
            wolf.PathFollower.StopFollowing();
            return;
        }
        Vector3 position = wolf.Perception.DetectedPosition;
        if (wolf.Movement.HasClearPath(position))
        {
            if (wolf.PathFollower.HasPath) wolf.PathFollower.StopFollowing();
            wolf.Movement.MoveTo(position);
            retryTimer = 0f;
            return;
        }
        UpdateChasePath(position);
    }

    private void UpdateChasePath(Vector3 position)
    {
        retryTimer -= Time.deltaTime;
        if (wolf.PathFollower.HasPath || retryTimer > 0f) return;
        wolf.PathFollower.SetDestination(position);
        retryTimer = wolf.SearchRetryTime;
    }
}
