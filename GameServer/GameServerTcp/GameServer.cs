using System;
using _smp = PangyaAPI.Utilities.Log;
using PangyaAPI.Utilities;
using PangyaAPI.Network.PangyaPacket;
using GameServer.Session;
using System.Collections.Generic;
using GameServer.GameType;
using GameServer.Game.System;
using GameServer.Game;
using GameServer.PacketFunc;
using PangyaAPI.Utilities.BinaryModels;
using static GameServer.GameType._Define;
using PangyaAPI.Utilities.Log;
using System.Linq;
using GameServer.Cmd;
using PangyaAPI.Network.Pangya_St;
using PangyaAPI.SQL.Manager;
using PangyaAPI.Network.PangyaSession;
using PangyaAPI.Network.PangyaServer;
using PangyaAPI.SQL;
using PangyaAPI.Network.Cmd;
using GameServer.PangyaEnums;
using packet_func = GameServer.PacketFunc.packet_func_cl;
using GameServer.Game.Utils;

namespace GameServer.GameServerTcp
{
    public partial class GameServer : Server
    {
        public int m_access_flag { get; private set; }
        public int m_create_user_flag { get; private set; }
        public int m_same_id_login_flag { get; private set; }
        DailyQuestInfo m_dqi;
        protected List<Channel> v_channel;
        public BroadcastList m_ticker;
        public BroadcastList m_notice;
        public GameServer() : base(new player_manager(2000))
        {
            v_channel = new List<Channel>();
            m_player_manager = new player_manager(2000);
            ConfigInit();
            init_Packets();
            init_load_channels();
            StartingServer();
            init_systems();
        }


        public override void ConfigInit()
        {
            base.ConfigInit();

            // Server Tipo
            m_si.tipo = 1;

            m_si.img_no = m_reader_ini.ReadInt16("SERVERINFO", "ICONINDEX");
            m_si.rate.exp = (short)m_reader_ini.readInt("SERVERINFO", "EXPRATE");
            m_si.rate.scratchy = (short)m_reader_ini.readInt("SERVERINFO", "SCRATCHY_RATE");
            m_si.rate.pang = (short)m_reader_ini.readInt("SERVERINFO", "PANGRATE");
            m_si.rate.club_mastery = (short)m_reader_ini.readInt("SERVERINFO", "CLUBMASTERYRATE");
            m_si.rate.papel_shop_rare_item = (short)m_reader_ini.readInt("SERVERINFO", "PAPEL_rate_RATE"); ;
            m_si.rate.papel_shop_cookie_item = (short)m_reader_ini.readInt("SERVERINFO", "PAPEL_COOKIE_ITEM_RATE"); ;
            m_si.rate.treasure = (short)m_reader_ini.readInt("SERVERINFO", "TREASURE_RATE"); ;
            m_si.rate.memorial_shop = (short)m_reader_ini.readInt("SERVERINFO", "MEMORIAL_RATE");
            m_si.rate.chuva = (short)m_reader_ini.readInt("SERVERINFO", "CHUVA_RATE");
            m_si.rate.grand_zodiac_event_time = (short)(m_reader_ini.readInt("SERVERINFO", "GZ_EVENT") >= 1 ? 1 : 0);// Ativo por padrão
            m_si.rate.grand_prix_event = (short)(m_reader_ini.readInt("SERVERINFO", "GP_EVENT") >= 1 ? 1 : 0);// Ativo por padrão
            m_si.rate.golden_time_event = ((short)(m_reader_ini.readInt("SERVERINFO", "GOLDEN_TIME_EVENT") >= 1 ? 1 : 0));// Ativo por padrão
            m_si.rate.login_reward_event = ((short)(m_reader_ini.readInt("SERVERINFO", "LOGIN_REWARD") >= 1 ? 1 : 0));// Ativo por padrão
            m_si.rate.bot_gm_event = ((short)(m_reader_ini.readInt("SERVERINFO", "BOT_GM_EVENT") >= 1 ? 1 : 0));// Ativo por padrão
            m_si.rate.smart_calculator = (/*m_reader_ini.readInt("SERVERINFO", "SMART_CALC") >= 1 ? true :*/ 0);// Atibo por padrão
            m_si.rate.angel_event = ((short)(m_reader_ini.readInt("SERVERINFO", "ANGEL_EVENT") >= 1 ? 1 : 0));// Atibo por padrão
            try
            {

                m_si.flag.ullFlag = m_reader_ini.ReadUInt64("SERVERINFO", "FLAG");

            }
            catch (exception e)
            {

                _smp.message_pool.push(("[GameServer.config_init][ErrorSystem] Config.FLAG" + e.getFullMessageError()));
            }


            // Recupera Valores de rate do gs do banco de dados
            var cmd_rci = new CmdRateConfigInfo(m_si.uid);  // Waiter

            if (cmd_rci.getException().getCodeError() != 0 || cmd_rci.isError()/*Deu erro na consulta não tinha o rate config info para esse gs, pode ser novo*/)
            {

                if (cmd_rci.getException().getCodeError() != 0)
                    _smp.message_pool.push(("[GameServer.config_init][ErrorSystem] " + cmd_rci.getException().getFullMessageError()));


                setAngelEvent(m_si.rate.angel_event);
                setratePang(m_si.rate.pang);
                setrateExp(m_si.rate.exp);
                setrateClubMastery(m_si.rate.club_mastery);
            }
            else
            {   // Conseguiu recuperar com sucesso os valores do gs

                setAngelEvent(m_si.rate.angel_event);
                setratePang(m_si.rate.pang);
                setrateExp(m_si.rate.exp);
                setrateClubMastery(m_si.rate.club_mastery);
            }
            m_si.app_rate = 100;    // Esse aqui nunca usei, deixei por que no DB do s4 tinha só cópiei
        }
        public bool getAccessFlag()
        {
            return m_access_flag == 1;
        }

        public bool getCreateUserFlag()
        {
            return m_create_user_flag == 1;
        }

        public bool canSameIDLogin()
        {
            return m_same_id_login_flag == 1;
        }
        private void setAngelEvent(short _angel_event)
        {// Evento para reduzir o quit rate, diminui 1 quit a cada jogo concluído
            m_si.event_flag.angel_wing = _angel_event > 0;

            // Update Event Angel
            m_si.rate.angel_event = _angel_event;
        }
        private void setratePang(short _pang)
        {
            // Update Flag Event
            m_si.event_flag.pang_x_plus = (_pang >= 200) ? true : false;

            // Update rate Pang
            m_si.rate.pang = _pang;
        }
        private void setrateExp(short _exp)
        {// Reseta flag antes de atualizar ela 
            m_si.event_flag.exp_x2 = m_si.event_flag.exp_x_plus = false;

            // Update Flag Event
            if (_exp > 200)
                m_si.event_flag.exp_x_plus = true;
            else if (_exp == 200)
                m_si.event_flag.exp_x2 = true;
            else
                m_si.event_flag.exp_x2 = m_si.event_flag.exp_x_plus = false;

            // Update rate Experiência
            m_si.rate.exp = _exp;
        }
        private void setrateClubMastery(short _club_mastery)
        {
            // Update Flag Event
            m_si.event_flag.club_mastery_x_plus = (_club_mastery >= 200) ? true : false;

            // Update rate Club Mastery
            m_si.rate.club_mastery = _club_mastery;
        }

