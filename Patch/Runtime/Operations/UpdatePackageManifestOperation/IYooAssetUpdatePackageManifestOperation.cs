using YooAsset;

namespace Dories.YooassetSystem.Patch.Runtime.Operations
{
    public interface IYooAssetUpdatePackageManifestOperation
    {
        LoadPackageManifestOperation UpdatePackageManifest(ResourcePackage package, string packageVersion, int timeout);
    }
}