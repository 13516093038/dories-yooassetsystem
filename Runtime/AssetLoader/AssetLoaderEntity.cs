using System;
using System.Collections.Generic;
#if DORIES_UNITASK_SUPPORT
using Cysharp.Threading.Tasks;
#else
using System.Threading.Tasks;
#endif
using Dories.YooAssetSystem.Runtime.LogSystem;
using UnityEngine;
using YooAsset;
using Object = UnityEngine.Object;

namespace Dories.YooassetSystem.Runtime.AssetLoader
{
    public class AssetLoaderEntity : MonoBehaviour
    {
        [SerializeField] private string ILog;

        private ResourcePackage _defaultPackage;
        private ILog _logger;
        private Dictionary<string, PackageAssetGroup> _packageAssetGroupDic = new();
        private Dictionary<string, ResourcePackage> _packageDic = new();

        private void Awake()
        {
            _logger = CreateLog(ILog);
        }

        private ILog CreateLog(string log)
        {
            if (!string.IsNullOrEmpty(log))
            {
                try
                {
                    var type = ResolveType(log);
                    if (type != null)
                        return Activator.CreateInstance(type) as ILog;
                }
                catch (Exception e)
                {
                    Debug.LogError($"Create logger failed: {e}, Type name: {log}");
                }
            }

            return new BuildInLogEntity();
        }

        private static Type ResolveType(string typeName)
        {
            if (string.IsNullOrEmpty(typeName))
                return null;

            var type = Type.GetType(typeName);
            if (type != null)
                return type;

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                type = assembly.GetType(typeName);
                if (type != null)
                    return type;
            }

            return null;
        }

        private ResourcePackage GetPackage(string packageName)
        {
            if (string.IsNullOrEmpty(packageName))
            {
                _logger.Error("[AssetLoader] Package name is null or empty");
                return null;
            }

            if(_packageDic.TryGetValue(packageName, out var package))
            {
                return package;
            }

            if (YooAssets.TryGetPackage(packageName, out package))
            {
                _packageDic[packageName] = package;
                return package;
            }
            else
            {
                _logger.Error($"[AssetLoader] Package: {packageName} not found, cannot get package");
                return null;
            }
        }

        public void SetDefaultPackage(string packageName)
        {
            _defaultPackage = GetPackage(packageName);
            if (_defaultPackage == null)
            {
                _logger.Error($"[AssetLoader] Package: {packageName} not found, cannot set as default package");
            }
        }

        #region Load Assets

#if DORIES_UNITASK_SUPPORT
        public async UniTask<T> LoadAssetAsync<T>(string assetName, uint priority = 0) where T : Object
#else
        public async Task<T> LoadAssetAsync<T>(string assetName, uint priority = 0) where T : Object
#endif
        {
            if (_defaultPackage == null)
            {
                _logger.Error("[AssetLoader] Default package is not set, cannot load asset");
                return null;
            }

            if (string.IsNullOrEmpty(assetName))
            {
                _logger.Error("[AssetLoader] Asset name is null or empty, cannot load asset");
                return null;
            }

            if (!_packageAssetGroupDic.TryGetValue(_defaultPackage.PackageName, out var packageAssetGroup))
            {
                packageAssetGroup = new PackageAssetGroup(_defaultPackage, _logger);
                _packageAssetGroupDic[_defaultPackage.PackageName] = packageAssetGroup;
            }
            return await packageAssetGroup.LoadAssetAsync<T>(assetName, priority);
        }


#if DORIES_UNITASK_SUPPORT
        public async UniTask<T> LoadAssetAsync<T>(string packageName, string assetName, uint priority = 0) where T : Object
#else
        public async Task<T> LoadAssetAsync<T>(string packageName, string assetName, uint priority = 0) where T : Object
#endif
        {
            if (string.IsNullOrEmpty(packageName) || string.IsNullOrEmpty(assetName))
            {
                _logger.Error($"[AssetLoader] Package name or asset name is null or empty");
                return null;
            }

            if (!_packageAssetGroupDic.TryGetValue(packageName, out var packageAssetGroup))
            {
                var package = GetPackage(packageName);
                if (package == null)
                {
                    _logger.Error($"[AssetLoader] Package: {packageName} not found, cannot load asset: {assetName}");
                    return null;
                }
                else
                {
                    packageAssetGroup = new PackageAssetGroup(package, _logger);
                    _packageAssetGroupDic[packageName] = packageAssetGroup;
                }
            }

            return await packageAssetGroup.LoadAssetAsync<T>(assetName, priority);
        }

        #endregion

        #region  UnloadAssets

        public void UnloadAsset(string assetName, bool forceUnload = false)
        {
            if (_defaultPackage == null)
            {
                _logger.Error("[AssetLoader] Default package is not set, cannot load asset");
                return;
            }

            if (string.IsNullOrEmpty(assetName))
            {
                return;
            }

            if (_packageAssetGroupDic.TryGetValue(_defaultPackage.PackageName, out var packageAssetGroup))
            {
                packageAssetGroup.UnloadAsset(assetName, forceUnload);
            }
            else
            {
                _logger.Error($"[AssetLoader] Asset: {assetName} not found, cannot unload");
            }
        }

        public void UnloadAsset(string packageName, string assetName, bool forceUnload = false)
        {
            if (string.IsNullOrEmpty(packageName) || string.IsNullOrEmpty(assetName))
            {
                _logger.Error($"[AssetLoader] Package name or asset name is null or empty");
                return;
            }

            if (!_packageAssetGroupDic.TryGetValue(packageName, out var packageAssetGroup))
            {
                _logger.Error($"[AssetLoader] Asset: {assetName} not found, cannot unload");
                return;
            }

            packageAssetGroup.UnloadAsset(assetName, forceUnload);
        }

