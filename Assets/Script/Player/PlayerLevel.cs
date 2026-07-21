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

    [Header("升級難度曲線")]
    public AnimationCurve experienceCurve;

    [Header("升級系統")]
    public UpgradeManager upgradeManager;


    void Start()
    {
        UpdateExpRequirement();
        UpdateLevelUI();
    }

    //測試經驗值使用
    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            AddExp(3);
        }
    }

    public int GetSuperGemExpAmount()
    {
        return Mathf.Max(
            Mathf.RoundToInt(
                expToNextLevel * 0.25f
            ),
            1
        );
    }

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
        UpdateExpRequirement();

        Debug.Log("升級！目前等級：" + level);

        if (upgradeManager != null)
        {
            upgradeManager.ShowUpgradePanel();
        }
    }
    void UpdateExpRequirement()
    {
        // 確保你有在 Inspector 丟入曲線，且裡面有點
        if (experienceCurve != null && experienceCurve.length > 0)
        {
            // 讀取曲線在該等級（X軸）對應的經驗值（Y軸）
            expToNextLevel = Mathf.RoundToInt(experienceCurve.Evaluate(level));
        }
        else
        {
            // 防呆保底：如果你忘記拉曲線，就用舊的公式（每級+5）避免遊戲壞掉
            expToNextLevel = 5 + (level - 1) * 5;
        }
    }
}