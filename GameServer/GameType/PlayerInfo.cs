using GameServer.Cmd;
using PangyaAPI.Network.Pangya_St;
using PangyaAPI.Utilities;
using PangyaAPI.Utilities.Log;
using System;
using System.Linq;
using _smp = PangyaAPI.Utilities.Log;
using snmdb = PangyaAPI.SQL.Manager;
using GameServer.Game.Manager;
using System.Collections.Generic;
using static GameServer.GameType._Define;
using PangyaAPI.SQL.Manager;
using GameServer.Session;
using GameServer.Game.Utils;

namespace GameServer.GameType
{
    public partial class PlayerInfo : player_info
    {

        public static readonly uint[] ExpByLevel = { 30, 40, 50, 60, 70, 140,					// ROOKIE
												   105, 125, 145, 165, 330,					// BEGINNER
												   248, 278, 308, 338, 675,					// JUNIOR
												   506, 546, 586, 626, 1253,					// SENIOR
												   1002, 1052, 1102, 1152, 2304,				// AMADOR
												   1843, 1903, 1963, 2023, 4046,				// SEMI PRO
												   3237, 3307, 3377, 3447, 6894,				// PRO
												   5515, 5595, 5675, 5755, 11511,				// NACIONAL
												   8058, 8148, 8238, 8328, 16655,				// WORLD PRO
												   8328, 8428, 8528, 8628, 17255,				// MESTRE
												   9490, 9690, 9890, 10090, 20181,			// TOP_MASTER
												   20181, 20481, 20781, 21081, 42161,			// WORLD_MASTER
												   37945, 68301, 122942, 221296, 442592,		// LEGEND
												   663887, 995831, 1493747, 2240620, 0 };// INFINIT_LEGEND

        public class stIdentifyKey
        {

            public uint _typeid;
            public uint id;
            public stIdentifyKey(uint __typeid, uint _id)
            {
                _typeid = (__typeid);
                id = (_id);
            }
            public static bool operator <(stIdentifyKey MyIntLeft, stIdentifyKey _ik)
            {

                // Classifica pelo ID, depois o typeid
                if (MyIntLeft.id != _ik.id)
                    return MyIntLeft.id < _ik.id;
                else
                    return MyIntLeft._typeid < _ik._typeid;
            }

            public static bool operator >(stIdentifyKey MyIntLeft, stIdentifyKey _ik)
            {

                // Classifica pelo ID, depois o typeid
                if (MyIntLeft.id != _ik.id)
                    return MyIntLeft.id < _ik.id;
                else
                    return MyIntLeft._typeid < _ik._typeid;
            }

        }

        /*
			 Skin[Title] map Call back function to trate Condition 
			*/
        public class stTitleMapCallback
        {

            // Function Callback type

            // Constructor
            public stTitleMapCallback(uint _ul = 0)
            {
                // Construtor sem callback ou argumento
            }

            // Construtor com callback e argumento
            public stTitleMapCallback(Func<object, int> _callback, object _arg)
            {
                call_back = _callback;
                arg = _arg;
            }

            public uint exec()
            {
                if (call_back != null)
                {
                    int result = call_back.Invoke(arg); // Chama o callback e pega o resultado (int)
                    return (uint)result; // Retorna o valor como uint
                }
                else
                {
                    // Exemplo de mensagem de erro
                    // _smp::message_pool::getInstance().push(new message("[PlayerInfo::stTitleMapCallBack::exec][Error] call_back is null.", CL_FILE_LOG_AND_CONSOLE));
                    return 0;
                }
            }
            Func<object, int> call_back;
            object arg;
        }


        private Dictionary<uint/*key*/, stTitleMapCallback> mp_title_callback { get; set; }

        static int better_hit_pangya_bronze(object _arg)
        {
            var pi = ((PlayerInfo)_arg);//BEGIN_CALL_BACK_TITLE_CONDITION(better_hit_pangya_bronze);

            if (pi.ui.getPangyaShotRate() >= 70.0)
                return 1; ; // passa na condição

            return 0; //END_CALL_BACK_TITLE_CONDITION(better_hit_pangya_bronze);
        }

        static int better_fairway_bronze(object _arg)
        {
            var pi = ((PlayerInfo)_arg);//BEGIN_CALL_BACK_TITLE_CONDITION(better_fairway_bronze);

            if (pi.ui.getFairwayRate() >= 70.0)
                return 1; ; // passa na condição

            return 0; //END_CALL_BACK_TITLE_CONDITION(better_fairway_bronze);
        }

        static int better_putt_bronze(object _arg)
        {
            var pi = ((PlayerInfo)_arg);//BEGIN_CALL_BACK_TITLE_CONDITION(better_putt_bronze);

            if (pi.ui.getPuttRate() >= 80.0)
                return 1; ; // passa na condição

            return 0; //END_CALL_BACK_TITLE_CONDITION(better_putt_bronze);
        }

        static int better_quit_rate_bronze(object _arg)
        {
            var pi = ((PlayerInfo)_arg);//BEGIN_CALL_BACK_TITLE_CONDITION(better_quit_rate_bronze);

            if (pi.ui.getQuitRate() <= 3.0)
                return 1; ; // passa na condição

            return 0; //END_CALL_BACK_TITLE_CONDITION(better_quit_rate_bronze);
        }

        static int better_hit_pangya_silver(object _arg)
        {
            var pi = ((PlayerInfo)_arg);//BEGIN_CALL_BACK_TITLE_CONDITION(better_hit_pangya_silver);

            if (pi.ui.getPangyaShotRate() >= 77.0)
                return 1; ; // passa na condição

            return 0; //END_CALL_BACK_TITLE_CONDITION(better_hit_pangya_silver);
        }

        static int better_fairway_silver(object _arg)
        {
            var pi = ((PlayerInfo)_arg);//BEGIN_CALL_BACK_TITLE_CONDITION(better_fairway_silver);

            if (pi.ui.getFairwayRate() >= 72.0)
                return 1; ; // passa na condição

            return 0; //END_CALL_BACK_TITLE_CONDITION(better_fairway_silver);
        }

