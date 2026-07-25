using UnityEngine;
using UnityEngine.EventSystems;

public class HoldToShowStatsPanel :
    MonoBehaviour,
    IPointerDownHandler,
    IPointerUpHandler
{
    [Header("角色面板控制器")]
    [Tooltip("掛在 GameManager 上的 PlayerStatsPanel")]
    public PlayerStatsPanel playerStatsPanel;

    //按下滑鼠或手指時顯示角色面板
    public void OnPointerDown(PointerEventData eventData)
    {
        if (playerStatsPanel == null)
        {
            Debug.LogWarning(
                "HoldToShowStatsPanel 尚未指定 PlayerStatsPanel。"
            );

            return;
        }

        playerStatsPanel.OpenPanel();
    }

    //放開滑鼠或手指時關閉角色面板
    public void OnPointerUp(PointerEventData eventData)
    {
        if (playerStatsPanel == null)
        {
            return;
        }

        playerStatsPanel.ClosePanel();
    }

    //避免按鈕被關閉或切換場景時，
    //角色面板仍殘留在畫面上
    private void OnDisable()
    {
        if (playerStatsPanel != null)
        {
            playerStatsPanel.ClosePanel();
        }
    }
}