# Patch

`PatchEntity` 负责按配置的 ResourcePackage 执行热更流程。

## 流程

`Init` → `RequestPackageVersion` → `UpdatePackageManifest` → `CreateDownloader` → `DownloadPackageFiles` → `DownloadFileOver` → `ClearCacheBundle`

## 配置

在 Inspector 中为每个包设置：

- PackageName
- PlayMode（离线 / 联机 / Web / 微信小游戏 / 抖音小游戏）
- RemoteService、BundleDecryptor、ManifestDecryptor 类型名
- 下载并发、失败重试、Tags、缓存清理策略

运行时通过链式 API 订阅回调后调用 `StartPatch()`。需要更新时会得到 `PatchDownloader`，可 `StartDownload` / `PauseDownload` / `ResumeDownload` / `CancelDownload`。

## 资源加密

YooAsset 构建时按包调用加密器，每个包是否加密记在清单里，运行时只解密加密过的包，所以可以只加密敏感的包。框架提供一套按位置生成密钥流的异或实现（`XorCipher`），业务只需填主密钥和加密规则。

**定位**：密钥在客户端，这套加密只能挡住 AssetStudio / AssetRipper 这类直接拆包的工具，防不了逆向抠密钥或运行时 hook `AssetBundle.LoadFromStream` 来 dump。数值、经济这类需要以服务端为准的逻辑，以及防篡改（用签名校验），都不要依赖它。

| 基类 | 程序集 | 用途 |
|---|---|---|
| `XorBundleStreamDecryptor` | Runtime | 边读边解密，用于离线 / 联机（本地文件） |
| `XorBundleMemoryDecryptor` | Runtime | 整包解密到内存，用于 Web / 小游戏（这些平台只支持内存解密） |
| `XorManifestDecryptor` | Runtime | 清单解密 |
| `XorBundleEncryptor` | Editor | 构建时加密，重写 `ShouldEncrypt(bundleName)` 选择要加密的包 |
| `XorManifestEncryptor` | Editor | 构建时加密清单 |

接入步骤：

1. 解密器子类放在 **AOT 程序集**（如 Launcher）。`PatchEntity.Awake` 在热更 DLL 加载前就会创建解密器。
   ```csharp
   public static class GameKeys { public static byte[] Master => /* 运行时拼出来，交给 Obfuz 混淆 */; }
   public sealed class GameBundleDecryptor : XorBundleStreamDecryptor { protected override byte[] MasterKey => GameKeys.Master; }
   public sealed class GameManifestDecryptor : XorManifestDecryptor { protected override byte[] MasterKey => GameKeys.Master; }
   ```
2. 加密器子类放在 Editor 程序集，使用同一个主密钥：
   ```csharp
   public sealed class GameBundleEncryptor : XorBundleEncryptor
   {
       protected override byte[] MasterKey => GameKeys.Master;
       protected override bool ShouldEncrypt(string bundleName) => bundleName.Contains("hotupdate") || bundleName.Contains("configs");
   }
   public sealed class GameManifestEncryptor : XorManifestEncryptor { protected override byte[] MasterKey => GameKeys.Master; }
   ```
3. 在构建窗口的 Bundle Encryptor / Manifest Encryptor 下拉里选**业务子类**（下拉也会列出抽象基类，选基类会构建失败）。脚本构建时设置 `BuildParameters.BundleEncryptor` / `ManifestEncryptor`。
4. 在 PatchEntity Inspector 的 Bundle Decryptor / Manifest Decryptor 下拉里选对应的解密器。两者要和构建时的设置配对：清单加密了，运行时却没有配置解密器，初始化会失败。

注意：

- 编辑器模拟模式不读 bundle，看不到加密效果。要用离线模式跑真包来验证。
- 主密钥和算法发布后不能修改，否则已发布的包和玩家本地缓存都无法解开。
- 加密文件比原文件多 8 字节文件头（nonce，取明文 SHA256 的前 8 字节）。同样的内容加密结果相同，不影响增量更新。
