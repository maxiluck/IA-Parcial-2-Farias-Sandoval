using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AgentPathFollower))]
public class HunterRefuge : MonoBehaviour
{
    [SerializeField] private List<SafeZone> safeZones = new List<SafeZone>();
    private AgentPathFollower pathFollower;

    public SafeZone CurrentZone => GetCurrentZone();
    public bool IsInsideSafeZone => CurrentZone != null;

    public bool HasTwoValidZones()
    {
        SafeZone first = null;
        for (int i = 0; i < safeZones.Count; i++)
        {
            SafeZone zone = safeZones[i];
            if (zone == null || zone.EntryNode == null || !zone.Contains(zone.EntryNode.Position)) continue;
            if (first == null) first = zone;
            else if (zone != first) return true;
        }
        return false;
    }

    private void Awake()
    {
        CacheComponents();
    }

    public SafeZone SelectNearestReachable(out List<NavigationNode> selectedPath, out float length)
    {
        SafeZone selected = null;
        selectedPath = null;
        length = float.PositiveInfinity;

        for (int i = 0; i < safeZones.Count; i++)
        {
            SafeZone zone = safeZones[i];
            if (zone == null || !zone.isActiveAndEnabled || zone.EntryNode == null) continue;
            if (!zone.Contains(zone.EntryNode.Position)) continue;

            List<NavigationNode> route = pathFollower.CalculatePath(zone.EntryNode);
            if (route.Count == 0) continue;

            float routeLength = CalculateLength(route);
            if (routeLength >= length) continue;

            selected = zone;
            selectedPath = route;
            length = routeLength;
        }

        return selected;
    }

    private void CacheComponents()
    {
        pathFollower = GetComponent<AgentPathFollower>();
    }

    private SafeZone GetCurrentZone()
    {
        for (int i = 0; i < safeZones.Count; i++)
        {
            SafeZone zone = safeZones[i];
            if (zone != null && zone.Contains(transform.position)) return zone;
        }
        return null;
    }

    private float CalculateLength(List<NavigationNode> route)
    {
        float distance = Vector3.Distance(transform.position, route[0].Position);
        for (int i = 1; i < route.Count; i++)
            distance += Vector3.Distance(route[i - 1].Position, route[i].Position);
        return distance;
    }
}
