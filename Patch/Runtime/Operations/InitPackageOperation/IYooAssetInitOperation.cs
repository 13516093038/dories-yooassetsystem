using YooAsset;

namespace Dories.YooAssetSystem.Patch.Runtime.Operations
{
    public interface IYooAssetInitOperation
    {
        InitializePackageOperation Initialize(ResourcePackage package, IRemoteService remoteServices,
            IBundleDecryptor bundleDecryptor = null);
    }
}