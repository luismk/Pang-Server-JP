using GameServer.PangType;
using PangyaAPI.Utilities;
using System.Collections.Generic;
using System.Linq;
using packet = PangyaAPI.TCP.PangyaPacket.Packet;
using packet_func = GameServer.PacketFunc.packet_func;
using static GameServer.PangType._Define;
using System;
using _smp = PangyaAPI.Utilities.Log;
using GameServer.Session;

namespace GameServer.Game
{
    public class Channel : Ex.ChannelBase
    {
        public Channel(ChannelInfo _ci, uint _type) : base(_ci, (int)_type)
        {
        }

        protected void addInviteTimeRequest(InviteChannelInfo _ici) { }
        protected void deleteInviteTimeRequest(InviteChannelInfo _ici) { }
        protected void deleteInviteTimeResquestByInvited(Player _session) { }

        // Tira o request do convidado da sala[Character] o tempo acabou para ele responder ao convite
        protected bool send_time_out_invite(InviteChannelInfo _ici) { return true; }
        void clear_invite_time() { }

        void removeSession(Player _session)
        {
            if (_session == null)
                throw new exception("[channel::removeSession][Error] _session is nullptr.");

            int index = -1;
            if ((index = findIndexSession(_session)) == -1)
            {
                throw new exception("[channel::removeSession][Error] _session not exists on vector sessions.");
            }

            v_sessions.Remove(_session);

            m_ci.curr_user--;

            // reseta(default) o channel que o player está no player info
            _session.m_pi.channel = byte.MaxValue;
            _session.m_pi.place = 0;

            deletePlayerInfo(_session);

        }
        void addSession(Player _session)
        {
            if (_session == null)
                throw new exception("[channel::addSession][Error] _session is null or invalid.");

            v_sessions.Add(_session);

            m_ci.curr_user++;

            // Channel id
            _session.m_pi.channel = m_ci.id;
            _session.m_pi.place = 0;

            // Calcula a condição do player e o sexo
            // Só faz calculo de Quita rate depois que o player
            // estiver no level Beginner E e jogado 50 games
            if (_session.m_pi.level >= 6 && _session.m_pi.ui.jogado >= 50)
            {
                float rate = _session.m_pi.ui.getQuitRate();

                if (rate < GOOD_PLAYER_ICON)
                    _session.m_pi.mi.state_flag.stFlagBit.azinha = 1;
                else if (rate >= QUITER_ICON_1 && rate < QUITER_ICON_2)
                    _session.m_pi.mi.state_flag.stFlagBit.quiter_1 = 1;
                else if (rate >= QUITER_ICON_2)
                    _session.m_pi.mi.state_flag.stFlagBit.quiter_2 = 1;
            }

            if (_session.m_pi.ei.char_info != null && _session.m_pi.ui.getQuitRate() < GOOD_PLAYER_ICON)
                _session.m_pi.mi.state_flag.stFlagBit.icon_angel = 0;/*_session.m_pi.ei.char_info.AngelEquiped();*/
            else
                _session.m_pi.mi.state_flag.stFlagBit.icon_angel = 0;

            _session.m_pi.mi.state_flag.stFlagBit.sexo = _session.m_pi.mi.sexo;



            makePlayerInfo(_session);
        }

        protected Player findSessionByOID(int _oid)
        {
            return m_player_info.Keys.FirstOrDefault(c => c.m_oid == _oid);
        }
        protected Player findSessionByUID(int _uid)
        {
            return m_player_info.Keys.FirstOrDefault(c => c.getUID() == _uid);
        }
        protected Player findSessionByNickname(string _nickname)
        {
            return m_player_info.Keys.FirstOrDefault(c => c.getNickname() == _nickname);
        }

