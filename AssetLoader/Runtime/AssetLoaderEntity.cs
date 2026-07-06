using System.Collections.Generic;
using System.Threading.Tasks;
using Dories.YooAssetSystem.LogSystem;
using UnityEngine;
using YooAsset;

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

        private List<Task<AssetHandle>> _loadTasks;

        public PackageAssetGroup(ResourcePackage package, ILog logger)
        {
            _package = package;
            _cacheDic = new Dictionary<string, HandleBase>();
            _refCountDic = new Dictionary<string, int>();
            _loadingTasker = new LoadingTasker();
            _logger = logger;
        }

        public async Task<T> LoadAssetAsync<T>(string assetName, uint priority = 0) where T : Object
        {
            if (string.IsNullOrEmpty(assetName))
            {
                return null;
            }

            if (_loadingTasker.TryGetLoadingTask(assetName, out var existingTask))
            {
                _logger.Debug($"[AssetLoader] Asset: {assetName} is loading, waiting for existing task...");
                // 等待現有的任務完成並返回結果
                var source = (TaskCompletionSource<T>)existingTask;
                var asset = await source.Task;
                if (asset != null)
                {
                    pack = this.GetFromCache(assetName);
                    if (pack != null)
                    {
                        pack.AddRef();
                        //Debug.Log($"【Load Shared】 => Current << {nameof(CacheBundle)} >> Cache Count: {this.count}, asset: {assetName}, ref: {pack.refCount}");
                    }
                }
                return asset;
            }
        }



        private HandleBase LoadAssetAsync<T>(string assetName) where T : Object
        {
            //缓存命中
            if(_cacheDic.TryGetValue(assetName, out HandleBase handle))
            {
                _refCountDic[assetName]++;
                return handle;
            }
            //缓存没有命中
            else
            {
                //检查是否有正在执行的加载任务
                if(_loadingTasker.HasLoadingTask(assetName))
                {
                    return null;
                }
                //添加正在执行的加载任务
                _loadingTasker.TryAddLoadingTask(assetName, _package.LoadAssetAsync<T>(assetName));
                return null;
            }
        }
    }
}