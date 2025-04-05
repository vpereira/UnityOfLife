using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    [SerializeField] private TMP_Text aliveCellsText;
    [SerializeField] private Grid grid;

    private GameManager gameManager;

    private int aliveCellsCount = 0;

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
    }

    // Update is called once per frame
    void Update()
    {
        if (aliveCellsCount != gameManager.GetAliveCellsCount())
        {
            aliveCellsCount = gameManager.GetAliveCellsCount();
            UpdateAliveCellsText();
        }        
    }
    private void UpdateAliveCellsText()
    {
        aliveCellsText.text = "Alive Cells: " + aliveCellsCount;
    }
}
