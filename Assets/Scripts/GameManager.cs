using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    [Header("Tilemaps")]
    [SerializeField] private Tilemap currentState;
    [SerializeField] private Tilemap nextState;

    [Header("Tiles & Defaults")]
    [SerializeField] private Tile cellTile;
    [SerializeField] private Pattern defaultPattern;
    [SerializeField] private Color defaultPatternColor = Color.white;

    [Header("Simulation Settings")]
    [SerializeField] private float updateInterval = 0.5f;
    [SerializeField] private bool cellCentered = true;
    [SerializeField] private bool wrapAroundEnabled = true;

    [Header("Scene Objects")]
    [SerializeField] private GameObject genesisPoint;
    [SerializeField] private GameObject gridLines;
    [SerializeField] private GameObject uiCanvasRoot;
    [SerializeField] private GameObject crosshair;
    [SerializeField] private UISelectionController selectionUI;

    private readonly Dictionary<Vector3Int, Color> tileColors = new();
    private readonly InputManager inputManager = new();

    private bool placementModeActive = false;
    private Coroutine simulationCoroutine;
    private readonly HashSet<string> seenPatterns = new();

    private Camera cam;
    private int generation = 0;
    public int Generation => generation;
    public bool WrapAroundEnabled => wrapAroundEnabled;
    public int GetAliveCellsCount() => tileColors.Count;

    void Awake() => cam = Camera.main;

    void Start()
    {
        ClearPatterns();
        // Seed with UI selection if available, else defaultPattern
        var seed = (selectionUI && selectionUI.SelectedPattern) ? selectionUI.SelectedPattern : defaultPattern;
        SetPattern(seed, cellCentered);
    }

    private void OnEnable() => simulationCoroutine = StartCoroutine(Simulate());
    private void OnDisable() { if (simulationCoroutine != null) StopCoroutine(simulationCoroutine); }

    private IEnumerator Simulate()
    {
        while (enabled)
        {
            StepSimulation();
            yield return new WaitForSeconds(updateInterval);
        }
    }

    // --- UI-triggered actions -------------------------------------------------

    void Update()
    {
        inputManager.Poll();

        if (inputManager.ToggleGrid) ToggleActive(gridLines);
        if (inputManager.ToggleUI) ToggleActive(uiCanvasRoot);
        if (inputManager.ToggleWrap) wrapAroundEnabled = !wrapAroundEnabled;

        if (inputManager.PatternNext) selectionUI?.NextPattern();
        if (inputManager.PatternPrev) selectionUI?.PrevPattern();
        if (inputManager.ColorNext) selectionUI?.NextColor();
        if (inputManager.ColorPrev) selectionUI?.PrevColor();

        if (inputManager.TogglePlacement)
        {
            placementModeActive = !placementModeActive;
            ToggleActive(crosshair);
            if (placementModeActive)
            {
                var camCenter = cam.transform.position;
                var centerCell = currentState.WorldToCell(camCenter);
                centerCell.z = 0;
                crosshair.transform.position = currentState.GetCellCenterWorld(centerCell);
            }
        }

        if (inputManager.SpawnRequested)
        {
            int count = inputManager.RepeatCount;

            // If modifiers demand random next selection, let the UI controller pick it.
            if (selectionUI)
            {
                if (inputManager.UseRandomPattern) selectionUI.SelectRandomPattern();
                if (inputManager.UseRandomColor) selectionUI.SelectRandomColor();
            }

            for (int i = 0; i < count; i++)
            {
                var cellPos = CameraGridUtil.GetRandomCellInsideCamera(cam, currentState);
                var pat = GetSelectedPattern(); // from UI, fallback to defaultPattern
                var col = GetSelectedColor();   // from UI, fallback to defaultColor
                PlacePattern(pat, cellPos, col);
            }

            inputManager.ResetState();
        }

        if (placementModeActive)
        {
            Vector3 worldPos = cam.ScreenToWorldPoint(Input.mousePosition);
            worldPos.z = 0;
            var cellPos = currentState.WorldToCell(worldPos);
            cellPos.z = 0;
            crosshair.transform.position = currentState.GetCellCenterWorld(cellPos);

            if (inputManager.PlacementClick)
            {
                // If modifiers demand random next selection, let the UI controller pick it.
                if (selectionUI)
                {
                    if (inputManager.UseRandomPattern) selectionUI.SelectRandomPattern();
                    if (inputManager.UseRandomColor) selectionUI.SelectRandomColor();
                }

                PlacePattern(GetSelectedPattern(), cellPos, GetSelectedColor());
            }

            if (inputManager.PlacementCancel)
            {
                placementModeActive = false;
                ToggleActive(crosshair);
            }
        }

        inputManager.ClearOneShot();
    }

    // --- Selection helpers (UI-driven) ---------------------------------------

    private Pattern GetSelectedPattern()
    {
        if (selectionUI && selectionUI.SelectedPattern)
            return selectionUI.SelectedPattern;
        return defaultPattern;
    }

    private Color GetSelectedColor()
    {
        var c = selectionUI ? selectionUI.SelectedColor : defaultPatternColor;
        c.a = 1f; // ensure visible even if UI color alpha is 0
        return c;
    }

    // --- Pattern placement & simulation --------------------------------------

    private void SetPattern(Pattern pattern, bool isCentered)
    {
        if (!pattern) { Debug.LogWarning("SetPattern: null pattern"); return; }

        Vector3Int offset;
        if (isCentered)
        {
            offset = currentState.WorldToCell(cam.transform.position);
            offset.z = 0;
        }
        else
        {
            offset = currentState.WorldToCell(genesisPoint.transform.position);
        }

        PlacePattern(pattern, offset, GetSelectedColor());
    }

    private void PlacePattern(Pattern pattern, Vector3Int cellPosition, Color color)
    {
        if (!pattern || pattern.cells == null || pattern.cells.Length == 0)
        {
            Debug.LogWarning("PlacePattern skipped: no pattern selected or pattern has no cells.");
            return;
        }

        color.a = 1f; // just in case
        foreach (var c in pattern.cells)
        {
            var pos = cellPosition + new Vector3Int(c.x, c.y, 0);
            currentState.SetTile(pos, cellTile);
            currentState.SetColor(pos, color);
            tileColors[pos] = color;
        }
    }

    private void StepSimulation()
    {
        var bounds = CameraGridUtil.GetCameraTileBounds(cam, currentState);
        var newTileColors = new Dictionary<Vector3Int, Color>();

        foreach (var cell in bounds.allPositionsWithin)
        {
            bool alive = IsCellAlive(cell);
            int n = CountAliveNeighbors(cell, bounds);

            if (!alive && n == 3)
            {
                nextState.SetTile(cell, cellTile);
                var col = GetFirstAliveNeighborColor(cell, bounds);
                nextState.SetColor(cell, col);
                newTileColors[cell] = col;
            }
            else if (alive && (n == 2 || n == 3))
            {
                nextState.SetTile(cell, cellTile);
                tileColors.TryGetValue(cell, out var keep);
                if (keep.a <= 0f) keep = defaultPatternColor;
                keep.a = 1f;
                nextState.SetColor(cell, keep);
                newTileColors[cell] = keep;
            }
            else
            {
                nextState.SetTile(cell, null);
            }
        }

        // orbit detection (optional)
        var hash = GetGridHash();
        if (!seenPatterns.Contains(hash)) seenPatterns.Add(hash);
        else Debug.Log($"Orbit detected at generation {generation}");

        currentState.ClearAllTiles();
        foreach (var kv in newTileColors)
        {
            currentState.SetTile(kv.Key, cellTile);
            currentState.SetColor(kv.Key, kv.Value);
        }
        tileColors.Clear();
        foreach (var kv in newTileColors) tileColors[kv.Key] = kv.Value;

        nextState.ClearAllTiles();
        generation++;
    }

    private int CountAliveNeighbors(Vector3Int cell, BoundsInt bounds)
    {
        int alive = 0;
        for (int dx = -1; dx <= 1; dx++)
            for (int dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0) continue;
                var n = new Vector3Int(cell.x + dx, cell.y + dy, 0);

                if (!bounds.Contains(n))
                {
                    if (!wrapAroundEnabled) continue;
                    n = WrapCoordinate(n, bounds);
                }

                if (IsCellAlive(n)) alive++;
            }
        return alive;
    }

    private Color GetFirstAliveNeighborColor(Vector3Int cell, BoundsInt bounds)
    {
        for (int dx = -1; dx <= 1; dx++)
            for (int dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0) continue;
                var n = new Vector3Int(cell.x + dx, cell.y + dy, 0);

                if (!bounds.Contains(n))
                {
                    if (!wrapAroundEnabled) continue;
                    n = WrapCoordinate(n, bounds);
                }

                if (tileColors.TryGetValue(n, out var col))
                {
                    col.a = 1f;
                    return col;
                }
            }
        var def = defaultPatternColor; def.a = 1f; return def;
    }

    private bool IsCellAlive(Vector3Int cell) => currentState.GetTile(cell) == cellTile;

    private Vector3Int WrapCoordinate(Vector3Int cell, BoundsInt b)
    {
        int x = cell.x, y = cell.y;
        if (x < b.xMin) x = b.xMax - 1; else if (x >= b.xMax) x = b.xMin;
        if (y < b.yMin) y = b.yMax - 1; else if (y >= b.yMax) y = b.yMin;
        return new Vector3Int(x, y, 0);
    }

    private static void ToggleActive(GameObject go) { if (go) go.SetActive(!go.activeSelf); }

    private void ClearPatterns()
    {
        currentState.ClearAllTiles();
        nextState.ClearAllTiles();
        tileColors.Clear();
    }

    // simple hash for orbit detection
    private string GetGridHash()
    {
        var sb = new System.Text.StringBuilder();
        foreach (var pos in tileColors.Keys) sb.Append(pos.x).Append(',').Append(pos.y).Append(';');
        return sb.ToString();
    }
}
