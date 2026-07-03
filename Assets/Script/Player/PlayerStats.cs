using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("生命")]
    public int maxHP = 100;

    [Header("移動")]
    public float moveSpeed = 6f;

    [Header("攻擊")]
    public int damage = 3;               
    public float attackRange = 6f;
    public float attackInterval = 1f;   //攻擊間隔

    [Header("暴擊")]
    [Range(0f, 1f)]
    public float criticalRate = 0f;
    public float criticalDamage = 2f;

    [Header("特殊能力")]
    public int luck = 0;
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

    public void AddDamage(int amount)
    {
        damage += amount;
    }

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

    public void AddLuck(int amount)
    {
        luck += amount;
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