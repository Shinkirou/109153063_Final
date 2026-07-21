using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    //敵人與玩家
    [Header("敵人")]
    [Tooltip("可以由此生成器產生的普通敵人 Prefab")]
    public GameObject[] enemyPrefabs;

    [Header("玩家")]
    [Tooltip("敵人生成位置與追逐目標所使用的玩家")]
    public Transform player;

    //生成設定
    [Header("生成設定")]
    [Tooltip("每隔多少秒生成一隻敵人")]
    public float spawnInterval = 2f;

    [Tooltip("敵人與玩家之間的生成距離")]
    public float spawnDistance = 15f;

    //生成計時器
    private float spawnTimer = 0f;

    [Header("敵人生成頻率成長")]
    [Tooltip("生成間隔縮短到曲線最末端需要的時間。7 分鐘 = 420 秒")]
    public float spawnIntervalScalingDuration = 420f;

    [Tooltip("X 軸為生成頻率進度 0～1，Y 軸為生成間隔倍率")]
    public AnimationCurve spawnIntervalMultiplierCurve =
        AnimationCurve.Linear(
            0f,
            1f,
            1f,
            0.25f
        );

    [Tooltip("防止生成間隔過短造成大量敵人瞬間生成")]
    public float minimumSpawnInterval = 0.05f;

    //敵人成長設定
    [Header("普通敵人時間成長")]
    [Tooltip("敵人成長到曲線最末端需要的時間。9 分鐘 = 540 秒")]
    public float scalingDuration = 540f;

    [Tooltip("X 軸為關卡進度 0～1，Y 軸為敵人生命倍率")]
    public AnimationCurve healthMultiplierCurve =
        AnimationCurve.Linear(
            0f,
            1f,
            1f,
            5f
        );

    [Tooltip("X 軸為關卡進度 0～1，Y 軸為敵人傷害倍率")]
    public AnimationCurve damageMultiplierCurve =
        AnimationCurve.Linear(
            0f,
            1f,
            1f,
            3f
        );

    [Header("測試")]
    [Tooltip("生成敵人時是否在 Console 顯示成長資料")]
    public bool showScalingDebug = false;

    //目前普通敵人發育階段已經經過的時間
    private float elapsedTime = 0f;


    [ContextMenu("測試／輸出目前敵人成長倍率")]
    private void PrintCurrentScaling()
    {
        float normalizedTime =
            scalingDuration > 0f
            ? Mathf.Clamp01(elapsedTime / scalingDuration)
            : 1f;

        float healthMultiplier =
            Mathf.Max(
                healthMultiplierCurve.Evaluate(normalizedTime),
                0f
            );

        float damageMultiplier =
            Mathf.Max(
                damageMultiplierCurve.Evaluate(normalizedTime),
                0f
            );

        float normalizedSpawnTime =
            spawnIntervalScalingDuration > 0f
            ? Mathf.Clamp01(
                elapsedTime / spawnIntervalScalingDuration
            )
            : 1f;

        float spawnIntervalMultiplier =
            Mathf.Max(
                spawnIntervalMultiplierCurve.Evaluate(
                    normalizedSpawnTime
                ),
                0f
            );

        float currentSpawnInterval =
            GetCurrentSpawnInterval();

        Debug.Log(
            $"目前敵人成長測試：" +
            $"經過時間 {elapsedTime:0.0} 秒，" +
            $"生命／傷害進度 {normalizedTime * 100f:0.#}%，" +
            $"生命倍率 {healthMultiplier:0.00}，" +
            $"傷害倍率 {damageMultiplier:0.00}，" +
            $"生成頻率進度 {normalizedSpawnTime * 100f:0.#}%，" +
            $"生成間隔倍率 {spawnIntervalMultiplier:0.00}，" +
            $"目前生成間隔 {currentSpawnInterval:0.00} 秒，" +
            $"難度倍率 {GameManager.DifficultyMultiplier:0.00}"
        );
    }

    //==================================================
    //Unity 生命週期
    void Update()
    {
        //累積本局經過時間
        elapsedTime += Time.deltaTime;
        
        //累積敵人生成計時
        spawnTimer += Time.deltaTime;

        //取得目前時間點的實際生成間隔
        float currentSpawnInterval = GetCurrentSpawnInterval();

        if (spawnTimer >= currentSpawnInterval)
        {
            SpawnEnemy();
            spawnTimer = 0f;
        }
    }

    private float GetCurrentSpawnInterval()
    {
        //將 0～420 秒轉換為 0～1
        float normalizedSpawnTime =
            spawnIntervalScalingDuration > 0f
            ? Mathf.Clamp01(
                elapsedTime / spawnIntervalScalingDuration
            )
            : 1f;

        //讀取目前生成間隔倍率
        float intervalMultiplier =
            Mathf.Max(
                spawnIntervalMultiplierCurve.Evaluate(
                    normalizedSpawnTime
                ),
                0f
            );

        //基礎間隔 × 當前倍率
        float currentSpawnInterval = spawnInterval * intervalMultiplier;

        //避免曲線設成 0 或負數，造成每幀生成
        return Mathf.Max(
            currentSpawnInterval,
            minimumSpawnInterval
        );
    }


    void SpawnEnemy()
    {
        //玩家不存在時無法決定生成位置
        if (player == null)
        {
            Debug.LogError(
                "EnemySpawner 尚未指定 Player。"
            );
            return;
        }

        //沒有設定敵人 Prefab 時停止生成
        if (enemyPrefabs == null || enemyPrefabs.Length == 0)
        {
            Debug.LogError(
                "EnemySpawner 沒有設定任何 Enemy Prefab。"
            );
            return;
        }

        //取得玩家周圍的隨機方向
        Vector2 randomCircle = Random.insideUnitCircle.normalized; //決定生成位置的隨機方向
        Vector3 spawnPosition = player.position + new Vector3(
            randomCircle.x,0f,randomCircle.y
        ) * spawnDistance;

        //隨機選擇一種敵人
        int randomIndex = Random.Range(0, enemyPrefabs.Length);

        GameObject selectedEnemy =enemyPrefabs[randomIndex];

        //避免陣列中存在空的 Prefab
        if (selectedEnemy == null)
        {
            Debug.LogWarning(
                $"EnemySpawner 的 Enemy Prefabs " +
                $"索引 {randomIndex} 沒有指定 Prefab。"
            );
            return;
        }

        //生成敵人
        GameObject enemy =
            Instantiate(
            selectedEnemy,
            spawnPosition,
            Quaternion.identity);

        //套用敵人的難度與時間成長
        ApplyEnemyScaling(enemy);

        //讓生成的敵人追逐玩家
        EnemyChase enemyChase = enemy.GetComponent<EnemyChase>();

        if (enemyChase != null)
        {
            enemyChase.target = player;
        }
    }

    //敵人成長
    private void ApplyEnemyScaling(GameObject enemy)
    {
        EnemyStats stats =
            enemy.GetComponent<EnemyStats>();

        if (stats == null)
        {
            Debug.LogWarning(
                $"生成的敵人 {enemy.name} 找不到 EnemyStats。"
            );
            return;
        }

        //保存 Prefab 原始數值，供 Debug 顯示
        int baseMaxHP = stats.maxHP;
        int baseDamage = stats.damage;

        //將經過時間轉換成 0～1 的關卡進度
        float normalizedTime =
            scalingDuration > 0f
            ? Mathf.Clamp01(elapsedTime / scalingDuration)
            : 1f;

        //讀取目前時間點的曲線倍率
        float healthTimeMultiplier =
            Mathf.Max(
                healthMultiplierCurve.Evaluate(normalizedTime),
                0f
            );

        float damageTimeMultiplier =
            Mathf.Max(
                damageMultiplierCurve.Evaluate(normalizedTime),
                0f
            );

        //取得遊戲難度倍率
        float difficultyMultiplier =
            Mathf.Max(
                GameManager.DifficultyMultiplier,
                0f
            );

        //基礎數值 × 難度倍率 × 時間倍率
        stats.maxHP =
            Mathf.Max(
                Mathf.RoundToInt(
                    baseMaxHP
                    * difficultyMultiplier
                    * healthTimeMultiplier
                ),
                1
            );

        stats.damage =
            Mathf.Max(
                Mathf.RoundToInt(
                    baseDamage
                    * difficultyMultiplier
                    * damageTimeMultiplier
                ),
                1
            );

        if (showScalingDebug)
        {
            Debug.Log(
                $"生成敵人：{stats.enemyName}，" +
                $"經過時間：{elapsedTime:0.0} 秒，" +
                $"關卡進度：{normalizedTime * 100f:0.#}%，" +
                $"生命倍率：{healthTimeMultiplier:0.00}，" +
                $"傷害倍率：{damageTimeMultiplier:0.00}，" +
                $"難度倍率：{difficultyMultiplier:0.00}，" +
                $"生命：{baseMaxHP} → {stats.maxHP}，" +
                $"傷害：{baseDamage} → {stats.damage}"
            );
        }
    }
}


