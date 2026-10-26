using System.Text;
using UnityEngine;

[System.Serializable]
public class EnemySpawnOption
{
    [Tooltip("方便在 Inspector 辨識此敵人，不影響遊戲邏輯")]
    public string enemyName;

    [Tooltip("這個選項要生成的敵人 Prefab")]
    public GameObject enemyPrefab;

    [Tooltip("X 軸為敵人種類進度 0～1，Y 軸為目前生成權重")]
    public AnimationCurve spawnWeightCurve =
        AnimationCurve.Linear(
            0f,
            1f,
            1f,
            1f
        );

    [Tooltip("每次抽中此敵人時，最少生成數量")]
    [Min(1)]
    public int minSpawnCount = 1;

    [Tooltip("每次抽中此敵人時，最多生成數量")]
    [Min(1)]
    public int maxSpawnCount = 1;

    [Tooltip("同一群敵人在生成中心周圍的散布半徑")]
    [Min(0f)]
    public float groupSpreadRadius = 0f;
}


public class EnemySpawner : MonoBehaviour
{
    //敵人與玩家設定
    [Header("敵人種類設定")]
    [Tooltip("所有可由此生成器產生的普通敵人")]
    public EnemySpawnOption[] enemySpawnOptions;


    [Header("玩家")]
    [Tooltip("敵人生成位置與追逐目標所使用的玩家")]
    public Transform player;

    //生成間隔設定
    [Header("生成設定")]
    [Tooltip("基礎生成間隔")]
    public float spawnInterval = 2f;

    [Tooltip("敵人與玩家之間的生成距離")]
    public float spawnDistance = 15f;

    [Tooltip("防止生成間隔過短造成大量敵人瞬間生成")]
    public float minimumSpawnInterval = 0.05f;


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

    //敵人種類權重設定
    [Header("敵人種類權重")]
    [Tooltip("敵人種類權重曲線走到最末端所需時間。9 分鐘 = 540 秒")]
    public float enemyTypeScalingDuration = 540f;

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

    //測試設定
    [Header("測試")]
    [Tooltip("生成敵人時是否在 Console 顯示生命與傷害成長資料")]
    public bool showScalingDebug = false;

    [Tooltip("生成敵人時是否在 Console 顯示抽中的敵人與生成數量")]
    public bool showSpawnSelectionDebug = false;

    //生成計時器
    private float spawnTimer = 0f;

    //目前普通敵人階段經過時間
    private float elapsedTime = 0f;


    //Unity 生命週期
    void Update()
    {
        // 本局結束後，不再執行遊戲操作。
        if (GameManager.IsGameOver || Time.timeScale <= 0f)
        {
            return;
        }

        //累積本局經過時間
        elapsedTime += Time.deltaTime;

        //累積敵人生成計時
        spawnTimer += Time.deltaTime;

        //取得目前時間點的實際生成間隔
        float currentSpawnInterval =
            GetCurrentSpawnInterval();

        if (spawnTimer >= currentSpawnInterval)
        {
            SpawnEnemy();

            spawnTimer = 0f;
        }
    }

    //敵人生成流程
    void SpawnEnemy()
    {
        //玩家不存在時，無法決定生成位置
        if (player == null)
        {
            Debug.LogError(
                "EnemySpawner 尚未指定 Player。"
            );

            return;
        }

        //沒有設定敵人選項時停止生成
        if (enemySpawnOptions == null ||
            enemySpawnOptions.Length == 0)
        {
            Debug.LogError(
                "EnemySpawner 沒有設定任何 Enemy Spawn Option。"
            );

            return;
        }

        //依照目前時間的曲線權重，抽出一種敵人
        EnemySpawnOption selectedOption =
            GetRandomEnemyOption();

        if (selectedOption == null)
        {
            return;
        }

        //取得玩家周圍的群組生成中心
        Vector3 groupCenter =
            GetRandomSpawnPosition();

        //依照該敵人的數量設定生成一隻或一群
        SpawnEnemyGroup(
            selectedOption,
            groupCenter
        );
    }


    //取得玩家周圍的隨機生成位置
    private Vector3 GetRandomSpawnPosition()
    {
        Vector2 randomDirection =
            Random.insideUnitCircle;

        //避免極少數情況抽到接近零的方向
        if (randomDirection.sqrMagnitude <= 0.0001f)
        {
            randomDirection = Vector2.right;
        }

        randomDirection.Normalize();

        Vector3 spawnDirection =
            new Vector3(
                randomDirection.x,
                0f,
                randomDirection.y
            );

        return player.position +
               spawnDirection * spawnDistance;
    }


