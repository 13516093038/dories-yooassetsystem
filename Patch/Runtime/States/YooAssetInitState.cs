using System.Threading.Tasks;
using Dories.Fsm.Runtime;
using Dories.YooassetSystem.Patch.Runtime.Operations;
using UnityEngine;
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
            _ = InitTask();
        }

        private async Task InitTask()
        {
            // 创建资源包裹类
            YooAssets.Initialize();
            foreach (var packageInfo in Owner.packagesInfoList)
            {
                var package = YooAssets.GetPackage(packageInfo.PackageName) ?? YooAssets.CreatePackage(packageInfo.PackageName);
                IYooAssetInitOperation initOperation = null;

                if (Application.isEditor && !Owner.isReleaseMode)
                {
                    initOperation = new EditorInitOperation();
                }
                else
                {
                    switch (Owner.playMode)
                    {
                        case PlayMode.OfflinePlayMode:
                            initOperation = new OfflineInitOperation();
                            break;
                        case PlayMode.HostPlayMode:
                            initOperation = new HostPlayInitOperation();
                            break;
                        case PlayMode.WebPlayMode:
                            initOperation = new WebInitOperation();
                            break;
                        case PlayMode.WeChatMiniGameMode:
                            initOperation = new WeChatMiniGameInitOperation();
                            break;
                        case PlayMode.TTMiniGameMode:
                            initOperation = new TTMiniGameInitOperation();
                            break;
                    }
                }

                if (initOperation == null)
                {
                    Owner._patchError?.Invoke($"未支持的 PlayMode: {Owner.playMode}");
                    Owner._patchFailed?.Invoke($"未支持的 PlayMode: {Owner.playMode}");
                    return;
                }

                var operation = initOperation.Initialize(
                    package,
                    packageInfo.RemoteService,
                    packageInfo.BundleDecryptor,
                    Owner.ManifestDecryptor);
                await operation;

                if (operation.Status != EOperationStatus.Succeeded)
                {
                    Owner._patchFailed?.Invoke(operation.Error);
                    Owner._patchError?.Invoke(operation.Error);
                    return;
                }
            }
            ChangeState<YooAssetRequestPackageVersionState>(m_Fsm);
        }
    }
}