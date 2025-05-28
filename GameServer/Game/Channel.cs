using GameServer.GameType;
using PangyaAPI.Utilities;
using System.Collections.Generic;
using System.Linq;
using static GameServer.GameType._Define;
using System;
using _smp = PangyaAPI.Utilities.Log;
using GameServer.Session;
using PangyaAPI.Utilities.BinaryModels;
using PangyaAPI.Utilities.Log;
using GameServer.Game.System;
using GameServer.Game.Manager;
using GameServer.PangyaEnums;
using GameServer.PacketFunc;
using PangyaAPI.Network.PangyaPacket;
using PangyaAPI.Network.Pangya_St;
using PangyaAPI.SQL.Manager;
using GameServer.Cmd;
using System.Runtime.InteropServices;
using PangyaAPI.IFF.JP.Models.Data;
namespace GameServer.Game
{
    public class Channel
    {

        protected enum ESTADO : byte
        {
            UNITIALIZED,
            INITIALIZED
        }

        protected enum LEAVE_ROOM_STATE : int
        {
            DO_NOTHING = -1,        // Faz nada
            SEND_UPDATE_CLIENT = 0, // bug arm g++
            ROOM_DESTROYED,
        }

        protected ChannelInfo m_ci;
        //RoomManager m_rm;

        protected uint m_type;           // Type GrandPrix, Natural, Normal

        protected int m_state;

        protected List<Player> v_sessions;
        protected Dictionary<Player, PlayerCanalInfoEx> m_player_info;

        protected List<InviteChannelInfo> v_invite;
        public Channel(ChannelInfo _ci, uint _type)
        {
            m_ci = _ci;
            m_type = _type;
            m_state = (int)ESTADO.INITIALIZED;
            v_sessions = new List<Player>();
            m_player_info = new Dictionary<Player, PlayerCanalInfoEx>();
            v_invite = new List<InviteChannelInfo>();
        }

        protected void addInviteTimeRequest(InviteChannelInfo _ici) { }
        protected void deleteInviteTimeRequest(InviteChannelInfo _ici) { }
        protected void deleteInviteTimeResquestByInvited(Player _session) { }

        // Tira o request do convidado da sala[Character] o tempo acabou para ele responder ao convite
        protected bool send_time_out_invite(InviteChannelInfo _ici) { return true; }
        void clear_invite_time() { }

        void removeSession(Player _session)
        {
            if (_session == null || !v_sessions.Any(c => c == _session))
                return;//retorna, ele nao esta na lista mais!

            int index;
            if ((index = findIndexSession(_session)) == -1)
            {
                throw new exception("[channel::removeSession][Error] _session not exists on vector sessions.");
            }

            v_sessions.RemoveAt(index);

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
                    _session.m_pi.mi.state_flag.azinha = true;
                else if (rate >= QUITER_ICON_1 && rate < QUITER_ICON_2)
                    _session.m_pi.mi.state_flag.quiter_1 = true;
                else if (rate >= QUITER_ICON_2)
                    _session.m_pi.mi.state_flag.quiter_2 = true;
            }

            if (_session.m_pi.ei.char_info != null && _session.m_pi.ui.getQuitRate() < GOOD_PLAYER_ICON)
                _session.m_pi.mi.state_flag.icon_angel = Convert.ToBoolean(_session.m_pi.ei.char_info.AngelEquiped());
            else
                _session.m_pi.mi.state_flag.icon_angel = false;

            _session.m_pi.mi.state_flag.sexo = _session.m_pi.mi.sexo == 1 ? true : false;



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
                nickNT = "@NT_" + _session.m_pi.nickname,
                title = _session.m_pi.ue.getTitle(),
                team_point = 1000,
                flag_visible_gm = (short)(_session.m_pi.mi.state_flag.visible ? 1 : 0)
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
            PlayerCanalInfoEx pci;

            if ((pci = getPlayerInfo(_session)) == null)
                throw new exception("[channel::updatePlayerInfo][Error] nao tem o player[UID=" + (_session.m_pi.uid)
                    + "] info dessa session no canal.");

            // Player Canal Info Update
            pci.uid = _session.m_pi.uid;
            pci.oid = _session.m_oid;
            pci.sala_numero = _session.m_pi.mi.sala_numero;
            pci.level = (byte)_session.m_pi.level;
            pci.team_point = 1000;
            pci.flag_visible_gm = (short)(_session.m_pi.mi.state_flag.visible ? 1 : 0);
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
                //    throw new exception("[channel::_enter_left_time_is_over][Error] Channel[ID=" + (c.getId())
                //        + "] Sala[NUMERO=" + (numero) + "] nao encontrou a sala no canal",ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.CHANNEL, 1202, 0));

                //r.setState(0);
                //r.setFlag(0);

                //// Limpa no Game o Timer
                //r.requestEndAfterEnter();


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

            _session.Send(packet_func.pacote095(0x102));
            _session.Send(packet_func.pacote04E(1));

