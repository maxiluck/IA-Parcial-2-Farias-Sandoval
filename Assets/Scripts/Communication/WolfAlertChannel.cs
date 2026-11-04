using System;
using UnityEngine;

public class WolfAlertChannel : MonoBehaviour
{
    [SerializeField, Min(0.1f)] private float alertResetDelay = 2f;

    private bool hasDetection;
    private float lastDetectionTime;

    public event Action<WolfCommunication, Vector3> AlertRaised;

    public bool ReportDetection(
        WolfCommunication sender,
        Vector3 position,
        bool updateReceivers)
    {
        bool newEncounter = IsNewEncounter();
        lastDetectionTime = Time.time;
        hasDetection = true;

        if (newEncounter || updateReceivers)
            AlertRaised?.Invoke(sender, position);

        return newEncounter;
    }

    private bool IsNewEncounter()
    {
        return !hasDetection || Time.time - lastDetectionTime >= alertResetDelay;
    }

    public void ResetEncounter()
    {
        hasDetection = false;
        lastDetectionTime = 0f;
    }
}
