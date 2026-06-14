using System.Collections.Generic;
using Dories.Fsm.Runtime;
using Dories.YooassetSystem.Patch.Runtime.Operations;
using YooAsset;

namespace Dories.YooassetSystem.Runtime.Patch.States
{
    public class YooAssetCreateDownloaderState : FsmState<PatchEntity>
    {
        private Fsm<PatchEntity> m_Fsm;
        
        protected override void OnEnter(Fsm<PatchEntity> fsm)
        {
            base.OnEnter(fsm);
            
            m_Fsm = fsm;
            CreateDownloaderTask();
        }

        private void CreateDownloaderTask()
        {
            var createDownloaderOperation = new DefaultCreateDownloaderOperation();

            foreach (var packageInfo in Owner.packagesInfoList)
            {
                if (Owner.m_Downloaders == null)
                {
                    Owner.m_Downloaders = new();
                }

                var package = YooAssets.GetPackage(packageInfo.PackageName);
                Owner.m_Downloaders.Add(packageInfo.PackageName,
                    createDownloaderOperation.CreateDownloader(package, packageInfo.DownloadingMaxNum,
                        packageInfo.FailedTryAgain));
            }

            int totalDownloadCount = 0;
            List<string> packageNames = new List<string>();

            foreach (var packageName in Owner.m_Downloaders)
            {
                totalDownloadCount +=  packageName.Value.TotalDownloadCount;
                packageNames.Add(packageName.Key);
            }
            
            if ( totalDownloadCount == 0)
            {
                //无需下载
                ChangeState<YooAssetDownloadFileOverState>(m_Fsm);
            }
            else
            {
                var patchDowner = new PatchDownlaoder(packageNames);
                Owner._patchDowner = patchDowner;
                Owner._needUpdateListener?.Invoke(patchDowner);
                ChangeState<YooAssetDownloadPackageFilesState>(m_Fsm);
            }   
        }
    }
}