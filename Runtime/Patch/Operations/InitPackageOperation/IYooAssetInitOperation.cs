using YooAsset;

namespace Dories.YooAssetSystem.Runtime.Patch.Operations
{
    public interface IYooAssetInitOperation
    {
        InitializePackageOperation Initialize(ResourcePackage package, IRemoteService remoteServices,
            IBundleDecryptor bundleDecryptor = null,
            IManifestDecryptor manifestDecryptor = null,
            EditorVirtualType editorVirtualType = EditorVirtualType.VirttualAssetBundle);
    }
}