        protected int findIndexSession(Player _session)
        {
            if (_session == null)
                throw new exception("[channel::findIndexSession][Error] _session is null.");

            for (var i = 0; i < v_sessions.Count(); ++i)
                if (v_sessions[i] == _session)
                    return i;

            return -1;
        }
        protected void makePlayerInfo(Player _session)
        {
            PlayerCanalInfoEx pci = new PlayerCanalInfoEx
            {
                // Player Canal Info Init
                uid = _session.m_pi.uid,
                oid = _session.m_oid,
                sala_numero = _session.m_pi.mi.sala_numero,
                level = (byte)_session.m_pi.level,
                capability = _session.m_pi.m_cap,
                nickname = "@" + _session.m_pi.nickname,

                title = _session.m_pi.ue.m_title,
                team_point = 1000,
                flag_visible_gm = 0
            };
            // Só faz calculo de Quita rate depois que o player
            // estiver no level Beginner E e jogado 50 games
            if (_session.m_pi.level >= 6 && _session.m_pi.ui.jogado >= 50)
            {
                float rate = _session.m_pi.ui.getQuitRate();

                if (rate < GOOD_PLAYER_ICON)
                {
                    pci.state_flag.stBit.azinha = 0;
                }
                else if (rate >= QUITER_ICON_1 && rate < QUITER_ICON_2)
                    pci.state_flag.stBit.quiter_1 = 1;
                else if (rate >= QUITER_ICON_2)
                    pci.state_flag.stBit.quiter_2 = 1;
            }

            if (_session.m_pi.ei.char_info != null && _session.m_pi.ui.getQuitRate() < GOOD_PLAYER_ICON)
                pci.state_flag.stBit.icon_angel = 0;
            else
                pci.state_flag.stBit.icon_angel = 0;

            pci.state_flag.stBit.sexo = _session.m_pi.mi.sexo;

            pci.guid_uid = _session.m_pi.gi.uid;

            if (m_player_info.GetValues(_session, true) == null)
            {
                m_player_info.Add(_session, pci);
            }
            // Update Player Location
            _session.m_pi.updateLocationDB();
        }
        protected void updatePlayerInfo(Player _session)
        {
            PlayerCanalInfoEx pci, _pci = new PlayerCanalInfoEx();

            if ((_pci = getPlayerInfo(_session)) == null)
                throw new exception("[channel::updatePlayerInfo][Error] nao tem o player[UID=" + (_session.m_pi.uid)
                    + "] info dessa session no canal.");
            // Copia do que esta no map
            pci = _pci;

            // Player Canal Info Update
            pci.uid = _session.m_pi.uid;
            pci.oid = _session.m_oid;
            pci.sala_numero = _session.m_pi.mi.sala_numero;

            pci.level = (byte)_session.m_pi.level;

            pci.team_point = 1000;
            pci.flag_visible_gm = 0;
            pci.capability = _session.m_pi.m_cap;
            pci.title = _session.m_pi.ue.m_title;
            // Só faz calculo de Quita rate depois que o player
            // estiver no level Beginner E e jogado 50 games
            if (_session.m_pi.level >= 6 && _session.m_pi.ui.jogado >= 50)
            {
                float rate = _session.m_pi.ui.getQuitRate();

                if (rate < GOOD_PLAYER_ICON)
                {
                    pci.state_flag.stBit.azinha = 1;
                }
                else if (rate >= QUITER_ICON_1 && rate < QUITER_ICON_2)
                    pci.state_flag.stBit.quiter_1 = 1;
                else if (rate >= QUITER_ICON_2)
                    pci.state_flag.stBit.quiter_2 = 1;
            }

            if (_session.m_pi.ei.char_info != null && _session.m_pi.ui.getQuitRate() < GOOD_PLAYER_ICON)
                pci.state_flag.stBit.icon_angel = 0;
            else
                pci.state_flag.stBit.icon_angel = 0;

            pci.state_flag.stBit.sexo = _session.m_pi.mi.sexo;

            // Salva novamente
            _pci = pci;

            // Update Location Player
            _session.m_pi.updateLocationDB();
        }
        protected void deletePlayerInfo(Player _session)
        {// Update Location player
            _session.m_pi.updateLocationDB();

            // Delete Player Info of session(player)
            m_player_info.Remove(_session);
        }