    //生成一隻或一群指定種類的敵人
    private void SpawnEnemyGroup(
        EnemySpawnOption selectedOption,
        Vector3 groupCenter
    )
    {
        int minimumCount =
            Mathf.Max(
                selectedOption.minSpawnCount,
                1
            );

        int maximumCount =
            Mathf.Max(
                selectedOption.maxSpawnCount,
                minimumCount
            );

        //整數 Random.Range 的最大值不包含在抽選範圍內
        int spawnCount =
            Random.Range(
                minimumCount,
                maximumCount + 1
            );

        float spreadRadius =
            Mathf.Max(
                selectedOption.groupSpreadRadius,
                0f
            );

        for (int i = 0; i < spawnCount; i++)
        {
            Vector2 randomOffset =
                Random.insideUnitCircle *
                spreadRadius;

            Vector3 memberPosition =
                groupCenter +
                new Vector3(
                    randomOffset.x,
                    0f,
                    randomOffset.y
                );

            SpawnSingleEnemy(
                selectedOption.enemyPrefab,
                memberPosition
            );
        }

        if (showSpawnSelectionDebug)
        {
            Debug.Log(
                $"本次生成選擇：" +
                $"{GetOptionDisplayName(selectedOption)}，" +
                $"生成數量：{spawnCount}，" +
                $"群組散布半徑：{spreadRadius:0.00}"
            );
        }
    }


    //生成一隻敵人並完成初始化
    private void SpawnSingleEnemy(
        GameObject enemyPrefab,
        Vector3 spawnPosition
    )
    {
        if (enemyPrefab == null)
        {
            Debug.LogWarning(
                "嘗試生成敵人時，Enemy Prefab 為空。"
            );

            return;
        }

        GameObject enemy =
            Instantiate(
                enemyPrefab,
                spawnPosition,
                Quaternion.identity
            );

        //套用敵人的難度與時間成長
        ApplyEnemyScaling(enemy);

        //讓生成的敵人追逐玩家
        EnemyChase enemyChase =
            enemy.GetComponent<EnemyChase>();

        if (enemyChase != null)
        {
            enemyChase.target = player;
        }
        else
        {
            Debug.LogWarning(
                $"生成的敵人 {enemy.name} 找不到 EnemyChase。"
            );
        }
    }

    //敵人種類加權抽選
    //依照目前時間的權重曲線抽選一種敵人
    private EnemySpawnOption GetRandomEnemyOption()
    {
        float normalizedTime =
            GetEnemyTypeNormalizedTime();

        float totalWeight = 0f;

        EnemySpawnOption lastValidOption = null;

        //第一次迴圈：計算總權重
        for (int i = 0;
             i < enemySpawnOptions.Length;
             i++)
        {
            EnemySpawnOption option =
                enemySpawnOptions[i];

            if (!IsValidSpawnOption(option))
            {
                continue;
            }

            float currentWeight =
                GetCurrentSpawnWeight(
                    option,
                    normalizedTime
                );

            if (currentWeight <= 0f)
            {
                continue;
            }

            totalWeight += currentWeight;
            lastValidOption = option;
        }

        //所有曲線目前都是 0，無法抽選
        if (totalWeight <= 0f)
        {
            Debug.LogWarning(
                "目前所有敵人的生成權重都是 0，" +
                "因此本次不會生成敵人。"
            );

            return null;
        }

        float roll =
            Random.Range(
                0f,
                totalWeight
            );

        //第二次迴圈：依權重區間找出抽中的敵人
        for (int i = 0;
             i < enemySpawnOptions.Length;
             i++)
        {
            EnemySpawnOption option =
                enemySpawnOptions[i];

            if (!IsValidSpawnOption(option))
            {
                continue;
            }

            float currentWeight =
                GetCurrentSpawnWeight(
                    option,
                    normalizedTime
                );

            if (currentWeight <= 0f)
            {
                continue;
            }

            if (roll < currentWeight)
            {
                return option;
            }

            roll -= currentWeight;
        }

        //處理浮點數極小誤差
        return lastValidOption;
    }


    //判斷敵人生成選項是否有效
    private bool IsValidSpawnOption(EnemySpawnOption option)
    {
        return option != null &&
               option.enemyPrefab != null;
    }


    //取得目前敵人種類曲線進度
    private float GetEnemyTypeNormalizedTime()
    {
        if (enemyTypeScalingDuration <= 0f)
        {
            return 1f;
        }

        return Mathf.Clamp01(
            elapsedTime /
            enemyTypeScalingDuration
        );
    }


