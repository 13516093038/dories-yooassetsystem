using YooAsset;

namespace Dories.YooassetSystem.Patch.Runtime.Operations
{
    /// <summary>
    /// 离线模式初始化操作
    /// </summary>
    public class OfflineInitOperation : IYooAssetInitOperation
    {
        public InitializePackageOperation Initialize(ResourcePackage package, IRemoteService remoteServices,
            IBundleDecryptor bundleDecryptor = null, IManifestDecryptor manifestDecryptor = null)
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