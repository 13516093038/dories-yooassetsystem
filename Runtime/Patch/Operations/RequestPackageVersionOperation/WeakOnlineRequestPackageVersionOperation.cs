using Dories.YooassetSystem.Runtime.Patch.YooAssetExtensions.OperationExtensions;
using YooAsset;

namespace Dories.YooassetSystem.Runtime.Patch.Operations.RequestPackageVersionOperation
{
    public class WeakOnlineRequestPackageVersionOperation : IYooAssetRequestPackageVersionOperation
    {
        public YooAsset.RequestPackageVersionOperation RequestPackageVersion(ResourcePackage package)
        {
            var operation = new WeakOnlineRequestPackageVersionHelper(package);
            operation.StartOperation();
            return operation;
        }
    }
}


