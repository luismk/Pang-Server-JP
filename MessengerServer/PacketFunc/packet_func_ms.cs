﻿using PangyaAPI.Network.PangyaPacket;
using PangyaAPI.Utilities.Log;
using PangyaAPI.Utilities;
using System;
using System.Collections.Generic;
using MessengerServer.Session;
using PangyaAPI.Network.PangyaSession;
using PangyaAPI.Utilities.BinaryModels;

namespace MessengerServer.PacketFunc
{
    public class packet_func : packet_func_base
    {
        // Cliente
        public static int packet012(object param, ParamDispatch pd)
        {
            try
            {                                                                                                                                                                         
                ((MessengerServerTcp.MessengerServer)param).requestLogin(((Player)pd._session), pd._packet);
                return 0;
            }
            catch (exception e)
            {

                message_pool.push(new message("[packet_func::packet012][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if (ExceptionError.STDA_SOURCE_ERROR_DECODE_TYPE(e.getCodeError()) != (uint)STDA_ERROR_TYPE.MESSAGE_SERVER)
                {
                    throw;
                }
            }

            return 0;
        }

        public static int packet013(object param,ParamDispatch pd)
        {                                                                
            try
            {

                 message_pool.push(new message("[packet_func::packet013][Log] Player[UID=" + Convert.ToString(((Player)pd._session).m_pi.uid) + "] mandou o Pacote013, nao sei o que esse pacote pede ou faz. Hex: \n\r" + pd._packet.Log(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                // Verifica se session est� autorizada para executar esse a��o,
                // se ele n�o fez o login com o Server ele n�o pode fazer nada at� que ele fa�a o login
                if (!((Player)pd._session).m_is_authorized)
                {
                    throw new exception("[packet_func::" + "packet013" + "][Error] Player[UID=" + Convert.ToString(((Player)pd._session).m_pi.uid) + "] Nao esta autorizado a fazer esse request por que ele ainda nao fez o login com o Server. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PACKET_FUNC_MS,
                        1, 0x5000501));
                }

            }
            catch (exception e)
            {

                 message_pool.push(new message("[packet_func::packet013][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if (ExceptionError.STDA_SOURCE_ERROR_DECODE_TYPE(e.getCodeError()) != (uint)STDA_ERROR_TYPE.MESSAGE_SERVER)
                {
                    throw;
                }
            }

            return 0;
        }

        public static int packet014(object param,ParamDispatch pd)
        {                                                               
            try
            {

                ((MessengerServerTcp.MessengerServer)param).requestFriendAndGuildMemberList(((Player)pd._session), pd._packet);

            }
            catch (exception e)
            {

                 message_pool.push(new message("[packet_func::packet014][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if (ExceptionError.STDA_SOURCE_ERROR_DECODE_TYPE(e.getCodeError()) != (uint)STDA_ERROR_TYPE.MESSAGE_SERVER)
                {
                    throw;
                }
            }

            return 0;
        }

        public static int packet016(object param,ParamDispatch pd)
        {                                                              
            try
            {

                ((MessengerServerTcp.MessengerServer)param).requestUpdatePlayerLogout(((Player)pd._session), pd._packet);

            }
            catch (exception e)
            {

                 message_pool.push(new message("[packet_func::packet016][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if (ExceptionError.STDA_SOURCE_ERROR_DECODE_TYPE(e.getCodeError()) != (uint)STDA_ERROR_TYPE.MESSAGE_SERVER)
                {
                    throw;
                }
            }

            return 0;
        }

        public static int packet017(object param,ParamDispatch pd)
        {                                                             
            try
            {

                ((MessengerServerTcp.MessengerServer)param).requestCheckNickname(((Player)pd._session), pd._packet);

            }
            catch (exception e)
            {

                 message_pool.push(new message("[packet_func::packet017][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if (ExceptionError.STDA_SOURCE_ERROR_DECODE_TYPE(e.getCodeError()) != (uint)STDA_ERROR_TYPE.MESSAGE_SERVER)
                {
                    throw;
                }
            }

            return 0;
        }

        public static int packet018(object param,ParamDispatch pd)
        {                                                            
            try
            {

                ((MessengerServerTcp.MessengerServer)param).requestAddFriend(((Player)pd._session), pd._packet);

            }
            catch (exception e)
            {

                 message_pool.push(new message("[packet_func::packet018][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if (ExceptionError.STDA_SOURCE_ERROR_DECODE_TYPE(e.getCodeError()) != (uint)STDA_ERROR_TYPE.MESSAGE_SERVER)
                {
                    throw;
                }
            }

            return 0;
        }

        public static int packet019(object param,ParamDispatch pd)
        {                                                           
            try
            {

                ((MessengerServerTcp.MessengerServer)param).requestConfirmFriend(((Player)pd._session), pd._packet);

            }
            catch (exception e)
            {

                 message_pool.push(new message("[packet_func::packet019][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if (ExceptionError.STDA_SOURCE_ERROR_DECODE_TYPE(e.getCodeError()) != (uint)STDA_ERROR_TYPE.MESSAGE_SERVER)
                {
                    throw;
                }
            }

            return 0;
        }

        public static int packet01A(object param,ParamDispatch pd)
        {    
            try
            {

                ((MessengerServerTcp.MessengerServer)param).requestBlockFriend(((Player)pd._session), pd._packet);

            }
            catch (exception e)
            {

                 message_pool.push(new message("[packet_func::packet01A][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if (ExceptionError.STDA_SOURCE_ERROR_DECODE_TYPE(e.getCodeError()) != (uint)STDA_ERROR_TYPE.MESSAGE_SERVER)
                {
                    throw;
                }
            }

            return 0;
        }

        public static int packet01B(object param,ParamDispatch pd)
        {        
            try
            {

                ((MessengerServerTcp.MessengerServer)param).requestUnblockFriend(((Player)pd._session), pd._packet);

            }
            catch (exception e)
            {

                 message_pool.push(new message("[packet_func::packet01B][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if (ExceptionError.STDA_SOURCE_ERROR_DECODE_TYPE(e.getCodeError()) != (uint)STDA_ERROR_TYPE.MESSAGE_SERVER)
                {
                    throw;
                }
            }

            return 0;
        }

        public static int packet01C(object param,ParamDispatch pd)
        {     
            try
            {

                ((MessengerServerTcp.MessengerServer)param).requestDeleteFriend(((Player)pd._session), pd._packet);

            }
            catch (exception e)
            {

                 message_pool.push(new message("[packet_func::packet01C][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if (ExceptionError.STDA_SOURCE_ERROR_DECODE_TYPE(e.getCodeError()) != (uint)STDA_ERROR_TYPE.MESSAGE_SERVER)
                {
                    throw;
                }
            }

            return 0;
        }

        public static int packet01D(object param,ParamDispatch pd)
        {     
            try
            {

                ((MessengerServerTcp.MessengerServer)param).requestUpdatePlayerState(((Player)pd._session), pd._packet);

            }
            catch (exception e)
            {

                 message_pool.push(new message("[packet_func::packet01D][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if (ExceptionError.STDA_SOURCE_ERROR_DECODE_TYPE(e.getCodeError()) != (uint)STDA_ERROR_TYPE.MESSAGE_SERVER)
                {
                    throw;
                }
            }

            return 0;
        }

        public static int packet01E(object param,ParamDispatch pd)
        {    
            try
            {

                ((MessengerServerTcp.MessengerServer)param).requestChatFriend(((Player)pd._session), pd._packet);

            }
            catch (exception e)
            {

                 message_pool.push(new message("[packet_func::packet01E][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if (ExceptionError.STDA_SOURCE_ERROR_DECODE_TYPE(e.getCodeError()) != (uint)STDA_ERROR_TYPE.MESSAGE_SERVER)
                {
                    throw;
                }
            }

            return 0;
        }

        public static int packet01F(object param,ParamDispatch pd)
        {         
            try
            {

                ((MessengerServerTcp.MessengerServer)param).requestAssignApelido(((Player)pd._session), pd._packet);

            }
            catch (exception e)
            {

                 message_pool.push(new message("[packet_func::packet01F][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if (ExceptionError.STDA_SOURCE_ERROR_DECODE_TYPE(e.getCodeError()) != (uint)STDA_ERROR_TYPE.MESSAGE_SERVER)
                {
                    throw;
                }
            }

            return 0;
        }

        public static int packet023(object param,ParamDispatch pd)
        {                         
            try
            {

                ((MessengerServerTcp.MessengerServer)param).requestUpdateChannelPlayerInfo(((Player)pd._session), pd._packet);

            }
            catch (exception e)
            {

                 message_pool.push(new message("[packet_func::packet023][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if (ExceptionError.STDA_SOURCE_ERROR_DECODE_TYPE(e.getCodeError()) != (uint)STDA_ERROR_TYPE.MESSAGE_SERVER)
                {
                    throw;
                }
            }

            return 0;
        }

        public static int packet024(object param,ParamDispatch pd)
        {      
            try
            {

                ((MessengerServerTcp.MessengerServer)param).requestNotifyPlayerWasInvitedToRoom(((Player)pd._session), pd._packet);

            }
            catch (exception e)
            {

                 message_pool.push(new message("[packet_func::packet024][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if (ExceptionError.STDA_SOURCE_ERROR_DECODE_TYPE(e.getCodeError()) != (uint)STDA_ERROR_TYPE.MESSAGE_SERVER)
                {
                    throw;
                }
            }

            return 0;
        }

        public static int packet025(object param,ParamDispatch pd)
        {
           
            

            try
            {

                ((MessengerServerTcp.MessengerServer)param).requestChatGuild(((Player)pd._session), pd._packet);

            }
            catch (exception e)
            {

                 message_pool.push(new message("[packet_func::packet025][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if (ExceptionError.STDA_SOURCE_ERROR_DECODE_TYPE(e.getCodeError()) != (uint)STDA_ERROR_TYPE.MESSAGE_SERVER)
                {
                    throw;
                }
            }

            return 0;
        }

        public static int packet028(object param,ParamDispatch pd)
        {
           
            

            try
            {

                ((MessengerServerTcp.MessengerServer)param).requestInvitePlayerToGuildBattleRoom(((Player)pd._session), pd._packet);

            }
            catch (exception e)
            {

                 message_pool.push(new message("[packet_func::packet028][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if (ExceptionError.STDA_SOURCE_ERROR_DECODE_TYPE(e.getCodeError()) != (uint)STDA_ERROR_TYPE.MESSAGE_SERVER)
                {
                    throw;
                }
            }

            return 0;
        }

        public static int packet029(object param,ParamDispatch pd)
        {
           
            

            try
            {

                // Verifica se session est� autorizada para executar esse a��o,
                // se ele n�o fez o login com o Server ele n�o pode fazer nada at� que ele fa�a o login
                if (!((Player)pd._session).m_is_authorized)
                {
                    throw new exception("[packet_func::" + "packet029" + "][Error] Player[UID=" + Convert.ToString(((Player)pd._session).m_pi.uid) + "] Nao esta autorizado a fazer esse request por que ele ainda nao fez o login com o Server. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PACKET_FUNC_MS,
                        1, 0x5000501));
                }

                uint player_uid_send = pd._packet.ReadUInt32();
                uint player_uid_receive = pd._packet.ReadUInt32();

                 message_pool.push(new message("[packet_func::packet029][Log] Player[UID=" + Convert.ToString(((Player)pd._session).m_pi.uid) + "] message Player[UID=" + Convert.ToString(player_uid_send) + "] gift item to player[UID=" + Convert.ToString(player_uid_receive) + "].", type_msg.CL_FILE_LOG_AND_CONSOLE));

            }
            catch (exception e)
            {

                 message_pool.push(new message("[packet_func::packet029][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if (ExceptionError.STDA_SOURCE_ERROR_DECODE_TYPE(e.getCodeError()) != (uint)STDA_ERROR_TYPE.MESSAGE_SERVER)
                {
                    throw;
                }
            }

            return 0;
        }

        public static int packet02A(object param,ParamDispatch pd)
        {
           
            

            try
            {

#if DEBUG
                 message_pool.push(new message("[packet_func::packet02A][Log] Player[UID=" + Convert.ToString(((Player)pd._session).m_pi.uid) + "] mandou o Pacote02A, player foi aceito na guild, nao faco o tratamento desse pacote ainda. Hex: \n\r" + pd._packet.Log(), type_msg.CL_FILE_LOG_AND_CONSOLE));
#else
					 message_pool.push(new message("[packet_func::packet02A][Log] Player[UID=" + Convert.ToString(((Player)pd._session).m_pi.uid) + "] mandou o Pacote02A, player foi aceito na guild, nao faco o tratamento desse pacote ainda. Hex: \n\r" + pd._packet.Log(), type_msg.CL_ONLY_CONSOLE));
#endif // _DEBUG

                // Verifica se session est� autorizada para executar esse a��o,
                // se ele n�o fez o login com o Server ele n�o pode fazer nada at� que ele fa�a o login
                if (!((Player)pd._session).m_is_authorized)
                {
                    throw new exception("[packet_func::" + "packet02A" + "][Error] Player[UID=" + Convert.ToString(((Player)pd._session).m_pi.uid) + "] Nao esta autorizado a fazer esse request por que ele ainda nao fez o login com o Server. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PACKET_FUNC_MS,
                        1, 0x5000501));
                }

            }
            catch (exception e)
            {

                 message_pool.push(new message("[packet_func::packet02A][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if (ExceptionError.STDA_SOURCE_ERROR_DECODE_TYPE(e.getCodeError()) != (uint)STDA_ERROR_TYPE.MESSAGE_SERVER)
                {
                    throw;
                }
            }

            return 0;
        }

        public static int packet02B(object param,ParamDispatch pd)
        {
           
            

            try
            {

#if DEBUG
                 message_pool.push(new message("[packet_func::packet02B][Log] Player[UID=" + Convert.ToString(((Player)pd._session).m_pi.uid) + "] mandou o Pacote02B, player foi quicado da guild, nao faco o tratamento desse pacote ainda. Hex: \n\r" + pd._packet.Log(), type_msg.CL_FILE_LOG_AND_CONSOLE));
#else
					 message_pool.push(new message("[packet_func::packet02B][Log] Player[UID=" + Convert.ToString(((Player)pd._session).m_pi.uid) + "] mandou o Pacote02B, player foi quicado da guild, nao faco o tratamento desse pacote ainda. Hex: \n\r" + pd._packet.Log(), type_msg.CL_ONLY_CONSOLE));
#endif // _DEBUG

                // Verifica se session est� autorizada para executar esse a��o,
                // se ele n�o fez o login com o Server ele n�o pode fazer nada at� que ele fa�a o login
                if (!((Player)pd._session).m_is_authorized)
                {
                    throw new exception("[packet_func::" + "packet02B" + "][Error] Player[UID=" + Convert.ToString(((Player)pd._session).m_pi.uid) + "] Nao esta autorizado a fazer esse request por que ele ainda nao fez o login com o Server. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PACKET_FUNC_MS,
                        1, 0x5000501));
                }

            }
            catch (exception e)
            {

                 message_pool.push(new message("[packet_func::packet02B][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if (ExceptionError.STDA_SOURCE_ERROR_DECODE_TYPE(e.getCodeError()) != (uint)STDA_ERROR_TYPE.MESSAGE_SERVER)
                {
                    throw;
                }
            }

            return 0;
        }

        public static int packet02C(object param,ParamDispatch pd)
        {
           
            

            try
            {

#if DEBUG
                 message_pool.push(new message("[packet_func::packet02C][Log] Player[UID=" + Convert.ToString(((Player)pd._session).m_pi.uid) + "] mandou o Pacote02C, troca a imagem da guild, nao faco o tratamento desse pacote ainda. Hex: \n\r" + pd._packet.Log(), type_msg.CL_FILE_LOG_AND_CONSOLE));
#else
					 message_pool.push(new message("[packet_func::packet02C][Log] Player[UID=" + Convert.ToString(((Player)pd._session).m_pi.uid) + "] mandou o Pacote02C, troca a imagem da guild, nao faco o tratamento desse pacote ainda. Hex: \n\r" + pd._packet.Log(), type_msg.CL_ONLY_CONSOLE));
#endif // _DEBUG

                // Verifica se session est� autorizada para executar esse a��o,
                // se ele n�o fez o login com o Server ele n�o pode fazer nada at� que ele fa�a o login
                if (!((Player)pd._session).m_is_authorized)
                {
                    throw new exception("[packet_func::" + "packet02C" + "][Error] Player[UID=" + Convert.ToString(((Player)pd._session).m_pi.uid) + "] Nao esta autorizado a fazer esse request por que ele ainda nao fez o login com o Server. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PACKET_FUNC_MS,
                        1, 0x5000501));
                }

            }
            catch (exception e)
            {

                 message_pool.push(new message("[packet_func::packet02C][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if (ExceptionError.STDA_SOURCE_ERROR_DECODE_TYPE(e.getCodeError()) != (uint)STDA_ERROR_TYPE.MESSAGE_SERVER)
                {
                    throw;
                }
            }

            return 0;
        }

        public static int packet02D(object param,ParamDispatch pd)
        {
           
            

            try
            {

#if DEBUG
                 message_pool.push(new message("[packet_func::packet02D][Log] Player[UID=" + Convert.ToString(((Player)pd._session).m_pi.uid) + "] mandou o Pacote02D, troca o nome da guild, nao faco o tratamento desse pacote ainda. Hex: \n\r" + pd._packet.Log(), type_msg.CL_FILE_LOG_AND_CONSOLE));
#else
					 message_pool.push(new message("[packet_func::packet02D][Log] Player[UID=" + Convert.ToString(((Player)pd._session).m_pi.uid) + "] mandou o Pacote02D, troca o nome da guild, nao faco o tratamento desse pacote ainda. Hex: \n\r" + pd._packet.Log(), type_msg.CL_ONLY_CONSOLE));
#endif // _DEBUG

                // Verifica se session est� autorizada para executar esse a��o,
                // se ele n�o fez o login com o Server ele n�o pode fazer nada at� que ele fa�a o login
                if (!((Player)pd._session).m_is_authorized)
                {
                    throw new exception("[packet_func::" + "packet02D" + "][Error] Player[UID=" + Convert.ToString(((Player)pd._session).m_pi.uid) + "] Nao esta autorizado a fazer esse request por que ele ainda nao fez o login com o Server. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PACKET_FUNC_MS,
                        1, 0x5000501));
                }

            }
            catch (exception e)
            {

                 message_pool.push(new message("[packet_func::packet02D][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if (ExceptionError.STDA_SOURCE_ERROR_DECODE_TYPE(e.getCodeError()) != (uint)STDA_ERROR_TYPE.MESSAGE_SERVER)
                {
                    throw;
                }
            }

            return 0;
        }

        // Server
        public static int packet_svFazNada(object param,ParamDispatch pd)
        {
           
            

            // Esse pacote � para os pacotes que o server envia para o cliente
            // e n�o precisa de tratamento depois que foi enviado para o cliente

            return 0;
        }

        // Auth Server
        public static int packet_as001(object param,ParamDispatch pd)
        {
           
            try
            {

                ((MessengerServerTcp.MessengerServer)param).requestAcceptGuildMember(pd._packet);

            }
            catch (exception e)
            {

                 message_pool.push(new message("[packet_func::packet_as001][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if (ExceptionError.STDA_SOURCE_ERROR_DECODE_TYPE(e.getCodeError()) != (uint)STDA_ERROR_TYPE.MESSAGE_SERVER)
                {
                    throw;
                }
            }

            return 0;
        }

        public static int packet_as002(object param,ParamDispatch pd)
        {  
            try
            {

                 ((MessengerServerTcp.MessengerServer)param).requestMemberExitedFromGuild(pd._packet);

            }
            catch (exception e)
            {

                 message_pool.push(new message("[packet_func::packet_as002][ErrorSystem] " + e.getFullMessageError()));

                if (ExceptionError.STDA_SOURCE_ERROR_DECODE_TYPE(e.getCodeError()) != (uint)STDA_ERROR_TYPE.MESSAGE_SERVER)
                {
                    throw;
                }
            }

            return 0;
        }

        public static int packet_as003(object param,ParamDispatch pd)
        {                                          
            try
            {

                 ((MessengerServerTcp.MessengerServer)param).requestKickGuildMember(pd._packet);

            }
            catch (exception e)
            {

                 message_pool.push(new message("[packet_func::packet_as003][ErrorSystem] " + e.getFullMessageError()));

                if (ExceptionError.STDA_SOURCE_ERROR_DECODE_TYPE(e.getCodeError()) != (uint)STDA_ERROR_TYPE.MESSAGE_SERVER)
                {
                    throw;
                }
            }

            return 0;
        }


        // BroadCast Friend And Guild Member Online
        public static void friend_broadcast(Dictionary<uint, Player> _m_player,
            PangyaBinaryWriter _p, SessionBase _s,
            byte _debug)
        {

            if (_s == null)
            {
                throw new exception("[packet_func::friend_broadcast][Error] session *_s is nullptr", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PACKET_FUNC_MS,
                    1, 2));
            }

            foreach (var el in _m_player)
            {

                if (el.Value != null && el.Value != _s)
                {
                    try
                    {
                        el.Value.Send(_p, _debug == 1);
                    }
                    catch (exception e)
                    {
                        sms.ms.getInstance().DisconnectSession(el.Value);
                    } 
                }
            }
            _m_player.Clear();
        }

        public static void friend_broadcast(Dictionary<uint, Player> _m_player,
            List<PangyaBinaryWriter> _v_p,
            SessionBase _s, byte _debug)
        {

            if (_s == null)
            {
                throw new exception("[packet_func::friend_broadcast][Error] session *_s is nullptr", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PACKET_FUNC_MS,
                    1, 2));
            }

            foreach (var el in _v_p)
            {

                if (el != null)
                {

                    foreach (var el2 in _m_player)
                    {

                        if (el2.Value != null && el2.Value != _s)
                        {  
                            try
                            {                                
                                (el2.Value).Send(el.GetBytes, 1); 
                            }
                            catch (exception e)
                            {
                                sms.ms.getInstance().DisconnectSession((el2.Value));

                            }

                        }
                    }           
                }
                else
                {
                    message_pool.push(new message("[packet_func::friend_broadcast][Error][WARNING] packet *p is nullptr.", type_msg.CL_FILE_LOG_AND_CONSOLE));
                }
            }

            _v_p.Clear();
            _m_player.Clear();
        }

        public static void session_send(PangyaBinaryWriter p, Player _session, int _debug)
        {
            if (_debug == 1)
            {
                Console.WriteLine("Send_Session:" + p.GetBytes.HexDump());
            }
            _session.Send(p);
        }
    }
}
