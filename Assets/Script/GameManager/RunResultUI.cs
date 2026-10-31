using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class RunResultUI : MonoBehaviour
{
    [Header("場景中的結算 UI")]
    public GameObject resultPanel;
    public TMP_Text titleText;
    public TMP_Text reasonText;

    private void Awake()
    {
        // 遊戲開始時隱藏結算面板。
        if (resultPanel != null)
        {
            resultPanel.SetActive(false);
        }
    }

    // 保留 GameManager 原本呼叫的入口。
    public static void Show(DefeatReason reason)
    {
        RunResultUI ui =
            Object.FindAnyObjectByType<RunResultUI>();

        if (ui == null)
        {
            Debug.LogError(
                "找不到 RunResultUI，請將腳本掛在啟用中的 Canvas_RunResult。"
            );
            return;
        }

        ui.ShowResult(reason);
    }

    private void ShowResult(DefeatReason reason)
    {
        if (resultPanel == null)
        {
            Debug.LogError("RunResultUI 尚未指定 Result Panel。");
            return;
        }

        // 關閉可能遮住結算畫面的其他面板。
        foreach (UpgradeManager manager in
            Object.FindObjectsByType<UpgradeManager>())
        {
            if (manager.upgradePanel != null)
            {
                manager.upgradePanel.SetActive(false);
            }
        }

        foreach (PauseManager manager in
            Object.FindObjectsByType<PauseManager>())
        {
            if (manager.pausePanel != null)
            {
                manager.pausePanel.SetActive(false);
            }
        }

        foreach (PlayerStatsPanel controller in
            Object.FindObjectsByType<PlayerStatsPanel>())
        {
            controller.ClosePanel();
        }

        if (titleText != null)
        {
            titleText.text = "挑戰失敗";
        }

        if (reasonText != null)
        {
            reasonText.text =
                reason == DefeatReason.PlayerDeath
                ? "失敗原因：角色死亡"
                : "失敗原因：Boss 討伐超時";
        }

        resultPanel.SetActive(true);

        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 0f;
    }

    // 給「重新開始」按鈕呼叫。
    public void OnRestartClicked()
    {
        GameManager.RestartCurrentScene();
    }

    // 給「回到主畫面」按鈕呼叫。
    public void OnMainMenuClicked()
    {
        GameManager.GoToMainMenu();
    }
}