using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using System.Collections;
using System.Collections.Generic;
using Code.Scripts;
using Unity.Cinemachine;

/// <summary>
/// 每关/每场景一个：负责本场景的出生点、入口/出口 Timeline、关卡结束时切到下一场景；
/// 以及 Cinemachine VCam 切换（激活某台/全部关闭/恢复跟拍）、游戏中触发的 Timeline 播放。
/// 入口/出口 Timeline 会在玩家加载后，将角色轨绑定到当前 Player 的 Animator（相机由场景/Cinemachine 控制），再播放。
/// </summary>
public class LevelSceneManager : MonoBehaviour
{
    [Header("Next Scene")]
    [Tooltip("玩家到达 LevelEndTrigger 后要加载的场景名；留空则只播出口 Timeline 不切场景（如结束界面）")]
    [SerializeField] string nextSceneName;

    [Header("Timeline (Optional)")]
    [Tooltip("进入本场景时播放的运镜/过场")]
    [SerializeField] PlayableDirector entryTimeline;
    [Tooltip("玩家到达结束点后、切场景前播放的运镜/过场")]
    [SerializeField] PlayableDirector exitTimeline;

    [Header("Timeline Runtime Binding (Player Only)")]
    [Tooltip("Timeline 里角色/胶囊轨的名称；留空则不绑定角色轨（相机由场景/Cinemachine 控制）")]
    [SerializeField] string playerTrackName = "Player";
    [Tooltip("Player 下要绑到角色轨的子路径（如 Capsule）；留空则绑 Player 根 Transform")]
    [SerializeField] string playerBindingPath = "Capsule";

    [Header("Player Control During Timeline")]
    [Tooltip("播放入口/出口 Timeline 时是否禁用玩家控制")]
    [SerializeField] bool disablePlayerControlDuringTimeline = true;

    [Header("Cinemachine VCams")]
    [Tooltip("本场景所有 Virtual Camera 的 GameObject（用于切换/关闭）；播 Timeline 前会全部 SetActive(false)，播完后恢复 followVCam")]
    [SerializeField] List<GameObject> vcamGameObjects = new List<GameObject>();
    [Tooltip("默认/跟拍 VCam；Timeline 播完后会只激活这一台")]
    [SerializeField] GameObject followVCam;
    [Tooltip("Player 下作为 VCam Follow 目标的子路径（留空用 Player 根）；与 playerBindingPath 可一致")]
    [SerializeField] string playerFollowPath = "";

    bool _isExiting;

    // ----- VCam 切换（供 CameraSwitchTrigger 或其它脚本调用） -----

    /// <summary>只激活指定 VCam，其它全部 SetActive(false)。</summary>
    public void ActivateVCam(GameObject vcam)
    {
        if (vcam == null) return;
        if (vcamGameObjects == null || vcamGameObjects.Count == 0) return;
        foreach (var go in vcamGameObjects)
            if (go != null) go.SetActive(go == vcam);
        Debug.Log($"[LevelSceneManager] ActivateVCam: {vcam.name}");
    }

    /// <summary>全部 VCam SetActive(false)；播 Timeline 前调用，让 Brain 只被 Timeline/Animator 驱动。</summary>
    public void DisableAllVCams()
    {
        if (vcamGameObjects == null) return;
        foreach (var go in vcamGameObjects)
            if (go != null) go.SetActive(false);
        Debug.Log("[LevelSceneManager] DisableAllVCams");
    }

    /// <summary>只激活跟拍 VCam；Timeline 播完后调用。</summary>
    public void ActivateFollowVCam()
    {
        if (followVCam == null) return;
        if (vcamGameObjects == null || vcamGameObjects.Count == 0) return;
        foreach (var go in vcamGameObjects)
            if (go != null) go.SetActive(go == followVCam);
        Debug.Log($"[LevelSceneManager] ActivateFollowVCam: {followVCam.name}");
    }

