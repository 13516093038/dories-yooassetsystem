using Dories.Fsm.Runtime;
using UnityEngine;

namespace Dories.YooassetSystem.Runtime.Patch.States
{
    public class YooAssetDownloadFileOverState : FsmState<PatchEntity>
    {
        protected override void OnEnter(Fsm<PatchEntity> fsm)
        {
            base.OnEnter(fsm);

            foreach (var packageInfo in Owner.packagesInfoList)
            {
                PlayerPrefs.SetString($"{packageInfo.PackageName}_GAME_VERSION",
                    packageInfo.PackageVersion);
                Debug.Log(
                    $"Update local package version success. [{packageInfo.PackageName}_GAME_VERSION]: <{packageInfo.PackageVersion}>");
            }

            ChangeState<YooAssetClearCacheBundleState>(fsm);
        }
    }
}