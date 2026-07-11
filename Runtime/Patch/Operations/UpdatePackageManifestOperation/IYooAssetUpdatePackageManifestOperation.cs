using YooAsset;

namespace Dories.YooAssetSystem.Runtime.Patch.Operations
{
    public interface IYooAssetUpdatePackageManifestOperation
    {
        LoadPackageManifestOperation UpdatePackageManifest(ResourcePackage package, string packageVersion, int timeout);
    }
}