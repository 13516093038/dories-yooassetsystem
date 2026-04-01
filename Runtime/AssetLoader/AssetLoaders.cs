using Cysharp.Threading.Tasks;
using System;
using Dories.YooassetSystem.Runtime.AssetLoader.Cacher;
using Dories.Componentization.Runtime;
using Dories.Componentization.Runtime.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;
using YooAsset;
using Dories.YooassetSystem.Runtime.AssetLoader.GroupCacher;

namespace Dories.YooassetSystem.Runtime.AssetLoader
{
    public enum LoadType
    {
        Any,
        Resources,
        Bundle
    }

    public class AssetLoaders : Entity, ISingleton
    {
        private string m_CurrentPackageName;
        private ResourcePackage m_CurrentPackage;
        
        private CacheBundle m_CacheBundle = ComponentFactory.GetOrAddSingletonComponent<CacheBundle>();
        private CacheResource  m_CacheResource = ComponentFactory.GetOrAddSingletonComponent<CacheResource>();
        private GroupBundle m_GroupBundle = ComponentFactory.Acquire<GroupBundle>();
        private GroupResource m_GroupResource = ComponentFactory.Acquire<GroupResource>();
        
        private bool m_IsReleased;
        
        internal const byte MAX_RETRY_COUNT = 3;

        public void SetDefaultPackage(string packageName)
        {
            ResourcePackage package = GetPackage(packageName);
            if (package != null)
            {
                YooAssets.SetDefaultPackage(package);
                m_CurrentPackageName = package.PackageName;
                m_CurrentPackage = package;
            }
        }

        private ResourcePackage GetPackage(string packageName)
        {
            if (string.IsNullOrEmpty(packageName))
            {
                Debug.LogError("Package name is null or empty.");
                return null;
            }
            return YooAssets.TryGetPackage(packageName);
        }
        
        public async UniTask Release()
        {
            if (!m_IsReleased)
            {
                m_IsReleased = true;

                // 遍歷卸載
                var packages = YooAssets.GetAllPackages();
                foreach (var package in packages)
                {
                    await package.DestroyAsync();
                    YooAssets.RemovePackage(package);
                }

                // 強制銷毀
                YooAssets.Destroy();
            }
        }

        #region Scene
        /// <summary>
        /// Only load scene from bundle
        /// </summary>
        /// <param name="assetName"></param>
        /// <param name="loadSceneMode"></param>
        /// <param name="activateOnLoad"></param>
        /// <param name="priority"></param>
        /// <param name="progression"></param>
        /// <returns></returns>
        public async UniTask<BundlePack> LoadSceneAsync(string assetName, LoadSceneMode loadSceneMode = LoadSceneMode.Single, bool activateOnLoad = true, uint priority = 100, Progression progression = null)
        {
            string packageName = m_CurrentPackageName;
            return await m_CacheBundle.LoadSceneAsync(packageName, assetName, loadSceneMode, LocalPhysicsMode.None, activateOnLoad, priority, progression);
        }

        public  async UniTask<BundlePack> LoadSceneAsync(string assetName, LoadSceneMode loadSceneMode = LoadSceneMode.Single, LocalPhysicsMode localPhysicsMode = LocalPhysicsMode.None, bool activateOnLoad = true, uint priority = 100, Progression progression = null)
        {
            string packageName = m_CurrentPackageName;
            return await m_CacheBundle.LoadSceneAsync(packageName, assetName, loadSceneMode, localPhysicsMode, activateOnLoad, priority, progression);
        }

        public  async UniTask<BundlePack> LoadSceneAsync(string packageName, string assetName, LoadSceneMode loadSceneMode = LoadSceneMode.Single, bool activateOnLoad = true, uint priority = 100, Progression progression = null)
        {
            return await m_CacheBundle.LoadSceneAsync(packageName, assetName, loadSceneMode, LocalPhysicsMode.None, activateOnLoad, priority, progression);
        }

        public  async UniTask<BundlePack> LoadSceneAsync(string packageName, string assetName, LoadSceneMode loadSceneMode = LoadSceneMode.Single, LocalPhysicsMode localPhysicsMode = LocalPhysicsMode.None, bool activateOnLoad = true, uint priority = 100, Progression progression = null)
        {
            return await m_CacheBundle.LoadSceneAsync(packageName, assetName, loadSceneMode, localPhysicsMode, activateOnLoad, priority, progression);
        }

        /// <summary>
        /// Only load scene from bundle
        /// </summary>
        /// <param name="assetName"></param>
        /// <param name="loadSceneMode"></param>
        /// <param name="progression"></param>
        /// <returns></returns>
        public  BundlePack LoadScene(string assetName, LoadSceneMode loadSceneMode = LoadSceneMode.Single, Progression progression = null)
        {
            string packageName = m_CurrentPackageName;
            return m_CacheBundle.LoadScene(packageName, assetName, loadSceneMode, LocalPhysicsMode.None, progression);
        }

        public  BundlePack LoadScene(string assetName, LoadSceneMode loadSceneMode = LoadSceneMode.Single, LocalPhysicsMode localPhysicsMode = LocalPhysicsMode.None, Progression progression = null)
        {
            string packageName = m_CurrentPackageName;
            return m_CacheBundle.LoadScene(packageName, assetName, loadSceneMode, localPhysicsMode, progression);
        }

        public  BundlePack LoadScene(string packageName, string assetName, LoadSceneMode loadSceneMode = LoadSceneMode.Single, Progression progression = null)
        {
            return m_CacheBundle.LoadScene(packageName, assetName, loadSceneMode, LocalPhysicsMode.None, progression);
        }

        public  BundlePack LoadScene(string packageName, string assetName, LoadSceneMode loadSceneMode = LoadSceneMode.Single, LocalPhysicsMode localPhysicsMode = LocalPhysicsMode.None, Progression progression = null)
        {
            return m_CacheBundle.LoadScene(packageName, assetName, loadSceneMode, localPhysicsMode, progression);
        }

        /// <summary>
        /// Only load scene from bundle
        /// </summary>
        /// <param name="assetName"></param>
        /// <param name="activateOnLoad"></param>
        /// <param name="priority"></param>
        /// <param name="progression"></param>
        /// <returns></returns>
        public  async UniTask<BundlePack> LoadSingleSceneAsync(string assetName, bool activateOnLoad = true, uint priority = 100, Progression progression = null)
        {
            string packageName = m_CurrentPackageName;
            return await m_CacheBundle.LoadSceneAsync(packageName, assetName, LoadSceneMode.Single, LocalPhysicsMode.None, activateOnLoad, priority, progression);
        }

        public  async UniTask<BundlePack> LoadSingleSceneAsync(string packageName, string assetName, bool activateOnLoad = true, uint priority = 100, Progression progression = null)
        {
            return await m_CacheBundle.LoadSceneAsync(packageName, assetName, LoadSceneMode.Single, LocalPhysicsMode.None, activateOnLoad, priority, progression);
        }

        /// <summary>
        /// Only load scene from bundle
        /// </summary>
        /// <param name="assetName"></param>
        /// <param name="progression"></param>
        /// <returns></returns>
        public  BundlePack LoadSingleScene(string assetName, Progression progression = null)
        {
            string packageName = m_CurrentPackageName;
            return m_CacheBundle.LoadScene(packageName, assetName, LoadSceneMode.Single, LocalPhysicsMode.None, progression);
        }

        public  BundlePack LoadSingleScene(string packageName, string assetName, Progression progression = null)
        {
            return m_CacheBundle.LoadScene(packageName, assetName, LoadSceneMode.Single, LocalPhysicsMode.None, progression);
        }

        /// <summary>
        /// Only load scene from bundle
        /// </summary>
        /// <param name="assetName"></param>
        /// <param name="activateOnLoad"></param>
        /// <param name="priority"></param>
        /// <param name="progression"></param>
        /// <returns></returns>
        public  async UniTask<BundlePack> LoadAdditiveSceneAsync(string assetName, bool activateOnLoad = true, uint priority = 100, Progression progression = null)
        {
            string packageName = m_CurrentPackageName;
            return await m_CacheBundle.LoadSceneAsync(packageName, assetName, LoadSceneMode.Additive, LocalPhysicsMode.None, activateOnLoad, priority, progression);
        }

        public  async UniTask<BundlePack> LoadAdditiveSceneAsync(string packageName, string assetName, bool activateOnLoad = true, uint priority = 100, Progression progression = null)
        {
            return await m_CacheBundle.LoadSceneAsync(packageName, assetName, LoadSceneMode.Additive, LocalPhysicsMode.None, activateOnLoad, priority, progression);
        }

        /// <summary>
        /// Only load scene from bundle
        /// </summary>
        /// <param name="assetName"></param>
        /// <param name="progression"></param>
        /// <returns></returns>
        public  BundlePack LoadAdditiveScene(string assetName, Progression progression = null)
        {
            string packageName = m_CurrentPackageName;
            return m_CacheBundle.LoadScene(packageName, assetName, LoadSceneMode.Additive, LocalPhysicsMode.None, progression);
        }

        public  BundlePack LoadAdditiveScene(string packageName, string assetName, Progression progression = null)
        {
            return m_CacheBundle.LoadScene(packageName, assetName, LoadSceneMode.Additive, LocalPhysicsMode.None, progression);
        }

        /// <summary>
        /// Only unload scene from bundle
        /// </summary>
        /// <param name="assetName"></param>
        /// <param name="recursively"></param>
        public  void UnloadScene(string assetName, bool recursively = false)
        {
            if (!m_IsReleased)
                m_CacheBundle.UnloadScene(assetName, recursively);
        }

        public  void ReleaseScenes()
        {
            if (!m_IsReleased)
                m_CacheBundle.ReleaseScenes();
        }

        [System.Obsolete("Use ReleaseScenes instead")]
        public  void ReleaseBundleScenes()
        {
            if (!m_IsReleased)
                m_CacheBundle.ReleaseScenes();
        }
        #endregion

        #region Cacher
        public  bool HasInCache(string assetName)
        {
            if (TryRefineResourcesPath(assetName, out int index))
            {
                return m_CacheResource.HasInCache(assetName.Substring(index));
            }
            else
            {
                return m_CacheBundle.HasInCache(assetName);
            }
        }

        /// <summary>
        /// Get asset object cache
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="assetName"></param>
        /// <returns>
        /// <para>From Resources is &lt;ResourcePack&gt;</para>
        /// <para>From Bundle is &lt;BundlePack&gt;</para>
        /// </returns>
        public  T GetFromCache<T>(string assetName) where T : AssetObject
        {
            if (TryRefineResourcesPath(assetName, out int index))
            {
                return m_CacheResource.GetFromCache(assetName.Substring(index)) as T;
            }
            else
            {
                return m_CacheBundle.GetFromCache(assetName) as T;
            }
        }

