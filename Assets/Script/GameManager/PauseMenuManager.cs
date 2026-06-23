using UnityEngine;
using TMPro;

public class PauseMenuManager : MonoBehaviour
{
    [Header("升級UI")]
    public GameObject upgradePanel;

    [Header("暫停菜單按鈕")]
    public GameObject pauseButtonsPanel;

    [Header("玩家")]
    public PlayerAttack playerAttack;
    public PlayerLevel playerLevel;

    private bool isShowingUpgrades = false;

    void OnEnable()
    {
        // 當暫停菜單激活時，隱藏升級面板，顯示暫停按鈕
        if (upgradePanel != null) upgradePanel.SetActive(false);
        if (pauseButtonsPanel != null) pauseButtonsPanel.SetActive(true);
    }

    public void ShowUpgradeOptions()
    {
        isShowingUpgrades = true;
        if (upgradePanel != null) upgradePanel.SetActive(true);
        if (pauseButtonsPanel != null) pauseButtonsPanel.SetActive(false);
    }

    public void HideUpgradeOptions()
    {
        isShowingUpgrades = false;
        if (upgradePanel != null) upgradePanel.SetActive(false);
        if (pauseButtonsPanel != null) pauseButtonsPanel.SetActive(true);
    }

    public void UpgradeDamage()
    {
        if (playerAttack != null)
        {
            playerAttack.damage += 1;
        }
        HideUpgradeOptions();
    }

    public void UpgradeAttackSpeed()
    {
        if (playerAttack != null)
        {
            playerAttack.attackInterval *= 0.9f;
        }
        HideUpgradeOptions();
    }

    public void UpgradeAttackRange()
    {
        if (playerAttack != null)
        {
            playerAttack.attackRange += 1f;
        }
        HideUpgradeOptions();
    }

    public void Resume()
    {
        HideUpgradeOptions();
        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.Resume();
        }
    }

    public void GoToMenu()
    {
        Time.timeScale = 1f;
        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.GoToMenu();
        }
    }
}
