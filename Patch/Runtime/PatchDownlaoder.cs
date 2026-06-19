using System;
using System.Collections.Generic;
#if DORIES_UNITASK_SUPPORT
using Cysharp.Threading.Tasks;
#else
using System.Threading.Tasks;
#endif
using UnityEngine;
using YooAsset;

namespace Dories.YooAssetSystem.Runtime.Patch
{
    public class PatchDownlaoder
    {
        public enum DownloadStatus
        {
            Undo,
            Downloading,
            Pausing,
            Cancel,
            Complete,
            Fail,
        }

        private sealed class DownloadPlanItem
        {
            public ResourceDownloaderOptions? ResourceOptions;
            public BundleDownloaderOptions? BundleOptions;

            public static DownloadPlanItem FromResource(ResourceDownloaderOptions options)
            {
                return new DownloadPlanItem { ResourceOptions = options };
            }

            public static DownloadPlanItem FromBundle(BundleDownloaderOptions options)
            {
                return new DownloadPlanItem { BundleOptions = options };
            }
        }

        internal Dictionary<string, Action<DownloadProgressChangedEventArgs>> _onDownloadProgressChanged;
        internal Dictionary<string, Action<DownloadCompletedEventArgs>> _onDownloadCompleted;
        internal Dictionary<string, Action<DownloadErrorEventArgs>> _onDownloadError;
        internal Dictionary<string, Action<DownloadFileStartedEventArgs>> _onDownloadFileStarted;

        internal Action _allPackageDownloadCompleted;
        internal Action<string> _onDownloadFailed;

        private readonly List<string> _packageNames;
        private readonly Dictionary<string, List<DownloadPlanItem>> _downloadPlans;

        private DownloaderOperation _curDownloader;

        public DownloadStatus Status { get; internal set; }
        public bool NeedDownload { get; internal set; }

        public string CurrentDownloadingPackage { get; internal set; }

        #region 下载计划配置

        /// <summary>
        /// 下载该 Package 的全部差异资源（整包热更）
        /// </summary>
        public PatchDownlaoder DownloadAll(string packageName, int maxConcurrency, int retryCount)
        {
            return AddResourceOptions(packageName, new ResourceDownloaderOptions(maxConcurrency, retryCount));
        }

        /// <summary>
        /// 只下载指定 Tag 的资源
        /// </summary>
        public PatchDownlaoder DownloadByTag(string packageName, string tag, int maxConcurrency, int retryCount)
        {
            return AddResourceOptions(packageName, new ResourceDownloaderOptions(tag, maxConcurrency, retryCount));
        }

        /// <summary>
        /// 下载多个 Tag 的资源
        /// </summary>
        public PatchDownlaoder DownloadByTags(string packageName, string[] tags, int maxConcurrency, int retryCount)
        {
            return AddResourceOptions(packageName, new ResourceDownloaderOptions(tags, maxConcurrency, retryCount));
        }

        /// <summary>
        /// 追加一条按 Tag/全量的下载计划（同一 Package 多条会 Combine）
        /// </summary>
        public PatchDownlaoder AddResourceOptions(string packageName, ResourceDownloaderOptions option)
        {
            EnsureCanConfigure(packageName);
            GetOrCreatePlanList(packageName).Add(DownloadPlanItem.FromResource(option));
            return this;
        }

        /// <summary>
        /// 追加一条按 AssetInfo 的下载计划
        /// </summary>
        public PatchDownlaoder AddBundleOptions(string packageName, BundleDownloaderOptions option)
        {
            EnsureCanConfigure(packageName);
            GetOrCreatePlanList(packageName).Add(DownloadPlanItem.FromBundle(option));
            return this;
        }

        /// <summary>
        /// 用单条计划替换某 Package 的全部配置
        /// </summary>
        public PatchDownlaoder SetDownloadPlan(string packageName, ResourceDownloaderOptions option)
        {
            ClearDownloadPlan(packageName);
            return AddResourceOptions(packageName, option);
        }

        /// <summary>
        /// 清空某 Package 的下载计划
        /// </summary>
        public PatchDownlaoder ClearDownloadPlan(string packageName)
        {
            EnsureCanConfigure(packageName);
            _downloadPlans.Remove(packageName);
            return this;
        }

        /// <summary>
        /// 兼容旧接口，等价于 <see cref="AddResourceOptions"/>
        /// </summary>
        [Obsolete("请使用 AddResourceOptions 或 DownloadAll")]
        public void SetDownloader(string packageName, ResourceDownloaderOptions option)
        {
            AddResourceOptions(packageName, option);
        }

        #endregion

        #region 下载计划查询

