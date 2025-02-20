using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using PangyaAPI.Utilities;
using PangyaAPI.Utilities.BinaryModels;
using System.Diagnostics;
using PangyaAPI.Network.PangyaPacket;
using Part = PangLib.IFF.JP.Models.Data.Part;
using GameServer.GameType;
using System.Linq;
using PangyaAPI.Utilities.Log;
using PangLib.IFF.JP.Models.Data;
using PangLib.IFF.JP.Models.Flags;

namespace PangyaAPI.Network.Pangya_St
{
    public class Global
    {                     
        public static readonly uint[] angel_wings = { 134309888u, 134580224u, 134842368u, 135120896u, 135366656u, 135661568u, 135858176u, 136194048u, 136398848u, 136660992u, 137185294u, 137447424u, 138004480u };
        public static readonly uint[] gacha_angel_wings = { 134309903u, 134580239u, 134842383u, 135120911u, 135366671u, 135661583u, 135858191u, 136194063u, 136398863u, 136661007u, 136923153u, 137185284u, 137447436u, 138004492u };
    }
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

    [StructLayout(LayoutKind.Sequential, Pack = 4, Size = 4)]
    public class uProperty
    {
        public uProperty(uint _ul = 0u)
        {
            ulProperty = _ul;
        }

        [field: MarshalAs(UnmanagedType.U4, SizeConst = 4)]
        public uint ulProperty { get; set; }
        public bool normal
        {
            get => (ulProperty & (1 << 0)) != 0;
            set => ulProperty = value ? (ulProperty | (1 << 0)) : (ulProperty & ~(1u << 0));
        }
        public bool unknown_1
        {
            get => (ulProperty & (1 << 1)) != 0;
            set => ulProperty = value ? (ulProperty | (1 << 2)) : (ulProperty & ~(1u << 2));
        }
        public bool unknown_2
        {
            get => (ulProperty & (1 << 3)) != 0;
            set => ulProperty = value ? (ulProperty | (1 << 3)) : (ulProperty & ~(1u << 3));
        }
        public bool mantle
        {
            get => (ulProperty & (1 << 4)) != 0;
            set => ulProperty = value ? (ulProperty | (1 << 4)) : (ulProperty & ~(1u << 4));
        }
        public bool unknown_3
        {
            get => (ulProperty & (1 << 5)) != 0;
            set => ulProperty = value ? (ulProperty | (1 << 5)) : (ulProperty & ~(1u << 5));
        }

        public bool only_rookie
        {
            get => (ulProperty & (1 << 6)) != 0;
            set => ulProperty = value ? (ulProperty | (1 << 6)) : (ulProperty & ~(1u << 6));
        }
          
        public bool natural
        {
            get => (ulProperty & (1 << 7)) != 0;
            set => ulProperty = value ? (ulProperty | (1 << 7)) : (ulProperty & ~(1u << 7));
        }

        public bool unknown_4
        {
            get => (ulProperty & (1 << 8)) != 0;
            set => ulProperty = value ? (ulProperty | (1 << 8)) : (ulProperty & ~(1u << 8));
        }
        public bool azul
        {
            get => (ulProperty & (1 << 9)) != 0;
            set => ulProperty = value ? (ulProperty | (1 << 9)) : (ulProperty & ~(1u << 9));
        }

        public bool verde
        {
            get => (ulProperty & (1 << 10)) != 0;
            set => ulProperty = value ? (ulProperty | (1 << 10)) : (ulProperty & ~(1u << 10));
        }

        public bool grand_prix
        {
            get => (ulProperty & (1 << 11)) != 0;
            set => ulProperty = value ? (ulProperty | (1 << 1)) : (ulProperty & ~(1u << 11));
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4, Size = 2)]
    public class uEventFlag
    {
        public uEventFlag(ushort ul = 0)
        {
            usEventFlag = ul;
        }

        [field: MarshalAs(UnmanagedType.U2, SizeConst = 2)]
        public ushort usEventFlag { get; set; }
        public bool pang_x_plus
        {
            get => (usEventFlag & (1 << 1)) != 0;
            set => usEventFlag = (ushort)(value ? (usEventFlag | (1 << 1)) : (usEventFlag & ~(1 << 1)));
        }

