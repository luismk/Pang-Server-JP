using PangyaAPI.Cryptor.HandlePacket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PangyaAPI.TCP.PangyaPacket
{
    public class Packet : PacketBase
    {
        public Packet()
        {
        }

        public Packet(ushort ID) : base(ID)
        {
        }

        public Packet(byte[] message, byte key) : base(message, key)
        {
        }
        public void AddFixedString(string value, int len)
        {
            WriteStr(value, len);
        }
        public void Version_Decrypt(uint @packet_version)
        {
            Pang.Packet_Ver_Decrypt(ref @packet_version);
        }
    }
}
