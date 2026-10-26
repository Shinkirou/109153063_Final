using UnityEngine;
using UnityEngine.SceneManagement;

public enum GamePhase
{
    Development,
    BossBattle, // 保留給後續 Boss 流程；本次未實作轉換。
    Victory,    // 保留給後續勝利流程；本次未實作轉換。
    Defeat
}

public enum DefeatReason
{
    None,
    PlayerDeath,
    BossTimeout // 接入點已保留；本次沒有加入 Boss 計時器。
}

// 保留原本的普通類別與靜態 API，不需要掛到場景物件。
public class GameManager
{
    public static float DifficultyMultiplier = 1.0f;
    public static GamePhase CurrentPhase { get; private set; } = GamePhase.Development;
    public static DefeatReason LastDefeatReason { get; private set; } = DefeatReason.None;
    public static bool IsGameOver =>
        CurrentPhase == GamePhase.Defeat || CurrentPhase == GamePhase.Victory;

    // 即使編輯器關閉 Domain Reload，也不要帶入上次 Play 的失敗狀態。
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetRuntimeState()
    {
        DifficultyMultiplier = 1f;
        ResetRun();
    }

    private static void ResetRun()
    {
        CurrentPhase = GamePhase.Development;
        LastDefeatReason = DefeatReason.None;
        Time.timeScale = 1f;
    }

    public static void Defeat(DefeatReason reason)
    {
        // 同一局只接受一次結束，避免重複建立結算畫面。
        if (IsGameOver)
        {
            return;
        }
        if (reason != DefeatReason.PlayerDeath && reason != DefeatReason.BossTimeout)
        {
            Debug.LogError("GameManager.Defeat 需要有效的失敗原因。");
            return;
        }

        // 先記錄狀態，其他面板關閉時才不會恢復遊戲時間。
        CurrentPhase = GamePhase.Defeat;
        LastDefeatReason = reason;
        Time.timeScale = 0f;
        Debug.Log("本局失敗：" + reason);
        RunResultUI.Show(reason);
    }

    public static void StartGame(float difficulty)
    {
        if (!Application.CanStreamedLevelBeLoaded("level01"))
        {
            Debug.LogError("無法載入 level01，請確認場景已加入目前 Build Profile 的 Scene List。");
            return;
        }
        DifficultyMultiplier = difficulty;
        ResetRun();
        SceneManager.LoadScene("level01");
    }

    public static void RestartCurrentScene()
    {
        Scene scene = SceneManager.GetActiveScene();
        if (Application.CanStreamedLevelBeLoaded(scene.path))
        {
            // 重開保留難度；生命、經驗與場景物件由重新載入初始化。
            ResetRun();
            SceneManager.LoadScene(scene.path);
            return;
        }
#if UNITY_EDITOR
        // 從編輯器直接測試尚未加入 Scene List 的已儲存場景。
        if (!string.IsNullOrEmpty(scene.path))
        {
            ResetRun();
            UnityEditor.SceneManagement.EditorSceneManager.LoadSceneInPlayMode(
                scene.path, new LoadSceneParameters(LoadSceneMode.Single));
            return;
        }
#endif
        // 無法重開時維持結算狀態，不把死亡中的遊戲恢復。
        Debug.LogError("無法重新開始。請先儲存場景，並將場景加入目前 Build Profile 的 Scene List。");
    }

    public static void GoToMainMenu()
    {
        if (!Application.CanStreamedLevelBeLoaded("Start"))
        {
            Debug.LogError("無法載入 Start，請確認場景已加入目前 Build Profile 的 Scene List。");
            return;
        }
        ResetRun();
        SceneManager.LoadScene("Start");
    }
}
