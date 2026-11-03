using UnityEngine;

public class PointOfInterest : MonoBehaviour
{
    [SerializeField, Min(1f)] private float maxHealth = 100f;

    public float CurrentHealth { get; private set; }
    public bool IsDepleted => CurrentHealth <= 0f;

    private void Awake()
    {
        InitializeHealth();
    }

    private void InitializeHealth()
    {
        CurrentHealth = maxHealth;
    }

    public void TakeDamage(float amount)
    {
        if (IsDepleted || amount <= 0f) return;

        CurrentHealth = Mathf.Max(0f, CurrentHealth - amount);

        if (IsDepleted)
            Destroy(gameObject);
    }
}
