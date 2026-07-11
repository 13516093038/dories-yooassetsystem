using System.Threading.Tasks;
using YooAsset;

namespace Dories.YooAssetSystem.Runtime.Patch.Operations
{
    public interface IYooAssetRequestPackageVersionOperation
    {
        Task<string> RequestPackageVersion(ResourcePackage package);
    }
}