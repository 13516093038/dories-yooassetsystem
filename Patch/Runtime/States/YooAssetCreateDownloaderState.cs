using System.Collections.Generic;
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
            var packageNames = new List<string>();

            foreach (var packageInfo in _owner.packagesInfoList)
            {
                packageNames.Add(packageInfo.PackageName);
            }

            var patchDowner = new PatchDownlaoder(packageNames);

            foreach (var packageInfo in _owner.packagesInfoList)
            {
                patchDowner.DownloadAll(
                    packageInfo.PackageName,
                    packageInfo.DownloadingMaxNum,
                    packageInfo.FailedTryAgain);
            }

            var (totalCount, _) = patchDowner.GetTotalPendingDownload();
            patchDowner.NeedDownload = totalCount > 0;

            if (totalCount == 0)
            {
                ChangeState<YooAssetDownloadFileOverState>();
                return;
            }

            _owner._patchDowner = patchDowner;
            ChangeState<YooAssetDownloadPackageFilesState>();
            _owner._needUpdateListener?.Invoke(patchDowner);
        }
    }
}
