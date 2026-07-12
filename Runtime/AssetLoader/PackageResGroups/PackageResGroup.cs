using Dories.YooAssetSystem.Runtime.LogSystem;
using YooAsset;

namespace Dories.YooassetSystem.Runtime.AssetLoader.PackageResGroups
{
    public class PackageResGroup
    {
        protected ILog _logger;
        protected ResourcePackage _package;

        public PackageResGroup(ResourcePackage package, ILog logger)
        {
            _package = package;
            _logger = logger;
        }
    }
}