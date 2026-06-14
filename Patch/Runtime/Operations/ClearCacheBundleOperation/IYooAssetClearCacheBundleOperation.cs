using YooAsset;

namespace Dories.YooAssetSystem.Patch.Runtime.Operations
{
    public interface IYooAssetClearCacheBundleOperation
    {
       ClearCacheOperation YooAssetClearCacheOperation(ResourcePackage package, ClearCacheOptions options);
    }
}