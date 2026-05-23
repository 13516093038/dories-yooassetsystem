using System.Collections.Generic;
using Dories.Componentization.Runtime;
using Dories.Componentization.Runtime.Utils;
using Dories.YooassetSystem.Runtime.Patch.Operations.ClearCacheBundleOperation;
using Dories.YooassetSystem.Runtime.Patch.Operations.CreateDownloaderOperation;
using Dories.YooassetSystem.Runtime.Patch.Operations.InitPackageOperation;
using Dories.YooassetSystem.Runtime.Patch.Operations.RequestPackageVersionOperation;
using Dories.YooassetSystem.Runtime.Patch.Operations.UpdatePackageManifestOperation;
using Dories.YooassetSystem.Runtime.Patch.Operations.DownloadFileOverOperation;
using Dories.YooassetSystem.Runtime.Patch.States;
using Dories.Fsm.Runtime;
using UnityEngine;
using YooAsset;

namespace Dories.YooassetSystem.Runtime.Patch
{
    public delegate void OnPatchSuccess();
    public delegate void OnPatchFail(string errorMsg);

    public class PackageInfo
    {
        [SerializeField] private string _packageName;
        [SerializeField] private PlayMode _playMode;

        private bool _isSupportWeakOnline;
        private IRemoteService _remoteService;
        private IBundleDecryptor _bundleDecryptor;
    }
    
    public class PatchEntity : EntityMono
    {
        [SerializeField] private bool _isEditorMode;

        [SerializeField] private PlayMode _PlayMode;
        [SerializeField, Header("AppPackagesInfo")]
        internal List<PackageInfo> _packagesInfoList;

        internal IRemoteService _remoteService;
        internal IBundleDecryptor _decryptionService;
     
        internal Dictionary<string, ResourceDownloaderOperation> m_Downloaders;
        internal OnPatchSuccess m_OnPatchSuccess;
        internal OnPatchFail m_OnPatchFail;
        
        private Fsm<PatchEntity> m_Fsm;

        private void Awake()
        {
            m_Fsm = ComponentFactory.GetOrAddSingletonComponent<FsmEntity>().CreateFsm(this);
            m_Fsm.AddState(new YooAssetInitState());
            m_Fsm.AddState(new YooAssetRequestPackageVersionState());
            m_Fsm.AddState(new YooAssetUpdatePackageManifestState());
            m_Fsm.AddState(new YooAssetCreateDownloaderState());
            m_Fsm.AddState(new YooAssetDownloadPackageFilesState());
            m_Fsm.AddState(new YooAssetDownloadFileOverState());
            m_Fsm.AddState(new YooAssetClearCacheBundleState());

            m_OnPatchSuccess += Clear;
        }

