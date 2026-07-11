using YooAsset;

namespace Dories.YooAssetSystem.Runtime.Patch.Operations
{
    /// <summary>
    /// Web 运行模式初始化参数构建（Editor 调试及小游戏平台未就绪时的回退方案）
    /// </summary>
    internal static class WebPlayModeInitHelper
    {
        public static void ApplyDefaultWebOptions(
            WebPlayModeOptions createParameters,
            IRemoteService remoteServices,
            IBundleDecryptor bundleDecryptor = null)
        {
            var webServerFileSystemParams = FileSystemParameters.CreateDefaultWebServerFileSystemParameters();
            var webNetworkFileSystemParams =
                FileSystemParameters.CreateDefaultWebNetworkFileSystemParameters(remoteServices);

            if (bundleDecryptor != null)
            {
                webServerFileSystemParams.AddParameter(EFileSystemParameter.AssetBundleDecryptor, bundleDecryptor);
            }

            createParameters.WebServerFileSystemParameters = webServerFileSystemParams;
            createParameters.WebNetworkFileSystemParameters = webNetworkFileSystemParams;
        }
    }
}
