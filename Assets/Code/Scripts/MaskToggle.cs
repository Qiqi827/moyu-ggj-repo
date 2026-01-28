using UnityEngine;
using UnityEngine.Rendering;

public class MaskToggle : MonoBehaviour
{
    public Volume maskVolume;
    public float transitionSpeed = 5f;

    private float targetWeight;

    void Start()
    {
        if (maskVolume != null)
        {
            // 核心逻辑：检测你在 Inspector 里设置的初始值
            // 如果初始权重 > 0.5，我们就认为默认是“开启”状态
            targetWeight = maskVolume.weight;
        }
        else
        {
            Debug.LogError("未关联 Volume 组件！");
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            // 优雅的切换：如果目标是 1 就变 0，否则变 1
            targetWeight = (targetWeight > 0.5f) ? 0f : 1f;
        }

        // 平滑过渡到目标权重
        maskVolume.weight = Mathf.MoveTowards(maskVolume.weight, targetWeight, Time.deltaTime * transitionSpeed);
    }
}