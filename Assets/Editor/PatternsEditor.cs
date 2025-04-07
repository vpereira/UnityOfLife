using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Pattern))]
public class PatternEditor : Editor
{
    private const int cellSize = 20;
    private const int padding = 10;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        Pattern pattern = (Pattern)target;

        GUILayout.Space(10);
        GUILayout.Label("Pattern Preview", EditorStyles.boldLabel);

        // Determine pattern bounds
        Vector2Int min = new Vector2Int(int.MaxValue, int.MaxValue);
        Vector2Int max = new Vector2Int(int.MinValue, int.MinValue);

        foreach (Vector2Int cell in pattern.cells)
        {
            if (cell.x < min.x) min.x = cell.x;
            if (cell.y < min.y) min.y = cell.y;
            if (cell.x > max.x) max.x = cell.x;
            if (cell.y > max.y) max.y = cell.y;
        }

        int patternWidth = (max.x - min.x + 1);
        int patternHeight = (max.y - min.y + 1);

        int previewWidth = patternWidth * cellSize + padding * 2;
        int previewHeight = patternHeight * cellSize + padding * 2;

        Rect previewRect = GUILayoutUtility.GetRect(previewWidth, previewHeight);

        Handles.BeginGUI();

        foreach (Vector2Int cell in pattern.cells)
        {
            int x = cell.x - min.x;
            int y = max.y - cell.y; // invert y for top-down display

            float px = previewRect.x + padding + x * cellSize;
            float py = previewRect.y + padding + y * cellSize;

            Rect cellRect = new Rect(px, py, cellSize, cellSize);
            Handles.DrawSolidRectangleWithOutline(cellRect, Color.gray, Color.black);
        }

        Handles.EndGUI();
    }
}
