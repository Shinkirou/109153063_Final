using UnityEngine;

public class PlayerStats : MonoBehaviour
{

    [Header("幸運系統")]
    public int luckLevel = 0;

    [Header("生命")]
    public int maxHP = 100;

    [Header("移動速度")]                    
    [Tooltip("最終移速 = 基礎移速 ×（1 + 移速倍率）")]
    public float baseMoveSpeed = 5f;
    public float moveSpeedMultiplier = 0f;
    public float FinalMoveSpeed
    {
        get
        {
            return Mathf.Max(
                baseMoveSpeed * (1f + moveSpeedMultiplier),
                0.1f
            );
        }
    }//避免移動速度變成 0 或負數，造成玩家無法移動

    [Header("攻擊距離")]
    [Tooltip("最終攻擊距離 = 基礎距離 ×（1 + 距離倍率）")]
    public float baseAttackRange = 5f;
    public float attackRangeMultiplier = 0f;
    public float FinalAttackRange
    {
        get
        {
            return Mathf.Max(
                baseAttackRange * (1f + attackRangeMultiplier),
                0.1f
            );
        }
    }//避免攻擊距離變成 0 或負數，造成玩家無法攻擊

    [Header("攻擊速度")]
    [Tooltip("最終攻速代表每秒攻擊次數")]
    public float baseAttackSpeed = 1f;
    public float attackSpeedMultiplier = 0f;
    public float FinalAttackSpeed
    {
        get
        {
            return Mathf.Max(
                baseAttackSpeed * (1f + attackSpeedMultiplier),
                0.1f
            );
        }
    }//避免攻擊速度變成 0 或負數，造成玩家無法攻擊
    public float AttackInterval
    {
        get{return 1f / FinalAttackSpeed;}
    }                                           

    [Header("攻擊力計算")]
    public int baseDamage = 3;
    public int flatDamageBonus = 0;
    public float damageMultiplier = 1f;
    public int FinalDamage
    {
        get{return Mathf.RoundToInt((baseDamage + flatDamageBonus) * damageMultiplier);}
    } 

    [Header("暴擊")]
    [Range(0f, 1f)]
    public float criticalRate = 0f;
    public float criticalDamage = 2f;

    [Header("特殊能力")]
    public float lifeSteal = 0f;
    public float armor = 0f;

    //方法：增加最大生命值
    public void AddMaxHP(int amount)
    {
        maxHP += amount;
    }

    //方法：增加移動速度
    public void AddMoveSpeedMultiplier(float amount)
    {
        moveSpeedMultiplier += amount;
        moveSpeedMultiplier =Mathf.Max(moveSpeedMultiplier, -0.9f);

        Debug.Log(
            $"移動速度倍率：{moveSpeedMultiplier:P0}"
        );
    }

    //方法：增加攻擊力
    public void AddFlatDamage(int amount)
    {
        flatDamageBonus += amount;
    }

    //方法：增加攻擊力倍率
    public void AddDamageMultiplier(float percent)
    {
        damageMultiplier += percent;
    }

    //方法：增加攻擊距離
    public void AddAttackRangeMultiplier(float amount)
    {
        attackRangeMultiplier += amount;
        attackRangeMultiplier =Mathf.Max(attackRangeMultiplier, -0.9f);

        Debug.Log(
            $"攻擊距離倍率：{attackRangeMultiplier:P0}"
        );
    }

    //方法：增加攻擊速度
    public void AddAttackSpeedMultiplier(float amount)
    {
        attackSpeedMultiplier += amount;
        attackSpeedMultiplier =Mathf.Max(attackSpeedMultiplier, -0.9f);

        Debug.Log(
            $"攻擊速度倍率：{attackSpeedMultiplier:P0}"
        );
    }

    //方法：增加暴擊率
    public void AddCriticalRate(float amount)
    {
        criticalRate += amount;

        if (criticalRate > 1f)
        {
            criticalRate = 1f;
        }
    }

    //方法：增加暴擊傷害
    public void AddCriticalDamage(float amount)
    {
        criticalDamage += amount;
    }

    //方法：增加幸運等級
    public void AddLuckLevel(int amount)
    {
        luckLevel += amount;

        if (luckLevel < 0)
        {
            luckLevel = 0;
        }

        Debug.Log($"幸運等級提升，目前 Luck Level：{luckLevel}");
    }

    //方法：增加吸血
    public void AddLifeSteal(float amount)
    {
        lifeSteal += amount;
    }

    //方法：增加護甲
    public void AddArmor(float amount)
    {
        armor += amount;
    }
}