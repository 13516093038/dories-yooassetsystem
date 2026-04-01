using System.Collections.Generic;
using Dories.Componentization.Runtime;
using UnityEngine;

namespace Dories.YooassetSystem.Runtime.AssetLoader.Cacher
{
    internal abstract class AssetCache<T> : Entity
    {
        /// <summary>
        /// 處理類型
        /// </summary>
        internal enum ProcessType
        {
            RawFile,
            Scene,
            Asset
        }

        /// <summary>
        /// 資源 Pack 緩存
        /// </summary>
        protected Dictionary<string, T> m_Cacher;

        /// <summary>
        /// 任務追蹤器
        /// </summary>
        protected LoadingTasker m_LoadingTasker;

        /// <summary>
        /// 正在卸載標記
        /// </summary>
        protected readonly HashSet<string> m_UnloadingAssets;

        /// <summary>
        /// 待卸載任務請求
        /// </summary>
        protected readonly Dictionary<string, List<bool>> m_PendingUnloads;

        /// <summary>
        /// 嘗試計數器緩存
        /// </summary>
        protected Dictionary<string, RetryCounter> m_RetryCounters;

        /// <summary>
        /// 當前進度數量 (Progress)
        /// </summary>
        public float currentCount { get; protected set; }

        /// <summary>
        /// 總共進度數量 (Progress)
        /// </summary>
        public float totalCount { get; protected set; }

        /// <summary>
        /// 緩存數量
        /// </summary>
        public int count => this.m_Cacher.Count;

        public abstract bool HasInCache(string assetName);

        public abstract T GetFromCache(string assetName);

        public AssetCache()
        {
            this.m_Cacher = new Dictionary<string, T>();
            this.m_LoadingTasker = new LoadingTasker();
            this.m_UnloadingAssets = new HashSet<string>();
            this.m_PendingUnloads = new Dictionary<string, List<bool>>();
            this.m_RetryCounters = new Dictionary<string, RetryCounter>();
        }

        #region Loading Task
        /// <summary>
        /// 檢查是否有正在執行的載入任務
        /// </summary>
        /// <param name="assetName"></param>
        /// <returns></returns>
        protected bool HasLoadingTask(string assetName)
        {
            return this.m_LoadingTasker.HasLoadingTask(assetName);
        }

        /// <summary>
        /// 獲取正在執行的載入任務
        /// </summary>
        /// <param name="assetName"></param>
        /// <param name="task"></param>
        /// <returns></returns>
        protected bool TryGetLoadingTask(string assetName, out object task)
        {
            return this.m_LoadingTasker.TryGetLoadingTask(assetName, out task);
        }

        /// <summary>
        /// 嘗試添加正在執行的載入任務
        /// </summary>
        /// <param name="assetName"></param>
        /// <param name="task"></param>
        protected void TryAddLoadingTask(string assetName, object task)
        {
            this.m_LoadingTasker.TryAddLoadingTask(assetName, task);
        }

        /// <summary>
        /// 嘗試移除正在執行的載入任務
        /// </summary>
        /// <param name="assetName"></param>
        protected void TryRemoveLoadingTask(string assetName)
        {
            this.m_LoadingTasker.TryRemoveLoadingTask(assetName);
        }
        #endregion

        #region Unloading Flag
        /// <summary>
        /// 檢查資產是否正在執行卸載
        /// </summary>
        protected bool HasUnloadingFlag(string assetName)
        {
            return this.m_UnloadingAssets.Contains(assetName);
        }

        /// <summary>
        /// 標記資產正在執行卸載
        /// </summary>
        protected void AddUnloadingFlag(string assetName)
        {
            this.m_UnloadingAssets.Add(assetName);
        }

        /// <summary>
        /// 移除資產卸載標記
        /// </summary>
        protected void RemoveUnloadingFlag(string assetName)
        {
            this.m_UnloadingAssets.Remove(assetName);
        }
        #endregion

