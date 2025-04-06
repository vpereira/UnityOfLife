using UnityEngine;

[CreateAssetMenu(fileName = "Pattern", menuName = "Scriptable Objects/Pattern")]
public class Pattern : ScriptableObject
{
    public Vector2Int[] cells;

    // get the center of the Cells
    public Vector3Int GetCenter()
    {
        if (cells.Length == 0) return Vector3Int.zero;

        int minX = cells[0].x;
        int maxX = cells[0].x;
        int minY = cells[0].y;
        int maxY = cells[0].y;

        foreach (Vector2Int cell in cells)
        {
            if (cell.x < minX) minX = cell.x;
            if (cell.x > maxX) maxX = cell.x;
            if (cell.y < minY) minY = cell.y;
            if (cell.y > maxY) maxY = cell.y;
        }

        return new Vector3Int((minX + maxX) / 2, (minY + maxY) / 2, 0);
    }
}
