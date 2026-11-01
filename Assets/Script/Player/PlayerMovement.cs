using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private CharacterController controller;
    private PlayerStats playerStats;
    private PlayerHealth playerHealth;

    [Header("翻滾設定")]
    public KeyCode rollKey = KeyCode.Space;
    [Min(0.01f)] public float rollDuration = 0.333f;
    [Min(0f)] public float rollInvincibilityDuration = 0.333f;
    [Min(0f)] public float rollCooldown = 1.5f;
    [Min(0f)] public float rollSpeedMultiplier = 3f;
    private float rollRemaining;
    private float nextRollTime;
    private Vector3 rollDirection;
    private Vector3 lastMoveDirection;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        playerStats = GetComponent<PlayerStats>();
        playerHealth = GetComponent<PlayerHealth>();
        lastMoveDirection = transform.forward;

        if (controller == null)
        {
            Debug.LogError(
                "PlayerMovement 找不到 CharacterController，" +
                "請確認 Player 身上有掛 CharacterController。"
            );
        }

        if (playerStats == null)
        {
            Debug.LogError(
                "PlayerMovement 找不到 PlayerStats，" +
                "請確認 Player 身上有掛 PlayerStats.cs。"
            );
        }
    }

    void Update()
    {
        // 本局結束後，不再執行遊戲操作。
        if (GameManager.IsGameOver || Time.timeScale <= 0f)
        {
            return;
        }

        if (controller == null || playerStats == null)
        {
            return;
        }

        float horizontal =Input.GetAxisRaw("Horizontal");

        float vertical =Input.GetAxisRaw("Vertical");

        Vector3 moveDirection =
            new Vector3(
                horizontal,
                0f,
                vertical
            ).normalized;

        if (rollRemaining <= 0f && Input.GetKeyDown(rollKey) && Time.time >= nextRollTime)
        {
            rollDirection = moveDirection != Vector3.zero ? moveDirection : lastMoveDirection;
            rollDirection.y = 0f;
            if (rollDirection.sqrMagnitude < 0.001f) rollDirection = Vector3.forward;
            rollDirection.Normalize();
            rollRemaining = Mathf.Max(0.01f, rollDuration);
            nextRollTime = Time.time + Mathf.Max(0f, rollCooldown);
            if (playerHealth != null) playerHealth.GrantRollInvincibility(rollInvincibilityDuration);
        }

        if (rollRemaining > 0f)
        {
            float rollStep = Mathf.Min(Time.deltaTime, rollRemaining);
            controller.Move(rollDirection * playerStats.FinalMoveSpeed * Mathf.Max(0f, rollSpeedMultiplier) * rollStep);
            rollRemaining = Mathf.Max(0f, rollRemaining - Time.deltaTime);
            transform.forward = rollDirection;
            lastMoveDirection = rollDirection;
            return;
        }

        controller.Move(
            moveDirection *
            playerStats.FinalMoveSpeed *
            Time.deltaTime
        );

        if (moveDirection != Vector3.zero)
        {
            transform.forward = moveDirection;
            lastMoveDirection = moveDirection;
        }
    }
}
