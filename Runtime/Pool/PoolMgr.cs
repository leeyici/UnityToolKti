using System.Collections.Generic;
using ToolKit.Singleton;
using UnityEngine;

namespace ToolKit.Pool
{
    public class PoolMgr : SingletonMono<PoolMgr>
    {
        // 对象池本体
        private readonly Dictionary<string, Queue<GameObject>> _poolDic = new();

        // 用于防止重复入池：记录所有已经入池的对象
        private readonly HashSet<GameObject> _inPoolSet = new();

        // 每种资源的最大缓存数量（默认不限制，可设置）
        private readonly Dictionary<string, int> _maxCacheCount = new();

        /// <summary>
        /// 获取对象（先从池中取，没有就实例化）
        /// </summary>
        public GameObject GetObj(string objName)
        {
            GameObject obj;

            if (_poolDic.TryGetValue(objName, out var queue) && queue.Count > 0)
            {
                obj = queue.Dequeue();
                _inPoolSet.Remove(obj);
            }
            else
            {
                obj = GameObject.Instantiate(Resources.Load<GameObject>(objName));
                obj.name = objName;
            }

            obj.SetActive(true);
            return obj;
        }

        /// <summary>
        /// 回收对象（放回池中）
        /// </summary>
        public void ReturnObj(string objName, GameObject obj)
        {
            if (obj == null) return;
            if (_inPoolSet.Contains(obj)) return; // 防止重复入池

            obj.SetActive(false);

            if (!_poolDic.TryGetValue(objName, out var queue))
            {
                queue = new Queue<GameObject>();
                _poolDic[objName] = queue;
            }

            // 检查是否超过最大缓存数量
            if (_maxCacheCount.TryGetValue(objName, out int maxCount) && queue.Count >= maxCount)
            {
                GameObject.Destroy(obj); // 超出限制直接销毁
                return;
            }

            queue.Enqueue(obj);
            _inPoolSet.Add(obj);
        }

        /// <summary>
        /// 设置每种资源的最大缓存数量（可选）
        /// </summary>
        public void SetMaxCache(string objName, int maxCount)
        {
            _maxCacheCount[name] = maxCount;
        }

        /// <summary>
        /// 预加载对象：提前创建放入池中
        /// </summary>
        public void Preload(string objName, int count)
        {
            if (!_poolDic.TryGetValue(name, out var queue))
            {
                queue = new Queue<GameObject>();
                _poolDic[name] = queue;
            }

            for (int i = 0; i < count; i++)
            {
                GameObject obj = GameObject.Instantiate(Resources.Load<GameObject>(name));
                obj.name = name;
                obj.SetActive(false);
                queue.Enqueue(obj);
                _inPoolSet.Add(obj);
            }
        }

        /// <summary>
        /// 打印对象池状态
        /// </summary>
        public void PrintPoolStatus()
        {
            Debug.Log("<color=cyan>当前对象池状态：</color>");
            foreach (var kv in _poolDic)
            {
                Debug.Log($"[池] {kv.Key} : {kv.Value.Count}");
            }
        }
    }
}
