using YooAsset;

namespace Dories.YooAssetSystem.Runtime.Patch.Operations
{
    /// <summary>
    /// 编辑器初始化操作
    /// </summary>
    public class EditorInitOperation : IYooAssetInitOperation
    {
        public InitializePackageOperation Initialize(ResourcePackage package, IRemoteService remoteServices,
            IBundleDecryptor bundleDecryptor = null,
            EditorVirtualType editorVirtualType = EditorVirtualType.VirttualAssetBundle)
        {
            var buildResult =
                EditorSimulateBuildInvoker.Build(package.PackageName, (int)editorVirtualType);
            var packageRoot = buildResult.PackageRootDirectory;
            var createParameters = new EditorSimulateModeOptions();
            createParameters.EditorFileSystemParameters =
                FileSystemParameters.CreateDefaultEditorFileSystemParameters(packageRoot);
            return package.InitializePackageAsync(createParameters);
        }
    }
}