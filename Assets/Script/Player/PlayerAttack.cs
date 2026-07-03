using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    private PlayerStats playerStats;            //讀取角色數值


    [Header("子彈設定")]
    public GameObject bulletPrefab;
    public Transform firePoint;

    private float attackTimer = 0f;

    void Start()
    {       
        playerStats = GetComponent<PlayerStats>();
        
        if(playerStats == null)
        {
            Debug.Log("找不到*玩家數值*這個腳本");
        }    
    }

    void Update()
    {
        if (playerStats == null)
        {
            return;
        }

        attackTimer += Time.deltaTime;

        //攻擊判定，確認攻擊間隔大於玩家攻擊間隔時，先尋找目標，然後發射子彈
        if (attackTimer >= playerStats.attackInterval)
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
            if (distance < nearestDistance && distance <= playerStats.attackRange)
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
            bulletScript.SetTarget(target.transform,playerStats.FinalDamage);
        }
    }
}