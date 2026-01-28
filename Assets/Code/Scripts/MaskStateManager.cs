using System;
using System.Collections;
using UnityEngine;

public class MaskStateManager : MonoBehaviour
{
    public enum MaskState
    {
        On,
        Off
    }

    public MaskState maskState = MaskState.On;
    
    private ArrayList _inactiveObjects;

    public void Awake()
    {
        _inactiveObjects = new ArrayList();
        RefreshSceneByMaskStatus();
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            ToggleMask();
        }
    }

    public void SetMaskState(MaskState state)
    {
        if (maskState == state) return;
        maskState = state;
        Debug.Log($"Mask state set to {maskState}");
        RefreshSceneByMaskStatus();
    }

    public void SetMaskOn()  => SetMaskState(MaskState.On);
    public void SetMaskOff() => SetMaskState(MaskState.Off);

    private void ToggleMask()
    {
        SetMaskState(maskState == MaskState.On ? MaskState.Off : MaskState.On);
    }

    private void RefreshSceneByMaskStatus()
    {
        var objectsWithMaskOn = GameObject.FindGameObjectsWithTag("SeenWithMaskOn");
        var objectsWithMaskOff = GameObject.FindGameObjectsWithTag("SeenWithMaskOff");
        
        // 先重新激活之前被禁用的对象
        foreach (var obj in _inactiveObjects)
        {
            ((GameObject)obj).SetActive(true);
            Debug.Log($"Object {((GameObject)obj).name} is now active");
        }
        
        // 然后清空列表
        _inactiveObjects.Clear();

        if (maskState == MaskState.On)
        {
            foreach (var obj in objectsWithMaskOff)
            {
                obj.SetActive(false);
                _inactiveObjects.Add(obj);
                Debug.Log($"Object {obj.name} is now inactive");
            }
        }
        else
        {
            foreach (var obj in objectsWithMaskOn)
            {
                obj.SetActive(false);
                _inactiveObjects.Add(obj);
                Debug.Log($"Object {obj.name} is now inactive");
            }
        }
        
        // 更新对象颜色
        UpdateObjectColors();
    }
    
    private void UpdateObjectColors()
    {
        var objectsWithMaskOn = GameObject.FindGameObjectsWithTag("SeenWithMaskOn");
        var objectsWithMaskOff = GameObject.FindGameObjectsWithTag("SeenWithMaskOff");
        
        // 更新 SeenWithMaskOn 标签的对象颜色（仅当它们处于活动状态时）
        foreach (var obj in objectsWithMaskOn)
        {
            if (obj.activeSelf)
            {
                SetObjectColor(obj, Color.grey);
            }
        }
        
        // 更新 SeenWithMaskOff 标签的对象颜色
        foreach (var obj in objectsWithMaskOff)
        {
            if (obj.activeSelf)
            {
                SetObjectColor(obj, Color.white);
            }
        }
    }
    
    private void SetObjectColor(GameObject obj, Color color)
    {
        Renderer renderer = obj.GetComponent<Renderer>();
        if (renderer != null)
        {
            // 创建材质实例以避免影响其他使用相同材质的对象
            Material material = renderer.material;
            material.color = color;
            Debug.Log($"Object {obj.name} color set to {color}");
        }
        else
        {
            Debug.LogWarning($"Object {obj.name} does not have a Renderer component");
        }
    }
}