using System;
using UnityEngine;

namespace Dories.YooassetSystem.Runtime.Patch.Operations.DownloadFileOverOperation
{
    public class DefaultDownloadFileOverOperation : IYooAssetsDownloadFileOverOperation
    {
        public Action DownloadFileOver()
        {
            return DownloadFileOverCallback;
        }

        private void DownloadFileOverCallback()
        {
            Debug.Log("下载完成");
        }
    }
}