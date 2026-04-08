using YooAsset;

namespace Dories.YooassetSystem.Runtime.Patch.Operations.InitPackageOperation
{
    public class OfflineInitOperation : IYooAssetInitOperation
    {
        public InitializationOperation Init(ResourcePackage package, string packageName, IRemoteServices remoteServices,
            IDecryptionServices decryptionServices = null)
        {
            var createParameters = new OfflinePlayModeParameters();
            createParameters.BuildinFileSystemParameters =
                FileSystemParameters.CreateDefaultBuildinFileSystemParameters(decryptionServices);
            return package.InitializeAsync(createParameters);
        }
    }
}