using YooAsset;

namespace Dories.YooassetSystem.Patch.Runtime.Operations
{
    public class DefaultCreateDownloaderOperation : IYooAssetCreateDownloaderOperation
    {
        public ResourceDownloaderOperation CreateDownloader(ResourcePackage package, int downloadingMaxNum, int failedTryAgain)
        {
           var downloader = package.CreateResourceDownloader(new ResourceDownloaderOptions(downloadingMaxNum, failedTryAgain));
           return downloader;
        }
    }
}