        // Tourney Tempo que pode entrar no tourney depois de ter começado acabou troca o info da sala
        // Arg1 Channel ponteiro, Arg2 Numero da Sala
        protected int _enter_left_time_is_over(object _arg1, object _arg2)
        {
            Channel c = (Channel)_arg1;
            short numero = (short)_arg2;

            try
            {

                if (c == null)
                    throw new exception("[channel::_enter_left_time_is_over][Error] Channel[ID=-1] Sala[NUMERO=" + (numero)
                        + "] channel ponteiro fornecido pelo argumento is invalid.");

                if (numero < 0)
                    throw new exception("[channel::_enter_left_time_is_over][Error] Channel[ID=" + c.getId()
                        + "] Sala[NUMERO=" + (numero) + "] numero da sala fornecido pelo argumento is invalid");

                BEGIN_FIND_ROOM_C(numero);

                //if (r == null)
                //    throw new exception("[channel::_enter_left_time_is_over][Error] Channel[ID=" + (c->getId())
                //        + "] Sala[NUMERO=" + (numero) + "] nao encontrou a sala no canal", STDA_MAKE_ERROR(STDA_ERROR_TYPE::CHANNEL, 1202, 0));

                //r->setState(0);
                //r->setFlag(0);

                //// Limpa no Game o Timer
                //r->requestEndAfterEnter();

                packet p;

                //             // Update Room ON LOBBY
                //             if (packet_func.pacote047(p, List<RoomInfo> { (RoomInfo)r.getInfo() }, 3))
                //packet_func.channel_broadcast(c, p, 1);

            }
            catch (exception e)
            {

            }
            return 0;
        }

        void BEGIN_FIND_ROOM_C(short numero)
        {
            //construir o objeto e pegar o room 
        }

        void END_FIND_ROOM_C()
        {
            //destruir o objeto e limpa(room)
        }


        public void enterChannel(Player _session)
        {
            //if (!_session.getState())
            //    throw new exception("[channel::enterChannel][Error] player nao esta conectado.");

            if (_session.m_pi.channel != -1)
                throw new exception("[channel::enterChannel][Error] player[UID=" + (_session.m_pi.uid)
                    + "] ja esta conectado em outro canal.");

            addSession(_session);

            packet p = new packet();

            packet_func.pacote095(ref p, _session, 0x102);
            packet_func.session_send(ref p, _session, 0); // Não sei direito desse aqui mas passa antes de entrar no canal, talvez é o que faz o cliente pedir MSN gs acho

            packet_func.pacote04E(ref p, _session, 1);
            packet_func.session_send(ref p, _session, 0);

            //// Verifica se o tempo do ticket premium user acabou e manda a mensagem para o player, e exclui o ticket do player no SERVER, DB e GAME
            //sPremiumSystem::getInstance().checkEndTimeTicket(_session);
        }
        public void leaveChannel(Player _session)
        {
            //!@ As vezes o player sai antes e não tem mais como deletar ele do canal
            //if (!_session.getState())
            //throw exception("Error player não conectar. Em channel::leaveChannel()", STDA_MAKE_ERROR(STDA_ERROR_TYPE::CHANNEL, 1, 0));

            try
            {

                if (_session.m_pi.lobby != (byte)0)
                    leaveLobby(_session);       // Sai da Lobby

                else // Sai da Sala Practice que não entra na lobby, [SINGLE PLAY]
                     //leaveRoom(_session, 0);

                    removeSession(_session);

            }
            catch (exception e)
            {

                removeSession(_session);

               // _smp.message_pool.push("[channel::leaveChannel][Error] " + e.getFullMessageError());


            }
        }

