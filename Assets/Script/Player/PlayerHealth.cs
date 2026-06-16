using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class PlayerHealth : MonoBehaviour
{
    [Header("生命設定")]
    public int maxHP = 100;
    public TMP_Text hpText;
    public Image hpFillImage;

    private int currentHP;

    [Header("無敵時間")]                      //玩家受到傷害後的無敵時間，避免連續受到傷害
    public float invincibleTime = 0.8f;
    private bool isInvincible = false;

    [Header("被攻擊視覺效果")]                 //玩家受到傷害時的閃爍效果，讓玩家更明顯地知道自己受到了攻擊
    public Renderer playerRenderer;
    public Color normalColor = Color.white;
    public Color damageColor = Color.red;
    public float flashInterval = 0.1f;

    void Start()
    {
        currentHP = maxHP;

        if (playerRenderer != null)
        {
            normalColor = playerRenderer.material.color;    //記錄玩家原本的顏色
        }

        UpdateHPUI();
        Debug.Log("玩家生命：" + currentHP);
    }

    void UpdateHPUI()
    {
        if (hpText != null)
        {
            hpText.text = "HP: " + currentHP + " / " + maxHP;
        }

        if (hpFillImage != null)
        {
            hpFillImage.fillAmount = (float)currentHP / maxHP;
        }
    }

    public void TakeDamage(int damage)
    {

        if (isInvincible) 
        {
            return;
        }//如果玩家正在無敵狀態，則不會受到傷害

        currentHP -= damage;

        Debug.Log("玩家受到傷害：" + damage + "，剩餘生命：" + currentHP);

        if (currentHP < 0)
        {
            currentHP = 0;
        }

        UpdateHPUI();

        Debug.Log("玩家受到傷害：" + damage + "，剩餘生命：" + currentHP);

        if (currentHP <= 0)
        {
            Die();
            return;
        }

        StartCoroutine(InvincibleCoroutine());
    }
    public void Heal(int amount)
    {
        currentHP += amount;

        if (currentHP > maxHP)
        {
            currentHP = maxHP;
        }

        UpdateHPUI();

        Debug.Log("玩家回復生命：" + amount + "，目前生命：" + currentHP);
    }

    IEnumerator InvincibleCoroutine()
        //玩家無敵判定
    {
        isInvincible = true;

        float timer = 0f;

        while (timer < invincibleTime)
        {
            if (playerRenderer != null)
            {
                playerRenderer.material.color = damageColor;
            }

            yield return new WaitForSeconds(flashInterval);

            if (playerRenderer != null)
            {
                playerRenderer.material.color = normalColor;
            }

            yield return new WaitForSeconds(flashInterval);

            timer += flashInterval * 2f;
        }

        if (playerRenderer != null)
        {
            playerRenderer.material.color = normalColor;
        }

        isInvincible = false;
    }

    void Die()
    {
        Debug.Log("Game Over");
        Time.timeScale = 0f;
    }

    
}
