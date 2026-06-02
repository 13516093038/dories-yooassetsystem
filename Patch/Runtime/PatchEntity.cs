using System;
using System.Collections.Generic;
using Dories.Componentization.Runtime;
using Dories.Fsm.Runtime;
using UnityEngine;
using YooAsset;


namespace Dories.YooassetSystem.Runtime.Patch
{
    public delegate void OnPatchSuccess();

    public delegate void OnPatchFail(string errorMsg);

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

            public string PackageName => packageName;
            public bool IsSupportWeakOnline => isSupportWeakOnline;
            public string RemoteServiceTypeName => remoteService;
            public string BundleDecryptorTypeName => bundleDecryptor;
            public int Timeout => timeout;
            public string PackageVersion { get; internal set; }

            /// <summary>
            /// 运行时实例，由 Awake 根据类型名创建
            /// </summary>
            public IRemoteService RemoteService { get; internal set; }

            public IBundleDecryptor BundleDecryptor { get; internal set; }
        }

        [SerializeField] internal bool isReleaseMode;
        [SerializeField] internal PlayMode playMode;
        [SerializeField] private string manifestDecryptor;

        [SerializeField, Header("AppPackagesInfo")]
        internal List<PackageInfo> packagesInfoList;

        internal IManifestDecryptor ManifestDecryptor { get; private set; }

        internal Dictionary<string, ResourceDownloaderOperation> m_Downloaders;
        internal OnPatchSuccess m_OnPatchSuccess;
        internal OnPatchFail m_OnPatchFail;

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
            GetComponent<FsmEntity>().DestroyFsm(_fsm);
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