        #region RawFile
        /// <summary>
        /// Get RawFile save path
        /// </summary>
        /// <param name="assetName"></param>
        /// <returns></returns>
        public  async UniTask<string> GetRawFilePathAsync(string assetName)
        {
            // Use preload to load bundle in cache, but for raw file the memory has not been allocated yet
            await PreloadRawFileAsync(assetName);
            var pack = GetFromCache<BundlePack>(assetName);
            if (pack != null)
            {
                // Get path from operation handle
                var operation = pack.GetOperationHandle<RawFileHandle>();
                string filePath = operation.GetRawFilePath();
                UnloadRawFile(assetName, true);
                return filePath;
            }

            return null;
        }

        /// <summary>
        /// Get RawFile save path from specific package
        /// </summary>
        /// <param name="packageName"></param>
        /// <param name="assetName"></param>
        /// <returns></returns>
        public  async UniTask<string> GetRawFilePathAsync(string packageName, string assetName)
        {
            // Use preload to load bundle in cache, but for raw file the memory has not been allocated yet
            await PreloadRawFileAsync(packageName, assetName);
            var pack = GetFromCache<BundlePack>(assetName);
            if (pack != null)
            {
                // Get path from operation handle
                var operation = pack.GetOperationHandle<RawFileHandle>();
                string filePath = operation.GetRawFilePath();
                UnloadRawFile(assetName, true);
                return filePath;
            }

            return null;
        }

        /// <summary>
        /// Get RawFile save path
        /// </summary>
        /// <param name="assetName"></param>
        /// <returns></returns>
        public  string GetRawFilePath(string assetName)
        {
            // Use preload to load bundle in cache, but for raw file the memory has not been allocated yet
            PreloadRawFile(assetName);
            var pack = GetFromCache<BundlePack>(assetName);
            if (pack != null)
            {
                var operation = pack.GetOperationHandle<RawFileHandle>();
                // Get path from operation handle
                string filePath = operation.GetRawFilePath();
                UnloadRawFile(assetName, true);
                return filePath;
            }

            return null;
        }

        /// <summary>
        /// Get RawFile save path from specific package
        /// </summary>
        /// <param name="packageName"></param>
        /// <param name="assetName"></param>
        /// <returns></returns>
        public  string GetRawFilePath(string packageName, string assetName)
        {
            // Use preload to load bundle in cache, but for raw file the memory has not been allocated yet
            PreloadRawFile(packageName, assetName);
            var pack = GetFromCache<BundlePack>(assetName);
            if (pack != null)
            {
                var operation = pack.GetOperationHandle<RawFileHandle>();
                // Get path from operation handle
                string filePath = operation.GetRawFilePath();
                UnloadRawFile(assetName, true);
                return filePath;
            }

            return null;
        }

        public  async UniTask PreloadRawFileAsync(string assetName, uint priority = 0, Progression progression = null, byte maxRetryCount = MAX_RETRY_COUNT)
        {
            var packageName = m_CurrentPackageName;
            if (TryRefineResourcesPath(assetName, out _))
            {
                Debug.Log("【Error】Only supports the bundle type.");
            }
            else
            {
                await m_CacheBundle.PreloadRawFileAsync(packageName, new string[] { assetName }, priority, progression, maxRetryCount);
            }
        }

        public  async UniTask PreloadRawFileAsync(string packageName, string assetName, uint priority = 0, Progression progression = null, byte maxRetryCount = MAX_RETRY_COUNT)
        {
            if (TryRefineResourcesPath(assetName, out _))
            {
                Debug.Log("【Error】Only supports the bundle type.");
            }
            else
            {
                await m_CacheBundle.PreloadRawFileAsync(packageName, new string[] { assetName }, priority, progression, maxRetryCount);
            }
        }

        public  async UniTask PreloadRawFileAsync(string[] assetNames, uint priority = 0, Progression progression = null, byte maxRetryCount = MAX_RETRY_COUNT)
        {
            var packageName = m_CurrentPackageName;
            await PreloadRawFileAsync(packageName, assetNames, priority, progression, maxRetryCount);
        }

        public  async UniTask PreloadRawFileAsync(string packageName, string[] assetNames, uint priority = 0, Progression progression = null, byte maxRetryCount = MAX_RETRY_COUNT)
        {
            int length = assetNames.Length;
            string[] rAssetNames = new string[length];
            string[] bAssetNames = new string[length];
            int rCount = 0;
            int bCount = 0;

            for (int i = 0; i < length; i++)
            {
                if (string.IsNullOrEmpty(assetNames[i]))
                {
                    continue;
                }

                if (TryRefineResourcesPath(assetNames[i], out _))
                {
                    rAssetNames[rCount++] = assetNames[i];
                }
                else
                {
                    bAssetNames[bCount++] = assetNames[i];
                }
            }

            // RawFile 只支持 Bundle
            if (rCount > 0)
            {
                Debug.Log("【Error】PreloadRawFile only supports the bundle type.");
            }

            if (bCount > 0)
            {
                await m_CacheBundle.PreloadRawFileAsync(packageName, bAssetNames, priority, progression, maxRetryCount);
            }
        }

        public  void PreloadRawFile(string assetName, Progression progression = null, byte maxRetryCount = MAX_RETRY_COUNT)
        {
            var packageName = m_CurrentPackageName;
            if (TryRefineResourcesPath(assetName, out _))
            {
                Debug.Log("【Error】Only supports the bundle type.");
            }
            else
            {
                m_CacheBundle.PreloadRawFile(packageName, new string[] { assetName }, progression, maxRetryCount);
            }
        }

        public  void PreloadRawFile(string packageName, string assetName, Progression progression = null, byte maxRetryCount = MAX_RETRY_COUNT)
        {
            if (TryRefineResourcesPath(assetName, out _))
            {
                Debug.Log("【Error】Only supports the bundle type.");
            }
            else
            {
                m_CacheBundle.PreloadRawFile(packageName, new string[] { assetName }, progression, maxRetryCount);
            }
        }

        public  void PreloadRawFile(string[] assetNames, Progression progression = null, byte maxRetryCount = MAX_RETRY_COUNT)
        {
            var packageName = m_CurrentPackageName;
            PreloadRawFile(packageName, assetNames, progression, maxRetryCount);
        }

        public  void PreloadRawFile(string packageName, string[] assetNames, Progression progression = null, byte maxRetryCount = MAX_RETRY_COUNT)
        {
            int length = assetNames.Length;
            string[] rAssetNames = new string[length];
            string[] bAssetNames = new string[length];
            int rCount = 0;
            int bCount = 0;

            for (int i = 0; i < length; i++)
            {
                if (string.IsNullOrEmpty(assetNames[i]))
                {
                    continue;
                }

                if (TryRefineResourcesPath(assetNames[i], out _))
                {
                    rAssetNames[rCount++] = assetNames[i];
                }
                else
                {
                    bAssetNames[bCount++] = assetNames[i];
                }
            }

            if (rCount > 0)
            {
                Debug.Log("【Error】Only supports the bundle type.");
            }

            if (bCount > 0)
            {
                m_CacheBundle.PreloadRawFile(packageName, bAssetNames, progression, maxRetryCount);
            }
        }

        /// <summary>
        /// Only load string type and byte[] type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="assetName"></param>
        /// <param name="priority"></param>
        /// <param name="progression"></param>
        /// <param name="maxRetryCount"></param>
        /// <returns></returns>
        public  async UniTask<T> LoadRawFileAsync<T>(string assetName, uint priority = 0, Progression progression = null, byte maxRetryCount = MAX_RETRY_COUNT)
        {
            var packageName = m_CurrentPackageName;
            if (TryRefineResourcesPath(assetName, out _))
            {
                Debug.Log("【Error】Only supports the bundle type.");
                return default;
            }
            return await m_CacheBundle.LoadRawFileAsync<T>(packageName, assetName, priority, progression, maxRetryCount);
        }

        public  async UniTask<T> LoadRawFileAsync<T>(string packageName, string assetName, uint priority = 0, Progression progression = null, byte maxRetryCount = MAX_RETRY_COUNT)
        {
            if (TryRefineResourcesPath(assetName, out _))
            {
                Debug.Log("【Error】Only supports the bundle type.");
                return default;
            }
            return await m_CacheBundle.LoadRawFileAsync<T>(packageName, assetName, priority, progression, maxRetryCount);
        }

        /// <summary>
        /// Only load string type and byte[] type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="assetName"></param>
        /// <param name="progression"></param>
        /// <param name="maxRetryCount"></param>
        /// <returns></returns>
        public  T LoadRawFile<T>(string assetName, Progression progression = null, byte maxRetryCount = MAX_RETRY_COUNT)
        {
            var packageName = m_CurrentPackageName;
            if (TryRefineResourcesPath(assetName, out _))
            {
                Debug.Log("【Error】Only supports the bundle type.");
                return default;
            }
            return m_CacheBundle.LoadRawFile<T>(packageName, assetName, progression, maxRetryCount);
        }

        public  T LoadRawFile<T>(string packageName, string assetName, Progression progression = null, byte maxRetryCount = MAX_RETRY_COUNT)
        {
            if (TryRefineResourcesPath(assetName, out _))
            {
                Debug.Log("【Error】Only supports the bundle type.");
                return default;
            }
            return m_CacheBundle.LoadRawFile<T>(packageName, assetName, progression, maxRetryCount);
        }

        public  void UnloadRawFile(string assetName, bool forceUnload = false)
        {
            if (TryRefineResourcesPath(assetName, out _))
            {
                Debug.Log("【Error】Only supports the bundle type.");
            }
            else if (!m_IsReleased)
            {
                m_CacheBundle.UnloadRawFile(assetName, forceUnload);
            }
        }

        public  void ReleaseRawFiles()
        {
            if (!m_IsReleased)
                m_CacheBundle.ReleaseRawFiles();
        }

        [System.Obsolete("Use ReleaseRawFiles instead")]
        public  void ReleaseBundleRawFiles()
        {
            if (!m_IsReleased)
                m_CacheBundle.ReleaseRawFiles();
        }
        #endregion

        #region Asset
        /// <summary>
        /// If use prefix "res#" will load from resources else will load from bundle
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="assetName"></param>
        /// <param name="priority"></param>
        /// <param name="progression"></param>
        /// <param name="maxRetryCount"></param>
        /// <returns></returns>
        public  async UniTask PreloadAssetAsync<T>(string assetName, uint priority = 0, Progression progression = null, byte maxRetryCount = MAX_RETRY_COUNT) where T : UnityEngine.Object
        {
            var packageName = m_CurrentPackageName;
            if (TryRefineResourcesPath(assetName, out int index))
            {
                await m_CacheResource.PreloadAssetAsync<T>(new string[] { assetName.Substring(index) }, progression, maxRetryCount);
            }
            else
            {
                await m_CacheBundle.PreloadAssetAsync<T>(packageName, new string[] { assetName }, priority, progression, maxRetryCount);
            }
        }

