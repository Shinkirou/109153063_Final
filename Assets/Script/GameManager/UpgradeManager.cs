using UnityEngine;

public class UpgradeManager : MonoBehaviour
{
    [Header("UI")]
    public GameObject upgradePanel;

    [Header("玩家")]
    public PlayerAttack playerAttack;

    public void ShowUpgradePanel()
    {
        upgradePanel.SetActive(true);
        Time.timeScale = 0f;
    }

    public void UpgradeDamage()
    {
        playerAttack.damage += 1;

        CloseUpgradePanel();
    }

    public void UpgradeAttackSpeed()
    {
        playerAttack.attackInterval *= 0.9f;

        CloseUpgradePanel();
    }

    public void UpgradeAttackRange()
    {
        playerAttack.attackRange += 1f;

        CloseUpgradePanel();
    }

    void CloseUpgradePanel()
    {
        upgradePanel.SetActive(false);
        Time.timeScale = 1f;
    }
}
