using PangyaAPI.SQL;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace PangyaAPI.TCP.Pangya_St
{
    public class IPBan
    {
        public enum _TYPE : byte
        {
            IP_BLOCK_NORMAL,
            IP_BLOCK_RANGE
        }             
        public _TYPE type;
        public uint ip;
        public uint mask;
    }

    // que guarda a estrutura de bits da propriedade do server
    [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 4)]
    public class uProperty
    {
        public uProperty(uint _ul = 0u)
        {
             ulProperty = _ul;
            setFlag();
        }

        private void setFlag()
        {
            BitArray bits = new BitArray(BitConverter.GetBytes(ulProperty));
            //bits = ngineTools.PadToFullByte(bits);

            //stBit = new _stBit()
            //{
            //    mantle = bits.Get(0).ToUInt(),
            //    only_rookie = bits.Get(1).ToUInt(),
            //    natural = bits.Get(2).ToUInt(),
            //    verde = bits.Get(3).ToUInt(),
            //    azul = bits.Get(4).ToUInt(),
            //    grand_prix = bits.Get(5).ToUInt()
            //};
        }

        [field: MarshalAs(UnmanagedType.U4, SizeConst = 4)]
        public uint ulProperty { get; set; }
        public _stBit stBit { get; set; } = new _stBit();
        public class _stBit
        {
            public uint mantle; // = 0; // Só GM ou pessoas autorizadas pode ver esse server
            public uint only_rookie; // = 0; // Só Rookie(Iniciante) Pode entrar
            public uint natural; // = 0; // Natural modo
            public uint verde; // = 0; // Cor Verde
            public uint azul; // = 0; // Cor Azul
            public uint grand_prix; // = 0; // Grand Prix
        }
    }
    // que guarda a estrutura de bits do event flag do server
    [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 2)]
    public class uEventFlag
    {
        public uEventFlag(short ul = 0)
        {
            usEventFlag = ul;
            stBit = new _stBit();
        }
        [field: MarshalAs(UnmanagedType.U2, SizeConst = 2)]
        public short usEventFlag { get; set; }
        public _stBit stBit;
        [StructLayout(LayoutKind.Sequential)]
        public class _stBit
        {
            public bool unknown; // Unknown
            public bool pang_x_plus; // Pang X2 e Maior que 2
            public bool exp_x2; // Exp X2
            public bool angel_wing; // Event Angel Wing(Diminui Quit a cada jogo)
            public bool exp_x_plus; // Exp X3 e Maior que 3
            public bool unknown2; // Unknown
            public bool club_mastery_x_plus; // Club Mastert X2 e Maior que 2
        }

        public void setState()
        {
            //BitArray bits = new BitArray(BitConverter.GetBytes(usEventFlag));
            //bits = EngineTools.PadToFullByte(bits);

            //stBit = new _stBit()
            //{
            //    unknown = bits.Get(0),
            //    pang_x_plus = bits.Get(1),
            //    exp_x2 = bits.Get(2),
            //    angel_wing = bits.Get(3),
            //    exp_x_plus = bits.Get(4),
            //    unknown2 = bits.Get(5),
            //    club_mastery_x_plus = bits.Get(6)
            //};
        }

        public uEventFlag setState(short value)
        {
            //usEventFlag = value;
            //BitArray bits = new BitArray(BitConverter.GetBytes(value));
            //bits = EngineTools.PadToFullByte(bits);

            //stBit = new _stBit()
            //{
            //    unknown = bits.Get(0),
            //    pang_x_plus = bits.Get(1),
            //    exp_x2 = bits.Get(2),
            //    angel_wing = bits.Get(3),
            //    exp_x_plus = bits.Get(4),
            //    unknown2 = bits.Get(5),
            //    club_mastery_x_plus = bits.Get(6)
            //};
            return this;
        }
     }

    public class uFlag
    {
        public uFlag(ulong _ull = 0u)

        { ullFlag = _ull; stBit = new _stBit(); }
        public ulong ullFlag;
        public class _stBit
        {
            public ulong all_game { get; set; } = 0; // Não pode jogar nada
            public ulong buy_and_gift_shop { get; set; } = 0; // Não pode comprar no shop
            public ulong gift_shop { get; set; } = 0; // Não pode enviar presente
            public ulong papel_shop { get; set; } = 0; // Não pode jogar no Papel Shop
            public ulong personal_shop { get; set; } = 0; // Não pode vender no personal shop
            public ulong stroke { get; set; } = 0; // Não pode jogar Stroke
            public ulong match { get; set; } = 0; // Não pode jogar Match
            public ulong tourney { get; set; } = 0; // Não pode jogar Tourney
            public ulong team_tourney { get; set; } = 0; // Não pode jogar Team Tourney(Agora é Short Game)
            public ulong guild_battle { get; set; } = 0; // Não pode jogar Guild Battle
            public ulong pang_battle { get; set; } = 0; // Não pode jogar Pang Battle
            public ulong approach { get; set; } = 0; // Não pode jogar Approach
            public ulong lounge { get; set; } = 0; // Não pode criar sala lounge e entrar sala lounge
            public ulong scratchy { get; set; } = 0; // Não pode jogar no Scratchy System
            public ulong rank_server { get; set; } = 0; // Não pode abrir o rank server
            public ulong ticker { get; set; } = 0; // Não pode mandar ticker
            public ulong mail_box { get; set; } = 0; // Desabilita Mail Box
            public ulong grand_zodiac { get; set; } = 0; // Acho que é o grand zodiac, se não for vai ser
            public ulong single_play { get; set; } = 0; // Acho que é o Single Play, se não for vai ser
            public ulong grand_prix { get; set; } = 0; // Acho que é o Grand Prix, se não for vai ser
            public ulong guild { get; set; } = 0; // Desabilita Guild
            public ulong ssc { get; set; } = 0; // Não pode jogar Special Shuffle Course
            public ulong memorial_shop { get; set; } = 0; // Não pode jogar no Memorial Shop
            public ulong short_game { get; set; } = 0; // Não pode jogar Short Game
            public ulong char_mastery { get; set; } = 0; // Não pode mexer no Character Mastery System
            public ulong lolo_copound_card { get; set; } = 0; // Não pode jogar no Lolo Copound Card System
            public ulong cadie_recycle { get; set; } = 0; // Não pode usar o Caddie Recycle Item System
            public ulong legacy_tiki_shop { get; set; } = 0; // Não pode usar o Legacy Tiki Shop System }
        }
        public _stBit stBit { get; set; }
    }


    [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 92)]
    public class ServerInfo
    {
        [field: MarshalAs(UnmanagedType.ByValTStr, SizeConst = 40)]
        public string nome { get; set; } = "";
        public int uid { get; set; }
        public int max_user { get; set; }
        public int curr_user { get; set; }
        [field: MarshalAs(UnmanagedType.ByValTStr, SizeConst = 18)]
        public string ip { get; set; } = "";
        public int port { get; set; }
        [field: MarshalAs(UnmanagedType.Struct, SizeConst = 4)]
        public uProperty propriedade = new uProperty();
        public int angelic_wings_num { get; set; }
        [field: MarshalAs(UnmanagedType.Struct, SizeConst = 4)]
        public uEventFlag event_flag = new uEventFlag();
        public short event_map { get; set; }
        public short app_rate { get; set; }
        public short unknown { get; set; } // pode ser scratchy rate ou não
        public short img_no { get; set; }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class ServerInfoEx : ServerInfo
    {
        internal int packet_version;

        public sbyte tipo { get; set; }
        [field: MarshalAs(UnmanagedType.ByValTStr, SizeConst = 40)]
        public string version { get; set; } = new string(new char[40]);
        [field: MarshalAs(UnmanagedType.ByValTStr, SizeConst = 40)]
        public string version_client { get; set; } = new string(new char[40]);
        [field: MarshalAs(UnmanagedType.Struct, SizeConst = 2)]
        public RateConfigInfo rate { get; set; } = new RateConfigInfo();
        [field: MarshalAs(UnmanagedType.Struct, SizeConst = 4)]
        public uFlag flag { get; set; } = new uFlag();
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class RateConfigInfo
    {
        public short scratchy { get; set; }
        public short papel_shop_rare_item { get; set; }
        public short papel_shop_cookie_item { get; set; }
        public short treasure { get; set; }
        public short pang { get; set; }
        public short exp { get; set; }
        public short club_mastery { get; set; }
        public short chuva { get; set; }
        public short memorial_shop { get; set; }
        public short grand_zodiac_event_time { get; set; }          // Verifica se o evento do grand zodiac está ativado no server
        public short angel_event { get; set; }                      // Verifica se o Angel Event Quit Reduce está ativo no server
        public short grand_prix_event { get; set; }             // Verifica se o Grand Prix evento está ativado no server
        public short golden_time_event { get; set; }                // Verifica se o Golden Time está ativado no server
        public short login_reward_event { get; set; }               // Verifica se o Login Reward está ativado no server
        public short bot_gm_event { get; set; }                 // Verifica se o Bot GM Event está ativado no server
        public short smart_calculator { get; set; }             // Verifica se o Smart Calculator está ativado no server

        public uint countBitGrandPrixEvent()
        {

            uint count = 0;
            // 16 Bit public short
            for (var i = 0; i < 16u; ++i)
            {
                var check = (grand_prix_event >> i);
                if ((check & 1) == 1)
                    count++;
            }
            return count;
        }
        public List<uint> getValueBitGrandPrixEvent()
        {

            List<uint> v_value = new List<uint>();

            // 16 Bit unisgned short
            for (var i = 0; i < 16; ++i)
            {
                var check = (grand_prix_event >> i);
                if ((check & 1) == 1)
                    v_value.Add((uint)i + 1);
            }
            return v_value;
        }

        public bool checkBitGrandPrixEvent(int _type)
        {
            if (_type == 0)
                return false;

            var check = Convert.ToUInt32(grand_prix_event);

            return ((check >> (_type - 1)) & 1) == 1;
        }


        public string toString()
        {
            return $"GRAND_ZODIAC_EVENT_TIME={grand_zodiac_event_time}, " +
                   $"GOLDEN_TIME_EVENT={golden_time_event}, " +
                   $"ANGEL_EVENT={angel_event}, " +
                   $"GRAND_PRIX_EVENT={grand_prix_event}, " +
                   $"LOGIN_REWARD_EVENT={login_reward_event}, " +
                   $"BOT_GM_EVENT={bot_gm_event}, " +
                   $"SMART_CALCULATOR_SYSTEM={smart_calculator}, " +
                   $"SCRATCHY={scratchy}, " +
                   $"PAPEL_SHOP_RARE_ITEM={papel_shop_rare_item}, " +
                   $"PAPEL_SHOP_COOKIE_ITEM={papel_shop_cookie_item}, " +
                   $"TREASURE={treasure}, " +
                   $"PANG={pang}, " +
                   $"EXP={exp}, " +
                   $"CLUB_MASTERY={club_mastery}, " +
                   $"CHUVA={chuva}, " +
                   $"MEMORIAL_SHOP={memorial_shop}";
        }
    }

    public partial class TableMac
    {
        public string Mac_Adress { get; set; }
        public DateTime Date { get; set; }

        public TableMac(string adress, DateTime insert_time)
        {
            Mac_Adress = adress;
            Date = insert_time;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class chat_macro_user
    {
        [field: MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string[] macro { get; set; }
        public chat_macro_user Init()
        {
            macro = new string[9];
            for (int i = 0; i < 9; i++)
            {
                macro[i] = "Pangya!";
            }

            return this;
        }
    }


    // Auth Server m_key Struture
    public class AuthServerKey
    {
        public AuthServerKey()
        {
        }
        public bool isValid()
        {
            return (valid == 1 && key[0] != '\0');
        }
        public bool checkKey(string _str)
        {
            return (isValid() && string.Compare(_str, key) == 0);
        }
        public int server_uid;
        [field: MarshalAs(UnmanagedType.ByValTStr, SizeConst = 17)] 
        public string key;               // 16 + null termineted string
        public byte valid = 1;
    }


    // Keys Of Login
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class KeysOfLogin
    {
        public KeysOfLogin()
        {
            keys = new string[2];
        }
        public byte valid;
        [field: MarshalAs(UnmanagedType.ByValTStr, SizeConst = 10)]
        public string[] keys { get; set; } = new string[2];
    }

    // Keys Of Login
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class AuthKeyInfo
    {
        public byte valid;
        [field: MarshalAs(UnmanagedType.ByValTStr, SizeConst = 10)]
        public string key { get; set; }
    }


    // Auth m_key Login Info
    // Keys Of Login
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class AuthKeyLoginInfo :AuthKeyInfo
    {
    }
    // Auth m_key Game Info
    public class AuthKeyGameInfo : AuthKeyInfo
    {
        public int server_uid;
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public partial class CharacterInfo
    {
        public enum Stats : byte
        {
            S_POWER,
            S_CONTROL,
            S_ACCURACY,
            S_SPIN,
            S_CURVE,
        }
        public uint _typeid { get; set; }
        public uint id { get; set; }
        public byte default_hair { get; set; }
        public byte default_shirts { get; set; }
        public byte gift_flag { get; set; }
        public byte Purchase { get; set; }
        /// <summary>
        /// Parts typeid, do 1 ao 24
        /// </summary>
        [field: MarshalAs(UnmanagedType.ByValArray, SizeConst = 24)]
        public uint[] parts_typeid { get; set; }
        /// <summary>
        /// Parts id, do 1 ao 24
        /// </summary>
        [field: MarshalAs(UnmanagedType.ByValArray, SizeConst = 24)]
        public uint[] parts_id { get; set; }
        /// <summary>
        ///Não sei bem direito o que é aqui
        /// </summary>
        [field: MarshalAs(UnmanagedType.ByValArray, SizeConst = 216)]
        public byte[] Blank { get; set; }
        /// <summary>
        ///Auxiliar Parts 5, aqui fica anel
        /// </summary>
        [field: MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
        public uint[] AuxPart { get; set; }
        /// <summary>
        ///Cut-in, no primeiro mas acho que pode ser cut-in no resto
        /// </summary>
        [field: MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public uint[] Cut_in { get; set; }
        /// <summary>
        ///Aqui é o character stats, como controle, força, spin e etc
        /// </summary>
        [field: MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
        public byte[] PCL { get; set; }
        /// <summary>
        /// Mastery, que aumenta os slot do stats do character
        /// </summary>
        public uint MasteryPoint { get; set; }
        [field: MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public uint[] Card_Character { get; set; }				// 4 Slot de card Character
        [field: MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public uint[] Card_Caddie { get; set; }             // 4 Slot de card Caddie
        [field: MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public uint[] Card_NPC { get; set; }

        public void Init()
        {
            if (Card_NPC == null)
                Card_NPC = new uint[4];
            if (Card_Character == null)
                Card_Character = new uint[4];
            if (Card_Caddie == null)
                Card_Caddie = new uint[4];
            if (parts_id == null)
                parts_id = new uint[24];
            if (parts_typeid == null)
                parts_typeid = new uint[24];
            if (AuxPart == null)
                AuxPart = new uint[5];
            if (Blank == null)
                Blank = new byte[216];
            if (Cut_in == null)
                Cut_in = new uint[5];
            if (PCL == null)
                PCL = new byte[5];
        }

        public void initComboDef()
        {
            Init();
        }
    }

    #region User Info


    public class BlockFlag
    {
        public BlockFlag()
        {
            if (m_flag == null || (m_flag.ullFlag == 0))
            {
                m_flag = new uFlag(0);
            }

            m_id_state = new IDStateBlockFlag(0);
        }
        public void setIDState(UInt64 _id_state)
        {
            if (m_flag == null || (m_flag.ullFlag == 0))
            {
                m_flag = new uFlag(_id_state);
            }

            m_id_state = new IDStateBlockFlag(_id_state);

            // Block Recursos do player
            if ((m_id_state.id_state.st_IDState.L_BLOCK_LOUNGE/* & 4*/)) // Block Lounge
                m_flag.ullFlag = m_flag.stBit.lounge = 1u; // Block Lounge
            if ((m_id_state.id_state.st_IDState.L_BLOCK_SHOP_LOUNGE/* & 8*/)) // Block Shop Lounge
                m_flag.stBit.personal_shop = 1u; // Block Shop Lounge
            if ((m_id_state.id_state.st_IDState.L_BLOCK_GIFT_SHOP/* & 16*/)) // Block Gift Shop
                m_flag.stBit.gift_shop = 1u; // Block Gift Shop
            if ((m_id_state.id_state.st_IDState.L_BLOCK_PAPEL_SHOP/* & 32*/)) // Block Papel Shop
                m_flag.stBit.papel_shop = 1u; // Block Papel Shop
            if ((m_id_state.id_state.st_IDState.L_BLOCK_SCRATCHY/* & 64*/)) // Block Scratchy
                m_flag.stBit.scratchy = 1u; // Block Scratchy
            if ((m_id_state.id_state.st_IDState.L_BLOCK_TICKER/* & 128*/)) // Block Ticker
                m_flag.stBit.ticker = 1u; // Block Ticker
            if ((m_id_state.id_state.st_IDState.L_BLOCK_MEMORIAL_SHOP/* & 256*/)) // Block Memorial Shop
                m_flag.stBit.memorial_shop = 1u; // Block Memorial Shop
        }

        public IDStateBlockFlag m_id_state;
        public uFlag m_flag;
    }

    // ------------------ Player Account Basic ---------------- //
    // Struct ID State Block Flag
    public class IDStateBlockFlag
    {
        public IDStateBlockFlag(ulong _id_state, int _block_time = -1)
        {
            id_state = new _uIDState(_id_state);
            block_time = _block_time;
        }
        public IDStateBlockFlag()
        {
            id_state = new _uIDState();
            block_time = -1;
        }

        public class _uIDState
        {
            public _uIDState(ulong _ull = 0u)
            { ull_IDState = _ull; setState(); }
            public _uIDState()
            { ull_IDState = 0; setState(); }
            public void setState()
            {
                BitArray bits = new BitArray(BitConverter.GetBytes(ull_IDState));
                bits = EngineTools.PadToFullByte(bits);

                st_IDState = new _stIDState()
                {
                    L_BLOCK_TEMPORARY = bits.Get(0),
                    L_BLOCK_FOREVER = bits.Get(1),
                    L_BLOCK_LOUNGE = bits.Get(2),
                    L_BLOCK_SHOP_LOUNGE = bits.Get(3),
                    L_BLOCK_GIFT_SHOP = bits.Get(4),
                    L_BLOCK_PAPEL_SHOP = bits.Get(5),
                    L_BLOCK_SCRATCHY = bits.Get(6),
                    L_BLOCK_TICKER = bits.Get(7),
                    L_BLOCK_MEMORIAL_SHOP = ull_IDState == 256,
                    L_BLOCK_ALL_IP = false,
                    L_BLOCK_MAC_ADDRESS = false
                };
            }
             public class _stIDState
            {
                public bool L_BLOCK_TEMPORARY { get; set; } = true;
                public bool L_BLOCK_FOREVER { get; set; } = true;
                public bool L_BLOCK_LOUNGE { get; set; } = true;
                public bool L_BLOCK_SHOP_LOUNGE { get; set; } = true;
                public bool L_BLOCK_GIFT_SHOP { get; set; } = true;
                public bool L_BLOCK_PAPEL_SHOP { get; set; } = true;
                public bool L_BLOCK_SCRATCHY { get; set; } = true;
                public bool L_BLOCK_TICKER { get; set; } = true;
                public bool L_BLOCK_MEMORIAL_SHOP { get; set; } = true;
                public bool L_BLOCK_ALL_IP { get; set; } = true;           // Bloquea todo IP que o player logar
                public bool L_BLOCK_MAC_ADDRESS { get; set; } = true;      // Bloquea o MAC Address
            }
            public ulong ull_IDState;
            public _stIDState st_IDState;
        }
        public _uIDState id_state;
        public int block_time;
    }

    #endregion
}
