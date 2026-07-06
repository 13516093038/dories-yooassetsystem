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
        private bool _isNeedBuildDownloaders = true;
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
            _isNeedBuildDownloaders = true;
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
            _isNeedBuildDownloaders = true;
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

            _isNeedBuildDownloaders = false;
        }

        /// <summary>
        /// 通过下载因子构建下载器
        /// </summary>
        /// <param name="downloaderOperationBuilderFactors"></param>
        /// <param name="isCombineDownloader"></param>
        /// <param name="packageName"></param>
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
        /// 获取包裹所需下载大小
        /// </summary>
        /// <param name="packageName">包裹名称</param>
        /// <returns>下载大小</returns>
        public long GetNeedDownloadSize(string packageName)
        {
            if (_isNeedBuildDownloaders)
            {
                _logger.Error("Downloaders are not built, please call BuildDownloaders() first");
                return 0;
            }

            if (!_resourceDownloaderOperations.TryGetValue(packageName, out var downloaderOperations))
            {
                return 0;
            }
           
            long downloadSize = 0;
            foreach(var downloader in downloaderOperations)
            {
                downloadSize += downloader.TotalDownloadBytes;
            }
            return downloadSize;
        }

        /// <summary>
        /// 获取所有包裹所需下载大小
        /// </summary>
        /// <returns>下载大小</returns>
        public long GetAllNeedDownloadSize()
        {
            if (_isNeedBuildDownloaders)
            {
                _logger.Error("Downloaders are not built, please call BuildDownloaders() first");
                return 0;
            }

            long downloadSize = 0;
            foreach(var packageInfo in _packageInfos)
            {
                downloadSize += GetNeedDownloadSize(packageInfo.PackageName);
            }
            return downloadSize;
        }

        /// <summary>
        /// 获取包裹所需下载文件数量
        /// </summary>
        /// <param name="packageName">包裹名称</param>
        /// <returns>下载文件数量</returns>
        public int GetNeedDownloadCount(string packageName)
        {
            if (_isNeedBuildDownloaders)
            {
                _logger.Error("Downloaders are not built, please call BuildDownloaders() first");
                return 0;
            }

            if (!_resourceDownloaderOperations.TryGetValue(packageName, out var downloaderOperations))
            {
                return 0;
            }

            int downloadCount = 0;
            foreach(var downloader in downloaderOperations)
            {
                downloadCount += downloader.TotalDownloadCount;
            }
            return downloadCount;
        }

        /// <summary>
        /// 获取所有包裹所需下载文件数量
        /// </summary>
        /// <returns></returns>
        public int GetAllNeedDownloadCount()
        {
            if (_isNeedBuildDownloaders)
            {
                _logger.Error("Downloaders are not built, please call BuildDownloaders() first");
                return 0;
            }
        
            int downloadCount = 0;
            foreach(var packageInfo in _packageInfos)
            {
                downloadCount += GetNeedDownloadCount(packageInfo.PackageName);
            }
            return downloadCount;
        }

        /// <summary>
        /// 包裹下载进度变更事件
        /// </summary>
        /// <param name="packageName">包裹名称</param>
        /// <param name="onDownloadProgressChanged">回调事件</param>
        public void DownloadProgressChangedEventArgs(string packageName,
            Action<DownloadProgressChangedEventArgs> onDownloadProgressChanged)
        {
            if (!_onDownloadProgressChanged.TryAdd(packageName, onDownloadProgressChanged))
            {
                _logger.Warn(
                    $"DownloadProgressChanged event already exists for package: {packageName}, overwrite it");
                _onDownloadProgressChanged[packageName] = onDownloadProgressChanged;
            }
        }

        /// <summary>
        /// 包裹下载完成事件
        /// </summary>
        /// <param name="packageName">包裹名称</param>
        /// <param name="onDownloadCompleted">回调事件</param>
        public void DownloadCompletedEventArgs(string packageName,
            Action<DownloadCompletedEventArgs> onDownloadCompleted)
        {
            if (!_onDownloadCompleted.TryAdd(packageName, onDownloadCompleted))
            {
                _logger.Warn($"DownloadCompleted event already exists for package: {packageName}, overwrite it");
                _onDownloadCompleted[packageName] = onDownloadCompleted;
            }
        }

        /// <summary>
        /// 包裹下载错误事件
        /// </summary>
        /// <param name="packageName">包裹名称</param>
        /// <param name="onDownloadError">回调事件</param>
        public void DownloadErrorEventArgs(string packageName, Action<DownloadErrorEventArgs> onDownloadError)
        {
            if (!_onDownloadError.TryAdd(packageName, onDownloadError))
            {
                _logger.Warn($"DownloadError event already exists for package: {packageName}, overwrite it");
                _onDownloadError[packageName] = onDownloadError;
            }
        }

        /// <summary>
        /// 包裹下载文件开始事件
        /// </summary>
        /// <param name="packageName">包裹名称</param>
        /// <param name="onDownloadFileStarted">回调事件</param>
        public void DownloadFileStartedEventArgs(string packageName,
            Action<DownloadFileStartedEventArgs> onDownloadFileStarted)
        {
            _onDownloadFileStarted[packageName] = onDownloadFileStarted;
        }

        /// <summary>
        /// 设置所有包裹下载进度变更事件
        /// </summary>
        /// <param name="onDownloadProgressChanged"></param>
        public void SetAllDownloadProgressChanged(Action<DownloadProgressChangedEventArgs> onDownloadProgressChanged)
        {
            _allDownloadProgressChanged = onDownloadProgressChanged;
        }

        /// <summary>
        /// 设置所有包裹下载完成事件
        /// </summary>
        /// <param name="onDownloadCompleted"></param>
        public void SetAllDownloadCompleted(Action<DownloadCompletedEventArgs> onDownloadCompleted)
        {
            _allDownloadCompleted += onDownloadCompleted;
        }

        /// <summary>
        /// 设置所有包裹下载错误事件
        /// </summary>
        /// <param name="onDownloadError"></param>
        public void SetAllDownloadError(Action<DownloadErrorEventArgs> onDownloadError)
        {
            _allDownloadError = onDownloadError;
        }

        /// <summary>
        /// 设置所有包裹下载文件开始事件
        /// </summary>
        /// <param name="onDownloadFileStarted"></param>
        public void SetAllDownloadFileStarted(Action<DownloadFileStartedEventArgs> onDownloadFileStarted)
        {
            _allDownloadFileStarted = onDownloadFileStarted;
        }


        /// <summary>
        /// 开始下载
        /// </summary>
        public void StartDownload()
        {
            if (_isNeedBuildDownloaders)
            {
                _logger.Error("Downloaders are not built, please call BuildDownloaders() first");
                return;
            }

            //如果取消下载了需要重新构建下载器
            if (Status == DownloadStatus.Cancel)
            {
                _resourceDownloaderOperations.Clear();
                BuildDownloaders();
            }

            if(GetAllNeedDownloadCount() == 0)
            {
                _logger.Info("There is no package to download");
                _allPackageDownloadCompleted?.Invoke();
                return;
            }

            if (Status != DownloadStatus.Undo && Status != DownloadStatus.Cancel && Status != DownloadStatus.Fail)
            {
                _logger.Error("Download status is not undo or cancel");
                return;
            }

            _logger.Info($"Start download, total download size: {GetAllNeedDownloadSize()}, total download count: {GetAllNeedDownloadCount()}");
            
            Status = DownloadStatus.Downloading;
            _ = DownloadTask();
        }

