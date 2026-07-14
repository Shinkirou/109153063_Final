using UnityEngine;

public class PlayerStats : MonoBehaviour
{

    [Header("幸運系統")]
    public int luckLevel = 0;

    [Header("生命")]
    public int maxHP = 100;

    [Header("移動")]
    public float moveSpeed = 6f;

    [Header("攻擊")]            
    public float attackRange = 6f;
    public float attackInterval = 1f;   //攻擊間隔

    [Header("攻擊力計算")]
    public int baseDamage = 3;
    public int flatDamageBonus = 0;
    public float damageMultiplier = 1f;

    public int FinalDamage
    {
        get
        {
            return Mathf.RoundToInt((baseDamage + flatDamageBonus) * damageMultiplier);
        }
    } 

    [Header("暴擊")]
    [Range(0f, 1f)]
    public float criticalRate = 0f;
    public float criticalDamage = 2f;

    [Header("特殊能力")]
    public float lifeSteal = 0f;
    public float armor = 0f;

    public void AddMaxHP(int amount)
    {
        maxHP += amount;
    }

    public void AddMoveSpeed(float amount)
    {
        moveSpeed += amount;
    }
    public void AddFlatDamage(int amount)
    {
        flatDamageBonus += amount;
    }

    public void AddDamageMultiplier(float percent)
    {
        damageMultiplier += percent;
    }

    /*
     //老加成攻擊力公式
    public void AddDamage(int amount)
    {
        damage += amount;
    }
    */
    

    public void AddAttackRange(float amount)
    {
        attackRange += amount;
    }

    public void MultiplyAttackSpeed(float multiplier)
    {
        attackInterval *= multiplier;

        if (attackInterval < 0.1f)
        {
            attackInterval = 0.1f;
        }
    }

    public void AddCriticalRate(float amount)
    {
        criticalRate += amount;

        if (criticalRate > 1f)
        {
            criticalRate = 1f;
        }
    }

    public void AddCriticalDamage(float amount)
    {
        criticalDamage += amount;
    }

    public void AddLuckLevel(int amount)
    {
        luckLevel += amount;

        if (luckLevel < 0)
        {
            luckLevel = 0;
        }

        Debug.Log($"幸運等級提升，目前 Luck Level：{luckLevel}");
    }

    public void AddLifeSteal(float amount)
    {
        lifeSteal += amount;
    }

    public void AddArmor(float amount)
    {
        armor += amount;
    }
}