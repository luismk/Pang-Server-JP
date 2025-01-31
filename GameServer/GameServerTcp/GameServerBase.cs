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

                    // Não conseguiu entrar no canal por que ele está cheio, deixa o enter como nullptr
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

        public override void requestChangeChatMacroUser(Player _session, Packet _packet)
        {
            base.requestChangeChatMacroUser(_session, _packet);
        }

        public override void requestChangeServer(Player _session, Packet _packet)
        {
            base.requestChangeServer(_session, _packet);
        }

        public override void requestChangeWhisperState(Player _session, Packet _packet)
        {
            base.requestChangeWhisperState(_session, _packet);
        }

        public override void requestChat(Player _session, Packet _packet)
        {
            base.requestChat(_session, _packet);
        }

        public override void requestCheckGameGuardAuthAnswer(Player _session, Packet _packet)
        {
            base.requestCheckGameGuardAuthAnswer(_session, _packet);
        }

        public override void requestCommandNoticeGM(Player _session, Packet _packet)
        {
            base.requestCommandNoticeGM(_session, _packet);
        }

        public override void requestCommonCmdGM(Player _session, Packet _packet)
        {
            base.requestCommonCmdGM(_session, _packet);
        }

        public override void requestEnterChannel(Player _session, Packet _packet)
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

        public override void requestEnterOtherChannelAndLobby(Player _session, Packet _packet)
        {
            base.requestEnterOtherChannelAndLobby(_session, _packet);
        }

        public override void requestExceptionClientMessage(Player _session, Packet _packet)
        {
            base.requestExceptionClientMessage(_session, _packet);
        }

        public override void requestLogin(Player _session, Packet _packet)
        {
           new LoginSystem().requestLogin(_session, _packet);
        }

        public override void requestNotifyNotDisplayPrivateMessageNow(Player _session, Packet _packet)
        {
            base.requestNotifyNotDisplayPrivateMessageNow(_session, _packet);
        }

        public override void requestPlayerInfo(Player _session, Packet _packet)
        {
            base.requestPlayerInfo(_session, _packet);
        }

        public override void requestPrivateMessage(Player _session, Packet _packet)
        {
            base.requestPrivateMessage(_session, _packet);
        }

        public override void requestQueueTicker(Player _session, Packet _packet)
        {
            base.requestQueueTicker(_session, _packet);
        }

        public override void requestSendNotice(string notice)
        {
            base.requestSendNotice(notice);
        }

        public override void requestSendTicker(Player _session, Packet _packet)
        {
            base.requestSendTicker(_session, _packet);
        }

        public override void requestTranslateSubPacket(Player _session, Packet _packet)
        {
            base.requestTranslateSubPacket(_session, _packet);
        }

        public override void requestUCCSystem(Player _session, Packet _packet)
        {
            base.requestUCCSystem(_session, _packet);
        }

        public override void requestUCCWebKey(Player _session, Packet _packet)
        {
            base.requestUCCWebKey(_session, _packet);
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
    }
}
