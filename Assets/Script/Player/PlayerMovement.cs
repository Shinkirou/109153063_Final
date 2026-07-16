using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private CharacterController controller;
    private PlayerStats playerStats;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        playerStats = GetComponent<PlayerStats>();

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

        controller.Move(
            moveDirection *
            playerStats.FinalMoveSpeed *
            Time.deltaTime
        );

        if (moveDirection != Vector3.zero)
        {
            transform.forward = moveDirection;
        }
    }
}