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
    [SerializeField] private Image colorSwatchImage;
    [SerializeField] private RectTransform patternPreviewRoot; // has GridLayoutGroup
    [SerializeField] private Image previewCellPrefab;      // 1x1 white sprite Image prefab
    [SerializeField] private int previewPadding = 1;
    [SerializeField] private int previewCellSize = 12;

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
        if (colorSwatchImage)
        {
            var c = SelectedColor;
            c.a = 1f; // Ensure full opacity for swatch
            colorSwatchImage.color = c;
        }
        RebuildPreview();
    }

    private void RebuildPreview()
    {
        if (!patternPreviewRoot) return;

        for (int i = patternPreviewRoot.childCount - 1; i >= 0; i--)
            Destroy(patternPreviewRoot.GetChild(i).gameObject);

        var p = SelectedPattern;
        if (p == null || p.cells == null || p.cells.Length == 0 || !previewCellPrefab) return;

        int minX = p.cells[0].x, maxX = p.cells[0].x;
        int minY = p.cells[0].y, maxY = p.cells[0].y;
        foreach (var c in p.cells) { if (c.x < minX) minX = c.x; if (c.x > maxX) maxX = c.x; if (c.y < minY) minY = c.y; if (c.y > maxY) maxY = c.y; }

        int width = (maxX - minX + 1);
        int height = (maxY - minY + 1);
        var grid = patternPreviewRoot.GetComponent<GridLayoutGroup>();
        if (grid)
        {
            grid.cellSize = new Vector2(previewCellSize, previewCellSize);
            grid.spacing = Vector2.zero;
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = width + previewPadding * 2;
            grid.startAxis = GridLayoutGroup.Axis.Vertical;
        }

        int cols = width + previewPadding * 2;
        int rows = height + previewPadding * 2;
        int total = cols * rows;

        for (int i = 0; i < total; i++)
        {
            var bg = Instantiate(previewCellPrefab, patternPreviewRoot);
            bg.color = new Color(0, 0, 0, 0);
        }

        foreach (var cell in p.cells)
        {
            int x = (cell.x - minX) + previewPadding;
            int y = (cell.y - minY) + previewPadding;
            int idx = x * rows + (rows - 1 - y);
            var img = patternPreviewRoot.GetChild(idx).GetComponent<Image>();
            img.color = SelectedColor;
        }
    }
}
