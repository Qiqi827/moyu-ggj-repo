using UnityEngine;
using System.Collections.Generic;

public class NetworkLineDrawer : MonoBehaviour
{
    public Material lineMaterial;
    private List<LineRenderer> linePool = new List<LineRenderer>();

    // 每次画线前，Manager 会调用这个清空
    public void ClearAllLines()
    {
        foreach (var lr in linePool)
        {
            if (lr != null) lr.enabled = false;
        }
    }

    // 唯一的画线接口：由 Manager 调用
    public void DrawLine(int index, Vector3 start, Vector3 end)
    {
        LineRenderer lr = GetOrCreateLine(index);
        lr.enabled = true;
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);
    }

    private LineRenderer GetOrCreateLine(int index)
    {
        if (index < linePool.Count) return linePool[index];

        GameObject lineObj = new GameObject("Line_" + index);
        lineObj.transform.SetParent(transform);
        LineRenderer lr = lineObj.AddComponent<LineRenderer>();
        
        lr.startWidth = 0.05f;
        lr.endWidth = 0.05f;
        lr.material = lineMaterial;
        lr.positionCount = 2;
        lr.useWorldSpace = true; // 确保使用世界坐标

        linePool.Add(lr);
        return lr;
    }
}