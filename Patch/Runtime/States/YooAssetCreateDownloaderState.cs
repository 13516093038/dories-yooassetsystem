using System.Collections.Generic;
using Dories.Fsm.Runtime;
using Dories.YooAssetSystem.Patch.Runtime.Operations;
using Dories.YooAssetSystem.Runtime.Patch.BuildInFsmSystem;
using YooAsset;

namespace Dories.YooAssetSystem.Runtime.Patch.States
{
    public class YooAssetCreateDownloaderState : FsmNodeEntity<PatchEntity>
    {
        private Fsm<PatchEntity> m_Fsm;
        
        protected internal override void OnEnter()
        {
            CreateDownloaderTask();
        }

        private void CreateDownloaderTask()
        {
            var createDownloaderOperation = new DefaultCreateDownloaderOperation();

            foreach (var packageInfo in _owner.packagesInfoList)
            {
                if (_owner.m_Downloaders == null)
                {
                    _owner.m_Downloaders = new();
                }

                var package = YooAssets.GetPackage(packageInfo.PackageName);
                _owner.m_Downloaders.Add(packageInfo.PackageName,
                    createDownloaderOperation.CreateDownloader(package, packageInfo.DownloadingMaxNum,
                        packageInfo.FailedTryAgain));
            }

            int totalDownloadCount = 0;
            List<string> packageNames = new List<string>();

            foreach (var packageName in _owner.m_Downloaders)
            {
                totalDownloadCount +=  packageName.Value.TotalDownloadCount;
                packageNames.Add(packageName.Key);
            }
            
            if ( totalDownloadCount == 0)
            {
                //无需下载
                ChangeState<YooAssetDownloadFileOverState>();
            }
            else
            {
                var patchDowner = new PatchDownlaoder(packageNames);
                _owner._patchDowner = patchDowner;
                _owner._needUpdateListener?.Invoke(patchDowner);
                ChangeState<YooAssetDownloadPackageFilesState>();
            }   
        }
    }
}