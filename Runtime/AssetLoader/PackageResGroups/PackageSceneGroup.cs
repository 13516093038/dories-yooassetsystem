using System;
using System.Collections.Generic;
#if DORIES_UNITASK_SUPPORT
using Cysharp.Threading.Tasks;
#else
using System.Threading.Tasks;
#endif
using Dories.YooassetSystem.Runtime.AssetLoader;
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

        /// <summary>
        /// 异步加载场景。
        /// allowSceneActivation=true：等到场景激活完成再返回；
        /// allowSceneActivation=false：等到可激活（进度约 0.9）即返回，业务再调用 SceneLoadResult.ActivateAsync。
        /// </summary>
#if DORIES_UNITASK_SUPPORT
        public async UniTask<SceneLoadResult> LoadSceneAsync(
#else
        public async Task<SceneLoadResult> LoadSceneAsync(
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
                var source = (UniTaskCompletionSource<SceneLoadResult>)existingTask;
#else
                var source = (TaskCompletionSource<SceneLoadResult>)existingTask;
#endif
                return await source.Task;
            }

            if (sceneMode == LoadSceneMode.Additive
                && _cacheDic.TryGetValue(sceneLocation, out var cachedHandle)
                && cachedHandle.IsValid)
            {
                _logger.Debug($"[AssetLoader] Scene: {sceneLocation} loaded from cache");
                return new SceneLoadResult(sceneLocation, cachedHandle);
            }

#if DORIES_UNITASK_SUPPORT
            var completionSource = new UniTaskCompletionSource<SceneLoadResult>();
#else
            var completionSource = new TaskCompletionSource<SceneLoadResult>();
#endif
            _loadingTasker.TryAddLoadingTask(sceneLocation, completionSource);

#if UNITY_EDITOR
            var loadTime = DateTime.Now;
#endif
            try
            {
                if (sceneMode == LoadSceneMode.Single)
                {
                    _cacheDic.Clear();
#if UNITY_EDITOR
                    _packageResLoadViewInfo.SceneLoadInfos.Clear();
#endif
                }

                var handle = _package.LoadSceneAsync(sceneLocation, sceneMode, physicsMode, allowSceneActivation, priority);

                if (allowSceneActivation)
                {
                    await handle;

                    if (handle.Status != EOperationStatus.Succeeded)
                    {
                        var error = string.IsNullOrEmpty(handle.Error)
                            ? $"Load scene failed: {sceneLocation}"
                            : handle.Error;
                        throw new Exception(error);
                    }
                }
                else
                {
                    await WaitUntilSceneReady(handle);

                    if (handle.Status == EOperationStatus.Failed)
                    {
                        var error = string.IsNullOrEmpty(handle.Error)
                            ? $"Load scene failed: {sceneLocation}"
                            : handle.Error;
                        throw new Exception(error);
                    }
                }

                _cacheDic[sceneLocation] = handle;
                var result = new SceneLoadResult(sceneLocation, handle);
                completionSource.TrySetResult(result);
                _logger.Debug(
                    allowSceneActivation
                        ? $"[AssetLoader] Scene: {sceneLocation} loaded and activated"
                        : $"[AssetLoader] Scene: {sceneLocation} ready for activation, progress: {handle.Progress}");

#if UNITY_EDITOR
                var costTime = (float)DateTime.Now.Subtract(loadTime).TotalMilliseconds;
                _packageResLoadViewInfo.SceneLoadInfos[sceneLocation] = new ResLoadInfo
                {
                    LoadTime = costTime,
                    RefCount = 0
                };
#endif

                return result;
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
        /// 等待场景加载到可激活状态（Unity 在 allowSceneActivation=false 时进度停在约 0.9）
        /// </summary>
#if DORIES_UNITASK_SUPPORT
        private static async UniTask WaitUntilSceneReady(SceneHandle handle)
#else
        private static async Task WaitUntilSceneReady(SceneHandle handle)
#endif
        {
            while (handle.IsValid && !handle.IsDone)
            {
                if (handle.Status == EOperationStatus.Failed)
                    return;

                if (handle.Progress >= 0.9f)
                    return;

#if DORIES_UNITASK_SUPPORT
                await UniTask.Yield();
#else
                await Task.Yield();
#endif
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
#if UNITY_EDITOR
            _packageResLoadViewInfo.SceneLoadInfos.Remove(sceneLocation);
#endif
            _logger.Debug($"[AssetLoader] Scene: {sceneLocation} unloaded successfully");
        }
    }
}
