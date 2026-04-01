using System;
using YooAsset;

namespace Dories.YooassetSystem.Runtime.Patch.Operations.CreateDownloaderOperation
{
    public class DefaultCreateDownloaderOperation : IYooAssetCreateDownloaderOperation
    {
        private int m_DownloadingMaxNum = 10;
        private int m_FailedTryAgain = 3;
        private Action<int ,Action> OnNeedUpdateCallback = null;
        private DownloaderOperation.DownloaderFinish m_DownloaderFinish = null;
        private DownloaderOperation.DownloadUpdate m_DownloadUpdate = null;
        private DownloaderOperation.DownloadError m_DownloadError = null;
        private DownloaderOperation.DownloadFileBegin m_DownloadFileBegin = null;

        public Action<int ,Action> GetOnNeedUpdateCallback()
        {
            return OnNeedUpdateCallback;
        }

        public DefaultCreateDownloaderOperation BuildOnNeedUpdateCallback(Action<int ,Action> onNeedUpdateCallback)
        {
            OnNeedUpdateCallback = onNeedUpdateCallback;
            return this;
        }

        public DefaultCreateDownloaderOperation BuildDownloadingMaxNumber(int downloadingMaxNumber)
        {
            m_DownloadingMaxNum = downloadingMaxNumber;
            return this;
        }

        public DefaultCreateDownloaderOperation BuildFailedTryAgain(int failedTryAgain)
        {
            m_FailedTryAgain = failedTryAgain;
            return this;
        }

        public DefaultCreateDownloaderOperation BuildDownloaderFinish(DownloaderOperation.DownloaderFinish downloaderFinish)
        {
            m_DownloaderFinish = downloaderFinish;
            return this;
        }

        public DefaultCreateDownloaderOperation BuildDownloaderUpdate(DownloaderOperation.DownloadUpdate downloaderUpdate)
        {
            m_DownloadUpdate = downloaderUpdate;
            return this;
        }

        public DefaultCreateDownloaderOperation BuildDownloaderError(DownloaderOperation.DownloadError downloaderError)
        {
            m_DownloadError = downloaderError;
            return this;
        }

        public DefaultCreateDownloaderOperation BuildDownloaderFileBegin(
            DownloaderOperation.DownloadFileBegin downloaderFileBegin)
        {
            m_DownloadFileBegin = downloaderFileBegin;
            return this;
        }

        public ResourceDownloaderOperation CreateDownloader(ResourcePackage package)
        {
            var downloader = package.CreateResourceDownloader(m_DownloadingMaxNum, m_FailedTryAgain);
            downloader.DownloadFinishCallback = m_DownloaderFinish;
            downloader.DownloadUpdateCallback = m_DownloadUpdate;
            downloader.DownloadErrorCallback = m_DownloadError;
            downloader.DownloadFileBeginCallback = m_DownloadFileBegin;
            return downloader;
        }
    }
}