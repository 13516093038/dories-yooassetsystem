using YooAsset;

namespace Dories.YooassetSystem.Patch.Runtime.Operations
{
    /// <summary>
    /// 热更新初始化操作
    /// </summary>
    public class HostPlayInitOperation : IYooAssetInitOperation
    {
        public InitializePackageOperation Initialize(ResourcePackage package, IRemoteService remoteServices,
            IBundleDecryptor bundleDecryptor = null, IManifestDecryptor manifestDecryptor = null)
        {
            var cacheFileSystemParams = FileSystemParameters.CreateDefaultSandboxFileSystemParameters(remoteServices);
            var builtinFileSystemParams = FileSystemParameters.CreateDefaultBuiltinFileSystemParameters();

            if (bundleDecryptor != null)
            {
                builtinFileSystemParams.AddParameter(EFileSystemParameter.AssetBundleDecryptor, bundleDecryptor);
            }
            if (manifestDecryptor != null)
            {
                builtinFileSystemParams.AddParameter(EFileSystemParameter.ManifestDecryptor, manifestDecryptor);
            }

            var createParameters = new HostPlayModeOptions();
            createParameters.BuiltinFileSystemParameters = builtinFileSystemParams;
            createParameters.CacheFileSystemParameters = cacheFileSystemParams;

            return package.InitializePackageAsync(createParameters);
        }
    }
}