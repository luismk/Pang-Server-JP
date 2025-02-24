using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.GameType
{
    // Dados dos holes no jogo
    public class Dados
    {
        public ushort score;
        public uint tacada;
        public byte finish = 1;
    }

    // Guild Match register
    public class GuildMatch
    {
        void clear()
        {
        }
        public uint[] uid = new uint[2];        // Guild UID: [0] e [1]
        public uint[] point = new uint[2];      // Guild Point: [0] e [1]
        public uint[] pang = new uint[2];       // Guild Pang: [0] e [1]
    }

    // Guild Points
    public class GuildPoints
    {
        public enum eGUILD_WIN : byte
        {
            WIN,
            LOSE,
            DRAW,
        }
        public void clear()
        {
        }
        public uint uid;
        public ulong point;
        public ulong pang;
        public eGUILD_WIN win;
    }

    // Guild Member Points
    public class GuildMemberPoints
    {
        void clear()
        {
        }
        public uint guild_uid;
        public uint member_uid;
        public uint point;
        public uint pang;
    }

}
