using System;
using _smp = PangyaAPI.Utilities.Log;
using PangyaAPI.Utilities.Log;
using PangyaAPI.Utilities;
using PangyaAPI.TCP.Session;
using PangyaAPI.TCP.PangyaPacket;
using PangyaAPI.TCP.PangyaServer;
using GameServer.Session;
using System.Collections.Generic;
using PangyaAPI.SQL;
using PangyaAPI.TCP.Cmd;
using GameServer.PangType;
using GameServer.Game;
using GameServer.PacketFunc;

namespace GameServer.GameServerTcp
{
    public partial class GameServerBase : Server
    {
        public int m_access_flag { get; private set; }
        public int m_create_user_flag { get; private set; }
        public int m_same_id_login_flag { get; private set; }
        DailyQuestInfo m_dqi;
        public LoginManager m_login_manager;
        protected List<Channel> v_channel;
        public GameServerBase() : base(new player_manager(2000))
        {
            v_channel = new List<Channel>();
            m_login_manager = new LoginManager();
            m_player_manager = new player_manager(2000); 
            ConfigInit();
            init_Packets();
            init_load_channels();

            StartingServer();
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

                _smp.message_pool.push(("[game_server::config_init][ErrorSystem] Config.FLAG" + e.getFullMessageError()));
            }

            // Game Guard Auth
            try
            {

                // m_g = (m_reader_ini.readInt("SERVERINFO", "GAMEGUARDAUTH") >= 1 ? 1 : 0);

            }
            catch (exception e)
            {

                _smp.message_pool.push(("[game_server::config_init][ErrorSystem] Config.GAMEGUARDAUTH. " + e.getFullMessageError()));
            }


            // Recupera Valores de rate do gs do banco de dados
            var cmd_rci = new CmdRateConfigInfo(m_si.uid);  // Waiter

            cmd_rci.ExecCmd();

            if (cmd_rci.getException().getCodeError() != 0 || cmd_rci.isError()/*Deu erro na consulta não tinha o rate config info para esse gs, pode ser novo*/)
            {

                if (cmd_rci.getException().getCodeError() != 0)
                    _smp.message_pool.push(("[game_server::config_init][ErrorSystem] " + cmd_rci.getException().getFullMessageError()));


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
            m_si.event_flag.stBit.angel_wing = _angel_event > 0;

            // Update Event Angel
            m_si.rate.angel_event = (short)_angel_event;
        }
        private void setratePang(short _pang)
        {
            // Update Flag Event
            m_si.event_flag.stBit.pang_x_plus = (_pang >= 200) ? true : false;

            // Update rate Pang
            m_si.rate.pang = (short)_pang;
        }
        private void setrateExp(short _exp)
        {// Reseta flag antes de atualizar ela 
            m_si.event_flag.stBit.exp_x2 = m_si.event_flag.stBit.exp_x_plus = true;

            // Update Flag Event
            if (_exp > 200)
                m_si.event_flag.stBit.exp_x_plus = true;
            else if (_exp == 200)
                m_si.event_flag.stBit.exp_x2 = true;
            else
                m_si.event_flag.stBit.exp_x2 = m_si.event_flag.stBit.exp_x_plus = false;

            // Update rate Experiência
            m_si.rate.exp = _exp;
        }
        private void setrateClubMastery(short _club_mastery)
        {
            // Update Flag Event
            m_si.event_flag.stBit.club_mastery_x_plus = (_club_mastery >= 200) ? true : false;

            // Update rate Club Mastery
            m_si.rate.club_mastery = (short)_club_mastery;
        }

