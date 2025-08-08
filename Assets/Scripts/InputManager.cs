using UnityEngine;

/// <summary>
/// Simple input manager to buffer number keys and support modifier keys like C and P.
/// Call Update() every frame from GameManager, and use ShouldTriggerCommand() to detect command triggers.
/// </summary>
public class InputManager
{
    public int RepeatCount { get; private set; } = 1;
    public bool UseRandomColor { get; private set; } = false;
    public bool UseRandomPattern { get; private set; } = false;

    private float inputTimeout = 1.5f;
    private float lastInputTime = -Mathf.Infinity;

    public void Update()
    {
        // Timeout handling
        if (Time.time - lastInputTime > inputTimeout)
        {
            ResetState();
        }

        // Detect number input (0–9)
        for (KeyCode key = KeyCode.Alpha0; key <= KeyCode.Alpha9; key++)
        {
            if (Input.GetKeyDown(key))
            {
                int digit = key - KeyCode.Alpha0;
                // Ignore leading zeros
                if (digit == 0 && RepeatCount == 1) return;

                if (RepeatCount == 1) RepeatCount = 0;
                RepeatCount = RepeatCount * 10 + digit;
                lastInputTime = Time.time;
                return;
            }
        }

        // Detect modifier keys
        if (Input.GetKeyDown(KeyCode.C))
        {
            UseRandomColor = true;
            lastInputTime = Time.time;
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            UseRandomPattern = true;
            lastInputTime = Time.time;
        }
    }

    /// <summary>
    /// Call this from GameManager to check if a trigger key was pressed (e.g., R, G, V).
    /// </summary>
    public bool ShouldTriggerCommand(KeyCode commandKey)
    {
        return Input.GetKeyDown(commandKey);
    }

    /// <summary>
    /// Reset state after executing the command or after timeout.
    /// </summary>
    public void ResetState()
    {
        RepeatCount = 1;
        UseRandomColor = false;
        UseRandomPattern = false;
        lastInputTime = -Mathf.Infinity;
    }
}
