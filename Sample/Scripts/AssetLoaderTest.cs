using System.Collections;
using Dories.YooAssetSystem.Runtime.Patch;
using Dories.YooassetSystem.Runtime.AssetLoader;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using YooAsset;
#if DORIES_UNITASK_SUPPORT
using Cysharp.Threading.Tasks;
#endif

/// <summary>
/// AssetLoader PlayMode 自动化测试（Unity Test Runner 规范）
/// </summary>
public class AssetLoaderTest
{
    private const string TestSceneName = "Test_Patch";
    private const string PackageName = "TestPackage1";
    private const string CubeLocation = "Packages/com.dories.yooassetsystem/Test/Res/Prefabs/Cube.prefab";
    private const float PatchTimeoutSeconds = 120f;

    private GameObject _spawnedInstance;

    [UnityTest]
    [Timeout(120000)]
    public IEnumerator LoadCube_AfterPatchComplete_ShouldInstantiate()
    {
        yield return SceneManager.LoadSceneAsync(TestSceneName, LoadSceneMode.Single);

        var patchEntity = Object.FindObjectOfType<PatchEntity>();
        var loader = Object.FindObjectOfType<AssetLoaderEntity>();

        Assert.IsNotNull(patchEntity, "场景中缺少 PatchEntity");
        Assert.IsNotNull(loader, "场景中缺少 AssetLoaderEntity");

        yield return new WaitForSeconds(1);

        // Test_Patch 场景中 PatchTest 会在 Start 自动 StartPatch，此处只等待包就绪
        yield return WaitForPackageReady(PackageName, PatchTimeoutSeconds);

        GameObject prefab = null;
#if DORIES_UNITASK_SUPPORT
        yield return loader.LoadAssetAsync<GameObject>(PackageName, CubeLocation)
            .ToCoroutine(result => prefab = result);
#else
        var loadTask = loader.LoadAssetAsync<GameObject>(PackageName, CubeLocation);
        yield return new WaitUntil(() => loadTask.IsCompleted);
        if (loadTask.IsFaulted)
            Assert.Fail(loadTask.Exception?.ToString());
        prefab = loadTask.Result;
#endif

        Assert.IsNotNull(prefab, $"资源加载失败: {CubeLocation}");

        _spawnedInstance = Object.Instantiate(prefab);
        Assert.IsNotNull(_spawnedInstance);
        Assert.IsTrue(_spawnedInstance.name.StartsWith("Cube"), $"实例名异常: {_spawnedInstance.name}");
    }

    [TearDown]
    public void TearDown()
    {
        if (_spawnedInstance != null)
            Object.Destroy(_spawnedInstance);
    }

    private static IEnumerator WaitForPackageReady(string packageName, float timeoutSeconds)
    {
        var deadline = Time.realtimeSinceStartup + timeoutSeconds;
        while (Time.realtimeSinceStartup < deadline)
        {


            if (YooAssets.TryGetPackage(packageName, out var package)
                && package.InitializeStatus == EOperationStatus.Succeeded)
            {
                yield break;
            }

            yield return null;
        }

        Assert.Fail($"等待包 {packageName} 初始化超时（{timeoutSeconds}s）");
    }
}
