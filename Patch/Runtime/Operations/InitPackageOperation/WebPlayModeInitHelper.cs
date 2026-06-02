using YooAsset;

namespace Dories.YooassetSystem.Patch.Runtime.Operations
{
    /// <summary>
    /// Web 运行模式初始化参数构建（Editor 调试及小游戏平台未就绪时的回退方案）
    /// </summary>
    internal static class WebPlayModeInitHelper
    {
        public static void ApplyDefaultWebOptions(
            WebPlayModeOptions createParameters,
            IRemoteService remoteServices,
            IBundleDecryptor bundleDecryptor = null,
            IManifestDecryptor manifestDecryptor = null)
        {
            var webServerFileSystemParams = FileSystemParameters.CreateDefaultWebServerFileSystemParameters();
            var webNetworkFileSystemParams =
                FileSystemParameters.CreateDefaultWebNetworkFileSystemParameters(remoteServices);

            if (bundleDecryptor != null)
            {
                webServerFileSystemParams.AddParameter(EFileSystemParameter.AssetBundleDecryptor, bundleDecryptor);
            }

            if (manifestDecryptor != null)
            {
                webServerFileSystemParams.AddParameter(EFileSystemParameter.ManifestDecryptor, manifestDecryptor);
            }

            createParameters.WebServerFileSystemParameters = webServerFileSystemParams;
            createParameters.WebNetworkFileSystemParameters = webNetworkFileSystemParams;
        }
    }
}
