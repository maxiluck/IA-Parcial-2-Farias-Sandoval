using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class SafeZone : MonoBehaviour
{
    [SerializeField] private NavigationNode entryNode;
    private BoxCollider area;

    public NavigationNode EntryNode => entryNode;

    private void Awake()
    {
        CacheArea();
    }

    public bool Contains(Vector3 position)
    {
        if (!isActiveAndEnabled) return false;
        if (area == null) CacheArea();
        if (area == null || !area.enabled) return false;

        Vector3 localPosition = transform.InverseTransformPoint(position) - area.center;
        Vector3 halfSize = area.size * 0.5f;
        return Mathf.Abs(localPosition.x) <= halfSize.x
            && Mathf.Abs(localPosition.y) <= halfSize.y
            && Mathf.Abs(localPosition.z) <= halfSize.z;
    }

    private void CacheArea()
    {
        area = GetComponent<BoxCollider>();
    }
}
