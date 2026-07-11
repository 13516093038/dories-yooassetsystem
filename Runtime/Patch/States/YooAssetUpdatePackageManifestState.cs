#if DORIES_UNITASK_SUPPORT
using Cysharp.Threading.Tasks;
#else
using System.Threading.Tasks;
#endif
using Dories.YooAssetSystem.Runtime.Patch.Operations;
using Dories.YooAssetSystem.Runtime.Patch.BuildInFsmSystem;
using YooAsset;

namespace Dories.YooAssetSystem.Runtime.Patch.States
{
    public class YooAssetUpdatePackageManifestState : FsmNodeEntity<PatchEntity>
    {
        protected internal override void OnEnter()
        {
            _ = UpdatePackageManifestTask();
        }

#if DORIES_UNITASK_SUPPORT
        private async UniTask UpdatePackageManifestTask()
#else
        private async Task UpdatePackageManifestTask()
#endif
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