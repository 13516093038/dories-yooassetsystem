using Dories.Fsm.Runtime;

namespace Dories.YooassetSystem.Runtime.Patch.States
{
    /// <summary>
    /// 资源文件下载状态
    /// </summary>
    public class YooAssetDownloadPackageFilesState : FsmState<PatchEntity>
    {
        private Fsm<PatchEntity> m_Fsm;

        protected override void OnEnter(Fsm<PatchEntity> fsm)
        {
            base.OnEnter(fsm);
            
            m_Fsm = fsm;
            Owner._patchDowner._allPackageDownloadCompleted = () =>
            {
                ChangeState<YooAssetDownloadFileOverState>(m_Fsm);
            };
        }
    }
}