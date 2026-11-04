using UnityEngine;

[RequireComponent(typeof(HunterAgent))]
public class HunterFeedback : MonoBehaviour
{
    [Header("Pantalla")]
    [SerializeField] private bool showHUD = true;
    [SerializeField] private Vector2 hudPosition = new Vector2(10f, 10f);
    [Header("Colores")]
    [SerializeField] private Color searchColor = Color.yellow;
    [SerializeField] private Color attackColor = Color.red;
    [SerializeField] private Color gatherColor = Color.green;
    [SerializeField] private Color exitColor = Color.cyan;
    [SerializeField] private Color fleeColor = new Color(1f, 0.5f, 0f);
    [SerializeField] private Color deadColor = Color.gray;
    [SerializeField, Min(0.05f)] private float damageFlashDuration = 0.25f;
    [Header("Barra de vida")]
    [SerializeField] private bool showHealthBar = true;
    [SerializeField, Min(0f)] private float healthBarHeight = 2.25f;

    [Header("Disparo")]
    [SerializeField] private LineRenderer shotLine;
    [SerializeField] private Vector3 shotOriginOffset = new Vector3(0f, 0.2f, 0f);
    [SerializeField, Min(0.05f)] private float shotDuration = 0.25f;

    private HunterAgent hunter;
    private HunterCombat combat;
    private HunterHealth health;
    private AgentStatusBillboard statusIcon;
    private AgentHealthBar healthBar;
    private AgentColorFeedback colors;
    private HunterHudPanel hud;
    private Vector3 shotEnd;
    private float attackFlashUntil;
    private float damageFlashUntil;
    private float shotUntil;
    private string lastAttack = "Ninguno";
    private string lastHit = "Ninguno";
    private StatusIcon recentAttackIcon = StatusIcon.Shot;

    private void Awake()
    {
        CacheComponents();
        PrepareShotLine();
    }

    private void Start()
    {
        CreateStatusIcon();
        CreateHealthBar();
    }

    private void CreateStatusIcon()
    {
        statusIcon = new AgentStatusBillboard(transform, 3.2f);
    }

    private void CreateHealthBar()
    {
        if (showHealthBar)
            healthBar = new AgentHealthBar(transform, healthBarHeight);
    }

    private void OnEnable()
    {
        SubscribeEvents();
    }

    private void Update()
    {
        UpdateColor();
        UpdateShotLine();
    }

    private void LateUpdate()
    {
        UpdateStatusIcon();
        UpdateHealthBar();
    }

    private void OnDisable()
    {
        UnsubscribeEvents();
        HideShotLine();
    }

    private void OnGUI()
    {
        if (showHUD && hud != null)
            hud.Draw(hudPosition, lastAttack, lastHit);
    }

    private void OnDrawGizmosSelected()
    {
        DrawRanges();
    }

    private void OnDrawGizmos()
    {
        DrawShot();
    }

    private void CacheComponents()
    {
        hunter = GetComponent<HunterAgent>();
        combat = GetComponent<HunterCombat>();
        health = GetComponent<HunterHealth>();
        colors = new AgentColorFeedback(transform, shotLine);
        hud = new HunterHudPanel(hunter, combat, health);
    }

    private void PrepareShotLine()
    {
        if (shotLine == null) return;
        shotLine.useWorldSpace = true;
        shotLine.positionCount = 2;
        shotLine.enabled = false;
    }

    private void SubscribeEvents()
    {
        if (combat != null)
            combat.AttackPerformed += ShowAttack;
        if (health != null)
            health.Damaged += ShowDamage;
    }

    private void UnsubscribeEvents()
    {
        if (combat != null)
            combat.AttackPerformed -= ShowAttack;
        if (health != null)
            health.Damaged -= ShowDamage;
    }

    private void ShowDamage()
    {
        damageFlashUntil = Time.time + damageFlashDuration;
    }

    private void ShowAttack(Vector3 hitPosition, bool isMelee)
    {
        lastAttack = isMelee ? "Cuerpo a cuerpo" : "A distancia";
        recentAttackIcon = isMelee ? StatusIcon.Bite : StatusIcon.Shot;
        if (hunter.CurrentTarget != null)
            lastHit = hunter.CurrentTarget.name + ": "
                + hunter.CurrentTarget.CurrentHealth.ToString("F0") + " / "
                + hunter.CurrentTarget.MaxHealth.ToString("F0");
        attackFlashUntil = Time.time + shotDuration;
        shotEnd = hitPosition;
        shotUntil = isMelee ? 0f : Time.time + shotDuration;
        if (!isMelee && shotLine != null)
        {
            shotLine.SetPosition(0, transform.position + shotOriginOffset);
            shotLine.SetPosition(1, shotEnd);
            shotLine.enabled = true;
        }
    }

    private void UpdateStatusIcon()
    {
        if (statusIcon == null) return;

        StatusIcon icon = hunter.IsDead ? StatusIcon.HunterDead
            : Time.time < attackFlashUntil ? recentAttackIcon
            : hunter.IsCollecting ? StatusIcon.Gather
            : hunter.IsFleeing ? StatusIcon.HunterFlee
            : combat != null && combat.AttackRemaining > 0f ? GetReloadIcon()
            : hunter.IsExiting ? StatusIcon.Exit
            : StatusIcon.None;

        statusIcon.Show(icon);
        statusIcon.FaceCamera();
    }

    private StatusIcon GetReloadIcon()
    {
        float progress = 1f - combat.AttackRemaining / combat.AttackInterval;
        int frame = Mathf.Clamp(Mathf.FloorToInt(progress * 8f), 0, 7);
        return (StatusIcon)((int)StatusIcon.ReloadFirst + frame);
    }

    private void UpdateHealthBar()
    {
        if (healthBar == null || health == null) return;

        healthBar.Show(health.CurrentHealth, health.MaxHealth);
        healthBar.FaceCamera();
    }

    private void UpdateColor()
    {
        colors.Apply(GetStateColor());
    }

    private Color GetStateColor()
    {
        if (hunter.IsDead) return deadColor;
        if (Time.time < damageFlashUntil) return Color.white;
        if (hunter.IsFleeing) return fleeColor;
        if (hunter.IsExiting) return exitColor;
        if (hunter.IsGathering) return gatherColor;
        if (hunter.IsAttacking || Time.time < attackFlashUntil) return attackColor;
        return searchColor;
    }

    private void UpdateShotLine()
    {
        if (shotLine != null && Time.time >= shotUntil)
            HideShotLine();
    }

    private void HideShotLine()
    {
        if (shotLine != null)
            shotLine.enabled = false;
    }

    private void DrawRanges()
    {
        HunterSensor sensor = GetComponent<HunterSensor>();
        HunterCombat attacks = GetComponent<HunterCombat>();
        if (sensor == null || attacks == null) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sensor.VisionRadius);
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, sensor.WolfVisionRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attacks.MeleeAttackRadius);
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, attacks.RangeAttackRadius);
    }

    private void DrawShot()
    {
        if (!Application.isPlaying || Time.time >= shotUntil) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position + shotOriginOffset, shotEnd);
    }
}
