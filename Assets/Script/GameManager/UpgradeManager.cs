using System.Collections.Generic;
using TMPro;
using UnityEditor.VersionControl;
using UnityEngine;

public class UpgradeManager : MonoBehaviour
{
    //當次升級抽到的卡
    private List<UpgradeCard> currentCards = new List<UpgradeCard>();

    [Header("UI")]
    public GameObject upgradePanel;

    [Header("玩家")]
    public PlayerStats playerStats;

    [Header("卡牌文字")]
    public TMP_Text cardText1;
    public TMP_Text cardText2;
    public TMP_Text cardText3;

    [Header("特殊卡設定")]
    [Range(0f, 100f)]
    public float luckCardChance = 15f;

    //處發升級，會打開升級面板，並暫停遊戲
    public void ShowUpgradePanel()
    {
        
        GenerateUpgradeCards();             //製作牌的方法
        UpdateCardUI();                     //更新UI文字

        //打開升級面板，暫停遊戲
        upgradePanel.SetActive(true);
        Time.timeScale = 0f;
    }

    //抽三張牌的方法，新增幸運卡的抽卡方式
    public void GenerateUpgradeCards()
    {
        currentCards.Clear();                                   //將上一輪升級卡牌清空

        //HashSet 不允許同一個 UpgradeType 重複存在，因此可以用來避免同一輪出現兩張相同能力卡。
        HashSet<UpgradeType> selectedTypes = new HashSet<UpgradeType>();

        // =========================
        // 1. 判定是否出現幸運牌
        float luckCardRoll = Random.Range(0f, 100f);            //每次升級只進行一次幸運牌判定
        // 15% 機率加入一張幸運牌
        if (luckCardRoll < luckCardChance)                      //這裡可以調整幸運牌的出現機率，先設為 100% 方便測試
        {
            UpgradeCard luckCard = CreateLuckCard();
            currentCards.Add(luckCard);

            Debug.Log($"本次升級抽中幸運牌，判定值：{luckCardRoll:0.00}"
            );
        }
        else
        {
            Debug.Log($"本次升級未抽中幸運牌，判定值：{luckCardRoll:0.00}"
            );
        }

        // =========================
        // 2. 用不重複屬性卡補滿三張
        while (currentCards.Count < 3)
        {
            UpgradeType randomType = GetRandomUpgradeType();    //隨機抽出一種能力

            //如果本輪已經抽過這個能力，就使用 continue 回到 while 開頭重新抽。
            if (selectedTypes.Contains(randomType))
            {
                continue;
            }
            
            selectedTypes.Add(randomType);                      //記錄這個能力已經出現過    
            CardRarity randomRarity = GetRandomRarity();        //再獨立決定這張卡的品質
            UpgradeCard attributeCard =
                CreateCardByTypeAndRarity(
                    randomType,
                    randomRarity
                );                                              //用能力類型與品質組裝卡牌

            currentCards.Add(attributeCard);
        }

        // =========================
        // 3. 打亂三張卡牌的位置
        ShuffleCurrentCards();

        //將抽到的卡牌順序打亂，避免幸運牌出現在第一格
        void ShuffleCurrentCards()
        {
            for (int i = 0; i < currentCards.Count; i++)
            {
                int randomIndex = Random.Range(i, currentCards.Count);

                UpgradeCard tempCard = currentCards[i];
                currentCards[i] = currentCards[randomIndex];
                currentCards[randomIndex] = tempCard;
            }
        }


        // =========================
        // 4. 在 Console 顯示抽卡結果
        for (int i = 0; i < currentCards.Count; i++)
        {
            UpgradeCard card = currentCards[i];

            if (card.category == CardCategory.Special)
            {
                Debug.Log(
                    $"第 {i + 1} 張卡：" +
                    $"{card.category} / " +
                    $"{card.cardName} / " +
                    $"{card.description}"
                );
            }
            else
            {
                Debug.Log(
                    $"第 {i + 1} 張卡：" +
                    $"{card.category} / " +
                    $"{card.rarity} / " +
                    $"{card.cardName} / " +
                    $"{card.description}"
                );
            }
        }
    }

