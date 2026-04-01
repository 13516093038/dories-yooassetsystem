using System.Collections.Generic;
using UnityEngine;

namespace Dories.YooassetSystem.Runtime.AssetLoader.Cacher
{
    /// <summary>
    /// Single-Flight 加载任务追踪器
    /// </summary>
    public class LoadingTasker
    {
        /// <summary>
        /// 追踪进行中的加载任务 (Single-Flight for Async)
        /// </summary>
        private readonly Dictionary<string, object> m_LoadingTasks;

        public LoadingTasker()
        {
            m_LoadingTasks = new Dictionary<string, object>();
        }

        /// <summary>
        /// 检查是否有正在执行的加载任务
        /// </summary>
        /// <param name="assetName"></param>
        /// <returns></returns>
        public bool HasLoadingTask(string assetName)
        {
            return this.m_LoadingTasks.ContainsKey(assetName);
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
            if (this.m_LoadingTasks.TryGetValue(assetName, out var existingTask))
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
            if (m_LoadingTasks.TryAdd(assetName, task))
                Debug.Log($"Adding loading flag: {assetName}.");
        }

        /// <summary>
        /// 嘗試移除正在執行的載入任務
        /// </summary>
        /// <param name="assetName"></param>
        public void TryRemoveLoadingTask(string assetName)
        {
            if (this.m_LoadingTasks.ContainsKey(assetName))
            {
                this.m_LoadingTasks.Remove(assetName);
                Debug.Log($"Removing loading flag: {assetName}.");
            }
        }
    }
}