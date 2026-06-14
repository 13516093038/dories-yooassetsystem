using YooAsset;

namespace Dories.YooAssetSystem.Patch.Runtime.Operations
{
    /// <summary>
    /// 编辑器初始化操作
    /// </summary>
    public class EditorInitOperation : IYooAssetInitOperation
    {
        public InitializePackageOperation Initialize(ResourcePackage package, IRemoteService remoteServices,
            IBundleDecryptor bundleDecryptor = null)
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