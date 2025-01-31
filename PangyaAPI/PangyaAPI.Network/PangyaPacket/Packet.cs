using PangyaAPI.Cryptor.HandlePacket;
using PangyaAPI.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PangyaAPI.Network.PangyaPacket
{
    public class Packet : PacketBase
    {
        public Packet()
        {
        }  

        public Packet(byte[] message, byte key) : base(message, key)
        {
        }

        public string Log()
        {
            return Message.HexDump();
        }

        public void Version_Decrypt(uint @packet_version)
        {
            Pang.Packet_Ver_Decrypt(ref @packet_version);
        }
    }
}
