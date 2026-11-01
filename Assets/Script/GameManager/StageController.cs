using TMPro;
using UnityEngine;

public class StageController : MonoBehaviour
{
    [Header("生存關設定")]
    [Tooltip("生存關的戰鬥時間，單位為秒。")]
    [Min(0f)]
    public float survivalDuration = 150f;

    [Header("關卡顯示")]
    [Tooltip("顯示目前關卡與剩餘時間的文字。")]
    public TMP_Text stageText;

    [Header("Enemy spawning")]
    public EnemySpawner enemySpawner;

    public static bool IsIntermission { get; private set; }
    public static bool IsCollectingExperience { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetIntermission() { IsIntermission = false; IsCollectingExperience = false; }

    [Header("限時擊殺關")]
    [Min(1f)] public float killStageDuration = 180f;
    [Min(1)] public int killTarget = 50;
    [Header("Boss 測試")]
    [Min(1f)] public float bossDuration = 180f;
    [Min(1)] public int bossBaseHealth = 1500;
    [Min(1)] public int bossDamage = 5;
    [Min(0.1f)] public float bossScale = 2f;
    private EnemyHealth bossHealth;
    private int stageIndex = 1;
    private int killCount;
    private static StageController activeStage;

    public static void NotifyEnemyKilled(EnemyHealth enemy)
    {
        if (activeStage == null || IsIntermission || GameManager.IsGameOver) return;
        if (activeStage.stageIndex == 3 && activeStage.bossHealth == enemy)
        {
            if (activeStage.stageText != null) activeStage.stageText.text = "Boss HP 0";
            GameManager.Victory();
            return;
        }
        if (activeStage.stageIndex != 2) return;
        activeStage.killCount++;
        if (activeStage.killCount >= activeStage.killTarget) activeStage.CompleteSurvivalStage();
    }
    private float remainingTime;
    private bool survivalCompleted;

    private void Start()
    {
        activeStage = this;
        IsIntermission = false;
        IsCollectingExperience = false;
        remainingTime = Mathf.Max(survivalDuration, 0f);
        survivalCompleted = false;

        RefreshStageText();
    }

    private void Update()
    {
        // 結算或生存關完成後，不再更新倒數。
        if (GameManager.IsGameOver || survivalCompleted)
        {
            return;
        }

        // Time.deltaTime 會受到暫停與升級選牌的 timeScale 影響。
        remainingTime = Mathf.Max(
            remainingTime - Time.deltaTime,
            0f
        );

        RefreshStageText();

        if (remainingTime <= 0f)
        {
            if (stageIndex == 1) CompleteSurvivalStage();
            else if (stageIndex == 2) GameManager.Defeat(DefeatReason.KillStageTimeout);
            else if (stageIndex == 3) GameManager.Defeat(DefeatReason.BossTimeout);
        }
    }

    private void CompleteSurvivalStage()
    {
        if (survivalCompleted || GameManager.IsGameOver) return;
        survivalCompleted = true;

        if (enemySpawner != null)
        {
            enemySpawner.enabled = false;
        }
        else
        {
            Debug.LogError("StageController: EnemySpawner is not assigned.", this);
        }

        StartCoroutine(IntermissionSequence());
        IsIntermission = true;
        Time.timeScale = 0f;

        foreach (GameObject enemy in GameObject.FindGameObjectsWithTag("Enemy"))
        {
            EnemyHealth health = enemy.GetComponent<EnemyHealth>();
            if (health != null) health.ClearForStage();
            else Debug.LogWarning("Stage clear: enemy has no EnemyHealth.", enemy);
        }

        Debug.Log("生存關完成！");

        // 之後會從這裡接入清場與關間獎勵。
    }

    private System.Collections.IEnumerator IntermissionSequence()
    {
        IsCollectingExperience = true;
        yield return ShowClearMessage();
        ExpGem[] gems = Object.FindObjectsByType<ExpGem>(FindObjectsInactive.Exclude);
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        PlayerLevel level = player != null ? player.GetComponent<PlayerLevel>() : null;
        if (level == null)
        {
            Debug.LogError("Intermission: PlayerLevel not found; experience was not collected.");
            IsCollectingExperience = false;
            yield break;
        }
        Vector3[] origins = new Vector3[gems.Length];
        for (int i = 0; i < gems.Length; i++) origins[i] = gems[i].transform.position;
        float elapsed = 0f;
        const float animationDuration = 1.8f;
        const float bounceDuration = 0.3f;
        const float bounceHeight = 0.6f;
        while (elapsed < animationDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            for (int i = 0; i < gems.Length; i++)
            {
                if (gems[i] == null) continue;
                if (elapsed < bounceDuration)
                {
                    float bounce = Mathf.Clamp01(elapsed / bounceDuration);
                    gems[i].transform.position = origins[i] + Vector3.up * (bounceHeight * Mathf.Sin(bounce * Mathf.PI * 0.5f));
                }
                else
                {
                    float t = Mathf.Clamp01((elapsed - bounceDuration) / (animationDuration - bounceDuration));
                    float accelerating = t * t * t;
                    Vector3 raisedOrigin = origins[i] + Vector3.up * bounceHeight;
                    gems[i].transform.position = Vector3.Lerp(raisedOrigin, player.transform.position, accelerating);
                }
            }
            yield return null;
        }
        foreach (ExpGem gem in gems) if (gem != null) gem.CollectForIntermission(level);
        IsCollectingExperience = false;
        if (level.upgradeManager == null) { Debug.LogError("UpgradeManager missing; rifle reward unavailable."); yield break; }
        level.upgradeManager.ShowPendingUpgrades();
        while (level.upgradeManager.IsUpgradePanelOpen) yield return null;
        level.upgradeManager.ShowRifleReward();
        if (!level.upgradeManager.IsRifleRewardOpen) { Debug.LogError("Rifle reward did not open; transition stopped."); yield break; }
        while (level.upgradeManager.IsRifleRewardOpen) yield return null;
        if (stageIndex == 2) { yield return StartBossAfterCountdown(); yield break; }
        for (int count = 3; count > 0; count--)
        {
            if (stageText != null) stageText.text = $"下一關準備 {count}";
            yield return new WaitForSecondsRealtime(1f);
        }
        stageIndex = 2;
        killCount = 0;
        remainingTime = Mathf.Max(1f, killStageDuration);
        survivalCompleted = false;
        if (enemySpawner != null) enemySpawner.enabled = true;
        IsIntermission = false;
        Time.timeScale = 1f;
        RefreshStageText();
    }

    private System.Collections.IEnumerator StartBossAfterCountdown()
    {
        if (enemySpawner == null || enemySpawner.enemySpawnOptions == null || enemySpawner.player == null)
        { Debug.LogError("Boss setup missing spawner or player."); yield break; }
        GameObject template = null;
        foreach (EnemySpawnOption option in enemySpawner.enemySpawnOptions)
            if (option != null && option.enemyPrefab != null) { template = option.enemyPrefab; break; }
        if (template == null) { Debug.LogError("Boss template unavailable."); yield break; }
        for (int count = 3; count > 0; count--)
        {
            if (stageText != null) stageText.text = $"Boss 準備 {count}";
            yield return new WaitForSecondsRealtime(1f);
        }
        GameObject boss = Instantiate(template, enemySpawner.player.position + enemySpawner.player.forward * 12f, Quaternion.identity);
        boss.name = "Boss (temporary)";
        boss.transform.localScale *= Mathf.Max(0.1f, bossScale);
        EnemyStats stats = boss.GetComponent<EnemyStats>();
        bossHealth = boss.GetComponent<EnemyHealth>();
        EnemyChase chase = boss.GetComponent<EnemyChase>();
        if (stats == null || bossHealth == null || chase == null)
        { Destroy(boss); Debug.LogError("Boss template lacks required components."); yield break; }
        stats.maxHP = Mathf.Max(1, Mathf.RoundToInt(bossBaseHealth * GameManager.DifficultyMultiplier));
        stats.damage = Mathf.Max(1, Mathf.RoundToInt(bossDamage * GameManager.DifficultyMultiplier));
        chase.target = enemySpawner.player;
        stageIndex = 3;
        remainingTime = Mathf.Max(1f, bossDuration);
        survivalCompleted = false;
        IsIntermission = false;
        Time.timeScale = 1f;
        RefreshStageText();
    }
    private System.Collections.IEnumerator ShowClearMessage()
    {
        if (stageText == null) yield break;
        stageText.text = "\u606d\u559c\u901a\u95dc\uff01";
        Vector3 originalScale = stageText.transform.localScale;
        float elapsed = 0f;
        while (elapsed < 1f)
        {
            elapsed += Time.unscaledDeltaTime;
            float pulse = 1f + 0.2f * Mathf.Sin(Mathf.Clamp01(elapsed) * Mathf.PI);
            stageText.transform.localScale = originalScale * pulse;
            yield return null;
        }
        stageText.transform.localScale = originalScale;
    }

    private void RefreshStageText()
    {
        if (stageText == null)
        {
            return;
        }

        // 無條件進位，讓倒數剛開始時顯示 02:30。
        int totalSeconds = Mathf.CeilToInt(remainingTime);
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;

        stageText.text = stageIndex == 1 ? $"生存關 {minutes:00}:{seconds:00}" : stageIndex == 2 ? $"限時擊殺 {killCount}/{killTarget}  {minutes:00}:{seconds:00}" : $"Boss HP { (bossHealth != null ? bossHealth.CurrentHealth : 0) }  {minutes:00}:{seconds:00}";
    }
}