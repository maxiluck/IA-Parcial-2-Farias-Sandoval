using UnityEngine;

public class AgentPerception : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Transform target;
    [SerializeField] private LayerMask obstacleMask;

    [Header("Visión")]
    [SerializeField] private float viewRadius = 8f;

    [SerializeField, Range(0f, 360f)]
    private float viewAngle = 90f;

    [SerializeField] private float eyeHeight = 0.5f;

    [Header("Frecuencia")]
    [SerializeField] private float checkInterval = 0.2f;

    [Header("Resultado")]
    [SerializeField] private bool canSeeTarget;

    public float ViewRadius => viewRadius;
    public float ViewAngle => viewAngle;
    public HunterHealth TargetHealth => targetMarker != null ? targetMarker.Health : null;

    public Vector3 DetectedPosition { get; private set; }

    public Vector3 EyePosition =>
        transform.position + Vector3.up * eyeHeight;
    private float checkTimer;
    private HunterTarget targetMarker;

    public bool CanSeeTarget => canSeeTarget && !TargetIsProtected;
    public bool TargetIsProtected => targetMarker != null && targetMarker.IsProtected;

    private void Awake()
    {
        CacheTarget();
    }

    private void CacheTarget()
    {
        targetMarker = target != null ? target.GetComponentInParent<HunterTarget>() : null;
    }

    private void Update()
    {
        UpdatePerception();
    }

    private void UpdatePerception()
    {
        checkTimer -= Time.deltaTime;

        if (checkTimer > 0f) return;

        checkTimer = checkInterval;
        canSeeTarget = CheckTarget();

        if (canSeeTarget)
            DetectedPosition = target.position;
    }

    private bool CheckTarget()
    {
        if (target == null || !target.gameObject.activeInHierarchy)
            return false;

        if (TargetIsProtected) return false;

        Vector3 direction = target.position - transform.position;

        if (!IsWithinDistance(direction))
            return false;

        if (!IsWithinAngle(direction))
            return false;

        return HasLineOfSight();
    }

    private bool IsWithinDistance(Vector3 direction)
    {
        return direction.sqrMagnitude <= viewRadius * viewRadius;
    }

    private bool IsWithinAngle(Vector3 direction)
    {
        direction.y = 0f;

        float angle = Vector3.Angle(transform.forward, direction);

        return angle <= viewAngle * 0.5f;
    }

    private bool HasLineOfSight()
    {
        Vector3 origin = EyePosition;
        Vector3 targetPoint = target.position + Vector3.up * eyeHeight;

        return !Physics.Linecast(
            origin,
            targetPoint,
            obstacleMask,
            QueryTriggerInteraction.Ignore);
    }

    private void OnDisable()
    {
        ResetPerception();
    }

    private void ResetPerception()
    {
        canSeeTarget = false;
        checkTimer = 0f;
    }
}
