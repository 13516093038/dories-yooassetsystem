# Patch

`PatchEntity` 负责按配置的 ResourcePackage 执行热更流程。

## 流程

`Init` → `RequestPackageVersion` → `UpdatePackageManifest` → `CreateDownloader` → `DownloadPackageFiles` → `DownloadFileOver` → `ClearCacheBundle`

## 配置

在 Inspector 中为每个包设置：

- PackageName
- PlayMode（离线 / 联机 / Web / 微信小游戏 / 抖音小游戏）
- RemoteService、BundleDecryptor 类型名
- 下载并发、失败重试、Tags、缓存清理策略

运行时通过链式 API 订阅回调后调用 `StartPatch()`。需要更新时会得到 `PatchDownloader`，可 `StartDownload` / `PauseDownload` / `ResumeDownload` / `CancelDownload`。
