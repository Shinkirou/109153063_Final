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

    private float remainingTime;
    private bool survivalCompleted;

    private void Start()
    {
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
            CompleteSurvivalStage();
        }
    }

    private void CompleteSurvivalStage()
    {
        survivalCompleted = true;

        Debug.Log("生存關完成！");

        // 之後會從這裡接入清場與關間獎勵。
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

        stageText.text = $"生存關 {minutes:00}:{seconds:00}";
    }
}