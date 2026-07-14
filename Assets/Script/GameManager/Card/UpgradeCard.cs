// 定義卡牌的資料類別
public class UpgradeCard
{
    public string cardName;             // 變數：儲存卡牌名稱
    public string description;          // 變數：儲存卡牌敘述

    public CardCategory category;       // 變數：指定這張卡的類別（屬性增長卡或特殊卡）
    /*屬性卡*/public UpgradeType upgradeType;     // 變數：指定這張卡對應哪種屬性
    /*屬性卡*/public CardRarity rarity;           // 變數：指定這張卡的稀有度

    public float flatValue;             // 變數：基礎直加值
    public float multiplierValue;       // 變數：百分比乘算值

    //當你要在程式中「創造新卡牌」時，負責把資料填進去的公式
    public UpgradeCard(
        string cardName,                //卡牌名稱
        string description,             //卡牌敘述
        CardCategory category,          //指定這張卡的類別（屬性增長卡或特殊卡）
        UpgradeType upgradeType,        //只有屬性卡才會有對應的屬性，特殊卡不會有
        CardRarity rarity,              //只有屬性卡才會有稀有度，特殊卡不會有
        float flatValue,                //只有屬性卡才會有基礎直加值，其他預設為0f
        float multiplierValue = 0f      //只有屬性神話卡才會有特別成算區，其他預設為0f
    )
    //將抽到的所有變數指定在這個物件顯示
    {
        this.cardName = cardName;
        this.description = description;
        this.category = category;
        this.upgradeType = upgradeType;
        this.rarity = rarity;
        this.flatValue = flatValue;
        this.multiplierValue = multiplierValue;
    }
}