using System;
using PangyaAPI.Cryptor.PangyaTableKey;
namespace PangyaAPI.Cryptor.Maker
{
    internal class CryptClient
    {
        /// <summary>
        ///     Decrypts data from client-side packets (sent from clients to servers.)
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

            if (source.Length < 5)
                throw new ArgumentOutOfRangeException(nameof(source), $"Packet too small ({source.Length} < 5)");

            var buffer = (byte[])source.Clone();          
            buffer[4] = PublicTableKey.CryptTable2[(key << 8) + source[0]];

            for (var i = 8; i < buffer.Length; i++) buffer[i] = (byte)(buffer[i] ^ buffer[i - 4]);

            var output = new byte[buffer.Length - 5];

            Array.Copy(buffer, 5, output, 0, buffer.Length - 5);

            return output;
        }

        /// <summary>
        ///     Encrypts data for client-side packets (sent from clients to servers.)
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
            var buffer = new byte[source.Length + 5];
            var pLen = buffer.Length - 4;

            buffer[0] = salt;
            buffer[1] = (byte)((pLen >> 0) & 0xFF);
            buffer[2] = (byte)((pLen >> 8) & 0xFF);
            buffer[4] = PublicTableKey.CryptTable2[oracleIndex];

            Array.Copy(source, 0, buffer, 5, source.Length);

            for (var i = buffer.Length - 1; i >= 8; i--) buffer[i] ^= buffer[i - 4];

            buffer[4] ^= PublicTableKey.CryptTable1[oracleIndex];
            return buffer;
        }

     
        public bool Decrypt_Packet(ref byte[] src, int key)
        {
            src[4] = Convert.ToByte(PublicTableKey.CryptTable2[src[0] + key] ^ src[4]);
            int num = src.Length - 4;
            byte[] array = new byte[num];
            Buffer.BlockCopy(src, 4, array, 0, num);
            for (int i = 4; i < num; i++)
            {
                array[i] = Convert.ToByte(array[i] ^ array[i - 4]);
            }
            num--;
            Buffer.BlockCopy(array, 1, src, 0, num);
            return true;
        }

        public bool _pangya_client_decrypt(ref byte[] buffin, byte key)
        {
            byte[] src = new byte[buffin.Length];
            Buffer.BlockCopy(buffin, 0, src, 0, buffin.Length);
            int key2 = key << 8;
            Decrypt_Packet(ref src, key2);
            int count = buffin.Length - 5;
            Buffer.BlockCopy(src, 0, buffin, 0, count);
            return true;
        }
    }
}
