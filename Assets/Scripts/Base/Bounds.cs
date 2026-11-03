using UnityEngine;

public class Bounds : MonoBehaviour
{
    [SerializeField, Min(1f)] private float _height = 16f;
    [SerializeField, Min(1f)] private float _width = 24f;
    [SerializeField] private bool _drawGizmos = true;

    public bool Contains(Vector3 position, float margin = 0f)
    {
        GetLimits(margin, out Vector2 minimum, out Vector2 maximum);

        return position.x >= minimum.x && position.x <= maximum.x
            && position.z >= minimum.y && position.z <= maximum.y;
    }

    public Vector3 CalculateBoundPosition(Vector3 position, float margin = 0f)
    {
        GetLimits(margin, out Vector2 minimum, out Vector2 maximum);

        position.x = Mathf.Clamp(position.x, minimum.x, maximum.x);
        position.z = Mathf.Clamp(position.z, minimum.y, maximum.y);
        return position;
    }

    public Vector3 GetInwardDirection(Vector3 position, float margin, float anticipationDistance)
    {
        GetLimits(margin, out Vector2 minimum, out Vector2 maximum);

        float distance = Mathf.Max(anticipationDistance, 0.01f);
        float left = Mathf.Clamp01((minimum.x + distance - position.x) / distance);
        float right = Mathf.Clamp01((position.x - maximum.x + distance) / distance);
        float bottom = Mathf.Clamp01((minimum.y + distance - position.z) / distance);
        float top = Mathf.Clamp01((position.z - maximum.y + distance) / distance);

        return new Vector3(left - right, 0f, bottom - top);
    }

    public Vector3 GetRandomPosition(float heightAboveCenter, float margin)
    {
        GetLimits(margin, out Vector2 minimum, out Vector2 maximum);

        return new Vector3(
            Random.Range(minimum.x, maximum.x),
            transform.position.y + heightAboveCenter,
            Random.Range(minimum.y, maximum.y));
    }

    private void GetLimits(float margin, out Vector2 minimum, out Vector2 maximum)
    {
        float safeMargin = Mathf.Clamp(
            margin, 0f, Mathf.Min(_width, _height) * 0.5f - 0.01f);

        Vector2 halfSize = new Vector2(_width, _height) * 0.5f;
        Vector2 center = new Vector2(transform.position.x, transform.position.z);
        Vector2 padding = Vector2.one * safeMargin;

        minimum = center - halfSize + padding;
        maximum = center + halfSize - padding;
    }

    private void OnDrawGizmos()
    {
        if (!_drawGizmos) return;

        DrawArea();
    }

    private void DrawArea()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(transform.position, new Vector3(_width, 0.1f, _height));
    }
}
