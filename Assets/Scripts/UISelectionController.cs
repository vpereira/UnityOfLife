using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UISelectionController : MonoBehaviour
{
    [Header("Data Sources")]
    [SerializeField] private Pattern[] patternLibrary;
    [SerializeField] private Color[] patternColors;

    [Header("UI")]
    [SerializeField] private TMP_Text patternNameText;
    [SerializeField] private RectTransform patternPreviewRoot; // has GridLayoutGroup
    [SerializeField] private Image previewCellPrefab;      // 1x1 white sprite Image prefab
    [SerializeField] private int previewPadding = 1;

    private int patternIndex = 0;
    private int colorIndex = 0;

    public Pattern SelectedPattern =>
        (patternLibrary != null && patternLibrary.Length > 0)
        ? patternLibrary[Mathf.Clamp(patternIndex, 0, patternLibrary.Length - 1)]
        : null;

    public Color SelectedColor =>
        (patternColors != null && patternColors.Length > 0)
        ? patternColors[Mathf.Clamp(colorIndex, 0, patternColors.Length - 1)]
        : Color.white;

    void Start() => RefreshUI();

    public void NextPattern() { patternIndex = Wrap(patternIndex + 1, patternLibrary?.Length ?? 0); RefreshUI(); }
    public void PrevPattern() { patternIndex = Wrap(patternIndex - 1, patternLibrary?.Length ?? 0); RefreshUI(); }
    public void NextColor() { colorIndex = Wrap(colorIndex + 1, patternColors?.Length ?? 0); RefreshUI(); }
    public void PrevColor() { colorIndex = Wrap(colorIndex - 1, patternColors?.Length ?? 0); RefreshUI(); }

    public int PatternCount => patternLibrary?.Length ?? 0;
    public int ColorCount => patternColors?.Length ?? 0;

    public void SelectRandomPattern() { if (PatternCount > 0) { patternIndex = Random.Range(0, PatternCount); RefreshUI(); } }
    public void SelectRandomColor() { if (ColorCount > 0) { colorIndex = Random.Range(0, ColorCount); RefreshUI(); } }

    private static int Wrap(int v, int len) => (len <= 0) ? 0 : (v % len + len) % len;

    private void RefreshUI()
    {
        if (patternNameText) patternNameText.text = SelectedPattern ? SelectedPattern.name : "(no pattern)";
        RebuildPreview();
    }

    private void RebuildPreview()
    {
        if (!patternPreviewRoot || !previewCellPrefab) return;

        // Clear
        for (int i = patternPreviewRoot.childCount - 1; i >= 0; i--)
            Destroy(patternPreviewRoot.GetChild(i).gameObject);

        var p = SelectedPattern;
        if (p == null || p.cells == null || p.cells.Length == 0) return;

        // ----- Bounds -----
        int minX = p.cells[0].x, maxX = p.cells[0].x;
        int minY = p.cells[0].y, maxY = p.cells[0].y;
        foreach (var c in p.cells)
        {
            if (c.x < minX) minX = c.x; if (c.x > maxX) maxX = c.x;
            if (c.y < minY) minY = c.y; if (c.y > maxY) maxY = c.y;
        }
        int width = (maxX - minX + 1);
        int height = (maxY - minY + 1);

        // Include padding border
        int cols = width + previewPadding * 2;
        int rows = height + previewPadding * 2;

        // ----- Compute cell size & origin in the Rect -----
        var rt = (RectTransform)patternPreviewRoot;
        var size = rt.rect.size;
        if (size.x <= 0f || size.y <= 0f) size = new Vector2(cols, rows);

        float cellSize = Mathf.Floor(Mathf.Min(size.x / cols, size.y / rows));
        if (cellSize < 1f) cellSize = 1f;

        // Origin at top-left in local space (for easy top-down placement)
        // We'll convert (x,y) → anchoredPosition so that (0,0) is top-left of content.
        // Build a helper to convert grid coordinates (col,row-from-top) to anchored position.
        Vector2 topLeft = new Vector2(-size.x * 0.5f, size.y * 0.5f);
        Vector2 CellTopLeft(int col, int rowFromTop)
            => topLeft + new Vector2(col * cellSize, -rowFromTop * cellSize);

        // Color (fully opaque)
        var col = SelectedColor; col.a = 1f;

        // ----- Paint live cells precisely -----
        foreach (var cc in p.cells)
        {
            // Convert cell to 0..cols-1 / 0..rows-1 coords (top-down Y)
            int x = (cc.x - minX) + previewPadding;
            int y = (cc.y - minY) + previewPadding;
            int rowFromTop = (rows - 1 - y); // invert because UI is top-down

            var live = Instantiate(previewCellPrefab, patternPreviewRoot);
            var img = live.GetComponent<Image>();
            if (img) img.color = col;

            var childRT = (RectTransform)live.transform;
            childRT.anchorMin = childRT.anchorMax = childRT.pivot = new Vector2(0.5f, 0.5f);  // center
            childRT.sizeDelta = new Vector2(cellSize, cellSize);
            childRT.anchoredPosition = CellTopLeft(x, rowFromTop);
        }
    }

}
