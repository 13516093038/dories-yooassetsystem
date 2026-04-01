using YooAsset;

namespace Dories.YooassetSystem.Runtime.Patch.Operations.RequestPackageVersionOperation
{
    public interface IYooAssetRequestPackageVersionOperation
    {
        YooAsset.RequestPackageVersionOperation RequestPackageVersion(ResourcePackage package);
    }
}