        public void checkEnterChannel(Player _session)
        {
            //if (!_session.getState())
            //    throw new exception("[channel::checkEnterChannel][Error] player nao esta conectado.");

            // Não é GM verifica se o player pode entrar nesse canal
            if (!Convert.ToBoolean(_session.m_pi.m_cap.stBit.game_master))
            {

                //if (_session.m_pi.level < 0 || _session.m_pi.level > 70)
                //    throw new exception("[channel::checkEnterChannel][Error] Player[UID=" + (_session.m_pi.uid) + ", LEVEL=" + (_session.m_pi.level)
                //        + "] nao tem o level necessario para entrar no canal[ID=" + (m_ci.id) + ", MIN=" + (0)
                //        + ", MAX=" + (70) + "].");

                //if (m_ci.flag.stBit.only_rookie && Convert.ToBoolean(_session.m_pi.level > enLEVEL.ROOKIE_A))
                //    throw new exception("[channel::checkEnterChannel][Error] Player[UID=" + (_session.m_pi.uid) + ", LEVEL=" + (_session.m_pi.level)
                //        + "] nao tem o level necessario para entrar no canal[ID=" + (m_ci.id) + ", MIN=" + (0)
                //        + ", MAX=" + (70) + "] com a flag So Rookie.");

                //if (m_ci.flag.stBit.junior_bellow && _session.m_pi.level > enLEVEL::JUNIOR_A)
                //    throw new exception("[channel::checkEnterChannel][Error] Player[UID=" + (_session.m_pi.uid) + ", LEVEL=" + (_session.m_pi.level)
                //        + "] nao tem o level necessario para entrar no canal[ID=" + (m_ci.id) + ", MIN=" + (0)
                //        + ", MAX=" + (70) + "] com a flag Junior A pra baixo.");

                //if (m_ci.flag.stBit.junior_above && _session.m_pi.level < enLEVEL.JUNIOR_E)
                //    throw new exception("[channel::checkEnterChannel][Error] Player[UID=" + (_session.m_pi.uid) + ", LEVEL=" + (_session.m_pi.level)
                //        + "] nao tem o level necessario para entrar no canal[ID=" + (m_ci.id) + ", MIN=" + (0)
                //        + ", MAX=" + (70) + "] com a flag Junior E pra cima.");

                //if (m_ci.flag.stBit.junior_between_senior && (_session.m_pi.level < enLEVEL.JUNIOR_E || _session.m_pi.level > enLEVEL.SENIOR_A))
                //    throw new exception("[channel::checkEnterChannel][Error] Player[UID=" + (_session.m_pi.uid) + ", LEVEL=" + (_session.m_pi.level)
                //        + "] nao tem o level necessario para entrar no canal[ID=" + (m_ci.id) + ", MIN=" + (0)
                //        + ", MAX=" + (70) + "] com a flag junior E a Senior A.");

                //if (m_ci.flag.stBit.beginner_between_junior && (_session.m_pi.level < enLEVEL.BEGINNER_E || _session.m_pi.level > enLEVEL.JUNIOR_A))
                //    throw new exception("[channel::checkEnterChannel][Error] Player[UID=" + (_session.m_pi.uid) + ", LEVEL=" + (_session.m_pi.level)
                //        + "] nao tem o level necessario para entrar no canal[ID=" + (m_ci.id) + ", MIN=" + (0)
                //        + ", MAX=" + (70) + "] com a flag Beginner E a Junior A.");
            }
        }

        public ChannelInfo getInfo() { return m_ci; }

        // Gets
        public byte getId() { return (byte)m_ci.id; }

        protected PlayerCanalInfoEx getPlayerInfo(Player _session)
        {
            if (_session == null)
                throw new exception("[channel::getPlayerInfo][Error] _session is null.");

            PlayerCanalInfoEx pci = null;


            pci = m_player_info.GetValues(_session).First();

            return pci;
        }

        // Check Invite Time
        void checkInviteTime() { }

        // stats
        public bool isFull()
        {
            bool ret = false;

            ret = m_ci.curr_user >= m_ci.max_user;
            return ret;
        }

