using System.Collections.Generic;
using UnityEngine;

public class UpgradeManager : MonoBehaviour
{
    //當次升級抽到的卡
    private List<UpgradeCard> currentCards = new List<UpgradeCard>();

    [Header("UI")]
    public GameObject upgradePanel;

    [Header("玩家")]
    public PlayerStats playerStats;

    public void ShowUpgradePanel()
    {
        
        GenerateUpgradeCards();             //製作牌的方法

        //打開升級面板，暫停遊戲
        upgradePanel.SetActive(true);
        Time.timeScale = 0f;
    }

    //抽三張牌的方法
    public void GenerateUpgradeCards()
    {
        currentCards.Clear();               //將上一輪升級卡牌清空

        for (int i = 0; i < 3; i++)
        {
            UpgradeCard card = CreateRandomCard();  //製造隨機卡牌方法
            currentCards.Add(card);                 //將製造好的卡牌存入清單中

            Debug.Log($"第 {i + 1} 張卡：{card.rarity} / {card.cardName} / {card.description}");
        }
    }

    //隨機產生單張卡
    UpgradeCard CreateRandomCard()
    {
        UpgradeType type = GetRandomUpgradeType();  //抽加成屬性
        CardRarity rarity = GetRandomRarity();      //抽稀有度

        //將抽完的稀有度組合並回傳
        return CreateCardByTypeAndRarity(type, rarity);
    }


    //隨機能力類型
    UpgradeType GetRandomUpgradeType()
    {
        //計算UpgradeType的長度，並隨機選一個數字決定抽到的能力
        int typeCount = System.Enum.GetValues(typeof(UpgradeType)).Length;
        int randomIndex = Random.Range(0, typeCount);

        return (UpgradeType)randomIndex;
    }

    //決定卡牌品質機率
    CardRarity GetRandomRarity()
    {
        int luck = 0;

        if (playerStats != null)
        {
            luck = playerStats.luck;
        }

        //計算每種稀有度出現的「基礎機率 %」 + 「幸運值加成 %」...不排除未來會進行修改
        float legendaryChance = 2f + luck * 0.2f;
        float epicChance = 8f + luck * 0.4f;
        float rareChance = 20f + luck * 0.8f;
        float mythicChance = 0.5f + luck * 0.1f;

        // 使用 Mathf.Clamp 限制機率的上下限，
        mythicChance = Mathf.Clamp(mythicChance, 0.5f, 10f);       // 神話最高 10%
        legendaryChance = Mathf.Clamp(legendaryChance, 2f, 20f);   // 傳說最高 20%
        epicChance = Mathf.Clamp(epicChance, 8f, 35f);             // 史詩最高 35%
        rareChance = Mathf.Clamp(rareChance, 20f, 60f);            // 稀有最高 60%

        //抽一個數字決定稀有度
        float roll = Random.Range(0f, 100f);

        //這邊簡單說，抽到的數字越大，就越不稀有
        if (roll < mythicChance)
        {
            return CardRarity.Mythic;
        }

        if (roll < mythicChance + legendaryChance)
        {
            return CardRarity.Legendary;
        }

        if (roll < mythicChance + legendaryChance + epicChance)
        {
            return CardRarity.Epic;
        }

        if (roll < mythicChance + legendaryChance + epicChance + rareChance)
        {
            return CardRarity.Rare;
        }

        return CardRarity.Common;
    }

    //依照上面已決定好的稀有度 類型，得到的回傳值來製作卡片，組裝階段
    UpgradeCard CreateCardByTypeAndRarity(UpgradeType type, CardRarity rarity)
    {
        //依據稀有度獲得卡牌數值
        float flatValue = GetFlatValueByRarity(rarity);
        float multiplierValue = GetMultiplierValueByRarity(rarity);
        //轉換中文名稱
        string rarityName = GetRarityName(rarity);
        string typeName = GetUpgradeTypeName(type);
        //動態產生中文敘述
        string cardName = $"{rarityName}{typeName}";
        string description = CreateDescription(type, flatValue, multiplierValue);

        //生出這張 UpgradeCard 的實體物件並回傳
        return new UpgradeCard(
            cardName,
            description,
            type,
            rarity,
            flatValue,
            multiplierValue
        );
    }

    //決定不同稀有度要加多少固定數值
    float GetFlatValueByRarity(CardRarity rarity)
    {
        switch (rarity)
        {
            case CardRarity.Common:
                return 1f;

            case CardRarity.Rare:
                return 2f;

            case CardRarity.Epic:
                return 3f;

            case CardRarity.Legendary:
                return 5f;

            case CardRarity.Mythic:
                return 3f;

            default:
                return 1f;
        }
    }

    //只有神話才享有的百分比加成
    float GetMultiplierValueByRarity(CardRarity rarity)
    {
        if (rarity == CardRarity.Mythic)
        {
            return 0.05f;
        }

        return 0f;
    }

    //中文命名
    string GetRarityName(CardRarity rarity)
    {
        switch (rarity)
        {
            case CardRarity.Common:
                return "普通";

            case CardRarity.Rare:
                return "稀有";

            case CardRarity.Epic:
                return "史詩";

            case CardRarity.Legendary:
                return "傳說";

            case CardRarity.Mythic:
                return "神話";

            default:
                return "未知";
        }
    }

    //把程式碼的英文屬性 UpgradeType 變成中文
    string GetUpgradeTypeName(UpgradeType type)
    {
        switch (type)
        {
            case UpgradeType.Damage:
                return "攻擊力";

            case UpgradeType.AttackRange:
                return "攻擊距離";

            case UpgradeType.AttackSpeed:
                return "攻擊速度";

            case UpgradeType.MaxHP:
                return "最大生命";

            case UpgradeType.MoveSpeed:
                return "移動速度";

            case UpgradeType.Luck:
                return "幸運值";

            case UpgradeType.CriticalRate:
                return "暴擊率";

            case UpgradeType.CriticalDamage:
                return "暴擊傷害";

            case UpgradeType.LifeSteal:
                return "吸血";

            case UpgradeType.Armor:
                return "防禦";

            default:
                return "未知能力";
        }
    }

    //卡牌詳細敘述
    string CreateDescription(UpgradeType type, float flatValue, float multiplierValue)
    {
        string typeName = GetUpgradeTypeName(type);

        if (multiplierValue > 0f)
        {
            return $"{typeName} +{flatValue}，並額外提升 {multiplierValue * 100f}%";
        }

        return $"{typeName} +{flatValue}";
    }


    //老加成攻擊力公式
    /*
    //測試用
    public void TestNormalDamageCard()
        {
            playerStats.AddFlatDamage(5);
        }

        public void TestMythicDamageCard()
        {
            playerStats.AddFlatDamage(3);
            playerStats.AddDamageMultiplier(0.05f);
        }

    
     /
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

    */

    void CloseUpgradePanel()
    {
        upgradePanel.SetActive(false);
        Time.timeScale = 1f;
    }
}
