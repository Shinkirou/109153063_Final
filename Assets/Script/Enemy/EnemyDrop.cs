using UnityEngine;

public class EnemyDrop : MonoBehaviour
{

    //掉落顆數資料
    [System.Serializable]
    public class DropCountWeight
    {
        [Tooltip("抽中此項時，掉落多少顆經驗球")]
        public int dropCount = 1;

        [Tooltip("此掉落顆數的抽選權重")]
        public float weight = 1f;
    }

    //經驗球品質
    public enum ExpGemQuality
    {
        Small,
        Medium,
        Large,
        Super
    }

    [System.Serializable]
    public class ExpGemDropWeight
    {
        [Tooltip("經驗球品質")]
        public ExpGemQuality quality;

        [Tooltip("此品質所使用的經驗球 Prefab")]
        public GameObject gemPrefab;

        [Tooltip("此品質的抽選權重")]
        public float weight = 1f;
    }

    //==================================================
    //Inspector 設定
    [Header("掉落顆數權重")]
    [Tooltip("依權重決定敵人死亡後掉落幾顆經驗球")]
    public DropCountWeight[] dropCountWeights;

    [Header("經驗球品質權重")]
    [Tooltip("每一顆經驗球都會獨立抽選一次品質")]
    public ExpGemDropWeight[] gemQualityWeights;

    [Header("掉落位置")]
    [Tooltip("經驗球生成時，在敵人周圍分散的範圍")]
    public float dropSpreadRadius = 0.8f;

    [Header("測試")]
    [Tooltip("是否在 Console 顯示本次掉落結果")]
    public bool showDropDebug = false;


    private PlayerStats playerStats;        //玩家數值，未來可用於掉落數量或品質的加成
    [Header("幸運對品質權重的影響")]
    [Tooltip("每級幸運降低多少小型經驗球權重")]
    public float smallWeightReductionPerLuck = 2f;

    [Tooltip("小型經驗球最低保留權重")]
    public float minimumSmallWeight = 15f;

    [Tooltip("每級幸運增加多少中型經驗球權重")]
    public float mediumWeightBonusPerLuck = 1f;

    [Tooltip("每級幸運增加多少大型經驗球權重")]
    public float largeWeightBonusPerLuck = 0.5f;

    [Tooltip("每級幸運增加多少超級經驗球權重")]
    public float superWeightBonusPerLuck = 0.15f;

    [Tooltip("超級經驗球最高權重")]
    public float maximumSuperWeight = 10f;

    private void Start()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

        if (playerObject == null)
        {
            Debug.LogWarning(
                $"{gameObject.name} 的 EnemyDrop 找不到 Player。"
            );

            return;
        }

        playerStats =
            playerObject.GetComponent<PlayerStats>();

