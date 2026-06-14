using YooAsset;

namespace Dories.YooassetSystem.Patch.Runtime.Operations
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