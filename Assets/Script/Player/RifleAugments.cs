using UnityEngine;

public class RifleAugments : MonoBehaviour
{
    [Header("雙線火力測試")]
    [Tooltip("Enable only for testing until the reward selection is connected.")]
    public bool dualFireEnabled = false;
    [Min(0f)] public float dualFireDamageMultiplier = 0.7f;
    [Min(0f)] public float dualFireSeparation = 0.5f;

    [Header("雙槍槍口與交錯時間")]
    public Transform leftFirePoint;
    public Transform rightFirePoint;
    [Range(0f, 0.95f)] public float dualFireDelayFraction = 0.2f;


    [Header("火力升溫測試")]
    public bool heatEnabled = false;
    [Min(0f)] public float heatMaxAttackSpeedBonus = 0.4f;
    [Min(0.1f)] public float heatSecondsToOverheat = 8f;
    [Min(0f)] public float heatCooldownSeconds = 2f;
    [Min(0.1f)] public float heatIdleCooldownSeconds = 4f;
    [Tooltip("Optional renderer for heat colour. Leave empty until a weapon model is available.")]
    public Renderer heatRenderer;
    public Color heatColdColor = Color.white;
    public Color heatHotColor = Color.red;
    [Range(0f, 1f)] public float currentHeat;
    public bool isOverheated;
    private float cooldownRemaining;
    private MaterialPropertyBlock heatProperties;
    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
    private static readonly int ColorId = Shader.PropertyToID("_Color");

    [Header("獵殺超頻測試")]
    public bool killOverdriveEnabled = false;
    [Min(0f)] public float killOverdriveAttackSpeedBonus = 0.3f;
    [Min(0f)] public float killOverdriveDuration = 3f;
    private float overdriveRemaining;
    public float OverdriveBonus => killOverdriveEnabled && overdriveRemaining > 0f ? Mathf.Max(0f, killOverdriveAttackSpeedBonus) : 0f;

    [Header("追擊標記測試")]
    public bool trackingMarkEnabled = false;
    [Min(0f)] public float trackingMarkBonusPerHit = 0.05f;
    [Min(0f)] public float trackingMarkMaxBonus = 0.3f;
    private EnemyHealth markedEnemy;
    private float trackingMarkBonus;

    public float RegisterMarkedHit(EnemyHealth enemy)
    {
        if (!trackingMarkEnabled || enemy == null) { markedEnemy = null; trackingMarkBonus = 0f; return 1f; }
        if (markedEnemy != enemy) { markedEnemy = enemy; trackingMarkBonus = 0f; }
        float multiplier = 1f + trackingMarkBonus;
        trackingMarkBonus = Mathf.Min(Mathf.Max(0f, trackingMarkMaxBonus), trackingMarkBonus + Mathf.Max(0f, trackingMarkBonusPerHit));
        return multiplier;
    }
    public void NotifyEnemyKilled()
    {
        if (killOverdriveEnabled && !GameManager.IsGameOver && !StageController.IsIntermission)
            overdriveRemaining = Mathf.Max(0f, killOverdriveDuration);
    }

    public void TickOverdrive()
    {
        if (!killOverdriveEnabled) overdriveRemaining = 0f;
        else overdriveRemaining = Mathf.Max(0f, overdriveRemaining - Time.deltaTime);
    }
    public void UpdateHeat(bool hasTarget)
    {
        if (!heatEnabled)
        {
            currentHeat = 0f;
            isOverheated = false;
            cooldownRemaining = 0f;
            UpdateHeatColor();
            return;
        }
        if (isOverheated)
        {
            cooldownRemaining -= Time.deltaTime;
            currentHeat = heatCooldownSeconds > 0f ? Mathf.Clamp01(cooldownRemaining / heatCooldownSeconds) : 0f;
            if (cooldownRemaining <= 0f)
            {
                isOverheated = false;
                currentHeat = 0f;
            }
        }
        else if (hasTarget)
        {
            currentHeat = Mathf.Clamp01(currentHeat + Time.deltaTime / Mathf.Max(0.1f, heatSecondsToOverheat));
            if (currentHeat >= 1f)
            {
                isOverheated = true;
                cooldownRemaining = Mathf.Max(0f, heatCooldownSeconds);

            }
        }
        else
        {
            currentHeat = Mathf.Clamp01(currentHeat - Time.deltaTime / Mathf.Max(0.1f, heatIdleCooldownSeconds));
        }
        UpdateHeatColor();
    }

    private void UpdateHeatColor()
    {
        if (heatRenderer == null) return;
        if (heatProperties == null) heatProperties = new MaterialPropertyBlock();
        heatRenderer.GetPropertyBlock(heatProperties);
        Color color = Color.Lerp(heatColdColor, heatHotColor, currentHeat);
        heatProperties.SetColor(BaseColorId, color);
        heatProperties.SetColor(ColorId, color);
        heatRenderer.SetPropertyBlock(heatProperties);
    }

}
