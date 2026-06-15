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

        internal Dictionary<string, Action<DownloadProgressChangedEventArgs>> _onDownloadProgressChanged;
        internal Dictionary<string, Action<DownloadCompletedEventArgs>> _onDownloadCompleted;
        internal Dictionary<string, Action<DownloadErrorEventArgs>> _onDownloadError;
        internal Dictionary<string, Action<DownloadFileStartedEventArgs>> _onDownloadFileStarted;

        internal List<string> _packageNames;
        internal Dictionary<string, ResourceDownloaderOptions> _downloaderOptions;

        internal Action _allPackageDownloadCompleted;
        private DownloaderOperation _curDownloader;

        public DownloadStatus Status { get; internal set; }
        public bool NeedDownload { get; internal set; }

        public string CurrentDownloadingPackage{get; internal set;}

        public PatchDownlaoder DownloadProgressChangedEventArgs(string packageName, Action<DownloadProgressChangedEventArgs> onDownloadProgressChanged)
        {
            if (!_onDownloadProgressChanged.TryAdd(packageName, onDownloadProgressChanged))
            {
                Debug.LogWarning($"DownloadProgressChanged event already exists for package: {packageName}, overwrite it");
                _onDownloadProgressChanged[packageName] = onDownloadProgressChanged;
            }
            return this;
        }

        public PatchDownlaoder DownloadCompletedEventArgs(string packageName, Action<DownloadCompletedEventArgs> onDownloadCompleted)
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

        public PatchDownlaoder DownloadFileStartedEventArgs(string packageName, Action<DownloadFileStartedEventArgs> onDownloadFileStarted)
        {
            _onDownloadFileStarted[packageName] = onDownloadFileStarted;
            return this;
        }

        public void SetDownloader(string packageName, ResourceDownloaderOptions option)
        {
            if (_downloaderOptions.TryAdd(packageName, option))
            {
                Debug.LogWarning($"Downloader option already exists for package: {packageName}, overwrite it");
                _downloaderOptions[packageName] = option;
            }
        }

        /// <summary>
        /// 开始下载
        /// </summary>
        public void StartDownload()
        {
            if (Status != DownloadStatus.Undo || Status != DownloadStatus.Cancel)
            {
                Debug.LogError("Download status is not undo or cancel");
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
            foreach (var downloaderName in _packageNames)
            {
                if (!_downloaderOptions.TryGetValue(downloaderName, out var option))
                {
                    option = new ResourceDownloaderOptions();
                }

                _curDownloader = YooAssets.GetPackage(downloaderName).CreateResourceDownloader(option);
                if (_onDownloadCompleted.TryGetValue(downloaderName, out var onDownloadCompleted))
                {
                    _curDownloader.DownloadCompleted += onDownloadCompleted;
                }
                if (_onDownloadProgressChanged.TryGetValue(downloaderName, out var onDownloadProgressChanged))
                {
                    _curDownloader.DownloadProgressChanged += onDownloadProgressChanged;
                }
                if (_onDownloadError.TryGetValue(downloaderName, out var onDownloadError))
                {
                    _curDownloader.DownloadError += onDownloadError;
                }
                if (_onDownloadFileStarted.TryGetValue(downloaderName, out var onDownloadFileStarted))
                {
                    _curDownloader.DownloadFileStarted += onDownloadFileStarted;
                }
                _curDownloader.StartDownload();
                Debug.Log("Start download: " + downloaderName);
                CurrentDownloadingPackage = downloaderName;
                await _curDownloader;
                _allPackageDownloadCompleted.Invoke();
            }
        }

        public void PauseDownload()
        {
            if (_curDownloader == null)
            {
                Debug.LogError("There is no downloading package");
                return;
            }
            Status = DownloadStatus.Pausing;
            _curDownloader?.PauseDownload();
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
                _curDownloader?.CancelDownload();
            }
        }

        internal PatchDownlaoder(List<string> packageNames)
        {
            Status = DownloadStatus.Undo;
            _packageNames = packageNames;
            _downloaderOptions = new Dictionary<string, ResourceDownloaderOptions>();
            _onDownloadProgressChanged = new Dictionary<string, Action<DownloadProgressChangedEventArgs>>();
            _onDownloadCompleted = new Dictionary<string, Action<DownloadCompletedEventArgs>>();
            _onDownloadError = new Dictionary<string, Action<DownloadErrorEventArgs>>();
            _onDownloadFileStarted = new Dictionary<string, Action<DownloadFileStartedEventArgs>>();
        }
    }
}