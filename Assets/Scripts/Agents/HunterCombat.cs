using System;
using UnityEngine;

[RequireComponent(typeof(HunterSensor), typeof(AgentMovement))]
public class HunterCombat : MonoBehaviour
{
    [Header("Ataques")]
    [SerializeField, Min(0.1f)] private float tba = 2f;
    [SerializeField, Min(0.1f)] private float meleeAttackRadius = 1.5f;
    [SerializeField, Min(0.1f)] private float rangeAttackRadius = 6f;
    [SerializeField, Min(0.1f)] private float meleeDamage = 50f;
    [SerializeField, Min(0.1f)] private float rangedDamage = 25f;
    [Header("Predicción")]
    [SerializeField, Min(0f)] private float maxPredictionTime = 1.5f;

    private HunterSensor sensor;
    private AgentMovement movement;
    private float attackRemaining;
    public bool IsReady => attackRemaining <= 0f;
    public float AttackRemaining => attackRemaining;
    public float AttackInterval => tba;
    public float MeleeAttackRadius => meleeAttackRadius;
    public float RangeAttackRadius => rangeAttackRadius;
    public event Action<Vector3, bool> AttackPerformed;

    private void Awake()
    {
        CacheComponents();
    }

    private void Update()
    {
        UpdateCooldown();
    }

    public bool TryAttack(BoidHealth target)
    {
        if (!IsReady || !sensor.CanSeeLivingBoid(target)) return false;
        Vector3 direction = target.transform.position - transform.position;
        direction.y = 0f;
        if (direction.sqrMagnitude <= meleeAttackRadius * meleeAttackRadius)
        {
            ApplyAttack(target, meleeDamage, true);
            return true;
        }
        if (direction.sqrMagnitude <= rangeAttackRadius * rangeAttackRadius)
        {
            ApplyAttack(target, rangedDamage, false);
            return true;
        }
        return false;
    }

    public Vector3 PredictPosition(Vector3 targetPosition, Vector3 targetVelocity)
    {
        float distance = Vector3.Distance(transform.position, targetPosition);
        float combinedSpeed = Mathf.Max(0.01f, movement.Velocity.magnitude + targetVelocity.magnitude);
        float predictionTime = Mathf.Min(maxPredictionTime, distance / combinedSpeed);
        Vector3 predictedPosition = targetPosition + targetVelocity * predictionTime;
        predictedPosition.y = transform.position.y;
        return predictedPosition;
    }

    private void CacheComponents()
    {
        sensor = GetComponent<HunterSensor>();
        movement = GetComponent<AgentMovement>();
    }

    private void UpdateCooldown()
    {
        attackRemaining = Mathf.Max(0f, attackRemaining - Time.deltaTime);
    }

    private void ApplyAttack(BoidHealth target, float damage, bool isMelee)
    {
        Vector3 hitPosition = target.transform.position;
        target.TakeDamage(damage);
        attackRemaining = tba;
        AttackPerformed?.Invoke(hitPosition, isMelee);
    }
}