        static int better_putt_silver(object _arg)
        {
            var pi = ((PlayerInfo)_arg);//BEGIN_CALL_BACK_TITLE_CONDITION(better_putt_silver);

            if (pi.ui.getPuttRate() >= 90.0)
                return 1; ; // passa na condição

            return 0; //END_CALL_BACK_TITLE_CONDITION(better_putt_silver);
        }

        static int better_quit_rate_silver(object _arg)
        {
            var pi = ((PlayerInfo)_arg);//BEGIN_CALL_BACK_TITLE_CONDITION(better_quit_rate_silver);

            if (pi.ui.getQuitRate() <= 2.0)
                return 1; ; // passa na condição

            return 0; //END_CALL_BACK_TITLE_CONDITION(better_quit_rate_silver);
        }

        static int better_hit_pangya_gold(object _arg)
        {
            var pi = ((PlayerInfo)_arg);//BEGIN_CALL_BACK_TITLE_CONDITION(better_hit_pangya_gold);

            if (pi.ui.getPangyaShotRate() >= 85.0)
                return 1; ; // passa na condição

            return 0; //END_CALL_BACK_TITLE_CONDITION(better_hit_pangya_gold);
        }

        static int better_fairway_gold(object _arg)
        {
            var pi = ((PlayerInfo)_arg);//BEGIN_CALL_BACK_TITLE_CONDITION(better_fairway_gold);

            if (pi.ui.getFairwayRate() >= 90.0)
                return 1; ; // passa na condição

            return 0; //END_CALL_BACK_TITLE_CONDITION(better_fairway_gold);
        }

        static int better_putt_gold(object _arg)
        {
            var pi = ((PlayerInfo)_arg);//BEGIN_CALL_BACK_TITLE_CONDITION(better_putt_gold);

            if (pi.ui.getPuttRate() >= 95.0)
                return 1; ; // passa na condição

            return 0; //END_CALL_BACK_TITLE_CONDITION(better_putt_gold);
        }

        static int better_quit_rate_gold(object _arg)
        {
            var pi = ((PlayerInfo)_arg);//BEGIN_CALL_BACK_TITLE_CONDITION(better_quit_rate_gold);

            if (pi.ui.getQuitRate() <= 1.0)
                return 1; ; // passa na condição

            return 0; //END_CALL_BACK_TITLE_CONDITION(better_quit_rate_gold);
        }

        static int atirador_de_ouro(object _arg)
        {
            var pi = ((PlayerInfo)_arg);//BEGIN_CALL_BACK_TITLE_CONDITION(atirador_de_ouro);

            if (pi.ti_current_season.getSumGold() >= 10u)
                return 1; ; // passa na condição

            return 0; //END_CALL_BACK_TITLE_CONDITION(atirador_de_ouro);
        }

        static int atirador_de_silver(object _arg)
        {
            var pi = ((PlayerInfo)_arg);//BEGIN_CALL_BACK_TITLE_CONDITION(atirador_de_silver);

            if (pi.ti_current_season.getSumSilver() >= 10u)
                return 1; ; // passa na condição

            return 0; //END_CALL_BACK_TITLE_CONDITION(atirador_de_silver);
        }

        static int atirador_de_bronze(object _arg)
        {
            var pi = ((PlayerInfo)_arg);//BEGIN_CALL_BACK_TITLE_CONDITION(atirador_de_bronze);

            if (pi.ti_current_season.getSumBronze() >= 10u)
                return 1; ; // passa na condição

            return 0; //END_CALL_BACK_TITLE_CONDITION(atirador_de_bronze);
        }

        static int master_course(object _arg)
        {
            var pi = ((PlayerInfo)_arg);//BEGIN_CALL_BACK_TITLE_CONDITION(master_course);

            if (pi.isMasterCourse())
                return 1; ; // passa na condição

            return 0; //END_CALL_BACK_TITLE_CONDITION(master_course);
        }

        static int natural_record_80(object _arg)
        {
            var pi = ((PlayerInfo)_arg);//BEGIN_CALL_BACK_TITLE_CONDITION(natural_record_80);

            // No JP ele pega o do Grand Prix
            if (pi.getSumRecordGrandPrix() <= -80)
                return 1; ; // passa na condição

            return 0; //END_CALL_BACK_TITLE_CONDITION(natural_record_80);
        }

        static int natural_record_200(object _arg)
        {
            var pi = ((PlayerInfo)_arg);//BEGIN_CALL_BACK_TITLE_CONDITION(natural_record_200);

            // No JP ele pega o do Grand Prix
            if (pi.getSumRecordGrandPrix() <= -200)
                return 1; ; // passa na condição

            return 0; //END_CALL_BACK_TITLE_CONDITION(natural_record_200);
        }

        static int natural_record_300(object _arg)
        {
            var pi = ((PlayerInfo)_arg);//BEGIN_CALL_BACK_TITLE_CONDITION(natural_record_300);

            // No JP ele pega o do Grand Prix
            if (pi.getSumRecordGrandPrix() <= -300)
                return 1; ; // passa na condição

            return 0; //END_CALL_BACK_TITLE_CONDITION(natural_record_300);
        }

        static int natural_record_350(object _arg)
        {
            var pi = ((PlayerInfo)_arg);//BEGIN_CALL_BACK_TITLE_CONDITION(natural_record_350);

            // No JP ele pega o do Grand Prix
            if (pi.getSumRecordGrandPrix() <= -350)
                return 1; ; // passa na condição

            return 0; //END_CALL_BACK_TITLE_CONDITION(natural_record_350);
        }

        static int natural_record_390(object _arg)
        {
            var pi = ((PlayerInfo)_arg);//BEGIN_CALL_BACK_TITLE_CONDITION(natural_record_390);

            // No JP ele pega o do Grand Prix
            if (pi.getSumRecordGrandPrix() <= -390)
                return 1; ; // passa na condição

            return 0; //END_CALL_BACK_TITLE_CONDITION(natural_record_390);
        }

        static int natural_record_420(object _arg)
        {
            var pi = ((PlayerInfo)_arg);//BEGIN_CALL_BACK_TITLE_CONDITION(natural_record_420);

            // No JP ele pega o do Grand Prix
            if (pi.getSumRecordGrandPrix() <= -420)
                return 1; ; // passa na condição

            return 0; //END_CALL_BACK_TITLE_CONDITION(natural_record_420);
        }

