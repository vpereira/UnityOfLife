using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;

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

    private int generation = 0;
    public int Generation => generation;

    private Coroutine simulationCoroutine;


    void Start()
    {
        ClearPatterns();
        SetPattern(pattern, cellCentered);
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

    private Vector3Int GetGenesisPosition()
    {
        return currentState.WorldToCell(genesisPoint.transform.position);
    }


    private void PlacePattern(Pattern pattern, Vector3Int cellPosition)
    {

        foreach (Vector2Int cell in pattern.cells)
        {
            Vector3Int targetCell = cellPosition + new Vector3Int(cell.x, cell.y, 0);
            currentState.SetTile(targetCell, cellTile);
        }
    }

    private void ClearPatterns()
    {
        currentState.ClearAllTiles();
        nextState.ClearAllTiles();
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
                if (!bounds.Contains(neighbor)) continue;

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

        // Iterate through visible camera area only
        foreach (Vector3Int cell in cameraBounds.allPositionsWithin)
        {
            bool isAlive = IsCellAlive(cell);
            int aliveNeighbors = CountAliveNeighbors(cell, cameraBounds);

            if (!isAlive && aliveNeighbors == 3)
            {
                nextState.SetTile(cell, cellTile);
            }
            else if (isAlive && (aliveNeighbors == 2 || aliveNeighbors == 3))
            {
                nextState.SetTile(cell, cellTile);
            }
            else
            {
                nextState.SetTile(cell, null);
            }
        }

        generation++;

        // Swap and clear
        Tilemap temp = currentState;
        currentState = nextState;
        nextState = temp;
        nextState.ClearAllTiles();
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

        if (Input.GetKeyDown(KeyCode.R))
        {
            Vector3Int randomCell = GetRandomCellInsideCamera();
            PlacePattern(pattern, randomCell); // spawn at random position
        }
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
