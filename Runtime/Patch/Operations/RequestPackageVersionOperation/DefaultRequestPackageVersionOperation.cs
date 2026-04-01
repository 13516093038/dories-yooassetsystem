using YooAsset;

namespace Dories.YooassetSystem.Runtime.Patch.Operations.RequestPackageVersionOperation
{
    public class DefaultRequestPackageVersionOperation : IYooAssetRequestPackageVersionOperation
    {
        public YooAsset.RequestPackageVersionOperation RequestPackageVersion(ResourcePackage package)
        {
            var operation = package.RequestPackageVersionAsync();
            return operation;
        }
    }
}