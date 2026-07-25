using TMPro;
using UnityEngine;

public class PlayerStatsPanel : MonoBehaviour
{

    [Header("角色面板")]
    [Tooltip("Canvas_PlayerHUD 底下的 CharacterPanel")]
    public GameObject statsPanel;


    [Header("玩家資料")]
    [Tooltip("玩家物件上的 PlayerStats")]
    public PlayerStats playerStats;
    [Tooltip("玩家物件上的 PlayerLevel")]
    public PlayerLevel playerLevel;


    [Header("面板文字")]
    [Tooltip("CharacterPanel 底下的 LuckLevel")]
    public TMP_Text luckText;
    [Tooltip("CharacterPanel 底下的 Character Information 1")]
    public TMP_Text leftStatsText;
    [Tooltip("CharacterPanel 底下的 Character Information 2")]
    public TMP_Text rightStatsText;


    [Header("更新設定")]
    [Tooltip("角色面板開啟時，是否持續即時更新角色數值")]
    public bool updateWhileOpen = true;
    [Header("鍵盤查看設定")]
    [Tooltip("按住此按鍵時顯示角色面板，放開後關閉")]
    public KeyCode holdToShowKey = KeyCode.Tab;
    [Tooltip("是否啟用鍵盤按住查看")]
    public bool enableKeyboardHold = true;

    //之後 PauseManager 可以使用這個屬性，
    //判斷角色面板目前是否開啟。
    public bool IsPanelOpen
    {
        get
        {
            return statsPanel != null &&
                   statsPanel.activeSelf;
        }
    }

    private void HandleKeyboardHold()
    {
        if (!enableKeyboardHold)
        {
            return;
        }

        //按下設定的按鍵時，開啟角色面板
        if (Input.GetKeyDown(holdToShowKey))
        {
            OpenPanel();
        }

        //放開設定的按鍵時，關閉角色面板
        if (Input.GetKeyUp(holdToShowKey))
        {
            ClosePanel();
        }
    }

    private void Start()
    {
        //遊戲開始時預設關閉角色面板
        if (statsPanel != null)
        {
            statsPanel.SetActive(false);
        }

        //先檢查必要欄位是否已設定
        ValidateReferences();
    }


    private void Update()
    {
        HandleKeyboardHold();

        //角色面板未開啟時，不需要刷新
        if (!IsPanelOpen)
        {
            return;
        }

        //允許即時更新時，持續刷新面板數值
        if (updateWhileOpen)
        {
            RefreshStatsUI();
        }
    }

    //給中央 Character Information Button 使用
    public void TogglePanel()
    {
        if (statsPanel == null)
        {
            Debug.LogError(
                "PlayerStatsPanel 尚未指定 Stats Panel。"
            );

            return;
        }

        bool shouldOpen =
            !statsPanel.activeSelf;

        statsPanel.SetActive(shouldOpen);

        //開啟面板時立即更新一次數值
        if (shouldOpen)
        {
            RefreshStatsUI();
        }
    }


    //給面板內關閉按鈕，或之後 PauseManager 使用
    public void ClosePanel()
    {
        if (statsPanel == null)
        {
            return;
        }

        statsPanel.SetActive(false);
    }


    //必要時可由其他系統直接開啟
    public void OpenPanel()
    {
        if (statsPanel == null)
        {
            Debug.LogError(
                "PlayerStatsPanel 尚未指定 Stats Panel。"
            );

            return;
        }

        statsPanel.SetActive(true);
        RefreshStatsUI();
    }
    public void RefreshStatsUI()
    {
        if (playerStats == null)
        {
            Debug.LogWarning(
                "PlayerStatsPanel 尚未指定 PlayerStats。"
            );

            return;
        }

        UpdateLuckText();
        UpdateLeftStatsText();
        UpdateRightStatsText();
    }

    //上方中央幸運資訊
    private void UpdateLuckText()
    {
        if (luckText == null)
        {
            return;
        }

        luckText.text =
            $"幸運等級：{playerStats.luckLevel}";
    }

    //左側角色資訊
    private void UpdateLeftStatsText()
    {
        if (leftStatsText == null)
        {
            return;
        }

        int currentLevel =
            playerLevel != null
            ? playerLevel.level
            : 1;

        leftStatsText.text =
            $"角色等級：Lv.{currentLevel}\n" +
            $"最大生命：{playerStats.FinalMaxHP}\n" +
            $"攻擊力：{playerStats.FinalDamage}\n" +
            $"攻擊速度：{playerStats.FinalAttackSpeed:0.00}\n" +
            $"攻擊距離：{playerStats.FinalAttackRange:0.00}";
    }

    //右側角色資訊
    private void UpdateRightStatsText()
    {
        if (rightStatsText == null)
        {
            return;
        }

        float finalArmor =
            Mathf.Max(
                playerStats.FinalArmor,
                0f
            );

        //與目前玩家受傷系統相同：
        //減傷率 = Armor / (50 + Armor)
        float damageReduction =
            finalArmor /
            (50f + finalArmor);

        rightStatsText.text =
            $"移動速度：{playerStats.FinalMoveSpeed:0.00}\n" +
            $"防禦：{finalArmor:0.##} (減傷 {damageReduction:P1})\n" +
            $"暴擊率：{playerStats.criticalRate:P1}\n" +
            $"暴擊傷害：{playerStats.FinalCriticalDamage:P0}\n" +
            $"吸血：{playerStats.lifeSteal:P1}";
    }


    private void ValidateReferences()
    {
        if (statsPanel == null)
        {
            Debug.LogWarning(
                "PlayerStatsPanel：尚未指定 CharacterPanel。"
            );
        }

        if (playerStats == null)
        {
            Debug.LogWarning(
                "PlayerStatsPanel：尚未指定 PlayerStats。"
            );
        }

        if (playerLevel == null)
        {
            Debug.LogWarning(
                "PlayerStatsPanel：尚未指定 PlayerLevel，" +
                "角色等級將暫時顯示為 Lv.1。"
            );
        }

        if (luckText == null)
        {
            Debug.LogWarning(
                "PlayerStatsPanel：尚未指定 LuckLevel 文字。"
            );
        }

        if (leftStatsText == null)
        {
            Debug.LogWarning(
                "PlayerStatsPanel：尚未指定左側角色資訊文字。"
            );
        }

        if (rightStatsText == null)
        {
            Debug.LogWarning(
                "PlayerStatsPanel：尚未指定右側角色資訊文字。"
            );
        }
    }
}