        // Lobby
        void enterLobby(Player _session, byte _lobby)
        {
            //if (!_session.get())
            //    throw new exception("[channel.enterLobby][Error] player[UID_TRASH=" + (_session.m_pi.uid)
            //        + "] nao esta conectado.");

            if (_session.m_pi.lobby != ~0)
                throw new exception("[channel.enterLobby][Error] player[UID=" + (_session.m_pi.uid)
                    + "] ja esta na lobby.");

            _session.m_pi.lobby = (byte)((_lobby == 0 || _lobby == 0) ? 1/*Padrão*/ : _lobby);
            _session.m_pi.place = 0;

            updatePlayerInfo(_session);

            packet p = new packet();

            List<PlayerCanalInfo> v_pci = new List<PlayerCanalInfo>();
            PlayerCanalInfo pci = null;

            //std.vector<RoomInfo> v_ri = m_rm.getRoomsInfo();

            var v_sessions = getSessions(_session.m_pi.lobby);

            for (int i = 0; i < v_sessions.Count; ++i)
            {
                if ((pci = getPlayerInfo(v_sessions[i])) != null)
                    v_pci.Add(pci);
            }

            pci = getPlayerInfo(_session);

            // Add o primeiro limpando a lobby
            packet_func.pacote046(ref p, _session, v_pci, 4);
            packet_func.session_send(ref p, _session, 0);

            if (v_pci.Count() > 0)
            {
                packet_func.pacote046(ref p, _session, v_pci, 5);
                packet_func.session_send(ref p, _session, 0);
            }
            //if (packet_func.pacote047(p, v_ri, 0))
            //    packet_func.session_send(ref p, _session, 0);

            packet_func.pacote046(ref p, _session, pci == null ? new vector<PlayerCanalInfo>() : new vector<PlayerCanalInfo>(pci), 1);
            packet_func.channel_broadcast(this, ref p, 0);

            v_pci.Clear();
        }
        void leaveLobby(Player _session)
        {/// !@tem que tira isso aqui por que tem que enviar para os outros player da lobby que ele sai,
         /// mesmo que o sock dele não pode mais enviar
            //if (!_session.getState())
            //throw exception("[channel::leaveLobby][Error] player nao esta conectado.", STDA_MAKE_ERROR(STDA_ERROR_TYPE::CHANNEL, 1, 0));

            // Sai da sala se estiver em uma sala
            //try
            //{
            //    //leaveRoom(_session, 0);
            //}
            //catch (exception&e) {

            //    _smp::message_pool::getInstance().push(new message("[channel::leaveLobby][Error] " + e.getFullMessageError(), CL_FILE_LOG_AND_CONSOLE));
            //}

            _session.m_pi.lobby = 0;
            _session.m_pi.place = 0;

            updatePlayerInfo(_session);

            sendUpdatePlayerInfo(_session, 2);
        }

        // Lobby Multi player
        void enterLobbyMultiPlayer(Player _session)
        {
            try
            {

                // Enter Lobby
                enterLobby(_session, 1/*Multi player*/);

                packet p = new packet(0xF5);
                packet_func.session_send(ref p, _session, 0);

            }
            catch (exception e)
            {

            }
        }
        void leaveLobbyMultiPlayer(Player _session)
        {

            try
            {

                // leave Lobby
                leaveLobby(_session);

                packet p = new packet(0xF6);
                packet_func.session_send(ref p, _session, 0);

            }
            catch (exception e)
            {

            }
        }

        // Lobby Grand Prix
        void enterLobbyGrandPrix(Player _session) { }
        void leaveLobbyGrandPrix(Player _session) { }

        //// Room
        //LEAVE_ROOM_STATE leaveRoom(Player _session, int _option) { }

        //// Room Lobby Multiplayer
        //LEAVE_ROOM_STATE leaveRoomMultiPlayer(Player _session, int _option) { }

        //// Room Lobby Grand Prix
        //LEAVE_ROOM_STATE leaveRoomGrandPrix(Player _session, int _option) { }

        //// GM Kick player room
        //LEAVE_ROOM_STATE kickPlayerRoom(Player _session, byte force) { }

        public vector<Player> getSessions(int _lobby = 0)
        {
            vector<Player> v_session = new vector<Player>();

            for (var i = 0; i < v_sessions.Count(); ++i)
            {
                if (v_sessions[i] != null && v_sessions[i].m_pi.channel != 0
                    && (_lobby == (int)~0 || v_sessions[i].m_pi.lobby != ~0))
                    v_session.Add(v_sessions[i]);

            }
            return v_session;
        }


        // Lobby
        public void requestEnterLobby(Player _session, packet _packet)
        {
            enterLobbyMultiPlayer(_session);
        }
        public void requestExitLobby(Player _session, packet _packet)
        {
            leaveLobbyMultiPlayer(_session);
        }
        void requestEnterLobbyGrandPrix(Player _session, packet _packet) { }
        void requestExitLobbyGrandPrix(Player _session, packet _packet) { }

        // Spy (GM) observer
        void requestEnterSpyRoom(Player _session, packet _packet) { }

