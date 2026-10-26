using UnityEngine;

public class EnemyChase : MonoBehaviour
{
    [Header("目標")] //選擇要追逐的目標
    public Transform target;

    [Header("敵人設定")]
    private EnemyStats enemyStats;

    void Start()
    {
        enemyStats = GetComponent<EnemyStats>();
    }

    void Update()
    {
        // 本局結束後，不再執行遊戲操作。
        if (GameManager.IsGameOver || Time.timeScale <= 0f)
        {
            return;
        }

        if (target == null)
        {
            return;
        }

        ChaseTarget();
    }

    void ChaseTarget()
    {
        Vector3 direction = target.position - transform.position;
        direction.y = 0f;

        direction = direction.normalized;

        transform.position += direction * enemyStats.moveSpeed * Time.deltaTime;

        if (direction != Vector3.zero)
        {
            transform.forward = direction;
        }
    }
}