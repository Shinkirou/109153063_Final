using UnityEngine;

public class EnemyDamage : MonoBehaviour
{
    [Header("攻擊設定")]
    public float attackInterval = 1f;

    private float attackTimer;

    private EnemyStats enemyStats;

    void Start()
    {
        enemyStats = GetComponent<EnemyStats>();
    }

    void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        attackTimer += Time.deltaTime;

        if (attackTimer >= attackInterval)
        {
            PlayerHealth playerHealth =
                other.GetComponent<PlayerHealth>();

            if (playerHealth != null)
            {
                playerHealth.TakeDamage(
                    enemyStats.damage
                );
            }

            attackTimer = 0f;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            attackTimer = 0f;
        }
    }
}
