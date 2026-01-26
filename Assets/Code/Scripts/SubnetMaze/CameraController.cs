using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Targeting")]
    public Transform target;          // 拖入你的 Player (Capsule)
    
    [Header("Positioning")]
    public float height = 20f;        // 相机距离玩家的高度
    public float forwardOffset = -5f; // 稍微往后拉一点，让视角有点角度（如果是纯垂直俯视可设为0）
    
    [Header("Smoothing")]
    public float smoothSpeed = 0.05f; // 平滑跟随速度
    public Vector3 velocity = Vector3.zero;

    void LateUpdate()
    {
        if (target == null) return;

        // 1. 计算目标位置
        // 我们保持相机在玩家上方 height 距离，并在 Z 轴上偏移一点 forwardOffset
        Vector3 desiredPosition = target.position + new Vector3(0, height, forwardOffset);

        // 2. 使用 SmoothDamp 进行平滑移动
        // 比起 Lerp，SmoothDamp 在相机跟随上更不容易产生抖动
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, smoothSpeed);

        // 3. 始终注视玩家
        transform.LookAt(target);
    }
}