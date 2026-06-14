using YooAsset;

namespace Dories.YooassetSystem.Patch.Runtime.Operations
{
    public interface IYooAssetClearCacheBundleOperation
    {
       ClearCacheOperation YooAssetClearCacheOperation(ResourcePackage package, ClearCacheOptions options);
    }
}