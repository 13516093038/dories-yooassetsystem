using YooAsset;

namespace Dories.YooassetSystem.Runtime.Patch.Operations.InitPackageOperation
{
    public class WeChatMiniGameInitOperation : IYooAssetInitOperation
    {
        public InitializationOperation Init(ResourcePackage package, string packageName, IRemoteServices remoteServices,
            IDecryptionServices decryptionServices = null)
        {
            var createParameters = new WebPlayModeParameters();
#if UNITY_WEBGL && WEIXINMINIGAME && !UNITY_EDITOR
            string packageRoot = $"{WeChatWASM.WX.env.USER_DATA_PATH}/__GAME_FILE_CACHE"; //注意：如果有子目录，请修改此处！
            IRemoteServices m_RemoteServices = new RemoteServices(defaultHostServer, fallbackHostServer);
            createParameters.WebServerFileSystemParameters =
            WechatFileSystemCreater.CreateFileSystemParameters(packageRoot, m_RemoteServices, decryptionServices);
#endif
            return package.InitializeAsync(createParameters);
        }
    }
}