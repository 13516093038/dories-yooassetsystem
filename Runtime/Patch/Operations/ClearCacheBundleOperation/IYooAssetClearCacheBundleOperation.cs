using YooAsset;

namespace Dories.YooAssetSystem.Runtime.Patch.Operations
{
    public interface IYooAssetClearCacheBundleOperation
    {
       ClearCacheOperation YooAssetClearCacheOperation(ResourcePackage package, ClearCacheOptions options);
    }
}