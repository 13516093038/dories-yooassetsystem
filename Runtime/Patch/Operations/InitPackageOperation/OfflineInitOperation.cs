using YooAsset;

namespace Dories.YooAssetSystem.Runtime.Patch.Operations
{
    /// <summary>
    /// 离线模式初始化操作
    /// </summary>
    public class OfflineInitOperation : IYooAssetInitOperation
    {
        public InitializePackageOperation Initialize(ResourcePackage package, IRemoteService remoteServices,
            IBundleDecryptor bundleDecryptor = null,
            IManifestDecryptor manifestDecryptor = null,
            EditorVirtualType editorVirtualType = EditorVirtualType.VirttualAssetBundle)
        {
            var builtinFileSystemParams = FileSystemParameters.CreateDefaultBuiltinFileSystemParameters();

            var createParameters = new OfflinePlayModeOptions();
            createParameters.BuiltinFileSystemParameters = builtinFileSystemParams;

            if (bundleDecryptor != null)
            {
                builtinFileSystemParams.AddParameter(EFileSystemParameter.AssetBundleDecryptor, bundleDecryptor);
            }

            if (manifestDecryptor != null)
            {
                builtinFileSystemParams.AddParameter(EFileSystemParameter.ManifestDecryptor, manifestDecryptor);
            }

            return package.InitializePackageAsync(createParameters);
        }
    }
}