using UnityEngine;

public class PerceptionFeedback : MonoBehaviour
{
    [SerializeField] private AgentPerception perception;

    [SerializeField, Min(3)]
    private int segments = 24;

    private void OnDrawGizmosSelected()
    {
        if (perception == null) return;

        DrawVisionCone();
    }

    private void DrawVisionCone()
    {
        Gizmos.color = perception.CanSeeTarget
            ? Color.green
            : Color.yellow;

        Vector3 origin = perception.EyePosition;
        float halfAngle = perception.ViewAngle * 0.5f;
        float step = perception.ViewAngle / Mathf.Max(3, segments);

        Vector3 firstPoint = GetArcPoint(-halfAngle);
        Vector3 previousPoint = firstPoint;

        Gizmos.DrawLine(origin, firstPoint);

        int steps = Mathf.Max(3, segments);

        for (int i = 1; i <= steps; i++)
        {
            float angle = -halfAngle + step * i;
            Vector3 currentPoint = GetArcPoint(angle);

            Gizmos.DrawLine(previousPoint, currentPoint);
            previousPoint = currentPoint;
        }

        Gizmos.DrawLine(origin, previousPoint);
    }

    private Vector3 GetArcPoint(float angle)
    {
        Vector3 direction =
            Quaternion.AngleAxis(angle, Vector3.up)
            * perception.transform.forward;

        return perception.EyePosition
            + direction * perception.ViewRadius;
    }
}