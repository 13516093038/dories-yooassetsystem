using Dories.YooassetSystem.Runtime.AssetLoader;
using UnityEngine;
using YooAsset;

public class AssetLoaderSample : MonoBehaviour
{
    private AssetLoaderEntity _assetLoaderEntity;

    private GameObject _asset;

    private RawFileObject _rawFile;

    private void Start()
    {
        _assetLoaderEntity = Object.FindObjectOfType<AssetLoaderEntity>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            LoadAsset();
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            UnloadAsset();
        }

        if (Input.GetKeyDown(KeyCode.D))
        {
            LoadRawFile();
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            UnloadRawFile();
        }
    }

    private async void LoadAsset()
    {
        var asset = await _assetLoaderEntity.LoadAssetAsync<GameObject>("TestPackage1", "Packages/com.dories.yooassetsystem/Sample/Res/Prefabs/Cube.prefab");
        _asset = Instantiate(asset);
    }

    private void UnloadAsset()
    {
        Destroy(_asset);
        _asset = null;
        _assetLoaderEntity.UnloadAsset("TestPackage1", "Packages/com.dories.yooassetsystem/Sample/Res/Prefabs/Cube.prefab");
    }
    
    private async void LoadRawFile()
    {
        RawFileObject rawFile = await _assetLoaderEntity.LoadRawFileAsync("TestPackage2", "Packages/com.dories.yooassetsystem/Sample/Res/RawFile/SampleText.txt");
        _rawFile = rawFile;
        Debug.Log(_rawFile.GetText());
    }

    private void UnloadRawFile()
    {
        _assetLoaderEntity.UnloadRawFile("TestPackage2", "Packages/com.dories.yooassetsystem/Sample/Res/RawFile/SampleText.txt");
        Destroy(_rawFile);
        _rawFile = null;
    }
}
