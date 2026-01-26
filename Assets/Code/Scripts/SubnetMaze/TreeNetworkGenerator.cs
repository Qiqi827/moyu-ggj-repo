using UnityEngine;
using System.Collections.Generic;

public class TreeNetworkGenerator : MonoBehaviour
{
    public GameObject nodePrefab;
    public int maxDepth = 3;        
    public int branchingFactor = 3; 
    public float levelDistance = 12f; 
    public float spreadAngle = 120f;   

    [HideInInspector] public NetworkNode rootNode;
    [HideInInspector] public List<NetworkNode> allGeneratedNodes = new List<NetworkNode>();

    public void GenerateTree()
    {
        allGeneratedNodes.Clear();
        rootNode = SpawnNode(Vector3.zero, "10.0.0.1", 0);
        GrowBranch(rootNode, 1, Vector3.forward);
    }

    void GrowBranch(NetworkNode parent, int depth, Vector3 direction)
    {
        if (depth >= maxDepth) return;
        for (int i = 0; i < branchingFactor; i++)
        {
            float angle = (i - (branchingFactor - 1) / 2f) * (spreadAngle / Mathf.Max(1, branchingFactor - 1));
            Vector3 branchDir = Quaternion.Euler(0, angle, 0) * direction;
            Vector3 spawnPos = parent.transform.position + branchDir * levelDistance;

            string[] ipParts = parent.ipAddress.Split('.');
            ipParts[depth] = (i + 1).ToString();
            string newIP = string.Join(".", ipParts);

            NetworkNode childNode = SpawnNode(spawnPos, newIP, depth);
            GrowBranch(childNode, depth + 1, branchDir);
        }
    }

    NetworkNode SpawnNode(Vector3 pos, string ip, int depth)
    {
        GameObject go = Instantiate(nodePrefab, pos, Quaternion.identity, transform);
        NetworkNode node = go.GetComponent<NetworkNode>();
        node.Initialize(ip); // 这里会调用 ConnectionManager 里的解析逻辑
        go.transform.localScale = Vector3.one * (1.2f - depth * 0.2f);
        allGeneratedNodes.Add(node);
        return node;
    }
}