        public bool exp_x2
        {
            get => (usEventFlag & (1 << 2)) != 0;
            set => usEventFlag = (ushort)(value ? (usEventFlag | (1 << 2)) : (usEventFlag & ~(1 << 2)));
        }

        public bool angel_wing
        {
            get => (usEventFlag & (1 << 3)) != 0;
            set => usEventFlag = (ushort)(value ? (usEventFlag | (1 << 3)) : (usEventFlag & ~(1 << 3)));
        }

        /// <summary>
        /// 3x
        /// </summary>
        public bool exp_x_plus
        {
            get => (usEventFlag & (1 << 4)) != 0;
            set => usEventFlag = (ushort)(value ? (usEventFlag | (1 << 4)) : (usEventFlag & ~(1 << 4)));
        }

        public bool unknown_0
        {
            get => (usEventFlag & (1 << 5)) != 0;
            set => usEventFlag = (ushort)(value ? (usEventFlag | (1 << 5)) : (usEventFlag & ~(1 << 5)));
        }

        public bool unknown_1
        {
            get => (usEventFlag & (1 << 6)) != 0;
            set => usEventFlag = (ushort)(value ? (usEventFlag | (1 << 6)) : (usEventFlag & ~(1 << 6)));
        }

        public bool unknown_2
        {
            get => (usEventFlag & (1 << 8)) != 0;
            set => usEventFlag = (ushort)(value ? (usEventFlag | (1 << 8)) : (usEventFlag & ~(1 << 8)));
        }

        public bool club_mastery_x_plus
        {
            get => (usEventFlag & (1 << 7)) != 0;
            set => usEventFlag = (ushort)(value ? (usEventFlag | (1 << 7)) : (usEventFlag & ~(1 << 7)));
        }


        public bool unknown_3
        {
            get => (usEventFlag & (1 << 9)) != 0;
            set => usEventFlag = (ushort)(value ? (usEventFlag | (1 << 9)) : (usEventFlag & ~(1 << 9)));
        }

        public bool unknown_4
        {
            get => (usEventFlag & (1 << 10)) != 0;
            set => usEventFlag = (ushort)(value ? (usEventFlag | (1 << 10)) : (usEventFlag & ~(1 << 10)));
        }

        public bool unknown_5
        {
            get => (usEventFlag & (1 << 11)) != 0;
            set => usEventFlag = (ushort)(value ? (usEventFlag | (1 << 11)) : (usEventFlag & ~(1 << 11)));
        }

        public bool unknown_6
        {
            get => (usEventFlag & (1 << 12)) != 0;
            set => usEventFlag = (ushort)(value ? (usEventFlag | (1 << 12)) : (usEventFlag & ~(1 << 12)));
        }

        public bool unknown_7
        {
            get => (usEventFlag & (1 << 13)) != 0;
            set => usEventFlag = (ushort)(value ? (usEventFlag | (1 << 13)) : (usEventFlag & ~(1 << 13)));
        }

        public bool unknown_8
        {
            get => (usEventFlag & (1 << 14)) != 0;
            set => usEventFlag = (ushort)(value ? (usEventFlag | (1 << 14)) : (usEventFlag & ~(1 << 14)));
        }

        public bool unknown_9
        {
            get => (usEventFlag & (1 << 15)) != 0;
            set => usEventFlag = (ushort)(value ? (usEventFlag | (1 << 15)) : (usEventFlag & ~(1 << 15)));
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4, Size = 8)]
    public class uFlag
    {
        public uFlag(ulong _ull = 0)
        {
            ullFlag = _ull;
        }

        public ulong ullFlag { get; set; }

        public bool unknown_1
        {
            get => (ullFlag & (1 << 0)) != 0;
            set => ullFlag = value ? (ullFlag | (1ul << 0)) : (ullFlag & ~(1ul << 0));
        }

        public bool all_game
        {
            get => (ullFlag & (1 << 1)) != 0;
            set => ullFlag = value ? (ullFlag | (1ul << 1)) : (ullFlag & ~(1ul << 1));
        }

        public bool buy_and_gift_shop
        {
            get => (ullFlag & (1 << 2)) != 0;
            set => ullFlag = value ? (ullFlag | (1ul << 2)) : (ullFlag & ~(1ul << 2));
        }