        #endregion
    }

    internal class PackageAssetGroup
    {
        private ILog _logger;
        private ResourcePackage _package;
        private LoadingTasker _loadingTasker;
        private Dictionary<string, HandleBase> _cacheDic;
        private Dictionary<string, int> _refCountDic;

        protected readonly Dictionary<string, List<bool>> _pendingUnloads;

        public PackageAssetGroup(ResourcePackage package, ILog logger)
        {
            _package = package;
            _cacheDic = new Dictionary<string, HandleBase>();
            _refCountDic = new Dictionary<string, int>();
            _loadingTasker = new LoadingTasker(logger);
            _pendingUnloads = new Dictionary<string, List<bool>>();
            _logger = logger;
        }

        #region Asset 加载

#if DORIES_UNITASK_SUPPORT
        public async UniTask<T> LoadAssetAsync<T>(string assetName, uint priority = 0) where T : Object
#else
        public async Task<T> LoadAssetAsync<T>(string assetName, uint priority = 0) where T : Object
#endif
        {
            if (string.IsNullOrEmpty(assetName))
            {
                return null;
            }

            //检查是否有正在执行的加载任务
            if (_loadingTasker.TryGetLoadingTask(assetName, out var existingTask))
            {
                _logger.Debug($"[AssetLoader] Asset: {assetName} is loading, waiting for existing task...");
#if DORIES_UNITASK_SUPPORT
                var source = (UniTaskCompletionSource<Object>)existingTask;
#else
                var source = (TaskCompletionSource<Object>)existingTask;
#endif

                var asset = await source.Task;

                if (asset != null)
                {
                    _refCountDic[assetName]++;
                }

                return asset as T;
            }

            //从缓存拿
            if (_cacheDic.TryGetValue(assetName, out HandleBase handle))
            {
                _refCountDic[assetName]++;
                return ((AssetHandle)handle).AssetObject as T;
            }
            else
            {
                //创建加载任务
#if DORIES_UNITASK_SUPPORT
                var completionSource = new UniTaskCompletionSource<Object>();
#else
                var completionSource = new TaskCompletionSource<Object>();
#endif
                _loadingTasker.TryAddLoadingTask(assetName, completionSource);

                try
                {
                    handle = _package.LoadAssetAsync<T>(assetName, priority);
                    await handle;
                    var asset = ((AssetHandle)handle).AssetObject as T;
                    _cacheDic.Add(assetName, handle);
                    _refCountDic.Add(assetName, 1);
                    completionSource.TrySetResult(asset);
                    _logger.Debug(
                        $"[AssetLoader] Asset: {assetName} loaded successfully, ref count: {_refCountDic[assetName]}");
                    return asset;
                }
                catch (Exception e)
                {
                    _logger.Error($"[AssetLoader] Asset: {assetName} loaded failed, error: {e}");
                    completionSource.TrySetException(e);
                    throw;
                }
                finally
                {
                    _loadingTasker.TryRemoveLoadingTask(assetName);
                    ProcessPendingUnloads(assetName);
                }
            }
        }
#endregion

        public void UnloadAsset(string assetName, bool forceUnload)
        {
            if (string.IsNullOrEmpty(assetName))
            {
                return;
            }

            if (_loadingTasker.HasLoadingTask(assetName))
            {
                AddPendingUnload(assetName, forceUnload);
            }
            else
            {
                UnLoadAssetCore(assetName, forceUnload);
            }
        }

        private void UnloadAssetCoreAsync(string assetName, bool forceUnload)
        {
            //是否有正在加载的该资源
            if (_loadingTasker.HasLoadingTask(assetName))
            {
                AddPendingUnload(assetName, forceUnload);
            }

        }

        private void AddPendingUnload(string assetName,bool forceUnload)
        {
            if (_pendingUnloads.TryGetValue(assetName, out var list))
            {
                list.Add(forceUnload);
            }
            else
            {
                list = new List<bool> { forceUnload };
                _pendingUnloads[assetName] = list;
            }
        }

        private void UnLoadAssetCore(string assetName, bool forceUnload)
        {
            if(!_cacheDic.TryGetValue(assetName, out HandleBase handle))
            {
                _logger.Warn($"[AssetLoader] Asset: {assetName} not found in cache, cannot unload");
            }
            else
            {
                _refCountDic[assetName]--;
                if (_refCountDic[assetName] <= 0 || forceUnload)
                {
                    _cacheDic.Remove(assetName);
                    _refCountDic.Remove(assetName);

                    handle.Release();
                    _package.TryUnloadUnusedAsset(assetName);
                }
            }
        }

        private void ProcessPendingUnloads(string assetName)
        {
            var pendingList = GetAndClearPendingUnloads(assetName);
            if (pendingList == null || pendingList.Count == 0)
                return;

            _logger.Debug($"[AssetLoader] Asset: {assetName}, pending count: {pendingList.Count}");

            // 按順序執行所有待執行的卸載
            foreach (var forceUnload in pendingList)
            {
                UnLoadAssetCore(assetName, forceUnload);
            }
        }

        private List<bool> GetAndClearPendingUnloads(string assetName)
        {
            if (!_pendingUnloads.ContainsKey(assetName))
                return null;

            var pendingList = _pendingUnloads[assetName];
            _pendingUnloads.Remove(assetName);

            _logger.Debug($"[AssetLoader] Retrieved {pendingList.Count} pending unloads for: {assetName}");
            return pendingList;
        }
    }
}