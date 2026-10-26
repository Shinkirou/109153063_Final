using UnityEngine;

public class ExpGem : MonoBehaviour
{

    //==================================================
    // 經驗球品質
   public enum ExpGemType
    {
        Small,
        Medium,
        Large,
        Super
    }
    //==================================================

    [Header("經驗值設定")]
    [Tooltip("此經驗球的品質")]
    public ExpGemType gemType = ExpGemType.Small;

    [Tooltip("一般經驗球提供的固定經驗值。超級經驗球不使用此數值")]
    public int expValue = 1;

    [Header("視覺效果")]
    [Tooltip("經驗球每秒旋轉速度")]
    public float rotateSpeed = 90f;

    [Tooltip("上下漂浮的速度")]
    public float floatSpeed = 3f;

    [Tooltip("上下漂浮的高度")]
    public float floatHeight = 0.2f;

    [Header("吸附特效")]
    [Tooltip("玩家進入此距離後，經驗球開始吸附")]
    public float pickupRange = 3f;

    [Tooltip("經驗球飛向玩家的速度")]
    public float magnetSpeed = 8f;

    [Header("測試")]
    [Tooltip("拾取時是否在 Console 顯示獲得的經驗值")]
    public bool showExpDebug = false;

    private Vector3 startPosition;
    private Transform player;

    //Unity 生命週期
    void Start()
    {
        startPosition = transform.position;

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
        {
            player = playerObject.transform;
        }
    }

    //視覺與移動
    void Update()
    {
        // 本局結束後，不再執行遊戲操作。
        if (GameManager.IsGameOver || Time.timeScale <= 0f)
        {
            return;
        }

        RotateGem();        //旋轉經驗球
        FloatGem();         //漂浮效果
        MagnetToPlayer();   //吸附至玩家
    }

    void RotateGem()
    {
        transform.Rotate(0f, rotateSpeed * Time.deltaTime, 0f);
    }

    void FloatGem()
    {
        if (player != null && Vector3.Distance(transform.position, player.position) <= pickupRange)
        {
            return;
        }

        transform.position = startPosition +
            Vector3.up * Mathf.Sin(Time.time * floatSpeed) * floatHeight;
    }

    void MagnetToPlayer()
    {
        if (player == null)
        {
            return;
        }

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance > pickupRange)
        {
            return;
        }

        transform.position = Vector3.MoveTowards(
            transform.position,
            player.position,
            magnetSpeed * Time.deltaTime * 1.5f
        );
    }

    //經驗值計算
    private int GetExperienceAmount(PlayerLevel playerLevel)
    {
        if (gemType == ExpGemType.Super)
        {
            return playerLevel.GetSuperGemExpAmount();
        }

        return Mathf.Max(expValue, 1);
    }

    //拾取
    void OnTriggerEnter(Collider other)
    {
        // 本局結束後，不再執行遊戲操作。
        if (GameManager.IsGameOver)
        {
            return;
        }

        if (!other.CompareTag("Player"))
        {
            return;
        }

        PlayerLevel playerLevel = other.GetComponent<PlayerLevel>();

        if (playerLevel == null)
        {
            Debug.LogWarning(
                "玩家物件找不到 PlayerLevel，" +
                "無法取得經驗值。"
            );

            return;
        }

        int gainedExp = GetExperienceAmount(playerLevel);
        playerLevel.AddExp(gainedExp);

        if (showExpDebug)
        {
            Debug.Log(
                $"拾取 {gemType} 經驗球，" +
                $"獲得 {gainedExp} 點經驗值。"
            );
        }

        Destroy(gameObject);
    }
}