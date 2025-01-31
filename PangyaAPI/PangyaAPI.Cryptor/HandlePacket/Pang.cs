using PangyaAPI.Cryptor.Maker;
using System;

namespace PangyaAPI.Cryptor.HandlePacket
{
    public static class Pang
    {
		public static byte[] ClientEncrypt(this byte[] message, byte Key)
        {
            return new CryptClient().Encrypt(message, Key, 0);
        }
        public static byte[] ClientDecrypt(this byte[] message, byte Key)
        {
            return new CryptClient().Decrypt(message, Key);
        }
        public static byte[] ServerEncrypt(this byte[] message, byte Key)
        {
            return new CryptServer().Encrypt(message, Key, 0);
        }
        public static byte[] ServerDecrypt(this byte[] message, byte Key)
        {
            return new CryptServer().Decrypt(message, Key);
        }
        public static void Packet_Ver_Decrypt(this ref uint client_build)
        {
            new CryptClientBuild().Decrypt(ref client_build);
        }
    }
}
