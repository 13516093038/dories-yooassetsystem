using System;
using System.Collections.Generic;
#if DORIES_UNITASK_SUPPORT
using Cysharp.Threading.Tasks;
#else
using System.Threading.Tasks;
#endif
using Dories.YooAssetSystem.Runtime.LogSystem;
using UnityEngine.SceneManagement;
using YooAsset;

namespace Dories.YooassetSystem.Runtime.AssetLoader.PackageResGroups
{
    public sealed class PackageSceneGroup : PackageResGroup
    {
        private LoadingTasker _loadingTasker;
        private Dictionary<string, SceneHandle> _cacheDic;

        public PackageSceneGroup(ResourcePackage package, ILog logger) : base(package, logger)
        {
            _cacheDic = new Dictionary<string, SceneHandle>();
            _loadingTasker = new LoadingTasker(logger);
        }

#if DORIES_UNITASK_SUPPORT
        public async UniTask<SceneHandle> LoadSceneAsync(
#else
        public async Task<SceneHandle> LoadSceneAsync(
#endif
            string sceneLocation,
            LoadSceneMode sceneMode = LoadSceneMode.Single,
            LocalPhysicsMode physicsMode = LocalPhysicsMode.None,
            bool allowSceneActivation = true,
            uint priority = 0)
        {
            if (string.IsNullOrEmpty(sceneLocation))
            {
                _logger.Error("[AssetLoader] Scene location is null or empty");
                return null;
            }

            if (_loadingTasker.TryGetLoadingTask(sceneLocation, out var existingTask))
            {
                _logger.Debug($"[AssetLoader] Scene: {sceneLocation} is loading, waiting for existing task...");
#if DORIES_UNITASK_SUPPORT
                var source = (UniTaskCompletionSource<SceneHandle>)existingTask;
#else
                var source = (TaskCompletionSource<SceneHandle>)existingTask;
#endif
                return await source.Task;
            }

            if (sceneMode == LoadSceneMode.Additive
                && _cacheDic.TryGetValue(sceneLocation, out var cachedHandle)
                && cachedHandle.IsValid)
            {
                _logger.Debug($"[AssetLoader] Scene: {sceneLocation} loaded from cache");
                return cachedHandle;
            }

#if DORIES_UNITASK_SUPPORT
            var completionSource = new UniTaskCompletionSource<SceneHandle>();
#else
            var completionSource = new TaskCompletionSource<SceneHandle>();
#endif
            _loadingTasker.TryAddLoadingTask(sceneLocation, completionSource);

            try
            {
                if (sceneMode == LoadSceneMode.Single)
                    _cacheDic.Clear();

                var handle = _package.LoadSceneAsync(sceneLocation, sceneMode, physicsMode, allowSceneActivation, priority);
                await handle;

                if (handle.Status != EOperationStatus.Succeeded)
                {
                    var error = string.IsNullOrEmpty(handle.Error)
                        ? $"Load scene failed: {sceneLocation}"
                        : handle.Error;
                    throw new Exception(error);
                }

                _cacheDic[sceneLocation] = handle;
                completionSource.TrySetResult(handle);
                _logger.Debug($"[AssetLoader] Scene: {sceneLocation} loaded successfully");
                return handle;
            }
            catch (Exception e)
            {
                _logger.Error($"[AssetLoader] Scene: {sceneLocation} loaded failed, error: {e}");
                completionSource.TrySetException(e);
                throw;
            }
            finally
            {
                _loadingTasker.TryRemoveLoadingTask(sceneLocation);
            }
        }

        /// <summary>
        /// 卸载已加载的场景
        /// </summary>
#if DORIES_UNITASK_SUPPORT
        public async UniTask UnloadSceneAsync(string sceneLocation)
#else
        public async Task UnloadSceneAsync(string sceneLocation)
#endif
        {
            if (string.IsNullOrEmpty(sceneLocation))
                return;

            if (_loadingTasker.HasLoadingTask(sceneLocation))
            {
                _logger.Warn($"[AssetLoader] Scene: {sceneLocation} is loading, cannot unload now");
                return;
            }

            if (!_cacheDic.TryGetValue(sceneLocation, out var handle) || !handle.IsValid)
            {
                _logger.Warn($"[AssetLoader] Scene: {sceneLocation} not found in cache, cannot unload");
                return;
            }

            var unloadOp = handle.UnloadSceneAsync();
            await unloadOp;

            if (unloadOp.Status != EOperationStatus.Succeeded)
            {
                _logger.Error($"[AssetLoader] Scene: {sceneLocation} unload failed, error: {unloadOp.Error}");
                return;
            }

            _cacheDic.Remove(sceneLocation);
            _logger.Debug($"[AssetLoader] Scene: {sceneLocation} unloaded successfully");
        }
    }
}
