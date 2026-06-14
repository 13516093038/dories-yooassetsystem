using YooAsset;

namespace Dories.YooAssetSystem.Patch.Runtime.Operations
{
    public interface IYooAssetCreateDownloaderOperation
    {
        ResourceDownloaderOperation CreateDownloader(ResourcePackage package, int downloadingMaxNum, int failedTryAgain);
    }
}