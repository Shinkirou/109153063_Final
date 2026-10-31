using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("子彈設定")]
    public float speed = 15f;
    public int damage = 3;
    public float lifeTime = 10f;

    private Transform target;
    //記錄子彈最後一次有效的飛行方向。
    private Vector3 lastDirection;


    //這顆子彈建立當下取得的暴擊屬性
    private float criticalRate;
    private float criticalDamage;

    [Header("暴擊視覺效果")]
    public Renderer bulletRenderer;
    public Color normalColor = Color.white;
    public Color criticalColor = Color.yellow;
    private bool isCritical;

    [Header("測試")]
    [Tooltip("是否在 Console 顯示每顆子彈的命中資訊")]
    public bool showHitEnemyDebug = false;

    //這顆子彈建立當下取得的吸血屬性
    private float lifeStealRate;
    private PlayerHealth playerHealth;

    void Start()
    {
        if (bulletRenderer == null)
        {
            bulletRenderer = GetComponent<Renderer>();
        }

        UpdateBulletColor();

        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        // 本局結束後，不再執行遊戲操作。
        if (GameManager.IsGameOver || Time.timeScale <= 0f)
        {
            return;
        }

        //目標存在時，持續更新子彈的追蹤方向。
        if (target != null)
        {
            Vector3 direction = target.position - transform.position;
            direction.y = 0f;

            //只有方向有效時才更新，避免覆蓋原本的飛行方向。
            if (direction.sqrMagnitude > 0.0001f)
            {
                lastDirection = direction.normalized;
            }
        }

        //目標消失後，沿著最後記錄的方向繼續飛行。
        transform.position += lastDirection * speed * Time.deltaTime;
    }

    //設定子彈目標與攻擊屬性
    public void SetTarget(
        Transform newTarget, int newDamage, 
        float newCriticalRate, float newCriticalDamage, 
        float newLifeStealRate, PlayerHealth newPlayerHealth)
    {
        target = newTarget;                         //設定子彈的攻擊目標
        damage = newDamage;                         //設定子彈的傷害值

        criticalRate = newCriticalRate;             //設定子彈的暴擊率
        criticalDamage = newCriticalDamage;         //設定子彈的暴擊傷害

        lifeStealRate = newLifeStealRate;           //設定子彈的吸血率
        playerHealth = newPlayerHealth;             //設定子彈的吸血對象

        //子彈生成時就決定這顆子彈是否暴擊
        isCritical = Random.value < criticalRate;

        UpdateBulletColor();
    }

    //根據這顆子彈是否暴擊，更新子彈顏色
    private void UpdateBulletColor()
    {
        if (bulletRenderer == null)
        {
            return;
        }

        bulletRenderer.material.color = isCritical ? criticalColor : normalColor;
    }

    void OnTriggerEnter(Collider other)
    {
        // 本局結束後，不再執行遊戲操作。
        if (GameManager.IsGameOver)
        {
            return;
        }


        EnemyHealth enemyHealth = other.GetComponent<EnemyHealth>();

        //不處理玩家碰撞
        if (other.CompareTag("Player"))
        {
            return;
        }

        //碰到的物件不是敵人
        if (enemyHealth == null)
        {
            return;
        }

        //計算這次攻擊的最終傷害
        int finalDamage = CalculateDamage();

        //每顆子彈只造成一次傷害。
        enemyHealth.TakeDamage(finalDamage);

        //根據這次實際造成的傷害進行吸血
        if (playerHealth != null)
        {
            playerHealth.ApplyLifeSteal(
                finalDamage,
                lifeStealRate
            );
        }

        if (showHitEnemyDebug)
        {
            Debug.Log(
                $"子彈命中敵人：" +
                $"基礎傷害 {damage}，" +
                $"暴擊率 {criticalRate * 100f:0.#}%，" +
                $"暴擊傷害 {criticalDamage * 100f:0.#}%，" +
                $"是否暴擊：{isCritical}，" +
                $"最終傷害 {finalDamage}"
            );
        }

        Destroy(gameObject);
    }

    //判定暴擊並計算最終傷害
    private int CalculateDamage()
    {
        //這顆子彈不是暴擊時，直接造成普通傷害
        if (!isCritical)
        {
            return damage;
        }

        //暴擊傷害 = 普通傷害 × 暴擊傷害倍率
        float criticalDamageResult = damage * criticalDamage;

        //維持整數傷害，將小數無條件捨去
        int finalDamage = Mathf.FloorToInt(criticalDamageResult);

        return Mathf.Max(finalDamage, 1);
    }
}
