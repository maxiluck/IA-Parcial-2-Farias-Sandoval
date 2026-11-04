using System;
using UnityEngine;

public class HunterSensor : MonoBehaviour
{
    [Header("Percepción de boids")]
    [SerializeField] private LayerMask boidLayers;
    [SerializeField] private LayerMask obstacleMask;
    [SerializeField, Min(0.1f)] private float visionRadius = 10f;
    [SerializeField, Min(0f)] private float eyeHeight = 0.3f;

    [Header("Percepción de lobos")]
    [SerializeField] private LayerMask wolfLayers;
    [SerializeField, Min(0.1f)] private float wolfVisionRadius = 8f;
    [Header("Consultas")]
    [SerializeField, Min(0.05f)] private float checkInterval = 0.15f;
    [SerializeField, Min(8)] private int initialCapacity = 32;
    [SerializeField, Min(0.01f)] private float clearanceRadius = 0.55f;

    private Collider[] detectedColliders;
    private float checkTimer;
    public float VisionRadius => visionRadius;
    public BoidHealth NearestLivingBoid { get; private set; }
    public BoidHealth NearestCorpse { get; private set; }
    public WolfAgent NearestWolf { get; private set; }
    public float WolfVisionRadius => wolfVisionRadius;

    private void Awake()
    {
        PrepareBuffer();
    }

    private void Start()
    {
        ScanEnvironment();
    }

    private void Update()
    {
        UpdatePerception();
    }

    private void OnDisable()
    {
        ClearDetection();
    }

    public bool CanSeeLivingBoid(BoidHealth target)
    {
        return IsAvailable(target) && !target.IsDead && IsVisible(target);
    }

    public bool CanSeeCorpse(BoidHealth target)
    {
        return IsAvailable(target) && target.IsDead && IsVisible(target);
    }

    public bool CanSeeWolf(WolfAgent wolf)
    {
        if (wolf == null || !wolf.isActiveAndEnabled) return false;
        Vector3 direction = wolf.transform.position - transform.position;
        return direction.sqrMagnitude <= wolfVisionRadius * wolfVisionRadius
            && HasLineOfSight(wolf.transform.position);
    }

    private bool IsAvailable(BoidHealth target)
    {
        return target != null && !target.IsCollected && target.gameObject.activeInHierarchy;
    }

    private bool IsVisible(BoidHealth target)
    {
        Vector3 direction = target.transform.position - transform.position;
        if (direction.sqrMagnitude > visionRadius * visionRadius) return false;
        return HasLineOfSight(target.transform.position);
    }

    public bool HasClearPath(Vector3 destination)
    {
        Vector3 origin = transform.position;
        destination.y = origin.y;
        if (Physics.CheckSphere(origin, clearanceRadius, obstacleMask, QueryTriggerInteraction.Ignore))
            return false;
        if (Physics.CheckSphere(destination, clearanceRadius, obstacleMask, QueryTriggerInteraction.Ignore))
            return false;
        Vector3 direction = destination - origin;
        float distance = direction.magnitude;
        if (distance <= 0.001f) return true;
        return !Physics.SphereCast(origin, clearanceRadius, direction / distance,
            out _, distance, obstacleMask, QueryTriggerInteraction.Ignore);
    }

    private void PrepareBuffer()
    {
        detectedColliders = new Collider[Mathf.Max(8, initialCapacity)];
    }

    private void UpdatePerception()
    {
        checkTimer -= Time.deltaTime;
        if (checkTimer > 0f) return;
        checkTimer = checkInterval;
        ScanEnvironment();
    }

    private void ScanEnvironment()
    {
        NearestLivingBoid = null;
        NearestCorpse = null;
        NearestWolf = null;
        int count = CollectColliders();
        float nearestLivingDistance = float.PositiveInfinity;
        float nearestCorpseDistance = float.PositiveInfinity;
        float nearestWolfDistance = float.PositiveInfinity;
        for (int i = 0; i < count; i++)
        {
            WolfAgent wolf = detectedColliders[i].GetComponentInParent<WolfAgent>();
            if (wolf != null)
            {
                float wolfDistance = (wolf.transform.position - transform.position).sqrMagnitude;
                if (wolfDistance < nearestWolfDistance && CanSeeWolf(wolf))
                {
                    nearestWolfDistance = wolfDistance;
                    NearestWolf = wolf;
                }
                continue;
            }
            BoidHealth boid = detectedColliders[i].GetComponentInParent<BoidHealth>();
            if (!IsAvailable(boid)) continue;
            float distance = (boid.transform.position - transform.position).sqrMagnitude;
            if (boid.IsDead)
            {
                if (distance >= nearestCorpseDistance || !CanSeeCorpse(boid)) continue;
                nearestCorpseDistance = distance;
                NearestCorpse = boid;
            }
            else
            {
                if (distance >= nearestLivingDistance || !CanSeeLivingBoid(boid)) continue;
                nearestLivingDistance = distance;
                NearestLivingBoid = boid;
            }
        }
    }

    private int CollectColliders()
    {
        int count;
        while (true)
        {
            float radius = Mathf.Max(visionRadius, wolfVisionRadius);
            int layers = boidLayers.value | wolfLayers.value;
            count = Physics.OverlapSphereNonAlloc(transform.position, radius,
                detectedColliders, layers, QueryTriggerInteraction.Collide);
            if (count < detectedColliders.Length) return count;
            Array.Resize(ref detectedColliders, detectedColliders.Length * 2);
        }
    }

    private bool HasLineOfSight(Vector3 targetPosition)
    {
        Vector3 offset = Vector3.up * eyeHeight;
        return !Physics.Linecast(transform.position + offset, targetPosition + offset,
            obstacleMask, QueryTriggerInteraction.Ignore);
    }

    private void ClearDetection()
    {
        NearestLivingBoid = null;
        NearestCorpse = null;
        NearestWolf = null;
        checkTimer = 0f;
    }
}
