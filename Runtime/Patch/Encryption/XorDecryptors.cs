using System.IO;
using YooAsset;

namespace Dories.YooAssetSystem.Runtime.Patch.Encryption
{
    /// <summary>
    /// 流式解密器基类（本地文件：离线 / 联机）。业务在 AOT 程序集继承并提供主密钥。
    /// 不要同时实现内存解密接口：YooAsset 会优先走内存解密。
    /// </summary>
    public abstract class XorBundleStreamDecryptor : IBundleStreamDecryptor
    {
        protected abstract byte[] MasterKey { get; }

        int IBundleStreamDecryptor.GetBufferSize(BundleDecryptArgs args) => 1024;

        Stream IBundleStreamDecryptor.CreateDecryptionStream(BundleDecryptArgs args)
        {
            return new XorDecryptStream(new FileStream(args.FilePath, FileMode.Open, FileAccess.Read, FileShare.Read), MasterKey);
        }
    }

    /// <summary>
    /// 内存解密器基类（Web / 小游戏只支持内存解密）。业务在 AOT 程序集继承并提供主密钥。
    /// </summary>
    public abstract class XorBundleMemoryDecryptor : IBundleMemoryDecryptor
    {
        protected abstract byte[] MasterKey { get; }

        byte[] IBundleMemoryDecryptor.GetDecryptedData(BundleDecryptArgs args)
        {
            var data = args.FileData ?? File.ReadAllBytes(args.FilePath);
            return XorCipher.Decrypt(MasterKey, data);
        }
    }

    /// <summary>
    /// 清单解密器基类。业务在 AOT 程序集继承并提供主密钥。
    /// </summary>
    public abstract class XorManifestDecryptor : IManifestDecryptor
    {
        protected abstract byte[] MasterKey { get; }

        byte[] IManifestDecryptor.Decrypt(byte[] fileData) => XorCipher.Decrypt(MasterKey, fileData);
    }
}
