using UnityEngine;

public class HunterAttackState : HunterState
{
    private AdvanceAgent targetAgent;
    private float pathRetryTimer;
    public string Status { get; private set; } = "Sin objetivo";

    public HunterAttackState(HunterAgent hunter, FiniteStateMachine stateMachine)
        : base(hunter, stateMachine) { }
    public override void Enter()
    {
        PreparePursuit();
    }
    public override void Update()
    {
        if (TryStartFlee()) return;
        if (LoseTarget()) return;
        if (TryCompleteAttack()) return;
        UpdatePursuit();
    }
    public override void Exit()
    {
        StopAttack();
    }

    private void PreparePursuit()
    {
        hunter.PathFollower.StopFollowing();
        pathRetryTimer = 0f;
        targetAgent = hunter.CurrentTarget != null
            ? hunter.CurrentTarget.GetComponent<AdvanceAgent>() : null;
        Status = "Persiguiendo al boid detectado";
    }

    private bool LoseTarget()
    {
        if (hunter.Sensor.CanSeeLivingBoid(hunter.CurrentTarget)) return false;
        // Perder el objetivo no reinicia el TBA.
        stateMachine.ChangeState(hunter.SearchState);
        return true;
    }

    private bool TryCompleteAttack()
    {
        if (!hunter.Combat.TryAttack(hunter.CurrentTarget)) return false;
        stateMachine.ChangeState(hunter.SearchState);
        return true;
    }

    private void UpdatePursuit()
    {
        Vector3 targetPosition = hunter.CurrentTarget.transform.position;
        Vector3 targetVelocity = targetAgent != null ? targetAgent.Velocity : Vector3.zero;
        Vector3 destination = hunter.Combat.PredictPosition(targetPosition, targetVelocity);
        if (!hunter.HuntingGround.Contains(destination)
            || !hunter.Sensor.HasClearPath(destination))
            destination = targetPosition;
        if (hunter.Sensor.HasClearPath(destination))
        {
            MoveDirectly(destination);
            return;
        }
        UpdatePursuitPath(targetPosition);
    }

    private void MoveDirectly(Vector3 destination)
    {
        if (hunter.PathFollower.HasPath)
            hunter.PathFollower.StopFollowing();
        hunter.Movement.MoveTo(destination);
        pathRetryTimer = 0f;
        Status = "Pursuit: anticipando la posición del boid";
    }

    private void UpdatePursuitPath(Vector3 targetPosition)
    {
        pathRetryTimer -= Time.deltaTime;
        if (hunter.PathFollower.HasPath || pathRetryTimer > 0f) return;
        bool foundPath = hunter.PathFollower.SetDestination(targetPosition);
        pathRetryTimer = hunter.PathRetryTime;
        Status = foundPath ? "Rodeando un obstáculo mediante A*" : "Sin camino hacia el boid";
    }

    private void StopAttack()
    {
        hunter.PathFollower.StopFollowing();
        if (!hunter.Sensor.CanSeeLivingBoid(hunter.CurrentTarget))
            hunter.SetTarget(null);
        targetAgent = null;
    }
}
