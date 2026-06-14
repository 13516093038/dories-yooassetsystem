using Dories.YooAssetSystem.Runtime.Patch.BuildInFsmSystem;

namespace Dories.YooAssetSystem.Runtime.Patch.States
{
    /// <summary>
    /// 资源文件下载状态
    /// </summary>
    public class YooAssetDownloadPackageFilesState : FsmNodeEntity<PatchEntity>
    {
        protected internal override void OnEnter()
        {
            _owner._patchDowner._allPackageDownloadCompleted = ChangeState<YooAssetDownloadFileOverState>;
        }
    }
}