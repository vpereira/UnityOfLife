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
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    [SerializeField] private GameObject genesisPoint;

    [SerializeField] private GameObject gridLines;

    private int generation = 0;
    public int Generation => generation;

    private Coroutine simulationCoroutine;


    void Start()
    {
        SetPattern(pattern, cellCentered);
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
        SetPattern(pattern, offset);
    }

    private Vector3Int GetGenesisPosition()
    {
        return currentState.WorldToCell(genesisPoint.transform.position);
    }


    private void SetPattern(Pattern pattern, Vector3Int cellPosition)
    {
        ClearPattern();

        foreach (Vector2Int cell in pattern.cells)
        {
            Vector3Int targetCell = cellPosition + new Vector3Int(cell.x, cell.y, 0);
            currentState.SetTile(targetCell, cellTile);
        }
    }

    private void ClearPattern()
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
        if(simulationCoroutine != null)
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

    private int CountAliveNeighbors(Vector3Int cell)
    {
        int aliveCount = 0;

        // Check all 8 neighboring cells
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                // Skip the current cell
                if (x == 0 && y == 0) continue;

                Vector3Int neighborCell = new Vector3Int(cell.x + x, cell.y + y);

                if (IsCellAlive(neighborCell))
                {
                    aliveCount++;
                }
            }
        }

        return aliveCount;
    }


    private void UpdateNextState()
    {

        // early return if no cells are alive
        if (GetAliveCellsCount() == 0) return;

        BoundsInt bounds = currentState.cellBounds;

        // Expand bounds by 1 to ensure all neighbors are evaluated
        bounds.xMin -= 1;
        bounds.yMin -= 1;
        bounds.xMax += 1;
        bounds.yMax += 1;

        // Iterate through the expanded bounds
        foreach (Vector3Int cell in bounds.allPositionsWithin)
        {
            bool isAlive = IsCellAlive(cell);
            int aliveNeighbors = CountAliveNeighbors(cell);

            if (!isAlive && aliveNeighbors == 3)
            {
                // Dead cell becomes alive
                nextState.SetTile(cell, cellTile);
            }
            else if (isAlive && (aliveNeighbors == 2 || aliveNeighbors == 3))
            {
                // Alive cell stays alive
                nextState.SetTile(cell, cellTile);
            }
            else
            {
                // Cell dies or remains dead
                nextState.SetTile(cell, null);
            }
        }

        // Update the current generation
        generation++;

        // Swap states
        Tilemap temp = currentState;
        currentState = nextState;
        nextState = temp;

        // Clear nextState to prepare for next iteration
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
    }
}


