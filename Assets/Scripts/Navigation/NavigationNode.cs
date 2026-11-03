using System.Collections.Generic;
using UnityEngine;

public class NavigationNode : MonoBehaviour
{
    [Header("Conexiones")]
    [SerializeField]
    private List<NavigationNode> neighbors =
        new List<NavigationNode>();

    [Header("Obstáculos")]
    [SerializeField] private LayerMask obstacleMask;

    [SerializeField, Min(0.01f)]
    private float clearanceRadius = 0.55f;

    public IReadOnlyList<NavigationNode> Neighbors => neighbors;
    public Vector3 Position => transform.position;

    public bool CanReach(NavigationNode neighbor, int restrictedLayers = 0)
    {
        if (neighbor == null || neighbor == this)
            return false;

        return IsPathClear(Position, neighbor.Position, restrictedLayers);
    }

    public bool CanReachFrom(Vector3 origin, int restrictedLayers = 0)
    {
        return IsPathClear(origin, Position, restrictedLayers);
    }

    private bool IsPathClear(Vector3 origin, Vector3 destination, int restrictedLayers)
    {
        int mask = obstacleMask.value | restrictedLayers;
        QueryTriggerInteraction triggers = restrictedLayers == 0
            ? QueryTriggerInteraction.Ignore : QueryTriggerInteraction.Collide;
        if (!HasClearance(origin, mask, triggers) || !HasClearance(destination, mask, triggers))
            return false;

        Vector3 direction = destination - origin;
        float distance = direction.magnitude;

        if (distance <= 0.001f)
            return true;

        return !Physics.SphereCast(
            origin,
            clearanceRadius,
            direction / distance,
            out _,
            distance,
            mask,
            triggers);
    }

    private bool HasClearance(Vector3 position, int mask, QueryTriggerInteraction triggers)
    {
        return !Physics.CheckSphere(
            position,
            clearanceRadius,
            mask,
            triggers);
    }

    private void OnDrawGizmos()
    {
        DrawNode();
        DrawConnections();
    }

    private void DrawNode()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(Position, 0.2f);
    }

    private void DrawConnections()
    {
        foreach (NavigationNode neighbor in neighbors)
        {
            if (neighbor == null || neighbor == this) continue;

            Gizmos.color = CanReach(neighbor)
                ? Color.cyan
                : Color.red;

            Gizmos.DrawLine(Position, neighbor.Position);
        }
    }
}
