using YooAsset;

namespace Dories.YooassetSystem.Patch.Runtime.Operations
{
    public interface IYooAssetInitOperation
    {
        InitializePackageOperation Initialize(ResourcePackage package, IRemoteService remoteServices,
            IBundleDecryptor bundleDecryptor = null, IManifestDecryptor manifestDecryptor = null);
    }
}