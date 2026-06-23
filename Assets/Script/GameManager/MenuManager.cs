using UnityEngine;
using TMPro;

public class MenuManager : MonoBehaviour
{
    [Header("難度按鈕")]
    public TextMeshProUGUI difficultyText;

    private float selectedDifficulty = 1f;
    private string[] difficultyNames = { "簡單", "普通", "困難" };
    private float[] difficultyValues = { 0.8f, 1f, 1.2f };
    private int currentDifficultyIndex = 1;  // 默認普通

    void OnEnable()
    {
        UpdateDifficultyDisplay();
    }

    public void SelectEasyMode()
    {
        currentDifficultyIndex = 0;
        UpdateDifficultyDisplay();
    }

    public void SelectNormalMode()
    {
        currentDifficultyIndex = 1;
        UpdateDifficultyDisplay();
    }

    public void SelectHardMode()
    {
        currentDifficultyIndex = 2;
        UpdateDifficultyDisplay();
    }

    public void StartGame()
    {
        selectedDifficulty = difficultyValues[currentDifficultyIndex];
        
        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.StartGame(selectedDifficulty);
        }
    }

    public void QuitGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

    private void UpdateDifficultyDisplay()
    {
        if (difficultyText != null)
        {
            difficultyText.text = "難度: " + difficultyNames[currentDifficultyIndex];
        }
    }
}
