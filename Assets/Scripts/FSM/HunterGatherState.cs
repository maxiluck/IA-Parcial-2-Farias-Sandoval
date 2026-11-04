using UnityEngine;

public class HunterGatherState : HunterState
{
    private Vector3 knownCorpsePosition;
    private float pathRetryTimer;
    private bool collecting;
    private bool usingDirectApproach;

    public float Remaining { get; private set; }
    public bool IsCollecting => collecting;
    public string Status { get; private set; } = "Sin cadáver";

    public HunterGatherState(HunterAgent hunter, FiniteStateMachine stateMachine)
        : base(hunter, stateMachine) { }

    public override void Enter()
    {
        PrepareGather();
    }

    public override void Update()
    {
        if (TryStartFlee()) return;
        if (CancelUnavailableTarget()) return;

        UpdateGather();
    }

    public override void Exit()
    {
        StopGather();
    }

    private void UpdateGather()
    {
        if (CanGather())
        {
            UpdateCollection();
            return;
        }

        UpdateApproach();
    }

    private void PrepareGather()
    {
        hunter.PathFollower.StopFollowing();
        knownCorpsePosition = hunter.CurrentTarget != null
            ? hunter.CurrentTarget.transform.position : hunter.transform.position;
        Remaining = hunter.GatherDuration;
        pathRetryTimer = 0f;
        collecting = false;
        usingDirectApproach = false;
        Status = "Acercándose al cadáver detectado";
    }

    private bool CancelUnavailableTarget()
    {
        BoidHealth target = hunter.CurrentTarget;
        if (target != null && target.IsDead && !target.IsCollected
            && target.gameObject.activeInHierarchy) return false;

        stateMachine.ChangeState(hunter.SearchState);
        return true;
    }

    private bool CanGather()
    {
        Vector3 direction = knownCorpsePosition - hunter.transform.position;
        direction.y = 0f;

        return direction.sqrMagnitude <= hunter.GatherDistance * hunter.GatherDistance
            && hunter.Sensor.CanSeeCorpse(hunter.CurrentTarget);
    }

    private void UpdateCollection()
    {
        if (!collecting)
        {
            hunter.PathFollower.StopFollowing();
            collecting = true;
            usingDirectApproach = false;
        }

        Status = "Recolectando cadáver";
        Remaining = Mathf.Max(0f, Remaining - Time.deltaTime);
        if (Remaining <= 0f)
            CompleteGather();
    }

    private void CompleteGather()
    {
        if (hunter.CurrentTarget.Collect())
            hunter.RegisterGather();

        State nextState = hunter.HasEnoughGathers ? hunter.ExitState : hunter.SearchState;
        stateMachine.ChangeState(nextState);
    }

    private void UpdateApproach()
    {
        collecting = false;
        Remaining = hunter.GatherDuration;

        if (hunter.Sensor.CanSeeCorpse(hunter.CurrentTarget)
            && hunter.Sensor.HasClearPath(knownCorpsePosition))
        {
            MoveDirectly();
            return;
        }

        UpdateApproachPath();
    }

    private void MoveDirectly()
    {
        if (hunter.PathFollower.HasPath)
            hunter.PathFollower.StopFollowing();

        hunter.Movement.MoveTo(knownCorpsePosition);
        usingDirectApproach = true;
        pathRetryTimer = 0f;
        Status = "Arrive: acercándose al cadáver visible";
    }

    private void UpdateApproachPath()
    {
        if (usingDirectApproach)
        {
            hunter.PathFollower.StopFollowing();
            usingDirectApproach = false;
        }

        pathRetryTimer -= Time.deltaTime;
        if (hunter.PathFollower.HasPath || pathRetryTimer > 0f) return;

        bool foundPath = hunter.PathFollower.SetDestination(knownCorpsePosition);
        pathRetryTimer = hunter.PathRetryTime;
        Status = foundPath ? "A*: viajando a la posición conocida del cadáver"
            : "Sin camino hacia el cadáver";
    }

    private void StopGather()
    {
        hunter.PathFollower.StopFollowing();
        hunter.SetTarget(null);
        Remaining = 0f;
        collecting = false;
        usingDirectApproach = false;
    }
}
