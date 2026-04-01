using YooAsset;

namespace Dories.YooassetSystem.Runtime.Patch.Operations.ClearCacheBundleOperation
{
    public class DefaultClearCacheBundleOperation : IYooAssetClearCacheBundleOperation
    {
        public ClearCacheFilesOperation ClearCacheBundle(ResourcePackage package)
        {
            var operation = package.ClearCacheFilesAsync(EFileClearMode.ClearUnusedBundleFiles);
            return operation;
        }
    }
}