using YooAsset;

namespace Dories.YooAssetSystem.Patch.Runtime.Operations
{
    public class WebInitOperation : IYooAssetInitOperation
    {
        public InitializePackageOperation Initialize(ResourcePackage package, IRemoteService remoteServices,
            IBundleDecryptor bundleDecryptor = null)
        {
            var createParameters = new WebPlayModeOptions();
            WebPlayModeInitHelper.ApplyDefaultWebOptions(
                createParameters, remoteServices, bundleDecryptor);

            return package.InitializePackageAsync(createParameters);
        }
    }
}