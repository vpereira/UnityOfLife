using UnityEngine;
using System.Collections;

/// <summary>
/// Reads Unity input each frame and exposes one-shot flags + buffered modifiers.
/// Call Poll() from Update(), read flags, then call ClearOneShot().
/// </summary>
public class InputManager
{
    // Buffered modifiers
    public int RepeatCount { get; private set; } = 1;
    public bool UseRandomColor { get; private set; }
    public bool UseRandomPattern { get; private set; }

    // One-shot actions (true only for the frame you read them, then ClearOneShot)
    public bool ToggleGrid { get; private set; }
    public bool ToggleUI { get; private set; }
    public bool ToggleWrap { get; private set; }
    public bool TogglePlacement { get; private set; }
    public bool SpawnRequested { get; private set; }
    public bool PlacementClick { get; private set; }   // LMB
    public bool PlacementCancel { get; private set; }   // Esc or RMB

    // Combo timeout
    [SerializeField] private float inputTimeout = 1.5f;
    private float lastInputTime = -Mathf.Infinity;

    public void Poll()
    {
        // timeout numeric/modifier buffer
        if (Time.time - lastInputTime > inputTimeout)
            ResetState();

        // --- digits 0..9 (buffer for RepeatCount) ---
        for (KeyCode key = KeyCode.Alpha0; key <= KeyCode.Alpha9; key++)
        {
            if (Input.GetKeyDown(key))
            {
                int digit = key - KeyCode.Alpha0;
                if (!(digit == 0 && RepeatCount == 1)) // ignore leading zero
                {
                    if (RepeatCount == 1) RepeatCount = 0;
                    RepeatCount = RepeatCount * 10 + digit;
                    lastInputTime = Time.time;
                }
                return; // only one digit per frame
            }
        }

        // --- modifiers (buffered until command fires or timeout) ---
        if (Input.GetKeyDown(KeyCode.C)) { UseRandomColor = true; lastInputTime = Time.time; }
        if (Input.GetKeyDown(KeyCode.P)) { UseRandomPattern = true; lastInputTime = Time.time; }

        // --- one-shot toggles/commands ---
        if (Input.GetKeyDown(KeyCode.G)) ToggleGrid = true;
        if (Input.GetKeyDown(KeyCode.V)) ToggleUI = true;
        if (Input.GetKeyDown(KeyCode.W)) ToggleWrap = true;
        if (Input.GetKeyDown(KeyCode.K)) TogglePlacement = true;

        if (Input.GetKeyDown(KeyCode.R))
        {
            SpawnRequested = true;
            if (RepeatCount < 1) RepeatCount = 1;
        }

        // Placement actions
        if (Input.GetMouseButtonDown(0)) PlacementClick = true;
        if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape)) PlacementCancel = true;
    }

    /// <summary>Clear only one-shot flags after GameManager processes them.</summary>
    public void ClearOneShot()
    {
        ToggleGrid = ToggleUI = ToggleWrap = TogglePlacement = false;
        SpawnRequested = false;
        PlacementClick = PlacementCancel = false;
    }

    /// <summary>Reset numeric/modifier buffer (call after executing R, or on timeout).</summary>
    public void ResetState()
    {
        RepeatCount = 1;
        UseRandomColor = false;
        UseRandomPattern = false;
        lastInputTime = -Mathf.Infinity;
    }
}
