using YooAsset;

namespace Dories.YooassetSystem.Patch.Runtime.Operations
{
    public interface IYooAssetCreateDownloaderOperation
    {
        ResourceDownloaderOperation CreateDownloader(ResourcePackage package, int downloadingMaxNum, int failedTryAgain);
    }
}