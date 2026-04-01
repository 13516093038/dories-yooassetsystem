using Cysharp.Threading.Tasks;
using Dories.Fsm.Runtime;
using YooAsset;

namespace Dories.YooassetSystem.Runtime.Patch.States
{
    public class YooAssetUpdatePackageManifestState : FsmState<PatchEntity>
    {
        private Fsm<PatchEntity> m_Fsm;
        
        protected override void OnEnter(Fsm<PatchEntity> fsm)
        {
            base.OnEnter(fsm);
            
            m_Fsm = fsm;
            UpdatePackageManifestTask().Forget();
        }

        private async UniTask UpdatePackageManifestTask()
        {
            foreach (var packageName in Owner.packagesNameList)
            {
                var packageInfo = Owner.m_PackageInfoDic[packageName];
                var operation =
                    Owner.m_UpdatePackageManifestOperation.UpdatePackageManifest(packageInfo.Package,
                        packageInfo.PackageVersion);
                await operation;
                if (operation.Status != EOperationStatus.Succeed)
                {
                    Owner.m_OnPatchFail?.Invoke(operation.Error);
                    return;
                }
            }
            ChangeState<YooAssetCreateDownloaderState>(m_Fsm);
        }
    }
}