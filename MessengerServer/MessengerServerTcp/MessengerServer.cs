using System;
using PangyaAPI.Network.PangyaServer;
using PangyaAPI.Utilities;
using MessengerServer.Session;
using PangyaAPI.Utilities.Log;
using PangyaAPI.IFF.JP.Extensions;
using PangyaAPI.SQL;
using PangyaAPI.Network.Cmd;
using PangyaAPI.SQL.Manager;
using MessengerServer.Cmd;
using MessengerServer.GameType;
using MessengerServer.PangyaEnums;
using System.Threading;
using System.Runtime.InteropServices;
using System.Linq;
using MessengerServer.Manager;
using PangyaAPI.Network.Pangya_St;
using PangyaAPI.Network.PangyaUtil;
using PangyaAPI.Network.PangyaPacket;
using MessengerServer.PacketFunc;
using PangyaAPI.Discord;

namespace MessengerServer.MessengerServerTcp
{
    public class MessengerServer : Server
    {
        public const int FRIEND_LIST_LIMIT = 50;
        public const int FRIEND_PAG_LIMIT = 30;
        player_manager m_player_manager;
        public MessengerServer()
        {
            if (m_state == ServerState.Failure)
            {
                message_pool.push(new message("[message_server::message_server][Error] falha ao incializar o message server.", type_msg.CL_FILE_LOG_AND_CONSOLE));
                return;
            }
            try
            {

                m_player_manager = new player_manager(this, 2000);

                set_sessionManager(m_player_manager);

                config_init();

                // Carrega IFF_STRUCT
                if (!sIff.getInstance().isLoad())
                    sIff.getInstance().load();

                // Request Cliente
                init_packets();

                // Initialized complete
                m_state = ServerState.Initialized;
            }
            catch (exception e)
            {

                message_pool.push(new message("[message_server::message_server][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                m_state = ServerState.Failure;
            }
        }

        public override bool CheckPacket(PangyaAPI.Network.PangyaSession.Session _session, packet packet)
        {
            var player = (Player)_session;
            var packetId = (PacketIDClient)packet.getTipo();

            // Verifica se o valor de packetId é válido no enum packetIDClient
            if (Enum.IsDefined(typeof(PacketIDClient), packetId))
            {
                Console.WriteLine($"[MessengerServer.Checkpacket][Log]: PLAYER[UID: {player.m_pi.uid}, CMPID: {packetId}]", ConsoleColor.Cyan);
                return true;
            }

            else// nao tem no packetIDClient
            {
                Console.WriteLine($"[MessengerServer.Checkpacket][Log]: PLAYER[UID: {player.m_pi.uid}, CMPID: 0x{packet.getTipo():X}]");
                return true;
            }
        }

        public override void onDisconnected(PangyaAPI.Network.PangyaSession.Session _session)
        {

            if (_session == null)
                throw new exception("[message_server::onDisconnect][Error] _session is nullptr.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.MESSAGE_SERVER, 60, 0));

            Player p = (Player)_session;

            bool ret = true;

            try
            {
                // S� envia o UpdatePlayerLogout se o player estiver autorizado(Fez o login)
                if (p.getState() && Interlocked.CompareExchange(ref ((Player)_session).m_pi.m_logout, ((Player)_session).m_pi.m_logout, 0) == 0 && p.m_is_authorized)
                    ret = sendUpdatePlayerLogoutToFriends(p);
            }
            catch (exception e)
            {
                message_pool.push(new message("[message_server::onDisconnected][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }

            // Log, Para n�o mostrar essa mensagem 2x
            if (ret)
                message_pool.push(new message("[message_server::onDisconnected][Log] Player Desconectou ID: " + (p.m_pi.id) + " UID: " + (p.m_pi.uid), type_msg.CL_FILE_LOG_AND_CONSOLE));

        }

        public override void OnHeartBeat()
        {
            // Aqui depois tenho que colocar uma verifica��o que eu queira fazer no server
            // Esse fun��o � chamada na thread monitor

            try
            {

                // Server ainda n�o est� totalmente iniciado
                if (m_state != ServerState.Initialized)
                    return;

                // Begin Check System Singleton Static

                // Carrega Smart Calculator Lib, S� inicializa se ele estiver ativado
                //if (m_si.rate.smart_calculator && !sSmartCalculator::getInstance().hasStopped() && !sSmartCalculator::getInstance().isLoad())
                //    sSmartCalculator::getInstance().load();

                // End Check System Singleton Static

            }
            catch (exception e)
            {

                message_pool.push(new message("[message_server::onHeartBeat][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }

            return;
        }

        public override void OnStart()
        {
            Console.Title = $"Messenger Server - P: {m_si.curr_user}";
        }

        protected override void onAcceptCompleted(PangyaAPI.Network.PangyaSession.Session _session)
        {
            try
            {

                packet p = new packet(0x2e);

                p.AddUInt8(1);
                p.AddUInt8(1);
                p.AddUInt32(_session.m_key);
                p.makeRaw();

                var mb = p.getBuffer();
                _session.requestSendBuffer(mb.buf, mb.len);
                p = null;
            }
            catch (exception ex)
            {
                message_pool.push(new message(
              $"[MessengerServer.onAcceptCompleted][ErrorSt]: {ex.getFullMessageError()}",
              type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
        }

        /// <summary>
        /// init packet to call !
        /// </summary>
        protected void init_packets()
        {
            packet_func.funcs.addPacketCall(0x12, packet_func.packet012, this);
            packet_func.funcs.addPacketCall(0x13, packet_func.packet013, this);
            packet_func.funcs.addPacketCall(0x14, packet_func.packet014, this);
            packet_func.funcs.addPacketCall(0x16, packet_func.packet016, this);
            packet_func.funcs.addPacketCall(0x17, packet_func.packet017, this);
            packet_func.funcs.addPacketCall(0x18, packet_func.packet018, this);
            packet_func.funcs.addPacketCall(0x19, packet_func.packet019, this);
            packet_func.funcs.addPacketCall(0x1A, packet_func.packet01A, this);
            packet_func.funcs.addPacketCall(0x1B, packet_func.packet01B, this);
            packet_func.funcs.addPacketCall(0x1C, packet_func.packet01C, this);
            packet_func.funcs.addPacketCall(0x1D, packet_func.packet01D, this);
            packet_func.funcs.addPacketCall(0x1E, packet_func.packet01E, this);
            packet_func.funcs.addPacketCall(0x1F, packet_func.packet01F, this);
            packet_func.funcs.addPacketCall(0x23, packet_func.packet023, this);
            packet_func.funcs.addPacketCall(0x24, packet_func.packet024, this);
            packet_func.funcs.addPacketCall(0x25, packet_func.packet025, this);
            packet_func.funcs.addPacketCall(0x28, packet_func.packet028, this);
            packet_func.funcs.addPacketCall(0x29, packet_func.packet029, this);
            packet_func.funcs.addPacketCall(0x2A, packet_func.packet02A, this);
            packet_func.funcs.addPacketCall(0x2B, packet_func.packet02B, this);
            packet_func.funcs.addPacketCall(0x2C, packet_func.packet02C, this);
            packet_func.funcs.addPacketCall(0x2D, packet_func.packet02D, this);

            // Resposta Server
            packet_func.funcs_sv.addPacketCall(0x2E, packet_func.packet_svFazNada, this);
            packet_func.funcs_sv.addPacketCall(0x2F, packet_func.packet_svFazNada, this);
            packet_func.funcs_sv.addPacketCall(0x30, packet_func.packet_svFazNada, this);
            packet_func.funcs_sv.addPacketCall(0x3B, packet_func.packet_svFazNada, this);
            packet_func.funcs_sv.addPacketCall(0x3C, packet_func.packet_svFazNada, this);
            packet_func.funcs_sv.addPacketCall(0x40, packet_func.packet_svFazNada, this); // Msg Aviso Lobby, cliente tamb�m aceita o Message Server enviar esse Pacote

            // Auth Server
            packet_func.funcs_as.addPacketCall(0x01, packet_func.packet_as001, this);
            packet_func.funcs_as.addPacketCall(0x02, packet_func.packet_as002, this);
            packet_func.funcs_as.addPacketCall(0x03, packet_func.packet_as003, this);

        }


        // Request Login
        public async void requestLogin(Player _session, packet _packet)
        {
            packet p = new packet();

            try
            {

                uint uid = _packet.ReadUInt32();
                var nickname = _packet.ReadString();

                message_pool.push(new message("UID: " + (uid), type_msg.CL_FILE_LOG_AND_CONSOLE));
                message_pool.push(new message("NICKNAME: " + nickname, type_msg.CL_FILE_LOG_AND_CONSOLE));

                if (uid == 0)
                    throw new exception("[message_server::requestLogin][Error] player[UID=" + (uid) + ", NICKNAME="
                            + nickname + "] tentou logar com Server, mas o uid eh invalido. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.MESSAGE_SERVER, 1, 0x5200101));

                if (nickname.empty())
                    throw new exception("[message_server::requestLogin][Error] player[UID=" + (uid) + ", NICKNAME="
                            + nickname + "] tentou logar com Server, mas o nickname esta vazio. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.MESSAGE_SERVER, 2, 0x5200102));

                // Verifica se o IP/MAC Address est� banido
                //if (haveBanList(_session.getIP(), "", false/*N�o tem MAC Address esse pacote*/))
                //    throw new exception("[message_server::requestLogin][Error] Player[UID=" + (uid) + ", NICKNAME=" + nickname + ", IP=" + _session.getIP()
                //            + "] tentou logar com o Server, mas ele esta com IP banido.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.MESSAGE_SERVER, 5, 0x5200105));

                var cmd_pi = new CmdPlayerInfo(uid);    // Waiter

                NormalManagerDB.add(0, cmd_pi, null, null);

                if (cmd_pi.getException().getCodeError() != 0)
                    throw cmd_pi.getException();

                _session.m_pi.set_info(cmd_pi.getInfo());

                if (nickname.CompareTo(_session.m_pi.nickname) != 0)
                    throw new exception("[message_server::requestLogin][Error] player[UID=" + (uid) + ", NICKNAME="
                            + nickname + "] tentou logar com Server, mas o nickname do databse[NICKNAME_DB=" + (_session.m_pi.nickname) + "] eh diferente do fornecido pelo cliente. Hacker ou Bug",
                            ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.MESSAGE_SERVER, 4, 0x5200104));

                // Verifica se o player est� bloqueado
                if (_session.m_pi.block_flag.m_id_state.ull_IDState != 0)
                {

                    if (_session.m_pi.block_flag.m_id_state.L_BLOCK_TEMPORARY && (_session.m_pi.block_flag.m_id_state.block_time == -1 || _session.m_pi.block_flag.m_id_state.block_time > 0))
                    {

                        throw new exception("[message_server::requestLogin][Log] Bloqueado por tempo[Time="
                                + (_session.m_pi.block_flag.m_id_state.block_time == -1 ? ("indeterminado") : ((_session.m_pi.block_flag.m_id_state.block_time / 60)
                                + "min " + (_session.m_pi.block_flag.m_id_state.block_time % 60) + "sec"))
                                + "]. player [UID=" + (_session.m_pi.uid) + ", ID=" + (_session.m_pi.id) + "]", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.MESSAGE_SERVER, 1029, 0));

                    }
                    else if (_session.m_pi.block_flag.m_id_state.L_BLOCK_FOREVER)
                    {

                        throw new exception("[message_server::requestLogin][Log] Bloqueado permanente. player [UID=" + (_session.m_pi.uid)
                                + ", ID=" + (_session.m_pi.id) + "]", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.MESSAGE_SERVER, 1030, 0));

                    }
                    //else if (_session.m_pi.block_flag.m_id_state.L_BLOCK_ALL_IP)
                    //{

                    //    // Bloquea todos os IP que o player logar e da error de que a area dele foi bloqueada

                    //    // Add o ip do player para a lista de ip banidos
                    //    NormalManagerDB.add(1, new CmdInsertBlockIP(_session.m_ip, "255.255.255.255"), message_server::SQLDBResponse, this);

                    //    // Resposta
                    //    throw new exception("[message_server::requestLogin][Log] Player[UID=" + (_session.m_pi.uid) + ", IP=" + (_session.getIP())
                    //            + "] Block ALL IP que o player fizer login.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.MESSAGE_SERVER, 1031, 0));

                    //}
                    //else if (_session.m_pi.block_flag.m_id_state.L_BLOCK_MAC_ADDRESS)
                    //{

                    //    // Bloquea o MAC Address que o player logar e da error de que a area dele foi bloqueada

                    //    // Add o MAC Address do player para a lista de MAC Address banidos
                    //    //NormalManagerDB.add(2, new CmdInsertBlockMAC(mac_address), message_server::SQLDBResponse, this);

                    //    // Resposta
                    //    throw new exception("[message_server::requestLogin][Log] Player[UID=" + (_session.m_pi.uid)
                    //            + ", IP=" + (_session.getIP()) + ", MAC=UNKNON] (MSG nao recebe o MAC Address do cliente) Block MAC Address que o player fizer login.",
                    //            ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.MESSAGE_SERVER, 1032, 0));

                    //}
                }

                // Verifica se j� tem outro socket com o mesmo uid conectado
                var s = HasLoggedWithOuterSocket(_session);

                if (s != null)
                {

                    message_pool.push(new message("[message_server::requestLogin][Log] Player[UID=" + (uid) + ", OID="
                            + (_session.m_oid) + ", IP=" + _session.getIP() + "] que esta logando agora, ja tem uma outra Session com o mesmo UID logado, desloga o outro Player[UID="
                            + (s.getUID()) + ", OID=" + (s.m_oid) + ", IP=" + s.getIP() + "]", type_msg.CL_FILE_LOG_AND_CONSOLE));

                    if (!DisconnectSession(s))
                        throw new exception("[message_server::requestLogin][Error] Nao conseguiu disconnectar o player[UID=" + (s.getUID())
                                + "OID=" + (s.m_oid) + ", IP=" + s.getIP() + "], ele pode esta com o bug do oid bloqueado, ou Session::UsaCtx bloqueado.",
                                ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.MESSAGE_SERVER, 3, 0x5200103));
                }

                //// Verifica com o Auth Server se o player est� connectado no server que ele diz e se � o mesmo IP ADDRESS

                // Loading Friend List
                _session.m_pi.m_friend_manager.init(_session.m_pi);

                //    Logado [Online]
                _session.m_pi.m_state = 4;

                //  Authorized a ficar online no server por tempo indeterminado
                _session.m_is_authorized = true;

                //Log
                message_pool.push(new message("[Login][Log] player[UID=" + (_session.m_pi.uid) + ", NICKNAME=" + (_session.m_pi.nickname) + "] logou com sucesso!", type_msg.CL_FILE_LOG_AND_CONSOLE));

                await DiscordWebhook.ChatLog($"🟢 Jogador {_session.m_pi.nickname} entrou no servidor.");

                // Resposta para o Pedido de Login
                p.init_plain(0x2F);

                p.AddByte(0);	// OK

                p.AddUInt32(_session.m_pi.uid);

                packet_func.session_send(p, _session, 1);

            }
            catch (exception e)
            {

                // Resposta
                p.init_plain(0x2F);

                p.AddByte(1);  // Error;

                packet_func.session_send(p, _session, 1);

                DisconnectSession(_session);

                message_pool.push(new message("[message_server::requestLogin][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
        }

        // public void ConfirmLoginOnOtherServer(Player Session, uint reqServerUid, AuthServerPlayerInfo aspi) { }

        public void requestFriendAndGuildMemberList(Player _session, packet _packet)
        {   //REQUEST_BEGIN("FriendAndGuildMemberList");

            packet p = new packet();

            try
            {

                message_pool.push(new message("[FriendList][Log] envia lista de amigos para o player[UID=" + (_session.m_pi.uid) + ", FRIENDS=" + _session.m_pi.m_friend_manager.getAllFriendAndGuildMember().Count + "].", type_msg.CL_FILE_LOG_AND_CONSOLE));

                // Verifica se Session est� varrizada para executar esse a��o, 
                // se ele n�o fez o login com o Server ele n�o pode fazer nada at� que ele fa�a o login
                //CHECK_SESSION_IS_AUTHORIZED("FriendAndGuildMemberList");

                var friend_list = _session.m_pi.m_friend_manager.getAllFriendAndGuildMember();

                var mp = new ManyPacket((ushort)friend_list.Count, FRIEND_PAG_LIMIT);

                // UPDATE ON GAME
                p.init_plain(0x30);

                p.AddUInt16(0x115);   // Sub packet Id

                p.AddUInt32(_session.m_pi.uid);
                p.AddUInt32(_session.m_pi.m_state);

                p.AddByte(1); // OK

                p.AddBuffer(_session.m_pi.m_cpi, Marshal.SizeOf(_session.m_pi.m_cpi));

                // Send To Player
                packet_func.session_send(p, _session, 1);

                FriendInfoEx pFi = null;

                // Resposta para Lista de Amigos e Membros da Guild
                if (mp.paginas > 0)
                {
                    for (var i = 0; i < mp.paginas; i++, mp.increse())
                    {
                        p.init_plain(0x30);

                        p.AddUInt16(0x102);   // Sub packet Id

                        p.AddBuffer(mp.pag, Marshal.SizeOf(mp.pag));

                        var begin = friend_list
                            .Skip(mp.index.start)  // Pula até o índice de início
                            .Take(mp.index.end - mp.index.start); // Pega apenas os elementos entre start e end

                        foreach (var fi in begin)
                        {
                            p.AddBuffer(fi, Marshal.SizeOf(new FriendInfo()));
                            var s = (Player)(m_session_manager.findSessionByUID((fi).uid) == null ? m_session_manager.findSessionByUID((fi).uid) : m_session_manager.FindSessionByNickname((fi).nickname));

                            // Se o Player tem ele na lista de amigos, e ele n�o estiver bloqueado na lista do amigo
                            if (s != null && (pFi = s.m_pi.m_friend_manager.findFriendInAllFriend(_session.m_pi.uid)) != null)
                            {   // Player est� online

                                p.AddBuffer(s.m_pi.m_cpi, Marshal.SizeOf(new ChannelPlayerInfo()));

                                // State Icon Player
                                p.AddByte(s.m_pi.m_state);

                                switch (s.m_pi.m_state)
                                {
                                    case 0: // IN GAME
                                        (fi).state.play = 1;
                                        break;
                                    case 1: // AFK
                                        (fi).state.AFK = 1;
                                        break;
                                    case 3: // BUSY
                                        (fi).state.busy = 1;
                                        break;
                                    case 4: // ON
                                    default:
                                        (fi).state.online = 1;
                                        break;
                                }

                               // Online
                               (fi).state.online = 1;

                            }
                            else
                            {   // player n�o est� online
                                p.AddInt16(-1);       // Sala Numero
                                p.AddInt32(-1);       // Sala Tipo
                                p.AddInt32(-1);       // Server GUID
                                p.AddByte(-1);        // Canal ID
                                p.AddZero(64);    // Canal Nome

                                // State Icon Player, OFFLINE not change icon
                                p.AddByte(5); // OFFLINE

                                // Offline
                                (fi).state.online = 0;
                            }

                            p.AddByte(fi.cUnknown_flag);

                            // Aqui quando � o player e ele est� guild � 1/*Master*/, 2 sub, e outros membro guild � 0, e quando � friend � o level
                            p.AddByte((byte)(fi.flag.ucFlag == 2/*S� Guild Member*/ ? (fi.uid == _session.m_pi.uid ? 1/*Master*/ : 0) : fi.level));

                            p.AddByte(fi.state.ucState);
                            p.AddByte(fi.flag.ucFlag);
                        }

                        packet_func.session_send(p, _session, 1);
                    }

                }
                else
                {

                    // N�o tem nenhum amigo, manda a p�gina vazia
                    p.init_plain(0x30);

                    p.AddUInt16(0x102);   // Sub packet Id

                    p.AddBuffer(mp.pag, Marshal.SizeOf(mp.pag));

                    packet_func.session_send(p, _session, 1);
                }

            }
            catch (exception e)
            {

                message_pool.push(new message("[channel::requestFriendAndGuildMemberList][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                p.init_plain(0x30);

                p.AddUInt16(0x102);   // Sub packet Id

                p.AddByte(1); // pagina

                p.AddUInt32(0);   // 0 Members

                packet_func.session_send(p, _session, 1);
            }

        }

        public void requestUpdateChannelPlayerInfo(Player _session, packet _packet)
        {   //REQUEST_BEGIN("UpdateChannelPlayerInfo");

            packet p = new packet();

            try
            {

                var cpi = _packet.Read<ChannelPlayerInfo>();

                _session.m_pi.m_cpi = cpi;

                // Verifica se Session est� varrizada para executar esse a��o, 
                // se ele n�o fez o login com o Server ele n�o pode fazer nada at� que ele fa�a o login
                //CHECK_SESSION_IS_AUTHORIZED("UpdateChannelPlayerInfo");

                message_pool.push(new message("[UpdateChannelPlayerInfo][Log] player[UID=" + (_session.m_pi.uid) + "] Atualizou Channel Info[NAME="
                        + (_session.m_pi.m_cpi.name) + ", ID=" + (_session.m_pi.m_cpi.id) + ", ROOM=" + (_session.m_pi.m_cpi.room.number)
                        + ", ROOM_TYPE=" + (_session.m_pi.m_cpi.room.type) + ", SERVER_UID=" + (_session.m_pi.m_cpi.server_uid) + "]", type_msg.CL_FILE_LOG_AND_CONSOLE));

                // UPDATE ON GAME
                p.init_plain(0x30);

                p.AddUInt16(0x115);   // Sub packet Id

                p.AddUInt32(_session.m_pi.uid);
                p.AddUInt32(_session.m_pi.m_state);

                p.AddByte(1); // OK

                p.AddBuffer(_session.m_pi.m_cpi, Marshal.SizeOf(_session.m_pi.m_cpi));

                // Send To Player
                packet_func.session_send(p, _session, 1);

                // Send To Player Friend(s)
                packet_func.friend_broadcast(m_player_manager.findAllFriend(_session.m_pi.m_friend_manager.getAllFriendAndGuildMember(true/*Not Send To Block Friend*/)), p, _session, 1);

            }
            catch (exception e)
            {

                message_pool.push(new message("[message_server::requestUpdateChannelPlayerInfo][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                p.init_plain(0x30);

                p.AddUInt16(0x115);   // Sub packet Id

                p.AddUInt32(_session.m_pi.uid);
                p.AddUInt32(_session.m_pi.m_state);

                p.AddByte(0); // Error(ACHO)

                packet_func.session_send(p, _session, 1);
            }
        }
        public void requestUpdatePlayerState(Player _session, packet _packet)
        {
            //REQUEST_BEGIN("UpdatePlayerState");

            packet p = new packet();

            try
            {

                var state = _packet.ReadUInt8();

                // Verifica se Session est� varrizada para executar esse a��o, 
                // se ele n�o fez o login com o Server ele n�o pode fazer nada at� que ele fa�a o login
                //CHECK_SESSION_IS_AUTHORIZED("UpdatePlayerState");

                // S� Atualiza se o state for diferente
                if (_session.m_pi.m_state != state)
                    _session.m_pi.m_state = state;

                // Update ON GAME - To player friend(s)
                p.init_plain(0x30);

                p.AddUInt16(0x115);   // Sub packet Id

                p.AddUInt32(_session.m_pi.uid);
                p.AddUInt32(_session.m_pi.m_state);

                p.AddByte(1); // OK

                p.AddBuffer(_session.m_pi.m_cpi, Marshal.SizeOf(_session.m_pi.m_cpi));

                // Send To Player Friend(s)
                packet_func.friend_broadcast(m_player_manager.findAllFriend(_session.m_pi.m_friend_manager.getAllFriendAndGuildMember(true/*Not Send To Block Friend*/)), p, _session, 1);
                switch ((USER_STATUS)state)
                {
                    case USER_STATUS.IS_PLAYING:
                        // Log
                        message_pool.push(new message("[MessengerServer::requestUpdatePlayerState][Log] player[UID=" + (_session.m_pi.uid) + "] PLAYING", type_msg.CL_FILE_LOG_AND_CONSOLE));
                        break;
                    case USER_STATUS.IS_RECONNECT:
                        // Log
                        message_pool.push(new message("[MessengerServer::requestUpdatePlayerState][Log] player[UID=" + (_session.m_pi.uid) + "] SLEEP", type_msg.CL_FILE_LOG_AND_CONSOLE));

                        break;
                    case USER_STATUS.IS_ONLINE:
                        // Log
                        message_pool.push(new message("[MessengerServer::requestUpdatePlayerState][Log] player[UID=" + (_session.m_pi.uid) + "] ONLINE", type_msg.CL_FILE_LOG_AND_CONSOLE));
                        break;
                    case USER_STATUS.IS_IDLE:

                        // Log
                        message_pool.push(new message("[MessengerServer::requestUpdatePlayerState][Log] player[UID=" + (_session.m_pi.uid) + "] BUSY", type_msg.CL_FILE_LOG_AND_CONSOLE));

                        break;
                    default:
                        break;
                }
            }
            catch (exception e)
            {

                message_pool.push(new message("[message_server::requestUpdatePlayerState][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
        }
        public void requestUpdatePlayerLogout(Player _session, packet _packet)
        {
            //REQUEST_BEGIN("UpdatePlayerLogout");

            try
            {

                // Verifica se Session est� varrizada para executar esse a��o, 
                // se ele n�o fez o login com o Server ele n�o pode fazer nada at� que ele fa�a o login
                //CHECK_SESSION_IS_AUTHORIZED("UpdatePlayerLogout");

                message_pool.push(new message("[PlayerLogout][Log] Player[UID=" + (_session.m_pi.uid) + "] deslogou-se", type_msg.CL_FILE_LOG_AND_CONSOLE));
                // Send Update Player Logout to your friends
                sendUpdatePlayerLogoutToFriends(_session);

            }
            catch (exception e)
            {

                message_pool.push(new message("[message_server::requestUpdatePlayerLogout][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

            }
        }

        public async void requestChatFriend(Player _session, packet _packet)
        {
            //REQUEST_BEGIN("ChatFriend");

            packet p = new packet();

            try
            {

                uint uid = _packet.ReadUInt32();
                var msg = _packet.ReadString();

                // Verifica se Session est� varrizada para executar esse a��o, 
                // se ele n�o fez o login com o Server ele n�o pode fazer nada at� que ele fa�a o login
                //CHECK_SESSION_IS_AUTHORIZED("ChatFriend");

                if (uid == 0)
                    throw new exception("[message_server::requestChatFriend][Error] player[UID=" + (_session.m_pi.uid) + "] tentou enviar Message[MSG="
                            + msg + "] para o Amigo[UID=" + (uid) + "], mas o uid is invalid(zero). Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.MESSAGE_SERVER, 1, 0x5200301));

                if (msg.empty())
                    throw new exception("[message_server::requestChatFriend][Error] player[UID=" + (_session.m_pi.uid) + "] tentou enviar Message[MSG="
                            + msg + "] para o Amigo[UID=" + (uid) + "], mas msg is empty. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.MESSAGE_SERVER, 2, 0x5200302));

                var pFi = _session.m_pi.m_friend_manager.findFriendInAllFriend(uid);

                if (pFi == null)
                    throw new exception("[message_server::requestChatFriend][Error] player[UID=" + (_session.m_pi.uid) + "] tentou enviar Message[MSG="
                            + msg + "] para o Amigo[UID=" + (uid) + "], mas player nao eh amigo dele. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.MESSAGE_SERVER, 3, 0x5200303));

                if (pFi.state.block == 1)
                    throw new exception("[message_server::requestChatFriend][Error] player[UID=" + (_session.m_pi.uid) + "] tentou enviar Message[MSG="
                            + msg + "] para o Amigo[UID=" + (uid) + "], mas o amigo esta bloqueado. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.MESSAGE_SERVER, 4, 0x5200304));

                var s = (Player)m_player_manager.findSessionByUID(uid);

                if (s == null)
                    throw new exception("[message_server::requestChatFriend][Error] player[UID=" + (_session.m_pi.uid) + "] tentou enviar Message[MSG="
                            + msg + "] para o Amigo[UID=" + (uid) + "], mas o Amigo nao esta online.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.MESSAGE_SERVER, 5, 0x5200305));

                pFi = s.m_pi.m_friend_manager.findFriendInAllFriend(_session.m_pi.uid);

                if (pFi == null)
                    throw new exception("[message_server::requestChatFriend][Error] player[UID=" + (_session.m_pi.uid) + "] tentou enviar Message[MSG="
                            + msg + "] para o Amigo[UID=" + (uid) + "], mas o amigo nao tem ele na lista de amigos. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.MESSAGE_SERVER, 6, 0x5200306));

                if (pFi.state.block == 1)
                    throw new exception("[message_server::requestChatFriend][Error] player[UID=" + (_session.m_pi.uid) + "] tentou enviar Message[MSG="
                            + msg + "] para o Amigo[UID=" + (uid) + "], mas amigo bloqueou ele. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.MESSAGE, 7, 0x5200307));

                // Log Para os GMs
                var gm = m_player_manager.findAllGM();

                if (!gm.empty())
                {

                    var msg_gm = "\\5" + (_session.m_pi.nickname) + ">" + (s.m_pi.nickname) + ": '" + msg + "'";

                    foreach (Player el in gm)
                    {

                        // Nao envia o log de MSN.PM novamente para o GM que enviou ou recebeu MSN.PM
                        if (el.m_pi.uid != _session.m_pi.uid && el.m_pi.uid != s.m_pi.uid)
                        {
                            // Responde no chat do player
                            p.init_plain(0x40);

                            p.AddByte(0);

                            p.AddString("\\1[MSN.PM]");   // Nickname

                            p.AddString(msg_gm);  // Message

                            packet_func.session_send(p, el, 1);
                        }
                    }
                    await DiscordWebhook.ChatLog("[MessengerServer::ChatFriend][Log] player[UID=" + (_session.m_pi.nickname) + "] enviou Message[MSG="
                        + msg + "] para seu Amigo[UID=" + (s.m_pi.nickname) + "]");
                }

                // Log
                message_pool.push(new message("[ChatFriend][Log] player[UID=" + (_session.m_pi.uid) + "] enviou Message[MSG="
                        + msg + "] para seu Amigo[UID=" + (s.m_pi.uid) + "]", type_msg.CL_FILE_LOG_AND_CONSOLE));

                // Resposta para send chat to friend
                p.init_plain(0x30);

                p.AddUInt16(0x113);   // Sub packet Id

                p.AddUInt32(_session.m_pi.uid);           // FROM
                p.AddString(_session.m_pi.nickname);  // FROM
                p.AddString(msg);

                p.AddByte(0); // Chat Friend

                packet_func.session_send(p, s, 1);      // TO

                // ------------------------------- Chat History Discord ------------------------------------
                // Envia a mensagem para o discord chat log se estiver ativado

                // Verifica se o m_chat_discod flag est� ativo para enviar o chat para o discord
                //if (m_si.rate.smart_calculator && m_chat_discord)
                //    sendMessageToDiscordChatHistory(
                //        "[MSN.PM]",                                                                                                     // From
                //        (_session.m_pi.nickname) + ">" + (s.m_pi.nickname) + ": '" + msg + "'"                      // Msg
                //    );

            }
            catch (exception e)
            {

                message_pool.push(new message("[message_server::requestChatFriend][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                p.init_plain(0x30);

                p.AddUInt16(0x113);   // Sub packet Id

                p.AddInt32(-1);   // Error

                packet_func.session_send(p, _session, 1);
            }
        }

        public void requestChatGuild(Player _session, packet _packet)
        {
            //REQUEST_BEGIN("ChatGuild");

            packet p = new packet();

            try
            {

                var msg = _packet.ReadString();

                // Verifica se Session est� varrizada para executar esse a��o, 
                // se ele n�o fez o login com o Server ele n�o pode fazer nada at� que ele fa�a o login
                //CHECK_SESSION_IS_AUTHORIZED("ChatGuild");

                if (_session.m_pi.guild_uid == 0)
                    throw new exception("[message_server::requestChatGuild][Error] player[UID=" + (_session.m_pi.uid) + "] tentou enviar Message[MSG="
                            + msg + "] para o Chat da Guild[UID=" + (_session.m_pi.guild_uid) + "], mas o player nao esta em uma guild. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.MESSAGE_SERVER, 1, 0x5200401));

                if (msg.empty())
                    throw new exception("[message_server::requestChatGuild][Error] player[UID=" + (_session.m_pi.uid) + "] tentou enviar Message[MSG="
                            + msg + "] para o Chat da Guild[UID=" + (_session.m_pi.guild_uid) + "], mas a msg is empty. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.MESSAGE_SERVER, 2, 0x5200402));

                // Log Para os GMs
                var gm = m_player_manager.findAllGM();

                if (!gm.empty())
                {

                    var guild_name = (_session.m_pi.guild_name);

                    var index = -1;

                    while ((index = guild_name.IndexOf(' ', (index != -1 ? index + 1 : 0))) != -1)
                        guild_name = guild_name.Remove(index, 1).Insert(index, " \\2");

                    var msg_gm = "[\\2" + guild_name + "\\0]\\5>" + (_session.m_pi.nickname) + ": '" + msg + "'";

                    foreach (Player el in gm)
                    {

                        // Nao envia o log de Club Chat novamente para o GM que enviou ou recebeu Club Chat
                        if (el.m_pi.uid != _session.m_pi.uid && el.m_pi.guild_uid != _session.m_pi.guild_uid)
                        {
                            // Responde no chat do player
                            p.init_plain(0x40);

                            p.AddByte(0);

                            p.AddString("\\1[CC]");   // Nickname

                            p.AddString(msg_gm);  // Message

                            packet_func.session_send(p, el, 1);
                        }
                    }
                }

                // Log

                message_pool.push(new message("[ChatGuild][Log] player[UID=" + (_session.m_pi.uid) + "] enviu Message[MSG=" + msg + "] no Chat da Guild[UID="
                        + (_session.m_pi.guild_uid) + "]", type_msg.CL_FILE_LOG_AND_CONSOLE));

                // Resposta para send chat to Guild
                p.init_plain(0x30);

                p.AddUInt16(0x113);   // Sub packet Id

                p.AddUInt32(_session.m_pi.uid);           // FROM
                p.AddString(_session.m_pi.nickname);  // FROM
                p.AddString(msg);

                p.AddByte(1); // Chat Guild

                packet_func.session_send(p, _session, 1);   // SEND TO PLAYER TOO

                // Usa o m_player_manager.findAllGuildMember, que pega todos os players que est�o na mesma guild
                packet_func.friend_broadcast(m_player_manager.findAllGuildMember(_session.m_pi.guild_uid), p, _session, 1); // All GUILD MEMBER
                                                                                                                            //packet_func.friend_broadcast(m_player_manager.findAllFriend(_session.m_pi.m_friend_manager.getAllGuildMember()), p, _session, 1);	// ALL GUILD MEMBER

                // ------------------------------- Chat History Discord ------------------------------------
                // Envia a mensagem para o discord chat log se estiver ativado

                // Verifica se o m_chat_discod flag est� ativo para enviar o chat para o discord
                //if (m_si.rate.smart_calculator && m_chat_discord)
                //    sendMessageToDiscordChatHistory(
                //        "[CC]",                                                                                                             // From
                //        "[" + (_session.m_pi.guild_name) + "]>" + (_session.m_pi.nickname) + ": '" + msg + "'"      // Msg
                //    );

            }
            catch (exception e)
            {

                message_pool.push(new message("[message_server::requestChatGuild][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                p.init_plain(0x30);

                p.AddUInt16(0x113);   // Sub packet Id

                p.AddInt32(-1);  // Error

                packet_func.session_send(p, _session, 1);
            }
        }

        public void requestCheckNickname(Player _session, packet _packet)
        {
            //REQUEST_BEGIN("CheckNickname");

            packet p = new packet();
            var nickname = "";
            try
            {

                nickname = _packet.ReadString();

                // Verifica se Session est� varrizada para executar esse a��o, 
                // se ele n�o fez o login com o Server ele n�o pode fazer nada at� que ele fa�a o login
                //CHECK_SESSION_IS_AUTHORIZED("CheckNickname");

                if (nickname.empty())
                    throw new exception("[message_server::requestCheckNickname][Error] player[UID=" + (_session.m_pi.uid) + "] tentou verificar o Nickname[value="
                            + nickname + "], mas o nickname is empty. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.MESSAGE_SERVER, 1, 0x5200501));

                var cmd_vn = new CmdVerifyNick(nickname);    // Waiter

                NormalManagerDB.add(0, cmd_vn, null, null);

                if (cmd_vn.getException().getCodeError() != 0)
                    throw cmd_vn.getException();

                if (!cmd_vn.getLastCheck())
                    throw new exception("[message_server::requestCheckNickname][Error] player[UID=" + (_session.m_pi.uid) + "] tentou verificar o Nickname[value="
                        + nickname + "], mas o nickname nao existe.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.MESSAGE_SERVER, 2, 1));

                // Log
                message_pool.push(new message("[CheckNickname][Log] player[UID=" + (_session.m_pi.uid) + "] pediu para verificar o Nickname[value=" + nickname + "]", type_msg.CL_FILE_LOG_AND_CONSOLE));


                // Resposta para Check Nickname
                p.init_plain(0x30);

                p.AddUInt16(0x117);   // Sub packet Id

                p.AddUInt32(0);   // OK

                p.AddString(nickname);
                p.AddUInt32(cmd_vn.getUID());

                packet_func.session_send(p, _session, 1);

            }
            catch (exception e)
            {

                message_pool.push(new message("[message_server::requestCheckNickname][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                p.init_plain(0x30);

                p.AddUInt16(0x117);   // Sub packet Id

                p.AddUInt32((ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) == (uint)STDA_ERROR_TYPE.MESSAGE_SERVER) ? ExceptionError.STDA_SYSTEM_ERROR_DECODE(e.getCodeError()) : 0x5200500);

                p.AddString(nickname);

                packet_func.session_send(p, _session, 1);
            }
        }
        public void requestAssignApelido(Player _session, packet _packet)
        {//REQUEST_BEGIN("AssingApelido");

            packet p = new packet();

            try
            {

                uint uid = _packet.ReadUInt32();
                var apelido = _packet.ReadString();

                // Verifica se Session est� varrizada para executar esse a��o, 
                // se ele n�o fez o login com o Server ele n�o pode fazer nada at� que ele fa�a o login
                //CHECK_SESSION_IS_AUTHORIZED("AssingApelido");

                if (uid == 0)
                    throw new exception("[message_server::requestAssingApelido][Error] player[UID=" + (_session.m_pi.uid) + "] tentou da um apelido para o Amigo[UID="
                            + (uid) + ", APELIDO=" + apelido + "], mas o uid is invalid(zero). Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.MESSAGE_SERVER, 1, 0x5200901));

                if (apelido.empty())
                    throw new exception("[message_server::requestAssingApelido][Error] player[UID=" + (_session.m_pi.uid) + "] tentou da um apelido para o Amigo[UID="
                            + (uid) + ", APELIDO=" + apelido + "], mas o apelido is empty. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.MESSAGE_SERVER, 2, 0x5200902));

                if (apelido.Count() >= 11)
                    throw new exception("[message_server::requestAssingApelido][Error] player[UID=" + (_session.m_pi.uid) + "] tentou da um apelido para o Amigo[UID="
                            + (uid) + ", APELIDO=" + apelido + "], mas o comprimento do apelido[max=11, request=" + (apelido.Count()) + "] eh invalido.",
                            ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.MESSAGE_SERVER, 3, 0x5200903));

                var pFi = _session.m_pi.m_friend_manager.findFriend(uid);

                if (pFi == null)
                    throw new exception("[message_server::requestAssingApelido][Error] player[UID=" + (_session.m_pi.uid) + "] tentou da um apelido para o Amigo[UID="
                            + (uid) + ", APELIDO=" + apelido + "], mas ele nao tem esse player como amigo. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.MESSAGE_SERVER, 4, 0x5200903));

                // UPDATE ON SERVER 
                pFi.apelido = apelido;

                // UPDATE ON DB
                _session.m_pi.m_friend_manager.requestUpdateFriendInfo(pFi);

                // Log
                message_pool.push(new message("[AssingApelido][Log] player[UID=" + (_session.m_pi.uid) + "] colocou apelido[VALUE="
                        + apelido + "] no Amigo[UID=" + (pFi.uid) + ", NICKNAME=" + (pFi.nickname) + "]", type_msg.CL_FILE_LOG_AND_CONSOLE));

                // Resposta para assing apelido
                p.init_plain(0x30);

                p.AddUInt16(0x119);   // Sub packet Id

                p.AddUInt32(0);   // OK

                p.AddUInt32(pFi.uid);
                p.AddString(pFi.apelido);

                packet_func.session_send(p, _session, 1);

            }
            catch (exception e)
            {

                message_pool.push(new message("[message_server::requestAssingApelido][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                p.init_plain(0x30);

                p.AddUInt16(0x119);   // Sub packet Id

                p.AddUInt32((ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) == (uint)STDA_ERROR_TYPE.MESSAGE_SERVER) ? ExceptionError.STDA_SYSTEM_ERROR_DECODE(e.getCodeError()) : 0x5200900);

                packet_func.session_send(p, _session, 1);
            }
        }

        public void requestBlockFriend(Player _session, packet _packet)
        {//REQUEST_BEGIN("BlockFriend");

            packet p = new packet();

            try
            {

                uint uid = _packet.ReadUInt32();

                // Verifica se Session est� varrizada para executar esse a��o, 
                // se ele n�o fez o login com o Server ele n�o pode fazer nada at� que ele fa�a o login
                //CHECK_SESSION_IS_AUTHORIZED("BlockFriend");

                if (uid == 0)
                    throw new exception("[message_server::requestBlockFriend][Error] player[UID=" + (_session.m_pi.uid) + "] tentou bloqueiar Amigo[UID="
                            + (uid) + "], mas o uid is invalid(zero). Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.MESSAGE_SERVER, 1, 0x5300101));

                var pFi = _session.m_pi.m_friend_manager.findFriend(uid);

                if (pFi == null)
                    throw new exception("[message_server::requestBlockFriend][Error] player[UID=" + (_session.m_pi.uid) + "] tentou bloqueiar Amigo[UID="
                        + (uid) + "], mas o player nao eh amigo dele. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.MESSAGE_SERVER, 2, 0x5300102));

                if (pFi.state.block == 1)
                    throw new exception("[message_server::requestBlockFriend][Error] player[UID=" + (_session.m_pi.uid) + "] tentou bloqueiar Amigo[UID="
                            + (uid) + "], mas o amigo ja esta bloqueado. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.MESSAGE_SERVER, 3, 0x5300103));

                var s = (Player)m_player_manager.findSessionByUID(uid);

                FriendInfoEx pFi2 = null;

                if (s != null)
                {   // Player est� online

                    if ((pFi2 = s.m_pi.m_friend_manager.findFriend(_session.m_pi.uid)) == null)
                        throw new exception("[message_server::requestBlockFriend][Error] player[UID=" + (_session.m_pi.uid) + "] tentou bloqueiar Amigo[UID="
                                + (uid) + "], mas o amigo nao tem ele na lista de amigos. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.MESSAGE_SERVER, 4, 0x5300104));

                    // Amigo
                    pFi.state.block = 1;

                    // UPDATE ON DB
                    _session.m_pi.m_friend_manager.requestUpdateFriendInfo(pFi);    // REQUEST

                    // Log
                    message_pool.push(new message("[BlockFriend][Log] player[UID=" + (_session.m_pi.uid) + "] bloqueou o Amigo[UID="
                            + (s.m_pi.uid) + ", NICKNAME=" + (s.m_pi.nickname) + "]", type_msg.CL_FILE_LOG_AND_CONSOLE));

                    // Resposta para o block friend REQUEST
                    p.init_plain(0x30);

                    p.AddUInt16(0x10C);   // Sub packet Id

                    p.AddUInt32(0);   // OK

                    p.AddUInt32(s.m_pi.uid);

                    packet_func.session_send(p, _session, 1);

                    // Resposta para o block friend REQUESTED, Envia que o player deslogou
                    p.init_plain(0x30);

                    p.AddUInt16(0x10F);   // Sub packet Id

                    p.AddUInt32(_session.m_pi.uid);

                    packet_func.session_send(p, s, 1);

                }
                else
                {

                    var cmd_pi = new CmdPlayerInfo(uid);    // Waiter

                    NormalManagerDB.add(0, cmd_pi, null, null);

                    if (cmd_pi.getException().getCodeError() != 0)
                        throw cmd_pi.getException();

                    var pi = cmd_pi.getInfo();

                    if (pi.uid == 0)
                        throw new exception("[message_server::requestBlockFriend][Error] player[UID=" + (_session.m_pi.uid) + "] tentou bloqueiar Amigo[UID="
                                + (uid) + "], mas player nao existe. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.MESSAGE_SERVER, 5, 0x5300105));

                    var fm = new FriendManager(pi);

                    fm.init(pi);

                    if (!fm.isInitialized())
                        throw new exception("[message_server::requestBlockFriend][Error] player[UID=" + (_session.m_pi.uid) + "] tentou bloqueiar Amigo[UID="
                                + (uid) + "], nao conseguiu inicializar Friend Manager do amigo. Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.MESSAGE_SERVER, 6, 0x5300106));

                    if ((pFi2 = fm.findFriend(_session.m_pi.uid)) == null)
                        throw new exception("[message_server::requestBlockFriend][Error] player[UID=" + (_session.m_pi.uid) + "] tentou bloqueiar Amigo[UID="
                                + (uid) + "], mas o amigo nao tem ele na lista de amigos. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.MESSAGE_SERVER, 4, 0x5300104));

                    // Amigo
                    pFi.state.block = 1;

                    // UPDATE ON DB
                    _session.m_pi.m_friend_manager.requestUpdateFriendInfo(pFi);    // REQUEST

                    // Log
                    message_pool.push(new message("[BlockFriend][Log] player[UID=" + (_session.m_pi.uid) + "] bloqueou o Amigo[UID="
                            + (pi.uid) + ", NICKNAME=" + (pi.nickname) + "]", type_msg.CL_FILE_LOG_AND_CONSOLE));

                    // Resposta para o block friend REQUEST
                    p.init_plain(0x30);

                    p.AddUInt16(0x10C);   // Sub packet Id

                    p.AddUInt32(0);   // OK

                    p.AddUInt32(pi.uid);

                    packet_func.session_send(p, _session, 1);
                }

            }
            catch (exception e)
            {

                message_pool.push(new message("[message_server::requestBlockFriend][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                p.init_plain(0x30);

                p.AddUInt16(0x10C);   // Sub packet Id

                p.AddUInt32((ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) == (uint)STDA_ERROR_TYPE.MESSAGE_SERVER) ? ExceptionError.STDA_SYSTEM_ERROR_DECODE(e.getCodeError()) : 0x5300100);

                packet_func.session_send(p, _session, 1);
            }
        }
        public void requestUnblockFriend(Player _session, packet _packet)
        {   //REQUEST_BEGIN("UnblockFriend");

            packet p = new packet();

            try
            {

                uint uid = _packet.ReadUInt32();

                // Verifica se Session est� varrizada para executar esse a��o, 
                // se ele n�o fez o login com o Server ele n�o pode fazer nada at� que ele fa�a o login
                //CHECK_SESSION_IS_AUTHORIZED("UnblockFriend");

                if (uid == 0)
                    throw new exception("[message_server::requestUnBlockFriend][Error] player[UID=" + (_session.m_pi.uid) + "] tentou desbloquear Amigo[UID="
                            + (uid) + "], mas uid is invalid(zero). Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.MESSAGE_SERVER, 1, 0x5300201));

                var pFi = _session.m_pi.m_friend_manager.findFriend(uid);

                if (pFi == null)
                    throw new exception("[message_server::requestUnBlockFriend][Error] player[UID=" + (_session.m_pi.uid) + "] tentou desbloquear Amigo[UID="
                            + (uid) + "], mas o player nao eh amigo dele. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.MESSAGE_SERVER, 2, 0x5300202));

                if (!(pFi.state.block == 1))
                    throw new exception("[message_server::requestUnBlockFriend][Error] player[UID=" + (_session.m_pi.uid) + "] tentou desbloquear Amigo[UID="
                            + (uid) + "], mas o amigo ja esta desbloqueado. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.MESSAGE_SERVER, 3, 0x5300203));

                var s = (Player)m_player_manager.findSessionByUID(uid);

                FriendInfoEx pFi2 = null;

                if (s != null)
                {   // Player est� online

                    if ((pFi2 = s.m_pi.m_friend_manager.findFriend(_session.m_pi.uid)) == null)
                        throw new exception("[message_server::requestUnBlockFriend][Error] player[UID=" + (_session.m_pi.uid) + "] tentou desbloquear Amigo[UID="
                                + (uid) + "], mas o amigo nao tem ele na lista de amigos. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.MESSAGE_SERVER, 4, 0x5200204));

                    // Amigo
                    pFi.state.block = 0;

                    // UPDATE ON DB
                    _session.m_pi.m_friend_manager.requestUpdateFriendInfo(pFi);    // REQUEST

                    // Log
                    message_pool.push(new message("[UnBlockFriend][Log] player[UID=" + (_session.m_pi.uid) + "] desbloqueou o Amigo[UID="
                            + (s.m_pi.uid) + ", NICKNAME=" + (s.m_pi.nickname) + "]", type_msg.CL_FILE_LOG_AND_CONSOLE));

                    // Resposta para o unblock friend REQUEST
                    p.init_plain(0x30);

                    p.AddUInt16(0x10D);   // Sub packet Id

                    p.AddUInt32(0);   // OK

                    p.AddUInt32(s.m_pi.uid);

                    packet_func.session_send(p, _session, 1);

                    // Resposta para o unblock friend REQUESTED - Passa Pacote que ele esta online
                    p.init_plain(0x30);

                    p.AddUInt16(0x115);   // Sub packet Id

                    p.AddUInt32(_session.m_pi.uid);
                    p.AddUInt32(_session.m_pi.m_state);

                    p.AddByte(1); // OK

                    p.AddBuffer(_session.m_pi.m_cpi, Marshal.SizeOf(_session.m_pi.m_cpi));

                    packet_func.session_send(p, s, 1);

                }
                else
                {

                    var cmd_pi = new CmdPlayerInfo(uid);    // Waiter

                    NormalManagerDB.add(0, cmd_pi, null, null);

                    if (cmd_pi.getException().getCodeError() != 0)
                        throw cmd_pi.getException();

                    var pi = cmd_pi.getInfo();

                    if (pi.uid == 0)
                        throw new exception("[message_server::requestUnBlockFriend][Error] player[UID=" + (_session.m_pi.uid) + "] tentou desbloquear Amigo[UID="
                                + (uid) + "], mas o player nao existe. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.MESSAGE_SERVER, 5, 0x5300205));

                    var fm = new FriendManager(pi);

                    fm.init(pi);

                    if (!fm.isInitialized())
                        throw new exception("[message_server::requestUnBlockFriend][Error] player[UID=" + (_session.m_pi.uid) + "] tentou desbloquear Amigo[UID="
                                + (uid) + "], mas nao conseguiu inicializar Friend Manager do amigo. Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.MESSAGE_SERVER, 6, 0x5300206));

                    if ((pFi2 = fm.findFriend(_session.m_pi.uid)) == null)
                        throw new exception("[message_server::requestUnBlockFriend][Error] player[UID=" + (_session.m_pi.uid) + "] tentou desbloquear Amigo[UID="
                                + (uid) + "], mas o amigo nao tem ele na lista de amigos. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.MESSAGE_SERVER, 4, 0x5300204));

                    // Amigo
                    pFi.state.block = 0;

                    // UPDATE ON DB
                    _session.m_pi.m_friend_manager.requestUpdateFriendInfo(pFi);    // REQUEST

                    // Log
                    message_pool.push(new message("[UnBlockFriend][Log] player[UID=" + (_session.m_pi.uid) + "] desbloqueou o Amigo[UID="
                            + (pi.uid) + ", NICKNAME=" + (pi.nickname) + "]", type_msg.CL_FILE_LOG_AND_CONSOLE));

                    // Resposta para o unblock friend REQUEST
                    p.init_plain(0x30);

                    p.AddUInt16(0x10D);   // Sub packet Id

                    p.AddUInt32(0);   // OK

                    p.AddUInt32(pi.uid);

                    packet_func.session_send(p, _session, 1);
                }
            }
            catch (exception e)
            {

                message_pool.push(new message("[message_server::requestUnblockFriend][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                p.init_plain(0x30);

                p.AddUInt16(0x10D);   // Sub packet Id

                p.AddUInt32((ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) == (uint)STDA_ERROR_TYPE.MESSAGE_SERVER) ? ExceptionError.STDA_SYSTEM_ERROR_DECODE(e.getCodeError()) : 0x5300200);

                packet_func.session_send(p, _session, 1);
            }
        }
        public void requestAddFriend(Player _session, packet _packet)
        {
            //REQUEST_BEGIN("AddFriend");

            packet p = new packet();

            try
            {

                uint uid = _packet.ReadUInt32();
                var nickname = _packet.ReadString();

                // Verifica se Session est� varrizada para executar esse a��o, 
                // se ele n�o fez o login com o Server ele n�o pode fazer nada at� que ele fa�a o login
                //CHECK_SESSION_IS_AUTHORIZED("AddFriend");

                if (uid == 0)
                    throw new exception("[message_server::requestAddFriend][Error] player[UID=" + (_session.m_pi.uid) + "] tentou add Friend[UID="
                            + (uid) + ", NICKNAME=" + nickname + "], mas o uid is invalid(zero). Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.MESSAGE_SERVER, 1, 0x5200601));

                if (nickname.empty())
                    throw new exception("[message_server::requestAddFriend][Error] player[UID=" + (_session.m_pi.uid) + "] tentou add Friend[UID="
                            + (uid) + ", NICKNAME=" + nickname + "], mas o nickname is empty. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.MESSAGE_SERVER, 2, 0x5200602));

                var pFi = _session.m_pi.m_friend_manager.findFriendInAllFriend(uid);

                if (pFi != null && pFi.flag._friend == 1)
                    throw new exception("[message_server::requestAddFriend][Error] player[UID=" + (_session.m_pi.uid) + "] tentou add Friend[UID="
                        + (uid) + ", NICKNAME=" + nickname + "], mas o player ja eh amigo dele.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.MESSAGE_SERVER, 3, 2));

                if (_session.m_pi.m_friend_manager.countFriend() >= FRIEND_LIST_LIMIT)
                    throw new exception("[message_server::requestAddFriend][Error] player[UID=" + (_session.m_pi.uid) + "] tentou add Friend[UID="
                            + (uid) + ", NICKNAME=" + nickname + "], mas ele esta com a lista de amigos cheia[LIMIT=" + (FRIEND_LIST_LIMIT) + "].", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.MESSAGE_SERVER, 4, 0x5200603));

                var s = (Player)m_player_manager.findSessionByUID(uid);

                FriendInfoEx fi = new FriendInfoEx(), fi2 = new FriendInfoEx();

                if (s != null)
                {   // Player est� connectado

                    if (string.Compare(nickname, s.m_pi.nickname) != 0)
                        throw new exception("[message_server::requestAddFriend][Error] player[UID=" + (_session.m_pi.uid) + "] tentou add Friend[UID="
                                + (uid) + ", NICKNAME=" + nickname + "], mas o nickname nao bate. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.MESSAGE, 7, 0x5200607));

                    if (s.m_pi.m_friend_manager.countFriend() >= FRIEND_LIST_LIMIT)
                        throw new exception("[message_server::requestAddFriend][Error] player[UID=" + (_session.m_pi.uid) + "] tentou add Friend[UID="
                                + (uid) + ", NICKNAME=" + nickname + "], mas o amigo esta com a lista full[LIMIT=" + (FRIEND_LIST_LIMIT) + "]", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.MESSAGE_SERVER, 5, 3));

                    // Friend to add
                    fi.uid = s.m_pi.uid;
                    fi.flag.ucFlag = (byte)((pFi == null) ? 1 : pFi.flag.ucFlag | 1);   // Friend

                    fi.apelido = "Friend";
                    fi.nickname = s.m_pi.nickname;
                    fi.state.online = 1;
                    fi.state.request_friend = 1;
                    fi.state.sex = s.m_pi.sex;

                    fi.level = (byte)s.m_pi.level;

                    // Friend that has add
                    fi2.uid = _session.m_pi.uid;
                    fi2.flag.ucFlag = (byte)((pFi == null) ? 1 : pFi.flag.ucFlag | 1);   // Friend
                    fi2.apelido = "Friend";
                    fi2.nickname = _session.m_pi.nickname;


                    fi2.state.online = 1;
                    fi2.state.sex = _session.m_pi.sex;

                    fi2.level = (byte)_session.m_pi.level;

                    // UPDATE ON SERVER AND DB
                    _session.m_pi.m_friend_manager.requestAddFriend(fi);    // Add On Player Request
                    s.m_pi.m_friend_manager.requestAddFriend(fi2);          // Add On Player Requested

                    // Log
                    message_pool.push(new message("[AddFriend][Log] player[UID=" + (_session.m_pi.uid) + "] add Amigo[UID=" + (s.m_pi.uid) + "]", type_msg.CL_FILE_LOG_AND_CONSOLE));

                    // Resposta para o add Friend
                    p.init_plain(0x30);

                    p.AddUInt16(0x104);   // Sub packet Id

                    p.AddUInt32(0);   // OK

                    p.AddBuffer(fi, Marshal.SizeOf(new FriendInfo()));

                    p.AddBuffer(s.m_pi.m_cpi, Marshal.SizeOf(new ChannelPlayerInfo()));

                    // State Icon Player
                    p.AddByte(s.m_pi.m_state);

                    p.AddByte(fi.cUnknown_flag);
                    p.AddByte(fi.level);
                    p.AddByte(fi.state.ucState);
                    p.AddByte(fi.flag.ucFlag);

                    packet_func.session_send(p, _session, 1);

                    // Resposta para o player que foi adicionado
                    p.init_plain(0x30);

                    p.AddUInt16(0x106);   // Sub packet Id

                    p.AddBuffer(fi2, Marshal.SizeOf(new FriendInfo()));
                    p.AddBuffer(_session.m_pi.m_cpi, Marshal.SizeOf(new ChannelPlayerInfo()));

                    // State Icon Player
                    p.AddByte(_session.m_pi.m_state);

                    p.AddByte(fi2.cUnknown_flag);
                    p.AddByte(fi2.level);
                    p.AddByte(fi2.state.ucState);
                    p.AddByte(fi2.flag.ucFlag);

                    packet_func.session_send(p, s, 1);

                }
                else
                {

                    var cmd_pi = new CmdPlayerInfo(uid);    // Waiter

                    NormalManagerDB.add(0, cmd_pi, null, null);

                    if (cmd_pi.getException().getCodeError() != 0)
                        throw cmd_pi.getException();

                    var pi = cmd_pi.getInfo();

                    if (pi.uid == 0)
                        throw new exception("[message_server::requestAddFriend][Error] player[UID=" + (_session.m_pi.uid) + "] tentou add Friend[UID="
                                + (uid) + ", NICKNAME=" + nickname + "], mas o player nao existe.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.MESSAGE_SERVER, 6, 0x5200606));

                    if (string.Compare(nickname, pi.nickname) != 0)
                        throw new exception("[message_server::requestAddFriend][Error] player[UID=" + (_session.m_pi.uid) + "] tentou add Friend[UID="
                           + (uid) + ", NICKNAME=" + nickname + "], mas o nickname nao bate. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.MESSAGE, 7, 0x5200607));

                    var fm = new FriendManager(pi);

                    fm.init(pi);

                    if (!fm.isInitialized())
                        throw new exception("[message_server::requestAddFriend][Error] player[UID=" + (_session.m_pi.uid) + "] tentou add Friend[UID="
                                + (uid) + ", NICKNAME=" + nickname + "], mas nao conseguiu inicializar o FriendManager do Amigo.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.MESSAGE_SERVER, 8, 0x5200607));

                    if (fm.countFriend() >= FRIEND_LIST_LIMIT)
                        throw new exception("[message_server::requestAddFriend][Error] player[UID=" + (_session.m_pi.uid) + "] tentou add Friend[UID="
                            + (uid) + ", NICKNAME=" + nickname + "], mas o amigo esta com a lista full[LIMIT=" + (FRIEND_LIST_LIMIT) + "]", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.MESSAGE_SERVER, 5, 3));

                    // Friend to add
                    fi.uid = pi.uid;
                    fi.flag.ucFlag = (byte)((pFi == null) ? 1 : pFi.flag.ucFlag | 1);   // Friend
                    fi.apelido = "Friend";
                    fi.nickname = pi.nickname;

                    fi.state.online = 1;
                    fi.state.request_friend = 1;
                    fi.state.sex = pi.sex;

                    fi.level = (byte)pi.level;

                    // Friend that has add
                    fi2.uid = _session.m_pi.uid;
                    fi2.flag.ucFlag = (byte)((pFi == null) ? 1 : pFi.flag.ucFlag | 1);   // Friend
                    fi2.apelido = "Friend";
                    fi2.nickname = _session.m_pi.nickname;

                    fi2.state.online = 1;
                    fi2.state.sex = _session.m_pi.sex;

                    fi2.level = (byte)_session.m_pi.level;

                    // UPDATE ON SERVER AND DB
                    _session.m_pi.m_friend_manager.requestAddFriend(fi);    // Add On Player Request
                    fm.requestAddFriend(fi2);                               // Add On Player Requested

                    // Log
                    message_pool.push(new message("[AddFriend][Log] player[UID=" + (_session.m_pi.uid) + "] add Amigo[UID=" + (pi.uid) + "]", type_msg.CL_FILE_LOG_AND_CONSOLE));

                    // Resposta para o add Friend
                    p.init_plain(0x30);

                    p.AddUInt16(0x104);   // Sub packet Id

                    p.AddUInt32(0);   // OK

                    p.AddBuffer(fi, Marshal.SizeOf(new FriendInfo()));
                    p.AddInt16(-1);       // Sala N�mero
                    p.AddInt32(-1);       // Sala Tipo
                    p.AddInt32(-1);       // Server GUID
                    p.AddByte(-1);        // Canal ID
                    p.AddZero(64);    // Canal Nome

                    // State Icon Player
                    p.AddByte(5); // OFFLINE

                    fi.state.online = 0;    // Offline

                    p.AddByte(fi.cUnknown_flag);
                    p.AddByte(fi.level);
                    p.AddByte(fi.state.ucState);
                    p.AddByte(fi.flag.ucFlag);

                    packet_func.session_send(p, _session, 1);
                }

            }
            catch (exception e)
            {

                message_pool.push(new message("[message_server::requestAddFriend][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                p.init_plain(0x30);

                p.AddUInt16(0x104);   // Sub packet Id

                p.AddUInt32((ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) == (uint)STDA_ERROR_TYPE.MESSAGE_SERVER) ? ExceptionError.STDA_SYSTEM_ERROR_DECODE(e.getCodeError()) : 0x5200600);

                packet_func.session_send(p, _session, 1);
            }
        }
        public void requestConfirmFriend(Player _session, packet _packet)
        {//REQUEST_BEGIN("ConfirmFriend");

            packet p = new packet();

            try
            {

                uint uid = _packet.ReadUInt32();

                // Verifica se Session est� varrizada para executar esse a��o, 
                // se ele n�o fez o login com o Server ele n�o pode fazer nada at� que ele fa�a o login
                //CHECK_SESSION_IS_AUTHORIZED("ConfirmFriend");

                if (uid == 0)
                    throw new exception("[message_server::requestConfirmFriend][Error] player[UID=" + (_session.m_pi.uid) + "] tentou aceitar Amigo[UID="
                            + (uid) + "], mas o uid is invalid(zero). Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.MESSAGE_SERVER, 1, 0x5200801));

                var pFi = _session.m_pi.m_friend_manager.findFriend(uid);

                if (pFi == null)
                    throw new exception("[message_server::requestConfirmFriend][Error] player[UID=" + (_session.m_pi.uid) + "] tentou aceitar Amigo[UID="
                            + (uid) + "], mas o player nao eh amigo dele. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.MESSAGE_SERVER, 2, 0x5200802));

                if (pFi.state.request_friend.IsTrue())
                    throw new exception("[message_server::requestConfirmFriend][Error] player[UID=" + (_session.m_pi.uid) + "] tentou aceitar Amigo[UID="
                            + (uid) + "], mas ele nao pode aceitar um amigo, que ele mesmo enviou pedido de amizade. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.MESSAGE_SERVER, 3, 0x5200803));

                if (pFi.state._friend.IsTrue())
                    throw new exception("[message_server::requestConfirmFriend][Error] player[UID=" + (_session.m_pi.uid) + "] tentou aceitar Amigo[UID="
                            + (uid) + "], mas o player ja eh seu amigo. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.MESSAGE_SERVER, 4, 0x5200804));

                var s = (Player)m_player_manager.findSessionByUID(uid);

                FriendInfoEx pFi2 = null;

                if (s != null)
                {   // Player est� online

                    if ((pFi2 = s.m_pi.m_friend_manager.findFriend(_session.m_pi.uid)) == null)
                        throw new exception("[message_server::requestConfirmFriend][Error] player[UID=" + (_session.m_pi.uid) + "] tentou aceitar Amigo[UID="
                                + (uid) + "], mas o player nao esta na lista do amigo que ele vai aceitar. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.MESSAGE_SERVER, 5, 0x5200804));

                    // Amigo
                    pFi.state._friend = 1;

                    // Amigo
                    pFi2.state.request_friend = 0;
                    pFi2.state._friend = 1;

                    // UPDATE ON SERVER AND DB
                    _session.m_pi.m_friend_manager.requestUpdateFriendInfo(pFi);    // REQUEST
                    s.m_pi.m_friend_manager.requestUpdateFriendInfo(pFi2);      // REQUESTED

                    // Log
                    message_pool.push(new message("[ConfirmFriend][Log] player[UID=" + (_session.m_pi.uid) + "] aceitou Amigo[UID="
                            + (s.m_pi.uid) + ", NICKNAME=" + (s.m_pi.nickname) + "]", type_msg.CL_FILE_LOG_AND_CONSOLE));

                    // Resposta para o confirm friend REQUEST
                    p.init_plain(0x30);

                    p.AddUInt16(0x109);   // Sub packet Id

                    p.AddUInt32(0);   // OK

                    p.AddUInt32(s.m_pi.uid);

                    packet_func.session_send(p, _session, 1);

                    // Resposta para o confirm friend REQUESTED
                    p.init_plain(0x30);

                    p.AddUInt16(0x10A);   // Sub packet Id

                    p.AddUInt32(0);   // OK

                    p.AddUInt32(_session.m_pi.uid);

                    packet_func.session_send(p, s, 1);

                }
                else
                {

                    var cmd_pi = new CmdPlayerInfo(uid);    // Waiter

                    NormalManagerDB.add(0, cmd_pi, null, null);



                    if (cmd_pi.getException().getCodeError() != 0)
                        throw cmd_pi.getException();

                    var pi = cmd_pi.getInfo();

                    if (pi.uid == 0)
                        throw new exception("[message_server::requestConfirmFriend][Error] player[UID=" + (_session.m_pi.uid) + "] tentou aceitar Amigo[UID="
                                + (uid) + "], mas o player nao existe. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.MESSAGE_SERVER, 6, 0x5200806));

                    var fm = new FriendManager(pi);

                    fm.init(pi);

                    if (!fm.isInitialized())
                        throw new exception("[message_server::requestConfirmFriend][Error] player[UID=" + (_session.m_pi.uid) + "] tentou aceitar Amigo[UID="
                                + (uid) + "], mas nao conseguiu incializar o Friend Manager do amigo. Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.MESSAGE_SERVER, 7, 0x5200807));

                    if ((pFi2 = fm.findFriend(_session.m_pi.uid)) == null)
                        throw new exception("[message_server::requestConfirmFriend][Error] player[UID=" + (_session.m_pi.uid) + "] tentou aceitar Amigo[UID="
                                + (uid) + "], mas o player nao esta na lista do amigo que ele vai aceitar. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.MESSAGE_SERVER, 5, 0x5200805));

                    // Amigo
                    pFi.state._friend = 1;

                    // Amigo
                    pFi2.state.request_friend = 0;
                    pFi2.state._friend = 1;


                    // UPDATE ON SERVER AND DB
                    _session.m_pi.m_friend_manager.requestUpdateFriendInfo(pFi);    // REQUEST
                    fm.requestUpdateFriendInfo(pFi2);                               // REQUESTED

                    // Log
                    message_pool.push(new message("[ConfirmFriend][Log] player[UID=" + (_session.m_pi.uid) + "] aceitou Amigo[UID="
                            + (pi.uid) + ", NICKNAME=" + (pi.nickname) + "]", type_msg.CL_FILE_LOG_AND_CONSOLE));

                    // Resposta para o confirm friend REQUEST
                    p.init_plain(0x30);

                    p.AddUInt16(0x109);   // Sub packet Id

                    p.AddUInt32(0);   // OK

                    p.AddUInt32(pi.uid);

                    packet_func.session_send(p, _session, 1);
                }

            }
            catch (exception e)
            {

                message_pool.push(new message("[message_server::requestConfirmFriend][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                p.init_plain(0x30);

                p.AddUInt16(0x109);   // Sub packet Id

                p.AddUInt32((ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) == (uint)STDA_ERROR_TYPE.MESSAGE_SERVER) ? ExceptionError.STDA_SYSTEM_ERROR_DECODE(e.getCodeError()) : 0x5200800);

                packet_func.session_send(p, _session, 1);
            }
        }


        public void requestDeleteFriend(Player _session, packet _packet)
        { //REQUEST_BEGIN("DeleteFriend");

            packet p = new packet();

            try
            {

                uint uid = _packet.ReadUInt32();
                var nickname = _packet.ReadString();

                // Verifica se Session est� varrizada para executar esse a��o, 
                // se ele n�o fez o login com o Server ele n�o pode fazer nada at� que ele fa�a o login
                //CHECK_SESSION_IS_AUTHORIZED("DeleteFriend");

                if (uid == 0)
                    throw new exception("[message_server::requestDeleteFriend][Error] player[UID=" + (_session.m_pi.uid) + "] tentou deletar Amigo[UID="
                            + (uid) + ", NICKNAME=" + nickname + "], mas o uid is invalid(zero). Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.MESSAGE_SERVER, 1, 0x5200701));

                if (nickname.empty())
                    throw new exception("[message_server::requestDeleteFriend][Error] player[UID=" + (_session.m_pi.uid) + "] tentou deletar Amigo[UID="
                            + (uid) + ", NICKNAME=" + nickname + "], mas nickname is empty. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.MESSAGE_SERVER, 2, 0x5200702));

                var pFi = _session.m_pi.m_friend_manager.findFriend(uid);

                if (pFi == null)
                    throw new exception("[message_server::requestDeleteFriend][Error] player[UID=" + (_session.m_pi.uid) + "] tentou deletar Amigo[UID="
                            + (uid) + ", NICKNAME=" + nickname + "], mas o player nao eh amigo dele. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.MESSAGE_SERVER, 3, 0x5200703));

                var s = (Player)m_player_manager.findSessionByUID(uid);

                FriendInfoEx pFi2 = null;

                if (s != null)
                {   // Player est� online

                    if (string.Compare(nickname, s.m_pi.nickname) != 0)
                        throw new exception("[message_server::requestDeleteFriend][Error] player[UID=" + (_session.m_pi.uid) + "] tentou deletar Amigo[UID="
                                + (uid) + ", NICKNAME=" + nickname + "], mas o nickname nao bate. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.MESSAGE, 6, 0x5200705));

                    if ((pFi2 = s.m_pi.m_friend_manager.findFriend(_session.m_pi.uid)) == null)
                        throw new exception("[message_server::requestDeleteFriend][Error] player[UID=" + (_session.m_pi.uid) + "] tentou deletar Amigo[UID="
                                + (uid) + ", NICKNAME=" + nickname + "], mas o amigo nao tem ele na lista de amigos. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.MESSAGE, 4, 0x5200704));

                    // UPDATE ON SERVER ON DB
                    _session.m_pi.m_friend_manager.requestDeleteFriend(pFi);    // REQUEST
                    s.m_pi.m_friend_manager.requestDeleteFriend(pFi2);      // REQUESTED

                    // Log
                    message_pool.push(new message("[DeleteFriend][Log] player[UID=" + (_session.m_pi.uid) + "] deletou Amigo[UID="
                            + (s.m_pi.uid) + ", NICKNAME=" + nickname + "]", type_msg.CL_FILE_LOG_AND_CONSOLE));

                    // Respsta para o delete friend TO REQUEST
                    p.init_plain(0x30);

                    p.AddUInt16(0x10B);   // Sub packet Id

                    p.AddUInt32(0);   // OK

                    p.AddUInt32(s.m_pi.uid);

                    packet_func.session_send(p, _session, 1);

                    // Resposta para o delete friend TO REQUESTED
                    p.init_plain(0x30);

                    p.AddUInt16(0x10B);   // Sub packet Id

                    p.AddUInt32(0);   // OK

                    p.AddUInt32(_session.m_pi.uid);

                    packet_func.session_send(p, s, 1);

                }
                else
                {

                    var cmd_pi = new CmdPlayerInfo(uid);    // Waiter

                    NormalManagerDB.add(0, cmd_pi, null, null);



                    if (cmd_pi.getException().getCodeError() != 0)
                        throw cmd_pi.getException();

                    var pi = cmd_pi.getInfo();

                    if (pi.uid == 0)
                        throw new exception("[message_server::requestDeleteFriend][Error] player[UID=" + (_session.m_pi.uid) + "] tentou deletar Amigo[UID="
                                + (uid) + ", NICKNAME=" + nickname + "], mas o player nao existe. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.MESSAGE_SERVER, 5, 0x5200705));

                    if (string.Compare(nickname, pi.nickname) != 0)
                        throw new exception("[message_server::requestDeleteFriend][Error] player[UID=" + (_session.m_pi.uid) + "] tentou deletar Amigo[UID="
                                + (uid) + ", NICKNAME=" + nickname + "], mas o nickname nao bate. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.MESSAGE_SERVER, 6, 0x5200706));

                    var fm = new FriendManager(pi);

                    fm.init(pi);

                    if (!fm.isInitialized())
                        throw new exception("[message_server::requestDeleteFriend][Error] player[UID=" + (_session.m_pi.uid) + "] tentou deletar Amigo[UID="
                                + (uid) + ", NICKNAME=" + nickname + "], mas nao conseguiu incializar o Friend Manager do Amigo. Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.MESSAGE_SERVER, 7, 0x5200707));

                    if ((pFi2 = fm.findFriend(_session.m_pi.uid)) == null)
                        throw new exception("[message_server::requestDeleteFriend][Error] player[UID=" + (_session.m_pi.uid) + "] tentou deletar Amigo[UID="
                                + (uid) + ", NICKNAME=" + nickname + "], mas o amigo nao tem ele na lista de amigos. Hacker ou Bug.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.MESSAGE, 8, 0x5200708));

                    // UPDATE ON SERVER ON DB
                    _session.m_pi.m_friend_manager.requestDeleteFriend(pFi);    // REQUEST
                    fm.requestDeleteFriend(pFi2);                               // REQUESTED

                    // Log
                    message_pool.push(new message("[DeleteFriend][Log] player[UID=" + (_session.m_pi.uid) + "] deletou Amigo[UID="
                            + (pi.uid) + ", NICKNAME=" + nickname + "]", type_msg.CL_FILE_LOG_AND_CONSOLE));

                    // Respsta para o delete friend TO REQUEST
                    p.init_plain(0x30);

                    p.AddUInt16(0x10B);   // Sub packet Id

                    p.AddUInt32(0);   // OK

                    p.AddUInt32(pi.uid);

                    packet_func.session_send(p, _session, 1);
                }

            }
            catch (exception e)
            {

                message_pool.push(new message("[message_server::requestDeleteFriend][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                p.init_plain(0x30);

                p.AddUInt16(0x10B);   // Sub packet Id

                p.AddUInt32((ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) == (uint)STDA_ERROR_TYPE.MESSAGE_SERVER) ? ExceptionError.STDA_SYSTEM_ERROR_DECODE(e.getCodeError()) : 0x5200700);

                packet_func.session_send(p, _session, 1);
            }
        }

        public void requestNotifyPlayerWasInvitedToRoom(Player _session, packet _packet)
        {
            //REQUEST_BEGIN("NotifyPlayerWasInvitedToRoom");

            try
            {

                // Verifica se Session est� varrizada para executar esse a��o, 
                // se ele n�o fez o login com o Server ele n�o pode fazer nada at� que ele fa�a o login
                //CHECK_SESSION_IS_AUTHORIZED("NotifyPlayerWasInvitedToRoom");

                uint player_invited_uid = _packet.ReadUInt32();

                if (player_invited_uid != _session.m_pi.uid)
                    throw new exception("[message_server::requestNotityPlayerWasInvitedToRoom][Error] Player[UID=" + (_session.m_pi.uid)
                            + "] que foi convidado passou um Player[UID=" + (player_invited_uid)
                            + "] com uid que nao eh o dele. Hacker ou Bug.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.MESSAGE_SERVER, 3749, 0));

                // Log
                message_pool.push(new message("[message_server::requestNotityPlayerWasInvitedToRoom][Log] Player[UID=" + (_session.m_pi.uid)
                        + "] foi convidado para um sala no jogo.", type_msg.CL_FILE_LOG_AND_CONSOLE));

            }
            catch (exception e)
            {

                message_pool.push(new message("[message_server::requestNotifyPlayerWasInvitedToRoom][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
        }
        public void requestInvitePlayerToGuildBattleRoom(Player _session, packet _packet)
        {
            try
            {

                // Verifica se Session est� varrizada para executar esse a��o, 
                // se ele n�o fez o login com o Server ele n�o pode fazer nada at� que ele fa�a o login
                //CHECK_SESSION_IS_AUTHORIZED("InvitPlayerToGuildBattleRoom");

                uint server_uid = _packet.ReadUInt32();
                byte channel_id = _packet.ReadUInt8();
                ushort room_numero = _packet.ReadUInt16();

                uint player_invite_uid = _packet.ReadUInt32();
                var player_invite_nickname = _packet.ReadString();

                uint player_invited_uid = _packet.ReadUInt32();

                if (player_invite_uid != _session.m_pi.uid)
                    throw new exception("[message_server::requestInvitPlayerToGuildBattleRoom][Error] Player[UID=" + (_session.m_pi.uid)
                            + "] nao bate com o Player[UID=" + (player_invite_uid) + "] que fez o request para convidar o player[UID="
                            + (player_invited_uid) + "] para a sala de Guild Battle. Hacker ou Bug.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.MESSAGE_SERVER, 3750, 0));

                // Log
                message_pool.push(new message("[message_server::requestInvitPlayerToGuildBattleRoom][Log] Player[UID="
                        + (_session.m_pi.uid) + ", NICKNAME=" + player_invite_nickname + "] convidou o Player[UID="
                        + (player_invited_uid) + "] no Server[UID=" + (server_uid) + ", CHANNEL_ID="
                        + (channel_id) + ", ROOM=" + (room_numero) + "] para Guild Battle.", type_msg.CL_FILE_LOG_AND_CONSOLE));

            }
            catch (exception e)
            {
                message_pool.push(new message("[message_server::requestInvitPlayerToGuildBattleRoom][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
        }

        public void requestAcceptGuildMember(packet packet) { }
        public void requestMemberExitedFromGuild(packet packet) { }
        public void requestKickGuildMember(packet packet) { }

        //// Auth Server Commands
        //public override void AuthCmdShutdown(int timeSec) { }
        //public override void AuthCmdBroadcastNotice(string notice) { }
        //public override void AuthCmdBroadcastTicker(string nickname, string msg) { }
        //public override void AuthCmdBroadcastCubeWinRare(string msg, uint option) { }
        //public override void AuthCmdDisconnectPlayer(uint reqServerUid, uint playerUid, byte force) { }
        //public override void AuthCmdConfirmDisconnectPlayer(uint playerUid) { }
        //public override void AuthCmdNewMailArrivedMailBox(uint playerUid, uint mailId) { }
        //public override void AuthCmdNewRate(uint tipo, uint qntd) { }
        //public override void AuthCmdReloadGlobalSystem(uint tipo) { }
        //public override void AuthCmdConfirmSendInfoPlayerOnline(uint reqServerUid, AuthServerPlayerInfo aspi) { }

        public override void SQLDBResponse(int msg_id, Pangya_DB pangya_db, object _arg) { }

        //protected override void ShutdownTime(int timeSec) { }

        protected bool sendUpdatePlayerLogoutToFriends(PangyaAPI.Network.PangyaSession.Session _session)
        {
            bool ret = true;
            var p = new packet();
            try
            {

                // J� enviou os pacote de update de logou do player
                // Se estiver 0 troca para 1, e retorno o que estava e compara, s� sai se for 1
                // Já enviou os pacotes de update de logout do player
                // Se estiver 0 troca para 1, e retorna o que estava e compara, só sai se for 1
                if (Interlocked.CompareExchange(ref ((Player)_session).m_pi.m_logout, 1, 0) == 1)
                    return false;

                // Resposta para os amigos do player, que ele deslogou
                p.init_plain(0x30);

                p.AddUInt16(0x10F); // Sub packet Id

                p.AddUInt32(((Player)_session).m_pi.uid);

                packet_func.friend_broadcast(m_player_manager.findAllFriend(((Player)_session).m_pi.m_friend_manager.getAllFriendAndGuildMember(true/*Not Send To Block Friend*/)), p, _session, 1);

            }
            catch (exception e)
            {

                message_pool.push(new message("[message_server::sendUpdatePlayerLogoutToFriends][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                // Error
                ret = false;
            }

            return ret;
        }
        public override void config_init()
        {
            base.config_init();

            // Tipo Server
            m_si.tipo = 3;


            // Recupera Valores de rate do server do banco de dados
            var cmd_rci = new CmdRateConfigInfo(m_si.uid);  // Waiter

            NormalManagerDB.add(0, cmd_rci, null, null);

            if (cmd_rci.getException().getCodeError() != 0 || cmd_rci.isError()/*Deu erro na consulta n�o tinha o rate config info para esse server, pode ser novo*/)
            {

                if (cmd_rci.getException().getCodeError() != 0)
                    message_pool.push(new message("[message_server::config_init][ErrorSystem] " + cmd_rci.getException().getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                message_pool.push(new message("[message_server::config_init][Error] nao conseguiu recuperar os valores de rate do server[UID="
                        + (m_si.uid) + "] no banco de dados. Utilizando valores padroes de rates.", type_msg.CL_FILE_LOG_AND_CONSOLE));

                m_si.rate.scratchy = 100;
                m_si.rate.papel_shop_rare_item = 100;
                m_si.rate.papel_shop_cookie_item = 100;
                m_si.rate.treasure = 100;
                m_si.rate.memorial_shop = 100;
                m_si.rate.chuva = 100;
                m_si.rate.grand_zodiac_event_time = 1; // Ativo por padr�o
                m_si.rate.grand_prix_event = 1;        // Ativo por padr�o
                m_si.rate.golden_time_event = 1;       // Ativo por padr�o
                m_si.rate.login_reward_event = 1;      // Ativo por padr�o
                m_si.rate.bot_gm_event = 1;            // Ativo por padr�o
                m_si.rate.smart_calculator = 0;        // Atibo por padr�o

                m_si.rate.angel_event = 0;             // Desativado por padr�o
                m_si.rate.pang = 0;
                m_si.rate.exp = 0;
                m_si.rate.club_mastery = 0;

                // Atualiza no banco de dados
                NormalManagerDB.add(2, new CmdUpdateRateConfigInfo(m_si.uid, m_si.rate), SQLDBResponse, this);

            }
            else
            {   // Conseguiu recuperar com sucesso os valores do server

                m_si.rate.scratchy = cmd_rci.getInfo().scratchy;
                m_si.rate.papel_shop_rare_item = cmd_rci.getInfo().papel_shop_rare_item;
                m_si.rate.papel_shop_cookie_item = cmd_rci.getInfo().papel_shop_cookie_item;
                m_si.rate.treasure = cmd_rci.getInfo().treasure;
                m_si.rate.memorial_shop = cmd_rci.getInfo().memorial_shop;
                m_si.rate.chuva = cmd_rci.getInfo().chuva;
                m_si.rate.grand_zodiac_event_time = cmd_rci.getInfo().grand_zodiac_event_time;
                m_si.rate.grand_prix_event = cmd_rci.getInfo().grand_prix_event;
                m_si.rate.golden_time_event = cmd_rci.getInfo().golden_time_event;
                m_si.rate.login_reward_event = cmd_rci.getInfo().login_reward_event;
                m_si.rate.bot_gm_event = cmd_rci.getInfo().bot_gm_event;
                m_si.rate.smart_calculator = cmd_rci.getInfo().smart_calculator;

                m_si.rate.angel_event = cmd_rci.getInfo().angel_event;
                m_si.rate.pang = cmd_rci.getInfo().pang;
                m_si.rate.exp = cmd_rci.getInfo().exp;
                m_si.rate.club_mastery = cmd_rci.getInfo().club_mastery;
            }
        }
        protected virtual void ReloadFiles() { }

        protected virtual void ReloadSystems() { }
        protected virtual void ReloadGlobalSystem(uint tipo) { }

        protected virtual void UpdateRateAndEvent(uint tipo, uint qntd) { }

        public override void authCmdShutdown(int _time_sec)
        {

        }

        public override void authCmdBroadcastNotice(string _notice)
        {
            // Message Server n�o usa esse Comando
            return;
        }

        public override void authCmdBroadcastTicker(string _nickname, string _msg)
        {
            // Message Server n�o usa esse Comando
            return;
        }

        public override void authCmdBroadcastCubeWinRare(string _msg, uint _option)
        {
            // Message Server n�o usa esse Comando
            return;
        }

        public override void authCmdDisconnectPlayer(uint _req_server_uid, uint _player_uid, byte _force)
        {
            try
            {

                var s = m_player_manager.findPlayer(_player_uid);

                if (s != null)
                {

                    // Log
                    message_pool.push(new message("[message_server::authCmdDisconnectPlayer][log] Comando do Auth Server, Server[UID=" + (_req_server_uid)
                            + "] pediu para desconectar o Player[UID=" + (s.m_pi.uid) + "]", type_msg.CL_FILE_LOG_AND_CONSOLE));

                    // Deconecta o Player
                    DisconnectSession(s);

                    // UPDATE ON Auth Server
                    //m_unit_connect.sendConfirmDisconnectPlayer(_req_server_uid, _player_uid);

                }
                else
                    message_pool.push(new message("[message_server::authCmdDisconnectPlayer][WARNING] Comando do Auth Server, Server[UID=" + (_req_server_uid)
                            + "] pediu para desconectar o Player[UID=" + (_player_uid) + "], mas nao encontrou ele no server.", type_msg.CL_FILE_LOG_AND_CONSOLE));

            }
            catch (exception e)
            {

                message_pool.push(new message("[message_server::authCmdDisconnectPlayer][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
        }

        public override void authCmdConfirmDisconnectPlayer(uint _player_uid)
        {
            // Message Server n�o usa esse Comando
            return;
        }

        public override void authCmdNewMailArrivedMailBox(uint _player_uid, uint _mail_id)
        {
            // Message Server n�o usa esse Comando
            return;
        }

        public override void authCmdNewRate(uint _tipo, uint _qntd)
        {
            // Message Server n�o usa esse Comando
            return;
        }

        public override void authCmdReloadGlobalSystem(uint _tipo)
        {
            // Message Server n�o usa esse Comando
            return;
        }

        public override void authCmdInfoPlayerOnline(uint _req_server_uid, uint _player_uid)
        {
            // Message Server n�o usa esse Comando
            return;
        }

        public override void authCmdConfirmSendInfoPlayerOnline(uint _req_server_uid, AuthServerPlayerInfo _aspi)
        {

        }

        public override void authCmdSendCommandToOtherServer(packet _packet)
        {

        }

        public override void authCmdSendReplyToOtherServer(packet _packet)
        {

        }

        public override void sendCommandToOtherServerWithAuthServer(packet _packet, uint _send_server_uid_or_type)
        {

        }

        public override void sendReplyToOtherServerWithAuthServer(packet _packet, uint _send_server_uid_or_type)
        {

        }

        public override bool CheckCommand(string commandLine)
        {
            throw new NotImplementedException();
        }

    }
}

// Server Static 
namespace sms
{
    public class ms : Singleton<MessengerServer.MessengerServerTcp.MessengerServer>
    {
    }
}