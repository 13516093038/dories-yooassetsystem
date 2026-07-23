using System;
using System.Collections.Generic;
#if DORIES_UNITASK_SUPPORT
using Cysharp.Threading.Tasks;
#else
using System.Threading.Tasks;
#endif
using Dories.YooAssetSystem.Runtime.LogSystem;
using YooAsset;
using Object = UnityEngine.Object;

namespace Dories.YooassetSystem.Runtime.AssetLoader.PackageResGroups
{
    internal class PackageAssetGroup : PackageResGroup
    {
        private LoadingTasker _loadingTasker;
        private Dictionary<string, HandleBase> _cacheDic;
        private Dictionary<string, int> _refCountDic;

        protected readonly Dictionary<string, List<bool>> _pendingUnloads;

        public PackageAssetGroup(ResourcePackage package, ILog logger) : base(package, logger)
        {
            _cacheDic = new Dictionary<string, HandleBase>();
            _refCountDic = new Dictionary<string, int>();
            _loadingTasker = new LoadingTasker(logger);
            _pendingUnloads = new Dictionary<string, List<bool>>();
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
#if UNITY_EDITOR
                    SyncResLoadRefCount(assetName);
#endif
                }

                return asset as T;
            }

            //从缓存拿
            if (_cacheDic.TryGetValue(assetName, out HandleBase handle))
            {
                _refCountDic[assetName]++;
#if UNITY_EDITOR
                SyncResLoadRefCount(assetName);
#endif
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

#if UNITY_EDITOR
                DateTime loadTime = DateTime.Now;
#endif
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

#if UNITY_EDITOR
                    float costTime = (float)DateTime.Now.Subtract(loadTime).TotalMilliseconds;    
                    var resLoadInfo = new ResLoadInfo();
                    resLoadInfo.LoadTime = costTime;
                    resLoadInfo.RefCount = _refCountDic[assetName];
                    _packageResLoadViewInfo.ResLoadInfos.Add(assetName, resLoadInfo);
#endif


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

#if UNITY_EDITOR
                    RemoveResLoadInfo(assetName);
#endif

                    handle.Release();
                    _package.TryUnloadUnusedAsset(assetName);
                }
#if UNITY_EDITOR
                else
                {
                    SyncResLoadRefCount(assetName);
                }
#endif
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

#if UNITY_EDITOR
        private void SyncResLoadRefCount(string assetName)
        {
            if (!_packageResLoadViewInfo.ResLoadInfos.TryGetValue(assetName, out var info))
                return;

            info.RefCount = _refCountDic[assetName];
            _packageResLoadViewInfo.ResLoadInfos[assetName] = info;
        }

        private void RemoveResLoadInfo(string assetName)
        {
            _packageResLoadViewInfo.ResLoadInfos.Remove(assetName);
        }
#endif
    }
}