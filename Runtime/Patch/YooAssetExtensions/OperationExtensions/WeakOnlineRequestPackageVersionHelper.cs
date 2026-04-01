using UnityEngine;
using YooAsset;

namespace Dories.YooassetSystem.Runtime.Patch.YooAssetExtensions.OperationExtensions
{
    public class WeakOnlineRequestPackageVersionHelper : RequestPackageVersionOperation
    {
        private enum ESteps
        {
            None,
            RequestPackageVersion,
            FindLastPackageVersion,
            Done,
        }
        
        private ESteps m_Steps;
        
        private YooAsset.RequestPackageVersionOperation m_VersionOp;

        public WeakOnlineRequestPackageVersionHelper(ResourcePackage package)
        {
            m_VersionOp = package.RequestPackageVersionAsync();
        }

        public new void StartOperation()
        {
            base.StartOperation();
            OperationSystem.StartOperation(m_VersionOp.PackageName, this);
        }

        internal override void InternalStart()
        {
            m_Steps = ESteps.RequestPackageVersion;
            m_VersionOp.StartOperation();
            AddChildOperation(m_VersionOp);
        }

        internal override void InternalUpdate()
        {
            if (m_Steps == ESteps.None || m_Steps == ESteps.Done)
                return;

            if (m_Steps == ESteps.RequestPackageVersion)
            {
                if (m_VersionOp.Status == EOperationStatus.Succeed)
                {
                    PackageVersion  = m_VersionOp.PackageVersion;
                    m_Steps =  ESteps.Done;
                    Status = EOperationStatus.Succeed;
                }
                else if (m_VersionOp.Status == EOperationStatus.Failed)
                {
                    m_Steps = ESteps.FindLastPackageVersion;
                    string version = PlayerPrefs.GetString($"{m_VersionOp.PackageName}_GAME_VERSION", string.Empty);
                    if(string.IsNullOrEmpty(version))
                    {
                        Status = EOperationStatus.Failed;
                        Error = "Can't find last package version";
                    }
                    else
                    {
                        PackageVersion = version;
                        Status = EOperationStatus.Succeed;
                    }
                    m_Steps =  ESteps.Done;
                }
            }
        }
    }
}