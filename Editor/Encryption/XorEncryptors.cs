using System.IO;
using Dories.YooAssetSystem.Runtime.Patch.Encryption;
using YooAsset;

namespace Dories.YooAssetSystem.Editor.Encryption
{
    /// <summary>
    /// 资源包加密器基类。业务继承并提供与解密器相同的主密钥，按包名决定是否加密。
    /// 构建窗口 Bundle Encryptor 下拉里选业务子类（基类为抽象类，选它会构建失败）。
    /// </summary>
    public abstract class XorBundleEncryptor : IBundleEncryptor
    {
        protected abstract byte[] MasterKey { get; }

        /// <summary>
        /// 是否加密该包，默认全部加密；bundleName 形如 defaultpackage_assets_hotupdate.bundle
        /// </summary>
        protected virtual bool ShouldEncrypt(string bundleName) => true;

        public BundleEncryptResult Encrypt(BundleEncryptArgs args)
        {
            // BundleEncryptArgs.BundleName 是 internal，构建时文件名即包名
            if (!ShouldEncrypt(Path.GetFileName(args.FilePath)))
                return new BundleEncryptResult(false, null);

            return new BundleEncryptResult(true, XorCipher.Encrypt(MasterKey, File.ReadAllBytes(args.FilePath)));
        }
    }

    /// <summary>
    /// 清单加密器基类。业务继承并提供与清单解密器相同的主密钥。
    /// </summary>
    public abstract class XorManifestEncryptor : IManifestEncryptor
    {
        protected abstract byte[] MasterKey { get; }

        public byte[] Encrypt(byte[] fileData) => XorCipher.Encrypt(MasterKey, fileData);
    }
}
