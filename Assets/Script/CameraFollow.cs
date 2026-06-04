using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("指定目標")] 
    public Transform target;

    [Header("相機位置")] //offset是相機與目標之間的距離
    public Vector3 offset = new Vector3(0f, 12f, -8f);

    [Header("相機速度")]
    public float followSpeed = 5f;

    [Header("相機角度")]
    public float pitch = 60f;
    public float yaw = 0f;
    public float roll = 0f;

    void LateUpdate()
    {
        //確保有目標可跟隨 沒有目標則重新檢查 有目標執行方法
        if (target == null)
        {
            return;
        }

        FollowTarget();
        RotateCamera();
    }

    void FollowTarget()
    {
        Vector3 targetPosition = target.position + offset;

        transform.position = Vector3.Lerp(
            transform.position,             // 攝影機現在的位置
            targetPosition,                 // 攝影機想去的位置
            followSpeed * Time.deltaTime    // 每一幀移動多少
        );
    }

    void RotateCamera()
    {
        transform.rotation = Quaternion.Euler(
            pitch,
            yaw,
            roll
        );
    }
}