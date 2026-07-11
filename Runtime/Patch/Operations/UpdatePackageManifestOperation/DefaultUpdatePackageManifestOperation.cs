using YooAsset;

namespace Dories.YooAssetSystem.Runtime.Patch.Operations
{
    public class DefaultUpdatePackageManifestOperation : IYooAssetUpdatePackageManifestOperation
    {
        public LoadPackageManifestOperation UpdatePackageManifest(ResourcePackage package, string packageVersion, int timeout)
        {
            return package.LoadPackageManifestAsync(new LoadPackageManifestOptions(packageVersion, timeout));
        }
    }
}