        public  async UniTask PreloadAssetAsync<T>(string packageName, string assetName, uint priority = 0, Progression progression = null, byte maxRetryCount = MAX_RETRY_COUNT) where T : UnityEngine.Object
        {
            if (TryRefineResourcesPath(assetName, out int index))
            {
                await m_CacheResource.PreloadAssetAsync<T>(new string[] { assetName.Substring(index) }, progression, maxRetryCount);
            }
            else
            {
                await m_CacheBundle.PreloadAssetAsync<T>(packageName, new string[] { assetName }, priority, progression, maxRetryCount);
            }
        }

        public  async UniTask PreloadAssetAsync<T>(string[] assetNames, uint priority = 0, Progression progression = null, byte maxRetryCount = MAX_RETRY_COUNT) where T : UnityEngine.Object
        {
            var packageName = m_CurrentPackageName;
            await PreloadAssetAsync<T>(packageName, assetNames, priority, progression, maxRetryCount);
        }

        public  async UniTask PreloadAssetAsync<T>(string packageName, string[] assetNames, uint priority = 0, Progression progression = null, byte maxRetryCount = MAX_RETRY_COUNT) where T : UnityEngine.Object
        {
            int length = assetNames.Length;
            string[] rAssetNames = new string[length];
            string[] bAssetNames = new string[length];
            int rCount = 0;
            int bCount = 0;

            for (int i = 0; i < length; i++)
            {
                if (string.IsNullOrEmpty(assetNames[i]))
                {
                    continue;
                }

                if (TryRefineResourcesPath(assetNames[i], out int index))
                {
                    rAssetNames[rCount++] = assetNames[i].Substring(index);
                }
                else
                {
                    bAssetNames[bCount++] = assetNames[i];
                }
            }

            if (rCount > 0 && bCount > 0)
            {
                await UniTask.WhenAll
                (
                    m_CacheResource.PreloadAssetAsync<T>(rAssetNames, progression, maxRetryCount),
                    m_CacheBundle.PreloadAssetAsync<T>(packageName, bAssetNames, priority, progression, maxRetryCount)
                );
            }
            else if (rCount > 0)
            {
                await m_CacheResource.PreloadAssetAsync<T>(rAssetNames, progression, maxRetryCount);
            }
            else if (bCount > 0)
            {
                await m_CacheBundle.PreloadAssetAsync<T>(packageName, bAssetNames, priority, progression, maxRetryCount);
            }
        }

        /// <summary>
        /// If use prefix "res#" will load from resources else will load from bundle
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="assetName"></param>
        /// <param name="progression"></param>
        /// <param name="maxRetryCount"></param>
        public  void PreloadAsset<T>(string assetName, Progression progression = null, byte maxRetryCount = MAX_RETRY_COUNT) where T : UnityEngine.Object
        {
            var packageName = m_CurrentPackageName;
            if (TryRefineResourcesPath(assetName, out int index))
            {
                m_CacheResource.PreloadAsset<T>(new string[] { assetName.Substring(index) }, progression, maxRetryCount);
            }
            else
            {
                m_CacheBundle.PreloadAsset<T>(packageName, new string[] { assetName }, progression, maxRetryCount);
            }
        }

        public  void PreloadAsset<T>(string packageName, string assetName, Progression progression = null, byte maxRetryCount = MAX_RETRY_COUNT) where T : UnityEngine.Object
        {
            if (TryRefineResourcesPath(assetName, out int index))
            {
                m_CacheResource.PreloadAsset<T>(new string[] { assetName.Substring(index) }, progression, maxRetryCount);
            }
            else
            {
                m_CacheBundle.PreloadAsset<T>(packageName, new string[] { assetName }, progression, maxRetryCount);
            }
        }

        public  void PreloadAsset<T>(string[] assetNames, Progression progression = null, byte maxRetryCount = MAX_RETRY_COUNT) where T : UnityEngine.Object
        {
            var packageName = m_CurrentPackageName;
            PreloadAsset<T>(packageName, assetNames, progression, maxRetryCount);
        }

        public  void PreloadAsset<T>(string packageName, string[] assetNames, Progression progression = null, byte maxRetryCount = MAX_RETRY_COUNT) where T : UnityEngine.Object
        {
            int length = assetNames.Length;
            string[] rAssetNames = new string[length];
            string[] bAssetNames = new string[length];
            int rCount = 0;
            int bCount = 0;

            for (int i = 0; i < length; i++)
            {
                if (string.IsNullOrEmpty(assetNames[i]))
                {
                    continue;
                }

                if (TryRefineResourcesPath(assetNames[i], out int index))
                {
                    rAssetNames[rCount++] = assetNames[i].Substring(index);
                }
                else
                {
                    bAssetNames[bCount++] = assetNames[i];
                }
            }

            if (rCount > 0)
            {
                m_CacheResource.PreloadAsset<T>(rAssetNames, progression, maxRetryCount);
            }

            if (bCount > 0)
            {
                m_CacheBundle.PreloadAsset<T>(packageName, bAssetNames, progression, maxRetryCount);
            }
        }

        /// <summary>
        /// If use prefix "res#" will load from resources else will load from bundle
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="assetName"></param>
        /// <param name="priority"></param>
        /// <param name="progression"></param>
        /// <param name="maxRetryCount"></param>
        /// <returns></returns>
        public  async UniTask<T> LoadAssetAsync<T>(string assetName, uint priority = 0, Progression progression = null, byte maxRetryCount = MAX_RETRY_COUNT) where T : UnityEngine.Object
        {
            var packageName = m_CurrentPackageName;
            if (TryRefineResourcesPath(assetName, out int index))
            {
                return await m_CacheResource.LoadAssetAsync<T>(assetName.Substring(index), progression, maxRetryCount);
            }
            else
            {
                return await m_CacheBundle.LoadAssetAsync<T>(packageName, assetName, priority, progression, maxRetryCount);
            }
        }

        public  async UniTask<T> LoadAssetAsync<T>(string packageName, string assetName, uint priority = 0, Progression progression = null, byte maxRetryCount = MAX_RETRY_COUNT) where T : UnityEngine.Object
        {
            if (TryRefineResourcesPath(assetName, out int index))
            {
                return await m_CacheResource.LoadAssetAsync<T>(assetName.Substring(index), progression, maxRetryCount);
            }
            else
            {
                return await m_CacheBundle.LoadAssetAsync<T>(packageName, assetName, priority, progression, maxRetryCount);
            }
        }

        /// <summary>
        /// If use prefix "res#" will load from resources else will load from bundle
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="assetName"></param>
        /// <param name="progression"></param>
        /// <param name="maxRetryCount"></param>
        /// <returns></returns>
        public  T LoadAsset<T>(string assetName, Progression progression = null, byte maxRetryCount = MAX_RETRY_COUNT) where T : UnityEngine.Object
        {
            var packageName = m_CurrentPackageName;
            if (TryRefineResourcesPath(assetName, out int index))
            {
                return m_CacheResource.LoadAsset<T>(assetName.Substring(index), progression, maxRetryCount);
            }
            else
            {
                return m_CacheBundle.LoadAsset<T>(packageName, assetName, progression, maxRetryCount);
            }
        }

        public  T LoadAsset<T>(string packageName, string assetName, Progression progression = null, byte maxRetryCount = MAX_RETRY_COUNT) where T : UnityEngine.Object
        {
            if (TryRefineResourcesPath(assetName, out int index))
            {
                return m_CacheResource.LoadAsset<T>(assetName.Substring(index), progression, maxRetryCount);
            }
            else
            {
                return m_CacheBundle.LoadAsset<T>(packageName, assetName, progression, maxRetryCount);
            }
        }

        /// <summary>
        /// If use prefix "res#" will load from resources else will load from bundle
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="assetName"></param>
        /// <param name="priority"></param>
        /// <param name="progression"></param>
        /// <param name="maxRetryCount"></param>
        /// <returns></returns>
        public  async UniTask<T> InstantiateAssetAsync<T>(string assetName, uint priority = 0, Progression progression = null, byte maxRetryCount = MAX_RETRY_COUNT) where T : UnityEngine.Object
        {
            var packageName = m_CurrentPackageName;
            if (TryRefineResourcesPath(assetName, out int index))
            {
                var asset = await m_CacheResource.LoadAssetAsync<T>(assetName.Substring(index), progression, maxRetryCount);
                var cloneAsset = (asset == null) ? null : UnityEngine.Object.Instantiate(asset);
                return cloneAsset;
            }
            else
            {
                var asset = await m_CacheBundle.LoadAssetAsync<T>(packageName, assetName, priority, progression, maxRetryCount);
                var cloneAsset = (asset == null) ? null : UnityEngine.Object.Instantiate(asset);
                return cloneAsset;
            }
        }

        public  async UniTask<T> InstantiateAssetAsync<T>(string packageName, string assetName, uint priority = 0, Progression progression = null, byte maxRetryCount = MAX_RETRY_COUNT) where T : UnityEngine.Object
        {
            if (TryRefineResourcesPath(assetName, out int index))
            {
                var asset = await m_CacheResource.LoadAssetAsync<T>(assetName.Substring(index), progression, maxRetryCount);
                var cloneAsset = (asset == null) ? null : UnityEngine.Object.Instantiate(asset);
                return cloneAsset;
            }
            else
            {
                var asset = await m_CacheBundle.LoadAssetAsync<T>(packageName, assetName, priority, progression, maxRetryCount);
                var cloneAsset = (asset == null) ? null : UnityEngine.Object.Instantiate(asset);
                return cloneAsset;
            }
        }

        public  async UniTask<T> InstantiateAssetAsync<T>(string assetName, Vector3 position, Quaternion rotation, uint priority = 0, Progression progression = null, byte maxRetryCount = MAX_RETRY_COUNT) where T : UnityEngine.Object
        {
            var packageName = m_CurrentPackageName;
            if (TryRefineResourcesPath(assetName, out int index))
            {
                var asset = await m_CacheResource.LoadAssetAsync<T>(assetName.Substring(index), progression, maxRetryCount);
                var cloneAsset = (asset == null) ? null : UnityEngine.Object.Instantiate(asset, position, rotation);
                return cloneAsset;
            }
            else
            {
                var asset = await m_CacheBundle.LoadAssetAsync<T>(packageName, assetName, priority, progression, maxRetryCount);
                var cloneAsset = (asset == null) ? null : UnityEngine.Object.Instantiate(asset, position, rotation);
                return cloneAsset;
            }
        }

