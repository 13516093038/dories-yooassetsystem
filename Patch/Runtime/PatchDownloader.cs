using System;
using System.Collections.Generic;
using System.Linq;
using Dories.YooAssetSystem.Runtime.Patch.LogSystem;
#if DORIES_UNITASK_SUPPORT
using Cysharp.Threading.Tasks;
#else
using System.Threading.Tasks;
#endif
using UnityEngine;
using YooAsset;

namespace Dories.YooAssetSystem.Runtime.Patch
{
    public class PatchDownloader
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

        internal Dictionary<string, Action<DownloadProgressChangedEventArgs>> _onDownloadProgressChanged;
        internal Dictionary<string, Action<DownloadCompletedEventArgs>> _onDownloadCompleted;
        internal Dictionary<string, Action<DownloadErrorEventArgs>> _onDownloadError;
        internal Dictionary<string, Action<DownloadFileStartedEventArgs>> _onDownloadFileStarted;

        internal Action _allPackageDownloadCompleted;
        internal Action<string> _onDownloadFailed;

        private Dictionary<string, List<ResourceDownloaderOperation>> _resourceDownloaderOperations;

        private DownloaderOperation _curDownloader;
        private List<PatchEntity.PackageInfo> _packageInfos;
        private ILog _logger;

        public DownloadStatus Status { get; internal set; }
        public bool NeedDownload { get; internal set; }

        public string CurrentDownloadingPackage { get; internal set; }

        internal PatchDownloader(List<PatchEntity.PackageInfo> packageInfos, ILog logger)
        {
            _packageInfos = packageInfos;
            _logger  = logger;
            Status = DownloadStatus.Undo;
            _onDownloadProgressChanged = new Dictionary<string, Action<DownloadProgressChangedEventArgs>>();
            _onDownloadCompleted = new Dictionary<string, Action<DownloadCompletedEventArgs>>();
            _onDownloadError = new Dictionary<string, Action<DownloadErrorEventArgs>>();
            _onDownloadFileStarted = new Dictionary<string, Action<DownloadFileStartedEventArgs>>();
            _resourceDownloaderOperations = new Dictionary<string, List<ResourceDownloaderOperation>>();

            foreach(var packageInfo in packageInfos)
            {
                ApplyPackageInfoDownloadOptions(packageInfo);
            }

            RefreshNeedDownload();
        }

        public void AddDownloaderWithTag(string packageName, string tag, int maxConcurrency, int retryCount)
        {
            if (!_resourceDownloaderOperations.TryGetValue(packageName, out var resourceDownloaderOperations))
            {
                resourceDownloaderOperations = new List<ResourceDownloaderOperation>();
                _resourceDownloaderOperations[packageName] = resourceDownloaderOperations;
            }

            resourceDownloaderOperations.Add(YooAssets.GetPackage(packageName)
                .CreateResourceDownloader(new ResourceDownloaderOptions(tag, maxConcurrency, retryCount)));

            RefreshNeedDownload();
        }
        

        public void AddDownloaderWithTags(string packageName, string[] tags, int maxConcurrency, int retryCount)
        {
            if (!_resourceDownloaderOperations.TryGetValue(packageName, out var resourceDownloaderOperations))
            {
                resourceDownloaderOperations = new List<ResourceDownloaderOperation>();
                _resourceDownloaderOperations[packageName] = resourceDownloaderOperations;
            }
            
            resourceDownloaderOperations.Add(YooAssets.GetPackage(packageName)
                .CreateResourceDownloader(new ResourceDownloaderOptions(tags, maxConcurrency, retryCount)));

            RefreshNeedDownload();
        }


        /// <summary>
        /// 按 PackageInfo 配置应用下载计划（Tag 为空则整包热更）。
        /// </summary>
        private void ApplyPackageInfoDownloadOptions(PatchEntity.PackageInfo packageInfo)
        {
            if (packageInfo == null)
                throw new ArgumentNullException(nameof(packageInfo));

            var packageName = packageInfo.PackageName;
            var maxConcurrency = NormalizeMaxConcurrency(packageInfo.DownloadingMaxNum);
            var retryCount = NormalizeRetryCount(packageInfo.FailedTryAgain);

            var tags = packageInfo.DownloadTags;

            if (!_resourceDownloaderOperations.TryGetValue(packageName, out var resourceDownloaderOperations))
            {
                resourceDownloaderOperations = new List<ResourceDownloaderOperation>();
                _resourceDownloaderOperations[packageName] = resourceDownloaderOperations;
            }

            if(tags == null || tags.Length == 0)
            {
                resourceDownloaderOperations.Add(YooAssets.GetPackage(packageName)
                    .CreateResourceDownloader(new ResourceDownloaderOptions(maxConcurrency, retryCount)));
            }
            else
            {
                resourceDownloaderOperations.Add(YooAssets.GetPackage(packageName)
                    .CreateResourceDownloader(new ResourceDownloaderOptions(tags, maxConcurrency, retryCount)));
            }
        }

        /// <summary>
        /// 重新计算是否存在待下载资源。
        /// </summary>
        private void RefreshNeedDownload()

        {
            var (count, _) = GetTotalPendingDownload();
            NeedDownload = count > 0;
        }

        private static int NormalizeMaxConcurrency(int maxConcurrency)
        {
            if (maxConcurrency <= 0)
                return 10;
            return Mathf.Clamp(maxConcurrency, 1, 32);
        }