    /// <summary>游戏中触发一段 Timeline（运镜）；会先 DisableAllVCams，播完后 ActivateFollowVCam。可选：播前 BindTimelineToRuntimePlayer。</summary>
    public void PlayTimeline(PlayableDirector timeline)
    {
        if (timeline == null) return;
        DisableAllVCams();
        timeline.stopped += OnTriggeredTimelineStopped;
        timeline.Play();
        Debug.Log("[LevelSceneManager] PlayTimeline (triggered)");
    }

    void OnTriggeredTimelineStopped(PlayableDirector director)
    {
        director.stopped -= OnTriggeredTimelineStopped;
        ActivateFollowVCam();
        Debug.Log("[LevelSceneManager] 触发的 Timeline 播完，恢复跟拍");
    }

    /// <summary>玩家加载后，将所有带 CinemachineCamera 的 VCam 的 Follow 设为当前 Player 的 Transform（或 playerFollowPath 子节点）。</summary>
    public void BindFollowVCamsToPlayer()
    {
        if (GameManager.Instance == null || GameManager.Instance.CurrentPlayer == null) return;
        if (vcamGameObjects == null || vcamGameObjects.Count == 0) return;

        GameObject player = GameManager.Instance.CurrentPlayer;
        Transform followTarget = string.IsNullOrEmpty(playerFollowPath)
            ? player.transform
            : player.transform.Find(playerFollowPath);
        if (followTarget == null) followTarget = player.transform;

        int bound = 0;
        foreach (var go in vcamGameObjects)
        {
            if (go == null) continue;
            var vcam = go.GetComponent<CinemachineCamera>();
            if (vcam == null) continue;
            vcam.Follow = followTarget;
            bound++;
            Debug.Log($"[LevelSceneManager] BindFollow: {go.name} -> {followTarget.name}");
        }
        if (bound > 0) Debug.Log($"[LevelSceneManager] BindFollowVCamsToPlayer 完成，共 {bound} 台");
    }

    void Start()
    {
        Debug.Log("[LevelSceneManager] Start");
        if (entryTimeline != null)
        {
            Debug.Log("[LevelSceneManager] 有入口 Timeline，等待玩家后绑定并播放");
            StartCoroutine(PlayEntryTimelineThenFinish());
        }
        else
        {
            Debug.Log("[LevelSceneManager] 无入口 Timeline，等待玩家后绑定 VCam Follow 并恢复跟拍");
            StartCoroutine(WaitForPlayerThenInitCamera());
        }
    }

    IEnumerator WaitForPlayerThenInitCamera()
    {
        while (GameManager.Instance == null || GameManager.Instance.CurrentPlayer == null)
            yield return null;
        BindFollowVCamsToPlayer();
        ActivateFollowVCam();
        EnsurePlayerControlEnabled();
    }

    IEnumerator PlayEntryTimelineThenFinish()
    {
        Debug.Log("[LevelSceneManager] 等待 GameManager 与玩家就绪...");
        while (GameManager.Instance == null || GameManager.Instance.CurrentPlayer == null)
            yield return null;
        Debug.Log("[LevelSceneManager] 玩家已就绪，绑定 VCam Follow 与入口 Timeline");

        BindFollowVCamsToPlayer();
        BindTimelineToRuntimePlayer(entryTimeline);

        if (disablePlayerControlDuringTimeline && GameManager.Instance != null)
            GameManager.Instance.SetPlayerControlEnabled(false);

        DisableAllVCams();
        Debug.Log("[LevelSceneManager] 播放入口 Timeline");
        entryTimeline.Play();
        yield return new WaitUntil(() => entryTimeline.state != PlayState.Playing);
        Debug.Log("[LevelSceneManager] 入口 Timeline 播完");

        ActivateFollowVCam();
        if (disablePlayerControlDuringTimeline && GameManager.Instance != null)
            GameManager.Instance.SetPlayerControlEnabled(true);
    }

