using System;
using System.Collections.Generic;
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

        public struct DownloaderOperationBuilderFactor
        {
            public string[] tags;
            public int maxConcurrency;
            public int retryCount;

            public DownloaderOperationBuilderFactor(string[] tags, int maxConcurrency, int retryCount)
            {
                this.tags = tags;
                this.maxConcurrency = maxConcurrency;
                this.retryCount = retryCount;
            }
        }

        internal Dictionary<string, Action<DownloadProgressChangedEventArgs>> _onDownloadProgressChanged;
        internal Dictionary<string, Action<DownloadCompletedEventArgs>> _onDownloadCompleted;
        internal Dictionary<string, Action<DownloadErrorEventArgs>> _onDownloadError;
        internal Dictionary<string, Action<DownloadFileStartedEventArgs>> _onDownloadFileStarted;

        internal Action<DownloadProgressChangedEventArgs> _allDownloadProgressChanged;
        internal Action<DownloadCompletedEventArgs> _allDownloadCompleted;
        internal Action<DownloadErrorEventArgs> _allDownloadError;
        internal Action<DownloadFileStartedEventArgs> _allDownloadFileStarted;

        internal Action _allPackageDownloadCompleted;
        internal Action<string> _onDownloadFailed;

        private Dictionary<string, List<DownloaderOperationBuilderFactor>> _downloaderOperationBuilderFactors;
        private Dictionary<string, List<ResourceDownloaderOperation>> _resourceDownloaderOperations;

        private DownloaderOperation _curDownloader;
        private List<PatchEntity.PackageInfo> _packageInfos;
        private ILog _logger;

        public DownloadStatus Status { get; internal set; }

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
            _downloaderOperationBuilderFactors = new Dictionary<string, List<DownloaderOperationBuilderFactor>>();
            _resourceDownloaderOperations = new Dictionary<string, List<ResourceDownloaderOperation>>();
            BuildPackageInfoFactors();
        }

        /// <summary>
        /// 构建检视器中配置的下载器因子
        /// </summary>
        private void BuildPackageInfoFactors()
        {
            foreach (var packageInfo in _packageInfos)
            {
                if (packageInfo.DownloadTags == null || packageInfo.DownloadTags.Length == 0)
                {
                    continue;
                }

                var factors = new List<DownloaderOperationBuilderFactor>();
                factors.Add(new DownloaderOperationBuilderFactor(packageInfo.DownloadTags,
                    packageInfo.DownloadingMaxNum, packageInfo.FailedTryAgain));
                
               _downloaderOperationBuilderFactors[packageInfo.PackageName] = factors;
            }
        }

        /// <summary>
        /// 额外添加下载因子
        /// </summary>
        /// <param name="packageName"></param>
        /// <param name="tag"></param>
        /// <param name="maxConcurrency"></param>
        /// <param name="retryCount"></param>
        public void AddDownloaderFactorWithTag(string packageName, string tag, int maxConcurrency, int retryCount)
        {
            if (!_downloaderOperationBuilderFactors.TryGetValue(packageName, out var downloaderOperationBuilderFactors))
            {
                downloaderOperationBuilderFactors = new List<DownloaderOperationBuilderFactor>();
                _downloaderOperationBuilderFactors[packageName] = downloaderOperationBuilderFactors;
            }

            downloaderOperationBuilderFactors.Add(new DownloaderOperationBuilderFactor(new string[] { tag }, maxConcurrency, retryCount));
        }

        /// <summary>
        /// 额外添加下载因子
        /// </summary>
        /// <param name="packageName"></param>
        /// <param name="tags"></param>
        /// <param name="maxConcurrency"></param>
        /// <param name="retryCount"></param>
        public void AddDownloaderFactorWithTags(string packageName, string[] tags, int maxConcurrency, int retryCount)
        {
            if (!_downloaderOperationBuilderFactors.TryGetValue(packageName, out var downloaderOperationBuilderFactors))
            {
                downloaderOperationBuilderFactors = new List<DownloaderOperationBuilderFactor>();
                _downloaderOperationBuilderFactors[packageName] = downloaderOperationBuilderFactors;
            }

            downloaderOperationBuilderFactors.Add(new DownloaderOperationBuilderFactor(tags, maxConcurrency, retryCount));
        }

        /// <summary>
        /// 通过下载因子构建下载器（外部调用）
        /// </summary>
        public void BuildDownloaders()
        {
            foreach (var packageInfo in _packageInfos)
            {
               
                if (_downloaderOperationBuilderFactors.TryGetValue(packageInfo.PackageName,
                        out List<DownloaderOperationBuilderFactor> downloaderOperationBuilderFactors))
                {
                    //有相关下载因子
                    _resourceDownloaderOperations[packageInfo.PackageName] = BuildDownloaderOperations(
                        downloaderOperationBuilderFactors, packageInfo.IsCombineDownloader, packageInfo.PackageName);
                }
                else
                {
                    //没有相关下载因子，则全量更新
                    _resourceDownloaderOperations[packageInfo.PackageName] = new List<ResourceDownloaderOperation>();
                    _resourceDownloaderOperations[packageInfo.PackageName].Add(YooAssets.GetPackage(packageInfo.PackageName)
                        .CreateResourceDownloader(new ResourceDownloaderOptions(packageInfo.DownloadingMaxNum, packageInfo.FailedTryAgain)));
                }
            }
        }

        /// <summary>
        /// 通过下载因子构建下载器
        /// </summary>
        /// <param name="downloaderOperationBuilderFactors"></param>
        /// <param name="isCombineDownloader"></param>
        /// <param name="packageName"></param>
        /// <returns></returns>
        private List<ResourceDownloaderOperation> BuildDownloaderOperations(
            List<DownloaderOperationBuilderFactor> downloaderOperationBuilderFactors, bool isCombineDownloader, string packageName)
        {
            List<ResourceDownloaderOperation> downloaderOperations = new List<ResourceDownloaderOperation>();
            foreach(var factor in downloaderOperationBuilderFactors)
            {
                downloaderOperations.Add(YooAssets.GetPackage(packageName)
                    .CreateResourceDownloader(new ResourceDownloaderOptions(factor.tags, factor.maxConcurrency, factor.retryCount)));
            }

            if(isCombineDownloader)
            {
                var combineDownloader = downloaderOperations[0];
                for(int i = 1; i < downloaderOperations.Count; i++)
                {
                     combineDownloader.Combine(downloaderOperations[i]);
                }
                downloaderOperations.Clear();
                downloaderOperations.Add(combineDownloader);
            }

            return downloaderOperations;
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
            _logger.Info("Start download");
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
