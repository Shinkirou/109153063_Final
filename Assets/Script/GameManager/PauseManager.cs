using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{

    [Header("暫停面板")]
    [Tooltip("暫停選單最外層物件")]
    public GameObject pausePanel;


    [Header("其他系統")]
    [Tooltip("用來判斷升級選卡畫面是否開啟")]
    public UpgradeManager upgradeManager;
    [Tooltip("用來處理角色資訊面板")]
    public PlayerStatsPanel playerStatsPanel;

    [Header("暫停按鍵")]
    [Tooltip("開啟或關閉暫停選單的按鍵")]
    public KeyCode pauseKey = KeyCode.Escape;

    public bool IsPaused
    {
        get
        {
            return pausePanel != null &&
                   pausePanel.activeSelf;
        }
    }

    private void Start()
    {
        //遊戲開始時確保暫停面板關閉
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }

        // 新局恢復時間；結算狀態則維持停止。
        Time.timeScale = GameManager.IsGameOver ? 0f : 1f;

        ValidateReferences();
    }


    private void Update()
    {
        // 本局結束後，不再執行遊戲操作。
        if (GameManager.IsGameOver)
        {
            return;
        }

        if (Input.GetKeyDown(pauseKey))
        {
            HandlePauseInput();
        }
    }


    private void OnDisable()
    {
        //切換場景或物件關閉時，
        //避免 Time.timeScale 停留在 0
        if (IsPaused && !GameManager.IsGameOver)
        {
            Time.timeScale = 1f;
        }
    }

    private void HandlePauseInput()
    {
        //角色資訊面板開啟時，
        //Esc 優先關閉角色面板
        if (playerStatsPanel != null &&
            playerStatsPanel.IsPanelOpen)
        {
            playerStatsPanel.ClosePanel();
            return;
        }

        //正在升級選卡時，不開啟一般暫停選單
        if (upgradeManager != null &&
            upgradeManager.IsUpgradePanelOpen)
        {
            return;
        }

        //一般狀態下切換暫停
        TogglePause();
    }

    public void TogglePause()
    {
        // 本局結束後，不再執行遊戲操作。
        if (GameManager.IsGameOver)
        {
            return;
        }

        if (IsPaused)
        {
            ResumeGame();
        }
        else
        {
            PauseGame();
        }
    }


    public void PauseGame()
    {
        // 本局結束後，不再執行遊戲操作。
        if (GameManager.IsGameOver)
        {
            return;
        }

        if (pausePanel == null)
        {
            Debug.LogError(
                "PauseManager 尚未指定 Pause Panel。"
            );

            return;
        }

        //避免角色面板和暫停面板同時顯示
        if (playerStatsPanel != null)
        {
            playerStatsPanel.ClosePanel();
        }

        pausePanel.SetActive(true);
        Time.timeScale = 0f;
    }


    public void ResumeGame()
    {
        // 本局結束後，不再執行遊戲操作。
        if (GameManager.IsGameOver)
        {
            return;
        }

        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }

        /*
         * 正常情況下，暫停面板無法與升級面板同時開啟。
         * 這裡仍保留判斷作為防呆。
         */
        if (upgradeManager != null &&
            upgradeManager.IsUpgradePanelOpen)
        {
            Time.timeScale = 0f;
            return;
        }

        Time.timeScale = 1f;
    }

    public void RestartGame()
    {
        GameManager.RestartCurrentScene();
    }


    public void ReturnToMainMenu()
    {
        // 場景確認可載入後，再由 GameManager 重設本局狀態。
        GameManager.GoToMainMenu();
    }

    private void ValidateReferences()
    {
        if (pausePanel == null)
        {
            Debug.LogWarning(
                "PauseManager：尚未指定 Pause Panel。"
            );
        }

        if (upgradeManager == null)
        {
            Debug.LogWarning(
                "PauseManager：尚未指定 UpgradeManager。"
            );
        }

        if (playerStatsPanel == null)
        {
            Debug.LogWarning(
                "PauseManager：尚未指定 PlayerStatsPanel。"
            );
        }
    }
}