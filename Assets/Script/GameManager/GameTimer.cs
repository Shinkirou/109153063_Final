using TMPro;
using UnityEngine;

public class GameTimer : MonoBehaviour
{
    [Header("時間顯示")]
    public TMP_Text timeText;

    // 其他腳本可以讀取，只有這個腳本能修改。
    public float ElapsedSeconds { get; private set; }

    private void Awake()
    {
        ElapsedSeconds = 0f;
        RefreshTimeText();
    }

    private void Update()
    {
        if (GameManager.IsGameOver)
        {
            return;
        }

        ElapsedSeconds += Time.deltaTime;
        RefreshTimeText();
    }

    private void RefreshTimeText()
    {
        if (timeText == null)
        {
            return;
        }

        int totalSeconds = Mathf.FloorToInt(ElapsedSeconds);
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;

        timeText.text = $"{minutes:00}:{seconds:00}";
    }
}