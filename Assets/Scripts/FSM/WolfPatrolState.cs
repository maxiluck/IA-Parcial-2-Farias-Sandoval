using UnityEngine;

public class WolfPatrolState : WolfState
{
    private int currentIndex;
    private float waitTimer;

    public WolfPatrolState(WolfAgent wolf, FiniteStateMachine stateMachine)
        : base(wolf, stateMachine) { }

    public override void Enter()
    {
        MoveToPatrolPoint();
    }

    public override void Update()
    {
        ForgetProtectedTarget();
        if (TryStartChase()) return;
        if (TryStartSearch()) return;

        UpdatePatrol();
    }

    public override void Exit()
    {
        wolf.PathFollower.StopFollowing();
    }

    private void ForgetProtectedTarget()
    {
        if (wolf.Perception.TargetIsProtected)
            wolf.Communication.ForgetKnownPosition();
    }

    private bool TryStartSearch()
    {
        if (!wolf.Communication.HasKnownPosition) return false;

        stateMachine.ChangeState(wolf.SearchState);
        return true;
    }

    private void UpdatePatrol()
    {
        if (wolf.PatrolPoints.Count == 0) return;

        if (wolf.PathFollower.HasPath || wolf.Movement.HasDestination)
            return;

        UpdateWait();
    }

    private void UpdateWait()
    {
        waitTimer -= Time.deltaTime;
        if (waitTimer > 0f) return;

        currentIndex = (currentIndex + 1) % wolf.PatrolPoints.Count;
        MoveToPatrolPoint();
    }

    private void MoveToPatrolPoint()
    {
        waitTimer = wolf.PatrolWaitTime;
        if (wolf.PatrolPoints.Count == 0) return;

        wolf.PathFollower.SetDestination(wolf.PatrolPoints[currentIndex]);
    }
}