        static int natural_record_470(object _arg)
        {
            var pi = ((PlayerInfo)_arg);//BEGIN_CALL_BACK_TITLE_CONDITION(natural_record_470);

            // No JP ele pega o do Grand Prix
            if (pi.getSumRecordGrandPrix() <= -470)
                return 1; ; // passa na condição

            return 0; //END_CALL_BACK_TITLE_CONDITION(natural_record_470);
        }

        static int natural_record_540(object _arg)
        {
            var pi = ((PlayerInfo)_arg);//BEGIN_CALL_BACK_TITLE_CONDITION(natural_record_540);

            // No JP ele pega o do Grand Prix
            if (pi.getSumRecordGrandPrix() <= -540)
                return 1; ; // passa na condição

            return 0; //END_CALL_BACK_TITLE_CONDITION(natural_record_540);
        }

        public PlayerInfo()
        {
            clear();

            // Inicializa o map de Title call back condition
            mp_title_callback.Add(0x15, new stTitleMapCallback(better_hit_pangya_bronze, this));
            mp_title_callback.Add(0x16, new stTitleMapCallback(better_fairway_bronze, this));
            mp_title_callback.Add(0x17, new stTitleMapCallback(better_putt_bronze, this));
            mp_title_callback.Add(0x18, new stTitleMapCallback(master_course, this));
            mp_title_callback.Add(0x19, new stTitleMapCallback(atirador_de_ouro, this));
            mp_title_callback.Add(0x1a, new stTitleMapCallback(atirador_de_silver, this));
            mp_title_callback.Add(0x1b, new stTitleMapCallback(atirador_de_bronze, this));
            mp_title_callback.Add(0x1C, new stTitleMapCallback(better_quit_rate_bronze, this));
            mp_title_callback.Add(0x32, new stTitleMapCallback(better_hit_pangya_silver, this));
            mp_title_callback.Add(0x33, new stTitleMapCallback(better_fairway_silver, this));
            mp_title_callback.Add(0x34, new stTitleMapCallback(better_putt_silver, this));
            mp_title_callback.Add(0x35, new stTitleMapCallback(better_quit_rate_silver, this));
            mp_title_callback.Add(0x45, new stTitleMapCallback(natural_record_420, this));
            mp_title_callback.Add(0x46, new stTitleMapCallback(natural_record_390, this));
            mp_title_callback.Add(0x47, new stTitleMapCallback(natural_record_350, this));
            mp_title_callback.Add(0x48, new stTitleMapCallback(natural_record_300, this));
            mp_title_callback.Add(0x49, new stTitleMapCallback(natural_record_200, this));
            mp_title_callback.Add(0x4a, new stTitleMapCallback(natural_record_80, this));
            mp_title_callback.Add(0x7B, new stTitleMapCallback(better_quit_rate_gold, this));
            mp_title_callback.Add(0x7C, new stTitleMapCallback(better_putt_gold, this));
            mp_title_callback.Add(0x7D, new stTitleMapCallback(better_fairway_gold, this));
            mp_title_callback.Add(0x7E, new stTitleMapCallback(better_hit_pangya_gold, this));
            mp_title_callback.Add(0x17C, new stTitleMapCallback(natural_record_470, this));
            mp_title_callback.Add(0x17D, new stTitleMapCallback(natural_record_540, this));
        }

        public void clear()
        {
            m_pl = new stPlayerLocationDB();
            m_update_pang_db = new stSyncUpdateDB();
            m_update_cookie_db = new stSyncUpdateDB();
            mp_title_callback = new Dictionary<uint, stTitleMapCallback>();
            m_cap = new uCapability();
            cg = new CouponGacha();
            mi = new MemberInfoEx();
            ui = new UserInfoEx();
            ei = new EquipedItem();
            cwlul = new ClubSetWorkshopLasUpLevel();
            cwtc = new ClubSetWorkshopTransformClubSet();
            pt = new PremiumTicket();
            ti_current_season = new TrofelInfo();
            ti_rest_season = new TrofelInfo();
            TutoInfo = new TutorialInfo();
            ue = new UserEquip();
            cmu = new chat_macro_user();
            a_ms_normal = new List<MapStatisticsEx>();
            a_msa_normal = new List<MapStatisticsEx>();
            a_ms_natural = new List<MapStatisticsEx>();
            a_msa_natural = new List<MapStatisticsEx>();
            a_ms_grand_prix = new List<MapStatisticsEx>();
            a_msa_grand_prix = new List<MapStatisticsEx>();

            for (int i = 0; i < MS_NUM_MAPS; i++)
            {
                a_ms_normal.Add(new MapStatisticsEx());
                a_msa_normal.Add(new MapStatisticsEx());
                a_ms_natural.Add(new MapStatisticsEx());
                a_msa_natural.Add(new MapStatisticsEx());
                a_ms_grand_prix.Add(new MapStatisticsEx());
                a_msa_grand_prix.Add(new MapStatisticsEx());
            }

            aa_ms_normal_todas_season = new List<MapStatisticsEx>();

            // Inicializando cada sessão com 21 mapas (ou MS_NUM_MAPS mapas)
            for (int j = 0; j < 9; j++)
            {
                for (int i = 0; i < MS_NUM_MAPS; i++)
                {
                    aa_ms_normal_todas_season.Add(new MapStatisticsEx());  // Inicializa cada mapa
                }
            }

            mp_scl = new Dictionary<uint, StateCharacterLounge>();

            mp_ce = new CharacterManager();
            mp_ci = new CaddieManager();
            mp_mi = new MascotManager();
            mp_wi = new WarehouseManager();

            mp_fi = new Dictionary<uint, FriendInfo>();   // Friend List

            ari = new AttendanceRewardInfoEx();

            mgr_achievement = new MgrAchievement();   // Manager Achievement
            v_card_info = new CardManager();
            v_cei = new CardEquipManager();
            v_ib = new List<ItemBuffEx>();
            mp_ui = new Dictionary<stIdentifyKey/*uint/*ID*/, UpdateItem>();
            v_tsi_current_season = new List<TrofelEspecialInfo>();
            v_tsi_rest_season = new List<TrofelEspecialInfo>();
            v_tgp_current_season = new List<TrofelEspecialInfo>();   // Trofel Grand Prix
            v_tgp_rest_season = new List<TrofelEspecialInfo>(); // Trofel Grand Prix
            v_mri = new List<MyRoomItem>();     // MyRoomItem
            v_gpc = new List<GrandPrixClear>(); // Grand Prix Clear os grand prix que o player já jogou

            mrc = new MyRoomConfig();
            df = new DolfiniLocker();   // DolfiniLocker
            gi = new GuildInfoEx();
            dqiu = new DailyQuestInfoUser();
            l5pg = new Last5PlayersGame();
            m_mail_box = new PlayerMailBox();
        }

      

