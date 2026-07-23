using Dories.YooassetSystem.Runtime.AssetLoader;
using Dories.YooAssetSystem.Runtime.LogSystem;
using YooAsset;

namespace Dories.YooassetSystem.Runtime.AssetLoader.PackageResGroups
{
    public class PackageResGroup
    {
        protected ILog _logger;
        protected ResourcePackage _package;
#if UNITY_EDITOR
        protected internal PackageResLoadViewInfo _packageResLoadViewInfo;
#endif
        
        public PackageResGroup(ResourcePackage package, ILog logger)
        {
            _package = package;
            _logger = logger;

#if UNITY_EDITOR
            _packageResLoadViewInfo = new PackageResLoadViewInfo(package.PackageName);
#endif
        }

#if UNITY_EDITOR
        internal PackageResLoadViewInfo GetResLoadViewInfo() => _packageResLoadViewInfo;
#endif
    }
}