        public void StartPatch(OnPatchSuccess success, OnPatchFail fail, IRemoteServices remoteServices = null, IDecryptionServices decryptionServices = null)
        {
            m_OnPatchSuccess += success;
            m_OnPatchFail += fail;
            m_RemoteServices = remoteServices;
            m_DecryptionServices = decryptionServices;

            switch (m_PlayMode)
            {
                case PlayMode.EditorSimulateMode:
                    SetYooAssetInitOperation(m_InitOperation ?? new EditorInitOperation());
                    SetYooAssetRequestPackageVersionOperation(m_RequestPackageVersionOperation ?? new DefaultRequestPackageVersionOperation());
                    SetYooAssetUpdatePackageManifestOperation(m_UpdatePackageManifestOperation ?? new DefaultUpdatePackageManifestOperation());
                    SetYooAssetCreateDownloaderOperation(m_CreateDownloaderOperation ?? new DefaultCreateDownloaderOperation());
                    SetYooAssetDownloadFileOverOperation(m_DownloadFileOverOperation ?? new DefaultDownloadFileOverOperation());
                    SetYooAssetClearCacheBundleOperation(m_ClearCacheBundleOperation ?? new DefaultClearCacheBundleOperation());
                    break;

                case PlayMode.OfflinePlayMode:
                    SetYooAssetInitOperation(m_InitOperation ?? new OfflineInitOperation());
                    SetYooAssetRequestPackageVersionOperation(m_RequestPackageVersionOperation ?? new DefaultRequestPackageVersionOperation());
                    SetYooAssetUpdatePackageManifestOperation(m_UpdatePackageManifestOperation ?? new DefaultUpdatePackageManifestOperation());
                    SetYooAssetCreateDownloaderOperation(m_CreateDownloaderOperation ?? new DefaultCreateDownloaderOperation());
                    SetYooAssetDownloadFileOverOperation(m_DownloadFileOverOperation ?? new DefaultDownloadFileOverOperation());
                    SetYooAssetClearCacheBundleOperation(m_ClearCacheBundleOperation ?? new DefaultClearCacheBundleOperation());
                    break;

                case PlayMode.HostPlayMode:
                    SetYooAssetInitOperation(m_InitOperation ?? new HostPlayInitOperation());
                    SetYooAssetRequestPackageVersionOperation(m_RequestPackageVersionOperation ?? new DefaultRequestPackageVersionOperation());
                    SetYooAssetUpdatePackageManifestOperation(m_UpdatePackageManifestOperation ?? new DefaultUpdatePackageManifestOperation());
                    SetYooAssetCreateDownloaderOperation(m_CreateDownloaderOperation ?? new DefaultCreateDownloaderOperation());
                    SetYooAssetDownloadFileOverOperation(m_DownloadFileOverOperation ?? new DefaultDownloadFileOverOperation());
                    SetYooAssetClearCacheBundleOperation(m_ClearCacheBundleOperation ?? new DefaultClearCacheBundleOperation());
                    break;

                case PlayMode.WeakOnlinePlayMode:
                    SetYooAssetInitOperation(m_InitOperation ?? new WeakOnlineInitOperation());
                    SetYooAssetRequestPackageVersionOperation(m_RequestPackageVersionOperation ?? new WeakOnlineRequestPackageVersionOperation());
                    SetYooAssetUpdatePackageManifestOperation(m_UpdatePackageManifestOperation ?? new DefaultUpdatePackageManifestOperation());
                    SetYooAssetCreateDownloaderOperation(m_CreateDownloaderOperation ?? new DefaultCreateDownloaderOperation());
                    SetYooAssetDownloadFileOverOperation(m_DownloadFileOverOperation ?? new DefaultDownloadFileOverOperation());
                    SetYooAssetClearCacheBundleOperation(m_ClearCacheBundleOperation ?? new DefaultClearCacheBundleOperation());
                    break;
                
                case PlayMode.TTMiniGameMode:
                    SetYooAssetInitOperation(m_InitOperation ?? new TTMiniGameInitOperation());
                    SetYooAssetRequestPackageVersionOperation(m_RequestPackageVersionOperation ?? new DefaultRequestPackageVersionOperation());
                    SetYooAssetUpdatePackageManifestOperation(m_UpdatePackageManifestOperation ?? new DefaultUpdatePackageManifestOperation());
                    SetYooAssetCreateDownloaderOperation(m_CreateDownloaderOperation ?? new DefaultCreateDownloaderOperation());
                    SetYooAssetDownloadFileOverOperation(m_DownloadFileOverOperation ?? new DefaultDownloadFileOverOperation());
                    SetYooAssetClearCacheBundleOperation(m_ClearCacheBundleOperation ?? new DefaultClearCacheBundleOperation());
                    break;
                
                case PlayMode.WeChatMiniGameMode:
                    break;
            }
            m_Fsm.Start<YooAssetInitState>();
        }

        private void Clear()
        {
            m_PackageInfoDic.Clear();
            ComponentFactory.GetOrAddSingletonComponent<FsmEntity>().DestroyFsm(m_Fsm);
        }
    }
}