using System.Threading.Tasks;
using YooAsset;

namespace Dories.YooassetSystem.Patch.Runtime.Operations
{
    public interface IYooAssetRequestPackageVersionOperation
    {
        Task<string> RequestPackageVersion(ResourcePackage package);
    }
}