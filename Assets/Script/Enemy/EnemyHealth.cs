using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    private EnemyDrop enemyDrop;
    private EnemyStats enemyStats;
    private int currentHP;

    [Header("測試")]
    [Tooltip("是否在 Console 顯示敵人的受傷與死亡資訊")]
    public bool showEnemyHealthDebug = false;

    void Start()
    {
        //取得怪物數值
        enemyStats = GetComponent<EnemyStats>();
        //取得怪物掉落物件
        enemyDrop = GetComponent<EnemyDrop>();      

        if (enemyStats == null)
        {
            Debug.LogError(
                $"{gameObject.name} 找不到 EnemyStats。"
            );

            enabled = false;
            return;
        }

        //將怪物最大生命加入目前生命計算
        currentHP = enemyStats.maxHP;               
    }

    public void TakeDamage(int damage)
    {
        if (damage <= 0)
        {
            return;
        }

        currentHP -= damage;

        if (showEnemyHealthDebug)
        {
            Debug.Log(
                $"{enemyStats.enemyName} 受到傷害，" +
                $"本次傷害：{damage}，" +
                $"目前血量：{currentHP} / {enemyStats.maxHP}"
            );
        }

        if (currentHP <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        //由 EnemyDrop 統一處理經驗球與未來其他掉落物
        if (enemyDrop != null)
        {
            enemyDrop.DropRewards();
        }

        else
        {
            Debug.LogWarning(
                $"{gameObject.name} 找不到 EnemyDrop，" +
                $"本次死亡不會產生掉落物。"
            );
        }

        if (showEnemyHealthDebug)
        {
            Debug.Log($"{enemyStats.enemyName} 死亡");
        }

        Destroy(gameObject);
    }
}