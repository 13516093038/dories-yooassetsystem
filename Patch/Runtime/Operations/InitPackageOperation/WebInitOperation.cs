using YooAsset;

namespace Dories.YooassetSystem.Patch.Runtime.Operations
{
    public class WebInitOperation : IYooAssetInitOperation
    {
        public InitializePackageOperation Initialize(ResourcePackage package, IRemoteService remoteServices,
            IBundleDecryptor bundleDecryptor = null, IManifestDecryptor manifestDecryptor = null)
        {
            var createParameters = new WebPlayModeOptions();
            WebPlayModeInitHelper.ApplyDefaultWebOptions(
                createParameters, remoteServices, bundleDecryptor, manifestDecryptor);

            return package.InitializePackageAsync(createParameters);
        }
    }
}