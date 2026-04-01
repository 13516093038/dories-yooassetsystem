using Cysharp.Threading.Tasks;
using System.Linq;
using Dories.YooassetSystem.Runtime.AssetLoader.Cacher;
using Dories.Componentization.Runtime.Utils;
using UnityEngine;

namespace Dories.YooassetSystem.Runtime.AssetLoader.GroupCacher
{
    /// <summary>
    /// 軟引用
    /// </summary>
    internal class GroupBundle : GroupCache<BundlePack>
    {
        private CacheBundle m_CacheBundle;

        internal GroupBundle()
        {
            m_CacheBundle = ComponentFactory.GetOrAddSingletonComponent<CacheBundle>();
        }
        
        #region RawFile
        public async UniTask PreloadRawFileAsync(int id, string packageName, string[] assetNames, uint priority, Progression progression, byte maxRetryCount)
        {
            if (assetNames == null || assetNames.Length == 0)
                return;

            await m_CacheBundle.PreloadRawFileAsync(packageName, assetNames, priority, progression, maxRetryCount);
            foreach (string assetName in assetNames)
            {
                if (string.IsNullOrEmpty(assetName))
                    continue;
                if (m_CacheBundle.HasInCache(assetName))
                    this.AddIntoCache(id, assetName);
            }

            Debug.Log($"【Preload RawFile with Group】 => Current << {nameof(GroupBundle)} >> Cache Count: {this.Count}, GroupId: {id}");
        }

        public void PreloadRawFile(int id, string packageName, string[] assetNames, Progression progression, byte maxRetryCount)
        {
            if (assetNames == null || assetNames.Length == 0)
                return;

            m_CacheBundle.PreloadRawFile(packageName, assetNames, progression, maxRetryCount);
            foreach (string assetName in assetNames)
            {
                if (string.IsNullOrEmpty(assetName))
                    continue;
                if (m_CacheBundle.HasInCache(assetName))
                    this.AddIntoCache(id, assetName);
            }

            Debug.Log($"【Preload RawFile with Group】 => Current << {nameof(GroupBundle)} >> Cache Count: {this.Count}, GroupId: {id}");
        }

        public async UniTask<T> LoadRawFileAsync<T>(int id, string packageName, string assetName, uint priority, Progression progression, byte maxRetryCount)
        {
            T asset = default;

            asset = await m_CacheBundle.LoadRawFileAsync<T>(packageName, assetName, priority, progression, maxRetryCount);

            if (asset != null)
            {
                this.AddIntoCache(id, assetName);
                var keyGroup = this.GetFromCache(id, assetName);
                if (keyGroup != null)
                {
                    keyGroup.AddRef();
                    Debug.Log($"【Load RawFile with Group】 => Current << {nameof(GroupBundle)} >> Cache Count: {this.Count}, KeyRef: {keyGroup.refCount}, GroupId: {id}");
                }
            }

            return asset;
        }

        public T LoadRawFile<T>(int id, string packageName, string assetName, Progression progression, byte maxRetryCount)
        {
            T asset = default;

            asset = m_CacheBundle.LoadRawFile<T>(packageName, assetName, progression, maxRetryCount);

            if (asset != null)
            {
                this.AddIntoCache(id, assetName);
                var keyGroup = this.GetFromCache(id, assetName);
                if (keyGroup != null)
                {
                    keyGroup.AddRef();
                    Debug.Log($"【Load RawFile with Group】 => Current << {nameof(GroupBundle)} >> Cache Count: {this.Count}, KeyRef: {keyGroup.refCount}, GroupId: {id}");
                }
            }

            return asset;
        }

        public void UnloadRawFile(int id, string assetName)
        {
            var keyGroup = this.GetFromCache(id, assetName);
            if (keyGroup != null)
            {
                keyGroup.DelRef();
                Debug.Log($"【Unload RawFile from Group】 => Decremented RefCount: {keyGroup.refCount}, Cache Count: {this.Count}, GroupId: {id}");

                // 使用引用計數釋放
                if (keyGroup.refCount <= 0)
                {
                    this.DelFromCache(id, keyGroup.assetName);
                    Debug.Log($"【Unload RawFile from Group Completed】 => RefCount reached 0, removed from cache. Cache Count: {this.Count}, GroupId: {id}");
                }

                // 總是使用引用計數模式
                m_CacheBundle.UnloadRawFile(keyGroup.assetName, false);
            }
        }

        public void UnloadRawFiles(int id)
        {
            if (this._keyCacher.Count > 0)
            {
                foreach (var keyGroup in this._keyCacher.ToArray())
                {
                    if (keyGroup.id != id)
                        continue;

                    // 依照計數次數釋放
                    for (int i = keyGroup.refCount; i > 0; i--)
                    {
                        m_CacheBundle.UnloadRawFile(keyGroup.assetName, false);
                    }

                    // 完成後, 直接刪除緩存
                    this.DelFromCache(keyGroup.id, keyGroup.assetName);
                }

                Debug.Log($"【Unload All RawFiles from Group】 => Released all references, Cache Count: {this.Count}, GroupId: {id}");
            }
        }
        #endregion