        // Room
        void requestMakeRoom(Player _session, packet _packet) { }
        void requestEnterRoom(Player _session, packet _packet) { }
        void requestChangeInfoRoom(Player _session, packet _packet) { }
        void requestExitRoom(Player _session, packet _packet) { }
        void requestShowInfoRoom(Player _session, packet _packet) { }
        void requestPlayerLocationRoom(Player _session, packet _packet) { }
        void requestChangePlayerStateReadyRoom(Player _session, packet _packet) { }
        void requestKickPlayerOfRoom(Player _session, packet _packet) { }
        void requestChangePlayerTeamRoom(Player _session, packet _packet) { }
        void requestChangePlayerStateAFKRoom(Player _session, packet _packet) { }
        void requestPlayerStateCharacterLounge(Player _session, packet _packet) { }
        void requestToggleAssist(Player _session, packet _packet) { }
        void requestInvite(Player _session, packet _packet) { }
        void requestCheckInvite(Player _session, packet _packet) { } // Esse aqui o O Server Original nao retorna nada para o cliente, acho que é só um check
        void requestChatTeam(Player _session, packet _packet) { }

        // Request Player sai do Web Guild, verifica se tem alguma atualização para passar para o player no gs
        void requestExitedFromWebGuild(Player _session, packet _packet) { }

        // Request Game
        void requestStartGame(Player _session, packet _packet) { }
        void requestInitHole(Player _session, packet _packet) { }
        void requestFinishLoadHole(Player _session, packet _packet) { }
        void requestFinishCharIntro(Player _session, packet _packet) { }
        void requestFinishHoleData(Player _session, packet _packet) { }

        // Server enviou a resposta do InitShot para o cliente
        void requestInitShotSended(Player _session, packet _packet) { }

        void requestInitShot(Player _session, packet _packet) { }
        void requestSyncShot(Player _session, packet _packet) { }
        void requestInitShotArrowSeq(Player _session, packet _packet) { }
        void requestShotEndData(Player _session, packet _packet) { }
        void requestFinishShot(Player _session, packet _packet) { }

        void requestChangeMira(Player _session, packet _packet) { }
        void requestChangeStateBarSpace(Player _session, packet _packet) { }
        void requestActivePowerShot(Player _session, packet _packet) { }
        void requestChangeClub(Player _session, packet _packet) { }
        void requestUseActiveItem(Player _session, packet _packet) { }
        void requestChangeStateTypeing(Player _session, packet _packet) { }
        void requestMoveBall(Player _session, packet _packet) { }
        void requestChangeStateChatBlock(Player _session, packet _packet) { }
        void requestActiveBooster(Player _session, packet _packet) { }
        void requestActiveReplay(Player _session, packet _packet) { }
        void requestActiveCutin(Player _session, packet _packet) { }
        void requestActiveAutoCommand(Player _session, packet _packet) { }
        void requestActiveAssistGreen(Player _session, packet _packet) { }

        // VersusBase
        void requestLoadGamePercent(Player _session, packet _packet) { }
        void requestMarkerOnCourse(Player _session, packet _packet) { }
        void requestStartTurnTime(Player _session, packet _packet) { }
        void requestUnOrPauseGame(Player _session, packet _packet) { }
        void requestLastPlayerFinishVersus(Player _session, packet _packet) { }
        void requestReplyContinueVersus(Player _session, packet _packet) { }

        // Match
        void requestTeamFinishHole(Player _session, packet _packet) { }

        // Practice
        void requestLeavePractice(Player _session, packet _packet) { }

        // Tourney
        void requestUseTicketReport(Player _session, packet _packet) { }

        // Grand Zodiac
        void requestLeaveChipInPractice(Player _session, packet _packet) { }
        void requestStartFirstHoleGrandZodiac(Player _session, packet _packet) { }
        void requestReplyInitialValueGrandZodiac(Player _session, packet _packet) { }

        // Ability Item
        void requestActiveRing(Player _session, packet _packet) { }
        void requestActiveRingGround(Player _session, packet _packet) { }
        void requestActiveRingPawsRainbowJP(Player _session, packet _packet) { }
        void requestActiveRingPawsRingSetJP(Player _session, packet _packet) { }
        void requestActiveRingPowerGagueJP(Player _session, packet _packet) { }
        void requestActiveRingMiracleSignJP(Player _session, packet _packet) { }
        void requestActiveWing(Player _session, packet _packet) { }
        void requestActivePaws(Player _session, packet _packet) { }
        void requestActiveGlove(Player _session, packet _packet) { }
        void requestActiveEarcuff(Player _session, packet _packet) { }

        // Request Enter Game After Started
        void requestEnterGameAfterStarted(Player _session, packet _packet) { }

