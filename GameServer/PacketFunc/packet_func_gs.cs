using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using Pangya_GameServer.Cmd;
using Pangya_GameServer.Game;
using Pangya_GameServer.Game.Manager;
using Pangya_GameServer.Game.System;
using Pangya_GameServer.Game.Utils;
using Pangya_GameServer.GameType;
using Pangya_GameServer.PangyaEnums;
using Pangya_GameServer.Session;
using PangyaAPI.Network.Cmd;
using PangyaAPI.Network.Pangya_St;
using PangyaAPI.Network.PangyaPacket;
using PangyaAPI.Network.PangyaSession;
using PangyaAPI.SQL;
using PangyaAPI.SQL.Manager;
using PangyaAPI.Utilities;
using PangyaAPI.Utilities.BinaryModels;
using PangyaAPI.Utilities.Log;
using static Pangya_GameServer.GameType._Define;
using _smp = PangyaAPI.Utilities.Log;
namespace Pangya_GameServer.PacketFunc
{
    /// <summary>
    /// somente as requisicoes feitas pelo cliente
    /// </summary>
    public class packet_func : packet_func_base
    {
        public static int packet_svFazNada(object param, ParamDispatch pd)
        {
            if (pd._packet.Id == (byte)PacketIDServer.SERVER_ROOMUSERLIST_0x48)
            {
                //var ps = pd._packet.getBuffer();
                //Console.WriteLine("packet_svFazNada:"+ .HexDump());
            }
            return 0;
        }

        public static int packet_sv4D(object param, ParamDispatch pd)
        {
            return 0;
        }

        public static int packet_svRequestInfo(object param, ParamDispatch pd)
        {
            return 0;
        }

        public static int packet_as001(object param, ParamDispatch pd)
        {
            return 0;
        }


        public static int packet002(object param, ParamDispatch _arg1)
        {
            try
            {
                sgs.gs.getInstance().requestLogin((Player)_arg1._session, _arg1._packet);

            }
            catch (exception ex)
            {
                Console.WriteLine($"[packet_func_cl::packet002][StError]: {ex.getFullMessageError()}");
                _arg1._session.Disconnect();//chama a desconexao
                return 0;
            }
            return 0;
        }

        public static int packet003(object param, ParamDispatch pd)
        {
            try
            {

                sgs.gs.getInstance().requestChat((Player)pd._session, pd._packet);

            }
            catch (exception ex)
            {
                Console.WriteLine($"[packet_func_cl::packet003][StError]:  {ex.getFullMessageError()}");
            }
            return 0;
        }

        public static int packet004(object param, ParamDispatch pd)
        {
            try
            {
                // Enter Channel, channel ID
                sgs.gs.getInstance().requestEnterChannel((Player)pd._session, pd._packet);
            }
            catch (exception ex)
            {
                Console.WriteLine($"[packet_func_cl::packet004][StError]:  {ex.getFullMessageError()}");
            }
            return 0;
        }

