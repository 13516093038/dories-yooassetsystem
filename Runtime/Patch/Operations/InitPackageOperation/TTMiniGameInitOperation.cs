using YooAsset;

namespace Dories.YooAssetSystem.Runtime.Patch.Operations
{
    /// <summary>
    /// 抖音小游戏初始化操作。
    /// 接入前请：
    /// 1. 安装字节小游戏 Unity 插件并导入 YooAsset Mini Game 扩展（TiktokFileSystem）
    /// 2. 在 Patch.Runtime.asmdef 中引用 YooAsset.MiniGame
    /// 文档：https://www.yooasset.com/docs/MiniGame#抖音小游戏
    /// </summary>
    public class TTMiniGameInitOperation : IYooAssetInitOperation
    {
        public InitializePackageOperation Initialize(
            ResourcePackage package,
            IRemoteService remoteServices,
            IBundleDecryptor bundleDecryptor = null,
            EditorVirtualType editorVirtualType = EditorVirtualType.VirttualAssetBundle)
        {
            var createParameters = new WebPlayModeOptions();

#if UNITY_WEBGL && !UNITY_EDITOR && DOUYINMINIGAME
            ApplyTTPlatformOptions(createParameters, remoteServices, bundleDecryptor);
#else
            // 未安装抖音插件或在 Editor 中调试时，回退到普通 Web 模式
            WebPlayModeInitHelper.ApplyDefaultWebOptions(
                createParameters, remoteServices, bundleDecryptor);
#endif

            return package.InitializePackageAsync(createParameters);
        }

#if UNITY_WEBGL && !UNITY_EDITOR && DOUYINMINIGAME
        private static void ApplyTTPlatformOptions(
            WebPlayModeOptions createParameters,
            IRemoteService remoteServices,
            IBundleDecryptor bundleDecryptor)
        {
            var webNetworkParams = bundleDecryptor != null
                ? TiktokFileSystemCreater.CreateFileSystemParameters(remoteServices, bundleDecryptor)
                : TiktokFileSystemCreater.CreateFileSystemParameters(remoteServices);

            createParameters.WebNetworkFileSystemParameters = webNetworkParams;
        }
#endif
    }
}
