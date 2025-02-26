using GameServer.Game;
using GameServer.GameType;
using GameServer.Session;
using PangyaAPI.Network.Cmd;
using PangyaAPI.Network.PangyaPacket;
using PangyaAPI.Utilities;
using PangyaAPI.Utilities.BinaryModels;
using PangyaAPI.Utilities.Log;
using System;
using System.Runtime.InteropServices;
using _smp = PangyaAPI.Utilities.Log;
using static GameServer.GameType._Define;
using GameServer.Cmd;
using PangyaAPI.SQL.Manager;

namespace GameServer.PacketFunc
{
    /// <summary>
    /// somente as requisicoes feitas pelo cliente
    /// </summary>
    public class packet_func_cl
    {
         public static int packet002(ParamDispatch _arg1)
        {
            try
            {
                sgs.gs.getInstance().requestLogin((Player)_arg1._session, _arg1._packet);
                return 1;
            }
            catch (exception ex)
            {
                Console.WriteLine($"[packet_func_cl::packet002][StError]: {ex.getFullMessageError()}");
                _arg1._session.Disconnect();//chama a desconexao
                return 0;
            }
            return 0;
        }

        public static int packet003(ParamDispatch pd)
        {
            try
            {

                sgs.gs.getInstance().requestChat((Player)pd._session, pd._packet); return 1;

            }
            catch (exception ex)
            {
                Console.WriteLine($"[packet_func_cl::packet003][StError]:  {ex.getFullMessageError()}");
            }
            return 0;
        }

        public static int packet004(ParamDispatch pd)
        {
            try
            {
                // Enter Channel, channel ID
                sgs.gs.getInstance().requestEnterChannel((Player)pd._session, pd._packet); return 1;
            }
            catch (exception ex)
            {
                Console.WriteLine($"[packet_func_cl::packet004][StError]:  {ex.getFullMessageError()}");
            }
            return 0;
        }

        public static int packet006(ParamDispatch pd)
        {
            try
            {

                var c = sgs.gs.getInstance().findChannel(((Player)pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestFinishGame(((Player)pd._session), pd._packet); return 1;
                }

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet006][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if ((STDA_ERROR_TYPE)ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) != STDA_ERROR_TYPE.CHANNEL)
                {
                    throw;
                }
                return 0;
            }
            return 0; return 0;

        }
        enum NICK_CHECK : byte
        {
            SUCCESS, // Sucesso por trocar o nick por que ele está disponível
            UNKNOWN_ERROR, // Erro desconhecido, Error ao verificar NICK
            NICK_IN_USE, // NICKNAME já em uso
            INCORRECT_NICK, // INCORRET nick, tamanho < 4 ou tem caracteres que não pode
            NOT_ENOUGH_COOKIE, // Não tem points suficiente
            HAVE_BAD_WORD, // Tem palavras que não pode no NICK
            ERROR_DB, // Erro DB
            EMPETY_ERROR, // Erro Vazio
            EMPETY_ERROR_2, // ERRO VAZIO 2
            SAME_NICK_USED, // O Mesmo nick vai ser usado, estou usando para o mesmo que o ID
            EMPETY_ERROR_3, // ERRO VAZIO 3
            CODE_ERROR_INFO = 12 // CODE  ERROR INFO arquivo iff, o código do erro para mostra no cliente
        }


        public static int packet007(ParamDispatch pd)
        {


            return 0;
        }


        public static int packet008(ParamDispatch pd)
        {
            _smp::message_pool.push(new message("[packet_func::packet008][Log]: " + pd._packet.Log(), type_msg.CL_FILE_LOG_AND_CONSOLE));

            try
            {

                Channel _channel = sgs.gs.getInstance().findChannel(((Player)pd._session).m_pi.channel);

                if (_channel != null)
                {
                    _channel.requestMakeRoom(((Player)pd._session), pd._packet); return 1;
                }

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet008][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if ((STDA_ERROR_TYPE)ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) != STDA_ERROR_TYPE.CHANNEL) // Por Hora relança qualquer exception que não seja do channel
                {
                    throw;
                }
                return 0;
            }
            return 0; return 0;
        }

