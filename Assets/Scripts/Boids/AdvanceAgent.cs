using UnityEngine;

[RequireComponent(typeof(BoidHealth), typeof(BoidSensor))]
public class AdvanceAgent : Agent
{
    [Header("Coto")]
    [SerializeField] private Bounds areaBounds;
    [SerializeField, Min(0.05f)] private float bodyRadius = 0.55f;

    [Header("Ajustes de Movimiento")]
    [SerializeField, Min(0.1f)] private float maxSpeed = 3f;
    [SerializeField, Min(0.1f)] private float maxSteering = 10f;
    [SerializeField] private bool blockY = true;

    [Header("Parametros de Evade")]
    [SerializeField, Min(0.1f)] private float visionRadius = 8f;
    [SerializeField, Min(0f)] private float maxPredictionTime = 1.5f;

    [Header("Parametros de Arrive")]
    [SerializeField, Min(0.1f)] private float slowingDistance = 3f;
    [SerializeField, Min(0.01f)] private float minDistance = 0.2f;
    [SerializeField, Min(0.1f)] private float poiVisionRadius = 8f;

    [Header("Parametros de Flocking")]
    [SerializeField, Min(0.1f)] private float _separationRadius = 1.6f;
    [SerializeField, Min(0.1f)] private float _alignmentRadius = 4.5f;
    [SerializeField, Min(0.1f)] private float _cohesionRadius = 4.5f;

    [SerializeField, Range(0f, 3f)] private float separationWhight = 3f;
    [SerializeField, Range(0f, 3f)] private float alignmentWhight = 0.7f;
    [SerializeField, Range(0f, 3f)] private float cohesionWhight = 0.6f;

    private BoidSensor sensor;
    private BoidHealth health;
    private Vector3 _velocity;

    public enum SteeringModes { Seek, Flee, Arrive, Persuit, Evade, Flocking }
    public SteeringModes currentSteering;

    public Bounds AreaBounds => areaBounds;
    public float BodyRadius => bodyRadius;
    public float VisionRadius => visionRadius;
    public float InterestVisionRadius => poiVisionRadius;
    public float SeparationRadius => _separationRadius;
    public float AlignmentRadius => _alignmentRadius;
    public float CohesionRadius => _cohesionRadius;
    public float FlockingRadius => Mathf.Max(
        _separationRadius, Mathf.Max(_alignmentRadius, _cohesionRadius));
    public float DetectionRadius => Mathf.Max(
        FlockingRadius, Mathf.Max(visionRadius, poiVisionRadius));
    public bool IsAlive => isActiveAndEnabled && health != null && !health.IsDead;

    private void Awake()
    {
        CacheComponents();
    }

    private void OnEnable()
    {
        InitializeMovement();
    }

    protected override void Update()
    {
        UpdateMovement();
    }

    private void OnDisable()
    {
        StopMovement();
    }

    private void CacheComponents()
    {
        sensor = GetComponent<BoidSensor>();
        health = GetComponent<BoidHealth>();
    }

