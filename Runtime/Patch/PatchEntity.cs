using System;
using System.Collections.Generic;
using Dories.YooAssetSystem.Runtime.LogSystem;
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
            public int Timeout => timeout;
            public string PackageVersion { get; internal set; }
            public int DownloadingMaxNum => downloadingMaxNum;
            public int FailedTryAgain => failedTryAgainTimes;
            public bool IsCombineDownloader => isCombineDownloader;
            public ClearCacheBundleInfo ClearCacheBundleInfo => clearCacheBundleInfo;
            public string[] DownloadTags => downloadTags;
            
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

        [SerializeField] internal string iLog;
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

        private ILog _logger;
        private FsmSystem<PatchEntity> _fsmSystem;
        private bool _isStarted = false;

        private void Awake()
        {
            if(playMode == PlayMode.OfflinePlayMode)
            {
                isAutoDownload = true;
            }

            _logger = CreateLog(iLog);
            
            foreach (var packageInfo in packagesInfoList)
            {
                packageInfo.RemoteService = CreateRemoteService(packageInfo.RemoteServiceTypeName);
                packageInfo.BundleDecryptor = CreateBundleDecryptor(packageInfo.BundleDecryptorTypeName);
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

        private IRemoteService CreateRemoteService(string remoteServiceTypeName)
        {
            if (!string.IsNullOrEmpty(remoteServiceTypeName))
            {
                try
                {
                    var type = ResolveType(remoteServiceTypeName);
                    if (type != null)
                        return Activator.CreateInstance(type) as IRemoteService;

                    _logger.Error($"CreateRemoteService failed: type not found, Type name: {remoteServiceTypeName}");
                }
                catch (Exception e)
                {
                    _logger.Error($"CreateRemoteService failed: {e}, Type name: {remoteServiceTypeName}");
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
                    var type = ResolveType(bundleDecryptorTypeName);
                    if (type != null)
                        return Activator.CreateInstance(type) as IBundleDecryptor;

                    _logger.Error($"CreateBundleDecryptor failed: type not found, Type name: {bundleDecryptorTypeName}");
                }
                catch (Exception e)
                {
                    _logger.Error($"CreateBundleDecryptor failed: {e},  Type name: {bundleDecryptorTypeName}");
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
                _logger.Error("Patch already started");
                return;
            }

            _isStarted = true;

            _fsmSystem = new FsmSystem<PatchEntity>(this, _logger);
            
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