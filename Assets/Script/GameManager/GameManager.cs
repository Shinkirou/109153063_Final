using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager
{
    // 全域靜態變數：任何場景的任何腳本都能直接讀取它！
    // 預設值為 1.0 (普通)
    public static float DifficultyMultiplier = 1.0f;

    // 開始遊戲：設定難度並載入 level01
    public static void StartGame(float difficulty)
    {
        DifficultyMultiplier = difficulty;
        Time.timeScale = 1f; // 確保遊戲時間是正常的 (避免暫停殘留)

        // 核心：直接載入你的主遊戲場景
        SceneManager.LoadScene("level01");
    }

    // 回到主選單 (之後死亡或按返回可以用)
    public static void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Start");
    }
}
