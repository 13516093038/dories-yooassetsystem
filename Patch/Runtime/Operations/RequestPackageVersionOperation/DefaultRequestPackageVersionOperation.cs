using System.Threading.Tasks;
using UnityEngine;
using YooAsset;

namespace Dories.YooAssetSystem.Patch.Runtime.Operations
{
    public class DefaultRequestPackageVersionOperation : IYooAssetRequestPackageVersionOperation
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

            return string.Empty;
        }
    }
}