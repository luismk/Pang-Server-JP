using GameServer.Game.Manager;
using PangyaAPI.Network.Pangya_St;
using PangyaAPI.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using static GameServer.PangType._Define;
namespace GameServer.PangType
{
    public abstract class PlayerInfoBase : player_info
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

              
        public PlayerInfoBase()
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
            MapStatistics[][] aa_ms_normal_todas_season = new MapStatistics[9][];

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

            mp_ce = new SortedList<uint, CharacterInfoEx>();     // Tem que usar multimap aqui, para nao ficar realocando memória, uso o ponteiro de um element, para o item equipado
            mp_ci = new SortedList<uint, CaddieInfoEx>();      // Tem que usar multimap aqui, para nao ficar realocando memória, uso o ponteiro de um element, para o item equipado
            mp_mi = new SortedList<uint, MascotInfoEx>();      // Tem que usar multimap aqui, para nao ficar realocando memória, uso o ponteiro de um element, para o item equipado
            mp_wi = new multimap<uint, WarehouseItemEx>();      // Tem que usar multimap aqui, para nao ficar realocando memória, uso o ponteiro de um element, para o item equipado

            mp_fi = new SortedList<uint, FriendInfo>();   // Friend List

            ari = new AttendanceRewardInfoEx();

            //MgrAchievement mgr_achievement = new             // Manager Achievement
            v_card_info = new List<CardInfo>();

            v_cei = new List<CardEquipInfoEx>();
            v_ib = new List<ItemBuffEx>();

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

        // get Size Cup Grand Zodiac from Grand Zodiac Pontos
        public abstract int getSizeCupGrandZodiac();

        // Finder

        // Friend
        public abstract FriendInfo findFriendInfoByUID(int _uid);
        public abstract FriendInfo findFriendInfoById(string _id);
        public abstract FriendInfo findFriendInfoByNickname(string _nickname);

        // Itens Equipaveis
        public abstract WarehouseItemEx findWarehouseItemById(int _id);
        public abstract WarehouseItemEx findWarehouseItemByTypeid(int _typeid);
        public abstract WarehouseItemEx findWarehouseItemByTypeidAndId(int _typeid, int _id); // Precisa desse para caso tenho um item com o mesmo id, mas com typeid diferente

        public abstract CharacterInfo findCharacterById(int _id);
        public abstract CharacterInfo findCharacterByTypeid(int _typeid);
        public abstract CharacterInfo findCharacterByTypeidAndId(int _typeid, int _id);   // Precisa desse para caso tenho um item com o mesmo id, mas com typeid diferente

        public abstract CaddieInfoEx findCaddieById(int _id);
        public abstract CaddieInfoEx findCaddieByTypeid(int _typeid);
        public abstract CaddieInfoEx findCaddieByTypeidAndId(int _typeid, int _id);   // Precisa desse para caso tenho um item com o mesmo id, mas com typeid diferente

        public abstract MascotInfoEx findMascotById(int _id);
        public abstract MascotInfoEx findMascotByTypeid(int _typeid);
        public abstract MascotInfoEx findMascotByTypeidAndId(int _typeid, int _id);   // Precisa desse para caso tenho um item com o mesmo id, mas com typeid diferente

        public abstract MyRoomItem findMyRoomItemById(int _id);    // Furniture
        public abstract MyRoomItem findMyRoomItemByTypeid(int _typeid);

        public abstract CardInfo findCardById(int _id);
        public abstract CardInfo findCardByTypeid(int _typeid);

        public abstract CardEquipInfoEx findCardEquipedById(int _id, int _char_typeid, int _slot);
        public abstract CardEquipInfoEx findCardEquipedByTypeid(int _typeid, int _char_typeid = 0, int _slot = 0, int _tipo = 0, int _efeito = 0);

        // Troféu especial
        public abstract TrofelEspecialInfo findTrofelEspecialById(int _id);
        public abstract TrofelEspecialInfo findTrofelEspecialByTypeid(int _typeid);
        public abstract TrofelEspecialInfo findTrofelEspecialByTypeidAndId(int _typeid, int _id);

        public abstract TrofelEspecialInfo findTrofelGrandPrixById(int _id);
        public abstract TrofelEspecialInfo findTrofelGrandPrixByTypeid(int _typeid);
        public abstract TrofelEspecialInfo findTrofelGrandPrixByTypeidAndId(int _typeid, int _id);

        public abstract GrandPrixClear findGrandPrixClear(int _typeid);

        //// Get Power Extra
        //ExtraPower getExtraPower(bool _pwr_condition_actived);

        // Get Slot Power
        public abstract int getSlotPower();

        public abstract int getCharacterMaxSlot(CharacterInfo.Stats _stats);
        public abstract int getClubSetMaxSlot(CharacterInfo.Stats _stats);

        // --- Checkers

        // Verifica se ele tem Item Boost equipado (agora só pang mastery e nitro) exp acho que não precisa agora
        public abstract PlayerRoomInfo.uItemBoost checkEquipedItemBoost();

        // Verifica se fez record em todos course, que pode fazer record
        // Cria um map com todos os maps que foram feito record, no normal, grand prix e natural, excluido o record assit
        public abstract bool isMasterCourse();

