using System.Threading.Tasks;
using Dories.Fsm.Runtime;
using Dories.YooAssetSystem.Patch.Runtime.Operations;
using Dories.YooAssetSystem.Runtime.Patch.BuildInFsmSystem;
using YooAsset;

namespace Dories.YooAssetSystem.Runtime.Patch.States
{
    public class YooAssetUpdatePackageManifestState : FsmNodeEntity<PatchEntity>
    {
        private Fsm<PatchEntity> m_Fsm;

        protected internal override void OnEnter()
        {
            _ = UpdatePackageManifestTask();
        }

        private async Task UpdatePackageManifestTask()
        {
            foreach (var packageInfo in _owner.packagesInfoList)
            {
                var operation = new DefaultUpdatePackageManifestOperation().UpdatePackageManifest(
                    YooAssets.GetPackage(packageInfo.PackageName), packageInfo.PackageVersion, packageInfo.Timeout);
                await operation;
                if (operation.Status != EOperationStatus.Succeeded)
                {
                    _owner._patchFailed?.Invoke(operation.Error);
                    _owner._patchError?.Invoke(operation.Error);
                    return;
                }
            }

            ChangeState<YooAssetCreateDownloaderState>();
        }
    }
}