using System;
using System.Collections.Generic;
using Dories.Logging;
using Dories.YooAssetSystem.Runtime.Patch.BuildInFsmSystem;
using Dories.YooAssetSystem.Runtime.Patch.Operations;
using Dories.YooAssetSystem.Runtime.Patch.States;
using UnityEngine;
using YooAsset;


namespace Dories.YooAssetSystem.Runtime.Patch
{
    public class PatchEntity : MonoBehaviour
    {
        [Serializable]
        public class PackageInfo
        {
            [SerializeField] private string packageName;
            [SerializeField] private EditorVirtualType editorVirtualType;
            [SerializeField] private bool isSupportWeakOnline = false;
            [SerializeField] private string remoteService = string.Empty;
            [SerializeField] private string bundleDecryptor = string.Empty;
            [SerializeField] private string manifestDecryptor = string.Empty;
            [SerializeField] private int timeout = 60;
            [SerializeField] private ClearCacheBundleInfo clearCacheBundleInfo = new ClearCacheBundleInfo();
            [SerializeField] private int downloadingMaxNum = 10;
            [SerializeField] private int failedTryAgainTimes = 3;
            [SerializeField] private bool isCombineDownloader;
            [SerializeField] private string[] downloadTags;

            public string PackageName => packageName;
            public EditorVirtualType EditorVirtualType => editorVirtualType;
            public bool IsSupportWeakOnline => isSupportWeakOnline;
            public string RemoteServiceTypeName => remoteService;
            public string BundleDecryptorTypeName => bundleDecryptor;
            public string ManifestDecryptorTypeName => manifestDecryptor;
            public int Timeout => timeout;
            public string PackageVersion { get; internal set; }
            public int DownloadingMaxNum => downloadingMaxNum;
            public int FailedTryAgain => failedTryAgainTimes;
            public bool IsCombineDownloader => isCombineDownloader;
            public ClearCacheBundleInfo ClearCacheBundleInfo => clearCacheBundleInfo;
            public string[] DownloadTags => downloadTags;
            
            public IRemoteService RemoteService { get; internal set; }

            public IBundleDecryptor BundleDecryptor { get; internal set; }

            public IManifestDecryptor ManifestDecryptor { get; internal set; }
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

        [SerializeField] internal bool isAutoDownload = false;
        [SerializeField] internal bool isReleaseMode;
        [SerializeField] internal PlayMode playMode;

        [SerializeField, Header("AppPackagesInfo")]
        internal List<PackageInfo> packagesInfoList;

        /// <summary>
        /// 只读访问已配置的 Package 列表
        /// </summary>
        public IReadOnlyList<PackageInfo> PackagesInfoList => packagesInfoList;

        public bool IsAutoDownload => isAutoDownload;
        
        internal Action<PatchDownloader> _needUpdateListener;
        internal PatchDownloader _patchDowner;
        internal Action _patchCompleted;
        internal Action<string> _patchFailed;
        internal Action<string> _patchError;

        private FsmSystem<PatchEntity> _fsmSystem;
        private bool _isStarted = false;

        private void Awake()
        {
            foreach (var packageInfo in packagesInfoList)
            {
                packageInfo.RemoteService = CreateInstance<IRemoteService>(packageInfo.RemoteServiceTypeName);
                packageInfo.BundleDecryptor = CreateInstance<IBundleDecryptor>(packageInfo.BundleDecryptorTypeName);
                packageInfo.ManifestDecryptor = CreateInstance<IManifestDecryptor>(packageInfo.ManifestDecryptorTypeName);
            }
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

        private static T CreateInstance<T>(string typeName) where T : class
        {
            if (!string.IsNullOrEmpty(typeName))
            {
                try
                {
                    var type = ResolveType(typeName);
                    if (type != null)
                        return Activator.CreateInstance(type) as T;

                    Log.Error("Patch", $"Create {typeof(T).Name} failed: type not found, Type name: {typeName}");
                }
                catch (Exception e)
                {
                    Log.Error("Patch", $"Create {typeof(T).Name} failed: {e}, Type name: {typeName}");
                }
            }

            return null;
        }

        public PatchEntity BuildNeedUpdateListener(Action<PatchDownloader> listener)
        {
            _needUpdateListener = listener;
            return this;
        }

        public PatchEntity BuildPatchCompleteListener(Action listener)
        {
            _patchCompleted = listener;
            return this;
        }

        public PatchEntity BuildPatchFailedListener(Action<string> listener)
        {
            _patchFailed = listener;
            return this;
        }

        public PatchEntity BuildPatchErrorListener(Action<string> listener)
        {
            _patchError = listener;
            return this;
        }
        
        public void StartPatch()
        {
            if (_isStarted)
            {
                Log.Error("Patch", "Patch already started");
                return;
            }

            _isStarted = true;

            _fsmSystem = new FsmSystem<PatchEntity>(this);
            
            _fsmSystem.AddNode(new YooAssetInitState());
            _fsmSystem.AddNode(new YooAssetRequestPackageVersionState());
            _fsmSystem.AddNode(new YooAssetUpdatePackageManifestState());
            _fsmSystem.AddNode(new YooAssetCreateDownloaderState());
            _fsmSystem.AddNode(new YooAssetDownloadPackageFilesState());
            _fsmSystem.AddNode(new YooAssetDownloadFileOverState());
            _fsmSystem.AddNode(new YooAssetClearCacheBundleState());
            
            _fsmSystem.StartFsm<YooAssetInitState>();
        }
    }
}