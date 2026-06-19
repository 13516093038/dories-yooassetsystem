#if DORIES_UNITASK_SUPPORT
using Cysharp.Threading.Tasks;
#else
using System.Threading.Tasks;
#endif
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

#if DORIES_UNITASK_SUPPORT
        private async UniTask ClearCacheBundle()
#else
        private async Task ClearCacheBundle()
#endif
        {
            foreach (var packageInfo in _owner.packagesInfoList)
            {
                ClearCacheOptions clearCacheOptions;
                var clearCacheInfo = packageInfo.ClearCacheBundleInfo;
                if (clearCacheInfo == null)
                {
                    clearCacheOptions = new ClearCacheOptions(ClearCacheOperationMode.ClearUnusedBundleFiles.ToClearCacheMethods());
                    await ExecuteClearCache(packageInfo.PackageName, clearCacheOptions);
                    continue;
                }

                if (clearCacheInfo.Mode is ClearCacheOperationMode.ClearBundleFilesByTags)
                {
                    clearCacheOptions =
                        new ClearCacheOptions(clearCacheInfo.Mode.ToClearCacheMethods(),
                            clearCacheInfo.Tags);
                }
                else if (clearCacheInfo.Mode is ClearCacheOperationMode.ClearBundleFilesByLocations)
                {
                    clearCacheOptions =
                        new ClearCacheOptions(clearCacheInfo.Mode.ToClearCacheMethods(),
                            clearCacheInfo.Locations);
                }
                else
                {
                    clearCacheOptions =
                        new ClearCacheOptions(clearCacheInfo.Mode.ToClearCacheMethods());
                }

                await ExecuteClearCache(packageInfo.PackageName, clearCacheOptions);
            }

            _owner._patchCompleted?.Invoke();
        }

#if DORIES_UNITASK_SUPPORT
        private async UniTask ExecuteClearCache(string packageName, ClearCacheOptions clearCacheOptions)
#else
        private async Task ExecuteClearCache(string packageName, ClearCacheOptions clearCacheOptions)
#endif
        {
            var operation = new DefaultClearCacheBundleOperation();
            var clearCacheOperation = operation.YooAssetClearCacheOperation(
                YooAssets.GetPackage(packageName),
                clearCacheOptions);

            await clearCacheOperation;
            if (clearCacheOperation.Status != EOperationStatus.Succeeded)
            {
                _owner._patchError?.Invoke(clearCacheOperation.Error);
            }
        }
    }
}