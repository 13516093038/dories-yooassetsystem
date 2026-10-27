using YooAsset;

namespace Dories.YooassetSystem.Runtime.AssetLoader.PackageResGroups
{
    public class PackageResGroup
    {
        protected ResourcePackage _package;
#if UNITY_EDITOR
        protected internal PackageResLoadViewInfo _packageResLoadViewInfo;
#endif
        
        public PackageResGroup(ResourcePackage package)
        {
            _package = package;

#if UNITY_EDITOR
            _packageResLoadViewInfo = new PackageResLoadViewInfo(package.PackageName);
#endif
        }

#if UNITY_EDITOR
        internal PackageResLoadViewInfo GetResLoadViewInfo() => _packageResLoadViewInfo;
#endif
    }
}