        /// <summary>
        /// 该 Package 是否配置了下载计划
        /// </summary>
        public bool HasDownloadPlan(string packageName)
        {
            return _downloadPlans.TryGetValue(packageName, out var plans) && plans.Count > 0;
        }

        /// <summary>
        /// 合并后的待下载文件数
        /// </summary>
        public int GetPendingDownloadCount(string packageName)
        {
            var downloader = BuildCombinedDownloader(packageName);
            return downloader?.TotalDownloadCount ?? 0;
        }

        /// <summary>
        /// 合并后的待下载字节数
        /// </summary>
        public long GetPendingDownloadBytes(string packageName)
        {
            var downloader = BuildCombinedDownloader(packageName);
            return downloader?.TotalDownloadBytes ?? 0;
        }

        /// <summary>
        /// 所有 Package 的待下载总计
        /// </summary>
        public (int count, long bytes) GetTotalPendingDownload()
        {
            int count = 0;
            long bytes = 0;
            foreach (var packageName in _packageNames)
            {
                count += GetPendingDownloadCount(packageName);
                bytes += GetPendingDownloadBytes(packageName);
            }

            return (count, bytes);
        }

        #endregion

        #region 事件订阅

        public PatchDownlaoder DownloadProgressChangedEventArgs(string packageName,
            Action<DownloadProgressChangedEventArgs> onDownloadProgressChanged)
        {
            if (!_onDownloadProgressChanged.TryAdd(packageName, onDownloadProgressChanged))
            {
                Debug.LogWarning(
                    $"DownloadProgressChanged event already exists for package: {packageName}, overwrite it");
                _onDownloadProgressChanged[packageName] = onDownloadProgressChanged;
            }

            return this;
        }

        public PatchDownlaoder DownloadCompletedEventArgs(string packageName,
            Action<DownloadCompletedEventArgs> onDownloadCompleted)
        {
            if (!_onDownloadCompleted.TryAdd(packageName, onDownloadCompleted))
            {
                Debug.LogWarning($"DownloadCompleted event already exists for package: {packageName}, overwrite it");
                _onDownloadCompleted[packageName] = onDownloadCompleted;
            }

            return this;
        }

        public PatchDownlaoder DownloadErrorEventArgs(string packageName, Action<DownloadErrorEventArgs> onDownloadError)
        {
            if (!_onDownloadError.TryAdd(packageName, onDownloadError))
            {
                Debug.LogWarning($"DownloadError event already exists for package: {packageName}, overwrite it");
                _onDownloadError[packageName] = onDownloadError;
            }

            return this;
        }

        public PatchDownlaoder DownloadFileStartedEventArgs(string packageName,
            Action<DownloadFileStartedEventArgs> onDownloadFileStarted)
        {
            _onDownloadFileStarted[packageName] = onDownloadFileStarted;
            return this;
        }

        #endregion

        /// <summary>
        /// 开始下载
        /// </summary>
        public void StartDownload()
        {
            if (Status != DownloadStatus.Undo && Status != DownloadStatus.Cancel)
            {
                Debug.LogError("Download status is not undo or cancel");
                return;
            }

            if (_downloadPlans.Count == 0)
            {
                Debug.LogWarning("No download plan configured.");
                Status = DownloadStatus.Fail;
                _onDownloadFailed?.Invoke("No download plan configured.");
                return;
            }

            Status = DownloadStatus.Downloading;
            _ = DownloadTask();
        }

#if DORIES_UNITASK_SUPPORT
        private async UniTask DownloadTask()
#else
        private async Task DownloadTask()
#endif
        {
            foreach (var packageName in _packageNames)
            {
                if (!HasDownloadPlan(packageName))
                    continue;

                var combined = BuildCombinedDownloader(packageName);
                if (combined == null || combined.TotalDownloadCount == 0)
                    continue;

                _curDownloader = combined;
                SubscribeDownloadEvents(packageName);

                try
                {
                    _curDownloader.StartDownload();
                    Debug.Log("Start download: " + packageName);
                    CurrentDownloadingPackage = packageName;
                    await _curDownloader;

                    if (_curDownloader.Status != EOperationStatus.Succeeded)
                    {
                        Status = DownloadStatus.Fail;
                        _onDownloadFailed?.Invoke(_curDownloader.Error);
                        return;
                    }
                }
                finally
                {
                    UnsubscribeDownloadEvents(packageName);
                }
            }

            _downloadPlans.Clear();
            Status = DownloadStatus.Complete;
            _allPackageDownloadCompleted?.Invoke();
        }

