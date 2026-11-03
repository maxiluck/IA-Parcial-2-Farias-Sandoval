using UnityEngine;

[RequireComponent(typeof(AdvanceAgent), typeof(BoidHealth))]
public class BoidFeedback : MonoBehaviour
{
    [Header("Colores")]
    [SerializeField] private Color flockingColor = Color.blue;
    [SerializeField] private Color evadeColor = new Color(1f, 0.5f, 0f);
    [SerializeField] private Color interestColor = Color.green;
    [SerializeField] private Color deadColor = Color.gray;
    [SerializeField, Min(0.01f)] private float damageFlashTime = 0.15f;

    [Header("Barra de vida")]
    [SerializeField] private Camera feedbackCamera;
    [SerializeField] private bool showHealthBar = true;
    [SerializeField] private float healthBarHeight = 1.2f;

    private AdvanceAgent agent;
    private BoidHealth health;
    private BoidSensor sensor;
    private BoidInteraction interaction;
    private AgentStatusBillboard statusIcon;
    private AgentHealthBar healthBar;
    private AgentColorFeedback colors;
    private float flashUntil;

    private void Awake()
    {
        CacheComponents();
    }

    private void Start()
    {
        CreateStatusIcon();
        CreateHealthBar();
    }

    private void CreateStatusIcon()
    {
        statusIcon = new AgentStatusBillboard(transform, 1.9f);
    }

    private void CreateHealthBar()
    {
        if (showHealthBar)
            healthBar = new AgentHealthBar(transform, healthBarHeight, feedbackCamera);
    }

    private void OnEnable()
    {
        SubscribeToDamage();
    }

    private void OnDisable()
    {
        UnsubscribeFromDamage();
    }

    private void LateUpdate()
    {
        UpdateColor();
        UpdateStatusIcon();
        UpdateHealthBar();
    }

    private void OnDrawGizmosSelected()
    {
        if (agent == null) agent = GetComponent<AdvanceAgent>();
        if (agent == null) return;

        DrawRadii();
    }

    private void CacheComponents()
    {
        agent = GetComponent<AdvanceAgent>();
        health = GetComponent<BoidHealth>();
        sensor = GetComponent<BoidSensor>();
        interaction = GetComponent<BoidInteraction>();
        colors = new AgentColorFeedback(transform);
    }

    private void SubscribeToDamage()
    {
        health.Damaged += ShowDamage;
    }

    private void UnsubscribeFromDamage()
    {
        health.Damaged -= ShowDamage;
    }

    private void ShowDamage()
    {
        flashUntil = Time.time + damageFlashTime;
    }

    private void UpdateColor()
    {
        colors.Apply(GetStateColor());
    }

    private Color GetStateColor()
    {
        if (health.IsDead) return deadColor;
        if (Time.time < flashUntil) return Color.white;
        if (agent.currentSteering == AdvanceAgent.SteeringModes.Evade) return evadeColor;
        if (agent.currentSteering == AdvanceAgent.SteeringModes.Arrive) return interestColor;

        return flockingColor;
    }

    private void UpdateStatusIcon()
    {
        if (statusIcon == null) return;

        StatusIcon icon = health.IsCollected ? StatusIcon.None
            : health.IsDead ? StatusIcon.BoidDead
            : sensor != null && sensor.Hunter != null ? StatusIcon.BoidFlee
            : interaction != null && interaction.IsEating ? StatusIcon.Eating
            : sensor != null && sensor.Interest != null ? StatusIcon.BaitSeen
            : StatusIcon.None;

        statusIcon.Show(icon);
        statusIcon.FaceCamera();
    }

    private void UpdateHealthBar()
    {
        if (healthBar == null) return;

        healthBar.Show(health.CurrentHealth, health.MaxHealth, !health.IsCollected);
        healthBar.FaceCamera();
    }

    private void DrawRadii()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, agent.SeparationRadius);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, agent.AlignmentRadius);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, agent.CohesionRadius);
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, agent.VisionRadius);
    }
}
