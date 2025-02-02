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
using static GameServer.PangType._Define;
using GameServer.Game;

namespace GameServer.PangType
{
    public partial class PlayerInfo : player_info
    {
        public enum enLEVEL : short
        {
            ROOKIE_F, ROOKIE_E, ROOKIE_D, ROOKIE_C, ROOKIE_B, ROOKIE_A,
            BEGINNER_E, BEGINNER_D, BEGINNER_C, BEGINNER_B, BEGINNER_A,
            JUNIOR_E, JUNIOR_D, JUNIOR_C, JUNIOR_B, JUNIOR_A,
            SENIOR_E, SENIOR_D, SENIOR_C, SENIOR_B, SENIOR_A,
            AMADOR_E, AMADOR_D, AMADOR_C, AMADOR_B, AMADOR_A,
            SEMI_PRO_E, SEMI_PRO_D, SEMI_PRO_C, SEMI_PRO_B, SEMI_PRO_A,
            PRO_E, PRO_D, PRO_C, PRO_B, PRO_A,
            NACIONAL_E, NACIONAL_D, NACIONAL_C, NACIONAL_B, NACIONAL_A,
            WORLD_PRO_E, WORLD_PRO_D, WORLD_PRO_C, WORLD_PRO_B, WORLD_PRO_A,
            MESTRE_E, MESTRE_D, MESTRE_C, MESTRE_B, MESTRE_A,
            TOP_MASTER_V, TOP_MASTER_IV, TOP_MASTER_III, TOP_MASTER_II, TOP_MASTER_I,
            WORLD_MASTER_V, WORLD_MASTER_IV, WORLD_MASTER_III, WORLD_MASTER_II, WORLD_MASTER_I,
            LEGEND_V, LEGEND_IV, LEGEND_III, LEGEND_II, LEGEND_I,
            INFINIT_LEGEND_V, INFINIT_LEGEND_IV, INFINIT_LEGEND_III, INFINIT_LEGEND_II, INFINIT_LEGEND_I
        }

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
            }
            stTitleMapCallback(Action<object> _callback, object _arg)
            {
                call_back = (_callback);
                arg = (_arg);
            }
            uint exec()
            {

                if (call_back != null)
                {
                    call_back.Invoke(arg);
                    return 1;
                }
                else
                    //  _smp::message_pool::getInstance().push(new message("[PlayerInfo::stTitleMapCallBack::exec][Error] call_back is nullptr.", CL_FILE_LOG_AND_CONSOLE));

                    return 0;
            }
            //uint id;
            Action<object> call_back;
            object arg;
        }


        public PlayerInfo()
        {
            clear();
        }

        public void clear()
        {


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
            a_ms_normal = new MapStatistics[MS_NUM_MAPS];
            a_msa_normal = new MapStatistics[MS_NUM_MAPS];
            a_ms_natural = new MapStatistics[MS_NUM_MAPS];
            a_msa_natural = new MapStatistics[MS_NUM_MAPS];
            a_ms_grand_prix = new MapStatistics[MS_NUM_MAPS];
            a_msa_grand_prix = new MapStatistics[MS_NUM_MAPS];
            for (int i = 0; i < MS_NUM_MAPS; i++)
            {
                a_ms_normal.SetValue(new MapStatistics(), i);
                a_msa_normal.SetValue(new MapStatistics(), i);
                a_ms_natural.SetValue(new MapStatistics(), i);
                a_msa_natural.SetValue(new MapStatistics(), i);
                a_ms_grand_prix.SetValue(new MapStatistics(), i);
                a_msa_grand_prix.SetValue(new MapStatistics(), i);
            }
            aa_ms_normal_todas_season = new MapStatistics[9][];

            // Inicializando cada sessão com 20 mapas (ou MS_NUM_MAPS mapas)
            for (int j = 0; j < 9; j++)
            {
                aa_ms_normal_todas_season[j] = new MapStatistics[MS_NUM_MAPS];
                for (int i = 0; i < MS_NUM_MAPS; i++)
                {
                    aa_ms_normal_todas_season[j][i] = new MapStatistics();  // Inicializa cada mapa
                }
            }

            mp_scl = new SortedList<uint, StateCharacterLounge>();

            mp_ce = new CharacterManager();     
            mp_ci = new CaddieManager();      
            mp_mi = new MascotManager();      
            mp_wi = new WarehouseManager();      

            mp_fi = new SortedList<uint, FriendInfo>();   // Friend List

            ari = new AttendanceRewardInfoEx();

            //MgrAchievement mgr_achievement = new             // Manager Achievement
            v_card_info = new CardManager();
            v_cei = new CardEquipManager();
            v_ib = new List<ItemBuffEx>();//fazer o manipulador correto
            mp_ui = new SortedList<stIdentifyKey/*uint/*ID*/, UpdateItem>();
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

        public void SetInfo(player_info info)
        {
            uid = info.uid;
            level = info.level;
            block_flag = info.block_flag;
            nickname = info.nickname;
            pass = info.pass;
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
        public MapStatistics[] a_ms_normal { get; set; }
        public MapStatistics[] a_msa_normal { get; set; }
        public MapStatistics[] a_ms_natural { get; set; }
        public MapStatistics[] a_msa_natural { get; set; }
        public MapStatistics[] a_ms_grand_prix { get; set; }
        public MapStatistics[] a_msa_grand_prix { get; set; }
        public MapStatistics[][] aa_ms_normal_todas_season { get; set; }   // Esse aqui é diferente, explico ele no pacote InitialLogin
        public SortedList<uint, StateCharacterLounge> mp_scl { get; set; }

        public CharacterManager mp_ce { get; set; }      //  
        public CaddieManager mp_ci { get; set; }       
        public MascotManager mp_mi { get; set; }       
        public WarehouseManager mp_wi { get; set; }       

        public SortedList<uint/*UID*/, FriendInfo> mp_fi { get; set; }    // Friend List

        public AttendanceRewardInfoEx ari { get; set; }

        //MgrAchievement mgr_achievement;             // Manager Achievement
        public CardManager v_card_info { get; set; }

        public CardEquipManager v_cei { get; set; }
        public List<ItemBuffEx> v_ib { get; set; }

        public SortedList<stIdentifyKey/*uint/*ID*/, UpdateItem> mp_ui;

        public List<TrofelEspecialInfo> v_tsi_current_season;
        public List<TrofelEspecialInfo> v_tsi_rest_season;
        public List<TrofelEspecialInfo> v_tgp_current_season;   // Trofel Grand Prix
        public List<TrofelEspecialInfo> v_tgp_rest_season; // Trofel Grand Prix
        public List<MyRoomItem> v_mri;      // MyRoomItem

        public List<GrandPrixClear> v_gpc;  // Grand Prix Clear os grand prix que o player já jogou

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
        public byte place { get; set; }            // Lugar que o player está no momento
        public byte lobby { get; set; } = DEFAULT_CHANNEL;            // Lobby
        public byte channel { get; set; } = DEFAULT_CHANNEL;          // Channel
        public byte whisper { get; set; } = 1; // Whisper 0 e 1, 0 OFF, 1 ON
        public uint state;
        public uint state_lounge;
        public bool m_state_logged;       // State logged que usa no login gs, e que eu possa usar aqui, por que tbm tenho que prevenir contra ataques DDoS
        public uCapability m_cap => mi?.capability;
        public ulong grand_zodiac_pontos;

        public ulong m_legacy_tiki_pts; // Point Shop(Tiki Shop antigo)

        //// Mail Box
        public PlayerMailBox m_mail_box;

        //	stPlayerLocationDB m_pl;
        //stSyncUpdateDB m_update_pang_db;
        //stSyncUpdateDB m_update_cookie_db;


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

                //  snmdb::NormalManagerDB.add(2, new CmdUpdateCookie(uid, _cookie, CmdUpdateCookie::INCREASE), SQLDBResponse, this);


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
            catch (exception e) {
                                       

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

                //snmdb::NormalManagerDB.add(1, new CmdUpdatePang(uid, _pang, CmdUpdatePang::INCREASE), PlayerInfo::SQLDBResponse, this);


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

        public bool checkEquipedItem(int _typeid)
        {
            return false;
        }

        public PlayerRoomInfo.uItemBoost checkEquipedItemBoost()
        {
            return null;
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

        public CaddieInfoEx findCaddieById(int _id)
        {
            return null;
        }

        public CaddieInfoEx findCaddieByTypeid(int _typeid)
        {
            return null;
        }

        public CaddieInfoEx findCaddieByTypeidAndId(int _typeid, int _id)
        {
            return null;
        }

        public CardInfo findCardById(int _id)
        {
            return null;
        }

        public CardInfo findCardByTypeid(int _typeid)
        {
            return null;
        }

        public CardEquipInfoEx findCardEquipedById(int _id, int _char_typeid, int _slot)
        {
            return null;
        }

        public CardEquipInfoEx findCardEquipedByTypeid(int _typeid, int _char_typeid = 0, int _slot = 0, int _tipo = 0, int _efeito = 0)
        {
            return null;
        }

        public CharacterInfo findCharacterById(int _id)
        {
            return null;
        }

        public CharacterInfo findCharacterByTypeid(int _typeid)
        {
            return null;
        }

        public CharacterInfo findCharacterByTypeidAndId(int _typeid, int _id)
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

        public GrandPrixClear findGrandPrixClear(int _typeid)
        {
            return null;
        }

        public MascotInfoEx findMascotById(int _id)
        {
            return null;
        }

        public MascotInfoEx findMascotByTypeid(int _typeid)
        {
            return null;
        }

        public MascotInfoEx findMascotByTypeidAndId(int _typeid, int _id)
        {
            return null;
        }

        public MyRoomItem findMyRoomItemById(int _id)
        {
            return null;
        }

        public MyRoomItem findMyRoomItemByTypeid(int _typeid)
        {
            return new MyRoomItem();
        }

        public TrofelEspecialInfo findTrofelEspecialById(int _id)
        {
            return new TrofelEspecialInfo();
        }

        public TrofelEspecialInfo findTrofelEspecialByTypeid(int _typeid)
        {
            return new TrofelEspecialInfo();
        }

        public TrofelEspecialInfo findTrofelEspecialByTypeidAndId(int _typeid, int _id)
        {
            return new TrofelEspecialInfo();
        }

        public TrofelEspecialInfo findTrofelGrandPrixById(int _id)
        {
            return new TrofelEspecialInfo();
        }

        public TrofelEspecialInfo findTrofelGrandPrixByTypeid(int _typeid)
        {
            return new TrofelEspecialInfo();
        }

        public TrofelEspecialInfo findTrofelGrandPrixByTypeidAndId(int _typeid, int _id)
        {
            return new TrofelEspecialInfo();
        }

        public WarehouseItemEx findWarehouseItemById(uint _id)
        {
            return mp_wi.findWarehouseItemById(_id);
        }

        public WarehouseItemEx findWarehouseItemByTypeid(int _typeid)
        {
            return new WarehouseItemEx();
        }

        public WarehouseItemEx findWarehouseItemByTypeidAndId(int _typeid, int _id)
        {
            return new WarehouseItemEx();
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

        public bool isAuxPartEquiped(int _typeid)
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

        public bool isPartEquiped(int _typeid, int _id)
        {
            return false;
        }

        public bool ownerCaddieItem(int _typeid)
        {
            return false;
        }

        public bool ownerHairStyle(int _typeid)
        {
            return false;
        }

        public bool ownerItem(int _typeid, int option = 0)
        {
            return false;
        }

        public bool ownerMailBoxItem(int _typeid)
        {
            return false;
        }

        public bool ownerSetItem(int _typeid)
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

        public bool updateGrandPrixClear(int _typeid, int _position)
        {
            return false;
        }

        public void updateLocationDB()
        {
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

        public WarehouseItemEx findWarehouseItemByTypeid(uint _typeid)
        {
            return mp_wi.findWarehouseItemByTypeid(_typeid);
        }

        public WarehouseItemEx findWarehouseItemByTypeidAndId(uint _typeid, uint _id)
        {                                                          
            return mp_wi.findWarehouseItemByTypeidAndId(_typeid, _id);
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


                //// Por Hora só sai, depois faço outro tipo de tratamento se precisar
                //if (_pangya_db.getException().getCodeError() != 0)
                //{

                //    // Trata alguns tipo aqui, que são necessários
                //    switch (_msg_id)
                //    {
                //        case 1: // Update Pang
                //            {
                //                // Error at update on DB
                //                pi.m_update_pang_db.errorUpdateOnDB();

                //                break;
                //            }
                //        case 2: // Update Cookie
                //            {
                //                // Error at update on DB
                //                pi.m_update_cookie_db.errorUpdateOnDB();

                //                break;
                //            }
                //        case 5: // Update Location Player on DB
                //            {
                //                // Error at update on DB
                //                pi.m_pl.errorUpdateOnDB();

                //                break;
                //            }
                //    }

                //    _smp::message_pool.push("[PlayerInfo::SQLDBResponse][Error] " + _pangya_db.getException().getFullMessageError());

                //    return;
                //}

                //switch (_msg_id)
                //{
                //    case 1: // UPDATE pang
                //        {

                //            // Success update on DB
                //            pi.m_update_pang_db.confirmUpdadeOnDB();

                //            // Não tem retorno então não precisa reinterpretar o pangya_db
                //            //var cmd_up = ( CmdUpdatePang)(_pangya_db);
                //            break;
                //        }
                //    case 2: // UPDATE cookie
                //        {

                //            // Success update on DB
                //            pi.m_update_cookie_db.confirmUpdadeOnDB();

                //            // Não tem retorno então não precisa reinterpretar o pangya_db
                //            //var cmd_uc = ( CmdUpdateCookie)(_pangya_db);
                //            break;
                //        }
                //    case 3: // UPDATE USER INFO
                //        {
                //            // Não tem retorno então não precisa reinterpretar o pangya_db
                //            // var cmd_uui = ( CmdUpdateUserInfo)(_pangya_db);
                //            break;
                //        }
                //    case 4: // Update Normal Trofel Info
                //        {
                //            break;
                //        }
                //    case 5: // Update Location Player on DB
                //        {
                //            // Success update on DB
                //            pi.m_pl.confirmUpdadeOnDB();

                //            break;
                //        }
                //    case 6: // Insert Grand Prix Clear
                //        {

                //            var cmd_igpc = (CmdInsertGrandPrixClear)(_pangya_db);

                //            break;
                //        }
                //    case 7: // Update Grand Prix Clear
                //        {
                //            var cmd_ugpc = (CmdUpdateGrandPrixClear)(_pangya_db);

                //            break;
                //        }
                //    case 8: // Update Grand Zodiac Pontos
                //        {
                //            var cmd_gzp = (CmdGrandZodiacPontos)(_pangya_db);

                //            break;
                //        }
                //    case 0:
                //    default:
                //        break;
                //}

            }
            catch (Exception)
            {

                throw;
            }
        }
    }

}
