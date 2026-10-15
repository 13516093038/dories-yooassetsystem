using Dories.YooAssetSystem.Runtime.Patch.BuildInFsmSystem;

namespace Dories.YooAssetSystem.Runtime.Patch.States
{
    public class YooAssetCreateDownloaderState : FsmNodeEntity<PatchEntity>
    {
        protected internal override void OnEnter()
        {
            if (!_owner.isReleaseMode || _owner.playMode == PlayMode.OfflinePlayMode)
            {
                ChangeState<YooAssetDownloadFileOverState>();
            }
            else
            {
                CreateDownloaderTask();
            }
        }

        private void CreateDownloaderTask()
        {
            var patchDownloader = new PatchDownloader(_owner.packagesInfoList, _logger);
            _owner._patchDowner = patchDownloader;
            _owner._needUpdateListener?.Invoke(patchDownloader);
            
            ChangeState<YooAssetDownloadPackageFilesState>();
        }
    }
}