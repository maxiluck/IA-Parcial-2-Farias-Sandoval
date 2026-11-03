using System.Collections.Generic;
using UnityEngine;

public class BoidSensor : MonoBehaviour
{
    [Header("Deteccion local")]
    [SerializeField] private LayerMask detectableLayers;
    [SerializeField] private LayerMask obstacleLayers;
    [SerializeField, Min(0.02f)] private float checkInterval = 0.1f;
    [SerializeField, Min(8)] private int initialCapacity = 32;
    [SerializeField] private float eyeHeight = 0.3f;

    private AdvanceAgent agent;
    private BoidHealth health;
    private Collider[] colliders;
    private readonly List<AdvanceAgent> neighbors = new List<AdvanceAgent>();
    private float checkTimer;
    private float nearestHunterDistance;
    private float nearestInterestDistance;

    public IReadOnlyList<AdvanceAgent> Neighbors => neighbors;
    public HunterTarget Hunter { get; private set; }
    public PointOfInterest Interest { get; private set; }

    private void Awake()
    {
        CacheComponents();
        PrepareBuffer();
    }

    private void Start()
    {
        ScanEnvironment();
    }

    private void Update()
    {
        UpdateScan();
    }

    private void OnDisable()
    {
        ClearDetection();
    }

    private void CacheComponents()
    {
        agent = GetComponent<AdvanceAgent>();
        health = GetComponent<BoidHealth>();
    }

    private void PrepareBuffer()
    {
        colliders = new Collider[Mathf.Max(8, initialCapacity)];
    }

    private void UpdateScan()
    {
        if (agent == null || health == null || health.IsDead)
        {
            ClearDetection();
            return;
        }

        checkTimer -= Time.deltaTime;
        if (checkTimer > 0f) return;

        ScanEnvironment();
    }

    private void ScanEnvironment()
    {
        ClearDetection();
        checkTimer = checkInterval;

        if (agent == null || health == null || health.IsDead) return;

        int count = CollectColliders();

        for (int i = 0; i < count; i++)
            InspectCollider(colliders[i]);
    }

    public void Refresh()
    {
        ScanEnvironment();
    }

    private int CollectColliders()
    {
        while (true)
        {
            int count = Physics.OverlapSphereNonAlloc(
                transform.position, agent.DetectionRadius, colliders,
                detectableLayers, QueryTriggerInteraction.Collide);

            if (count < colliders.Length) return count;

            System.Array.Resize(ref colliders, colliders.Length * 2);
        }
    }

    private void InspectCollider(Collider body)
    {
        if (body == null || body.transform.IsChildOf(transform)) return;

        AdvanceAgent neighbor = body.GetComponentInParent<AdvanceAgent>();
        if (neighbor != null)
        {
            InspectNeighbor(neighbor);
            return;
        }

        HunterTarget hunter = body.GetComponentInParent<HunterTarget>();
        if (hunter != null)
        {
            InspectHunter(hunter);
            return;
        }

        PointOfInterest interest = body.GetComponentInParent<PointOfInterest>();
        if (interest != null)
            InspectInterest(interest);
    }

    private void InspectNeighbor(AdvanceAgent neighbor)
    {
        if (neighbor == agent || !neighbor.IsAlive || neighbors.Contains(neighbor))
            return;

        float distance = (neighbor.transform.position - transform.position).sqrMagnitude;
        if (distance > agent.FlockingRadius * agent.FlockingRadius) return;
        if (!HasLineOfSight(neighbor.transform.position)) return;

        neighbors.Add(neighbor);
    }

    private void InspectHunter(HunterTarget hunter)
    {
        if (!hunter.isActiveAndEnabled) return;

        float distance = (hunter.transform.position - transform.position).sqrMagnitude;
        if (distance > agent.VisionRadius * agent.VisionRadius) return;
        if (distance >= nearestHunterDistance) return;
        if (!HasLineOfSight(hunter.transform.position)) return;

        Hunter = hunter;
        nearestHunterDistance = distance;
    }

    private void InspectInterest(PointOfInterest interest)
    {
        if (!interest.isActiveAndEnabled || interest.IsDepleted) return;

        float distance = (interest.transform.position - transform.position).sqrMagnitude;
        if (distance > agent.InterestVisionRadius * agent.InterestVisionRadius) return;
        if (distance >= nearestInterestDistance) return;
        if (!HasLineOfSight(interest.transform.position)) return;

        Interest = interest;
        nearestInterestDistance = distance;
    }

    private bool HasLineOfSight(Vector3 targetPosition)
    {
        Vector3 offset = Vector3.up * eyeHeight;

        return !Physics.Linecast(
            transform.position + offset, targetPosition + offset,
            obstacleLayers, QueryTriggerInteraction.Ignore);
    }

    private void ClearDetection()
    {
        neighbors.Clear();
        Hunter = null;
        Interest = null;
        nearestHunterDistance = float.PositiveInfinity;
        nearestInterestDistance = float.PositiveInfinity;
    }
}
