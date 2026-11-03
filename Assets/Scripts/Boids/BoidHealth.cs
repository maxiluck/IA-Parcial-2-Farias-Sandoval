using System;
using System.Collections;
using UnityEngine;

public class BoidHealth : MonoBehaviour
{
    [Header("Salud")]
    [SerializeField, Min(1f)] private float maxHealth = 50f;

    [Header("Respawn dentro del coto")]
    [SerializeField, Min(0.1f)] private float respawnTime = 4f;
    [SerializeField] private LayerMask spawnBlockingLayers;
    [SerializeField, Min(1)] private int spawnAttempts = 20;
    [SerializeField, Min(0.05f)] private float spawnRetryTime = 0.25f;

    private AdvanceAgent agent;
    private BoidSensor sensor;
    private Collider[] colliders;
    private Renderer[] renderers;
    private float spawnHeight;

    public float CurrentHealth { get; private set; }
    public float MaxHealth => maxHealth;
    public bool IsDead { get; private set; }
    public bool IsCollected { get; private set; }

    public event Action Damaged;

    private void Awake()
    {
        CacheComponents();
        ResetHealth();
    }

    private void Start()
    {
        SaveSpawnHeight();
    }

    private void CacheComponents()
    {
        agent = GetComponent<AdvanceAgent>();
        sensor = GetComponent<BoidSensor>();
        colliders = GetComponentsInChildren<Collider>(true);
        renderers = GetComponentsInChildren<Renderer>(true);
    }

    private void SaveSpawnHeight()
    {
        if (agent != null && agent.AreaBounds != null)
            spawnHeight = transform.position.y - agent.AreaBounds.transform.position.y;
    }

    private void ResetHealth()
    {
        CurrentHealth = maxHealth;
        IsDead = false;
        IsCollected = false;
    }

    public void TakeDamage(float amount)
    {
        if (IsDead || amount <= 0f) return;

        CurrentHealth = Mathf.Max(0f, CurrentHealth - amount);
        Damaged?.Invoke();

        if (CurrentHealth <= 0f)
            Die();
    }

    private void Die()
    {
        IsDead = true;

        if (agent == null) return;

        agent.StopMovement();
        agent.enabled = false;
    }

    public bool Collect()
    {
        if (!IsDead || IsCollected) return false;

        if (agent == null || agent.AreaBounds == null)
        {
            Debug.LogWarning("El boid necesita su AdvanceAgent y el Bounds del coto.", this);
            return false;
        }

        IsCollected = true;
        StartCoroutine(RespawnRoutine());
        return true;
    }

    private IEnumerator RespawnRoutine()
    {
        SetBodyVisible(false);
        yield return new WaitForSeconds(respawnTime);

        Vector3 position;
        WaitForSeconds retryWait = new WaitForSeconds(spawnRetryTime);

        while (!TryChooseRespawnPosition(out position))
            yield return retryWait;

        RespawnAt(position);
    }

    private bool TryChooseRespawnPosition(out Vector3 position)
    {
        position = transform.position;

        for (int i = 0; i < spawnAttempts; i++)
        {
            position = agent.AreaBounds.GetRandomPosition(spawnHeight, agent.BodyRadius);

            bool occupied = Physics.CheckSphere(
                position, agent.BodyRadius, spawnBlockingLayers,
                QueryTriggerInteraction.Collide);

            if (!occupied) return true;
        }

        return false;
    }

    private void RespawnAt(Vector3 position)
    {
        transform.position = position;
        ResetHealth();
        SetBodyVisible(true);
        agent.enabled = true;

        if (sensor != null)
            sensor.Refresh();
    }

    private void SetBodyVisible(bool visible)
    {
        foreach (Renderer body in renderers)
            if (body != null) body.enabled = visible;

        foreach (Collider body in colliders)
            if (body != null) body.enabled = visible;
    }
}
