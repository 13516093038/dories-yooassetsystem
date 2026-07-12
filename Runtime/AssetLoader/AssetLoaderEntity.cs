using System;
using System.Collections.Generic;
using Dories.YooassetSystem.Runtime.AssetLoader.PackageResGroups;
#if DORIES_UNITASK_SUPPORT
using Cysharp.Threading.Tasks;
#else
using System.Threading.Tasks;
#endif
using Dories.YooAssetSystem.Runtime.LogSystem;
using UnityEngine;
using YooAsset;
using Object = UnityEngine.Object;

namespace Dories.YooassetSystem.Runtime.AssetLoader
{
    public class AssetLoaderEntity : MonoBehaviour
    {
        [SerializeField] private string ILog;

        private ResourcePackage _defaultPackage;
        private ILog _logger;
        private Dictionary<string, PackageAssetGroup> _packageAssetGroupDic = new();
        
        private Dictionary<string, ResourcePackage> _packageDic = new();

        private void Awake()
        {
            _logger = CreateLog(ILog);
        }

        private ILog CreateLog(string log)
        {
            if (!string.IsNullOrEmpty(log))
            {
                try
                {
                    var type = ResolveType(log);
                    if (type != null)
                        return Activator.CreateInstance(type) as ILog;
                }
                catch (Exception e)
                {
                    Debug.LogError($"Create logger failed: {e}, Type name: {log}");
                }
            }

            return new BuildInLogEntity();
        }

        private static Type ResolveType(string typeName)
        {
            if (string.IsNullOrEmpty(typeName))
                return null;

            var type = Type.GetType(typeName);
            if (type != null)
                return type;

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                type = assembly.GetType(typeName);
                if (type != null)
                    return type;
            }

            return null;
        }

        private ResourcePackage GetPackage(string packageName)
        {
            if (string.IsNullOrEmpty(packageName))
            {
                _logger.Error("[AssetLoader] Package name is null or empty");
                return null;
            }

            if(_packageDic.TryGetValue(packageName, out var package))
            {
                return package;
            }

            if (YooAssets.TryGetPackage(packageName, out package))
            {
                _packageDic[packageName] = package;
                return package;
            }
            else
            {
                _logger.Error($"[AssetLoader] Package: {packageName} not found, cannot get package");
                return null;
            }
        }

        public void SetDefaultPackage(string packageName)
        {
            _defaultPackage = GetPackage(packageName);
            if (_defaultPackage == null)
            {
                _logger.Error($"[AssetLoader] Package: {packageName} not found, cannot set as default package");
            }
        }

        #region Load Assets

#if DORIES_UNITASK_SUPPORT
        public async UniTask<T> LoadAssetAsync<T>(string assetName, uint priority = 0) where T : Object
#else
        public async Task<T> LoadAssetAsync<T>(string assetName, uint priority = 0) where T : Object
#endif
        {
            if (_defaultPackage == null)
            {
                _logger.Error("[AssetLoader] Default package is not set, cannot load asset");
                return null;
            }

            if (string.IsNullOrEmpty(assetName))
            {
                _logger.Error("[AssetLoader] Asset name is null or empty, cannot load asset");
                return null;
            }

            if (!_packageAssetGroupDic.TryGetValue(_defaultPackage.PackageName, out var packageAssetGroup))
            {
                packageAssetGroup = new PackageAssetGroup(_defaultPackage, _logger);
                _packageAssetGroupDic[_defaultPackage.PackageName] = packageAssetGroup;
            }
            return await packageAssetGroup.LoadAssetAsync<T>(assetName, priority);
        }


#if DORIES_UNITASK_SUPPORT
        public async UniTask<T> LoadAssetAsync<T>(string packageName, string assetName, uint priority = 0) where T : Object
#else
        public async Task<T> LoadAssetAsync<T>(string packageName, string assetName, uint priority = 0) where T : Object
#endif
        {
            if (string.IsNullOrEmpty(packageName) || string.IsNullOrEmpty(assetName))
            {
                _logger.Error($"[AssetLoader] Package name or asset name is null or empty");
                return null;
            }

            if (!_packageAssetGroupDic.TryGetValue(packageName, out var packageAssetGroup))
            {
                var package = GetPackage(packageName);
                if (package == null)
                {
                    _logger.Error($"[AssetLoader] Package: {packageName} not found, cannot load asset: {assetName}");
                    return null;
                }
                else
                {
                    packageAssetGroup = new PackageAssetGroup(package, _logger);
                    _packageAssetGroupDic[packageName] = packageAssetGroup;
                }
            }

            return await packageAssetGroup.LoadAssetAsync<T>(assetName, priority);
        }

        #endregion

        #region  UnloadAssets

        public void UnloadAsset(string assetName, bool forceUnload = false)
        {
            if (_defaultPackage == null)
            {
                _logger.Error("[AssetLoader] Default package is not set, cannot load asset");
                return;
            }

            if (string.IsNullOrEmpty(assetName))
            {
                return;
            }

            if (_packageAssetGroupDic.TryGetValue(_defaultPackage.PackageName, out var packageAssetGroup))
            {
                packageAssetGroup.UnloadAsset(assetName, forceUnload);
            }
            else
            {
                _logger.Error($"[AssetLoader] Asset: {assetName} not found, cannot unload");
            }
        }

        public void UnloadAsset(string packageName, string assetName, bool forceUnload = false)
        {
            if (string.IsNullOrEmpty(packageName) || string.IsNullOrEmpty(assetName))
            {
                _logger.Error($"[AssetLoader] Package name or asset name is null or empty");
                return;
            }

            if (!_packageAssetGroupDic.TryGetValue(packageName, out var packageAssetGroup))
            {
                _logger.Error($"[AssetLoader] Asset: {assetName} not found, cannot unload");
                return;
            }

            packageAssetGroup.UnloadAsset(assetName, forceUnload);
        }

        #endregion
    }
}