        public ulong cookie { get; set; }
        public CouponGacha cg { get; set; }
        public MemberInfoEx mi { get; set; }
        public UserInfoEx ui { get; set; }
        public EquipedItem ei { get; set; }
        ClubSetWorkshopLasUpLevel cwlul { get; set; }
        ClubSetWorkshopTransformClubSet cwtc { get; set; }
        public PremiumTicket pt { get; set; }
        public TrofelInfo ti_current_season { get; set; }
        public TrofelInfo ti_rest_season { get; set; }
        public TutorialInfo TutoInfo { get; set; }
        public UserEquip ue { get; set; }
        public chat_macro_user cmu { get; set; }
        public List<MapStatisticsEx> a_ms_normal { get; set; }
        public List<MapStatisticsEx> a_msa_normal { get; set; }
        public List<MapStatisticsEx> a_ms_natural { get; set; }
        public List<MapStatisticsEx> a_msa_natural { get; set; }
        public List<MapStatisticsEx> a_ms_grand_prix { get; set; }
        public List<MapStatisticsEx> a_msa_grand_prix { get; set; }
        public List<MapStatisticsEx> aa_ms_normal_todas_season { get; set; }   // Esse aqui é diferente, explico ele no pacote InitialLogin
        public Dictionary<uint, StateCharacterLounge> mp_scl { get; set; }

        public CharacterManager mp_ce { get; set; }      //  
        public CaddieManager mp_ci { get; set; }
        public MascotManager mp_mi { get; set; }
        public WarehouseManager mp_wi { get; set; }

        public Dictionary<uint/*UID*/, FriendInfo> mp_fi { get; set; }    // Friend List

        public AttendanceRewardInfoEx ari { get; set; }

       public MgrAchievement mgr_achievement;             // Manager Achievement
        public CardManager v_card_info { get; set; }

        public CardEquipManager v_cei { get; set; }
        public List<ItemBuffEx> v_ib { get; set; }

        public Dictionary<stIdentifyKey/*uint/*ID*/, UpdateItem> mp_ui { get; set; }

        public List<TrofelEspecialInfo> v_tsi_current_season { get; set; }
        public List<TrofelEspecialInfo> v_tsi_rest_season { get; set; }
        public List<TrofelEspecialInfo> v_tgp_current_season { get; set; }   // Trofel Grand Prix
        public List<TrofelEspecialInfo> v_tgp_rest_season { get; set; } // Trofel Grand Prix
        public List<MyRoomItem> v_mri { get; set; }      // MyRoomItem

        public List<GrandPrixClear> v_gpc { get; set; }  // Grand Prix Clear os grand prix que o player já jogou

        public MyRoomConfig mrc { get; set; }
        public DolfiniLocker df { get; set; }   // DolfiniLocker
        public GuildInfoEx gi { get; set; }
        public DailyQuestInfoUser dqiu { get; set; }
        public Last5PlayersGame l5pg { get; set; }
        public class stLocation
        {
            public float x { get; set; }
            public float y { get; set; }
            public float z { get; set; }
            public float r { get; set; }    // Face

            public static stLocation operator +(stLocation _old_location, stLocation _add_location)
            {
                return new stLocation()
                {
                    x = _old_location.x + _add_location.x,
                    y = _old_location.y + _add_location.y,
                    z = _old_location.z + _add_location.z,
                    r = _old_location.r + _add_location.r
                };
            }
            public static stLocation operator -(stLocation _old_location, stLocation _add_location)
            {
                return new stLocation()
                {
                    x = _old_location.x - _add_location.x,
                    y = _old_location.y - _add_location.y,
                    z = _old_location.z - _add_location.z,
                    r = _old_location.r - _add_location.r
                };
            }
        }
        public stLocation location { get; set; } = new stLocation();
        public sbyte place { get; set; } = -1;            // Lugar que o player está no momento
        public byte lobby { get; set; } = DEFAULT_CHANNEL;            // Lobby
        public byte channel { get; set; } = DEFAULT_CHANNEL;          // Channel
        public byte whisper { get; set; } = 1; // Whisper 0 e 1, 0 OFF, 1 ON
        public uint state { get; set; }
        public uint state_lounge { get; set; }
        public uCapability m_cap { get; set; }   //chamar de outra forma
        public ulong grand_zodiac_pontos { get; set; }
        public ulong m_legacy_tiki_pts { get; set; } // Point Shop(Tiki Shop antigo)                                          
        //// Mail Box
        public PlayerMailBox m_mail_box { get; set; }

        public stPlayerLocationDB m_pl { get; set; }
        public stSyncUpdateDB m_update_pang_db;
        public stSyncUpdateDB m_update_cookie_db;


