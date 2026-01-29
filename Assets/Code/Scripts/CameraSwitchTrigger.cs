using UnityEngine;

/// <summary>
/// 挂在 Trigger 物体上：玩家进入时激活指定 VCam，离开时可选恢复跟拍。
/// 需在 LevelSceneManager 里配置 vcamGameObjects 和 followVCam。
/// </summary>
[RequireComponent(typeof(Collider))]
public class CameraSwitchTrigger : MonoBehaviour
{
    [Tooltip("玩家进入时要激活的 Virtual Camera（拖该 VCam 的 GameObject）")]
    [SerializeField] GameObject vcamToActivate;

    [Tooltip("玩家离开 Trigger 时是否恢复跟拍")]
    [SerializeField] bool restoreFollowOnExit = true;

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        var manager = FindObjectOfType<LevelSceneManager>();
        if (manager == null)
        {
            Debug.LogWarning("[CameraSwitchTrigger] 场景中未找到 LevelSceneManager");
            return;
        }
        if (vcamToActivate == null)
        {
            Debug.LogWarning("[CameraSwitchTrigger] 未设置 vcamToActivate");
            return;
        }
        manager.ActivateVCam(vcamToActivate);
    }

    void OnTriggerExit(Collider other)
    {
        if (!restoreFollowOnExit || !other.CompareTag("Player")) return;

        var manager = FindObjectOfType<LevelSceneManager>();
        if (manager != null)
            manager.ActivateFollowVCam();
    }
}
