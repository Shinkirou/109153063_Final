using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header("子彈設定")]
    public GameObject bulletPrefab;
    public Transform firePoint;

    private RifleAugments rifle;
    private bool pendingRightShot;
    private float rightShotDelay;
    private float EffectiveAttackInterval => playerStats.AttackInterval / (1f + (rifle != null ? (rifle.heatEnabled ? Mathf.Max(0f, rifle.heatMaxAttackSpeedBonus) * rifle.currentHeat : 0f) + rifle.OverdriveBonus : 0f));
    private PlayerStats playerStats;        //用來取得玩家的攻擊屬性
    private PlayerHealth playerHealth;      //用來取得玩家的吸血屬性


    //用來累積距離上次攻擊經過了多少時間
    private float attackTimer = 0f;

    void Start()
    {
        rifle = GetComponent<RifleAugments>();
        playerStats = GetComponent<PlayerStats>();
        playerHealth = GetComponent<PlayerHealth>();

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
        // 本局結束後，不再執行遊戲操作。
        if (GameManager.IsGameOver || Time.timeScale <= 0f)
        {
            return;
        }

        if (playerStats == null)
        {
            return;
        }

        //累積每一幀經過的時間
        if (rifle != null)
        {
            rifle.TickOverdrive();
            rifle.UpdateHeat(FindNearestEnemy() != null);
        }
        if (rifle != null && rifle.isOverheated)
        {
            attackTimer = 0f;
            pendingRightShot = false;
            return;
        }
        if (pendingRightShot)
        {
            rightShotDelay -= Time.deltaTime;
            if (rightShotDelay <= 0f)
            {
                pendingRightShot = false;
                if (rifle != null && rifle.dualFireEnabled)
                {
                    GameObject rightTarget = FindNearestEnemy();
                    if (rightTarget != null) FireDualShot(rightTarget, false);
                }
            }
        }

        attackTimer += Time.deltaTime;

        //透過讀取PlayerStats的AttackInterval，判斷是否可以攻擊
        if (attackTimer >= EffectiveAttackInterval)
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

        if (rifle != null && rifle.dualFireEnabled)
        {
            FireDualShot(target, true);
            rightShotDelay = EffectiveAttackInterval * Mathf.Clamp(rifle.dualFireDelayFraction, 0f, 0.95f);
            pendingRightShot = true;
        }
        else
        {
            pendingRightShot = false;
            SpawnBullet(target, firePoint.position, playerStats.FinalDamage, Vector3.zero);
        }
    }

    private void FireDualShot(GameObject target, bool isLeft)
    {
        Transform muzzle = isLeft ? rifle.leftFirePoint : rifle.rightFirePoint;
        Vector3 forward = target.transform.position - firePoint.position;
        forward.y = 0f;
        Vector3 side = forward.sqrMagnitude > 0.0001f
            ? Vector3.Cross(Vector3.up, forward.normalized)
            : transform.right;
        Vector3 offset = side * rifle.dualFireSeparation * 0.5f * (isLeft ? 1f : -1f);
        Vector3 position = muzzle != null ? muzzle.position : firePoint.position + offset;
        int shotDamage = Mathf.Max(1, Mathf.RoundToInt(playerStats.FinalDamage * rifle.dualFireDamageMultiplier));
        SpawnBullet(target, position, shotDamage, offset);
    }
    private void SpawnBullet(GameObject target, Vector3 position, int shotDamage, Vector3 aimOffset)
    {
        GameObject bullet = Instantiate(
            bulletPrefab,
            position,
            Quaternion.identity
        );

        Bullet bulletScript =
            bullet.GetComponent<Bullet>();

        if (bulletScript != null)
        {
            bulletScript.SetTarget(
                target.transform, shotDamage,
                playerStats.criticalRate, playerStats.FinalCriticalDamage,
                playerStats.lifeSteal, playerHealth, aimOffset
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