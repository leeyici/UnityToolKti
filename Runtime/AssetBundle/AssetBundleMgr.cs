using System;
using System.Collections;
using System.Collections.Generic;
using ToolKit.Singleton;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ToolKit.AssetBundle
{
    /// <summary>
    /// AssetBundle管理器：负责加载主包、依赖包与资源，并支持资源卸载。
    /// </summary>
    public class AssetBundleMgr : SingletonMono<AssetBundleMgr>
    {
        private UnityEngine.AssetBundle _mainAb;                         // 主AssetBundle包（即平台标识包）
        private AssetBundleManifest _manifest;                           // 主包中的Manifest，包含所有依赖关系
        private bool _isAbLoading;                                       // 防止多个异步同时加载AB包，造成重复加载报错
        private readonly Dictionary<string, UnityEngine.AssetBundle> _abDic = new();  // 存储已加载的AssetBundle

        private string PathUrl => Application.streamingAssetsPath + "/";

        /// <summary>
        /// 平台名称对应的主包名
        /// </summary>
        private string MainAbName
        {
            get
            {
#if UNITY_IOS
                return "IOS";
#elif UNITY_ANDROID
                return "Android";
#else
                return "PC";
#endif
            }
        }

        #region 同步加载

         /// <summary>
        /// 加载主包和指定资源的所有依赖包
        /// </summary>
        /// <param name="abName">资源所属的AssetBundle名</param>
        private void LoadMainAndDependencies(string abName)
        {
            // 加载主包和Manifest
            if (_mainAb == null)
            {
                _mainAb = UnityEngine.AssetBundle.LoadFromFile(PathUrl + MainAbName);
                _manifest = _mainAb.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
            }

            // 加载依赖包
            string[] dependencies = _manifest.GetAllDependencies(abName);
            foreach (var dep in dependencies)
            {
                if (!_abDic.ContainsKey(dep))
                {
                    var ab = UnityEngine.AssetBundle.LoadFromFile(PathUrl + dep);
                    _abDic.Add(dep, ab);
                }
            }
        }

        /// <summary>
        /// 加载AssetBundle，如果未加载则加载到字典
        /// </summary>
        /// <param name="abName">AssetBundle名称</param>
        private void LoadAssetBundle(string abName)
        {
            if (!_abDic.ContainsKey(abName))
            {
                var ab = UnityEngine.AssetBundle.LoadFromFile(PathUrl + abName);
                _abDic.Add(abName, ab);
            }
        }

        /// <summary>
        /// 同步加载资源（无类型）
        /// </summary>
        public Object LoadRes(string abName, string resName)
        {
            LoadMainAndDependencies(abName);
            LoadAssetBundle(abName);

            Object obj = _abDic[abName].LoadAsset(resName);
            return obj is GameObject ? Instantiate(obj) : obj;
        }

        /// <summary>
        /// 同步加载资源（指定类型） Lua
        /// </summary>
        public Object LoadRes(string abName, string resName, Type type)
        {
            LoadMainAndDependencies(abName);
            LoadAssetBundle(abName);

            Object obj = _abDic[abName].LoadAsset(resName, type);
            return obj is GameObject ? Instantiate(obj) : obj;
        }

        /// <summary>
        /// 同步加载资源（泛型版） C#
        /// </summary>
        public T LoadRes<T>(string abName, string resName) where T : Object
        {
            LoadMainAndDependencies(abName);
            LoadAssetBundle(abName);

            T obj = _abDic[abName].LoadAsset<T>(resName);
            return obj is GameObject ? Instantiate(obj) : obj;
        }

        #endregion

        #region 异步加载

         /// <summary>
        /// 异步加载主包与指定包的所有依赖包
        /// </summary>
        /// <param name="abName">资源所在的AssetBundle名</param>
        /// <param name="onComplete">加载完成回调</param>
        private IEnumerator LoadMainAndDependenciesAsync(string abName, Action onComplete)
        {
            while (_isAbLoading)
            {
                yield return null;
            }

            // 如果主包未加载，先异步加载主包及Manifest 
            if (_mainAb == null)
            {
                _isAbLoading = true;
                var mainRequest = UnityEngine.AssetBundle.LoadFromFileAsync(PathUrl + MainAbName);
                yield return mainRequest;

                _isAbLoading = false;
                _mainAb = mainRequest.assetBundle;
                _manifest = _mainAb.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
            }

            // 获取所有依赖包名
            string[] dependencies = _manifest.GetAllDependencies(abName);
            foreach (var dep in dependencies)
            {
                while (_isAbLoading)
                {
                    yield return null;
                }
                
                // 如果依赖包未加载，异步加载依赖包
                if (!_abDic.ContainsKey(dep))
                {
                    _isAbLoading = true;
                    var depRequest = UnityEngine.AssetBundle.LoadFromFileAsync(PathUrl + dep);
                    yield return depRequest;

                    _isAbLoading = false;
                    _abDic.Add(dep, depRequest.assetBundle);
                }
            }

            // 回调通知依赖包加载完成
            onComplete?.Invoke();
        }

        /// <summary>
        /// 异步加载指定AssetBundle
        /// </summary>
        /// <param name="abName">AssetBundle名称</param>
        /// <param name="onLoaded">加载完成回调，参数为加载的AssetBundle</param>
        private IEnumerator LoadAssetBundleAsync(string abName, Action<UnityEngine.AssetBundle> onLoaded)
        {
            // 如果已经加载过，直接回调
            if (_abDic.TryGetValue(abName, out var existingAb))
            {
                onLoaded?.Invoke(existingAb);
                yield break;
            }

            while (_isAbLoading)
            {
                yield return null;
            }

            // 异步加载AssetBundle文件
            _isAbLoading = true;
            var abRequest = UnityEngine.AssetBundle.LoadFromFileAsync(PathUrl + abName);
            yield return abRequest;
            
            _isAbLoading = false;

            if (abRequest.assetBundle != null)
            {
                _abDic.Add(abName, abRequest.assetBundle);
                onLoaded?.Invoke(abRequest.assetBundle);
            }
            else
            {
                Debug.LogError($"Failed to load AssetBundle: {abName}");
                onLoaded?.Invoke(null);
            }
        }
        
        /// <summary>
        /// 异步加载资源（不指定类型）
        /// </summary>
        public void LoadResAsync(string abName, string resName, Action<Object> onLoaded)
        {
            StartCoroutine(LoadResCoroutine(abName, resName, onLoaded));
        }

        private IEnumerator LoadResCoroutine(string abName, string resName, Action<Object> onLoaded)
        {
            Object result = null;

            // 1. 异步加载主包和依赖包
            yield return LoadMainAndDependenciesAsync(abName, () => { });

            // 2. 异步加载目标AB包
            yield return LoadAssetBundleAsync(abName, ab =>
            {
                if (ab != null)
                {
                    var request = ab.LoadAssetAsync(resName);
                    request.completed += _ =>
                    {
                        result = request.asset;
                    };
                }
            });

            while (result == null)
                yield return null;

            // 3. GameObject自动实例化
            if (result is GameObject go)
                onLoaded?.Invoke(GameObject.Instantiate(go));
            else
                onLoaded?.Invoke(result);
        }

        /// <summary>
        /// 异步加载资源（指定Type）
        /// </summary>
        public void LoadResAsync(string abName, string resName, Type type, Action<Object> onLoaded)
        {
            StartCoroutine(LoadResCoroutine(abName, resName, type, onLoaded));
        }

        private IEnumerator LoadResCoroutine(string abName, string resName, Type type, Action<Object> onLoaded)
        {
            Object result = null;

            // 1. 异步加载主包和依赖包
            yield return LoadMainAndDependenciesAsync(abName, () => { });

            // 2. 异步加载目标AB包
            yield return LoadAssetBundleAsync(abName, ab =>
            {
                if (ab != null)
                {
                    var request = ab.LoadAssetAsync(resName, type);
                    request.completed += _ =>
                    {
                        result = request.asset;
                    };
                }
            });

            while (result == null)
                yield return null;

            // 3. GameObject自动实例化
            if (result is GameObject go)
                onLoaded?.Invoke(GameObject.Instantiate(go));
            else
                onLoaded?.Invoke(result);
        }

        /// <summary>
        /// 外部调用的异步资源加载接口（泛型）
        /// </summary>
        /// <typeparam name="T">资源类型</typeparam>
        /// <param name="abName">资源所在AssetBundle名</param>
        /// <param name="resName">资源名</param>
        /// <param name="onLoaded">资源加载完成回调，参数为资源对象</param>
        public void LoadResAsync<T>(string abName, string resName, Action<T> onLoaded) where T : Object
        {
            StartCoroutine(LoadResCoroutine(abName, resName, onLoaded));
        }

        /// <summary>
        /// 资源异步加载协程实现
        /// </summary>
        private IEnumerator LoadResCoroutine<T>(string abName, string resName, Action<T> onLoaded) where T : Object
        {
            T result = null;

            // 1. 异步加载主包及依赖包
            yield return LoadMainAndDependenciesAsync(abName, () => { });

            // 2. 异步加载目标资源包
            yield return LoadAssetBundleAsync(abName, ab =>
            {
                if (ab != null)
                {
                    // 异步加载资源
                    var request = ab.LoadAssetAsync<T>(resName);
                    request.completed += _ =>
                    {
                        result = request.asset as T;
                    };
                }
            });

            // 等待资源加载完成
            while (result == null)
                yield return null;

            // 3. 如果资源是GameObject，实例化后返回，否则直接返回资源对象
            if (result is GameObject go)
            {
                onLoaded?.Invoke(GameObject.Instantiate(go) as T);
            }
            else
            {
                onLoaded?.Invoke(result);
            }
        }

        #endregion
        
        /// <summary>
        /// 卸载指定资源包
        /// </summary>
        /// <param name="abName">包名</param>
        /// <param name="unloadAllLoadedObjects">是否卸载内存中所有资源对象</param>
        public void Unload(string abName, bool unloadAllLoadedObjects = false)
        {
            if (_abDic.TryGetValue(abName, out var ab))
            {
                ab.Unload(unloadAllLoadedObjects);
                _abDic.Remove(abName);
            }
        }

        /// <summary>
        /// 卸载所有加载的资源包
        /// </summary>
        /// <param name="unloadAllLoadedObjects">是否卸载内存中所有资源对象</param>
        public void UnloadAll(bool unloadAllLoadedObjects = false)
        {
            UnityEngine.AssetBundle.UnloadAllAssetBundles(unloadAllLoadedObjects);
            _abDic.Clear();

            _mainAb = null;
            _manifest = null;
        }
    }
}