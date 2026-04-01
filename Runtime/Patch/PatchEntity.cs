using System.Collections.Generic;
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
    
    public class PatchEntity : MonoBehaviour
    {
        [SerializeField] private PlayMode m_PlayMode;
        [SerializeField, Header("AppPackagesName")]
        internal List<string> packagesNameList;

        internal IRemoteServices m_RemoteServices;
        internal IYooAssetInitOperation m_InitOperation;
        internal IYooAssetRequestPackageVersionOperation m_RequestPackageVersionOperation;
        internal IYooAssetUpdatePackageManifestOperation m_UpdatePackageManifestOperation;
        internal IYooAssetCreateDownloaderOperation m_CreateDownloaderOperation;
        internal IYooAssetsDownloadFileOverOperation m_DownloadFileOverOperation;
        internal IYooAssetClearCacheBundleOperation m_ClearCacheBundleOperation;
        internal Dictionary<string, PackageInfo> m_PackageInfoDic;
        internal Dictionary<string, ResourceDownloaderOperation> m_Downloaders;
        internal OnPatchSuccess m_OnPatchSuccess;
        internal OnPatchFail m_OnPatchFail;
        
        private Fsm<PatchEntity> m_Fsm;

        private void Awake()
        {
            m_PackageInfoDic = new Dictionary<string, PackageInfo>();
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

        public void SetYooAssetInitOperation(IYooAssetInitOperation initOperation)
        {
            m_InitOperation = initOperation;
        }

        public void SetYooAssetRequestPackageVersionOperation(
            IYooAssetRequestPackageVersionOperation requestPackageVersionOperation)
        {
            m_RequestPackageVersionOperation = requestPackageVersionOperation;
        }

        public void SetYooAssetUpdatePackageManifestOperation(
            IYooAssetUpdatePackageManifestOperation updatePackageManifestOperation)
        {
            m_UpdatePackageManifestOperation = updatePackageManifestOperation;
        }

        public void SetYooAssetCreateDownloaderOperation(IYooAssetCreateDownloaderOperation createDownloaderOperation)
        {
            m_CreateDownloaderOperation = createDownloaderOperation;
        }

        public void SetYooAssetDownloadFileOverOperation(IYooAssetsDownloadFileOverOperation downloadFileOverOperation)
        {
            m_DownloadFileOverOperation = downloadFileOverOperation;
        }

        public void SetYooAssetClearCacheBundleOperation(IYooAssetClearCacheBundleOperation clearCacheBundleOperation)
        {
            m_ClearCacheBundleOperation = clearCacheBundleOperation;
        }

        public void StartPatch(OnPatchSuccess success, OnPatchFail fail, IRemoteServices remoteServices = null)
        {
            m_OnPatchSuccess += success;
            m_OnPatchFail += fail;
            m_RemoteServices = remoteServices;
            
            switch (m_PlayMode)
            {
                case PlayMode.EditorSimulateMode:
                    SetYooAssetInitOperation(new EditorInitOperation());
                    SetYooAssetRequestPackageVersionOperation(new DefaultRequestPackageVersionOperation());
                    SetYooAssetUpdatePackageManifestOperation(new DefaultUpdatePackageManifestOperation());
                    SetYooAssetCreateDownloaderOperation(new DefaultCreateDownloaderOperation());
                    SetYooAssetDownloadFileOverOperation(new DefaultDownloadFileOverOperation());
                    SetYooAssetClearCacheBundleOperation(new DefaultClearCacheBundleOperation());
                    break;

                case PlayMode.OfflinePlayMode:
                    SetYooAssetInitOperation(new OfflineInitOperation());
                    SetYooAssetRequestPackageVersionOperation(new DefaultRequestPackageVersionOperation());
                    SetYooAssetUpdatePackageManifestOperation(new DefaultUpdatePackageManifestOperation());
                    SetYooAssetCreateDownloaderOperation(new DefaultCreateDownloaderOperation());
                    SetYooAssetDownloadFileOverOperation(new DefaultDownloadFileOverOperation());
                    SetYooAssetClearCacheBundleOperation(new DefaultClearCacheBundleOperation());
                    break;

                case PlayMode.HostPlayMode:
                    SetYooAssetInitOperation(new HostPlayInitOperation());
                    SetYooAssetRequestPackageVersionOperation(new DefaultRequestPackageVersionOperation());
                    SetYooAssetUpdatePackageManifestOperation(new DefaultUpdatePackageManifestOperation());
                    SetYooAssetCreateDownloaderOperation(new DefaultCreateDownloaderOperation());
                    SetYooAssetDownloadFileOverOperation(new DefaultDownloadFileOverOperation());
                    SetYooAssetClearCacheBundleOperation(new DefaultClearCacheBundleOperation());
                    break;

                case PlayMode.WeakOnlinePlayMode:
                    SetYooAssetInitOperation(new WeakOnlineInitOperation());
                    SetYooAssetRequestPackageVersionOperation(new WeakOnlineRequestPackageVersionOperation());
                    SetYooAssetUpdatePackageManifestOperation(new DefaultUpdatePackageManifestOperation());
                    SetYooAssetCreateDownloaderOperation(new DefaultCreateDownloaderOperation());
                    SetYooAssetDownloadFileOverOperation(new DefaultDownloadFileOverOperation());
                    SetYooAssetClearCacheBundleOperation(new DefaultClearCacheBundleOperation());
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