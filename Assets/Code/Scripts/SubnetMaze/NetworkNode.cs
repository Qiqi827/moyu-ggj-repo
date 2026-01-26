using UnityEngine;

public class NetworkNode : MonoBehaviour
{
    public string ipAddress;
    [HideInInspector] public uint ipUint;
    private MeshRenderer meshRenderer;

    // 由生成器调用，确保 IP 和渲染器同步初始化
    public void Initialize(string newIp)
    {
        ipAddress = newIp;
        ipUint = ConnectionManager.IPToUint(ipAddress);
        meshRenderer = GetComponent<MeshRenderer>();
    }

    // 唯一的变色接口：由 Manager 调用
    public void UpdateVisual(bool isCurrent, bool isReachable)
    {
        if (meshRenderer == null) meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer == null) return;

        if (isCurrent)
        {
            meshRenderer.material.color = Color.green; // 玩家所在
        }
        else if (isReachable)
        {
            meshRenderer.material.color = Color.cyan; // 可连接
        }
        else
        {
            // 不可连接：半透明深灰 (请确保材质 Rendering Mode 是 Transparent)
            meshRenderer.material.color = new Color(0.2f, 0.2f, 0.2f, 0.3f);
        }
    }
}