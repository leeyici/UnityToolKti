namespace ToolKit.Singleton
{
    /// <summary>
    /// 通用非 MonoBehaviour 单例基类，适用于普通类的单例模式。
    /// 使用约束：泛型参数 T 必须具有无参构造函数。
    /// </summary>
    public class SingletonBase<T> where T : class, new()
    {
        // 单例实例
        private static T _instance;

        // 同步锁，确保线程安全（如多线程环境中使用）
        private static readonly object _lock = new object();

        /// <summary>
        /// 是否已经初始化过单例。
        /// </summary>
        public static bool IsInitialized => _instance != null;

        /// <summary>
        /// 获取单例实例，首次访问时将自动创建。
        /// </summary>
        public static T Instance
        {
            get
            {
                if (_instance == null)                   // 第一次判空（快速判断，避免加锁）
                {
                    lock (_lock)                         // 加锁，防止多线程同时进入
                    {
                        if (_instance == null)           // 第二次判空（锁内再判断，确保只创建一次）
                        {
                            _instance = new T();
                        }
                    }
                }

                return _instance;
            }
        }


        /// <summary>
        /// 手动清除单例实例（通常用于测试或重置场景）。
        /// </summary>
        public static void ResetInstance()
        {
            lock (_lock)
            {
                _instance = null;
            }
        }
    }
}