using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public GameObject expGemPrefab;                 //經驗值預製體

    private EnemyStats enemyStats;
    private int currentHP; 


    void Start()
    {
        enemyStats = GetComponent<EnemyStats>();    //取得怪物數值
        currentHP = enemyStats.maxHP;               //將怪物生命加入計算
    }

    public void TakeDamage(int damage)
    {
        currentHP -= damage;

        Debug.Log(enemyStats.enemyName + " 受到傷害，目前血量：" + currentHP);

        if (currentHP <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        if (expGemPrefab != null)
        {
            Instantiate(
                expGemPrefab,
                transform.position,
                Quaternion.identity
            );
        }
        Debug.Log(enemyStats.enemyName + " 死亡");
        Destroy(gameObject);
    }
}