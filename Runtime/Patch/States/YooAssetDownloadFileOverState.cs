using Dories.Fsm.Runtime;
using UnityEngine;

namespace Dories.YooassetSystem.Runtime.Patch.States
{
    public class YooAssetDownloadFileOverState : FsmState<PatchEntity>
    {
        protected override void OnEnter(Fsm<PatchEntity> fsm)
        {
            base.OnEnter(fsm);
            
            Owner.m_DownloadFileOverOperation.DownloadFileOver()?.Invoke();
            foreach (var packageInfo in Owner.m_PackageInfoDic)
            {
                PlayerPrefs.SetString($"{packageInfo.Value.Package.PackageName}_GAME_VERSION",
                    packageInfo.Value.PackageVersion);
                Debug.Log(
                    $"Update local package version success. [{packageInfo.Value.Package.PackageName}_GAME_VERSION]: <{packageInfo.Value.PackageVersion}>");
            }
            ChangeState<YooAssetClearCacheBundleState>(fsm);
        }
    }
}