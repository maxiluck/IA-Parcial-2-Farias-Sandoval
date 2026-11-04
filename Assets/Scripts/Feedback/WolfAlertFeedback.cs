using UnityEngine;

[RequireComponent(typeof(WolfCommunication))]
public class WolfAlertFeedback : MonoBehaviour
{
    [Header("Audio del aullido")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip howlClip;

    [Header("Indicadores")]
    [SerializeField, Min(0.1f)] private float feedbackDuration = 1f;
    [SerializeField, Min(0.1f)] private float alertRadius = 1.2f;

    private WolfCommunication communication;
    private WolfCombat combat;
    private AgentPerception perception;
    private AgentPathFollower pathFollower;
    private AgentStatusBillboard statusIcon;
    private float howlUntil;
    private float receivedUntil;
    private float captureUntil;

    private void Awake()
    {
        CacheComponents();
    }

    private void Start()
    {
        CreateStatusIcon();
    }

    private void CreateStatusIcon()
    {
        statusIcon = new AgentStatusBillboard(transform, 2.1f);
    }

    private void OnEnable()
    {
        SubscribeToEvents();
    }

    private void LateUpdate()
    {
        UpdateStatusIcon();
    }

    private void OnDisable()
    {
        UnsubscribeFromEvents();
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying || communication == null) return;

        DrawKnownPosition();
        DrawAlert();
    }

    private void CacheComponents()
    {
        communication = GetComponent<WolfCommunication>();
        combat = GetComponent<WolfCombat>();
        perception = GetComponent<AgentPerception>();
        pathFollower = GetComponent<AgentPathFollower>();
    }

    private void SubscribeToEvents()
    {
        communication.Howled += ShowHowl;
        communication.AlertReceived += ShowReceivedAlert;
        if (combat != null) combat.CapturedTarget += ShowCapture;
    }

    private void UnsubscribeFromEvents()
    {
        communication.Howled -= ShowHowl;
        communication.AlertReceived -= ShowReceivedAlert;
        if (combat != null) combat.CapturedTarget -= ShowCapture;
    }

    private void ShowCapture()
    {
        captureUntil = Time.time + feedbackDuration;
        Debug.Log($"[{name}] Alcanzó al cazador.", this);
    }

    private void ShowHowl()
    {
        howlUntil = Time.time + feedbackDuration;
        Debug.Log($"[{name}] Aullido. Posicion transmitida: {communication.KnownPosition}", this);

        if (audioSource != null && howlClip != null)
            audioSource.PlayOneShot(howlClip);
    }

    private void ShowReceivedAlert()
    {
        receivedUntil = Time.time + feedbackDuration;
        Debug.Log($"[{name}] Alerta recibida. Posicion conocida: {communication.KnownPosition}", this);
    }

    private void UpdateStatusIcon()
    {
        if (statusIcon == null) return;

        StatusIcon icon = Time.time < howlUntil ? StatusIcon.Howl
            : Time.time < receivedUntil ? StatusIcon.AlertReceived
            : Time.time < captureUntil ? StatusIcon.Bite
            : perception != null && perception.CanSeeTarget ? StatusIcon.WolfSeesHunter
            : pathFollower != null && pathFollower.HasPath ? StatusIcon.Pathfinding
            : communication.HasKnownPosition ? StatusIcon.Search
            : StatusIcon.Patrol;

        statusIcon.Show(icon);
        statusIcon.FaceCamera();
    }

    private void DrawKnownPosition()
    {
        if (!communication.HasKnownPosition) return;

        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(communication.KnownPosition, 0.3f);
        Gizmos.DrawLine(transform.position, communication.KnownPosition);
    }

    private void DrawAlert()
    {
        if (Time.time < howlUntil)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, alertRadius);
        }

        if (Time.time < receivedUntil)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(transform.position, alertRadius);
        }

        if (Time.time < captureUntil && combat != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, combat.CaptureRadius);
        }
    }
}
