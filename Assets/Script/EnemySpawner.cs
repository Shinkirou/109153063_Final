using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("敵人")] //敵人預製體
    public GameObject enemyPrefab;

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

        GameObject enemy = Instantiate(
            enemyPrefab,
            spawnPosition,
            Quaternion.identity
        );

        //讓生成的敵人追逐玩家
        EnemyChase enemyChase = enemy.GetComponent<EnemyChase>();

        if (enemyChase != null)
        {
            enemyChase.target = player;
        }
    }
}
