# AssetLoader

`AssetLoaderEntity` 在补丁完成后按包加载资源。

## 能力

- 资源：`LoadAssetAsync` / `UnloadAsset`
- 场景：`LoadSceneAsync` / `UnloadScene`
- RawFile：`LoadRawFileAsync` / `LoadRawFileTextAsync` / `LoadRawFileBytesAsync` / `UnloadRawFile`
- 可用 `SetDefaultPackage` 省略后续调用中的包名

安装 UniTask 后，异步接口返回 `UniTask`；否则返回 `Task`。
