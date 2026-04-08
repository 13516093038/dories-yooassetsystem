using Cysharp.Threading.Tasks;
using Dories.Fsm.Runtime;
using YooAsset;

namespace Dories.YooassetSystem.Runtime.Patch.States
{
    public class YooAssetInitState : FsmState<PatchEntity>
    {
        private Fsm<PatchEntity> m_Fsm;
        
        protected override void OnEnter(Fsm<PatchEntity> fsm)
        {
            base.OnEnter(fsm);
            
            m_Fsm = fsm;
            InitTask().Forget();
        }
        
        private async UniTask InitTask()
        {
            // 创建资源包裹类
            YooAssets.Initialize();
            foreach (var packageName in Owner.packagesNameList)
            {
                var package = YooAssets.TryGetPackage(packageName);
                if (package == null)
                    package = YooAssets.CreatePackage(packageName);
                var operation = Owner.m_InitOperation.Init(package, packageName, Owner.m_RemoteServices,
                    Owner.m_DecryptionServices);
                await operation;
                if (operation.Status == EOperationStatus.Succeed)
                {
                    var packageInfo = new PackageInfo();
                    packageInfo.Package = package;
                    Owner.m_PackageInfoDic.Add(packageName, packageInfo);
                }
                else
                {
                    Owner.m_OnPatchFail?.Invoke(operation.Error);
                    return;
                }
            }
            ChangeState<YooAssetRequestPackageVersionState>(m_Fsm);
        }
    }
}