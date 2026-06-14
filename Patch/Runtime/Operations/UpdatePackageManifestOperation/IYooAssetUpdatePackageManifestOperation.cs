using YooAsset;

namespace Dories.YooAssetSystem.Patch.Runtime.Operations
{
    public interface IYooAssetUpdatePackageManifestOperation
    {
        LoadPackageManifestOperation UpdatePackageManifest(ResourcePackage package, string packageVersion, int timeout);
    }
}