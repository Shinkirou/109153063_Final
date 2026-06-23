using UnityEngine;
using TMPro;

public class MenuManager : MonoBehaviour
{
    [Header("UI 面板設定")]
    public GameObject mainMenuPanel;      // 主選單面板 (包含標題、開始、結束)
    public GameObject difficultyPanel;    // 難度選擇面板 (包含四個難度相關按鈕)
    public GameObject howToPlayPanel;     // 玩法介紹面板 (選填，如果想做的話)

    private float[] difficultyValues = { 1f, 1.5f, 2.25f }; // 簡單=0.8, 普通=1, 困難=1.2

    void Start()
    {
        // 遊戲一開始，確保只顯示主選單，其餘隱藏
        ShowMainMenu();
    }

    // --- 面板切換功能 ---

    // 顯示主選單，隱藏難度選單
    public void ShowMainMenu()
    {
        if (mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(true);
        }
        if (difficultyPanel != null)
        {
            difficultyPanel.SetActive(false);
        }
        if (howToPlayPanel != null)
        {
            howToPlayPanel.SetActive(false);
        }
    }

    // 顯示難度選單，隱藏主選單
    public void ShowDifficultyMenu()
    {
        if (mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(false);
        }
        if (difficultyPanel != null)
        {
            difficultyPanel.SetActive(true);
        }
    }

    // 打開玩法介紹
    public void OpenHowToPlay()
    {
        if (howToPlayPanel != null)
        {
            howToPlayPanel.SetActive(true);
        }
    }

    // 關閉玩法介紹
    public void CloseHowToPlay()
    {
        if (howToPlayPanel != null) 
        {
            howToPlayPanel.SetActive(false);
        }
    }

    // --- 難度按鈕點擊事件 (點擊後直接帶入數值並開始遊戲) ---

    public void SelectEasyMode()
    {
        GameManager.StartGame(1f); // 簡單
    }

    public void SelectNormalMode()
    {
        GameManager.StartGame(1.5f); // 普通
    }

    public void SelectHardMode()
    {
        GameManager.StartGame(2.25f); // 困難
    }

    // --- 結束遊戲 ---
    public void QuitGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false; // 在編輯器內停止遊玩
        #else
            Application.Quit(); // 正式打包後關閉遊戲
        #endif
    }
}