        public  async UniTask<T> InstantiateAssetAsync<T>(string packageName, string assetName, Vector3 position, Quaternion rotation, uint priority = 0, Progression progression = null, byte maxRetryCount = MAX_RETRY_COUNT) where T : UnityEngine.Object
        {
            if (TryRefineResourcesPath(assetName, out int index))
            {
                var asset = await m_CacheResource.LoadAssetAsync<T>(assetName.Substring(index), progression, maxRetryCount);
                var cloneAsset = (asset == null) ? null : UnityEngine.Object.Instantiate(asset, position, rotation);
                return cloneAsset;
            }
            else
            {
                var asset = await m_CacheBundle.LoadAssetAsync<T>(packageName, assetName, priority, progression, maxRetryCount);
                var cloneAsset = (asset == null) ? null : UnityEngine.Object.Instantiate(asset, position, rotation);
                return cloneAsset;
            }
        }

        public  async UniTask<T> InstantiateAssetAsync<T>(string assetName, Vector3 position, Quaternion rotation, Transform parent, uint priority = 0, Progression progression = null, byte maxRetryCount = MAX_RETRY_COUNT) where T : UnityEngine.Object
        {
            var packageName = m_CurrentPackageName;
            if (TryRefineResourcesPath(assetName, out int index))
            {
                var asset = await m_CacheResource.LoadAssetAsync<T>(assetName.Substring(index), progression, maxRetryCount);
                var cloneAsset = (asset == null) ? null : UnityEngine.Object.Instantiate(asset, position, rotation, parent);
                return cloneAsset;
            }
            else
            {
                var asset = await m_CacheBundle.LoadAssetAsync<T>(packageName, assetName, priority, progression, maxRetryCount);
                var cloneAsset = (asset == null) ? null : UnityEngine.Object.Instantiate(asset, position, rotation, parent);
                return cloneAsset;
            }
        }

        public  async UniTask<T> InstantiateAssetAsync<T>(string packageName, string assetName, Vector3 position, Quaternion rotation, Transform parent, uint priority = 0, Progression progression = null, byte maxRetryCount = MAX_RETRY_COUNT) where T : UnityEngine.Object
        {
            if (TryRefineResourcesPath(assetName, out int index))
            {
                var asset = await m_CacheResource.LoadAssetAsync<T>(assetName.Substring(index), progression, maxRetryCount);
                var cloneAsset = (asset == null) ? null : UnityEngine.Object.Instantiate(asset, position, rotation, parent);
                return cloneAsset;
            }
            else
            {
                var asset = await m_CacheBundle.LoadAssetAsync<T>(packageName, assetName, priority, progression, maxRetryCount);
                var cloneAsset = (asset == null) ? null : UnityEngine.Object.Instantiate(asset, position, rotation, parent);
                return cloneAsset;
            }
        }

        public  async UniTask<T> InstantiateAssetAsync<T>(string assetName, Transform parent, uint priority = 0, Progression progression = null, byte maxRetryCount = MAX_RETRY_COUNT) where T : UnityEngine.Object
        {
            var packageName = m_CurrentPackageName;
            if (TryRefineResourcesPath(assetName, out int index))
            {
                var asset = await m_CacheResource.LoadAssetAsync<T>(assetName.Substring(index), progression, maxRetryCount);
                var cloneAsset = (asset == null) ? null : UnityEngine.Object.Instantiate(asset, parent);
                return cloneAsset;
            }
            else
            {
                var asset = await m_CacheBundle.LoadAssetAsync<T>(packageName, assetName, priority, progression, maxRetryCount);
                var cloneAsset = (asset == null) ? null : UnityEngine.Object.Instantiate(asset, parent);
                return cloneAsset;
            }
        }

        public  async UniTask<T> InstantiateAssetAsync<T>(string packageName, string assetName, Transform parent, uint priority = 0, Progression progression = null, byte maxRetryCount = MAX_RETRY_COUNT) where T : UnityEngine.Object
        {
            if (TryRefineResourcesPath(assetName, out int index))
            {
                var asset = await m_CacheResource.LoadAssetAsync<T>(assetName.Substring(index), progression, maxRetryCount);
                var cloneAsset = (asset == null) ? null : UnityEngine.Object.Instantiate(asset, parent);
                return cloneAsset;
            }
            else
            {
                var asset = await m_CacheBundle.LoadAssetAsync<T>(packageName, assetName, priority, progression, maxRetryCount);
                var cloneAsset = (asset == null) ? null : UnityEngine.Object.Instantiate(asset, parent);
                return cloneAsset;
            }
        }

        public  async UniTask<T> InstantiateAssetAsync<T>(string assetName, Transform parent, bool worldPositionStays, uint priority = 0, Progression progression = null, byte maxRetryCount = MAX_RETRY_COUNT) where T : UnityEngine.Object
        {
            var packageName = m_CurrentPackageName;
            if (TryRefineResourcesPath(assetName, out int index))
            {
                var asset = await m_CacheResource.LoadAssetAsync<T>(assetName.Substring(index), progression, maxRetryCount);
                var cloneAsset = (asset == null) ? null : UnityEngine.Object.Instantiate(asset, parent, worldPositionStays);
                return cloneAsset;
            }
            else
            {
                var asset = await m_CacheBundle.LoadAssetAsync<T>(packageName, assetName, priority, progression, maxRetryCount);
                var cloneAsset = (asset == null) ? null : UnityEngine.Object.Instantiate(asset, parent, worldPositionStays);
                return cloneAsset;
            }
        }

        public  async UniTask<T> InstantiateAssetAsync<T>(string packageName, string assetName, Transform parent, bool worldPositionStays, uint priority = 0, Progression progression = null, byte maxRetryCount = MAX_RETRY_COUNT) where T : UnityEngine.Object
        {
            if (TryRefineResourcesPath(assetName, out int index))
            {
                var asset = await m_CacheResource.LoadAssetAsync<T>(assetName.Substring(index), progression, maxRetryCount);
                var cloneAsset = (asset == null) ? null : UnityEngine.Object.Instantiate(asset, parent, worldPositionStays);
                return cloneAsset;
            }
            else
            {
                var asset = await m_CacheBundle.LoadAssetAsync<T>(packageName, assetName, priority, progression, maxRetryCount);
                var cloneAsset = (asset == null) ? null : UnityEngine.Object.Instantiate(asset, parent, worldPositionStays);
                return cloneAsset;
            }
        }

        /// <summary>
        /// If use prefix "res#" will load from resources else will load from bundle
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="assetName"></param>
        /// <param name="progression"></param>
        /// <param name="maxRetryCount"></param>
        /// <returns></returns>
        public  T InstantiateAsset<T>(string assetName, Progression progression = null, byte maxRetryCount = MAX_RETRY_COUNT) where T : UnityEngine.Object
        {
            var packageName = m_CurrentPackageName;
            if (TryRefineResourcesPath(assetName, out int index))
            {
                var asset = m_CacheResource.LoadAsset<T>(assetName.Substring(index), progression, maxRetryCount);
                var cloneAsset = (asset == null) ? null : UnityEngine.Object.Instantiate(asset);
                return cloneAsset;
            }
            else
            {
                var asset = m_CacheBundle.LoadAsset<T>(packageName, assetName, progression, maxRetryCount);
                var cloneAsset = (asset == null) ? null : UnityEngine.Object.Instantiate(asset);
                return cloneAsset;
            }
        }

        public  T InstantiateAsset<T>(string packageName, string assetName, Progression progression = null, byte maxRetryCount = MAX_RETRY_COUNT) where T : UnityEngine.Object
        {
            if (TryRefineResourcesPath(assetName, out int index))
            {
                var asset = m_CacheResource.LoadAsset<T>(assetName.Substring(index), progression, maxRetryCount);
                var cloneAsset = (asset == null) ? null : UnityEngine.Object.Instantiate(asset);
                return cloneAsset;
            }
            else
            {
                var asset = m_CacheBundle.LoadAsset<T>(packageName, assetName, progression, maxRetryCount);
                var cloneAsset = (asset == null) ? null : UnityEngine.Object.Instantiate(asset);
                return cloneAsset;
            }
        }

        public  T InstantiateAsset<T>(string assetName, Vector3 position, Quaternion rotation, Progression progression = null, byte maxRetryCount = MAX_RETRY_COUNT) where T : UnityEngine.Object
        {
            var packageName = m_CurrentPackageName;
            if (TryRefineResourcesPath(assetName, out int index))
            {
                var asset = m_CacheResource.LoadAsset<T>(assetName.Substring(index), progression, maxRetryCount);
                var cloneAsset = (asset == null) ? null : UnityEngine.Object.Instantiate(asset, position, rotation);
                return cloneAsset;
            }
            else
            {
                var asset = m_CacheBundle.LoadAsset<T>(packageName, assetName, progression, maxRetryCount);
                var cloneAsset = (asset == null) ? null : UnityEngine.Object.Instantiate(asset, position, rotation);
                return cloneAsset;
            }
        }

        public  T InstantiateAsset<T>(string packageName, string assetName, Vector3 position, Quaternion rotation, Progression progression = null, byte maxRetryCount = MAX_RETRY_COUNT) where T : UnityEngine.Object
        {
            if (TryRefineResourcesPath(assetName, out int index))
            {
                var asset = m_CacheResource.LoadAsset<T>(assetName.Substring(index), progression, maxRetryCount);
                var cloneAsset = (asset == null) ? null : UnityEngine.Object.Instantiate(asset, position, rotation);
                return cloneAsset;
            }
            else
            {
                var asset = m_CacheBundle.LoadAsset<T>(packageName, assetName, progression, maxRetryCount);
                var cloneAsset = (asset == null) ? null : UnityEngine.Object.Instantiate(asset, position, rotation);
                return cloneAsset;
            }
        }

        public  T InstantiateAsset<T>(string assetName, Vector3 position, Quaternion rotation, Transform parent, Progression progression = null, byte maxRetryCount = MAX_RETRY_COUNT) where T : UnityEngine.Object
        {
            var packageName = m_CurrentPackageName;
            if (TryRefineResourcesPath(assetName, out int index))
            {
                var asset = m_CacheResource.LoadAsset<T>(assetName.Substring(index), progression, maxRetryCount);
                var cloneAsset = (asset == null) ? null : UnityEngine.Object.Instantiate(asset, position, rotation, parent);
                return cloneAsset;
            }
            else
            {
                var asset = m_CacheBundle.LoadAsset<T>(packageName, assetName, progression, maxRetryCount);
                var cloneAsset = (asset == null) ? null : UnityEngine.Object.Instantiate(asset, position, rotation, parent);
                return cloneAsset;
            }
        }

