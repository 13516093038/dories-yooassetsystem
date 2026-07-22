using YooAsset;

namespace Dories.YooAssetSystem.Runtime.Patch.Operations
{
    /// <summary>
    /// 热更新初始化操作
    /// </summary>
    public class HostPlayInitOperation : IYooAssetInitOperation
    {
        public InitializePackageOperation Initialize(ResourcePackage package, IRemoteService remoteServices,
            IBundleDecryptor bundleDecryptor = null,
            EditorVirtualType editorVirtualType = EditorVirtualType.VirttualAssetBundle)
        {
            var cacheFileSystemParams = FileSystemParameters.CreateDefaultSandboxFileSystemParameters(remoteServices);
            cacheFileSystemParams.AddParameter(EFileSystemParameter.InstallCleanupMode, EInstallCleanupMode.None);

            var builtinFileSystemParams = FileSystemParameters.CreateDefaultBuiltinFileSystemParameters();
            builtinFileSystemParams.AddParameter(EFileSystemParameter.CopyBuiltinPackageManifest, true);

            if (bundleDecryptor != null)
            {
                builtinFileSystemParams.AddParameter(EFileSystemParameter.AssetBundleDecryptor, bundleDecryptor);
                cacheFileSystemParams.AddParameter(EFileSystemParameter.AssetBundleDecryptor, bundleDecryptor);
            }

            var createParameters = new HostPlayModeOptions();

            createParameters.BuiltinFileSystemParameters = builtinFileSystemParams;
            createParameters.CacheFileSystemParameters = cacheFileSystemParams;

            return package.InitializePackageAsync(createParameters);
        }
    }
}