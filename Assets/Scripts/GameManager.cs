using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    [Header("Tilemaps")]
    [SerializeField] private Tilemap currentState;
    [SerializeField] private Tilemap nextState;

    [Header("Tiles & Patterns")]
    [SerializeField] private Tile cellTile;
    [SerializeField] private Pattern pattern;
    [SerializeField] private List<Pattern> patternLibrary = new();

    [Header("Simulation Settings")]
    [SerializeField] private float updateInterval = 0.5f; //seconds
    [SerializeField] private bool cellCentered = true;
    [SerializeField] private bool wrapAroundEnabled = true;

    [Header("UI Elements")]
    [SerializeField] private GameObject genesisPoint;
    [SerializeField] private GameObject gridLines;
    [SerializeField] private GameObject uiCanvasRoot;
    [SerializeField] private GameObject crosshair;

    [Header("Colors")]
    [SerializeField] private Color defaultPatternColor = Color.white;
    [SerializeField]
    private Color[] patternColors = new Color[]
    {
        Color.red,
        Color.green,
        Color.yellow,
        Color.black,
    };

    private int generation = 0;
    public int Generation => generation;

    public bool WrapAroundEnabled => wrapAroundEnabled;

    private Coroutine simulationCoroutine;

    private bool useRandomColorNext = false;

    private int colorIndex = 0;

    private Dictionary<Vector3Int, Color> tileColors = new();
    private InputManager inputManager = new InputManager();

    private bool placementModeActive = false;

    private HashSet<string> seenPatterns = new();

    public int GetAliveCellsCount() => tileColors.Count;
    private Camera cam; // Camera.main cache

    void Awake()
    {
        cam = Camera.main;
    }

    void Start()
    {
        ClearPatterns();
        SetPattern(pattern, cellCentered);
    }

    private string GetGridHash()
    {
        var sb = new System.Text.StringBuilder();
        foreach (var pos in tileColors.Keys)
            sb.Append($"{pos.x},{pos.y};");
        return sb.ToString();
    }

    private BoundsInt GetCameraTileBounds()
    {
        float camHeight = cam.orthographicSize * 2f;
        float camWidth = camHeight * cam.aspect;

        Vector3 bottomLeft = cam.transform.position - new Vector3(camWidth / 2f, camHeight / 2f, 0);
        Vector3 topRight = cam.transform.position + new Vector3(camWidth / 2f, camHeight / 2f, 0);

        Vector3Int min = currentState.WorldToCell(bottomLeft);
        Vector3Int max = currentState.WorldToCell(topRight);

        return new BoundsInt(min.x, min.y, 0, max.x - min.x + 1, max.y - min.y + 1, 1);
    }


    private void SetPattern(Pattern pattern, bool isCentered = true)
    {
        if (pattern == null)
        {
            Debug.LogError("Pattern is null");
            return;
        }

        Vector3Int offset = Vector3Int.zero;

        // Set the pattern in the current state tilemap
        if (isCentered)
        {
            Vector3 cameraCenterWorld = cam.transform.position;
            offset = currentState.WorldToCell(cameraCenterWorld);

            // Optional: snap to nearest tile
            offset.z = 0;
            // offset = pattern.GetCenter();
        }
        else
        {
            offset = GetGenesisPosition();
        }
        PlacePattern(pattern, offset);
    }

    private Color GetRandomColor()
    {
        return new Color(Random.value, Random.value, Random.value);
    }

    private Color GetNextColor()
    {
        Color color = patternColors[colorIndex];

        if (color == null)
            return defaultPatternColor;

        colorIndex = (colorIndex + 1) % patternColors.Length;
        return color;
    }

    private Vector3Int GetGenesisPosition()
    {
        return currentState.WorldToCell(genesisPoint.transform.position);
    }


    private void PlacePattern(Pattern pattern, Vector3Int cellPosition, Color color)
    {
        foreach (Vector2Int cell in pattern.cells)
        {
            Vector3Int targetCell = cellPosition + new Vector3Int(cell.x, cell.y, 0);
            currentState.SetTile(targetCell, cellTile);
            currentState.SetColor(targetCell, color);
            tileColors[targetCell] = color;
        }
    }

    private void PlacePattern(Pattern pattern, Vector3Int cellPosition)
    {
        PlacePattern(pattern, cellPosition, GetColor(useRandomColorNext));
    }

    private void ClearPatterns()
    {
        currentState.ClearAllTiles();
        nextState.ClearAllTiles();
        tileColors.Clear();
    }

    private void OnEnable()
    {
        simulationCoroutine = StartCoroutine(Simulate());
    }

    private void OnDisable()
    {
        if (simulationCoroutine != null)
            StopCoroutine(simulationCoroutine);
    }

    private IEnumerator Simulate()
    {
        while (enabled)
        {
            // Update the next state based on the current state
            StepsSimulation();
            yield return new WaitForSeconds(updateInterval);
        }
    }

    // Suppose you're checking cell (10,10) and it has live neighbors at:
    // (9,10), (10,11), and (11,11)
    // Then CountAliveNeighbors(new Vector3Int(10,10)) would return 3.
    private int CountAliveNeighbors(Vector3Int cell, BoundsInt bounds)
    {
        int aliveCount = 0;

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0) continue;

                Vector3Int neighbor = new Vector3Int(cell.x + x, cell.y + y, 0);

                // Skip if outside bounds
                if (!bounds.Contains(neighbor))
                    if (wrapAroundEnabled)
                        neighbor = WrapCoordinate(neighbor, bounds);
                    else
                        continue;

                if (IsCellAlive(neighbor))
                {
                    aliveCount++;
                }
            }
        }
        return aliveCount;
    }

    private void StepsSimulation()
    {
        BoundsInt cameraBounds = GetCameraTileBounds();
        Dictionary<Vector3Int, Color> newTileColors = new();
        // Iterate through visible camera area only
        foreach (Vector3Int cell in cameraBounds.allPositionsWithin)
        {
            bool isAlive = IsCellAlive(cell);
            int aliveNeighbors = CountAliveNeighbors(cell, cameraBounds);

            if (!isAlive && aliveNeighbors == 3)
            {
                nextState.SetTile(cell, cellTile);
                Color inheritedColor = GetFirstAliveNeighborColor(cell, cameraBounds);
                nextState.SetColor(cell, inheritedColor);
                newTileColors[cell] = inheritedColor;
            }
            else if (isAlive && (aliveNeighbors == 2 || aliveNeighbors == 3))
            {
                nextState.SetTile(cell, cellTile);
                if (tileColors.TryGetValue(cell, out var color))
                    nextState.SetColor(cell, color);
                newTileColors[cell] = color; // Keep the same color
            }
            else
            {
                nextState.SetTile(cell, null);
            }
        }

        // Check if the current grid configuration has been seen before
        string gridHash = GetGridHash();
        if (!seenPatterns.Contains(gridHash))
            seenPatterns.Add(gridHash);
        else
            Debug.Log($"Orbit detected at generation {generation}");

        currentState.ClearAllTiles();
        foreach (var posColor in newTileColors)
        {
            currentState.SetTile(posColor.Key, cellTile);
            currentState.SetColor(posColor.Key, posColor.Value);
        }
        tileColors = newTileColors;
        nextState.ClearAllTiles();
        generation++;

    }


    private Color GetFirstAliveNeighborColor(Vector3Int cell, BoundsInt bounds)
    {
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0) continue;

                Vector3Int neighbor = new Vector3Int(cell.x + x, cell.y + y, 0);
                if (bounds.Contains(neighbor) && tileColors.TryGetValue(neighbor, out var color))
                    return color;
            }
        }

        return defaultPatternColor;
    }


    private bool IsCellAlive(Vector3Int cell)
    {
        return currentState.GetTile(cell) == cellTile;
    }

    private Color GetColor(bool modifier)
    {
        return modifier ? GetNextColor() : defaultPatternColor;
    }

    void Update()
    {
        inputManager.Poll();

        if (inputManager.ToggleGrid)
            gridLines.SetActive(!gridLines.activeSelf);

        if (inputManager.ToggleUI)
            uiCanvasRoot.SetActive(!uiCanvasRoot.activeSelf);

        if (inputManager.ToggleWrap)
            wrapAroundEnabled = !wrapAroundEnabled;

        if (inputManager.TogglePlacement)
        {
            placementModeActive = !placementModeActive;
            crosshair.SetActive(placementModeActive);
            if (placementModeActive)
            {
                Vector3 camCenter = cam.transform.position;
                Vector3Int centerCell = currentState.WorldToCell(camCenter);
                centerCell.z = 0;
                crosshair.transform.position = currentState.GetCellCenterWorld(centerCell);
            }
        }

        if (inputManager.SpawnRequested)
        {
            int count = inputManager.RepeatCount;
            for (int i = 0; i < count; i++)
            {
                Vector3Int randomCell = GetRandomCellInsideCamera();

                Pattern selectedPattern = inputManager.UseRandomPattern && patternLibrary.Count > 0
                    ? patternLibrary[Random.Range(0, patternLibrary.Count)]
                    : pattern;

                PlacePattern(selectedPattern, randomCell, GetColor(inputManager.UseRandomColor));
            }
            inputManager.ResetState(); // clear buffered C/P and number
        }

        if (placementModeActive)
        {
            Vector3 worldPos = cam.ScreenToWorldPoint(Input.mousePosition);
            worldPos.z = 0;
            Vector3Int cellPos = currentState.WorldToCell(worldPos);
            cellPos.z = 0;
            crosshair.transform.position = currentState.GetCellCenterWorld(cellPos);

            if (inputManager.PlacementClick)
            {
                PlacePattern(pattern, cellPos, GetColor(useRandomColorNext));
                useRandomColorNext = false;
            }

            if (inputManager.PlacementCancel)
            {
                placementModeActive = false;
                crosshair.SetActive(false);
            }
        }

        inputManager.ClearOneShot(); // clear per-frame toggles
    }


    private Vector3Int WrapCoordinate(Vector3Int cell, BoundsInt bounds)
    {
        int x = cell.x;
        int y = cell.y;

        if (x < bounds.xMin) x = bounds.xMax - 1;
        if (x >= bounds.xMax) x = bounds.xMin;

        if (y < bounds.yMin) y = bounds.yMax - 1;
        if (y >= bounds.yMax) y = bounds.yMin;

        return new Vector3Int(x, y, 0);
    }

    private Vector3Int GetRandomCellInsideCamera()
    {
        float camHeight = cam.orthographicSize * 2f;
        float camWidth = camHeight * cam.aspect;

        // Bottom-left corner in world space
        Vector3 bottomLeft = cam.transform.position - new Vector3(camWidth / 2f, camHeight / 2f, 0f);

        // Pick a random point inside the view
        float randomX = Random.Range(0f, camWidth);
        float randomY = Random.Range(0f, camHeight);

        Vector3 worldPos = bottomLeft + new Vector3(randomX, randomY, 0f);
        Vector3Int cellPos = currentState.WorldToCell(worldPos);
        cellPos.z = 0;

        return cellPos;
    }
}
