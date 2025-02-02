using GameServer.PangType;
using PangyaAPI.Utilities;
using System.Collections.Generic;
using System.Linq;
using packet = PangyaAPI.Network.PangyaPacket.Packet;
using packet_func = GameServer.PacketFunc.packet_func;
using static GameServer.PangType._Define;
using System;
using _smp = PangyaAPI.Utilities.Log;
using GameServer.Session;
using PangyaAPI.Utilities.BinaryModels;
using PangyaAPI.Utilities.Log;
using GameServer.Game.System;

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
                // Player Canal Info clear
                uid = _session.m_pi.uid,
                oid = _session.m_oid,
                sala_numero = _session.m_pi.mi.sala_numero,
                level = (byte)_session.m_pi.level,
                capability = _session.m_pi.mi.capability,
                nickname = _session.m_pi.nickname,
                nickNT = "@" + _session.m_pi.nickname,

                title = _session.m_pi.ue.getTitle(),
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

            if (!m_player_info.ContainsKey(_session))
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
            pci.capability = _session.m_pi.mi.capability;
            pci.title = _session.m_pi.ue.getTitle();
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

            if (_session.m_pi.channel != DEFAULT_CHANNEL)
                throw new exception("[channel::enterChannel][Error] player[UID=" + (_session.m_pi.uid)
                    + "] ja esta conectado em outro canal.");

            addSession(_session);

            var p = packet_func.pacote095(0x102);
            _session.Send(p);

            p = packet_func.pacote04E(1);
            _session.Send(p);

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

                if (_session.m_pi.lobby != ~0)
                    leaveLobby(_session);       // Sai da Lobby

                else // Sai da Sala Practice que não entra na lobby, [SINGLE PLAY]
                    leaveRoom(_session, 0);

                removeSession(_session);

            }
            catch (exception e)
            {

                removeSession(_session);

                // _smp.message_pool.push("[channel::leaveChannel][Error] " + e.getFullMessageError());


            }
        }
        LEAVE_ROOM_STATE leaveRoom(Player _session, int _option)
        {
            return LEAVE_ROOM_STATE.DO_NOTHING;
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

        public byte[] Build() { return m_ci.Build(); }
        // Gets
        public byte getId() { return (byte)m_ci.id; }

        protected PlayerCanalInfoEx getPlayerInfo(Player _session)
        {
            if (_session == null)
                throw new exception("[channel::getPlayerInfo][Error] _session is null.");

            PlayerCanalInfoEx pci = null;
            PlayerCanalInfoEx i;

            if ((i = m_player_info.First(c => c.Key.m_pi.uid == _session.m_pi.uid).Value) != null)
                pci = i;


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
        public void enterLobby(Player _session, byte _lobby)
        {
            //if (!_session.get())
            //    throw new exception("[channel.enterLobby][Error] player[UID_TRASH=" + (_session.m_pi.uid)
            //        + "] nao esta conectado.");

            if (_session.m_pi.lobby != DEFAULT_CHANNEL)
                throw new exception("[channel.enterLobby][Error] player[UID=" + (_session.m_pi.uid)
                    + "] ja esta na lobby.");

            _session.m_pi.lobby = (byte)((_lobby == 0 || _lobby == 0) ? 1/*Padrão*/ : _lobby);
            _session.m_pi.place = 0;

            updatePlayerInfo(_session);

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
            var p = packet_func.pacote046(v_pci, 4);
            _session.Send(p);

            if (v_pci.Count() > 0)
            {
                _session.Send(packet_func.pacote046(v_pci, 5));
            }
            //if (packet_func.pacote047(p, v_ri, 0))
            //    packet_func.session_send(ref p, _session, 0);

            _session.SendChannel_broadcast(this, packet_func.pacote046(pci == null ? new vector<PlayerCanalInfo>() : new vector<PlayerCanalInfo>(pci), 1));

            v_pci.Clear();
        }
        public void leaveLobby(Player _session)
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

            _session.m_pi.lobby = DEFAULT_CHANNEL; //correct ;)
            _session.m_pi.place = DEFAULT_CHANNEL; //correct :)
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

                var p = new PangyaBinaryWriter();
                p.init_plain(0xF5);
                _session.Send(p);

            }
            catch (exception e)
            {
                Console.WriteLine("[Channel::enterLobbyMultiPlayer][StError]: " + e.getFullMessageError());
            }
        }
        void leaveLobbyMultiPlayer(Player _session)
        {

            try
            {

                // leave Lobby
                leaveLobby(_session);

                var p = new PangyaBinaryWriter();
                p.init_plain(0xF6);
                _session.Send(p);

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
                if (v_sessions[i] != null && v_sessions[i].m_pi.channel != DEFAULT_CHANNEL
                    && (_lobby == ~0 || v_sessions[i].m_pi.lobby != DEFAULT_CHANNEL))
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
        public void requestEnterLobbyGrandPrix(Player _session, packet _packet) { }
        public void requestExitLobbyGrandPrix(Player _session, packet _packet) { }

        // Spy (GM) observer
        public void requestEnterSpyRoom(Player _session, packet _packet) { }

        // Room
        public void requestMakeRoom(Player _session, packet _packet) { }
        public void requestEnterRoom(Player _session, packet _packet) { }
        public void requestChangeInfoRoom(Player _session, packet _packet) { }
        public void requestExitRoom(Player _session, packet _packet) { }
        public void requestShowInfoRoom(Player _session, packet _packet) { }
        public void requestPlayerLocationRoom(Player _session, packet _packet) { }
        public void requestChangePlayerStateReadyRoom(Player _session, packet _packet) { }
        public void requestKickPlayerOfRoom(Player _session, packet _packet) { }
        public void requestChangePlayerTeamRoom(Player _session, packet _packet) { }
        public void requestChangePlayerStateAFKRoom(Player _session, packet _packet) { }
        public void requestPlayerStateCharacterLounge(Player _session, packet _packet) { }
        public void requestToggleAssist(Player _session, packet _packet) { }
        public void requestInvite(Player _session, packet _packet) { }
        public void requestCheckInvite(Player _session, packet _packet) { } // Esse aqui o O Server Original nao retorna nada para o cliente, acho que é só um check
        public void requestChatTeam(Player _session, packet _packet) { }

        // Request Player sai do Web Guild, verifica se tem alguma atualização para passar para o player no gs
        public void requestExitedFromWebGuild(Player _session, packet _packet) { }

        // Request Game
        public void requestStartGame(Player _session, packet _packet) { }
        public void requestInitHole(Player _session, packet _packet) { }
        public void requestFinishLoadHole(Player _session, packet _packet) { }
        public void requestFinishCharIntro(Player _session, packet _packet) { }
        public void requestFinishHoleData(Player _session, packet _packet) { }

        // Server enviou a resposta do InitShot para o cliente
        public void requestInitShotSended(Player _session, packet _packet) { }

        public void requestInitShot(Player _session, packet _packet) { }
        public void requestSyncShot(Player _session, packet _packet) { }
        public void requestInitShotArrowSeq(Player _session, packet _packet) { }
        public void requestShotEndData(Player _session, packet _packet) { }
        public void requestFinishShot(Player _session, packet _packet) { }

        public void requestChangeMira(Player _session, packet _packet) { }
        public void requestChangeStateBarSpace(Player _session, packet _packet) { }
        public void requestActivePowerShot(Player _session, packet _packet) { }
        public void requestChangeClub(Player _session, packet _packet) { }
        public void requestUseActiveItem(Player _session, packet _packet) { }
        public void requestChangeStateTypeing(Player _session, packet _packet) { }
        public void requestMoveBall(Player _session, packet _packet) { }
        public void requestChangeStateChatBlock(Player _session, packet _packet) { }
        public void requestActiveBooster(Player _session, packet _packet) { }
        public void requestActiveReplay(Player _session, packet _packet) { }
        public void requestActiveCutin(Player _session, packet _packet) { }
        public void requestActiveAutoCommand(Player _session, packet _packet) { }
        public void requestActiveAssistGreen(Player _session, packet _packet) { }

        // VersusBase
        public void requestLoadGamePercent(Player _session, packet _packet) { }
        public void requestMarkerOnCourse(Player _session, packet _packet) { }
        public void requestStartTurnTime(Player _session, packet _packet) { }
        public void requestUnOrPauseGame(Player _session, packet _packet) { }
        public void requestLastPlayerFinishVersus(Player _session, packet _packet) { }
        public void requestReplyContinueVersus(Player _session, packet _packet) { }

        // Match
        public void requestTeamFinishHole(Player _session, packet _packet) { }

        // Practice
        public void requestLeavePractice(Player _session, packet _packet) { }

        // Tourney
        public void requestUseTicketReport(Player _session, packet _packet) { }

        // Grand Zodiac
        public void requestLeaveChipInPractice(Player _session, packet _packet) { }
        public void requestStartFirstHoleGrandZodiac(Player _session, packet _packet) { }
        public void requestReplyInitialValueGrandZodiac(Player _session, packet _packet) { }

        // Ability Item
        public void requestActiveRing(Player _session, packet _packet) { }
        public void requestActiveRingGround(Player _session, packet _packet) { }
        public void requestActiveRingPawsRainbowJP(Player _session, packet _packet) { }
        public void requestActiveRingPawsRingSetJP(Player _session, packet _packet) { }
        public void requestActiveRingPowerGagueJP(Player _session, packet _packet) { }
        public void requestActiveRingMiracleSignJP(Player _session, packet _packet) { }
        public void requestActiveWing(Player _session, packet _packet) { }
        public void requestActivePaws(Player _session, packet _packet) { }
        public void requestActiveGlove(Player _session, packet _packet) { }
        public void requestActiveEarcuff(Player _session, packet _packet) { }

        // Request Enter Game After Started
        public void requestEnterGameAfterStarted(Player _session, packet _packet) { }

        public void requestFinishGame(Player _session, packet _packet) { }

        public void requestChangeWindNextHoleRepeat(Player _session, packet _packet) { }

        // Grand Prix
        public void requestEnterRoomGrandPrix(Player _session, packet _packet) { }
        public void requestExitRoomGrandPrix(Player _session, packet _packet) { }

        // Player Report Chat Game
        public void requestPlayerReportChatGame(Player _session, packet _packet) { }

        // Common Command GM
        public void requestExecCCGVisible(Player _session, packet _packet) { }
        public void requestExecCCGChangeWindVersus(Player _session, packet _packet) { }
        public void requestExecCCGChangeWeather(Player _session, packet _packet) { }
        public void requestExecCCGGoldenBell(Player _session, packet _packet) { }
        public void requestExecCCGIdentity(Player _session, packet _packet) { }
        public void requestExecCCGKick(Player _session, packet _packet) { }
        public void requestExecCCGDestroy(Player _session, packet _packet) { }

        // MyRoom
        public void requestChangePlayerItemMyRoom(Player _session, packet _packet) { }
        public void requestOpenTicketReportScroll(Player _session, packet _packet) { }
        public void requestChangeMascotMessage(Player _session, packet _packet) { }

        // Caddie Extend Days and Set Notice Holyday Caddie
        public void requestPayCaddieHolyDay(Player _session, packet _packet) { }
        public void requestSetNoticeBeginCaddieHolyDay(Player _session, packet _packet) { }

        // Shop
        public void requestBuyItemShop(Player _session, packet _packet) { }
        public void requestGiftItemShop(Player _session, packet _packet) { }

        // MyRoom Extend or Remove Part Rental
        public void requestExtendRental(Player _session, packet _packet) { }
        public void requestDeleteRental(Player _session, packet _packet) { }

        // Attendance reward, Premios por logar no pangya
        public void requestCheckAttendanceReward(Player _session, packet _packet)
        {
            try
            {
                sAttendanceRewardSystem.getInstance().requestCheckAttendance(_session, _packet);
            }
            catch (exception e)
            {
                _smp::message_pool.push(new message("[channel::requestCheckAttendanceReward][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                throw;
            }
        }

        public void requestAttendanceRewardLoginCount(Player _session, packet _packet)
        {
            try
            {
                sAttendanceRewardSystem.getInstance().requestUpdateCountLogin(_session, _packet);
            }
            catch (exception e)
            {
                _smp::message_pool.push(new message("[channel::requestCheckAttendanceReward][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                throw;
            }

        }

        // Daily Quest
        public void requestDailyQuest(Player _session, packet _packet) { }
        public void requestAcceptDailyQuest(Player _session, packet _packet) { }
        public void requestTakeRewardDailyQuest(Player _session, packet _packet) { }
        public void requestLeaveDailyQuest(Player _session, packet _packet) { }

        // Cadie's Cauldron
        public void requestCadieCauldronExchange(Player _session, packet _packet) { }

        // Character Stats
        public void requestCharacterStatsUp(Player _session, packet _packet) { }
        public void requestCharacterStatsDown(Player _session, packet _packet) { }
        public void requestCharacterMasteryExpand(Player _session, packet _packet) { }
        public void requestCharacterCardEquip(Player _session, packet _packet) { }
        public void requestCharacterCardEquipWithPatcher(Player _session, packet _packet) { }
        public void requestCharacterRemoveCard(Player _session, packet _packet) { }

        // ClubSet Enchant
        public void requestClubSetStatsUpdate(Player _session, packet _packet) { }

        // Tiki's Shop
        public void requestTikiShopExchangeItem(Player _session, packet _packet) { }

        // Item Equipado
        public void requestChangePlayerItemChannel(Player _session, packet _packet) { }
        public void requestChangePlayerItemRoom(Player _session, packet _packet) { }

        // Delete Active Item
        public void requestDeleteActiveItem(Player _session, packet _packet) { }

        // ClubSet WorkShop
        public void requestClubSetWorkShopTransferMasteryPts(Player _session, packet _packet) { }
        public void requestClubSetWorkShopRecoveryPts(Player _session, packet _packet) { }
        public void requestClubSetWorkShopUpLevel(Player _session, packet _packet) { }
        public void requestClubSetWorkShopUpLevelConfirm(Player _session, packet _packet) { }
        public void requestClubSetWorkShopUpLevelCancel(Player _session, packet _packet) { }
        public void requestClubSetWorkShopUpRank(Player _session, packet _packet) { }
        public void requestClubSetWorkShopUpRankTransformConfirm(Player _session, packet _packet) { }
        public void requestClubSetWorkShopUpRankTransformCancel(Player _session, packet _packet) { }

        // ClubSet Reset
        public void requestClubSetReset(Player _session, packet _packet) { }

        // Tutorial
        public void requestMakeTutorial(Player _session, packet _packet) { }

        // Web Link
        public void requestEnterWebLinkState(Player _session, packet _packet) { }

        // Pede o Cookies
        public void requestCookie(Player _session, packet _packet) { }

        // Pede para atualizar Gacha Coupon(s)
        public void requestUpdateGachaCoupon(Player _session, packet _packet) { }

        // Box System, Box que envia para o MailBox e a Box que envia direto para o MyRoom
        public void requestOpenBoxMail(Player _session, packet _packet) { }
        public void requestOpenBoxMyRoom(Player _session, packet _packet) { }

        // Memorial System
        public void requestPlayMemorial(Player _session, packet _packet) { }

        // Card System
        public void requestOpenCardPack(Player _session, packet _packet) { }
        public void requestLoloCardCompose(Player _session, packet _packet) { }

        // Card Special/ Item Buff
        public void requestUseCardSpecial(Player _session, packet _packet) { }
        public void requestUseItemBuff(Player _session, packet _packet) { }

        // Comet Refill
        public void requestCometRefill(Player _session, packet _packet) { }

        // MailBox
        public void requestOpenMailBox(Player _session, packet _packet) { }
        public void requestInfoMail(Player _session, packet _packet) { }
        public void requestSendMail(Player _session, packet _packet) { }
        public void requestTakeItemFomMail(Player _session, packet _packet) { }
        public void requestDeleteMail(Player _session, packet _packet) { }

        // Dolfini Locker
        public void requestMakePassDolfiniLocker(Player _session, packet _packet) { }
        public void requestCheckDolfiniLockerPass(Player _session, packet _packet) { }
        public void requestChangeDolfiniLockerPass(Player _session, packet _packet) { }
        public void requestChangeDolfiniLockerModeEnter(Player _session, packet _packet) { }
        public void requestDolfiniLockerItem(Player _session, packet _packet) { }
        public void requestDolfiniLockerPang(Player _session, packet _packet) { }
        public void requestUpdateDolfiniLockerPang(Player _session, packet _packet) { }
        public void requestAddDolfiniLockerItem(Player _session, packet _packet) { }
        public void requestRemoveDolfiniLockerItem(Player _session, packet _packet) { }

        // Legacy Tiki Shop (Point Shop)
        public void requestOpenLegacyTikiShop(Player _session, packet _packet) { }
        public void requestPointLegacyTikiShop(Player _session, packet _packet) { }
        public void requestExchangeTPByItemLegacyTikiShop(Player _session, packet _packet) { }
        public void requestExchangeItemByTPLegacyTikiShop(Player _session, packet _packet) { }

        // Personal Shop
        public void requestOpenEditSaleShop(Player _session, packet _packet) { }
        public void requestCloseSaleShop(Player _session, packet _packet) { }
        public void requestChangeNameSaleShop(Player _session, packet _packet) { }
        public void requestOpenSaleShop(Player _session, packet _packet) { }
        public void requestVisitCountSaleShop(Player _session, packet _packet) { }
        public void requestPangSaleShop(Player _session, packet _packet) { }
        public void requestCancelEditSaleShop(Player _session, packet _packet) { }
        public void requestViewSaleShop(Player _session, packet _packet) { }
        public void requestCloseViewSaleShop(Player _session, packet _packet) { }
        public void requestBuyItemSaleShop(Player _session, packet _packet) { }

        // Papel Shop
        public void requestOpenPapelShop(Player _session, packet _packet) { }
        public void requestPlayPapelShop(Player _session, packet _packet) { }

        // Msg Chat da Sala
        public void requestSendMsgChatRoom(Player _session, string _msg) { }

        // senders
        public void sendUpdateRoomInfo(RoomInfoEx _ri, int _option) { }
        void sendUpdatePlayerInfo(Player _session, int _option)
        {
            PlayerCanalInfo pci = getPlayerInfo(_session);
            if (_session.m_gi.visible == 0)
            {
                pci.capability.ulCapability = 0;
            }
            else
            {
                updatePlayerInfo(_session);
            }
            _session.Send(packet_func.pacote046(new vector<PlayerCanalInfo> { (pci == null) ? new PlayerCanalInfo() : pci }, _option));
        }

        // Destroy Room
        void destroyRoom(short _number) { }
    }
}
