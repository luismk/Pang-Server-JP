using System;
using _smp = PangyaAPI.Utilities.Log;
using PangyaAPI.Utilities;
using PangyaAPI.Network.PangyaPacket;
using GameServer.Session;
using System.Collections.Generic;
using GameServer.PangType;
using GameServer.Game.System;
using GameServer.Game;
using GameServer.PacketFunc;
using PangyaAPI.Utilities.BinaryModels;
using static GameServer.PangType._Define;
using PangyaAPI.Utilities.Log;
using System.Net.Sockets;
using System.Runtime.Remoting.Channels;
using System.Linq;
using GameServer.Cmd;
using PangyaAPI.Network.Pangya_St;
using PangyaAPI.SQL.Manager;
using System.Security.Cryptography;

namespace GameServer.GameServerTcp
{
    public partial class GameServer : GameServerBase
    {
        public override void blockOID(uint _oid)
        {
            base.blockOID(_oid);
        }

        public override bool checkCommand(string[] _command)
        {
            return base.checkCommand(_command);
        }

        public override void check_player()
        {
            base.check_player();
        }

        public override void clear()
        {
            base.clear();
        }

        public override void destroyRoom(byte _channel_owner, short _number)
        {
            base.destroyRoom(_channel_owner, _number);
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

                    _session.Send(packet_func.pacote04E(1));

                    return enter;   // Ele já está nesse canal
                }

                if (enter.isFull())
                {

                    // Não conseguiu entrar no canal por que ele está cheio, deixa o enter como null
                    enter = null;
                    _session.Send(packet_func.pacote04E(2));

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
            catch
            {
            }

            return enter;
        }

        public override List<Player> findAllGM()
        {
            return base.findAllGM();
        }

        public override Channel findChannel(byte _channel)
        {
            return base.findChannel(_channel);
        }

        public override Player findPlayer(uint _uid, bool _oid = false)
        {
            return base.findPlayer(_uid, _oid);
        }

        public override void init_load_channels()
        {
            base.init_load_channels();
        }

        public override void init_Packets()
        {
            base.init_Packets();
        }

        public override void init_systems()
        {
            base.init_systems();
        }

        public override void makeBotGMEventRoom()
        {
            base.makeBotGMEventRoom();
        }

        public override void makeGrandZodiacEventRoom()
        {
            base.makeGrandZodiacEventRoom();
        }

        public override void makeListOfPlayersToGoldenTime()
        {
            base.makeListOfPlayersToGoldenTime();
        }

        public override void reloadGlobalSystem(uint _tipo)
        {
            base.reloadGlobalSystem(_tipo);
        }

        public override void reload_files()
        {
            base.reload_files();
        }

        public override void reload_systems()
        {
            base.reload_systems();
        }

        public void requestChangeChatMacroUser(Player _session, Packet _packet)
        {
        }

        public void requestChangeServer(Player _session, Packet _packet)
        {
        }

        public void requestChangeWhisperState(Player _session, Packet _packet)
        {
        }

        public void requestChat(Player _session, Packet _packet)
        {
            try
            {

                string nickname = "", msg = "";

                nickname = _packet.ReadPStr();
                msg = _packet.ReadPStr();

                // Verifica a mensagem com palavras proibida e manda para o log e bloquea o chat dele
                _smp::message_pool.push(new message("[requestChat]: Player[UID: " + _session.m_pi.uid + ", \tMessage: " + msg + "]", type_msg.CL_FILE_LOG_AND_CONSOLE));

                var c = findChannel(_session.m_pi.channel);

                if (c != null)
                {

                    // LOG GM
                    // Envia para todo os GM do server essa message
                    var gm = m_player_manager.findAllGM();

                    if (gm.Count > 0)
                    {
                    }
                }

                // Normal Message
                if (_session.m_pi.mi.sala_numero != ushort.MaxValue)
                    c.requestSendMsgChatRoom(_session, msg);
                else
                {                                                 
                    //is low :/
                    _session.SendLobby_broadcast(packet_func.pacote040(_session.m_pi, msg, (byte)(_session.m_pi.m_cap.stBit.game_master ? 0x80 : 0)));
                                                   
                }

            }
            catch
            { }
        }

        public void requestCheckGameGuardAuthAnswer(Player _session, Packet _packet)
        {
        }

        public void requestCommandNoticeGM(Player _session, Packet _packet)
        {
        }

        public void requestCommonCmdGM(Player _session, Packet _packet)
        {
        }

