using System;
using System.Collections.Generic;
#if DORIES_UNITASK_SUPPORT
using Cysharp.Threading.Tasks;
#else
using System.Threading.Tasks;
#endif
using Dories.YooAssetSystem.LogSystem;
using UnityEngine;
using YooAsset;
using Object = UnityEngine.Object;

namespace Dories.YooassetSystem.AssetLoader.Runtime
{
    public class AssetLoaderEntity : MonoBehaviour
    {
        [SerializeField] private string ILog;

        private ResourcePackage _defaultPackage;

        private ILog _logger;

        public void SetDefaultPackage(string packageName)
        {
            _defaultPackage = YooAssets.GetPackage(packageName);
        }   
    }

    internal class PackageAssetGroup
    {
        private ILog _logger;
        private ResourcePackage _package;
        private LoadingTasker _loadingTasker;
        private Dictionary<string, HandleBase> _cacheDic;
        private Dictionary<string, int> _refCountDic;

        public PackageAssetGroup(ResourcePackage package, ILog logger)
        {
            _package = package;
            _cacheDic = new Dictionary<string, HandleBase>();
            _refCountDic = new Dictionary<string, int>();
            _loadingTasker = new LoadingTasker();
            _logger = logger;
        }

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
                var source = (UniTaskCompletionSource<T>)existingTask;
#else
                var source = (TaskCompletionSource<T>)existingTask;
#endif

                var asset = await source.Task;

                if (asset != null)
                {
                    _refCountDic[assetName]++;
                }

                return asset;
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
                var completionSource = new UniTaskCompletionSource<T>();
#else
                var completionSource = new TaskCompletionSource<T>();
#endif
                _loadingTasker.TryAddLoadingTask(assetName, completionSource);

                try
                {
                    handle = _package.LoadAssetAsync<T>(assetName, priority);
                    await handle;
                    var asset = ((AssetHandle)handle).AssetObject as T;
                    completionSource.TrySetResult(asset);
                    return asset;
                }
                catch (Exception e)
                {
                    completionSource.TrySetException(e);
                    throw;
                }
                finally
                {
                    _loadingTasker.TryRemoveLoadingTask(assetName);
                }
            }
        }
    }
}