            //// Verifica se o tempo do ticket premium user acabou e manda a mensagem para o player, e exclui o ticket do player no SERVER, DB e GAME
            //sPremiumSystem.checkEndTimeTicket(_session);
        }
        public void leaveChannel(Player _session)
        {
            //!@ As vezes o player sai antes e não tem mais como deletar ele do canal
            //if (!_session.getState())
            //throw new exception("Error player não conectar. Em channel::leaveChannel()",ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.CHANNEL, 1, 0));

            try
            {

                if (_session.m_pi.lobby != 0)
                    leaveLobby(_session);       // Sai da Lobby

                else // Sai da Sala Practice que não entra na lobby, [SINGLE PLAY]
                    leaveRoom(_session, 0);

                removeSession(_session);

            }
            catch (exception e)
            {

                removeSession(_session);

                _smp.message_pool.push("[channel::leaveChannel][Error] " + e.getFullMessageError());


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
            if (!_session.m_pi.m_cap.game_master)
            {

                if (_session.m_pi.level < 0 || _session.m_pi.level > 70)
                    throw new exception("[channel::checkEnterChannel][Error] Player[UID=" + (_session.m_pi.uid) + ", LEVEL=" + (_session.m_pi.level)
                        + "] nao tem o level necessario para entrar no canal[ID=" + (m_ci.id) + ", MIN=" + (0)
                        + ", MAX=" + (70) + "].");

                if (m_ci.flag.only_rookie && Convert.ToBoolean(_session.m_pi.level > (short)enLEVEL.ROOKIE_A))
                    throw new exception("[channel::checkEnterChannel][Error] Player[UID=" + (_session.m_pi.uid) + ", LEVEL=" + (_session.m_pi.level)
                        + "] nao tem o level necessario para entrar no canal[ID=" + (m_ci.id) + ", MIN=" + (0)
                        + ", MAX=" + (70) + "] com a flag So Rookie.");

                if (m_ci.flag.junior_bellow && _session.m_pi.level > (short)enLEVEL.JUNIOR_A)
                    throw new exception("[channel::checkEnterChannel][Error] Player[UID=" + (_session.m_pi.uid) + ", LEVEL=" + (_session.m_pi.level)
                        + "] nao tem o level necessario para entrar no canal[ID=" + (m_ci.id) + ", MIN=" + (0)
                        + ", MAX=" + (70) + "] com a flag Junior A pra baixo.");

                if (m_ci.flag.junior_above && _session.m_pi.level < (short)enLEVEL.JUNIOR_E)
                    throw new exception("[channel::checkEnterChannel][Error] Player[UID=" + (_session.m_pi.uid) + ", LEVEL=" + (_session.m_pi.level)
                        + "] nao tem o level necessario para entrar no canal[ID=" + (m_ci.id) + ", MIN=" + (0)
                        + ", MAX=" + (70) + "] com a flag Junior E pra cima.");

                if (m_ci.flag.junior_between_senior && (_session.m_pi.level < (short)enLEVEL.JUNIOR_E || _session.m_pi.level > (short)enLEVEL.SENIOR_A))
                    throw new exception("[channel::checkEnterChannel][Error] Player[UID=" + (_session.m_pi.uid) + ", LEVEL=" + (_session.m_pi.level)
                        + "] nao tem o level necessario para entrar no canal[ID=" + (m_ci.id) + ", MIN=" + (0)
                        + ", MAX=" + (70) + "] com a flag junior E a Senior A.");

                if (m_ci.flag.beginner_between_junior && (_session.m_pi.level < (short)enLEVEL.BEGINNER_E || _session.m_pi.level > (short)enLEVEL.JUNIOR_A))
                    throw new exception("[channel::checkEnterChannel][Error] Player[UID=" + (_session.m_pi.uid) + ", LEVEL=" + (_session.m_pi.level)
                        + "] nao tem o level necessario para entrar no canal[ID=" + (m_ci.id) + ", MIN=" + (0)
                        + ", MAX=" + (70) + "] com a flag Beginner E a Junior A.");
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
        public void checkInviteTime() { }

        // stats
        public bool isFull()
        {
            return m_ci.curr_user >= m_ci.max_user;
        }

        // Lobby
        public void enterLobby(Player _session, byte _lobby)
        {
            //if (!_session.get())
            //    throw new exception("[channel.enterLobby][Error] player[UID_TRASH=" + (_session.m_pi.m_uid)
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
                {
                    v_pci.Add(pci);
                }
            }

            pci = getPlayerInfo(_session);

            // Add o primeiro limpando a lobby
            var p = packet_func.pacote046(v_pci, 4);
            _session.Send(p);

            if (v_pci.Count() > 0)
            {
                _session.Send(packet_func.pacote046(v_pci, 5));
            }
            //if (packet_func.pacote047(p, v_ri, 0)) // e a listagem de salas!
            //    packet_func.session_send(ref p, _session, 0);

            _session.SendChannel_broadcast(packet_func.pacote046(pci == null ? new vector<PlayerCanalInfo>() : new vector<PlayerCanalInfo>(pci), 1));

            v_pci.Clear();
        }
        public void leaveLobby(Player _session)
        {/// !@tem que tira isso aqui por que tem que enviar para os outros player da lobby que ele sai,
         /// mesmo que o sock dele não pode mais enviar
            //if (!_session.getState())
            //throw new exception("[channel::leaveLobby][Error] player nao esta conectado.",ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.CHANNEL, 1, 0));

            // Sai da sala se estiver em uma sala
            //try
            //{
            //    //leaveRoom(_session, 0);
            //}
            //catch (exception&e) {

            //    _smp::message_pool.push(new message("[channel::leaveLobby][Error] " + e.getFullMessageError(), CL_FILE_LOG_AND_CONSOLE));
            //}

            _session.m_pi.lobby = DEFAULT_CHANNEL; //correct ;)
            _session.m_pi.place = -1; //correct :)
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

                _session.Send(new PangyaBinaryWriter(0xF5));

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

                _session.Send(new PangyaBinaryWriter(0xF6));
            }
            catch (exception e)
            {
                Console.WriteLine("[Channel::leaveLobbyMultiPlayer][StError]: " + e.getFullMessageError());
            }
        }

        // Lobby Grand Prix
        public void enterLobbyGrandPrix(Player _session)
        {
            try
            {
                //falta as outras checagens!
                if (!sgs.gs.getInstance().getInfo().propriedade.grand_prix)
                    throw new exception("[channel::enterLobbyGrandPrix][Error] player[UID=" + (_session.m_pi.uid) + "] Channel[ID=" + (m_ci.id)
                            + "] tentou entrar na lobby Grand Prix, mas ele esta desativo. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.CHANNEL, 1, 0x750001));

                enterLobby(_session, 176/*Grand Prix*/);
                // entra Lobby Grand Prix
                using (var p = new PangyaBinaryWriter(0x250))
                {
                    p.WriteUInt32(0u);    // OK

                    // Count Type Grand Prix que está ativo
                    // Tipo 0 é ativo por sem precisar desses valores
                    p.WriteUInt32(sgs.gs.getInstance().getInfo().rate.countBitGrandPrixEvent());

                    // Grand Prix Event: Types
                    foreach (var el in sgs.gs.getInstance().getInfo().rate.getValueBitGrandPrixEvent())
                        p.WriteUInt32(el);

                    // Count de	grand prix clear, (typeid, position)
                    p.WriteInt32(_session.m_pi.v_gpc.Count());

                    foreach (var el in _session.m_pi.v_gpc)
                        p.WriteStruct(el, new GrandPrixClear());

                    // Avg. Score do player
                    p.Write(_session.m_pi.ui.getMediaScore());
                    _session.Send(p);
                }
            }
            catch (exception e)
            {
                _smp::message_pool.push(new message("[channel::enterLobbyGrandPrix][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
                using (var p = new PangyaBinaryWriter(0x250))
                {
                    p.WriteUInt32((ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) == (uint)STDA_ERROR_TYPE.CHANNEL) ? ExceptionError.STDA_SYSTEM_ERROR_DECODE(e.getCodeError()) : 0x750000);
                    _session.Send(p);
                }
            }
        }
        public void leaveLobbyGrandPrix(Player _session)
        {
            leaveLobby(_session);
            // Sai Lobby Grand Prix
            using (var p = new PangyaBinaryWriter(0x251))
            {
                p.WriteUInt32(0u);    // OK
                _session.Send(p);
            }
        }

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
        public void requestEnterLobby(Player _session, Packet _packet)
        {
            enterLobbyMultiPlayer(_session);
        }
        public void requestExitLobby(Player _session, Packet _packet)
        {
            leaveLobbyMultiPlayer(_session);
        }
        public void requestEnterLobbyGrandPrix(Player _session, Packet _packet)
        {
            enterLobbyGrandPrix(_session);
        }
        public void requestExitLobbyGrandPrix(Player _session, Packet _packet)
        {
            leaveLobbyGrandPrix(_session);
        }

        // Spy (GM) observer
        public void requestEnterSpyRoom(Player _session, Packet _packet) { }

        // Room
        public void requestMakeRoom(Player _session, Packet _packet)
        {
            using (var p = new PangyaBinaryWriter(0x49))
            {
                p.WriteUInt16(2); // Error

                _session.Send(p);
            }
        }
        public void requestEnterRoom(Player _session, Packet _packet) { }
        public void requestChangeInfoRoom(Player _session, Packet _packet)
        {

        }
        public void requestExitRoom(Player _session, Packet _packet) { }
        public void requestShowInfoRoom(Player _session, Packet _packet) { }
        public void requestPlayerLocationRoom(Player _session, Packet _packet) { }
        public void requestChangePlayerStateReadyRoom(Player _session, Packet _packet) { }
        public void requestKickPlayerOfRoom(Player _session, Packet _packet) { }
        public void requestChangePlayerTeamRoom(Player _session, Packet _packet) { }
        public void requestChangePlayerStateAFKRoom(Player _session, Packet _packet) { }
        public void requestPlayerStateCharacterLounge(Player _session, Packet _packet) { }
        public void requestToggleAssist(Player _session, Packet _packet) { }
        public void requestInvite(Player _session, Packet _packet) { }
        public void requestCheckInvite(Player _session, Packet _packet) { } // Esse aqui o O Server Original nao retorna nada para o cliente, acho que é só um check
        public void requestChatTeam(Player _session, Packet _packet) { }

        // Request Player sai do Web Guild, verifica se tem alguma atualização para passar para o player no gs
        public void requestExitedFromWebGuild(Player _session, Packet _packet) { }

        // Request Game
        public void requestStartGame(Player _session, Packet _packet) { }
        public void requestInitHole(Player _session, Packet _packet) { }
        public void requestFinishLoadHole(Player _session, Packet _packet) { }
        public void requestFinishCharIntro(Player _session, Packet _packet) { }
        public void requestFinishHoleData(Player _session, Packet _packet) { }

        // Server enviou a resposta do InitShot para o cliente
        public void requestInitShotSended(Player _session, Packet _packet) { }

        public void requestInitShot(Player _session, Packet _packet) { }
        public void requestSyncShot(Player _session, Packet _packet) { }
        public void requestInitShotArrowSeq(Player _session, Packet _packet) { }
        public void requestShotEndData(Player _session, Packet _packet) { }
        public void requestFinishShot(Player _session, Packet _packet) { }

        public void requestChangeMira(Player _session, Packet _packet) { }
        public void requestChangeStateBarSpace(Player _session, Packet _packet) { }
        public void requestActivePowerShot(Player _session, Packet _packet) { }
        public void requestChangeClub(Player _session, Packet _packet) { }
        public void requestUseActiveItem(Player _session, Packet _packet) { }
        public void requestChangeStateTypeing(Player _session, Packet _packet) { }
        public void requestMoveBall(Player _session, Packet _packet) { }
        public void requestChangeStateChatBlock(Player _session, Packet _packet) { }
        public void requestActiveBooster(Player _session, Packet _packet) { }
        public void requestActiveReplay(Player _session, Packet _packet) { }
        public void requestActiveCutin(Player _session, Packet _packet) { }
        public void requestActivevarCommand(Player _session, Packet _packet) { }
        public void requestActiveAssistGreen(Player _session, Packet _packet) { }

        // VersusBase
        public void requestLoadGamePercent(Player _session, Packet _packet) { }
        public void requestMarkerOnCourse(Player _session, Packet _packet) { }
        public void requestStartTurnTime(Player _session, Packet _packet) { }
        public void requestUnOrPauseGame(Player _session, Packet _packet) { }
        public void requestLastPlayerFinishVersus(Player _session, Packet _packet) { }
        public void requestReplyContinueVersus(Player _session, Packet _packet) { }

        // Match
        public void requestTeamFinishHole(Player _session, Packet _packet) { }

        // Practice
        public void requestLeavePractice(Player _session, Packet _packet) { }

        // Tourney
        public void requestUseTicketReport(Player _session, Packet _packet) { }

        // Grand Zodiac
        public void requestLeaveChipInPractice(Player _session, Packet _packet) { }
        public void requestStartFirstHoleGrandZodiac(Player _session, Packet _packet) { }
        public void requestReplyInitialValueGrandZodiac(Player _session, Packet _packet) { }

        // Ability Item
        public void requestActiveRing(Player _session, Packet _packet) { }
        public void requestActiveRingGround(Player _session, Packet _packet) { }
        public void requestActiveRingPawsRainbowJP(Player _session, Packet _packet) { }
        public void requestActiveRingPawsRingSetJP(Player _session, Packet _packet) { }
        public void requestActiveRingPowerGagueJP(Player _session, Packet _packet) { }
        public void requestActiveRingMiracleSignJP(Player _session, Packet _packet) { }
        public void requestActiveWing(Player _session, Packet _packet) { }
        public void requestActivePaws(Player _session, Packet _packet) { }
        public void requestActiveGlove(Player _session, Packet _packet) { }
        public void requestActiveEarcuff(Player _session, Packet _packet) { }

        // Request Enter Game After Started
        public void requestEnterGameAfterStarted(Player _session, Packet _packet) { }

        public void requestFinishGame(Player _session, Packet _packet) { }

        public void requestChangeWindNextHoleRepeat(Player _session, Packet _packet) { }

        // Grand Prix
        public void requestEnterRoomGrandPrix(Player _session, Packet _packet) { }
        public void requestExitRoomGrandPrix(Player _session, Packet _packet) { }

        // Player Report Chat Game
        public void requestPlayerReportChatGame(Player _session, Packet _packet) { }

        // Common Command GM
        public void requestExecCCGVisible(Player _session, Packet _packet)
        {
            try
            {

                int visible = _packet.ReadInt16();

                //var r = m_rm.findRoom(_session.m_pi.mi.sala_numero);
                //BEGIN_FIND_ROOM(_session.m_pi.mi.sala_numero);

                //if (r == null && _session.m_pi.mi.sala_numero != -1)
                //    throw new exception("[channel::requestExecCCGVisible][Error] player[UID=" + (_session.m_pi.m_uid) + "] Channel[ID=" + (m_ci.id)
                //            + "] tentou executar o comando visible, mas nao encontrou a sala[NUMERO=" + (_session.m_pi.mi.sala_numero)
                //            + "] que esta nos dados dele. Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.CHANNEL, 10, 0x5700100));

                _session.m_gi.visible = _session.m_pi.mi.state_flag.visible = Convert.ToBoolean(visible);//0 é off, 1 on

                updatePlayerInfo(_session);

                //if (r != null) //update room game
                //    r.updatePlayerInfo(_session);

                // Log
                _smp::message_pool.push(new message("[channel::requestExecCCGVisible][Log] player[UID=" + (_session.m_pi.uid) + "] trocou VISIBLE STATUS[STATE="
                        + (Convert.ToBoolean(visible) ? ("ON") : ("OFF")) + "]", type_msg.CL_FILE_LOG_AND_CONSOLE));

                // UPDATE ON GAME
                sendUpdatePlayerInfo(_session, 3);

                // if (r != null)
                // r.sendCharacter(_session, 3);

                // END_FIND_ROOM;

            }
            catch (exception e)
            {

                _smp::message_pool.push(new message("[channel::requestExecCCGVisible][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                throw;
            }
        }
        public void requestExecCCGChangeWindVersus(Player _session, Packet _packet) { }
        public void requestExecCCGChangeWeather(Player _session, Packet _packet) { }
        public void requestExecCCGGoldenBell(Player _session, Packet _packet) { }
        public void requestExecCCGIdentity(Player _session, Packet _packet)
        {
            try
            {
                uCapability cap = new uCapability(_packet.ReadInt32());
                string nick = _packet.ReadPStr();

                // Verifica se session está varrizada para executar esse ação, 
                // se ele não fez o login com o Server ele não pode fazer nada até que ele faça o login
                //CHECK_SESSION_IS_AUTHORIZED("ExecCCGIdentity");

                if (nick.empty())
                    throw new exception("[channel::requestExecCCGIdentity][Error] player[UID=" + (_session.m_pi.uid)
                            + "] tentou executar o comando identity, mas o nick is empty. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.CHANNEL, 11, 0x5700100));

                if (string.Compare(nick, _session.m_pi.nickname) != 0)
                    throw new exception("[channel::requestExecCCGIdentity][Error] player[UID=" + (_session.m_pi.uid) + "] tentou executar o comando identity, mas o nick[NICK="
                            + nick + "] nao bate com o do player[NICK=" + (_session.m_pi.nickname) + "]. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.CHANNEL, 12, 0x5700100));

                if (!_session.m_pi.m_cap.gm_normal && !_session.m_pi.m_cap.game_master)
                    throw new exception("[channel::requestExecCCGIdentity][Error] player[UID=" + (_session.m_pi.uid)
                            + "] tentou executar o comando identity, mas ele nao eh gm e nunca foi. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.CHANNEL, 13, 0x5700100));

                ////var r = m_rm.findRoom(_session.m_pi.mi.sala_numero);
                //BEGIN_FIND_ROOM(_session.m_pi.mi.sala_numero);

                //if (r == null && _session.m_pi.mi.sala_numero != -1)
                //    throw new exception("[channel::requestExecCCGIdentity][Error] player[UID=" + (_session.m_pi.uid)
                //            + "] tentou executar o comando identity, mas nao encontrou a sala[NUMERO=" + (_session.m_pi.mi.sala_numero)
                //            + "] que esta nos dados dele. Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.CHANNEL, 14, 0x5700100));

                var p = new PangyaBinaryWriter();

                if (cap.ulCapability == -1)// negativo
                {   // player está tentando voltar a ser GM novament

                    // Valta para o GM
                    if (_session.m_pi.m_cap.gm_normal)
                    {
                        _session.m_pi.m_cap.gm_normal = false;//remove to flag set
                        _session.m_pi.m_cap.game_master = true; //set flag new
                        _session.m_pi.m_cap.title_gm = true;

                        updatePlayerInfo(_session);

                        //if (r != null)
                        //    r.updatePlayerInfo(_session);

                        // Log
                        _smp::message_pool.push(new message("[channel::requestExecCCGIdentity][Log] player[UID=" + (_session.m_pi.uid) + "] trocou a capacidade dele, para GM Total(Admin)", type_msg.CL_FILE_LOG_AND_CONSOLE));

                        _session.Send(packet_func.pacote09A(_session.m_pi.m_cap.ulCapability));

                        sendUpdatePlayerInfo(_session, 3);
                    }
                }
                else
                {

                    // [GM] Player Normal
                    if (cap.gm_normal)
                    {
                        _session.m_pi.m_cap.gm_normal = true;
                        Console.WriteLine("code2:\n" + _session.m_pi.m_cap.ToString());

                        updatePlayerInfo(_session);

                        //if (r != null)
                        //    r.updatePlayerInfo(_session);

                        // Log
                        _smp::message_pool.push(new message("[channel::requestExecCCGIdentity][Log] player[UID=" + (_session.m_pi.uid) + "] trocou a capacidade dele, para GM Normal(user normal)", type_msg.CL_FILE_LOG_AND_CONSOLE));

                        // UPDATE ON GAME 
                        _session.Send(packet_func.pacote09A(_session.m_pi.m_cap.ulCapability));

                        sendUpdatePlayerInfo(_session, 3);

                        //if (r != null)
                        //    r.sendCharacter(_session, 3);
                    }
                }

                //END_FIND_ROOM;

            }
            catch (exception e)
            {

                _smp::message_pool.push(new message("[channel::requestExecCCGIdentity][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                throw;
            }
        }
        public void requestExecCCGKick(Player _session, Packet _packet) { }
        public void requestExecCCGDestroy(Player _session, Packet _packet) { }

        // MyRoom
        public void requestChangePlayerItemMyRoom(Player _session, Packet _packet)
        {
            byte type = 0;
            uint item_id;
            int error = 4;

            try
            {

                type = _packet.ReadByte();

                switch (type)
                {
                    case 0: // Character Equipado Parts Complete
                        {
                            CharacterInfo ci, pCe = null;

                            ci = (CharacterInfo)_packet.Read(new CharacterInfo());
                            if (ci.id != 0 && (pCe = _session.m_pi.findCharacterById(ci.id)) != null
                                    && (sIff.getInstance()._getItemGroupIdentify(pCe._typeid) == PangyaAPI.IFF.JP.Models.Flags.IFF_GROUP.CHARACTER && sIff.getInstance()._getItemGroupIdentify(ci._typeid) == PangyaAPI.IFF.JP.Models.Flags.IFF_GROUP.CHARACTER))
                            {

                                // Checks Parts Equiped
                                _session.checkCharacterEquipedPart(ci);

                                // Check auxparts Equiped
                                _session.checkCharacterEquipedAuxPart(ci);

                                pCe = ci;

                                NormalManagerDB.add(0, new Cmd.CmdUpdateCharacterAllPartEquiped(_session.m_pi.uid, ci), null/*SQLDBResponse*/, /*this*/ null);

                                _session.m_pi.mp_ce[ci.id] = ci;
                                _session.m_pi.ei.char_info = ci;
                            }
                            else
                            {

                                error = (ci.id == 0) ? 1/*Invalid Item Id*/ : (pCe == null ? 2/*Not Found Item*/ : 3/*Item Typeid is Wrong*/);

                                _smp::message_pool.push(new message("[channel::requestChangePlayerItemMyRoom][Error] player[UID=" + (_session.m_pi.uid)
                                        + "] tentou Atualizar os Parts do Character[ID=" + (ci.id) + "], mas deu Error[VALUE=" + (error)
                                        + "]. Hacker ou Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));
                            }



                            _session.Send(packet_func.pacote06B(_session.m_pi, type, error));
                            break;
                        }
                    case 5: // only Character ID EQUIPADO
                        {
                            CharacterInfo pCe = null;

                            if ((item_id = _packet.ReadUInt32()) != 0 && (pCe = _session.m_pi.findCharacterById(item_id)) != null
                                  && sIff.getInstance()._getItemGroupIdentify(pCe._typeid) == PangyaAPI.IFF.JP.Models.Flags.IFF_GROUP.CHARACTER)
                            {

                                _session.m_pi.ei.char_info = pCe;
                                _session.m_pi.ue.character_id = item_id;

                                updatePlayerInfo(_session);

                                PlayerCanalInfo pci = getPlayerInfo(_session);
                                _session.Send(packet_func.pacote06B(_session.m_pi, type, error));

                                // Update ON DB
                                NormalManagerDB.add(0, new CmdUpdateCharacterEquiped(_session.m_pi.uid, (int)item_id), null, this);

                            }
                            else
                            {

                                error = (item_id == 0) ? 1/*Invalid Item Id*/ : (pCe == null ? 2/*Not Found Item*/ : 3/*Item Typeid is Wrong*/);

                                _smp::message_pool.push(new message("[channel::requestChangePlayerItemMyRoom][Error] player[UID=" + (_session.m_pi.uid)
                                        + "] tentou equipar o Character[ID=" + (item_id) + "], mas deu Error[VALUE=" + (error)
                                        + "]. Hacker ou Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));
                            }

                            _session.Send(packet_func.pacote06B(_session.m_pi, type, error));

                            break;
                        }
                    default:
                        _session.Send(packet_func.pacote06B(_session.m_pi, type, error));//teste
                        break;
                }
            }
            catch (exception e)
            {
                _session.Send(packet_func.pacote06B(_session.m_pi, type, 1));

                _smp::message_pool.push(new message("[channel::requestChangePlayerItemMyRoom][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
        }
        public void requestOpenTicketReportScroll(Player _session, Packet _packet) { }
        public void requestChangeMascotMessage(Player _session, Packet _packet) { }

        // Caddie Extend Days and Set Notice Holyday Caddie
        public void requestPayCaddieHolyDay(Player _session, Packet _packet) { }
        public void requestSetNoticeBeginCaddieHolyDay(Player _session, Packet _packet) { }

        // Shop

        public void requestEnterShop(Player _session, Packet packet)
        {
            try
            {
                if (_session.m_pi.block_flag.m_flag.buy_and_gift_shop)
                    throw new exception("[channel::requestEnterShop][Error] player[UID=" + (_session.m_pi.uid)
                            + "] tentou jogar no Papel Shop, mas ele nao pode. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.CHANNEL, 3, 0x790002));


                var p = new PangyaBinaryWriter((ushort)0x20E);

                p.Write(0);
                p.Write(0); // Não sei pode ACHO "ser Value acho, ou erro, pode ser dizendo que o shop esta bloqueado"

                _session.Send(p);
            }
            catch (exception e)
            {
                throw e;
            }
        }
        public void requestBuyItemShop(Player _session, Packet _packet) { }
        public void requestGiftItemShop(Player _session, Packet _packet) { }

        // MyRoom Extend or Remove Part Rental
        public void requestExtendRental(Player _session, Packet _packet) { }
        public void requestDeleteRental(Player _session, Packet _packet) { }

        // Attendance reward, Premios por logar no pangya
        public void requestCheckAttendanceReward(Player _session, Packet _packet)
        {
            try
            {                   // Attendance Reward System
                if (!sAttendanceRewardSystem.getInstance().isLoad())
                    sAttendanceRewardSystem.getInstance().load();

                sAttendanceRewardSystem.getInstance().requestCheckAttendance(_session, _packet);
            }
            catch (exception e)
            {
                _smp::message_pool.push(new message("[channel::requestCheckAttendanceReward][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                throw;
            }
        }

        public void requestAttendanceRewardLoginCount(Player _session, Packet _packet)
        {
            try
            {                   // Attendance Reward System
                if (!sAttendanceRewardSystem.getInstance().isLoad())
                    sAttendanceRewardSystem.getInstance().load();

                sAttendanceRewardSystem.getInstance().requestUpdateCountLogin(_session, _packet);
            }
            catch (exception e)
            {
                _smp::message_pool.push(new message("[channel::requestCheckAttendanceReward][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                throw;
            }

        }

        // Daily Quest
        public void requestDailyQuest(Player _session, Packet _packet) { }
        public void requestAcceptDailyQuest(Player _session, Packet _packet) { }
        public void requestTakeRewardDailyQuest(Player _session, Packet _packet) { }
        public void requestLeaveDailyQuest(Player _session, Packet _packet) { }

        // Cadie's Cauldron
        public void requestCadieCauldronExchange(Player _session, Packet _packet) { }

        // Character Stats
        public void requestCharacterStatsUp(Player _session, Packet _packet) { }
        public void requestCharacterStatsDown(Player _session, Packet _packet) { }
        public void requestCharacterMasteryExpand(Player _session, Packet _packet) { }
        public void requestCharacterCardEquip(Player _session, Packet _packet) { }
        public void requestCharacterCardEquipWithPatcher(Player _session, Packet _packet) { }
        public void requestCharacterRemoveCard(Player _session, Packet _packet) { }

        // ClubSet Enchant
        public void requestClubSetStatsUpdate(Player _session, Packet _packet) { }

        // Tiki's Shop
        public void requestTikiShopExchangeItem(Player _session, Packet _packet) { }

        // Item Equipado
        public void requestChangePlayerItemChannel(Player _session, Packet _packet)
        {
            byte type = 255;
            uint item_id;
            int error = 0/*SUCCESS*/;
            try
            {

                type = _packet.ReadByte();
                switch (type)
                {
                    case 1: // Caddie
                        {
                            CaddieInfoEx pCi = null;

                            // Caddie
                            if ((item_id = _packet.ReadUInt32()) != 0 && (pCi = _session.m_pi.findCaddieById(item_id)) != null &&

                         sIff.getInstance()._getItemGroupIdentify(pCi._typeid) == PangyaAPI.IFF.JP.Models.Flags.IFF_GROUP.CADDIE)
                            {

                                // Check if item is in map of update item
                                var v_it = _session.m_pi.findUpdateItemByTypeidAndId(pCi._typeid, pCi.id);

                                if (v_it.Any())
                                {

                                    foreach (var el in v_it)
                                    {

                                        if (el.Value.type == UpdateItem.UI_TYPE.CADDIE)
                                        {

                                            // Desequipa o caddie
                                            _session.m_pi.ei.cad_info = null;
                                            _session.m_pi.ue.caddie_id = 0;

                                            item_id = 0;

                                        }
                                        else if (el.Value.type == UpdateItem.UI_TYPE.CADDIE_PARTS)
                                        {

                                            // Limpa o caddie Parts
                                            pCi.parts_typeid = 0u;
                                            pCi.parts_end_date_unix = 0;
                                            pCi.end_parts_date = new PangyaTime();

                                            _session.m_pi.ei.cad_info = pCi;
                                            _session.m_pi.ue.caddie_id = item_id;
                                        }

                                        // Tira esse Update Item do map
                                        _session.m_pi.mp_ui.Remove(el.Key);
                                    }

                                }
                                else
                                {

                                    // Caddie is Good, Update caddie equiped ON SERVER AND DB
                                    _session.m_pi.ei.cad_info = pCi;
                                    _session.m_pi.ue.caddie_id = item_id;

                                    // Verifica se o caddie pode ser equipado
                                    if (_session.checkCaddieEquiped(_session.m_pi.ue))
                                        item_id = _session.m_pi.ue.caddie_id;

                                }

                                // Update ON DB
                                NormalManagerDB.add(0, new CmdUpdateCaddieEquiped(_session.m_pi.uid, (int)item_id), null /*channel::SQLDBResponse*/, this);

                            }
                            else if (_session.m_pi.ue.caddie_id > 0 && _session.m_pi.ei.cad_info != null)
                            {  // Desequipa Caddie

                                error = (item_id == 0) ? 1/*client give invalid item id*/ : (pCi == null ? 2/*Item Not Found*/ : 3/*Erro item typeid invalid*/);

                                if (error > 1)
                                {
                                    _smp::message_pool.push(new message("[channel::requestChangePlayerItemChannel][Log][WARNING] player[UID=" + (_session.m_pi.uid)
                                            + "] tentou trocar o Caddie[ID=" + (item_id) + "], mas deu Error[VALUE="
                                            + (error) + "], desequipando o caddie. Hacker ou Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));
                                }

                                // Check if item is in map of update item
                                var v_it = _session.m_pi.findUpdateItemByTypeidAndId(_session.m_pi.ei.cad_info._typeid, _session.m_pi.ei.cad_info.id);

                                if (v_it.Any())
                                {

                                    foreach (var el in v_it)
                                    {

                                        // Caddie já vai se desequipar, só verifica o parts
                                        if (el.Value.type == UpdateItem.UI_TYPE.CADDIE_PARTS)
                                        {

                                            // Limpa o caddie Parts
                                            _session.m_pi.ei.cad_info.parts_typeid = 0u;
                                            _session.m_pi.ei.cad_info.parts_end_date_unix = 0;
                                            _session.m_pi.ei.cad_info.end_parts_date = new PangyaTime();
                                        }

                                        // Tira esse Update Item do map
                                        _session.m_pi.mp_ui.Remove(el.Key);
                                    }

                                }

                                _session.m_pi.ei.cad_info = null;
                                _session.m_pi.ue.caddie_id = 0;

                                item_id = 0;

                                // Zera o Error para o cliente desequipar o caddie que o server desequipou
                                error = 0;

                                // Att No DB
                                NormalManagerDB.add(0, new CmdUpdateCaddieEquiped(_session.m_pi.uid, (int)item_id), null /*channel::SQLDBResponse*/, this);

                            } // else Não tem nenhum caddie equipado, para desequipar, então o cliente só quis atualizar o estado

                        }
                        break;
                    case 2: // Ball
                        {
                            WarehouseItemEx pWi = null;

                            if ((item_id = _packet.ReadUInt32()) != 0 && (pWi = _session.m_pi.findWarehouseItemByTypeid(item_id)) != null
                                    && sIff.getInstance()._getItemGroupIdentify(pWi._typeid) == PangyaAPI.IFF.JP.Models.Flags.IFF_GROUP.BALL)
                            {

                                _session.m_pi.ei.comet = pWi;
                                _session.m_pi.ue.ball_typeid = item_id;     // Ball(Comet) é o typeid que o cliente passa

                                // Verifica se a bola pode ser equipada
                                //  if (_session.checkBallEquiped(_session.m_pi.ue))
                                item_id = _session.m_pi.ue.ball_typeid;

                                // Update ON DB
                                NormalManagerDB.add(0, new CmdUpdateBallEquiped(_session.m_pi.uid, item_id), null, this);

                            }
                            else if (item_id == 0)
                            { // Bola 0 coloca a bola padrão para ele, se for premium user coloca a bola de premium user

                                // Zera para equipar a bola padrão
                                _session.m_pi.ei.comet = null;
                                _session.m_pi.ue.ball_typeid = 0;

                                // Verifica se a Bola pode ser equipada (Coloca para equipar a bola padrão
                                //if (_session.checkBallEquiped(_session.m_pi.ue))
                                item_id = _session.m_pi.ue.ball_typeid;

                                // Update ON DB
                                NormalManagerDB.add(0, new CmdUpdateBallEquiped(_session.m_pi.uid, item_id), null, this);

                            }
                            else
                            {

                                error = (pWi == null ? 2/*Not Found Item*/ : 3/*Item Type is Wrong*/);

                                message_pool.push(new message("[channel::requestChangePlayerItemChannel][Error] player[UID=" + (_session.m_pi.uid)
                                        + "] tentou trocar Ball[TYPEID=" + (item_id) + "], mas deu Error[VALUE=" + (error)
                                        + "], Equipando Ball Padrao. Hacker ou Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));

                                pWi = _session.m_pi.findWarehouseItemByTypeid(DEFAULT_COMET_TYPEID);

                                if (pWi != null)
                                {

                                    message_pool.push(new message("[channel::requestChangePlayerItemChannel][Log][WARNING] player[UID=" + (_session.m_pi.uid)
                                            + "] tentou trocar a Ball[TYPEID=" + (item_id) + "], mas deu Error[VALUE="
                                            + (error) + "], colocando a Ball Padrao do player. Hacker ou Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));

                                    _session.m_pi.ei.comet = pWi;
                                    item_id = _session.m_pi.ue.ball_typeid = pWi._typeid;

                                    // Zera o Error para o cliente equipar a Ball Padrão que o server equipou
                                    error = 0;

                                    // Update ON DB
                                    NormalManagerDB.add(0, new CmdUpdateBallEquiped(_session.m_pi.uid, item_id), null, this);

                                }
                                else
                                {

                                }
                            }

                        }
                        break;
                    case 3: // ClubSet
                        {
                            WarehouseItemEx pWi = null;

                            // ClubSet
                            if ((item_id = _packet.ReadUInt32()) != 0 && (pWi = _session.m_pi.findWarehouseItemByTypeid(item_id)) != null
                                            && sIff.getInstance()._getItemGroupIdentify(pWi._typeid) == PangyaAPI.IFF.JP.Models.Flags.IFF_GROUP.CLUBSET)
                            {

                                var c_it = _session.m_pi.findUpdateItemByTypeidAndType(item_id, UpdateItem.UI_TYPE.WAREHOUSE);

                                if (c_it.Any())
                                {

                                    _session.m_pi.ei.clubset = pWi;

                                    // Esse C do WarehouseItem, que pega do DB, não é o ja updado inicial da taqueira é o que fica tabela enchant, 
                                    // que no original fica no warehouse msm, eu só confundi quando fiz
                                    _session.m_pi.ei.csi.setValues(pWi.id, pWi._typeid, pWi.c);

                                    var cs = sIff.getInstance().findClubSet(pWi._typeid);

                                    if (cs != null)
                                    {

                                        for (var j = 0; j < 5; ++j)
                                            _session.m_pi.ei.csi.enchant_c[j] = (short)(cs.SlotStats.getSlot[j] + pWi.clubset_workshop.c[j]);

                                        _session.m_pi.ue.clubset_id = item_id;

                                        // Verifica se o ClubSet pode ser equipado
                                        //if (_session.checkClubSetEquiped(_session.m_pi.ue))
                                        item_id = _session.m_pi.ue.clubset_id;

                                        // Update ON DB
                                        NormalManagerDB.add(0, new CmdUpdateClubsetEquiped(_session.m_pi.uid, (int)item_id), null/*channel::SQLDBResponse*/, this);

                                    }
                                }
                            }
                        }
                        break;
                    case 4:
                        {
                            CharacterInfo pCe = null;
                            if ((item_id = _packet.ReadUInt32()) != 0 && (pCe = _session.m_pi.findCharacterById(item_id)) != null
                                    && sIff.getInstance()._getItemGroupIdentify(pCe._typeid) == PangyaAPI.IFF.JP.Models.Flags.IFF_GROUP.CHARACTER)
                            {

                                _session.m_pi.ei.char_info = pCe;
                                _session.m_pi.ue.character_id = item_id;

                                // Update ON DB
                                NormalManagerDB.add(0, new CmdUpdateCharacterEquiped(_session.m_pi.uid, (int)item_id), null, null);

                                // Update Player Info Channel and Room
                                updatePlayerInfo(_session);

                            }
                            else
                            {

                                error = (item_id == 0) ? 1/*client give invalid item id*/ : (pCe == null ? 2/*Item Not Found*/ : 3/*Erro item typeid invalid*/);

                                if (_session.m_pi.mp_ce.Count > 0)
                                {

                                    _smp::message_pool.push(new message("[channel::requestChangePlayerItemChannel][Log][WARNING] player[UID=" + (_session.m_pi.uid)
                                            + "] tentou trocar o Character[ID=" + (item_id) + "], mas deu Error[VALUE="
                                            + (error) + "], colocando o primeiro character do player. Hacker ou Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));

                                    _session.m_pi.ei.char_info = _session.m_pi.mp_ce.First().Value;
                                    item_id = _session.m_pi.ue.character_id = _session.m_pi.ei.char_info.id;

                                    // Zera o Error para o cliente equipar o Primeiro Character do map de character do player, que o server equipou
                                    error = 0;

                                    // Update ON DB
                                    NormalManagerDB.add(0, new CmdUpdateCharacterEquiped(_session.m_pi.uid, (int)item_id), null, this);

                                    // Update Player Info Channel and Room
                                    updatePlayerInfo(_session);

                                }
                            }
                        }
                        break;
                    case 5: // Mascot
                        {
                            MascotInfoEx pMi = null;

                            if ((item_id = _packet.ReadUInt32()) != 0)
                            {

                                if ((pMi = _session.m_pi.findMascotById(item_id)) != null && sIff.getInstance()._getItemGroupIdentify(pMi._typeid) == PangyaAPI.IFF.JP.Models.Flags.IFF_GROUP.MASCOT)
                                {

                                    var m_it = _session.m_pi.findUpdateItemByTypeidAndType(_session.m_pi.ue.mascot_id, UpdateItem.UI_TYPE.MASCOT);

                                    if (m_it != null)
                                    {

                                        // Desequipa o Mascot que acabou o tempo dele
                                        _session.m_pi.ei.mascot_info = null;
                                        _session.m_pi.ue.mascot_id = 0;

                                        item_id = 0;

                                    }
                                    else
                                    {

                                        // Mascot is Good, Update mascot equiped ON SERVER AND DB
                                        _session.m_pi.ei.mascot_info = pMi;
                                        _session.m_pi.ue.mascot_id = item_id;

                                        // Verifica se o Mascot pode ser equipado
                                        //if (_session.checkMascotEquiped(_session.m_pi.ue))
                                        item_id = _session.m_pi.ue.mascot_id;

                                    }

                                    // Update ON DB
                                    NormalManagerDB.add(0, new CmdUpdateMascotEquiped(_session.m_pi.uid, (int)item_id), null/*channel::SQLDBResponse*/, this);

                                }
                                else
                                {

                                    error = (item_id == 0) ? 1/*client give invalid item id*/ : (pMi == null ? 2/*Item Not Found*/ : 3/*Erro item typeid invalid*/);

                                    if (error > 1)
                                    {
                                        _smp::message_pool.push(new message("[channel::requestChangePlayerItemChannel][Log][WARNING] player[UID=" + (_session.m_pi.uid)
                                                + "] tentou trocar o Mascot[ID=" + (item_id) + "], mas deu Error[VALUE="
                                                + (error) + "], desequipando o Mascot. Hacker ou Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));
                                    }

                                    _session.m_pi.ei.mascot_info = null;
                                    _session.m_pi.ue.mascot_id = 0;

                                    item_id = 0;

                                    // Att No DB
                                    NormalManagerDB.add(0, new CmdUpdateMascotEquiped(_session.m_pi.uid, (int)item_id), null/* channel::SQLDBResponse*/, this);
                                }

                            }
                            else if (_session.m_pi.ue.mascot_id > 0 && _session.m_pi.ei.mascot_info != null)
                            {   // Desequipa Mascot

                                _session.m_pi.ei.mascot_info = null;
                                _session.m_pi.ue.mascot_id = 0;

                                item_id = 0;

                                // Att No DB
                                NormalManagerDB.add(0, new CmdUpdateMascotEquiped(_session.m_pi.uid, (int)item_id), null/* channel::SQLDBResponse*/, this);

                            } // else Não tem nenhum mascot equipado, para desequipar, então o cliente só quis atualizar o estado

                            break;
                        }
                    default:
                        break;
                }
                _session.Send(packet_func.pacote04B(_session, type, error));
            }
            catch (exception e)
            {

                message_pool.push(new message("[channel::requestChangePlayerItemChannel][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                _session.Send(packet_func.pacote04B(_session, type, (int)(ExceptionError.STDA_SOURCE_ERROR_DECODE_TYPE(e.getCodeError()) == (uint)STDA_ERROR_TYPE.CHANNEL ? ExceptionError.STDA_SOURCE_ERROR_DECODE_TYPE(e.getCodeError()) : 1/*Unknown Error*/)));

            }
        }
        public void requestChangePlayerItemRoom(Player _session, Packet _packet)
        {
            _session.Send(packet_func.pacote04B(_session, _packet.ReadByte(), 0));
        }

        // Delete Active Item
        public void requestDeleteActiveItem(Player _session, Packet _packet) { }

        // ClubSet WorkShop
        public void requestClubSetWorkShopTransferMasteryPts(Player _session, Packet _packet) { }
        public void requestClubSetWorkShopRecoveryPts(Player _session, Packet _packet) { }
        public void requestClubSetWorkShopUpLevel(Player _session, Packet _packet) { }
        public void requestClubSetWorkShopUpLevelConfirm(Player _session, Packet _packet) { }
        public void requestClubSetWorkShopUpLevelCancel(Player _session, Packet _packet) { }
        public void requestClubSetWorkShopUpRank(Player _session, Packet _packet) { }
        public void requestClubSetWorkShopUpRankTransformConfirm(Player _session, Packet _packet) { }
        public void requestClubSetWorkShopUpRankTransformCancel(Player _session, Packet _packet) { }

        // ClubSet Reset
        public void requestClubSetReset(Player _session, Packet _packet) { }

        // Tutorial
        public void requestMakeTutorial(Player _session, Packet _packet) { }

        // Web Link
        public void requestEnterWebLinkState(Player _session, Packet _packet)
        {
            try
            {
                // Att Lugar que o player está, ele está vendo weblink
                _session.m_pi.place = _packet.ReadSByte();

            }
            catch (exception e)
            {

                message_pool.push(new message("[channel::requestEnterWebLinkState][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
        }

        // Pede o Cookies
        public void requestCookie(Player _session, Packet _packet)
        {
            try
            {

                // Verifica se session está varrizada para executar esse ação, 
                // se ele não fez o login com o Server ele não pode fazer nada até que ele faça o login
                // CHECK_SESSION_IS_AUTHORIZED("Cookie");

                // Sempre atualiza o Cookie do server com o valor que está no banco de dados

                // Update cookie do server com o que está no banco de dados
                _session.m_pi.updateCookie();

                _session.Send(packet_func.pacote096(_session.m_pi));

                // Vou colocar aqui para atualizar os Grand Zodiac Pontos por que quando eu fazer o evento o Grand Zodiac ele vai consumir os pontos na página web, 
                // aí vou atualizar aqui com o do banco de dados
                var cmd_gzp = new CmdGrandZodiacPontos(_session.m_pi.uid, CmdGrandZodiacPontos.eCMD_GRAND_ZODIAC_TYPE.CGZT_GET);

                NormalManagerDB.add(0, cmd_gzp, null, null);

                if (cmd_gzp.getException().getCodeError() != 0)
                    throw cmd_gzp.getException();

                _session.m_pi.grand_zodiac_pontos = cmd_gzp.getPontos();

            }
            catch (exception e)
            {

                _smp::message_pool.push(new message("[channel::requestCookie][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
        }

        // Pede para atualizar Gacha Coupon(s)
        public void requestUpdateGachaCoupon(Player _session, Packet _packet) { }

        // Box System, Box que envia para o MailBox e a Box que envia direto para o MyRoom
        public void requestOpenBoxMail(Player _session, Packet _packet) { }
        public void requestOpenBoxMyRoom(Player _session, Packet _packet) { }

        // Memorial System
        public void requestPlayMemorial(Player _session, Packet _packet)
        {
            PangyaBinaryWriter p= new PangyaBinaryWriter();

            try
            {


                if (_session.m_pi.block_flag.m_flag.memorial_shop)
                    throw new exception("[channel::requestPlayerMemorial][Error] player[UID=" + (_session.m_pi.uid)
                            + "] tentou jogar no Memorial Shop, mas ele nao pode. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.CHANNEL, 6, 0x790001));

                if (!sMemorialSystem.getInstance().isLoad())
                    sMemorialSystem.getInstance().load();

                var coin_typeid = _packet.ReadUInt32();

                if (coin_typeid == 0)
                    throw new exception("[channel::requestPlayMemorial][Error] player[UID=" + (_session.m_pi.uid) + "] tentou jogar Memorial com a coin[TYPEID="
                            + (coin_typeid) + "], mas o coin_typeid is invalid(zero). Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.CHANNEL, 1, 0x6300301));

                if (sIff.getInstance().getItemGroupIdentify(coin_typeid) != sIff.getInstance().ITEM)
                    throw new exception("[channel::requestPlayMemorial][Error] player[UID=" + (_session.m_pi.uid) + "] tentou jogar Memorial com a coin[TYPEID="
                            + (coin_typeid) + "], mas a coin is not Item Valid. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.CHANNEL, 2, 0x6300302));

                var pWi = _session.m_pi.findWarehouseItemByTypeid(coin_typeid);

                if (pWi == null)
                    throw new exception("[channel::requestPlayMemorial][Error] player[UID=" + (_session.m_pi.uid) + "] tentou jogar Memorial com a coin[TYPEID="
                            + (coin_typeid) + "], mas o ele nao possui a coin. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.CHANNEL, 3, 0x6300303));

                var coin = sIff.getInstance().findItem(pWi._typeid);

                if (coin == null || !coin.Active)
                    throw new exception("[channel::requestPlayMemorial][Error] player[UID=" + (_session.m_pi.uid) + "] tentou jogar Memorial com a coin[TYPEID="
                            + (coin_typeid) + "], mas nao tem a coin na IFF_STRUCT do Server. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.CHANNEL, 4, 0x6300304));

                // Achievement System
                //SysAchievement sys_achieve;

                // Memorial System
                var c = sMemorialSystem.getInstance().findCoin(coin.ID);

                if (c == null)
                    throw new exception("[channel::requestPlayMemorial][Error] player[UID=" + (_session.m_pi.uid) + "] tentou jogar Memorial com a coin[TYPEID="
                            + (coin_typeid) + "], mas nao tem essa coin no Memorial System do Server. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.CHANNEL, 5, 0x6300305));

                //// Achievement add + 1 ao contador de Play Coin no memorial shop
                //if (c.tipo == MEMORIAL_COIN_TYPE::MCT_NORMAL)
                //    sys_achieve.incrementCounter(0x6C4000B2u/*Normal Coin*/);
                //else if (c.tipo == MEMORIAL_COIN_TYPE::MCT_SPECIAL)
                //    sys_achieve.incrementCounter(0x6C4000B3u/*Special Coin*/);

                var win_item = sMemorialSystem.getInstance().drawCoin(_session, c);

                if (win_item.empty())
                    throw new exception("[channel::requestPlayMemorial][Error] player[UID=" + (_session.m_pi.uid) + "] tentou jogar Memorial com a coin[TYPEID="
                            + (coin_typeid) + "], mas não conseguiu sortear um item do memorial shop. Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.CHANNEL, 6, 0x6300306));

                List<stItem> v_item = new List<stItem>();
                stItem item = new stItem();

                // Init Item Ganho
                BuyItem bi = new BuyItem();
                Mascot mascot = null;

                foreach (var el in win_item)
                {
                    item.clear();

                    bi.id = -1;
                    bi._typeid = el._typeid;

                    // Check se é Mascot, para colocar por dia o tempo que é a quantidade
                    if (sIff.getInstance().getItemGroupIdentify(el._typeid) == sIff.getInstance().MASCOT && (mascot = sIff.getInstance().findMascot(el._typeid)) != null
                        && mascot.Shop.flag_shop.time_shop.dia > 0 && mascot.Shop.flag_shop.time_shop.active)
                    {   // é Mascot por Tempo
                        bi.qntd = 1;
                        bi.time = (short)el.qntd;
                    }
                    else
                        bi.qntd = el.qntd;

                    item_manager.initItemFromBuyItem(_session.m_pi, ref item, bi, false, 0, 0, 1);

                    if (item._typeid == 0)
                        throw new exception("[channel::requestPlayMemorial][Error] player[UID=" + (_session.m_pi.uid) + "] tentou jogar Memorial com a coin[TYPEID="
                                + (coin_typeid) + "], mas nao conseguiu inicializar o Item[TYPEID=" + (bi._typeid) + "]", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.CHANNEL, 7, 0x6300307));

                    // Verifica se já possui o item, o caddie item verifica se tem o caddie para depois verificar se tem o caddie item
                    if ((sIff.getInstance().IsCanOverlapped(item._typeid) && sIff.getInstance().getItemGroupIdentify(item._typeid) != sIff.getInstance().CAD_ITEM) || !_session.m_pi.ownerItem(item._typeid))
                    {
                        if (item_manager.isSetItem(item._typeid))
                        {
                            var v_stItem = item_manager.getItemOfSetItem(_session, item._typeid, false, 1/*Não verifica o Level*/);

                            if (!v_stItem.empty())
                            {
                                // Já verificou lá em cima se tem os item so set, então não precisa mais verificar aqui
                                // Só add eles ao vector de venda
                                // Verifica se pode ter mais de 1 item e se não ver se não tem o item
                                foreach (var _el in v_stItem)
                                    if ((sIff.getInstance().IsCanOverlapped(_el._typeid) && sIff.getInstance().getItemGroupIdentify(el._typeid) != sIff.getInstance().CAD_ITEM) || !_session.m_pi.ownerItem(_el._typeid))
                                        v_item.Add(_el);
                            }
                            else
                                throw new exception("[channel::requestPlayMemorial][Error] player[UID=" + (_session.m_pi.uid) + "] tentou jogar Memorial com a coin[TYPEID="
                                        + (coin_typeid) + "], mas SetItem que ele ganhou no Memorial Shop, nao tem Item[TYPEID=" + (bi._typeid) + "]. Bug.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.CHANNEL, 8, 0x6300308));
                        }
                        else
                            v_item.Add(item);

                    }
                    else if (sIff.getInstance().getItemGroupIdentify(item._typeid) == sIff.getInstance().CAD_ITEM)
                        throw new exception("[channel::requestPlayMemorial][Error] player[UID=" + (_session.m_pi.uid) + "] tentou jogar Memorial com a coin[TYPEID="
                                + (coin_typeid) + "], mas o CaddieItem que ele ganhou, nao tem o caddie, Item[TYPEID=" + (bi._typeid) + "]. Bug.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.CHANNEL, 9, 0x6300309));
                    else
                        throw new exception("[channel::requestPlayMemorial][Error] player[UID=" + (_session.m_pi.uid) + "] tentou jogar Memorial com a coin[TYPEID="
                                + (coin_typeid) + "], mas ele ja tem o Item[TYPEID=" + (bi._typeid) + "]. Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.CHANNEL, 10, 0x6300310));

                    // Achievement add +1 ao contador de item raro que ganhou
                    //if (el.tipo >= 0 && el.tipo < 3)
                    //    sys_achieve.incrementCounter(0x6C4000B5u/*Rare Win*/);
                    //else if (el.tipo >= 3)
                    //    sys_achieve.incrementCounter(0x6C4000B4u/*Super Rare Win*/);
                }


                // UPDATE ON SERVER AND DB

                // Delete Coin
                item.clear();

                item.type = 2;
                item.id = (int)pWi.id;
                item._typeid = c._typeid;
                item.qntd = 1;
                item.STDA_C_ITEM_QNTD = (ushort)(item.qntd * -1);

                //        if (item_manager.removeItem(item, _session) <= 0)
                //            throw new exception("[channel::requestPlayMemorial][Error] player[UID=" + (_session.m_pi.uid) + "] tentou jogar Memorial com a coin[TYPEID="
                //                    + (coin_typeid) + "], mas nao conseguiu deletar Coin. Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.CHANNEL, 11, 0x6300311));

                //        // Add ao vector depois que add os itens ganho no memorial

                //        std::string str = "";

                //        // Coloca Item ganho no My Room do player
                //        var rai = item_manager.addItem(v_item, _session, 0, 0);

                //        if (rai.fails.Count() > 0 && rai.type != item_manager.RetAddItem::T_SUCCESS_PANG_AND_EXP_AND_CP_POUCH)
                //        {

                //            for (var i = 0u; i < v_item.Count(); ++i)
                //            {
                //                if (i == 0)
                //                    str += "[TYPEID=" + (v_item[i]._typeid) + ", ID=" + (v_item[i].id) + ", QNTD=" + ((v_item[i].qntd > 0xFFu) ? v_item[i].qntd : v_item[i].STDA_C_ITEM_QNTD)
                //                        + (v_item[i].STDA_C_ITEM_TIME > 0 ? ", TEMPO=" + (v_item[i].STDA_C_ITEM_TIME) : std::string("")) +"]";

                //        else
                //                str += ", [TYPEID=""" + (v_item[i]._typeid) + ", ID=" + (v_item[i].id) + ", QNTD=" + ((v_item[i].qntd > 0xFFu) ? v_item[i].qntd : v_item[i].STDA_C_ITEM_QNTD)
                //                    + (v_item[i].STDA_C_ITEM_TIME > 0 ? ", TEMPO=" + (v_item[i].STDA_C_ITEM_TIME) : std::string("")) +"]";
                //        }

                //        throw new exception("[channel::requestPlayMemorial][Error] player[UID=" + (_session.m_pi.uid) + "] tentou jogar Memorial com a coin[TYPEID="
                //                + (coin_typeid) + "], mas ele nao conseguiu adicionar os item(ns){" + str + "}. Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.CHANNEL, 12, 0x6300312));
                //    }else
                //    {
                //        // Init Item Add Log
                //        for (var i = 0u; i < v_item.Count(); ++i)
                //        {
                //            if (i == 0)
                //                str += "[TYPEID=" + (v_item[i]._typeid) + ", ID=" + (v_item[i].id) + ", QNTD=" + ((v_item[i].qntd > 0xFFu) ? v_item[i].qntd : v_item[i].STDA_C_ITEM_QNTD)
                //                    + (v_item[i].STDA_C_ITEM_TIME > 0 ? ", TEMPO=" + (v_item[i].STDA_C_ITEM_TIME) : std::string("")) +"]";

                //        else
                //            str += ", [TYPEID=""" + (v_item[i]._typeid) + ", ID=" + (v_item[i].id) + ", QNTD=" + ((v_item[i].qntd > 0xFFu) ? v_item[i].qntd : v_item[i].STDA_C_ITEM_QNTD)
                //                + (v_item[i].STDA_C_ITEM_TIME > 0 ? ", TEMPO=" + (v_item[i].STDA_C_ITEM_TIME) : std::string("")) +"]";
                //    }
                //}

                // Add a Coin agora no Vector de itens
                v_item.Add(item);

                // DB Register Rare Win Log
                //if (!win_item.empty() && win_item.begin().tipo > 0 && win_item.Count() == 1)
                //	snmdb::NormalManagerDB.getInstance().add(24, new CmdInsertMemorialRareWinLog(_session.m_pi.uid, c._typeid, * win_item.begin()), channel::SQLDBResponse, this);

                // Log
                //_smp::message_pool.push(new message("[MemorialSystem::Play][Log] player[UID=" + (_session.m_pi.uid) + "] jogou Coin[TYPEID="
                //        + (c._typeid) + "] no Memorial Shop e ganhou o Item(ns){" + str + "}", type_msg.CL_FILE_LOG_AND_CONSOLE));

                // UPDATE ON GAME
                p.init_plain(0x216);

                p.WriteUInt32((uint)UtilTime.GetSystemTimeAsUnix());
                p.WriteUInt32((uint)v_item.Count());    // Count;

                foreach (var el in v_item)
                {
                    p.WriteByte(el.type);
                    p.WriteUInt32(el._typeid);
                    p.WriteInt32(el.id);
                    p.WriteUInt32(el.flag_time);
                    p.WriteBuffer(el.stat, Marshal.SizeOf(el.stat));
                    p.WriteUInt32((el.STDA_C_ITEM_TIME > 0) ? el.STDA_C_ITEM_TIME : el.STDA_C_ITEM_QNTD);
                    p.WriteZero(25);
                }

                packet_func.session_send(p, _session, 1);

                // Resposta ao Play Memorial
                p.init_plain(0x264);

                p.WriteUInt32(0);   // OK

                p.WriteUInt32((uint)win_item.Count());  // Count

                foreach (var el in win_item)
                {
                    p.WriteInt32(el.tipo);
                    p.WriteUInt32(el._typeid);
                    p.WriteUInt32(el.qntd);
                }

                packet_func.session_send(p, _session, 1);

                // Update Achievement ON SERVER, DB and GAME
                //sys_achieve.finish_and_update(_session);

            }
            catch (exception e)
            {

                _smp::message_pool.push(new message("[channel::requestPlayMemorial][ErrorSystem]" + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                p.init_plain(0x264);

                p.WriteUInt32((ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) == (uint)STDA_ERROR_TYPE.CHANNEL) ? ExceptionError.STDA_SYSTEM_ERROR_DECODE(e.getCodeError()) : 0x6300300);

                packet_func.session_send(p, _session, 1);
            }
        }

        // Card System
        public void requestOpenCardPack(Player _session, Packet _packet) { }
        public void requestLoloCardCompose(Player _session, Packet _packet) { }

        // Card Special/ Item Buff
        public void requestUseCardSpecial(Player _session, Packet _packet) { }
        public void requestUseItemBuff(Player _session, Packet _packet) { }

        // Comet Refill
        public void requestCometRefill(Player _session, Packet _packet) { }

        // MailBox
        public void requestOpenMailBox(Player _session, Packet _packet)
        {

            PangyaBinaryWriter p = new PangyaBinaryWriter();

            try
            {

                //if (_session.m_pi.block_flag.m_flag.mail_box)
                //    throw new exception("[channel::requestOpenMailBox][Error] player[UID=" + (_session.m_pi.m_uid)
                //            + "] tentou abrir Mail Box, mas ele nao pode. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.CHANNEL, 5, 0x790001));

                var pagina = _packet.ReadUInt32();

                if (pagina <= 0)
                    throw new exception("[channel::requestOpenMailBox][Error] Player[UID=" + (_session.m_pi.uid)
                            + "] tentou abrir Mail Box[Pagina=" + (pagina)
                            + "], mas a pagina eh invalida.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.CHANNEL, 6, 0x790002)); ;

                _smp::message_pool.push(new message("[channel::requestOpenMailBox][Log] Player[UID=" + (_session.m_pi.uid) + "]\tRequest Pagina: " + (pagina) + " MailBox", type_msg.CL_FILE_LOG_AND_CONSOLE));

                // Verifica se session está varrizada para executar esse ação, 
                // se ele não fez o login com o Server ele não pode fazer nada até que ele faça o login

                var mails = _session.m_pi.m_mail_box.GetPage(pagina);
 
                if (mails.Any())
                {

                    //Log
                    _smp::message_pool.push(new message("[channel::requestOpenMailBox][Log] Player[UID=" + (_session.m_pi.uid)
                                              + "] abriu o MailBox[Pagina=" + (pagina) + "] com sucesso.", type_msg.CL_FILE_LOG_AND_CONSOLE));
                    // pagina existe, envia ela
                    _session.Send(packet_func.pacote211(mails, pagina, _session.m_pi.m_mail_box.getTotalPages()/*cmd_mbi.getTotalPage()*/));

                }
                else
                { // MailBox Vazio                                                  
                    _session.Send(packet_func.pacote211(new List<MailBox>(), pagina, 1));
                }

            }
            catch (exception e)
            {

                _smp::message_pool.push(new message("[channel::requestOpenMailBox][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                p.init_plain(0x211);

                p.WriteUInt32((ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) == (uint)STDA_ERROR_TYPE.CHANNEL) ? ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) : 0x5500200);

                _session.Send(p);
            }
        }
        public void requestInfoMail(Player _session, Packet _packet)
        {
            try
            {

                var email_id = _packet.ReadUInt32();

                _smp::message_pool.push(new message("[channel::requestInfoMail][Log] Player[UID=" + (_session.m_pi.uid) + "]\tRequest Email Info: " + (email_id), type_msg.CL_FILE_LOG_AND_CONSOLE));

                // Verifica se session está varrizada para executar esse ação, 
                // se ele não fez o login com o Server ele não pode fazer nada até que ele faça o login
                //CHECK_SESSION_IS_AUTHORIZED("InfoMail");

                var email = _session.m_pi.m_mail_box.getEmailInfo(email_id);
  
                if (email.id == 0)
                    throw new exception("[channel::requestInfoMail][Error] Player[UID=" + (_session.m_pi.uid) + "] pediu para ver o info do Mail[ID=" + (email_id)
                            + "], mais ele nao existe no banco de dados. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.CHANNEL, 0x5500251, 1));

                try
                {
                    item_manager.checkSetItemOnEmail(_session, email);
                }
                catch (exception e)
                {
                    // Se não for item vector vazio, relança a exception
                    if (!ExceptionError.STDA_ERROR_CHECK_SOURCE_AND_ERROR_TYPE(e.getCodeError(), STDA_ERROR_TYPE._ITEM_MANAGER, 20))
                        throw;
                }

                // Log

                _smp::message_pool.push(new message("[channel::requestInfoMail][Log] Player[UID=" + (_session.m_pi.uid) + "] pediu info do Mail[ID="
                        + (email.id) + "] com sucesso.", type_msg.CL_FILE_LOG_AND_CONSOLE));
                _session.Send(packet_func.pacote212(email), 1);

            }
            catch (exception e)
            {

                _smp::message_pool.push(new message("[channel::requestInfoMail][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                PangyaBinaryWriter p = new PangyaBinaryWriter();

                p.init_plain(0x212);

                p.WriteUInt32((ExceptionError.STDA_SOURCE_ERROR_DECODE_TYPE(e.getCodeError()) == (uint)STDA_ERROR_TYPE.CHANNEL) ? ExceptionError.STDA_SYSTEM_ERROR_DECODE(e.getCodeError()) : 0x5500250);

                _session.Send(p);
            }
        }
        public void requestSendMail(Player _session, Packet _packet) { }
        public void requestTakeItemFomMail(Player _session, Packet _packet) { }
        public void requestDeleteMail(Player _session, Packet _packet) { }

        // Dolfini Locker
        public void requestMakePassDolfiniLocker(Player _session, Packet _packet) { }
        public void requestCheckDolfiniLockerPass(Player _session, Packet _packet) { }
        public void requestChangeDolfiniLockerPass(Player _session, Packet _packet) { }
        public void requestChangeDolfiniLockerModeEnter(Player _session, Packet _packet) { }
        public void requestDolfiniLockerItem(Player _session, Packet _packet) { }
        public void requestDolfiniLockerPang(Player _session, Packet _packet) { }
        public void requestUpdateDolfiniLockerPang(Player _session, Packet _packet) { }
        public void requestAddDolfiniLockerItem(Player _session, Packet _packet) { }
        public void requestRemoveDolfiniLockerItem(Player _session, Packet _packet) { }

        // Legacy Tiki Shop (Point Shop)
        public void requestOpenLegacyTikiShop(Player _session, Packet _packet) { }
        public void requestPointLegacyTikiShop(Player _session, Packet _packet) { }
        public void requestExchangeTPByItemLegacyTikiShop(Player _session, Packet _packet) { }
        public void requestExchangeItemByTPLegacyTikiShop(Player _session, Packet _packet) { }

        // Personal Shop
        public void requestOpenEditSaleShop(Player _session, Packet _packet) { }
        public void requestCloseSaleShop(Player _session, Packet _packet) { }
        public void requestChangeNameSaleShop(Player _session, Packet _packet) { }
        public void requestOpenSaleShop(Player _session, Packet _packet) { }
        public void requestVisitCountSaleShop(Player _session, Packet _packet) { }
        public void requestPangSaleShop(Player _session, Packet _packet) { }
        public void requestCancelEditSaleShop(Player _session, Packet _packet) { }
        public void requestViewSaleShop(Player _session, Packet _packet) { }
        public void requestCloseViewSaleShop(Player _session, Packet _packet) { }
        public void requestBuyItemSaleShop(Player _session, Packet _packet) { }

        // Papel Shop
        public void requestOpenPapelShop(Player _session, Packet _packet)
        {
            try
            {
                using (var p = new PangyaBinaryWriter(0x10B))
                {
                    p.WriteUInt32(0);
                    p.WriteUInt64((ulong)_session.m_pi.mi.papel_shop.limit_count);
                    _session.Send(p);
                }
            }
            catch (exception e)
            {
                _smp::message_pool.push(new message("[channel::requestOpenPapelShop][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
                using (var p = new PangyaBinaryWriter(0x10B))
                {
                    p.WriteInt64(-1);

                    p.WriteUInt32((ExceptionError.STDA_SOURCE_ERROR_DECODE_TYPE(e.getCodeError()) == (uint)STDA_ERROR_TYPE.CHANNEL) ? ExceptionError.STDA_SYSTEM_ERROR_DECODE(e.getCodeError()) : 0x5800100);
                    _session.Send(p);
                }
            }
        }
        public void requestPlayPapelShop(Player _session, Packet _packet)
        {
            PangyaBinaryWriter p = new PangyaBinaryWriter();

            try
            {


                if (_session.m_pi.block_flag.m_flag.papel_shop)
                    throw new exception("[channel::requestPlayPapelShop][Error] player[UID=" + (_session.m_pi.uid)
                            + "] tentou jogar no Papel Shop, mas ele nao pode. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.CHANNEL, 3, 0x790001));

                if (_session.m_pi.level < 1)
                    throw new exception("[channel::requestPlayPapelShop][Error] player[UID=" + (_session.m_pi.uid) + "] tentou jogar o Papel Shop Normal, mas nao tem o level necessario[level="
                            + (_session.m_pi.level) + ", request=1]", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.CHANNEL, 8, 0x5900108));

                if (!sPapelShopSystem.getInstance().isLoad())
                    sPapelShopSystem.getInstance().load();

                if (sPapelShopSystem.getInstance().isLimittedPerDay() && _session.m_pi.mi.papel_shop.remain_count <= 0)
                    throw new exception("[channel::requestPlayPapelShop][Warning] player[UID=" + (_session.m_pi.uid)
                        + "] tentou jogar o Papel Shop Normal, mas o limite por dia esta ativado, e ele nao tem mais vezes no dia ele ja chegou ao seu limite.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.CHANNEL, 1, 0x5900101));

                var coupon = sPapelShopSystem.getInstance().hasCoupon(_session);

                if ((coupon == null || coupon.STDA_C_ITEM_QNTD < 1) && _session.m_pi.ui.pang < sPapelShopSystem.getInstance().getPriceNormal())
                    throw new exception("[channel::requestPlayPapelShop][Error] player[UID=" + (_session.m_pi.uid) + "] tentou jogar o Papel Shop Normal, ele nao tem Coupon e nem Pangs suficiente[value="
                            + (_session.m_pi.ui.pang) + ", request=" + (sPapelShopSystem.getInstance().getPriceNormal()) + "]. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.CHANNEL, 2, 0x5900102));

                var balls = sPapelShopSystem.getInstance().dropBalls(_session);

                if (!balls.Any())
                    throw new exception("[channel::requestPlayPapelShop][Error] player[UID=" + (_session.m_pi.uid) + "] tentou jogar o Papel Shop Normal, mas nao conseguiu sortear as bolas. Bug",
                            ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.CHANNEL, 3, 0x5900103));

                List<stItem> v_item = new List<stItem>();
                stItem item = new stItem();
                BuyItem bi = new BuyItem();

                //  SysAchievement sys_achieve;

                // Reserva memória para o vector, não realocar depois a cada push_back ou insert
                //   v_item.Re(balls.Count() + 1/*coupon*/);

                foreach (var el in balls)
                {

                    item.clear();

                    bi.id = -1;
                    bi._typeid = el.ctx_psi._typeid;
                    bi.qntd = el.qntd;

                    item_manager.initItemFromBuyItem(_session.m_pi, ref item, bi, false, 0, 0, 1);

                    if (item._typeid == 0)
                        throw new exception("[channel::requestPlayPapelShop][Error] player[UID=" + (_session.m_pi.uid) + "] tentou jogar o Papel Shop Normal, mas nao conseguiu inicializar o Item[TYPEID="
                                + (bi._typeid) + "]. Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.CHANNEL, 4, 0x5900104));

                    var it = v_item.FirstOrDefault(el2 => el2._typeid == item._typeid);


                    if (it != null)
                    {   // Já tem o item soma as quantidades
                        it.qntd += item.qntd;
                        it.STDA_C_ITEM_QNTD = (ushort)it.qntd;
                    }
                    else    // Não tem, coloca ele na lista
                    {
                        v_item.Add(item);
                        it = v_item.Last();
                    }
                }

                // UPDATE ON SERVER

                string ids = "";

                for (var i = 0; i < v_item.Count(); ++i)
                    ids += ((i == 0) ? ("") : (", ")) + "TYPEID=" + (v_item[i]._typeid) + ", ID=" + (v_item[i].id) + ", QNTD=" + (v_item[i].STDA_C_ITEM_QNTD);

                // Add ao Server e DB
                //var rai = item_manager.addItem(v_item, _session, 0, 0);

                //    if (rai.fails.Count() > 0 && rai.type != item_manager.RetAddItem.TYPE.T_SUCCESS_PANG_AND_EXP_AND_CP_POUCH)
                //    throw new exception("[channel::requestPlayPapelShop][Error] player[UID=" + (_session.m_pi.uid) + "] tentou jogar o Papel Shop Normal, mas nao conseguiu adicionar o(s) Item(ns){"
                //            + ids + "}", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.CHANNEL, 6, 0x5900106));

                // Delete Coupon e coloca no vector de att item, se tiver coupon
                if (coupon != null)
                {
                    item.clear();

                    item.type = 2;
                    item.id = (int)coupon.id;
                    item._typeid = coupon._typeid;
                    item.qntd = 1;
                    item.STDA_C_ITEM_QNTD = (ushort)((ushort)item.qntd * -1);

                    //if (item_manager.removeItem(item, _session) <= 0)
                    //    throw new exception("[channel::requestPlayPapelShop][Error] player[UID=" + (_session.m_pi.uid) + "] tentou jogar o Papel Shop Normal, mas nao conseguiu deletar o Coupon[TYPEID="
                    //        + (coupon._typeid) + ", ID=" + (coupon.id) + "]. Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.CHANNEL, 5, 0x5900105));

                    // Add ao vector
                    v_item.Add(item);

                }
                else    // Não tem Coupon Tira Pangs do player
                    _session.m_pi.consomePang(sPapelShopSystem.getInstance().getPriceNormal());

                // Update Papel Shop Count Player. Se o limite por dia estiver habilitado, decrementa 1 do player
                sPapelShopSystem.getInstance().updatePlayerCount(_session);

                // Verificar se ganhou item Raro, se sim, cria um log no banco de dados
                foreach (var el in balls)
                {
                    if (el.ctx_psi.tipo == PAPEL_SHOP_TYPE.PST_RARE)
                    {
                        Console.WriteLine("[PapelShopSystem::PlayNormal][Log] player[UID=" + (_session.m_pi.uid) + "] ganhou Item Raro[TYPEID="
                                + (el.ctx_psi._typeid) + ", QNTD=" + (el.qntd) + ", BALL_COLOR=" + (el.color) + ", PROBABILIDADE=" + (el.ctx_psi.probabilidade) + "]");

                        // Adiciona +1 ao contador de item Rare Win no Papel Shop
                        // sys_achieve.incrementCounter(0x6C400081u/*Rare Win*/);

                        // NormalManagerD.add(19, new CmdInsertPapelShopRareWinLog(_session.m_pi.uid, el), SQLDBResponse, this);
                    }
                }

                // UPDATE Achievement ON SERVER, DB and GAME

                // Add +1 ao contador de jogo ao play Palpel Shop
                //  sys_achieve.incrementCounter(0x6C40004Au/*Play Papel Shop*/);

                // Log
                Console.WriteLine("[PapelShopSystem::PlayNormal][Log] player[UID=" + (_session.m_pi.uid) + "] jogou Papel Shop Normal e ganhou Item(ns){" + ids + "}");

                // UPDATE ON GAME
                p.init_plain(0x216);

                p.WriteUInt32((uint)UtilTime.GetSystemTimeAsUnix());
                p.WriteUInt32((uint)v_item.Count());

                foreach (var el in v_item)
                {
                    p.WriteByte(el.type);
                    p.WriteUInt32(el._typeid);
                    p.WriteInt32((int)(new Random().Next())/*el.id*/);
                    p.WriteUInt32(el.flag_time);
                    p.WriteStruct(el.stat, new stItem.item_stat());
                    p.WriteUInt32((el.STDA_C_ITEM_TIME > 0) ? el.STDA_C_ITEM_TIME : el.STDA_C_ITEM_QNTD);
                    p.WriteZero(25);  // C[0~4] 10 Bytes e mais outras coisas, que tem na struct stItem216 explicando
                }
                _session.Send(p);

                p.init_plain(0xFB);

                if (sPapelShopSystem.getInstance().isLimittedPerDay())
                {
                    p.WriteInt32(_session.m_pi.mi.papel_shop.remain_count);
                    p.WriteInt32(-2);                                             // Flag
                }
                else
                {
                    p.WriteInt32(-1);
                    p.WriteInt32(-3);                                             // Flag
                }

                _session.Send(p);

                // Resposta para o Play Papel Shop Normal
                p.init_plain(0x21B);

                p.WriteUInt32(0);     // OK

                p.WriteUInt32((coupon != null) ? coupon.id : 0);

                p.WriteUInt32((uint)balls.Count());

                foreach (var el in balls)
                {
                    p.WriteUInt32((uint)el.color);
                    p.WriteUInt32(el.ctx_psi._typeid);
                    p.WriteUInt32((uint)((el.item is stItem) ? ((stItem)el.item).id : 0));    // Precisa do ID, se não ele add 2 itens, o do pacote 216 e o desse
                    p.WriteUInt32(el.qntd);
                    p.WriteUInt32((uint)el.ctx_psi.tipo);
                }

                p.WriteUInt64(_session.m_pi.ui.pang);
                p.WriteUInt64(_session.m_pi.cookie);

                _session.Send(p);

                // UPDATE Achievement ON SERVER, DB and GAME
                //  sys_achieve.finish_and_update(_session);

            }
            catch (exception e)
            {

                _smp::message_pool.push(new message("[channel::requestPlayPapelShop][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                p.init_plain(0x21B);

                p.WriteUInt32((ExceptionError.STDA_SOURCE_ERROR_DECODE_TYPE(e.getCodeError()) == (uint)STDA_ERROR_TYPE.CHANNEL) ? ExceptionError.STDA_SYSTEM_ERROR_DECODE(e.getCodeError()) : 0x5900100);

                _session.Send(p);
            }
        }

        // Msg Chat da Sala
        public void requestSendMsgChatRoom(Player _session, string _msg) { }

        // senders
        public void sendUpdateRoomInfo(RoomInfoEx _ri, int _option) { }
        void sendUpdatePlayerInfo(Player _session, int _option)
        {
            PlayerCanalInfo pci = getPlayerInfo(_session);
            _session.SendChannel_broadcast(packet_func.pacote046(new vector<PlayerCanalInfo> { (pci == null) ? new PlayerCanalInfo() : pci }, _option));
        }

        // Destroy Room
        void destroyRoom(short _number) { }

    }
}