        public  T InstantiateAsset<T>(string packageName, string assetName, Vector3 position, Quaternion rotation, Transform parent, Progression progression = null, byte maxRetryCount = MAX_RETRY_COUNT) where T : UnityEngine.Object
        {
            if (TryRefineResourcesPath(assetName, out int index))
            {
                var asset = m_CacheResource.LoadAsset<T>(assetName.Substring(index), progression, maxRetryCount);
                var cloneAsset = (asset == null) ? null : UnityEngine.Object.Instantiate(asset, position, rotation, parent);
                return cloneAsset;
            }
            else
            {
                var asset = m_CacheBundle.LoadAsset<T>(packageName, assetName, progression, maxRetryCount);
                var cloneAsset = (asset == null) ? null : UnityEngine.Object.Instantiate(asset, position, rotation, parent);
                return cloneAsset;
            }
        }

        public  T InstantiateAsset<T>(string assetName, Transform parent, Progression progression = null, byte maxRetryCount = MAX_RETRY_COUNT) where T : UnityEngine.Object
        {
            var packageName = m_CurrentPackageName;
            if (TryRefineResourcesPath(assetName, out int index))
            {
                var asset = m_CacheResource.LoadAsset<T>(assetName.Substring(index), progression, maxRetryCount);
                var cloneAsset = (asset == null) ? null : UnityEngine.Object.Instantiate(asset, parent);
                return cloneAsset;
            }
            else
            {
                var asset = m_CacheBundle.LoadAsset<T>(packageName, assetName, progression, maxRetryCount);
                var cloneAsset = (asset == null) ? null : UnityEngine.Object.Instantiate(asset, parent);
                return cloneAsset;
            }
        }

        public  T InstantiateAsset<T>(string packageName, string assetName, Transform parent, Progression progression = null, byte maxRetryCount = MAX_RETRY_COUNT) where T : UnityEngine.Object
        {
            if (TryRefineResourcesPath(assetName, out int index))
            {
                var asset = m_CacheResource.LoadAsset<T>(assetName.Substring(index), progression, maxRetryCount);
                var cloneAsset = (asset == null) ? null : UnityEngine.Object.Instantiate(asset, parent);
                return cloneAsset;
            }
            else
            {
                var asset = m_CacheBundle.LoadAsset<T>(packageName, assetName, progression, maxRetryCount);
                var cloneAsset = (asset == null) ? null : UnityEngine.Object.Instantiate(asset, parent);
                return cloneAsset;
            }
        }

        public  T InstantiateAsset<T>(string assetName, Transform parent, bool worldPositionStays, Progression progression = null, byte maxRetryCount = MAX_RETRY_COUNT) where T : UnityEngine.Object
        {
            var packageName = m_CurrentPackageName;
            if (TryRefineResourcesPath(assetName, out int index))
            {
                var asset = m_CacheResource.LoadAsset<T>(assetName.Substring(index), progression, maxRetryCount);
                var cloneAsset = (asset == null) ? null : UnityEngine.Object.Instantiate(asset, parent, worldPositionStays);
                return cloneAsset;
            }
            else
            {
                var asset = m_CacheBundle.LoadAsset<T>(packageName, assetName, progression, maxRetryCount);
                var cloneAsset = (asset == null) ? null : UnityEngine.Object.Instantiate(asset, parent, worldPositionStays);
                return cloneAsset;
            }
        }

        public  T InstantiateAsset<T>(string packageName, string assetName, Transform parent, bool worldPositionStays, Progression progression = null, byte maxRetryCount = MAX_RETRY_COUNT) where T : UnityEngine.Object
        {
            if (TryRefineResourcesPath(assetName, out int index))
            {
                var asset = m_CacheResource.LoadAsset<T>(assetName.Substring(index), progression, maxRetryCount);
                var cloneAsset = (asset == null) ? null : UnityEngine.Object.Instantiate(asset, parent, worldPositionStays);
                return cloneAsset;
            }
            else
            {
                var asset = m_CacheBundle.LoadAsset<T>(packageName, assetName, progression, maxRetryCount);
                var cloneAsset = (asset == null) ? null : UnityEngine.Object.Instantiate(asset, parent, worldPositionStays);
                return cloneAsset;
            }
        }

        public  void UnloadAsset(string assetName, bool forceUnload = false)
        {
            if (TryRefineResourcesPath(assetName, out int index))
            {
                m_CacheResource.UnloadAsset(assetName.Substring(index), forceUnload);
            }
            else if (!m_IsReleased)
            {
                m_CacheBundle.UnloadAsset(assetName, forceUnload);
            }
        }

        public  void ReleaseAssets(LoadType loadType = LoadType.Any)
        {
            if (loadType == LoadType.Any)
            {
                m_CacheResource.ReleaseAssets();
                if (!m_IsReleased)
                    m_CacheBundle.ReleaseAssets();
            }
            else if (loadType == LoadType.Resources) m_CacheResource.ReleaseAssets();
            else if (loadType == LoadType.Bundle && !m_IsReleased) m_CacheBundle.ReleaseAssets();
        }

        [System.Obsolete("Use ReleaseAssets instead")]
        public  void ReleaseResourceAssets()
        {
            m_CacheResource.ReleaseAssets();
        }

        [System.Obsolete("Use ReleaseAssets instead")]
        public  void ReleaseBundleAssets()
        {
            if (!m_IsReleased)
                m_CacheBundle.ReleaseAssets();
        }
        #endregion
        #endregion

        #region Group Cacher
        public  bool HasInCache(int groupId, string assetName)
        {
            if (TryRefineResourcesPath(assetName, out int index))
            {
                return m_GroupResource.HasInCache(groupId, assetName.Substring(index));
            }
            else
            {
                return m_GroupBundle.HasInCache(groupId, assetName);
            }
        }

        #region RawFile
        public  async UniTask PreloadRawFileAsync(int groupId, string assetName, uint priority = 0, Progression progression = null, byte maxRetryCount = MAX_RETRY_COUNT)
        {
            var packageName = m_CurrentPackageName;
            if (TryRefineResourcesPath(assetName, out _))
            {
                Debug.Log("【Error】Only supports the bundle type.");
            }
            else
            {
                await m_GroupBundle.PreloadRawFileAsync(groupId, packageName, new string[] { assetName }, priority, progression, maxRetryCount);
            }
        }

        public  async UniTask PreloadRawFileAsync(int groupId, string packageName, string assetName, uint priority = 0, Progression progression = null, byte maxRetryCount = MAX_RETRY_COUNT)
        {
            if (TryRefineResourcesPath(assetName, out _))
            {
                Debug.Log("【Error】Only supports the bundle type.");
            }
            else
            {
                await m_GroupBundle.PreloadRawFileAsync(groupId, packageName, new string[] { assetName }, priority, progression, maxRetryCount);
            }
        }

        public  async UniTask PreloadRawFileAsync(int groupId, string[] assetNames, uint priority = 0, Progression progression = null, byte maxRetryCount = MAX_RETRY_COUNT)
        {
            var packageName = m_CurrentPackageName;
            await PreloadRawFileAsync(groupId, packageName, assetNames, priority, progression, maxRetryCount);
        }

        public  async UniTask PreloadRawFileAsync(int groupId, string packageName, string[] assetNames, uint priority = 0, Progression progression = null, byte maxRetryCount = MAX_RETRY_COUNT)
        {
            int length = assetNames.Length;
            string[] rAssetNames = new string[length];
            string[] bAssetNames = new string[length];
            int rCount = 0;
            int bCount = 0;

            for (int i = 0; i < length; i++)
            {
                if (string.IsNullOrEmpty(assetNames[i]))
                {
                    continue;
                }

                if (TryRefineResourcesPath(assetNames[i], out _))
                {
                    rAssetNames[rCount++] = assetNames[i];
                }
                else
                {
                    bAssetNames[bCount++] = assetNames[i];
                }
            }

            if (rCount > 0)
            {
                Debug.Log("【Error】Only supports the bundle type.");
            }

            if (bCount > 0)
            {
                await m_GroupBundle.PreloadRawFileAsync(groupId, packageName, bAssetNames, priority, progression, maxRetryCount);
            }
        }

        public  void PreloadRawFile(int groupId, string assetName, Progression progression = null, byte maxRetryCount = MAX_RETRY_COUNT)
        {
            var packageName = m_CurrentPackageName;
            if (TryRefineResourcesPath(assetName, out _))
            {
                Debug.Log("【Error】Only supports the bundle type.");
            }
            else
            {
                m_GroupBundle.PreloadRawFile(groupId, packageName, new string[] { assetName }, progression, maxRetryCount);
            }
        }

        public  void PreloadRawFile(int groupId, string packageName, string assetName, Progression progression = null, byte maxRetryCount = MAX_RETRY_COUNT)
        {
            if (TryRefineResourcesPath(assetName, out _))
            {
                Debug.Log("【Error】Only supports the bundle type.");
            }
            else
            {
                m_GroupBundle.PreloadRawFile(groupId, packageName, new string[] { assetName }, progression, maxRetryCount);
            }
        }

        public  void PreloadRawFile(int groupId, string[] assetNames, Progression progression = null, byte maxRetryCount = MAX_RETRY_COUNT)
        {
            var packageName = m_CurrentPackageName;
            PreloadRawFile(groupId, packageName, assetNames, progression, maxRetryCount);
        }

        public  void PreloadRawFile(int groupId, string packageName, string[] assetNames, Progression progression = null, byte maxRetryCount = MAX_RETRY_COUNT)
        {
            int length = assetNames.Length;
            string[] rAssetNames = new string[length];
            string[] bAssetNames = new string[length];
            int rCount = 0;
            int bCount = 0;

            for (int i = 0; i < length; i++)
            {
                if (string.IsNullOrEmpty(assetNames[i]))
                {
                    continue;
                }

                if (TryRefineResourcesPath(assetNames[i], out _))
                {
                    rAssetNames[rCount++] = assetNames[i];
                }
                else
                {
                    bAssetNames[bCount++] = assetNames[i];
                }
            }

            if (rCount > 0)
            {
                Debug.Log("【Error】Only supports the bundle type.");
            }

            if (bCount > 0)
            {
                m_GroupBundle.PreloadRawFile(groupId, packageName, bAssetNames, progression, maxRetryCount);
            }
        }

        /// <summary>
        /// Only load string type and byte[] type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="groupId"></param>
        /// <param name="assetName"></param>
        /// <param name="priority"></param>
        /// <param name="progression"></param>
        /// <param name="maxRetryCount"></param>
        /// <returns></returns>
        public  async UniTask<T> LoadRawFileAsync<T>(int groupId, string assetName, uint priority = 0, Progression progression = null, byte maxRetryCount = MAX_RETRY_COUNT)
        {
            var packageName = m_CurrentPackageName;
            if (TryRefineResourcesPath(assetName, out _))
            {
                Debug.Log("【Error】Only supports the bundle type.");
                return default;
            }
            else
            {
                return await m_GroupBundle.LoadRawFileAsync<T>(groupId, packageName, assetName, priority, progression, maxRetryCount);
            }
        }

