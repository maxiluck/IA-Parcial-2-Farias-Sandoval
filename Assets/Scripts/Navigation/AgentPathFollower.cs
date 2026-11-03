using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AgentMovement))]
public class AgentPathFollower : MonoBehaviour
{
    [SerializeField] private NavigationGraph graph;
    [SerializeField] private LayerMask restrictedLayers;

    private AgentMovement movement;
    private List<NavigationNode> path = new List<NavigationNode>();

    private int currentIndex;
    private bool followingPath;

    public bool HasPath => followingPath;
    public IReadOnlyList<NavigationNode> CurrentPath => path;
    public int CurrentPathIndex => currentIndex;

    private void Awake()
    {
        CacheComponents();
    }

    private void Update()
    {
        FollowPath();
    }

    private void CacheComponents()
    {
        movement = GetComponent<AgentMovement>();
    }

    public bool SetDestination(
    NavigationNode destination,
    bool forcePathfinding = false)
    {
        StopFollowing();

        if (graph == null)
        {
            Debug.LogWarning("Falta asignar el grafo de " + name + ".", this);
            return false;
        }

        if (destination == null)
        {
            Debug.LogWarning("El destino de " + name + " es nulo. Revisá el nodo solicitado.", this);
            return false;
        }

        if (!forcePathfinding &&
            destination.CanReachFrom(transform.position, restrictedLayers.value))
        {
            movement.MoveTo(destination.Position);
            return true;
        }

        List<NavigationNode> route = CalculatePath(destination);
        if (route.Count == 0)
        {
            NavigationNode start =
                graph.GetClosestReachableNode(transform.position, restrictedLayers.value);

            Debug.LogWarning(
                $"Sin ruta para {name}. Posición: {transform.position}. " +
                $"Inicio: {(start != null ? start.name : "NINGUNO")}. " +
                $"Destino: {destination.name}. " +
                $"Destino transitable: {destination.CanReachFrom(destination.Position, restrictedLayers.value)}",
                this);

            return false;
        }

        return FollowCalculatedPath(route);
    }

    public bool SetDestination(Vector3 position)
    {
        if (graph == null)
        {
            StopFollowing();
            Debug.LogWarning("Falta asignar el grafo.", this);
            return false;
        }

        List<NavigationNode> route = AStarPathfinding.FindPathToPosition(
            transform.position, position, graph.Nodes, restrictedLayers.value);

        if (route.Count == 0)
        {
            StopFollowing();
            Debug.LogWarning("Sin ruta para " + name + " hacia " + position
                + ". Revisá los nodos del grafo y las capas restringidas.", this);
            return false;
        }

        return FollowCalculatedPath(route);
    }

    public void StopFollowing()
    {
        followingPath = false;
        path.Clear();

        if (movement != null)
            movement.Stop();
    }

    public List<NavigationNode> CalculatePath(NavigationNode destination)
    {
        if (graph == null || destination == null) return new List<NavigationNode>();
        NavigationNode start = graph.GetClosestReachableNode(transform.position, restrictedLayers.value);
        List<NavigationNode> route =
            AStarPathfinding.FindPath(start, destination, restrictedLayers.value);

        RemoveUnnecessaryStart(route);
        return route;
    }

    private void RemoveUnnecessaryStart(List<NavigationNode> route)
    {
        for (int i = route.Count - 1; i > 0; i--)
        {
            NavigationNode node = route[i];
            if (node == null) continue;
            if (!node.CanReachFrom(transform.position, restrictedLayers.value)) continue;

            // Empezamos en el punto más avanzado de la ruta de A* que tenga paso libre.
            route.RemoveRange(0, i);
            return;
        }
    }

    public bool FollowCalculatedPath(List<NavigationNode> route)
    {
        StopFollowing();
        if (route == null || route.Count == 0) return false;
        path = route;
        currentIndex = 0;
        followingPath = true;
        MoveToCurrentNode();
        return true;
    }

    private void FollowPath()
    {
        if (!followingPath || movement.HasDestination) return;

        if (!ReachedCurrentNode())
        {
            StopFollowing();
            return;
        }

        currentIndex++;

        if (currentIndex >= path.Count)
        {
            StopFollowing();
            return;
        }

        MoveToCurrentNode();
    }

    private bool ReachedCurrentNode()
    {
        NavigationNode node = path[currentIndex];
        if (node == null) return false;
        Vector3 direction = node.Position - transform.position;
        direction.y = 0f;
        float tolerance = movement.StoppingDistance + 0.01f;
        return direction.sqrMagnitude <= tolerance * tolerance;
    }

    private void MoveToCurrentNode()
    {
        NavigationNode node = path[currentIndex];

        if (node == null)
        {
            StopFollowing();
            return;
        }

        movement.MoveTo(node.Position);
    }

    private void OnDisable()
    {
        StopFollowing();
    }

    public NavigationNode GetSearchNode(Vector3 center, float radius)
    {
        if (graph == null) return null;

        NavigationNode currentNode =
            graph.GetClosestReachableNode(transform.position, restrictedLayers.value);

        return graph.GetRandomNodeInRadius(
            center,
            radius,
            currentNode,
            restrictedLayers.value);
    }
}
