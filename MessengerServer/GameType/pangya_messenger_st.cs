using PangyaAPI.Network.Pangya_St;
using System;
using System.Runtime.InteropServices;

namespace MessengerServer.GameType
{
    public class _Define
    {
        public const int FRIEND_LIST_LIMIT = 50;
        public const int FRIEND_PAG_LIMIT = 30;
    }
    // PlayerInfo
    public class player_info
    {
        public player_info(uint _ul = 0u)
        {
            clear();
        }
        public void clear()
        {
            block_flag = new BlockFlag();
            guild_name = "";
            id = "";
            nickname = "";
        }

        public void set_info(player_info info)
        {
            if (info == null)
                throw new ArgumentNullException(nameof(info), "O parâmetro 'info' não pode ser nulo.");

            uid = info.uid;
            m_cap = info.m_cap;
            block_flag = info.block_flag != null ? info.block_flag : new BlockFlag();
            guild_uid = info.guild_uid;
            guild_name = info.guild_name;
            server_uid = info.server_uid;
            level = info.level;
            sex = info.sex;
            id = info.id;
            nickname = info.nickname;
        }
        public uint uid;
        public uint m_cap;
        public BlockFlag block_flag = new BlockFlag();
        public uint guild_uid;
        public string guild_name = "";
        public uint server_uid;
        public ushort level;
        public byte sex;
        public string id = "";
        public string nickname = "";
    }

    // Canal Player Info
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public class ChannelPlayerInfo
    {
        public ChannelPlayerInfo() => clear();

        public void clear()
        {
            room = new Room();
            server_uid = uint.MaxValue;
            id = byte.MaxValue;
            name = "";
        }
        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        public class Room
        {
            public void clear()
            {
                number = ushort.MaxValue;
            }
            public ushort number;
            public int type;
        }
        [MarshalAs(UnmanagedType.Struct, SizeConst = 6)]
        public Room room = new Room();

        public uint server_uid;
        public byte id;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string name = "";
    }

