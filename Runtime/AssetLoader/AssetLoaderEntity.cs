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
using UnityEngine.SceneManagement;
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
        private Dictionary<string, PackageSceneGroup> _packageSceneGroupDic = new();
        private Dictionary<string, PackageRawFileGroup> _packageRawFileGroupDic = new();

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

        #region Load Scenes

        private PackageSceneGroup GetOrCreateSceneGroup(string packageName)
        {
            if (_packageSceneGroupDic.TryGetValue(packageName, out var group))
                return group;

            var package = GetPackage(packageName);
            if (package == null)
                return null;

            group = new PackageSceneGroup(package, _logger);
            _packageSceneGroupDic[packageName] = group;
            return group;
        }

#if DORIES_UNITASK_SUPPORT
        public async UniTask<SceneHandle> LoadSceneAsync(
#else
        public async Task<SceneHandle> LoadSceneAsync(
#endif
            string sceneLocation,
            LoadSceneMode sceneMode = LoadSceneMode.Single,
            LocalPhysicsMode physicsMode = LocalPhysicsMode.None,
            bool allowSceneActivation = true,
            uint priority = 0)
        {
            if (_defaultPackage == null)
            {
                _logger.Error("[AssetLoader] Default package is not set, cannot load scene");
                return null;
            }

            if (string.IsNullOrEmpty(sceneLocation))
            {
                _logger.Error("[AssetLoader] Scene location is null or empty, cannot load scene");
                return null;
            }

            var group = GetOrCreateSceneGroup(_defaultPackage.PackageName);
            if (group == null)
                return null;

            return await group.LoadSceneAsync(sceneLocation, sceneMode, physicsMode, allowSceneActivation, priority);
        }

#if DORIES_UNITASK_SUPPORT
        public async UniTask<SceneHandle> LoadSceneAsync(
#else
        public async Task<SceneHandle> LoadSceneAsync(
#endif
            string packageName,
            string sceneLocation,
            LoadSceneMode sceneMode = LoadSceneMode.Single,
            LocalPhysicsMode physicsMode = LocalPhysicsMode.None,
            bool allowSceneActivation = true,
            uint priority = 0)
        {
            if (string.IsNullOrEmpty(packageName) || string.IsNullOrEmpty(sceneLocation))
            {
                _logger.Error("[AssetLoader] Package name or scene location is null or empty");
                return null;
            }

            var group = GetOrCreateSceneGroup(packageName);
            if (group == null)
            {
                _logger.Error($"[AssetLoader] Package: {packageName} not found, cannot load scene: {sceneLocation}");
                return null;
            }

            return await group.LoadSceneAsync(sceneLocation, sceneMode, physicsMode, allowSceneActivation, priority);
        }

        public void UnloadScene(string sceneLocation)
        {
            if (_defaultPackage == null)
            {
                _logger.Error("[AssetLoader] Default package is not set, cannot unload scene");
                return;
            }

            if (string.IsNullOrEmpty(sceneLocation))
                return;

            if (_packageSceneGroupDic.TryGetValue(_defaultPackage.PackageName, out var group))
            {
#if DORIES_UNITASK_SUPPORT
                group.UnloadSceneAsync(sceneLocation).Forget();
#else
                _ = group.UnloadSceneAsync(sceneLocation);
#endif
            }
        }

        public void UnloadScene(string packageName, string sceneLocation)
        {
            if (string.IsNullOrEmpty(packageName) || string.IsNullOrEmpty(sceneLocation))
            {
                _logger.Error("[AssetLoader] Package name or scene location is null or empty");
                return;
            }

            if (_packageSceneGroupDic.TryGetValue(packageName, out var group))
            {
#if DORIES_UNITASK_SUPPORT
                group.UnloadSceneAsync(sceneLocation).Forget();
#else
                _ = group.UnloadSceneAsync(sceneLocation);
#endif
            }
        }

        #endregion

        #region Load RawFiles

        private PackageRawFileGroup GetOrCreateRawFileGroup(string packageName)
        {
            if (_packageRawFileGroupDic.TryGetValue(packageName, out var group))
                return group;

            var package = GetPackage(packageName);
            if (package == null)
                return null;

            group = new PackageRawFileGroup(package, _logger);
            _packageRawFileGroupDic[packageName] = group;
            return group;
        }

#if DORIES_UNITASK_SUPPORT
        public async UniTask<RawFileObject> LoadRawFileAsync(string location, uint priority = 0)
#else
        public async Task<RawFileObject> LoadRawFileAsync(string location, uint priority = 0)
#endif
        {
            if (_defaultPackage == null)
            {
                _logger.Error("[AssetLoader] Default package is not set, cannot load raw file");
                return null;
            }

            if (string.IsNullOrEmpty(location))
            {
                _logger.Error("[AssetLoader] RawFile location is null or empty");
                return null;
            }

            var group = GetOrCreateRawFileGroup(_defaultPackage.PackageName);
            if (group == null)
                return null;

            return await group.LoadRawFileAsync(location, priority);
        }

#if DORIES_UNITASK_SUPPORT
        public async UniTask<RawFileObject> LoadRawFileAsync(string packageName, string location, uint priority = 0)
#else
        public async Task<RawFileObject> LoadRawFileAsync(string packageName, string location, uint priority = 0)
#endif
        {
            if (string.IsNullOrEmpty(packageName) || string.IsNullOrEmpty(location))
            {
                _logger.Error("[AssetLoader] Package name or raw file location is null or empty");
                return null;
            }

            var group = GetOrCreateRawFileGroup(packageName);
            if (group == null)
            {
                _logger.Error($"[AssetLoader] Package: {packageName} not found, cannot load raw file: {location}");
                return null;
            }

            return await group.LoadRawFileAsync(location, priority);
        }

#if DORIES_UNITASK_SUPPORT
        public async UniTask<byte[]> LoadRawFileBytesAsync(string location, uint priority = 0)
#else
        public async Task<byte[]> LoadRawFileBytesAsync(string location, uint priority = 0)
#endif
        {
            if (_defaultPackage == null)
            {
                _logger.Error("[AssetLoader] Default package is not set, cannot load raw file");
                return null;
            }

            var group = GetOrCreateRawFileGroup(_defaultPackage.PackageName);
            if (group == null)
                return null;

            return await group.LoadRawFileBytesAsync(location, priority);
        }

#if DORIES_UNITASK_SUPPORT
        public async UniTask<byte[]> LoadRawFileBytesAsync(string packageName, string location, uint priority = 0)
#else
        public async Task<byte[]> LoadRawFileBytesAsync(string packageName, string location, uint priority = 0)
#endif
        {
            var group = GetOrCreateRawFileGroup(packageName);
            if (group == null)
                return null;

            return await group.LoadRawFileBytesAsync(location, priority);
        }

#if DORIES_UNITASK_SUPPORT
        public async UniTask<string> LoadRawFileTextAsync(string location, uint priority = 0)
#else
        public async Task<string> LoadRawFileTextAsync(string location, uint priority = 0)
#endif
        {
            if (_defaultPackage == null)
            {
                _logger.Error("[AssetLoader] Default package is not set, cannot load raw file");
                return null;
            }

            var group = GetOrCreateRawFileGroup(_defaultPackage.PackageName);
            if (group == null)
                return null;

            return await group.LoadRawFileTextAsync(location, priority);
        }

#if DORIES_UNITASK_SUPPORT
        public async UniTask<string> LoadRawFileTextAsync(string packageName, string location, uint priority = 0)
#else
        public async Task<string> LoadRawFileTextAsync(string packageName, string location, uint priority = 0)
#endif
        {
            var group = GetOrCreateRawFileGroup(packageName);
            if (group == null)
                return null;

            return await group.LoadRawFileTextAsync(location, priority);
        }

        public void UnloadRawFile(string location, bool forceUnload = false)
        {
            if (_defaultPackage == null)
            {
                _logger.Error("[AssetLoader] Default package is not set, cannot unload raw file");
                return;
            }

            if (string.IsNullOrEmpty(location))
                return;

            if (_packageRawFileGroupDic.TryGetValue(_defaultPackage.PackageName, out var group))
                group.UnloadRawFile(location, forceUnload);
        }

        public void UnloadRawFile(string packageName, string location, bool forceUnload = false)
        {
            if (string.IsNullOrEmpty(packageName) || string.IsNullOrEmpty(location))
            {
                _logger.Error("[AssetLoader] Package name or raw file location is null or empty");
                return;
            }

            if (_packageRawFileGroupDic.TryGetValue(packageName, out var group))
                group.UnloadRawFile(location, forceUnload);
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