        // Verifica se está com o item equipado
        public abstract bool checkEquipedItem(int _typeid);

        // Soma dos score dos record do natural(mas o JP pega o do grand prix)
        public abstract int getSumRecordGrandPrix();

        // verifica se é um amigo
        public abstract bool isFriend(int _uid);

        // Tem o item(Possuí o item)
        public abstract bool ownerCaddieItem(int _typeid);
        public abstract bool ownerHairStyle(int _typeid);
        public abstract bool ownerSetItem(int _typeid);
        public abstract bool ownerItem(int _typeid, int option = 0);   // Verifica todos os itens;
        public abstract bool ownerMailBoxItem(int _typeid);

        public abstract bool isPartEquiped(int _typeid, int _id);
        public abstract bool isAuxPartEquiped(int _typeid);

        // Consome moedas
        public abstract void consomeMoeda(ulong _pang, ulong _cookie);
        public abstract void consomeCookie(ulong _cookie);
        public abstract void consomePang(ulong _pang);

        // Adiciona moedas
        public abstract void addMoeda(ulong _pang, ulong _cookie);
        public abstract void addCookie(ulong _cookie);
        public abstract void addPang(ulong _pang);

        // Atualiza os valores do gs com o que está no banco de dados
        public abstract void updateMoeda();
        public abstract void updateCookie();
        public abstract void updatePang();

        // Adiciona Pang
        public abstract void addPang(uint _uid, ulong _pang);

        // Adiciona Cookie Point(CP)
        public abstract void addCookie(uint _uid, ulong _cookie);

        // Add (Soma) User Info
        public abstract void addUserInfo(UserInfoEx _ui, ulong _total_pang_win_game = 0);

        // Update User Info ON DB
        public abstract void updateUserInfo();

        // Update User Info ON DB Estático
        public abstract void updateUserInfo(uint _uid, UserInfoEx _ui);

        // Update Trofel Info
        public abstract void updateTrofelInfo(int _trofel_typeid, bool _trofel_rank);

        // Update Trofel Info Estático
        public abstract void updateTrofelInfo(uint _uid, int _trofel_typeid, bool _trofel_rank);

        // Update Medal
        public abstract void updateMedal(uMedalWin _medal_win);

        // Update Medal Estático
        public abstract void updateMedal(uint _uid, uMedalWin _medal_win);

        // Adiciona Exp
        public abstract int addExp(uint _exp);

        // update location player on DB
        public abstract void updateLocationDB();

        // Update Grand Prix Clear
        public abstract bool updateGrandPrixClear(int _typeid, int _position);

        // Update Grand Zodiac Pontos
        public abstract void addGrandZodiacPontos(ulong _pontos);

        public abstract bool checkAlterationCookieOnDB();
        public abstract bool checkAlterationPangOnDB();

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
        public MapStatistics[][] aa_ms_normal_todas_season { get; set; }   // Esse aqui é diferente, explico ele no pacote principal
        public SortedList<uint, StateCharacterLounge> mp_scl { get; set; }

        public SortedList<uint/*ID*/, CharacterInfoEx> mp_ce;      // Tem que usar multimap aqui, para nao ficar realocando memória, uso o ponteiro de um element, para o item equipado
        public SortedList<uint/*ID*/, CaddieInfoEx> mp_ci;       // Tem que usar multimap aqui, para nao ficar realocando memória, uso o ponteiro de um element, para o item equipado
        public SortedList<uint/*ID*/, MascotInfoEx> mp_mi;       // Tem que usar multimap aqui, para nao ficar realocando memória, uso o ponteiro de um element, para o item equipado
        public multimap<uint/*ID*/, WarehouseItemEx> mp_wi;        // Tem que usar multimap aqui, para nao ficar realocando memória, uso o ponteiro de um element, para o item equipado

        public SortedList<uint/*UID*/, FriendInfo> mp_fi;    // Friend List

        public AttendanceRewardInfoEx ari;

        //MgrAchievement mgr_achievement;             // Manager Achievement
        public List<CardInfo> v_card_info;

        public List<CardEquipInfoEx> v_cei;
        public List<ItemBuffEx> v_ib;

        public SortedList<stIdentifyKey/*uint/*ID*/, UpdateItem> mp_ui;

        public List<TrofelEspecialInfo> v_tsi_current_season;
        public List<TrofelEspecialInfo> v_tsi_rest_season;
        public List<TrofelEspecialInfo> v_tgp_current_season;   // Trofel Grand Prix
        public List<TrofelEspecialInfo> v_tgp_rest_season; // Trofel Grand Prix
        public List<MyRoomItem> v_mri;      // MyRoomItem

        public List<GrandPrixClear> v_gpc;  // Grand Prix Clear os grand prix que o player já jogou

        public MyRoomConfig mrc;
        public DolfiniLocker df;   // DolfiniLocker
        public GuildInfoEx gi;
        public DailyQuestInfoUser dqiu;
        public Last5PlayersGame l5pg;
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
        public byte lobby { get; set; } = byte.MaxValue;            // Lobby
        public byte channel { get; set; } = byte.MaxValue;          // Channel
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
    }
}