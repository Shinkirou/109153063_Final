using UnityEngine;

public class EnemyChase : MonoBehaviour
{
    [Header("Target")] //選擇要追逐的目標
    public Transform target;

    [Header("Enemy Settings")]
    public float moveSpeed = 3f;

    void Update()
    {
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

        transform.position += direction * moveSpeed * Time.deltaTime;

        if (direction != Vector3.zero)
        {
            transform.forward = direction;
        }
    }
}