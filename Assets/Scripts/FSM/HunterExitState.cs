using UnityEngine;

public class HunterExitState : HunterState
{
    private float retryTimer;
    private bool routeFound;

    public string Status { get; private set; } = "Sin iniciar";

    public HunterExitState(HunterAgent hunter, FiniteStateMachine stateMachine)
        : base(hunter, stateMachine) { }

    public override void Enter()
    {
        PrepareExit();
        CalculateExitPath();
    }

    public override void Update()
    {
        if (TryStartFlee()) return;
        if (TryCompleteExit()) return;

        UpdateNavigation();
    }

    public override void Exit()
    {
        hunter.PathFollower.StopFollowing();
    }

    private void PrepareExit()
    {
        hunter.PathFollower.StopFollowing();
        hunter.SetTarget(null);
        retryTimer = 0f;
        routeFound = false;
    }

    private void CalculateExitPath()
    {
        routeFound = hunter.PathFollower.SetDestination(hunter.ExitNode, true);
        retryTimer = hunter.PathRetryTime;
        Status = routeFound ? "Viajando a la salida mediante A*"
            : "Sin camino a la salida: revisar conexiones y obstáculos";
    }

    private void UpdateNavigation()
    {
        if (hunter.PathFollower.HasPath || hunter.Movement.HasDestination) return;

        retryTimer -= Time.deltaTime;
        if (retryTimer > 0f) return;

        CalculateExitPath();
    }

    private bool TryCompleteExit()
    {
        if (!routeFound || hunter.PathFollower.HasPath || hunter.Movement.HasDestination)
            return false;

        Vector3 direction = hunter.ExitNode.Position - hunter.transform.position;
        direction.y = 0f;
        float arrivalDistance = hunter.ExitArrivalDistance;
        if (direction.sqrMagnitude > arrivalDistance * arrivalDistance) return false;

        hunter.CompleteCycle();
        stateMachine.ChangeState(hunter.SearchState);
        return true;
    }
}
