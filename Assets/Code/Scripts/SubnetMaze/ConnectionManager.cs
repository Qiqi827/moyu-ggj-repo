using UnityEngine;
using System;
using System.Net;
using System.Collections.Generic;

public class ConnectionManager : MonoBehaviour
{
    [Header("Network Config")]
    public int maskBits = 24;
    
    private List<NetworkNode> allNodes;
    private PlayerController player;
    private NetworkLineDrawer lineDrawer;

    void Start()
    {
        // 1. 生成树状拓扑
        var treeGen = GetComponent<TreeNetworkGenerator>();
        if (treeGen != null)
        {
            treeGen.GenerateTree();
            allNodes = treeGen.allGeneratedNodes;
        }

        // 2. 获取唯一引用
        player = FindObjectOfType<PlayerController>();
        lineDrawer = FindObjectOfType<NetworkLineDrawer>();

        // 3. 玩家初始位置
        if (player != null && treeGen != null && treeGen.rootNode != null)
        {
            player.currentNode = treeGen.rootNode;
            player.transform.position = treeGen.rootNode.transform.position + Vector3.up * 1.5f;
        }

        SyncNetworkState();
    }

    void Update()
    {
        // A/D 键切换掩码
        if (Input.GetKeyDown(KeyCode.D)) maskBits = Mathf.Clamp(maskBits + 1, 0, 32);
        if (Input.GetKeyDown(KeyCode.A)) maskBits = Mathf.Clamp(maskBits - 1, 0, 32);

        // 核心：每帧只计算一次，统一同步
        SyncNetworkState();
    }

    void SyncNetworkState()
    {
        if (player == null || player.currentNode == null || allNodes == null || lineDrawer == null) return;

        // 步骤 A: 先清空所有旧线
        lineDrawer.ClearAllLines();
        
        int lineIdx = 0;
        uint currentMask = GetMaskUint();
        uint playerIP = player.currentNode.ipUint;

        // 步骤 B: 遍历所有节点，执行唯一判定
        foreach (var node in allNodes)
        {
            if (node == null) continue;

            bool isCurrent = (node == player.currentNode);
            // 这里是全游戏唯一的判定逻辑点
            bool isReachable = (playerIP & currentMask) == (node.ipUint & currentMask);

            // 步骤 C: 同时发送指令
            // 1. 变色
            node.UpdateVisual(isCurrent, isReachable);

            // 2. 画线 (只给非当前位置且可触达的节点画)
            if (!isCurrent && isReachable)
            {
                lineDrawer.DrawLine(lineIdx, player.transform.position, node.transform.position);
                lineIdx++;
            }
        }
    }

    public bool CanConnect(uint ipA, uint ipB)
    {
        uint mask = GetMaskUint();
        return (ipA & mask) == (ipB & mask);
    }

    public uint GetMaskUint()
    {
        if (maskBits <= 0) return 0;
        if (maskBits >= 32) return 0xFFFFFFFF;
        return 0xFFFFFFFF << (32 - maskBits);
    }

    // 静态工具函数
    public static uint IPToUint(string ipStr)
    {
        try {
            IPAddress address = IPAddress.Parse(ipStr.Trim());
            byte[] bytes = address.GetAddressBytes();
            if (BitConverter.IsLittleEndian) Array.Reverse(bytes);
            return BitConverter.ToUInt32(bytes, 0);
        } catch { return 0; }
    }

    public static string UintToIP(uint ip)
    {
        return string.Format("{0}.{1}.{2}.{3}", (ip >> 24) & 0xFF, (ip >> 16) & 0xFF, (ip >> 8) & 0xFF, ip & 0xFF);
    }
}