        public void PauseDownload()
        {
            if (_curDownloader == null)
            {
                Debug.LogError("There is no downloading package");
                return;
            }

            Status = DownloadStatus.Pausing;
            _curDownloader.PauseDownload();
        }

        public void ResumeDownload()
        {
            if (Status != DownloadStatus.Pausing)
            {
                Debug.LogError("Download status is not pausing");
                return;
            }

            Status = DownloadStatus.Downloading;
            _curDownloader?.ResumeDownload();
        }

        public void CancelDownload()
        {
            if (_curDownloader == null)
            {
                Debug.LogError("There is no downloading package");
                return;
            }

            if (Status == DownloadStatus.Downloading)
            {
                Status = DownloadStatus.Cancel;
                _curDownloader.CancelDownload();
            }
        }

        internal PatchDownlaoder(List<string> packageNames)
        {
            Status = DownloadStatus.Undo;
            _packageNames = packageNames;
            _downloadPlans = new Dictionary<string, List<DownloadPlanItem>>();
            _onDownloadProgressChanged = new Dictionary<string, Action<DownloadProgressChangedEventArgs>>();
            _onDownloadCompleted = new Dictionary<string, Action<DownloadCompletedEventArgs>>();
            _onDownloadError = new Dictionary<string, Action<DownloadErrorEventArgs>>();
            _onDownloadFileStarted = new Dictionary<string, Action<DownloadFileStartedEventArgs>>();
        }

        private List<DownloadPlanItem> GetOrCreatePlanList(string packageName)
        {
            if (!_downloadPlans.TryGetValue(packageName, out var plans))
            {
                plans = new List<DownloadPlanItem>();
                _downloadPlans[packageName] = plans;
            }

            return plans;
        }

        private void EnsureCanConfigure(string packageName)
        {
            if (Status != DownloadStatus.Undo && Status != DownloadStatus.Cancel)
            {
                throw new InvalidOperationException("下载已开始，不能修改下载计划");
            }

            if (!_packageNames.Contains(packageName))
            {
                throw new ArgumentException($"未知 Package: {packageName}", nameof(packageName));
            }
        }

        private ResourceDownloaderOperation BuildCombinedDownloader(string packageName)
        {
            if (!_downloadPlans.TryGetValue(packageName, out var plans) || plans.Count == 0)
                return null;

            var package = YooAssets.GetPackage(packageName);
            ResourceDownloaderOperation combined = null;

            foreach (var item in plans)
            {
                ResourceDownloaderOperation partial;
                if (item.ResourceOptions.HasValue)
                {
                    partial = package.CreateResourceDownloader(item.ResourceOptions.Value);
                }
                else if (item.BundleOptions.HasValue)
                {
                    partial = package.CreateResourceDownloader(item.BundleOptions.Value);
                }
                else
                {
                    continue;
                }

                combined = combined == null ? partial : CombineDownloaders(combined, partial);
            }

            return combined;
        }

        private static ResourceDownloaderOperation CombineDownloaders(
            ResourceDownloaderOperation first,
            ResourceDownloaderOperation second)
        {
            first.Combine(second);
            return first;
        }

        private void SubscribeDownloadEvents(string packageName)
        {
            if (_onDownloadCompleted.TryGetValue(packageName, out var onDownloadCompleted))
                _curDownloader.DownloadCompleted += onDownloadCompleted;
            if (_onDownloadProgressChanged.TryGetValue(packageName, out var onDownloadProgressChanged))
                _curDownloader.DownloadProgressChanged += onDownloadProgressChanged;
            if (_onDownloadError.TryGetValue(packageName, out var onDownloadError))
                _curDownloader.DownloadError += onDownloadError;
            if (_onDownloadFileStarted.TryGetValue(packageName, out var onDownloadFileStarted))
                _curDownloader.DownloadFileStarted += onDownloadFileStarted;
        }

        private void UnsubscribeDownloadEvents(string packageName)
        {
            if (_curDownloader == null)
                return;

            if (_onDownloadCompleted.TryGetValue(packageName, out var onDownloadCompleted))
                _curDownloader.DownloadCompleted -= onDownloadCompleted;
            if (_onDownloadProgressChanged.TryGetValue(packageName, out var onDownloadProgressChanged))
                _curDownloader.DownloadProgressChanged -= onDownloadProgressChanged;
            if (_onDownloadError.TryGetValue(packageName, out var onDownloadError))
                _curDownloader.DownloadError -= onDownloadError;
            if (_onDownloadFileStarted.TryGetValue(packageName, out var onDownloadFileStarted))
                _curDownloader.DownloadFileStarted -= onDownloadFileStarted;
        }
    }
}