    //建立特殊幸運牌
    UpgradeCard CreateLuckCard()
    {
        string cardName = "幸運牌";
        string description = "幸運等級 +1";

        return new UpgradeCard(
            cardName,
            description,
            CardCategory.Special,

            //特殊卡目前不使用這兩個欄位
            //先填入預設值，之後程式會依 category 判斷
            UpgradeType.Damage,
            CardRarity.Common,

            1f,
            0f
        );
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
        int luckLevel = 0;

        if (playerStats != null)
        {
            luckLevel = playerStats.luckLevel;
        }

        //初始權重，抽到的稀有等級按照這個比例分配，總和為 121
        float commonWeight = 81f;
        float rareWeight = 27f;
        float epicWeight = 9f;
        float legendaryWeight = 3f;
        float mythicWeight = 1f;

        //每提升 1 級 luckLevel，調整各品質權重
        commonWeight -= luckLevel * 4f;
        rareWeight += luckLevel * 2f;
        epicWeight += luckLevel * 1.2f;
        legendaryWeight += luckLevel * 0.5f;
        mythicWeight += luckLevel * 0.3f;

        //防止普通權重變成負數
        commonWeight = Mathf.Max(commonWeight, 10f);

        //計算目前所有品質的總權重
        float totalWeight =
            commonWeight +
            rareWeight +
            epicWeight +
            legendaryWeight +
            mythicWeight;

        Debug.Log(
            $"Luck Level：{luckLevel} | " +
            $"普通：{commonWeight:0.0} | " +
            $"稀有：{rareWeight:0.0} | " +
            $"史詩：{epicWeight:0.0} | " +
            $"傳說：{legendaryWeight:0.0} | " +
            $"神話：{mythicWeight:0.0}"
        );

        //在 0 到總權重之間抽一個數字
        float roll = Random.Range(0f, totalWeight);

        if (roll < mythicWeight)
        {
            return CardRarity.Mythic;
        }

        roll -= mythicWeight;

        if (roll < legendaryWeight)
        {
            return CardRarity.Legendary;
        }

        roll -= legendaryWeight;

        if (roll < epicWeight)
        {
            return CardRarity.Epic;
        }

        roll -= epicWeight;

        if (roll < rareWeight)
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
            CardCategory.Attribute,
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

    //卡牌詳細敘述，依據不同屬性與數值，產生不同的文字敘述，產生與實際效果一致的卡牌說明
    string CreateDescription(UpgradeType type, float flatValue, float multiplierValue)
    {
        string description;                             
        string typeName = GetUpgradeTypeName(type);     

        switch (type)
        {
            case UpgradeType.Damage:
                description =
                    $"攻擊力 +{flatValue * 1f:0}";
                break;

            case UpgradeType.AttackRange:
                description =
                    $"攻擊距離 +{flatValue * 5f:0}%";
                break;

            case UpgradeType.AttackSpeed:
                description =
                    $"攻擊速度 +{flatValue * 5f:0}%";
                break;

            case UpgradeType.MaxHP:
                description =
                    $"最大生命 +{flatValue * 10f:0}";
                break;

            case UpgradeType.MoveSpeed:
                description =
                    $"移動速度 +{flatValue * 5f:0}%";
                break;

            case UpgradeType.CriticalRate:
                description =
                    $"暴擊率 +{flatValue:0}%";
                break;

            case UpgradeType.CriticalDamage:
                description =
                    $"暴擊傷害 +{flatValue * 10f:0}%";
                break;

            case UpgradeType.LifeSteal:
                description =
                    $"吸血 +{flatValue:0}%";
                break;

            case UpgradeType.Armor:
                description =
                    $"防禦 +{flatValue:0.#}";
                break;

            default:
                description =
                    $"未知效果 +{flatValue:0.#}";
                break;
        }

        // 神話卡的額外百分比描述
        if (multiplierValue > 0f)
        {
            description +=
                $"\n全部{typeName}額外提升 " +
                $"{multiplierValue * 100f:0.#}%";
        }

        return description;
    }

    //更新UI文字
    void UpdateCardUI()
    {
        if (currentCards.Count < 3)
        {
            Debug.LogWarning("目前卡牌數量不足 3 張，無法更新 UI。");
            return;
        }

        SetCardText(cardText1, currentCards[0]);
        SetCardText(cardText2, currentCards[1]);
        SetCardText(cardText3, currentCards[2]);

        //單張卡牌的顯示樣式，包含幸運卡出現時顯示
        void SetCardText(TMP_Text textComponent, UpgradeCard card)
        {
            if (textComponent == null)
            {
                Debug.LogWarning("有卡牌文字元件尚未在 Inspector 指定。");
                return;
            }

            if (card.category == CardCategory.Special)
            {
                textComponent.text =
                    "【命運眷顧】\n" +
                    $"聽說運氣會變得更好..." 
                    ;         
                return;
            }

            string rarityName = GetRarityName(card.rarity);

            textComponent.text =
                $"【{rarityName}】\n" +
                card.description;
        }
    }
        
    //當玩家點擊 UI 畫面上的第 1、2、3 個按鈕時，就會觸發這個方法
    public void SelectCard(int index)
    {
        if (index < 0 || index >= currentCards.Count)
        {
            // 防呆機制：如果傳進來的數字小於 0，或者大於我們抽出的卡牌總數（目前是 3 張），就報警告並攔截
            Debug.LogWarning("選擇的卡牌 index 超出範圍");
            return;
        }

        if (playerStats == null)
        {
            Debug.LogError("UpgradeManager 尚未指定 PlayerStats。");
            return;
        }

        UpgradeCard selectedCard = currentCards[index];

        Debug.Log($"選擇卡牌：{selectedCard.category} / {selectedCard.cardName}");

        // 根據卡牌類別，決定要使用哪個方法來應用效果，如果是特殊卡就呼叫 ApplySpecialCardEffect，否則呼叫 ApplyCardEffect
        if (selectedCard.category == CardCategory.Special)
        {
            ApplySpecialCardEffect(selectedCard);
        }
        else
        {
            ApplyCardEffect(selectedCard);
        }

        CloseUpgradePanel();
    }

    //負責處理一般屬性卡的效果，像是攻擊力、攻擊速度等，並把卡片裡的隨機數值加到玩家的 playerStats中
    void ApplyCardEffect(UpgradeCard card)
    {
        switch (card.upgradeType)
        {
            //選擇攻擊力卡牌時，會把卡片裡的 flatValue 加到玩家的攻擊力上，如果是神話卡還會額外加上 multiplierValue 的百分比加成
            case UpgradeType.Damage:
                playerStats.AddFlatDamage((int)card.flatValue);

                if (card.multiplierValue > 0f)
                {
                    playerStats.AddDamageMultiplier(card.multiplierValue);
                }

                break;

            //選擇攻擊距離卡牌時，會把卡片裡的 flatValue 乘上 0.05f，然後加到玩家的攻擊距離上
            case UpgradeType.AttackRange:
                {
                    float attackRangePercent = card.flatValue * 0.05f;
                    playerStats.AddAttackRangeMultiplier(attackRangePercent);

                    break;
                }


            //選擇攻擊速度卡牌時，會把卡片裡的 flatValue 乘上 0.05f，然後加到玩家的攻擊速度上
            case UpgradeType.AttackSpeed:
                {
                    float attackSpeedPercent =card.flatValue * 0.05f;
                    playerStats.AddAttackSpeedMultiplier(attackSpeedPercent);

                    break;
                }

            //選擇暴擊率卡牌時，會把卡片裡的 flatValue 乘上 0.01f，然後加到玩家的暴擊率上
            case UpgradeType.CriticalRate:
                playerStats.AddCriticalRate(card.flatValue * 0.01f);
                break;

            //選擇暴擊傷害卡牌時，會把卡片裡的 flatValue 乘上 0.1f，然後加到玩家的暴擊傷害上
            case UpgradeType.CriticalDamage:
                playerStats.AddCriticalDamage(card.flatValue * 0.1f);
                break;

            //選擇最大生命卡牌時，會把卡片裡的 flatValue 乘上 10，然後加到玩家的最大生命上
            case UpgradeType.MaxHP:
                playerStats.AddMaxHP((int)(card.flatValue * 10));
                break;

            //選擇移動速度卡牌時，會把卡片裡的 flatValue 乘上 0.05f，然後加到玩家的移動速度上
            case UpgradeType.MoveSpeed:
                {
                    float moveSpeedPercent = card.flatValue * 0.05f;
                    playerStats.AddMoveSpeedMultiplier(moveSpeedPercent);

                    break;
                }

            //選擇吸血卡牌時，會把卡片裡的 flatValue 乘上 0.01f，然後加到玩家的吸血上
            case UpgradeType.LifeSteal:
                playerStats.AddLifeSteal(card.flatValue * 0.01f);
                break;

            //選擇防禦卡牌時，會把卡片裡的 flatValue 乘上 3，然後加到玩家的防禦上
            case UpgradeType.Armor:
                playerStats.AddArmor(card.flatValue * 3);
                break;
        }
    }

    //負責處理特殊卡的效果，像是幸運牌
    void ApplySpecialCardEffect(UpgradeCard card)
    {
        if (card.cardName == "幸運牌")
        {
            playerStats.AddLuckLevel(1);

            Debug.Log(
                $"幸運牌生效，目前幸運等級：{playerStats.luckLevel}"
            );

            return;
        }

        Debug.LogWarning(
            $"尚未處理這張特殊卡：{card.cardName}"
        );
    }

    //關閉升級面板，並恢復遊戲時間
    void CloseUpgradePanel()
    {
        upgradePanel.SetActive(false);
        Time.timeScale = 1f;
    }
}
