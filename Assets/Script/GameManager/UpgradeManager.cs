using System.Collections.Generic;
using TMPro;
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

    [Header("幸運牌設定")]
    [Range(0f, 100f)]
    public float luckCardChance = 15f;

    #region 神話卡抽取設定
    [Header("神話卡設定")]
    [Tooltip("沒有任何幸運等級時，每個卡位抽到神話卡的基礎機率")]
    [Range(0f, 100f)]
    public float baseMythicCardChance = 1.5f;

    [Tooltip("每提升 1 級幸運，神話卡機率增加多少")]
    [Range(0f, 10f)]
    public float mythicChancePerLuckLevel = 0.25f;

    [Tooltip("神話卡出現機率的最高上限")]
    [Range(0f, 100f)]
    public float maxMythicCardChance = 10f;
    #endregion

    #region 一般屬性卡數值設定
    [Header("一般屬性卡每點效果")]

    [InspectorName("攻擊力")]
    [Tooltip("每 1 點 flatValue 增加多少固定攻擊力")]
    [Min(0f)]
    public float damagePFV = 3f;

    [InspectorName("攻擊距離")]
    [Tooltip("每 1 點 flatValue 增加多少攻擊距離，例如 0.08 = 8%")]
    [Min(0f)]
    public float attackRangePFV = 0.08f;

    [InspectorName("攻擊速度")]
    [Tooltip("每 1 點 flatValue 增加多少攻擊速度，例如 0.12 = 12%")]
    [Min(0f)]
    public float attackSpeedPFV = 0.12f;

    [InspectorName("最大生命")]
    [Tooltip("每 1 點 flatValue 增加多少最大生命")]
    [Min(0f)]
    public float maxHPPFV = 10f;

    [InspectorName("移動速度")]
    [Tooltip("每 1 點 flatValue 增加多少移動速度，例如 0.08 = 8%")]
    [Min(0f)]
    public float moveSpeedPFV = 0.08f;

    [InspectorName("暴擊率")]
    [Tooltip("每 1 點 flatValue 增加多少暴擊率，例如 0.05 = 5 個百分點")]
    [Min(0f)]
    public float criticalRatePFV = 0.05f;

    [InspectorName("暴擊傷害")]
    [Tooltip("每 1 點 flatValue 增加多少暴擊傷害，例如 0.10 = 10 個百分點")]
    [Min(0f)]
    public float criticalDamagePFV = 0.10f;

    [InspectorName("吸血")]
    [Tooltip("每 1 點 flatValue 增加多少吸血，例如 0.025 = 2.5 個百分點")]
    [Min(0f)]
    public float lifeStealPFV = 0.025f;

    [InspectorName("防禦")]
    [Tooltip("每 1 點 flatValue 增加多少防禦")]
    [Min(0f)]
    public float armorPFV = 3f;
    #endregion

    //處發升級，會打開升級面板，並暫停遊戲
    public void ShowUpgradePanel()
    {
        
        GenerateUpgradeCards();             //製作牌的方法
        UpdateCardUI();                     //更新UI文字

        //打開升級面板，暫停遊戲
        upgradePanel.SetActive(true);
        Time.timeScale = 0f;
    }

    //產生本次升級使用的三張卡牌
    public void GenerateUpgradeCards()
    {
        //清除上一輪升級留下的卡牌
        currentCards.Clear();

        //HashSet 不允許相同的 UpgradeType 重複存在
        //記錄本輪已經出現過的屬性類型
        HashSet<UpgradeType> selectedAttributeTypes = new HashSet<UpgradeType>();
        //紀錄本輪已經出現過的神話卡
        HashSet<SpecialCardType> selectedMythicTypes = new HashSet<SpecialCardType>();

        //依照固定流程產生卡牌
        TryGenerateLuckCard();                                                      //嘗試產生幸運牌
        FillRemainingCardSlots(selectedAttributeTypes, selectedMythicTypes);        //每一個卡位都會先判斷神話，失敗才產生一般屬性卡
        ShuffleCurrentCards();                                                      //打亂本輪三張卡牌的位置
        PrintCurrentCards();                                                        //在 Console 顯示本次抽卡結果       
    }

    //嘗試產生一張特殊幸運牌
    void TryGenerateLuckCard()
    {
        //每次升級只進行一次幸運牌判定
        float luckCardRoll = Random.Range(0f, 100f);

        if (luckCardRoll < luckCardChance)
        {
            UpgradeCard luckCard = CreateLuckCard();
            currentCards.Add(luckCard);

            Debug.Log(
                $"本次升級抽中幸運牌，判定值：{luckCardRoll:0.00}"
            );

            return;
        }

        Debug.Log(
            $"本次升級未抽中幸運牌，判定值：{luckCardRoll:0.00}"
        );
    }

    //將剩餘卡位補滿到三張
    void FillRemainingCardSlots(HashSet<UpgradeType> selectedAttributeTypes, HashSet<SpecialCardType> selectedMythicTypes)
    {
        while (currentCards.Count < 3)
        {
            //每個剩餘卡位先獨立進行神話卡判定
            if (ShouldGenerateMythicCard())
            {
                UpgradeCard mythicCard = CreateRandomMythicCard(selectedMythicTypes);

                if (mythicCard != null)
                {
                    currentCards.Add(mythicCard);
                    continue;
                }
            }

            //神話判定失敗時，產生一般屬性卡
            UpgradeCard attributeCard = CreateRandomAttributeCard(selectedAttributeTypes);

            if (attributeCard != null)
            {
                currentCards.Add(attributeCard);
                continue;
            }

            //理論上不應該發生
            //防止建立卡牌失敗時形成無限迴圈
            Debug.LogError(
                "無法建立神話卡或一般屬性卡，停止補牌。"
            );

            break;
        }
    }

    //判斷目前卡位是否產生神話卡
    bool ShouldGenerateMythicCard()
    {
        float finalMythicChance =
            GetFinalMythicCardChance();

        float roll =
            Random.Range(0f, 100f);

        bool success =
            roll < finalMythicChance;

        Debug.Log(
            $"神話卡判定：" +
            $"Luck Level {GetCurrentLuckLevel()} | " +
            $"目前機率 {finalMythicChance:0.00}% | " +
            $"判定值 {roll:0.00} | " +
            $"結果：{(success ? "成功" : "失敗")}"
        );

        return success;
    }


    //計算目前最終神話卡機率
    float GetFinalMythicCardChance()
    {
        int luckLevel =
            GetCurrentLuckLevel();

        float finalChance =
            baseMythicCardChance
            + luckLevel * mythicChancePerLuckLevel;

        return Mathf.Clamp(
            finalChance,
            0f,
            maxMythicCardChance
        );
    }


    //安全取得目前玩家的幸運等級
    int GetCurrentLuckLevel()
    {
        if (playerStats == null)
        {
            return 0;
        }

        return Mathf.Max(
            playerStats.luckLevel,
            0
        );
    }

    //建立一張本輪尚未出現的一般屬性卡
    UpgradeCard CreateRandomAttributeCard( HashSet<UpgradeType> selectedAttributeTypes)
    {
        //UpgradeType 總數目前大於三張卡位
        //因此正常情況一定能找到未重複的屬性
        int safetyCounter = 0;
        int maxAttempts = 100;

        while (safetyCounter < maxAttempts)
        {
            safetyCounter++;

            UpgradeType randomType =
                GetRandomUpgradeType();

            //本輪已經有相同屬性，就重新抽取
            if (selectedAttributeTypes.Contains(randomType))
            {
                continue;
            }

            selectedAttributeTypes.Add(randomType);

            CardRarity randomRarity = GetRandomRarity();

            return CreateCardByTypeAndRarity(
                randomType,
                randomRarity
            );
        }

        Debug.LogError(
            "建立一般屬性卡失敗：" +
            "嘗試次數已超過安全上限。"
        );

        return null;
    }

    //建立一張本輪尚未出現的神話卡
    UpgradeCard CreateRandomMythicCard(HashSet<SpecialCardType> selectedMythicTypes)
    {
        //只把四種神話卡加入候選清單
        //不包含 None 和 Luck
        List<SpecialCardType> availableMythicTypes =
            new List<SpecialCardType>
            {
            SpecialCardType.MythicCombat,
            SpecialCardType.MythicDefense,
            SpecialCardType.MythicCritical,
            SpecialCardType.MythicSurvival
            };

        //移除本輪已經出現過的神話類型
        availableMythicTypes.RemoveAll(
            type => selectedMythicTypes.Contains(type)
        );

        //已經沒有可用神話卡
        if (availableMythicTypes.Count == 0)
        {
            Debug.LogWarning(
                "本輪所有神話卡都已經出現，" +
                "無法再產生不重複神話卡。"
            );

            return null;
        }

        //從剩餘神話卡中等機率選一張
        int randomIndex =
            Random.Range(
                0,
                availableMythicTypes.Count
            );

        SpecialCardType selectedType =
            availableMythicTypes[randomIndex];

        //紀錄本輪已經出現
        selectedMythicTypes.Add(selectedType);

        return CreateMythicCard(selectedType);
    }

    //根據神話類型建立對應神話卡
    UpgradeCard CreateMythicCard(SpecialCardType mythicType)
    {
        switch (mythicType)
        {
            case SpecialCardType.MythicCombat:
                return new UpgradeCard(
                    "戰鬥本能",
                    "攻擊力、攻擊距離、攻擊速度\n" +
                    "最終提升10%",
                    CardCategory.Special,

                    //特殊卡不使用一般屬性欄位
                    UpgradeType.Damage,
                    CardRarity.Common,

                    0f,
                    0f,
                    SpecialCardType.MythicCombat
                );

            case SpecialCardType.MythicDefense:
                return new UpgradeCard(
                    "鋼鐵之軀",
                    "最大生命、防禦最終提升20%\n" +
                    "移動速度最終降低10%",
                    CardCategory.Special,

                    UpgradeType.Damage,
                    CardRarity.Common,

                    0f,
                    0f,
                    SpecialCardType.MythicDefense
                );

            case SpecialCardType.MythicCritical:
                return new UpgradeCard(
                    "致命一擊",
                    "暴擊率增加+15%\n" +
                    "最終暴擊傷害提升10%",
                    CardCategory.Special,

                    UpgradeType.Damage,
                    CardRarity.Common,

                    0f,
                    0f,
                    SpecialCardType.MythicCritical
                );

            case SpecialCardType.MythicSurvival:
                return new UpgradeCard(
                    "鮮血追獵",
                    "吸血增加+10%\n" +
                    "移動速度最終提升20%",
                    CardCategory.Special,

                    UpgradeType.Damage,
                    CardRarity.Common,

                    0f,
                    0f,
                    SpecialCardType.MythicSurvival
                );

            default:
                Debug.LogError(
                    $"無法建立未知神話卡：{mythicType}"
                );

                return null;
        }
    }

    //打亂本輪三張卡牌的位置
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

    //在 Console 顯示本輪抽卡結果
    void PrintCurrentCards()
    {
        for (int i = 0; i < currentCards.Count; i++)
        {
            UpgradeCard card = currentCards[i];

            //特殊卡沒有一般品質顯示
            if (card.category == CardCategory.Special)
            {
                Debug.Log(
                    $"第 {i + 1} 張卡：" +
                    $"{card.category} / " +
                    $"{card.cardName} / " +
                    $"{card.description}"
                );

                continue;
            }

            //一般屬性卡顯示品質、名稱與描述
            Debug.Log(
                $"第 {i + 1} 張卡：" +
                $"{card.category} / " +
                $"{card.rarity} / " +
                $"{card.cardName} / " +
                $"{card.description}"
            );
        }
    }

    //建立特殊幸運牌
    UpgradeCard CreateLuckCard()
    {
        string cardName = "幸運牌";
        string description = "幸運等級 +3";

        return new UpgradeCard(
            cardName,
            description,
            CardCategory.Special,

            // 特殊卡不使用 UpgradeType 與 CardRarity
            // 目前先放入預設值
            UpgradeType.Damage,
            CardRarity.Common, 1f, 0f,

            // 明確指定這是一張幸運牌
            SpecialCardType.Luck
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

    //根據幸運等級決定一般屬性卡品質
    CardRarity GetRandomRarity()
    {
        int luckLevel = GetCurrentLuckLevel();

        //新的一般品質權重
        float commonWeight = 27f - luckLevel * 1.5f;

        float rareWeight = 9f + luckLevel * 0.8f;

        float epicWeight = 3f + luckLevel * 0.4f;

        float legendaryWeight = 1f + luckLevel * 0.2f;

        // Luck 18 之後，普通牌權重允許歸零
        // 但不允許變成負數
        commonWeight =
            Mathf.Max(commonWeight, 0f);

        float totalWeight =
            commonWeight
            + rareWeight
            + epicWeight
            + legendaryWeight;

        Debug.Log(
            $"一般品質權重 | " +
            $"Luck Level：{luckLevel} | " +
            $"普通：{commonWeight:0.0} | " +
            $"稀有：{rareWeight:0.0} | " +
            $"史詩：{epicWeight:0.0} | " +
            $"傳說：{legendaryWeight:0.0}"
        );

        float roll = Random.Range(0f, totalWeight);

        //由最高品質往下判斷
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
                return 1.5f;

            case CardRarity.Epic:
                return 2f;

            case CardRarity.Legendary:
                return 3f;

            default:
                return 1f;
        }
    }

    //只有神話才享有的百分比加成
    float GetMultiplierValueByRarity(CardRarity rarity)
    {
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
                {
                    float amount =
                        flatValue * damagePFV;

                    description =
                        $"攻擊力 +{amount:0.#}";
                    break;
                }

            case UpgradeType.AttackRange:
                {
                    float amount =
                        flatValue *
                        attackRangePFV *
                        100f;

                    description =
                        $"攻擊距離 +{amount:0.#}%";
                    break;
                }

            case UpgradeType.AttackSpeed:
                {
                    float amount =
                        flatValue *
                        attackSpeedPFV *
                        100f;

                    description =
                        $"攻擊速度 +{amount:0.#}%";
                    break;
                }

            case UpgradeType.MaxHP:
                {
                    float amount =
                        flatValue * maxHPPFV;

                    description =
                        $"最大生命 +{amount:0.#}";
                    break;
                }

            case UpgradeType.MoveSpeed:
                {
                    float amount =
                        flatValue *
                        moveSpeedPFV *
                        100f;

                    description =
                        $"移動速度 +{amount:0.#}%";
                    break;
                }

            case UpgradeType.CriticalRate:
                {
                    float amount =
                        flatValue *
                        criticalRatePFV *
                        100f;

                    description =
                        $"暴擊率 +{amount:0.#}%";
                    break;
                }

            case UpgradeType.CriticalDamage:
                {
                    float amount =
                        flatValue *
                        criticalDamagePFV *
                        100f;

                    description =
                        $"暴擊傷害 +{amount:0.#}%";
                    break;
                }

            case UpgradeType.LifeSteal:
                {
                    float amount =
                        flatValue *
                        lifeStealPFV *
                        100f;

                    description =
                        $"吸血 +{amount:0.#}%";
                    break;
                }

            case UpgradeType.Armor:
                {
                    float amount =
                        flatValue * armorPFV;

                    description =
                        $"防禦 +{amount:0.#}";
                    break;
                }

            default:
                description =
                    $"未知效果 +{flatValue:0.#}";
                break;
        }

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
                switch (card.specialCardType)
                {
                    case SpecialCardType.Luck:
                        textComponent.text =
                            "【命運眷顧】\n" +
                            "聽說運氣會變得更好...";
                        break;

                    case SpecialCardType.MythicCombat:
                    case SpecialCardType.MythicDefense:
                    case SpecialCardType.MythicCritical:
                    case SpecialCardType.MythicSurvival:
                        textComponent.text =
                            $"【神話】{card.cardName}\n" +
                            card.description;
                        break;

                    default:
                        textComponent.text =
                            "【未知特殊卡】\n" +
                            card.description;
                        break;
                }

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
            case UpgradeType.Damage:
                ApplyDamageUpgrade(card);
                break;

            case UpgradeType.AttackRange:
                ApplyAttackRangeUpgrade(card);
                break;

            case UpgradeType.AttackSpeed:
                ApplyAttackSpeedUpgrade(card);
                break;

            case UpgradeType.CriticalRate:
                ApplyCriticalRateUpgrade(card);
                break;

            case UpgradeType.CriticalDamage:
                ApplyCriticalDamageUpgrade(card);
                break;

            case UpgradeType.MaxHP:
                ApplyMaxHPUpgrade(card);
                break;

            case UpgradeType.MoveSpeed:
                ApplyMoveSpeedUpgrade(card);
                break;

            case UpgradeType.LifeSteal:
                ApplyLifeStealUpgrade(card);
                break;

            case UpgradeType.Armor:
                ApplyArmorUpgrade(card);
                break;

            default:
                Debug.LogWarning(
                    $"尚未處理這個屬性卡效果：{card.upgradeType}"
                );
                break;
        }

        //套用攻擊力卡牌效果
        void ApplyDamageUpgrade(UpgradeCard upgradeCard)
        {
            int damageAmount =
                Mathf.RoundToInt(
                    upgradeCard.flatValue *
                    damagePFV
                );

            playerStats.AddFlatDamage(
                damageAmount
            );

            if (upgradeCard.multiplierValue > 0f)
            {
                playerStats.AddDamageMultiplier(
                    upgradeCard.multiplierValue
                );
            }
        }

        //套用攻擊距離卡牌效果
        void ApplyAttackRangeUpgrade(UpgradeCard upgradeCard)
        {
            float attackRangePercent =
                upgradeCard.flatValue *
                attackRangePFV;

            playerStats.AddAttackRangeMultiplier(
                attackRangePercent
            );
        }

        //套用攻擊速度卡牌效果
        void ApplyAttackSpeedUpgrade(UpgradeCard upgradeCard)
        {
            float attackSpeedPercent =
                upgradeCard.flatValue *
                attackSpeedPFV;

            playerStats.AddAttackSpeedMultiplier(
                attackSpeedPercent
            );
        }

        //套用暴擊率卡牌效果
        void ApplyCriticalRateUpgrade(UpgradeCard upgradeCard)
        {
            float criticalRatePercent =
                upgradeCard.flatValue *
                criticalRatePFV;

            playerStats.AddCriticalRate(
                criticalRatePercent
            );
        }

        //套用暴擊傷害卡牌效果
        void ApplyCriticalDamageUpgrade(UpgradeCard upgradeCard)
        {
            float criticalDamagePercent =
                upgradeCard.flatValue *
                criticalDamagePFV;

            playerStats.AddCriticalDamage(
                criticalDamagePercent
            );
        }

        //套用最大生命卡牌效果
        void ApplyMaxHPUpgrade(UpgradeCard upgradeCard)
        {
            int maxHPAmount =
                Mathf.RoundToInt(
                    upgradeCard.flatValue *
                    maxHPPFV
                );

            playerStats.AddMaxHP(maxHPAmount);

            PlayerHealth playerHealth = playerStats.GetComponent<PlayerHealth>();

            if (playerHealth != null)
            {
                playerHealth.RefreshMaxHP(true);
            }
            else
            {
                Debug.LogWarning(
                    "套用最大生命卡後找不到 PlayerHealth。"
                );
            }
        }

        //套用移動速度卡牌效果
        void ApplyMoveSpeedUpgrade(UpgradeCard upgradeCard)
        {
            float moveSpeedPercent =
                upgradeCard.flatValue *
                moveSpeedPFV;

            playerStats.AddMoveSpeedMultiplier(
                moveSpeedPercent
            );
        }

        //套用吸血卡牌效果
        void ApplyLifeStealUpgrade(UpgradeCard upgradeCard)
        {
            float lifeStealPercent =
                upgradeCard.flatValue *
                lifeStealPFV;

            playerStats.AddLifeSteal(
                lifeStealPercent
            );
        }

        //套用防禦卡牌效果
        void ApplyArmorUpgrade(UpgradeCard upgradeCard)
        {
            float armorAmount =
                upgradeCard.flatValue *
                armorPFV;

            playerStats.AddArmor(
                armorAmount
            );
        }
    }

    //負責處理幸運牌與四種神話卡的效果
    void ApplySpecialCardEffect(UpgradeCard card)
    {
        switch (card.specialCardType)
        {
            //幸運牌：幸運等級 +3
            case SpecialCardType.Luck:
                playerStats.AddLuckLevel(3);

                Debug.Log(
                    $"幸運牌生效，目前幸運等級：" +
                    $"{playerStats.luckLevel}"
                );
                break;


            //戰鬥本能：
            //攻擊力、攻擊距離、攻擊速度最終提升 10%
            case SpecialCardType.MythicCombat:
                playerStats.AddDamageMultiplier(0.10f);
                playerStats.AddAttackRangeMultiplier(0.10f);
                playerStats.AddAttackSpeedMultiplier(0.10f);

                Debug.Log(
                    "神話卡【戰鬥本能】生效：" +
                    "攻擊力、攻擊距離、攻擊速度最終提升 10%"
                );
                break;


            //鋼鐵之軀：
            //最大生命、防禦最終提升 20%
            //移動速度最終降低 10%
            case SpecialCardType.MythicDefense:
                playerStats.AddMaxHPMultiplier(0.20f);
                playerStats.AddArmorMultiplier(0.20f);
                playerStats.AddMoveSpeedMultiplier(-0.10f);

                PlayerHealth playerHealth =
                    playerStats.GetComponent<PlayerHealth>();

                if (playerHealth != null)
                {
                    playerHealth.RefreshMaxHP(true);
                }
                else
                {
                    Debug.LogWarning(
                        "套用鋼鐵之軀後找不到 PlayerHealth。"
                    );
                }

                Debug.Log(
                    "神話卡【鋼鐵之軀】生效：" +
                    "最大生命、防禦最終提升 20%，" +
                    "移動速度最終降低 10%"
                );
                break;


            //致命一擊：
            //暴擊率增加 15 個百分點
            //最終暴擊傷害提升 10%
            case SpecialCardType.MythicCritical:
                playerStats.AddCriticalRate(0.15f);
                playerStats.AddCriticalDamageMultiplier(0.10f);

                Debug.Log(
                    "神話卡【致命一擊】生效：" +
                    "暴擊率增加 15 個百分點，" +
                    "最終暴擊傷害提升 10%"
                );
                break;


            //鮮血追獵：
            //吸血增加 10 個百分點
            //移動速度最終提升 20%
            case SpecialCardType.MythicSurvival:
                playerStats.AddLifeSteal(0.1f);
                playerStats.AddMoveSpeedMultiplier(0.20f);

                Debug.Log(
                    "神話卡【鮮血追獵】生效：" +
                    "吸血增加 10 個百分點，" +
                    "移動速度最終提升 20%"
                );
                break;


            default:
                Debug.LogWarning(
                    $"尚未處理這張特殊卡：" +
                    $"{card.specialCardType} / {card.cardName}"
                );
                break;
        }
    }

    //關閉升級面板，並恢復遊戲時間
    void CloseUpgradePanel()
    {
        upgradePanel.SetActive(false);
        Time.timeScale = 1f;
    }
}
