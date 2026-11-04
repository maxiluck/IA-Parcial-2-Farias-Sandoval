using System;
using UnityEngine;

[RequireComponent(typeof(AgentPerception))]
public class WolfCombat : MonoBehaviour
{
    [Header("Captura")]
    [SerializeField, Min(0.1f)] private float captureRadius = 1.5f;

    private AgentPerception perception;

    public float CaptureRadius => captureRadius;
    public event Action CapturedTarget;

    private void Awake()
    {
        CachePerception();
    }

    private void CachePerception()
    {
        perception = GetComponent<AgentPerception>();
    }

    public bool TryCapture()
    {
        if (perception == null || !perception.CanSeeTarget) return false;

        HunterHealth target = perception.TargetHealth;
        if (target == null || target.IsDead || !IsWithinCaptureRadius(target.transform.position))
            return false;

        Capture(target);
        return true;
    }

    private bool IsWithinCaptureRadius(Vector3 targetPosition)
    {
        Vector3 offset = targetPosition - transform.position;
        offset.y = 0f;
        return offset.sqrMagnitude <= captureRadius * captureRadius;
    }

    private void Capture(HunterHealth target)
    {
        target.TakeDamage(target.CurrentHealth);
        CapturedTarget?.Invoke();
    }
}
