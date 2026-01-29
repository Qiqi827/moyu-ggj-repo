using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Playables;
using System.Collections;
using Code.Scripts;

/// <summary>
/// 每关/每场景一个：负责本场景的出生点、入口/出口 Timeline、关卡结束时切到下一场景。
/// 适用于关卡、开始界面（下一场景为第一关）、结束界面（下一场景可为主菜单或空）。
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

    [Header("Player Control During Timeline")]
    [Tooltip("播放入口/出口 Timeline 时是否禁用玩家控制")]
    [SerializeField] bool disablePlayerControlDuringTimeline = true;

    bool _isExiting;

    void Start()
    {
        if (entryTimeline != null)
            StartCoroutine(PlayEntryTimelineThenFinish());
        else
            EnsurePlayerControlEnabled();
    }

    IEnumerator PlayEntryTimelineThenFinish()
    {
        if (disablePlayerControlDuringTimeline && GameManager.Instance != null)
            GameManager.Instance.SetPlayerControlEnabled(false);

        entryTimeline.Play();
        yield return new WaitUntil(() => entryTimeline.state != PlayState.Playing);

        if (disablePlayerControlDuringTimeline && GameManager.Instance != null)
            GameManager.Instance.SetPlayerControlEnabled(true);
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

        if (exitTimeline != null)
        {
            if (disablePlayerControlDuringTimeline && GameManager.Instance != null)
                GameManager.Instance.SetPlayerControlEnabled(false);

            exitTimeline.stopped += OnExitTimelineStopped;
            exitTimeline.Play();
        }
        else
        {
            LoadNextScene();
        }
    }

    void OnExitTimelineStopped(PlayableDirector director)
    {
        director.stopped -= OnExitTimelineStopped;
        if (disablePlayerControlDuringTimeline && GameManager.Instance != null)
            GameManager.Instance.SetPlayerControlEnabled(true);
        LoadNextScene();
    }

    void LoadNextScene()
    {
        if (string.IsNullOrEmpty(nextSceneName))
            return;
        SceneManager.LoadScene(nextSceneName);
    }
}
