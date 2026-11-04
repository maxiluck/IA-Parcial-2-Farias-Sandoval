using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AgentMovement), typeof(AgentPathFollower), typeof(HunterTarget))]
[RequireComponent(typeof(HunterSensor), typeof(HunterCombat))]
[RequireComponent(typeof(HunterRefuge), typeof(HunterHealth))]
public class HunterAgent : MonoBehaviour
{
    [Header("Coto de caza")]
    [SerializeField] private Bounds huntingGround;
    [SerializeField] private NavigationNode huntingGroundEntry;
    [SerializeField] private List<NavigationNode> searchNodes = new List<NavigationNode>();
    [SerializeField] private InterestObjectSpawner interestSpawner;

    [Header("Busqueda")]
    [SerializeField, Min(0f)] private float searchWaitTime = 0.5f;
    [SerializeField, Min(0.1f)] private float pathRetryTime = 2f;

    [Header("Recolección")]
    [SerializeField, Min(0.1f)] private float gatherDuration = 2f;
    [SerializeField, Min(0.1f)] private float gatherDistance = 1.5f;

    [Header("Objetivo y reinicio")]
    [SerializeField, Min(1)] private int requiredGathers = 3;
    [SerializeField] private NavigationNode exitNode;
    [SerializeField] private Transform initialPoint;
    [SerializeField, Min(0.01f)] private float exitArrivalDistance = 0.3f;

    [Header("Huida")]
    [SerializeField, Min(0.1f)] private float safeZoneWaitTime = 5f;

    private AgentMovement movement;
    private AgentPathFollower pathFollower;
    private HunterSensor sensor;
    private HunterCombat combat;
    private HunterTarget targetMarker;
    private HunterRefuge refuge;
    private HunterHealth health;
    private FiniteStateMachine stateMachine;
    private HunterSearchState searchState;
    private HunterAttackState attackState;
    private HunterGatherState gatherState;
    private HunterExitState exitState;
    private HunterFleeState fleeState;
    private HunterDeadState deadState;

    public AgentMovement Movement => movement;
    public AgentPathFollower PathFollower => pathFollower;
    public HunterSensor Sensor => sensor;
    public HunterCombat Combat => combat;
    public HunterRefuge Refuge => refuge;
    public HunterHealth Health => health;
    public float SafeZoneWaitTime => safeZoneWaitTime;
    public State FleeState => fleeState;
    public SafeZone SelectedSafeZone => fleeState != null ? fleeState.SelectedZone : null;
    public float FleeWaitRemaining => fleeState != null ? fleeState.WaitRemaining : 0f;
    public float RefugeRouteLength => fleeState != null ? fleeState.RouteLength : 0f;
    public bool ShouldFlee => refuge != null && !refuge.IsInsideSafeZone
        && sensor.CanSeeWolf(sensor.NearestWolf);
    public BoidHealth CurrentTarget { get; private set; }
    public State SearchState => searchState;
    public State AttackState => attackState;
    public State GatherState => gatherState;
    public State ExitState => exitState;
    public int GatherCount { get; private set; }
    public int CompletedCycles { get; private set; }
    public int RequiredGathers => requiredGathers;
    public bool HasEnoughGathers => GatherCount >= requiredGathers;
    public NavigationNode ExitNode => exitNode;
    public float ExitArrivalDistance => exitArrivalDistance;
    public float GatherDuration => gatherDuration;
    public float GatherDistance => gatherDistance;
    public float GatherRemaining => gatherState != null ? gatherState.Remaining : 0f;
    public Bounds HuntingGround => huntingGround;
    public NavigationNode HuntingGroundEntry => huntingGroundEntry;
    public IReadOnlyList<NavigationNode> SearchNodes => searchNodes;
    public InterestObjectSpawner InterestSpawner => interestSpawner;
    public float SearchWaitTime => searchWaitTime;
    public float PathRetryTime => pathRetryTime;
    public bool IsSearching => stateMachine != null && stateMachine.CurrentState == searchState;
    public bool IsAttacking => stateMachine != null && stateMachine.CurrentState == attackState;
    public bool IsGathering => stateMachine != null && stateMachine.CurrentState == gatherState;
    public bool IsCollecting => IsGathering && gatherState.IsCollecting;
    public bool IsExiting => stateMachine != null && stateMachine.CurrentState == exitState;
    public bool IsFleeing => stateMachine != null && stateMachine.CurrentState == fleeState;
    public bool IsDead => health != null && health.IsDead;
    public string StateName => IsDead ? "Dead" : IsFleeing ? "Flee" : IsExiting ? "Exit" : IsGathering ? "Gather" : IsAttacking ? "Attack"
        : IsSearching ? "Search" : "Inactivo";
    public string ActionDescription => IsDead ? "Reapareciendo en " + health.RespawnRemaining.ToString("F1") + " s"
        : IsFleeing ? fleeState.Status : IsExiting ? exitState.Status
        : IsGathering ? gatherState.Status : IsAttacking ? attackState.Status
        : IsSearching ? searchState.Status : "Sin iniciar";

