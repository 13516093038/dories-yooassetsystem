using System.Threading.Tasks;
using Dories.Fsm.Runtime;
using Dories.YooassetSystem.Patch.Runtime.Operations;
using YooAsset;

namespace Dories.YooassetSystem.Runtime.Patch.States
{
    public class YooAssetClearCacheBundleState : FsmState<PatchEntity>
    {
        protected override void OnEnter(Fsm<PatchEntity> fsm)
        {
            base.OnEnter(fsm);
            _ = ClearCacheBundle();
        }
        
        private async ValueTask ClearCacheBundle()
        {
            foreach (var packageInfo in Owner.packagesInfoList)
            {
                var operation = new DefaultClearCacheBundleOperation();

                var clearCacheOperation = operation.YooAssetClearCacheOperation(YooAssets.GetPackage(packageInfo.PackageName),
                    new ClearCacheOptions(ClearCacheMethods.ClearUnusedBundleFiles));
                
                await clearCacheOperation;
                if(clearCacheOperation.Status == EOperationStatus.Succeeded)
                {
                    //清理成功
                }
                else
                {
                   Owner._patchError?.Invoke(clearCacheOperation.Error);
                }
                Owner._patchCompleted();
            }
        }
    }
}