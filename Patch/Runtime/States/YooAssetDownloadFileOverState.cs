using Dories.YooAssetSystem.Runtime.Patch.BuildInFsmSystem;
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
                _logger.Info(
                    $"Update local package version success. [{packageInfo.PackageName}_GAME_VERSION]: <{packageInfo.PackageVersion}>");
            }

            ChangeState<YooAssetClearCacheBundleState>();
        }
    }
}