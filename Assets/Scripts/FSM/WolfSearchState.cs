using UnityEngine;

public class WolfSearchState : WolfState
{
    private Vector3 lastKnownPosition;
    private int knownPositionVersion;
    private bool routeFound;
    private bool finalApproachStarted;
    private float retryTimer;
    private float lookTimer;

    public WolfSearchState(WolfAgent wolf, FiniteStateMachine stateMachine)
        : base(wolf, stateMachine) { }

    public override void Enter()
    {
        InitializeSearch();
        CalculateSearchPath();
    }

    public override void Update()
    {
        if (ReturnToPatrol()) return;
        if (TryStartChase()) return;
        if (TryUpdateSearchPosition()) return;

        UpdateSearch();
    }

    public override void Exit()
    {
        wolf.PathFollower.StopFollowing();
    }

    private void InitializeSearch()
    {
        lastKnownPosition = wolf.Communication.KnownPosition;
        knownPositionVersion = wolf.Communication.PositionVersion;
        finalApproachStarted = false;
        lookTimer = wolf.SearchLookTime;
    }

    private bool TryUpdateSearchPosition()
    {
        if (knownPositionVersion == wolf.Communication.PositionVersion)
            return false;

        InitializeSearch();
        CalculateSearchPath();
        return true;
    }

    private void UpdateSearch()
    {
        if (!routeFound)
        {
            RetrySearchPath();
            LookAround();
            return;
        }

        if (wolf.PathFollower.HasPath || wolf.Movement.HasDestination)
            return;

        if (!finalApproachStarted)
        {
            StartFinalApproach();
            return;
        }

        UpdateAreaSearch();
    }

    private void CalculateSearchPath()
    {
        if (wolf.Movement.HasClearPath(lastKnownPosition))
        {
            wolf.PathFollower.StopFollowing();
            wolf.Movement.MoveTo(lastKnownPosition);
            routeFound = true;
            finalApproachStarted = true;
        }
        else
        {
            routeFound = wolf.PathFollower.SetDestination(lastKnownPosition);
        }

        retryTimer = wolf.SearchRetryTime;
    }

    private void RetrySearchPath()
    {
        retryTimer -= Time.deltaTime;
        if (retryTimer <= 0f)
            CalculateSearchPath();
    }

    private void StartFinalApproach()
    {
        finalApproachStarted = true;
        if (wolf.Movement.HasClearPath(lastKnownPosition))
            wolf.Movement.MoveTo(lastKnownPosition);
    }

    private void UpdateAreaSearch()
    {
        lookTimer -= Time.deltaTime;
        if (lookTimer > 0f)
        {
            LookAround();
            return;
        }

        ExploreNearbyNode();
    }

    private void ExploreNearbyNode()
    {
        lookTimer = wolf.SearchLookTime;

        NavigationNode destination = wolf.PathFollower.GetSearchNode(
            lastKnownPosition, wolf.SearchRadius);

        if (destination != null)
            wolf.PathFollower.SetDestination(destination, true);
    }

    private void LookAround()
    {
        wolf.transform.Rotate(Vector3.up, wolf.SearchTurnSpeed * Time.deltaTime);
    }
}