        public override void OnHeartBeat()
        {
            try
            {
                // Server ainda não está totalmente iniciado
                if (!this._isRunning)
                    return;

                // Check Invite Time Channels
                foreach (var el in v_channel)
                    el.checkInviteTime();

                // Begin Check System Singleton Static
                // Carrega IFF_STRUCT
                if (!sIff.getInstance().isLoad())
                    sIff.getInstance().load();

                // Carrega Card System
                //if (!sCardSystem.getInstance().isLoad())
                //    sCardSystem.getInstance().load();

                //// Carrega Comet Refill System
                //if (!sCometRefillSystem.getInstance().isLoad())
                //    sCometRefillSystem.getInstance().load();

                // Carrega Papel Shop System
                if (!sPapelShopSystem.getInstance().isLoad())
                    sPapelShopSystem.getInstance().load();

                //// Carrega Box System
                //if (!sBoxSystem.getInstance().isLoad())
                //    sBoxSystem.getInstance().load();

                //// Carrega Memorial System
                //if (!sMemorialSystem.getInstance().isLoad())
                //    sMemorialSystem.getInstance().load();

                //// Carrega Cube Coin System
                //if (!sCubeCoinSystem.getInstance().isLoad())
                //    sCubeCoinSystem.getInstance().load();

                //// Treasure Hunter System
                //if (!sTreasureHunterSystem.getInstance().isLoad())
                //    sTreasureHunterSystem.getInstance().load();

                //// Drop System
                //if (!sDropSystem.getInstance().isLoad())
                //    sDropSystem.getInstance().load();

                // Attendance Reward System
                if (!sAttendanceRewardSystem.getInstance().isLoad())
                    sAttendanceRewardSystem.getInstance().load();

                //// Map Dados Estáticos
                //if (!sMap.getInstance().isLoad())
                //    sMap.getInstance().load();

                //// Approach Mission
                //if (!sApproachMissionSystem.getInstance().isLoad())
                //    sApproachMissionSystem.getInstance().load();

                //// Grand Zodiac Event
                //if (!sGrandZodiacEvent.getInstance().isLoad())
                //    sGrandZodiacEvent.getInstance().load();

                //// Coin Cube Location System
                //if (!sCoinCubeLocationUpdateSystem.getInstance().isLoad())
                //    sCoinCubeLocationUpdateSystem.getInstance().load();

                //// Golden Time System
                //if (!sGoldenTimeSystem.getInstance().isLoad())
                //    sGoldenTimeSystem.getInstance().load();

                //// Login Reward System
                //if (!sLoginRewardSystem.getInstance().isLoad())
                //    sLoginRewardSystem.getInstance().load();

                //// Carrega Smart Calculator Lib, Só inicializa se ele estiver ativado
                //if (m_si.rate.smart_calculator && !sSmartCalculator.getInstance().hasStopped() && !sSmartCalculator.getInstance().isLoad())
                //    sSmartCalculator.getInstance().load();

                //// End Check System Singleton Static

                //// check Grand Zodiac Event Time
                //if (m_si.rate.grand_zodiac_event_time && sGrandZodiacEvent.getInstance().checkTimeToMakeRoom())
                //    makeGrandZodiacEventRoom();

                //// check Bot GM Event Time
                //if (m_si.rate.bot_gm_event && sBotGMEvent.getInstance().checkTimeToMakeRoom())
                //    makeBotGMEventRoom();

                //// check Golden Time Round Update
                //if (m_si.rate.golden_time_event && sGoldenTimeSystem.getInstance().checkRound())
                //    makeListOfPlayersToGoldenTime();

                //// update Login Reward
                //if (m_si.rate.login_reward_event)
                //    sLoginRewardSystem.getInstance().updateLoginReward();

                //// Check Daily Quest
                //if (MgrDailyQuest.checkCurrentQuest(m_dqi))
                //    MgrDailyQuest.updateDailyQuest(m_dqi);  // Atualiza daily quest

                //// Check Update Dia do Papel Shop System
                //sPapelShopSystem.getInstance().updateDia();

                //if (sTreasureHunterSystem.getInstance().checkUpdateTimePointCourse())
                //{

                //    packet p;

                //    packet_func.pacote131(p);

                //    foreach (var el in v_channel)
                //        packet_func.channel_broadcast(el, p, 1);
                //}
                //// End Check Treasure Hunter

                //// Check Notice (GM or Cube Win Rare)
                //BroadcastList.RetNoticeCtx rt;

                //rt = m_notice.peek();

                //if (rt.ret == BroadcastList.RET_TYPE.OK)
                //{

                //    packet p;

                //    if (rt.nc.type == BroadcastList.TYPE.GM_NOTICE)
                //    {    // GM Notice

                //        p.init_plain((unsigned short)0x42);

                //        p.addString(rt.nc.notice);

                //    }
                //    else if (rt.nc.type == BroadcastList.TYPE.CUBE_WIN_RARE)
                //    {   // Cube Win Rare Notice

                //        p.init_plain((unsigned short)0x1D3);

                //        p.addUint32(1);             // Count

                //        //for (auto i = 0u; i < 2u; ++i) {
                //        p.addUint32(rt.nc.option);
                //        p.addString(rt.nc.notice);
                //        //}

                //    }

                //    // Broadcast to All Channels
                //    foreach (var el in v_channel)
                //        packet_func.channel_broadcast(el, p, 1);
                //}

                //// Check Ticker
                //rt = m_ticker.peek();

                //if (rt.ret == BroadcastList.RET_TYPE.OK && rt.nc.type == BroadcastList.TYPE.TICKER)
                //{   // Ticker Msg

                //    packet p((unsigned short)0xC9);

                //    p.addString(rt.nc.nickname);
                //    p.addString(rt.nc.notice);

                //    // Broadcast to All Channels
                //    foreach (var el in v_channel)
                //        packet_func.channel_broadcast(el, p, 1);
                //}

            }
            catch (exception e)
            {
                _smp.message_pool.push("[GameServer.onHeartBeat][ErrorSystem] " + e.getFullMessageError(), _smp.type_msg.CL_FILE_LOG_AND_CONSOLE);
            }
        }

        public override void SQLDBResponse(int _msg_id, Pangya_DB _pangya_db, object _arg)
        {
            if (_pangya_db is CmdServerList cmdServerList)
            {
                base.SQLDBResponse(_msg_id, cmdServerList, _arg);
                return;
            }


            if (_arg == null)
            {
                _smp.message_pool.push("[GameServer.SQLDBResponse][Error] _arg is null na msg_id = " + (_msg_id));
                return;
            }

            // Por Hora só sai, depois faço outro tipo de tratamento se precisar
            if (_pangya_db.getException().getCodeError() != 0)
                throw new exception("[GameServer.SQLDBResponse][Error] " + _pangya_db.getException().getFullMessageError());

            switch (_msg_id)
            {

                default:
                    {
                        break;
                    }
            }      
        }


        public virtual void destroyRoom(byte _channel_owner, short _number)
        {

        }

        public virtual void sendServerListAndChannelListToSession(Player _session)
        {
            _session.Send(packet_func_sv.pacote09F(m_server_list, v_channel));
        }

        public virtual void sendDateTimeToSession(Player _session)
        {
            using (var p = new PangyaBinaryWriter((ushort)0xBA))
            {
                p.WriteTime();
                _session.Send(p);
            }
        }

