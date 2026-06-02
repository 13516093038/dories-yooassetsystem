using YooAsset;

namespace Dories.YooassetSystem.Patch.Runtime.Operations
{
    /// <summary>
    /// 编辑器初始化操作
    /// </summary>
    public class EditorInitOperation : IYooAssetInitOperation
    {
        public InitializePackageOperation Initialize(ResourcePackage package, IRemoteService remoteServices,
            IBundleDecryptor bundleDecryptor = null, IManifestDecryptor manifestDecryptor = null)
        {
            var buildResult =
                EditorSimulateBuildInvoker.Build(package.PackageName, (int)EBundleType.VirtualAssetBundle);
            var packageRoot = buildResult.PackageRootDirectory;
            var createParameters = new EditorSimulateModeOptions();
            createParameters.EditorFileSystemParameters =
                FileSystemParameters.CreateDefaultEditorFileSystemParameters(packageRoot);
            return package.InitializePackageAsync(createParameters);
        }
    }
}