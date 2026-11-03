using System;
using System.Collections;
using UnityEngine;

public class HunterHealth : MonoBehaviour
{
    [SerializeField, Min(1f)] private float maxHealth = 100f;
    [SerializeField, Min(0.1f)] private float respawnTime = 5f;

    public float RespawnTime => respawnTime;

    private Renderer[] renderers;
    private Collider[] colliders;
    private float respawnAt;

    public float CurrentHealth { get; private set; }
    public float MaxHealth => maxHealth;
    public bool IsDead { get; private set; }
    public float RespawnRemaining => IsDead ? Mathf.Max(0f, respawnAt - Time.time) : 0f;

    public event Action Damaged;
    public event Action Died;
    public event Action Respawned;

    private void Awake()
    {
        CacheBody();
        InitializeHealth();
    }

    private void InitializeHealth()
    {
        CurrentHealth = maxHealth;
    }

    private void CacheBody()
    {
        renderers = GetComponentsInChildren<Renderer>(true);
        colliders = GetComponentsInChildren<Collider>(true);
    }

    public void TakeDamage(float damage)
    {
        if (IsDead || damage <= 0f) return;

        CurrentHealth = Mathf.Max(0f, CurrentHealth - damage);
        Damaged?.Invoke();

        if (CurrentHealth <= 0f)
            Die();
    }

    private void Die()
    {
        IsDead = true;
        respawnAt = Time.time + respawnTime;
        Died?.Invoke();
        SetBodyVisible(false);
        StartCoroutine(RespawnRoutine());
    }

    private IEnumerator RespawnRoutine()
    {
        yield return new WaitForSeconds(respawnTime);

        CurrentHealth = maxHealth;
        IsDead = false;
        Respawned?.Invoke();
        SetBodyVisible(true);
    }

    private void SetBodyVisible(bool visible)
    {
        foreach (Renderer body in renderers)
            if (body != null) body.enabled = visible;

        foreach (Collider body in colliders)
            if (body != null) body.enabled = visible;
    }
}
