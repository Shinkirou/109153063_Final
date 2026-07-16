using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header("子彈設定")]
    public GameObject bulletPrefab;
    public Transform firePoint;

    private PlayerStats playerStats;

    //用來累積距離上次攻擊經過了多少時間
    private float attackTimer = 0f;

    void Start()
    {
        playerStats = GetComponent<PlayerStats>();

        if (playerStats == null)
        {
            Debug.LogError(
                "PlayerAttack 找不到 PlayerStats，" +
                "請確認兩個腳本都掛在同一個 Player 物件上。"
            );
        }
    }

    void Update()
    {
        if (playerStats == null)
        {
            return;
        }

        //累積每一幀經過的時間
        attackTimer += Time.deltaTime;

        //透過讀取PlayerStats的AttackInterval，判斷是否可以攻擊
        if (attackTimer >= playerStats.AttackInterval)
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
        GameObject[] enemies =
            GameObject.FindGameObjectsWithTag("Enemy");

        GameObject nearestEnemy = null;
        float nearestDistance = Mathf.Infinity;

        foreach (GameObject enemy in enemies)
        {
            float distance = Vector3.Distance(
                transform.position,
                enemy.transform.position
            );

            /*
             * 只有比目前最近敵人更近，
             * 並且在最終攻擊距離內的敵人才會被選中。
             */
            if (
                distance < nearestDistance &&
                distance <= playerStats.FinalAttackRange
            )
            {
                nearestDistance = distance;
                nearestEnemy = enemy;
            }
        }

        return nearestEnemy;
    }

    void Shoot(GameObject target)
    {
        if (bulletPrefab == null)
        {
            Debug.LogError(
                "PlayerAttack 尚未指定 Bullet Prefab。"
            );

            return;
        }

        if (firePoint == null)
        {
            Debug.LogError(
                "PlayerAttack 尚未指定 Fire Point。"
            );

            return;
        }

        GameObject bullet = Instantiate(
            bulletPrefab,
            firePoint.position,
            Quaternion.identity
        );

        Bullet bulletScript =
            bullet.GetComponent<Bullet>();

        if (bulletScript != null)
        {
            bulletScript.SetTarget(
                target.transform,
                playerStats.FinalDamage
            );
        }
        else
        {
            Debug.LogWarning(
                "產生的子彈物件上找不到 Bullet.cs。"
            );
        }
    }
}