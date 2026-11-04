using System;
using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(AgentPerception))]
public class WolfCommunication : MonoBehaviour
{
    [Header("Alerta")]
    [SerializeField] private WolfAlertChannel channel;
    [FormerlySerializedAs("howlInterval")]
    [SerializeField, Min(0.1f)] private float alertUpdateInterval = 2f;

    private AgentPerception perception;
    private float nextAlertTime;

    public bool HasKnownPosition { get; private set; }
    public Vector3 KnownPosition { get; private set; }
    public int PositionVersion { get; private set; }

    public event Action Howled;
    public event Action AlertReceived;

    private void Awake()
    {
        CacheComponents();
    }

    private void OnEnable()
    {
        SubscribeToChannel();
    }

    private void OnDisable()
    {
        UnsubscribeFromChannel();
    }

    private void CacheComponents()
    {
        perception = GetComponent<AgentPerception>();
    }

    private void SubscribeToChannel()
    {
        if (channel == null)
        {
            Debug.LogWarning("Falta asignar el canal de alertas del lobo.", this);
            return;
        }

        channel.AlertRaised += ReceiveAlert;
    }

    private void UnsubscribeFromChannel()
    {
        if (channel != null)
            channel.AlertRaised -= ReceiveAlert;
    }

    public void ReportDetection()
    {
        if (!perception.CanSeeTarget) return;

        RememberPosition(perception.DetectedPosition);
        ShareKnownPosition();
    }

    public void ForgetKnownPosition()
    {
        if (HasKnownPosition)
        {
            HasKnownPosition = false;
            KnownPosition = Vector3.zero;
            PositionVersion++;
        }
        nextAlertTime = 0f;
        if (channel != null) channel.ResetEncounter();
    }

    private void ShareKnownPosition()
    {
        if (channel == null) return;

        bool updateReceivers = Time.time >= nextAlertTime;
        bool newEncounter = channel.ReportDetection(this, KnownPosition, updateReceivers);

        if (updateReceivers || newEncounter)
            nextAlertTime = Time.time + alertUpdateInterval;

        if (newEncounter)
            Howled?.Invoke();
    }

    private void ReceiveAlert(WolfCommunication sender, Vector3 position)
    {
        if (perception.TargetIsProtected)
        {
            ForgetKnownPosition();
            return;
        }
        if (sender == this) return;

        Vector3 observedPosition = perception.CanSeeTarget
            ? perception.DetectedPosition
            : position;

        RememberPosition(observedPosition);
        AlertReceived?.Invoke();
    }

    private void RememberPosition(Vector3 position)
    {
        if (HasKnownPosition && KnownPosition == position) return;

        KnownPosition = position;
        HasKnownPosition = true;
        PositionVersion++;
    }
}
