using System.Threading.Tasks;
using UnityEngine;
using YooAsset;

namespace Dories.YooAssetSystem.Runtime.Patch.Operations
{
    public class DefaultRequestPackageVersionOperation : IYooAssetRequestPackageVersionOperation
    {
        public async Task<string> RequestPackageVersion(ResourcePackage package)
        {
            var operation = package.RequestPackageVersionAsync();
            await operation;

            if (operation.Status != EOperationStatus.Succeeded)
            {
                Debug.LogError($"Failed to request package version: {operation.Error}");
                return string.Empty;
            }
            else
            {
                return operation.PackageVersion;
            }
        }
    }
}