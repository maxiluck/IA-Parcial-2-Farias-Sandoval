using UnityEngine;

[RequireComponent(typeof(AgentPathFollower))]
public class NavigationPathFeedback : MonoBehaviour
{
    [SerializeField] private Color pathColor = Color.green;

    private AgentPathFollower pathFollower;

    private void Awake()
    {
        CacheComponents();
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying || pathFollower == null)
            return;

        if (!pathFollower.HasPath)
            return;

        DrawRemainingPath();
    }

    private void CacheComponents()
    {
        pathFollower = GetComponent<AgentPathFollower>();
    }

    private void DrawRemainingPath()
    {
        Gizmos.color = pathColor;

        Vector3 previousPosition = transform.position;

        for (int i = pathFollower.CurrentPathIndex;
             i < pathFollower.CurrentPath.Count;
             i++)
        {
            NavigationNode node = pathFollower.CurrentPath[i];

            if (node == null) return;

            Vector3 position = node.Position;
            position.y = transform.position.y;

            Gizmos.DrawLine(previousPosition, position);
            Gizmos.DrawWireSphere(position, 0.15f);

            previousPosition = position;
        }
    }
}