        public  async UniTask<T> LoadRawFileAsync<T>(int groupId, string packageName, string assetName, uint priority = 0, Progression progression = null, byte maxRetryCount = MAX_RETRY_COUNT)
        {
            if (TryRefineResourcesPath(assetName, out _))
            {
                Debug.Log("【Error】Only supports the bundle type.");
                return default;
            }
            else
            {
                return await m_GroupBundle.LoadRawFileAsync<T>(groupId, packageName, assetName, priority, progression, maxRetryCount);
            }
        }

        /// <summary>
        /// Only load string type and byte[] type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="groupId"></param>
        /// <param name="assetName"></param>
        /// <param name="progression"></param>
        /// <param name="maxRetryCount"></param>
        /// <returns></returns>
        public  T LoadRawFile<T>(int groupId, string assetName, Progression progression = null, byte maxRetryCount = MAX_RETRY_COUNT)
        {
            var packageName = m_CurrentPackageName;
            if (TryRefineResourcesPath(assetName, out _))
            {
                Debug.Log("【Error】Only supports the bundle type.");
                return default;
            }
            else
            {
                return m_GroupBundle.LoadRawFile<T>(groupId, packageName, assetName, progression, maxRetryCount);
            }
        }

        public  T LoadRawFile<T>(int groupId, string packageName, string assetName, Progression progression = null, byte maxRetryCount = MAX_RETRY_COUNT)
        {
            if (TryRefineResourcesPath(assetName, out _))
            {
                Debug.Log("【Error】Only supports the bundle type.");
                return default;
            }
            else
            {
                return m_GroupBundle.LoadRawFile<T>(groupId, packageName, assetName, progression, maxRetryCount);
            }
        }

        /// <summary>
        /// Unload a single raw file from the specified group (soft reference unload).
        /// This method decrements the reference count in both GroupCache and CacheBundle layers.
        /// The actual resource will only be released when all references across all groups reach zero.
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="assetName"></param>
        public  void UnloadRawFile(int groupId, string assetName)
        {
            if (TryRefineResourcesPath(assetName, out _))
            {
                Debug.Log("【Error】Only supports the bundle type.");
            }
            else if (!m_IsReleased)
            {
                m_GroupBundle.UnloadRawFile(groupId, assetName);
            }
        }

        /// <summary>
        /// Unload all raw files from the specified group (soft reference unload).
        /// This method releases all resources associated with the group by decrementing their reference counts.
        /// Resources shared with other groups will remain loaded until all group references are released.
        /// Note: This uses reference counting and will not force unload resources that are still referenced by other groups.
        /// </summary>
        /// <param name="groupId"></param>
        public  void UnloadRawFiles(int groupId)
        {
            if (!m_IsReleased)
                m_GroupBundle.UnloadRawFiles(groupId);
        }

        [System.Obsolete("Use UnloadRawFiles instead")]
        public  void ReleaseBundleRawFiles(int groupId)
        {
            if (!m_IsReleased)
                m_GroupBundle.UnloadRawFiles(groupId);
        }
        #endregion

        #region Asset
        /// <summary>
        /// If use prefix "res#" will load from resources else will load from bundle
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="groupId"></param>
        /// <param name="assetName"></param>
        /// <param name="priority"></param>
        /// <param name="progression"></param>
        /// <param name="maxRetryCount"></param>
        /// <returns></returns>
        public  async UniTask PreloadAssetAsync<T>(int groupId, string assetName, uint priority = 0, Progression progression = null, byte maxRetryCount = MAX_RETRY_COUNT) where T : UnityEngine.Object
        {
            var packageName = m_CurrentPackageName;
            if (TryRefineResourcesPath(assetName, out int index))
            {
                await m_GroupResource.PreloadAssetAsync<T>(groupId, new string[] { assetName.Substring(index) }, progression, maxRetryCount);
            }
            else
            {
                await m_GroupBundle.PreloadAssetAsync<T>(groupId, packageName, new string[] { assetName }, priority, progression, maxRetryCount);
            }
        }

        public  async UniTask PreloadAssetAsync<T>(int groupId, string packageName, string assetName, uint priority = 0, Progression progression = null, byte maxRetryCount = MAX_RETRY_COUNT) where T : UnityEngine.Object
        {
            if (TryRefineResourcesPath(assetName, out int index))
            {
                await m_GroupResource.PreloadAssetAsync<T>(groupId, new string[] { assetName.Substring(index) }, progression, maxRetryCount);
            }
            else
            {
                await m_GroupBundle.PreloadAssetAsync<T>(groupId, packageName, new string[] { assetName }, priority, progression, maxRetryCount);
            }
        }

        public  async UniTask PreloadAssetAsync<T>(int groupId, string[] assetNames, uint priority = 0, Progression progression = null, byte maxRetryCount = MAX_RETRY_COUNT) where T : UnityEngine.Object
        {
            var packageName = m_CurrentPackageName;
            await PreloadAssetAsync<T>(groupId, packageName, assetNames, priority, progression, maxRetryCount);
        }

        public  async UniTask PreloadAssetAsync<T>(int groupId, string packageName, string[] assetNames, uint priority = 0, Progression progression = null, byte maxRetryCount = MAX_RETRY_COUNT) where T : UnityEngine.Object
        {
            int length = assetNames.Length;
            string[] rAssetNames = new string[length];
            string[] bAssetNames = new string[length];
            int rCount = 0;
            int bCount = 0;

            for (int i = 0; i < length; i++)
            {
                if (string.IsNullOrEmpty(assetNames[i]))
                {
                    continue;
                }

                if (TryRefineResourcesPath(assetNames[i], out int index))
                {
                    rAssetNames[rCount++] = assetNames[i].Substring(index);
                }
                else
                {
                    bAssetNames[bCount++] = assetNames[i];
                }
            }

            if (rCount > 0 && bCount > 0)
            {
                await UniTask.WhenAll
                (
                    m_GroupResource.PreloadAssetAsync<T>(groupId, rAssetNames, progression, maxRetryCount),
                    m_GroupBundle.PreloadAssetAsync<T>(groupId, packageName, bAssetNames, priority, progression, maxRetryCount)
                );
            }
            else if (rCount > 0)
            {
                await m_GroupResource.PreloadAssetAsync<T>(groupId, rAssetNames, progression, maxRetryCount);
            }
            else if (bCount > 0)
            {
                await m_GroupBundle.PreloadAssetAsync<T>(groupId, packageName, bAssetNames, priority, progression, maxRetryCount);
            }
        }

        /// <summary>
        /// If use prefix "res#" will load from resources else will load from bundle
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="groupId"></param>
        /// <param name="assetName"></param>
        /// <param name="progression"></param>
        /// <param name="maxRetryCount"></param>
        public  void PreloadAsset<T>(int groupId, string assetName, Progression progression = null, byte maxRetryCount = MAX_RETRY_COUNT) where T : UnityEngine.Object
        {
            var packageName = m_CurrentPackageName;
            if (TryRefineResourcesPath(assetName, out int index))
            {
                m_GroupResource.PreloadAsset<T>(groupId, new string[] { assetName.Substring(index) }, progression, maxRetryCount);
            }
            else
            {
                m_GroupBundle.PreloadAsset<T>(groupId, packageName, new string[] { assetName }, progression, maxRetryCount);
            }
        }

        public  void PreloadAsset<T>(int groupId, string packageName, string assetName, Progression progression = null, byte maxRetryCount = MAX_RETRY_COUNT) where T : UnityEngine.Object
        {
            if (TryRefineResourcesPath(assetName, out int index))
            {
                m_GroupResource.PreloadAsset<T>(groupId, new string[] { assetName.Substring(index) }, progression, maxRetryCount);
            }
            else
            {
                m_GroupBundle.PreloadAsset<T>(groupId, packageName, new string[] { assetName }, progression, maxRetryCount);
            }
        }

        public  void PreloadAsset<T>(int groupId, string[] assetNames, Progression progression = null, byte maxRetryCount = MAX_RETRY_COUNT) where T : UnityEngine.Object
        {
            var packageName = m_CurrentPackageName;
            PreloadAsset<T>(groupId, packageName, assetNames, progression, maxRetryCount);
        }

        public  void PreloadAsset<T>(int groupId, string packageName, string[] assetNames, Progression progression = null, byte maxRetryCount = MAX_RETRY_COUNT) where T : UnityEngine.Object
        {
            int length = assetNames.Length;
            string[] rAssetNames = new string[length];
            string[] bAssetNames = new string[length];
            int rCount = 0;
            int bCount = 0;

            for (int i = 0; i < length; i++)
            {
                if (string.IsNullOrEmpty(assetNames[i]))
                {
                    continue;
                }

                if (TryRefineResourcesPath(assetNames[i], out int index))
                {
                    rAssetNames[rCount++] = assetNames[i].Substring(index);
                }
                else
                {
                    bAssetNames[bCount++] = assetNames[i];
                }
            }

            if (rCount > 0)
            {
                m_GroupResource.PreloadAsset<T>(groupId, rAssetNames, progression, maxRetryCount);
            }

            if (bCount > 0)
            {
                m_GroupBundle.PreloadAsset<T>(groupId, packageName, bAssetNames, progression, maxRetryCount);
            }
        }

        /// <summary>
        /// If use prefix "res#" will load from resources else will load from bundle
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="groupId"></param>
        /// <param name="assetName"></param>
        /// <param name="priority"></param>
        /// <param name="progression"></param>
        /// <param name="maxRetryCount"></param>
        /// <returns></returns>
        public  async UniTask<T> LoadAssetAsync<T>(int groupId, string assetName, uint priority = 0, Progression progression = null, byte maxRetryCount = MAX_RETRY_COUNT) where T : UnityEngine.Object
        {
            var packageName = m_CurrentPackageName;
            if (TryRefineResourcesPath(assetName, out int index))
            {
                return await m_GroupResource.LoadAssetAsync<T>(groupId, assetName.Substring(index), progression, maxRetryCount);
            }
            else
            {
                return await m_GroupBundle.LoadAssetAsync<T>(groupId, packageName, assetName, priority, progression, maxRetryCount);
            }
        }

        public  async UniTask<T> LoadAssetAsync<T>(int groupId, string packageName, string assetName, uint priority = 0, Progression progression = null, byte maxRetryCount = MAX_RETRY_COUNT) where T : UnityEngine.Object
        {
            if (TryRefineResourcesPath(assetName, out int index))
            {
                return await m_GroupResource.LoadAssetAsync<T>(groupId, assetName.Substring(index), progression, maxRetryCount);
            }
            else
            {
                return await m_GroupBundle.LoadAssetAsync<T>(groupId, packageName, assetName, priority, progression, maxRetryCount);
            }
        }