        public bool gift_shop
        {
            get => (ullFlag & (1 << 3)) != 0;
            set => ullFlag = value ? (ullFlag | (1ul << 3)) : (ullFlag & ~(1ul << 3));
        }

        public bool papel_shop
        {
            get => (ullFlag & (1 << 4)) != 0;
            set => ullFlag = value ? (ullFlag | (1ul << 4)) : (ullFlag & ~(1ul << 4));
        }

        public bool personal_shop
        {
            get => (ullFlag & (1 << 5)) != 0;
            set => ullFlag = value ? (ullFlag | (1ul << 5)) : (ullFlag & ~(1ul << 5));
        }

        public bool stroke
        {
            get => (ullFlag & (1 << 6)) != 0;
            set => ullFlag = value ? (ullFlag | (1ul << 6)) : (ullFlag & ~(1ul << 6));
        }

        public bool match
        {
            get => (ullFlag & (1 << 7)) != 0;
            set => ullFlag = value ? (ullFlag | (1ul << 7)) : (ullFlag & ~(1ul << 7));
        }

        public bool tourney
        {
            get => (ullFlag & (1 << 8)) != 0;
            set => ullFlag = value ? (ullFlag | (1ul << 8)) : (ullFlag & ~(1ul << 8));
        }

        public bool guild_battle
        {
            get => (ullFlag & (1 << 10)) != 0;
            set => ullFlag = value ? (ullFlag | (1ul << 10)) : (ullFlag & ~(1ul << 10));
        }

        public bool pang_battle
        {
            get => (ullFlag & (1 << 11)) != 0;
            set => ullFlag = value ? (ullFlag | (1ul << 11)) : (ullFlag & ~(1ul << 11));
        }

        public bool grand_prix
        {
            get => (ullFlag & (1 << 21)) != 0;
            set => ullFlag = value ? (ullFlag | (1ul << 21)) : (ullFlag & ~(1ul << 21));
        }

        public bool char_mastery
        {
            get => (ullFlag & (1 << 30)) != 0;
            set => ullFlag = value ? (ullFlag | (1ul << 30)) : (ullFlag & ~(1ul << 30));
        }

        public bool cadie_recycle
        {
            get => (ullFlag & (1ul << 33)) != 0;
            set => ullFlag = value ? (ullFlag | (1ul << 33)) : (ullFlag & ~(1ul << 33));
        }

        public bool copound_card_system
        {
            get => (ullFlag & (1ul << 32)) != 0;
            set => ullFlag = value ? (ullFlag | (1ul << 32)) : (ullFlag & ~(1ul << 32));
        }

        public bool short_game
        {
            get => (ullFlag & (1ul << 29)) != 0;
            set => ullFlag = value ? (ullFlag | (1ul << 29)) : (ullFlag & ~(1ul << 29));
        }

        public bool memorial_shop
        {
            get => (ullFlag & (1ul << 28)) != 0;
            set => ullFlag = value ? (ullFlag | (1ul << 28)) : (ullFlag & ~(1ul << 28));
        }

        public bool guild
        {
            get => (ullFlag & (1ul << 24)) != 0;
            set => ullFlag = value ? (ullFlag | (1ul << 24)) : (ullFlag & ~(1ul << 24));
        }
    }



    [StructLayout(LayoutKind.Sequential, Pack = 4, Size = 92)]
    public class ServerInfo
    {
        [field: MarshalAs(UnmanagedType.ByValArray, SizeConst = 40)]
        private byte[] name_bytes;
        public string nome { get => name_bytes.GetString(); set => name_bytes.SetString(value); }
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
        public short scratch_rate { get; set; } // pode ser scratchy rate ou não
        public short img_no { get; set; }
        public ServerInfo()
        {
            name_bytes = new byte[40];
        }

