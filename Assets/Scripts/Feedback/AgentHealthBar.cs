using UnityEngine;
using UnityEngine.UI;

public sealed class AgentHealthBar
{
    private const float Width = 100f;
    private const float Height = 12f;
    private const float Border = 2f;
    private const float WorldScale = 0.012f;

    private readonly GameObject root;
    private readonly RectTransform fillRect;
    private readonly Image fillImage;
    private Camera viewCamera;

    public AgentHealthBar(Transform owner, float height, Camera camera = null)
    {
        viewCamera = camera != null ? camera : Camera.main;
        root = new GameObject("Health Bar (visual only)", typeof(RectTransform), typeof(Canvas), typeof(Image));
        root.layer = 2; // Ignore Raycast
        root.transform.SetParent(owner, false);
        root.transform.localPosition = Vector3.up * height;
        root.transform.localScale = Vector3.one * WorldScale;

        RectTransform backgroundRect = (RectTransform)root.transform;
        backgroundRect.sizeDelta = new Vector2(Width, Height);

        Canvas canvas = root.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.overrideSorting = true;
        canvas.sortingOrder = 110;

        Image background = root.GetComponent<Image>();
        background.color = new Color(0.05f, 0.07f, 0.09f, 0.85f);
        background.raycastTarget = false;

        GameObject fill = new GameObject("Health Fill", typeof(RectTransform), typeof(Image));
        fill.layer = 2;
        fill.transform.SetParent(root.transform, false);
        fillRect = (RectTransform)fill.transform;
        fillRect.anchorMin = new Vector2(0f, 0.5f);
        fillRect.anchorMax = fillRect.anchorMin;
        fillRect.pivot = new Vector2(0f, 0.5f);
        fillRect.anchoredPosition = new Vector2(Border, 0f);

        fillImage = fill.GetComponent<Image>();
        fillImage.raycastTarget = false;
        Show(1f, 1f);
        FaceCamera();
    }

    public void Show(float current, float maximum, bool visible = true)
    {
        if (root == null) return;
        if (root.activeSelf != visible) root.SetActive(visible);
        if (!visible) return;

        float ratio = maximum > 0f ? Mathf.Clamp01(current / maximum) : 0f;
        fillRect.sizeDelta = new Vector2((Width - Border * 2f) * ratio, Height - Border * 2f);
        fillImage.color = Color.Lerp(Color.red, Color.green, ratio);
    }

    public void FaceCamera()
    {
        if (root == null || !root.activeSelf) return;
        if (viewCamera == null) viewCamera = Camera.main;
        if (viewCamera != null) root.transform.rotation = viewCamera.transform.rotation;
    }
}
