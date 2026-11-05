using YooAsset;

namespace Dories.YooAssetSystem.Runtime.Patch.Operations
{
    public class WebInitOperation : IYooAssetInitOperation
    {
        public InitializePackageOperation Initialize(ResourcePackage package, IRemoteService remoteServices,
            IBundleDecryptor bundleDecryptor = null,
            IManifestDecryptor manifestDecryptor = null,
            EditorVirtualType editorVirtualType = EditorVirtualType.VirttualAssetBundle)
        {
            var createParameters = new WebPlayModeOptions();
            WebPlayModeInitHelper.ApplyDefaultWebOptions(
                createParameters, remoteServices, bundleDecryptor, manifestDecryptor);

            return package.InitializePackageAsync(createParameters);
        }
    }
}