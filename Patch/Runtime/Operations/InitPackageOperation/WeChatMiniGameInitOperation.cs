using YooAsset;

namespace Dories.YooassetSystem.Patch.Runtime.Operations
{
    /// <summary>
    /// 微信小游戏初始化操作。
    /// 接入前请：
    /// 1. 安装 WX-WASM-SDK-V2 并导入 YooAsset Mini Game 扩展（WechatFileSystem）
    /// 2. 在 Patch.Runtime.asmdef 中引用 YooAsset.MiniGame
    /// 文档：https://www.yooasset.com/docs/MiniGame#微信小游戏
    /// </summary>
    public class WeChatMiniGameInitOperation : IYooAssetInitOperation
    {
        public InitializePackageOperation Initialize(
            ResourcePackage package,
            IRemoteService remoteServices,
            IBundleDecryptor bundleDecryptor = null,
            IManifestDecryptor manifestDecryptor = null)
        {
            var createParameters = new WebPlayModeOptions();

#if UNITY_WEBGL && !UNITY_EDITOR && (WEIXINMINIGAME || UNITY_WECHATMINIGAME)
            ApplyWeChatPlatformOptions(createParameters, remoteServices, bundleDecryptor, manifestDecryptor);
#else
            // 未安装微信插件或在 Editor 中调试时，回退到普通 Web 模式
            WebPlayModeInitHelper.ApplyDefaultWebOptions(
                createParameters, remoteServices, bundleDecryptor, manifestDecryptor);
#endif

            return package.InitializePackageAsync(createParameters);
        }

#if UNITY_WEBGL && !UNITY_EDITOR && (WEIXINMINIGAME || UNITY_WECHATMINIGAME)
        private static void ApplyWeChatPlatformOptions(
            WebPlayModeOptions createParameters,
            IRemoteService remoteServices,
            IBundleDecryptor bundleDecryptor,
            IManifestDecryptor manifestDecryptor)
        {
            // 缓存根目录需与微信插件 CDN 配置一致，详见 YooAsset 小游戏文档
            string packageRoot = $"{WeChatWASM.WX.env.USER_DATA_PATH}/__GAME_FILE_CACHE/yoo";

            var webNetworkParams = WechatFileSystemCreater.CreateFileSystemParameters(
                packageRoot,
                remoteServices,
                bundleDecryptor);

            if (manifestDecryptor != null)
            {
                webNetworkParams.AddParameter(EFileSystemParameter.ManifestDecryptor, manifestDecryptor);
            }

            createParameters.WebNetworkFileSystemParameters = webNetworkParams;
        }
#endif
    }
}
