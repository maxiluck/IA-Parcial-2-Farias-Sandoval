using System.Collections.Generic;
using UnityEngine;

public static class AStarPathfinding
{
    public static List<NavigationNode> FindPath(
        NavigationNode start,
        NavigationNode goal,
        int restrictedLayers = 0)
    {
        if (start == null || goal == null)
            return new List<NavigationNode>();

        if (!start.CanReachFrom(start.Position, restrictedLayers)
            || !goal.CanReachFrom(goal.Position, restrictedLayers))
            return new List<NavigationNode>();

        List<NavigationNode> openNodes = new List<NavigationNode>();

        Dictionary<NavigationNode, NavigationNode> cameFrom =
            new Dictionary<NavigationNode, NavigationNode>();

        Dictionary<NavigationNode, float> costs =
            new Dictionary<NavigationNode, float>();

        Dictionary<NavigationNode, float> priorities =
            new Dictionary<NavigationNode, float>();

        openNodes.Add(start);
        costs[start] = 0f;
        priorities[start] = Vector3.Distance(start.Position, goal.Position);

        while (openNodes.Count > 0)
        {
            NavigationNode current = GetLowestPriority(openNodes, priorities);

            if (current == goal)
                return BuildPath(start, goal, cameFrom);

            openNodes.Remove(current);

            foreach (NavigationNode neighbor in current.Neighbors)
            {
                if (!current.CanReach(neighbor, restrictedLayers)) continue;

                float connectionCost =
                    Vector3.Distance(current.Position, neighbor.Position);

                float newCost = costs[current] + connectionCost;

                if (costs.TryGetValue(neighbor, out float previousCost)
                    && newCost >= previousCost)
                {
                    continue;
                }

                costs[neighbor] = newCost;
                cameFrom[neighbor] = current;

                float estimatedDistance =
                    Vector3.Distance(neighbor.Position, goal.Position);

                priorities[neighbor] = newCost + estimatedDistance;

                if (!openNodes.Contains(neighbor))
                    openNodes.Add(neighbor);
            }
        }

        return new List<NavigationNode>();
    }

    // Evalúa los accesos y salidas alcanzables para minimizar el trayecto completo,
    // no solamente la distancia entre dos nodos elegidos por proximidad.
    public static List<NavigationNode> FindPathToPosition(
        Vector3 origin,
        Vector3 target,
        IReadOnlyList<NavigationNode> nodes,
        int restrictedLayers = 0)
    {
        if (nodes == null) return new List<NavigationNode>();

        HashSet<NavigationNode> goals = new HashSet<NavigationNode>();
        NavigationNode fallback = null;
        float fallbackDistance = float.PositiveInfinity;

        foreach (NavigationNode node in nodes)
        {
            if (node == null || !node.isActiveAndEnabled ||
                !node.CanReachFrom(node.Position, restrictedLayers))
                continue;

            float distance = Vector3.Distance(node.Position, target);
            if (distance < fallbackDistance)
            {
                fallbackDistance = distance;
                fallback = node;
            }

            if (node.CanReachFrom(target, restrictedLayers))
                goals.Add(node);
        }

        // Si la posición recordada está en una zona inaccesible, nos acercamos
        // al nodo transitable más próximo, como hacía la búsqueda anterior.
        if (goals.Count == 0 && fallback != null)
            goals.Add(fallback);

        List<NavigationNode> openNodes = new List<NavigationNode>();
        Dictionary<NavigationNode, NavigationNode> cameFrom =
            new Dictionary<NavigationNode, NavigationNode>();
        Dictionary<NavigationNode, float> costs =
            new Dictionary<NavigationNode, float>();
        Dictionary<NavigationNode, float> priorities =
            new Dictionary<NavigationNode, float>();

        foreach (NavigationNode node in nodes)
        {
            if (node == null || !node.isActiveAndEnabled ||
                !node.CanReachFrom(origin, restrictedLayers))
                continue;

            float entryCost = Vector3.Distance(origin, node.Position);
            openNodes.Add(node);
            costs[node] = entryCost;
            priorities[node] = entryCost + Vector3.Distance(node.Position, target);
        }

        while (openNodes.Count > 0)
        {
            NavigationNode current = GetLowestPriority(openNodes, priorities);
            if (goals.Contains(current))
                return BuildPathFromSources(current, cameFrom);

            openNodes.Remove(current);

            foreach (NavigationNode neighbor in current.Neighbors)
            {
                if (!current.CanReach(neighbor, restrictedLayers)) continue;

                float newCost = costs[current] +
                    Vector3.Distance(current.Position, neighbor.Position);
                if (costs.TryGetValue(neighbor, out float previousCost) &&
                    newCost >= previousCost)
                    continue;

                costs[neighbor] = newCost;
                cameFrom[neighbor] = current;
                priorities[neighbor] = newCost +
                    Vector3.Distance(neighbor.Position, target);

                if (!openNodes.Contains(neighbor))
                    openNodes.Add(neighbor);
            }
        }

        return new List<NavigationNode>();
    }

    private static List<NavigationNode> BuildPathFromSources(
        NavigationNode goal,
        Dictionary<NavigationNode, NavigationNode> cameFrom)
    {
        List<NavigationNode> path = new List<NavigationNode>();
        NavigationNode current = goal;

        while (current != null)
        {
            path.Add(current);
            if (!cameFrom.TryGetValue(current, out current))
                break;
        }

        path.Reverse();
        return path;
    }

    private static NavigationNode GetLowestPriority(
        List<NavigationNode> openNodes,
        Dictionary<NavigationNode, float> priorities)
    {
        NavigationNode bestNode = openNodes[0];

        for (int i = 1; i < openNodes.Count; i++)
        {
            NavigationNode candidate = openNodes[i];

            if (priorities[candidate] < priorities[bestNode])
                bestNode = candidate;
        }

        return bestNode;
    }

    private static List<NavigationNode> BuildPath(
        NavigationNode start,
        NavigationNode goal,
        Dictionary<NavigationNode, NavigationNode> cameFrom)
    {
        List<NavigationNode> path = new List<NavigationNode>();
        NavigationNode current = goal;

        while (current != start)
        {
            path.Add(current);
            current = cameFrom[current];
        }

        path.Add(start);
        path.Reverse();

        return path;
    }
}
