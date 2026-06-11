using UnityEngine;

public class ExpGem : MonoBehaviour
{
    [Header("經驗值設定")]
    public int expValue = 1;

    [Header("視覺效果")]                //旋轉速度、漂浮速度和高度
    public float rotateSpeed = 90f;
    public float floatSpeed = 3f;
    public float floatHeight = 0.2f;

    [Header("吸附特效")]                //玩家吸附範圍和吸附速度
    public float pickupRange = 3f;
    public float magnetSpeed = 8f;

    private Vector3 startPosition;
    private Transform player;

    void Start()
    {
        startPosition = transform.position;

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
        {
            player = playerObject.transform;
        }
    }

    void Update()
    {
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
            magnetSpeed * Time.deltaTime
        );
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        PlayerLevel playerLevel = other.GetComponent<PlayerLevel>();

        if (playerLevel != null)
        {
            playerLevel.AddExp(expValue);
        }

        Destroy(gameObject);
    }
}