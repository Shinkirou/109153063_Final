using UnityEngine;

public class EnemyDamage : MonoBehaviour
{
    private EnemyStats enemyStats;

    void Start()
    {
        enemyStats = GetComponent<EnemyStats>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();

        if (playerHealth != null)
        {
            playerHealth.TakeDamage(enemyStats.damage);
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();

        if (playerHealth != null)
        {
            playerHealth.TakeDamage(enemyStats.damage);
        }
    }
}
