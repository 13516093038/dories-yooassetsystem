using System.Threading.Tasks;
using Dories.Fsm.Runtime;
using Dories.YooassetSystem.Patch.Runtime.Operations;
using YooAsset;

namespace Dories.YooassetSystem.Runtime.Patch.States
{
    public class YooAssetRequestPackageVersionState : FsmState<PatchEntity>
    {
        private Fsm<PatchEntity> m_Fsm;
        
        protected override void OnEnter(Fsm<PatchEntity> fsm)
        {
            base.OnEnter(fsm);
            
            m_Fsm = fsm;
            _ = RequestPackageVersionTask();
        }

        private async Task RequestPackageVersionTask()
        {
            foreach (var packageInfo in Owner.packagesInfoList)
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
                    Owner.m_OnPatchFail?.Invoke("Request package version failed");
                    return;
                }

                packageInfo.PackageVersion = version;
            }
           
            ChangeState<YooAssetUpdatePackageManifestState>(m_Fsm);
        }
    }
}