        private static int NormalizeRetryCount(int retryCount)
        {
            return retryCount > 0 ? retryCount : 3;
        }

        /// <summary>
        /// 合并后的待下载文件数
        /// </summary>
        public int GetPendingDownloadCount(string packageName)
        {
            int count = 0;
            foreach(var downloader in _resourceDownloaderOperations[packageName])
            {
                count += downloader.TotalDownloadCount;
            }
            return count;
        }

        /// <summary>
        /// 合并后的待下载字节数
        /// </summary>
        public long GetPendingDownloadBytes(string packageName)
        {
            long bytes = 0;
            foreach(var downloader in _resourceDownloaderOperations[packageName])
            {
                bytes += downloader.TotalDownloadBytes;
            }
            return bytes;
        }

        /// <summary>
        /// 所有 Package 的待下载总计
        /// </summary>
        public (int count, long bytes) GetTotalPendingDownload()
        {
            int count = 0;
            long bytes = 0;
            foreach (var packageInfo in _packageInfos)
            {
                count += GetPendingDownloadCount(packageInfo.PackageName);
                bytes += GetPendingDownloadBytes(packageInfo.PackageName);
            }

            return (count, bytes);
        }


        public PatchDownloader DownloadProgressChangedEventArgs(string packageName,
            Action<DownloadProgressChangedEventArgs> onDownloadProgressChanged)
        {
            if (!_onDownloadProgressChanged.TryAdd(packageName, onDownloadProgressChanged))
            {
                _logger.Warn(
                    $"DownloadProgressChanged event already exists for package: {packageName}, overwrite it");
                _onDownloadProgressChanged[packageName] = onDownloadProgressChanged;
            }

            return this;
        }

        public PatchDownloader DownloadCompletedEventArgs(string packageName,
            Action<DownloadCompletedEventArgs> onDownloadCompleted)
        {
            if (!_onDownloadCompleted.TryAdd(packageName, onDownloadCompleted))
            {
                _logger.Warn($"DownloadCompleted event already exists for package: {packageName}, overwrite it");
                _onDownloadCompleted[packageName] = onDownloadCompleted;
            }

            return this;
        }

        public PatchDownloader DownloadErrorEventArgs(string packageName, Action<DownloadErrorEventArgs> onDownloadError)
        {
            if (!_onDownloadError.TryAdd(packageName, onDownloadError))
            {
                _logger.Warn($"DownloadError event already exists for package: {packageName}, overwrite it");
                _onDownloadError[packageName] = onDownloadError;
            }

            return this;
        }

        public PatchDownloader DownloadFileStartedEventArgs(string packageName,
            Action<DownloadFileStartedEventArgs> onDownloadFileStarted)
        {
            _onDownloadFileStarted[packageName] = onDownloadFileStarted;
            return this;
        }

        /// <summary>
        /// 开始下载
        /// </summary>
        public void StartDownload()
        {
            if (Status != DownloadStatus.Undo && Status != DownloadStatus.Cancel && Status != DownloadStatus.Fail)
            {
                _logger.Error("Download status is not undo or cancel");
                return;
            }

            if(!NeedDownload)
            {
                _logger.Warn("No need to download");
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
            foreach (var packageInfo in _packageInfos)
            {
                if (!packageInfo.IsCombineDownloader)
                {
                    var downloader = BuildCombinedDownloader(packageInfo.PackageName);
                    _resourceDownloaderOperations[packageInfo.PackageName].Clear();
                    _resourceDownloaderOperations[packageInfo.PackageName].Add(downloader);
                }

                foreach (var downloader in _resourceDownloaderOperations[packageInfo.PackageName])
                {
                    _curDownloader = downloader;
                    SubscribeDownloadEvents(packageInfo.PackageName);

                    try
                    {
                        _curDownloader.StartDownload();
                        _logger.Info("Start download: " + packageInfo.PackageName);
                        CurrentDownloadingPackage = packageInfo.PackageName;
                        await _curDownloader;

                        if (_curDownloader.Status != EOperationStatus.Succeeded)
                        {
                            Status = DownloadStatus.Fail;
                            _logger.Error($"Download {packageInfo.PackageName} error: {_curDownloader.Error}");
                            _onDownloadFailed?.Invoke(_curDownloader.Error);
                            return;
                        }
                    }
                    finally
                    {
                        UnsubscribeDownloadEvents(packageInfo.PackageName);
                    }
                }
            }

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
                _logger.Error("Download status is not pausing");
                return;
            }

            Status = DownloadStatus.Downloading;
            _curDownloader?.ResumeDownload();
        }

        public void CancelDownload()
        {
            if (_curDownloader == null)
            {
                _logger.Error("There is no downloading package");
                return;
            }

            if (Status == DownloadStatus.Downloading)
            {
                Status = DownloadStatus.Cancel;
                _curDownloader.CancelDownload();
            }
        }

        private ResourceDownloaderOperation BuildCombinedDownloader(string packageName)
        {
            if (!_resourceDownloaderOperations.TryGetValue(packageName, out var resourceDownloaderOperations) ||
                resourceDownloaderOperations.Count == 0)
            {
                _logger.Error($"No resource downloader operations for package: {packageName}");
                 return null;
            }

            return resourceDownloaderOperations.Aggregate((a, b) => CombineDownloaders(a, b));
        }

        private ResourceDownloaderOperation CombineDownloaders(
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