    // Friend Info
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public class FriendInfo
    {
        public FriendInfo(uint _ul = 0u)
        {
            clear();
        }
        public void clear()
        {
            nickname = "";
            apelido = "";
            lUnknown = -1;
            lUnknown2 = 0;
            lUnknown3 = -1;
            lUnknown4 = 0;
            lUnknown5 = 0;
            lUnknown6 = 0;
            lUnknown7 = 0;
        }
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 22)]
        public string nickname = "";
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 11)]
        public string apelido = "";
        public uint uid;
        public int lUnknown;
        public int lUnknown2;
        public int lUnknown3;
        public int lUnknown4;
        public int lUnknown5;
        public int lUnknown6;
        public int lUnknown7; // Esse aqui s� tem no JP, esse valor a+, peguei ele sempre zero, das vezes que vi no pacote
    }

    // Friend Info Ex
    public class FriendInfoEx : FriendInfo
    {
        public FriendInfoEx(uint _ul = 0u) : base()
        {
            clear();
        }
        public new void clear()
        {

            base.clear();

            cUnknown_flag = 255;
            level = 0;
            flag = new uFlag(); // Flag se o player � amigo ou � membro guild
            state = new uState(); // Sex, online, friend, request, block e etc
            flag.clear();
            state.clear();
        }
        [StructLayout(LayoutKind.Explicit, Size = 1)]
        public struct uState
        {
            [FieldOffset(0)] public byte ucState;

            public void clear() => ucState = 0;

            public byte sex
            {
                get => (byte)((ucState & 0b0000_0001) != 0 ? 1 : 0);
                set => ucState = (byte)(value != 0 ? ucState | 0b0000_0001 : ucState & ~0b0000_0001);
            }

            public byte online
            {
                get => (byte)((ucState & 0b0000_0010) != 0 ? 1 : 0);
                set => ucState = (byte)(value != 0 ? ucState | 0b0000_0010 : ucState & ~0b0000_0010);
            }

            public byte _friend
            {
                get => (byte)((ucState & 0b0000_0100) != 0 ? 1 : 0);
                set => ucState = (byte)(value != 0 ? ucState | 0b0000_0100 : ucState & ~0b0000_0100);
            }

            public byte request_friend
            {
                get => (byte)((ucState & 0b0000_1000) != 0 ? 1 : 0);
                set => ucState = (byte)(value != 0 ? ucState | 0b0000_1000 : ucState & ~0b0000_1000);
            }

            public byte block
            {
                get => (byte)((ucState & 0b0001_0000) != 0 ? 1 : 0);
                set => ucState = (byte)(value != 0 ? ucState | 0b0001_0000 : ucState & ~0b0001_0000);
            }

            public byte play
            {
                get => (byte)((ucState & 0b0010_0000) != 0 ? 1 : 0);
                set => ucState = (byte)(value != 0 ? ucState | 0b0010_0000 : ucState & ~0b0010_0000);
            }

            public byte AFK
            {
                get => (byte)((ucState & 0b0100_0000) != 0 ? 1 : 0);
                set => ucState = (byte)(value != 0 ? ucState | 0b0100_0000 : ucState & ~0b0100_0000);
            }

            public byte busy
            {
                get => (byte)((ucState & 0b1000_0000) != 0 ? 1 : 0);
                set => ucState = (byte)(value != 0 ? ucState | 0b1000_0000 : ucState & ~0b1000_0000);
            }
        }
        [StructLayout(LayoutKind.Explicit, Size = 1)]
        public struct uFlag
        {
            [FieldOffset(0)] public byte ucFlag;

            public void clear() => ucFlag = 0;

            public byte _friend
            {
                get => (byte)((ucFlag & 0b0000_0001) != 0 ? 1 : 0);
                set => ucFlag = (byte)(value != 0 ? ucFlag | 0b0000_0001 : ucFlag & ~0b0000_0001);
            }

            public byte guild_member
            {
                get => (byte)((ucFlag & 0b0000_0010) != 0 ? 1 : 0);
                set => ucFlag = (byte)(value != 0 ? ucFlag | 0b0000_0010 : ucFlag & ~0b0000_0010);
            }
        }
        public byte cUnknown_flag;
        public uFlag flag = new uFlag(); // Flag se o player � amigo ou � membro guild
        public uState state = new uState(); // Sex, online, friend, request, block e etc
        public byte level;
    }

    // Many Packet
    public class ManyPacket
    {
        public ManyPacket(in ushort _size, in ushort _limit)
        {
            this.const_total = _size;
            this.const_limit = _limit;

            // Initialize data
            init();
        }
        public void clear()
        {

        }
        public void init()
        {
            // Calcula Initial data

            paginas = (ushort)(const_total / const_limit);

            if ((const_total % const_limit) != 0)
            {
                ++paginas;
            }

            pag.pagina = 1;
            pag.total = const_total;
            pag.current = (const_total <= const_limit) ? const_total : const_limit;

            // Calcule Index
            calcIndex();
        }
        public void increse()
        {

            if (pag.total > 0)
            {
                pag.pagina++;

                if (pag.total <= const_limit)
                {
                    pag.current = pag.total = 0;
                }
                else
                {
                    pag.total -= const_limit;
                    pag.current = (pag.total <= const_limit) ? (ushort)pag.total : const_limit;
                }

                // Cacule Index
                calcIndex();
            }
        }
        public class Pagina
        {
            public void clear()
            {

            }
            public byte pagina;
            public ushort total;
            public ushort current;
        }
        public class Index
        {
            public void clear()
            {
            }
            public ushort start;
            public ushort end;
        }
        protected void calcIndex()
        {
             // Calcule Index
            index.start = (ushort)((pag.pagina - 1) * const_limit);
            index.end = (ushort)(index.start + ((pag.total <= const_limit) ? pag.total : const_limit));
        }
        protected readonly ushort const_total;
        public readonly ushort const_limit;
        public ushort paginas;
        public Pagina pag = new Pagina();
        public Index index = new Index();
    }
}
