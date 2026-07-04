
using Dories.YooAssetSystem.Runtime.Patch;
using UnityEngine;
using UnityEngine.UI;
using YooAsset;

public class PatchTest : MonoBehaviour
{
    [SerializeField] private PatchEntity patchEntity;
    [SerializeField] private GameObject PatchPanel;
    [SerializeField] private Button startPatchBtn;
    [SerializeField] private Button pausePatchBtn;
    [SerializeField] private Button resumePatchBtn;
    [SerializeField] private Button cancelPatchBtn;

    [SerializeField] private Text loadedText;

    [SerializeField] private Image progressImage;

    private PatchDownloader _patchDownloader;

    private void Start()
    {
        PatchPanel.SetActive(false);
        StartPatch();
    }

    private void StartPatch()
    {
        patchEntity.BuildNeedUpdateListener(downloader =>
            {
                PatchPanel.SetActive(true);
                _patchDownloader = downloader;
                _patchDownloader.DownloadProgressChangedEventArgs("TestPackage1", args =>
                {
                    loadedText.text = $"{args.CurrentDownloadCount}/{args.TotalDownloadCount}";
                    progressImage.fillAmount = args.Progress;
                });
                downloader.DownloadFileStartedEventArgs("TestPackage1",
                    args => { Debug.Log($"Download file started: {args.FileName}"); });

                _patchDownloader.DownloadProgressChangedEventArgs("TestPackage2", args =>
                {
                    loadedText.text = $"{args.CurrentDownloadCount}/{args.TotalDownloadCount}";
                    progressImage.fillAmount = args.Progress;
                });
                _patchDownloader.DownloadFileStartedEventArgs("TestPackage2",
                    args => { Debug.Log($"Download file started: {args.FileName}"); });

                if (_patchDownloader.NeedDownload)
                {
                    startPatchBtn.onClick.AddListener(() => { _patchDownloader.StartDownload(); });
                    pausePatchBtn.onClick.AddListener(() => { _patchDownloader.PauseDownload(); });
                    resumePatchBtn.onClick.AddListener(() => { _patchDownloader.ResumeDownload(); });
                    cancelPatchBtn.onClick.AddListener(() => { _patchDownloader.CancelDownload(); });
                }
                else
                {
                    loadedText.text = "No need to download";
                    progressImage.fillAmount = 1;
                }
            })
            .BuildPatchCompleteListener(() =>
            {
                Debug.Log("Patch complete");
                LoadCube();
            })
            .BuildPatchFailedListener(error => { Debug.Log("Patch failed: " + error); })
            .BuildPatchErrorListener(error => { Debug.Log("Patch error: " + error); })
            .StartPatch();
    }

    private async void LoadCube()
    {
        var handle = YooAssets.GetPackage("TestPackage1").LoadAssetAsync<GameObject>(
            "Packages/com.dories.yooassetsystem/Patch/Test/Res/Cube.prefab");
        await handle;
        GameObject.Instantiate(handle.AssetObject);
        handle.Release();
    }
}
