using UnityEngine;

public class UpgradeManager : MonoBehaviour
{
    [Header("UI")]
    public GameObject upgradePanel;

    [Header("玩家")]
    public PlayerStats playerStats;

    public void ShowUpgradePanel()
    {
        upgradePanel.SetActive(true);
        Time.timeScale = 0f;
    }

    public void UpgradeDamage()
    {
        playerStats.AddDamage(1);

        CloseUpgradePanel();
    }

    public void UpgradeAttackSpeed()
    {
        playerStats.MultiplyAttackSpeed(0.9f);

        CloseUpgradePanel();
    }

    public void UpgradeAttackRange()
    {
        playerStats.AddAttackRange(1f);
        CloseUpgradePanel();
    }

    public void UpgradeLuck()
    {
        playerStats.AddLuck(1);
        CloseUpgradePanel();
    }

    public void UpgradeCriticalRate()
    {
        playerStats.AddCriticalRate(0.05f);
        CloseUpgradePanel();
    }

    public void UpgradeCriticalDamage()
    {
        playerStats.AddCriticalDamage(0.25f);
        CloseUpgradePanel();
    }

    void CloseUpgradePanel()
    {
        upgradePanel.SetActive(false);
        Time.timeScale = 1f;
    }
}
