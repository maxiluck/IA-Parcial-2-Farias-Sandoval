using UnityEngine;

[RequireComponent(typeof(BoidSensor), typeof(BoidHealth))]
public class BoidInteraction : MonoBehaviour
{
    [SerializeField, Min(0f)] private float damageToPOI = 10f;
    [SerializeField, Min(0.1f)] private float damageInterval = 1f;
    [SerializeField, Min(0.1f)] private float interactionRadius = 1.5f;

    private BoidSensor sensor;
    private BoidHealth health;
    private PointOfInterest currentInterest;
    private float damageTimer;

    public bool IsEating => currentInterest != null && health != null && !health.IsDead;

    private void Awake()
    {
        CacheComponents();
    }

    private void Update()
    {
        UpdateInteraction();
    }

    private void CacheComponents()
    {
        sensor = GetComponent<BoidSensor>();
        health = GetComponent<BoidHealth>();
    }

    private void UpdateInteraction()
    {
        if (!CanInteract())
        {
            ResetInteraction();
            return;
        }

        SelectInterest();
        UpdateDamage();
    }

    private bool CanInteract()
    {
        if (health.IsDead || sensor.Hunter != null || sensor.Interest == null)
            return false;

        if (sensor.Interest.IsDepleted) return false;

        Vector3 direction = sensor.Interest.transform.position - transform.position;
        direction.y = 0f;

        return direction.sqrMagnitude <= interactionRadius * interactionRadius;
    }

    private void SelectInterest()
    {
        if (currentInterest == sensor.Interest) return;

        currentInterest = sensor.Interest;
        damageTimer = 0f;
    }

    private void UpdateDamage()
    {
        damageTimer += Time.deltaTime;
        if (damageTimer < damageInterval) return;

        currentInterest.TakeDamage(damageToPOI);
        damageTimer = 0f;
    }

    private void ResetInteraction()
    {
        currentInterest = null;
        damageTimer = 0f;
    }
}
