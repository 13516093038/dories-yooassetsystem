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
    public class YooAssetRequestPackageVersionState : FsmNodeEntity<PatchEntity>
    {
        protected internal override void OnEnter()
        {
            _ = RequestPackageVersionTask();
        }

#if DORIES_UNITASK_SUPPORT
        private async UniTask RequestPackageVersionTask()
#else
        private async Task RequestPackageVersionTask()
#endif
        {
            foreach (var packageInfo in _owner.packagesInfoList)
            {
                var package = YooAssets.GetPackage(packageInfo.PackageName);
                string version = string.Empty;
                if (packageInfo.IsSupportWeakOnline)
                {
                    version = await new WeakOnlineRequestPackageVersionOperation().RequestPackageVersion(package);
                }
                else
                {
                    version = await new DefaultRequestPackageVersionOperation().RequestPackageVersion(package);
                }

                if(string.IsNullOrEmpty(version))
                {
                    _owner._patchFailed?.Invoke("Request package version failed");
                    _owner._patchError?.Invoke("Request package version failed");
                    return;
                }

                packageInfo.PackageVersion = version;
            }
           
            ChangeState<YooAssetUpdatePackageManifestState>();
        }
    }
}