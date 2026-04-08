using YooAsset;

namespace Dories.YooassetSystem.Runtime.Patch.Operations.InitPackageOperation
{
    public class HostPlayInitOperation : IYooAssetInitOperation
    {
        public InitializationOperation Init(ResourcePackage package, string packageName, IRemoteServices remoteServices,
            IDecryptionServices decryptionServices)
        {
            var createParameters = new HostPlayModeParameters();
            createParameters.BuildinFileSystemParameters =
                FileSystemParameters.CreateDefaultBuildinFileSystemParameters(decryptionServices);
            createParameters.CacheFileSystemParameters =
                FileSystemParameters.CreateDefaultCacheFileSystemParameters(remoteServices, decryptionServices);
            return package.InitializeAsync(createParameters);
        }
    }
}