        if (playerStats == null)
        {
            Debug.LogWarning(
                "Player 物件找不到 PlayerStats，" +
                "經驗球品質將使用基礎權重。"
            );
        }
    }

    [ContextMenu("測試／輸出目前經驗球品質權重")]
    private void PrintCurrentGemWeights()
    {
        int luckLevel =
            playerStats != null
            ? Mathf.Max(playerStats.luckLevel, 0)
            : 0;

        float smallWeight = 0f;
        float mediumWeight = 0f;
        float largeWeight = 0f;
        float superWeight = 0f;

        foreach (ExpGemDropWeight option in gemQualityWeights)
        {
            if (option == null)
            {
                continue;
            }

            float adjustedWeight =
                GetAdjustedGemWeight(option);

            switch (option.quality)
            {
                case ExpGemQuality.Small:
                    smallWeight = adjustedWeight;
                    break;

                case ExpGemQuality.Medium:
                    mediumWeight = adjustedWeight;
                    break;

                case ExpGemQuality.Large:
                    largeWeight = adjustedWeight;
                    break;

                case ExpGemQuality.Super:
                    superWeight = adjustedWeight;
                    break;
            }
        }

        float totalWeight =
            smallWeight +
            mediumWeight +
            largeWeight +
            superWeight;

        if (totalWeight <= 0f)
        {
            Debug.LogWarning(
                "目前經驗球品質總權重為 0。"
            );

            return;
        }

        Debug.Log(
            $"目前 Luck Level：{luckLevel}，" +
            $"Small：{smallWeight:0.00} " +
            $"({smallWeight / totalWeight:P1})，" +
            $"Medium：{mediumWeight:0.00} " +
            $"({mediumWeight / totalWeight:P1})，" +
            $"Large：{largeWeight:0.00} " +
            $"({largeWeight / totalWeight:P1})，" +
            $"Super：{superWeight:0.00} " +
            $"({superWeight / totalWeight:P1})"
        );
    }

    //對外掉落入口
    public void DropRewards()
    {
        int dropCount = GetRandomDropCount();

        if (dropCount <= 0)
        {
            if (showDropDebug)
            {
                Debug.Log(
                    $"{gameObject.name} 本次沒有產生經驗球。"
                );
            }

            return;
        }

        for (int i = 0; i < dropCount; i++)
        {
            SpawnRandomExpGem();
        }

        if (showDropDebug)
        {
            Debug.Log(
                $"{gameObject.name} 本次掉落 {dropCount} 顆經驗球。"
            );
        }
    }

    //抽選掉落顆數
    private int GetRandomDropCount()
    {
        if (
            dropCountWeights == null ||
            dropCountWeights.Length == 0
        )
        {
            Debug.LogWarning(
                $"{gameObject.name} 的 EnemyDrop " +
                $"沒有設定掉落顆數權重。"
            );

            return 0;
        }

        float totalWeight = 0f;

        foreach (DropCountWeight option in dropCountWeights)
        {
            if (option == null)
            {
                continue;
            }

            totalWeight += Mathf.Max(option.weight, 0f);
        }

        if (totalWeight <= 0f)
        {
            Debug.LogWarning(
                $"{gameObject.name} 的掉落顆數總權重為 0。"
            );

            return 0;
        }

        float randomValue =
            Random.Range(0f, totalWeight);

        float accumulatedWeight = 0f;

        foreach (DropCountWeight option in dropCountWeights)
        {
            if (option == null)
            {
                continue;
            }

            accumulatedWeight +=
                Mathf.Max(option.weight, 0f);

            if (randomValue <= accumulatedWeight)
            {
                return Mathf.Max(option.dropCount, 0);
            }
        }

        //浮點數誤差時的保底結果
        return Mathf.Max(
            dropCountWeights[
                dropCountWeights.Length - 1
            ].dropCount,
            0
        );
    }

    //抽選並生成經驗球
    private void SpawnRandomExpGem()
    {
        ExpGemDropWeight selectedGem =
            GetRandomGemQuality();

        if (selectedGem == null)
        {
            return;
        }

        if (selectedGem.gemPrefab == null)
        {
            Debug.LogWarning(
                $"{gameObject.name} 抽中了 " +
                $"{selectedGem.quality} 經驗球，" +
                $"但尚未指定對應 Prefab。"
            );

            return;
        }

        Vector2 randomOffset =
            Random.insideUnitCircle * dropSpreadRadius;

        Vector3 spawnPosition =
            transform.position
            + new Vector3(
                randomOffset.x,
                0f,
                randomOffset.y
            );

        Instantiate(
            selectedGem.gemPrefab,
            spawnPosition,
            Quaternion.identity
        );

        if (showDropDebug)
        {
            Debug.Log(
                $"{gameObject.name} 掉落：" +
                $"{selectedGem.quality} 經驗球"
            );
        }
    }

    private float GetAdjustedGemWeight(ExpGemDropWeight option)
    {
        if (option == null)
        {
            return 0f;
        }

        float baseWeight =
            Mathf.Max(option.weight, 0f);

        int luckLevel =
            playerStats != null
            ? Mathf.Max(playerStats.luckLevel, 0)
            : 0;

        switch (option.quality)
        {
            case ExpGemQuality.Small:
                return Mathf.Max(
                    baseWeight
                    - luckLevel
                    * smallWeightReductionPerLuck,
                    minimumSmallWeight
                );

            case ExpGemQuality.Medium:
                return Mathf.Max(
                    baseWeight
                    + luckLevel
                    * mediumWeightBonusPerLuck,
                    0f
                );

            case ExpGemQuality.Large:
                return Mathf.Max(
                    baseWeight
                    + luckLevel
                    * largeWeightBonusPerLuck,
                    0f
                );

            case ExpGemQuality.Super:
                return Mathf.Clamp(
                    baseWeight
                    + luckLevel
                    * superWeightBonusPerLuck,
                    0f,
                    maximumSuperWeight
                );

            default:
                return baseWeight;
        }
    }

    //經驗球品質權重抽選
    private ExpGemDropWeight GetRandomGemQuality()
    {
        if (
            gemQualityWeights == null ||
            gemQualityWeights.Length == 0
        )
        {
            Debug.LogWarning(
                $"{gameObject.name} 的 EnemyDrop " +
                $"沒有設定經驗球品質權重。"
            );

            return null;
        }

        //第一輪：計算所有品質調整後的總權重
        float totalWeight = 0f;

        foreach (ExpGemDropWeight gemOption in gemQualityWeights)
        {
            if (gemOption == null)
            {
                continue;
            }

            totalWeight +=
                GetAdjustedGemWeight(gemOption);
        }

        if (totalWeight <= 0f)
        {
            Debug.LogWarning(
                $"{gameObject.name} 的經驗球品質總權重為 0。"
            );

            return null;
        }

        //在總權重範圍內抽一個隨機值
        float randomValue =
            Random.Range(0f, totalWeight);

        float accumulatedWeight = 0f;

        //第二輪：找出隨機值落在哪一個品質區間
        foreach (ExpGemDropWeight gemOption in gemQualityWeights)
        {
            if (gemOption == null)
            {
                continue;
            }

            accumulatedWeight +=
                GetAdjustedGemWeight(gemOption);

            if (randomValue <= accumulatedWeight)
            {
                return gemOption;
            }
        }

        //浮點數誤差時的保底結果
        for (int i = gemQualityWeights.Length - 1; i >= 0; i--)
        {
            if (gemQualityWeights[i] != null)
            {
                return gemQualityWeights[i];
            }
        }

        return null;
    }
}