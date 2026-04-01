using YooAsset;

namespace Dories.YooassetSystem.Runtime.Patch.Operations.InitPackageOperation
{
    /// <summary>
    /// 编辑器初始化操作
    /// </summary>
    public class EditorInitOperation : IYooAssetInitOperation
    {
        public InitializationOperation Init(ResourcePackage package, string packageName, IRemoteServices remoteServices)
        {
            var buildResult = EditorSimulateModeHelper.SimulateBuild(packageName);
            var packageRoot = buildResult.PackageRootDirectory;
            var createParameters = new EditorSimulateModeParameters();
            createParameters.EditorFileSystemParameters =
                FileSystemParameters.CreateDefaultEditorFileSystemParameters(packageRoot);
            return package.InitializeAsync(createParameters);
        }
    }
}