using GameServer.PangyaEnums;
using PangyaAPI.Network.Pangya_St;
using PangyaAPI.Utilities;
using PangyaAPI.Utilities.BinaryModels;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using BlockFlag = PangyaAPI.Network.Pangya_St.BlockFlag;
namespace GameServer.GameType
{
    /// <summary>
    /// define 
    /// </summary>
    public static class _Define
    {
        public const ulong EXPIRES_CACHE_TIME = 3 * 1000Ul; // 3 Segundos
        public const uint NUM_OF_EMAIL_PER_PAGE = 20u; // 20 Emails por p�gina
        public const uint LIMIT_OF_UNREAD_EMAIL = 300u; // 300 Emails n�o lidos que pode enviar para o player         
        public const uint UPDATE_TIME_INTERVALE_HOUR = 24u;
        public const long STDA_10_MICRO_PER_MICRO = 10L;
        public const long STDA_10_MICRO_PER_MILLI = STDA_10_MICRO_PER_MICRO * 1000L;
        public const long STDA_10_MICRO_PER_SEC = STDA_10_MICRO_PER_MILLI * 1000L;
        public const long STDA_10_MICRO_PER_MIN = STDA_10_MICRO_PER_SEC * 60L;
        public const long STDA_10_MICRO_PER_HOUR = STDA_10_MICRO_PER_MIN * 60L;
        public const long STDA_10_MICRO_PER_DAY = STDA_10_MICRO_PER_HOUR * 24L;

        public const byte DEFAULT_CHANNEL = byte.MaxValue; // channel invalid
        public const ushort DEFAULT_ROOM_ID = ushort.MaxValue; // room invalid

        public const uint CLEAR_10_DAILY_QUEST_TYPEID = 0x78800001; // Quest 10 clear daily quest
        public const uint ASSIST_ITEM_TYPEID = 0x1BE00016;
        public const uint GRAND_PRIX_TICKET = 0x1A000264;
        public const uint LIMIT_GRAND_PRIX_TICKET = 50; // Limit de Grand Prix Ticket que o player pode ter, chegou nesse limit não drop mais ele do hole
        public const uint MULLIGAN_ROSE_TYPEID = 0x1800000E;
        public const uint DEFAULT_COMET_TYPEID = 0x14000000;
        public const uint AIR_KNIGHT_SET = 0x10000000;
        public const uint CLUB_PATCHER_TYPEID = 0x1A00018F;
        public const uint MILAGE_POINT_TYPEID = 0x1A0002A7;
        public const uint TIKI_POINT_TYPEID = 0x1A0002A6;
        public const uint SPECIAL_SHUFFLE_COURSE_TICKET_TYPEID = 0x1A0000F7;
        public const uint PANG_POUCH_TYPEID = 0x1A000010;
        public const uint EXP_POUCH_TYPEID = 0x1A00015D;
        public const uint CP_POUCH_TYPEID = 0x1A000160;
        public const uint DECREASE_COMBO_VALUE = 3; // No JP é 10, no USA era 3
        public const float MEDIDA_PARA_YARDS = 0.3125f; // Usava 0.31251 Medida uinterna do pangya que no visual é o Yards
        // Icon Player Good(angel), Quiter 1 e 2 
        public const float GOOD_PLAYER_ICON = 3.0f;
        public const float QUITER_ICON_1 = 20.0f;
        public const float QUITER_ICON_2 = 30.0f;

        // Corta com toma, e corta com safety
        public static readonly uint[] active_item_cant_have_2_inveroty = { 402653229u, 402653231u };

        public const uint TROFEL_GM_EVENT_TYPEID = 0x2D0A3B00;

        public const byte cadie_cauldron_Hermes_random_id = 2;
        public const byte cadie_cauldron_Jester_random_id = 3;
        public const byte cadie_cauldron_Twilight_random_id = 4;
        public const int MS_NUM_MAPS = 22; // TH, US, KR is 20
        // !@ tempor�rio
        public const uint PREMIUM_TICKET_TYPEID = 0x1A100002u;
        // !@ tempor�rio
        public const uint PREMIUM_2_TICKET_TYPEID = 0x1A100003u;

        // !@ tempor�rio
        public const uint PREMIUM_BALL_TYPEID = 0x140000D8u;
        // !@ tempor�rio
        public const uint PREMIUM_2_BALL_TYPEID = 0x140000E9u; // Sakura (Premium)

        // !@ tempor�rio
        public const uint PREMIUM_2_CLUBSET_TYPEID = 0x100000F7u; // Rank D(0x1000005D), Rank S(0x1000006B), (Premium)

        // !@ tempor�rio
        public const uint PREMIUM_2_AUTO_CALIPER_TYPEID = 0x1A000040u;

        // !@ tempor�rio
        public const uint PREMIUM_2_MASCOT_TYPEID = 0x4000004Bu; // Lolo (Premium)

        public const uint TICKET_REPORT_SCROLL_TYPEID = 0x1A000042u;
        public const uint TICKET_REPORT_TYPEID = 0x1A000041u;
    }

    public partial class player_info
    {
        public uint uid { get; set; }
        public BlockFlag block_flag { get; set; }
        public short level { get; set; }
        public string id { get; set; }
        public string nickname { get; set; }
        public string pass { get; set; }
        public player_info()
        {
            block_flag = new BlockFlag();
            id = "";
            nickname = "";
            pass = "";
        }
    }
    public class stSyncUpdateDB
    {
        public enum eSTATE_UPDATE : byte
        {
            NONE,
            REQUEST_UPDATE,
            UPDATE_CONFIRMED,
            ERROR_UPDATE
        }

        private eSTATE_UPDATE m_state;
        private readonly object m_lock = new object();
        private readonly AutoResetEvent m_cv = new AutoResetEvent(false);

        public stSyncUpdateDB()
        {
            m_state = eSTATE_UPDATE.NONE;
        }

        public void requestUpdateOnDB()
        {
            int timeoutCount = 3; // 30 segundos check, para enviar nova requisição
            int timeoutMs = 10000; // 10 segundos de espera

            lock (m_lock)
            {
                if (m_state == eSTATE_UPDATE.REQUEST_UPDATE)
                {
                    while (m_state == eSTATE_UPDATE.REQUEST_UPDATE && timeoutCount > 0)
                    {
                        bool signaled = m_cv.WaitOne(timeoutMs);

                        if (!signaled)
                        {
                            timeoutCount--;
                        }
                    }

                    if (timeoutCount == 0)
                    {
                        // Log de advertência se o tempo expirar
                        Console.WriteLine("[SyncUpdateDB::RequestUpdateOnDB][WARNING] 30 segundos consumidos, mudança de estado forçada.");
                    }
                }

                // Atualiza o estado para REQUEST_UPDATE
                m_state = eSTATE_UPDATE.REQUEST_UPDATE;
            }
        }

        public void confirmUpdadeOnDB()
        {
            lock (m_lock)
            {
                if (m_state != eSTATE_UPDATE.REQUEST_UPDATE)
                {
                    throw new Exception("[SyncUpdateDB::ConfirmUpdateOnDB][Error] m_state está errado, não é REQUEST_UPDATE.");
                }

                // Atualiza o estado para UPDATE_CONFIRMED
                m_state = eSTATE_UPDATE.UPDATE_CONFIRMED;

                // Acorda as threads que estão esperando a condition variable
                m_cv.Set();
            }
        }

        public void errorUpdateOnDB()
        {
            lock (m_lock)
            {
                if (m_state != eSTATE_UPDATE.REQUEST_UPDATE)
                {
                    throw new Exception("[SyncUpdateDB::ErrorUpdateOnDB][Error] m_state está errado, não é REQUEST_UPDATE.");
                }

                // Atualiza o estado para ERROR_UPDATE
                m_state = eSTATE_UPDATE.ERROR_UPDATE;

                // Acorda as threads que estão esperando a condition variable
                m_cv.Set();
            }
        }
    }

    // Player Location para atualização do no banco de dados
    public class stPlayerLocationDB : stSyncUpdateDB
    {
        public stPlayerLocationDB(uint _ul = 0u)
        {
            clear();
        }
        
        ~stPlayerLocationDB()
        {
            clear();
        }

        void clear()
        {

            channel = 255;
            lobby = 255;
            room = 255;
            place =new PlayerPlace();
        }
        public byte channel;
        public byte lobby;
        public ushort room;
        public PlayerPlace place;   
    }

    public class PlayerPlace
    {
        public byte ulPlace; // armazena o estado das flags
                
        public PlayerPlace(byte ul = 0)
        {
            ulPlace = 0; // Começa sem flags ativas
        }

        public bool None
        {
            get => ulPlace == 0;
        }

        // Propriedade para o "MainLobby" (primeiro bit)
        public bool main_lobby /// ulPlace for igual a 1
        {
            get => ((PlaceFlags)ulPlace).HasFlag(PlaceFlags.MainLobby); // Verifica se o bit 1 está ativado
            set
            {
                if (value) ulPlace |= (byte)PlaceFlags.MainLobby; // Ativa o bit 0
                else ulPlace &= (byte)~PlaceFlags.MainLobby; // Desativa o bit 0
            }
        }

        // Propriedade para o "MiniGame" (segundo bit)
        public bool web_link_or_my_room
        {
            get => ((PlaceFlags)ulPlace).HasFlag(PlaceFlags.WebLinkOrMyRoom); // Verifica se o bit 1 está ativado
            set
            {
                if (value) ulPlace |= (byte)PlaceFlags.WebLinkOrMyRoom; // Ativa o bit 1
                else ulPlace &= (byte)~PlaceFlags.WebLinkOrMyRoom; // Desativa o bit 1
            }
        }

        // Propriedade para o "PlayRoom" (segundo e quarto bit)
        public bool game_play
        {
            get => ((PlaceFlags)ulPlace).HasFlag(PlaceFlags.GamePlay); // Verifica se o bit 1 está ativado
            set
            {
                if (value) ulPlace |= (byte)PlaceFlags.GamePlay; // Ativa os bits 1 e 3
                else ulPlace &= (byte)~PlaceFlags.GamePlay; // Desativa os bits 1 e 3
            }
        }
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"------------------- PLACE FLAGS -------------------");
            sb.AppendLine($"ulPlace: {ulPlace} (0x{ulPlace:X2})"); // Exibe o valor em decimal e hexadecimal

            // Verificando as flags ativadas e adicionando à string
            if (game_play) sb.AppendLine("GamePlay: True");
            if (main_lobby) sb.AppendLine("MainLobby: True");
            if (web_link_or_my_room) sb.AppendLine("WebLinkOrMyRoom: True");
            if (None) sb.AppendLine("None: True");
            sb.AppendLine($"---------------------------------------------------");