        void requestFinishGame(Player _session, packet _packet) { }

        void requestChangeWindNextHoleRepeat(Player _session, packet _packet) { }

        // Grand Prix
        void requestEnterRoomGrandPrix(Player _session, packet _packet) { }
        void requestExitRoomGrandPrix(Player _session, packet _packet) { }

        // Player Report Chat Game
        void requestPlayerReportChatGame(Player _session, packet _packet) { }

        // Common Command GM
        void requestExecCCGVisible(Player _session, packet _packet) { }
        void requestExecCCGChangeWindVersus(Player _session, packet _packet) { }
        void requestExecCCGChangeWeather(Player _session, packet _packet) { }
        void requestExecCCGGoldenBell(Player _session, packet _packet) { }
        void requestExecCCGIdentity(Player _session, packet _packet) { }
        void requestExecCCGKick(Player _session, packet _packet) { }
        void requestExecCCGDestroy(Player _session, packet _packet) { }

        // MyRoom
        void requestChangePlayerItemMyRoom(Player _session, packet _packet) { }
        void requestOpenTicketReportScroll(Player _session, packet _packet) { }
        void requestChangeMascotMessage(Player _session, packet _packet) { }

        // Caddie Extend Days and Set Notice Holyday Caddie
        void requestPayCaddieHolyDay(Player _session, packet _packet) { }
        void requestSetNoticeBeginCaddieHolyDay(Player _session, packet _packet) { }

        // Shop
        void requestBuyItemShop(Player _session, packet _packet) { }
        void requestGiftItemShop(Player _session, packet _packet) { }

        // MyRoom Extend or Remove Part Rental
        void requestExtendRental(Player _session, packet _packet) { }
        void requestDeleteRental(Player _session, packet _packet) { }

        // Attendance reward, Premios por logar no pangya
        void requestCheckAttendanceReward(Player _session, packet _packet) { }
        void requestAttendanceRewardLoginCount(Player _session, packet _packet) { }

        // Daily Quest
        void requestDailyQuest(Player _session, packet _packet) { }
        void requestAcceptDailyQuest(Player _session, packet _packet) { }
        void requestTakeRewardDailyQuest(Player _session, packet _packet) { }
        void requestLeaveDailyQuest(Player _session, packet _packet) { }

        // Cadie's Cauldron
        void requestCadieCauldronExchange(Player _session, packet _packet) { }

        // Character Stats
        void requestCharacterStatsUp(Player _session, packet _packet) { }
        void requestCharacterStatsDown(Player _session, packet _packet) { }
        void requestCharacterMasteryExpand(Player _session, packet _packet) { }
        void requestCharacterCardEquip(Player _session, packet _packet) { }
        void requestCharacterCardEquipWithPatcher(Player _session, packet _packet) { }
        void requestCharacterRemoveCard(Player _session, packet _packet) { }

        // ClubSet Enchant
        void requestClubSetStatsUpdate(Player _session, packet _packet) { }

        // Tiki's Shop
        void requestTikiShopExchangeItem(Player _session, packet _packet) { }

        // Item Equipado
        void requestChangePlayerItemChannel(Player _session, packet _packet) { }
        void requestChangePlayerItemRoom(Player _session, packet _packet) { }

        // Delete Active Item
        void requestDeleteActiveItem(Player _session, packet _packet) { }

        // ClubSet WorkShop
        void requestClubSetWorkShopTransferMasteryPts(Player _session, packet _packet) { }
        void requestClubSetWorkShopRecoveryPts(Player _session, packet _packet) { }
        void requestClubSetWorkShopUpLevel(Player _session, packet _packet) { }
        void requestClubSetWorkShopUpLevelConfirm(Player _session, packet _packet) { }
        void requestClubSetWorkShopUpLevelCancel(Player _session, packet _packet) { }
        void requestClubSetWorkShopUpRank(Player _session, packet _packet) { }
        void requestClubSetWorkShopUpRankTransformConfirm(Player _session, packet _packet) { }
        void requestClubSetWorkShopUpRankTransformCancel(Player _session, packet _packet) { }

        // ClubSet Reset
        void requestClubSetReset(Player _session, packet _packet) { }

        // Tutorial
        void requestMakeTutorial(Player _session, packet _packet) { }

