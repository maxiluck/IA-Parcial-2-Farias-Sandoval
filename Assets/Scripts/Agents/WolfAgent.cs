using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AgentMovement), typeof(AgentPathFollower), typeof(AgentPerception))]
[RequireComponent(typeof(WolfCommunication), typeof(WolfCombat))]
public class WolfAgent : MonoBehaviour
{
    [Header("Patrulla")]
    [SerializeField] private List<NavigationNode> patrolPoints = new List<NavigationNode>();
    [SerializeField, Min(0f)] private float patrolWaitTime = 0.5f;
    [SerializeField, Min(0.1f)] private float patrolRetryTime = 1f;

    [Header("Busqueda")]
    [SerializeField, Min(0f)] private float searchTurnSpeed = 90f;
    [SerializeField, Min(0.1f)] private float searchRetryTime = 1f;
    [SerializeField, Min(0.1f)] private float searchLookTime = 3f;
    [SerializeField, Min(1f)] private float searchRadius = 25f;

    private AgentMovement movement;
    private AgentPathFollower pathFollower;
    private AgentPerception perception;
    private WolfCommunication communication;
    private WolfCombat combat;

    private FiniteStateMachine stateMachine;
    private WolfPatrolState patrolState;
    private WolfChaseState chaseState;
    private WolfSearchState searchState;

    public IReadOnlyList<NavigationNode> PatrolPoints => patrolPoints;
    public float PatrolWaitTime => patrolWaitTime;
    public float PatrolRetryTime => patrolRetryTime;
    public float SearchTurnSpeed => searchTurnSpeed;
    public float SearchRetryTime => searchRetryTime;
    public float SearchLookTime => searchLookTime;
    public float SearchRadius => searchRadius;

    public AgentMovement Movement => movement;
    public AgentPathFollower PathFollower => pathFollower;
    public AgentPerception Perception => perception;
    public WolfCommunication Communication => communication;
    public WolfCombat Combat => combat;

    public State PatrolState => patrolState;
    public State ChaseState => chaseState;
    public State SearchState => searchState;

    private void Awake()
    {
        CacheComponents();
        CreateStateMachine();
    }

    private void Start()
    {
        StartPatrol();
    }

    private void Update()
    {
        stateMachine.Update();
    }

    private void CacheComponents()
    {
        movement = GetComponent<AgentMovement>();
        pathFollower = GetComponent<AgentPathFollower>();
        perception = GetComponent<AgentPerception>();
        communication = GetComponent<WolfCommunication>();
        combat = GetComponent<WolfCombat>();
    }

    private void CreateStateMachine()
    {
        stateMachine = new FiniteStateMachine();
        patrolState = new WolfPatrolState(this, stateMachine);
        chaseState = new WolfChaseState(this, stateMachine);
        searchState = new WolfSearchState(this, stateMachine);
    }

    private void StartPatrol()
    {
        stateMachine.ChangeState(patrolState);
    }
}
