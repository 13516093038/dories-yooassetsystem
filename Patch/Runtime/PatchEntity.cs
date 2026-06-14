using System;
using System.Collections.Generic;
using Dories.Componentization.Runtime;
using Dories.Fsm.Runtime;
using Dories.YooassetSystem.Patch.Runtime.Operations;
using UnityEngine;
using YooAsset;


namespace Dories.YooassetSystem.Runtime.Patch
{
    public class PatchEntity : EntityMono
    {
        [Serializable]
        public class PackageInfo
        {
            [SerializeField] private string packageName;
            [SerializeField] private bool isSupportWeakOnline = false;
            [SerializeField] private string remoteService = string.Empty;
            [SerializeField] private string bundleDecryptor = string.Empty;
            [SerializeField] private int timeout = 60;
            [SerializeField] private int downloadingMaxNum = 10;
            [SerializeField] private int failedTryAgainTimes = 3;
            [SerializeField] private ClearCacheBundleInfo clearCacheBundleInfo;

            public string PackageName => packageName;
            public bool IsSupportWeakOnline => isSupportWeakOnline;
            public string RemoteServiceTypeName => remoteService;
            public string BundleDecryptorTypeName => bundleDecryptor;
            public int Timeout => timeout;
            public string PackageVersion { get; internal set; }
            public int DownloadingMaxNum => downloadingMaxNum;
            public int FailedTryAgain => failedTryAgainTimes;
            public ClearCacheBundleInfo ClearCacheBundleInfo => clearCacheBundleInfo;

            /// <summary>
            /// 运行时实例，由 Awake 根据类型名创建
            /// </summary>
            public IRemoteService RemoteService { get; internal set; }

            public IBundleDecryptor BundleDecryptor { get; internal set; }
        }

        [Serializable]
        public class ClearCacheBundleInfo
        {
            [SerializeField] private ClearCacheOperationMode mode = ClearCacheOperationMode.ClearUnusedBundleFiles;
            [SerializeField] private string[] locations;
            [SerializeField] private string[] tags;

            public ClearCacheOperationMode Mode => mode;
            public string[] Locations => locations;
            public string[] Tags => tags;
        }

        [SerializeField] internal bool isReleaseMode;
        [SerializeField] internal PlayMode playMode;
        [SerializeField] private string manifestDecryptor;

        [SerializeField, Header("AppPackagesInfo")]
        internal List<PackageInfo> packagesInfoList;

        internal IManifestDecryptor ManifestDecryptor { get; private set; }
        internal Dictionary<string, ResourceDownloaderOperation> m_Downloaders;
        internal Action<PatchDownlaoder> _needUpdateListener;
        internal PatchDownlaoder _patchDowner;
        internal Action _patchCompleted;
        internal Action<string> _patchFailed;
        internal Action<string> _patchError;

        private Fsm<PatchEntity> _fsm;

        private void Awake()
        {
            var fsmEntity = AddComponent<FsmEntity>();
            _fsm = fsmEntity.CreateFsm(this);

            ManifestDecryptor = CreateManifestDecryptor(manifestDecryptor);

            foreach (var packageInfo in packagesInfoList)
            {
                packageInfo.RemoteService = CreateRemoteService(packageInfo.RemoteServiceTypeName);
                packageInfo.BundleDecryptor = CreateBundleDecryptor(packageInfo.BundleDecryptorTypeName);
            }
        }

        protected override void OnDestroy()
        {
            GetComponentCSharp<FsmEntity>().DestroyFsm(_fsm);
            base.OnDestroy();
        }

        private IRemoteService CreateRemoteService(string remoteServiceTypeName)
        {
            if (!string.IsNullOrEmpty(remoteServiceTypeName))
            {
                try
                {
                    return Activator.CreateInstance(Type.GetType(remoteServiceTypeName)) as IRemoteService;
                }
                catch (Exception e)
                {
                    Debug.LogError($"CreateRemoteService failed: {e}");
                }
            }

            return null;
        }

        private IBundleDecryptor CreateBundleDecryptor(string bundleDecryptorTypeName)
        {
            if (!string.IsNullOrEmpty(bundleDecryptorTypeName))
            {
                try
                {
                    return Activator.CreateInstance(Type.GetType(bundleDecryptorTypeName)) as IBundleDecryptor;
                }
                catch (Exception e)
                {
                    Debug.LogError($"CreateBundleDecryptor failed: {e}");
                }
            }

            return null;
        }

        private IManifestDecryptor CreateManifestDecryptor(string manifestDecryptorTypeName)
        {
            if (!string.IsNullOrEmpty(manifestDecryptorTypeName))
            {
                try
                {
                    return Activator.CreateInstance(Type.GetType(manifestDecryptorTypeName)) as IManifestDecryptor;
                }
                catch (Exception e)
                {
                    Debug.LogError($"CreateManifestDecryptor failed: {e}");
                }
            }

            return null;
        }
    }
}