        #region Unloading Flag
        /// <summary>
        /// 檢查是否有待執行的卸載請求
        /// </summary>
        protected bool HasPendingUnload(string assetName)
        {
            return this.m_PendingUnloads.ContainsKey(assetName) && this.m_PendingUnloads[assetName].Count > 0;
        }

        /// <summary>
        /// 添加待執行的卸載請求
        /// </summary>
        protected void AddPendingUnload(string assetName, bool forceUnload)
        {
            if (!this.m_PendingUnloads.ContainsKey(assetName))
                this.m_PendingUnloads[assetName] = new List<bool>();

            this.m_PendingUnloads[assetName].Add(forceUnload);
            Debug.Log(
                $"Added pending unload for: {assetName}, total pending: {this.m_PendingUnloads[assetName].Count}");
        }

        /// <summary>
        /// 獲取並清除所有待執行的卸載請求
        /// </summary>
        protected List<bool> GetAndClearPendingUnloads(string assetName)
        {
            if (!this.m_PendingUnloads.ContainsKey(assetName))
                return null;

            var pendingList = this.m_PendingUnloads[assetName];
            this.m_PendingUnloads.Remove(assetName);
            
            Debug.Log($"Retrieved {pendingList.Count} pending unloads for: {assetName}");
            return pendingList;
        }
        #endregion

        #region Retry Counter
        /// <summary>
        /// 開始與建立重試計數器
        /// </summary>
        /// <param name="assetName"></param>
        /// <param name="maxRetryCount"></param>
        protected void StartRetryCounter(string assetName, byte maxRetryCount)
        {
            if (!this.m_RetryCounters.ContainsKey(assetName))
                this.m_RetryCounters.Add(assetName, new RetryCounter(maxRetryCount));
        }

        /// <summary>
        /// 獲取重試計數器
        /// </summary>
        /// <param name="assetName"></param>
        /// <returns></returns>
        protected RetryCounter GetRetryCounter(string assetName)
        {
            this.m_RetryCounters.TryGetValue(assetName, out RetryCounter retryCounter);
            return retryCounter;
        }

        /// <summary>
        /// 停止重試計數器
        /// </summary>
        /// <param name="assetName"></param>
        protected void StopRetryCounter(string assetName)
        {
            if (this.m_RetryCounters.ContainsKey(assetName))
                this.m_RetryCounters.Remove(assetName);
        }
        #endregion

        /// <summary>
        /// 檢查卸載條件
        /// </summary>
        /// <param name="assetName"></param>
        /// <param name="forceUnload"></param>
        /// <param name="processType"></param>
        /// <param name="unloadAction"></param>
        protected void CheckUnload(string assetName, bool forceUnload, ProcessType processType, System.Action<string, bool, ProcessType> unloadAction)
        {
            if (string.IsNullOrEmpty(assetName))
                return;

            // 如果正在載入, 將卸載請求加入待執行隊列
            if (this.HasLoadingTask(assetName))
            {
                this.AddPendingUnload(assetName, forceUnload);
                Debug.Log($"【Pending Unload】 Asset: {assetName} is loading, queued unload request.");
                return;
            }

            // 如果正在執行卸載, 跳過
            if (this.HasUnloadingFlag(assetName))
            {
                Debug.Log($"【Try Unload】 Asset: {assetName} is already unloading...");
                return;
            }

            unloadAction(assetName, forceUnload, processType);
        }

        /// <summary>
        /// 處理所有待執行的卸載
        /// </summary>
        /// <param name="assetName">資產名稱</param>
        /// <param name="unloadAction">自定義卸載方法</param>
        protected void ProcessPendingUnloads(string assetName, ProcessType processType, System.Action<string, bool, ProcessType> unloadAction)
        {
            var pendingList = this.GetAndClearPendingUnloads(assetName);
            if (pendingList == null || pendingList.Count == 0)
                return;

            Debug.Log($"【Processing Pending Unloads】 Asset: {assetName}, pending count: {pendingList.Count}");

            // 按順序執行所有待執行的卸載
            foreach (var forceUnload in pendingList)
            {
                unloadAction(assetName, forceUnload, processType);
            }
        }
    }
}
