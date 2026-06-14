using YooAsset;

namespace Dories.YooAssetSystem.Patch.Runtime.Operations
{
    /// <summary>
    /// 热更新初始化操作
    /// </summary>
    public class HostPlayInitOperation : IYooAssetInitOperation
    {
        public InitializePackageOperation Initialize(ResourcePackage package, IRemoteService remoteServices,
            IBundleDecryptor bundleDecryptor = null)
        {
            var cacheFileSystemParams = FileSystemParameters.CreateDefaultSandboxFileSystemParameters(remoteServices);
            var builtinFileSystemParams = FileSystemParameters.CreateDefaultBuiltinFileSystemParameters();

            if (bundleDecryptor != null)
            {
                builtinFileSystemParams.AddParameter(EFileSystemParameter.AssetBundleDecryptor, bundleDecryptor);
            }

            var createParameters = new HostPlayModeOptions();
            createParameters.BuiltinFileSystemParameters = builtinFileSystemParams;
            createParameters.CacheFileSystemParameters = cacheFileSystemParams;

            return package.InitializePackageAsync(createParameters);
        }
    }
}