#if DORIES_UNITASK_SUPPORT
        private async UniTask DownloadTask()
#else
        private async Task DownloadTask()
#endif
        {
            foreach (var downloaderOperation in _resourceDownloaderOperations)
            {
                string packageName = downloaderOperation.Key;
                foreach (var downloader in downloaderOperation.Value)
                {
                    //所有回调
                    downloader.DownloadCompleted += (args) => _allDownloadCompleted?.Invoke(args);
                    downloader.DownloadProgressChanged += (args) => _allDownloadProgressChanged?.Invoke(args);
                    downloader.DownloadError += (args) => _allDownloadError?.Invoke(args);
                    downloader.DownloadFileStarted += (args) => _allDownloadFileStarted?.Invoke(args);

                    //包裹回调
                    if (_onDownloadCompleted.TryGetValue(packageName, out var packageDownloadCompleted))
                    {
                        downloader.DownloadCompleted += packageDownloadCompleted;
                    }

                    if (_onDownloadProgressChanged.TryGetValue(packageName, out var packageDownloadProgressChanged))
                    {
                        downloader.DownloadProgressChanged += packageDownloadProgressChanged;
                    }

                    if (_onDownloadError.TryGetValue(packageName, out var packageDownloadError))
                    {
                        downloader.DownloadError += packageDownloadError;
                    }

                    if (_onDownloadFileStarted.TryGetValue(packageName, out var packageDownloadFileStarted))
                    {
                        downloader.DownloadFileStarted += packageDownloadFileStarted;
                    }

                    //开始下载
                    downloader.StartDownload();
                    _logger.Info(
                        $"Start download: {packageName}, download size: {(downloader.TotalDownloadBytes / 1024 / 1024):F2} MB, download count: {downloader.TotalDownloadCount}");
                    _curDownloader = downloader;
                    await _curDownloader;
                    if (_curDownloader.Status != EOperationStatus.Succeeded)
                    {
                        Status = DownloadStatus.Fail;
                        _logger.Error($"Download {packageName} error: {_curDownloader.Error}");
                        _onDownloadFailed?.Invoke(_curDownloader.Error);

                        _resourceDownloaderOperations.Clear();
                        return;
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
    }
}
