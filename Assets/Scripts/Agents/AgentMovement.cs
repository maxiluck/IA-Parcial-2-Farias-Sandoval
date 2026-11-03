using UnityEngine;

public class AgentMovement : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] private float maxSpeed = 4f;
    [SerializeField] private float maxSteering = 8f;

    [Header("Llegada")]
    [SerializeField] private float slowingDistance = 2f;
    [SerializeField] private float stoppingDistance = 0.15f;

    [Header("Restricciones físicas")]
    [SerializeField] private LayerMask movementObstacles;
    [SerializeField, Min(0.01f)] private float bodyRadius = 0.5f;

    private Vector3 velocity;
    private Vector3 destination;
    private bool hasDestination;

    public Vector3 Velocity => velocity;
    public bool HasDestination => hasDestination;
    public float StoppingDistance => stoppingDistance;

    public bool HasClearPath(Vector3 position)
    {
        if (movementObstacles.value == 0) return true;
        position.y = transform.position.y;
        if (Physics.CheckSphere(transform.position, bodyRadius, movementObstacles, QueryTriggerInteraction.Collide))
            return false;
        Vector3 direction = position - transform.position;
        float distance = direction.magnitude;
        if (distance <= 0.001f) return true;
        return !Physics.SphereCast(transform.position, bodyRadius, direction / distance,
            out _, distance, movementObstacles, QueryTriggerInteraction.Collide);
    }

    private void Update()
    {
        UpdateMovement();
    }

    public void MoveTo(Vector3 position)
    {
        destination = position;
        destination.y = transform.position.y;
        hasDestination = true;
    }

    public void Stop()
    {
        hasDestination = false;
        velocity = Vector3.zero;
    }

    private void UpdateMovement()
    {
        if (!hasDestination) return;

        Vector3 direction = destination - transform.position;
        direction.y = 0f;

        float distance = direction.magnitude;

        if (distance <= stoppingDistance)
        {
            Stop();
            return;
        }

        UpdateVelocity(direction, distance);
        ApplyMovement();
        UpdateRotation();
    }

    private void UpdateVelocity(Vector3 direction, float distance)
    {
        float desiredSpeed = maxSpeed;

        if (distance < slowingDistance)
            desiredSpeed *= distance / slowingDistance;

        Vector3 desiredVelocity = direction.normalized * desiredSpeed;
        Vector3 steering = desiredVelocity - velocity;

        velocity += Vector3.ClampMagnitude(
            steering, maxSteering * Time.deltaTime);

        velocity = Vector3.ClampMagnitude(velocity, maxSpeed);
        velocity.y = 0f;
    }

    private void ApplyMovement()
    {
        Vector3 nextPosition = transform.position + velocity * Time.deltaTime;
        if (!HasClearPath(nextPosition))
        {
            Stop();
            return;
        }
        transform.position = nextPosition;
    }

    private void UpdateRotation()
    {
        if (velocity.sqrMagnitude > 0.001f)
            transform.forward = velocity.normalized;
    }

    private void OnDisable()
    {
        Stop();
    }
}
