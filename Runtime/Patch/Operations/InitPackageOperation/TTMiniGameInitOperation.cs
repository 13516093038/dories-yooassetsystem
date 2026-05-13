using YooAsset;

namespace Dories.YooassetSystem.Runtime.Patch.Operations.InitPackageOperation
{
    public class TTMiniGameInitOperation :  IYooAssetInitOperation
    {
        public new InitializationOperation Init(ResourcePackage package, string packageName, IRemoteServices remoteServices,
            IDecryptionServices decryptionServices = null)
        {
            var createParameters = new WebPlayModeParameters();
#if UNITY_WEBGL && DOUYINMINIGAME && !UNITY_EDITOR
            string packageRoot = "yoo";
            createParameters.WebServerFileSystemParameters =
                TiktokFileSystemCreater.CreateFileSystemParameters(packageRoot, remoteServices, null);
#endif
            return package.InitializeAsync(createParameters);
        }
    }
}