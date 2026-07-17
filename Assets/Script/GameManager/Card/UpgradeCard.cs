// 特殊卡牌的種類
// 用來區分幸運牌與四種神話牌
public enum SpecialCardType
{
    None,               // 不是特殊牌，或尚未指定
    Luck,               // 幸運牌
    MythicCombat,       // 戰鬥本能
    MythicDefense,      // 鋼鐵之軀
    MythicCritical,     // 致命一擊
    MythicSurvival      // 鮮血追獵
}


// 定義卡牌的資料類別
public class UpgradeCard
{
    public string cardName;             // 卡牌名稱
    public string description;          // 卡牌敘述

    public CardCategory category;       // 屬性卡或特殊卡

    // 只有一般屬性卡會使用
    public UpgradeType upgradeType;
    public CardRarity rarity;

    // 只有特殊卡會使用
    public SpecialCardType specialCardType;

    public float flatValue;             // 基礎直加值
    public float multiplierValue;       // 百分比乘算值


    // 建立一張新的卡牌時
    // 負責把所有卡牌資料存進 UpgradeCard
    public UpgradeCard(
        string cardName,
        string description,
        CardCategory category,
        UpgradeType upgradeType,
        CardRarity rarity,
        float flatValue,
        float multiplierValue = 0f,
        SpecialCardType specialCardType = SpecialCardType.None
    )
    {
        this.cardName = cardName;
        this.description = description;
        this.category = category;

        this.upgradeType = upgradeType;
        this.rarity = rarity;

        this.flatValue = flatValue;
        this.multiplierValue = multiplierValue;

        this.specialCardType = specialCardType;
    }
}