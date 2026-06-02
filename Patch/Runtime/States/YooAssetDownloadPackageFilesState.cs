using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Dories.Fsm.Runtime;
using YooAsset;

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
            DownloadPackageFiles().Forget();
        }
        

        private async UniTaskVoid DownloadPackageFiles()
        {
            List<UniTask> downloadTasks = new List<UniTask>();
            foreach (var downloader in Owner.m_Downloaders)
            {
                downloader.Value.BeginDownload();
                downloadTasks.Add(downloader.Value.ToUniTask());
            }
            await UniTask.WhenAll(downloadTasks);

            foreach (var downloadTask in Owner.m_Downloaders)
            {
                if (downloadTask.Value.Status == EOperationStatus.Succeed)
                {
                   
                }
                else
                {
                    Owner.m_OnPatchFail?.Invoke(downloadTask.Value.Error);
                    return;
                }
            }
            //下载成功
            ChangeState<YooAssetDownloadFileOverState>(m_Fsm);
        }
    }
}