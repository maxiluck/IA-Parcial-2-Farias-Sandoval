using System.Collections.Generic;
using UnityEngine;

public class NavigationGraph : MonoBehaviour
{
    [SerializeField]
    private List<NavigationNode> nodes =
        new List<NavigationNode>();

    public IReadOnlyList<NavigationNode> Nodes => nodes;

    public NavigationNode GetClosestNode(Vector3 position, int restrictedLayers = 0)
    {
        NavigationNode closestNode = null;
        float closestDistance = float.PositiveInfinity;

        foreach (NavigationNode node in nodes)
        {
            if (node == null || !node.isActiveAndEnabled) continue;

            float distance = (node.Position - position).sqrMagnitude;
            if (distance >= closestDistance) continue;

            // El destino debe ser transitable, aunque la posición recordada no lo sea.
            if (!node.CanReachFrom(node.Position, restrictedLayers)) continue;

            closestDistance = distance;
            closestNode = node;
        }

        return closestNode;
    }

    public NavigationNode GetClosestReachableNode(Vector3 position, int restrictedLayers = 0)
    {
        NavigationNode closestNode = null;
        float closestDistance = float.PositiveInfinity;

        foreach (NavigationNode node in nodes)
        {
            if (node == null || !node.CanReachFrom(position, restrictedLayers))
                continue;

            float distance = (node.Position - position).sqrMagnitude;

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestNode = node;
            }
        }

        return closestNode;
    }

    public NavigationNode GetRandomNodeInRadius(
    Vector3 center,
    float radius,
    NavigationNode excludedNode,
    int restrictedLayers = 0)
    {
        NavigationNode selectedNode = null;
        int candidates = 0;

        foreach (NavigationNode node in nodes)
        {
            if (node == null || node == excludedNode)
                continue;

            float distance = (node.Position - center).sqrMagnitude;

            if (distance > radius * radius)
                continue;

            if (!node.CanReachFrom(node.Position, restrictedLayers))
                continue;

            candidates++;

            if (Random.Range(0, candidates) == 0)
                selectedNode = node;
        }

        return selectedNode;
    }
}
