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
            AttackNearestEnemy();
            attackTimer = 0f;
        }
    }

    void AttackNearestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

        GameObject nearestEnemy = null;         //用於存儲最近的敵人
        float nearestDistance = Mathf.Infinity; //初始化為無限大

        foreach (GameObject enemy in enemies)   //把 enemies 陣列裡的每一個 enemy 拿出來檢查
        {
            float distance = Vector3.Distance(
                transform.position,
                enemy.transform.position
            );  //計算玩家和敵人之間的距離

            //判斷如果這個距離比目前找到的最近距離更近，且在攻擊範圍內，就更新最近的敵人和距離
            if (distance < nearestDistance && distance <= attackRange)
            {
                nearestDistance = distance;
                nearestEnemy = enemy;
                //Debug.Log("找到更近的敵人: " + enemy.name + " 距離: " + distance);
                //把這隻敵人記錄成目前最近的敵人
            }
        }

        if (nearestEnemy == null)
        {
            return;
        }

        //子彈生成
        GameObject bullet = Instantiate(
           bulletPrefab,
           firePoint.position,
           Quaternion.identity
        );

        Bullet bulletScript = bullet.GetComponent<Bullet>();

        if (bulletScript != null)
        {
            bulletScript.SetTarget(
                nearestEnemy.transform,
                damage
            );
        }
    }
}