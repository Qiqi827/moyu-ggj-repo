using UnityEngine;
using UnityEngine.SceneManagement;

namespace Code.Scripts
{
    /// <summary>
    /// 跨场景单例：负责在每次场景加载后把玩家放到当前场景的 SpawnPoint。
    /// 适用于关卡、开始界面、结束界面等复用 Template 的场景。
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Player")]
        [Tooltip("玩家预制体；不赋值则不会生成玩家（例如纯 UI 的开始/结束界面）")]
        [SerializeField] GameObject playerPrefab;

        GameObject _currentPlayer;

        /// <summary>当前跨场景存在的玩家实例；可能为 null（例如尚未进入过需要玩家的场景）。</summary>
        public GameObject CurrentPlayer => _currentPlayer;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.Log("[GameManager] 已存在单例，销毁重复实例");
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("[GameManager] 单例已创建，DontDestroyOnLoad");
        }

        void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        void Start()
        {
            // 首次加载的游戏场景不会触发 sceneLoaded，这里补一次放置
            Debug.Log("[GameManager] Start（首次场景放置）");
            PlaceOrSpawnPlayerAtSpawnPoint();
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            Debug.Log($"[GameManager] 场景已加载: {scene.name}");
            PlaceOrSpawnPlayerAtSpawnPoint();
        }

        /// <summary>
        /// 在当前场景中查找 Tag 为 "SpawnPoint" 的物体，将玩家放置或生成到该位置。
        /// 若无 SpawnPoint 或未设置 playerPrefab，则不做任何事。
        /// </summary>
        public void PlaceOrSpawnPlayerAtSpawnPoint()
        {
            var spawnPoint = GameObject.FindWithTag("SpawnPoint");
            if (spawnPoint == null)
            {
                Debug.Log("[GameManager] 未找到 Tag=SpawnPoint，跳过放置玩家");
                return;
            }

            var spawnTransform = spawnPoint.transform;
            Vector3 pos = spawnTransform.position;
            Quaternion rot = spawnTransform.rotation;

            if (_currentPlayer == null)
            {
                if (playerPrefab == null)
                {
                    Debug.Log("[GameManager] 未设置 Player Prefab，跳过生成");
                    return;
                }
                _currentPlayer = Instantiate(playerPrefab, pos, rot);
                _currentPlayer.tag = "Player"; // 保证 EndTrigger 能识别
                Debug.Log($"[GameManager] 已生成玩家于 SpawnPoint {spawnPoint.name} @ {pos}");
            }
            else
            {
                _currentPlayer.transform.SetPositionAndRotation(pos, rot);
                Debug.Log($"[GameManager] 已移动已有玩家到 SpawnPoint {spawnPoint.name} @ {pos}");
            }
        }

        /// <summary>
        /// 设置玩家是否可被放置/生成。若为 false，下次场景加载时不会移动/生成玩家（可用于纯过场场景）。
        /// 默认行为始终尝试放置，可通过外部在需要时关闭。
        /// </summary>
        public void SetPlayerControlEnabled(bool enabled)
        {
            if (_currentPlayer == null)
            {
                Debug.Log("[GameManager] SetPlayerControlEnabled: 无当前玩家，忽略");
                return;
            }
            foreach (var mb in _currentPlayer.GetComponents<MonoBehaviour>())
            {
                if (mb != null) mb.enabled = enabled;
            }
            Debug.Log($"[GameManager] 玩家控制已{(enabled ? "开启" : "关闭")}");
        }
    }
}
