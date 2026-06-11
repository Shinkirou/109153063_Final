using UnityEngine;

public class PlayerLevel : MonoBehaviour
{
    [Header("玩家等級")]
    public int level = 1;               //當前等級
    public int currentExp = 0;          //當前經驗值
    public int expToNextLevel = 5;      //當前升級所需經驗值  

    [Header("升級系統")]
    public UpgradeManager upgradeManager;

    public void AddExp(int amount)
    {
        currentExp += amount;

        Debug.Log("獲得經驗：" + amount + "，目前經驗：" + currentExp);

        if (currentExp >= expToNextLevel)
        {
            LevelUp();
        }
    }

    void LevelUp()
    {
        level++;
        currentExp -= expToNextLevel;
        expToNextLevel += 5;

        Debug.Log("升級！目前等級：" + level);

        if (upgradeManager != null)
        {
            upgradeManager.ShowUpgradePanel();
        }
    }
}