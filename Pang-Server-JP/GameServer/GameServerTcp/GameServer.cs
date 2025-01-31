using System;
using _smp = PangyaAPI.Utilities.Log;
using PangyaAPI.Utilities.Log;
using PangyaAPI.Utilities;
using PangyaAPI.Network.PangyaSession;
using PangyaAPI.Network.PangyaPacket;
using PangyaAPI.Network.PangyaServer;
using GameServer.Session;
using System.Collections.Generic;
using PangyaAPI.SQL;
using PangyaAPI.Network.Cmd;
using GameServer.PangType;
using GameServer.Game;
using GameServer.PacketFunc;
using PangyaAPI.Utilities.BinaryModels;
using System.Net.Sockets;
using GameServer.PangDefinition;

namespace GameServer.GameServerTcp
{
    public partial class GameServerBase : Server
    {
        public int m_access_flag { get; private set; }
        public int m_create_user_flag { get; private set; }
        public int m_same_id_login_flag { get; private set; }
        DailyQuestInfo m_dqi;                                   
        protected List<Channel> v_channel;
        public GameServerBase() : base(new player_manager(2000))
        {
            v_channel = new List<Channel>();         
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

                _smp.message_pool.push(("[GameServer::config_init][ErrorSystem] Config.FLAG" + e.getFullMessageError()));
            }

            // Game Guard Auth
            try
            {

                // m_g = (m_reader_ini.readInt("SERVERINFO", "GAMEGUARDAUTH") >= 1 ? 1 : 0);

            }
            catch (exception e)
            {

                _smp.message_pool.push(("[GameServer::config_init][ErrorSystem] Config.GAMEGUARDAUTH. " + e.getFullMessageError()));
            }


            // Recupera Valores de rate do gs do banco de dados
            var cmd_rci = new CmdRateConfigInfo(m_si.uid);  // Waiter
                                                
            if (cmd_rci.getException().getCodeError() != 0 || cmd_rci.isError()/*Deu erro na consulta não tinha o rate config info para esse gs, pode ser novo*/)
            {

                if (cmd_rci.getException().getCodeError() != 0)
                    _smp.message_pool.push(("[GameServer::config_init][ErrorSystem] " + cmd_rci.getException().getFullMessageError()));


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
                if (!this._isRunning)
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
                           
        public virtual void sendServerListAndChannelListToSession(Player _session)
        {                                                                 
            _session.Send(packet_func.pacote09F(m_server_list, v_channel));
        }
        public virtual void sendDateTimeToSession(Player _session)
        {
            var localtime = new PangyaTime();
            localtime.CreateTime();
            using (var p = new PangyaBinaryWriter((ushort)0xBA))
            {
                p.WriteStruct(localtime, new PangyaTime());
                _session.Send(p);
            }
        }

        public virtual void sendRankServer(Player _session) { }

        public virtual Channel findChannel(byte _channel) { return v_channel.Find(c => c.getId() == _channel); }

        public virtual Player findPlayer(uint _uid, bool _oid = false) { return null; }

        // find All GM Online
        public virtual List<Player> findAllGM() { return null; }

        public virtual void blockOID(uint _oid) { }
        public virtual void unblockOID(uint _oid) { }

        DailyQuestInfo getDailyQuestInfo() { return m_dqi; }           
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
            ChannelInfo ci = new ChannelInfo();
            int num_channel = m_reader_ini.readInt("CHANNELINFO", "NUM_CHANNEL");

            for (byte i = 0; i < num_channel; ++i)
            {
                ci.id = i;
                ci.name = m_reader_ini.ReadString("CHANNEL" + (i + 1), "NAME");
                ci.max_user = m_reader_ini.ReadInt16("CHANNEL" + (i + 1), "MAXUSER");
                ci.max_level_allow = m_reader_ini.ReadUInt32("CHANNEL" + (i + 1), "MAXLEVEL");
                ci.min_level_allow = m_reader_ini.ReadUInt32("CHANNEL" + (i + 1), "LOWLEVEL");

                try
                {                 
                    ci.SetFlag(m_reader_ini.ReadUInt32("CHANNEL" + (i + 1), "FLAG"));
                }
                catch (Exception e)
                {

                    _smp.message_pool.push("[GameServer::init_load_channels][ErrorSystem] " + e.Message);
                }

                v_channel.Add(new Channel(ci, m_si.propriedade.ulProperty));
                ci = new ChannelInfo();
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
                Response.Write(new byte[] { 0x00, 0x06, 0x00, 0x00, 0x3f, 0x00, 0x01, 0x01 });
                Response.WriteByte(_session.m_key);
                _session.SafeSend(Response.GetBytes);
            }
            catch (Exception ex)
            {
                _smp.message_pool.push(new message(
              $"[GameServer::onAcceptCompleted][ErrorSt] {ex.Message}\nStack Trace: {ex.StackTrace}",
              type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
        }

        public override bool CheckPacket(SessionBase _session, Packet packet)
        {
            var player = (Player)_session;
            WriteConsole.WriteLine($"[GameServer::CheckPacket][Log]: PLAYER[UID= {player.m_pi.uid}, GPID = {(PacketGame)packet.Id}]", ConsoleColor.Cyan);
            return true;
        }

        public override void onDisconnected(SessionBase _session)
        {
            if (_session == null)
                throw new exception("[GameServer::onDisconnected][Error] _session is nullptr");

            var _player = (Player)_session;

            _smp::message_pool.push(new message("[GameServer::onDisconnected][Log] Player Desconectou. ID: " + _player.m_pi.id + "  UID: " + _player.m_pi.uid));

            /// Novo
            var _channel = findChannel(_player.m_pi.channel);

            try
            {

                if (_channel != null)
                    _channel.leaveChannel(_player);

            }
            catch (exception e)
            {

                _smp::message_pool.push(new message("[GameServer::onDisconnect][Error] " + e.getFullMessageError()));
            }


        }

        //chama alguma coisa aqui!
        public override void OnStart()
        {
            Console.Title = $"Game Server - P: {m_si.curr_user}";                                             
        }
    }
}
