using UnityEngine;
using UnityEngine.SceneManagement;

public class GameStateManager : MonoBehaviour
{
    public enum GameState
    {
        Menu,           // 開始菜單
        Playing,        // 遊戲中
        Paused,         // 暫停
        GameOver        // 遊戲結束
    }

    public static GameStateManager Instance { get; private set; }

    private GameState currentState = GameState.Menu;
    public GameState CurrentState => currentState;

    [Header("UI面板")]
    public GameObject menuPanel;
    public GameObject pausePanel;
    public GameObject gameOverPanel;

    [Header("遊戲設定")]
    public float gameDifficulty = 1f;  // 難度倍數 (簡單=0.8, 普通=1, 困難=1.2)

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        SetGameState(GameState.Menu);
    }

    void Update()
    {
        // ESC鍵暫停/取消暫停
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (currentState == GameState.Playing)
            {
                Pause();
            }
            else if (currentState == GameState.Paused)
            {
                Resume();
            }
        }
    }

    public void SetGameState(GameState newState)
    {
        if (currentState == newState) return;

        currentState = newState;

        switch (currentState)
        {
            case GameState.Menu:
                ShowMenuPanel();
                Time.timeScale = 1f;
                break;

            case GameState.Playing:
                HideAllPanels();
                Time.timeScale = 1f;
                break;

            case GameState.Paused:
                ShowPausePanel();
                Time.timeScale = 0f;
                break;

            case GameState.GameOver:
                ShowGameOverPanel();
                Time.timeScale = 0f;
                break;
        }
    }

    public void StartGame(float difficulty = 1f)
    {
        gameDifficulty = difficulty;
        SetGameState(GameState.Playing);
    }

    public void Pause()
    {
        if (currentState == GameState.Playing)
        {
            SetGameState(GameState.Paused);
        }
    }

    public void Resume()
    {
        if (currentState == GameState.Paused)
        {
            SetGameState(GameState.Playing);
        }
    }

    public void EndGame()
    {
        SetGameState(GameState.GameOver);
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GoToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void ShowMenuPanel()
    {
        if (menuPanel != null) menuPanel.SetActive(true);
        if (pausePanel != null) pausePanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
    }

    private void ShowPausePanel()
    {
        if (pausePanel != null) pausePanel.SetActive(true);
        if (menuPanel != null) menuPanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
    }

    private void ShowGameOverPanel()
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
        if (menuPanel != null) menuPanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(false);
    }

    private void HideAllPanels()
    {
        if (menuPanel != null) menuPanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
    }
}
