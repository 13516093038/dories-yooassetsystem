using System.Threading.Tasks;
using Dories.YooAssetSystem.Patch.Runtime.Operations;
using Dories.YooAssetSystem.Runtime.Patch.BuildInFsmSystem;
using YooAsset;

namespace Dories.YooAssetSystem.Runtime.Patch.States
{
    public class YooAssetClearCacheBundleState : FsmNodeEntity<PatchEntity>
    {
        protected internal override void OnEnter()
        {
            _ = ClearCacheBundle();
        }
        
        private async Task ClearCacheBundle()
        {
            foreach (var packageInfo in _owner.packagesInfoList)
            {
                ClearCacheOptions clearCacheOptions;

                if (packageInfo.ClearCacheBundleInfo.Mode is ClearCacheOperationMode.ClearBundleFilesByTags)
                {
                    clearCacheOptions =
                        new ClearCacheOptions(packageInfo.ClearCacheBundleInfo.Mode.ToClearCacheMethods(),
                            packageInfo.ClearCacheBundleInfo.Tags);
                }
                else if (packageInfo.ClearCacheBundleInfo.Mode is ClearCacheOperationMode.ClearBundleFilesByLocations)
                {
                    clearCacheOptions =
                        new ClearCacheOptions(packageInfo.ClearCacheBundleInfo.Mode.ToClearCacheMethods(),
                            packageInfo.ClearCacheBundleInfo.Locations);
                }
                else
                {
                    clearCacheOptions =
                        new ClearCacheOptions(packageInfo.ClearCacheBundleInfo.Mode.ToClearCacheMethods());
                }
                
                var operation = new DefaultClearCacheBundleOperation();

                var clearCacheOperation = operation.YooAssetClearCacheOperation(
                    YooAssets.GetPackage(packageInfo.PackageName),
                    clearCacheOptions);
                
                await clearCacheOperation;
                if(clearCacheOperation.Status == EOperationStatus.Succeeded)
                {
                    //清理成功
                }
                else
                {
                    _owner._patchError?.Invoke(clearCacheOperation.Error);
                }
                _owner._patchCompleted();
            }
        }
    }
}