        public virtual void sendRankServer(Player _session)
        {

            try
            {

                //if (_session.m_pi.block_flag.m_flag.rank_server)
                //    throw new exception("[GameServer.sendRankServer][Error] Player[UID=" + (_session.m_pi.m_uid)
                //            + "] esta bloqueado o Rank Server, ele nao pode acessar o rank server.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.GAME_SERVER, 7010, 0));

                var cmd_sl = new CmdServerList(TYPE_SERVER.RANK);   // Waiter

                NormalManagerDB.add(0, cmd_sl, null, null);

                if (cmd_sl.getException().getCodeError() != 0)
                    throw cmd_sl.getException();

                var sl = cmd_sl.getServerList();

                if (sl.Count == 0)
                    throw new exception("[GameServer.sendRankServer][WARNING] Player[UID=" + (_session.m_pi.uid)
                            + "] requisitou o Rank Server, mas nao tem nenhum Rank Server online no DB.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.GAME_SERVER, 7011, 0));

                using (var p = new PangyaBinaryWriter(0xA2))
                {
                    p.WritePStr(sl[0].ip);
                    p.WriteInt32(sl[0].port);
                    _session.Send(p);
                }


            }
            catch (exception e)
            {

                _smp::message_pool.push(new message("[GameServer.sendRankServer][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                using (var p = new PangyaBinaryWriter(0xA2))
                {
                    // Erro manda tudo 0
                    p.WriteUInt16(0u);  // String IP
                    p.WriteUInt32(0u);  // Port
                    _session.Send(p);
                }
            }
        }

        public virtual Channel findChannel(byte _channel)
        {
            if (_channel == 255)
                return null;

            for (var i = 0; i < v_channel.Count; ++i)
                if (v_channel[i].getId() == _channel)
                    return v_channel[i];

            return null;
        }

        public virtual Player findPlayer(uint _uid, bool _oid = false)
        {
            return (Player)(_oid ? FindSessionByOid(_uid) : FindSessionByUid(_uid));
        }           

        public virtual void blockOID(uint _oid) { m_player_manager.blockOID(_oid); }
        public virtual void unblockOID(uint _oid) { m_player_manager.unblockOID(_oid); }

        DailyQuestInfo getDailyQuestInfo() { return m_dqi; }

        // Set Event Server
        public virtual void setAngelEvent(uint _angel_event) { }

        // Update Daily Quest Info
        public virtual void updateDailyQuest(DailyQuestInfo _dqi) { }

        // send Update Room Info, find room nos canais e atualiza o info
        //	public virtual void sendUpdateRoomInfo(room _r, int _option) { }


        public player_manager m_player_manager;

        public virtual bool checkCommand(string[] _command) { return true; }
        public virtual void reload_files() { }
        public virtual void init_systems() 
        {
            // SINCRONAR por que se não alguem pode pegar lixo de memória se ele ainda nao estiver inicializado
            var cmd_dqi = new Cmd.CmdDailyQuestInfo();

            NormalManagerDB.add(1, cmd_dqi, SQLDBResponse, this);
                                           
            if (cmd_dqi.getException().getCodeError() != 0)
                throw new exception("[game_server::game_server][Error] nao conseguiu pegar o Daily Quest Info[Exption: "
                    + cmd_dqi.getException().getFullMessageError() + "]", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.GAME_SERVER, 277, 0));

            // Initialize Daily Quest of Server
            m_dqi = cmd_dqi.getInfo();


            // Para previnir de da exception no destructor do versus::treasure system
            var lixo = sRandomGen.getInstance().IsGood();

            // Carrega IFF_STRUCT
            if (!sIff.getInstance().isLoad())
                sIff.getInstance().load();

            //// Carrega Card System
            //if (!sCardSystem.getInstance().isLoad())
            //    sCardSystem.getInstance().load();

            //// Carrega Comet Refill System
            //if (!sCometRefillSystem.getInstance().isLoad())
            //    sCometRefillSystem.getInstance().load();

            // Carrega Papel Shop System
            if (!sPapelShopSystem.getInstance().isLoad())
                sPapelShopSystem.getInstance().load();

            //// Carrega Box System
            //if (!sBoxSystem.getInstance().isLoad())
            //    sBoxSystem.getInstance().load();

            //// Carrega Memorial System
            //if (!sMemorialSystem.getInstance().isLoad())
            //    sMemorialSystem.getInstance().load();

            //// Carrega Cube Coin System
            //if (!sCubeCoinSystem.getInstance().isLoad())
            //    sCubeCoinSystem.getInstance().load();

            //// Carrega Treasure Hunter System
            //if (!sTreasureHunterSystem.getInstance().isLoad())
            //    sTreasureHunterSystem.getInstance().load();

            //// Carrega Drop System
            //if (!sDropSystem.getInstance().isLoad())
            //    sDropSystem.getInstance().load();

            // Carrega Attendance Reward System
            if (!sAttendanceRewardSystem.getInstance().isLoad())
                sAttendanceRewardSystem.getInstance().load();

            //// Carrega Map Dados Estáticos
            //if (!sMap.getInstance().isLoad())
            //    sMap.getInstance().load();

            //// Carrega Approach Mission System
            //if (!sApproachMissionSystem.getInstance().isLoad())
            //    sApproachMissionSystem.getInstance().load();

            //// Carrega Grand Zodiac Event System
            //if (!sGrandZodiacEvent.getInstance().isLoad())
            //    sGrandZodiacEvent.getInstance().load();

            //// Carrega Coin Cube Location Update Syatem
            //if (!sCoinCubeLocationUpdateSystem.getInstance().isLoad())
            //    sCoinCubeLocationUpdateSystem.getInstance().load();

            //// Carrega Golden Time System
            //if (!sGoldenTimeSystem.getInstance().isLoad())
            //    sGoldenTimeSystem.getInstance().load();

            //// Carrega Login Reward System
            //if (!sLoginRewardSystem.getInstance().isLoad())
            //    sLoginRewardSystem.getInstance().load();

            //// Carrega Bot GM Event
            //if (!sBotGMEvent.getInstance().isLoad())
            //    sBotGMEvent.getInstance().load();

            //// Coloca aqui para ele não dá erro na hora de destruir o Room Grand Prix static instance
            //RoomGrandPrix::initFirstInstance();

            //// Coloca aqui para ele não dá erro na hora de destruir o Room Grand Zodiac Event static instance
            //RoomGrandZodiacEvent::initFirstInstance();

            //// Coloca aqui para ele não dá erro na hora de destruir o Room Bot GM Event static instance
            //RoomBotGMEvent::initFirstInstance();


        }
        public virtual void init_Packets()
        {
            this.funcs.addPacketCall(0x02, packet_func.packet002);
            this.funcs.addPacketCall(0x03, packet_func.packet003);
            this.funcs.addPacketCall(0x04, packet_func.packet004);
            this.funcs.addPacketCall(0x06, packet_func.packet006);
            this.funcs.addPacketCall(0x07, packet_func.packet007);
            this.funcs.addPacketCall(0x08, packet_func.packet008);
            this.funcs.addPacketCall(0x09, packet_func.packet009);
            this.funcs.addPacketCall(0x0A, packet_func.packet00A);
            this.funcs.addPacketCall(0x0B, packet_func.packet00B);
            this.funcs.addPacketCall(0x0C, packet_func.packet00C);
            this.funcs.addPacketCall(0x0D, packet_func.packet00D);
            this.funcs.addPacketCall(0x0E, packet_func.packet00E);
            this.funcs.addPacketCall(0x0F, packet_func.packet00F);
            this.funcs.addPacketCall(0x10, packet_func.packet010);
            this.funcs.addPacketCall(0x11, packet_func.packet011);
            this.funcs.addPacketCall(0x12, packet_func.packet012);
            this.funcs.addPacketCall(0x13, packet_func.packet013);
            this.funcs.addPacketCall(0x14, packet_func.packet014);
            this.funcs.addPacketCall(0x15, packet_func.packet015);
            this.funcs.addPacketCall(0x16, packet_func.packet016);
            this.funcs.addPacketCall(0x17, packet_func.packet017);
            this.funcs.addPacketCall(0x18, packet_func.packet018);
            this.funcs.addPacketCall(0x19, packet_func.packet019);
            this.funcs.addPacketCall(0x1A, packet_func.packet01A);
            this.funcs.addPacketCall(0x1B, packet_func.packet01B);
            this.funcs.addPacketCall(0x1C, packet_func.packet01C);
            this.funcs.addPacketCall(0x1D, packet_func.packet01D);
            this.funcs.addPacketCall(0x1F, packet_func.packet01F);
            this.funcs.addPacketCall(0x20, packet_func.packet020);
            this.funcs.addPacketCall(0x22, packet_func.packet022);
            this.funcs.addPacketCall(0x26, packet_func.packet026);
            this.funcs.addPacketCall(0x29, packet_func.packet029);
            this.funcs.addPacketCall(0x2A, packet_func.packet02A);
            this.funcs.addPacketCall(0x2D, packet_func.packet02D);
            this.funcs.addPacketCall(0x2F, packet_func.packet02F);
            this.funcs.addPacketCall(0x30, packet_func.packet030);
            this.funcs.addPacketCall(0x31, packet_func.packet031);
            this.funcs.addPacketCall(0x32, packet_func.packet032);
            this.funcs.addPacketCall(0x33, packet_func.packet033);
            this.funcs.addPacketCall(0x34, packet_func.packet034);
            this.funcs.addPacketCall(0x35, packet_func.packet035);
            this.funcs.addPacketCall(0x36, packet_func.packet036);
            this.funcs.addPacketCall(0x37, packet_func.packet037);
            this.funcs.addPacketCall(0x39, packet_func.packet039);
            this.funcs.addPacketCall(0x3A, packet_func.packet03A);
            this.funcs.addPacketCall(0x3C, packet_func.packet03C);
            this.funcs.addPacketCall(0x3D, packet_func.packet03D);
            this.funcs.addPacketCall(0x3E, packet_func.packet03E);
            this.funcs.addPacketCall(0x41, packet_func.packet041);
            this.funcs.addPacketCall(0x42, packet_func.packet042);
            this.funcs.addPacketCall(0x43, packet_func.packet043);
            this.funcs.addPacketCall(0x47, packet_func.packet047);
            this.funcs.addPacketCall(0x48, packet_func.packet048);
            this.funcs.addPacketCall(0x4A, packet_func.packet04A);
            this.funcs.addPacketCall(0x4B, packet_func.packet04B);
            this.funcs.addPacketCall(0x4F, packet_func.packet04F);
            this.funcs.addPacketCall(0x54, packet_func.packet054);
            this.funcs.addPacketCall(0x55, packet_func.packet055);
            this.funcs.addPacketCall(0x57, packet_func.packet057);
            this.funcs.addPacketCall(0x5C, packet_func.packet05C);
            this.funcs.addPacketCall(0x60, packet_func.packet060);
            this.funcs.addPacketCall(0x61, packet_func.packet061);
            this.funcs.addPacketCall(0x63, packet_func.packet063);
            this.funcs.addPacketCall(0x64, packet_func.packet064);
            this.funcs.addPacketCall(0x65, packet_func.packet065);
            this.funcs.addPacketCall(0x66, packet_func.packet066);
            this.funcs.addPacketCall(0x67, packet_func.packet067);
            this.funcs.addPacketCall(0x69, packet_func.packet069);
            this.funcs.addPacketCall(0x6B, packet_func.packet06B);
            this.funcs.addPacketCall(0x73, packet_func.packet073);
            this.funcs.addPacketCall(0x74, packet_func.packet074);
            this.funcs.addPacketCall(0x75, packet_func.packet075);
            this.funcs.addPacketCall(0x76, packet_func.packet076);
            this.funcs.addPacketCall(0x77, packet_func.packet077);
            this.funcs.addPacketCall(0x78, packet_func.packet078);
            this.funcs.addPacketCall(0x79, packet_func.packet079);
            this.funcs.addPacketCall(0x7A, packet_func.packet07A);
            this.funcs.addPacketCall(0x7B, packet_func.packet07B);
            this.funcs.addPacketCall(0x7C, packet_func.packet07C);
            this.funcs.addPacketCall(0x7D, packet_func.packet07D);
            this.funcs.addPacketCall(0x81, packet_func.packet081);
            this.funcs.addPacketCall(0x82, packet_func.packet082);
            this.funcs.addPacketCall(0x83, packet_func.packet083);
            this.funcs.addPacketCall(0x88, packet_func.packet088);
            this.funcs.addPacketCall(0x8B, packet_func.packet08B);
            this.funcs.addPacketCall(0x8F, packet_func.packet08F);
            this.funcs.addPacketCall(0x98, packet_func.packet098);
            this.funcs.addPacketCall(0x9C, packet_func.packet09C);
            this.funcs.addPacketCall(0x9D, packet_func.packet09D);
            this.funcs.addPacketCall(0x9E, packet_func.packet09E);
            this.funcs.addPacketCall(0xA1, packet_func.packet0A1);
            this.funcs.addPacketCall(0xA2, packet_func.packet0A2);
            this.funcs.addPacketCall(0xAA, packet_func.packet0AA);
            this.funcs.addPacketCall(0xAB, packet_func.packet0AB);
            this.funcs.addPacketCall(0xAE, packet_func.packet0AE);
            this.funcs.addPacketCall(0xB2, packet_func.packet0B2);
            // Recebi esse pacote quando troquei de server, e no outro eu tinha jogado um Match feito bastante Achievement
            // e pegado daily quest, desistido do resto e aceito a do dia e aberto alguns card packs, ai troquei de server e recebi esse pacote
            //2018-11-17 20:43:07.307 Tipo : 180(0xB4), desconhecido ou nao implementado.func_arr.getPacketCall()     Error Code : 335609856
            //2018-11-17 20:43:07.307 size packet : 5
            //0000 B4 00 01 00 00 -- -- -- -- -- -- -- -- -- -- --    ................
            this.funcs.addPacketCall(0xB4, packet_func.packet0B4);
            this.funcs.addPacketCall(0xB5, packet_func.packet0B5);
            this.funcs.addPacketCall(0xB7, packet_func.packet0B7);
            this.funcs.addPacketCall(0xB9, packet_func.packet0B9);
            this.funcs.addPacketCall(0xBA, packet_func.packet0BA);
            this.funcs.addPacketCall(0xBD, packet_func.packet0BD);
            this.funcs.addPacketCall(0xC1, packet_func.packet0C1);
            this.funcs.addPacketCall(0xC9, packet_func.packet0C9);
            this.funcs.addPacketCall(0xCA, packet_func.packet0CA);
            this.funcs.addPacketCall(0xCB, packet_func.packet0CB);
            this.funcs.addPacketCall(0xCC, packet_func.packet0CC);
            this.funcs.addPacketCall(0xCD, packet_func.packet0CD);
            this.funcs.addPacketCall(0xCE, packet_func.packet0CE);
            this.funcs.addPacketCall(0xCF, packet_func.packet0CF);
            this.funcs.addPacketCall(0xD0, packet_func.packet0D0);
            this.funcs.addPacketCall(0xD1, packet_func.packet0D1);
            this.funcs.addPacketCall(0xD2, packet_func.packet0D2);
            this.funcs.addPacketCall(0xD3, packet_func.packet0D3);
            this.funcs.addPacketCall(0xD4, packet_func.packet0D4);
            this.funcs.addPacketCall(0xD5, packet_func.packet0D5);
            this.funcs.addPacketCall(0xD8, packet_func.packet0D8);
            this.funcs.addPacketCall(0xDE, packet_func.packet0DE);
            this.funcs.addPacketCall(0xE5, packet_func.packet0E5);
            this.funcs.addPacketCall(0xE6, packet_func.packet0E6);
            this.funcs.addPacketCall(0xE7, packet_func.packet0E7);
            this.funcs.addPacketCall(0xEB, packet_func.packet0EB);
            this.funcs.addPacketCall(0xEC, packet_func.packet0EC);
            this.funcs.addPacketCall(0xEF, packet_func.packet0EF);
            this.funcs.addPacketCall(0xF4, packet_func.packet0F4);
            this.funcs.addPacketCall(0xFB, packet_func.packet0FB);
            this.funcs.addPacketCall(0xFE, packet_func.packet0FE);
            this.funcs.addPacketCall(0x119, packet_func.packet119);
            this.funcs.addPacketCall(0x126, packet_func.packet126);
            this.funcs.addPacketCall(0x127, packet_func.packet127);
            this.funcs.addPacketCall(0x128, packet_func.packet128);
            this.funcs.addPacketCall(0x129, packet_func.packet129);
            this.funcs.addPacketCall(0x12C, packet_func.packet12C);
            this.funcs.addPacketCall(0x12D, packet_func.packet12D);
            this.funcs.addPacketCall(0x12E, packet_func.packet12E);
            this.funcs.addPacketCall(0x12F, packet_func.packet12F);
            this.funcs.addPacketCall(0x130, packet_func.packet130);
            this.funcs.addPacketCall(0x131, packet_func.packet131);
            this.funcs.addPacketCall(0x137, packet_func.packet137);
            this.funcs.addPacketCall(0x138, packet_func.packet138);
            this.funcs.addPacketCall(0x140, packet_func.packet140);
            this.funcs.addPacketCall(0x141, packet_func.packet141);
            this.funcs.addPacketCall(0x143, packet_func.packet143);
            this.funcs.addPacketCall(0x144, packet_func.packet144);
            this.funcs.addPacketCall(0x145, packet_func.packet145);
            this.funcs.addPacketCall(0x146, packet_func.packet146);
            this.funcs.addPacketCall(0x147, packet_func.packet147);
            this.funcs.addPacketCall(0x14B, packet_func.packet14B);
            this.funcs.addPacketCall(0x151, packet_func.packet151);
            this.funcs.addPacketCall(0x152, packet_func.packet152);
            this.funcs.addPacketCall(0x153, packet_func.packet153);
            this.funcs.addPacketCall(0x154, packet_func.packet154);
            this.funcs.addPacketCall(0x155, packet_func.packet155);
            this.funcs.addPacketCall(0x156, packet_func.packet156);
            this.funcs.addPacketCall(0x157, packet_func.packet157);
            this.funcs.addPacketCall(0x158, packet_func.packet158);
            this.funcs.addPacketCall(0x15C, packet_func.packet15C);
            this.funcs.addPacketCall(0x15D, packet_func.packet15D);
            this.funcs.addPacketCall(0x164, packet_func.packet164);
            this.funcs.addPacketCall(0x165, packet_func.packet165);
            this.funcs.addPacketCall(0x166, packet_func.packet166);
            this.funcs.addPacketCall(0x167, packet_func.packet167);
            this.funcs.addPacketCall(0x168, packet_func.packet168);
            this.funcs.addPacketCall(0x169, packet_func.packet169);
            this.funcs.addPacketCall(0x16B, packet_func.packet16B);
            this.funcs.addPacketCall(0x16C, packet_func.packet16C);
            this.funcs.addPacketCall(0x16D, packet_func.packet16D);
            this.funcs.addPacketCall(0x16E, packet_func.packet16E);
            this.funcs.addPacketCall(0x16F, packet_func.packet16F);
            this.funcs.addPacketCall(0x171, packet_func.packet171);
            this.funcs.addPacketCall(0x172, packet_func.packet172);
            this.funcs.addPacketCall(0x176, packet_func.packet176);
            this.funcs.addPacketCall(0x177, packet_func.packet177);
            this.funcs.addPacketCall(0x179, packet_func.packet179);
            this.funcs.addPacketCall(0x17A, packet_func.packet17A);
            this.funcs.addPacketCall(0x17F, packet_func.packet17F);
            this.funcs.addPacketCall(0x180, packet_func.packet180);
            this.funcs.addPacketCall(0x181, packet_func.packet181);
            this.funcs.addPacketCall(0x184, packet_func.packet184);
            this.funcs.addPacketCall(0x185, packet_func.packet185);
            this.funcs.addPacketCall(0x187, packet_func.packet187);
            this.funcs.addPacketCall(0x188, packet_func.packet188);
            this.funcs.addPacketCall(0x189, packet_func.packet189);
            this.funcs.addPacketCall(0x18A, packet_func.packet18A);
            this.funcs.addPacketCall(0x18B, packet_func.packet18B);
            this.funcs.addPacketCall(0x18C, packet_func.packet18C);
            this.funcs.addPacketCall(0x18D, packet_func.packet18D);
            this.funcs.addPacketCall(0x192, packet_func.packet192);
            this.funcs.addPacketCall(0x196, packet_func.packet196);
            this.funcs.addPacketCall(0x197, packet_func.packet197);
            this.funcs.addPacketCall(0x198, packet_func.packet198);
            this.funcs.addPacketCall(0x199, packet_func.packet199);
        }
        public virtual void init_load_channels()
        {
            try
            {
                int num_channel = m_reader_ini.readInt("CHANNELINFO", "NUM_CHANNEL");

                for (byte i = 0; i < num_channel; ++i)
                {
                    ChannelInfo ci = new ChannelInfo();
                    try
                    {
                        ci.id = i;
                        ci.name = m_reader_ini.ReadString("CHANNEL" + (i + 1), "NAME");
                        ci.max_user = m_reader_ini.ReadInt16("CHANNEL" + (i + 1), "MAXUSER");
                        ci.min_level_allow = m_reader_ini.ReadUInt32("CHANNEL" + (i + 1), "LOWLEVEL");
                        ci.max_level_allow = m_reader_ini.ReadUInt32("CHANNEL" + (i + 1), "MAXLEVEL");
                        ci.flag.ulFlag = m_reader_ini.ReadUInt32("CHANNEL" + (i + 1), "FLAG");
                    }
                    catch (Exception e)
                    {
                        _smp.message_pool.push("[GameServer.init_load_channels][ErrorSystem] " + e.Message);
                    }

                    v_channel.Add(new Channel(ci, m_si.propriedade.ulProperty));
                }
            }
            catch (Exception e)
            {
                _smp.message_pool.push("[GameServer.init_load_channels][ErrorSystem] " + e.Message);
            }

        }
        public virtual void reload_systems() { }
        public virtual void reloadGlobalSystem(uint _tipo) { }

        // Update rate e Event of Server
        public virtual void updaterateAndEvent(uint _tipo, uint _qntd) { }

        // Shutdown With Time


        // Check Player Itens

        public virtual void check_player() { }

        // Make Grand Zodiac Event Room
        public virtual void makeGrandZodiacEventRoom() { }

        // Make List of Players to Golden Time Event
        public virtual void makeListOfPlayersToGoldenTime() { }

        // Make Bot GM Event Room
        public virtual void makeBotGMEventRoom() { }

        protected override void onAcceptCompleted(SessionBase _session)
        {
            try
            {
                var Response = new PangyaBinaryWriter();
                //Gera Packet com chave de criptografia (posisão 8)
                Response.Write(new byte[] { 0x00, 0x06, 0x00, 0x00, 0x3f, 0x00 });
                Response.WriteByte(1);  // OPTION 1
                Response.WriteByte(1);	// OPTION 2
                Response.WriteByte(_session.m_key);//key
                _session.SafeSend(Response.GetBytes);
            }
            catch (Exception ex)
            {
                _smp.message_pool.push(new message(
              $"[GameServer.onAcceptCompleted][ErrorSt] {ex.Message}\nStack Trace: {ex.StackTrace}",
              type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
        }

        public override bool CheckPacket(SessionBase _session, Packet packet)
        {
            var player = (Player)_session;
            var packetId = (PacketIDClient)packet.Id;

            // Verifica se o valor de packetId é válido no enum PacketIDClient
            if (Enum.IsDefined(typeof(PacketIDClient), packetId))
            {
                if (packetId != PacketIDClient.PLAYER_KEEPLIVE) 
                WriteConsole.WriteLine($"[GameServer.CheckPacket][Log]: PLAYER[UID: {player.m_pi.uid}, CGPID: {packetId}]", ConsoleColor.Cyan);
                return true;
            }
            else// nao tem no PacketIDClient
            {
                WriteConsole.WriteLine($"[GameServer.CheckPacket][Log]: PLAYER[UID: {player.m_pi.uid}, CGPID: 0x{packet.Id:X}]");
                return true;
            }
        }


        public override void onDisconnected(SessionBase _session)
        {
            if (_session == null)
                throw new exception("[GameServer.onDisconnected][Error] _session is null");

            var _player = (Player)_session;

            _smp.message_pool.push(new message("[GameServer.onDisconnected][Log] Player Desconectou. ID: " + _player.m_pi.id + "  UID: " + _player.m_pi.uid));

            /// Novo
            var _channel = findChannel(_player.m_pi.channel);

            try
            {

                if (_channel != null)
                    _channel.leaveChannel(_player);

            }
            catch (exception e)
            {
                _smp.message_pool.push(new message("[GameServer.onDisconnect][Error] " + e.getFullMessageError()));
            }
        }

        //chama alguma coisa aqui!
        public override void OnStart()
        {
            Console.Title = $"Game Server - P: {m_si.curr_user}";
        }

        public Channel enterChannel(Player _session, byte _channel)
        {
            Channel enter = null, last = null;
            var p = new PangyaBinaryWriter();
            try
            {

                if ((enter = findChannel(_channel)) == null)
                    throw new Exception("[GameServer::enterChannel][Error] id channel nao exite.");

                if (enter.getId() == _session.m_pi.channel)
                {
                    _session.Send(packet_func_sv.pacote04E(1));
                    return enter;   // Ele já está nesse canal
                }

                if (enter.isFull())
                {
                    // Não conseguiu entrar no canal por que ele está cheio, deixa o enter como null
                    enter = null;
                    _session.Send(packet_func_sv.pacote04E(2));
                }
                else
                {
                    // Verifica se pode entrar no canal
                    enter.checkEnterChannel(_session);

                    // Sai do canal antigo se ele estiver em outro canal
                    if (_session.m_pi.channel != DEFAULT_CHANNEL && (last = findChannel(_session.m_pi.channel)) != null)
                        last.leaveChannel(_session);

                    // Entra no canal
                    enter.enterChannel(_session);
                }
            }
            catch (exception e)
            {
                _smp::message_pool.push(new message("[GameServer.enterChannel][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
                _session.Send(packet_func_sv.pacote04E(-1));
            }

            return enter;
        }

        public void requestChangeChatMacroUser(Player _session, Packet _packet)
        {
            try
            {

                // Verifica se session está autorizada para executar esse ação, 
                // se ele não fez o login com o Server ele não pode fazer nada até que ele faça o login

                chat_macro_user cmu;

                cmu = (chat_macro_user)_packet.Read(new chat_macro_user());

                // UPDATE ON GAME

                // Se vazio substitiu por um macro padrão
                for (var i = 0u; i < 9; ++i)
                    if (string.IsNullOrEmpty(cmu.macro[i]))
                        cmu.macro[i] = "PangYa! Por favor configure seu chat macro";

                _session.m_pi.cmu = cmu;

                // UPDATE ON DB
                NormalManagerDB.add(3, new Cmd.CmdUpdateChatMacroUser(_session.m_pi.uid, _session.m_pi.cmu), SQLDBResponse, this);

            }
            catch (exception e)
            {
                _smp::message_pool.push(new message("[GameServer.requestChangeChatMacroUser][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
        }

        public void requestChangeServer(Player _session, Packet _packet)
        {

            try
            {

                var server_uid = _packet.ReadUInt32();

                var it = m_server_list.FirstOrDefault(c => c.uid == server_uid);

                if (it == null)
                    throw new exception("[GameServer.requestChangeServer][Error] Player[UID=" + (_session.m_pi.uid)
                            + "] tentou trocar de server para o Server[UID=" + (server_uid)
                            + "], mas ele nao esta no server list mais.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.GAME_SERVER, 0x7500001, 1));

                if (_session.m_pi.lobby != 0 && _session.m_pi.lobby == 176u/*Grand Prix*/
                    && !it.propriedade.grand_prix/*Não é Grand Prix o Server*/)
                    throw new exception("[GameServer.requestChangeServer][Error] Player[UID=" + (_session.m_pi.uid)
                            + "] tentou trocar de server para o Server[UID=" + (server_uid)
                            + "], mas o player esta na lobby grand prix e o server que ele quer entrar nao e' grand prix.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.GAME_SERVER, 0x7500002, 2));

                var cmd_akg = new CmdAuthKeyGame(_session.m_pi.uid, server_uid);    // waitable

                NormalManagerDB.add(0, cmd_akg, null, null);

                if (cmd_akg.getException().getCodeError() != 0)
                    throw cmd_akg.getException();

                var auth_key_game = cmd_akg.getAuthKey();

                var cmd_uakl = new CmdUpdateAuthKeyLogin(_session.m_pi.uid, 1); // waitable

                NormalManagerDB.add(0, cmd_uakl, null, null);

                if (cmd_uakl.getException().getCodeError() != 0)
                    throw cmd_uakl.getException();

                _session.Send(packet_func_sv.pacote1D4(auth_key_game));

            }
            catch (exception e)
            {

                _smp::message_pool.push(new message("[requestChangeServer][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                // Envia server lista novamente para o player ele foi proibido de entrar no server ou não conseguiu por algum motivo ou erro
                sendServerListAndChannelListToSession(_session);
            }
        }

        public void requestChangeWhisperState(Player _session, Packet _packet)
        {
            try
            {

                var whisper = _packet.ReadByte();

                // Verifica se session está autorizada para executar esse ação, 
                // se ele não fez o login com o Server ele não pode fazer nada até que ele faça o login
                //CHECK_SESSION_IS_AUTHORIZED("ChangeWisperState");

                if (whisper > 1)
                    throw new exception("[GameServer.requestChangeWhisperState][Error] player[UID=" + (_session.m_pi.uid) + "] tentou alterar o estado do Whisper[state="
                            + ((ushort)whisper) + "], mas ele mandou um valor invalido. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.GAME_SERVER, 1, 0x5300101));

                _session.m_pi.mi.state_flag.whisper = (_session.m_pi.whisper = whisper) == 1 ? true : false;

                // Log
                _smp::message_pool.push(new message("[Whisper::ChangeState][Log] player[UID=" + (_session.m_pi.uid) + "] trocou o Whisper State para : " + (whisper.IsTrue() ? ("ON") : ("OFF")), type_msg.CL_FILE_LOG_AND_CONSOLE));


            }
            catch (exception e)
            {
                _smp::message_pool.push(new message("[GameServer.requestChangeWhisperState][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
        }

        public void requestChat(Player _session, Packet _packet)
        {
            try
            {

                string nickname = "", msg = "";

                nickname = _packet.ReadPStr();
                msg = _packet.ReadPStr();

                // Verifica a mensagem com palavras proibida e manda para o log e bloquea o chat dele
                _smp::message_pool.push(new message("[GameServer.requestChat][Log]: PLAYER[UID: " + _session.m_pi.uid + ", MSG: " + msg + "]", type_msg.CL_FILE_LOG_AND_CONSOLE));

                var c = findChannel(_session.m_pi.channel);

                if (c != null)
                {

                    // LOG GM
                    // Envia para todo os GM do server essa message
                    var gmList = FindAllGM();

                    if (gmList.Any())
                    {
                        string msg_gm = "\\5" + _session.m_pi.nickname + ": '" + msg + "'";
                        string from = "\\1[Channel=" + c.getInfo().name + ", \\1ROOM=" + _session.m_pi.mi.sala_numero + "]";

                        int index = from.IndexOf(' ');
                        if (index != -1)
                            from = from.Substring(0, index) + " \\1" + from.Substring(index + 1);

                        // Rotina normal de notificação para GM no chat global
                        foreach (Player el in gmList)
                        {
                            if (((el.m_gi.channel && el.m_pi.channel == c.getInfo().id) || el.m_gi.whisper.IsTrue() || el.m_gi.isOpenPlayerWhisper(_session.m_pi.uid))
                                && /* Check SAME Channel and Room*/(el.m_pi.channel != _session.m_pi.channel || el.m_pi.mi.sala_numero != _session.m_pi.mi.sala_numero))
                            {
                                // Responde no chat do player     
                                el.Send(packet_func_sv.pacote040(from, msg_gm, 0));
                            }
                        }
                    }
                }

                // Normal Message
                if (_session.m_pi.mi.sala_numero != ushort.MaxValue)
                    c.requestSendMsgChatRoom(_session, msg);
                else
                {
                    //is low :/
                    _session.SendLobby_broadcast(packet_func_sv.pacote040(_session.m_pi.nickname, msg, (byte)(_session.m_pi.m_cap.game_master ? 128 : 0)));

                    var p = new PangyaBinaryWriter(new byte[] { 0x4d, 0x02 });
                    p.WriteUInt32(0); //sucess 0
                    _session.Send(p, true);
                }

            }
            catch (exception e)
            {
                Console.WriteLine(e.getFullMessageError());
            }
        }

        public void requestCheckGameGuardAuthAnswer(Player _session, Packet _packet)
        {
        }

        public void requestCommandNoticeGM(Player _session, Packet _packet)
        {
            try
            {

                if (!(_session.m_pi.m_cap.game_master/* & 4*/))
                    throw new exception("[GameServer.requestCommandNoticeGM][Error] player[UID=" + (_session.m_pi.uid)
                            + "] nao eh GM mas tentou executar comando GM. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.GAME_SERVER, 1, 0x5700100));

                string notice = _packet.ReadString();

                if (notice.empty())
                    throw new exception("[GameServer.requestCommandNoticeGM][Error] player[UID=" + (_session.m_pi.uid)
                            + "] tentou executar o comando de notice, mas a notice is empty. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.GAME_SERVER, 8, 0x5700100));

                // Log
                _smp::message_pool.push(new message("[GameServer.requestCommandNoticeGM][Log] player[UID=" + (_session.m_pi.uid) + "] enviou notice[NOTICE="
                        + notice + "] para todos do game server.", type_msg.CL_FILE_LOG_AND_CONSOLE));

                using (var p = new PangyaBinaryWriter(0x40))
                {
                    p.WriteByte(7); // Notice

                    p.WritePStr(_session.m_pi.nickname);
                    p.WritePStr(notice);

                    _session.SendChannel_broadcast(p.GetBytes);
                }
            }
            catch (exception e)
            {

                _smp::message_pool.push(new message("[GameServer.requestCommandNoticeGM][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
                using (var p = new PangyaBinaryWriter(0x40))
                {
                    p.WriteByte(7); // Notice

                    p.WritePStr(_session.m_pi.nickname);
                    p.WritePStr("Nao conseguiu executar o comando.");
                    _session.Send(p);

                }
            }

        }

        public void requestCommonCmdGM(Player _session, Packet _packet)
        {
            try
            {
                _session.requestCommonCmdGM(_packet);
            }
            catch (exception e)
            {
                Console.WriteLine(e.getFullMessageError());
            }
        }

        public void requestEnterChannel(Player _session, Packet _packet)
        {
            try
            {
                _packet.ReadByte(out byte channel);
                // Enter Channel
                enterChannel(_session, channel);
            }
            catch (exception e)
            {
                Console.WriteLine(e.getFullMessageError());
            }
        }

        public void requestEnterOtherChannelAndLobby(Player _session, Packet _packet)
        {
            try
            {

                // Lobby anterior que o player estava
                var lobby = _session.m_pi.lobby;

                var c = enterChannel(_session, _packet.ReadByte());

                if (c != null)
                    c.enterLobby(_session, lobby);

            }
            catch (exception e)
            {

                _smp::message_pool.push(new message("[GameServer.requestEnterOtherChannelAndLobby][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

            }
        }

        public void requestExceptionClientMessage(Player _session, Packet _packet)
        {
            byte tipo = _packet.ReadByte();

            var exception_msg = _packet.ReadPStr();

            _smp::message_pool.push(new message("[GameServer.requestExceptionClientMessage][Log] PLAYER[UID=" + (_session.m_pi.uid) + ", EXTIPO="
                    + ((ushort)tipo) + ", MSG=" + exception_msg + "]", type_msg.CL_ONLY_FILE_LOG));
            _session.Disconnect();
        }

        public void requestLogin(Player _session, Packet _packet)
        {
            new LoginSystem().requestLogin(_session, _packet);
        }

        public void requestNotifyNotDisplayPrivateMessageNow(Player _session, Packet _packet)
        {
            try
            {
                string nickname = _packet.ReadPStr();

                if (nickname.empty())
                    throw new exception("[GameServer.requestNotifyNotDisplayPrivateMessageNow][Error] Player[UID=" + (_session.m_pi.uid)
                            + "] nao pode ver mensagem agora, mas o nickname de quem enviou a mensagem para ele eh invalido(empty). Hacker ou Bug.",
                            ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.GAME_SERVER, 0x750050, 0));
                // Procura o player pelo nickname, para ver se ele está online
                var s = (Player)Program.gs.FindSessionByNickname(nickname);
                if (s != null && s.getConnected())
                {
                    // Log
                    _smp::message_pool.push(new message("[GameServer.requestNotifyNotDisplayPrivateMessageNow][Log] Player[UID=" + (_session.m_pi.uid)
                            + "] recebeu mensagem do Player[UID=" + (s.m_pi.uid) + ", NICKNAME=" + nickname + "], mas ele nao pode ver a mensagem agora.", type_msg.CL_FILE_LOG_AND_CONSOLE));

                    s.Send(packet_func_sv.pacote040(nickname, "", 4));

                }
            }
            catch (exception e)
            {
                _smp::message_pool.push(new message("[GameServer.requestNotifyNotDisplayPrivateMessageNow][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
        }

        public void requestPlayerInfo(Player _session, Packet _packet)
        {
            try
            {
                uint uid = _packet.ReadUInt32();
                byte season = _packet.ReadByte();
                _smp.message_pool.push(new message($"[GameServer.requestPlayerInfo][Log] PLAYER[UID: {_session.m_pi.uid}, R_UID: {uid}, SEASON: {(int)season}]", type_msg.CL_ONLY_CONSOLE));

                Player s = null;
                PlayerInfo pi = null;
                CharacterInfo ci = new CharacterInfo();

                if (uid == _session.m_pi.uid)
                {

                    pi = _session.m_pi;

                }
                else if ((s = findPlayer(uid)) != null)
                {
                    pi = s.m_pi;
                }
                else
                {

                    var cmd_mi = new CmdMemberInfo(uid);

                    NormalManagerDB.add(0, cmd_mi, null, null);

                    if (cmd_mi.getException().getCodeError() != 0)
                        throw cmd_mi.getException();

                    MemberInfoEx mi = cmd_mi.getInfo();

                    // Verifica se não é o mesmo UID, pessoas diferentes
                    // Quem quer ver a info não é GM aí verifica se o player é GM
                    if (uid != _session.m_pi.uid && !mi.capability.game_master/* & 4/*(GM)*/)
                    {

                        _session.Send(packet_func_sv.pacote089(uid, season, 3));

                    }
                    else
                    {

                        List<MapStatisticsEx> v_ms_n, v_msa_n, v_ms_na, v_msa_na, v_ms_g, v_msa_g;

                        var cmd_ci = new CmdCharacterInfo(uid, CmdCharacterInfo.TYPE.ONE, -1);

                        NormalManagerDB.add(0, cmd_ci, null, null);

                        if (cmd_ci.getException().getCodeError() != 0)
                            throw cmd_ci.getException();

                        ci = cmd_ci.getInfo();

                        var cmd_ue = new CmdUserEquip(uid);

                        NormalManagerDB.add(0, cmd_ue, null, null);

                        if (cmd_ue.getException().getCodeError() != 0)
                            throw cmd_ue.getException();

                        UserEquip ue = cmd_ue.getInfo();

                        var cmd_ui = new CmdUserInfo(uid);

                        NormalManagerDB.add(0, cmd_ui, null, null);

                        if (cmd_ui.getException().getCodeError() != 0)
                            throw cmd_ui.getException();

                        UserInfoEx ui = cmd_ui.getInfo();

                        var cmd_gi = new CmdGuildInfo(uid, 0);

                        NormalManagerDB.add(0, cmd_gi, null, null);

                        if (cmd_gi.getException().getCodeError() != 0)
                            throw cmd_gi.getException();

                        var gi = cmd_gi.getInfo();

                        var cmd_ms = new CmdMapStatistics(uid, (CmdMapStatistics.TYPE_SEASON)(season), CmdMapStatistics.TYPE.NORMAL, CmdMapStatistics.TYPE_MODO.M_NORMAL);

                        NormalManagerDB.add(0, cmd_ms, null, null);

                        if (cmd_ms.getException().getCodeError() != 0)
                            throw cmd_ms.getException();

                        v_ms_n = cmd_ms.getMapStatistics();

                        cmd_ms.setType(CmdMapStatistics.TYPE.ASSIST);

                        NormalManagerDB.add(0, cmd_ms, null, null);

                        if (cmd_ms.getException().getCodeError() != 0)
                            throw cmd_ms.getException();

                        v_msa_n = cmd_ms.getMapStatistics();

                        cmd_ms.setType(CmdMapStatistics.TYPE.NORMAL);
                        cmd_ms.setModo(CmdMapStatistics.TYPE_MODO.M_NATURAL);

                        NormalManagerDB.add(0, cmd_ms, null, null);

                        if (cmd_ms.getException().getCodeError() != 0)
                            throw cmd_ms.getException();

                        v_ms_na = cmd_ms.getMapStatistics();

                        cmd_ms.setType(CmdMapStatistics.TYPE.ASSIST);

                        NormalManagerDB.add(0, cmd_ms, null, null);

                        if (cmd_ms.getException().getCodeError() != 0)
                            throw cmd_ms.getException();

                        v_msa_na = cmd_ms.getMapStatistics();

                        cmd_ms.setType(CmdMapStatistics.TYPE.NORMAL);
                        cmd_ms.setModo(CmdMapStatistics.TYPE_MODO.M_GRAND_PRIX);

                        NormalManagerDB.add(0, cmd_ms, null, null);

                        if (cmd_ms.getException().getCodeError() != 0)
                            throw cmd_ms.getException();

                        v_ms_g = cmd_ms.getMapStatistics();

                        cmd_ms.setType(CmdMapStatistics.TYPE.ASSIST);

                        NormalManagerDB.add(0, cmd_ms, null, null);

                        if (cmd_ms.getException().getCodeError() != 0)
                            throw cmd_ms.getException();

                        v_msa_g = cmd_ms.getMapStatistics();

                        var cmd_tei = new CmdTrophySpecial(uid, (CmdTrophySpecial.TYPE_SEASON)(season), CmdTrophySpecial.TYPE.NORMAL);

                        NormalManagerDB.add(0, cmd_tei, null, null);

                        if (cmd_tei.getException().getCodeError() != 0)
                            throw cmd_tei.getException();

                        List<TrofelEspecialInfo> v_tei = cmd_tei.getInfo();

                        var cmd_ti = new CmdTrofelInfo(uid, (CmdTrofelInfo.TYPE_SEASON)(season));

                        NormalManagerDB.add(0, cmd_ti, null, null);

                        if (cmd_ti.getException().getCodeError() != 0)
                            throw cmd_ti.getException();

                        TrofelInfo ti = cmd_ti.getInfo();

                        cmd_tei.setType(CmdTrophySpecial.TYPE.GRAND_PRIX);

                        NormalManagerDB.add(0, cmd_tei, null, null);

                        if (cmd_tei.getException().getCodeError() != 0)
                            throw cmd_tei.getException();

                        List<TrofelEspecialInfo> v_tegi = cmd_tei.getInfo();

                        _session.Send(packet_func_sv.pacote157(mi, season));

                        _session.Send(packet_func_sv.pacote15E(uid, ci));

                        _session.Send(packet_func_sv.pacote156(uid, ue, season));

                        _session.Send(packet_func_sv.pacote158(uid, ui, season));

                        _session.Send(packet_func_sv.pacote15D(uid, gi));

                        _session.Send(packet_func_sv.pacote15C(uid, v_ms_na, v_msa_na, Convert.ToByte((season != 0) ? 0x33 : 0x0A)));

                        _session.Send(packet_func_sv.pacote15C(uid, v_ms_g, v_msa_g, Convert.ToByte((season != 0) ? 0x34 : 0x0B)));

                        _session.Send(packet_func_sv.pacote15B(uid, season));

                        _session.Send(packet_func_sv.pacote15A(uid, v_tei, season));

                        _session.Send(packet_func_sv.pacote159(uid, ti, season));

                        _session.Send(packet_func_sv.pacote15C(uid, v_ms_n.ToList(), v_msa_n.ToList(), season));

                        _session.Send(packet_func_sv.pacote257(uid, v_tegi, season));

                        _session.Send(packet_func_sv.pacote089(uid, season));
                    }

                    return;
                }

                // Verifica se não é o mesmo UID, pessoas diferentes
                // Quem quer ver a info não é GM aí verifica se o player é GM
                if (uid != _session.m_pi.uid && !pi.m_cap.game_master/* & 4/*(GM)*/)
                {
                    _session.Send(packet_func_sv.pacote089(uid, season, 3));

                }
                else
                {

                    var pCi = pi.findCharacterById(pi.ue.character_id);

                    if (pCi != null)
                        ci = pCi;

                    List<MapStatisticsEx> v_ms_n = new List<MapStatisticsEx>(), v_msa_n = new List<MapStatisticsEx>(), v_ms_na = new List<MapStatisticsEx>(), v_msa_na = new List<MapStatisticsEx>(), v_ms_g = new List<MapStatisticsEx>(), v_msa_g = new List<MapStatisticsEx>();

                    for (byte i = 0; i < MS_NUM_MAPS; ++i)
                        if (pi.a_ms_normal[i].best_score != 127)
                            v_ms_n.Add(pi.a_ms_normal[i]);

                    for (byte i = 0; i < MS_NUM_MAPS; ++i)
                        if (pi.a_msa_normal[i].best_score != 127)
                            v_msa_n.Add(pi.a_msa_normal[i]);

                    for (byte i = 0; i < MS_NUM_MAPS; ++i)
                        if (pi.a_ms_natural[i].best_score != 127)
                            v_ms_na.Add(pi.a_ms_natural[i]);

                    for (byte i = 0; i < MS_NUM_MAPS; ++i)
                        if (pi.a_msa_natural[i].best_score != 127)
                            v_msa_na.Add(pi.a_msa_natural[i]);

                    for (byte i = 0; i < MS_NUM_MAPS; ++i)
                        if (pi.a_ms_grand_prix[i].best_score != 127)
                            v_ms_g.Add(pi.a_ms_grand_prix[i]);

                    for (byte i = 0; i < MS_NUM_MAPS; ++i)
                        if (pi.a_msa_grand_prix[i].best_score != 127)
                            v_msa_g.Add(pi.a_msa_grand_prix[i]);

                    _session.Send(packet_func_sv.pacote157(pi.mi, season));

                    _session.Send(packet_func_sv.pacote15E(pi.uid, ci));

                    _session.Send(packet_func_sv.pacote156(pi.uid, pi.ue, season));

                    _session.Send(packet_func_sv.pacote158(pi.uid, pi.ui, season));

                    _session.Send(packet_func_sv.pacote15D(pi.uid, pi.gi));

                    _session.Send(packet_func_sv.pacote15C(pi.uid, v_ms_na, v_msa_na, (byte)((season != 0) ? 0x33 : 0x0A)));

                    _session.Send(packet_func_sv.pacote15C(pi.uid, v_ms_g, v_msa_g, (byte)((season != 0) ? 0x34 : 0x0B)));

                    _session.Send(packet_func_sv.pacote15B(uid, season));

                    _session.Send(packet_func_sv.pacote15A(pi.uid, (season != 0) ? pi.v_tsi_current_season : pi.v_tsi_rest_season, season));

                    _session.Send(packet_func_sv.pacote159(pi.uid, (season != 0) ? pi.ti_current_season : pi.ti_rest_season, season));

                    _session.Send(packet_func_sv.pacote15C(pi.uid, v_ms_n, v_msa_n, season));

                    _session.Send(packet_func_sv.pacote257(pi.uid, (season != 0) ? pi.v_tgp_current_season : pi.v_tgp_rest_season, season));


                    _session.Send(packet_func_sv.pacote089(uid, season));

                }
            }
            catch (Exception e)
            {
                message_pool.push(new message($"[GameServer::RequestPlayerInfo][ErrorSystem] {e.Message}", type_msg.CL_ONLY_CONSOLE));
                _session.Send(packet_func_sv.pacote089(0));
            }
        }

        public void requestPrivateMessage(Player _session, Packet _packet)
        {
            PangyaBinaryWriter p = new PangyaBinaryWriter();
            Player s = null;
            string nickname = "";

            try
            {

                // Verifica se session está autorizada para executar esse ação, 
                // se ele não fez o login com o Server ele não pode fazer nada até que ele faça o login
                //    CHECK_SESSION_IS_AUTHORIZED("PrivateMessage");

                nickname = _packet.ReadPStr();
                string msg = _packet.ReadPStr();

                if (nickname.empty())
                    throw new exception("[GameServer.requestPrivateMessage][Error] player[UID=" + (_session.m_pi.uid) + "] tentou enviar message privada[msg=" + msg + "] para o player[NICKNAME="
                            + nickname + "], mas o nick esta vazio. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.GAME_SERVER, 1, 5));

                if (msg.empty())
                    throw new exception("[GameServer.requestPrivateMessage][Error] player[UID=" + (_session.m_pi.uid) + "] tentou enviar message privada[msg=" + msg + "] para o player[NICKNAME="
                        + nickname + "], mas message esta vazia. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.GAME_SERVER, 4, 5));

                // Verifica se o player tem os itens necessários(PREMIUM USER OR GM) para usar essa função
                if (nickname.Contains("#SC") || nickname.Contains("#CS"))
                {

                    // Só sai do Private message se for comando do Smart Calculator, se não faz as outras verificações para enviar o PM
                    //if (m_si.rate.smart_calculator && checkSmartCalculatorCmd(_session, msg, (nickname.compare("#SC") == 0 ? eTYPE_CALCULATOR_CMD::SMART_CALCULATOR : eTYPE_CALCULATOR_CMD::CALCULATOR_STADIUM)))
                    //    return;
                }

                s = (Player)FindSessionByNickname(nickname);

                if (s == null || !s.GetState() || !s.getConnected())
                    throw new exception("[GameServer.requestPrivateMessage][WARNING] player[UID=" + (_session.m_pi.uid) + "] tentou enviar message privada[msg=" + msg + "] para o player[NICKNAME="
                            + nickname + "], mas o player nao esta online nesse server.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.GAME_SERVER, 2, 5));

                // Whisper Block
                if (!s.m_pi.whisper.IsTrue())
                    throw new exception("[GameServer.requestPrivateMessage][WARNING] player[UID=" + (_session.m_pi.uid) + "] tentou enviar message privada[msg=" + msg + "] para o player[NICKNAME="
                            + nickname + "], mas o whisper do player esta bloqueado.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.GAME_SERVER, 5, 4));

                if ((s.m_pi.lobby == 255/*não está na lobby*/ && s.m_pi.mi.sala_numero == ushort.MaxValue/*e não está em nenhum sala*/) || s.m_pi.place == 2)
                    throw new exception("[GameServer.requestPrivateMessage][WARNING] player[UID=" + (_session.m_pi.uid) + "] tentou enviar message privada[msg=" + msg + "] para o player[NICKNAME="
                            + nickname + "], mas o player nao pode receber message agora, por que nao pode ver o chat. pode estar no Papel Shop e Etc.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.GAME_SERVER, 3, 4));

                // Arqui procura por palavras inapropriadas na message

                // Envia para todo os GM do serve   r essa message
                var gm = FindAllGM();

                if (!gm.Any())
                {

                    var msg_gm = "\\5" + (_session.m_pi.nickname) + ">" + (s.m_pi.nickname) + ": '" + msg + "'";

                    foreach (Player el in gm)
                    {
                        if ((el.m_gi.whisper.IsTrue() || el.m_gi.isOpenPlayerWhisper(_session.m_pi.uid) || el.m_gi.isOpenPlayerWhisper(s.m_pi.uid))
                            && /*Nao envia o log de PM novamente para o GM que enviou ou recebeu PM*/(el.m_pi.uid != _session.m_pi.uid && el.m_pi.uid != s.m_pi.uid))
                        {
                            // Responde no chat do player
                            p.init_plain(0x40);

                            p.WriteByte(0);

                            p.WritePStr("\\1[PM]"); // Nickname

                            p.WritePStr(msg_gm);    // Message
                            el.Send(p);
                        }
                    }

                }

                // Log
                _smp::message_pool.push(new message("[PrivateMessage][Log] player[UID=" + (_session.m_pi.uid) + "] enviou a Message[" + msg + "] para o player[UID=" + (s.m_pi.uid) + "]", type_msg.CL_FILE_LOG_AND_CONSOLE));

                // Resposta para o que enviou a private message
                p.init_plain(0x84);

                p.WriteByte(0); // FROM

                p.WritePStr(s.m_pi.nickname);   // Nickname TO
                p.WritePStr(msg);
                _session.Send(p);

                // Resposta para o player que vai receber a private message
                p.init_plain(0x84);

                p.WriteByte(1); // TO

                p.WritePStr(_session.m_pi.nickname);    // Nickname FROM
                p.WritePStr(msg);
                s.Send(p);

                // Envia a mensagem para o Chat History do discord se ele estiver ativo

                // Verifica se o m_chat_discod flag está ativo para enviar o chat para o discord
                //     if (m_si.rate.smart_calculator && m_chat_discord)
                //sendMessageToDiscordChatHistory(
                //	"[PM]",                                                                                                             // From
                //             (_session.m_pi.nickname) + ">" + (s.m_pi.nickname) + ": '" + msg + "'"						// Msg
                //);

            }
            catch (exception e)
            {

                _smp::message_pool.push(new message("[GameServer.requestPrivateMessage][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                p.init_plain(0x40);

                p.WriteByte((ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) == (uint)STDA_ERROR_TYPE.GAME_SERVER) ? (byte)ExceptionError.STDA_SYSTEM_ERROR_DECODE(e.getCodeError()) : 5);
                if (s != null && s.getConnected())
                    p.WritePStr(s.m_pi.nickname);
                else
                    p.WritePStr(nickname);  // Player não está online usa o nickname que ele forneceu
                _session.Send(p);
            }
        }

        public void requestQueueTicker(Player _session, Packet _packet)
        {
            ////REQUEST_BEGIN("QueueTicker");

            PangyaBinaryWriter p = new PangyaBinaryWriter();

            try
            {

                //if (_session.m_pi.block_flag.m_flag.ticker)
                //    throw new exception("[GameServer.requestQueueTicker][Error] player[UID=" + (_session.m_pi.m_uid)
                //            + "] tentou abrir a fila do Ticker, mas o ticker esta bloqueado. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.GAME_SERVER, 10, 1/*UNKNOWN ERROR*/));

                var count = m_ticker.getSize();

                var time_left_milisecond = count * 30000;

                // Send Count Ticker and time left for send ticker
                p.init_plain(0xCA);

                p.WriteUInt16((ushort)count);
                p.WriteUInt32(time_left_milisecond);
                _session.Send(p);

            }
            catch (exception e)
            {

                _smp::message_pool.push(new message("[GameServer.requestQueueTicker][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                // estou usando pacote de troca nickname, por que n�o sei qual o pangya manda, quando da erro no mandar ticker, nunca peguei esse erro
                p.init_plain(0x50);

                p.WriteUInt32((ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) == (uint)STDA_ERROR_TYPE.GAME_SERVER) ? ExceptionError.STDA_SYSTEM_ERROR_DECODE(e.getCodeError()) : 1/*UNKNOWN ERROR*/);

                _session.Send(p);
            }
        }

        public void requestSendNotice(string notice)
        {
        }

        public void requestSendTicker(Player _session, Packet _packet)
        {
        }

        public void requestTranslateSubPacket(Player _session, Packet _packet)
        {
        }

        public void requestUCCSystem(Player _session, Packet _packet)
        {
            _session.HandleUCC(_packet);
        }

        public void requestUCCWebKey(Player _session, Packet _packet)
        {
        }

        public void sendChannelListToSession(Player _session)
        {
            try
            {
                _session.Send(packet_func_sv.pacote04D(v_channel));
            }
            catch (exception e)
            {
                _smp.message_pool.push("[GameServer.sendChannelListToSession][ErrorSystem] " + e.getFullMessageError());
            }
        }
    }
}