    //取得指定敵人在目前時間的生成權重
    private float GetCurrentSpawnWeight(EnemySpawnOption option,float normalizedTime)
    {
        if (option == null ||
            option.spawnWeightCurve == null)
        {
            return 0f;
        }

        return Mathf.Max(
            option.spawnWeightCurve.Evaluate(
                normalizedTime
            ),
            0f
        );
    }


    private string GetOptionDisplayName(EnemySpawnOption option)
    {
        if (option == null)
        {
            return "未知敵人";
        }

        if (!string.IsNullOrWhiteSpace(option.enemyName))
        {
            return option.enemyName;
        }

        if (option.enemyPrefab != null)
        {
            return option.enemyPrefab.name;
        }

        return "未命名敵人";
    }

    //生成間隔
    private float GetCurrentSpawnInterval()
    {
        //將 0～420 秒轉換為 0～1
        float normalizedSpawnTime =
            spawnIntervalScalingDuration > 0f
            ? Mathf.Clamp01(
                elapsedTime /
                spawnIntervalScalingDuration
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
        float currentSpawnInterval =
            spawnInterval *
            intervalMultiplier;

        //避免曲線設成 0 或負數，造成每幀生成
        return Mathf.Max(
            currentSpawnInterval,
            minimumSpawnInterval
        );
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
            ? Mathf.Clamp01(
                elapsedTime /
                scalingDuration
            )
            : 1f;

        //讀取目前時間點的曲線倍率
        float healthTimeMultiplier =
            Mathf.Max(
                healthMultiplierCurve.Evaluate(
                    normalizedTime
                ),
                0f
            );

        float damageTimeMultiplier =
            Mathf.Max(
                damageMultiplierCurve.Evaluate(
                    normalizedTime
                ),
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
                    baseMaxHP *
                    difficultyMultiplier *
                    healthTimeMultiplier
                ),
                1
            );

        stats.damage =
            Mathf.Max(
                Mathf.RoundToInt(
                    baseDamage *
                    difficultyMultiplier *
                    damageTimeMultiplier
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

    //ContextMenu 測試
    [ContextMenu("測試／輸出目前敵人成長倍率")]
    private void PrintCurrentScaling()
    {
        float normalizedTime =
            scalingDuration > 0f
            ? Mathf.Clamp01(
                elapsedTime /
                scalingDuration
            )
            : 1f;

        float healthMultiplier =
            Mathf.Max(
                healthMultiplierCurve.Evaluate(
                    normalizedTime
                ),
                0f
            );

        float damageMultiplier =
            Mathf.Max(
                damageMultiplierCurve.Evaluate(
                    normalizedTime
                ),
                0f
            );

        float normalizedSpawnTime =
            spawnIntervalScalingDuration > 0f
            ? Mathf.Clamp01(
                elapsedTime /
                spawnIntervalScalingDuration
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


    [ContextMenu("測試／輸出目前敵人種類權重")]
    private void PrintCurrentEnemyWeights()
    {
        if (enemySpawnOptions == null ||
            enemySpawnOptions.Length == 0)
        {
            Debug.LogWarning(
                "尚未設定任何 Enemy Spawn Option。"
            );

            return;
        }

        float normalizedTime =
            GetEnemyTypeNormalizedTime();

        float totalWeight = 0f;

        for (int i = 0;
             i < enemySpawnOptions.Length;
             i++)
        {
            EnemySpawnOption option =
                enemySpawnOptions[i];

            if (!IsValidSpawnOption(option))
            {
                continue;
            }

            totalWeight +=
                GetCurrentSpawnWeight(
                    option,
                    normalizedTime
                );
        }

        StringBuilder result =
            new StringBuilder();

        result.AppendLine(
            "目前敵人種類生成權重："
        );

        result.AppendLine(
            $"經過時間：{elapsedTime:0.0} 秒"
        );

        result.AppendLine(
            $"曲線進度：{normalizedTime * 100f:0.#}%"
        );

        for (int i = 0;
             i < enemySpawnOptions.Length;
             i++)
        {
            EnemySpawnOption option =
                enemySpawnOptions[i];

            if (!IsValidSpawnOption(option))
            {
                result.AppendLine(
                    $"索引 {i}：設定不完整"
                );

                continue;
            }

            float weight =
                GetCurrentSpawnWeight(
                    option,
                    normalizedTime
                );

            float probability =
                totalWeight > 0f
                ? weight / totalWeight * 100f
                : 0f;

            result.AppendLine(
                $"{GetOptionDisplayName(option)}：" +
                $"權重 {weight:0.00}，" +
                $"實際機率 {probability:0.00}%"
            );
        }

        result.AppendLine(
            $"總權重：{totalWeight:0.00}"
        );

        Debug.Log(result.ToString());
    }
}