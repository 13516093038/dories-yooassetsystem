using System.Threading.Tasks;
using Dories.Fsm.Runtime;
using Dories.YooAssetSystem.Patch.Runtime.Operations;
using Dories.YooAssetSystem.Runtime.Patch.BuildInFsmSystem;
using UnityEngine;
using YooAsset;

namespace Dories.YooAssetSystem.Runtime.Patch.States
{
    public class YooAssetInitState : FsmNodeEntity<PatchEntity>
    {
        private Fsm<PatchEntity> m_Fsm;

        protected internal override void OnEnter()
        {
            _ = InitTask();
        }

        private async Task InitTask()
        {
            // 创建资源包裹类
            YooAssets.Initialize();
            foreach (var packageInfo in _owner.packagesInfoList)
            {
                var package = YooAssets.GetPackage(packageInfo.PackageName) ?? YooAssets.CreatePackage(packageInfo.PackageName);
                IYooAssetInitOperation initOperation = null;

                if (Application.isEditor && !_owner.isReleaseMode)
                {
                    initOperation = new EditorInitOperation();
                }
                else
                {
                    switch (_owner.playMode)
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
                    _owner._patchError?.Invoke($"未支持的 PlayMode: {_owner.playMode}");
                    _owner._patchFailed?.Invoke($"未支持的 PlayMode: {_owner.playMode}");
                    return;
                }

                var operation = initOperation.Initialize(
                    package,
                    packageInfo.RemoteService,
                    packageInfo.BundleDecryptor);
                await operation;

                if (operation.Status != EOperationStatus.Succeeded)
                {
                    _owner._patchFailed?.Invoke(operation.Error);
                    _owner._patchError?.Invoke(operation.Error);
                    return;
                }
            }
            ChangeState<YooAssetRequestPackageVersionState>();
        }
    }
}