        public void requestEnterChannel(Player _session, Packet _packet)
        {
            try
            {
                _packet.ReadByte(out byte channel);
                // Enter Channel
                enterChannel(_session, channel);
            }
            catch
            { }
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

                _smp::message_pool.push(new message("[game_server::requestEnterOtherChannelAndLobby][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

            }
        }

        public void requestExceptionClientMessage(Player _session, Packet _packet)
        {
        }

        public void requestLogin(Player _session, Packet _packet)
        {
            new LoginSystem().requestLogin(_session, _packet);
        }

        public void requestNotifyNotDisplayPrivateMessageNow(Player _session, Packet _packet)
        {
        }

        public void requestPlayerInfo(Player _session, Packet _packet)
        {
            try
            {
                uint uid = _packet.ReadUInt32();
                byte season = _packet.ReadByte();
                _smp.message_pool.push(new message($"Player UID: {_session.m_pi.uid}.\tPlayer Info request uid: {uid}\tseason: {(int)season}", type_msg.CL_ONLY_CONSOLE));

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
                    if (uid != _session.m_pi.uid && !mi.capability.stBit.game_master/* & 4/*(GM)*/)
                    {

                        _session.Send(packet_func.pacote089(uid, season, 3)); 

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

                        var cmd_gi= new  CmdGuildInfo(uid, 0);

                        NormalManagerDB.add(0, cmd_gi, null, null);

                        if (cmd_gi.getException().getCodeError() != 0)
                            throw cmd_gi.getException();

                        var gi = cmd_gi.getInfo();

                      var cmd_ms = new  CmdMapStatistics(uid, (CmdMapStatistics.TYPE_SEASON)(season), CmdMapStatistics.TYPE.NORMAL, CmdMapStatistics.TYPE_MODO.M_NORMAL);

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

                        _session.Send(packet_func.pacote157(mi, season));

                        _session.Send(packet_func.pacote15E(uid, ci));

                        _session.Send(packet_func.pacote156(uid, ue, season));

                        _session.Send(packet_func.pacote158(uid, ui, season));

                        _session.Send(packet_func.pacote15D(uid, gi));

                        _session.Send(packet_func.pacote15C(uid, v_ms_na, v_msa_na, Convert.ToByte((season != 0) ? 0x33 : 0x0A)));

                        _session.Send(packet_func.pacote15C(uid, v_ms_g, v_msa_g, Convert.ToByte((season != 0) ? 0x34 : 0x0B)));

                        _session.Send(packet_func.pacote15B(uid, season));

                        _session.Send(packet_func.pacote15A(uid, v_tei, season));

                        _session.Send(packet_func.pacote159(uid, ti, season));

                        _session.Send(packet_func.pacote15C(uid, v_ms_n.ToList(), v_msa_n.ToList(), season));

                        _session.Send(packet_func.pacote257(uid, v_tegi, season));

                        _session.Send(packet_func.pacote089(uid, season));
                    }

                    return;
                }

                // Verifica se não é o mesmo UID, pessoas diferentes
                // Quem quer ver a info não é GM aí verifica se o player é GM
                if (uid != _session.m_pi.uid && !pi.m_cap.stBit.game_master/* & 4/*(GM)*/)
                {                                                      
                    _session.Send(packet_func.pacote089(uid, season, 3));

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

                    _session.Send(packet_func.pacote157(pi.mi, season));

                    _session.Send(packet_func.pacote15E(pi.uid, ci));

                    _session.Send(packet_func.pacote156(pi.uid, pi.ue, season));

                    _session.Send(packet_func.pacote158(pi.uid, pi.ui, season));

                    _session.Send(packet_func.pacote15D(pi.uid, pi.gi));

                    _session.Send(packet_func.pacote15C(pi.uid, v_ms_na, v_msa_na, (byte)((season != 0) ? 0x33 : 0x0A)));

                    _session.Send(packet_func.pacote15C(pi.uid, v_ms_g, v_msa_g, (byte)((season != 0) ? 0x34 : 0x0B)));

                    _session.Send(packet_func.pacote15B(uid, season));

                    _session.Send(packet_func.pacote15A(pi.uid, (season != 0) ? pi.v_tsi_current_season : pi.v_tsi_rest_season, season));

                    _session.Send(packet_func.pacote159(pi.uid, (season != 0) ? pi.ti_current_season : pi.ti_rest_season, season));

                    _session.Send(packet_func.pacote15C(pi.uid, v_ms_n, v_msa_n, season));

                    _session.Send(packet_func.pacote257(pi.uid, (season != 0) ? pi.v_tgp_current_season : pi.v_tgp_rest_season, season));


                    _session.Send(packet_func.pacote089(uid, season));

                }
            }
            catch (Exception e)
            {
                message_pool.push(new message($"[GameServer::RequestPlayerInfo][ErrorSystem] {e.Message}", type_msg.CL_ONLY_CONSOLE));
                _session.Send(packet_func.pacote089(0));                          
            }                                                                                                                                                       
        }

        public void requestPrivateMessage(Player _session, Packet _packet)
        {
        }

        public void requestQueueTicker(Player _session, Packet _packet)
        {
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
        }

        public void requestUCCWebKey(Player _session, Packet _packet)
        {
        }

        public void sendChannelListToSession(Player _session)
        {

            try
            {
                var p = packet_func.pacote04D(v_channel);
                _session.Send(p);
            }
            catch (exception e)
            {
                _smp.message_pool.push("[GameServer::sendChannelListToSession][ErrorSystem] " + e.getFullMessageError());
            }
        }

        public override void sendDateTimeToSession(Player _session)
        {
            base.sendDateTimeToSession(_session);
        }


        public override void sendServerListAndChannelListToSession(Player _session)
        {
            base.sendServerListAndChannelListToSession(_session);
        }

        public override void setAngelEvent(uint _angel_event)
        {
            base.setAngelEvent(_angel_event);
        }

        public override void unblockOID(uint _oid)
        {
            base.unblockOID(_oid);
        }

        public override void updateDailyQuest(DailyQuestInfo _dqi)
        {
            base.updateDailyQuest(_dqi);
        }

        public override void updaterateAndEvent(uint _tipo, uint _qntd)
        {
            base.updaterateAndEvent(_tipo, _qntd);
        }

        public List<Channel> getChannelList()
        {
            return v_channel;
        }
    }
}
