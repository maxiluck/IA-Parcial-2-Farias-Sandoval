using UnityEngine;

public class HunterTarget : MonoBehaviour
{
    private Vector3 previousPosition;
    private HunterRefuge refuge;
    private HunterHealth health;

    public Vector3 Velocity { get; private set; }
    public bool IsProtected => health != null && health.IsDead
        || refuge != null && refuge.IsInsideSafeZone;
    public HunterHealth Health => health;

    private void Awake()
    {
        CacheComponents();
    }

    private void CacheComponents()
    {
        refuge = GetComponent<HunterRefuge>();
        health = GetComponent<HunterHealth>();
    }

    private void OnEnable()
    {
        ResetTracking();
    }

    private void LateUpdate()
    {
        UpdateVelocity();
    }

    public void ResetTracking()
    {
        previousPosition = transform.position;
        Velocity = Vector3.zero;
    }

    private void UpdateVelocity()
    {
        if (Time.deltaTime <= 0f) return;

        Velocity = (transform.position - previousPosition) / Time.deltaTime;
        previousPosition = transform.position;
    }
}
