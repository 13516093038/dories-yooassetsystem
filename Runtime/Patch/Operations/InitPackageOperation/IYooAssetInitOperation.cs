using YooAsset;

namespace Dories.YooassetSystem.Runtime.Patch.Operations.InitPackageOperation
{
    public interface IYooAssetInitOperation
    {
        InitializationOperation Init(ResourcePackage package, string packageName, IRemoteServices remoteServices,
            IDecryptionServices decryptionServices = null);
    }
}