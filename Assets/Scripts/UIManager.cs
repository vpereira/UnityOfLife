using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class UIManager : MonoBehaviour
{
    [SerializeField] private TMP_Text aliveCellsText;
    [SerializeField] private TMP_Text fpsText;
    [SerializeField] private Grid grid;
    [SerializeField] private TMP_Text generationText;


    private GameManager gameManager;

    private int aliveCellsCount = 0;

    private int FramesPerSec { get; set; }
    [SerializeField] private float frequency = 0.5f;

    void Awake()
    {
        if (gameManager == null)
        {
            gameManager = grid.GetComponent<GameManager>();
            if (gameManager == null)
            {
                Debug.LogError("GameManager not found in the scene.");
            }
        }
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
       aliveCellsCount = gameManager.GetAliveCellsCount();
       UpdateAliveCellsText();
       StartCoroutine(UpdateFPS()); 
       StartCoroutine(UpdateAliveCells());
    }

    private void UpdateAliveCellsText()
    {
        aliveCellsText.text = "Alive Cells: " + ((byte)aliveCellsCount).ToString();
    }

    private void UpdateGenerationText()
    {
        generationText.text = "Generation: " + gameManager.Generation.ToString();
    }

    private IEnumerator UpdateAliveCells()
    {
        while (true)
        {
            yield return new WaitForSeconds(frequency);
            aliveCellsCount = gameManager.GetAliveCellsCount();
            UpdateAliveCellsText();
            UpdateGenerationText();
        }
    }

    private IEnumerator UpdateFPS()
    {
        while (true)
        {
            int lastFrameCount = Time.frameCount;
            float lastTime = Time.realtimeSinceStartup;
            yield return new WaitForSeconds(frequency);

            float timeSpan = Time.realtimeSinceStartup - lastTime;
            int frameCount = Time.frameCount - lastFrameCount;

            FramesPerSec = Mathf.RoundToInt(frameCount / timeSpan);
            fpsText.text = "FPS: " + FramesPerSec.ToString();
        }
    }
}
