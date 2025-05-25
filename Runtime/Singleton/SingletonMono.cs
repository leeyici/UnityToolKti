using UnityEngine;

namespace ToolKit.Singleton
{
    /// <summary>
    /// 通用 MonoBehaviour 单例模板，支持懒加载、自动创建 GameObject、重复实例保护。
    /// </summary>
    public class SingletonMono<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance;
        private static readonly object _lock = new object();

        /// <summary>
        /// 是否已经初始化了单例。
        /// </summary>
        public static bool IsInitialized => _instance != null;

        /// <summary>
        /// 获取单例实例。如果不存在，则尝试查找或自动创建。
        /// </summary>
        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            // 先尝试从场景中查找
                            _instance = FindObjectOfType<T>();

                            if (_instance == null)
                            {
                                // 没找到则自动创建 GameObject 并挂载脚本
                                GameObject singletonObject = new GameObject(typeof(T).Name);
                                _instance = singletonObject.AddComponent<T>();

                                Debug.Log($"[SingletonMono] Auto-created instance of {typeof(T)}.");
                            }
                        }
                    }
                }

                return _instance;
            }
        }

        /// <summary>
        /// 初始化时设置为唯一实例，如果已存在其他实例则销毁自己。
        /// </summary>
        protected virtual void Awake()
        {
            lock (_lock)
            {
                if (_instance == null)
                {
                    _instance = this as T;
                    // 可选：跨场景持久化
                    // DontDestroyOnLoad(gameObject);
                }
                else if (_instance != this)
                {
                    Debug.LogWarning($"[SingletonMono] Duplicate instance of {typeof(T)} detected. Destroying this one.");
                    Destroy(gameObject);
                }
            }
        }

        /// <summary>
        /// 销毁时清除实例引用，防止内存残留。
        /// </summary>
        protected virtual void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }
    }
}