        public override void OnHeartBeat()
        {
            try
            {
                // Server ainda não está totalmente iniciado
                if (this.m_state != ServerState.Good)
                    return;

                // Tirei o list IP/MAC block daqui e coloquei no monitor no gs, por que agora eles são da classe gs
            }
            catch (exception e)
            {
                _smp.message_pool.push("[login_server::onHeartBeat][ErrorSystem] " + e.getFullMessageError(), _smp.type_msg.CL_FILE_LOG_AND_CONSOLE);
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


        public virtual void destroyRoom(byte _channel_owner, short _number) { }


        public virtual void clear() { }

        public virtual Channel enterChannel(Player _session, byte _channel) { return null; }

        public virtual void sendChannelListToSession(Player _session) { }
        public virtual void sendServerListAndChannelListToSession(Player _session) { }
        public virtual void sendDateTimeToSession(Player _session) { }

        public virtual void sendRankServer(Player _session) { }

        public virtual Channel findChannel(byte _channel) { return v_channel.Find(c => c.getId() == _channel); }

        public virtual Player findPlayer(uint _uid, bool _oid = false) { return null; }

        // find All GM Online
        public virtual List<Player> findAllGM() { return null; }

        public virtual void blockOID(uint _oid) { }
        public virtual void unblockOID(uint _oid) { }

        DailyQuestInfo getDailyQuestInfo() { return m_dqi; }

        public virtual LoginManager getLoginManager() { return m_login_manager; }


        // Login
        public virtual void requestLogin(Player _session, Packet _Packet) { }

        // Channel
        public virtual void requestEnterChannel(Player _session, Packet _Packet) { }

        public virtual void requestEnterOtherChannelAndLobby(Player _session, Packet _Packet) { }

        // Change Server
        public virtual void requestChangeServer(Player _session, Packet _Packet) { }

        // UCC::Self Design System [Info, Save, Web Key]
        public virtual void requestUCCWebKey(Player _session, Packet _Packet) { }
        public virtual void requestUCCSystem(Player _session, Packet _Packet) { }

        // Chat
        public virtual void requestChat(Player _session, Packet _Packet) { }

        // Chat Macro
        public virtual void requestChangeChatMacroUser(Player _session, Packet _Packet) { }

        // Request Player Info
        public virtual void requestPlayerInfo(Player _session, Packet _Packet) { }

        // Private message
        public virtual void requestPrivateMessage(Player _session, Packet _Packet) { }
        public virtual void requestChangeWhisperState(Player _session, Packet _Packet) { }
        public virtual void requestNotifyNotDisplayPrivateMessageNow(Player _session, Packet _Packet) { }

        // Command GM
        public virtual void requestCommonCmdGM(Player _session, Packet _Packet) { }
        public virtual void requestCommandNoticeGM(Player _session, Packet _Packet) { }

        // Request translate Sub Packet
        public virtual void requestTranslateSubPacket(Player _session, Packet _Packet) { }

        // Ticker
        public virtual void requestSendNotice(string notice) { }

        public virtual void requestSendTicker(Player _session, Packet _Packet) { }
        public virtual void requestQueueTicker(Player _session, Packet _Packet) { }

        // Exception Client message
        public virtual void requestExceptionClientMessage(Player _session, Packet _Packet) { }

        // Game Guard Auth
        public virtual void requestCheckGameGuardAuthAnswer(Player _session, Packet _Packet) { }

        // Set rate Server

        // Set Event Server
        public virtual void setAngelEvent(uint _angel_event) { }

        // Update Daily Quest Info
        public virtual void updateDailyQuest(DailyQuestInfo _dqi) { }

        // send Update Room Info, find room nos canais e atualiza o info
        //	public virtual void sendUpdateRoomInfo(room _r, int _option) { }


        player_manager m_player_manager;


        public virtual bool checkCommand(string[] _command) { return true; }

        public virtual void reload_files() { }

        public virtual void init_systems() { }
        public virtual void init_Packets()
        {
            this.addPacketCall(0x02, packet_func.packet002);
            this.addPacketCall(0x03, packet_func.packet003);
            this.addPacketCall(0x04, packet_func.packet004);
            this.addPacketCall(0x81, packet_func.packet081);
        }
        public virtual void init_load_channels()
        {
            ChannelInfo ci = new ChannelInfo();
            int num_channel = m_reader_ini.readInt("CHANNELINFO", "NUM_CHANNEL");

            for (byte i = 0; i < num_channel; ++i)
            {
                ci.id = i;
                ci.name = m_reader_ini.ReadString("CHANNEL" + (i + 1), "NAME");
                ci.max_user = m_reader_ini.ReadInt16("CHANNEL" + (i + 1), "MAXUSER");

                try
                {
                    ci.flag.ulFlag = m_reader_ini.ReadUInt32("CHANNEL" + (i + 1), "FLAG");
                }
                catch (Exception e)
                {

                    _smp.message_pool.push("[game_server::init_load_channels][ErrorSystem] " + e.Message);
                }

                v_channel.Add(new Channel(ci, m_si.propriedade.ulProperty));
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
                var Packet = new Packet();
                Packet.Write(new byte[] { 0x00, 0x06, 0x00, 0x00, 0x3F, 0x00 });
                Packet.WriteByte(1);	// OPTION 1
                Packet.WriteByte(1);	// OPTION 2
                Packet.WriteByte(_session.m_key);	// Key
                _session.SendResponse(Packet.GetBytes(), true);
            }
            catch (Exception ex)
            {
                _smp.message_pool.push(ex.Message, "GameServer::onAcceptCompleted", 808);
            }
        }
        public override bool CheckPacket(SessionBase session, Packet Packet)
        {
            return true;
        }

        public override void onDisconnected(SessionBase _session)
        {
            if (_session == null)
                throw new exception("[game_server::onDisconnected][Error] _session is nullptr");

            var _player = (Player)_session;

            _smp::message_pool.push(new message("[game_server::onDisconnected][Log] Player Desconectou. ID: " + _player.m_pi.id + "  UID: " + _player.m_pi.uid));

            /// Novo
            var _channel = findChannel(_player.m_pi.channel);

            try
            {

                if (_channel != null)
                    _channel.leaveChannel(_player);

            }
            catch (exception e)
            {

                _smp::message_pool.push(new message("[game_server::onDisconnect][Error] " + e.getFullMessageError()));
            }


        }

        //chama alguma coisa aqui!
        public override void OnStart()
        {
            Console.Title = $"Game Server - P: {m_si.curr_user}";                                             
        }
    }
}
