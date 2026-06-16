using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerLevel : MonoBehaviour
{
    [Header("玩家等級")]
    public int level = 1;               //當前等級
    public int currentExp = 0;          //當前經驗值
    public int expToNextLevel = 5;      //當前升級所需經驗值  

    [Header("升級面板")]
    public TMP_Text levelText;
    public TMP_Text expText;
    public Image expFillImage;

    [Header("升級系統")]
    public UpgradeManager upgradeManager;

    public void AddExp(int amount)
    {
        currentExp += amount;

        while (currentExp >= expToNextLevel)
        {
            LevelUp();
        }

        UpdateLevelUI();
    }

    void UpdateLevelUI()
    {
        if (levelText != null)
        {
            levelText.text = "Lv." + level;
        }

        if (expText != null)
        {
            expText.text = "EXP:" + currentExp + " / " + expToNextLevel;
        }

        if (expFillImage != null)
        {
            expFillImage.fillAmount = (float)currentExp / expToNextLevel;
        }
    }

    void LevelUp()
    {
        currentExp -= expToNextLevel;
        level++;

        expToNextLevel += 5;

        Debug.Log("升級！目前等級：" + level);

        if (upgradeManager != null)
        {
            upgradeManager.ShowUpgradePanel();
        }
    }
}