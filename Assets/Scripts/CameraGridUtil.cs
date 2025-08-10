using UnityEngine;
using UnityEngine.Tilemaps;

public static class CameraGridUtil
{
    public static void GetCameraRect(Camera cam, out float camWidth, out float camHeight, out Vector3 bottomLeft)
    {
        camHeight = cam.orthographicSize * 2f;
        camWidth = camHeight * cam.aspect;
        bottomLeft = cam.transform.position - new Vector3(camWidth / 2f, camHeight / 2f, 0f);
    }

    public static Vector3Int GetRandomCellInsideCamera(Camera cam, Tilemap map)
    {
        GetCameraRect(cam, out var camWidth, out _, out var bottomLeft);
        float rx = Random.Range(0f, camWidth);
        float ry = Random.Range(0f, camWidth / cam.aspect);
        var worldPos = bottomLeft + new Vector3(rx, ry, 0f);
        var cellPos = map.WorldToCell(worldPos);
        cellPos.z = 0;
        return cellPos;
    }

    public static BoundsInt GetCameraTileBounds(Camera cam, Tilemap map)
    {
        GetCameraRect(cam, out var camWidth, out var camHeight, out var bottomLeft);
        var topRight = cam.transform.position + new Vector3(camWidth / 2f, camHeight / 2f, 0f);

        var min = map.WorldToCell(bottomLeft);
        var max = map.WorldToCell(topRight);

        return new BoundsInt(min.x, min.y, 0, max.x - min.x + 1, max.y - min.y + 1, 1);
    }

    public static Vector3Int WorldToCellSnapped(Tilemap map, Vector3 world)
    {
        var cell = map.WorldToCell(world);
        cell.z = 0;
        return cell;
    }
}