        public void addCookie(ulong _cookie)
        {
            if (_cookie <= 0)
                throw new exception("[PlayerInfo::addCookie][Error] _cookie valor invalido: " + _cookie);

            try
            {
                // Check alteration on cookie of DB 
                if (checkAlterationCookieOnDB())
                    throw new exception("[PlayerInfo::addCookie][Error] Player[UID=" + uid + "] cookie on db is different of server.");

                cookie += _cookie;

                //m_update_cookie_db.requestUpdateOnDB();

                //  snmdb::NormalManagerDB.add(2, new CmdUpdateCookie(m_uid, _cookie, CmdUpdateCookie::INCREASE), SQLDBResponse, this);


            }
            catch (exception e)
            {

                _smp::message_pool.push(new message("[PlayerInfo::addCookie][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                throw;
            }

            _smp::message_pool.push(new message("[PlayerInfo::addCookie][Log] Player: " + uid + ", ganhou " + _cookie + " e ficou com " + cookie + " Cookie(s).", type_msg.CL_FILE_LOG_AND_CONSOLE));
        }

        public void addCookie(uint _uid, ulong _cookie)
        {
        }

        public int addExp(uint _exp)
        {
            if (_exp == 0)
                throw new exception("[PlayerInfo::addExp][Error] _exp is invalid(zero)", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PLAYER_INFO, 21, 0));

            int ret = -1;

            var exp = (uint)~0u;

            try
            {

                if ((exp = ExpByLevel[level]) == ~0u)
                    _smp::message_pool.push(new message("[AddExp][Log] player[UID=" + uid + "] ja eh infinit legend I, nao precisar mais add exp para ele.", type_msg.CL_FILE_LOG_AND_CONSOLE));
                else
                {
                    // Att Exp do player
                    ui.exp += _exp;

                    if (ui.exp >= exp)
                    { // LEVEL UP!
                        byte new_level = 0, ant_level = 0;

                        // Atualiza todos os levels das estruturas que o player tem
                        ant_level = (byte)level;

                        // Check if up n levels
                        do
                        {
                            new_level = (byte)++level;

                            mi.level = new_level;
                            ui.level = new_level;

                            // Att Exp do player
                            ui.exp -= exp;

                            // LEVEL UP!
                            ret = new_level - ant_level;

                        } while ((exp = ExpByLevel[level]) != ~0u && ui.exp > exp);

                        _smp::message_pool.push(new message("[AddExp][Log] player[UID=" + uid + "] Upou de Level[FROM=" + ant_level + ", TO="
                                + new_level + "]", type_msg.CL_FILE_LOG_AND_CONSOLE));

                    }
                    else // Update só a Exp
                        ret = 0;

                    _smp::message_pool.push(new message("[AddExp][Log] player[UID=" + uid + "] adicionou Experiencia[value=" + _exp + "] e ficou com [LEVEL="
                            + level + ", EXP=" + ui.exp + "]", type_msg.CL_FILE_LOG_AND_CONSOLE));

                    // UPDATE ON DB, LEVEL AND EXP
                    snmdb::NormalManagerDB.add(3, new Cmd.CmdUpdateLevelAndExp(uid, level, ui.exp), SQLDBResponse, this);
                }

            }
            catch (exception e)
            {


                _smp::message_pool.push(new message("[PlayerInfo::addExp][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                throw;
            }

            return ret;
        }

        public void addGrandZodiacPontos(ulong _pontos)
        {
            if (_pontos < 0)
                throw new exception("[PlayerInfo::addGrandZodiacPontos][Error] invalid _pontos(" + _pontos + "), ele eh negativo.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PLAYER_INFO, 101, 0));

            grand_zodiac_pontos += _pontos;

            // Update no Banco de dados
            snmdb::NormalManagerDB.add(8, new CmdGrandZodiacPontos(uid, (uint)grand_zodiac_pontos, CmdGrandZodiacPontos.eCMD_GRAND_ZODIAC_TYPE.CGZT_UPDATE), SQLDBResponse, this);

            // Log
            _smp::message_pool.push(new message("[PlayerInfo::addGrandZodiacPontos][Log] Player[UID=" + uid
                    + "] add " + _pontos + " pontos do grand zodiac e ficou com " + grand_zodiac_pontos, type_msg.CL_FILE_LOG_AND_CONSOLE));

        }

        public void addMoeda(ulong _pang, ulong _cookie)
        {
            if (_pang > 0)
                addPang(_pang);

            if (_cookie > 0)
                addCookie(_cookie);
        }

        public void addPang(ulong _pang)
        {

            if (_pang <= 0)
                throw new exception("[PlayerInfo::addPang][Error] _pang valor invalido: " + _pang);

            try
            {

                // Check alteration on pang of DB 
                if (checkAlterationPangOnDB())
                {

                    // Pang é diferente atualiza o pang com o valor do banco de daos
                    _smp::message_pool.push(new message("[PlayerInfo::addPang][Error] Player[UID=" + uid + "] pang on db is different of server.", type_msg.CL_FILE_LOG_AND_CONSOLE));

                    var old_pang = ui.pang;

                    // Atualiza o valor do pang do server com o do banco de dados
                    updatePang();

                    // Log
                    _smp::message_pool.push(new message("[PlayerInfo::addPang][Log] Player[UID=" + uid
                            + "] o Pang[DB=" + ui.pang + ", GS=" + old_pang
                            + "] no banco de dados eh diferente do que esta no server, atualiza para o valor do banco de dados.", type_msg.CL_FILE_LOG_AND_CONSOLE));
                }

                // Add o pang para o player
                ui.pang += _pang;

                //m_update_pang_db.requestUpdateOnDB();

                //snmdb::NormalManagerDB.add(1, new CmdUpdatePang(m_uid, _pang, CmdUpdatePang::INCREASE), PlayerInfo::SQLDBResponse, this);


            }
            catch (exception e)
            {

                _smp::message_pool.push(new message("[PlayerInfo::addPang][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                throw;
            }
            _smp::message_pool.push(new message("[PlayerInfo::addPang][Log] Player: " + uid + ", ganhou " + _pang + " e ficou com " + ui.pang + " Pang(s).", type_msg.CL_FILE_LOG_AND_CONSOLE));

        }

        public void addPang(uint _uid, ulong _pang)
        {
            if (_pang <= 0)
                throw new exception("[PlayerInfo::addPang][Error] _pang valor invalido: " + _pang, ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PLAYER_INFO, 21, 0));

            snmdb::NormalManagerDB.add(1, new CmdUpdatePang(_uid, _pang, CmdUpdatePang.T_UPDATE_PANG.INCREASE), SQLDBResponse, null);

            _smp::message_pool.push(new message("[PlayerInfo::addPang][Log] Player: " + _uid + ", ganhou " + _pang + " Pang(s).", type_msg.CL_FILE_LOG_AND_CONSOLE));
        }


        public void addUserInfo(UserInfoEx _ui, ulong _total_pang_win_game = 0)
        {
            ui.add(_ui, (uint)_total_pang_win_game);

            // Update User Info ON DB
            updateUserInfo();
        }

        public bool checkAlterationCookieOnDB()
        {
            var cmd_cp = new CmdCookie(uid);    // Waiter

            snmdb::NormalManagerDB.add(0, cmd_cp, null, null);


            if (cmd_cp.getException().getCodeError() != 0)
                throw cmd_cp.getException();

            return (cmd_cp.getCookie() != cookie);
        }

        public bool checkAlterationPangOnDB()
        {
            var cmd_pang = new CmdPang(uid);    // Waiter

            snmdb::NormalManagerDB.add(0, cmd_pang, null, null);

            if (cmd_pang.getException().getCodeError() != 0)
                throw cmd_pang.getException();

            return (cmd_pang.getPang() != ui.pang);
        }

        public bool checkEquipedItem(uint _typeid)
        {
            return false;
        }

        public PlayerRoomInfo.uItemBoost checkEquipedItemBoost()
        {

            PlayerRoomInfo.uItemBoost ib = new PlayerRoomInfo.uItemBoost();

            // Pang
            //for (auto & _el : mp_wi)
            //{

            //    // Pang Boost X2
            //    // Verifica a quantidade do item para gastar menos processo se ele não tiver a quantidade necessária para ativar a flag
            //    if (_el.second.STDA_C_ITEM_QNTD > 0 && std::find(passive_item_pang_x2, LAST_ELEMENT_IN_ARRAY(passive_item_pang_x2), _el.second._typeid) != LAST_ELEMENT_IN_ARRAY(passive_item_pang_x2))
            //        ib.stItemBoost.ucPangMastery = 1u;

            //    // Pang Boost X4
            //    // Verifica a quantidade do item para gastar menos processo se ele não tiver a quantidade necessária para ativar a flag
            //    if (_el.second.STDA_C_ITEM_QNTD > 0 && std::find(passive_item_pang_x4, LAST_ELEMENT_IN_ARRAY(passive_item_pang_x4), _el.second._typeid) != LAST_ELEMENT_IN_ARRAY(passive_item_pang_x4))
            //        ib.stItemBoost.ucPangNitro = 1u;

            //    // Tenta não consumir mais processo, quando já estiver as duas flag setada.
            //    // Tentando verificar outros itens que possa ter ainda no map
            //    if (ib.stItemBoost.ucPangMastery == 1u && ib.stItemBoost.ucPangNitro == 1u)
            //        break;
            //}

            return ib;
        }

        public void consomeCookie(ulong _cookie)
        {
        }

        public void consomeMoeda(ulong _pang, ulong _cookie)
        {
        }

        public void consomePang(ulong _pang)
        {
        }

        public CaddieInfoEx findCaddieById(uint _id)
        {
            return mp_ci.findCaddieById(_id);
        }

        public CaddieInfoEx findCaddieByTypeid(uint _typeid)
        {
            return mp_ci.findCaddieByTypeid(_typeid);
        }

        public CaddieInfoEx findCaddieByTypeidAndId(uint _typeid = 0, uint _id = 0)
        {
            return mp_ci.findCaddieByTypeidAndId(_typeid, _id);
        }

        public CardInfo findCardById(uint _id)
        {
            return null;
        }

        public CardInfo findCardByTypeid(uint _typeid)
        {
            return null;
        }

        public CardEquipInfoEx findCardEquipedById(int _id, int _char_typeid, int _slot)
        {
            return null;
        }

        public CardEquipInfoEx findCardEquipedByTypeid(uint _typeid, int _char_typeid = 0, int _slot = 0, int _tipo = 0, int _efeito = 0)
        {
            return null;
        }

        public CharacterInfo findCharacterById(uint _id)
        {
            return this.mp_ce.findCharacterById(_id);
        }

        public CharacterInfo findCharacterByTypeid(uint _typeid)
        {
            return null;
        }

        public CharacterInfo findCharacterByTypeidAndId(uint _typeid, uint _id)
        {
            return null;
        }

        public FriendInfo findFriendInfoById(string _id)
        {
            return null;
        }

        public FriendInfo findFriendInfoByNickname(string _nickname)
        {
            return null;
        }

        public FriendInfo findFriendInfoByUID(int _uid)
        {
            if (_uid == 0u)
            {

                return null;
            }

            var it = mp_fi.Where(c => c.Key == _uid);

            return it.Any() ? it.First().Value : null;
        }

        public GrandPrixClear findGrandPrixClear(uint _typeid)
        {
            return null;
        }

        public MascotInfoEx findMascotById(uint _id)
        {
            return (MascotInfoEx)mp_mi.findMascotById((uint)_id);
        }

        public MascotInfoEx findMascotByTypeid(uint _typeid)
        {
            return null;
        }

        public MascotInfoEx findMascotByTypeidAndId(uint _typeid, uint _id)
        {
            return null;
        }

        public MyRoomItem findMyRoomItemById(uint _id)
        {
            return null;
        }

        public MyRoomItem findMyRoomItemByTypeid(uint _typeid)
        {
            return new MyRoomItem();
        }

        public TrofelEspecialInfo findTrofelEspecialById(uint _id)
        {
            return new TrofelEspecialInfo();
        }

        public TrofelEspecialInfo findTrofelEspecialByTypeid(uint _typeid)
        {
            return new TrofelEspecialInfo();
        }

        public TrofelEspecialInfo findTrofelEspecialByTypeidAndId(uint _typeid, uint _id)
        {
            return new TrofelEspecialInfo();
        }

        public TrofelEspecialInfo findTrofelGrandPrixById(uint _id)
        {
            return new TrofelEspecialInfo();
        }

        public TrofelEspecialInfo findTrofelGrandPrixByTypeid(uint _typeid)
        {
            return new TrofelEspecialInfo();
        }

        public TrofelEspecialInfo findTrofelGrandPrixByTypeidAndId(uint _typeid, uint _id)
        {
            return new TrofelEspecialInfo();
        }

        public WarehouseItemEx findWarehouseItemById(uint _id)
        {
            return mp_wi.findWarehouseItemById(_id);
        }

        public WarehouseItemEx findWarehouseItemByTypeid(uint _typeid)
        {
            return mp_wi.findWarehouseItemByTypeid(_typeid);
        }

        public WarehouseItemEx findWarehouseItemByTypeidAndId(uint _typeid, uint _id)
        {
            return mp_wi.findWarehouseItemByTypeidAndId(_typeid, _id);
        }

        public int getCharacterMaxSlot(CharacterInfo.Stats _stats)
        {
            return 1;
        }

        public int getClubSetMaxSlot(CharacterInfo.Stats _stats)
        {
            return 1;
        }

        public int getSizeCupGrandZodiac()
        {
            int size_cup = 1;

            if (grand_zodiac_pontos < 300)
                size_cup = 9;
            else if (grand_zodiac_pontos < 600)
                size_cup = 8;
            else if (grand_zodiac_pontos < 1200)
                size_cup = 7;
            else if (grand_zodiac_pontos < 1800)
                size_cup = 6;
            else if (grand_zodiac_pontos < 4000)
                size_cup = 5;
            else if (grand_zodiac_pontos < 5200)
                size_cup = 4;
            else if (grand_zodiac_pontos < 7600)
                size_cup = 3;
            else if (grand_zodiac_pontos < 10000)
                size_cup = 2;

            return size_cup;
        }

        public int getSlotPower()
        {
            return 1;
        }

        public int getSumRecordGrandPrix()
        {
            return 1;
        }

        public bool isAuxPartEquiped(uint _typeid)
        {
            return false;
        }

        public bool isFriend(int _uid)
        {
            return false;
        }

        public bool isMasterCourse()
        {
            return false;
        }

        public bool isPartEquiped(uint _typeid, uint _id)
        {
            return false;
        }

        public bool ownerCaddieItem(uint _typeid)
        {
            return false;
        }

        public bool ownerHairStyle(uint _typeid)
        {
            return false;
        }

        public bool ownerItem(uint _typeid, int option = 0)
        {
            return false;
        }

        public bool ownerMailBoxItem(uint _typeid)
        {
            return false;
        }

        public bool ownerSetItem(uint _typeid)
        {
            return false;
        }

        public void updateCookie()
        {
            try
            {

                var cmd_cp = new CmdCookie(uid);    // Waiter

                snmdb::NormalManagerDB.add(0, cmd_cp, null, null);

                if (cmd_cp.getException().getCodeError() != 0)
                    throw cmd_cp.getException();

                cookie = cmd_cp.getCookie();

            }
            catch (exception e)
            {

                _smp::message_pool.push(new message("[PlayerInfo::updateCookie][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                // Relanção por que essa função não tem retorno para verifica, então a exception garante que o código não vai continua
                throw;
            }
        }

        public bool updateGrandPrixClear(uint _typeid, int _position)
        {
            return false;
        }

        public void updateLocationDB()
        {
            try
            {

                m_pl.channel = channel;
                m_pl.lobby = lobby;
                m_pl.room = mi.sala_numero;
                m_pl.place.ulPlace = (byte)place;
             
			 _smp.message_pool.push(new message("[PlayerInfo::updateLocationDB][Log]: " + m_pl.place.ToString(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                //// Sincroniza para não ter valores inseridos errados no banco de dados
                m_pl.requestUpdateOnDB();

                NormalManagerDB.add(5, new CmdUpdatePlayerLocation(uid, m_pl), SQLDBResponse, this);

            }
            catch (exception e)
            {
                _smp::message_pool.push(new message("[PlayerInfo::updateLocationDB][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }

        }

        public void updateMedal(uMedalWin _medal_win)
        {
            if (_medal_win.ucMedal == 0u)
                throw new exception("[PlayerInfo::updateMedal][Error] Player[UID=" + uid
                        + "] tentou atualizar medalhas, mas passou nenhuma medalha para atualizar. Hacker ou Bug.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PLAYER_INFO, 600, 0));

            // Update medal info player
            ui.medal.add(_medal_win);

            // Update Info do player na database
            updateUserInfo();
        }

        public void updateMedal(uint _uid, uMedalWin _medal_win)
        {

            if (_medal_win.ucMedal == 0u)
                throw new exception("[PlayerInfo::updateMedal][Error] Player[UID=" + uid
                        + "] tentou atualizar medalhas, mas passou nenhuma medalha para atualizar. Hacker ou Bug.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PLAYER_INFO, 600, 0));

            // Update medal info player
            ui.medal.add(_medal_win);

            // Update Info do player na database
            updateUserInfo();
        }

        public void updateMoeda()
        {
            // Update Cookie
            updateCookie();

            // Update Pang
            updatePang();
        }

        public void updatePang()
        {
            try
            {

                var cmd_pang = new CmdPang(uid);    // Waiter

                snmdb::NormalManagerDB.add(0, cmd_pang, null, null);

                if (cmd_pang.getException().getCodeError() != 0)
                    throw cmd_pang.getException();

                ui.pang = cmd_pang.getPang();
            }
            catch (exception e)
            {

                _smp::message_pool.push(new message("[PlayerInfo::updatePang][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                // Relanção por que essa função não tem retorno para verifica, então a exception garante que o código não vai continua
                throw;
            }
        }

        public void updateTrofelInfo(int _trofel_typeid, bool _trofel_rank)
        {
        }

        public void updateTrofelInfo(uint _uid, int _trofel_typeid, bool _trofel_rank)
        {
        }

        public void updateUserInfo()
        {
            snmdb::NormalManagerDB.add(3, new CmdUpdateUserInfo(uid, ui), SQLDBResponse, this);

            _smp::message_pool.push(new message("[PlayerInfo::updateUserInfo][Log] Atualizou info do player[UID=" + uid + "]", type_msg.CL_FILE_LOG_AND_CONSOLE));
        }

        public void updateUserInfo(uint _uid, UserInfoEx _ui)
        {
            if (_uid == 0)
                throw new exception("[PlayerInfo::updateUserInfo][Error] _uid is invalid(zero)", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PLAYER_INFO, 300, 0));

            snmdb::NormalManagerDB.add(3, new CmdUpdateUserInfo(_uid, _ui), SQLDBResponse, null);

            _smp::message_pool.push(new message("[PlayerInfo::updateUserInfo][Log] Atualizou info do player[UID=" + _uid + "]", type_msg.CL_FILE_LOG_AND_CONSOLE));

        }

        public Dictionary<stIdentifyKey/*int/*ID*/, UpdateItem> findUpdateItemByTypeidAndType(uint _typeid, UpdateItem.UI_TYPE _type)
        {
            return mp_ui
                .Where(it => it.Value._typeid == _typeid && it.Value.type == _type)
                .ToDictionary(it => it.Key, it => it.Value);
        }

        public Dictionary<stIdentifyKey/*int/*ID*/, UpdateItem> findUpdateItemByTypeidAndId(uint _typeid, uint _id)
        {
            return mp_ui
                .Where(it => it.Value._typeid == _typeid && it.Value.id == _id)
                .ToDictionary(it => it.Key, it => it.Value);
        }



        public void ReloadMemberInfo()
        {
            try
            {
                var cmd_member_info = new CmdMemberInfo(uid);

                NormalManagerDB.add(0, cmd_member_info, null, null);

                if (cmd_member_info.getException().getCodeError() != 0)
                    throw cmd_member_info.getException();
                //get new
                var _mi = cmd_member_info.getInfo();
                ui.level = _mi.level;
                mi.level = _mi.level;
                mi.sexo = _mi.sexo;
                mi.do_tutorial = _mi.do_tutorial;
                mi.school = _mi.school;
                mi.capability = _mi.capability;//ajeitar @@@@
                m_cap = _mi.capability;//ajeitar @@@@
                mi.manner_flag = _mi.manner_flag;
                mi.guild_name = _mi.guild_name;
                mi.guild_pang = _mi.guild_pang;
                mi.guild_point = _mi.guild_point;
                mi.event_1 = _mi.event_1;
                mi.event_2 = _mi.event_2;
                mi.papel_shop = _mi.papel_shop;
                mi.papel_shop_last_update = _mi.papel_shop_last_update;
                mi.flag_block = _mi.flag_block;
                mi.channeling_flag = _mi.channeling_flag;
                this.level = _mi.level;
            }
            catch (exception e)
            {

                _smp::message_pool.push(new message("[PlayerInfo::updateMemberInfo][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                // Relanção por que essa função não tem retorno para verifica, então a exception garante que o código não vai continua
                throw;
            }
        }


        public static void SQLDBResponse(int _msg_id, PangyaAPI.SQL.Pangya_DB _pangya_db, object _arg)
        {
            if (_arg == null)
            {
                _smp.message_pool.push("[PlayerInfo::SQLDBResponse][WARNING] _arg is null na msg_id = " + (_msg_id));
                return;
            }

            try
            {
                var pi = (PlayerInfo)_arg;


                // Por Hora só sai, depois faço outro tipo de tratamento se precisar
                if (_pangya_db.getException().getCodeError() != 0)
                {

                    // Trata alguns tipo aqui, que são necessários
                    switch (_msg_id)
                    {
                        case 1: // Update Pang
                            {
                                // Error at update on DB
                                pi.m_update_pang_db.errorUpdateOnDB();

                                break;
                            }
                        case 2: // Update Cookie
                            {
                                // Error at update on DB
                                pi.m_update_cookie_db.errorUpdateOnDB();

                                break;
                            }
                        case 5: // Update Location Player on DB
                            {
                                // Error at update on DB
                                pi.m_pl.errorUpdateOnDB();

                                break;
                            }
                    }

                    _smp::message_pool.push("[PlayerInfo::SQLDBResponse][Error] " + _pangya_db.getException().getFullMessageError());

                    return;
                }

                switch (_msg_id)
                {
                    case 1: // UPDATE pang
                        {

                            // Success update on DB
                            pi.m_update_pang_db.confirmUpdadeOnDB();
                            break;
                        }
                    case 2: // UPDATE cookie
                        {

                            // Success update on DB
                            pi.m_update_cookie_db.confirmUpdadeOnDB();
                            break;
                        }
                    case 3: // UPDATE USER INFO
                        {
                            break;
                        }
                    case 4: // Update Normal Trofel Info
                        {
                            break;
                        }
                    case 5: // Update Location Player on DB
                        {
                            // Success update on DB
                            pi.m_pl.confirmUpdadeOnDB();

                            var cmd_upl = (CmdUpdatePlayerLocation)(_pangya_db);

                            _smp::message_pool.push(new message("[PlayerInfo::SQLDBResponse][Log] Player[UID=" + (cmd_upl.getUID())
                                    + "] Atualizou sua Localizacao[CHANNEL=" + (cmd_upl.getInfo().channel) + ", LOBBY=" + ((short)cmd_upl.getInfo().lobby)
                                    + ", ROOM=" + ((short)cmd_upl.getInfo().room) + ", PLACE=" + (cmd_upl.getInfo().place.ulPlace) + "]", type_msg.CL_FILE_LOG_AND_CONSOLE));
                            break;
                        }
                    case 6: // Insert Grand Prix Clear
                        {

                            //   var cmd_igpc = (CmdInsertGrandPrixClear)(_pangya_db);

                            break;
                        }
                    case 7: // Update Grand Prix Clear
                        {
                            //  var cmd_ugpc = (CmdUpdateGrandPrixClear)(_pangya_db);

                            break;
                        }
                    case 8: // Update Grand Zodiac Pontos
                        {
                            //   var cmd_gzp = (CmdGrandZodiacPontos)(_pangya_db);

                            break;
                        }
                    case 0:
                    default:
                        break;
                }

            }
            catch (exception e)
            {

                _smp::message_pool.push(new message("[PlayerInfo::SQLDBResponse][Error] QUERY_MSG[ID=" + (_msg_id)
                        + "]" + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
        }

    }
}