            return sb.ToString();
        }

    }

    // MemberInfo dados principais do player, tem id, nick, guild, level, exp, e etc)
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public class MemberInfo
    {
        public MemberInfo()
        {
            Clear();
        }

        public void Clear()
        {
            rank = new uint[3];
            id_bytes = new byte[22];
            nick_name_bytes = new byte[22];
            guild_name_bytes = new byte[17];
            guild_mark_img_bytes = new byte[12];
            ucUnknown35 = new byte[35];
            nick_NT_bytes = new byte[22];
            ucUnknown108 = new byte[106];
            capability = new uCapability();
            state_flag = new uMemberInfoStateFlag();
            papel_shop = new PlayerPapelShopInfo();
            oid = uint.MaxValue;
        }

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 22)]
        private byte[] id_bytes;
        public string id
        {
            get => id_bytes.GetString();
            set => id_bytes.SetString(value);
        }
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 22)]
        private byte[] nick_name_bytes;
        public string nick_name
        {
            get => nick_name_bytes.GetString();
            set => nick_name_bytes.SetString(value);
        }
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 17)]
        private byte[] guild_name_bytes;
        public string guild_name
        {
            get => guild_name_bytes.GetString();
            set => guild_name_bytes.SetString(value);
        }

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
        private byte[] guild_mark_img_bytes;
        public string guild_mark_img
        {
            get => guild_mark_img_bytes.GetString();
            set => guild_mark_img_bytes.SetString(value);
        }
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 35)]
        public byte[] ucUnknown35;
        public uint school;
        [field: MarshalAs(UnmanagedType.Struct)]
        public uCapability capability;
        public uint galleryUid;
        public uint oid;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public uint[] rank;
        public uint guild_uid;
        public uint guild_mark_img_no; // só tem no JP
        [field: MarshalAs(UnmanagedType.Struct)]
        public uMemberInfoStateFlag state_flag;
        public ushort flag_login_time;
        [field: MarshalAs(UnmanagedType.Struct)]
        public PlayerPapelShopInfo papel_shop;
        public uint point_point_event;         // S4 TH
        public ulong flag_block;                // S4 TH é 32 bytes é time_block, mas no Fresh UP JP o flag block do pacote principal é de 64, então não tem mais o time block
        public uint channeling_flag;			// S4 TH
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 22)]
        private byte[] nick_NT_bytes;
        public string nick_NT
        {
            get => nick_NT_bytes.GetString();
            set => nick_NT_bytes.SetString(value);
        }
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 106)]
        public byte[] ucUnknown108;

        /// <summary>
        /// Size = 297 bytes
        /// </summary>
        /// <returns></returns>
        public byte[] Build()
        {
            using (var p = new PangyaBinaryWriter())
            {
                p.WriteStr(id, 22);
                p.WriteStr(nick_name, 22);
                p.WriteStr(guild_name, 17);
                p.WriteStr(guild_mark_img, 12);
                p.WriteBytes(ucUnknown35, 35);      // ainda não sei direito o que tem aqui	, talvez seja o nome da escola!]
                p.WriteUInt32(school);          // ainda não o que é aqui direito
                p.WriteInt32(capability.ulCapability);
                p.WriteUInt32(galleryUid);          // S4 TH gallery m_uid, spectator m_uid
                p.WriteUInt32(oid);
                p.WriteUInt32(rank[0]);             // S4 TH rank]
                p.WriteUInt32(rank[1]);             // S4 TH rank]
                p.WriteUInt32(rank[2]);             // S4 TH rank]
                p.WriteUInt32(guild_uid);
                p.WriteUInt32(guild_mark_img_no);   // só tem no JP
                p.WriteByte(state_flag.ucByte);//state_flag.ucByte(falta saber das flags)
                p.WriteUInt16(flag_login_time);     // 1 é primeira vez que logou, 2 já não é mais a primeira vez que fez login no server
                p.WriteUInt16(papel_shop.remain_count);
                p.WriteUInt16(papel_shop.current_count);
                p.WriteUInt16(papel_shop.limit_count);
                p.WriteUInt32(point_point_event);           // S4 TH
                p.WriteUInt64(flag_block);              // S4 TH é 32 bytes é time_block, mas no Fresh UP JP o flag block do pacote principal é de 64, então não tem mais o time block
                p.WriteUInt32(channeling_flag);         // S4 TH
                p.WriteStr(nick_NT, 22);             // S4 TH
                p.WriteBytes(ucUnknown108, 106);                // S4 TH
                //if (p.GetSize == 297)
                //    Debug.WriteLine("Test Size MemberInfo Ok");

                return p.GetBytes;
            }
        }
    }

    // MemberInfoEx extendido tem o m_uid, limite papel shop e tutorial,
    // so os que nao manda para o pangya no pacote MemberInfo
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public class MemberInfoEx : MemberInfo
    {
        public MemberInfoEx()
        {
            Clear();
            papel_shop_last_update = new PangyaTime();
            papel_shop_last_update.CreateTime();
            sala_numero = _Define.DEFAULT_ROOM_ID;
        }
        public uint uid { get; set; }
        public uint guild_point { get; set; }
        public long guild_pang { get; set; }
        public ushort sala_numero { get; set; }
        public byte sexo;
        public byte level;
        public byte do_tutorial;
        public byte event_1;
        public byte event_2;
        public uint manner_flag;
        [field: MarshalAs(UnmanagedType.Struct, SizeConst = 16)]
        public PangyaTime papel_shop_last_update;
    }
    public class uCapability
    {
        private int _ulCapability;

        // Propriedade ulCapability com get e set utilizando operações bitwise
        public int ulCapability
        {
            get => _ulCapability;
            set
            {
                _ulCapability = value;
            }
        }

        // Flags de bit diretamente nos setters
        public bool A_I_MODE
        {
            get => (ulCapability & (uint)CapabilityFlags.A_I_MODE) != 0; // 0x01
            set
            {
                if (value) _ulCapability |= 1; // Ativar o bit
                else _ulCapability &= ~1; // Desativar o bit
            }
        }

        public bool gallery //TODO: # 2 n�o sei bem mas estava na flag que o mlk falou que era GM + PC Bang + Premium 
        {
            get => (_ulCapability & (int)CapabilityFlags.GALLERY) != 0; // 0x02
            set
            {
                if (value) _ulCapability |= 2; // Ativar o bit
                else _ulCapability &= ~2; // Desativar o bit
            }
        }

        public bool game_master
        {
            get => (_ulCapability & (int)CapabilityFlags.GAME_MASTER) != 0; // 0x04
            set
            {
                if (value) _ulCapability |= 4; // Ativar o bit
                else _ulCapability &= ~4; // Desativar o bit
            }
        }

        public bool gm_edit_site
        {
            get => (_ulCapability & (int)CapabilityFlags.GM_EDIT_SITE) != 0; // 0x08
            set
            {
                if (value) _ulCapability |= 8; // Ativar o bit
                else _ulCapability &= ~8; // Desativar o bit
            }
        }

        public bool observer
        {
            get => (_ulCapability & 14) == 14; // 
            set
            {
                if (value) _ulCapability |= 14; // 
                else _ulCapability &= ~14; // 
            }
        }



        public bool unknown_1
        {
            get => (_ulCapability & (int)CapabilityFlags.UNKNOWN_1) != 0; // 0x40
            set
            {
                if (value) _ulCapability |= (int)CapabilityFlags.UNKNOWN_1; // Ativar o bit
                else _ulCapability &= ~(int)CapabilityFlags.UNKNOWN_1; // Desativar o bit
            }
        }

        public bool block_give_item_gm
        {
            get => (_ulCapability & 16) != 0; // 0x40
            set
            {
                if (value) _ulCapability |= 16; // Ativar o bit
                else _ulCapability &= ~16; // Desativar o bit
            }
        }

        public bool mod_system_event  //pode esta errado
        {
            get => (_ulCapability & 64) != 0; // 0x40
            set
            {
                if (value) _ulCapability |= 64; // Ativar o bit
                else _ulCapability &= ~64; // Desativar o bit
            }
        }

        public bool gm_normal
        {
            get => (_ulCapability & 128) != 0; // 0x80
            set
            {
                if (value) _ulCapability |= 128; // Ativar o bit
                else _ulCapability &= ~128; // Desativar o bit
            }
        }

        public bool block_gift_shop
        {
            get => (_ulCapability & (int)CapabilityFlags.BLOCK_GIFT_SHOP) != 0; // 0x100
            set
            {
                if (value) _ulCapability |= 256; // Ativar o bit
                else _ulCapability &= ~256; // Desativar o bit
            }
        }

        public bool login_test_server
        {
            get => (_ulCapability & (int)CapabilityFlags.LOGIN_TEST_SERVER) != 0; // 0x200
            set
            {
                if (value) _ulCapability |= 512; // Ativar o bit
                else _ulCapability &= ~512; // Desativar o bit
            }
        }

        public bool mantle
        {
            get => (_ulCapability & (int)CapabilityFlags.MANTLE) != 0; // 0x400
            set
            {
                if (value) _ulCapability |= 1024; // Ativar o bit
                else _ulCapability &= ~1024; // Desativar o bit
            }
        }

        public bool unknown3
        {
            get => (_ulCapability & 2048) != 0; // 0x800
            set
            {
                if (value) _ulCapability |= 2048; // Ativar o bit
                else _ulCapability &= ~2048; // Desativar o bit
            }
        }

        public bool premium_user
        {
            get => (_ulCapability & (int)CapabilityFlags.PREMIUM_USER) != 0; // 0x02
            set
            {
                if (value) _ulCapability |= 16384; // Ativar o bit
                else _ulCapability &= ~16384; // Desativar o bit
            }
        }

        public bool title_gm
        {
            get => (_ulCapability & (int)CapabilityFlags.TITLE_GM) != 0; // 0x1000
            set
            {
                if (value) _ulCapability |= 32768; // Ativar o bit
                else _ulCapability &= ~32768; // Desativar o bit
            }
        }

        // Construtores
        public uCapability()
        {
            _ulCapability = 0;
        }

        public uCapability(int ul)
        {
            _ulCapability = ul;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"-------------------CAP------------------------");

            sb.AppendLine($"Capability: {ulCapability}");
            // Verificando as flags ativadas e adicionando à string
            if (A_I_MODE) sb.AppendLine("A_I_MODE: True");
            if (gallery) sb.AppendLine("gallery: True");
            if (game_master) sb.AppendLine("game_master: True");
            if (gm_edit_site) sb.AppendLine("gm_edit_site: True");
            if (observer) sb.AppendLine("observer: True");
            if (unknown_1) sb.AppendLine("unknown_1: True");
            if (block_give_item_gm) sb.AppendLine("block_give_item_gm: True");
            if (mod_system_event) sb.AppendLine("mod_system_event: True");
            if (gm_normal) sb.AppendLine("gm_normal: True");
            if (block_gift_shop) sb.AppendLine("block_gift_shop: True");
            if (login_test_server) sb.AppendLine("login_test_server: True");
            if (mantle) sb.AppendLine("mantle: True");
            if (unknown3) sb.AppendLine("unknown3: True");
            if (premium_user) sb.AppendLine("premium_user: True");
            if (title_gm) sb.AppendLine("title_gm: True");

            if (sb.Length == 0)
            {
                sb.AppendLine("Nenhuma flag ativada.");
            }
            sb.AppendLine($"-------------------------------------------");

            return sb.ToString();
        }
    }
    public class uMemberInfoStateFlag
    {
        public byte ucByte { get; set; }
        public bool channel
        {
            get => (ucByte & 0x01) != 0;
            set => ucByte = (byte)(value ? (ucByte | 0x01) : (ucByte & ~0x01));
        }          // channel
        public bool visible
        {
            get => (ucByte & 0x02) != 0;
            set => ucByte = (byte)(value ? (ucByte | 0x02) : (ucByte & ~0x02));
        }          // Visible
        public bool whisper
        {
            get => (ucByte & 0x04) != 0;
            set => ucByte = (byte)(value ? (ucByte | 0x04) : (ucByte & ~0x04));
        }          // Whisper
        public bool sexo
        {
            get => (ucByte & 0x08) != 0;
            set => ucByte = (byte)(value ? (ucByte | 0x08) : (ucByte & ~0x08));
        }             // Genero - (ACHO)Já logou mais de uma vez
        public bool azinha
        {
            get => (ucByte & 0x10) != 0;
            set => ucByte = (byte)(value ? (ucByte | 0x10) : (ucByte & ~0x10));
        }           // Azinha, Quit rate menor que 3%
        public bool icon_angel
        {
            get => (ucByte & 0x20) != 0;
            set => ucByte = (byte)(value ? (ucByte | 0x20) : (ucByte & ~0x20));
        }       // Angel Wings
        public bool quiter_1
        {
            get => (ucByte & 0x40) != 0;
            set => ucByte = (byte)(value ? (ucByte | 0x40) : (ucByte & ~0x40));
        }         // Quit rate maior que 31% e menor que 41%
        public bool quiter_2
        {
            get => (ucByte & 0x80) != 0;
            set => ucByte = (byte)(value ? (ucByte | 0x80) : (ucByte & ~0x80));
        }         // Quit rate maior que 41%
    }

    // Player Papel Shop Info
    [StructLayout(LayoutKind.Sequential, Pack = 4, Size = 6)]
    public class PlayerPapelShopInfo
    {
        [field: MarshalAs(UnmanagedType.U2, SizeConst = 2)]
        public short remain_count;
        [field: MarshalAs(UnmanagedType.U2, SizeConst = 2)]
        public short current_count;
        [field: MarshalAs(UnmanagedType.U2, SizeConst = 2)]
        public short limit_count;
        public PlayerPapelShopInfo()
        {
            remain_count = -1;
            current_count = -1;
            limit_count = -1;
        }
    }

    // Medal Win
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public class uMedalWin
    {
        [field: MarshalAs(UnmanagedType.U1, SizeConst = 1)]
        public byte ucMedal { get; set; }
        public _stMedal stMedal { get; set; }
        public uMedalWin()
        {
            stMedal = new _stMedal();
        }
        public class _stMedal
        {
            public byte lucky = 1;
            public byte speediest = 1;
            public byte best_drive = 1;
            public byte best_chipin = 1;
            public byte best_long_puttin = 1;
            public byte best_recovery = 0;
        }
    }
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public class UserInfo
    {
        public UserInfo(uint _ul = 0u)
        {
            clear();
        }
        public void clear()
        {
            best_pang = new long[5];
            best_score = new byte[5];
            medal = new stMedal();
        }
        public void add(UserInfo _ui)
        {

            if (_ui.best_drive > best_drive)
                best_drive = _ui.best_drive;

            if (_ui.best_long_putt > best_long_putt)
                best_long_putt = _ui.best_long_putt;

            if (_ui.best_chip_in > best_chip_in)
                best_chip_in = _ui.best_chip_in;

            // Combo e Todal Combos
            if (_ui.combo < 0)
            {   // Negativo

                // tira só do combo, não de todos os combos que foram feitos
                if (combo <= _Define.DECREASE_COMBO_VALUE)
                    combo = 0;
                else
                    combo += _ui.combo;

            }
            else
            {                   // Positivo

                combo += _ui.combo;

                // Só soma o all combo se combo > que all_combo
                if (combo > all_combo)
                    all_combo += _ui.combo;
            }

            // Event Angel ativado, quitado < 0
            if (_ui.quitado < 0)
            {

                // Se for 0 não subtrai
                if ((quitado + _ui.quitado) <= 0)
                    quitado = 0;
                else
                    quitado += _ui.quitado;

            }
            else // Normal soma o quit do player se ele quitou
                quitado += _ui.quitado;

            // Skin (Pang Battle)
            if ((skin_all_in_count + _ui.skin_all_in_count) >= 5)
            {

                skin_all_in_count = 0;
                skin_pang += 1000; // dá 1000 pangs por que ele jogou 5 Pang Battle

            }
            else
                skin_all_in_count += _ui.skin_all_in_count;

            tacada += _ui.tacada;
            putt += _ui.putt;
            tempo += _ui.tempo;
            tempo_tacada += _ui.tempo_tacada;
            acerto_pangya += _ui.acerto_pangya;
            timeout += _ui.timeout;
            ob += _ui.ob;
            total_distancia += _ui.total_distancia;
            hole += _ui.hole;
            hole_in += (_ui.hole - _ui.hole_in);
            hio += _ui.hio;
            bunker += _ui.bunker;
            fairway += _ui.fairway;
            albatross += _ui.albatross;
            putt_in += _ui.putt_in;
            media_score += _ui.media_score;
            best_score[0] += _ui.best_score[0];
            best_score[1] += _ui.best_score[1];
            best_score[2] += _ui.best_score[2];
            best_score[3] += _ui.best_score[3];
            best_score[4] += _ui.best_score[4];
            best_pang[0] += _ui.best_pang[0];
            best_pang[1] += _ui.best_pang[1];
            best_pang[2] += _ui.best_pang[2];
            best_pang[3] += _ui.best_pang[3];
            best_pang[4] += _ui.best_pang[4];
            sum_pang += _ui.sum_pang;
            event_flag += _ui.event_flag;
            jogado += _ui.jogado;
            team_game += _ui.team_game;
            team_win += _ui.team_win;
            team_hole += _ui.team_hole;
            ladder_point += _ui.ladder_point;
            ladder_hole += _ui.ladder_hole;
            ladder_win += _ui.ladder_win;
            ladder_lose += _ui.ladder_lose;
            ladder_draw += _ui.ladder_draw;
            skin_pang += _ui.skin_pang;
            skin_win += _ui.skin_win;
            skin_lose += _ui.skin_lose;
            skin_run_hole += _ui.skin_run_hole;
            //skin_all_in_count += _ui.skin_all_in_count; // aqui adiciona lá em cima, por que ele reseta em 5
            skin_strike_point += _ui.skin_strike_point;
            disconnect += _ui.disconnect;
            jogados_disconnect += _ui.jogados_disconnect;
            event_value += _ui.event_value;
            sys_school_serie += _ui.sys_school_serie;
            game_count_season += _ui.game_count_season;

            // Medal
            medal.add(_ui.medal);

        }
        public int tacada { get; set; }
        public int putt { get; set; }
        public int tempo { get; set; }
        public int tempo_tacada { get; set; }
        public float best_drive { get; set; }           // Max Distancia
        public int acerto_pangya { get; set; }
        public int timeout { get; set; }
        public int ob { get; set; }
        public int total_distancia { get; set; }
        public int hole { get; set; }
        public int hole_in { get; set; }        // Aqui é os holes que não foram concluídos Ex: Give up, ou no Match o outro player ganho sem precisar do player terminar o hole
        public int hio { get; set; }
        public short bunker { get; set; }
        public int fairway { get; set; }
        public int albatross { get; set; }
        public int mad_conduta { get; set; }    // Aqui é hole in, mas no info não tras ele por que ele já foi salvo no hole alí em cima
        public int putt_in { get; set; }
        public float best_long_putt { get; set; }
        public float best_chip_in { get; set; }
        public uint exp { get; set; }
        public byte level { get; set; }
        public UInt64 pang { get; set; }
        public int media_score { get; set; }
        [field: MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
        public byte[] best_score { get; set; }              // Best Score Por Estrela, mas acho que o pangya nao usa mais isso
        public byte event_flag { get; set; }
        [field: MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
        public Int64[] best_pang { get; set; }          // Best Pang por Estrela, mas acho que o pangya nao usa mais isso
        public Int64 sum_pang { get; set; }             // A soma do pangs das 5 estrela acho
        public int jogado { get; set; }
        public int team_hole { get; set; }
        public int team_win { get; set; }
        public int team_game { get; set; }
        public int ladder_point { get; set; }               // Ladder é o Match acho, de tourneio não sei direito
        public int ladder_hole { get; set; }
        public int ladder_win { get; set; }
        public int ladder_lose { get; set; }
        public int ladder_draw { get; set; }
        public int combo { get; set; }
        public int all_combo { get; set; }
        public int quitado { get; set; }
        public long skin_pang { get; set; }         // Skin é o Pang Battle tem valor negativo ele """##### Ajeitei agora(ACHO)
        public int skin_win { get; set; }
        public int skin_lose { get; set; }
        public int skin_all_in_count { get; set; }
        public int skin_run_hole { get; set; }              // Correu desistiu (ACHO)
        public int skin_strike_point { get; set; }          // Antes era o nao_sei
        public int jogados_disconnect { get; set; }     // Antes era o jogos_nao_sei
        public short event_value { get; set; }
        public int disconnect { get; set; }             // Vou deixar aqui o disconect count (antes era skin_strike_point)
        [field: MarshalAs(UnmanagedType.Struct, SizeConst = 6)]
        public stMedal medal { get; set; }
        public int sys_school_serie { get; set; }           // Sistema antigo do pangya JP que era de Serie de escola, respondia as perguntas se passasse ia pra outra serie é da 1° a 5°
        public int game_count_season { get; set; }
        public short _16bit_nao_sei { get; set; }
        public float getMediaScore()
        {   // AVG SCORE

            // Verifica se é 0, por que não pode dividir 18 por 0 que dá excessão, 
            // por que não pode dividir nenhum número por 0
            if ((hole - hole_in) == 0)
                return 0;

            return (18 / (hole - hole_in)) * media_score + 72;
        }
        public float getPangyaShotRate()
        {

            // Previne divisão por 0
            if (tacada == 0)
                return 0;

            return ((float)acerto_pangya / tacada) * 100;
        }
        public float getFairwayRate()
        {

            // Previne divisão por 0
            if ((hole - hole_in) == 0)
                return 0;

            return ((float)fairway / (hole - hole_in)) * 100;
        }
        public float getPuttRate()
        {

            // Previne divisão por 0
            if (putt == 0)
                return 0;

            return ((float)putt_in / putt) * 100;
        }
        public float getOBRate()
        {

            // Previne divisão por 0
            if ((tacada + putt) == 0)
                return 0;

            return ((float)ob / (tacada + putt)) * 100;
        }
        public float getMatchWinRate()
        {

            // Previne divisão por 0
            if (team_game == 0)
                return 0;

            return ((float)team_win / team_game) * 100;
        }
        public float getShotTimeRate()
        {

            // Previne divisão por 0
            if ((tacada + putt) == 0)
                return 0;

            return ((float)tempo_tacada / (tacada + putt)) * 100;
        }

        public float getQuitRate()
        {

            // Previne divisão por 0
            if (jogado == 0)
                return 0;

            return quitado * 100 / jogado;
        }

        public new string ToString()
        {
            return "Tacada: " + (tacada) + "  Putt: " + (putt) + "  Tempo: " + (tempo) + "  Tempo Tacada: " + (tempo_tacada)
                + "  Best drive: " + (best_drive) + "  Acerto pangya: " + (acerto_pangya) + "  timeout: " + (timeout)
                + "  OB: " + (ob) + "  Total distancia: " + (total_distancia) + "  hole: " + (hole)
                + "  Hole in: " + (hole_in) + "  HIO: " + (hio) + "  Bunker: " + (bunker) + "  Fairway: " + (fairway)
                + "  Albratross: " + (albatross) + "  Mad conduta: " + (mad_conduta) + "  Putt in: " + (putt_in)
                + "  Best long puttin: " + (best_long_putt) + "  Best chipin: " + (best_chip_in) + "  Exp: " + (exp)
                + "  Level: " + (level) + "  Pang: " + (pang) + "  Media score: " + (media_score)
                + "  Best score[" + (best_score[0]) + ", " + (best_score[1]) + ", " + (best_score[2])
                + ", " + (best_score[3]) + ", " + (best_score[4]) + "]  Event flag: " + (event_flag)
                + "  Best pang[" + (best_pang[0]) + ", " + (best_pang[1]) + ", " + (best_pang[2]) + ", " + (best_pang[3])
                + ", " + (best_pang[4]) + "]  Soma pang: " + (sum_pang) + "  Jogado: " + (jogado) + "  Team Hole: " + (team_hole)
                + "  Team win: " + (team_win) + "  Team game: " + (team_game) + "  Ladder point: " + (ladder_point)
                + "  Ladder hole: " + (ladder_hole) + "  Ladder win: " + (ladder_win) + "  Ladder lose: " + (ladder_lose)
                + "  Ladder draw: " + (ladder_draw) + "  Combo: " + (combo) + "  All combo: " + (all_combo)
                + "  Quitado: " + (quitado) + "  Skin pang: " + (skin_pang) + "  Skin win: " + (skin_win)
                + "  Skin lose: " + (skin_lose) + "  Skin all in count: " + (skin_all_in_count) + "  Skin run hole: " + (skin_run_hole)
                + "  Disconnect(MY): " + (disconnect) + "  Jogados Disconnect(MY): " + (jogados_disconnect) + "  Event value: " + (event_value)
                + "  Skin Strike Point: " + (skin_strike_point) + "  Sistema School Serie: " + (sys_school_serie)
                + "  Game count season: " + (game_count_season) + "  _16bit nao sei: " + (_16bit_nao_sei);
        }
        /// <summary>
        /// Size = 265 Bytes
        /// </summary>
        /// <returns></returns>
        public byte[] Build()
        {
            using (var p = new PangyaBinaryWriter())
            {
                p.WriteInt32(tacada);
                p.WriteInt32(putt);
                p.WriteInt32(tempo);
                p.WriteInt32(tempo_tacada);
                p.Write(best_drive);           // Max Distancia
                p.WriteInt32(acerto_pangya);
                p.WriteInt32(timeout);
                p.WriteInt32(ob);
                p.WriteInt32(total_distancia);
                p.WriteInt32(hole);
                p.WriteInt32(hole_in);        // Aqui é os holes que não foram concluídos Ex: Give up, ou no Match o outro player ganho sem precisar do player terminar o hole
                p.WriteInt32(hio);
                p.WriteInt16(bunker);
                p.WriteInt32(fairway);
                p.WriteInt32(albatross);
                p.WriteInt32(mad_conduta);    // Aqui é hole in, mas no info não tras ele por que ele já foi salvo no hole alí em cima
                p.WriteInt32(putt_in);
                p.Write(best_long_putt);
                p.Write(best_chip_in);
                p.WriteUInt32(exp);
                p.WriteByte(level);
                p.WriteUInt64(pang);
                p.WriteInt32(media_score);
                for (int i = 0; i < 5; i++)
                    p.WriteByte(best_score[i]);              // Best Score Por Estrela, mas acho que o pangya nao usa mais isso
                p.WriteByte(event_flag);
                for (int i = 0; i < 5; i++)
                    p.WriteInt64(best_pang[i]);          // Best Pang por Estrela, mas acho que o pangya nao usa mais isso
                p.WriteInt64(sum_pang);             // A soma do pangs das 5 estrela acho
                p.WriteInt32(jogado);
                p.WriteInt32(team_hole);
                p.WriteInt32(team_win);
                p.WriteInt32(team_game);
                p.WriteInt32(ladder_point);               // Ladder é o Match acho, de tourneio não sei direito
                p.WriteInt32(ladder_hole);
                p.WriteInt32(ladder_win);
                p.WriteInt32(ladder_lose);
                p.WriteInt32(ladder_draw);
                p.WriteInt32(combo);
                p.WriteInt32(all_combo);
                p.WriteInt32(quitado);
                p.WriteInt64(skin_pang);         // Skin é o Pang Battle tem valor negativo ele """##### Ajeitei agora(ACHO)
                p.WriteInt32(skin_win);
                p.WriteInt32(skin_lose);
                p.WriteInt32(skin_all_in_count);
                p.WriteInt32(skin_run_hole);              // Correu desistiu (ACHO)
                p.WriteInt32(skin_strike_point);          // Antes era o nao_sei
                p.WriteInt32(jogados_disconnect);     // Antes era o jogos_nao_sei
                p.WriteInt16(event_value);
                p.WriteInt32(disconnect);             // Vou deixar aqui o disconect count (antes era skin_strike_point)
                p.WriteStruct(medal, new stMedal());
                p.WriteInt32(sys_school_serie);           // Sistema antigo do pangya JP que era de Serie de escola, respondia as perguntas se passasse ia pra outra serie é da 1° a 5°
                p.WriteInt32(game_count_season);
                p.WriteInt16(_16bit_nao_sei);
                //if (p.GetSize == 265)
                //    Debug.WriteLine("UserInfo Size Okay");

                return p.GetBytes;
            }
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public class UserInfoEx : UserInfo
    {
        public UserInfoEx(int ul = 0) : base()
        { }
        public void add(UserInfoEx _ui, ulong _total_pang_win_game)
        {
            base.add(_ui);
            if (_total_pang_win_game > 0)
                total_pang_win_game += _total_pang_win_game;
        }
        public ulong total_pang_win_game { get; set; }
    }

    // Medal
    [StructLayout(LayoutKind.Sequential, Pack = 4, Size = 24)]
    public class stMedal
    {
        public void add(stMedal _medal)
        {
            lucky += _medal.lucky;
            fast += _medal.fast;
            best_drive += _medal.best_drive;
            best_chipin += _medal.best_chipin;
            best_puttin += _medal.best_puttin;
            best_recovery += _medal.best_recovery;
        }
        public void add(uMedalWin _medal_win)
        {
            if (_medal_win.stMedal.lucky == 1)
                lucky++;
            else if (_medal_win.stMedal.speediest == 1)
                fast++;
            else if (_medal_win.stMedal.best_drive == 1)
                best_drive++;
            else if (_medal_win.stMedal.best_chipin == 1)
                best_chipin++;
            else if (_medal_win.stMedal.best_long_puttin == 1)
                best_puttin++;
            else if (_medal_win.stMedal.best_recovery == 1)
                best_recovery++;
        }
        public int lucky { get; set; }
        public int fast { get; set; }
        public int best_drive { get; set; }
        public int best_chipin { get; set; }
        public int best_puttin { get; set; }
        public int best_recovery { get; set; }
    }
    /// <summary>
    /// System time public classure based on Windows uinternal SYSTEMTIME public class
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 4, Size = 16)]
    public class PangyaTime
    {
        /// <summary>
        /// Year
        /// </summary>
        public ushort Year { get; set; }

        /// <summary>
        /// Month
        /// </summary>
        public ushort Month { get; set; }

        /// <summary>
        /// Day of Week
        /// </summary>
        public ushort DayOfWeek { get; set; }

        /// <summary>
        /// Day
        /// </summary>
        public ushort Day { get; set; }

        /// <summary>
        /// Hour
        /// </summary>
        public ushort Hour { get; set; }

        /// <summary>
        /// Minute
        /// </summary>
        public ushort Minute { get; set; }

        /// <summary>
        /// Second
        /// </summary>
        public ushort Second { get; set; }

        /// <summary>
        /// Millisecond
        /// </summary>
        public ushort MilliSecond { get; set; }

        public bool TimeActive
        {
            get
            {
                return Year > 0 && Month > 0 && DayOfWeek > 0 && Day > 0 && Hour > 0 && Minute > 0 && Second > 0 && MilliSecond > 0;
            }
        }

        public bool IsEmpty
        {
            get
            {
                return Year == 0 && Month == 0 && DayOfWeek == 0 && Day == 0 && Hour == 0 && Minute == 0 && Second == 0 && MilliSecond == 0;
            }
        }

        public DateTime ConvertTime()
        {
            return new DateTime(Year, Month, Day, Hour, Minute, Second, MilliSecond);
        }

        public void CreateTime(string format)
        {
            var date = DateTime.Parse(format);

            Year = (ushort)date.Year;
            Month = (ushort)date.Month;
            Minute = (ushort)date.Minute;
            Day = (ushort)date.Day;
            Hour = (ushort)date.Hour;
            Second = (ushort)date.Second;
            MilliSecond = (ushort)date.Millisecond;
        }

        public void CreateTime()
        {
            var date = DateTime.Now;

            Year = (ushort)date.Year;
            Month = (ushort)date.Month;
            Minute = (ushort)date.Minute;
            Day = (ushort)date.Day;
            Hour = (ushort)date.Hour;
            Second = (ushort)date.Second;
            MilliSecond = (ushort)date.Millisecond;
        }

        public void CreateTime(DateTime date)
        {
            if (date != DateTime.MinValue)
            {

                Year = (ushort)date.Year;
                Month = (ushort)date.Month;
                Minute = (ushort)date.Minute;
                Day = (ushort)date.Day;
                Hour = (ushort)date.Hour;
                Second = (ushort)date.Second;
                MilliSecond = (ushort)date.Millisecond;

            }
        }

        public void Clear()
        {
            Year = 0;
            Month = 0;
            Minute = 0;
            Day = 0;
            Hour = 0;
            Second = 0;
            MilliSecond = 0;
        }
        public PangyaTime(int init = 0)
        {
            if (init == 1)
                CreateTime();
        }
        public PangyaTime()
        {
        }

        public PangyaTime(ushort _Year, ushort _Month = 0, ushort _Day = 0, ushort _Hour = 0, ushort _Minute = 0, ushort _Second = 0, ushort _Millisecond = 0)
        {
            Year = _Year;
            Month = _Month;
            Minute = _Minute;
            Day = _Day;
            Hour = _Hour;
            Second = _Second;
            MilliSecond = _Millisecond;
        }
        public PangyaTime(ushort _Year, ushort _Month = 0, ushort _Day = 0, ushort _DayOfWeek = 0, ushort _Hour = 0, ushort _Minute = 0, ushort _Second = 0, ushort _Millisecond = 0)
        {
            Year = _Year;
            Month = _Month;
            Minute = _Minute;
            Day = _Day;
            DayOfWeek = _DayOfWeek;
            Hour = _Hour;
            Second = _Second;
            MilliSecond = _Millisecond;
        }

    }

    public class CouponGacha
    {
        public uint partial_ticket;
        public uint normal_ticket;
    }
    // Counter Item Info
    public class CounterItemInfo
    {
        public CounterItemInfo(uint _ul = 0u)
        {
            clear();
        }
        public void clear()
        {
        }
        public bool isValid()
        {
            return (id > 0 && _typeid != 0);
        }
        public byte active;
        public uint _typeid = new uint();
        public uint id = 0;
        public uint value = 0;
    }

    // Quest Stuff Info
    public class QuestStuffInfo
    {
        public void clear()
        {
        }
        public bool isValid()
        {
            return (id > 0 && _typeid != 0);
        }
        public uint id = 0;
        public uint _typeid = new uint();
        public uint counter_item_id = 0;
        public uint clear_date_unix = new uint();
    }
    public class AchievementInfo : IDisposable
    {
        public enum AchievementStatus : byte
        {
            Pending = 1,
            Excluded,
            Active,
            Concluded
        }

        public void Dispose()
        {
            // Implementação necessária, caso existam recursos não gerenciados
        }

        public void Clear()
        {
            active = 0;
            _typeid = 0;
            id = 0;
            status = 0;

            v_qsi.Clear();
            map_counter_item.Clear();
        }

        public CounterItemInfo FindCounterItemById(uint id)
        {
            if (id == 0)
                throw new Exception("[AchievementInfo::FindCounterItemById][Error] id is invalid");

            return map_counter_item.TryGetValue(id, out var counterItem) ? counterItem : null;
        }

        public CounterItemInfo FindCounterItemByTypeId(uint typeId)
        {
            if (typeId == 0)
                throw new Exception("[AchievementInfo::FindCounterItemByTypeId][Error] typeId is invalid");

            foreach (var item in map_counter_item.Values)
            {
                if (item._typeid == typeId)
                    return item;
            }

            return null;
        }

        public QuestStuffInfo FindQuestStuffById(uint id)
        {
            if (id == 0)
                throw new Exception("[AchievementInfo::FindQuestStuffById][Error] id is invalid");

            foreach (var quest in v_qsi)
            {
                if (quest.id == id)
                    return quest;
            }

            return null;
        }

        public QuestStuffInfo FindQuestStuffByTypeId(uint typeId)
        {
            if (typeId == 0)
                throw new Exception("[AchievementInfo::FindQuestStuffByTypeId][Error] typeId is invalid");

            foreach (var quest in v_qsi)
            {
                if (quest._typeid == typeId)
                    return quest;
            }

            return null;
        }

        public uint AddCounterByTypeId(uint typeId, uint value)
        {
            if (typeId == 0)
                throw new Exception("[AchievementInfo::AddCounterByTypeId][Error] typeId is invalid");

            uint count = 0;
            foreach (var quest in v_qsi)
            {
                if (quest.clear_date_unix == 0)
                {
                    var counterItem = FindCounterItemById(quest.counter_item_id);
                    if (counterItem != null && counterItem._typeid == typeId)
                    {
                        counterItem.value += value;
                        count++;
                    }
                }
            }

            return count;
        }

        public bool CheckAllQuestClear()
        {
            foreach (var quest in v_qsi)
            {
                if (quest.clear_date_unix == 0)
                    return false;
            }

            return true;
        }

        public byte active;
        public uint _typeid = 0;
        public uint id = 0;
        public uint status = 0; // 1 pendente, 2 excluído, 3 ativo, 4 concluído
        public Dictionary<uint, CounterItemInfo> map_counter_item = new Dictionary<uint, CounterItemInfo>();
        public List<QuestStuffInfo> v_qsi = new List<QuestStuffInfo>();
    }


    // Achievement Info Ex
    public class AchievementInfoEx : AchievementInfo
    {
        public AchievementInfoEx() : base()
        {
            clear();
        }
        public void clear()
        {
            quest_base_typeid = 0;
        }
        // A ultima quest do Achievement que tem o counter item adicionado no db e depois replica para os outro que nao foi concluído
        // Se não tiver cria um counter item para todas as quest
        public uint quest_base_typeid = new uint();
        public List<QuestStuffInfo>.Enumerator getQuestBase()
        {

            if (quest_base_typeid == 0)
            {
                return v_qsi.ToList().GetEnumerator();
            }

            return v_qsi.Where(c => c._typeid == quest_base_typeid).ToList().GetEnumerator();
        }
    }

    // Premium Ticket User
    public class PremiumTicket
    {
        public uint id;
        public uint _typeid;
        public uint unix_sec_date;
        public uint unix_end_date;
    }

    // Request Info
    public class RequestInfo
    {
        public uint uid;
        public byte season;
        public byte show;     // 12 pacotes enviados pode enviar o pacote089
    }

    // Itens Equipado do Player
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public class EquipedItem
    {
        public CharacterInfo char_info { get; set; }
        public CaddieInfoEx cad_info { get; set; }
        public MascotInfoEx mascot_info { get; set; }
        public ClubSetInfo csi { get; set; }
        public WarehouseItem comet { get; set; }
        public WarehouseItem clubset { get; set; }

        public EquipedItem()
        { cad_info = new CaddieInfoEx(); csi = new ClubSetInfo(); comet = new WarehouseItem(); clubset = new WarehouseItem(); mascot_info = new MascotInfoEx(); }

        /// <summary>
        /// Character(513 bytes), Caddie(28 bytes), ClubSet(28 bytes), Mascot(62 bytes), Total Size 628 
        /// </summary>
        /// <returns>Equiped Item(628 array of byte)</returns>
        public byte[] Build()
        {
            using (var p = new PangyaBinaryWriter())
            {
                if (char_info != null && char_info.id != 0)
                {
                    p.WriteBytes(char_info.Build());
                }
                else
                {
                    p.WriteZero(513);
                }

                // CWriteie Info
                if (cad_info != null && cad_info.id != 0)
                {
                    p.WriteBytes(cad_info.Build());
                }
                else
                {
                    p.WriteZero(25);
                }

                // Club Set Info
                if (clubset != null && clubset.id != 0)
                {
                    p.WriteBytes(csi.Build());
                }
                else
                {
                    p.WriteZero(28);
                }

                // Mascot Info
                if (mascot_info != null && mascot_info.id != 0)
                {
                    p.WriteBytes(mascot_info.Build());
                }
                else
                {
                    p.WriteZero(62);
                }


                //if (p.GetSize == 628)
                //    Debug.WriteLine("Equiped Item Size Okay");

                return p.GetBytes;
            }
        }

    }

    // Itens Equipado do Player
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public class UserEquipedItem
    {
        public CharacterInfo char_info;
        public CaddieInfoEx cad_info;
        public MascotInfo mascot_info;
        public ClubSetInfo csi;
        public UserEquipedItem()
        { cad_info = new CaddieInfoEx(); csi = new ClubSetInfo(); }
    }

    // Estado do Character no Lounge
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public class StateCharacterLounge
    {
        void clear()
        {
            camera_zoom = 1;
            scale_head = 1;
            walk_speed = 1;
            fUnknown = 1;
        }
        public float camera_zoom;  // Zoom da câmera
        public float scale_head;   // Tamanho da cabeça do character
        public float walk_speed;   // Velocidade que o player anda no lounge
        public float fUnknown;
    }

    // MyRoom Config
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public class MyRoomConfig
    {
        public short allow_enter;     // Se pode ou não entrar no My Room
        public byte public_lock;      // Se tem senha ou não
        [field: MarshalAs(UnmanagedType.ByValTStr, SizeConst = 15)]
        public string pass;//15]                  // Senha
        [field: MarshalAs(UnmanagedType.ByValArray, SizeConst = 90)]
        public byte[] ucUnknown90;//[90]  // Não o que é ainda
        public MyRoomConfig()
        {
            ucUnknown90 = new byte[90];
        }
    }

    // MyRoom Item
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public class MyRoomItem
    {
        public uint id;
        public uint _typeid;
        public short number;
        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        public class Location
        {
            public float x;
            public float y;
            public float z;
            public float r;

        }
        public MyRoomItem()
        {
            location = new Location();
        }
        [field: MarshalAs(UnmanagedType.Struct)]
        public Location location;
        public byte equiped;     // Equipado ou não, 1 YES, 0 NO
    }

    // Dolfine Locker
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public class DolfiniLocker
    {
        public DolfiniLocker()
        {
            v_item = new List<DolfiniLockerItem>();
            pass = "";
        }
        void clear()
        {
            pang = 0;

            v_item.Clear();
        }
        public uint isLocker()
        {

            if (pass[0] == '\0')
                return 2;   // Senha não foi criada ainda
            else if (!locker && pass_check)
                return 76;// 1;	// Senha já foi verificada para essa session

            return 76;  // Senha ainda não foi verificada para essa session
        }
        public bool ownerItem(uint _typeid)
        {

            var it = v_item.Where(c => c.item._typeid == _typeid);


            return it.Any();
        }
        [field: MarshalAs(UnmanagedType.ByValTStr, SizeConst = 7)]
        public string pass = string.Empty;//[7]
        public ulong pang;
        public bool locker;               // Essa opção tem que ser do gs para pedir para o player verificar a senha todas vez do locker
        public bool pass_check;  // 1 já foi verificado a senha nessa session, 0 ainda não foi verificada
        public List<DolfiniLockerItem> v_item;
    }
    // Dolfini Locker Item
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public class DolfiniLockerItem
    {
        public ulong index; // ID do item no dolfini Locker
        public TradeItem item;

        public DolfiniLockerItem()
        {
            item = new TradeItem();
        }
    }

    // TradeItem
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public class TradeItem
    {
        public uint _typeid;
        public uint id;
        public uint qntd;
        [field: MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public byte[] ucUnknown3;//[3]
        public ulong pang;
        public uint upgrade_custo;
        [field: MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
        public short[] c;
        public short usUnknown;
        [field: MarshalAs(UnmanagedType.ByValTStr, SizeConst = 9)]
        public string sd_idx;//[9]
        public short sd_seq;
        public byte sd_status;
        public TradeItem()
        {
            c = new short[5];
            card = new Card();
        }
        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        public class Card
        {
            [field: MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
            public uint[] character;
            [field: MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
            public uint[] caddie;
            [field: MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
            public uint[] NPC;
            public short character_slot_count;
            public short caddie_slot_count;
            public short NPC_slot_count;
            public Card()
            {
                character = new uint[5];
                caddie = new uint[5];
                NPC = new uint[5];
            }
        }

        public Card card;
        [field: MarshalAs(UnmanagedType.ByValTStr, SizeConst = 41)]
        public string sd_name;//[41]
        [field: MarshalAs(UnmanagedType.ByValTStr, SizeConst = 22)]
        public string sd_copier_nick;//[22]

        public byte[] Build()
        {
            using (var p = new PangyaBinaryWriter())
            {
                p.WriteUInt32(_typeid);
                p.WriteUInt32(id);
                p.WriteUInt32(qntd);
                p.WriteBytes(ucUnknown3, 3);//[3]
                p.WriteUInt64(pang);
                p.WriteUInt32(upgrade_custo);
                p.WriteInt16(c);//array
                p.WriteInt16(usUnknown);
                p.WriteStr(sd_idx, 9);//[9]
                p.WriteInt16(sd_seq);
                p.WriteByte(sd_status);
                p.WriteUInt32(card.character);
                p.WriteUInt32(card.caddie);
                p.WriteUInt32(card.NPC);
                p.WriteInt16(card.character_slot_count);
                p.WriteInt16(card.caddie_slot_count);
                p.WriteInt16(card.NPC_slot_count);
                p.WriteStr(sd_name, 41);//[41]
                p.WriteStr(sd_copier_nick, 22);//[22]
                return p.GetBytes;
            }
        }
    }


    // Item Generico
    public class stItem
    {
        public void clear()
        {
            id = 0;
            _typeid = 0;
            type_iff = 0;
            type = 0;
            flag = 0;
            flag_time = 0;
            qntd = 0;
            name = string.Empty;
            icon = string.Empty;
            stat.clear();
            ucc.clear();
            is_cash = 0;
            price = 0;
            desconto = 0;
            date.clear();
            date_reserve = 0;
            Array.Clear(c, 0, c.Length);
        }
        public int id = new int();
        public uint _typeid = new uint();

        public byte type_iff; // Tipo que está no iff structure, tipo no Part.iff, 1 parte de baixo da roupa, 3 luva, 8 e 9 UCC etc
        public byte type; // 2 Normal Item
        public byte flag; // 1 Padrão item fornecido pelo server, 5 UCC_BLANK
        public byte flag_time; // 6 rental(dia), 2 hora(acho), 4 minuto(acho)
        public uint qntd = new uint();

        public string name = new string(new char[64]);
        public string icon = new string(new char[41]);
        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        public class item_stat
        {
            public void clear()
            {
                qntd_ant = 0;
                qntd_dep = 0;
            }
            public uint qntd_ant = new uint();
            public uint qntd_dep = new uint();
        }

        public item_stat stat = new item_stat();

        public class UCC
        {
            public void clear()
            {
                IDX = string.Empty;
                status = 0;
                seq = 0;
            }
            public string IDX = new string(new char[9]); // UCC INDEX STRING
            public uint status = new uint();
            public uint seq = new uint();
        }

        public UCC ucc = new UCC();

        // C++ TO C# CONVERTER TASK: C# does not allow bit fields:
        public byte is_cash = 0;
        public uint price = new uint();
        public uint desconto = new uint();

        public class stDate
        {
            public stDate()
            {
                clear();
            }
            public void clear()
            {
                active = 0;
                date.clear();
            }
            public uint active = 1; // 1 Actived, 0 Desatived
            public class stDateSys
            {
                public void clear()
                {
                    sysDate = new PangyaTime[2];
                }
                public PangyaTime[] sysDate = new PangyaTime[2]; // 0 Begin, 1 End
            }
            public stDateSys date = new stDateSys();
        }

        public stDate date = new stDate();
        public ushort date_reserve;

        public ushort[] c = new ushort[5];
        public ushort STDA_C_ITEM_QNTD { get => c[0]; set => c[0] = value; }
        public ushort STDA_C_ITEM_TICKET_REPORT_ID_HIGH { get => c[1]; set => c[1] = value; }
        public ushort STDA_C_ITEM_TICKET_REPORT_ID_LOW { get => c[2]; set => c[2] = value; }
        public ushort STDA_C_ITEM_TIME { get => c[3]; set => c[3] = value; }
    }

    // stItem Extended
    public class stItemEx : stItem
    {
        public stItemEx(uint _ul = 0u)
        {
            clear();
        }
        public new void clear()
        {
        }
        public class ClubSetWorkshop
        {
            public void clear()
            {
            }
            public ushort[] c = new ushort[5];
            public uint mastery = new uint();
            public char level;
            public uint rank = new uint();
            public uint recovery = new uint();
        }
        public ClubSetWorkshop clubset_workshop = new ClubSetWorkshop();
    }

    /**** Base Item do pacote 0x216 Update Item No Game
	**/
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public class stItem216
    {
        public byte type;
        public uint _typeid;
        public uint id;
        public uint flag_time;
        public uint qntd_ant;
        public uint qntd_dep;
        public uint qntd;
        [field: MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
        public short[] c; //5
        [field: MarshalAs(UnmanagedType.ByValTStr, SizeConst = 9)]
        public string ucc_idx; //9
        public byte seq;  // ou stats
        public uint card_typeid;
        public byte card_slot;
        public stItem216()
        {
            c = new short[5];
        }
    }

    // Friend Info
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public class FriendInfo
    {
        public uint uid;
        public byte sex;  // gender, genero, sexo, 0 masculino, 1 Feminino
        [field: MarshalAs(UnmanagedType.ByValTStr, SizeConst = 22)]
        public string id;//[22]
        [field: MarshalAs(UnmanagedType.ByValTStr, SizeConst = 22)]
        public string nickname;//[22]
        [field: MarshalAs(UnmanagedType.ByValTStr, SizeConst = 15)]
        public string apelido;//[15]
    }

    // Daily Quest Info
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public class DailyQuestInfo
    {
        public DailyQuestInfo()
        {
            clear();
        }
        public DailyQuestInfo(int _typeid_0, uint _typeid_1, uint _typeid_2, PangyaTime _st)
        {
            date = _st;
            _typeid = new uint[3] { (uint)_typeid_1, (uint)_typeid_2, (uint)_typeid_2 };

        }

        public void clear()
        {
            date = new PangyaTime();
            _typeid = new uint[3];
        }
        [field: MarshalAs(UnmanagedType.Struct)]
        public PangyaTime date;            // System Time Windows
        [field: MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public uint[] _typeid;// [3];    // array[3] Typeid da Quest
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public class DailyQuestInfoUser
    {
        public uint now_date;      // Data que a quest está (current quest), do sistema de daily quest
        public uint accept_date;   // Data da última quest que foi aceita
        public uint current_date;  // Data que a quest está (current quest), do player
        public uint count;         // Número de quests do dia
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public uint[] _typeid;     // Máximo de 3 quests por dia

        public DailyQuestInfoUser()
        {
            _typeid = new uint[3];
        }

        public DailyQuestInfoUser(DailyQuestInfoUser user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            now_date = user.now_date;
            accept_date = user.accept_date;
            current_date = user.current_date;
            count = user.count;
            _typeid = (uint[])user._typeid.Clone(); // Clonar o array para evitar referências compartilhadas
        }

        public DailyQuestInfoUser(uint initialValue)
        {
            now_date = initialValue;
            accept_date = initialValue;
            current_date = initialValue;
            count = initialValue;
            _typeid = new uint[3];
        }
    }

    // Remove Daily Quest
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public class RemoveDailyQuestUser
    {
        public uint id;
        public uint _typeid;
    }

    // Add DailyQuest
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public class AddDailyQuestUser
    {
        [field: MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string name;// [64];
        public uint _typeid;
        public uint quest_typeid;
        public uint status;
    }

    // Player Canal Info
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public class PlayerCanalInfo
    {
        public PlayerCanalInfo()
        {
            clear();
        }
        public void clear()
        {
            sala_numero = _Define.DEFAULT_ROOM_ID;
            capability = new uCapability();
            state_flag = new uStateFlag();
            guild_mark_img = "";// [12];
            nickNT_bytes = new byte[18];// [18];                // Acho
            ucUnknown106 = new byte[106];// [109];
            nickname_bytes = new byte[22];
        }

        public byte[] Build()
        {
            using (var p = new PangyaBinaryWriter())
            {
                p.WriteUInt32(uid);
                p.WriteUInt32(oid);
                p.WriteUInt16(sala_numero);
                p.WriteStr(nickname, 22);
                p.WriteByte(level);
                p.WriteInt32(capability.ulCapability);
                p.WriteUInt32(title);
                p.WriteUInt32(team_point);
                p.WriteByte(state_flag.ucByte);
                p.WriteUInt32(guid_uid);
                p.WriteUInt32(guild_index_mark);
                p.WriteStr(guild_mark_img, 12);
                p.WriteInt16(flag_visible_gm);
                p.WriteUInt32(l_unknown);
                p.WriteStr(nickNT, 22);             // S4 TH
                p.WriteBytes(ucUnknown106, 106);				// S4 TH     
                //if (p.GetSize == 200)
                //    Debug.WriteLine("PlayerCanalInfo Size OKay");

                return p.GetBytes;
            }
        }


        public uint uid;
        public uint oid;
        public ushort sala_numero;
        [field: MarshalAs(UnmanagedType.ByValArray, SizeConst = 22)]
        private byte[] nickname_bytes;// [22];      
        public string nickname { get => nickname_bytes.GetString(); set => nickname_bytes.SetString(value); }
        public byte level;
        public uCapability capability;
        public uint title;
        public uint team_point;             // Acho que é o team point
        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        public class uStateFlag
        {
            public byte ucByte;
        }
        [field: MarshalAs(UnmanagedType.Struct)]
        public uStateFlag state_flag;
        public uint guid_uid;
        public uint guild_index_mark;
        [field: MarshalAs(UnmanagedType.ByValTStr, SizeConst = 12)]
        public string guild_mark_img;// [12];
        public short flag_visible_gm;//th é vip                         
        public uint l_unknown;// [6];
        [field: MarshalAs(UnmanagedType.ByValArray, SizeConst = 18)]
        private byte[] nickNT_bytes;// [22];      
        public string nickNT { get => nickNT_bytes.GetString(); set => nickNT_bytes.SetString(value); }            // Acho
        [field: MarshalAs(UnmanagedType.ByValArray, SizeConst = 106)]
        public byte[] ucUnknown106;// [109];
    }

    // Player Canal Info
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public class PlayerCanalInfoEx : PlayerCanalInfo
    {
        public PlayerCanalInfoEx()
        {
            base.clear();
            state_flag = new uStateFlagEx();
        }
        [field: MarshalAs(UnmanagedType.Struct)]
        public new uStateFlagEx state_flag;
        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        public class uStateFlagEx : uStateFlag
        {
            public uStateFlagEx()
            {
                stBit = new _stBit();
            }
            public _stBit stBit;
            public class _stBit
            {
                public byte away;             // AFK
                public byte sexo;            // Genero
                public byte quiter_1;        // Quit rate maior que 31% e menor que 41%
                public byte quiter_2;         // Quit rate maior que 41%
                public byte azinha;           // Azinha, Quit rate menor que 3%
                public byte icon_angel;       // Angel Wings
                public byte ucUnknown_bit7;   // Unknown Bit 7
                public byte ucUnknown_bit8;   // Unknown Bit 8
            }
        }
    }
    [StructLayout(LayoutKind.Sequential, Pack = 4, Size = 341)]
    public class PlayerRoomInfo
    {
        public PlayerRoomInfo()
        {
            clear();
        }
        protected void clear()
        {
            usUnknown_flg = 3490;
            place = new PlayerPlace(0x0A);
            ucUnknown3 = 66;
            capability = new uCapability();
            state_flag = new StateFlag();
            skin = new uint[6];
            location = new stLocation();
            shop = new PersonShop();
            flag_item_boost = new uItemBoost();
            nickNT = "@NT";
            ucUnknown105 = new byte[105];
        }
        public uint oid;
        [field: MarshalAs(UnmanagedType.ByValTStr, SizeConst = 22)]
        public string nickname;// [22];
        [field: MarshalAs(UnmanagedType.ByValTStr, SizeConst = 17)]
        public string guild_name;// [17];
        public byte position;
        [field: MarshalAs(UnmanagedType.Struct, SizeConst = 4)]
        public uCapability capability;
        public uint title;
        public uint char_typeid;       // Character Typeid
        [field: MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
        public uint[] skin;// [6];
        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        public class StateFlag
        {
            public void clear()
            {
                usFlag = 0;
                ucByte = new byte[2];
            }
            public ushort usFlag;
            public byte[] ucByte;
            public byte team = 1;
            public byte team2 = 1;
            public byte away = 1;
            public byte master = 1;
            public byte master2 = 1;
            public byte sexo = 1;
            public byte quiter_1 = 1;
            public byte quiter_2 = 1;
            public byte azinha = 1;
            public byte ready = 1;
            public byte unknown_bit11 = 1;
            public byte unknown_bit12 = 1;
            public byte unknown_bit13 = 1;
            public byte unknown_bit14 = 1;
            public byte unknown_bit15 = 1;
            public byte unknown_bit16 = 1;
        }
        [field: MarshalAs(UnmanagedType.Struct, SizeConst = 2)]
        public StateFlag state_flag;//2 bytes
        public byte level;
        public byte icon_angel;
        public PlayerPlace place;         // Tem o valor 0x0A aqui quase sempre das vezes que vi esse pacote, Pode ser o Place(lugar que o player está) tipo Room = 10(hex:0x0A)
        public uint guild_uid;
        [field: MarshalAs(UnmanagedType.ByValTStr, SizeConst = 12)]
        public string guild_mark_img; //[12];
        public uint uid;
        //---------Action
        public uint state_lounge;//animate
        public short usUnknown_flg;//Unknown1	// Acho que seja uma flag tbm
        public uint state;//Posture	// Acho que seja estado de "lugar" pelo que lembro
        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        public class stLocation
        {
            // Corrigindo o operador de soma
            public static stLocation operator +(stLocation a, stLocation _add_location)
            {
                return new stLocation()
                {
                    x = a.x += _add_location.x,
                    z = a.z += _add_location.z,
                    r = a.r += _add_location.r
                };
            }
            public static stLocation operator -(stLocation a, stLocation _add_location)
            {
                return new stLocation()
                {
                    x = a.x -= _add_location.x,
                    z = a.z -= _add_location.z,
                    r = a.r -= _add_location.r
                };
            }

            public float x;
            public float z;
            public float r;
        }
        [field: MarshalAs(UnmanagedType.Struct, SizeConst = 12)]
        public stLocation location;
        //----------
        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        public class PersonShop
        {
            public uint active;
            [field: MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
            public string name;//64
        }
        [field: MarshalAs(UnmanagedType.Struct, SizeConst = 68)]
        public PersonShop shop;
        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        public class uItemBoost
        {
            public ushort ulItemBoost;
            public class _stItemBoost
            {
                public byte ucPangMastery = 1;
                public byte ucPangNitro = 1;

               public _stItemBoost()
                {
                    ucPangMastery = 0;
                    ucPangNitro = 0;

                }
            }
            public uItemBoost()
            {
                stItemBoost = new _stItemBoost();
            }

            public _stItemBoost stItemBoost;
        }
        public uint mascot_typeid;
        [field: MarshalAs(UnmanagedType.Struct, SizeConst = 2)]
        public uItemBoost flag_item_boost;// Boost EXP, Pang e etc(2 bytes)
        public uint ulUnknown_flg;// Pode ser a flag de teasure do player, ou de drop item
        [field: MarshalAs(UnmanagedType.ByValTStr, SizeConst = 22)]
        public string nickNT;//[22] Acho que seja o ID na ntreev do player, a empresa que mantêm as contas, no JP era o gamepot
        [field: MarshalAs(UnmanagedType.ByValArray, SizeConst = 105)]
        public byte[] ucUnknown105;// Unknown 106 bytes que sempre fica com valor 0
        public byte convidado;   // Flag Convidado, [Não sei bem por que os que entra na sala normal tem valor igual aqui, já que é flag de convidado waiting]
        public float avg_score;// Media score "media de holes feito pelo player"
        public float ucUnknown3;// Não sei mas sempre é 0 depois do media score(66 no th)
        public byte[] Build()
        {
            using (var p = new PangyaBinaryWriter())
            {
                p.WriteUInt32(oid);
                p.WriteStr(nickname, 22);// [22]);
                p.WriteStr(guild_name, 17);// [17]);
                p.WriteByte(position);
                p.WriteInt32(capability.ulCapability);
                p.WriteUInt32(title);
                p.WriteUInt32(char_typeid);       // Character Typeid
                p.WriteUInt32(skin);// [6]); 
                p.WriteUInt16(state_flag.usFlag);
                p.WriteByte(level);
                p.WriteByte(icon_angel);
                p.WriteByte(place.ulPlace);         // Tem o valor 0x0A aqui quase sempre das vezes que vi esse pacote, Pode ser o Place(lugar que o player está) tipo Room = 10(hex:0x0A)
                p.WriteUInt32(guild_uid);
                p.WriteStr(guild_mark_img, 12); //[12]);
                p.WriteUInt32(uid);
                //---------Action
                p.WriteUInt32(state_lounge);//animate
                p.WriteInt16(usUnknown_flg);//Unknown1	// Acho que seja uma flag tbm
                p.WriteUInt32(state);//Posture	// Acho que seja estado de "lugar" pelo que lembro

                p.WriteSingle(location.x);
                p.WriteSingle(location.z);
                p.WriteSingle(location.r);
                p.WriteUInt32(shop.active);
                p.WriteStr(shop.name, 64);//64
                p.WriteUInt32(mascot_typeid);
                p.WriteUInt16(flag_item_boost.ulItemBoost);
                p.WriteUInt32(ulUnknown_flg);// Pode ser a flag de teasure do player, ou de drop item
                p.WriteStr(nickNT, 22);//[22] Acho que seja o ID na ntreev do player, a empresa que mantêm as contas, no JP era o gamepot
                p.WriteBytes(ucUnknown105, 105);// Unknown 106 bytes que sempre fica com valor 0
                p.WriteByte(convidado);   // Flag Convidado, [Não sei bem por que os que entra na sala normal tem valor igual aqui, já que é flag de convidado waiting]
                p.WriteSingle(avg_score);// Media score "media de holes feito pelo player"
                p.WriteSingle(ucUnknown3);// Não sei mas sempre é 0 depois do media score(66 no th)}

                return p.GetBytes;
            }
        }
    }

    // Player Room Info Ex
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public class PlayerRoomInfoEx : PlayerRoomInfo
    {
        public PlayerRoomInfoEx()
        {
            clear();
            ci = new CharacterInfo();
        }
        [field: MarshalAs(UnmanagedType.Struct, SizeConst = 513)]
        public CharacterInfo ci { get; set; }

        public byte[] BuildEx()
        {
            using (var p = new PangyaBinaryWriter())
            {
                p.WriteBytes(Build());
                p.WriteBytes(ci.Build());
                return p.GetBytes;
            }
        }
    }
    //    // Sala Guild Info(tenho que olhar mais direito se esta correto)
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public class RoomGuildInfo
    {
        public RoomGuildInfo()
        {
            clear();
        }

        public void clear(int type  = 0)
        {
            if (type ==0)
            {
                guild_1_uid = 0;
                guild_1_index_mark = 0;
                guild_1_mark = "";
                guild_1_nome = "";

                guild_2_uid = 0;
                guild_2_index_mark = 0;
                guild_2_mark = "";
                guild_2_nome = "";
            }
            if (type == 1)
            {
                guild_1_uid = 0;
                guild_1_index_mark = 0;
                guild_1_mark = "";
                guild_1_nome = "";
            }
            if (type == 2)
            {
                guild_2_uid = 0;
                guild_2_index_mark = 0;
                guild_2_mark = "";
                guild_2_nome = "";
            }
        }
         public uint guild_1_uid;
        public uint guild_2_uid;
        [field: MarshalAs(UnmanagedType.ByValTStr, SizeConst = 12)]
        public string guild_1_mark;             // mark string o pangya JP não usa aqui fica 0
        [field: MarshalAs(UnmanagedType.ByValTStr, SizeConst = 12)]
        public string guild_2_mark;             // mark string o pangya JP não usa aqui fica 0
        public ushort guild_1_index_mark;
        public ushort guild_2_index_mark;
        [field: MarshalAs(UnmanagedType.ByValTStr, SizeConst = 20)]
        public string guild_1_nome;
        [field: MarshalAs(UnmanagedType.ByValTStr, SizeConst = 20)]
        public string guild_2_nome;
    }

    //    // Sala Grand Prix Info
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public class RoomGrandPrixInfo
    {
        public uint dados_typeid;
        public uint rank_typeid;
        public uint tempo;
        public uint active;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4, Size = 4)]
    public class NaturalAndShortGame
    {                          
        public NaturalAndShortGame(uint _ul = 0)
        {
            ulNaturalAndShortGame = _ul;
        }
        
        public uint ulNaturalAndShortGame { get; set; }  
        public uint natural;           // Natural Modo
        public uint short_game;    // Short Game Modo
    }


    // Treasure Hunter Info
   public class TreasureHunterInfo
    {
        public void clear()
        {
        }
        public byte course;
        public uint point;
    }

   public class TreasureHunterItem
    {
       public void clear() {  }
        public uint _typeid;
        public uint qntd;
        public uint probabilidade;
       public byte flag;
        public byte active = 1;
    }

    // SalaInfo
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public class RoomInfo
    {
        public enum eCOURSE : byte
        {
            BLUE_LAGOON,
            BLUE_WATER,
            SEPIA_WIND,
            WIND_HILL,
            WIZ_WIZ,
            WEST_WIZ,
            BLUE_MOON,
            SILVIA_CANNON,
            ICE_CANNON,
            WHITE_WIZ,
            SHINNING_SAND,
            PINK_WIND,
            DEEP_INFERNO = 13,
            ICE_SPA,
            LOST_SEAWAY,
            EASTERN_VALLEY,
            CHRONICLE_1_CHAOS,
            ICE_INFERNO,
            WIZ_CITY,
            ABBOT_MINE,
            MYSTIC_RUINS,
            GRAND_ZODIAC = 64,
            RANDOM = 127,
            UNK = 0x7F
        }
        public enum TIPO : uint
        {
            STROKE,
            MATCH,
            LOUNGE,
            TOURNEY = 4,
            TOURNEY_TEAM,
            GUILD_BATTLE,
            PANG_BATTLE,
            APPROCH = 10,
            GRAND_ZODIAC_INT,
            GRAND_ZODIAC_ADV = 13,
            GRAND_ZODIAC_PRACTICE,
            SPECIAL_SHUFFLE_COURSE = 18,
            PRACTICE,
            GRAND_PRIX,
        }
        public enum MODO : uint
        {
            M_FRONT,
            M_BACK,
            M_RANDOM,
            M_SHUFFLE,
            M_REPEAT,
            M_SHUFFLE_COURSE,
        }
        public enum INFO_CHANGE : uint
        {
            NAME,
            SENHA,
            TIPO,
            COURSE,
            QNTD_HOLE,
            MODO,
            TEMPO_VS,
            MAX_PLAYER,
            TEMPO_30S,
            STATE_FLAG,
            UNKNOWN,
            HOLE_REPEAT,
            FIXED_HOLE,
            ARTEFATO,
            NATURAL,
        }
        public RoomInfo()
        {
            clear();
        }
        protected void clear()
        {

            numero = ushort.MaxValue;
            senha_flag = 1;
            state = 1;
            _30s = 30;

            guilds = new RoomGuildInfo(); // Valores inicias
            key = new byte[17];
        }
        [field: MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string nome;// [64];
        public byte senha_flag;  // Sala sem senha = 1, Sala com senha = 0
        public byte state;       // Sala em espera = 1, Sala em Game = 0
        public byte flag;                 // Sala que pode entrar depois que começou = 1
        public byte max_Player;
        public byte num_Player;
        [field: MarshalAs(UnmanagedType.ByValArray, SizeConst = 17)]
        public byte[] key;
        public byte _30s;                 // Modo Multiplayer do pangya acho, sempre 0x1E (dec: 30) no pangya
        public byte qntd_hole;
        public byte tipo_show;            // esse é o tipo que mostra no pacote, esse pode mudar dependendo do tipo real da sala, fica ou camp, ou VS ou especial, não coloca todos os tipos aqui
        public ushort numero;
        public byte modo;
        public eCOURSE course;
        public uint time_vs;
        public uint time_30s;
        public uint trofel;
        public short state_flag;          // Quando é sala de 100 player o mais de gm event aqui é 0x100
        public RoomGuildInfo guilds;
        public uint rate_pang;
        public uint rate_exp;
        public int master;         // Tem valores negativos, por que a sala usa ele para grand prix e etc
        public byte tipo_ex;          // tipo extended, que fala o tipo da sala certinho
        public uint artefato;          // Aqui usa pra GP efeitos especiais do GP
                                       //int natural;			// Aqui usa para Short Game Também
        public NaturalAndShortGame natural;       // Aqui usa para Short Game Também
        public RoomGrandPrixInfo grand_prix;
    }
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public class RoomInfoEx : RoomInfo
    {
       public RoomInfoEx()
        {

            base.clear();

            hole_repeat = 0;
            fixed_hole = 0;
            tipo = 0;
            state_afk = 0;
            channel_rookie = false;
            angel_event = false;
        }
        public new NaturalAndShortGame natural;
        [field: MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string senha;// [64];                     // Senha da sala
        public byte tipo;                 // Tipo real da sala
        public byte hole_repeat;          // Número do hole que vai ser repetido
        public uint fixed_hole;            // Aqui é 1 Para Hole(Pin"Beam") Fixo, e 0 para aleatório
        public byte state_afk;   // Estado afk da sala, usar para depois começar a sala, já que o pangya não mostra se a sala está afk
        public bool channel_rookie;   // Flag que guarda, se o channel é rookie ou não, onde a sala foi criada, vem da Flag do channel
        public bool angel_event;      // Flag que guarda se o Angel Event está ligado
        public byte flag_gm;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public class RateValue
    {
        public uint pang;
        public uint exp;
        public uint clubset;
        public uint rain;
        public uint treasure;
        public byte persist_rain;
    }


    // Item Pangya Base Para Pacote216
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public class ItemPangyaBase
    {

        public byte tipo;
        public uint _typeid;
        public uint id;
        public uint tipo_unidade_add;
        public uint qntd_ant;
        public uint qntd_dep;
        public uint qntd;
        [field: MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public byte[] unknown;// [8];
        public short qntd_time;
    }
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public class ItemPangya : ItemPangyaBase
    {
        [field: MarshalAs(UnmanagedType.ByValTStr, SizeConst = 9)]
        public string sd_idx;// [9];
        public uint sd_status;
        public uint sd_seq;
        [field: MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
        public byte[] unknown2;//[5];
    }

    // BuyItem
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public class BuyItem
    {
        public int id;
        public uint _typeid;
        public short time;
        public short usUnknown;
        public uint qntd;
        public uint pang;
        public uint cookie;
    }

    // Email Info
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public class EmailInfo
    {
        public EmailInfo()
        {
            clear();
        }

        public void clear()
        {
            id = uint.MaxValue;
            lida_yn = 0;
            from_id = "";
            msg = "";
            gift_date = "";
            itens = new List<item>();
        }
        public uint id;
        [field: MarshalAs(UnmanagedType.ByValTStr, SizeConst = 22)]
        public string from_id;//[22];
        [field: MarshalAs(UnmanagedType.ByValTStr, SizeConst = 20)]
        public string gift_date;//[20];
        [field: MarshalAs(UnmanagedType.ByValTStr, SizeConst = 100)]
        public string msg;//[100];
        public byte lida_yn;
        public DateTime RegDate => string.IsNullOrEmpty(gift_date) ? DateTime.Now : DateTime.Parse(gift_date);
        public class item
        {
            public uint id;
            public uint _typeid;
            public byte flag_time;
            public uint qntd;
            public uint tempo_qntd;
            public ulong pang;
            public ulong cookie;
            public uint gm_id;
            public uint flag_gift;
            [field: MarshalAs(UnmanagedType.ByValTStr, SizeConst = 9)]
            public string ucc_img_mark;//[9];
            [field: MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            public byte[] ucUnknown3;//[3];
            public short type;

            public item()
            {
                clear();
            }

            public void clear()
            {
                ucc_img_mark = "";
                ucUnknown3 = new byte[3];
            }

            public byte[] Build()
            {
                using (var p = new PangyaBinaryWriter())
                {
                    p.WriteUInt32(id);
                    p.WriteUInt32(_typeid);
                    p.WriteByte(flag_time);
                    p.WriteUInt32(qntd);
                    p.WriteUInt32(tempo_qntd);
                    p.WriteUInt64(pang);
                    p.WriteUInt64(cookie);
                    p.WriteUInt32(gm_id);
                    p.WriteUInt32(flag_gift);
                    p.WriteStr(ucc_img_mark, 9);//[9];
                    p.WriteBytes(ucUnknown3, 3);//[3];
                    p.WriteInt16(type);
                    return p.GetBytes;
                }
            }
            public item(uint _id, uint typeid, byte _flag_time, uint _qntd, ushort _tempo_qntd, uint _pang, uint _cookie, uint _gm_id, uint _flag_gift, string _ucc_img_mark, short _type)
            {
                id = _id;
                _typeid = typeid;
                flag_time = _flag_time;
                qntd = _qntd;
                pang = _pang;
                cookie = _cookie;
                gm_id = _gm_id;
                tempo_qntd = _tempo_qntd;
                ucc_img_mark = _ucc_img_mark;
                flag_gift = _flag_gift;
                type = _type;
            }
        }
        public List<item> itens;

        public byte[] Build()
        {
            using (var p = new PangyaBinaryWriter())
            {
                p.WriteUInt32(id);
                p.WritePStr(from_id);
                p.WritePStr(RegDate.ToString("dd/MM/yyyy"));
                p.WritePStr(msg);
                p.WriteByte(lida_yn); // Flag que mostra o item, 1 mostra, 0 não mostra

                p.WriteInt32(itens.Count);

                for (var i = 0; i < itens.Count; ++i)
                {
                    p.WriteBytes(itens[i].Build());
                }
                return p.GetBytes;
            }
        }

    }

    // EmailInfoEx
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public class EmailInfoEx : EmailInfo
    {
        public EmailInfoEx()
        {
            clear();
            visit_count = 0;
        }

        public EmailInfoEx(uint v)
        {
        }

        public uint visit_count;
    }

    //// Mail Box
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public class MailBox
    {
        public uint id;
        [field: MarshalAs(UnmanagedType.ByValArray, SizeConst = 30)]
        public byte[] from_id_bytes;// [30];
        public string from_id { get => from_id_bytes.GetString(); set => from_id_bytes.SetString(value); }
        [field: MarshalAs(UnmanagedType.ByValArray, SizeConst = 80)]
        public byte[] msg_bytes;// [80];
        public string msg { get => msg_bytes.GetString(); set => msg_bytes.SetString(value); }
        [field: MarshalAs(UnmanagedType.ByValArray, SizeConst = 18)]
        public byte[] unknown2;// [18];
        public uint visit_count;
        public byte lida_yn;
        public uint item_num;           // Número de itens que tem nesse anexado a esse email
        public EmailInfo.item item;
        public MailBox()
        {
            from_id_bytes = new byte[30];
            msg_bytes = new byte[80];
        }

        public byte[] Build()
        {
            using (var p = new PangyaBinaryWriter())
            {
                p.WriteUInt32(id);
                p.WriteStr(from_id, 30);// [30]);
                p.WriteStr(msg, 80);// [80]);
                p.WriteBytes(unknown2, 18);// [18]);
                p.WriteUInt32(visit_count);
                p.WriteByte(lida_yn);
                p.WriteUInt32(item_num);
                // Sometimes mail don't contain any items, need to check if mail contains an item or not.
                if (item?.Build() is byte[] itemData)
                    p.WriteBytes(itemData);
                return p.GetBytes;
            }
        }
    }

    //// Ticket Report Scroll Info
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public class TicketReportScrollInfo
    {
        public void clear()
        {
            id = uint.MaxValue;
            date = new PangyaTime();

            v_players = new List<stPlayerDados>();
        }
        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        public class stPlayerDados
        {
            public stPlayerDados()
            {
                clear();
            }
            public void clear()
            {
                ucUnknown_flg = 2;
            }
            public uint uid;
            public ulong pang;
            public ulong bonus_pang;
            public uint trofel_typeid;
            public uint exp;
            public uint mascot_typeid;
            public byte premium_user;
            public byte item_boost; // [Bit] 1 = Pang Mastery x2, 2 = Pang Nitro x4, 3 = (ACHO) Exp x2
            public uint level;
            public byte score;
            public uMedalWin medalha;
            public byte trofel;
            [field: MarshalAs(UnmanagedType.ByValTStr, SizeConst = 22)]
            public string id;//[22];
            [field: MarshalAs(UnmanagedType.ByValTStr, SizeConst = 22)]
            public string nickname;//[22];
            public uint ulUnknown;
            public uint guild_uid;
            public uint mark_index;        // Guild, isso é do JP, que ele nao usa o EMBLEM NUMER
            [field: MarshalAs(UnmanagedType.ByValTStr, SizeConst = 12)]
            public string guild_mark_img;//[12];
            public uint tipo;
            public byte state;
            public byte ucUnknown_flg;    // Não sei mas sempre peguei o valor 2
            public PangyaTime finish_time;
        }
        public uint id;
        public PangyaTime date;
        public List<stPlayerDados> v_players;
    }

    // Estrutura que Guarda as informações dos Convites do Canal
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct InviteChannelInfo
    {
        public ushort room_number;
        public uint invite_uid;
        public uint invited_uid;
        public PangyaTime time;
    }

    // Command Info, os Comando do Auth Server
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public class CommandInfo
    {
        public CommandInfo()
        {
            arg = new int[5];
        }
        public uint idx;
        public uint id;
        public int[] arg;// [5];
        public uint target;
        public short flag;
        public byte valid;
        public PangyaTime reserveDate;
    }

    //// Update Item
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public class UpdateItem
    {
        public enum UI_TYPE : byte
        {
            CADDIE,
            CADDIE_PARTS,
            MASCOT,
            WAREHOUSE,
        }
        public UI_TYPE type;
        public uint _typeid;
        public uint id;
        public UpdateItem(UI_TYPE _type, uint typeid, uint _id)
        {
            this.type = _type;
            this._typeid = typeid;
            this.id = _id;
        }
    }

    //// Grand Prix Clear
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public class GrandPrixClear
    {
        public uint _typeid;
        public uint position;
    }

    //// Guild Update Activity Info
    //// Guarda os dados das atualizações que os Clubs tem de alterações
    //// Como membro kickado, sair do club e aceito no club
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct GuildUpdateActivityInfo
    {
        public enum TYPE_UPDATE : byte
        {
            TU_ACCEPTED_MEMBER,
            TU_EXITED_MEMBER,
            TU_KICKED_MEMBER,
        }

        public ulong index; // ID do update activity
        public uint club_uid;
        public uint owner_uid; // Quem fez a Ação
        public uint player_uid;
        public TYPE_UPDATE type;
        public PangyaTime reg_date;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public class ChangePlayerItemRoom
    {
        public enum TYPE_CHANGE : byte
        {
            TC_CADDIE = 1,
            TC_BALL,
            TC_CLUBSET,
            TC_CHARACTER,
            TC_MASCOT,
            TC_ITEM_EFFECT_LOUNGE,  // Hermes x2, Twilight, Jester x2
            TC_ALL,                 // CHARACTER, CADDIE, CLUBSET e BALL essa é a ordem
            TC_UNKNOWN = 255,
        }
        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        public class stItemEffectLounge
        {
            public enum TYPE_EFFECT : uint
            {
                TE_BIG_HEAD = 1,
                TE_FAST_WALK,
                TE_TWILIGHT,
            }
            public uint item_id;   // Aqui ele manda 0 o cliente, não sei por que, deveria mandar o id do item equipado
            public TYPE_EFFECT effect;
        }
        public TYPE_CHANGE type;                   // Type Change
        public uint caddie;                // Caddie ID
        public uint ball;                  // Ball Typeid
        public uint clubset;               // ClubSet ID
        public uint character;         // Character ID
        public uint mascot;                // Mascot ID
        public stItemEffectLounge effect_lounge;   // Item effect lounge
    }

    // Trofel Info      
    public class TrofelInfo
    {
        public TrofelInfo()
        {
            clear();
        }
        public void clear()
        {
            Array.Clear(ama_6_a_1, 0, ama_6_a_1.Length); // Limpa todos os valores de ama_6_a_1
            Array.Clear(pro_1_a_7, 0, pro_1_a_7.Length); // Limpa todos os valores de pro_1_a_7
        }
        public void update(uint _type, byte _rank)
        {

            // Maior que Pro 7
            if (_type > 12)
                throw new exception("[TrofelInfo::update][Error] _type[VALUE=" + (_type) + "] is invalid", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PANGYA_GAME_ST, 200, 0));

            if (_rank == 0u || _rank > 3)
                throw new exception("[TrofelInfo::update][Error] _rank[VALUE=" + (_rank) + "] is invalid", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PANGYA_GAME_ST, 201, 0)); ;

            if (_type < 6)
            { // AMA

                ama_6_a_1[_type, _rank - 1]++;

            }
            else
            { // >= 6 PRO

                pro_1_a_7[_type - 6, _rank - 1]++;
            }
        }

        public uint getSumGold()
        {
            uint gold_sum = 0u;

            // Itera sobre as linhas da matriz ama_6_a_1
            for (int i = 0; i < ama_6_a_1.GetLength(0); i++)
            {
                gold_sum += (uint)ama_6_a_1[i, 0]; // Coluna 0 para o ouro
            }

            // Itera sobre as linhas da matriz pro_1_a_7
            for (int i = 0; i < pro_1_a_7.GetLength(0); i++)
            {
                gold_sum += (uint)pro_1_a_7[i, 0]; // Coluna 0 para o ouro
            }

            return gold_sum;
        }

        public uint getSumSilver()
        {
            uint silver_sum = 0u;

            // Itera sobre as linhas da matriz ama_6_a_1
            for (int i = 0; i < ama_6_a_1.GetLength(0); i++)
            {
                silver_sum += (uint)ama_6_a_1[i, 1]; // Coluna 1 para a prata
            }

            // Itera sobre as linhas da matriz pro_1_a_7
            for (int i = 0; i < pro_1_a_7.GetLength(0); i++)
            {
                silver_sum += (uint)pro_1_a_7[i, 1]; // Coluna 1 para a prata
            }

            return silver_sum;
        }

        public uint getSumBronze()
        {
            uint bronze_sum = 0u;

            // Itera sobre as linhas da matriz ama_6_a_1
            for (int i = 0; i < ama_6_a_1.GetLength(0); i++)
            {
                bronze_sum += (uint)ama_6_a_1[i, 2]; // Coluna 2 para o bronze
            }

            // Itera sobre as linhas da matriz pro_1_a_7
            for (int i = 0; i < pro_1_a_7.GetLength(0); i++)
            {
                bronze_sum += (uint)pro_1_a_7[i, 2]; // Coluna 2 para o bronze
            }

            return bronze_sum;
        }

        public short[,] ama_6_a_1 = new short[6, 3];    // Ama 6~1, Ouro, Prata e Bronze
        public short[,] pro_1_a_7 = new short[7, 3];    // Pro 1~7, Ouro, Prate e Bronze
                                                        // 
        public byte[] Build()
        {
            using (var p = new PangyaBinaryWriter())
            {
                // Gravar ama_6_a_1 (6 linhas x 3 colunas)
                for (int i = 0; i < 6; i++)  // 6 linhas
                {
                    for (int j = 0; j < 3; j++)  // 3 colunas (Ouro, Prata, Bronze)
                    {
                        p.WriteInt16(ama_6_a_1[i, j]);  // Escreve cada valor como short
                    }
                }

                // Gravar pro_1_a_7 (7 linhas x 3 colunas)
                for (int i = 0; i < 7; i++)  // 7 linhas
                {
                    for (int j = 0; j < 3; j++)  // 3 colunas (Ouro, Prata, Bronze)
                    {
                        p.WriteInt16(pro_1_a_7[i, j]);  // Escreve cada valor como short
                    }
                }

                return p.GetBytes;
            }
        }
    }

    // Trofel Especial Info
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public class TrofelEspecialInfo
    {
        public uint id;
        public uint _typeid;
        public uint qntd;

        public byte[] Build()
        {
            using (var p = new PangyaBinaryWriter())
            {
                p.WriteUInt32(id);
                p.WriteUInt32(_typeid);
                p.WriteUInt32(qntd);
                return p.GetBytes;
            }
        }
    }

    // Item Equipados
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public class UserEquip
    {
        public UserEquip()
        {
            clear();
        }
        public void clear()
        {
            item_slot = new uint[10];
            skin_id = new uint[6];
            skin_typeid = new uint[6];
            poster = new uint[2];
        }
        public uint caddie_id;
        public uint character_id;
        public uint clubset_id;
        public uint ball_typeid;
        [field: MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
        public uint[] item_slot;//[10];      // 10 Item slot
        [field: MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
        public uint[] skin_id;//[6];     // 6 skin id, tem o title, frame, stick e etc
        [field: MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
        public uint[] skin_typeid; // 6 skin typeid, tem o title, frame, stick e etc
        public uint mascot_id;
        [field: MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        public uint[] poster;     // Poster, tem 2 o poster A e poster B
        public uint getTitle()
        {
            return skin_typeid[5];// Titulo Typeid
        }
        /// <summary>
        /// Size = 116 Bytes
        /// </summary>
        /// <returns></returns>
        public byte[] Build()
        {
            using (var p = new PangyaBinaryWriter())
            {
                p.WriteUInt32(caddie_id);
                p.WriteUInt32(character_id);
                p.WriteUInt32(clubset_id);
                p.WriteUInt32(ball_typeid);

                for (int i = 0; i < 10; i++)
                    p.WriteUInt32(item_slot[i]);//[10];      // 10 Item slot

                for (int i = 0; i < 6; i++)
                    p.WriteUInt32(skin_id[i]);//[6];     // 6 skin id, tem o title, frame, stick e etc

                for (int i = 0; i < 6; i++)
                    p.WriteUInt32(skin_typeid[i]); // 6 skin typeid, tem o title, frame, stick e etc

                p.WriteUInt32(mascot_id);
                for (int i = 0; i < 2; i++)
                    p.WriteUInt32(poster[i]);     // Poster, tem 2 o poster A e poster B
                //if (p.GetSize == 116)
                //    Debug.WriteLine("EquipData Size Okay");

                return p.GetBytes;
            }
        }
    }
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public class MapStatistics
    {
        public MapStatistics()
        {
            clear();
        }
        public void clear(sbyte _course = 0)
        {
            best_score = 127;
            course = _course;
        }
        public bool isRecorded()
        {

            // Player fez record nesse Course
            return (best_score != 127 ? true : false);
        }
        public sbyte course;
        public uint tacada;
        public uint putt;
        public uint hole;
        public uint fairway;
        public uint hole_in;
        public uint putt_in;
        public int total_score;
        public sbyte best_score;
        public ulong best_pang;
        public uint character_typeid;
        public byte event_score;

        public byte[] Build()
        {
            using (var p = new PangyaBinaryWriter())
            {

                p.WriteSByte(course);
                p.WriteUInt32(tacada);
                p.WriteUInt32(putt);
                p.WriteUInt32(hole);
                p.WriteUInt32(fairway);
                p.WriteUInt32(hole_in);
                p.WriteUInt32(putt_in);
                p.WriteInt32(total_score);
                p.WriteSByte(best_score);
                p.WriteUInt64(best_pang);
                p.WriteUInt32(character_typeid);
                p.WriteByte(event_score);
                return p.GetBytes;
            }
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    // MapStatisticsEx esse tem o tipo que não vai no pacote que passa pro cliente
    public class MapStatisticsEx : MapStatistics
    {
        public MapStatisticsEx(uint _ul = 0)
        {
            clear();
        }
        public MapStatisticsEx(MapStatistics _cpy)
        {
            tipo = 0;
        }
        public byte tipo;             // Tipo, 0 Normal, 0x32 Natural, 0x33 Grand Prix
    }
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    // Caddie Info
    public class CaddieInfo
    {
        public uint id;
        public uint _typeid;
        public uint parts_typeid;
        public byte level;
        public uint exp;
        public byte rent_flag;
        public short end_date_unix;
        public short parts_end_date_unix;
        public byte purchase;
        public short check_end;

    }

    // Caddie Info Ex
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public class CaddieInfoEx : CaddieInfo
    {
        public PangyaTime end_date;
        public PangyaTime end_parts_date;
        public bool need_update;         // Precisa Atulizar para o cliente

        public CaddieInfoEx()
        {
            end_date = new PangyaTime();
            end_parts_date = new PangyaTime();
        }
        public void updatePartsEndDate()
        {

            ulong diff_end_parts_date = (ulong)(end_parts_date.IsEmpty ? 0 : UtilTime.GetLocalDateDiffDESC(end_parts_date.ConvertTime()));

            // Não tem mais o parts _typeid acabou o tempo dela
            if (diff_end_parts_date <= 0)
            {

                parts_end_date_unix = 0;

                // Zera Parts_Typeid
                if (parts_typeid > 0)
                {

                    parts_typeid = 0;

                    need_update = true;   // Precisa Atulizar para o cliente
                }

            }
            else
                parts_end_date_unix = (short)((diff_end_parts_date /= _Define.STDA_10_MICRO_PER_HOUR) == 0 ? 1 : (short)diff_end_parts_date);

        }
        public void updateEndDate()
        {

            ulong diff_end_date = (ulong)(end_date.IsEmpty ? 0 : UtilTime.GetLocalDateDiffDESC(end_date.ConvertTime()));

            if (diff_end_date <= 0)
                end_date_unix = 0;
            else
                end_date_unix = ((short)((diff_end_date /= _Define.STDA_10_MICRO_PER_DAY) == 0 ? 1/*Less 1 Day, bot not left time*/ : (short)diff_end_date));

        }

        //precisa ser o CaddieInfoEx mesmo, depois modifico
        public void Check()
        {
            // Update Timestamp Unix of caddie and caddie Parts

            // End Date Unix Update 
            updateEndDate();

            // Parts End Date Unix Update

            updatePartsEndDate();
        }
        /// <summary>
        /// Size = 25 (0x19)
        /// </summary>
        /// <param name="is_login"></param>
        /// <returns></returns>
        public byte[] Build(bool is_login = true)
        {
            try
            {
                Check();

                using (var p = new PangyaBinaryWriter())
                {
                    p.WriteUInt32(id);
                    p.WriteUInt32(_typeid);
                    p.WriteUInt32(parts_typeid);
                    p.WriteByte(level);
                    p.WriteUInt32(exp);
                    p.WriteByte(rent_flag);
                    p.WriteInt16(end_date_unix);
                    p.WriteInt16(parts_end_date_unix);
                    p.WriteByte(purchase);
                    p.WriteInt16(check_end);

                    //if (p.GetSize == 25)
                    //    Debug.WriteLine("GetCaddieInfo Size Okay");

                    return p.GetBytes;
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }

    // Club Set Info
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public class ClubSetInfo
    {
        public uint id;
        public uint _typeid;
        [field: MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
        public short[] slot_c;// [5];        // Total de slot para upa do stats, força, controle, spin e etc
        [field: MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
        public short[] enchant_c;// [5];     // Enchant Club, Força, controle, spin e etc

        public void setValues(uint _uid, uint id_type, short[] value)
        {
            slot_c = value;
            _typeid = id_type;
            id = _uid;
        }
        public ClubSetInfo()
        {
            slot_c = new short[5];
            enchant_c = new short[5];
        }
        /// <summary>
        /// Size = 28 bytes (0x)
        /// </summary>
        /// <returns></returns>
        public byte[] Build()
        {
            using (var p = new PangyaBinaryWriter())
            {
                p.WriteUInt32(id);
                p.WriteUInt32(_typeid);

                for (int i = 0; i < 5; i++)
                    p.WriteInt16(slot_c[i]);// [5];        // Total de slot para upa do stats, força, controle, spin e etc

                for (int i = 0; i < 5; i++)
                    p.WriteInt16(enchant_c[i]);// [5];     // Enchant Club, Força, controle, spin e etc
                //if (p.GetSize == 28)
                //    Debug.WriteLine("GetClubData Size Okay");

                return p.GetBytes;
            }
        }
    }

    // Mascot Info
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public class MascotInfo
    {
        public uint id;
        public uint _typeid;
        public byte level;
        public uint exp;
        [field: MarshalAs(UnmanagedType.ByValArray, SizeConst = 30)]
        private byte[] message_bytes;
        public string message { get => message_bytes.GetString(); set => message_bytes.SetString(value); }
        public short tipo;
        [field: MarshalAs(UnmanagedType.Struct)]
        public PangyaTime data;
        public byte flag;
        public MascotInfo()
        {
            data = new PangyaTime();
            message_bytes = new byte[30];
        }


        /// <summary>
        /// Size = 62 bytes(0x3E)
        /// </summary>
        /// <returns></returns>
        public byte[] Build()
        {
            using (var p = new PangyaBinaryWriter())
            {
                p.WriteUInt32(id);
                p.WriteUInt32(_typeid);
                p.WriteByte(level);
                p.WriteUInt32(exp);
                p.WriteStr(message, 30);
                p.WriteInt16(tipo);
                p.WriteStruct(data, new PangyaTime());
                p.WriteByte(flag);
                return p.GetBytes;
            }
        }
    }

    // Mascot Info Ex, tem o IsCash flag nele
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public class MascotInfoEx : MascotInfo
    {
        public bool checkUpdate()
        {

            if (data.IsEmpty)
                need_update = 1;

            return (need_update == 1);
        }

        public byte is_cash;
        public uint price;
        public byte need_update;
    }

    // Item Warehouse
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public class WarehouseItem
    {
        public WarehouseItem()
        {
            c = new short[5];
            card = new Card()
            { caddie = new uint[4], character = new uint[4], NPC = new uint[4] };
            clubset_workshop = new ClubsetWorkshop() { c = new short[5] };
            ucc = new UCC();
        }
        public uint id { get; set; }
        public uint _typeid { get; set; }
        public int ano { get; set; }            // acho que seja só tempo que o item ainda tem
        [field: MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
        public short[] c { get; set; }      // Stats do item ctrl, força etc, se não usa isso o [0] é a quantidade
        public byte purchase { get; set; }
        public sbyte flag { get; set; }
        public long apply_date { get; set; }
        public long end_date { get; set; }
        public sbyte type { get; set; }
        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        public class UCC
        {
            [field: MarshalAs(UnmanagedType.ByValArray, SizeConst = 40)]
            private byte[] name_bytes;
            public string name { get => name_bytes.GetString(); set => name_bytes.SetString(value); }
            public sbyte trade { get; set; }     // Aqui pode(acho) ser qntd de sd que foi vendida
            [field: MarshalAs(UnmanagedType.ByValArray, SizeConst = 9)]
            private byte[] idx_bytes;
            public string idx { get => idx_bytes.GetString(); set => idx_bytes.SetString(value); }             // 8 
            public sbyte status { get; set; }
            public short seq { get; set; }          // aqui é a seq de sd que vendeu
            [field: MarshalAs(UnmanagedType.ByValArray, SizeConst = 22)]
            private byte[] copier_nick_bytes;
            public string copier_nick { get => copier_nick_bytes.GetString(); set => copier_nick_bytes.SetString(value); }
            public uint copier { get; set; }                // m_uid de quem fez a sd

            public UCC()
            {
                name_bytes = new byte[40];
                idx_bytes = new byte[9];
                copier_nick_bytes = new byte[22];
            }
        }
        [field: MarshalAs(UnmanagedType.Struct)]
        public UCC ucc;
        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        public class Card
        {
            [field: MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public uint[] character { get; set; }
            [field: MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public uint[] caddie { get; set; }
            [field: MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public uint[] NPC { get; set; }
        }
        [field: MarshalAs(UnmanagedType.Struct)]
        public Card card;
        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        public class ClubsetWorkshop
        {
            public short flag { get; set; }
            [field: MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
            public short[] c { get; set; }
            // Stats do item ctrl, força etc, se não usa isso o [0] é a quantidade			public uint  mastery {get; set;}
            public uint mastery { get; set; }
            public uint recovery_pts { get; set; }
            public int level { get; set; }
            public uint rank { get; set; }          // UP eu chamo esse
            public uint calcRank(short[] _c)
            {
                uint total = (uint)(c[0] + _c[0] + c[1] + _c[1] + c[2] + _c[2] + c[3] + _c[3] + c[4] + _c[4]);

                if (total >= 30 && total < 60)
                    return (total - 30) / 5;

                return uint.MaxValue;
            }
            public uint calcLevel(short[] _c)
            {

                uint total = (uint)(c[0] + _c[0] + c[1] + _c[1] + c[2] + _c[2] + c[3] + _c[3] + c[4] + _c[4]);

                if (total >= 30 && total < 60)
                    return (total - 30) % 5;

                return uint.MaxValue;
            }
            public uint s_calcRank(short[] _c)
            {

                uint total = (uint)(_c[0] + _c[1] + _c[2] + _c[3] + _c[4]);

                if (total >= 30 && total < 60)
                    return (total - 30) / 5;

                return uint.MaxValue;
            }
            public uint s_calcLevel(short[] _c)
            {

                uint total = (uint)(_c[0] + _c[1] + _c[2] + _c[3] + _c[4]);

                if (total >= 30 && total < 60)
                    return (total - 30) % 5;

                return uint.MaxValue;
            }
        }
        [field: MarshalAs(UnmanagedType.Struct)]
        public ClubsetWorkshop clubset_workshop;
        /// <summary>
        /// Size 196 Bytes
        /// </summary>
        /// <returns></returns>
        public byte[] Build()
        {
            using (var p = new PangyaBinaryWriter())
            {
                // Serializa os campos primitivos
                p.WriteUInt32(id);
                p.WriteUInt32(_typeid);
                p.WriteInt32(ano);
                p.WriteInt16(c);

                p.WriteByte(purchase);
                p.WriteSByte(flag);//sbyte
                p.WriteInt64(apply_date);
                p.WriteInt64(end_date);
                p.WriteSByte(type);//sbyte

                // Serializa o objeto UCC
                p.WriteStr(ucc.name, 40);
                p.WriteSByte(ucc.trade);//sbyte
                p.WriteStr(ucc.idx, 9);
                p.WriteSByte(ucc.status);//sbyte
                p.WriteInt16(ucc.seq);
                p.WriteStr(ucc.copier_nick, 22);
                p.WriteUInt32(ucc.copier);
                p.WriteUInt32(card.character);
                p.WriteUInt32(card.caddie);
                p.WriteUInt32(card.NPC);
                // Serializa o objeto ClubsetWorkshop
                p.WriteInt16(clubset_workshop.flag);
                p.WriteInt16(clubset_workshop.c);
                p.WriteUInt32(clubset_workshop.mastery);
                p.WriteUInt32(clubset_workshop.recovery_pts);
                p.WriteInt32(clubset_workshop.level);
                p.WriteUInt32(clubset_workshop.rank);
 
                return p.GetBytes;
            }
        }
    }
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public class WarehouseItemEx : WarehouseItem
    {
        // Date to Calcule dates
        public uint apply_date_unix_local;
        public uint end_date_unix_local;
        public short STDA_C_ITEM_QNTD { get => c[0]; set => c[0] = value; }
        public short STDA_C_ITEM_TICKET_REPORT_ID_HIGH { get => c[1]; set => c[1] = value; }
        public short STDA_C_ITEM_TICKET_REPORT_ID_LOW { get => c[2]; set => c[2] = value; }
        public short STDA_C_ITEM_TIME { get => c[3]; set => c[3] = value; }
    }

    // ClubSet Workshop Last Up Level
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public class ClubSetWorkshopLasUpLevel
    {
        public uint clubset_id;
        public uint stat;
    }

    // ClubSet WorkShop Transform ClubSet In Special ClubSet
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public class ClubSetWorkshopTransformClubSet
    {
        public uint clubset_id;
        public uint stat;
        public uint transform_typeid;
    }
    // Personal Shop Item
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public class PersonalShopItem
    {
        public uint index;     // Index Sequência do item no shop
        public TradeItem item;
        public PersonalShopItem()
        { clear(); }
        public void clear()
        {
            item = new TradeItem();
           
        }
        public byte[] Build()
        {
            using (var p = new PangyaBinaryWriter())
            {
                p.WriteUInt32(index);
                p.WriteBytes(item.Build());
                return p.GetBytes;
            }
        }
    }

    // Tutorial Info
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public class TutorialInfo
    {
        public uint getTutoAll()
        {
            return rookie | beginner | advancer;
        }
        public uint rookie;
        public uint beginner;
        public uint advancer;
    }

    // Card Info
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public class CardInfo
    {
        public uint id;
        public uint _typeid;
        public uint slot;
        public uint efeito;
        public uint efeito_qntd;
        public uint qntd;
        public PangyaTime use_date;
        public PangyaTime end_date;
        public byte type;
        public byte use_yn;

        public byte[] Build()
        {
            using (var p = new PangyaBinaryWriter())
            {
                p.WriteUInt32(id);
                p.WriteUInt32(_typeid);
                p.WriteUInt32(slot);
                p.WriteUInt32(efeito);
                p.WriteUInt32(efeito_qntd);
                p.WriteUInt32(qntd);
                if (use_date == null)
                    use_date = new PangyaTime();
                if (end_date == null)
                    end_date = new PangyaTime();
                p.WriteStruct(use_date, new PangyaTime());
                p.WriteStruct(end_date, new PangyaTime());
                p.WriteByte(type);
                p.WriteByte(use_yn);
                return p.GetBytes;
            }
        }
    }

    // Card Equip Info
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public class CardEquipInfo
    {
        public uint id;
        public uint _typeid;
        public uint parts_typeid;
        public uint parts_id;
        public uint efeito;
        public uint efeito_qntd;
        public uint slot;
        public PangyaTime use_date;
        public PangyaTime end_date;
        public uint tipo;
        public byte use_yn;

        public byte[] Build()
        {
            using (var p = new PangyaBinaryWriter())
            {
                p.WriteUInt32(id);
                p.WriteUInt32(_typeid);
                p.WriteUInt32(slot);
                p.WriteUInt32(efeito);
                p.WriteUInt32(efeito_qntd);
                p.WriteUInt32(slot);

                if (use_date == null)
                    use_date = new PangyaTime();
                if (end_date == null)
                    end_date = new PangyaTime();

                p.WriteStruct(use_date, new PangyaTime());
                p.WriteStruct(end_date, new PangyaTime());
                p.WriteUInt32(tipo);
                p.WriteByte(use_yn);
                return p.GetBytes;
            }
        }
    }

    // Card Equip Info Ex
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public class CardEquipInfoEx : CardEquipInfo
    {
        public ulong index;
    }



    // Message Off
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public class MsgOffInfo
    {

        public uint from_uid;
        public short id;
        [field: MarshalAs(UnmanagedType.ByValArray, SizeConst = 22)]
        public string nick;//[22];
        [field: MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
        public string msg;//[64];
        [field: MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public string date;// [17];
        public byte Un;
    }

    // Attendence Reward Info
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class AttendanceRewardInfo
    {
        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        public class item
        {
            public uint _typeid;
            public uint qntd;

            public void clear()
            {
                _typeid = 0;
                qntd = 0;
            }
        }
        public byte login;
        [field: MarshalAs(UnmanagedType.Struct)]
        public item now;
        [field: MarshalAs(UnmanagedType.Struct)]
        public item after;
        public uint counter;
        public AttendanceRewardInfo()
        {
            clear();
        }
        public void clear()
        {
            now = new item();
            after = new item();
        }

        public byte[] Build()
        {
            using (var p = new PangyaBinaryWriter())
            {
                p.WriteByte(login);
                p.WriteUInt32(now._typeid);
                p.WriteUInt32(now.qntd);
                p.WriteUInt32(after._typeid);
                p.WriteUInt32(after.qntd);
                p.WriteUInt32(counter);
                return p.GetBytes;
            }
        }
    }

    // Attendance Reward Info Ex
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public class AttendanceRewardInfoEx : AttendanceRewardInfo
    {
        public AttendanceRewardInfoEx()
        {
            last_login = new PangyaTime();
            base.clear();
        }
        [field: MarshalAs(UnmanagedType.Struct, SizeConst = 8)]
        public PangyaTime last_login;   // Data do ultimo login
    }

    // Attendance Reward Item Context
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public class AttendanceRewardItemCtx
    {
        public uint _typeid;
        public uint qntd;
        public byte tipo;
    }

    // Last Five Players Played with player
    public class Last5PlayersGame
    {
        public Last5PlayersGame()
        {
            // Inicializa o array com 5 elementos de LastPlayerGame
            this.players = new List<LastPlayerGame>();

            // Inicializa cada elemento do array
            for (int i = 0; i < 5; i++)
            {
                players.Add(new LastPlayerGame());
            }
        }
        public class LastPlayerGame
        {

            public uint sex;   // gender, genero, sexo, 0 masculino, 1 Feminino
            public string nick;
            public string id;
            public uint uid;
            public static bool operator !=(LastPlayerGame a, LastPlayerGame b)
            {
                return a.uid == b.uid && a.id == b.id;
            }
            public static bool operator ==(LastPlayerGame a, LastPlayerGame b)
            {
                return a.uid == b.uid && a.id == b.id;
            }

            public bool Equals(LastPlayerGame obj)
            {
                return (uid == obj.uid
                            && id == obj.id);
            }

            public byte[] Build()
            {
                using (var p = new PangyaBinaryWriter())
                {
                    p.WriteUInt32(sex);
                    p.WriteStr(nick, 22);
                    p.WriteStr(id, 22);
                    p.WriteUInt32(uid);
                    return p.GetBytes;
                }
            }
        }

        public void add(player_info _pi, uint _sex)
        {
            var _array = players.ToArray();
            if (players[0].uid != _pi.uid)
            {
                var it = players.Where(c => c.uid == _pi.uid).First();
                // Put Player Last Position
                if (it != players[5])
                {
                    // Rotate To Right
                    var idx = Array.IndexOf(_array, it);
                    Array.Reverse(_array, idx + 1, 5);
                }
                _array.SetValue(4, 5);
                // Update
                _array[0].uid = _pi.uid;
            }
            // já está em primeiro não precisa mexer mais, só atualizar o friend e o nick, que ele pode ter mudado       
            _array[0].sex = _sex;
            _array[0].nick = _pi.nickname;
            players = _array.ToList();
        }
        public List<LastPlayerGame> players;  // Last Five Players Played with player
    }


    // Time 32, HighTime, LowTime
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public class time32
    {
        void setTime(int time)
        {
            high_time = (short)(time / 0xFFFF);
            low_time = (short)(time % 0xFFFF);
        }
        public uint getTime()
        {
            return (uint)((high_time * 0xFFFF) | low_time);
        }
        private short high_time;
        private short low_time;
    }

    // Item Buff (Exemple: Yam, Bola Arco-iris)
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public class ItemBuff
    {
        public enum eTYPE : uint
        {
            NONE,
            YAM_AND_GOLD,
            RAINBOW,
            RED,
            GREEN,
            YELLOW,
        }
        public uint id;
        public uint _typeid;
        public uint parts_typeid;
        public uint parts_id;
        public uint efeito;
        public uint efeito_qntd;
        public uint slot;
        public PangyaTime use_date;
        [field: MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
        public byte[] ucUnknown12;
        public time32 tempo;
        public uint tipo;
        public byte use_yn;

        public byte[] Build()
        {
            using (var p = new PangyaBinaryWriter())
            {
                p.WriteUInt32(id);
                p.WriteUInt32(_typeid);
                p.WriteUInt32(parts_typeid);
                p.WriteUInt32(parts_id);
                p.WriteUInt32(efeito);
                p.WriteUInt32(efeito_qntd);
                p.WriteUInt32(slot);
                p.WriteStruct(use_date, new PangyaTime());
                p.WriteBytes(ucUnknown12, 12);
                p.WriteStruct(tempo, new time32());
                p.WriteUInt32(tipo);
                p.WriteByte(use_yn);

                return p.GetBytes;
            }
        }
    }

    // Item Buff Ex
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public class ItemBuffEx : ItemBuff
    {
        public long index;
        public PangyaTime end_date;
        public uint percent;       // Rate, tipo 2 é 0 por que é 100
    }

    // Guild Info
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public class GuildInfo
    {

        public uint uid;                                          
        public byte leadder;
        [field: MarshalAs(UnmanagedType.ByValArray, SizeConst = 31)]
        public byte[] name_Bytes;
        public string name
        {
            get => name_Bytes.GetString();
            set => name_Bytes.SetString(value);
        }
        public uint index_mark_emblem;
        public ulong ull_unknown;
        public ulong pang;
        [field: MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public byte[] _16unknown;
        public uint point;
        public GuildInfo()
        {
            clear();
        }

        public void clear()
        {
            name_Bytes = new byte[31];
            _16unknown = new byte[16];
        }

        public byte[] Build()
        {
            using (var p = new PangyaBinaryWriter())
            {
                p.WriteStruct(this, new GuildInfo()); 
                return p.GetBytes;
            }
        }
    }

    // GuildInfoEx
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public class GuildInfoEx : GuildInfo
    {
        [field: MarshalAs(UnmanagedType.ByValTStr, SizeConst = 12)]
        public string mark_emblem;
        public GuildInfoEx()
        {
            clear();
         }

    }

    // Canal Info
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class ChannelInfo
    {
        public ChannelInfo()
        {
            clear();
        }

        public void clear()
        {
            flag = new UFlag();
        }
        public class UFlag
        {
            public uint ulFlag { get; set; }

            public bool all
            {
                get => (ulFlag & 1) != 0;
                set => ulFlag = Convert.ToUInt32(value ? (ulFlag | 1) : (ulFlag & ~1));
            }

            public bool junior_bellow
            {
                get => (ulFlag & 512) != 0;
                set => ulFlag = (uint)(value ? (ulFlag | 512) : (ulFlag & ~512));
            }

            public bool junior_above
            {
                get => (ulFlag & 1024) != 0;
                set => ulFlag = (uint)(value ? (ulFlag | 1024) : (ulFlag & ~1024));
            }

            public bool only_rookie
            {
                get => (ulFlag & 2048) != 0;
                set => ulFlag = (uint)(value ? (ulFlag | 2048) : (ulFlag & ~2048));
            }

            public bool beginner_between_junior
            {
                get => (ulFlag & 4096) != 0;
                set => ulFlag = (uint)(value ? (ulFlag | 4096) : (ulFlag & ~4096));
            }

            public bool junior_between_senior
            {
                get => (ulFlag & 8192) != 0;
                set => ulFlag = (uint)(value ? (ulFlag | 8192) : (ulFlag & ~8192));
            }
        }

        public string name { get; set; }
        public short max_user { get; set; }
        public short curr_user { get; set; }
        public byte id { get; set; }
        [field: MarshalAs(UnmanagedType.Struct)]
        public UFlag flag { get; set; }
        public uint flag2 { get; set; }
        public uint min_level_allow { get; set; }
        public uint max_level_allow { get; set; }

        public byte[] Build()
        {
            using (var Response = new PangyaBinaryWriter())
            {
                Response.WriteStr(name, 64);
                Response.WriteInt16(max_user);
                Response.WriteInt16(curr_user);
                Response.WriteByte(id); //Lobby ID
                Response.WriteUInt32(flag.ulFlag); //ルーム制限あるね- channel flag
                Response.WriteUInt32(flag2); //メンテナンス表記+ナチュラルマーク- flag2
                Response.WriteUInt32(min_level_allow); //メンテナンス表記+なんか    
                Response.WriteUInt32(max_level_allow); //メンテナンス表記+Granplix
                return Response.GetBytes;
            }
        }
    }
}