        public byte[] Build()
        {
            using (var p = new PangyaBinaryWriter())
            {
                p.WriteStr(nome, 40);
                p.WriteInt32(uid);
                p.WriteInt32(max_user);
                p.WriteInt32(curr_user);
                p.WriteStr(ip, 18);
                p.WriteInt32(port);
                p.WriteUInt32(propriedade.ulProperty);
                p.WriteInt32(angelic_wings_num);
                p.WriteUInt16(event_flag.usEventFlag);
                p.WriteInt16(event_map);
                p.WriteInt16(app_rate);
                p.WriteInt16(scratch_rate); // pode ser scratchy rate ou não
                p.WriteInt16(img_no);
                return p.GetBytes;
            }
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public class ServerInfoEx : ServerInfo
    {
        public uint packet_version { get; set; }

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
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
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

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public class chat_macro_user
    {
        [field: MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string[] macro { get; set; }

        public chat_macro_user()
        {
            macro = new string[9];
            for (int i = 0; i < 9; i++)
            {
                macro[i] = "Pangya!";
            }
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
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
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
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public class AuthKeyInfo
    {
        public byte valid;
        [field: MarshalAs(UnmanagedType.ByValTStr, SizeConst = 10)]
        public string key { get; set; }
    }


    // Auth m_key Login Info
    // Keys Of Login
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public class AuthKeyLoginInfo : AuthKeyInfo
    {
    }
    // Auth m_key Game Info
    public class AuthKeyGameInfo : AuthKeyInfo
    {
        public int server_uid;
    }


    [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 513)]
    public class CharacterInfo
    {
        public CharacterInfo()
        {
            clear();
        }

        public CharacterInfo(Packet packet)
        {

        }

        public enum Stats : int
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
        public byte purchase { get; set; }
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
        public uint[] auxparts { get; set; }
        /// <summary>
        ///Cut-in, no primeiro mas acho que pode ser cut-in no resto
        /// </summary>
        [field: MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public uint[] cut_in { get; set; }
        /// <summary>
        ///Aqui é o character stats, como controle, força, spin e etc
        /// </summary>
        [field: MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
        public byte[] pcl { get; set; }
        /// <summary>
        /// Mastery, que aumenta os slot do stats do character
        /// </summary>
        public uint mastery { get; set; }
        [field: MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public uint[] Card_Character { get; set; }				// 4 Slot de card Character
        [field: MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public uint[] Card_Caddie { get; set; }             // 4 Slot de card Caddie
        [field: MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public uint[] Card_NPC { get; set; }

        public void clear()
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
            if (auxparts == null)
                auxparts = new uint[5];
            if (Blank == null)
                Blank = new byte[216];
            if (cut_in == null)
                cut_in = new uint[4];
            if (pcl == null)
                pcl = new byte[5];

            Card_NPC.ClearArray();
            Card_Character.ClearArray();
            Card_Caddie.ClearArray();
            parts_id.ClearArray();
            parts_typeid.ClearArray();
            auxparts.ClearArray();
            Blank.ClearArray();
            cut_in.ClearArray();
            pcl.ClearArray();
        }

        public byte AngelEquiped()
        {
            uint typeId = (uint)(_typeid & 0x000000FF);
            uint partNum;
            
            var angel = Global.gacha_angel_wings.FirstOrDefault(el => sIff.getInstance().getItemCharIdentify(el) == typeId);
            if (angel != 0 && (partNum = sIff.getInstance().getItemCharPartNumber(angel)) >= 0u && parts_typeid[partNum] == angel)
                return 1; // 3% icon rosa e drop chance A+ e treasure point A+

            // Verifica se o item está na lista de Gacha Angel Wings
            var gachaAngel = Global.gacha_angel_wings.FirstOrDefault(el => sIff.getInstance().getItemCharIdentify(el) == typeId);
            if (gachaAngel != 0 && (partNum = sIff.getInstance().getItemCharPartNumber(gachaAngel)) >= 0u && parts_typeid[partNum] == gachaAngel)
                return 2; // Drop chance A+ e treasure point A+

            return 0; // Nenhuma Angel Wings equipada                
        }

        public bool isEquipedPartSlotThirdCaddieCardSlot()
        {
            for (var i = 0u; i < (parts_typeid.Length); ++i)
            {
                Part part;
                if (parts_id[i] != 0 && (part = sIff.getInstance().findPart(parts_typeid[i])) != null)
                {
                    if (((Part)null)._CardSlot.CaddieSlot != 0) // Tem um Part que Libera o terceiro Caddie Card Slot
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public bool isPartEquiped(uint _part_typeid, int _id)
        {

            if (_part_typeid == 0)
            {
                return false;
            }

            if (sIff.getInstance().getItemCharIdentify(_part_typeid) != (_typeid & 0x000000FF))
            {
                return false;
            }

            var part_num = sIff.getInstance().getItemCharPartNumber(_part_typeid);

            if (parts_typeid[part_num] != _part_typeid || parts_id[part_num] != _id)
            {
                return false;
            }

            return true;
        }
        public bool isPartEquiped(uint _part_typeid)
        {

            if (_part_typeid == 0)
            {
                return false;
            }

            if (sIff.getInstance().getItemCharIdentify(_part_typeid) != (_typeid & 0x000000FF))
            {
                return false;
            }

            var part_num = sIff.getInstance().getItemCharPartNumber(_part_typeid);

            if (parts_typeid[part_num] != _part_typeid)
            {
                return false;
            }

            return true;
        }


        public bool isAuxPartEquiped(uint _auxPart_typeid)
        {

            if (_auxPart_typeid == 0)
            {
                return false;
            }

            for (var i = 0u; i < (auxparts.Length); ++i)
            {
                if (auxparts[i] == _auxPart_typeid)
                {
                    return true;
                }
            }

            return false;
        }

        public void unequipPart(Part _part)
        { // Deseequipa o Part do character e coloca os Parts Default do Character no lugar

            if (_part == null)
            {

                // Singleton<list_fifo_console_asyc<message>>.getInstance().push(new message("[CharacterInfo::unequipPart][Error] IFF::Part* _part is invalid(nullptr).", stdA.Globals.CL_FILE_LOG_AND_CONSOLE));

                return;
            }
            for (var i = 0u; i < (parts_typeid.Length); ++i)
            {

                if (_part.position_mask.getSlot((int)i))
                { // Coloca Def Parts

                    uint def_part = (uint)(((i | (uint)(_typeid << 5)) << 13) | 0x8000400);

                    parts_typeid[i] = (sIff.getInstance().findPart(def_part) != null) ? (uint)def_part : 0;
                    parts_id[i] = 0;
                }
            }
        }


        public void unequipPart(uint _typeid)
        {

            // Invalid Typeid
            if (_typeid == 0u)
            {
                return;
            }

            var part = sIff.getInstance().findPart(_typeid);

            if (part != null)
            {
                unequipPart(part);
            }
            else
            {

                //        Singleton<list_fifo_console_asyc<message>>.getInstance().push(new message("[CharacterInfo::unequipPart][Error][WARNIG] Part[TYPEID=" + Convert.ToString(_typeid) + "], mas ele nao existe no IFF_STRUCT do server, desequipa sem usar a funcao do character. Hacker ou Bug.", stdA.Globals.CL_FILE_LOG_AND_CONSOLE));

                // Não vai pegar todos os Slots que o Part ocupava para desequipar, desequipa o só onde tem o typeid   
                for (var i = 0u; i < (parts_typeid.Length); ++i)
                {

                    // Não vai pergar todos os 
                    if (parts_typeid[i] == _typeid)
                    { // Coloca Def Parts

                        uint def_part = (uint)(((i | (uint)(_typeid << 5)) << 13) | 0x8000400);

                        parts_typeid[i] = (sIff.getInstance().findPart(def_part) != null) ? (uint)def_part : 0;
                        parts_id[i] = 0;

                        break;
                    }
                }
            }

        }

        public void unequipAuxPart(uint _typeid)
        {

            // Invalid Typeid
            if (_typeid == 0u)
            {
                return;
            }
            for (var i = 0u; i < (auxparts.Length); ++i)
            {

                if (auxparts[i] == _typeid)
                {

                    auxparts[i] = 0;

                    // Já desequipou sai
                    break;
                }
            }
        }


        public sbyte getSlotOfStatsFromsbyteEquipedPartItem(Stats __stat)
        {   // Get Slot of stats from Character equiped item

            sbyte value = 0;

            // Invalid Stats type, Unknown type Stats
            if (__stat > Stats.S_CURVE)
                return -1;

            for (var i = 0; i < (Marshal.SizeOf(parts_typeid) / Marshal.SizeOf(parts_typeid[0])); ++i)
            {
                Part part;
                if (parts_id[i] != 0 && (part = sIff.getInstance().findPart(parts_typeid[i])) != null)
                    value += (sbyte)part.SlotStats.getSlot[(int)__stat];
            }

            return value;
        }

        public byte getSlotOfStatsFromCharEquipedPartItem(Stats __stat)
        { // Get Slot of stats from Character equiped item

            byte value = 0;
            Part part = null;

            // Invalid Stats type, Unknown type Stats
            if (__stat > Stats.S_CURVE)
            {
                return 255;
            }

            for (var i = 0u; i < (parts_typeid.Length); ++i)
            {

                if (parts_id[i] != 0 && (part = sIff.getInstance().findPart(parts_typeid[i])) != null)
                {

                    switch (__stat)
                    {
                        case Stats.S_POWER:
                            value += (byte)part.Stats.Power;
                            break;
                        case Stats.S_CONTROL:
                            value += (byte)part.Stats.Control;
                            break;
                        case Stats.S_ACCURACY:
                            value += (byte)part.Stats.Impact;
                            break;
                        case Stats.S_SPIN:
                            value += (byte)part.Stats.Spin;
                            break;
                        case Stats.S_CURVE:
                            value += (byte)part.Stats.Curve;
                            break;
                        default:
                            break;
                    }
                }
            }

            return value;
        }

        public byte getSlotOfStatsFromCharEquipedAuxPart(Stats __stat)
        {

            byte value = 0;
            AuxPart aux_part = null;

            // Invalid Stats type, Unknown type Stats
            if (__stat > Stats.S_CURVE)
            {
                return 255;
            }

            for (var i = 0u; i < (auxparts.Length); ++i)
            {

                if (auxparts[i] != 0 && (aux_part = sIff.getInstance().findAuxPart(auxparts[i])) != null)
                {
                    switch (__stat)
                    {
                        case Stats.S_POWER:
                            value += aux_part.PowerSlot;
                            break;
                        case Stats.S_CONTROL:
                            value += aux_part.ControlSlot;
                            break;
                        case Stats.S_ACCURACY:
                            value += aux_part.ImpactSlot;
                            break;
                        case Stats.S_SPIN:
                            value += aux_part.SpinSlot;
                            break;
                        case Stats.S_CURVE:
                            value += aux_part.CurveSlot;
                            break;
                        default:
                            break;
                    }
                }
            }

            return value;
        }
        //public byte getSlotOfStatsFromSetEffectTable(Stats __stat)
        //{

        //    byte value = 0;
        //    int ret = 0;

        //    // Set Effect Table
        //    SetEffectTable iff_SET = null;

        //    // Ids que já foram
        //    List<uint> check_id = new List<uint>();

        //    // Invalid Stats type, Unknown type Stats
        //    if (__stat > Stats.S_CURVE)
        //    {
        //        return 255;
        //    }

        //    // Part Item                                                                                     
        //    for (var i = 0u; i < (parts_typeid.Length); ++i)
        //    {

        //        if (parts_typeid[i] != 0)
        //        {

        //            iff_SET = sIff.getInstance().findFirstItemInSetEffectTable(parts_typeid[i]);

        //            // O Item no Set Effect Table
        //            if (iff_SET != null)
        //            {

        //                if (check_id.Count == 0 || std::find(check_id.GetEnumerator(),
        //                    check_id.end(), iff_SET.id) == check_id.end())
        //                {

        //                    // add id para o check
        //                    check_id.Add(iff_SET.id);

        //                    // Verifica sem tem todos os itens da tabela de efeito equipados
        //                    ret = 1;
        //                    for (var j = 0u; j < (iff_SET.item._typeid.Length); ++j)
        //                    {

        //                        if (iff_SET.item._typeid[j] != 0u)
        //                        {

        //                            if (sIff.getInstance().getItemGroupIdentify(iff_SET.item._typeid[j]) == iff.PART)
        //                            {

        //                                if (!isPartEquiped(iff_SET.item._typeid[j]))
        //                                {

        //                                    // Não tem o outro item equipado
        //                                    ret = 0;

        //                                    break;
        //                                }

        //                            }
        //                            else if (sIff.getInstance().getItemGroupIdentify(iff_SET.item._typeid[j]) == iff.AUX_PART)
        //                            {

        //                                if (!isAuxPartEquiped(iff_SET.item._typeid[j]))
        //                                {

        //                                    // Não tem o outro item equipado
        //                                    ret = 0;

        //                                    break;
        //                                }

        //                            }
        //                        }
        //                    }

        //                    // Não tem todos os itens equipados
        //                    if (ret == 0)
        //                    {
        //                        continue;
        //                    }

        //                    // Effect 6 ONE_ALL_STATS
        //                    // C++ TO C# CONVERTER WARNING: This 'sizeof' ratio was replaced with a direct reference to the array length:
        //                     std::for_each(iff_SET->effect.effect, ((iff_SET->effect.effect) + (sizeof((iff_SET->effect.effect)) / sizeof((iff_SET->effect.effect)[0]))), [&](auto& _el)
        //                    std::for_each(iff_SET.effect.effect, ((iff_SET.effect.effect) + ((iff_SET.effect.effect).Length)), (_el) =>
        //                    {
        //                        if (_el == IFF.SetEffectTable.eEFFECT.ONE_ALL_STATS)
        //                        {
        //                            value++;
        //                        }
        //                    });

        //                    // Slot
        //                    value += iff_SET.slot[(int)__stat];
        //                }
        //            }
        //        }
        //    }
        //}
        public void initComboDef()
        {   // Initialize o combo de roupas padrões do Character
            clear();
            if (_typeid == 0)
                return;

            for (var i = 0; i < (Marshal.SizeOf(parts_typeid) / Marshal.SizeOf(parts_typeid[0])); ++i)
            {
                var part_typeid = (uint)((((_typeid << 5) | i) << 13) | 0x8000400);

                if (sIff.getInstance().findPart(part_typeid) != null)
                    parts_typeid[i] = part_typeid;
            }
        }
        /// <summary>
        /// Size = 513 bytes
        /// </summary>
        /// <returns></returns>
        public byte[] Build()
        {
            using (var p = new PangyaBinaryWriter())
            {
                p.Write(_typeid);
                p.Write(id);
                p.Write(default_hair);
                p.Write(default_shirts);
                p.Write(gift_flag);
                p.Write(purchase);

                for (var Index = 0; Index < 24; Index++)
                    p.Write(parts_typeid[Index]);

                for (var Index = 0; Index < 24; Index++)
                    p.Write(parts_id[Index]);

                p.WriteZero(216); //deve ser algum objeto ainda nao terminado

                for (int i = 0; i < 5; i++)
                    p.WriteUInt32(auxparts[i]);

                for (int i = 0; i < 4; i++)
                    p.WriteUInt32(cut_in[i]);

                for (int i = 0; i < 5; i++)
                    p.WriteByte(pcl[i]);

                p.WriteUInt32(mastery);

                for (int i = 0; i < 4; i++)
                    p.WriteUInt32(Card_Caddie[i]);

                for (int i = 0; i < 4; i++)
                    p.WriteUInt32(Card_Character[i]);

                for (int i = 0; i < 4; i++)
                    p.WriteUInt32(Card_NPC[i]);
                //if (p.GetSize == 513)
                //    Debug.WriteLine("GetCharacterInfo Size Okay");

                return p.GetBytes;
            }
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
        public void setIDState(ulong _id_state)
        {
            if (m_flag == null || (m_flag.ullFlag == 0))
            {
                m_flag = new uFlag(_id_state);
            }

            m_id_state = new IDStateBlockFlag(_id_state);

            // Block Recursos do player
            //if ((m_id_state.L_BLOCK_LOUNGE/* & 4*/)) // Block Lounge
            //    m_flag..lounge = true; // Block Lounge
            //if ((m_id_state.L_BLOCK_SHOP_LOUNGE/* & 8*/)) // Block Shop Lounge
            //    m_flag.personal_shop = true; // Block Shop Lounge
            //if ((m_id_state.L_BLOCK_GIFT_SHOP/* & 16*/)) // Block Gift Shop
            //    m_flag.gift_shop = true; // Block Gift Shop
            //if ((m_id_state.L_BLOCK_PAPEL_SHOP/* & 32*/)) // Block Papel Shop
            //    m_flag.papel_shop = true; // Block Papel Shop
            //if ((m_id_state.L_BLOCK_SCRATCHY/* & 64*/)) // Block Scratchy
            //    m_flag.scratchy = true; // Block Scratchy
            //if ((m_id_state.L_BLOCK_TICKER/* & 128*/)) // Block Ticker
            //    m_flag.ticker = true; // Block Ticker
            //if ((m_id_state.L_BLOCK_MEMORIAL_SHOP/* & 256*/)) // Block Memorial Shop
            //    m_flag.memorial_shop = true; // Block Memorial Shop
        }

        public IDStateBlockFlag m_id_state;
        public uFlag m_flag;
    }

    // ------------------ Player Account Basic ---------------- //
    // Struct ID State Block Flag
    public class IDStateBlockFlag
    {
        public IDStateBlockFlag(ulong _ul)
        {
            _ull_IDState = _ul;
        }

        private ulong _ull_IDState;

        public ulong ull_IDState
        {
            get { return _ull_IDState; }
            set
            {
                _ull_IDState = value;
                // Atualiza as flags booleanas com base no novo valor de ull_IDState
                L_BLOCK_LOUNGE = (_ull_IDState & 4) == 4;
                L_BLOCK_SHOP_LOUNGE = (_ull_IDState & 8) == 8;
                L_BLOCK_GIFT_SHOP = (_ull_IDState & 16) == 16;
                L_BLOCK_PAPEL_SHOP = (_ull_IDState & 32) == 32;
                L_BLOCK_SCRATCHY = (_ull_IDState & 64) == 64;
                L_BLOCK_TICKER = (_ull_IDState & 128) == 128;
                L_BLOCK_MEMORIAL_SHOP = (_ull_IDState & 256) == 256;
                L_BLOCK_TEMPORARY = (_ull_IDState & 512) == 512;
                L_BLOCK_FOREVER = (_ull_IDState & 1024) == 1024;
            }
        }

        public bool L_BLOCK_LOUNGE
        {
            get => (_ull_IDState & 4) == 4;
            set => _ull_IDState = value ? (_ull_IDState | 4) : (_ull_IDState & ~(4ul));
        }

        public bool L_BLOCK_SHOP_LOUNGE
        {
            get => (_ull_IDState & 8) == 8;
            set => _ull_IDState = value ? (_ull_IDState | 8) : (_ull_IDState & ~(8ul));
        }

        public bool L_BLOCK_GIFT_SHOP
        {
            get => (_ull_IDState & 16) == 16;
            set => _ull_IDState = value ? (_ull_IDState | 16) : (_ull_IDState & ~(16ul));
        }

        public bool L_BLOCK_PAPEL_SHOP
        {
            get => (_ull_IDState & 32) == 32;
            set => _ull_IDState = value ? (_ull_IDState | 32) : (_ull_IDState & ~(32ul));
        }

        public bool L_BLOCK_SCRATCHY
        {
            get => (_ull_IDState & 64) == 64;
            set => _ull_IDState = value ? (_ull_IDState | 64) : (_ull_IDState & ~(64ul));
        }

        public bool L_BLOCK_TICKER
        {
            get => (_ull_IDState & 128) == 128;
            set => _ull_IDState = value ? (_ull_IDState | 128) : (_ull_IDState & ~(128ul));
        }

        public bool L_BLOCK_MEMORIAL_SHOP
        {
            get => (_ull_IDState & 256) == 256;
            set => _ull_IDState = value ? (_ull_IDState | 256) : (_ull_IDState & ~(256ul));
        }

        public bool L_BLOCK_TEMPORARY
        {
            get => (_ull_IDState & 512) == 512;
            set => _ull_IDState = value ? (_ull_IDState | 512) : (_ull_IDState & ~(512ul));
        }

        public bool L_BLOCK_FOREVER
        {
            get => (_ull_IDState & 1024) == 1024;
            set => _ull_IDState = value ? (_ull_IDState | 1024) : (_ull_IDState & ~(1024ul));
        }

        public int block_time;
    }

    #endregion
}