        /// <summary>
        /// If use prefix "res#" will load from resources else will load from bundle
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="groupId"></param>
        /// <param name="assetName"></param>
        /// <param name="progression"></param>
        /// <returns></returns>
        public  T LoadAsset<T>(int groupId, string assetName, Progression progression = null, byte maxRetryCount = MAX_RETRY_COUNT) where T : UnityEngine.Object
        {
            var packageName = m_CurrentPackageName;
            if (TryRefineResourcesPath(assetName, out int index))
            {
                return m_GroupResource.LoadAsset<T>(groupId, assetName.Substring(index), progression, maxRetryCount);
            }
            else
            {
                return m_GroupBundle.LoadAsset<T>(groupId, packageName, assetName, progression, maxRetryCount);
            }
        }

        public  T LoadAsset<T>(int groupId, string packageName, string assetName, Progression progression = null, byte maxRetryCount = MAX_RETRY_COUNT) where T : UnityEngine.Object
        {
            if (TryRefineResourcesPath(assetName, out int index))
            {
                return m_GroupResource.LoadAsset<T>(groupId, assetName.Substring(index), progression, maxRetryCount);
            }
            else
            {
                return m_GroupBundle.LoadAsset<T>(groupId, packageName, assetName, progression, maxRetryCount);
            }
        }

        /// <summary>
        /// If use prefix "res#" will load from resources else will load from bundle
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="groupId"></param>
        /// <param name="assetName"></param>
        /// <param name="priority"></param>
        /// <param name="progression"></param>
        /// <param name="maxRetryCount"></param>
        /// <returns></returns>
        public  async UniTask<T> InstantiateAssetAsync<T>(int groupId, string assetName, uint priority = 0, Progression progression = null, byte maxRetryCount = MAX_RETRY_COUNT) where T : UnityEngine.Object
        {
            var packageName = m_CurrentPackageName;
            if (TryRefineResourcesPath(assetName, out int index))
            {
                var asset = await m_GroupResource.LoadAssetAsync<T>(groupId, assetName.Substring(index), progression, maxRetryCount);
                var cloneAsset = (asset == null) ? null : UnityEngine.Object.Instantiate(asset);
                return cloneAsset;
            }
            else
            {
                var asset = await m_GroupBundle.LoadAssetAsync<T>(groupId, packageName, assetName, priority, progression, maxRetryCount);
                var cloneAsset = (asset == null) ? null : UnityEngine.Object.Instantiate(asset);
                return cloneAsset;
            }
        }

        public  async UniTask<T> InstantiateAssetAsync<T>(int groupId, string packageName, string assetName, uint priority = 0, Progression progression = null, byte maxRetryCount = MAX_RETRY_COUNT) where T : UnityEngine.Object
        {
            if (TryRefineResourcesPath(assetName, out int index))
            {
                var asset = await m_GroupResource.LoadAssetAsync<T>(groupId, assetName.Substring(index), progression, maxRetryCount);
                var cloneAsset = (asset == null) ? null : UnityEngine.Object.Instantiate(asset);
                return cloneAsset;
            }
            else
            {
                var asset = await m_GroupBundle.LoadAssetAsync<T>(groupId, packageName, assetName, priority, progression, maxRetryCount);
                var cloneAsset = (asset == null) ? null : UnityEngine.Object.Instantiate(asset);
                return cloneAsset;
            }
        }

        public  async UniTask<T> InstantiateAssetAsync<T>(int groupId, string assetName, Vector3 position, Quaternion rotation, uint priority = 0, Progression progression = null, byte maxRetryCount = MAX_RETRY_COUNT) where T : UnityEngine.Object
        {
            var packageName = m_CurrentPackageName;
            if (TryRefineResourcesPath(assetName, out int index))
            {
                var asset = await m_GroupResource.LoadAssetAsync<T>(groupId, assetName.Substring(index), progression, maxRetryCount);
                var cloneAsset = (asset == null) ? null : UnityEngine.Object.Instantiate(asset, position, rotation);
                return cloneAsset;
            }
            else
            {
                var asset = await m_GroupBundle.LoadAssetAsync<T>(groupId, packageName, assetName, priority, progression, maxRetryCount);
                var cloneAsset = (asset == null) ? null : UnityEngine.Object.Instantiate(asset, position, rotation);
                return cloneAsset;
            }
        }

        public  async UniTask<T> InstantiateAssetAsync<T>(int groupId, string packageName, string assetName, Vector3 position, Quaternion rotation, uint priority = 0, Progression progression = null, byte maxRetryCount = MAX_RETRY_COUNT) where T : UnityEngine.Object
        {
            if (TryRefineResourcesPath(assetName, out int index))
            {
                var asset = await m_GroupResource.LoadAssetAsync<T>(groupId, assetName.Substring(index), progression, maxRetryCount);
                var cloneAsset = (asset == null) ? null : UnityEngine.Object.Instantiate(asset, position, rotation);
                return cloneAsset;
            }
            else
            {
                var asset = await m_GroupBundle.LoadAssetAsync<T>(groupId, packageName, assetName, priority, progression, maxRetryCount);
                var cloneAsset = (asset == null) ? null : UnityEngine.Object.Instantiate(asset, position, rotation);
                return cloneAsset;
            }
        }

        public  async UniTask<T> InstantiateAssetAsync<T>(int groupId, string assetName, Vector3 position, Quaternion rotation, Transform parent, uint priority = 0, Progression progression = null, byte maxRetryCount = MAX_RETRY_COUNT) where T : UnityEngine.Object
        {
            var packageName = m_CurrentPackageName;
            if (TryRefineResourcesPath(assetName, out int index))
            {
                var asset = await m_GroupResource.LoadAssetAsync<T>(groupId, assetName.Substring(index), progression, maxRetryCount);
                var cloneAsset = (asset == null) ? null : UnityEngine.Object.Instantiate(asset, position, rotation, parent);
                return cloneAsset;
            }
            else
            {
                var asset = await m_GroupBundle.LoadAssetAsync<T>(groupId, packageName, assetName, priority, progression, maxRetryCount);
                var cloneAsset = (asset == null) ? null : UnityEngine.Object.Instantiate(asset, position, rotation, parent);
                return cloneAsset;
            }
        }

        public  async UniTask<T> InstantiateAssetAsync<T>(int groupId, string packageName, string assetName, Vector3 position, Quaternion rotation, Transform parent, uint priority = 0, Progression progression = null, byte maxRetryCount = MAX_RETRY_COUNT) where T : UnityEngine.Object
        {
            if (TryRefineResourcesPath(assetName, out int index))
            {
                var asset = await m_GroupResource.LoadAssetAsync<T>(groupId, assetName.Substring(index), progression, maxRetryCount);
                var cloneAsset = (asset == null) ? null : UnityEngine.Object.Instantiate(asset, position, rotation, parent);
                return cloneAsset;
            }
            else
            {
                var asset = await m_GroupBundle.LoadAssetAsync<T>(groupId, packageName, assetName, priority, progression, maxRetryCount);
                var cloneAsset = (asset == null) ? null : UnityEngine.Object.Instantiate(asset, position, rotation, parent);
                return cloneAsset;
            }
        }

        public  async UniTask<T> InstantiateAssetAsync<T>(int groupId, string assetName, Transform parent, uint priority = 0, Progression progression = null, byte maxRetryCount = MAX_RETRY_COUNT) where T : UnityEngine.Object
        {
            var packageName = m_CurrentPackageName;
            if (TryRefineResourcesPath(assetName, out int index))
            {
                var asset = await m_GroupResource.LoadAssetAsync<T>(groupId, assetName.Substring(index), progression, maxRetryCount);
                var cloneAsset = (asset == null) ? null : UnityEngine.Object.Instantiate(asset, parent);
                return cloneAsset;
            }
            else
            {
                var asset = await m_GroupBundle.LoadAssetAsync<T>(groupId, packageName, assetName, priority, progression, maxRetryCount);
                var cloneAsset = (asset == null) ? null : UnityEngine.Object.Instantiate(asset, parent);
                return cloneAsset;
            }
        }

        public  async UniTask<T> InstantiateAssetAsync<T>(int groupId, string packageName, string assetName, Transform parent, uint priority = 0, Progression progression = null, byte maxRetryCount = MAX_RETRY_COUNT) where T : UnityEngine.Object
        {
            if (TryRefineResourcesPath(assetName, out int index))
            {
                var asset = await m_GroupResource.LoadAssetAsync<T>(groupId, assetName.Substring(index), progression, maxRetryCount);
                var cloneAsset = (asset == null) ? null : UnityEngine.Object.Instantiate(asset, parent);
                return cloneAsset;
            }
            else
            {
                var asset = await m_GroupBundle.LoadAssetAsync<T>(groupId, packageName, assetName, priority, progression, maxRetryCount);
                var cloneAsset = (asset == null) ? null : UnityEngine.Object.Instantiate(asset, parent);
                return cloneAsset;
            }
        }

        public  async UniTask<T> InstantiateAssetAsync<T>(int groupId, string assetName, Transform parent, bool worldPositionStays, uint priority = 0, Progression progression = null, byte maxRetryCount = MAX_RETRY_COUNT) where T : UnityEngine.Object
        {
            var packageName = m_CurrentPackageName;
            if (TryRefineResourcesPath(assetName, out int index))
            {
                var asset = await m_GroupResource.LoadAssetAsync<T>(groupId, assetName.Substring(index), progression, maxRetryCount);
                var cloneAsset = (asset == null) ? null : UnityEngine.Object.Instantiate(asset, parent, worldPositionStays);
                return cloneAsset;
            }
            else
            {
                var asset = await m_GroupBundle.LoadAssetAsync<T>(groupId, packageName, assetName, priority, progression, maxRetryCount);
                var cloneAsset = (asset == null) ? null : UnityEngine.Object.Instantiate(asset, parent, worldPositionStays);
                return cloneAsset;
            }
        }

        public  async UniTask<T> InstantiateAssetAsync<T>(int groupId, string packageName, string assetName, Transform parent, bool worldPositionStays, uint priority = 0, Progression progression = null, byte maxRetryCount = MAX_RETRY_COUNT) where T : UnityEngine.Object
        {
            if (TryRefineResourcesPath(assetName, out int index))
            {
                var asset = await m_GroupResource.LoadAssetAsync<T>(groupId, assetName.Substring(index), progression, maxRetryCount);
                var cloneAsset = (asset == null) ? null : UnityEngine.Object.Instantiate(asset, parent, worldPositionStays);
                return cloneAsset;
            }
            else
            {
                var asset = await m_GroupBundle.LoadAssetAsync<T>(groupId, packageName, assetName, priority, progression, maxRetryCount);
                var cloneAsset = (asset == null) ? null : UnityEngine.Object.Instantiate(asset, parent, worldPositionStays);
                return cloneAsset;
            }
        }

