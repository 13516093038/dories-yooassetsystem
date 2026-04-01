using Cysharp.Threading.Tasks;
using Dories.Fsm.Runtime;
using YooAsset;

namespace Dories.YooassetSystem.Runtime.Patch.States
{
    public class YooAssetClearCacheBundleState : FsmState<PatchEntity>
    {
        protected override void OnEnter(Fsm<PatchEntity> fsm)
        {
            base.OnEnter(fsm);
            ClearCacheBundle().Forget();
        }
        
        private async UniTaskVoid ClearCacheBundle()
        {
            foreach (var packageName in Owner.packagesNameList)
            {
                var packageInfo = Owner.m_PackageInfoDic[packageName];
                var operation = Owner.m_ClearCacheBundleOperation.ClearCacheBundle(packageInfo.Package);
                await operation;
                if(operation.Status == EOperationStatus.Succeed)
                {
                    //清理成功
                }
                else
                {
                    Owner.m_OnPatchFail.Invoke(operation.Error);
                    return;
                }
            }
            Owner.m_OnPatchSuccess?.Invoke();
        }
    }
}