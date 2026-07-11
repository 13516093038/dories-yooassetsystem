using YooAsset;

namespace Dories.YooAssetSystem.Runtime.Patch.Operations
{
    /// <summary>
    /// 离线模式初始化操作
    /// </summary>
    public class OfflineInitOperation : IYooAssetInitOperation
    {
        public InitializePackageOperation Initialize(ResourcePackage package, IRemoteService remoteServices,
            IBundleDecryptor bundleDecryptor = null)
        {
            var builtinFileSystemParams = FileSystemParameters.CreateDefaultBuiltinFileSystemParameters();

            var createParameters = new OfflinePlayModeOptions();
            createParameters.BuiltinFileSystemParameters = builtinFileSystemParams;

            if (bundleDecryptor != null)
            {
                builtinFileSystemParams.AddParameter(EFileSystemParameter.AssetBundleDecryptor, bundleDecryptor);
            }

            return package.InitializePackageAsync(createParameters);
        }
    }
}