        /// <summary>
        /// If use prefix "res#" will load from resources else will load from bundle
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="groupId"></param>
        /// <param name="assetName"></param>
        /// <param name="progression"></param>
        /// <param name="maxRetryCount"></param>
        /// <returns></returns>
        public  T InstantiateAsset<T>(int groupId, string assetName, Progression progression = null, byte maxRetryCount = MAX_RETRY_COUNT) where T : UnityEngine.Object
        {
            var packageName = m_CurrentPackageName;
            if (TryRefineResourcesPath(assetName, out int index))
            {
                var asset = m_GroupResource.LoadAsset<T>(groupId, assetName.Substring(index), progression, maxRetryCount);
                var cloneAsset = (asset == null) ? null : UnityEngine.Object.Instantiate(asset);
                return cloneAsset;
            }
            else
            {
                var asset = m_GroupBundle.LoadAsset<T>(groupId, packageName, assetName, progression, maxRetryCount);
                var cloneAsset = (asset == null) ? null : UnityEngine.Object.Instantiate(asset);
                return cloneAsset;
            }
        }

        public  T InstantiateAsset<T>(int groupId, string packageName, string assetName, Progression progression = null, byte maxRetryCount = MAX_RETRY_COUNT) where T : UnityEngine.Object
        {
            if (TryRefineResourcesPath(assetName, out int index))
            {
                var asset = m_GroupResource.LoadAsset<T>(groupId, assetName.Substring(index), progression, maxRetryCount);
                var cloneAsset = (asset == null) ? null : UnityEngine.Object.Instantiate(asset);
                return cloneAsset;
            }
            else
            {
                var asset = m_GroupBundle.LoadAsset<T>(groupId, packageName, assetName, progression, maxRetryCount);
                var cloneAsset = (asset == null) ? null : UnityEngine.Object.Instantiate(asset);
                return cloneAsset;
            }
        }

        public  T InstantiateAsset<T>(int groupId, string assetName, Vector3 position, Quaternion rotation, Progression progression = null, byte maxRetryCount = MAX_RETRY_COUNT) where T : UnityEngine.Object
        {
            var packageName = m_CurrentPackageName;
            if (TryRefineResourcesPath(assetName, out int index))
            {
                var asset = m_GroupResource.LoadAsset<T>(groupId, assetName.Substring(index), progression, maxRetryCount);
                var cloneAsset = (asset == null) ? null : UnityEngine.Object.Instantiate(asset, position, rotation);
                return cloneAsset;
            }
            else
            {
                var asset = m_GroupBundle.LoadAsset<T>(groupId, packageName, assetName, progression, maxRetryCount);
                var cloneAsset = (asset == null) ? null : UnityEngine.Object.Instantiate(asset, position, rotation);
                return cloneAsset;
            }
        }

        public  T InstantiateAsset<T>(int groupId, string packageName, string assetName, Vector3 position, Quaternion rotation, Progression progression = null, byte maxRetryCount = MAX_RETRY_COUNT) where T : UnityEngine.Object
        {
            if (TryRefineResourcesPath(assetName, out int index))
            {
                var asset = m_GroupResource.LoadAsset<T>(groupId, assetName.Substring(index), progression, maxRetryCount);
                var cloneAsset = (asset == null) ? null : UnityEngine.Object.Instantiate(asset, position, rotation);
                return cloneAsset;
            }
            else
            {
                var asset = m_GroupBundle.LoadAsset<T>(groupId, packageName, assetName, progression, maxRetryCount);
                var cloneAsset = (asset == null) ? null : UnityEngine.Object.Instantiate(asset, position, rotation);
                return cloneAsset;
            }
        }

        public  T InstantiateAsset<T>(int groupId, string assetName, Vector3 position, Quaternion rotation, Transform parent, Progression progression = null, byte maxRetryCount = MAX_RETRY_COUNT) where T : UnityEngine.Object
        {
            var packageName = m_CurrentPackageName;
            if (TryRefineResourcesPath(assetName, out int index))
            {
                var asset = m_GroupResource.LoadAsset<T>(groupId, assetName.Substring(index), progression, maxRetryCount);
                var cloneAsset = (asset == null) ? null : UnityEngine.Object.Instantiate(asset, position, rotation, parent);
                return cloneAsset;
            }
            else
            {
                var asset = m_GroupBundle.LoadAsset<T>(groupId, packageName, assetName, progression, maxRetryCount);
                var cloneAsset = (asset == null) ? null : UnityEngine.Object.Instantiate(asset, position, rotation, parent);
                return cloneAsset;
            }
        }

        public  T InstantiateAsset<T>(int groupId, string packageName, string assetName, Vector3 position, Quaternion rotation, Transform parent, Progression progression = null, byte maxRetryCount = MAX_RETRY_COUNT) where T : UnityEngine.Object
        {
            if (TryRefineResourcesPath(assetName, out int index))
            {
                var asset = m_GroupResource.LoadAsset<T>(groupId, assetName.Substring(index), progression, maxRetryCount);
                var cloneAsset = (asset == null) ? null : UnityEngine.Object.Instantiate(asset, position, rotation, parent);
                return cloneAsset;
            }
            else
            {
                var asset = m_GroupBundle.LoadAsset<T>(groupId, packageName, assetName, progression, maxRetryCount);
                var cloneAsset = (asset == null) ? null : UnityEngine.Object.Instantiate(asset, position, rotation, parent);
                return cloneAsset;
            }
        }

        public  T InstantiateAsset<T>(int groupId, string assetName, Transform parent, Progression progression = null, byte maxRetryCount = MAX_RETRY_COUNT) where T : UnityEngine.Object
        {
            var packageName = m_CurrentPackageName;
            if (TryRefineResourcesPath(assetName, out int index))
            {
                var asset = m_GroupResource.LoadAsset<T>(groupId, assetName.Substring(index), progression, maxRetryCount);
                var cloneAsset = (asset == null) ? null : UnityEngine.Object.Instantiate(asset, parent);
                return cloneAsset;
            }
            else
            {
                var asset = m_GroupBundle.LoadAsset<T>(groupId, packageName, assetName, progression, maxRetryCount);
                var cloneAsset = (asset == null) ? null : UnityEngine.Object.Instantiate(asset, parent);
                return cloneAsset;
            }
        }

        public  T InstantiateAsset<T>(int groupId, string packageName, string assetName, Transform parent, Progression progression = null, byte maxRetryCount = MAX_RETRY_COUNT) where T : UnityEngine.Object
        {
            if (TryRefineResourcesPath(assetName, out int index))
            {
                var asset = m_GroupResource.LoadAsset<T>(groupId, assetName.Substring(index), progression, maxRetryCount);
                var cloneAsset = (asset == null) ? null : UnityEngine.Object.Instantiate(asset, parent);
                return cloneAsset;
            }
            else
            {
                var asset = m_GroupBundle.LoadAsset<T>(groupId, packageName, assetName, progression, maxRetryCount);
                var cloneAsset = (asset == null) ? null : UnityEngine.Object.Instantiate(asset, parent);
                return cloneAsset;
            }
        }

        public  T InstantiateAsset<T>(int groupId, string assetName, Transform parent, bool worldPositionStays, Progression progression = null, byte maxRetryCount = MAX_RETRY_COUNT) where T : UnityEngine.Object
        {
            var packageName = m_CurrentPackageName;
            if (TryRefineResourcesPath(assetName, out int index))
            {
                var asset = m_GroupResource.LoadAsset<T>(groupId, assetName.Substring(index), progression, maxRetryCount);
                var cloneAsset = (asset == null) ? null : UnityEngine.Object.Instantiate(asset, parent, worldPositionStays);
                return cloneAsset;
            }
            else
            {
                var asset = m_GroupBundle.LoadAsset<T>(groupId, packageName, assetName, progression, maxRetryCount);
                var cloneAsset = (asset == null) ? null : UnityEngine.Object.Instantiate(asset, parent, worldPositionStays);
                return cloneAsset;
            }
        }

        public  T InstantiateAsset<T>(int groupId, string packageName, string assetName, Transform parent, bool worldPositionStays, Progression progression = null, byte maxRetryCount = MAX_RETRY_COUNT) where T : UnityEngine.Object
        {
            if (TryRefineResourcesPath(assetName, out int index))
            {
                var asset = m_GroupResource.LoadAsset<T>(groupId, assetName.Substring(index), progression, maxRetryCount);
                var cloneAsset = (asset == null) ? null : UnityEngine.Object.Instantiate(asset, parent, worldPositionStays);
                return cloneAsset;
            }
            else
            {
                var asset = m_GroupBundle.LoadAsset<T>(groupId, packageName, assetName, progression, maxRetryCount);
                var cloneAsset = (asset == null) ? null : UnityEngine.Object.Instantiate(asset, parent, worldPositionStays);
                return cloneAsset;
            }
        }

        public  void UnloadAsset(int groupId, string assetName)
        {
            if (TryRefineResourcesPath(assetName, out int index))
            {
                m_GroupResource.UnloadAsset(groupId, assetName.Substring(index));
            }
            else if (!m_IsReleased)
            {
                m_GroupBundle.UnloadAsset(groupId, assetName);
            }
        }

        public  void UnloadAssets(int groupId, LoadType loadType = LoadType.Any)
        {
            if (loadType == LoadType.Any)
            {
                m_GroupResource.UnloadAssets(groupId);
                if (!m_IsReleased)
                    m_GroupBundle.UnloadAssets(groupId);
            }
            else if (loadType == LoadType.Resources) m_GroupResource.UnloadAssets(groupId);
            else if (loadType == LoadType.Bundle && !m_IsReleased) m_GroupBundle.UnloadAssets(groupId);
        }

        [System.Obsolete("Use UnloadAssets instead")]
        public  void ReleaseResourceAssets(int groupId)
        {
            m_GroupResource.UnloadAssets(groupId);
        }

        [System.Obsolete("Use UnloadAssets instead")]
        public  void ReleaseBundleAssets(int groupId)
        {
            if (!m_IsReleased)
                m_GroupBundle.UnloadAssets(groupId);
        }
        #endregion
        #endregion

        /// <summary>
        /// 解析區分 Resources 或 Bundle 加載名稱規則
        /// </summary>
        /// <param name="assetName"></param>
        /// <returns></returns>
        internal  bool TryRefineResourcesPath(string assetName, out int actualNameIndex)
        {
            if (string.IsNullOrEmpty(assetName))
            {
                actualNameIndex = 0;
                return false;
            }

            const string prefix = "res#";
            if (assetName.AsSpan().StartsWith(prefix.AsSpan()))
            {
                actualNameIndex = prefix.Length;
                return true;
            }

            actualNameIndex = 0;
            return false;
        }
    }
}