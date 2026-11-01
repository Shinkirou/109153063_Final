using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class PlayerHealth : MonoBehaviour
{
    [Header("生命設定")]
    [Tooltip("找不到 PlayerStats 時使用的備用最大生命")]
    public int fallbackMaxHP = 100;

    public TMP_Text hpText;
    public Image hpFillImage;

    public bool IsDead { get; private set; }

    private int currentHP;
    private int currentMaxHP;

    private PlayerStats playerStats;

    //護甲減傷公式中的常數值，越大護甲的減傷效果越明顯
    private const float ArmorReductionConstant = 50f;       

    [Header("無敵時間")]                      //玩家受到傷害後的無敵時間，避免連續受到傷害
    [Min(0f)] public float hitInvincibilityDuration = 0.1f;
    [Min(0f)] public float sameAttackerInterval = 0.5f;
    private float hitInvincibleUntil;
    private float rollInvincibleUntil;
    private readonly Dictionary<GameObject, float> attackerNextHit = new Dictionary<GameObject, float>();
    private readonly List<GameObject> expiredAttackers = new List<GameObject>();
    private Coroutine damageFlash;

    public void GrantRollInvincibility(float duration)
    {
        if (!IsDead && !GameManager.IsGameOver)
            rollInvincibleUntil = Mathf.Max(rollInvincibleUntil, Time.time + Mathf.Max(0f, duration));
    }

    [Header("被攻擊視覺效果")]                 //玩家受到傷害時的閃爍效果，讓玩家更明顯地知道自己受到了攻擊
    public Renderer playerRenderer;
    public Color normalColor = Color.white;
    public Color damageColor = Color.red;
    public float flashInterval = 0.1f;

    [Header("吸血設定")]
    private float lifeStealRemainder = 0f;  //尚未累積滿 1 點的小數吸血量

    [Header("測試")]
    [Tooltip("是否在 Console 顯示玩家生命、受傷、回血與吸血資訊")]
    public bool showHealthDebug = false;

    void Start()
    {
        IsDead = false;
        //PlayerHealth 與 PlayerStats 應掛在同一個玩家物件上
        playerStats = GetComponent<PlayerStats>();

        if (playerStats != null)
        {
            //使用 PlayerStats 計算後的最終最大生命
            currentMaxHP = playerStats.FinalMaxHP;
        }
        else
        {
            //找不到 PlayerStats 時使用備用值
            currentMaxHP = fallbackMaxHP;

            Debug.LogError(
                "PlayerHealth 找不到 PlayerStats，" +
                "目前使用備用最大生命。"
            );
        }

        //遊戲開始時恢復為滿血
        currentHP = currentMaxHP;

        if (playerRenderer != null)
        {
            //記錄玩家原本的材質顏色
            normalColor = playerRenderer.material.color;
        }

        UpdateHPUI();

        if (showHealthDebug)
        {
            Debug.Log(
                $"玩家初始生命：{currentHP} / {currentMaxHP}"
            );
        }
    }

    void UpdateHPUI()
    {
        if (hpText != null)
        {
            hpText.text = $"HP: {currentHP} / {currentMaxHP}";
        }

        if (hpFillImage != null)
        {
            hpFillImage.fillAmount =
                currentMaxHP > 0
                ? (float)currentHP / currentMaxHP
                : 0f;
        }
    }

    public void RefreshMaxHP(bool healGainedAmount = true)
    {
        // 本局結束後，不再執行遊戲操作。
        if (IsDead || GameManager.IsGameOver)
        {
            return;
        }

        if (playerStats == null)
        {
            Debug.LogWarning(
                "PlayerHealth 無法更新最大生命，因為找不到 PlayerStats。"
            );
            return;
        }

        int oldMaxHP = currentMaxHP;
        int newMaxHP = playerStats.FinalMaxHP;

        int gainedMaxHP = newMaxHP - oldMaxHP;

        currentMaxHP = newMaxHP;

        // 最大生命增加時，同步補回增加的生命量
        if (healGainedAmount && gainedMaxHP > 0)
        {
            currentHP += gainedMaxHP;
        }

        currentHP = Mathf.Clamp(currentHP, 0, currentMaxHP);

        UpdateHPUI();

        if (showHealthDebug)
        {
            Debug.Log(
                $"最大生命更新：{oldMaxHP} → {currentMaxHP}，" +
                $"目前生命：{currentHP} / {currentMaxHP}"
            );
        }
    }

    public void TakeDamage(int damage, GameObject attacker = null)
    {
        // 本局結束後，不再執行遊戲操作。
        if (IsDead || GameManager.IsGameOver)
        {
            return;
        }

        // 玩家處於無敵狀態時，不受到傷害
        if (damage <= 0 || Time.timeScale <= 0f || Time.time < hitInvincibleUntil || Time.time < rollInvincibleUntil)
        {
            return;
        }

        if (attacker != null && attackerNextHit.TryGetValue(attacker, out float nextHit) && Time.time < nextHit)
            return;

        expiredAttackers.Clear();
        foreach (var entry in attackerNextHit)
            if (entry.Key == null || Time.time >= entry.Value) expiredAttackers.Add(entry.Key);
        foreach (GameObject expired in expiredAttackers) attackerNextHit.Remove(expired);
        if (attacker != null) attackerNextHit[attacker] = Time.time + Mathf.Max(0f, sameAttackerInterval);
        hitInvincibleUntil = Time.time + Mathf.Max(0f, hitInvincibilityDuration);

        //根據護甲計算實際受到的傷害
        int finalDamage = CalculateFinalDamage(damage);

        //扣除生命
        currentHP -= finalDamage;

        //防止生命低於 0
        if (currentHP < 0)
        {
            currentHP = 0;
        }

        UpdateHPUI();

        float displayedArmor =
            playerStats != null
            ? Mathf.Max(playerStats.FinalArmor, 0f)
            : 0f;

        float damageReductionPercent =
            displayedArmor
            / (ArmorReductionConstant + displayedArmor)
            * 100f;

        int reducedDamage = damage - finalDamage;

        if (showHealthDebug)
        {
            Debug.Log(
                $"玩家受到傷害：" +
                $"原始傷害 {damage}，" +
                $"理論減傷率 {damageReductionPercent:0.#}%，" +
                $"減少傷害 {reducedDamage} 點，" +
                $"最終傷害 {finalDamage}，" +
                $"剩餘生命 {currentHP} / {currentMaxHP}"
            );
        }

        //生命歸零時死亡
        if (currentHP <= 0)
        {
            Die();
            return;
        }

        if (damageFlash != null) StopCoroutine(damageFlash);
        damageFlash = StartCoroutine(InvincibleCoroutine());

        //根據玩家最終護甲計算實際受到的傷害
        int CalculateFinalDamage(int originalDamage)
        {
            //防止傳入負數傷害
            originalDamage = Mathf.Max(originalDamage, 0);

            //原始傷害為 0 時，不造成傷害
            if (originalDamage == 0)
            {
                return 0;
            }

            //找不到 PlayerStats 時，不套用護甲減傷
            if (playerStats == null)
            {
                return originalDamage;
            }

            //取得玩家最終護甲，並防止護甲低於 0
            float finalArmor = Mathf.Max(playerStats.FinalArmor, 0f);

            //計算護甲的理論減傷率
            float damageReductionRate =
                finalArmor
                / (ArmorReductionConstant + finalArmor);

            //計算理論上可以減少多少傷害
            float reducedDamageFloat = originalDamage * damageReductionRate;

            //將「減少的傷害量」無條件捨去
            int reducedDamage = Mathf.FloorToInt(reducedDamageFloat);

            //原始傷害扣除減少的傷害
            int finalDamage = originalDamage - reducedDamage;

            //最低仍受到 1 點傷害
            finalDamage = Mathf.Max(finalDamage, 1);

            //將計算結果傳回 TakeDamage()
            return finalDamage;
        }
    }

    public void Heal(int amount)
    {
        // 本局結束後，不再執行遊戲操作。
        if (IsDead || GameManager.IsGameOver)
        {
            return;
        }

        currentHP += amount;

        if (currentHP > currentMaxHP)
        {
            currentHP = currentMaxHP;
        }

        UpdateHPUI();

        if (showHealthDebug)
        {
            Debug.Log("玩家回復生命：" + amount + "，目前生命：" + currentHP);
        }
    }

    //根據造成的實際傷害與吸血率進行回血
    public void ApplyLifeSteal(int damageDealt,float lifeStealRate)
    {
        // 本局結束後，不再執行遊戲操作。
        if (IsDead || GameManager.IsGameOver)
        {
            return;
        }

        //沒有造成傷害或沒有吸血率時，不處理
        if (damageDealt <= 0 || lifeStealRate <= 0f)
        {
            return;
        }

        //本次理論吸血量
        float lifeStealAmount = damageDealt * lifeStealRate;

        //加上先前尚未滿 1 點的小數
        lifeStealRemainder += lifeStealAmount;

        //只取累積後的整數部分
        int healAmount = Mathf.FloorToInt(lifeStealRemainder);

        //尚未累積滿 1 點時，不回血
        if (healAmount <= 0)
        {
            return;
        }

        //扣除已經轉換成實際回血的部分
        lifeStealRemainder -= healAmount;

        Heal(healAmount);

        if (showHealthDebug)
        {
            Debug.Log(
                $"吸血生效：" +
                $"造成傷害 {damageDealt}，" +
                $"吸血率 {lifeStealRate * 100f:0.#}%，" +
                $"回復生命 {healAmount}，" +
                $"剩餘吸血累積 {lifeStealRemainder:0.##}"
            );
        }
    }

    //玩家無敵判定
    IEnumerator InvincibleCoroutine()      
    {
        float timer = 0f;

        while (timer < hitInvincibilityDuration)
        {
            if (playerRenderer != null)
            {
                playerRenderer.material.color = damageColor;
            }

            yield return new WaitForSeconds(Mathf.Max(0.01f, flashInterval));

            if (playerRenderer != null)
            {
                playerRenderer.material.color = normalColor;
            }

            yield return new WaitForSeconds(Mathf.Max(0.01f, flashInterval));

            timer += Mathf.Max(0.01f, flashInterval) * 2f;
        }

        if (playerRenderer != null)
        {
            playerRenderer.material.color = normalColor;
        }

        damageFlash = null;
    }

    void Die()
    {
        if (IsDead)
        {
            return;
        }

        IsDead = true;
        StopAllCoroutines();

        if (playerRenderer != null)
        {
            playerRenderer.material.color = normalColor;
        }

        Debug.Log("角色死亡");
        GameManager.Defeat(DefeatReason.PlayerDeath);

    }    
}
