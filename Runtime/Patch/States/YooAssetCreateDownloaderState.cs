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
            _owner._needUpdateListener?.Invoke(patchDownloader);
            
            ChangeState<YooAssetDownloadPackageFilesState>();
            if (_owner.isAutoDownload)
            {
                patchDownloader.BuildDownloaders();
                patchDownloader.StartDownload();
            }
        }
    }
}