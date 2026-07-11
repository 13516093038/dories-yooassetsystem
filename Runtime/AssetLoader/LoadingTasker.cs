using System.Collections.Generic;
using Dories.YooAssetSystem.Runtime.LogSystem;

namespace Dories.YooassetSystem.Runtime.AssetLoader
{
    /// <summary>
    /// Single-Flight 加载任务追踪器
    /// </summary>
    internal class LoadingTasker
    {
        /// <summary>
        /// 追踪进行中的加载任务 (Single-Flight for Async)
        /// </summary>
        private readonly Dictionary<string, object> _LoadingTasks;
        private ILog _logger;


        public LoadingTasker(ILog logger)
        {
            _LoadingTasks = new Dictionary<string, object>();
            _logger = logger;
        }

        /// <summary>
        /// 检查是否有正在执行的加载任务
        /// </summary>
        /// <param name="assetName"></param>
        /// <returns></returns>
        public bool HasLoadingTask(string assetName)
        {
            return _LoadingTasks.ContainsKey(assetName);
        }

        /// <summary>
        /// 獲取正在執行的載入任務
        /// </summary>
        /// <param name="assetName"></param>
        /// <param name="task"></param>
        /// <returns></returns>
        public bool TryGetLoadingTask(string assetName, out object task)
        {
            task = default;
            if (_LoadingTasks.TryGetValue(assetName, out var existingTask))
            {
                task = existingTask;
                return true;
            }
            return false;
        }

        /// <summary>
        /// 嘗試添加正在執行的載入任務
        /// </summary>
        /// <param name="assetName"></param>
        /// <param name="task"></param>
        public void TryAddLoadingTask(string assetName, object task)
        {
            if (_LoadingTasks.TryAdd(assetName, task))
            {
                _logger.Debug($"[AssetLoader] Adding loading flag: {assetName}.");
            }     
        }

        /// <summary>
        /// 嘗試移除正在執行的載入任務
        /// </summary>
        /// <param name="assetName"></param>
        public void TryRemoveLoadingTask(string assetName)
        {
            if (_LoadingTasks.ContainsKey(assetName))
            {
                _LoadingTasks.Remove(assetName);
                _logger.Debug($"[AssetLoader] Removing loading flag: {assetName}.");
            }
        }
    }
}