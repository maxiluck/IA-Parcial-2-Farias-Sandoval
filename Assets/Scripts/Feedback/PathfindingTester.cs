using System.Collections.Generic;
using UnityEngine;

public class PathfindingTester : MonoBehaviour
{
    [SerializeField] private NavigationNode startNode;
    [SerializeField] private NavigationNode goalNode;

    private List<NavigationNode> path = new List<NavigationNode>();

    private void Start()
    {
        CalculatePath();
    }

    [ContextMenu("Calculate Path")]
    private void CalculatePath()
    {
        path.Clear();

        if (startNode == null || goalNode == null)
        {
            Debug.LogWarning("Asigná el nodo inicial y el destino.", this);
            return;
        }

        path = AStarPathfinding.FindPath(startNode, goalNode);

        if (path.Count == 0)
        {
            Debug.LogWarning("No existe un camino entre los nodos.", this);
            return;
        }

        Debug.Log("Camino encontrado. Cantidad de nodos: " + path.Count, this);
    }

    private void OnDrawGizmos()
    {
        DrawPath();
    }

    private void DrawPath()
    {
        if (path == null || path.Count == 0) return;

        Gizmos.color = Color.green;
        Vector3 offset = Vector3.up * 0.3f;

        for (int i = 0; i < path.Count; i++)
        {
            if (path[i] == null) continue;

            Vector3 position = path[i].Position + offset;
            Gizmos.DrawSphere(position, 0.25f);

            if (i + 1 < path.Count && path[i + 1] != null)
            {
                Vector3 nextPosition = path[i + 1].Position + offset;
                Gizmos.DrawLine(position, nextPosition);
            }
        }
    }
}