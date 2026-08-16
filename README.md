# Dories YooAssetSystem

基于 YooAsset 的热更补丁与资源加载运行时模块。

## 安装

在 Unity `Packages/manifest.json` 中加入：

```json
"com.dories.yooassetsystem": "https://github.com/13516093038/dories-yooassetsystem.git#split/yooassetsystem"
```

本包不声明 UPM 传递依赖。工程中需要自行放入 YooAsset（asmdef 按 GUID 引用）。安装 UniTask 后会自动启用 `DORIES_UNITASK_SUPPORT`。

## 功能

- **PatchEntity**：按包初始化、请求版本、更新清单、创建下载器、下载与清理缓存。
- **PatchDownloader**：开始 / 暂停 / 继续 / 取消下载，并按包订阅进度与错误。
- **AssetLoaderEntity**：按包加载资源、场景、RawFile，并支持卸载。
- **PlayMode**：离线、联机、Web、微信小游戏、抖音小游戏。
- **编辑器检视器**：在 Inspector 中配置包信息、远程服务与解密器类型。

更细的接口说明见 `README.Patch.md` 与 `README.AssetLoader.md`。

## 用法

热更：

```csharp
patchEntity
    .BuildNeedUpdateListener(downloader =>
    {
        downloader.DownloadProgressChangedEventArgs("DefaultPackage", args =>
        {
            Debug.Log($"{args.CurrentDownloadCount}/{args.TotalDownloadCount}");
        });
        downloader.StartDownload();
    })
    .BuildPatchCompleteListener(() => Debug.Log("Patch complete"))
    .BuildPatchFailedListener(error => Debug.LogError(error))
    .StartPatch();
```

资源加载：

```csharp
var cube = await assetLoaderEntity.LoadAssetAsync<GameObject>(
    "DefaultPackage",
    "Assets/Res/Prefabs/Cube.prefab");

var scene = await assetLoaderEntity.LoadSceneAsync(
    "DefaultPackage",
    "Assets/Res/Scenes/Game.unity",
    allowSceneActivation: false);
```

也可直接使用预制体 `Prefabs/Patch@Node` 与 `Prefabs/AssetLoader@Node`。

## Sample

打开 `Sample/PatchSample`。热更完成后会切到 `Sample/AssetLoadScene`。

资源加载场景按键：

- A：加载 Cube
- S：卸载 Cube
- D：加载 RawFile 文本
- F：卸载 RawFile
