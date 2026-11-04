using System.Collections.Generic;
using UnityEngine;

public class HunterFleeState : State
{
    private HunterAgent hunter;
    private float retryTimer;
    private bool waiting;

    public SafeZone SelectedZone { get; private set; }
    public float RouteLength { get; private set; }
    public float WaitRemaining { get; private set; }
    public string Status { get; private set; } = "Sin refugio";

    public HunterFleeState(HunterAgent hunter, FiniteStateMachine stateMachine)
        : base(stateMachine)
    {
        this.hunter = hunter;
    }

    public override void Enter()
    {
        PrepareFlee();
        ChooseRefuge();
    }

    public override void Update()
    {
        UpdateFlee();
    }

    public override void Exit()
    {
        hunter.PathFollower.StopFollowing();
    }

    private void UpdateFlee()
    {
        if (waiting)
        {
            UpdateSafeWait();
            return;
        }
        UpdateEscape();
    }

    private void PrepareFlee()
    {
        hunter.PathFollower.StopFollowing();
        hunter.SetTarget(null);
        SelectedZone = null;
        RouteLength = 0f;
        WaitRemaining = hunter.SafeZoneWaitTime;
        retryTimer = 0f;
        waiting = false;
    }

    private void ChooseRefuge()
    {
        SelectedZone = hunter.Refuge.SelectNearestReachable(out List<NavigationNode> route, out float length);
        retryTimer = hunter.PathRetryTime;
        if (SelectedZone == null)
        {
            RouteLength = 0f;
            Status = "Sin refugio alcanzable: revisar nodos, zonas y conexiones";
            return;
        }
        RouteLength = length;
        hunter.PathFollower.FollowCalculatedPath(route);
        Status = "Huyendo por A* hacia " + SelectedZone.name;
    }

    private void UpdateEscape()
    {
        if (hunter.PathFollower.HasPath || hunter.Movement.HasDestination) return;
        if (SelectedZone != null && SelectedZone.Contains(hunter.transform.position))
        {
            BeginSafeWait();
            return;
        }
        retryTimer -= Time.deltaTime;
        if (retryTimer <= 0f) ChooseRefuge();
    }

    private void BeginSafeWait()
    {
        hunter.PathFollower.StopFollowing();
        WaitRemaining = hunter.SafeZoneWaitTime;
        waiting = true;
        Status = "Esperando dentro de " + SelectedZone.name;
    }

    private void UpdateSafeWait()
    {
        if (SelectedZone == null || !SelectedZone.Contains(hunter.transform.position))
        {
            waiting = false;
            ChooseRefuge();
            return;
        }
        WaitRemaining = Mathf.Max(0f, WaitRemaining - Time.deltaTime);
        if (WaitRemaining > 0f) return;

        State nextState = hunter.HasEnoughGathers ? hunter.ExitState : hunter.SearchState;
        stateMachine.ChangeState(nextState);
    }
}
