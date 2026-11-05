using System;
using System.IO;
using System.Security.Cryptography;

namespace Dories.YooAssetSystem.Runtime.Patch.Encryption
{
    /// <summary>
    /// 按位置推导密钥流的异或加密。
    /// 文件格式：[8 字节 nonce][密文]。nonce 取明文 SHA256 前 8 字节，同内容同密文（不影响增量更新），
    /// 每个文件的密钥流 = f(主密钥, nonce)，任意位置可独立解密，支持 Seek 流式读取。
    /// 注意：密钥在客户端，只能挡住直接拆包，不防逆向与运行时 dump。发布后不要修改主密钥或本算法。
    /// </summary>
    public static class XorCipher
    {
        public const int HeaderSize = 8;

        public static byte[] Encrypt(byte[] masterKey, byte[] plain)
        {
            byte[] nonce;
            using (var sha = SHA256.Create())
                nonce = sha.ComputeHash(plain);

            var result = new byte[HeaderSize + plain.Length];
            Buffer.BlockCopy(nonce, 0, result, 0, HeaderSize);
            Buffer.BlockCopy(plain, 0, result, HeaderSize, plain.Length);
            Apply(DeriveSeed(masterKey, result), result, HeaderSize, plain.Length, 0);
            return result;
        }

        public static byte[] Decrypt(byte[] masterKey, byte[] data)
        {
            if (data == null || data.Length < HeaderSize)
                throw new InvalidDataException("加密数据长度不足，缺少文件头");

            var plain = new byte[data.Length - HeaderSize];
            Buffer.BlockCopy(data, HeaderSize, plain, 0, plain.Length);
            Apply(DeriveSeed(masterKey, data), plain, 0, plain.Length, 0);
            return plain;
        }

        /// <summary>
        /// 由主密钥和文件头里的 nonce 推导该文件的密钥流种子
        /// </summary>
        public static ulong DeriveSeed(byte[] masterKey, byte[] header)
        {
            if (masterKey == null || masterKey.Length == 0)
                throw new ArgumentException("主密钥不能为空", nameof(masterKey));

            var input = new byte[masterKey.Length + HeaderSize];
            Buffer.BlockCopy(masterKey, 0, input, 0, masterKey.Length);
            Buffer.BlockCopy(header, 0, input, masterKey.Length, HeaderSize);
            using (var sha = SHA256.Create())
                return BitConverter.ToUInt64(sha.ComputeHash(input), 0);
        }

        /// <summary>
        /// 对 buffer[offset, offset+count) 原地异或；position 是这段数据在明文中的起始位置
        /// </summary>
        public static void Apply(ulong seed, byte[] buffer, int offset, int count, long position)
        {
            long block = -1;
            ulong word = 0;
            for (int i = 0; i < count; i++)
            {
                long p = position + i;
                if ((p >> 3) != block)
                {
                    block = p >> 3;
                    word = SplitMix64(seed + (ulong)block * 0x9E3779B97F4A7C15UL);
                }
                buffer[offset + i] ^= (byte)(word >> (int)((p & 7) << 3));
            }
        }

        private static ulong SplitMix64(ulong z)
        {
            z = (z ^ (z >> 30)) * 0xBF58476D1CE4E5B9UL;
            z = (z ^ (z >> 27)) * 0x94D049BB133111EBUL;
            return z ^ (z >> 31);
        }
    }

    /// <summary>
    /// 边读边解密的只读流，对外隐藏文件头，Position/Length 均按明文计算
    /// </summary>
    public sealed class XorDecryptStream : Stream
    {
        private readonly Stream _inner;
        private readonly ulong _seed;

        public XorDecryptStream(Stream inner, byte[] masterKey)
        {
            _inner = inner;
            var header = new byte[XorCipher.HeaderSize];
            inner.Position = 0;
            int read = 0;
            while (read < header.Length)
            {
                int n = inner.Read(header, read, header.Length - read);
                if (n <= 0)
                    throw new InvalidDataException("加密数据长度不足，缺少文件头");
                read += n;
            }
            _seed = XorCipher.DeriveSeed(masterKey, header);
        }

        public override bool CanRead => true;
        public override bool CanSeek => _inner.CanSeek;
        public override bool CanWrite => false;
        public override long Length => _inner.Length - XorCipher.HeaderSize;

        public override long Position
        {
            get => _inner.Position - XorCipher.HeaderSize;
            set => _inner.Position = value + XorCipher.HeaderSize;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            long position = Position;
            int n = _inner.Read(buffer, offset, count);
            XorCipher.Apply(_seed, buffer, offset, n, position);
            return n;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            switch (origin)
            {
                case SeekOrigin.Begin: Position = offset; break;
                case SeekOrigin.Current: Position += offset; break;
                case SeekOrigin.End: Position = Length + offset; break;
            }
            return Position;
        }

        public override void Flush() { }
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _inner.Dispose();
            base.Dispose(disposing);
        }
    }
}
