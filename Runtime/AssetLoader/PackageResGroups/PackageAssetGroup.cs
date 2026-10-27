using System;
using System.Collections.Generic;
#if DORIES_UNITASK_SUPPORT
using Cysharp.Threading.Tasks;
#else
using System.Threading.Tasks;
#endif
using Dories.Logging;
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

        public PackageAssetGroup(ResourcePackage package) : base(package)
        {
            _cacheDic = new Dictionary<string, HandleBase>();
            _refCountDic = new Dictionary<string, int>();
            _loadingTasker = new LoadingTasker();
            _pendingUnloads = new Dictionary<string, List<bool>>();
        }

        #region Asset 加载

        public async void LoadAssetAsync<T>(string assetName, Action<T> callback, uint priority = 0) where T : Object
        {
            if (string.IsNullOrEmpty(assetName))
            {
                Log.Error("AssetLoader", "Asset name is null or empty");
                return;
            }

            try
            {
                callback?.Invoke(await LoadAssetAsync<T>(assetName, priority));
            }
            catch (Exception)
            {
                // 失败日志在返回值重载里已经写过。这里只接住异常，避免 async void 变成未处理异常。
            }
        }

#if DORIES_UNITASK_SUPPORT
        public async UniTask<T> LoadAssetAsync<T>(string assetName, uint priority = 0) where T : Object
#else
        public async Task<T> LoadAssetAsync<T>(string assetName, uint priority = 0) where T : Object
#endif
        {
            if (string.IsNullOrEmpty(assetName))
            {
                Log.Error("AssetLoader", "Asset name is null or empty");
                return null;
            }

            if (_loadingTasker.TryGetLoadingTask(assetName, out var existingTask))
            {
                Log.Debug("AssetLoader", $"Asset: {assetName} is loading, waiting for existing task...");
                var asset = await WaitLoading((LoadGate)existingTask);
                if (asset != null)
                    Retain(assetName);
                return asset as T;
            }

            if (_cacheDic.ContainsKey(assetName))
                return UseCached<T>(assetName);

            var gate = new LoadGate();
            _loadingTasker.TryAddLoadingTask(assetName, gate);
            DateTime loadTime = DateTime.Now;
            try
            {
                var handle = _package.LoadAssetAsync<T>(assetName, priority);
                await handle;
                var asset = FinishLoad<T>(assetName, handle, loadTime);
                gate.SetResult(asset);
                return asset;
            }
            catch (Exception e)
            {
                Log.Error("AssetLoader", $"Asset: {assetName} loaded failed, error: {e}");
                gate.SetException(e);
                throw;
            }
            finally
            {
                _loadingTasker.TryRemoveLoadingTask(assetName);
                ProcessPendingUnloads(assetName);
            }
        }

#if DORIES_UNITASK_SUPPORT
        private async UniTask<Object> WaitLoading(LoadGate gate)
#else
        private async Task<Object> WaitLoading(LoadGate gate)
#endif
        {
#if DORIES_UNITASK_SUPPORT
            var mine = new UniTaskCompletionSource<Object>();
#else
            var mine = new TaskCompletionSource<Object>();
#endif
            gate.AddWaiter((asset, exception) =>
            {
                if (exception != null)
                    mine.TrySetException(exception);
                else
                    mine.TrySetResult(asset as Object);
            });
            return await mine.Task;
        }

        private void Retain(string assetName)
        {
            _refCountDic[assetName]++;
#if UNITY_EDITOR
            SyncResLoadRefCount(assetName);
#endif
        }

        private T UseCached<T>(string assetName) where T : Object
        {
            Retain(assetName);
            return ((AssetHandle)_cacheDic[assetName]).AssetObject as T;
        }
#endregion

        private T FinishLoad<T>(string assetName, HandleBase handle, DateTime loadTime) where T : Object
        {
            var asset = ((AssetHandle)handle).AssetObject as T;
            _cacheDic.Add(assetName, handle);
            _refCountDic.Add(assetName, 1);
            Log.Debug("AssetLoader", $"Asset: {assetName} loaded successfully, ref count: {_refCountDic[assetName]}");

#if UNITY_EDITOR
            var resLoadInfo = new ResLoadInfo();
            resLoadInfo.LoadTime = (float)DateTime.Now.Subtract(loadTime).TotalMilliseconds;
            resLoadInfo.RefCount = _refCountDic[assetName];
            _packageResLoadViewInfo.ResLoadInfos.Add(assetName, resLoadInfo);
#endif
            return asset;
        }

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
                Log.Warn("AssetLoader", $"Asset: {assetName} not found in cache, cannot unload");
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

            Log.Debug("AssetLoader", $"Asset: {assetName}, pending count: {pendingList.Count}");

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

            Log.Debug("AssetLoader", $"Retrieved {pendingList.Count} pending unloads for: {assetName}");
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