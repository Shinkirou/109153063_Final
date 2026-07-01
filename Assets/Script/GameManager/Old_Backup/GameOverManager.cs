using UnityEngine;
using TMPro;

/*
public class GameOverManager : MonoBehaviour
{
    [Header("遊戲結束UI")]
    public TextMeshProUGUI finalLevelText;
    public TextMeshProUGUI finalScoreText;
    public TextMeshProUGUI survivalTimeText;

    private float survivalStartTime;

    void Start()
    {
        survivalStartTime = Time.realtimeSinceStartup;
    }

    void OnEnable()
    {
        UpdateGameOverUI();
    }

    public void UpdateGameOverUI()
    {
        PlayerLevel playerLevel = FindObjectOfType<PlayerLevel>();
        
        if (playerLevel != null && finalLevelText != null)
        {
            finalLevelText.text = "最終等級: " + playerLevel.level;
        }

        if (finalScoreText != null)
        {
            int totalExp = 0;
            if (playerLevel != null)
            {
                // 簡單計算得分：(等級 - 1) * 平均經驗需求 + 當前經驗
                totalExp = (playerLevel.level - 1) * 10 + playerLevel.currentExp;
            }
            finalScoreText.text = "總經驗: " + totalExp;
        }

        if (survivalTimeText != null)
        {
            float survivalTime = Time.realtimeSinceStartup - survivalStartTime;
            int minutes = (int)survivalTime / 60;
            int seconds = (int)survivalTime % 60;
            survivalTimeText.text = string.Format("生存時間: {0:D2}:{1:D2}", minutes, seconds);
        }
    }

    public void RestartGame()
    {
        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.RestartGame();
        }
    }

    public void GoToMenu()
    {
        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.GoToMenu();
        }
    }
}
*/