        public static int packet009(ParamDispatch pd)
        {
            try
            {

                Channel _channel = sgs.gs.getInstance().findChannel(((Player)pd._session).m_pi.channel);

                if (_channel != null)
                {
                    _channel.requestEnterRoom(((Player)pd._session), pd._packet); return 1;
                }

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet009][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if ((STDA_ERROR_TYPE)ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) != STDA_ERROR_TYPE.CHANNEL) // Por Hora relança qualquer exception que não seja do channel
                {
                    throw;
                }
                return 0;
            }
            return 0; return 0;
        }

        public static int packet00A(ParamDispatch pd)
        {
            try
            {

                Channel _channel = sgs.gs.getInstance().findChannel(((Player)pd._session).m_pi.channel);

                if (_channel != null)
                {
                    _channel.requestChangeInfoRoom(((Player)pd._session), pd._packet); return 1;
                }

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet00A][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if ((STDA_ERROR_TYPE)ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) != STDA_ERROR_TYPE.CHANNEL) // Por Hora relança qualquer exception que não seja do channel
                {
                    throw;
                }
                return 0;
            }
            return 0;
        }

        public static int packet00B(ParamDispatch pd)
        {



            try
            {

                Channel _channel = sgs.gs.getInstance().findChannel(((Player)pd._session).m_pi.channel);

                if (_channel != null)
                {
                    _channel.requestChangePlayerItemChannel(((Player)pd._session), pd._packet); return 1;
                }

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet00B][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if ((STDA_ERROR_TYPE)ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) != STDA_ERROR_TYPE.CHANNEL) // Por Hora relança qualquer exception que não seja do channel
                {
                    throw;
                }
                return 0;
            }
            return 0;

        }

        public static int packet00C(ParamDispatch pd)
        {
            try
            {

                Channel _channel = sgs.gs.getInstance().findChannel(((Player)pd._session).m_pi.channel);

                // Bloquear para ver se funciona o sync do entra depois no camp,
                // mesmo que o outro(0x9D) chama primeiro esse(0x0C) é mais rápido para verificar se o player está em uma sala
                //

                if (_channel != null)
                {
                    _channel.requestChangePlayerItemRoom(((Player)pd._session), pd._packet); return 1;
                }

                // Bloquear para ver se funciona o sync do entra depois no camp,
                // mesmo que o outro(0x9D) chama primeiro esse(0x0C) é mais rápido para verificar se o player está em uma sala
                //

            }
            catch (exception e)
            {
                // Bloquear para ver se funciona o sync do entra depois no camp,
                // mesmo que o outro(0x9D) chama primeiro esse(0x0C) é mais rápido para verificar se o player está em uma sala
                //  
                _smp.message_pool.push(new message("[packet_func::packet00C][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if ((STDA_ERROR_TYPE)ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) != STDA_ERROR_TYPE.CHANNEL) // Por Hora relança qualquer exception que não seja do channel
                {
                    throw;
                }
                return 0;
            }
            return 0; return 0;
        }

        public static int packet00D(ParamDispatch pd)
        {
            try
            {

                Channel c = sgs.gs.getInstance().findChannel(((Player)pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestChangePlayerStateReadyRoom(((Player)pd._session), pd._packet); return 1;
                }

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet00D][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if ((STDA_ERROR_TYPE)ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) != STDA_ERROR_TYPE.CHANNEL) // Por Hora relança qualquer exception que não seja do channel
                {
                    throw;
                }
                return 0;
            }
            return 0;

        }

        public static int packet00E(ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(((Player)pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestStartGame(((Player)pd._session), pd._packet); return 1;
                }

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet00E][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if ((STDA_ERROR_TYPE)ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) != STDA_ERROR_TYPE.CHANNEL)
                {
                    throw;
                }
                return 0;
            }
            return 0;

        }

        public static int packet00F(ParamDispatch pd)
        {



            try
            {

                Channel c = sgs.gs.getInstance().findChannel(((Player)pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestExitRoom(((Player)pd._session), pd._packet); return 1;
                }

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet00F][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if ((STDA_ERROR_TYPE)ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) != STDA_ERROR_TYPE.CHANNEL) // Por Hora relança qualquer exception que não seja do channel
                {
                    throw;
                }
                return 0;
            }
            return 0;

        }

        public static int packet010(ParamDispatch pd)
        {



            try
            {

                Channel c = sgs.gs.getInstance().findChannel(((Player)pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestChangePlayerTeamRoom(((Player)pd._session), pd._packet); return 1;
                }

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet010][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if ((STDA_ERROR_TYPE)ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) != STDA_ERROR_TYPE.CHANNEL) // Por Hora relança qualquer exception que não seja do channel
                {
                    throw;
                }
                return 0;
            }
            return 0;

        }

        public static int packet011(ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(((Player)pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestFinishLoadHole(((Player)pd._session), pd._packet); return 1;
                }

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet011][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if ((STDA_ERROR_TYPE)ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) != STDA_ERROR_TYPE.CHANNEL)
                {
                    throw;
                }
                return 0;
            }
            return 0;

        }

        public static int packet012(ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(((Player)pd._session).m_pi.channel);

#if DEBUG
                _smp.message_pool.push(new message("[packet_func::packet12][Log] request Player[UID=" + Convert.ToString(((Player)pd._session).m_pi.uid) + "]", type_msg.CL_FILE_LOG_AND_CONSOLE));
#endif // _DEBUG

                if (c != null)
                {
                    c.requestInitShot(((Player)pd._session), pd._packet); return 1;
                }

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet012][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if ((STDA_ERROR_TYPE)ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) != STDA_ERROR_TYPE.CHANNEL)
                {
                    throw;
                }
                return 0;
            }
            return 0;

        }

        public static int packet013(ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(((Player)pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestChangeMira(((Player)pd._session), pd._packet); return 1;
                }

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet013][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if ((STDA_ERROR_TYPE)ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) != STDA_ERROR_TYPE.CHANNEL)
                {
                    throw;
                }
                return 0;
            }
            return 0;

        }

        public static int packet014(ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(((Player)pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestChangeStateBarSpace(((Player)pd._session), pd._packet); return 1;
                }

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet014][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if ((STDA_ERROR_TYPE)ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) != STDA_ERROR_TYPE.CHANNEL)
                {
                    throw;
                }
                return 0;
            }
            return 0;

        }

        public static int packet015(ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(((Player)pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestActivePowerShot(((Player)pd._session), pd._packet); return 1;
                }

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet015][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if ((STDA_ERROR_TYPE)ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) != STDA_ERROR_TYPE.CHANNEL)
                {
                    throw;
                }
                return 0;
            }
            return 0; return 0;
        }

        public static int packet016(ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(((Player)pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestChangeClub(((Player)pd._session), pd._packet); return 1;
                }

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet016][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if ((STDA_ERROR_TYPE)ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) != STDA_ERROR_TYPE.CHANNEL)
                {
                    throw;
                }
                return 0;
            }
            return 0;
            return 0;

        }

        public static int packet017(ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(((Player)pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestUseActiveItem(((Player)pd._session), pd._packet); return 1;
                }

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet017][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if ((STDA_ERROR_TYPE)ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) != STDA_ERROR_TYPE.CHANNEL)
                {
                    throw;
                }
                return 0;
            }
            return 0;

        }

        public static int packet018(ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(((Player)pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestChangeStateTypeing(((Player)pd._session), pd._packet); return 1;
                }

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet018][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if ((STDA_ERROR_TYPE)ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) != STDA_ERROR_TYPE.CHANNEL)
                {
                    throw;
                }
                return 0;
            }
            return 0;

        }

        public static int packet019(ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(((Player)pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestMoveBall(((Player)pd._session), pd._packet); return 1;
                }

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet019][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if ((STDA_ERROR_TYPE)ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) != STDA_ERROR_TYPE.CHANNEL)
                {
                    throw;
                }
                return 0;
            }
            return 0;

        }

        public static int packet01A(ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(((Player)pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestInitHole(((Player)pd._session), pd._packet); return 1;
                }

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet01A][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if ((STDA_ERROR_TYPE)ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) != STDA_ERROR_TYPE.CHANNEL)
                {
                    throw;
                }
                return 0;
            }
            return 0;

        }

        public static int packet01B(ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(((Player)pd._session).m_pi.channel);

#if DEBUG
                _smp.message_pool.push(new message("[packet_func::packet1B][Log] request Player[UID=" + Convert.ToString(((Player)pd._session).m_pi.uid) + "]", type_msg.CL_FILE_LOG_AND_CONSOLE));
#endif // _DEBUG

                if (c != null)
                {
                    c.requestSyncShot(((Player)pd._session), pd._packet); return 1;
                }

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet01B][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if ((STDA_ERROR_TYPE)ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) != STDA_ERROR_TYPE.CHANNEL)
                {
                    throw;
                }
                return 0;
            }
            return 0;

        }

        public static int packet01C(ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(((Player)pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestFinishShot(((Player)pd._session), pd._packet); return 1;
                }

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet01C][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if ((STDA_ERROR_TYPE)ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) != STDA_ERROR_TYPE.CHANNEL)
                {
                    throw;
                }
                return 0;
            }
            return 0;

        }

        public static int packet01D(ParamDispatch pd)
        {



            try
            {

                Channel c = sgs.gs.getInstance().findChannel(((Player)pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestBuyItemShop(((Player)pd._session), pd._packet); return 1;
                }

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet01D][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if ((STDA_ERROR_TYPE)ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) != STDA_ERROR_TYPE.CHANNEL) // Por Hora relança qualquer exception que não seja do channel
                {
                    throw;
                }
                return 0;
            }
            return 0;

        }

        public static int packet01F(ParamDispatch pd)
        {



            try
            {

                Channel c = sgs.gs.getInstance().findChannel(((Player)pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestGiftItemShop(((Player)pd._session), pd._packet); return 1;
                }

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet01F][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if ((STDA_ERROR_TYPE)ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) != STDA_ERROR_TYPE.CHANNEL) // Por Hora relança qualquer excpetion que não seja do channel
                {
                    throw;
                }
                return 0;
            }
            return 0;

        }

        public static int packet020(ParamDispatch pd)
        {
            try
            {
                Channel c = sgs.gs.getInstance().findChannel(((Player)pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestChangePlayerItemMyRoom(((Player)pd._session), pd._packet); return 1;
                }
            }
            catch (exception e)
            {
                _smp.message_pool.push(new message("[packet_func::packet020][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if ((STDA_ERROR_TYPE)ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) != STDA_ERROR_TYPE.CHANNEL) // Por Hora relança qualquer exception que não seja do channel
                {
                    throw;
                }
                return 0;
            }
            return 0;
        }

        public static int packet022(ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(((Player)pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestStartTurnTime(((Player)pd._session), pd._packet); return 1;
                }

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet022][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if ((STDA_ERROR_TYPE)ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) != STDA_ERROR_TYPE.CHANNEL)
                {
                    throw;
                }
                return 0;
            }
            return 0;

        }

        public static int packet026(ParamDispatch pd)
        {
            try
            {

                Channel c = sgs.gs.getInstance().findChannel(((Player)pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestKickPlayerOfRoom(((Player)pd._session), pd._packet); return 1;
                }

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet026][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
                return 0;

            }
            return 0;

        }

        public static int packet029(ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(((Player)pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestCheckInvite(((Player)pd._session), pd._packet); return 1;
                }

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet029][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if ((STDA_ERROR_TYPE)ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) != STDA_ERROR_TYPE.CHANNEL)
                {
                    throw;
                }
                return 0;
            }
            return 0;

        }

        public static int packet02A(ParamDispatch pd)
        {



            try
            {

                sgs.gs.getInstance().requestPrivateMessage(((Player)pd._session), pd._packet); return 1;

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet02A][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if ((STDA_ERROR_TYPE)ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) != STDA_ERROR_TYPE.GAME_SERVER)
                {
                    throw;
                }
                return 0;
            }
            return 0;

        }

        public static int packet02D(ParamDispatch pd)
        {
            try
            {

                Channel c = sgs.gs.getInstance().findChannel(((Player)pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestShowInfoRoom(((Player)pd._session), pd._packet); return 1;
                }

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet02D][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
                return 0;

            }

            return 0;
        }

        public static int packet02F(ParamDispatch pd)
        {



            try
            {

                sgs.gs.getInstance().requestPlayerInfo(((Player)pd._session), pd._packet); return 1;

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet02F][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if ((STDA_ERROR_TYPE)ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) != STDA_ERROR_TYPE.GAME_SERVER)
                {
                    throw;
                }
                return 0;
            }
            return 0;

        }

        public static int packet030(ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(((Player)pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestUnOrPauseGame(((Player)pd._session), pd._packet); return 1;
                }

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet030][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if ((STDA_ERROR_TYPE)ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) != STDA_ERROR_TYPE.CHANNEL)
                {
                    throw;
                }
                return 0;
            }
            return 0;

        }

        public static int packet031(ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(((Player)pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestFinishHoleData(((Player)pd._session), pd._packet); return 1;
                }

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet031][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if ((STDA_ERROR_TYPE)ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) != STDA_ERROR_TYPE.CHANNEL)
                {
                    throw;
                }
                return 0;
            }
            return 0;

        }

        public static int packet032(ParamDispatch pd)
        {



            try
            {

                Channel c = sgs.gs.getInstance().findChannel(((Player)pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestChangePlayerStateAFKRoom(((Player)pd._session), pd._packet); return 1;
                }

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet032][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if ((STDA_ERROR_TYPE)ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) != STDA_ERROR_TYPE.CHANNEL)
                {
                    throw;
                }
                return 0;
            }
            return 0;

        }

        public static int packet033(ParamDispatch pd)
        {



            try
            {

                sgs.gs.getInstance().requestExceptionClientMessage(((Player)pd._session), pd._packet); return 1;

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet033][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if ((STDA_ERROR_TYPE)ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) != STDA_ERROR_TYPE.GAME_SERVER)
                {
                    throw;
                }
                return 0;
            }
            return 0;

        }

        public static int packet034(ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(((Player)pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestFinishCharIntro(((Player)pd._session), pd._packet); return 1;
                }

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet034][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if ((STDA_ERROR_TYPE)ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) != STDA_ERROR_TYPE.CHANNEL)
                {
                    throw;
                }
                return 0;
            }
            return 0;

        }

        public static int packet035(ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(((Player)pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestTeamFinishHole(((Player)pd._session), pd._packet); return 1;
                }

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet035][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if ((STDA_ERROR_TYPE)ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) != STDA_ERROR_TYPE.CHANNEL)
                {
                    throw;
                }
                return 0;
            }
            return 0;

        }

        public static int packet036(ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(((Player)pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestReplyContinueVersus(((Player)pd._session), pd._packet); return 1;
                }

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet036][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if ((STDA_ERROR_TYPE)ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) != STDA_ERROR_TYPE.CHANNEL)
                {
                    throw;
                }
                return 0;
            }
            return 0;

        }

        public static int packet037(ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(((Player)pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestLastPlayerFinishVersus(((Player)pd._session), pd._packet); return 1;
                }

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet037][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if ((STDA_ERROR_TYPE)ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) != STDA_ERROR_TYPE.CHANNEL)
                {
                    throw;
                }
                return 0;
            }
            return 0;

        }

        public static int packet039(ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(((Player)pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestPayCaddieHolyDay(((Player)pd._session), pd._packet); return 1;
                }

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet039][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if ((STDA_ERROR_TYPE)ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) != STDA_ERROR_TYPE.CHANNEL)
                {
                    throw;
                }
                return 0;
            }
            return 0;

        }

        public static int packet03A(ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(((Player)pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestPlayerReportChatGame(((Player)pd._session), pd._packet); return 1;
                }

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet03A][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if ((STDA_ERROR_TYPE)ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) != STDA_ERROR_TYPE.CHANNEL)
                {
                    throw;
                }
                return 0;
            }
            return 0;

        }

        // 2018-03-04 19:26:39.633	Tipo: 60(0x3C), desconhecido ou nao implementado. func_arr::getPacketCall()	 Error Code: 335609856
        // 2018-03-04 19:26:39.633	size packet: 4
        //
        //0000 3C 00 1F 01 -- -- -- -- -- -- -- -- -- -- -- -- 	<...............
        //static int packet03C(void* _arg1, void* _arg2);	// manda msg OFF na opção 0x6F e a opção 0x11F pede a lista de amigos para enviar presente

        public static int packet03C(ParamDispatch pd)
        {



            try
            {

                sgs.gs.getInstance().requestTranslateSubPacket(((Player)pd._session), pd._packet); return 1;

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet03C][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if ((STDA_ERROR_TYPE)ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) != STDA_ERROR_TYPE.GAME_SERVER)
                {
                    throw;
                }
                return 0;
            }
            return 0;

        }

        public static int packet03D(ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(((Player)pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestCookie(((Player)pd._session), pd._packet); return 1;
                }

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet03D][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if ((STDA_ERROR_TYPE)ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) != STDA_ERROR_TYPE.CHANNEL)
                {
                    throw;
                }
                return 0;
            }
            return 0;

        }

        public static int packet03E(ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(((Player)pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestEnterSpyRoom(((Player)pd._session), pd._packet); return 1;
                }

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet03E][ErrorSystem] " + e.getCodeError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if ((STDA_ERROR_TYPE)ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) != STDA_ERROR_TYPE.CHANNEL)
                {
                    throw;
                }
                return 0;
            }
            return 0;

        }

        public static int packet041(ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(((Player)pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestExecCCGIdentity(((Player)pd._session), pd._packet); return 1;
                }

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet041][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if ((STDA_ERROR_TYPE)ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) != STDA_ERROR_TYPE.CHANNEL)
                {
                    throw;
                }
                return 0;
            }
            return 0;

        }

        public static int packet042(ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(((Player)pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestInitShotArrowSeq(((Player)pd._session), pd._packet); return 1;
                }

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet042][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if ((STDA_ERROR_TYPE)ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) != STDA_ERROR_TYPE.CHANNEL)
                {
                    throw;
                }
                return 0;
            }
            return 0;

        }

        public static int packet043(ParamDispatch pd)
        {



            try
            {

                sgs.gs.getInstance().sendServerListAndChannelListToSession(((Player)pd._session));

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet043][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if ((STDA_ERROR_TYPE)ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) != STDA_ERROR_TYPE.GAME_SERVER)
                {
                    throw;
                }
                return 0;
            }
            return 0;

        }

        public static int packet047(ParamDispatch pd)
        {
            try
            {

                sgs.gs.getInstance().sendRankServer(((Player)pd._session));

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet047][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if ((STDA_ERROR_TYPE)ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) != STDA_ERROR_TYPE.GAME_SERVER)
                {
                    throw;
                }
                return 0;
            }
            return 0;

        }

        public static int packet048(ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(((Player)pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestLoadGamePercent(((Player)pd._session), pd._packet); return 1;
                }

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet048][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if ((STDA_ERROR_TYPE)ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) != STDA_ERROR_TYPE.CHANNEL)
                {
                    throw;
                }
                return 0;
            }
            return 0;

        }

        public static int packet04A(ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(((Player)pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestActiveReplay(((Player)pd._session), pd._packet); return 1;
                }

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet04A][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if ((STDA_ERROR_TYPE)ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) != STDA_ERROR_TYPE.CHANNEL)
                {
                    throw;
                }
                return 0;
            }
            return 0;

        }

        public static int packet04B(ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(((Player)pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestClubSetStatsUpdate(((Player)pd._session), pd._packet); return 1;
                }

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet04B][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if ((STDA_ERROR_TYPE)ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) != STDA_ERROR_TYPE.CHANNEL)
                {
                    throw;
                }
                return 0;
            }
            return 0;

        }

        public static int packet04F(ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(((Player)pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestChangeStateChatBlock(((Player)pd._session), pd._packet); return 1;
                }

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet04F][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if ((STDA_ERROR_TYPE)ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) != STDA_ERROR_TYPE.CHANNEL)
                {
                    throw;
                }
                return 0;
            }
            return 0;

        }

        public static int packet054(ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(((Player)pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestChatTeam(((Player)pd._session), pd._packet); return 1;
                }

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet054][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if ((STDA_ERROR_TYPE)ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) != STDA_ERROR_TYPE.CHANNEL)
                {
                    throw;
                }
                return 0;
            }
            return 0;

        }

        public static int packet055(ParamDispatch pd)
        {



            try
            {

                sgs.gs.getInstance().requestChangeWhisperState(((Player)pd._session), pd._packet); return 1;

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet055][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if ((STDA_ERROR_TYPE)ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) != STDA_ERROR_TYPE.GAME_SERVER)
                {
                    throw;
                }
                return 0;
            }
            return 0;

        }

        public static int packet057(ParamDispatch pd)
        {



            try
            {

                sgs.gs.getInstance().requestCommandNoticeGM(((Player)pd._session), pd._packet); return 1;

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet057][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if ((STDA_ERROR_TYPE)ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) != STDA_ERROR_TYPE.GAME_SERVER)
                {
                    throw;
                }
                return 0;
            }
            return 0;

        }

        public static int packet05C(ParamDispatch pd)
        {



            try
            {

                sgs.gs.getInstance().sendDateTimeToSession(((Player)pd._session));

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet05C][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if ((STDA_ERROR_TYPE)ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) != STDA_ERROR_TYPE.GAME_SERVER)
                {
                    throw;
                }
                return 0;
            }
            return 0;

        }

        // 2018 - 12 - 01 18:49 : 14.928 size packet : 4
        // Destroy Room, 2 Bytes Room Number
        // 0000 60 00 01 00 -- -- -- -- -- -- -- -- -- -- -- --    `...............
        public static int packet060(ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(((Player)pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestExecCCGDestroy(((Player)pd._session), pd._packet); return 1;
                }

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet060][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if ((STDA_ERROR_TYPE)ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) != STDA_ERROR_TYPE.CHANNEL)
                {
                    throw;
                }
                return 0;
            }
            return 0;

        }

        // 2018 - 12 - 01 18:48 : 02.634 size packet : 6
        // Disconnect User, 2 Bytes Online ID
        // 0000 61 00 00 00 00 00 -- -- -- -- -- -- -- -- -- --a...............
        public static int packet061(ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(((Player)pd._session).m_pi.channel);

                if (c != null)
                {
                    _smp.message_pool.push(new message("[packet_func::packet061][Log] player[UID=" + Convert.ToString(((Player)pd._session).m_pi.uid) + "] tentou desconectar um player, mas o server ja faz o tratamento do packet08F do comando GM.", type_msg.CL_FILE_LOG_AND_CONSOLE));
                }

                // Verifica se session está varrizada para executar esse ação,
                // se ele não fez o login com o Server ele não pode fazer nada até que ele faça o login
                if (!((Player)pd._session).m_is_authorized)
                {
                    //throw new exception("[packet_func::" + "packet061" + "][Error] Player[UID=" + Convert.ToString(((Player)pd._session).m_pi.m_uid) + "] Nao esta varrizado a fazer esse request por que ele ainda nao fez o login com o Server. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PACKET_FUNC_SV,
                    //    1, 0x7000501));
                }

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet061][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if ((STDA_ERROR_TYPE)ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) != STDA_ERROR_TYPE.CHANNEL)
                {
                    throw;
                }
                return 0;
            }
            return 0;

        }

        public static int packet063(ParamDispatch pd)
        {



            try
            {

                Channel c = sgs.gs.getInstance().findChannel(((Player)pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestPlayerLocationRoom(((Player)pd._session), pd._packet); return 1;
                }

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet063][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));


            }
            return 0;
        }

        public static int packet064(ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(((Player)pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestDeleteActiveItem(((Player)pd._session), pd._packet); return 1;
                }

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet064][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if ((STDA_ERROR_TYPE)ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) != STDA_ERROR_TYPE.CHANNEL)
                {
                    throw;
                }
                return 0;
            }
            return 0;

        }

        public static int packet065(ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(((Player)pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestActiveBooster(((Player)pd._session), pd._packet); return 1;
                }

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet065][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if ((STDA_ERROR_TYPE)ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) != STDA_ERROR_TYPE.CHANNEL)
                {
                    throw;
                }
                return 0;
            }
            return 0;

        }

        public static int packet066(ParamDispatch pd)
        {



            try
            {

                sgs.gs.getInstance().requestSendTicker(((Player)pd._session), pd._packet); return 1;

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet066][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if ((STDA_ERROR_TYPE)ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) != STDA_ERROR_TYPE.GAME_SERVER)
                {
                    throw;
                }
                return 0;
            }
            return 0;

        }

        public static int packet067(ParamDispatch pd)
        {



            try
            {

                sgs.gs.getInstance().requestQueueTicker(((Player)pd._session), pd._packet); return 1;

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet067][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if ((STDA_ERROR_TYPE)ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) != STDA_ERROR_TYPE.GAME_SERVER)
                {
                    throw;
                }
                return 0;
            }
            return 0;

        }

        public static int packet069(ParamDispatch pd)
        {



            try
            {

                sgs.gs.getInstance().requestChangeChatMacroUser(((Player)pd._session), pd._packet); return 1;

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet069][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if ((STDA_ERROR_TYPE)ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) != STDA_ERROR_TYPE.GAME_SERVER)
                {
                    throw;
                }
                return 0;
            }
            return 0;

        }

        public static int packet06B(ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(((Player)pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestSetNoticeBeginCaddieHolyDay(((Player)pd._session), pd._packet); return 1;
                }

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet06B][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if ((STDA_ERROR_TYPE)ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) != STDA_ERROR_TYPE.CHANNEL)
                {
                    throw;
                }
                return 0;
            }
            return 0;

        }

        public static int packet073(ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(((Player)pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestChangeMascotMessage(((Player)pd._session), pd._packet); return 1;
                }

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet73][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if ((STDA_ERROR_TYPE)ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) != STDA_ERROR_TYPE.CHANNEL)
                {
                    throw;
                }
                return 0;
            }
            return 0;

        }

        public static int packet074(ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(((Player)pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestCancelEditSaleShop(((Player)pd._session), pd._packet); return 1;
                }

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet074][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if ((STDA_ERROR_TYPE)ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) != STDA_ERROR_TYPE.CHANNEL)
                {
                    throw;
                }
                return 0;
            }
            return 0;

        }

        public static int packet075(ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(((Player)pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestCloseSaleShop(((Player)pd._session), pd._packet); return 1;
                }

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet075][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if ((STDA_ERROR_TYPE)ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) != STDA_ERROR_TYPE.CHANNEL)
                {
                    throw;
                }
                return 0;
            }
            return 0;

        }

        public static int packet076(ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(((Player)pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestOpenEditSaleShop(((Player)pd._session), pd._packet); return 1;
                }

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet076][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if ((STDA_ERROR_TYPE)ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) != STDA_ERROR_TYPE.CHANNEL)
                {
                    throw;
                }
                return 0;
            }
            return 0;

        }

        public static int packet077(ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(((Player)pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestViewSaleShop(((Player)pd._session), pd._packet); return 1;
                }

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet077][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if ((STDA_ERROR_TYPE)ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) != STDA_ERROR_TYPE.CHANNEL)
                {
                    throw;
                }
                return 0;
            }
            return 0;

        }

        public static int packet078(ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(((Player)pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestCloseViewSaleShop(((Player)pd._session), pd._packet); return 1;
                }

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet078][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if ((STDA_ERROR_TYPE)ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) != STDA_ERROR_TYPE.CHANNEL)
                {
                    throw;
                }
                return 0;
            }
            return 0;

        }

        public static int packet079(ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(((Player)pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestChangeNameSaleShop(((Player)pd._session), pd._packet); return 1;
                }

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet079][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if ((STDA_ERROR_TYPE)ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) != STDA_ERROR_TYPE.CHANNEL)
                {
                    throw;
                }
                return 0;
            }
            return 0;

        }

        public static int packet07A(ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(((Player)pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestVisitCountSaleShop(((Player)pd._session), pd._packet); return 1;
                }

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet07A][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if ((STDA_ERROR_TYPE)ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) != STDA_ERROR_TYPE.CHANNEL)
                {
                    throw;
                }
                return 0;
            }
            return 0;

        }

        public static int packet07B(ParamDispatch pd)
        {



            try
            {

                var r = sgs.gs.getInstance().findChannel(((Player)pd._session).m_pi.channel);

                if (r != null)
                {
                    r.requestPangSaleShop(((Player)pd._session), pd._packet); return 1;
                }

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet07B][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if ((STDA_ERROR_TYPE)ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) != STDA_ERROR_TYPE.CHANNEL)
                {
                    throw;
                }
                return 0;
            }
            return 0;

        }

        public static int packet07C(ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(((Player)pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestOpenSaleShop(((Player)pd._session), pd._packet); return 1;
                }

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet07C][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if ((STDA_ERROR_TYPE)ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) != STDA_ERROR_TYPE.CHANNEL)
                {
                    throw;
                }
                return 0;
            }
            return 0;

        }

        public static int packet07D(ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(((Player)pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestBuyItemSaleShop(((Player)pd._session), pd._packet); return 1;
                }

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet07D][Error] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if ((STDA_ERROR_TYPE)ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) != STDA_ERROR_TYPE.CHANNEL)
                {
                    throw;
                }
                return 0;
            }
            return 0;

        }

        public static int packet081(ParamDispatch pd)
        {



            try
            {
                Channel c = sgs.gs.getInstance().findChannel(((Player)pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestEnterLobby(((Player)pd._session), pd._packet); return 1;
                }

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet081][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if ((STDA_ERROR_TYPE)ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) != STDA_ERROR_TYPE.CHANNEL)
                {
                    throw;
                }
                return 0;
            }
            return 0;

        }

        public static int packet082(ParamDispatch pd)
        {
            try
            {
                Channel c = sgs.gs.getInstance().findChannel(((Player)pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestExitLobby(((Player)pd._session), pd._packet); return 1;
                }

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet082][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

            }

            return 0;
        }

        public static int packet083(ParamDispatch pd)
        {



            try
            {
                sgs.gs.getInstance().requestEnterOtherChannelAndLobby(((Player)pd._session), pd._packet); return 1;

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet083][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if ((STDA_ERROR_TYPE)ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) != STDA_ERROR_TYPE.GAME_SERVER)
                {
                    throw;
                }
                return 0;
            }
            return 0;

        }

        public static int packet088(ParamDispatch pd)
        {



            try
            {

                sgs.gs.getInstance().requestCheckGameGuardAuthAnswer(((Player)pd._session), pd._packet); return 1;

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet088][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if ((STDA_ERROR_TYPE)ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) != STDA_ERROR_TYPE.GAME_SERVER)
                {
                    throw;
                }
                return 0;
            }
            return 0;

        }

        public static int packet08B(ParamDispatch pd)
        {



            try
            {

                // Verifica se session está varrizada para executar esse ação,
                // se ele não fez o login com o Server ele não pode fazer nada até que ele faça o login
                if (!((Player)pd._session).m_is_authorized)
                {
                    throw new exception("[packet_func::" + "packet08B" + "][Error] Player[UID=" + Convert.ToString(((Player)pd._session).m_pi.uid) + "] Nao esta varrizado a fazer esse request por que ele ainda nao fez o login com o Server. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PACKET_FUNC_SV,
                        1, 0x7000501));
                }

                CmdServerList cmd_sl = new CmdServerList(TYPE_SERVER.MSN); // waitable

                NormalManagerDB.add(0, cmd_sl, null, null);

                if (cmd_sl.getException().getCodeError() != 0)
                    throw cmd_sl.getException();

                var v_si = cmd_sl.getServerList();

                ((Player)pd._session).Send(packet_func_sv.pacote0FC(v_si));
                return 1;
            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet08B][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
            return 0;
        }

        public static int packet08F(ParamDispatch pd)
        {
            try
            {

                sgs.gs.getInstance().requestCommonCmdGM(((Player)pd._session), pd._packet); return 1;

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet08F][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if ((STDA_ERROR_TYPE)ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) != STDA_ERROR_TYPE.GAME_SERVER)
                {
                    throw;
                }
                return 0;
            }
            return 0;

        }

        public static int packet098(ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(((Player)pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestOpenPapelShop(((Player)pd._session), pd._packet); return 1;
                }

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet098][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if ((STDA_ERROR_TYPE)ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) != STDA_ERROR_TYPE.CHANNEL)
                {
                    throw;
                }
                return 0;
            }
            return 0;

        }

        public static int packet09C(ParamDispatch pd)
        {



            try
            {

                // Verifica se session está varrizada para executar esse ação,
                // se ele não fez o login com o Server ele não pode fazer nada até que ele faça o login
                if (!((Player)pd._session).m_is_authorized)
                {
                    throw new exception("[packet_func::" + "packet09C(Last5Player)" + "][Error] Player[UID=" + Convert.ToString(((Player)pd._session).m_pi.uid) + "] Nao esta varrizado a fazer esse request por que ele ainda nao fez o login com o Server. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PACKET_FUNC_SV,
                        1, 0x7000501));
                }

                //// Last 5 Player Game Info   
                ((Player)pd._session).Send(packet_func_sv.pacote10E(((Player)pd._session).m_pi.l5pg));
                return 1;
            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet09C][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
            return 0;
        }

        public static int packet09D(ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(((Player)pd._session).m_pi.channel);

                // Bloquear para ver se funciona o sync do entra depois no camp,
                // mesmo que o outro(0x9D) chama primeiro esse(0x0C) é mais rápido para verificar se o player está em uma sala


                if (c != null)
                {
                    c.requestEnterGameAfterStarted(((Player)pd._session), pd._packet); return 1;
                }

                // Bloquear para ver se funciona o sync do entra depois no camp,
                // mesmo que o outro(0x9D) chama primeiro esse(0x0C) é mais rápido para verificar se o player está em uma sala


            }
            catch (exception e)
            {

                // Bloquear para ver se funciona o sync do entra depois no camp,
                // mesmo que o outro(0x9D) chama primeiro esse(0x0C) é mais rápido para verificar se o player está em uma sala


                _smp.message_pool.push(new message("[packet_func::packet09D][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if ((STDA_ERROR_TYPE)ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) != STDA_ERROR_TYPE.CHANNEL)
                {
                    throw;
                }
                return 0;
            }
            return 0;

        }

        public static int packet09E(ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(((Player)pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestUpdateGachaCoupon(((Player)pd._session), pd._packet); return 1;
                }

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet09E][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if ((STDA_ERROR_TYPE)ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) != STDA_ERROR_TYPE.CHANNEL)
                {
                    throw;
                }
                return 0;
            }
            return 0;

        }

        public static int packet0A1(ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(((Player)pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestEnterWebLinkState(((Player)pd._session), pd._packet); return 1;
                }

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet0A1][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if ((STDA_ERROR_TYPE)ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) != STDA_ERROR_TYPE.CHANNEL)
                {
                    throw;
                }
                return 0;
            }
            return 0;

        }

        public static int packet0A2(ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(((Player)pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestExitedFromWebGuild(((Player)pd._session), pd._packet); return 1;
                }

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet0A2][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if ((STDA_ERROR_TYPE)ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) != STDA_ERROR_TYPE.CHANNEL)
                {
                    throw;
                }
                return 0;
            }
            return 0;

        }

        public static int packet0AA(ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(((Player)pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestUseTicketReport(((Player)pd._session), pd._packet); return 1;
                }

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet0AA][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if ((STDA_ERROR_TYPE)ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) != STDA_ERROR_TYPE.CHANNEL)
                {
                    throw;
                }
                return 0;
            }
            return 0;

        }

        public static int packet0AB(ParamDispatch pd)
        {

            try
            {

                var c = sgs.gs.getInstance().findChannel(((Player)pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestOpenTicketReportScroll(((Player)pd._session), pd._packet); return 1;
                }

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet0AB][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if ((STDA_ERROR_TYPE)ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) != STDA_ERROR_TYPE.CHANNEL)
                {
                    throw;
                }
                return 0;
            }
            return 0;

        }

        public static int packet0AE(ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(((Player)pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestMakeTutorial(((Player)pd._session), pd._packet); return 1;
                }

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet0AE][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if ((STDA_ERROR_TYPE)ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) != STDA_ERROR_TYPE.CHANNEL)
                {
                    throw;
                }
                return 0;
            }
            return 0;

        }

        public static int packet0B2(ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(((Player)pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestOpenBoxMyRoom(((Player)pd._session), pd._packet); return 1;
                }

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet0B2][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if ((STDA_ERROR_TYPE)ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) != STDA_ERROR_TYPE.CHANNEL)
                {
                    throw;
                }
                return 0;
            }
            return 0;

        }

        public static int packet0B4(ParamDispatch pd)
        {



            try
            {

                // Esse pacote é que o player aceitou convite do player entrou na sala saiu e relogou, ai manda esse pacote com o número da sala
                /*_smp::message_pool::push(new message("[packet_func::packet0B4][Log] Player[UID=" + (((Player)pd._session).m_pi.m_uid)
                        + "] mandou o Pacote0B4 mas nao sei o que ele pede ou faz ainda. Hex: \n\r"
                        + hex_util::BufferToHexString(pd._packet.getBuffer(), pd._packet.getSize()), type_msg.CL_FILE_LOG_AND_CONSOLE));*/

                // Verifica se session está varrizada para executar esse ação,
                // se ele não fez o login com o Server ele não pode fazer nada até que ele faça o login
                if (!((Player)pd._session).m_is_authorized)
                {
                    throw new exception("[packet_func::" + "packet0B4" + "][Error] Player[UID=" + Convert.ToString(((Player)pd._session).m_pi.uid) + "] Nao esta varrizado a fazer esse request por que ele ainda nao fez o login com o Server. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PACKET_FUNC_SV,
                        1, 0x7000501));
                }

                byte option = pd._packet.ReadByte();
                ushort numero_sala = pd._packet.ReadUInt16();

                // Log
                _smp.message_pool.push(new message("[packet_func::packet0B4][Log][Option=" + Convert.ToString((ushort)option) + "] Player[UID=" + Convert.ToString(((Player)pd._session).m_pi.uid) + "] foi convidado por um player aceitou o pedido saiu da sala[NUMERO=" + Convert.ToString(numero_sala) + "] e relogou.", type_msg.CL_FILE_LOG_AND_CONSOLE));

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet0B4][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if ((STDA_ERROR_TYPE)ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) != STDA_ERROR_TYPE.CHANNEL)
                {
                    throw;
                }
                return 0;
            }
            return 0;

        }

        public static int packet0B5(ParamDispatch pd)
        {



            try
            {

                // Verifica se session está varrizada para executar esse ação,
                // se ele não fez o login com o Server ele não pode fazer nada até que ele faça o login
                if (!((Player)pd._session).m_is_authorized)
                {
                    throw new exception("[packet_func::" + "packet0B5(MyrRoomHouseInfo)" + "][Error] Player[UID=" + Convert.ToString(((Player)pd._session).m_pi.uid) + "] Nao esta varrizado a fazer esse request por que ele ainda nao fez o login com o Server. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PACKET_FUNC_SV,
                        1, 0x7000501));
                }

                uint from_uid = new uint();
                uint to_uid = new uint();

                from_uid = pd._packet.ReadUInt32();
                to_uid = pd._packet.ReadUInt32();

                var p = new PangyaBinaryWriter();
                p.init_plain((ushort)0x12B);

                // Aqui o player só pode pedir para entrar no dele mesmo
                if (from_uid == to_uid && ((Player)pd._session).m_pi.mrc.allow_enter == 1)
                { // Isso tinha no season 4, agora nos season posteriores tiraram isso
                    p.WriteUInt32(1); // option;

                    p.WriteUInt32(to_uid);

                    p.WriteBuffer(((Player)pd._session).m_pi.mrc, Marshal.SizeOf(new MyRoomConfig()));
                }
                else
                {
                    p.WriteUInt32(0);

                    p.WriteUInt32(to_uid);
                }

                ((Player)pd._session).Send(p);
                return 1;
            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet0B5][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
            return 0;

        }

        public static int packet0B7(ParamDispatch pd)
        {



            try
            {

                // Verifica se session está varrizada para executar esse ação,
                // se ele não fez o login com o Server ele não pode fazer nada até que ele faça o login
                if (!((Player)pd._session).m_is_authorized)
                {
                    throw new exception("[packet_func::" + "packet0B7(InfoPlayerMyRoom)" + "][Error] Player[UID=" + Convert.ToString(((Player)pd._session).m_pi.uid) + "] Nao esta varrizado a fazer esse request por que ele ainda nao fez o login com o Server. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PACKET_FUNC_SV,
                        1, 0x7000501));
                }

                // @@!! Ajeitar para pegar a função estática da sala que initializa o Player Room Info
                PlayerRoomInfoEx pri = new PlayerRoomInfoEx();

                // Player Room Info Init
                pri.oid = ((Player)pd._session).m_oid;
                pri.position = 0; // posição na sala
                pri.capability = ((Player)pd._session).m_pi.m_cap;
                pri.title = ((Player)pd._session).m_pi.ue.skin_typeid[5];

                if (((Player)pd._session).m_pi.ei.char_info != null)
                {
                    pri.char_typeid = ((Player)pd._session).m_pi.ei.char_info._typeid;
                }

                pri.skin[4] = 0; // Aqui tem que ser zero, se for outro valor não mostra a imagem do character equipado

                pri.state_flag.master = 1;
                pri.state_flag.ready = 1; // Sempre está pronto(ready) o master

                pri.state_flag.sexo = ((Player)pd._session).m_pi.mi.sexo;

                // Só faz calculo de Quita rate depois que o player
                // estiver no level Beginner E e jogado 50 games
                if (((Player)pd._session).m_pi.level >= 6 && ((Player)pd._session).m_pi.ui.jogado >= 50)
                {
                    float rate = ((Player)pd._session).m_pi.ui.getQuitRate();

                    if (rate < GOOD_PLAYER_ICON)
                    {
                        pri.state_flag.azinha = 1;
                    }
                    else if (rate >= QUITER_ICON_1 && rate < QUITER_ICON_2)
                    {
                        pri.state_flag.quiter_1 = 1;
                    }
                    else if (rate >= QUITER_ICON_2)
                    {
                        pri.state_flag.quiter_2 = 1;
                    }
                }

                pri.level = (byte)((Player)pd._session).m_pi.mi.level;

                if (((Player)pd._session).m_pi.ei.char_info != null && ((Player)pd._session).m_pi.ui.getQuitRate() < GOOD_PLAYER_ICON)
                {
                    pri.icon_angel = ((Player)pd._session).m_pi.ei.char_info.AngelEquiped();
                }
                else
                {
                    pri.icon_angel = 0;
                }

                pri.place.ulPlace = 10; // 0x0A dec"10" _session.m_pi.place
                pri.guild_uid = ((Player)pd._session).m_pi.gi.uid;
                //pri.guild_mark_index = ((Player)pd._session).m_pi.gi.index_mark_emblem;
                pri.uid = ((Player)pd._session).m_pi.uid;
                pri.state_lounge = ((Player)pd._session).m_pi.state_lounge;
                pri.usUnknown_flg = 0;
                pri.state = ((Player)pd._session).m_pi.state;
                pri.location = new PlayerRoomInfo.stLocation() { x = ((Player)pd._session).m_pi.location.x, z = ((Player)pd._session).m_pi.location.z, r = ((Player)pd._session).m_pi.location.r };
                pri.shop = new PlayerRoomInfo.PersonShop();

                if (((Player)pd._session).m_pi.ei.mascot_info != null)
                {
                    pri.mascot_typeid = ((Player)pd._session).m_pi.ei.mascot_info._typeid;
                }

                pri.flag_item_boost = ((Player)pd._session).m_pi.checkEquipedItemBoost();
                pri.ulUnknown_flg = 0;
                //pri.id_NT não estou usando ainda
                //pri.ucUnknown106
                pri.convidado = 0; // Flag Convidado, [Não sei bem por que os que entra na sala normal tem valor igual aqui, já que é flag de convidado waiting], Valor constante da sala para os players(ACHO)
                pri.avg_score = ((Player)pd._session).m_pi.ui.getMediaScore();
                //pri.ucUnknown3

                if (((Player)pd._session).m_pi.ei.char_info != null)
                {
                    pri.ci = ((Player)pd._session).m_pi.ei.char_info;
                }

                var p = new PangyaBinaryWriter((ushort)0x168); // Character Equipado

                p.WriteBytes(pri.BuildEx());//preciso fazer

                ((Player)pd._session).Send(p);


                p.init_plain(0x12D); // Itens do Myroom, Mala, Email, sofa, teto chao, e poster, "NESSA SEASON, SÓ USA POSTER"

                p.WriteUInt32(1); // Option

                p.WriteUInt16((ushort)((Player)pd._session).m_pi.v_mri.Count);

                for (var i = 0; i < ((Player)pd._session).m_pi.v_mri.Count; ++i)
                {
                    p.WriteStruct(((Player)pd._session).m_pi.v_mri[i], new MyRoomItem());
                }

                ((Player)pd._session).Send(p);

                return 1;//return 1 sucess
            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet0B7][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
                return 0;
            }

            return 0;

        }

        public static int packet0B9(ParamDispatch pd)
        {



            try
            {

                sgs.gs.getInstance().requestUCCSystem(((Player)pd._session), pd._packet); return 1;

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet0B9][ErrorSystem]", type_msg.CL_FILE_LOG_AND_CONSOLE));

                if ((STDA_ERROR_TYPE)ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) != STDA_ERROR_TYPE.GAME_SERVER)
                {
                    throw;
                }
                return 0;
            }
            return 0;

        }

        public static int packet0BA(ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(((Player)pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestInvite(((Player)pd._session), pd._packet); return 1;
                }

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet0BA][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if ((STDA_ERROR_TYPE)ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) != STDA_ERROR_TYPE.CHANNEL)
                {
                    throw;
                }
                return 0;
            }
            return 0;

        }

        public static int packet0BD(ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(((Player)pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestUseCardSpecial(((Player)pd._session), pd._packet); return 1;
                }

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet0BD][ErrorSytem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if ((STDA_ERROR_TYPE)ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) != STDA_ERROR_TYPE.CHANNEL)
                {
                    throw;
                }
                return 0;
            }
            return 0;

        }

        public static int packet0C1(ParamDispatch pd)
        {



            try
            {

                // Verifica se session está varrizada para executar esse ação,
                // se ele não fez o login com o Server ele não pode fazer nada até que ele faça o login
                if (!((Player)pd._session).m_is_authorized)
                {
                    throw new exception("[packet_func::" + "packet0C1(UpdatePlace)" + "][Error] Player[UID=" + Convert.ToString(((Player)pd._session).m_pi.uid) + "] Nao esta varrizado a fazer esse request por que ele ainda nao fez o login com o Server. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PACKET_FUNC_SV,
                        1, 0x7000501));
                }

                ((Player)pd._session).m_pi.place = pd._packet.ReadSByte(); // Att place(lugar)
                _smp.message_pool.push(new message("[packet_func::packet0C1][Log] " + ((Player)pd._session).m_pi.place.ToString(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                // Update Location Player on DB
                ((Player)pd._session).m_pi.updateLocationDB();
                return 1;//sucess
            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet0C1][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
            return 0;

        }

        public static int packet0C9(ParamDispatch pd)
        {



            try
            {

                sgs.gs.getInstance().requestUCCWebKey(((Player)pd._session), pd._packet); return 1;

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet0C9][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if ((STDA_ERROR_TYPE)ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) != STDA_ERROR_TYPE.GAME_SERVER)
                {
                    throw;
                }
                return 0;
            }
            return 0;

        }

        public static int packet0CA(ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(((Player)pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestOpenCardPack(((Player)pd._session), pd._packet); return 1;
                }

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet0CA][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if ((STDA_ERROR_TYPE)ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) != STDA_ERROR_TYPE.CHANNEL)
                {
                    throw;
                }
                return 0;
            }
            return 0;

        }

        public static int packet0CB(ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(((Player)pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestFinishGame(((Player)pd._session), pd._packet); return 1;
                }

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet0CB][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if ((STDA_ERROR_TYPE)ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) != STDA_ERROR_TYPE.CHANNEL)
                {
                    throw;
                }
                return 0;
            }
            return 0;

        }

        public static int packet0CC(ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(((Player)pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestCheckDolfiniLockerPass(((Player)pd._session), pd._packet); return 1;
                }

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet0CC][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if ((STDA_ERROR_TYPE)ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) != STDA_ERROR_TYPE.CHANNEL)
                {
                    throw;
                }
                return 0;
            }
            return 0;

        }

        public static int packet0CD(ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(((Player)pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestDolfiniLockerItem(((Player)pd._session), pd._packet); return 1;
                }

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet0CD][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if ((STDA_ERROR_TYPE)ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) != STDA_ERROR_TYPE.CHANNEL)
                {
                    throw;
                }
                return 0;
            }
            return 0;

        }

        public static int packet0CE(ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(((Player)pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestAddDolfiniLockerItem(((Player)pd._session), pd._packet); return 1;
                }

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet0CE][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if ((STDA_ERROR_TYPE)ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) != STDA_ERROR_TYPE.CHANNEL)
                {
                    throw;
                }
                return 0;
            }
            return 0;

        }

        public static int packet0CF(ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(((Player)pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestRemoveDolfiniLockerItem(((Player)pd._session), pd._packet); return 1;
                }

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet0CF][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if ((STDA_ERROR_TYPE)ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) != STDA_ERROR_TYPE.CHANNEL)
                {
                    throw;
                }
                return 0;
            }
            return 0;

        }

        public static int packet0D0(ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(((Player)pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestMakePassDolfiniLocker(((Player)pd._session), pd._packet); return 1;
                }

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet0D0][Error] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if ((STDA_ERROR_TYPE)ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) != STDA_ERROR_TYPE.CHANNEL)
                {
                    throw;
                }
                return 0;
            }
            return 0;

        }

        public static int packet0D1(ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(((Player)pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestChangeDolfiniLockerPass(((Player)pd._session), pd._packet); return 1;
                }

            }
            catch (exception e)
            {
                _smp.message_pool.push(new message("[packet_func::packet0D1][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if ((STDA_ERROR_TYPE)ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) != STDA_ERROR_TYPE.CHANNEL)
                {
                    throw;
                }
                return 0;
            }
            return 0;

        }

        public static int packet0D2(ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(((Player)pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestChangeDolfiniLockerModeEnter(((Player)pd._session), pd._packet); return 1;
                }

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet0D2][ErroSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if ((STDA_ERROR_TYPE)ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) != STDA_ERROR_TYPE.CHANNEL)
                {
                    throw;
                }
                return 0;
            }
            return 0;

        }

        public static int packet0D3(ParamDispatch pd)
        {
            try
            {

                // Verifica se session está varrizada para executar esse ação,
                // se ele não fez o login com o Server ele não pode fazer nada até que ele faça o login
                if (!((Player)pd._session).m_is_authorized)
                {
                    throw new exception("[packet_func::" + "packet0D3(CheckDolfiniLocker)" + "][Error] Player[UID=" + Convert.ToString(((Player)pd._session).m_pi.uid) + "] Nao esta varrizado a fazer esse request por que ele ainda nao fez o login com o Server. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PACKET_FUNC_SV,
                        1, 0x7000501));
                }

                uint check = 0u;

                check = ((Player)pd._session).m_pi.df.isLocker();

                var p = new PangyaBinaryWriter((ushort)0x170);

                p.WriteUInt32(0); // option
                p.WriteUInt32(check);

                ((Player)pd._session).Send(p);
                return 1;
            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet0D3][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
            return 0;
        }

        public static int packet0D4(ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(((Player)pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestUpdateDolfiniLockerPang(((Player)pd._session), pd._packet); return 1;
                }

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet0D4][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if ((STDA_ERROR_TYPE)ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) != STDA_ERROR_TYPE.CHANNEL)
                {
                    throw;
                }
                return 0;
            }
            return 0;

        }

        public static int packet0D5(ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(((Player)pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestDolfiniLockerPang(((Player)pd._session), pd._packet); return 1;
                }

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet0D5][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if ((STDA_ERROR_TYPE)ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) != STDA_ERROR_TYPE.CHANNEL)
                {
                    throw;
                }
                return 0;
            }
            return 0;

        }

        public static int packet0D8(ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(((Player)pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestUseItemBuff(((Player)pd._session), pd._packet); return 1;
                }

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet0D8][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if ((STDA_ERROR_TYPE)ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) != STDA_ERROR_TYPE.CHANNEL)
                {
                    throw;
                }
                return 0;
            }
            return 0;

        }

        public static int packet0DE(ParamDispatch pd)
        {



            try
            {

                // Player não pode ver a message privada que o player mandou, avisa para o server
                /*_smp::message_pool::push(new message("[packet_func::packet0DE][Log] Player[UID=" + (((Player)pd._session).m_pi.m_uid)
                        + "] mandou o Pacote0DE mas nao sei o que ele pede ou faz ainda. Hex: \n\r"
                        + hex_util::BufferToHexString(pd._packet.getBuffer(), pd._packet.getSize()), type_msg.CL_FILE_LOG_AND_CONSOLE));*/

                // Envia mensagem para o player que enviou o MP que o player não pode ver a mensagem
                sgs.gs.getInstance().requestNotifyNotDisplayPrivateMessageNow(((Player)pd._session), pd._packet); return 1;

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet0DE][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if ((STDA_ERROR_TYPE)ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) != STDA_ERROR_TYPE.GAME_SERVER)
                {
                    throw;
                }
                return 0;
            }
            return 0;

        }

        public static int packet0E5(ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(((Player)pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestActiveCutin(((Player)pd._session), pd._packet); return 1;
                }

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet0E5][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if ((STDA_ERROR_TYPE)ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) != STDA_ERROR_TYPE.CHANNEL)
                {
                    throw;
                }
                return 0;
            }
            return 0;

        }

        public static int packet0E6(ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(((Player)pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestExtendRental(((Player)pd._session), pd._packet); return 1;
                }

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet0E6][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if ((STDA_ERROR_TYPE)ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) != STDA_ERROR_TYPE.CHANNEL)
                {
                    throw;
                }
                return 0;
            }
            return 0;

        }

        public static int packet0E7(ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(((Player)pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestDeleteRental(((Player)pd._session), pd._packet); return 1;
                }

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet0E7][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if ((STDA_ERROR_TYPE)ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) != STDA_ERROR_TYPE.CHANNEL)
                {
                    throw;
                }
                return 0;
            }
            return 0;

        }

        public static int packet0EB(ParamDispatch pd)
        {



            try
            {

                Channel c = sgs.gs.getInstance().findChannel(((Player)pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestPlayerStateCharacterLounge(((Player)pd._session), pd._packet); return 1;
                }

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet0EB][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if ((STDA_ERROR_TYPE)ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) != STDA_ERROR_TYPE.CHANNEL) // Por Hora relança qualquer exception que não seja do channel
                {
                    throw;
                }
                return 0;
            }
            return 0;

        }

        public static int packet0EC(ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(((Player)pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestCometRefill(((Player)pd._session), pd._packet); return 1;
                }

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet0EC][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if ((STDA_ERROR_TYPE)ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) != STDA_ERROR_TYPE.CHANNEL)
                {
                    throw;
                }
                return 0;
            }
            return 0;

        }

        public static int packet0EF(ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(((Player)pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestOpenBoxMail(((Player)pd._session), pd._packet); return 1;
                }

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet0EF][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if ((STDA_ERROR_TYPE)ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) != STDA_ERROR_TYPE.CHANNEL)
                {
                    throw;
                }
                return 0;
            }
            return 0;

        }

        public static int packet0F4(ParamDispatch pd)
        {
            try
            {
                // TTL = Time To Live    

                // Verifica se session está varrizada para executar esse ação,
                // se ele não fez o login com o Server ele não pode fazer nada até que ele faça o login
                if (!((Player)pd._session).m_is_authorized)
                {
                    throw new exception("[packet_func::" + "packet0F4" + "][Error] Player[UID=" + Convert.ToString(((Player)pd._session).m_pi.uid) + "] Nao esta varrizado a fazer esse request por que ele ainda nao fez o login com o Server. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PACKET_FUNC_SV,
                        1, 0x7000501));
                }

                ((Player)pd._session).m_tick_bot = Environment.TickCount;

                //pd._session.m_time_start = std::clock();
                //pd._session.m_tick = std::clock();
                if (((Player)pd._session).m_pi.uid > 0)
                {
                    var cp = ((Player)pd._session).m_pi.cookie;
                    var pang = ((Player)pd._session).m_pi.ui.pang;
                    ((Player)pd._session).m_pi.updateMoeda();
                    // ((Player)pd._session).m_pi.ReloadMemberInfo();
                    using (PangyaBinaryWriter p = new PangyaBinaryWriter())
                    {
                        if (cp != ((Player)pd._session).m_pi.cookie)  //so envia se estiver com valores novos
                        {
                            //// Update ON GAME(cookies)
                            p.init_plain(0x96);
                            p.WriteUInt64(((Player)pd._session).m_pi.cookie);
                            p.WriteUInt32(0);
                            pd._session.Send(p);
                        }
                        if (pang != ((Player)pd._session).m_pi.ui.pang) //so envia se estiver com valores novos
                        {
                            p.Clear();
                            // UPDATE pang ON GAME(pangs)
                            p.init_plain(0xC8);
                            p.WriteUInt64(((Player)pd._session).m_pi.ui.pang);
                            p.WriteUInt32(0);
                            pd._session.Send(p);
                        }
                    }
                }
                return 1;

            }
            catch (exception e)
            {
                _smp.message_pool.push(new message("[packet_func::packet0F4][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
            return 0;
        }

        public static int packet0FB(ParamDispatch pd)
        {
            try
            {

                // Verifica se session está varrizada para executar esse ação,
                // se ele não fez o login com o Server ele não pode fazer nada até que ele faça o login
                if (!((Player)pd._session).m_is_authorized)
                {
                    throw new exception("[packet_func::" + "packet0FB(WebKey)" + "][Error] Player[UID=" + Convert.ToString(((Player)pd._session).m_pi.uid) + "] Nao esta varrizado a fazer esse request por que ele ainda nao fez o login com o Server. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PACKET_FUNC_SV,
                        1, 0x7000501));
                }

                var cmd_gwk = new CmdGeraWebKey(((Player)pd._session).m_pi.uid);

                NormalManagerDB.add(0, cmd_gwk, null, null);


                if (cmd_gwk.getException().getCodeError() != 0)
                    throw cmd_gwk.getException();

                var webKey = cmd_gwk.getKey();

                ((Player)pd._session).Send(packet_func_sv.pacote1AD(webKey, 1));

                return 1;
            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet0FB][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                ((Player)pd._session).Send(packet_func_sv.pacote1AD("", 0));
            }
            return 0;

        }

        public static int packet0FE(ParamDispatch pd)
        {
            //packet 254, no send response!        
            try
            {
                // Verifica se session está varrizada para executar esse ação,
                // se ele não fez o login com o Server ele não pode fazer nada até que ele faça o login     
                CHECK_SESSION_IS_AUTHORIZED((Player)pd._session, "packet0FE");

                var p = new PangyaBinaryWriter(0x1B1);
                ///UCC COMPRESS
                p.WriteInt32(0x0132DC55);
                p.WriteByte(0x19);
                p.WriteZero(6);
                p.WriteInt16(0x2211);
                p.WriteZero(17);
                p.WriteByte(0x11);//@@@@@ aqui diz que esta compresss
                p.WriteInt16(0);
                ((Player)pd._session).Send(p.GetBytes);//tem que ser sem compress, se nao o projectg envia pacotes estranhos!
                return 1;

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet0FE][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }

            return 0;

        }

        public static int packet119(ParamDispatch pd)
        {
            var p = new PangyaBinaryWriter();

            try
            {

                sgs.gs.getInstance().requestChangeServer(((Player)pd._session), pd._packet); return 1;

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet119][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
            return 0;

        }

        public static int packet126(ParamDispatch pd)
        {
            try
            {

                var c = sgs.gs.getInstance().findChannel(((Player)pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestOpenLegacyTikiShop(((Player)pd._session), pd._packet); return 1;
                }

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet126][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if ((STDA_ERROR_TYPE)ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) != STDA_ERROR_TYPE.CHANNEL)
                {
                    throw; // Relança
                }
            }
            return 0;
        }

        public static int packet127(ParamDispatch pd)
        {

            try
            {

                var c = sgs.gs.getInstance().findChannel(((Player)pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestPointLegacyTikiShop(((Player)pd._session), pd._packet); return 1;
                }

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet127][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if ((STDA_ERROR_TYPE)ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) != STDA_ERROR_TYPE.CHANNEL)
                {
                    throw; // Relança
                }
            }
            return 0;
        }

        public static int packet128(ParamDispatch pd)
        {
            try
            {

                var c = sgs.gs.getInstance().findChannel(((Player)pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestExchangeTPByItemLegacyTikiShop(((Player)pd._session), pd._packet); return 1;
                }

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet128][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if ((STDA_ERROR_TYPE)ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) != STDA_ERROR_TYPE.CHANNEL)
                {
                    throw; // Relança
                    return 0;
                }
            }
            return 0;
        }

        public static int packet129(ParamDispatch pd)
        {
            try
            {

                var c = sgs.gs.getInstance().findChannel(((Player)pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestExchangeItemByTPLegacyTikiShop(((Player)pd._session), pd._packet); return 1;
                }

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet129][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if ((STDA_ERROR_TYPE)ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) != STDA_ERROR_TYPE.CHANNEL)
                {
                    throw; // Relança
                }
            }
            return 0;
        }

        public static int packet12C(ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(((Player)pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestFinishGame(((Player)pd._session), pd._packet); return 1;
                }

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet12C][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if ((STDA_ERROR_TYPE)ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) != STDA_ERROR_TYPE.CHANNEL)
                {
                    throw;
                }
                return 0;
            }
            return 0;

        }

        public static int packet12D(ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(((Player)pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestReplyInitialValueGrandZodiac(((Player)pd._session), pd._packet); return 1;
                }

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet12D][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if ((STDA_ERROR_TYPE)ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) != STDA_ERROR_TYPE.CHANNEL)
                {
                    throw;
                }
                return 0;
            }
            return 0;

        }

        public static int packet12E(ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(((Player)pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestMarkerOnCourse(((Player)pd._session), pd._packet); return 1;
                }

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet12E][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if ((STDA_ERROR_TYPE)ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) != STDA_ERROR_TYPE.CHANNEL)
                {
                    throw;
                }
                return 0;
            }
            return 0;

        }

        public static int packet12F(ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(((Player)pd._session).m_pi.channel);

#if DEBUG
                _smp.message_pool.push(new message("[packet_func::packet12F][Log] request Player[UID=" + Convert.ToString(((Player)pd._session).m_pi.uid) + "]", type_msg.CL_FILE_LOG_AND_CONSOLE));
#endif // _DEBUG

                if (c != null)
                {
                    c.requestShotEndData(((Player)pd._session), pd._packet); return 1;
                }

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet12F][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if ((STDA_ERROR_TYPE)ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) != STDA_ERROR_TYPE.CHANNEL)
                {
                    throw;
                }
                return 0;
            }
            return 0;

        }

        public static int packet130(ParamDispatch pd)
        {
            var p = new PangyaBinaryWriter();

            try
            {

                var c = sgs.gs.getInstance().findChannel(((Player)pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestLeavePractice(((Player)pd._session), pd._packet); return 1;
                }

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet130][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }

            return 0;
        }

        public static int packet131(ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(((Player)pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestLeaveChipInPractice(((Player)pd._session), pd._packet); return 1;
                }

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet131][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if ((STDA_ERROR_TYPE)ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) != STDA_ERROR_TYPE.CHANNEL)
                {
                    throw;
                }
                return 0;
            }
            return 0;

        }

        public static int packet137(ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(((Player)pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestStartFirstHoleGrandZodiac(((Player)pd._session), pd._packet); return 1;
                }

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet137][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if ((STDA_ERROR_TYPE)ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) != STDA_ERROR_TYPE.CHANNEL)
                {
                    throw;
                }
                return 0;
            }
            return 0;

        }

        public static int packet138(ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(((Player)pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestActiveWing(((Player)pd._session), pd._packet); return 1;
                }

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet138][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if ((STDA_ERROR_TYPE)ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) != STDA_ERROR_TYPE.CHANNEL)
                {
                    throw;
                }
                return 0;
            }
            return 0;

        }

        public static int packet140(ParamDispatch pd)
        {
            try
            {

                // Verifica se session está varrizada para executar esse ação,
                // se ele não fez o login com o Server ele não pode fazer nada até que ele faça o login
                CHECK_SESSION_IS_AUTHORIZED((Player)pd._session, "packet140(requestEnterShop)");

                var c = sgs.gs.getInstance().findChannel(((Player)pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestEnterShop(((Player)pd._session), pd._packet); return 1;
                }
            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet140][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
            return 0;
        }

        public static int packet141(ParamDispatch pd)
        {
            try
            {

                var c = sgs.gs.getInstance().findChannel(((Player)pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestChangeWindNextHoleRepeat(((Player)pd._session), pd._packet); return 1;
                }

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet141][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if ((STDA_ERROR_TYPE)ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) != STDA_ERROR_TYPE.CHANNEL)
                {
                    throw;
                }
                return 0;
            }
            return 0;

        }

        public static int packet143(ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(((Player)pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestOpenMailBox(((Player)pd._session), pd._packet); return 1;
                }

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet143][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if ((STDA_ERROR_TYPE)ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) != STDA_ERROR_TYPE.CHANNEL)
                {
                    throw;
                }
                return 0;
            }
            return 0;

        }

        public static int packet144(ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(((Player)pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestInfoMail(((Player)pd._session), pd._packet); return 1;
                }

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet144][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if ((STDA_ERROR_TYPE)ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) != STDA_ERROR_TYPE.CHANNEL)
                {
                    throw;
                }
                return 0;
            }
            return 0;

        }

        public static int packet145(ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(((Player)pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestSendMail(((Player)pd._session), pd._packet); return 1;
                }

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet145][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if ((STDA_ERROR_TYPE)ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) != STDA_ERROR_TYPE.CHANNEL)
                {
                    throw;
                }
                return 0;
            }
            return 0;

        }

        public static int packet146(ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(((Player)pd._session).m_pi.channel);

#if DEBUG
                _smp.message_pool.push(new message("[packet_func::packet146][Log] Request player[UID=" + Convert.ToString(((Player)pd._session).m_pi.uid) + "]", type_msg.CL_FILE_LOG_AND_CONSOLE));
#endif // _DEBUG

                if (c != null)
                {
                    c.requestTakeItemFomMail(((Player)pd._session), pd._packet); return 1;
                }

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet146][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if ((STDA_ERROR_TYPE)ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) == STDA_ERROR_TYPE.CHANNEL)
                {
                    throw;
                }
                return 0;
            }
            return 0;

        }

        public static int packet147(ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(((Player)pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestDeleteMail(((Player)pd._session), pd._packet); return 1;
                }

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet147][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if ((STDA_ERROR_TYPE)ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) != STDA_ERROR_TYPE.CHANNEL)
                {
                    throw;
                }
                return 0;
            }
            return 0;

        }

        public static int packet14B(ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(((Player)pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestPlayPapelShop(((Player)pd._session), pd._packet); return 1;
                }

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet14B][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if ((STDA_ERROR_TYPE)ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) != STDA_ERROR_TYPE.CHANNEL)
                {
                    throw;
                }
                return 0;
            }
            return 0;

        }

        public static int packet151(ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(((Player)pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestDailyQuest(((Player)pd._session), pd._packet); return 1;
                }

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet151][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if ((STDA_ERROR_TYPE)ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) != STDA_ERROR_TYPE.CHANNEL)
                {
                    throw;
                }
                return 0;
            }
            return 0;

        }

        public static int packet152(ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(((Player)pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestAcceptDailyQuest(((Player)pd._session), pd._packet); return 1;
                }

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet152][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if ((STDA_ERROR_TYPE)ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) != STDA_ERROR_TYPE.CHANNEL)
                {
                    throw;
                }
                return 0;
            }
            return 0;

        }

        public static int packet153(ParamDispatch pd)
        {
            try
            {

                var c = sgs.gs.getInstance().findChannel(((Player)pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestTakeRewardDailyQuest(((Player)pd._session), pd._packet); return 1;
                }

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet153][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if ((STDA_ERROR_TYPE)ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) != STDA_ERROR_TYPE.CHANNEL)
                {
                    throw;
                }
                return 0;
            }
            return 0;

        }

        public static int packet154(ParamDispatch pd)
        {
            try
            {

                var c = sgs.gs.getInstance().findChannel(((Player)pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestLeaveDailyQuest(((Player)pd._session), pd._packet); return 1;
                }

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet154][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if ((STDA_ERROR_TYPE)ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) != STDA_ERROR_TYPE.CHANNEL)
                {
                    throw;
                }
                return 0;
            }
            return 0;

        }

        public static int packet155(ParamDispatch pd)
        {
            try
            {

                var c = sgs.gs.getInstance().findChannel(((Player)pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestLoloCardCompose(((Player)pd._session), pd._packet); return 1;
                }

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet155][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if ((STDA_ERROR_TYPE)ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) != STDA_ERROR_TYPE.CHANNEL)
                {
                    throw;
                }
                return 0;
            }
            return 0;
        }

        public static int packet156(ParamDispatch pd)
        {
            try
            {

                var c = sgs.gs.getInstance().findChannel(((Player)pd._session).m_pi.channel);

                if (c != null)
                {
                    //  c.requestActiveAutoCommand(((Player)pd._session), pd._packet);                 return 1;
                }

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet156][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if ((STDA_ERROR_TYPE)ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) != STDA_ERROR_TYPE.CHANNEL)
                {
                    throw;
                }
                return 0;
            }
            return 0;

        }

        public static int packet157(ParamDispatch pd)
        {
            var p = new PangyaBinaryWriter();

            try
            {

                uint uid = pd._packet.ReadUInt32();

                // Log
#if DEBUG
                _smp.message_pool.push(new message("[packet_func::packet157][Log] Player[UID=" + Convert.ToString(((Player)pd._session).m_pi.uid) + "].\tPlayer Achievement request uid " + Convert.ToString(uid), type_msg.CL_FILE_LOG_AND_CONSOLE));
#else
					_smp.message_pool.push(new message("[packet_func::packet157][Log] Player[UID=" + Convert.ToString(((Player)pd._session).m_pi.uid) + "].\tPlayer Achievement request uid: " + Convert.ToString(uid), CL_ONLY_FILE_LOG));
#endif // _DEBUG

                //// Verifica se session está varrizada para executar esse ação,
                //// se ele não fez o login com o Server ele não pode fazer nada até que ele faça o login
                //if (!((Player)pd._session).m_is_authorized)
                //{
                //    throw new exception("[packet_func::" + "packet157(requestAchievementInfo)" + "][Error] Player[UID=" + Convert.ToString(((Player)pd._session).m_pi.m_uid) + "] Nao esta varrizado a fazer esse request por que ele ainda nao fez o login com o Server. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PACKET_FUNC_SV,
                //        1, 0x7000501));
                //}

                //MgrAchievement mgr_achievement = null;
                //player s = null;

                //if (((Player)pd._session).m_pi.m_uid == m_uid) // O player solicitou o próprio achievement info
                //{
                //    mgr_achievement = ((Player)pd._session).m_pi.mgr_achievement;
                //}
                //else if ((s = sgs.gs.getInstance().findPlayer(m_uid)) != null) // O player solicitou o achievement info de outro player online
                //{
                //    mgr_achievement = s.m_pi.mgr_achievement;
                //}
                //else
                //{ // O player solicitou o achievement info de outro player off-line

                //    MgrAchievement mgr_achievement = new MgrAchievement();

                //    mgr_achievement.initAchievement(new uint(m_uid));

                //    mgr_achievement.sendAchievementGuiToPlayer(((Player)pd._session));

                //    
                //}

                //if (mgr_achievement == null)
                //{
                //    pacote22C(p,
                //        ((Player)pd._session), 1);
                //    ((Player)pd._session).Send(p, // Error
                //        ((Player)pd._session), 1);
                //}
                //else
                //{
                //    mgr_achievement.sendAchievementGuiToPlayer(((Player)pd._session));
                //}
                return 1;
            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet157][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                //pacote22C(p,
                //    ((Player)pd._session), 1);
                //((Player)pd._session).Send(p, // Error
                //    ((Player)pd._session), 1);
            }
            return 0;
        }

        public static int packet158(ParamDispatch pd)
        {
            try
            {

                var c = sgs.gs.getInstance().findChannel(((Player)pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestCadieCauldronExchange(((Player)pd._session), pd._packet); return 1;
                }

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet158][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if ((STDA_ERROR_TYPE)ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) != STDA_ERROR_TYPE.CHANNEL)
                {
                    throw;
                }
                return 0;
            }
            return 0;

        }

        public static int packet15C(ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(((Player)pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestActivePaws(((Player)pd._session), pd._packet); return 1;
                }

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet15C][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if ((STDA_ERROR_TYPE)ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) != STDA_ERROR_TYPE.CHANNEL)
                {
                    throw;
                }
                return 0;
            }
            return 0;

        }

        public static int packet15D(ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(((Player)pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestActiveRing(((Player)pd._session), pd._packet); return 1;
                }

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet15D][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if ((STDA_ERROR_TYPE)ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) != STDA_ERROR_TYPE.CHANNEL)
                {
                    throw;
                }
                return 0;
            }
            return 0;

        }

        public static int packet164(ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(((Player)pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestClubSetWorkShopUpLevel(((Player)pd._session), pd._packet); return 1;
                }

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet164][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if ((STDA_ERROR_TYPE)ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) != STDA_ERROR_TYPE.CHANNEL)
                {
                    throw;
                }
                return 0;
            }
            return 0;

        }

        public static int packet165(ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(((Player)pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestClubSetWorkShopUpLevelConfirm(((Player)pd._session), pd._packet); return 1;
                }

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet165][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if ((STDA_ERROR_TYPE)ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) != STDA_ERROR_TYPE.CHANNEL)
                {
                    throw;
                }
                return 0;
            }
            return 0;

        }

        public static int packet166(ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(((Player)pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestClubSetWorkShopUpLevelCancel(((Player)pd._session), pd._packet); return 1;
                }

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet166][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if ((STDA_ERROR_TYPE)ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) != STDA_ERROR_TYPE.CHANNEL)
                {
                    throw;
                }
                return 0;
            }
            return 0;

        }

        public static int packet167(ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(((Player)pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestClubSetWorkShopUpRank(((Player)pd._session), pd._packet); return 1;
                }

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet167][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if ((STDA_ERROR_TYPE)ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) != STDA_ERROR_TYPE.CHANNEL)
                {
                    throw;
                }
                return 0;
            }
            return 0;

        }

        public static int packet168(ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(((Player)pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestClubSetWorkShopUpRankTransformConfirm(((Player)pd._session), pd._packet); return 1;
                }

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet168][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if ((STDA_ERROR_TYPE)ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) != STDA_ERROR_TYPE.CHANNEL)
                {
                    throw;
                }
                return 0;
            }
            return 0;

        }

        public static int packet169(ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(((Player)pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestClubSetWorkShopUpRankTransformCancel(((Player)pd._session), pd._packet); return 1;
                }

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet169][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if ((STDA_ERROR_TYPE)ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) != STDA_ERROR_TYPE.CHANNEL)
                {
                    throw;
                }
                return 0;
            }
            return 0;

        }

        public static int packet16B(ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(((Player)pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestClubSetWorkShopRecoveryPts(((Player)pd._session), pd._packet); return 1;
                }

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet16B][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if ((STDA_ERROR_TYPE)ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) != STDA_ERROR_TYPE.CHANNEL)
                {
                    throw;
                }
                return 0;
            }
            return 0;

        }

        public static int packet16C(ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(((Player)pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestClubSetWorkShopTransferMasteryPts(((Player)pd._session), pd._packet); return 1;
                }

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet16C][ErrorSystem] ", type_msg.CL_FILE_LOG_AND_CONSOLE));

                if ((STDA_ERROR_TYPE)ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) != STDA_ERROR_TYPE.CHANNEL)
                {
                    throw;
                }
                return 0;
            }
            return 0;

        }

        public static int packet16D(ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(((Player)pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestClubSetReset(((Player)pd._session), pd._packet); return 1;
                }

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet16D][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if ((STDA_ERROR_TYPE)ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) != STDA_ERROR_TYPE.CHANNEL)
                {
                    throw;
                }
                return 0;
            }
            return 0;

        }

        public static int packet16E(ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(((Player)pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestCheckAttendanceReward(((Player)pd._session), pd._packet); return 1;
                }

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet16E][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if ((STDA_ERROR_TYPE)ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) != STDA_ERROR_TYPE.CHANNEL)
                {
                    throw;
                }
                return 0;
            }
            return 0;

        }

        public static int packet16F(ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(((Player)pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestAttendanceRewardLoginCount(((Player)pd._session), pd._packet); return 1;
                }

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet16F][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if ((STDA_ERROR_TYPE)ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) != STDA_ERROR_TYPE.CHANNEL)
                {
                    throw;
                }
                return 0;
            }
            return 0;

        }

        public static int packet171(ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(((Player)pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestActiveEarcuff(((Player)pd._session), pd._packet); return 1;
                }

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet171][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if ((STDA_ERROR_TYPE)ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) != STDA_ERROR_TYPE.CHANNEL)
                {
                    throw;
                }
                return 0;
            }
            return 0;

        }

        public static int packet172(ParamDispatch pd)
        {



            try
            {
                Player _session = (Player)pd._session;
                // !@ Log
                message_pool.push(new message("[packet_func::packet172][Log] Player[UID=" + Convert.ToString(((Player)pd._session).m_pi.uid) + "] request open Event Workshop 2013.", type_msg.CL_FILE_LOG_AND_CONSOLE));
               
			   return 1;
            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet172][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
            return 0;
        }

        public static int packet176(ParamDispatch pd)
        {
            try
            {

                var c = sgs.gs.getInstance().findChannel(((Player)pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestEnterLobbyGrandPrix(((Player)pd._session), pd._packet); return 1;
                }

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet176][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if ((STDA_ERROR_TYPE)ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) != STDA_ERROR_TYPE.CHANNEL)
                {
                    throw;
                }
                return 0;
            }
            return 0;

        }

        public static int packet177(ParamDispatch pd)
        {
            try
            {

                var c = sgs.gs.getInstance().findChannel(((Player)pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestExitLobbyGrandPrix(((Player)pd._session), pd._packet); return 1;
                }

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet177][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if ((STDA_ERROR_TYPE)ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) != STDA_ERROR_TYPE.CHANNEL)
                {
                    throw;
                }
                return 0;
            }
            return 0;

        }

        public static int packet179(ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(((Player)pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestEnterRoomGrandPrix(((Player)pd._session), pd._packet); return 1;
                }

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet179][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if ((STDA_ERROR_TYPE)ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) != STDA_ERROR_TYPE.CHANNEL)
                {
                    throw;
                }
                return 0;
            }
            return 0;

        }

        public static int packet17A(ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(((Player)pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestExitRoomGrandPrix(((Player)pd._session), pd._packet); return 1;
                }

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet17A][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if ((STDA_ERROR_TYPE)ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) != STDA_ERROR_TYPE.CHANNEL)
                {
                    throw;
                }
                return 0;
            }
            return 0;

        }

        public static int packet17F(ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(((Player)pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestPlayMemorial(((Player)pd._session), pd._packet); return 1;
                }

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet17F][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if ((STDA_ERROR_TYPE)ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) != STDA_ERROR_TYPE.CHANNEL)
                {
                    throw;
                }
                return 0;
            }
            return 0;

        }

        public static int packet180(ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(((Player)pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestActiveGlove(((Player)pd._session), pd._packet); return 1;
                }

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet180][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if ((STDA_ERROR_TYPE)ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) != STDA_ERROR_TYPE.CHANNEL)
                {
                    throw;
                }
                return 0;
            }
            return 0;

        }

        public static int packet181(ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(((Player)pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestActiveRingGround(((Player)pd._session), pd._packet); return 1;
                }

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet181][ErroSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if ((STDA_ERROR_TYPE)ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) != STDA_ERROR_TYPE.CHANNEL)
                {
                    throw;
                }
                return 0;
            }
            return 0;

        }

        public static int packet184(ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(((Player)pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestToggleAssist(((Player)pd._session), pd._packet); return 1;
                }

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet184][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if ((STDA_ERROR_TYPE)ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) != STDA_ERROR_TYPE.CHANNEL)
                {
                    throw;
                }
                return 0;
            }
            return 0;

        }

        public static int packet185(ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(((Player)pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestActiveAssistGreen(((Player)pd._session), pd._packet); return 1;
                }

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet185][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if ((STDA_ERROR_TYPE)ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) != STDA_ERROR_TYPE.CHANNEL)
                {
                    throw;
                }
                return 0;
            }
            return 0;

        }

        public static int packet187(ParamDispatch pd)
        {
            try
            {

                var c = sgs.gs.getInstance().findChannel(((Player)pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestCharacterMasteryExpand(((Player)pd._session), pd._packet); return 1;
                }

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet187][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));


            }
            return 0;

        }

        public static int packet188(ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(((Player)pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestCharacterStatsUp(((Player)pd._session), pd._packet); return 1;
                }

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet188][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if ((STDA_ERROR_TYPE)ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) != STDA_ERROR_TYPE.CHANNEL)
                {
                    throw;
                }
                return 0;
            }
            return 0;

        }

        public static int packet189(ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(((Player)pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestCharacterStatsDown(((Player)pd._session), pd._packet); return 1;
                }

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet189][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if ((STDA_ERROR_TYPE)ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) != STDA_ERROR_TYPE.CHANNEL)
                {
                    throw;
                }
                return 0;
            }
            return 0;

        }

        public static int packet18A(ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(((Player)pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestCharacterCardEquip(((Player)pd._session), pd._packet); return 1;
                }

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet18A][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if ((STDA_ERROR_TYPE)ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) != STDA_ERROR_TYPE.CHANNEL)
                {
                    throw;
                }
                return 0;
            }
            return 0;

        }

        public static int packet18B(ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(((Player)pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestCharacterCardEquipWithPatcher(((Player)pd._session), pd._packet); return 1;
                }

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet18B][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if ((STDA_ERROR_TYPE)ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) != STDA_ERROR_TYPE.CHANNEL)
                {
                    throw;
                }
                return 0;
            }
            return 0;

        }

        public static int packet18C(ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(((Player)pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestCharacterRemoveCard(((Player)pd._session), pd._packet); return 1;
                }

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet18C][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if ((STDA_ERROR_TYPE)ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) != STDA_ERROR_TYPE.CHANNEL)
                {
                    throw;
                }
                return 0;
            }
            return 0;

        }

        public static int packet18D(ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(((Player)pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestTikiShopExchangeItem(((Player)pd._session), pd._packet); return 1;
                }

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet18D][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if ((STDA_ERROR_TYPE)ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) != STDA_ERROR_TYPE.CHANNEL)
                {
                    throw;
                }
                return 0;
            }
            return 0;

        }

        public static int packet192(ParamDispatch pd)
        {
            try
            {

                // !@ Log
                _smp.message_pool.push(new message("[packet_func::packet192][Log] Player[UID=" + Convert.ToString(((Player)pd._session).m_pi.uid) + "] request open Event Arin 2014.", type_msg.CL_FILE_LOG_AND_CONSOLE));

                _smp.message_pool.push(new message("[packet_func::packet192][Log] Player[UID=" + Convert.ToString(((Player)pd._session).m_pi.uid) + "] " + pd._packet.Log(), type_msg.CL_FILE_LOG_AND_CONSOLE));

			   return 1;
            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet192][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }

            return 0;
        }

        public static int packet196(ParamDispatch pd)
        {
            try
            {

                var c = sgs.gs.getInstance().findChannel(((Player)pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestActiveRingPawsRainbowJP(((Player)pd._session), pd._packet); return 1;
                }

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet196][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if ((STDA_ERROR_TYPE)ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) != STDA_ERROR_TYPE.CHANNEL)
                {
                    throw;
                }
                return 0;
            }
            return 0;

        }

        public static int packet197(ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(((Player)pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestActiveRingPowerGagueJP(((Player)pd._session), pd._packet); return 1;
                }

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet197][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if ((STDA_ERROR_TYPE)ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) != STDA_ERROR_TYPE.CHANNEL)
                {
                    throw;
                }
                return 0;
            }
            return 0;

        }

        public static int packet198(ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(((Player)pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestActiveRingMiracleSignJP(((Player)pd._session), pd._packet); return 1;
                }

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet198][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if ((STDA_ERROR_TYPE)ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) != STDA_ERROR_TYPE.CHANNEL)
                {
                    throw;
                }
                return 0;
            }
            return 0;

        }

        public static int packet199(ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(((Player)pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestActiveRingPawsRingSetJP(((Player)pd._session), pd._packet); return 1;
                }

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet199][ErrorSystem]", type_msg.CL_FILE_LOG_AND_CONSOLE));

                if ((STDA_ERROR_TYPE)ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) != STDA_ERROR_TYPE.CHANNEL)
                {
                    throw;
                }
                return 0;
            }
            return 0;

        }



        public static int packet_sv055(ParamDispatch pd)
        {
            try
            {

                var c = sgs.gs.getInstance().findChannel(((Player)pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestInitShotSended(((Player)pd._session), pd._packet); return 1;
                }

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet_sv055][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if ((STDA_ERROR_TYPE)ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) != STDA_ERROR_TYPE.CHANNEL)
                {
                    throw;
                }
                return 0;
            }
            return 0;

        }


        private static void CHECK_SESSION_IS_AUTHORIZED(Player _session, string method)
        {
            if (!_session.m_is_authorized)
                throw new exception("[packet_func::" + ((method)) + "][Error] Player[UID=" + (_session.m_pi.uid) + "] Nao esta autorizado a fazer esse request por que ele ainda nao fez o login com o Server. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PACKET_FUNC_SV, 1, 0x7000501));
        }
    }
}
