using Cysharp.Threading.Tasks;
using Dories.Fsm.Runtime;
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
            RequestPackageVersionTask().Forget();
        }

        private async UniTask RequestPackageVersionTask()
        {
            foreach (var packageName in Owner.packagesNameList)
            {
                var operation = Owner.m_RequestPackageVersionOperation.RequestPackageVersion(Owner.m_PackageInfoDic[packageName].Package);
                await operation;
                if (operation.Status == EOperationStatus.Succeed)
                {
                    Owner.m_PackageInfoDic[packageName].PackageVersion = operation.PackageVersion;
                }
                else
                {
                    Owner.m_OnPatchFail?.Invoke(operation.Error);
                    return;
                }
            }
           
            ChangeState<YooAssetUpdatePackageManifestState>(m_Fsm);
        }
    }
}