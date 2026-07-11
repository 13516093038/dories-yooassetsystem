using YooAsset;

namespace Dories.YooAssetSystem.Runtime.Patch.Operations
{
    public class DefaultClearCacheBundleOperation : IYooAssetClearCacheBundleOperation
    {
        public ClearCacheOperation YooAssetClearCacheOperation(ResourcePackage package, ClearCacheOptions options)
        {
            var operation = package.ClearCacheAsync(options);
            return operation;
        }
    }
}