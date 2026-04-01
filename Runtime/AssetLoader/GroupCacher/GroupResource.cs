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
    internal class GroupResource : GroupCache<ResourcePack>
    {
        private CacheResource m_CacheResource;

        public GroupResource()
        {
            m_CacheResource = ComponentFactory.GetOrAddSingletonComponent<CacheResource>();
        }
        
        public async UniTask PreloadAssetAsync<T>(int id, string[] assetNames, Progression progression, byte maxRetryCount) where T : Object
        {
            if (assetNames == null || assetNames.Length == 0)
                return;

            await m_CacheResource.PreloadAssetAsync<T>(assetNames, progression, maxRetryCount);
            foreach (string assetName in assetNames)
            {
                if (string.IsNullOrEmpty(assetName))
                    continue;
                if (m_CacheResource.HasInCache(assetName))
                    this.AddIntoCache(id, assetName);
            }

            Debug.Log($"【Preload Asset with Group】 => Current << {nameof(GroupResource)} >> Cache Count: {this.Count}, GroupId: {id}");
        }

        public void PreloadAsset<T>(int id, string[] assetNames, Progression progression, byte maxRetryCount) where T : Object
        {
            if (assetNames == null || assetNames.Length == 0)
                return;

            m_CacheResource.PreloadAsset<T>(assetNames, progression, maxRetryCount);
            foreach (string assetName in assetNames)
            {
                if (string.IsNullOrEmpty(assetName))
                    continue;
                if (m_CacheResource.HasInCache(assetName))
                    this.AddIntoCache(id, assetName);
            }

            Debug.Log($"【Preload Asset with Group】 => Current << {nameof(GroupResource)} >> Cache Count: {this.Count}, GroupId: {id}");
        }

        /// <summary>
        /// 【GroupResource】資源加載
        /// </summary>
        /// <param name="id"></param>
        /// <param name="assetName"></param>
        /// <returns></returns>
        public async UniTask<T> LoadAssetAsync<T>(int id, string assetName, Progression progression, byte maxRetryCount) where T : Object
        {
            T asset = null;

            asset = await m_CacheResource.LoadAssetAsync<T>(assetName, progression, maxRetryCount);

            if (asset != null)
            {
                this.AddIntoCache(id, assetName);
                var keyGroup = this.GetFromCache(id, assetName);
                if (keyGroup != null)
                {
                    keyGroup.AddRef();
                    Debug.Log($"【Load Asset with Group】 => Current << {nameof(GroupResource)} >> Cache Count: {this.Count}, KeyRef: {keyGroup.refCount}, GroupId: {id}");
                }
            }

            return asset;
        }

        public T LoadAsset<T>(int id, string assetName, Progression progression, byte maxRetryCount) where T : Object
        {
            T asset = null;

            asset = m_CacheResource.LoadAsset<T>(assetName, progression, maxRetryCount);

            if (asset != null)
            {
                this.AddIntoCache(id, assetName);
                var keyGroup = this.GetFromCache(id, assetName);
                if (keyGroup != null)
                {
                    keyGroup.AddRef();
                    Debug.Log($"【Load Asset with Group】 => Current << {nameof(GroupResource)} >> Cache Count: {this.Count}, KeyRef: {keyGroup.refCount}, GroupId: {id}");
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
                m_CacheResource.UnloadAsset(keyGroup.assetName, false);
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
                        m_CacheResource.UnloadAsset(keyGroup.assetName, false);
                    }

                    // 完成後, 直接刪除緩存
                    this.DelFromCache(keyGroup.id, keyGroup.assetName);
                }

                Debug.Log($"【Unload All Assets from Group】 => Released all references, Cache Count: {this.Count}, GroupId: {id}");
            }
        }
    }
}