    /// <summary>
    /// 将 PlayableDirector 的角色轨绑定到当前场景的 Player 的 Animator（运行时生成的对象）。相机由场景/Cinemachine 控制，不在此绑定。
    /// Timeline 轨需为 Animation Track 等绑定 Animator 的类型；Track 名称需与 playerTrackName 一致。
    /// </summary>
    public void BindTimelineToRuntimePlayer(PlayableDirector director)
    {
        if (director == null || director.playableAsset == null)
        {
            Debug.LogWarning("[LevelSceneManager] BindTimeline: director 或 playableAsset 为空，跳过");
            return;
        }
        if (GameManager.Instance == null || GameManager.Instance.CurrentPlayer == null)
        {
            Debug.LogWarning("[LevelSceneManager] BindTimeline: GameManager 或 CurrentPlayer 为空，跳过");
            return;
        }

        GameObject player = GameManager.Instance.CurrentPlayer;
        Transform playerBinding = string.IsNullOrEmpty(playerBindingPath)
            ? player.transform
            : player.transform.Find(playerBindingPath);
        if (playerBinding == null) playerBinding = player.transform;

        Animator playerAnimator = playerBinding.GetComponent<Animator>();
        if (playerAnimator == null) playerAnimator = playerBinding.GetComponentInChildren<Animator>();

        var timeline = director.playableAsset as TimelineAsset;
        if (timeline == null)
        {
            Debug.LogWarning("[LevelSceneManager] BindTimeline: playableAsset 不是 TimelineAsset，跳过");
            return;
        }

        int bound = 0;
        foreach (var binding in timeline.outputs)
        {
            var track = binding.sourceObject as TrackAsset;
            if (track == null) continue;

            if (!string.IsNullOrEmpty(playerTrackName) && track.name == playerTrackName)
            {
                if (playerAnimator != null)
                {
                    director.SetGenericBinding(track, playerAnimator);
                    bound++;
                    Debug.Log($"[LevelSceneManager] 已绑定轨 \"{track.name}\" -> {playerBinding.name} Animator");
                }
                else
                    Debug.LogWarning($"[LevelSceneManager] 角色轨 \"{track.name}\" 需要 Animator，未在 {playerBinding.name} 上找到，跳过");
            }
        }
        Debug.Log($"[LevelSceneManager] BindTimeline 完成，共绑定 {bound} 条轨（仅 Player）");
    }

    void EnsurePlayerControlEnabled()
    {
        if (disablePlayerControlDuringTimeline && GameManager.Instance != null)
            GameManager.Instance.SetPlayerControlEnabled(true);
    }

    /// <summary>
    /// 由 LevelEndTrigger 在玩家进入结束区域时调用。
    /// </summary>
    public void OnPlayerReachedEnd()
    {
        if (_isExiting) return;
        _isExiting = true;
        Debug.Log("[LevelSceneManager] 玩家到达结束点");

        if (exitTimeline != null)
        {
            Debug.Log("[LevelSceneManager] 绑定并播放出口 Timeline");
            BindTimelineToRuntimePlayer(exitTimeline);

            if (disablePlayerControlDuringTimeline && GameManager.Instance != null)
                GameManager.Instance.SetPlayerControlEnabled(false);

            DisableAllVCams();
            exitTimeline.stopped += OnExitTimelineStopped;
            exitTimeline.Play();
        }
        else
        {
            Debug.Log("[LevelSceneManager] 无出口 Timeline，直接切场景");
            LoadNextScene();
        }
    }

    void OnExitTimelineStopped(PlayableDirector director)
    {
        director.stopped -= OnExitTimelineStopped;
        Debug.Log("[LevelSceneManager] 出口 Timeline 播完，切场景");
        if (disablePlayerControlDuringTimeline && GameManager.Instance != null)
            GameManager.Instance.SetPlayerControlEnabled(true);
        LoadNextScene();
    }

    void LoadNextScene()
    {
        if (string.IsNullOrEmpty(nextSceneName))
        {
            Debug.Log("[LevelSceneManager] nextSceneName 为空，不加载场景");
            return;
        }
        Debug.Log($"[LevelSceneManager] LoadScene: {nextSceneName}");
        SceneManager.LoadScene(nextSceneName);
    }
}
