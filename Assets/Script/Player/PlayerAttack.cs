using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header("攻擊設定")]
    public float attackRange = 6f;
    public float attackInterval = 1f;
    public int damage = 3;

    [Header("子彈設定")]
    public GameObject bulletPrefab;
    public Transform firePoint;

    private float attackTimer = 0f;

    void Update()
    {
        attackTimer += Time.deltaTime;

        if (attackTimer >= attackInterval)
        {
            GameObject target = FindNearestEnemy();

            if (target != null)
            {
                Shoot(target);
            }

            attackTimer = 0f;
        }
    }

    GameObject FindNearestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

        GameObject nearestEnemy = null;
        float nearestDistance = Mathf.Infinity;

        foreach (GameObject enemy in enemies)
        {
            float distance = Vector3.Distance(
                transform.position,
                enemy.transform.position
            );  //計算玩家和敵人之間的距離
                
            //判斷如果這個距離比目前找到的最近距離更近，且在攻擊範圍內
            if (distance < nearestDistance && distance <= attackRange)
            {
                nearestDistance = distance;
                nearestEnemy = enemy;
                //Debug.Log("找到更近的敵人: " + enemy.name + " 距離: " + distance);
                //把這隻敵人記錄成目前最近的敵人
            }
        }

        return nearestEnemy;
    }

    void Shoot(GameObject target)
    {
       
        GameObject bullet = Instantiate(
            bulletPrefab,
            firePoint.position,
            Quaternion.identity
        );

        Bullet bulletScript = bullet.GetComponent<Bullet>();

        if (bulletScript != null)
        {
            bulletScript.SetTarget(target.transform,damage);
        }
    }
}