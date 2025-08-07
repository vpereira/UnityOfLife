using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    [SerializeField] private Tilemap currentState;
    [SerializeField] private Tilemap nextState;

    [SerializeField] private Tile cellTile;

    [SerializeField] private float updateInterval = 0.5f; //seconds

    [SerializeField] private bool cellCentered;

    [SerializeField] private Pattern pattern;

    [SerializeField] private GameObject genesisPoint;

    [SerializeField] private GameObject gridLines;

    [SerializeField] private bool wrapAroundEnabled = false;

    [SerializeField] private Color defaultPatternColor = Color.white;

    private int generation = 0;
    public int Generation => generation;

    private Coroutine simulationCoroutine;


    private bool useRandomColorNext = false;

    private Dictionary<Vector3Int, Color> tileColors = new();

    private HashSet<string> seenPatterns = new();

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
        Camera cam = Camera.main;
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
            Vector3 cameraCenterWorld = Camera.main.transform.position;
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

    private Vector3Int GetGenesisPosition()
    {
        return currentState.WorldToCell(genesisPoint.transform.position);
    }


    private void PlacePattern(Pattern pattern, Vector3Int cellPosition)
    {
        Color patternColor = useRandomColorNext ? GetRandomColor() : defaultPatternColor;
        foreach (Vector2Int cell in pattern.cells)
        {
            Vector3Int targetCell = cellPosition + new Vector3Int(cell.x, cell.y, 0);
            currentState.SetTile(targetCell, cellTile);
            currentState.SetColor(targetCell, patternColor);
            tileColors[targetCell] = patternColor; // Store the color for this cell
        }
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
            UpdateNextState();
            // Wait for the specified update interval
            yield return new WaitForSeconds(updateInterval);
        }
    }

    public int GetAliveCellsCount()
    {
        int aliveCount = 0;

        // Iterate through all cells in the current state tilemap
        foreach (Vector3Int cell in currentState.cellBounds.allPositionsWithin)
        {
            if (IsCellAlive(cell))
            {
                aliveCount++;
            }
        }
        return aliveCount;
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

    private void UpdateNextState()
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

        // Swap and clear
        Tilemap temp = currentState;
        currentState = nextState;
        tileColors = newTileColors;
        nextState = temp;
        nextState.ClearAllTiles();
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
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            gridLines.SetActive(!gridLines.activeSelf);
        }

        if (Input.GetKeyDown(KeyCode.C))
            useRandomColorNext = true;


        if (Input.GetKeyDown(KeyCode.R))
        {
            Vector3Int randomCell = GetRandomCellInsideCamera();
            PlacePattern(pattern, randomCell); // spawn at random position
            useRandomColorNext = false; // Reset the flag after placing the pattern
        }

        if (Input.GetKeyDown(KeyCode.W))
            wrapAroundEnabled = !wrapAroundEnabled;

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
        Camera cam = Camera.main;
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