        // Web Link
        void requestEnterWebLinkState(Player _session, packet _packet) { }

        // Pede o Cookies
        void requestCookie(Player _session, packet _packet) { }

        // Pede para atualizar Gacha Coupon(s)
        void requestUpdateGachaCoupon(Player _session, packet _packet) { }

        // Box System, Box que envia para o MailBox e a Box que envia direto para o MyRoom
        void requestOpenBoxMail(Player _session, packet _packet) { }
        void requestOpenBoxMyRoom(Player _session, packet _packet) { }

        // Memorial System
        void requestPlayMemorial(Player _session, packet _packet) { }

        // Card System
        void requestOpenCardPack(Player _session, packet _packet) { }
        void requestLoloCardCompose(Player _session, packet _packet) { }

        // Card Special/ Item Buff
        void requestUseCardSpecial(Player _session, packet _packet) { }
        void requestUseItemBuff(Player _session, packet _packet) { }

        // Comet Refill
        void requestCometRefill(Player _session, packet _packet) { }

        // MailBox
        void requestOpenMailBox(Player _session, packet _packet) { }
        void requestInfoMail(Player _session, packet _packet) { }
        void requestSendMail(Player _session, packet _packet) { }
        void requestTakeItemFomMail(Player _session, packet _packet) { }
        void requestDeleteMail(Player _session, packet _packet) { }

        // Dolfini Locker
        void requestMakePassDolfiniLocker(Player _session, packet _packet) { }
        void requestCheckDolfiniLockerPass(Player _session, packet _packet) { }
        void requestChangeDolfiniLockerPass(Player _session, packet _packet) { }
        void requestChangeDolfiniLockerModeEnter(Player _session, packet _packet) { }
        void requestDolfiniLockerItem(Player _session, packet _packet) { }
        void requestDolfiniLockerPang(Player _session, packet _packet) { }
        void requestUpdateDolfiniLockerPang(Player _session, packet _packet) { }
        void requestAddDolfiniLockerItem(Player _session, packet _packet) { }
        void requestRemoveDolfiniLockerItem(Player _session, packet _packet) { }

        // Legacy Tiki Shop (Point Shop)
        void requestOpenLegacyTikiShop(Player _session, packet _packet) { }
        void requestPointLegacyTikiShop(Player _session, packet _packet) { }
        void requestExchangeTPByItemLegacyTikiShop(Player _session, packet _packet) { }
        void requestExchangeItemByTPLegacyTikiShop(Player _session, packet _packet) { }

        // Personal Shop
        void requestOpenEditSaleShop(Player _session, packet _packet) { }
        void requestCloseSaleShop(Player _session, packet _packet) { }
        void requestChangeNameSaleShop(Player _session, packet _packet) { }
        void requestOpenSaleShop(Player _session, packet _packet) { }
        void requestVisitCountSaleShop(Player _session, packet _packet) { }
        void requestPangSaleShop(Player _session, packet _packet) { }
        void requestCancelEditSaleShop(Player _session, packet _packet) { }
        void requestViewSaleShop(Player _session, packet _packet) { }
        void requestCloseViewSaleShop(Player _session, packet _packet) { }
        void requestBuyItemSaleShop(Player _session, packet _packet) { }

        // Papel Shop
        void requestOpenPapelShop(Player _session, packet _packet) { }
        void requestPlayPapelShop(Player _session, packet _packet) { }

        // Msg Chat da Sala
        void requestSendMsgChatRoom(Player _session, string _msg) { }

        // senders
        void sendUpdateRoomInfo(RoomInfoEx _ri, int _option) { }
        void sendUpdatePlayerInfo(Player _session, int _option)
        {
            packet p = new packet();
            PlayerCanalInfo pci = getPlayerInfo(_session);
            if (_session.m_gi.visible == 0)
            {
                pci.capability.ulCapability = 0;
            }
            else
            {
                updatePlayerInfo(_session);
            }
            packet_func.pacote046(ref p, _session, new vector<PlayerCanalInfo> { (pci == null) ? new PlayerCanalInfo() : pci }, _option);
            packet_func.channel_broadcast(this, ref p, 0);
        }

        // Destroy Room
        void destroyRoom(short _number) { }
    }
}
