using System;
using System.Collections.Generic;
#if DORIES_UNITASK_SUPPORT
using Cysharp.Threading.Tasks;
#else
using System.Threading.Tasks;
#endif
using Dories.Logging;
using YooAsset;

namespace Dories.YooassetSystem.Runtime.AssetLoader.PackageResGroups
{
    /// <summary>
    /// RawFile 资源组，负责原生文件的加载与卸载
    /// </summary>
    public sealed class PackageRawFileGroup : PackageResGroup
    {
        private LoadingTasker _loadingTasker;
        private Dictionary<string, AssetHandle> _cacheDic;
        private Dictionary<string, int> _refCountDic;
        private readonly Dictionary<string, List<bool>> _pendingUnloads;

        public PackageRawFileGroup(ResourcePackage package) : base(package)
        {
            _cacheDic = new Dictionary<string, AssetHandle>();
            _refCountDic = new Dictionary<string, int>();
            _loadingTasker = new LoadingTasker();
            _pendingUnloads = new Dictionary<string, List<bool>>();
        }

#if DORIES_UNITASK_SUPPORT
        public async UniTask<RawFileObject> LoadRawFileAsync(string location, uint priority = 0)
#else
        public async Task<RawFileObject> LoadRawFileAsync(string location, uint priority = 0)
#endif
        {
            if (string.IsNullOrEmpty(location))
            {
                Log.Error("AssetLoader", "RawFile location is null or empty");
                return null;
            }

            if (_loadingTasker.TryGetLoadingTask(location, out var existingTask))
            {
                Log.Debug("AssetLoader", $"RawFile: {location} is loading, waiting for existing task...");
                var gate = (LoadGate)existingTask;
#if DORIES_UNITASK_SUPPORT
                    var mine = new UniTaskCompletionSource<RawFileObject>();
    #else
                    var mine = new TaskCompletionSource<RawFileObject>();
#endif
                gate.AddWaiter((rawFile, exception) =>
                {
                    if (exception != null)
                        mine.TrySetException(exception);
                    else
                        mine.TrySetResult(rawFile as RawFileObject);
                });
                var rawFile = await mine.Task;

                if (rawFile != null)
                {
                    _refCountDic[location]++;
#if UNITY_EDITOR
                    SyncRawFileRefCount(location);
#endif
                }
                return rawFile;
            }

            if (_cacheDic.TryGetValue(location, out var handle) && handle.IsValid)
            {
                _refCountDic[location]++;
#if UNITY_EDITOR
                SyncRawFileRefCount(location);
#endif
                return handle.GetAssetObject<RawFileObject>();
            }

            var loadGate = new LoadGate();
            _loadingTasker.TryAddLoadingTask(location, loadGate);

#if UNITY_EDITOR
            var loadTime = DateTime.Now;
#endif
            try
            {
                handle = _package.LoadAssetAsync<RawFileObject>(location, priority);
                await handle;

                if (handle.Status != EOperationStatus.Succeeded)
                {
                    var error = string.IsNullOrEmpty(handle.Error)
                        ? $"Load raw file failed: {location}"
                        : handle.Error;
                    throw new Exception(error);
                }

                var rawFileObject = handle.GetAssetObject<RawFileObject>();
                _cacheDic[location] = handle;
                _refCountDic[location] = 1;
                loadGate.SetResult(rawFileObject);
                Log.Debug("AssetLoader",
                    $"RawFile: {location} loaded successfully, ref count: {_refCountDic[location]}");

#if UNITY_EDITOR
                var costTime = (float)DateTime.Now.Subtract(loadTime).TotalMilliseconds;
                _packageResLoadViewInfo.RawFileLoadInfos[location] = new ResLoadInfo
                {
                    LoadTime = costTime,
                    RefCount = _refCountDic[location]
                };
#endif

                return rawFileObject;
            }
            catch (Exception e)
            {
                Log.Error("AssetLoader", $"RawFile: {location} loaded failed, error: {e}");
                loadGate.SetException(e);
                throw;
            }
            finally
            {
                _loadingTasker.TryRemoveLoadingTask(location);
                ProcessPendingUnloads(location);
            }
        }

#if DORIES_UNITASK_SUPPORT
        public async UniTask<byte[]> LoadRawFileBytesAsync(string location, uint priority = 0)
#else
        public async Task<byte[]> LoadRawFileBytesAsync(string location, uint priority = 0)
#endif
        {
            var rawFile = await LoadRawFileAsync(location, priority);
            return rawFile?.GetBytes();
        }

#if DORIES_UNITASK_SUPPORT
        public async UniTask<string> LoadRawFileTextAsync(string location, uint priority = 0)
#else
        public async Task<string> LoadRawFileTextAsync(string location, uint priority = 0)
#endif
        {
            var rawFile = await LoadRawFileAsync(location, priority);
            return rawFile?.GetText();
        }

        public void UnloadRawFile(string location, bool forceUnload = false)
        {
            if (string.IsNullOrEmpty(location))
                return;

            if (_loadingTasker.HasLoadingTask(location))
            {
                AddPendingUnload(location, forceUnload);
                return;
            }

            UnloadRawFileCore(location, forceUnload);
        }

        private void AddPendingUnload(string location, bool forceUnload)
        {
            if (_pendingUnloads.TryGetValue(location, out var list))
                list.Add(forceUnload);
            else
                _pendingUnloads[location] = new List<bool> { forceUnload };
        }

        private void UnloadRawFileCore(string location, bool forceUnload)
        {
            if (!_cacheDic.TryGetValue(location, out var handle))
            {
                Log.Warn("AssetLoader", $"RawFile: {location} not found in cache, cannot unload");
                return;
            }

            _refCountDic[location]--;
            if (_refCountDic[location] <= 0 || forceUnload)
            {
                _cacheDic.Remove(location);
                _refCountDic.Remove(location);

#if UNITY_EDITOR
                _packageResLoadViewInfo.RawFileLoadInfos.Remove(location);
#endif

                if (handle.IsValid)
                    handle.Release();

                _package.TryUnloadUnusedAsset(location);
                Log.Debug("AssetLoader", $"RawFile: {location} unloaded");
            }
            else
            {
#if UNITY_EDITOR
                SyncRawFileRefCount(location);
#endif
                Log.Debug("AssetLoader",
                    $"RawFile: {location} ref count decreased to {_refCountDic[location]}");
            }
        }

        private void ProcessPendingUnloads(string location)
        {
            if (!_pendingUnloads.TryGetValue(location, out var pendingList))
                return;

            _pendingUnloads.Remove(location);
            if (pendingList.Count == 0)
                return;

            Log.Debug("AssetLoader", $"RawFile: {location}, pending unload count: {pendingList.Count}");
            foreach (var forceUnload in pendingList)
                UnloadRawFileCore(location, forceUnload);
        }

#if UNITY_EDITOR
        private void SyncRawFileRefCount(string location)
        {
            if (!_packageResLoadViewInfo.RawFileLoadInfos.TryGetValue(location, out var info))
                return;

            info.RefCount = _refCountDic[location];
            _packageResLoadViewInfo.RawFileLoadInfos[location] = info;
        }
#endif
    }
}
