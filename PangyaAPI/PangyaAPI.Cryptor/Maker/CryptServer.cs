using System;
using PangyaAPI.Utilities.Cryptography;
using PangyaAPI.Cryptor.PangyaTableKey;
namespace PangyaAPI.Cryptor.Maker
{
    internal class CryptServer
    {
        /// <summary>
        ///     Decrypts data from server-side packets (sent from servers to clients.)
        /// </summary>
        /// <param name="source">The encrypted packet data.</param>
        /// <param name="key">Key to decrypt with.</param>
        /// <returns>The decrypted packet data.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown if the key is invalid or the packet data is too short.
        /// </exception>
        public byte[] Decrypt(byte[] source, byte key)
        {
            if (key >= 0x10) throw new ArgumentOutOfRangeException(nameof(key), $"Key too large ({key} >= 0x10)");

            if (source.Length < 8)
                throw new ArgumentOutOfRangeException(nameof(source), $"Packet too small ({source.Length} < 8)");

            var oracleByte = PublicTableKey.CryptTable2[(key << 8) + source[0]];
            var buffer = (byte[])source.Clone();

            buffer[7] ^= oracleByte;

            for (var i = 10; i < source.Length; i++) buffer[i] ^= buffer[i - 4];

            var compressedData = new byte[source.Length - 8];
            Array.Copy(buffer, 8, compressedData, 0, source.Length - 8);
            return Lzo.Decompress(compressedData);
        }

        /// <summary>
        ///     Encrypts data for server-side packets (sent from servers to clients.)
        /// </summary>
        /// <param name="source">The decrypted packet data.</param>
        /// <param name="key">Key to encrypt with.</param>
        /// <param name="salt">Random salt value to encrypt with.</param>
        /// <returns>The encrypted packet data.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown if an invalid key is specified.
        /// </exception>
        public byte[] Encrypt(byte[] source, byte key, byte salt)
        {
            if (key >= 0x10) throw new ArgumentOutOfRangeException(nameof(key), $"Key too large ({key} >= 0x10)");

            var oracleIndex = (key << 8) + salt;
            var compressedData = Lzo.Compress(source);
            var buffer = new byte[compressedData.Length + 8];
            var pLen = buffer.Length - 3;

            var u = source.Length;
            var x = (u + u / 255) & 0xff;
            var v = (u - x) / 255;
            var y = (v + v / 255) & 0xff;
            var w = (v - y) / 255;
            var z = (w + w / 255) & 0xff;

            buffer[0] = salt;
            buffer[1] = (byte)((pLen >> 0) & 0xFF);
            buffer[2] = (byte)((pLen >> 8) & 0xFF);
            buffer[3] = (byte)(PublicTableKey.CryptTable1[oracleIndex] ^ PublicTableKey.CryptTable2[oracleIndex]);
            buffer[5] = (byte)z;
            buffer[6] = (byte)y;
            buffer[7] = (byte)x;

            Array.Copy(compressedData, 0, buffer, 8, compressedData.Length);

            for (var i = buffer.Length - 1; i >= 10; i--) buffer[i] ^= buffer[i - 4];

            buffer[7] ^= PublicTableKey.CryptTable2[oracleIndex];

            return buffer;
        }
    }
}
