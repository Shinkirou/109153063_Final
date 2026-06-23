using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("敵人")] //敵人預製體
    public GameObject[] enemyPrefabs;

    [Header("玩家")] //玩家(追逐的目標)
    public Transform player;

    [Header("生成設定")]                     //生成設定
    public float spawnInterval = 2f;        //生成間隔
    public float spawnDistance = 15f;       //生成距離

    private float spawnTimer = 0f;

    void Update()
    {
        spawnTimer += Time.deltaTime;

        if (spawnTimer >= spawnInterval)
        {
            SpawnEnemy();
            spawnTimer = 0f;
        }
    }

    void SpawnEnemy()
    {
        Vector2 randomCircle = Random.insideUnitCircle.normalized; //決定生成位置的隨機方向
        Vector3 spawnPosition = player.position + new Vector3(
            randomCircle.x,0f,randomCircle.y
        ) * spawnDistance;

        int randomIndex =
        Random.Range(0, enemyPrefabs.Length);

        GameObject selectedEnemy =enemyPrefabs[randomIndex];

        GameObject enemy =
            Instantiate(
            selectedEnemy,
            spawnPosition,
            Quaternion.identity);

        // 抓取剛生出來的怪物的數值組件
        EnemyStats stats = enemy.GetComponent<EnemyStats>();
        if (stats != null)
        {
            // 根據 GameManager 記錄的難度，直接強化或弱化怪物！
            stats.maxHP = Mathf.RoundToInt(stats.maxHP * GameManager.DifficultyMultiplier);
            stats.damage = Mathf.RoundToInt(stats.damage * GameManager.DifficultyMultiplier);

            // 如果想讓困難模式怪物跑更快，也可以連速度一起乘（選填）
            // stats.moveSpeed *= GameManager.DifficultyMultiplier;
        }

        //讓生成的敵人追逐玩家
        EnemyChase enemyChase = enemy.GetComponent<EnemyChase>();

        if (enemyChase != null)
        {
            enemyChase.target = player;
        }
    }
}
