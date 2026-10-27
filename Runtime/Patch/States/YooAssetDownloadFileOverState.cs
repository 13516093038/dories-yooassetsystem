using Dories.YooAssetSystem.Runtime.Patch.BuildInFsmSystem;
using Dories.Logging;
using UnityEngine;

namespace Dories.YooAssetSystem.Runtime.Patch.States
{
    public class YooAssetDownloadFileOverState : FsmNodeEntity<PatchEntity>
    {
        protected internal override void OnEnter()
        {
            foreach (var packageInfo in _owner.packagesInfoList)
            {
                PlayerPrefs.SetString($"{packageInfo.PackageName}_GAME_VERSION",
                          packageInfo.PackageVersion);
                Log.Info("Patch",
                    $"Update local package version success. [{packageInfo.PackageName}_GAME_VERSION]: <{packageInfo.PackageVersion}>");
            }
            ChangeState<YooAssetClearCacheBundleState>();
        }
    }
}