using System;
using YooAsset;

namespace Dories.YooassetSystem.Runtime.Patch.Operations.CreateDownloaderOperation
{
    public interface IYooAssetCreateDownloaderOperation
    {
        ResourceDownloaderOperation CreateDownloader(ResourcePackage package);

        Action<int ,Action> GetOnNeedUpdateCallback();
    }
}