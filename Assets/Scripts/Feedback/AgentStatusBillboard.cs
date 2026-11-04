using UnityEngine;


public enum StatusIcon
{
    None = -1,
    Gather = 0,
    Shot = 1,
    HunterFlee = 2,
    Exit = 3,
    HunterDead = 4,
    BaitSeen = 5,
    Eating = 6,
    BoidFlee = 7,
    BoidDead = 8,
    WolfSeesHunter = 9,
    Howl = 10,
    AlertReceived = 11,
    Search = 12,
    Pathfinding = 13,
    Patrol = 14,
    Bite = 15,
    ReloadFirst = 16,
    ReloadLast = 23
}

public sealed class AgentStatusBillboard
{
    private const int Columns = 6;
    private const int Rows = 4;
    private const int CellSize = 128;
    private const int IconCount = Columns * Rows;

    private static Sprite[] icons;
    private static bool warnedMissingAtlas;

    private readonly GameObject iconObject;
    private readonly SpriteRenderer iconRenderer;
    private Camera viewCamera;
    private StatusIcon currentIcon = StatusIcon.None;

    public AgentStatusBillboard(Transform owner, float height, float size = 0.85f)
    {
        iconObject = new GameObject("Status Icon (visual only)");
        iconObject.transform.SetParent(owner, false);
        iconObject.transform.localPosition = Vector3.up * height;
        iconObject.transform.localScale = Vector3.one * size;

        iconRenderer = iconObject.AddComponent<SpriteRenderer>();
        iconRenderer.sortingOrder = 100;
        iconRenderer.enabled = false;
        viewCamera = Camera.main;
        LoadIcons();
    }

    public void Show(StatusIcon icon)
    {
        if (iconRenderer == null || currentIcon == icon) return;

        currentIcon = icon;
        int index = (int)icon;
        if (index < 0 || icons == null || index >= icons.Length)
        {
            iconRenderer.enabled = false;
            return;
        }

        iconRenderer.sprite = icons[index];
        iconRenderer.enabled = icons[index] != null;
    }

    public void FaceCamera()
    {
        if (iconObject == null || !iconRenderer.enabled) return;
        if (viewCamera == null) viewCamera = Camera.main;
        if (viewCamera != null)
            iconObject.transform.rotation = viewCamera.transform.rotation;
    }

    private static void LoadIcons()
    {
        if (icons != null && icons.Length == IconCount && icons[0] != null) return;

        Texture2D atlas = Resources.Load<Texture2D>("StatusIcons/status_atlas");
        if (atlas == null || atlas.width != Columns * CellSize || atlas.height != Rows * CellSize)
        {
            if (!warnedMissingAtlas)
            {
                Debug.LogWarning("Falta el atlas de indicadores o sus dimensiones no son 768x512.");
                warnedMissingAtlas = true;
            }
            icons = null;
            return;
        }

        icons = new Sprite[IconCount];
        for (int index = 0; index < IconCount; index++)
        {
            int column = index % Columns;
            int rowFromBottom = Rows - 1 - index / Columns;
            Rect cell = new Rect(column * CellSize, rowFromBottom * CellSize,
                CellSize, CellSize);
            icons[index] = Sprite.Create(atlas, cell, new Vector2(0.5f, 0.5f),
                CellSize, 0, SpriteMeshType.FullRect);
        }
    }
}
