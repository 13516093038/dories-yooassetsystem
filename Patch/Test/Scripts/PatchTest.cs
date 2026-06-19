using Dories.YooAssetSystem.Runtime.Patch;
using UnityEngine;

public class PatchTest : MonoBehaviour
{
    [SerializeField] private PatchEntity patchEntity;

    void Start()
    {
        patchEntity.BuildNeedUpdateListener(downloader =>
            {
                var (count, bytes) = downloader.GetTotalPendingDownload();
                Debug.Log($"Need update, count: {count}, bytes: {bytes}");

                // 可按需覆盖默认整包下载计划，例如只下指定 Tag：
                // downloader.ClearDownloadPlan("DefaultPackage");
                // downloader.DownloadByTag("DefaultPackage", "Login", 10, 3);

                downloader.StartDownload();
            })
            .BuildPatchCompleteListener(() => { Debug.Log("Patch complete"); })
            .BuildPatchFailedListener(error => { Debug.Log("Patch failed: " + error); })
            .BuildPatchErrorListener(error => { Debug.Log("Patch error: " + error); })
            .StartPatch();
    }
}
