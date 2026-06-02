using System.Threading.Tasks;
using Dories.Fsm.Runtime;
using Dories.YooassetSystem.Patch.Runtime.Operations;
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
            _ = UpdatePackageManifestTask();
        }

        private async Task UpdatePackageManifestTask()
        {
            foreach (var packageInfo in Owner.packagesInfoList)
            {

                var operation = new DefaultUpdatePackageManifestOperation().UpdatePackageManifest(
                    YooAssets.GetPackage(packageInfo.PackageName), packageInfo.PackageVersion, packageInfo.Timeout);
                await operation;
                if (operation.Status != EOperationStatus.Succeeded)
                {
                    Owner.m_OnPatchFail?.Invoke(operation.Error);
                    return;
                }
            }

            ChangeState<YooAssetCreateDownloaderState>(m_Fsm);
        }
    }
}