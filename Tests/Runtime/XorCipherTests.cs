using System;
using System.IO;
using System.Text;
using Dories.YooAssetSystem.Runtime.Patch.Encryption;
using NUnit.Framework;

namespace Dories.YooAssetSystem.Tests
{
    public class XorCipherTests
    {
        private static readonly byte[] Key = Encoding.UTF8.GetBytes("dories-test-key");

        private static byte[] MakePlain(int length)
        {
            var data = new byte[length];
            new Random(42).NextBytes(data);
            return data;
        }

        [Test]
        public void EncryptDecrypt_RoundTrip()
        {
            var plain = MakePlain(10007);
            var encrypted = XorCipher.Encrypt(Key, plain);

            Assert.AreEqual(plain.Length + XorCipher.HeaderSize, encrypted.Length);
            Assert.AreNotEqual(plain, encrypted);
            Assert.AreEqual(plain, XorCipher.Decrypt(Key, encrypted));
        }

        [Test]
        public void Encrypt_IsDeterministic_AndKeyMatters()
        {
            var plain = MakePlain(512);
            Assert.AreEqual(XorCipher.Encrypt(Key, plain), XorCipher.Encrypt(Key, plain));
            Assert.AreNotEqual(plain, XorCipher.Decrypt(Encoding.UTF8.GetBytes("wrong"), XorCipher.Encrypt(Key, plain)));
        }

        [Test]
        public void DecryptStream_SeekAndReadAnywhere()
        {
            var plain = MakePlain(10007);
            using var stream = new XorDecryptStream(new MemoryStream(XorCipher.Encrypt(Key, plain)), Key);
            Assert.AreEqual(plain.Length, stream.Length);

            foreach (var (pos, len) in new[] { (0, 13), (5, 100), (4093, 3000), (9990, 17) })
            {
                stream.Seek(pos, SeekOrigin.Begin);
                var buffer = new byte[len + 3];
                int read = stream.Read(buffer, 3, len);
                Assert.AreEqual(len, read);
                for (int i = 0; i < len; i++)
                    Assert.AreEqual(plain[pos + i], buffer[3 + i], $"pos {pos + i}");
            }
        }
    }
}
