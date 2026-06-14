using System.Threading.Tasks;
using UnityEngine;
using YooAsset;

namespace Dories.YooassetSystem.Patch.Runtime.Operations
{
    public class WeakOnlineRequestPackageVersionOperation : IYooAssetRequestPackageVersionOperation
    {
        public async Task<string> RequestPackageVersion(ResourcePackage package)
        {
            var operation = package.RequestPackageVersionAsync();
            await operation;

            if (operation.Status == EOperationStatus.Succeeded)
            {
                PlayerPrefs.SetString(package.PackageName + "_GAME_VERSION", operation.PackageVersion);
                return operation.PackageVersion;
            }
            else
            {
                return PlayerPrefs.GetString(package.PackageName + "_GAME_VERSION", string.Empty);
            }
        }
    }
}