        #region Asset
        public async UniTask PreloadAssetAsync<T>(int id, string packageName, string[] assetNames, uint priority, Progression progression, byte maxRetryCount) where T : Object
        {
            if (assetNames == null || assetNames.Length == 0)
                return;

            await m_CacheBundle.PreloadAssetAsync<T>(packageName, assetNames, priority, progression, maxRetryCount);
            foreach (string assetName in assetNames)
            {
                if (string.IsNullOrEmpty(assetName))
                    continue;
                if (m_CacheBundle.HasInCache(assetName))
                    this.AddIntoCache(id, assetName);
            }

            Debug.Log($"【Preload Asset with Group】 => Current << {nameof(GroupBundle)} >> Cache Count: {this.Count}, GroupId: {id}");
        }

        public void PreloadAsset<T>(int id, string packageName, string[] assetNames, Progression progression, byte maxRetryCount) where T : Object
        {
            if (assetNames == null || assetNames.Length == 0)
                return;

            m_CacheBundle.PreloadAsset<T>(packageName, assetNames, progression, maxRetryCount);
            foreach (string assetName in assetNames)
            {
                if (string.IsNullOrEmpty(assetName))
                    continue;
                if (m_CacheBundle.HasInCache(assetName))
                    this.AddIntoCache(id, assetName);
            }

            Debug.Log($"【Preload Asset with Group】 => Current << {nameof(GroupBundle)} >> Cache Count: {this.Count}, GroupId: {id}");
        }

        public async UniTask<T> LoadAssetAsync<T>(int id, string packageName, string assetName, uint priority, Progression progression, byte maxRetryCount) where T : Object
        {
            T asset = null;

            asset = await m_CacheBundle.LoadAssetAsync<T>(packageName, assetName, priority, progression, maxRetryCount);

            if (asset != null)
            {
                this.AddIntoCache(id, assetName);
                var keyGroup = this.GetFromCache(id, assetName);
                if (keyGroup != null)
                {
                    keyGroup.AddRef();
                    Debug.Log($"【Load Asset with Group】 => Current << {nameof(GroupBundle)} >> Cache Count: {this.Count}, KeyRef: {keyGroup.refCount}, GroupId: {id}");
                }
            }

            return asset;
        }

        public T LoadAsset<T>(int id, string packageName, string assetName, Progression progression, byte maxRetryCount) where T : Object
        {
            T asset = null;

            asset = m_CacheBundle.LoadAsset<T>(packageName, assetName, progression, maxRetryCount);

            if (asset != null)
            {
                this.AddIntoCache(id, assetName);
                var keyGroup = this.GetFromCache(id, assetName);
                if (keyGroup != null)
                {
                    keyGroup.AddRef();
                    Debug.Log($"【Load Asset with Group】 => Current << {nameof(GroupBundle)} >> Cache Count: {this.Count}, KeyRef: {keyGroup.refCount}, GroupId: {id}");
                }
            }

            return asset;
        }

        public void UnloadAsset(int id, string assetName)
        {
            var keyGroup = this.GetFromCache(id, assetName);
            if (keyGroup != null)
            {
                keyGroup.DelRef();
                Debug.Log($"【Unload Asset from Group】 => Decremented RefCount: {keyGroup.refCount}, Cache Count: {this.Count}, GroupId: {id}");

                // 使用引用計數釋放
                if (keyGroup.refCount <= 0)
                {
                    this.DelFromCache(id, keyGroup.assetName);
                    Debug.Log($"【Unload Asset from Group Completed】 => RefCount reached 0, removed from cache. Cache Count: {this.Count}, GroupId: {id}");
                }

                // 總是使用引用計數模式
                m_CacheBundle.UnloadAsset(keyGroup.assetName, false);
            }
        }

        public void UnloadAssets(int id)
        {
            if (this._keyCacher.Count > 0)
            {
                foreach (var keyGroup in this._keyCacher.ToArray())
                {
                    if (keyGroup.id != id)
                        continue;

                    // 依照計數次數釋放
                    for (int i = keyGroup.refCount; i > 0; i--)
                    {
                        m_CacheBundle.UnloadAsset(keyGroup.assetName, false);
                    }

                    // 完成後, 直接刪除緩存
                    this.DelFromCache(keyGroup.id, keyGroup.assetName);
                }

                Debug.Log($"【Unload All Assets from Group】 => Released all references, Cache Count: {this.Count}, GroupId: {id}");
            }
        }
        #endregion
    }
}
