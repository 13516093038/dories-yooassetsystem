using System.Collections.Generic;
using Dories.YooAssetSystem.Runtime.LogSystem;
using YooAsset;

namespace Dories.YooassetSystem.Runtime.AssetLoader.PackageResGroups
{
    public class PackageSceneGroup : PackageAssetGroup
    {
        private LoadingTasker _loadingTasker;
        private Dictionary<string, HandleBase> _cacheDic;
        
        public PackageSceneGroup(ResourcePackage package, ILog logger) : base(package, logger)
        {
            _cacheDic = new Dictionary<string, HandleBase>();
            _loadingTasker = new LoadingTasker(logger);
        }
    }

   
}