    public void RegisterGather()
    {
        GatherCount++;
    }

    public void SetTarget(BoidHealth target)
    {
        CurrentTarget = target;
    }

    public void CompleteCycle()
    {
        CompletedCycles++;
        ResetCycle();
    }

    private void Awake()
    {
        CacheComponents();
        CreateStateMachine();
    }

    private void Start()
    {
        StartSearch();
    }

    private void Update()
    {
        stateMachine.Update();
    }

    private void OnEnable()
    {
        if (health == null) return;
        health.Died += OnDied;
        health.Respawned += OnRespawned;
    }

    private void OnDisable()
    {
        if (health != null)
        {
            health.Died -= OnDied;
            health.Respawned -= OnRespawned;
        }
        StopNavigation();
    }

    private void CacheComponents()
    {
        movement = GetComponent<AgentMovement>();
        pathFollower = GetComponent<AgentPathFollower>();
        sensor = GetComponent<HunterSensor>();
        combat = GetComponent<HunterCombat>();
        targetMarker = GetComponent<HunterTarget>();
        refuge = GetComponent<HunterRefuge>();
        health = GetComponent<HunterHealth>();
    }

    private void CreateStateMachine()
    {
        stateMachine = new FiniteStateMachine();
        searchState = new HunterSearchState(this, stateMachine);
        attackState = new HunterAttackState(this, stateMachine);
        gatherState = new HunterGatherState(this, stateMachine);
        exitState = new HunterExitState(this, stateMachine);
        fleeState = new HunterFleeState(this, stateMachine);
        deadState = new HunterDeadState(this, stateMachine);
    }

    private void OnDied()
    {
        stateMachine.ChangeState(deadState);
    }

    private void OnRespawned()
    {
        ResetCycle();
        stateMachine.ChangeState(searchState);
    }

    private void StartSearch()
    {
        if (!ValidateConfiguration()) return;

        ResetCycle();
        stateMachine.ChangeState(searchState);
    }

    private bool ValidateConfiguration()
    {
        if (sensor == null || combat == null || refuge == null)
        {
            Debug.LogWarning("Agregá HunterSensor, HunterCombat y HunterRefuge al padre Hunter.", this);
            return false;
        }

        if (huntingGround == null || huntingGroundEntry == null || searchNodes.Count == 0)
        {
            Debug.LogWarning("Asigná el Bounds, la entrada y los nodos de búsqueda del coto.", this);
            return false;
        }

        if (!huntingGround.Contains(huntingGroundEntry.Position))
        {
            Debug.LogWarning("El nodo de entrada debe estar dentro del coto.", this);
            return false;
        }

        if (exitNode == null || initialPoint == null)
        {
            Debug.LogWarning("Asigná Exit Node y el punto fijo Initial Point.", this);
            return false;
        }

        if (initialPoint.IsChildOf(transform))
        {
            Debug.LogWarning("Initial Point debe ser un objeto fijo de la zona inicial, fuera de Hunter.", this);
            return false;
        }

        if (!refuge.HasTwoValidZones())
        {
            Debug.LogWarning("Configurá dos SafeZone distintas, activas y con su Entry Node interior en HunterRefuge.", this);
            return false;
        }

        return true;
    }

    private void ResetCycle()
    {
        StopNavigation();
        CurrentTarget = null;
        GatherCount = 0;
        transform.SetPositionAndRotation(initialPoint.position, initialPoint.rotation);
        targetMarker.ResetTracking();
    }

    private void StopNavigation()
    {
        if (pathFollower != null)
            pathFollower.StopFollowing();
    }
}
