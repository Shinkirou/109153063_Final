using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("幸運系統")]
    public int luckLevel = 0;
    public int maxLuckLevel = 10;

    public float LuckProgress
    {
        get
        {
            if (maxLuckLevel <= 0)
            {
                return 0f;
            }

            float currentLuck =
                Mathf.Clamp(
                    luckLevel,
                    0,
                    maxLuckLevel
                );

            return Mathf.Log(currentLuck + 1f) /
                   Mathf.Log(maxLuckLevel + 1f);
        }
    }


    //==================================================
    // 最大生命
    //==================================================

    [Header("生命")]
    [Tooltip("最終最大生命 = 最大生命 ×（1 + 最大生命倍率）")]
    public int maxHP = 100;

    public float maxHPMultiplier = 0f;

    public int FinalMaxHP
    {
        get
        {
            return Mathf.Max(
                Mathf.RoundToInt(
                    maxHP * (1f + maxHPMultiplier)
                ),
                1
            );
        }
    }


    //==================================================
    // 移動速度
    //==================================================

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
    }


    //==================================================
    // 攻擊距離
    //==================================================

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
    }


    //==================================================
    // 攻擊速度
    //==================================================

    [Header("攻擊速度")]
    [Tooltip("最終攻擊速度 = 基礎攻擊速度 ×（1 + 攻擊速度倍率）")]
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
    }

    //每次攻擊之間需要等待的秒數
    public float AttackInterval
    {
        get
        {
            return 1f / FinalAttackSpeed;
        }
    }


    //==================================================
    // 攻擊力
    //==================================================

    [Header("攻擊力")]
    [Tooltip("最終攻擊力 =（基礎攻擊力 + 固定加成）× 攻擊力倍率")]
    public int baseDamage = 3;

    public int flatDamageBonus = 0;

    // 攻擊力使用 1 作為初始倍率
    // 1 = 100%，1.05 = 105%
    public float damageMultiplier = 1f;

    public int FinalDamage
    {
        get
        {
            return Mathf.Max(
                Mathf.RoundToInt(
                    (baseDamage + flatDamageBonus)
                    * damageMultiplier
                ),
                0
            );
        }
    }


    //==================================================
    // 暴擊
    //==================================================

    [Header("暴擊")]
    [Range(0f, 1f)]
    public float criticalRate = 0f;

    [Tooltip("2 代表暴擊造成原本傷害的 200%")]
    public float criticalDamage = 2f;

    [Tooltip("最終暴擊傷害 = 暴擊傷害 ×（1 + 暴擊傷害倍率）")]
    public float criticalDamageMultiplier = 0f;

    public float FinalCriticalDamage
    {
        get
        {
            return Mathf.Max(
                criticalDamage
                * (1f + criticalDamageMultiplier),
                1f
            );
        }
    }


    //==================================================
    // 吸血
    //==================================================

    [Header("吸血")]
    [Range(0f, 1f)]
    public float lifeSteal = 0f;


    //==================================================
    // 防禦
    //==================================================

    [Header("防禦")]
    [Tooltip("最終防禦 = 防禦 ×（1 + 防禦倍率）")]
    public float armor = 0f;

    public float armorMultiplier = 0f;

    public float FinalArmor
    {
        get
        {
            return Mathf.Max(
                armor * (1f + armorMultiplier),
                0f
            );
        }
    }


    //==================================================
    // 最大生命修改方法
    //==================================================

    // 增加固定最大生命值
    public void AddMaxHP(int amount)
    {
        maxHP += amount;

        maxHP = Mathf.Max(maxHP, 1);

        Debug.Log(
            $"最大生命：{maxHP}，" +
            $"最終最大生命：{FinalMaxHP}"
        );
    }

    // 增加最大生命最終倍率
    public void AddMaxHPMultiplier(float amount)
    {
        maxHPMultiplier += amount;

        // 最多允許降低 90%
        maxHPMultiplier =
            Mathf.Max(maxHPMultiplier, -0.9f);

        Debug.Log(
            $"最大生命倍率：{maxHPMultiplier:P0}，" +
            $"最終最大生命：{FinalMaxHP}"
        );
    }


    //==================================================
    // 移動速度修改方法
    //==================================================

    public void AddMoveSpeedMultiplier(float amount)
    {
        moveSpeedMultiplier += amount;

        // 防止移動速度降低至 0 或負數
        moveSpeedMultiplier =
            Mathf.Max(moveSpeedMultiplier, -0.9f);

        Debug.Log(
            $"移動速度倍率：{moveSpeedMultiplier:P0}，" +
            $"最終移動速度：{FinalMoveSpeed:0.00}"
        );
    }


    //==================================================
    // 攻擊力修改方法
    //==================================================

    public void AddFlatDamage(int amount)
    {
        flatDamageBonus += amount;

        Debug.Log(
            $"固定攻擊力加成：{flatDamageBonus}，" +
            $"最終攻擊力：{FinalDamage}"
        );
    }

    public void AddDamageMultiplier(float percent)
    {
        damageMultiplier += percent;

        // 防止最終攻擊力倍率變成負數
        damageMultiplier =
            Mathf.Max(damageMultiplier, 0f);

        Debug.Log(
            $"攻擊力倍率：{damageMultiplier:P0}，" +
            $"最終攻擊力：{FinalDamage}"
        );
    }


    //==================================================
    // 攻擊距離修改方法
    //==================================================

    public void AddAttackRangeMultiplier(float amount)
    {
        attackRangeMultiplier += amount;

        attackRangeMultiplier =
            Mathf.Max(attackRangeMultiplier, -0.9f);

        Debug.Log(
            $"攻擊距離倍率：{attackRangeMultiplier:P0}，" +
            $"最終攻擊距離：{FinalAttackRange:0.00}"
        );
    }


    //==================================================
    // 攻擊速度修改方法
    //==================================================

    public void AddAttackSpeedMultiplier(float amount)
    {
        attackSpeedMultiplier += amount;

        attackSpeedMultiplier =
            Mathf.Max(attackSpeedMultiplier, -0.9f);

        Debug.Log(
            $"攻擊速度倍率：{attackSpeedMultiplier:P0}，" +
            $"最終攻擊速度：{FinalAttackSpeed:0.00}"
        );
    }


    //==================================================
    // 暴擊修改方法
    //==================================================

    // 直接增加暴擊率百分點
    // 例如 0.05f 代表增加 5 個百分點
    public void AddCriticalRate(float amount)
    {
        criticalRate += amount;

        criticalRate =
            Mathf.Clamp(criticalRate, 0f, 1f);

        Debug.Log(
            $"暴擊率：{criticalRate:P0}"
        );
    }

    // 增加原始暴擊傷害
    // 例如 0.1f 代表從 200% 增加到 210%
    public void AddCriticalDamage(float amount)
    {
        criticalDamage += amount;

        criticalDamage =
            Mathf.Max(criticalDamage, 1f);

        Debug.Log(
            $"暴擊傷害：{criticalDamage:P0}，" +
            $"最終暴擊傷害：{FinalCriticalDamage:P0}"
        );
    }

    // 增加最終暴擊傷害倍率
    // 例如 0.1f 代表最終暴擊傷害提升 10%
    public void AddCriticalDamageMultiplier(float amount)
    {
        criticalDamageMultiplier += amount;

        criticalDamageMultiplier =
            Mathf.Max(
                criticalDamageMultiplier,
                -0.9f
            );

        Debug.Log(
            $"暴擊傷害倍率：" +
            $"{criticalDamageMultiplier:P0}，" +
            $"最終暴擊傷害：" +
            $"{FinalCriticalDamage:P0}"
        );
    }


    //==================================================
    // 吸血修改方法
    //==================================================

    // 直接增加吸血百分點
    // 例如 0.05f 代表增加 5 個百分點
    public void AddLifeSteal(float amount)
    {
        lifeSteal += amount;

        lifeSteal =
            Mathf.Clamp(lifeSteal, 0f, 1f);

        Debug.Log(
            $"吸血：{lifeSteal:P0}"
        );
    }


    //==================================================
    // 防禦修改方法
    //==================================================

    // 增加固定防禦值
    public void AddArmor(float amount)
    {
        armor += amount;

        armor = Mathf.Max(armor, 0f);

        Debug.Log(
            $"防禦：{armor:0.##}，" +
            $"最終防禦：{FinalArmor:0.##}"
        );
    }

    // 增加防禦最終倍率
    public void AddArmorMultiplier(float amount)
    {
        armorMultiplier += amount;

        armorMultiplier =
            Mathf.Max(armorMultiplier, -0.9f);

        Debug.Log(
            $"防禦倍率：{armorMultiplier:P0}，" +
            $"最終防禦：{FinalArmor:0.##}"
        );
    }


    //==================================================
    // 幸運修改方法
    //==================================================

    public void AddLuckLevel(int amount)
    {
        luckLevel += amount;

        luckLevel = Mathf.Clamp(
            luckLevel,
            0,
            maxLuckLevel
        );

        Debug.Log(
            $"幸運等級提升，" +
            $"目前 Luck Level：{luckLevel}"
        );
    }

}