        public static int packet006(object param, ParamDispatch pd)
        {
            try
            {

                var c = sgs.gs.getInstance().findChannel(Tools.reinterpret_cast<Player>(pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestFinishGame(Tools.reinterpret_cast<Player>(pd._session), pd._packet);
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
            return 0;

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


        public static int packet007(object param, ParamDispatch pd)
        {


            return 0;
        }


        public static int packet008(object param, ParamDispatch pd)
        {
            message_pool.push(new message("[packet_func::packet008]\n\rHex Dump.\n\r" + pd._packet.Log(), type_msg.CL_FILE_LOG_AND_CONSOLE));

            try
            {

                Channel _channel = sgs.gs.getInstance().findChannel(Tools.reinterpret_cast<Player>(pd._session).m_pi.channel);

                if (_channel != null)
                {
                    _channel.requestMakeRoom(Tools.reinterpret_cast<Player>(pd._session), pd._packet);
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
            return 0;
        }

        public static int packet009(object param, ParamDispatch pd)
        {
            try
            {

                Channel _channel = sgs.gs.getInstance().findChannel(Tools.reinterpret_cast<Player>(pd._session).m_pi.channel);

                if (_channel != null)
                {
                    _channel.requestEnterRoom(Tools.reinterpret_cast<Player>(pd._session), pd._packet);
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
            return 0;
        }

        public static int packet00A(object param, ParamDispatch pd)
        {
            try
            {

                Channel _channel = sgs.gs.getInstance().findChannel(Tools.reinterpret_cast<Player>(pd._session).m_pi.channel);

                if (_channel != null)
                {
                    _channel.requestChangeInfoRoom(Tools.reinterpret_cast<Player>(pd._session), pd._packet);
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

        public static int packet00B(object param, ParamDispatch pd)
        {



            try
            {

                Channel _channel = sgs.gs.getInstance().findChannel(Tools.reinterpret_cast<Player>(pd._session).m_pi.channel);

                if (_channel != null)
                {
                    _channel.requestChangePlayerItemChannel(Tools.reinterpret_cast<Player>(pd._session), pd._packet);
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

        public static int packet00C(object param, ParamDispatch pd)
        {
            try
            {

                Channel _channel = sgs.gs.getInstance().findChannel(Tools.reinterpret_cast<Player>(pd._session).m_pi.channel);

                // Bloquear para ver se funciona o sync do entra depois no camp,
                // mesmo que o outro(0x9D) chama primeiro esse(0x0C) é mais rápido para verificar se o player está em uma sala
                //
                pd._session.lockSync();

                if (_channel != null)
                {
                    _channel.requestChangePlayerItemRoom(Tools.reinterpret_cast<Player>(pd._session), pd._packet);
                }

                // Bloquear para ver se funciona o sync do entra depois no camp,
                // mesmo que o outro(0x9D) chama primeiro esse(0x0C) é mais rápido para verificar se o player está em uma sala
                //
                pd._session.unlockSync();
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
            return 0;
        }

        public static int packet00D(object param, ParamDispatch pd)
        {
            try
            {

                Channel c = sgs.gs.getInstance().findChannel(Tools.reinterpret_cast<Player>(pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestChangePlayerStateReadyRoom(Tools.reinterpret_cast<Player>(pd._session), pd._packet);
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

        public static int packet00E(object param, ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(Tools.reinterpret_cast<Player>(pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestStartGame(Tools.reinterpret_cast<Player>(pd._session), pd._packet);
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

        public static int packet00F(object param, ParamDispatch pd)
        {



            try
            {

                Channel c = sgs.gs.getInstance().findChannel(Tools.reinterpret_cast<Player>(pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestExitRoom(Tools.reinterpret_cast<Player>(pd._session), pd._packet);
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

        public static int packet010(object param, ParamDispatch pd)
        {



            try
            {

                Channel c = sgs.gs.getInstance().findChannel(Tools.reinterpret_cast<Player>(pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestChangePlayerTeamRoom(Tools.reinterpret_cast<Player>(pd._session), pd._packet);
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

        public static int packet011(object param, ParamDispatch pd)
        { 
            try
            {

                var c = sgs.gs.getInstance().findChannel(Tools.reinterpret_cast<Player>(pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestFinishLoadHole(Tools.reinterpret_cast<Player>(pd._session), pd._packet);
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

        public static int packet012(object param, ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(Tools.reinterpret_cast<Player>(pd._session).m_pi.channel);

#if DEBUG
                _smp.message_pool.push(new message("[packet_func::packet12][Log] request Player[UID=" + Convert.ToString(Tools.reinterpret_cast<Player>(pd._session).m_pi.uid) + "]", type_msg.CL_FILE_LOG_AND_CONSOLE));
#endif // _DEBUG

                if (c != null)
                {
                    c.requestInitShot(Tools.reinterpret_cast<Player>(pd._session), pd._packet);
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

        public static int packet013(object param, ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(Tools.reinterpret_cast<Player>(pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestChangeMira(Tools.reinterpret_cast<Player>(pd._session), pd._packet);
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

        public static int packet014(object param, ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(Tools.reinterpret_cast<Player>(pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestChangeStateBarSpace(Tools.reinterpret_cast<Player>(pd._session), pd._packet);
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

        public static int packet015(object param, ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(Tools.reinterpret_cast<Player>(pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestActivePowerShot(Tools.reinterpret_cast<Player>(pd._session), pd._packet);
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
            return 0;
        }

        public static int packet016(object param, ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(Tools.reinterpret_cast<Player>(pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestChangeClub(Tools.reinterpret_cast<Player>(pd._session), pd._packet);
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

        }

        public static int packet017(object param, ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(Tools.reinterpret_cast<Player>(pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestUseActiveItem(Tools.reinterpret_cast<Player>(pd._session), pd._packet);
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

        public static int packet018(object param, ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(Tools.reinterpret_cast<Player>(pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestChangeStateTypeing(Tools.reinterpret_cast<Player>(pd._session), pd._packet);
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

        public static int packet019(object param, ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(Tools.reinterpret_cast<Player>(pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestMoveBall(Tools.reinterpret_cast<Player>(pd._session), pd._packet);
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

        public static int packet01A(object param, ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(Tools.reinterpret_cast<Player>(pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestInitHole(Tools.reinterpret_cast<Player>(pd._session), pd._packet);
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

        public static int packet01B(object param, ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(Tools.reinterpret_cast<Player>(pd._session).m_pi.channel);

#if DEBUG
                _smp.message_pool.push(new message("[packet_func::packet1B][Log] request Player[UID=" + Convert.ToString(Tools.reinterpret_cast<Player>(pd._session).m_pi.uid) + "]", type_msg.CL_FILE_LOG_AND_CONSOLE));
#endif // _DEBUG

                if (c != null)
                {
                    c.requestSyncShot(Tools.reinterpret_cast<Player>(pd._session), pd._packet);
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

        public static int packet01C(object param, ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(Tools.reinterpret_cast<Player>(pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestFinishShot(Tools.reinterpret_cast<Player>(pd._session), pd._packet);
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

        public static int packet01D(object param, ParamDispatch pd)
        {



            try
            {

                Channel c = sgs.gs.getInstance().findChannel(Tools.reinterpret_cast<Player>(pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestBuyItemShop(Tools.reinterpret_cast<Player>(pd._session), pd._packet);
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

        public static int packet01F(object param, ParamDispatch pd)
        {



            try
            {

                Channel c = sgs.gs.getInstance().findChannel(Tools.reinterpret_cast<Player>(pd._session).m_pi.channel);

                if (c != null)
                {
                    // c.requestGiftItemShop(Tools.reinterpret_cast<Player>(pd._session), pd._packet);
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

        public static int packet020(object param, ParamDispatch pd)
        {
            try
            {
                Channel c = sgs.gs.getInstance().findChannel(Tools.reinterpret_cast<Player>(pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestChangePlayerItemMyRoom(Tools.reinterpret_cast<Player>(pd._session), pd._packet);
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

        public static int packet022(object param, ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(Tools.reinterpret_cast<Player>(pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestStartTurnTime(Tools.reinterpret_cast<Player>(pd._session), pd._packet);
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

        public static int packet026(object param, ParamDispatch pd)
        {
            try
            {

                Channel c = sgs.gs.getInstance().findChannel(Tools.reinterpret_cast<Player>(pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestKickPlayerOfRoom(Tools.reinterpret_cast<Player>(pd._session), pd._packet);
                }

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet026][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
                return 0;

            }
            return 0;

        }

        public static int packet029(object param, ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(Tools.reinterpret_cast<Player>(pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestCheckInvite(Tools.reinterpret_cast<Player>(pd._session), pd._packet);
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

        public static int packet02A(object param, ParamDispatch pd)
        {



            try
            {

                sgs.gs.getInstance().requestPrivateMessage(Tools.reinterpret_cast<Player>(pd._session), pd._packet);

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

        public static int packet02D(object param, ParamDispatch pd)
        {
            try
            {

                Channel c = sgs.gs.getInstance().findChannel(Tools.reinterpret_cast<Player>(pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestShowInfoRoom(Tools.reinterpret_cast<Player>(pd._session), pd._packet);
                }

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet02D][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
                return 0;

            }

            return 0;
        }

        public static int packet02F(object param, ParamDispatch pd)
        {



            try
            {

                sgs.gs.getInstance().requestPlayerInfo(Tools.reinterpret_cast<Player>(pd._session), pd._packet);

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

        public static int packet030(object param, ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(Tools.reinterpret_cast<Player>(pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestUnOrPauseGame(Tools.reinterpret_cast<Player>(pd._session), pd._packet);
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

        public static int packet031(object param, ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(Tools.reinterpret_cast<Player>(pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestFinishHoleData(Tools.reinterpret_cast<Player>(pd._session), pd._packet);
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

        public static int packet032(object param, ParamDispatch pd)
        {



            try
            {

                Channel c = sgs.gs.getInstance().findChannel(Tools.reinterpret_cast<Player>(pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestChangePlayerStateAFKRoom(Tools.reinterpret_cast<Player>(pd._session), pd._packet);
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

        public static int packet033(object param, ParamDispatch pd)
        {



            try
            {

                sgs.gs.getInstance().requestExceptionClientMessage(Tools.reinterpret_cast<Player>(pd._session), pd._packet);

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

        public static int packet034(object param, ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(Tools.reinterpret_cast<Player>(pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestFinishCharIntro(Tools.reinterpret_cast<Player>(pd._session), pd._packet);
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

        public static int packet035(object param, ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(Tools.reinterpret_cast<Player>(pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestTeamFinishHole(Tools.reinterpret_cast<Player>(pd._session), pd._packet);
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

        public static int packet036(object param, ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(Tools.reinterpret_cast<Player>(pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestReplyContinueVersus(Tools.reinterpret_cast<Player>(pd._session), pd._packet);
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

        public static int packet037(object param, ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(Tools.reinterpret_cast<Player>(pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestLastPlayerFinishVersus(Tools.reinterpret_cast<Player>(pd._session), pd._packet);
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

        public static int packet039(object param, ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(Tools.reinterpret_cast<Player>(pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestPayCaddieHolyDay(Tools.reinterpret_cast<Player>(pd._session), pd._packet);
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

        public static int packet03A(object param, ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(Tools.reinterpret_cast<Player>(pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestPlayerReportChatGame(Tools.reinterpret_cast<Player>(pd._session), pd._packet);
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

        public static int packet03C(object param, ParamDispatch pd)
        {



            try
            {

                sgs.gs.getInstance().requestTranslateSubPacket(Tools.reinterpret_cast<Player>(pd._session), pd._packet);

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

        public static int packet03D(object param, ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(Tools.reinterpret_cast<Player>(pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestCookie(Tools.reinterpret_cast<Player>(pd._session), pd._packet);
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

        public static int packet03E(object param, ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(Tools.reinterpret_cast<Player>(pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestEnterSpyRoom(Tools.reinterpret_cast<Player>(pd._session), pd._packet);
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

        public static int packet041(object param, ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(Tools.reinterpret_cast<Player>(pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestExecCCGIdentity(Tools.reinterpret_cast<Player>(pd._session), pd._packet);
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

        public static int packet042(object param, ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(Tools.reinterpret_cast<Player>(pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestInitShotArrowSeq(Tools.reinterpret_cast<Player>(pd._session), pd._packet);
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

        public static int packet043(object param, ParamDispatch pd)
        {



            try
            {

                sgs.gs.getInstance().sendServerListAndChannelListToSession(Tools.reinterpret_cast<Player>(pd._session));

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

        public static int packet047(object param, ParamDispatch pd)
        {
            try
            {

                sgs.gs.getInstance().sendRankServer(Tools.reinterpret_cast<Player>(pd._session));

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

        public static int packet048(object param, ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(Tools.reinterpret_cast<Player>(pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestLoadGamePercent(Tools.reinterpret_cast<Player>(pd._session), pd._packet);
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

        public static int packet04A(object param, ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(Tools.reinterpret_cast<Player>(pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestActiveReplay(Tools.reinterpret_cast<Player>(pd._session), pd._packet);
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

        public static int packet04B(object param, ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(Tools.reinterpret_cast<Player>(pd._session).m_pi.channel);

                if (c != null)
                {
                    //  c.requestClubSetStatsUpdate(Tools.reinterpret_cast<Player>(pd._session), pd._packet);
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

        public static int packet04F(object param, ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(Tools.reinterpret_cast<Player>(pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestChangeStateChatBlock(Tools.reinterpret_cast<Player>(pd._session), pd._packet);
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

        public static int packet054(object param, ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(Tools.reinterpret_cast<Player>(pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestChatTeam(Tools.reinterpret_cast<Player>(pd._session), pd._packet);
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

        public static int packet055(object param, ParamDispatch pd)
        {



            try
            {

                sgs.gs.getInstance().requestChangeWhisperState(Tools.reinterpret_cast<Player>(pd._session), pd._packet);

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

        public static int packet057(object param, ParamDispatch pd)
        {



            try
            {

                sgs.gs.getInstance().requestCommandNoticeGM(Tools.reinterpret_cast<Player>(pd._session), pd._packet);

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

        public static int packet05C(object param, ParamDispatch pd)
        {



            try
            {

                sgs.gs.getInstance().sendDateTimeToSession(Tools.reinterpret_cast<Player>(pd._session));

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
        public static int packet060(object param, ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(Tools.reinterpret_cast<Player>(pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestExecCCGDestroy(Tools.reinterpret_cast<Player>(pd._session), pd._packet);
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
        public static int packet061(object param, ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(Tools.reinterpret_cast<Player>(pd._session).m_pi.channel);

                if (c != null)
                {
                    _smp.message_pool.push(new message("[packet_func::packet061][Log] player[UID=" + Convert.ToString(Tools.reinterpret_cast<Player>(pd._session).m_pi.uid) + "] tentou desconectar um player, mas o server ja faz o tratamento do packet08F do comando GM.", type_msg.CL_FILE_LOG_AND_CONSOLE));
                }

                // Verifica se session está varrizada para executar esse ação,
                // se ele não fez o login com o Server ele não pode fazer nada até que ele faça o login
                if (!Tools.reinterpret_cast<Player>(pd._session).m_is_authorized)
                {
                    //throw new exception("[packet_func::" + "packet061" + "][Error] Player[UID=" + Convert.ToString(Tools.reinterpret_cast<Player>(pd._session).m_pi.m_uid) + "] Nao esta autorizado a fazer esse request por que ele ainda nao fez o login com o Server. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PACKET_FUNC_SV,
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

        public static int packet063(object param, ParamDispatch pd)
        {



            try
            {

                Channel c = sgs.gs.getInstance().findChannel(Tools.reinterpret_cast<Player>(pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestPlayerLocationRoom(Tools.reinterpret_cast<Player>(pd._session), pd._packet);
                }

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet063][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));


            }
            return 0;
        }

        public static int packet064(object param, ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(Tools.reinterpret_cast<Player>(pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestDeleteActiveItem(Tools.reinterpret_cast<Player>(pd._session), pd._packet);
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

        public static int packet065(object param, ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(Tools.reinterpret_cast<Player>(pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestActiveBooster(Tools.reinterpret_cast<Player>(pd._session), pd._packet);
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

        public static int packet066(object param, ParamDispatch pd)
        { 
            try
            {

                sgs.gs.getInstance().requestSendTicker(Tools.reinterpret_cast<Player>(pd._session), pd._packet);

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

        public static int packet067(object param, ParamDispatch pd)
        {



            try
            {

                sgs.gs.getInstance().requestQueueTicker(Tools.reinterpret_cast<Player>(pd._session), pd._packet);

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

        public static int packet069(object param, ParamDispatch pd)
        {



            try
            {

                sgs.gs.getInstance().requestChangeChatMacroUser(Tools.reinterpret_cast<Player>(pd._session), pd._packet);

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

        public static int packet06B(object param, ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(Tools.reinterpret_cast<Player>(pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestSetNoticeBeginCaddieHolyDay(Tools.reinterpret_cast<Player>(pd._session), pd._packet);
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

        public static int packet073(object param, ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(Tools.reinterpret_cast<Player>(pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestChangeMascotMessage(Tools.reinterpret_cast<Player>(pd._session), pd._packet);
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

        public static int packet074(object param, ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(Tools.reinterpret_cast<Player>(pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestCancelEditSaleShop(Tools.reinterpret_cast<Player>(pd._session), pd._packet);
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

        public static int packet075(object param, ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(Tools.reinterpret_cast<Player>(pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestCloseSaleShop(Tools.reinterpret_cast<Player>(pd._session), pd._packet);
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

        public static int packet076(object param, ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(Tools.reinterpret_cast<Player>(pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestOpenEditSaleShop(Tools.reinterpret_cast<Player>(pd._session), pd._packet);
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

        public static int packet077(object param, ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(Tools.reinterpret_cast<Player>(pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestViewSaleShop(Tools.reinterpret_cast<Player>(pd._session), pd._packet);
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

        public static int packet078(object param, ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(Tools.reinterpret_cast<Player>(pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestCloseViewSaleShop(Tools.reinterpret_cast<Player>(pd._session), pd._packet);
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

        public static int packet079(object param, ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(Tools.reinterpret_cast<Player>(pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestChangeNameSaleShop(Tools.reinterpret_cast<Player>(pd._session), pd._packet);
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

        public static int packet07A(object param, ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(Tools.reinterpret_cast<Player>(pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestVisitCountSaleShop(Tools.reinterpret_cast<Player>(pd._session), pd._packet);
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

        public static int packet07B(object param, ParamDispatch pd)
        {



            try
            {

                var r = sgs.gs.getInstance().findChannel(Tools.reinterpret_cast<Player>(pd._session).m_pi.channel);

                if (r != null)
                {
                    r.requestPangSaleShop(Tools.reinterpret_cast<Player>(pd._session), pd._packet);
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

        public static int packet07C(object param, ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(Tools.reinterpret_cast<Player>(pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestOpenSaleShop(Tools.reinterpret_cast<Player>(pd._session), pd._packet);
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

        public static int packet07D(object param, ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(Tools.reinterpret_cast<Player>(pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestBuyItemSaleShop(Tools.reinterpret_cast<Player>(pd._session), pd._packet);
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

        public static int packet081(object param, ParamDispatch pd)
        {



            try
            {
                Channel c = sgs.gs.getInstance().findChannel(Tools.reinterpret_cast<Player>(pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestEnterLobby(Tools.reinterpret_cast<Player>(pd._session), pd._packet);
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

        public static int packet082(object param, ParamDispatch pd)
        {
            try
            {
                Channel c = sgs.gs.getInstance().findChannel(Tools.reinterpret_cast<Player>(pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestExitLobby(Tools.reinterpret_cast<Player>(pd._session), pd._packet);
                }

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet082][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

            }

            return 0;
        }

        public static int packet083(object param, ParamDispatch pd)
        {



            try
            {
                sgs.gs.getInstance().requestEnterOtherChannelAndLobby(Tools.reinterpret_cast<Player>(pd._session), pd._packet);

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

        public static int packet088(object param, ParamDispatch pd)
        {



            try
            {

                sgs.gs.getInstance().requestCheckGameGuardAuthAnswer(Tools.reinterpret_cast<Player>(pd._session), pd._packet);

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

        public static int packet08B(object param, ParamDispatch pd)
        {



            try
            {

                // Verifica se session está varrizada para executar esse ação,
                // se ele não fez o login com o Server ele não pode fazer nada até que ele faça o login
                if (!Tools.reinterpret_cast<Player>(pd._session).m_is_authorized)
                {
                    throw new exception("[packet_func::" + "packet08B" + "][Error] Player[UID=" + Convert.ToString(Tools.reinterpret_cast<Player>(pd._session).m_pi.uid) + "] Nao esta autorizado a fazer esse request por que ele ainda nao fez o login com o Server. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PACKET_FUNC_SV,
                        1, 0x7000501));
                }

                CmdServerList cmd_sl = new CmdServerList(TYPE_SERVER.MSN); // waitable

                NormalManagerDB.add(0, cmd_sl, null, null);

                if (cmd_sl.getException().getCodeError() != 0)
                    throw cmd_sl.getException();

                var v_si = cmd_sl.getServerList();

                session_send(pacote0FC(v_si), pd._session);

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet08B][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
            return 0;
        }

        public static int packet08F(object param, ParamDispatch pd)
        {
            try
            {

                sgs.gs.getInstance().requestCommonCmdGM(Tools.reinterpret_cast<Player>(pd._session), pd._packet);

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

        public static int packet098(object param, ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(Tools.reinterpret_cast<Player>(pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestOpenPapelShop(Tools.reinterpret_cast<Player>(pd._session), pd._packet);
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

        public static int packet09C(object param, ParamDispatch pd)
        {



            try
            {

                // Verifica se session está varrizada para executar esse ação,
                // se ele não fez o login com o Server ele não pode fazer nada até que ele faça o login
                if (!Tools.reinterpret_cast<Player>(pd._session).m_is_authorized)
                {
                    throw new exception("[packet_func::" + "packet09C(Last5Player)" + "][Error] Player[UID=" + Convert.ToString(Tools.reinterpret_cast<Player>(pd._session).m_pi.uid) + "] Nao esta autorizado a fazer esse request por que ele ainda nao fez o login com o Server. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PACKET_FUNC_SV,
                        1, 0x7000501));
                }

                //// Last 5 Player Game Info   
                session_send(pacote10E(Tools.reinterpret_cast<Player>(pd._session).m_pi.l5pg), pd._session);

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet09C][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
            return 0;
        }

        public static int packet09D(object param, ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(Tools.reinterpret_cast<Player>(pd._session).m_pi.channel);

                // Bloquear para ver se funciona o sync do entra depois no camp,
                // mesmo que o outro(0x9D) chama primeiro esse(0x0C) é mais rápido para verificar se o player está em uma sala
                pd._session.lockSync();

                if (c != null)
                {
                    c.requestEnterGameAfterStarted(Tools.reinterpret_cast<Player>(pd._session), pd._packet);
                }

                // Bloquear para ver se funciona o sync do entra depois no camp,
                // mesmo que o outro(0x9D) chama primeiro esse(0x0C) é mais rápido para verificar se o player está em uma sala
                pd._session.unlockSync();


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

        public static int packet09E(object param, ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(Tools.reinterpret_cast<Player>(pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestUpdateGachaCoupon(Tools.reinterpret_cast<Player>(pd._session), pd._packet);
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

        public static int packet0A1(object param, ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(Tools.reinterpret_cast<Player>(pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestEnterWebLinkState(Tools.reinterpret_cast<Player>(pd._session), pd._packet);
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

        public static int packet0A2(object param, ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(Tools.reinterpret_cast<Player>(pd._session).m_pi.channel);

                if (c != null)
                {
                    //   c.requestExitedFromWebGuild(Tools.reinterpret_cast<Player>(pd._session), pd._packet);
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

        public static int packet0AA(object param, ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(Tools.reinterpret_cast<Player>(pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestUseTicketReport(Tools.reinterpret_cast<Player>(pd._session), pd._packet);
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

        public static int packet0AB(object param, ParamDispatch pd)
        {

            try
            {

                var c = sgs.gs.getInstance().findChannel(Tools.reinterpret_cast<Player>(pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestOpenTicketReportScroll(Tools.reinterpret_cast<Player>(pd._session), pd._packet);
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

        public static int packet0AE(object param, ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(Tools.reinterpret_cast<Player>(pd._session).m_pi.channel);

                if (c != null)
                {
                    // c.requestMakeTutorial(Tools.reinterpret_cast<Player>(pd._session), pd._packet);
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

        public static int packet0B2(object param, ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(Tools.reinterpret_cast<Player>(pd._session).m_pi.channel);

                if (c != null)
                {
                    // c.requestOpenBoxMyRoom(Tools.reinterpret_cast<Player>(pd._session), pd._packet);
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

        public static int packet0B4(object param, ParamDispatch pd)
        {



            try
            {

                // Esse pacote é que o player aceitou convite do player entrou na sala saiu e relogou, ai manda esse pacote com o número da sala
                /*_smp::message_pool::push(new message("[packet_func::packet0B4][Log] Player[UID=" + (Tools.reinterpret_cast<Player>(pd._session).m_pi.m_uid)
                        + "] mandou o Pacote0B4 mas nao sei o que ele pede ou faz ainda. Hex: \n\r"
                        + hex_util::BufferToHexString(pd._packet.getBuffer(), pd._packet.getSize()), type_msg.CL_FILE_LOG_AND_CONSOLE));*/

                // Verifica se session está varrizada para executar esse ação,
                // se ele não fez o login com o Server ele não pode fazer nada até que ele faça o login
                if (!Tools.reinterpret_cast<Player>(pd._session).m_is_authorized)
                {
                    throw new exception("[packet_func::" + "packet0B4" + "][Error] Player[UID=" + Convert.ToString(Tools.reinterpret_cast<Player>(pd._session).m_pi.uid) + "] Nao esta autorizado a fazer esse request por que ele ainda nao fez o login com o Server. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PACKET_FUNC_SV,
                        1, 0x7000501));
                }

                byte option = pd._packet.ReadByte();
                ushort numero_sala = pd._packet.ReadUInt16();

                // Log
                _smp.message_pool.push(new message("[packet_func::packet0B4][Log][Option=" + Convert.ToString((ushort)option) + "] Player[UID=" + Convert.ToString(Tools.reinterpret_cast<Player>(pd._session).m_pi.uid) + "] foi convidado por um player aceitou o pedido saiu da sala[NUMERO=" + Convert.ToString(numero_sala) + "] e relogou.", type_msg.CL_FILE_LOG_AND_CONSOLE));

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

        public static int packet0B5(object param, ParamDispatch pd)
        {



            try
            {

                // Verifica se session está varrizada para executar esse ação,
                // se ele não fez o login com o Server ele não pode fazer nada até que ele faça o login
                if (!Tools.reinterpret_cast<Player>(pd._session).m_is_authorized)
                {
                    throw new exception("[packet_func::" + "packet0B5(MyrRoomHouseInfo)" + "][Error] Player[UID=" + Convert.ToString(Tools.reinterpret_cast<Player>(pd._session).m_pi.uid) + "] Nao esta autorizado a fazer esse request por que ele ainda nao fez o login com o Server. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PACKET_FUNC_SV,
                        1, 0x7000501));
                }

                uint from_uid = new uint();
                uint to_uid = new uint();

                from_uid = pd._packet.ReadUInt32();
                to_uid = pd._packet.ReadUInt32();

                var p = new PangyaBinaryWriter();
                p.init_plain((ushort)0x12B);

                // Aqui o player só pode pedir para entrar no dele mesmo
                if (from_uid == to_uid && Tools.reinterpret_cast<Player>(pd._session).m_pi.mrc.allow_enter == 1)
                { // Isso tinha no season 4, agora nos season posteriores tiraram isso
                    p.WriteUInt32(1); // option;

                    p.WriteUInt32(to_uid);

                    p.WriteBytes(Tools.reinterpret_cast<Player>(pd._session).m_pi.mrc.ToArray());
                }
                else
                {
                    p.WriteUInt32(0);

                    p.WriteUInt32(to_uid);
                }

                session_send(p, pd._session);

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet0B5][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
            return 0;

        }

        public static int packet0B7(object param, ParamDispatch pd)
        {



            try
            {

                // Verifica se session está varrizada para executar esse ação,
                // se ele não fez o login com o Server ele não pode fazer nada até que ele faça o login
                if (!Tools.reinterpret_cast<Player>(pd._session).m_is_authorized)
                {
                    throw new exception("[packet_func::" + "packet0B7(InfoPlayerMyRoom)" + "][Error] Player[UID=" + Convert.ToString(Tools.reinterpret_cast<Player>(pd._session).m_pi.uid) + "] Nao esta autorizado a fazer esse request por que ele ainda nao fez o login com o Server. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PACKET_FUNC_SV,
                        1, 0x7000501));
                }

                // @@!! Ajeitar para pegar a função estática da sala que initializa o Player Room Info
                PlayerRoomInfoEx pri = new PlayerRoomInfoEx();

                // Player Room Info Init
                pri.oid = (uint)Tools.reinterpret_cast<Player>(pd._session).m_oid;
                pri.position = 0; // posição na sala
                pri.capability = Tools.reinterpret_cast<Player>(pd._session).m_pi.m_cap;
                pri.title = Tools.reinterpret_cast<Player>(pd._session).m_pi.ue.skin_typeid[5];

                if (Tools.reinterpret_cast<Player>(pd._session).m_pi.ei.char_info != null)
                {
                    pri.char_typeid = Tools.reinterpret_cast<Player>(pd._session).m_pi.ei.char_info._typeid;
                }

                pri.skin[4] = 0; // Aqui tem que ser zero, se for outro valor não mostra a imagem do character equipado

                pri.state_flag.master = 1;
                pri.state_flag.ready = 1; // Sempre está pronto(ready) o master

                pri.state_flag.sexo = Tools.reinterpret_cast<Player>(pd._session).m_pi.mi.sexo;

                // Só faz calculo de Quita rate depois que o player
                // estiver no level Beginner E e jogado 50 games
                if (Tools.reinterpret_cast<Player>(pd._session).m_pi.level >= 6 && Tools.reinterpret_cast<Player>(pd._session).m_pi.ui.jogado >= 50)
                {
                    float rate = Tools.reinterpret_cast<Player>(pd._session).m_pi.ui.getQuitRate();

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

                pri.level = (byte)Tools.reinterpret_cast<Player>(pd._session).m_pi.mi.level;

                if (Tools.reinterpret_cast<Player>(pd._session).m_pi.ei.char_info != null && Tools.reinterpret_cast<Player>(pd._session).m_pi.ui.getQuitRate() < GOOD_PLAYER_ICON)
                {
                    pri.icon_angel = Tools.reinterpret_cast<Player>(pd._session).m_pi.ei.char_info.AngelEquiped();
                }
                else
                {
                    pri.icon_angel = 0;
                }

                pri.place.ulPlace = 10; // 0x0A dec"10" _session.m_pi.place
                pri.guild_uid = Tools.reinterpret_cast<Player>(pd._session).m_pi.gi.uid;
                //pri.guild_mark_index = Tools.reinterpret_cast<Player>(pd._session).m_pi.gi.index_mark_emblem;
                pri.uid = Tools.reinterpret_cast<Player>(pd._session).m_pi.uid;
                pri.state_action.state_lounge = Tools.reinterpret_cast<Player>(pd._session).m_pi.state_lounge;
                pri.state_action.usUnknown_flg = 0;
                pri.state_action.state = Tools.reinterpret_cast<Player>(pd._session).m_pi.state;
                pri.location = new PlayerRoomInfo.stLocation() { x = Tools.reinterpret_cast<Player>(pd._session).m_pi.location.x, z = Tools.reinterpret_cast<Player>(pd._session).m_pi.location.z, r = Tools.reinterpret_cast<Player>(pd._session).m_pi.location.r };
                pri.shop = new PlayerRoomInfo.PersonShop();

                if (Tools.reinterpret_cast<Player>(pd._session).m_pi.ei.mascot_info != null)
                {
                    pri.mascot_typeid = Tools.reinterpret_cast<Player>(pd._session).m_pi.ei.mascot_info._typeid;
                }

                pri.flag_item_boost = Tools.reinterpret_cast<Player>(pd._session).m_pi.checkEquipedItemBoost();
                pri.ulUnknown_flg = 0;
                //pri.id_NT não estou usando ainda
                //pri.ucUnknown106
                pri.convidado = 0; // Flag Convidado, [Não sei bem por que os que entra na sala normal tem valor igual aqui, já que é flag de convidado waiting], Valor constante da sala para os players(ACHO)
                pri.avg_score = Tools.reinterpret_cast<Player>(pd._session).m_pi.ui.getMediaScore();
                //pri.ucUnknown3

                if (Tools.reinterpret_cast<Player>(pd._session).m_pi.ei.char_info != null)
                {
                    pri.ci = Tools.reinterpret_cast<Player>(pd._session).m_pi.ei.char_info;
                }

                var p = new PangyaBinaryWriter((ushort)0x168); // Character Equipado

                p.WriteBytes(pri.ToArrayEx());//preciso fazer

                session_send(p, pd._session);


                p.init_plain(0x12D); // Itens do Myroom, Mala, Email, sofa, teto chao, e poster, "NESSA SEASON, SÓ USA POSTER"

                p.WriteUInt32(1); // Option

                p.WriteUInt16((ushort)Tools.reinterpret_cast<Player>(pd._session).m_pi.v_mri.Count);

                for (var i = 0; i < Tools.reinterpret_cast<Player>(pd._session).m_pi.v_mri.Count; ++i)
                {
                    p.WriteBytes(Tools.reinterpret_cast<Player>(pd._session).m_pi.v_mri[i].ToArray());
                }

                session_send(p, pd._session);

                //return 1 sucess
            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet0B7][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
                return 0;
            }

            return 0;

        }

        public static int packet0B9(object param, ParamDispatch pd)
        {



            try
            {

                sgs.gs.getInstance().requestUCCSystem(Tools.reinterpret_cast<Player>(pd._session), pd._packet);

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

        public static int packet0BA(object param, ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(Tools.reinterpret_cast<Player>(pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestInvite(Tools.reinterpret_cast<Player>(pd._session), pd._packet);
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

        public static int packet0BD(object param, ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(Tools.reinterpret_cast<Player>(pd._session).m_pi.channel);

                if (c != null)
                {
                    //  c.requestUseCardSpecial(Tools.reinterpret_cast<Player>(pd._session), pd._packet);
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

        public static int packet0C1(object param, ParamDispatch pd)
        {



            try
            {

                // Verifica se session está varrizada para executar esse ação,
                // se ele não fez o login com o Server ele não pode fazer nada até que ele faça o login
                if (!Tools.reinterpret_cast<Player>(pd._session).m_is_authorized)
                {
                    throw new exception("[packet_func::" + "packet0C1(UpdatePlace)" + "][Error] Player[UID=" + Convert.ToString(Tools.reinterpret_cast<Player>(pd._session).m_pi.uid) + "] Nao esta autorizado a fazer esse request por que ele ainda nao fez o login com o Server. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PACKET_FUNC_SV,
                        1, 0x7000501));
                }

                Tools.reinterpret_cast<Player>(pd._session).m_pi.place = pd._packet.ReadSByte(); // Att place(lugar)
                _smp.message_pool.push(new message("[packet_func::packet0C1][Log] " + Tools.reinterpret_cast<Player>(pd._session).m_pi.place.ToString(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                // Update Location Player on DB
                Tools.reinterpret_cast<Player>(pd._session).m_pi.updateLocationDB();
                //sucess
            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet0C1][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
            return 0;

        }

        public static int packet0C9(object param, ParamDispatch pd)
        {



            try
            {

                sgs.gs.getInstance().requestUCCWebKey(Tools.reinterpret_cast<Player>(pd._session), pd._packet);

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

        public static int packet0CA(object param, ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(Tools.reinterpret_cast<Player>(pd._session).m_pi.channel);

                if (c != null)
                {
                    //  c.requestOpenCardPack(Tools.reinterpret_cast<Player>(pd._session), pd._packet);
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

        public static int packet0CB(object param, ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(Tools.reinterpret_cast<Player>(pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestFinishGame(Tools.reinterpret_cast<Player>(pd._session), pd._packet);
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

        public static int packet0CC(object param, ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(Tools.reinterpret_cast<Player>(pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestCheckDolfiniLockerPass(Tools.reinterpret_cast<Player>(pd._session), pd._packet);
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

        public static int packet0CD(object param, ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(Tools.reinterpret_cast<Player>(pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestDolfiniLockerItem(Tools.reinterpret_cast<Player>(pd._session), pd._packet);
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

        public static int packet0CE(object param, ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(Tools.reinterpret_cast<Player>(pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestAddDolfiniLockerItem(Tools.reinterpret_cast<Player>(pd._session), pd._packet);
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

        public static int packet0CF(object param, ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(Tools.reinterpret_cast<Player>(pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestRemoveDolfiniLockerItem(Tools.reinterpret_cast<Player>(pd._session), pd._packet);
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

        public static int packet0D0(object param, ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(Tools.reinterpret_cast<Player>(pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestMakePassDolfiniLocker(Tools.reinterpret_cast<Player>(pd._session), pd._packet);
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

        public static int packet0D1(object param, ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(Tools.reinterpret_cast<Player>(pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestChangeDolfiniLockerPass(Tools.reinterpret_cast<Player>(pd._session), pd._packet);
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

        public static int packet0D2(object param, ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(Tools.reinterpret_cast<Player>(pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestChangeDolfiniLockerModeEnter(Tools.reinterpret_cast<Player>(pd._session), pd._packet);
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

        public static int packet0D3(object param, ParamDispatch pd)
        {
            try
            {

                // Verifica se session está varrizada para executar esse ação,
                // se ele não fez o login com o Server ele não pode fazer nada até que ele faça o login
                if (!Tools.reinterpret_cast<Player>(pd._session).m_is_authorized)
                {
                    throw new exception("[packet_func::" + "packet0D3(CheckDolfiniLocker)" + "][Error] Player[UID=" + Convert.ToString(Tools.reinterpret_cast<Player>(pd._session).m_pi.uid) + "] Nao esta autorizado a fazer esse request por que ele ainda nao fez o login com o Server. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PACKET_FUNC_SV,
                        1, 0x7000501));
                }

                uint check = 0u;

                check = Tools.reinterpret_cast<Player>(pd._session).m_pi.df.isLocker();

                var p = new PangyaBinaryWriter((ushort)0x170);

                p.WriteUInt32(0); // option
                p.WriteUInt32(check);

                session_send(p, pd._session);

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet0D3][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
            return 0;
        }

        public static int packet0D4(object param, ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(Tools.reinterpret_cast<Player>(pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestUpdateDolfiniLockerPang(Tools.reinterpret_cast<Player>(pd._session), pd._packet);
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

        public static int packet0D5(object param, ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(Tools.reinterpret_cast<Player>(pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestDolfiniLockerPang(Tools.reinterpret_cast<Player>(pd._session), pd._packet);
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

        public static int packet0D8(object param, ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(Tools.reinterpret_cast<Player>(pd._session).m_pi.channel);

                if (c != null)
                {
                    // c.requestUseItemBuff(Tools.reinterpret_cast<Player>(pd._session), pd._packet);
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

        public static int packet0DE(object param, ParamDispatch pd)
        {



            try
            {

                // Player não pode ver a message privada que o player mandou, avisa para o server
                /*_smp::message_pool::push(new message("[packet_func::packet0DE][Log] Player[UID=" + (Tools.reinterpret_cast<Player>(pd._session).m_pi.m_uid)
                        + "] mandou o Pacote0DE mas nao sei o que ele pede ou faz ainda. Hex: \n\r"
                        + hex_util::BufferToHexString(pd._packet.getBuffer(), pd._packet.getSize()), type_msg.CL_FILE_LOG_AND_CONSOLE));*/

                // Envia mensagem para o player que enviou o MP que o player não pode ver a mensagem
                sgs.gs.getInstance().requestNotifyNotDisplayPrivateMessageNow(Tools.reinterpret_cast<Player>(pd._session), pd._packet);

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

        public static int packet0E5(object param, ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(Tools.reinterpret_cast<Player>(pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestActiveCutin(Tools.reinterpret_cast<Player>(pd._session), pd._packet);
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

        public static int packet0E6(object param, ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(Tools.reinterpret_cast<Player>(pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestExtendRental(Tools.reinterpret_cast<Player>(pd._session), pd._packet);
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

        public static int packet0E7(object param, ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(Tools.reinterpret_cast<Player>(pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestDeleteRental(Tools.reinterpret_cast<Player>(pd._session), pd._packet);
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

        public static int packet0EB(object param, ParamDispatch pd)
        {



            try
            {

                Channel c = sgs.gs.getInstance().findChannel(Tools.reinterpret_cast<Player>(pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestPlayerStateCharacterLounge(Tools.reinterpret_cast<Player>(pd._session), pd._packet);
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

        public static int packet0EC(object param, ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(Tools.reinterpret_cast<Player>(pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestCometRefill(Tools.reinterpret_cast<Player>(pd._session), pd._packet);
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

        public static int packet0EF(object param, ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(Tools.reinterpret_cast<Player>(pd._session).m_pi.channel);

                if (c != null)
                {
                    // c.requestOpenBoxMail(Tools.reinterpret_cast<Player>(pd._session), pd._packet);
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

        public static int packet0F4(object param, ParamDispatch pd)
        {
            try
            {
                // TTL = Time To Live    

                // Verifica se session está varrizada para executar esse ação,
                // se ele não fez o login com o Server ele não pode fazer nada até que ele faça o login
                if (!Tools.reinterpret_cast<Player>(pd._session).m_is_authorized)
                {
                    throw new exception("[packet_func::" + "packet0F4" + "][Error] Player[UID=" + Convert.ToString(Tools.reinterpret_cast<Player>(pd._session).m_pi.uid) + "] Nao esta autorizado a fazer esse request por que ele ainda nao fez o login com o Server. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PACKET_FUNC_SV,
                        1, 0x7000501));
                }

                Tools.reinterpret_cast<Player>(pd._session).m_tick_bot = Environment.TickCount;

                //pd._session.m_time_start = std::clock();
                //pd._session.m_tick = std::clock();
                if (Tools.reinterpret_cast<Player>(pd._session).m_pi.uid > 0)
                {
                    var cp = Tools.reinterpret_cast<Player>(pd._session).m_pi.cookie;
                    var pang = Tools.reinterpret_cast<Player>(pd._session).m_pi.ui.pang;
                    Tools.reinterpret_cast<Player>(pd._session).m_pi.updateMoeda();
                    // Tools.reinterpret_cast<Player>(pd._session).m_pi.ReloadMemberInfo();
                    using (var p = new PangyaBinaryWriter())
                    {
                        if (cp != Tools.reinterpret_cast<Player>(pd._session).m_pi.cookie)  //so envia se estiver com valores novos
                        {
                            //// Update ON GAME(cookies)
                            p.init_plain(0x96);
                            p.WriteUInt64(Tools.reinterpret_cast<Player>(pd._session).m_pi.cookie);
                            p.WriteUInt32(0);
                            session_send(p, Tools.reinterpret_cast<Player>(pd._session), 1);
                        }
                        if (pang != Tools.reinterpret_cast<Player>(pd._session).m_pi.ui.pang) //so envia se estiver com valores novos
                        {
                            // UPDATE pang ON GAME(pangs)
                            p.init_plain(0xC8);
                            p.WriteUInt64(Tools.reinterpret_cast<Player>(pd._session).m_pi.ui.pang);
                            p.WriteUInt32(0);
                            session_send(p, Tools.reinterpret_cast<Player>(pd._session), 1);
                        }
                    }
                }


            }
            catch (exception e)
            {
                _smp.message_pool.push(new message("[packet_func::packet0F4][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
            return 0;
        }

        public static int packet0FB(object param, ParamDispatch pd)
        {
            try
            {

                // Verifica se session está varrizada para executar esse ação,
                // se ele não fez o login com o Server ele não pode fazer nada até que ele faça o login
                if (!Tools.reinterpret_cast<Player>(pd._session).m_is_authorized)
                {
                    throw new exception("[packet_func::" + "packet0FB(WebKey)" + "][Error] Player[UID=" + Convert.ToString(Tools.reinterpret_cast<Player>(pd._session).m_pi.uid) + "] Nao esta autorizado a fazer esse request por que ele ainda nao fez o login com o Server. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PACKET_FUNC_SV,
                        1, 0x7000501));
                }

                var cmd_gwk = new CmdGeraWebKey(Tools.reinterpret_cast<Player>(pd._session).m_pi.uid);

                NormalManagerDB.add(0, cmd_gwk, null, null);


                if (cmd_gwk.getException().getCodeError() != 0)
                    throw cmd_gwk.getException();

                var webKey = cmd_gwk.getKey();

                session_send(pacote1AD(webKey, 1), pd._session);


            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet0FB][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                session_send(pacote1AD("", 0), pd._session);
            }
            return 0;

        }

        public static int packet0FE(object param, ParamDispatch pd)
        {
            //packet 254, no send response!        
            try
            {
                // Verifica se session está varrizada para executar esse ação,
                // se ele não fez o login com o Server ele não pode fazer nada até que ele faça o login     
                CHECK_SESSION_IS_AUTHORIZED((Player)pd._session, "packet0FE");

                //  UCCSystem.HandleUCCLoadTools.reinterpret_cast<Player>(pd._session);
                session_send(pacote0E2(), pd._session);
            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet0FE][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }

            return 0;

        }

        public static int packet119(object param, ParamDispatch pd)
        {
            var p = new PangyaBinaryWriter();

            try
            {

                sgs.gs.getInstance().requestChangeServer(Tools.reinterpret_cast<Player>(pd._session), pd._packet);

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet119][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
            return 0;

        }

        public static int packet126(object param, ParamDispatch pd)
        {
            try
            {

                var c = sgs.gs.getInstance().findChannel(Tools.reinterpret_cast<Player>(pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestOpenLegacyTikiShop(Tools.reinterpret_cast<Player>(pd._session), pd._packet);
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

        public static int packet127(object param, ParamDispatch pd)
        {

            try
            {

                var c = sgs.gs.getInstance().findChannel(Tools.reinterpret_cast<Player>(pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestPointLegacyTikiShop(Tools.reinterpret_cast<Player>(pd._session), pd._packet);
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

        public static int packet128(object param, ParamDispatch pd)
        {
            try
            {

                var c = sgs.gs.getInstance().findChannel(Tools.reinterpret_cast<Player>(pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestExchangeTPByItemLegacyTikiShop(Tools.reinterpret_cast<Player>(pd._session), pd._packet);
                }

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet128][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if ((STDA_ERROR_TYPE)ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) != STDA_ERROR_TYPE.CHANNEL)
                {
                    throw; // Relança 
                }
            }
            return 0;
        }

        public static int packet129(object param, ParamDispatch pd)
        {
            try
            {

                var c = sgs.gs.getInstance().findChannel(Tools.reinterpret_cast<Player>(pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestExchangeItemByTPLegacyTikiShop(Tools.reinterpret_cast<Player>(pd._session), pd._packet);
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

        public static int packet12C(object param, ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(Tools.reinterpret_cast<Player>(pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestFinishGame(Tools.reinterpret_cast<Player>(pd._session), pd._packet);
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

        public static int packet12D(object param, ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(Tools.reinterpret_cast<Player>(pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestReplyInitialValueGrandZodiac(Tools.reinterpret_cast<Player>(pd._session), pd._packet);
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

        public static int packet12E(object param, ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(Tools.reinterpret_cast<Player>(pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestMarkerOnCourse(Tools.reinterpret_cast<Player>(pd._session), pd._packet);
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

        public static int packet12F(object param, ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(Tools.reinterpret_cast<Player>(pd._session).m_pi.channel);

#if DEBUG
                _smp.message_pool.push(new message("[packet_func::packet12F][Log] request Player[UID=" + Convert.ToString(Tools.reinterpret_cast<Player>(pd._session).m_pi.uid) + "]", type_msg.CL_FILE_LOG_AND_CONSOLE));
#endif // _DEBUG

                if (c != null)
                {
                    c.requestShotEndData(Tools.reinterpret_cast<Player>(pd._session), pd._packet);
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

        public static int packet130(object param, ParamDispatch pd)
        {
            var p = new PangyaBinaryWriter();

            try
            {

                var c = sgs.gs.getInstance().findChannel(Tools.reinterpret_cast<Player>(pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestLeavePractice(Tools.reinterpret_cast<Player>(pd._session), pd._packet);
                }

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet130][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }

            return 0;
        }

        public static int packet131(object param, ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(Tools.reinterpret_cast<Player>(pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestLeaveChipInPractice(Tools.reinterpret_cast<Player>(pd._session), pd._packet);
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

        public static int packet137(object param, ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(Tools.reinterpret_cast<Player>(pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestStartFirstHoleGrandZodiac(Tools.reinterpret_cast<Player>(pd._session), pd._packet);
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

        public static int packet138(object param, ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(Tools.reinterpret_cast<Player>(pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestActiveWing(Tools.reinterpret_cast<Player>(pd._session), pd._packet);
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

        public static int packet140(object param, ParamDispatch pd)
        {
            try
            {

                // Verifica se session está varrizada para executar esse ação,
                // se ele não fez o login com o Server ele não pode fazer nada até que ele faça o login
                CHECK_SESSION_IS_AUTHORIZED((Player)pd._session, "packet140(requestEnterShop)");

                var c = sgs.gs.getInstance().findChannel(Tools.reinterpret_cast<Player>(pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestEnterShop(Tools.reinterpret_cast<Player>(pd._session), pd._packet);
                }
            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet140][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
            return 0;
        }

        public static int packet141(object param, ParamDispatch pd)
        {
            try
            {

                var c = sgs.gs.getInstance().findChannel(Tools.reinterpret_cast<Player>(pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestChangeWindNextHoleRepeat(Tools.reinterpret_cast<Player>(pd._session), pd._packet);
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

        public static int packet143(object param, ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(Tools.reinterpret_cast<Player>(pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestOpenMailBox(Tools.reinterpret_cast<Player>(pd._session), pd._packet);
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

        public static int packet144(object param, ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(Tools.reinterpret_cast<Player>(pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestInfoMail(Tools.reinterpret_cast<Player>(pd._session), pd._packet);
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

        public static int packet145(object param, ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(Tools.reinterpret_cast<Player>(pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestSendMail(Tools.reinterpret_cast<Player>(pd._session), pd._packet);
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

        public static int packet146(object param, ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(Tools.reinterpret_cast<Player>(pd._session).m_pi.channel);

#if DEBUG
                _smp.message_pool.push(new message("[packet_func::packet146][Log] Request player[UID=" + Convert.ToString(Tools.reinterpret_cast<Player>(pd._session).m_pi.uid) + "]", type_msg.CL_FILE_LOG_AND_CONSOLE));
#endif // _DEBUG

                if (c != null)
                {
                    c.requestTakeItemFomMail(Tools.reinterpret_cast<Player>(pd._session), pd._packet);
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

        public static int packet147(object param, ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(Tools.reinterpret_cast<Player>(pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestDeleteMail(Tools.reinterpret_cast<Player>(pd._session), pd._packet);
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

        public static int packet14B(object param, ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(Tools.reinterpret_cast<Player>(pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestPlayPapelShop(Tools.reinterpret_cast<Player>(pd._session), pd._packet);
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

        public static int packet151(object param, ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(Tools.reinterpret_cast<Player>(pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestDailyQuest(Tools.reinterpret_cast<Player>(pd._session), pd._packet);
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

        public static int packet152(object param, ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(Tools.reinterpret_cast<Player>(pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestAcceptDailyQuest(Tools.reinterpret_cast<Player>(pd._session), pd._packet);
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

        public static int packet153(object param, ParamDispatch pd)
        {
            try
            {

                var c = sgs.gs.getInstance().findChannel(Tools.reinterpret_cast<Player>(pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestTakeRewardDailyQuest(Tools.reinterpret_cast<Player>(pd._session), pd._packet);
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

        public static int packet154(object param, ParamDispatch pd)
        {
            try
            {

                var c = sgs.gs.getInstance().findChannel(Tools.reinterpret_cast<Player>(pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestLeaveDailyQuest(Tools.reinterpret_cast<Player>(pd._session), pd._packet);
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

        public static int packet155(object param, ParamDispatch pd)
        {
            try
            {

                var c = sgs.gs.getInstance().findChannel(Tools.reinterpret_cast<Player>(pd._session).m_pi.channel);

                if (c != null)
                {
                    //c.requestLoloCardCompose(Tools.reinterpret_cast<Player>(pd._session), pd._packet);
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

        public static int packet156(object param, ParamDispatch pd)
        {
            try
            {

                var c = sgs.gs.getInstance().findChannel(Tools.reinterpret_cast<Player>(pd._session).m_pi.channel);

                if (c != null)
                {
                    //  c.requestActiveAutoCommand(Tools.reinterpret_cast<Player>(pd._session), pd._packet);                 
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

        public static int packet157(object param, ParamDispatch pd)
        {
            var p = new PangyaBinaryWriter();

            try
            {

                uint uid = pd._packet.ReadUInt32();

                // Log
#if DEBUG
                _smp.message_pool.push(new message("[packet_func::packet157][Log] Player[UID=" + Convert.ToString(Tools.reinterpret_cast<Player>(pd._session).m_pi.uid) + "].\tPlayer Achievement request uid " + Convert.ToString(uid), type_msg.CL_FILE_LOG_AND_CONSOLE));
#else
					_smp.message_pool.push(new message("[packet_func::packet157][Log] Player[UID=" + Convert.ToString(Tools.reinterpret_cast<Player>(pd._session).m_pi.uid) + "].\tPlayer Achievement request uid: " + Convert.ToString(uid), type_msg.CL_ONLY_FILE_LOG));
#endif // _DEBUG

                //// Verifica se session está varrizada para executar esse ação,
                //// se ele não fez o login com o Server ele não pode fazer nada até que ele faça o login
                //if (!Tools.reinterpret_cast<Player>(pd._session).m_is_authorized)
                //{
                //    throw new exception("[packet_func::" + "packet157(requestAchievementInfo)" + "][Error] Player[UID=" + Convert.ToString(Tools.reinterpret_cast<Player>(pd._session).m_pi.m_uid) + "] Nao esta autorizado a fazer esse request por que ele ainda nao fez o login com o Server. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PACKET_FUNC_SV,
                //        1, 0x7000501));
                //}

                MgrAchievement mgr_achievement = null;
                Player s = null;

                if (Tools.reinterpret_cast<Player>(pd._session).m_pi.uid == uid) // O player solicitou o próprio achievement info
                {
                    mgr_achievement = Tools.reinterpret_cast<Player>(pd._session).m_pi.mgr_achievement;
                }
                else if ((s = sgs.gs.getInstance().findPlayer(uid)) != null) // O player solicitou o achievement info de outro player online
                {
                    mgr_achievement = s.m_pi.mgr_achievement;
                }
                else
                { // O player solicitou o achievement info de outro player off-line

                    mgr_achievement = new MgrAchievement();

                    mgr_achievement.initAchievement(uid);

                    mgr_achievement.sendAchievementGuiToPlayer(Tools.reinterpret_cast<Player>(pd._session)); 
                }

                if (mgr_achievement == null)
                {
                    session_send(pacote22C(1), Tools.reinterpret_cast<Player>(pd._session)); // unsucess  
                }
                else
                {
                    mgr_achievement.sendAchievementGuiToPlayer(Tools.reinterpret_cast<Player>(pd._session));
                }

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet157][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                session_send(pacote22C(1), Tools.reinterpret_cast<Player>(pd._session)); // unsucess  
            }
            return 0;
        }

        public static int packet158(object param, ParamDispatch pd)
        {
            try
            {

                var c = sgs.gs.getInstance().findChannel(Tools.reinterpret_cast<Player>(pd._session).m_pi.channel);

                if (c != null)
                {
                    // c.requestCadieCauldronExchange(Tools.reinterpret_cast<Player>(pd._session), pd._packet);
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

        public static int packet15C(object param, ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(Tools.reinterpret_cast<Player>(pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestActivePaws(Tools.reinterpret_cast<Player>(pd._session), pd._packet);
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

        public static int packet15D(object param, ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(Tools.reinterpret_cast<Player>(pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestActiveRing(Tools.reinterpret_cast<Player>(pd._session), pd._packet);
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

        public static int packet164(object param, ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(Tools.reinterpret_cast<Player>(pd._session).m_pi.channel);

                if (c != null)
                {
                    //  c.requestClubSetWorkShopUpLevel(Tools.reinterpret_cast<Player>(pd._session), pd._packet);
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

        public static int packet165(object param, ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(Tools.reinterpret_cast<Player>(pd._session).m_pi.channel);

                if (c != null)
                {
                    //  c.requestClubSetWorkShopUpLevelConfirm(Tools.reinterpret_cast<Player>(pd._session), pd._packet);
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

        public static int packet166(object param, ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(Tools.reinterpret_cast<Player>(pd._session).m_pi.channel);

                if (c != null)
                {
                    //  c.requestClubSetWorkShopUpLevelCancel(Tools.reinterpret_cast<Player>(pd._session), pd._packet);
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

        public static int packet167(object param, ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(Tools.reinterpret_cast<Player>(pd._session).m_pi.channel);

                if (c != null)
                {
                    // c.requestClubSetWorkShopUpRank(Tools.reinterpret_cast<Player>(pd._session), pd._packet);
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

        public static int packet168(object param, ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(Tools.reinterpret_cast<Player>(pd._session).m_pi.channel);

                if (c != null)
                {
                    // c.requestClubSetWorkShopUpRankTransformConfirm(Tools.reinterpret_cast<Player>(pd._session), pd._packet);
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

        public static int packet169(object param, ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(Tools.reinterpret_cast<Player>(pd._session).m_pi.channel);

                if (c != null)
                {
                    //  c.requestClubSetWorkShopUpRankTransformCancel(Tools.reinterpret_cast<Player>(pd._session), pd._packet);
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

        public static int packet16B(object param, ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(Tools.reinterpret_cast<Player>(pd._session).m_pi.channel);

                if (c != null)
                {
                    //  c.requestClubSetWorkShopRecoveryPts(Tools.reinterpret_cast<Player>(pd._session), pd._packet);
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

        public static int packet16C(object param, ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(Tools.reinterpret_cast<Player>(pd._session).m_pi.channel);

                if (c != null)
                {
                    //   c.requestClubSetWorkShopTransferMasteryPts(Tools.reinterpret_cast<Player>(pd._session), pd._packet);
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

        public static int packet16D(object param, ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(Tools.reinterpret_cast<Player>(pd._session).m_pi.channel);

                if (c != null)
                {
                    // c.requestClubSetReset(Tools.reinterpret_cast<Player>(pd._session), pd._packet);
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

        public static int packet16E(object param, ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(Tools.reinterpret_cast<Player>(pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestCheckAttendanceReward(Tools.reinterpret_cast<Player>(pd._session), pd._packet);
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

        public static int packet16F(object param, ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(Tools.reinterpret_cast<Player>(pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestAttendanceRewardLoginCount(Tools.reinterpret_cast<Player>(pd._session), pd._packet);
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

        public static int packet171(object param, ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(Tools.reinterpret_cast<Player>(pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestActiveEarcuff(Tools.reinterpret_cast<Player>(pd._session), pd._packet);
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

        public static int packet172(object param, ParamDispatch pd)
        {



            try
            {
                Player _session = (Player)pd._session;
                // !@ Log
                message_pool.push(new message("[packet_func::packet172][Log] Player[UID=" + Convert.ToString(Tools.reinterpret_cast<Player>(pd._session).m_pi.uid) + "] request open Event Workshop 2013.", type_msg.CL_FILE_LOG_AND_CONSOLE));

                session_send(pacote0E2(), pd._session);

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet172][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
            return 0;
        }

        public static int packet176(object param, ParamDispatch pd)
        {
            try
            {

                var c = sgs.gs.getInstance().findChannel(Tools.reinterpret_cast<Player>(pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestEnterLobbyGrandPrix(Tools.reinterpret_cast<Player>(pd._session), pd._packet);
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

        public static int packet177(object param, ParamDispatch pd)
        {
            try
            {

                var c = sgs.gs.getInstance().findChannel(Tools.reinterpret_cast<Player>(pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestExitLobbyGrandPrix(Tools.reinterpret_cast<Player>(pd._session), pd._packet);
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

        public static int packet179(object param, ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(Tools.reinterpret_cast<Player>(pd._session).m_pi.channel);

                if (c != null)
                {
                    //c.requestEnterRoomGrandPrix(Tools.reinterpret_cast<Player>(pd._session), pd._packet);
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

        public static int packet17A(object param, ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(Tools.reinterpret_cast<Player>(pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestExitRoomGrandPrix(Tools.reinterpret_cast<Player>(pd._session), pd._packet);
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

        public static int packet17F(object param, ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(Tools.reinterpret_cast<Player>(pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestPlayMemorial(Tools.reinterpret_cast<Player>(pd._session), pd._packet);
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

        public static int packet180(object param, ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(Tools.reinterpret_cast<Player>(pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestActiveGlove(Tools.reinterpret_cast<Player>(pd._session), pd._packet);
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

        public static int packet181(object param, ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(Tools.reinterpret_cast<Player>(pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestActiveRingGround(Tools.reinterpret_cast<Player>(pd._session), pd._packet);
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

        public static int packet184(object param, ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(Tools.reinterpret_cast<Player>(pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestToggleAssist(Tools.reinterpret_cast<Player>(pd._session), pd._packet);
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

        public static int packet185(object param, ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(Tools.reinterpret_cast<Player>(pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestActiveAssistGreen(Tools.reinterpret_cast<Player>(pd._session), pd._packet);
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

        public static int packet187(object param, ParamDispatch pd)
        {
            try
            {

                var c = sgs.gs.getInstance().findChannel(Tools.reinterpret_cast<Player>(pd._session).m_pi.channel);

                if (c != null)
                {
                    // c.requestCharacterMasteryExpand(Tools.reinterpret_cast<Player>(pd._session), pd._packet);
                }

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet187][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));


            }
            return 0;

        }

        public static int packet188(object param, ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(Tools.reinterpret_cast<Player>(pd._session).m_pi.channel);

                if (c != null)
                {
                    // c.requestCharacterStatsUp(Tools.reinterpret_cast<Player>(pd._session), pd._packet);
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

        public static int packet189(object param, ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(Tools.reinterpret_cast<Player>(pd._session).m_pi.channel);

                if (c != null)
                {
                    // c.requestCharacterStatsDown(Tools.reinterpret_cast<Player>(pd._session), pd._packet);
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

        public static int packet18A(object param, ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(Tools.reinterpret_cast<Player>(pd._session).m_pi.channel);

                if (c != null)
                {
                    // c.requestCharacterCardEquip(Tools.reinterpret_cast<Player>(pd._session), pd._packet);
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

        public static int packet18B(object param, ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(Tools.reinterpret_cast<Player>(pd._session).m_pi.channel);

                if (c != null)
                {
                    //c.requestCharacterCardEquipWithPatcher(Tools.reinterpret_cast<Player>(pd._session), pd._packet);
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

        public static int packet18C(object param, ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(Tools.reinterpret_cast<Player>(pd._session).m_pi.channel);

                if (c != null)
                {
                    //c.requestCharacterRemoveCard(Tools.reinterpret_cast<Player>(pd._session), pd._packet);
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

        public static int packet18D(object param, ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(Tools.reinterpret_cast<Player>(pd._session).m_pi.channel);

                if (c != null)
                {
                    // c.requestTikiShopExchangeItem(Tools.reinterpret_cast<Player>(pd._session), pd._packet);
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

        public static int packet192(object param, ParamDispatch pd)
        {
            try
            {

                // !@ Log
                _smp.message_pool.push(new message("[packet_func::packet192][Log] Player[UID=" + Convert.ToString(Tools.reinterpret_cast<Player>(pd._session).m_pi.uid) + "] request open Event Arin 2014.", type_msg.CL_FILE_LOG_AND_CONSOLE));

                _smp.message_pool.push(new message("[packet_func::packet192][Log] Player[UID=" + Convert.ToString(Tools.reinterpret_cast<Player>(pd._session).m_pi.uid) + "] " + pd._packet.Log(), type_msg.CL_FILE_LOG_AND_CONSOLE));


            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[packet_func::packet192][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }

            return 0;
        }

        public static int packet196(object param, ParamDispatch pd)
        {
            try
            {

                var c = sgs.gs.getInstance().findChannel(Tools.reinterpret_cast<Player>(pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestActiveRingPawsRainbowJP(Tools.reinterpret_cast<Player>(pd._session), pd._packet);
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

        public static int packet197(object param, ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(Tools.reinterpret_cast<Player>(pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestActiveRingPowerGagueJP(Tools.reinterpret_cast<Player>(pd._session), pd._packet);
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

        public static int packet198(object param, ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(Tools.reinterpret_cast<Player>(pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestActiveRingMiracleSignJP(Tools.reinterpret_cast<Player>(pd._session), pd._packet);
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

        public static int packet199(object param, ParamDispatch pd)
        {



            try
            {

                var c = sgs.gs.getInstance().findChannel(Tools.reinterpret_cast<Player>(pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestActiveRingPawsRingSetJP(Tools.reinterpret_cast<Player>(pd._session), pd._packet);
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



        public static int packet_sv055(object param, ParamDispatch pd)
        {
            try
            {

                var c = sgs.gs.getInstance().findChannel(Tools.reinterpret_cast<Player>(pd._session).m_pi.channel);

                if (c != null)
                {
                    c.requestInitShotSended(Tools.reinterpret_cast<Player>(pd._session), pd._packet);
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


        //////

        public static int principal(ref PangyaBinaryWriter p, PlayerInfo pi, ServerInfoEx _si)
        {
            try
            {
                if (pi == null)
                    throw new exception("Erro PlayerInfo *pi is null. packet_func::principal()");

                p.WritePStr(_si.version_client);

                // member info
                p.WriteUInt16(ushort.MaxValue);//num the room
                                               //write struct member info player      
                p.WriteBytes(pi.mi.ToArray());//new version
                // write struct user info player(statistic)
                p.WriteUInt32(pi.uid);
                p.WriteBytes(pi.ui.ToArray());//new version

                // write struct Trofel Info
                p.WriteBytes(pi.ti_current_season.ToArray());
                //write struct User Equip
                p.WriteBytes(pi.ue.ToArray());//new version
                for (byte st_i = 0; st_i < MS_NUM_MAPS; st_i++)
                {
                    p.WriteBytes(pi.a_ms_normal[st_i].ToArray());
                }

                // Map Statistics Natural
                for (byte st_i = 0; st_i < MS_NUM_MAPS; st_i++)
                {
                    p.WriteBytes(pi.a_ms_natural[st_i].ToArray());
                }

                // Map Statistics Grand Prix
                for (byte st_i = 0; st_i < MS_NUM_MAPS; st_i++)
                {
                    p.WriteBytes(pi.a_ms_grand_prix[st_i].ToArray());
                }

                // Map Statistics Normal for all seasons
                for (int j = 0; j < 9; j++)
                {
                    for (var st_i = 0; st_i < MS_NUM_MAPS; st_i++)        //talvez algum problema aqui!
                    {
                        p.WriteBytes(pi.aa_ms_normal_todas_season[st_i].ToArray());
                    }
                }
                //Equiped Items
                #region EquipedItem
                p.WriteBytes(pi.ei.ToArray());

                #endregion
                // Write Time, 16 Bytes
                p.WriteTime();

                // Config do Server(struct for server)
                p.WriteUInt16(pi.mi.flag_login_time); // Valor padrão, 1 na primeira vez, 2 para logins subsequentes 
                p.WriteBytes(pi.mi.papel_shop.ToArray());
                p.WriteInt32(0); // Valor novo no JP, indicado como 0 em novas contas 
                p.WriteUInt64(pi.block_flag.m_flag.ullFlag); // Flag do server para bloquear sistemas 
                p.WriteUInt32(pi.ari.counter); // Quantidade de vezes que logou 
                p.WriteUInt32(_si.propriedade.ulProperty); 
                return 0;
            }
            catch (exception e)
            {
                _smp.message_pool.push("[packet_func_gs::InitialLogin]", e);
                return 1;
            }
        }

        public static byte[] pacote047(List<RoomInfo> v_element, int option)
        {

            PangyaBinaryWriter p = new PangyaBinaryWriter();

            p.init_plain(0x47);
            p.WriteByte((byte)((option == 0) ? v_element.Count() : 1));              // count;
            p.WriteByte((byte)option);
            p.WriteUInt16(-1);                 // Não sei bem, mas sempre peguei esse pacote com -1 aqui             
            for (var i = 0; i < v_element.Count(); ++i)
                p.WriteBytes(v_element[i].ToArray()); 
            return p.GetBytes;
        }

        public static List<PangyaBinaryWriter> pacote046(List<PlayerCanalInfo> v_element, int option)
        {
            var responses = new List<PangyaBinaryWriter>();
            int elements = v_element.Count;
            int itensPorPacote = 20;

            // Divide a lista apenas se necessário
            var splitList = (elements * 200 < (MAX_BUFFER_PACKET - 100))
                ? new List<List<PlayerCanalInfo>> { v_element } // Envia tudo em um pacote
                : v_element.Select((item, index) => new { item, index })
                           .GroupBy(x => x.index / itensPorPacote)
                           .Select(g => g.Select(x => x.item).ToList())
                           .ToList();

            // Gera pacotes corretamente
            foreach (var lista in splitList)
            {
                var p = new PangyaBinaryWriter();
                p.Write(new byte[] { 0x46, 0x00 });
                p.WriteByte((byte)option);
                p.WriteByte((byte)lista.Count);

                foreach (var item in lista)
                {
                    p.WriteBytes(item.ToArray());
                }

                responses.Add(p);
            }

            return responses;
        }

        public static byte[] pacote11F(PlayerInfo pi, short tipo)
        {
            var p = new PangyaBinaryWriter();
            if (pi == null)
                throw new exception("Erro PlayerInfo *pi is null. packet_func::pacote11F()");

            p.init_plain(0x11F);

            p.WriteInt16(tipo);

            p.WriteBytes(pi.TutoInfo.ToArray());
            return p.GetBytes;
        }

        public static byte[] pacote1A9(int ttl_milliseconds/*time to live*/, int option = 1)
        {
            var p = new PangyaBinaryWriter(0x1A9);

            p.WriteByte((byte)option);

            p.WriteInt32(ttl_milliseconds);
            return p.GetBytes;
        }

        public static byte[] pacote095(short sub_tipo, int option = 0, PlayerInfo pi = null)
        {
            var p = new PangyaBinaryWriter(0x95);

            p.WriteInt16(sub_tipo);

            if (sub_tipo == 0x102)
                p.WriteByte((byte)option);

            else if (sub_tipo == 0x111)
            {
                p.WriteInt32(option);

                if (pi == null)
                {
                    //delete p;

                    throw new exception("Erro PlayerInfo *pi is null. packet_func::pacote095()");
                }

                p.WriteUInt64(pi.ui.pang);
            }
            return p.GetBytes;
        }

        public static List<PangyaBinaryWriter> pacote25D(List<TrofelEspecialInfo> v_element, int option)
        {
            var responses = new List<PangyaBinaryWriter>();
            int elements = v_element.Count;
            int itensPorPacote = 20;

            // Divide a lista apenas se necessário
            var splitList = (elements * 200 < (MAX_BUFFER_PACKET - 100))
                ? new List<List<TrofelEspecialInfo>> { v_element } // Envia tudo em um pacote
                : v_element.Select((item, index) => new { item, index })
                           .GroupBy(x => x.index / itensPorPacote)
                           .Select(g => g.Select(x => x.item).ToList())
                           .ToList();

            // Gera pacotes corretamente
            foreach (var lista in splitList)
            {
                var p = new PangyaBinaryWriter(0x25D);
                p.WriteByte((byte)option);
                p.WriteUInt32((uint)lista.Count);
                p.WriteUInt32((uint)lista.Count);

                foreach (var item in lista)
                {
                    p.WriteBytes(item.ToArray());
                }

                responses.Add(p);
            }

            return responses;
        }
        public static byte[] pacote156(uint _uid, UserEquip _ue, byte season)
        {
            var p = new PangyaBinaryWriter(0x156);

            p.WriteByte(season);

            p.WriteUInt32(_uid);
            p.WriteBytes(_ue.ToArray());
            return p.GetBytes;
        }


        public static byte[] pacote157(MemberInfoEx _mi, byte season)
        {
            var p = new PangyaBinaryWriter(0x157);

            p.WriteByte(season);

            p.WriteUInt32(_mi.uid);
            p.WriteUInt16(_mi.sala_numero);
            p.WriteBytes(_mi.ToArray());
            p.WriteUInt32(_mi.uid);
            p.WriteUInt32(_mi.guild_point);
            return p.GetBytes;
        }

        public static byte[] pacote158(uint _uid, UserInfoEx _ui, byte season)
        {
            var p = new PangyaBinaryWriter(0x158);

            p.WriteByte((byte)season);
            p.WriteUInt32(_uid);
            p.WriteBytes(_ui.ToArray());//new version 
            return p.GetBytes;
        }

        public static byte[] pacote159(uint uid, TrofelInfo ti, byte season)
        {
            var p = new PangyaBinaryWriter(0x159);
            p.WriteByte(season);
            p.WriteUInt32(uid);
            p.WriteBytes(ti.ToArray());
            return p.GetBytes;
        }

        public static byte[] pacote15A(uint uid, List<TrofelEspecialInfo> vTei, byte season)
        {
            var p = new PangyaBinaryWriter(0x15A);
            p.WriteByte(season);
            p.WriteUInt32(uid);
            p.WriteUInt16((ushort)vTei.Count);

            foreach (var item in vTei)
                p.WriteBytes(item.ToArray());

            return p.GetBytes;
        }

        public static byte[] pacote15B(uint uid, byte season)
        {
            var p = new PangyaBinaryWriter(0x15B);
            p.WriteByte(season);
            p.WriteUInt32(uid);
            p.WriteInt16(0); // Count desconhecido
            return p.GetBytes;
        }

        public static byte[] pacote15C(uint uid, List<MapStatisticsEx> vMs, List<MapStatisticsEx> vMsa, byte season)
        {
            var p = new PangyaBinaryWriter(0x15C);
            p.WriteByte(season);
            p.WriteUInt32(uid);
            p.WriteInt32(vMs.Count);

            foreach (var item in vMs)
                p.WriteBytes(item.ToArray());

            p.WriteInt32(vMsa.Count);

            foreach (var item in vMsa)
                p.WriteBytes(item.ToArray());

            return p.GetBytes;
        }

        public static byte[] pacote15D(uint uid, GuildInfo gi)
        {
            var p = new PangyaBinaryWriter(0x15D);
            p.WriteUInt32(uid);
            p.WriteBytes(gi.ToArray());
            return p.GetBytes;
        }

        public static byte[] pacote15E(uint uid, CharacterInfo ci)
        {
            var p = new PangyaBinaryWriter(0x15E);
            p.WriteUInt32(uid);
            p.WriteBytes(ci.ToArray());
            return p.GetBytes;
        }

        public static byte[] pacote096(PlayerInfo pi)
        {
            if (pi == null)
                throw new exception("Erro PlayerInfo *pi is null. packet_func::pacote096()");
            using (var p = new PangyaBinaryWriter(0x96))
            {
                p.WriteUInt64(pi.cookie);
                return p.GetBytes;
            }
        }

        public static byte[] pacote181(List<ItemBuffEx> v_element, int option = 0)
        {
            using (var p = new PangyaBinaryWriter(0x181))
            {
                p.WriteInt32(option);

                if (option == 0)
                {
                    p.WriteByte(v_element.Count());
                    for (int i = 0; i < v_element.Count; i++)
                        p.WriteBytes(v_element[i].ToArray());

                }
                else if (option == 2)
                {
                    p.WriteUInt32((uint)v_element.Count);

                    for (int i = 0; i < v_element.Count; i++)
                    {
                        p.WriteUInt32(v_element[i]._typeid);
                        p.WriteBytes(v_element[i].ToArray());

                    }
                }
                else
                    p.WriteByte(0);

                return p.GetBytes;
            }
        }

        public static byte[] pacote13F(int option = 0)
        {
            using (var p = new PangyaBinaryWriter(0x13F))
            {
                p.WriteByte(option);
                return p.GetBytes;
            }
        }


        public static byte[] pacote136()
        {
            using (var p = new PangyaBinaryWriter(0x136))
            {
                return p.GetBytes;
            }
        }

        public static byte[] pacote137(CardEquipManager v_element)
        {
            using (var p = new PangyaBinaryWriter())
            {
                p.init_plain(0x137);

                p.WriteUInt16((short)v_element.Count());
                foreach (var CardEquip in v_element)
                {
                    var array = CardEquip.ToArray();
                    p.WriteBytes(array);
                } 
                return p.GetBytes;
            }
        }
        public static byte[] pacote138(CardManager v_element, int option = 0)
        {
            using (var p = new PangyaBinaryWriter())
            {
                p.init_plain(0x138);
                p.WriteInt32(option);
                p.WriteUInt16((ushort)v_element.Count);
                foreach (var Card in v_element.Values)
                    p.WriteBytes(Card.ToArray());

                return p.GetBytes;
            }
        }

        public static byte[] pacote1F()
        {
            using (var p = new PangyaBinaryWriter(0x01F))
            {
                return p.GetBytes;
            }
        }

        public static byte[] pacote131(int option = 1)
        {
            using (var p = new PangyaBinaryWriter(0x131))
            {


                if (!sTreasureHunterSystem.getInstance().isLoad())
                    sTreasureHunterSystem.getInstance().load();

                p.WriteByte(option);

                p.WriteByte(MS_NUM_MAPS);
                var _TreasureHunterInfo = sTreasureHunterSystem.getInstance().getAllCoursePoint();
                for (int i = 0; i < _TreasureHunterInfo.Count(); i++)
                {
                    p.WriteBytes(_TreasureHunterInfo[i].ToArray());
                }

                return p.GetBytes;
            }
        }

        public static byte[] pacote072(UserEquip ue)
        {
            var p = new PangyaBinaryWriter();

            p.init_plain(0x72);
            p.WriteBytes(ue.ToArray());
            return p.GetBytes;
        }

        public static byte[] pacote0E1(MascotManager v_element, int option = 0)
        {
            var p = new PangyaBinaryWriter(0xE1);

            p.Write(v_element.Build());
            return p.GetBytes;
        }

        public static byte[] pacote0E2()
        {
            return new byte[] { 0x0E, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
        }

        public static PangyaBinaryWriter pacote21E(List<AchievementInfoEx> v_element, int option = 0)
        {
            var p = new PangyaBinaryWriter();
            try
            {
                p.init_plain((ushort)0x21E);
                p.WriteUInt32(0); // SUCCESS    
                p.WriteUInt32((uint)v_element.Count);
                p.WriteUInt32((uint)v_element.Count);
                foreach (var ai in v_element)
                {
                    p.WriteByte(ai.active);
                    p.WriteUInt32(ai._typeid);
                    p.WriteInt32(ai.id);
                    p.WriteUInt32(ai.status);
                    p.WriteUInt32((uint)ai.v_qsi.Count);

                    foreach (var qsi in ai.v_qsi)
                    {
                        CounterItemInfo cii = null;

                        p.WriteUInt32(qsi._typeid);

                        if (qsi.counter_item_id > 0 && (cii = ai.FindCounterItemById((uint)qsi.counter_item_id)) != null)
                        {
                            p.WriteUInt32(cii._typeid);
                            p.WriteInt32(cii.id);
                        }
                        else
                        {
                            p.WriteZero(8);
                        }

                        p.WriteUInt32(qsi.clear_date_unix);
                    }
                }
                return p;
            }
            catch
            {
                return p;
            }
        }

        public static PangyaBinaryWriter pacote21D(List<CounterItemInfo> v_element, int option = 0)
        {
            var p = new PangyaBinaryWriter();
            try
            {
                p.init_plain((ushort)0x21D);
                p.WriteUInt32(0); // SUCCESS    
                p.WriteUInt32((uint)v_element.Count);
                p.WriteUInt32((uint)v_element.Count);
                foreach (var counter in v_element)
                {
                    p.WriteByte(counter.active);//;
                    p.WriteUInt32(counter._typeid);//
                    p.WriteInt32(counter.id);//
                    p.WriteUInt32(counter.value);//
                }
                return p;
            }
            catch
            { 
                return p;
            }
        }

        public static PangyaBinaryWriter pacote22D(List<AchievementInfoEx> v_element, int option = 0)
        {
            var p = new PangyaBinaryWriter();
            try
            {
                p.init_plain((ushort)0x22D);
                p.WriteUInt32(0); // SUCCESS
                p.WriteUInt32((uint)v_element.Count());
                p.WriteUInt32((uint)v_element.Count());

                foreach (var ai in v_element)
                {
                    p.WriteUInt32(ai._typeid);
                    p.WriteInt32(ai.id);
                    p.WriteUInt32((uint)ai.v_qsi.Count);
                    CounterItemInfo cii = null;
                    foreach (var qsi in ai.v_qsi)
                    {
                        p.WriteUInt32(qsi._typeid);
                        p.WriteUInt32(qsi.counter_item_id > 0 && (cii = ai.FindCounterItemById((uint)qsi.counter_item_id)) != null ? cii.value : 0);
                        p.WriteUInt32(qsi.clear_date_unix);
                    }
                }
                return p;
            }
            catch
            {
                return p;
            }
        }
        public static PangyaBinaryWriter pacote22C(int option = 0)
        {
            var p = new PangyaBinaryWriter();
            try
            {
                p.init_plain(0x22C);
                p.WriteInt32(option); // SUCCESS
                
                return p;
            }
            catch
            {
                return p;
            }
        }

        public static PangyaBinaryWriter pacote073(List<WarehouseItemEx> v_element, int option = 0)
        {
            var p = new PangyaBinaryWriter();
            try
            {
                p.init_plain(0x73);
                p.WriteUInt16((short)v_element.Count);
                p.WriteUInt16((short)v_element.Count);
                foreach (var item in v_element)
                {
                    p.WriteBytes(item.ToArray());
                }
                return p;
            }
            catch
            {
                if (p.GetSize == 2)
                {
                    p.WriteUInt16(0);
                    p.WriteUInt16(0);
                } 
                return p;
            }
        }

        public static byte[] pacote071(CaddieManager v_element, int option = 0)
        {
            var p = new PangyaBinaryWriter();
            try
            {
                p.init_plain(0x71);
                p.WriteInt16((short)v_element.Count);
                p.WriteInt16((short)v_element.Count);
                foreach (var char_info in v_element.Values)
                {
                    p.WriteBytes(char_info.getInfo().ToArray());
                }
                return p.GetBytes;
            }
            catch (Exception)
            {
                return p.GetBytes;
            }
        }

        /// <summary>
        /// Send Packet for Info Characters(Personagens)
        /// </summary>
        /// <param name="v_element">object list</param>
        /// <param name="option">what?</param>
        /// <returns>obj using for write data</returns>
        public static byte[] pacote070(CharacterManager v_element, int option = 0)
        {
            var p = new PangyaBinaryWriter();
            try
            {
                p.init_plain(0x70);
                p.WriteInt16((short)v_element.Count);
                p.WriteInt16((short)v_element.Count);
                foreach (var char_info in v_element.Values)
                {
                    p.WriteBytes(char_info.ToArray());
                }
                return p.GetBytes;
            }
            catch (Exception)
            {
                return p.GetBytes;
            }
        }

        /// <summary>
        /// packet 9D use channel list!
        /// </summary>
        /// <param name="v_element"></param>
        /// <param name="build_s">true is server, false is chanell call!</param>
        /// <returns></returns>
        public static byte[] pacote04D(List<Channel> v_element, bool build_s = false)
        {
            try
            {
                using (var p = new PangyaBinaryWriter())
                {
                    if (!build_s)
                        p.init_plain(0x4D); //channel list!         

                    p.WriteByte(v_element.Count);
                    foreach (var channel in v_element)
                        p.WriteBytes(channel.getInfo().ToArray());

                    return p.GetBytes;
                }
            }
            catch (exception ex)
            {
                _smp.message_pool.push(new message(
              $"[packet_func::pacote04D][ErrorSystem] {ex.getFullMessageError()}",
              type_msg.CL_FILE_LOG_AND_CONSOLE));
                return new byte[] { 0x4D, 0x00, 0x00 };
            }
        }

        public static byte[] pacote248(
            AttendanceRewardInfo ari,
            int option = 0)
        {
            using (var p = new PangyaBinaryWriter())
            {
                p.Write(new byte[] { 0x48, 0x02 });
                p.WriteInt32(option);
                p.WriteBytes(ari.ToArray());
                return p.GetBytes;
            }
        }

        public static byte[] pacote249(
            AttendanceRewardInfo ari,
            int option = 0)
        {
            using (var p = new PangyaBinaryWriter())
            {
                p.Write(new byte[] { 0x49, 0x02 });
                p.WriteInt32(option);
                p.WriteBytes(ari.ToArray());
                return p.GetBytes;
            }
        }

        public static byte[] pacote257(uint _uid, List<TrofelEspecialInfo> v_tegi, byte season)
        {
            using (var p = new PangyaBinaryWriter())
            {
                p.init_plain(0x257);

                p.WriteByte(season);
                p.WriteUInt32(_uid);

                p.WriteInt16((short)v_tegi.Count);
                foreach (var item in v_tegi)
                    p.WriteBytes(item.ToArray());
                return p.GetBytes;
            }
        }

        public static byte[] pacote04E(int option, int _codeErrorInfo = 0)
        {
            /* Option Values
                * 1 Sucesso
                * 2 Channel Full
                * 3 Nao encontrou canal
                * 4 Nao conseguiu pegar informções do canal
                * 6 ErrorCode Info
                */
            using (var p = new PangyaBinaryWriter(0x4E))
            {
                p.WriteByte((byte)option);

                if (_codeErrorInfo != 0)
                    p.WriteInt32(_codeErrorInfo);
                return p.GetBytes;
            }
        }


        public static byte[] pacote040(string nick, string msg, byte option)
        {

            if ((option == 0 || option == 0x80) && string.IsNullOrEmpty(nick))
                throw new exception("Error PlayerInfo *pi is null. packet_func::pacote040()");

            using (var p = new PangyaBinaryWriter(0x40))
            {
                p.WriteByte(option);

                if (option == 0 || option == 0x80 || option == 4)
                {
                    p.WritePStr(nick);
                    if (option != 4)
                        p.WritePStr(msg);
                }
                return p.GetBytes;
            }
        }

        public static byte[] pacote044(ServerInfoEx _si, int option, PlayerInfo pi = null, int valor = 0)
        {
            var p = new PangyaBinaryWriter(0x44);

            if (option == 0 && pi == null)
                throw new exception("Erro PlayerInfo *pi is null. packet_func::pacote044()");

            p.WriteByte(option);   // Option

            switch (option)
            {
                case 1:
                    p.WriteByte(0);
                    break;
                case 0xD3:
                    p.WriteByte(0);
                    break;
                case 0xD2:
                    p.WriteInt32(valor);
                    break;
                default:
                    if (option == 0 && principal(ref p, pi, _si) == 1)
                    { }
                    break;
            }
            return p.GetBytes;
        }

        public static byte[] pacote0B2(

List<MsgOffInfo> v_element,
int option = 0)
        {
            var p = new PangyaBinaryWriter();

            p.init_plain((ushort)0xB2);

            p.WriteInt32(2); // Não sei bem o que é, mas pode ser uma opção

            p.WriteInt32(option);

            p.WriteUInt32((uint)v_element.Count);

            foreach (MsgOffInfo i in v_element)
            {
                p.WriteBytes(i.ToArray());
            }

            return p.GetBytes;
        }

        public static byte[] pacote0D4(CaddieManager v_element)
        {
            using (var p = new PangyaBinaryWriter())
            {
                p.init_plain(0xD4);
                p.WriteUInt32((uint)v_element.Count());
                foreach (var item in v_element.Values)
                    p.WriteBytes(item.getInfo().ToArray());

                return p.GetBytes;
            }
        }

        // Metôdos de auxílio de criação de pacotes


        public static byte[] pacote210(

                List<MailBox> v_element,
                int option = 0)
        {
            var p = new PangyaBinaryWriter();

            p.init_plain((ushort)0x210);

            p.WriteInt32(option);

            p.WriteInt32(v_element.Count);

            for (var i = 0; i < v_element.Count; ++i)
            {
                p.WriteBytes(v_element[i].ToArray());
            }

            return p.GetBytes;
        }



        public static byte[] pacote211(
            List<MailBox> v_element,
            int pagina,
            int paginas, int error = 0)
        {
            var p = new PangyaBinaryWriter();

            p.init_plain((ushort)0x211);

            p.WriteInt32(error);

            if (error == 0)
            {
                p.WriteInt32(pagina);
                p.WriteInt32(paginas);
                p.WriteInt32(v_element.Count);

                for (var i = 0; i < v_element.Count; ++i)
                {
                    p.WriteBytes(v_element[i].ToArray());
                }
            }

            return p.GetBytes;
        }

        public static byte[] pacote214(int error = 0)
        {

            var p = new PangyaBinaryWriter();
            p.init_plain((ushort)0x214);

            p.WriteInt32(error);

            return p.GetBytes;
        }

        public static byte[] pacote215(
            List<MailBox> v_element,
            int pagina,
            int paginas, int error = 0)
        {
            var p = new PangyaBinaryWriter();

            p.init_plain((ushort)0x215);

            p.WriteInt32(error);

            if (error == 0)
            {
                p.WriteInt32(pagina);
                p.WriteInt32(paginas);
                p.WriteUInt32((uint)v_element.Count);

                for (var i = 0; i < v_element.Count; ++i)
                {
                    p.WriteBytes(v_element[i].ToArray());
                }
            }

            return p.GetBytes;
        }

        public static byte[] pacote216(
            List<stItem> v_item,
            int option = 0)
        {

            var p = new PangyaBinaryWriter();
            p.init_plain((ushort)0x216);

            p.WriteInt32((int)UtilTime.GetSystemTimeAsUnix());

            if (v_item.Count > 0)
            {
                p.WriteInt32(v_item.Count);

                foreach (stItem i in v_item)
                {

                    // Begin Base Item
                    p.WriteByte(i.type);
                    p.WriteUInt32(i._typeid);
                    p.WriteInt32(i.id);
                    p.WriteUInt32(i.flag_time);
                    p.WriteBytes(i.stat.ToArray());
                    p.WriteUInt32((i.flag_time == 0) ? i.c[0] : i.c[3]);
                    p.WriteUInt16(i.c);
                    // End Base Item

                    if (i.type == 2)
                    {
                        try
                        {
                            p.WriteString(i.ucc.IDX);
                        }
                        catch (exception e)
                        {
                            if (ExceptionError.STDA_SOURCE_ERROR_DECODE_TYPE(e.getCodeError()) == STDA_ERROR_TYPE.PACKET && ExceptionError.STDA_ERROR_DECODE(e.getCodeError()) == 3)
                            {
                                p.WriteInt16(0);
                            }
                            else
                            {
                                throw;
                            }
                        }

                        p.WriteUInt32(i.ucc.status);
                        p.WriteUInt32(i.ucc.seq);
                        p.WriteZeroByte(5); // É o Unknown de cima
                    }
                }
            }
            else
            {
                p.WriteInt32(option);
            }

            return p.GetBytes;
        }

        public static byte[] pacote10E(Last5PlayersGame l5pg)
        {
            var p = new PangyaBinaryWriter(0x10E);
            foreach (var p_log in l5pg.players)
            {
                p.WriteBytes(p_log.ToArray());
            }
            return p.GetBytes;
        }
        public static byte[] pacote0FC(List<ServerInfo> v_si)
        {
            var p = new PangyaBinaryWriter(0xFC);
            p.WriteByte((byte)v_si.Count);

            foreach (ServerInfo i in v_si)
                p.WriteBytes(i.ToArray());

            return p.GetBytes;
        }



        public static byte[] pacote101(int option = 0)
        {
            var p = new PangyaBinaryWriter();

            p.init_plain((ushort)0x101);

            p.WriteByte((byte)option);
            return p.GetBytes;
        }
        public static byte[] pacote0B4(

List<TrofelEspecialInfo> v_element,
int option = 0)
        {
            var p = new PangyaBinaryWriter();

            p.init_plain((ushort)0xB4);

            p.WriteInt16((short)option);

            p.WriteByte((byte)v_element.Count);

            foreach (TrofelEspecialInfo i in v_element)
            {
                p.Write(i.id);
                p.Write(i._typeid);
                p.Write(i.qntd); 
            }

            return p.GetBytes;
        }

        public static byte[] pacote0F1(int option = 0)
        {
            var p = new PangyaBinaryWriter();

            p.init_plain((ushort)0xF1);

            p.WriteByte((byte)option);

            return p.GetBytes;
        }


        public static byte[] pacote0F5()
        {
            var p = new PangyaBinaryWriter(0x0F5);
            return p.GetBytes;
        }


        public static byte[] pacote0F6()
        {
            var p = new PangyaBinaryWriter(0x0F6);
            return p.GetBytes;
        }


        public static byte[] pacote169(
           TrofelInfo ti,
            int option = 0)
        {
            var p = new PangyaBinaryWriter();
            p.init_plain((ushort)0x169);

            p.WriteByte((byte)option);

            p.WriteBytes(ti.ToArray());

            return p.GetBytes;
        }

        public static byte[] pacote09F(List<ServerInfo> v_server, List<Channel> v_channel)
        {
            using (var p = new PangyaBinaryWriter((ushort)0x9F))
            {
                p.WriteByte((byte)v_server.Count);

                for (var i = 0; i < v_server.Count; ++i)
                {
                    p.WriteBytes(v_server[i].ToArray());
                }
                p.WriteBytes(pacote04D(v_channel, true));
                return p.GetBytes;
            }
        }

        public static byte[] pacote089(uint _uid = 0, byte season = 0, uint err_code = 1)
        {

            using (var p = new PangyaBinaryWriter((ushort)0x89))
            {
                p.WriteUInt32(err_code);
                if (err_code > 0)
                {
                    p.WriteByte(season);
                    p.WriteUInt32(_uid);
                }
                return p.GetBytes;
            }
        }

        public static byte[] pacote211(List<MailBox> v_element, uint pagina, uint paginas, uint error = 0)
        {

            using (var p = new PangyaBinaryWriter(0x211))
            {
                p.WriteUInt32(error);

                if (error == 0)
                {
                    p.WriteUInt32(pagina);
                    p.WriteUInt32(paginas);
                    p.WriteInt32(v_element.Count);

                    for (int i = 0; i < v_element.Count; ++i)
                    {
                        p.WriteBytes(v_element[i].ToArray());
                    }
                }

                return p.GetBytes;
            }
        }

        public static byte[] pacote212(EmailInfo ei, uint error = 0)
        {

            using (var p = new PangyaBinaryWriter(0x212))
            {
                p.WriteUInt32(error);

                if (error == 0)
                {
                    p.WriteBytes(ei.ToArray());
                }

                return p.GetBytes;
            }
        }


        public static byte[] pacote06B(PlayerInfo pi, byte type, int err_code = 4)
        {

            if (pi == null)
            {
                throw new exception("Erro PlayerInfo *pi is null. packet_func::pacote06B()", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PACKET_FUNC_SV,
                    1, 0));
            }
            var p = new PangyaBinaryWriter(0x6B);

            p.WriteByte(err_code); // Error Code, 4 Sucesso, diferente é erro
            p.WriteByte(type);

            if (err_code == 4)
            {
                switch (type)
                {
                    case 0: // Character Equipado Com os Parts Equipado
                        if (pi.ei.char_info != null)
                        {
                            p.WriteBytes(pi.ei.char_info.ToArray());//, Marshal.SizeOf(new CharacterInfo));
                        }
                        else
                        {
                            p.WriteZero(513);
                        }
                        break;
                    case 1: // Caddie Equipado
                        if (pi.ei.cad_info != null)
                        {
                            p.WriteInt32(pi.ei.cad_info.id);
                        }
                        else
                        {
                            p.WriteZero(4);
                        }
                        break;
                    case 2: // Itens Equipáveis
                        p.WriteUInt32(pi.ue.item_slot);//, Marshal.SizeOf(new pi.ue.item_slot));
                        break;
                    case 3: // Ball e Clubset Equipado
                        if (pi.ei.comet != null) // Ball
                        {
                            p.WriteUInt32(pi.ei.comet._typeid);
                        }
                        else
                        {
                            p.WriteZero(4);
                        }
                        p.WriteInt32(pi.ei.csi.id); // ClubSet ID
                        break;
                    case 4: // Skins
                        p.WriteUInt32(pi.ue.skin_typeid);//, Marshal.SizeOf(new pi.ue.skin_typeid));
                        break;
                    case 5: // Only Chracter Equipado
                        if (pi.ei.char_info != null)
                        {
                            p.WriteInt32(pi.ei.char_info.id);
                        }
                        else
                        {
                            p.WriteZero(4);
                        }
                        break;
                    case 8: // Mascot Equipado
                        if (pi.ei.mascot_info != null)
                        {
                            p.WriteBytes(pi.ei.mascot_info.ToArray());
                        }
                        else
                        {
                            p.WriteZero(62);
                        }
                        break;
                    case 9: // Character Cutin Equipado
                        if (pi.ei.char_info != null)
                        {
                            p.WriteInt32(pi.ei.char_info.id);
                            p.WriteUInt32(pi.ei.char_info.cut_in);//, Marshal.SizeOf(new pi.ei.char_info.cut_in));
                        }
                        else
                        {
                            p.WriteZero(20);
                        }
                        break;
                    case 10: // Poster Equipado
                        p.WriteUInt32(pi.ue.poster);//, Marshal.SizeOf(new pi.ue.poster));
                        break;
                }
            }

            return p.GetBytes;
        }

        public static byte[] pacote1D4(string _AuthKeyLogin, int option = 0)
        {
            using (var p = new PangyaBinaryWriter(0x1D4))
            {
                p.WriteInt32(option);

                if (option == 0 && !string.IsNullOrEmpty(_AuthKeyLogin))
                    p.WritePStr(_AuthKeyLogin);

                return p.GetBytes;
            }
        }
        public static PangyaBinaryWriter pacote04B(Player _session, byte _type,
         int error = 0, int _valor = 0)
        {

            var p = new PangyaBinaryWriter(0x4B);
            if (_session == null)
            {
                throw new exception("Error _session is null. Em packet_func::pacote04B()", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PACKET_FUNC_SV,
                    1, 0));
            }

            if (!_session.getState())
            {
                throw new exception("Error player nao esta mais connectado. Em packet_func::pacote04B()", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PACKET_FUNC_SV,
                    2, 0));
            }
            p.WriteInt32(error);

            if (error == 0)
            {
                p.WriteByte(_type);

                p.WriteInt32(_session.m_oid);

                switch (_type)
                {
                    case 1: // Caddie
                        if (_session.m_pi.ei.cad_info != null)
                        {
                            p.WriteBytes(_session.m_pi.ei.cad_info.ToArray());
                        }
                        else
                        {
                            p.WriteZero(25);
                        }
                        break;
                    case 2: // Ball(Comet)
                        if (_session.m_pi.ei.comet != null)
                        {
                            p.WriteUInt32(_session.m_pi.ei.comet._typeid);
                        }
                        else
                        {
                            p.WriteZero(4);
                        }
                        break;
                    case 3: // ClubSet
                        p.WriteBytes(_session.m_pi.ei.csi.ToArray());
                        break;
                    case 4: // Character
                        if (_session.m_pi.ei.char_info != null)
                        {
                            p.WriteBytes(_session.m_pi.ei.char_info.ToArray());
                        }
                        else
                        {
                            p.WriteZero(513);
                        }
                        break;
                    case 5: // Mascot
                        if (_session.m_pi.ei.mascot_info != null)
                        {
                            p.WriteBytes(_session.m_pi.ei.mascot_info.ToArray());
                        }
                        else
                        {
                            p.WriteZero(62);
                        }
                        break;
                    case 6: // Itens Active 1 = Jester big cabeça, 2 = Hermes velocidade x2, 3 = Twilight Fogos na cabeça
                        {
                            p.WriteInt32(_valor);

                            //if (_valor == (int)ChangePlayerItemRoom.stItemEffectLounge.TYPE_EFFECT.TE_TWILIGHT)
                            //{
                            //    p.WriteInt32(1); // Ativa Fogos
                            //}
                            //else
                            //{

                            //    if (_session.m_pi.ei.char_info != null)
                            //    {
                            //        var it = (_session.m_pi.ei.char_info == null) ? _session.m_pi.mp_scl.end() : _session.m_pi.mp_scl.find(_session.m_pi.ei.char_info.id);

                            //        if (it == _session.m_pi.mp_scl.end())
                            //        {

                            //            _smp.message_pool.getInstance().push(new message("[channel::pacote04B][Error] player[UID=" + Convert.ToString(_session.m_pi.m_uid) + "] nao tem os estados do character na lounge. Criando um novo para ele. Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));

                            //            // Add New State Character Lounge
                            //            var pair = _session.m_pi.mp_scl.insert(Tuple.Create(_session.m_pi.ei.char_info.id, new StateCharacterLounge({ })));

                            //            it = pair.first;
                            //        }

                            //        switch (_valor)
                            //        {
                            //            case ChangePlayerItemRoom.stItemEffectLounge.TYPE_EFFECT.TE_BIG_HEAD: // Jester (Big head)
                            //                p.WriteFloat(it.second.scale_head);
                            //                break;
                            //            case ChangePlayerItemRoom.stItemEffectLounge.TYPE_EFFECT.TE_FAST_WALK: // Hermes (Velocidade x2)
                            //                p.WriteFloat(it.second.walk_speed);
                            //                break;
                            //        }
                            //    }
                        }
                        break;
                    case 7: // Player game
                            // Nada Aqui
                        break;
                    default:
                        throw new exception("Error tipo desconhecido. Em packet_func::pacote04B()", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PACKET_FUNC_SV,
                            3, 0));
                }
            }

            return p;
        }

        public static byte[] pacote1AD(string webKey, int option)
        {
            using (var p = new PangyaBinaryWriter(0x1AD))
            {
                p.WriteInt32(option);

                if (webKey.empty())
                    p.WriteInt16(0);
                else
                    p.WritePStr(webKey);

                return p.GetBytes;
            }
        }

        public static byte[] pacote102(PlayerInfo pi)
        {
            if (pi == null)
            {
                throw new exception("[packet_func::pacote12][Error] PlayerInfo *pi is null.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PACKET_FUNC_SV,
                    1, 0));
            }
            using (var p = new PangyaBinaryWriter(0x102))
            {
                p.WriteUInt32(pi.cg.normal_ticket);
                p.WriteUInt32(pi.cg.partial_ticket);

                p.WriteUInt64(pi.ui.pang);
                p.WriteUInt64(pi.cookie);


                return p.GetBytes;
            }
        }

        public static byte[] pacote144(int option = 0)
        {
            var p = new PangyaBinaryWriter(0x144);
            p.WriteByte((byte)option);

            return p.GetBytes;
        }

        public static byte[] pacote09A(int ulCapability)
        {        // UPDATE ON GAME
            var p = new PangyaBinaryWriter(0x9A);

            p.WriteInt32(ulCapability);
            return p.GetBytes;
        }

        //tested, melhorar com tempo@@@@
        public static bool pacote048(ref PangyaBinaryWriter p, Player _session, List<PlayerRoomInfoEx> v_element, int option = 0)
        {
            TPlayerRoom_Action opt = (TPlayerRoom_Action)(option & 0xFF);

            Debug.WriteLine($"pacote048 => enum: {opt}, code: {option & 0xFF}, code2: {option & 0x100}");
            
            try
            {

                if ((option & 0xFF) == 2)
                { // exit player
                    p.init_plain(0x48);
                    p.WriteSByte((sbyte)option);
                    p.WriteInt16(-1);
                    p.WriteInt32(_session.m_oid);
                    return true;
                }
                else if ((option & 0xFF) == 7)
                {
                    int elementSize = (option & 0x100) != 0 ? Marshal.SizeOf(new PlayerRoomInfo()) : Marshal.SizeOf(new PlayerRoomInfoEx());
                    int maxPacket = Marshal.SizeOf(new PlayerRoomInfoEx());
                    int total = v_element.Count;
                    int por_packet = (maxPacket - 100 > elementSize) ? (maxPacket - 100) / elementSize : 1;

                    int index = 0;

                    while (index < total)
                    {
                        p.init_plain(0x48);
                        p.WriteSByte((sbyte)option);
                        p.WriteInt16(-1);

                        if ((option & 0xFF) == 0 || (option & 0xFF) == 5)
                            p.WriteSByte((sbyte)Math.Min(por_packet, total - index));
                        else if ((option & 0xFF) == 7)
                            p.WriteSByte((sbyte)total);
                        else if ((option & 0xFF) == 3 || (option & 0xFF) == 3)
                        {
                            elementSize = 348;
                            p.WriteInt32(_session.m_oid);
                        }
                        for (int i = 0; i < por_packet && index < total; i++, index++)
                        {
                            var playerRoom = v_element[index];
                            if (elementSize == 348)
                            {
                                p.WriteBytes(playerRoom.ToArray());
                            }
                            else
                            {
                                p.WriteBytes(playerRoom.ToArrayEx());
                            }
                        }

                        p.WriteByte(0);     // Final list de PlayerRoomInfo

                        session_send(p, _session, 1);//-> MAKE_END_SPLIT_PACKET
                    }
                    return true;
                }
                else
                {
                    int elementSize = (option & 0x100) != 0 ? Marshal.SizeOf(new PlayerRoomInfo()) : Marshal.SizeOf(new PlayerRoomInfoEx());
                    int elements = v_element.Count;
                    int totalSize = elements * elementSize;

                    try
                    {
                        if (totalSize < MAX_BUFFER_PACKET - 100)//-> MAKE_END_SPLIT_PACKET nao tem, so no else, OK?
                        {
                            p.init_plain(0x48);
                            p.WriteSByte((sbyte)option);
                            p.WriteInt16(-1);

                            if ((option & 0xFF) == 0 || (option & 0xFF) == 5)
                                p.WriteByte((byte)elements);
                            else if ((option & 0xFF) == 3 || (option & 0xFF) == 3)
                            {
                                elementSize = 348;
                                p.WriteInt32(_session.m_oid);
                            }

                            foreach (var playerRoom in v_element)
                            {

                                if (elementSize == 348)
                                {
                                    p.WriteBytes(playerRoom.ToArray());
                                }
                                else
                                {
                                    p.WriteBytes(playerRoom.ToArrayEx());
                                }
                            }
                            p.WriteByte(0);
                            return true;
                        }
                        else
                        {
                            int total = elements;
                            int por_packet = ((MAX_BUFFER_PACKET - 100) > elementSize) ? (MAX_BUFFER_PACKET - 100) / elementSize : 1;

                            int index = 0;

                            while (index < total)
                            {
                                p.init_plain(0x48);

                                if ((option & 0xFF) == 0 && index != 0)
                                    p.WriteByte(5); // append players
                                else
                                    p.WriteByte((byte)option);

                                p.WriteInt16(-1);

                                if ((option & 0xFF) == 0 || (option & 0xFF) == 5)
                                    p.WriteSByte((sbyte)Math.Min(por_packet, total - index));
                                else if ((option & 0xFF) == 3)
                                {
                                    elementSize = 348;
                                    p.WriteInt32(_session.m_oid);
                                }

                                for (int i = 0; i < por_packet && index < total; i++, index++)
                                {
                                    var playerRoom = v_element[index];
                                    if (elementSize == 348)
                                    {
                                        p.WriteBytes(playerRoom.ToArray());
                                    }
                                    else
                                    {
                                        p.WriteBytes(playerRoom.ToArrayEx());
                                    }
                                }

                                p.WriteByte(0); // Final list de PlayerRoomInfo

                                session_send(p, _session, 1);//-> MAKE_END_SPLIT_PACKET
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("[pacote048][Fatal] " + ex);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("[pacote048][Fatal] " + ex);
            }
            return false;
        }

        public static PangyaBinaryWriter pacote04A(RoomInfoEx _ri, short option)
        {
            var p = new PangyaBinaryWriter();
            p.init_plain(0x4A);

            p.WriteInt16(option);      // pode ser valor constante da sala ou o número, ainda não descobri, sempre passa -1 des vezes que vi

            // Tem que ser o tipo_show, por que ele é o que o cliente quer,
            // o tipo(real) só server conhece para poder fazer o jogo direito 
            p.WriteByte(_ri.tipo_show);
            p.WriteByte((byte)_ri.course);
            p.WriteByte(_ri.qntd_hole);
            p.WriteByte(_ri.modo); 
            if (_ri.getModo() == RoomInfo.eMODO.M_REPEAT)
            {
                p.WriteByte(_ri.hole_repeat);
                p.WriteUInt32(_ri.fixed_hole);
            } 
            p.WriteUInt32(_ri.natural.ulNaturalAndShortGame);
            p.WriteByte(_ri.max_player);
            p.WriteByte(_ri._30s);        // constante 30 de pangya
            p.WriteSByte((sbyte)_ri.state_flag);
            p.WriteUInt32(_ri.time_vs);
            p.WriteUInt32(_ri.time_30s);
            p.WriteUInt32(_ri.trofel); 
            p.WriteByte(_ri.senha_flag); // Senha Flag
            if (_ri.senha.Length > 0)
            {
                p.WritePStr(_ri.senha);
            }
            p.WriteString(_ri.nome); 
            return p;
        }

        public static byte[] pacote049(room _room, TGAME_CREATE_RESULT option = 0)
        {
            try
            {
                if (_room == null)
                    throw new exception("Error _room is null. EM packet_func::pacote049()", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PACKET_FUNC_SV, 3, 0));

                var p = new PangyaBinaryWriter();

                p.init_plain(0x49);
                if (option == 0)//sucess
                {
                    p.WriteInt16((short)option);
                    p.WriteBytes(_room.getInfo().ToArray());
                }
                else
                    p.WriteByte((byte)option);//write error code packet 
                return p.GetBytes;
            }
            catch (Exception e)
            {

                throw e;
            }
        }

        public static byte[] pacote225(DailyQuestInfoUser _dq,
            List<RemoveDailyQuestUser> _delete_quest,
            int option = 0)
        {
            var p = new PangyaBinaryWriter();
            p.init_plain((ushort)0x225);

            p.WriteInt32(option);

            if (option == 0)
            { 
                // Convert to UTC send to client
                p.WriteInt32((int)UtilTime.TzLocalUnixToUnixUTC(_dq.current_date));
                p.WriteInt32((int)UtilTime.TzLocalUnixToUnixUTC(_dq.accept_date));

                p.WriteUInt32(_dq.count);
                p.WriteUInt32(_dq._typeid);

                p.WriteInt32(_delete_quest.Count);

                foreach (RemoveDailyQuestUser it in _delete_quest)
                {
                    p.WriteInt32(it.id);
                }   
            }

            return p.GetBytes;
        }

        public static byte[] pacote226(List<AchievementInfoEx> v_element, int option = 0)
        {
            var p = new PangyaBinaryWriter();
            p.init_plain((ushort)0x226);

            p.WriteInt32(option);

            if (option == 0)
            {
                if (v_element.Count > 0)
                {
                    CounterItemInfo cii = null;

                    p.WriteInt32(v_element.Count);

                    foreach (AchievementInfoEx i in v_element)
                    {
                        p.WriteByte(i.active);
                        p.WriteUInt32(i._typeid);
                        p.WriteInt32(i.id);
                        p.WriteUInt32(i.status);
                        p.WriteUInt32((uint)i.v_qsi.Count);
                        foreach (var ii in i.v_qsi) 
                        {
                            p.WriteUInt32(ii._typeid);

                            if (ii.counter_item_id > 0 && (cii = i.FindCounterItemById((uint)ii.counter_item_id)) != null)
                            {
                                p.WriteUInt32(cii._typeid);
                                p.WriteInt32(cii.id);
                            }
                            else // não tem o counter id e nem o typeid
                            {
                                p.WriteZeroByte(8);
                            }

                            p.WriteUInt32(ii.clear_date_unix);
                        }
                    }
                }
            }
            else
            {
                p.WriteInt32(0);
            }

            return p.GetBytes;
        }

        public static byte[] pacote227(List<AchievementInfoEx> v_element,
            int option = 0)
        {

            var p = new PangyaBinaryWriter();
            p.init_plain((ushort)0x227);

            p.WriteInt32(option);

            if (v_element.Count > 0)
            {

                p.WriteInt32(v_element.Count);

                foreach (var el in v_element)
                {
                    p.WriteInt32(el.id);
                }
            }
            else
            {
                p.WriteInt32(0);
            }

            return p.GetBytes;
        }

        public static byte[] pacote228(List<AchievementInfoEx> v_element, int option = 0)
        {

            var p = new PangyaBinaryWriter();
            p.init_plain((ushort)0x228);

            p.WriteInt32(option);

            if (option == 0)
            {
                if (v_element.Count > 0)
                {
                    p.WriteInt32(v_element.Count);

                    foreach (var el in v_element)
                    {
                        p.WriteInt32(el.id);
                    }
                }
            }

            return p.GetBytes;
        }

        // Metôdos de auxílio de criação de pacotes 
        public static void channel_broadcast(Channel _channel, byte[] p, int _debug = 1)
        {
            try
            {
                var channel_session = _channel.getSessions();
                for (var i = 0; i < channel_session.Count; ++i)
                    MAKE_SEND_BUFFER(p, channel_session[i]);
            }
            catch (Exception e)
            {
                message_pool.push(new message("[channel_broadcast(byte[])] Exception: " + e.ToString(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
        }

        public static void channel_broadcast(Channel _channel, PangyaBinaryWriter p, int _debug = 1)
        {
            try
            {
                var channel_session = _channel.getSessions();
                for (var i = 0; i < channel_session.Count; ++i)
                    MAKE_SEND_BUFFER(p.GetBytes, (Player)channel_session[i]);
            }
            catch (Exception e)
            {
                message_pool.push(new message("[channel_broadcast(byte[])] Exception: " + e.ToString(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
        }

        public static void channel_broadcast(Channel _channel, List<byte[]> v_p, int _debug = 1)
        {
            try
            {
                for (var i = 0; i < v_p.Count; ++i)
                {
                    if (v_p[i] != null)
                    {
                        var channel_session = _channel.getSessions();
                        for (int ii = 0; ii < channel_session.Count; ii++)
                            MAKE_SEND_BUFFER(v_p[i], channel_session[ii]);
                    }
                }
            }
            catch (Exception e)
            {
                message_pool.push(new message("[channel_broadcast(List<byte[]>)] Exception: " + e.ToString(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
        }

        public static void channel_broadcast(Channel _channel, List<PangyaBinaryWriter> v_p, int _debug = 1)
        {
            try
            {
                for (int i = 0; i < v_p.Count; ++i)
                {
                    var writer = v_p[i];
                    if (writer != null)
                    {
                        var channel_session = _channel.getSessions();
                        for (int ii = 0; ii < channel_session.Count; ii++)
                            MAKE_SEND_BUFFER(writer.GetBytes, channel_session[ii]);
                    }
                }
            }
            catch (Exception e)
            {
                message_pool.push(new message("[channel_broadcast(List<PangyaBinaryWriter>)] Exception: " + e.ToString(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
        }

        public static void lobby_broadcast(Channel _channel, byte[] p, int _debug = 1)
        {
            try
            {
                var channel_session = _channel.getSessions();
                for (var i = 0; i < channel_session.Count; ++i)
                {
                    if (channel_session[i].m_pi.mi.sala_numero == ushort.MaxValue)
                        MAKE_SEND_BUFFER(p, channel_session[i]);
                }
            }
            catch (Exception e)
            {
                message_pool.push(new message("[lobby_broadcast] Exception: " + e.ToString(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
        }

        public static void room_broadcast(room _room, PangyaBinaryWriter p, int _debug = 1)
        {
            try
            {
                var room_session = _room.getSessions(null, false/*without invited*/);
                for (var i = 0; i < room_session.Count; ++i)
                {
                    if (room_session[i] != null)
                    {
                        MAKE_SEND_BUFFER(p.GetBytes, room_session[i]);
                    }
                }
            }
            catch (Exception e)
            {
                message_pool.push(new message("[room_broadcast(writer)] Exception: " + e.ToString(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
        }

        public static void room_broadcast(room _room, byte[] p, int _debug = 1)
        {
            try
            {
                var room_session = _room.getSessions(null, false/*without invited*/);
                for (var i = 0; i < room_session.Count; ++i)
                {
                    if (room_session[i] != null)
                    {
                        MAKE_SEND_BUFFER(p, room_session[i]);
                    }
                }
            }
            catch (Exception e)
            {
                message_pool.push(new message("[room_broadcast(writer)] Exception: " + e.ToString(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
        }

        public static void room_broadcast(room _room, List<PangyaBinaryWriter> v_p, int _debug = 1)
        {
            try
            {
                for (var i = 0; i < v_p.Count; ++i)
                {
                    if (v_p[i] != null)
                    {
                        var room_session = _room.getSessions();
                        for (var ii = 0; ii < room_session.Count; ++ii)
                            MAKE_SEND_BUFFER(v_p[i].GetBytes, room_session[ii]);
                    }
                }
            }
            catch (Exception e)
            {
                message_pool.push(new message("[room_broadcast(List)] Exception: " + e.ToString(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
        }

        public static void game_broadcast(Game.GameBase _game, byte[] p, int _debug = 1)
        {
            try
            {
                var game_session = _game.getSessions();
                for (var i = 0; i < game_session.Count; ++i)
                    MAKE_SEND_BUFFER(p, game_session[i]);
            }
            catch (Exception e)
            {
                message_pool.push(new message("[game_broadcast(byte[])] Exception: " + e.ToString(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
        }


        public static void game_broadcast(Game.GameBase _game, PangyaBinaryWriter p, int _debug = 1)
        {
            try
            {
                var game_session = _game.getSessions();
                for (var i = 0; i < game_session.Count; ++i)
                    MAKE_SEND_BUFFER(p.GetBytes, game_session[i]);
            }
            catch (Exception e)
            {
                message_pool.push(new message("[game_broadcast(byte[])] Exception: " + e.ToString(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
        }

        public static void game_broadcast(Game.GameBase _game, List<PangyaBinaryWriter> v_p, int _debug = 1)
        {
            try
            {
                for (var i = 0; i < v_p.Count; ++i)
                {
                    if (v_p[i] != null)
                    {
                        var game_session = _game.getSessions();
                        for (var ii = 0; ii < game_session.Count; ++ii)
                            MAKE_SEND_BUFFER(v_p[i].GetBytes, game_session[ii]);
                    }
                    else
                    {
                        message_pool.push(new message("Error byte[] p is null, packet_func::game_broadcast()", type_msg.CL_FILE_LOG_AND_CONSOLE));
                    }
                }
            }
            catch (Exception e)
            {
                message_pool.push(new message("[game_broadcast(List)] Exception: " + e.ToString(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
        }

        public static void vector_send(PangyaBinaryWriter _p, List<Player> _v_s, int _debug = 1)
        {
            try
            {
                foreach (var el in _v_s)
                    MAKE_SEND_BUFFER(_p.GetBytes, el);
            }
            catch (Exception e)
            {
                message_pool.push(new message("[vector_send(Session)] Exception: " + e.ToString(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
        }


        public static void vector_send(List<PangyaBinaryWriter> _v_p, List<Player> _v_s, int _debug = 1)
        {
            try
            {
                foreach (var el in _v_p)
                {
                    if (el != null)
                    {
                        foreach (var el2 in _v_s)
                            MAKE_SEND_BUFFER(el.GetBytes, el2);
                    }
                    else
                        message_pool.push(new message("Error byte[] p is null, packet_func::vector_send(Player)", type_msg.CL_FILE_LOG_AND_CONSOLE));
                }
            }
            catch (Exception e)
            {
                message_pool.push(new message("[vector_send(Player List)] Exception: " + e.ToString(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
        }

        public static void session_send(PangyaBinaryWriter p, PangyaAPI.Network.PangyaSession.Session s, int _debug = 1)
        {
            try
            {
                if (s == null)
                    throw new exception("Error session s is null, packet_func::session_send()", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PACKET_FUNC_SV, 1, 2));

                MAKE_SEND_BUFFER(p.GetBytes, s);
            }
            catch (Exception e)
            {
                message_pool.push(new message("[session_send(byte[])] Exception: " + e.ToString(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
        }

        public static void session_send(byte[] p, PangyaAPI.Network.PangyaSession.Session s, int _debug = 1)
        {
            try
            {
                if (s == null)
                    throw new exception("Error session s is null, packet_func::session_send()", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PACKET_FUNC_SV, 1, 2));

                MAKE_SEND_BUFFER(p, s);
            }
            catch (Exception e)
            {
                message_pool.push(new message("[session_send(byte[])] Exception: " + e.ToString(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
        }

        public static void session_send(PangyaBinaryWriter p, Player s, int _debug = 1)
        {
            try
            {
                if (s == null)
                    throw new exception("Error session s is null, packet_func::session_send()", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PACKET_FUNC_SV, 1, 2));

                MAKE_SEND_BUFFER(p.GetBytes, s);
            }
            catch (Exception e)
            {
                message_pool.push(new message("[session_send(writer)] Exception: " + e.ToString(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
        }

        public static void session_send(List<PangyaBinaryWriter> v_p, Player s, int _debug = 1)
        {
            try
            {
                if (s == null)
                    throw new exception("Error session s is null, packet_func::session_send()", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PACKET_FUNC_SV, 1, 2));

                for (var i = 0; i < v_p.Count; ++i)
                {
                    if (v_p[i] != null && v_p[i].GetSize > 0)
                        MAKE_SEND_BUFFER(v_p[i].GetBytes, s);
                    else
                        message_pool.push(new message("Error byte[] p is null, packet_func::session_send()", type_msg.CL_FILE_LOG_AND_CONSOLE));
                }
            }
            catch (Exception e)
            {
                message_pool.push(new message("[session_send(writer list)] Exception: " + e.ToString(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
        }

        public static void session_send(List<byte[]> v_p, Player s, int _debug = 1)
        {
            try
            {
                if (s == null)
                    throw new exception("Error session s is null, packet_func::session_send()", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PACKET_FUNC_SV, 1, 2));

                for (var i = 0; i < v_p.Count; ++i)
                {
                    if (v_p[i] != null)
                        MAKE_SEND_BUFFER(v_p[i], s);
                    else
                        message_pool.push(new message("Error byte[] p is null, packet_func::session_send()", type_msg.CL_FILE_LOG_AND_CONSOLE));
                }
            }
            catch (Exception e)
            {
                message_pool.push(new message("[session_send(byte[] list)] Exception: " + e.ToString(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
        }

        public static byte[] pacote04C(int option)
        {
            var p = new PangyaBinaryWriter();
            p.init_plain(0x4C);

            p.WriteInt16((short)option);
            return p.GetBytes;
        }

        public static byte[] pacote0AA(Player _session, List<stItem> v_item)
        {
            if (_session == null || !_session.getState())
                throw new exception("Error player nao esta conectado. Em packet_func::pacote0AA()", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PACKET_FUNC_SV, 50, 0));

            var p = new PangyaBinaryWriter();
            if (v_item.Count() > 0)
            {
                p.init_plain((ushort)0xAA);

                p.WriteUInt16((ushort)v_item.Count()); // Count, ele só manda de 1 msm não manda todos, não sei por que

                for (var i = 0; i < v_item.Count(); ++i)
                {

                    p.WriteUInt32(v_item[i]._typeid);
                    p.WriteInt32(v_item[i].id);
                    p.WriteUInt16(v_item[i].STDA_C_ITEM_TIME);
                    p.WriteByte(v_item[i].flag_time);
                    p.WriteUInt16((ushort)v_item[i].stat.qntd_dep);
                    p.WriteBuffer(v_item[i].date.date.sysDate[1], Marshal.SizeOf(new PangyaTime()));
                    p.WriteStr(v_item[i].ucc.IDX, 9);

                    // Aqui é a reflexão desse pacote, usa no ticket report
                    if (v_item[i]._typeid == 0x1A000042)
                    {
                        p.WriteUInt16(v_item[i].STDA_C_ITEM_TICKET_REPORT_ID_HIGH);
                        p.WriteUInt16(v_item[i].STDA_C_ITEM_TICKET_REPORT_ID_LOW);

                        p.WriteBuffer(v_item[i].date.date.sysDate[1], Marshal.SizeOf(new PangyaTime()));
                    }
                }

                p.WriteUInt64(_session.m_pi.ui.pang);
                p.WriteUInt64(_session.m_pi.cookie);
            }
            return p.GetBytes;
        }

        public static PangyaBinaryWriter pacote196(Player _session, StateCharacterLounge stateCharacterLounge)
        {
            var p = new PangyaBinaryWriter((ushort)0x196);

            p.WriteInt32(_session.m_oid);//coloquei 1 pra testar

            p.WriteBytes(stateCharacterLounge.ToArray());

            return p;
        }
    }
}