    private void InitializeMovement()
    {
        Vector3 direction = new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f));
        if (direction.sqrMagnitude < 0.001f) direction = Vector3.forward;

        _velocity = direction.normalized * maxSpeed;
        Velocity = _velocity;
    }

    public void StopMovement()
    {
        _velocity = Vector3.zero;
        Velocity = Vector3.zero;
    }

    private void UpdateMovement()
    {
        if (health.IsDead) return;

        UpdateVelocity();
        ApplyMovement();
        KeepInsideArea();
        UpdateRotation();
    }

    private void UpdateVelocity()
    {
        _velocity += SteeringVector() * Time.deltaTime;
        _velocity = Vector3.ClampMagnitude(_velocity, maxSpeed);

        if (blockY) _velocity.y = 0f;

        Velocity = _velocity;
    }

    private void ApplyMovement()
    {
        Vector3 nextPosition = transform.position + _velocity * Time.deltaTime;

        if (!CanMoveTo(nextPosition))
        {
            StopMovement();
            return;
        }

        transform.position = nextPosition;
    }

    private bool CanMoveTo(Vector3 position)
    {
        for (int i = 0; i < sensor.Neighbors.Count; i++)
        {
            AdvanceAgent neighbor = sensor.Neighbors[i];
            if (neighbor == null || !neighbor.IsAlive) continue;

            float minimumDistance = bodyRadius + neighbor.BodyRadius;
            Vector3 nextOffset = position - neighbor.transform.position;
            Vector3 currentOffset = transform.position - neighbor.transform.position;
            nextOffset.y = 0f;
            currentOffset.y = 0f;

            if (nextOffset.sqrMagnitude < minimumDistance * minimumDistance
                && nextOffset.sqrMagnitude < currentOffset.sqrMagnitude)
                return false;
        }

        return true;
    }

    private void KeepInsideArea()
    {
        if (areaBounds == null) return;

        Vector3 position = transform.position;
        Vector3 boundedPosition = areaBounds.CalculateBoundPosition(position, bodyRadius);
        if (position == boundedPosition) return;

        if (position.x != boundedPosition.x) _velocity.x = 0f;
        if (position.z != boundedPosition.z) _velocity.z = 0f;

        transform.position = boundedPosition;
        Velocity = _velocity;
    }

    private void UpdateRotation()
    {
        if (_velocity.sqrMagnitude > 0.001f)
            transform.forward = _velocity.normalized;
    }

    private Vector3 SteeringVector()
    {
        Vector3 steering = SelectBehavior();
        steering += CalculateSeparation() * separationWhight;
        steering = ApplyBoundarySteering(steering);

        return Vector3.ClampMagnitude(steering, maxSteering);
    }

    private Vector3 ApplyBoundarySteering(Vector3 steering)
    {
        if (areaBounds == null || currentSteering == SteeringModes.Arrive)
            return steering;

        Vector3 inward = areaBounds.GetInwardDirection(
            transform.position, bodyRadius, maxSpeed);

        float urgency = Mathf.Clamp01(inward.magnitude);
        if (urgency <= 0f) return steering;

        Vector3 desiredVelocity = inward.normalized * maxSpeed;
        Vector3 boundarySteering = CalculateSteeringForce(desiredVelocity);
        return Vector3.Lerp(steering, boundarySteering, urgency);
    }

    private Vector3 SelectBehavior()
    {
        if (sensor.Hunter != null && sensor.Hunter.isActiveAndEnabled)
        {
            currentSteering = SteeringModes.Evade;
            return Evade(sensor.Hunter);
        }

        if (sensor.Interest != null && !sensor.Interest.IsDepleted)
        {
            currentSteering = SteeringModes.Arrive;
            return Arrive(GetReachableInterestPosition());
        }

        currentSteering = SteeringModes.Flocking;
        return Flocking();
    }

    private Vector3 GetReachableInterestPosition()
    {
        Vector3 position = sensor.Interest.transform.position;
        return areaBounds != null
            ? areaBounds.CalculateBoundPosition(position, bodyRadius)
            : position;
    }

    private Vector3 Flocking()
    {
        return CalculateAlignment() * alignmentWhight
            + CalculateCohesion() * cohesionWhight;
    }

    private Vector3 CalculateSeparation()
    {
        Vector3 desired = Vector3.zero;

        for (int i = 0; i < sensor.Neighbors.Count; i++)
        {
            AdvanceAgent neighbor = sensor.Neighbors[i];
            if (neighbor == null || !neighbor.IsAlive) continue;

            Vector3 away = transform.position - neighbor.transform.position;
            away.y = 0f;
            float distance = away.sqrMagnitude;

            if (distance > _separationRadius * _separationRadius) continue;

            if (distance < 0.001f)
                desired += GetInstanceID() < neighbor.GetInstanceID() ? Vector3.left : Vector3.right;
            else
                desired += away / distance;
        }

        if (desired.sqrMagnitude < 0.001f) return Vector3.zero;

        return CalculateSteeringForce(desired.normalized * maxSpeed);
    }

    private Vector3 CalculateAlignment()
    {
        Vector3 desired = Vector3.zero;
        int count = 0;

        for (int i = 0; i < sensor.Neighbors.Count; i++)
        {
            AdvanceAgent neighbor = sensor.Neighbors[i];
            if (neighbor == null || !neighbor.IsAlive) continue;
            if (!InRange(neighbor.transform.position, _alignmentRadius)) continue;

            desired += neighbor.Velocity;
            count++;
        }

        if (count == 0) return Vector3.zero;

        return CalculateSteeringForce((desired / count).normalized * maxSpeed);
    }

    private Vector3 CalculateCohesion()
    {
        Vector3 center = Vector3.zero;
        int count = 0;

        for (int i = 0; i < sensor.Neighbors.Count; i++)
        {
            AdvanceAgent neighbor = sensor.Neighbors[i];
            if (neighbor == null || !neighbor.IsAlive) continue;
            if (!InRange(neighbor.transform.position, _cohesionRadius)) continue;

            center += neighbor.transform.position;
            count++;
        }

        return count == 0 ? Vector3.zero : Seek(center / count);
    }

    private bool InRange(Vector3 position, float radius)
    {
        Vector3 direction = position - transform.position;
        if (blockY) direction.y = 0f;

        return direction.sqrMagnitude <= radius * radius;
    }

    private Vector3 CalculateSteeringForce(Vector3 desiredVelocity)
    {
        return Vector3.ClampMagnitude(desiredVelocity - _velocity, maxSteering);
    }

    private Vector3 CalculateDesired(Vector3 targetPosition, float speed)
    {
        Vector3 direction = targetPosition - transform.position;
        if (blockY) direction.y = 0f;

        return direction.normalized * speed;
    }

    private Vector3 Seek(Vector3 targetPosition)
    {
        return CalculateSteeringForce(CalculateDesired(targetPosition, maxSpeed));
    }

    private Vector3 Flee(Vector3 targetPosition)
    {
        return CalculateSteeringForce(-CalculateDesired(targetPosition, maxSpeed));
    }

    private Vector3 Arrive(Vector3 targetPosition)
    {
        Vector3 direction = targetPosition - transform.position;
        if (blockY) direction.y = 0f;

        float distance = direction.magnitude;
        if (distance <= minDistance)
            return CalculateSteeringForce(Vector3.zero);

        float speed = maxSpeed * Mathf.Min(1f, distance / slowingDistance);
        return CalculateSteeringForce(CalculateDesired(targetPosition, speed));
    }

    private Vector3 CalculateFuture(HunterTarget target)
    {
        Vector3 direction = target.transform.position - transform.position;
        if (blockY) direction.y = 0f;

        float prediction = Mathf.Min(
            maxPredictionTime, direction.magnitude / (maxSpeed + target.Velocity.magnitude));

        return target.transform.position + target.Velocity * prediction;
    }

    private Vector3 Persuit(HunterTarget target)
    {
        return Seek(CalculateFuture(target));
    }

    private Vector3 Evade(HunterTarget target)
    {
        return Flee(CalculateFuture(target));
    }
}
