using Dories.YooAssetSystem.Runtime.Patch.BuildInFsmSystem;

namespace Dories.YooAssetSystem.Runtime.Patch.States
{
    public class YooAssetCreateDownloaderState : FsmNodeEntity<PatchEntity>
    {
        protected internal override void OnEnter()
        {
            CreateDownloaderTask();
        }

        private void CreateDownloaderTask()
        {
            var patchDownloader = new PatchDownloader(_owner.packagesInfoList, _logger);
            _owner._patchDowner = patchDownloader;

            if (_owner.isAutoDownload)
            {
                if (patchDownloader.NeedDownload)
                {
                    _logger.Info("Start download by auto download");
                    ChangeState<YooAssetDownloadPackageFilesState>();
                    _owner._needUpdateListener?.Invoke(patchDownloader);
                    patchDownloader.StartDownload();
                }
                else
                {
                    _logger.Info("No need to download, skip download");
                    ChangeState<YooAssetDownloadFileOverState>();
                }
            }
            else
            {
                //用户可以在此动态设置下载器
                  ChangeState<YooAssetDownloadPackageFilesState>();
                 _owner._needUpdateListener?.Invoke(patchDownloader);
            }
        }
    }
}