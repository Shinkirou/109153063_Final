using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("生命設定")]
    public int maxHP = 100;

    private int currentHP;

    void Start()
    {
        currentHP = maxHP;

        Debug.Log("玩家生命：" + currentHP);
    }

    public void TakeDamage(int damage)
    {
        currentHP -= damage;

        Debug.Log("玩家受到傷害：" + damage + "，剩餘生命：" + currentHP);

        if (currentHP <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log("Game Over");

        Time.timeScale = 0f;
    }

    public void Heal(int amount)
    {
        currentHP += amount;

        if (currentHP > maxHP)
        {
            currentHP = maxHP;
        }

        Debug.Log("玩家回復生命：" + amount + "，目前生命：" + currentHP);
    }
}
