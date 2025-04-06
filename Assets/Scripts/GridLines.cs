using UnityEngine;

public class GridLineDrawer : MonoBehaviour
{
    public int width = 100;
    public int height = 100;
    public float cellSize = 1f;
    public Material lineMaterial;
    public Color lineColor = Color.gray;
    public float lineWidth = 0.05f;

    void Start()
    {
        UpdateGridSizeToMatchCamera();
        AlignCameraToBottomLeft();
        DrawGrid();
    }

    void AlignCameraToBottomLeft()
    {
        Camera cam = Camera.main;
        float camHeight = cam.orthographicSize * 2f;
        float camWidth = camHeight * cam.aspect;

        // Move the camera to center on the grid (starting at bottom-left)
        cam.transform.position = new Vector3(camWidth / 2f, camHeight / 2f, -10f);
    }

Vector3 GetCameraBottomLeft()
{
    Camera cam = Camera.main;
    float camHeight = cam.orthographicSize * 2f;
    float camWidth = camHeight * cam.aspect;

    Vector3 camCenter = cam.transform.position;
    return new Vector3(
        camCenter.x - camWidth / 2f,
        camCenter.y - camHeight / 2f,
        0f
    );
}

   

    void UpdateGridSizeToMatchCamera()
    {
        Camera cam = Camera.main;

        // Ensure camera is orthographic
        if (!cam.orthographic)
        {
            Debug.LogError("GridLineDrawer requires an orthographic camera.");
            return;
        }

        float camHeightWorldUnits = cam.orthographicSize * 2f;
        float camWidthWorldUnits = camHeightWorldUnits * cam.aspect;

        width = Mathf.CeilToInt(camWidthWorldUnits / cellSize);
        height = Mathf.CeilToInt(camHeightWorldUnits / cellSize);
    }
    
void DrawGrid()
{
    Vector3 bottomLeft = GetCameraBottomLeft();

    // Adjust so the grid is centered around camera position
    bottomLeft = new Vector3(
        Mathf.Floor(bottomLeft.x / cellSize) * cellSize,
        Mathf.Floor(bottomLeft.y / cellSize) * cellSize,
        0f
    );

    for (int x = 0; x <= width; x++)
    {
        Vector3 start = bottomLeft + new Vector3(x * cellSize, 0, 0);
        Vector3 end = bottomLeft + new Vector3(x * cellSize, height * cellSize, 0);
        CreateLine(start, end);
    }

    for (int y = 0; y <= height; y++)
    {
        Vector3 start = bottomLeft + new Vector3(0, y * cellSize, 0);
        Vector3 end = bottomLeft + new Vector3(width * cellSize, y * cellSize, 0);
        CreateLine(start, end);
    }
}

    void CreateLine(Vector3 start, Vector3 end)
    {
        GameObject line = new GameObject("GridLine");
        line.transform.parent = this.transform;

        var lr = line.AddComponent<LineRenderer>();
        lr.positionCount = 2;
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);
        lr.material = lineMaterial;
        lr.startWidth = lineWidth;
        lr.endWidth = lineWidth;
        lr.useWorldSpace = true;
        lr.loop = false;
        lr.startColor = lineColor;
        lr.endColor = lineColor;
        lr.sortingOrder = -10;
    }
}
