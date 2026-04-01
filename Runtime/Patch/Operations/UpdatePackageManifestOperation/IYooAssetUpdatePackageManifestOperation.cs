using YooAsset;

namespace Dories.YooassetSystem.Runtime.Patch.Operations.UpdatePackageManifestOperation
{
    public interface IYooAssetUpdatePackageManifestOperation
    {
        YooAsset.UpdatePackageManifestOperation UpdatePackageManifest(ResourcePackage package, string packageVersion);
    }
}