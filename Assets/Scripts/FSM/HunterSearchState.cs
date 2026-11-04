using UnityEngine;

public class HunterSearchState : HunterState
{
    private NavigationNode lastSearchNode;
    private float waitTimer;

    public string Status { get; private set; } = "Sin iniciar";

    public HunterSearchState(HunterAgent hunter, FiniteStateMachine stateMachine)
        : base(hunter, stateMachine) { }

    public override void Enter()
    {
        ResetSearch();
        ResumeSearch();
    }

    public override void Update()
    {
        if (CheckTransitions()) return;
        ContinueSearch();
    }

    private bool CheckTransitions()
    {
        if (TryStartFlee()) return true;
        if (TryStartExit()) return true;
        ForgetLostTarget();
        return TryStartGather() || TryStartAttack();
    }

    private void ContinueSearch()
    {
        UpdateSpawning();
        if (!FollowCurrentTarget()) UpdateNavigation();
    }

    public override void Exit()
    {
        hunter.PathFollower.StopFollowing();
    }

    private void ResetSearch()
    {
        lastSearchNode = null;
        waitTimer = 0f;
    }

    private bool TryStartAttack()
    {
        if (!hunter.HuntingGround.Contains(hunter.transform.position)) return false;
        if (!hunter.Combat.IsReady) return false;
        BoidHealth target = hunter.CurrentTarget;
        if (target == null)
            target = hunter.Sensor.NearestLivingBoid;
        if (!hunter.Sensor.CanSeeLivingBoid(target)) return false;
        hunter.SetTarget(target);
        stateMachine.ChangeState(hunter.AttackState);
        return true;
    }

    private bool TryStartExit()
    {
        if (!hunter.HasEnoughGathers) return false;

        stateMachine.ChangeState(hunter.ExitState);
        return true;
    }

    private void ResumeSearch()
    {
        ForgetLostTarget();
        if (FollowCurrentTarget()) return;

        UpdateNavigation();
    }

    private void ForgetLostTarget()
    {
        if (hunter.CurrentTarget == null) return;
        if (hunter.Sensor.CanSeeLivingBoid(hunter.CurrentTarget)) return;

        hunter.SetTarget(null);
        hunter.PathFollower.StopFollowing();
    }

    private bool FollowCurrentTarget()
    {
        BoidHealth target = hunter.CurrentTarget;
        if (target == null) return false;

        if (hunter.PathFollower.HasPath)
            hunter.PathFollower.StopFollowing();

        Vector3 direction = target.transform.position - hunter.transform.position;
        direction.y = 0f;
        float meleeRadius = hunter.Combat.MeleeAttackRadius;

        if (direction.sqrMagnitude <= meleeRadius * meleeRadius
            || !hunter.Sensor.HasClearPath(target.transform.position))
        {
            hunter.Movement.Stop();
            Status = "Conservando la presa visible mientras termina el TBA";
            return true;
        }

        hunter.Movement.MoveTo(target.transform.position);
        Status = "Siguiendo a la misma presa mientras termina el TBA";
        return true;
    }

    private bool TryStartGather()
    {
        if (!hunter.HuntingGround.Contains(hunter.transform.position)) return false;
        BoidHealth corpse = hunter.Sensor.NearestCorpse;
        if (!hunter.Sensor.CanSeeCorpse(corpse)) return false;
        hunter.SetTarget(corpse);
        stateMachine.ChangeState(hunter.GatherState);
        return true;
    }

    private void UpdateSpawning()
    {
        if (hunter.InterestSpawner == null) return;
        if (!hunter.HuntingGround.Contains(hunter.transform.position)) return;

        hunter.InterestSpawner.Tick(hunter.HuntingGround);
    }

    private void UpdateNavigation()
    {
        if (hunter.PathFollower.HasPath || hunter.Movement.HasDestination) return;

        waitTimer -= Time.deltaTime;
        if (waitTimer > 0f) return;

        if (!hunter.HuntingGround.Contains(hunter.transform.position))
        {
            MoveToHuntingGround();
            return;
        }

        MoveToSearchNode();
    }

    private void MoveToHuntingGround()
    {
        StartRoute(hunter.HuntingGroundEntry, "Viajando al coto mediante A*");
    }

    private void MoveToSearchNode()
    {
        NavigationNode destination = SelectSearchNode();

        if (destination == null)
        {
            Status = "Faltan nodos distintos y válidos dentro del coto";
            waitTimer = hunter.PathRetryTime;
            return;
        }

        if (StartRoute(destination, "Buscando en el coto mediante A*"))
            lastSearchNode = destination;
    }

    private NavigationNode SelectSearchNode()
    {
        NavigationNode selected = null;
        int candidates = 0;

        for (int i = 0; i < hunter.SearchNodes.Count; i++)
        {
            NavigationNode node = hunter.SearchNodes[i];

            if (node == null || node == lastSearchNode) continue;
            if (!hunter.HuntingGround.Contains(node.Position)) continue;

            candidates++;
            if (Random.Range(0, candidates) == 0)
                selected = node;
        }

        return selected;
    }

    private bool StartRoute(NavigationNode destination, string description)
    {
        bool foundPath = hunter.PathFollower.SetDestination(destination, true);

        Status = foundPath ? description : "Sin camino: revisar el grafo y los obstáculos";
        waitTimer = foundPath ? hunter.SearchWaitTime : hunter.PathRetryTime;

        return foundPath;
    }
}
