using PangyaAPI.Network.PangyaPacket;
using PangyaAPI.Utilities.Log;
using PangyaAPI.Utilities;
using System;
using System.Collections.Generic;
using LoginServer.Session;
using PangyaAPI.Network.PangyaSession;
using PangyaAPI.Utilities.BinaryModels;
using PangyaAPI.Network.Cmd;
using PangyaAPI.Network.Pangya_St;
using PangyaAPI.SQL.Manager;
using System.Diagnostics;
using System.Xml.Linq;
using PangyaAPI.IFF.JP.Extensions;
using LoginServer.Cmd;
using LoginServer.PangyaEnums;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;
using sls;

namespace LoginServer.PacketFunc
{
    public class packet_func : packet_func_base
    {
        public static void SUCCESS_LOGIN(string from, object arg1, Player session)
        {
            session.m_pi.m_state = 1;
            message_pool.push(new message($"[packet_func::{from}][Log] Player logou. [ID={session.m_pi.id}, UID={session.m_pi.uid}]", type_msg.CL_FILE_LOG_AND_CONSOLE));

            succes_login(arg1, session);
        }


        public static int packet001(object param, ParamDispatch pd)
        {


            try
            {

                sls.ls.getInstance().requestLogin((Player)pd._session, pd._packet);

            }
            catch (exception e)
            {

                message_pool.push(new message("[packet_func::packet001][Log][ErrorSystem] " + e.getFullMessageError()));

                if (ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) != (uint)STDA_ERROR_TYPE.LOGIN_SERVER)
                    throw;
            }

            return 0;
        }

        public static int packet003(object param, ParamDispatch pd)
        {


            string auth_key_game = "";

            try
            {

                uint server_uid = pd._packet.ReadUInt32();

                message_pool.push(new message("[packet_func::packet003][Log] Server UID: " + (server_uid)));

                // Verifica se session está varrizada para executar esse ação, 
                // se ele não fez o login com o Server ele não pode fazer nada até que ele faça o login
                // CHECK_SESSION_IS_AUTHORIZED("packet003");

                // Registra o logon no server_uid do player_uid
                var cmd_rls = new CmdRegisterLogonServer(((Player)pd._session).m_pi.uid, server_uid);

                NormalManagerDB.add(0, cmd_rls, null, null);


                if (cmd_rls.getException().getCodeError() != 0)
                    throw cmd_rls.getException();

                var cmd_auth_key_game = new CmdAuthKeyGame(((Player)pd._session).m_pi.uid, server_uid);

                NormalManagerDB.add(0, cmd_auth_key_game, null, null);

                if (cmd_auth_key_game.getException().getCodeError() != 0)
                    throw cmd_auth_key_game.getException();

                auth_key_game = cmd_auth_key_game.getAuthKey();

                message_pool.push(new message("[packet_func::packet003][Log] AuthKeyGame: " + auth_key_game
                        + ", do player: " + (((Player)pd._session).m_pi.uid), type_msg.CL_FILE_LOG_AND_CONSOLE));

            }
            catch (exception e)
            {

                message_pool.push(new message("[packet_func::packet003][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if (!ExceptionError.STDA_ERROR_CHECK_SOURCE_AND_ERROR(e.getCodeError(), (uint)STDA_ERROR_TYPE.EXEC_QUERY, 6/*AuthKeyLogin*/))
                    throw;
            }
            packet p =  pacote003(auth_key_game);
            session_send(p, ((Player)pd._session), 1);
             
            return 0;
        }

        public static int packet004(object param, ParamDispatch pd)
        {


            try
            {

                sls.ls.getInstance().requestDownPlayerOnGameServer(((Player)pd._session), pd._packet);

            }
            catch (exception e)
            {
                session_send(pacote00E(((Player)pd._session), "", 12, 500053), ((Player)pd._session), 1);

                message_pool.push(new message("[packet_func::packet004][Error] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }

            return 0;
        }

        public static int packet006(object param, ParamDispatch pd)
        {
            string wnick = "";
            try
            {

                wnick = pd._packet.ReadString();

                message_pool.push(new message("[packet_func::packet006][Log] Save Nickname: " + wnick, type_msg.CL_FILE_LOG_AND_CONSOLE));

                // Verifica se session está varrizada para executar esse ação, 
                // se ele não fez o login com o Server ele não pode fazer nada até que ele faça o login
                // CHECK_SESSION_IS_AUTHORIZED("packet006");

                var cmd_sn = new CmdSaveNick(((Player)pd._session).m_pi.uid, wnick);

                NormalManagerDB.add(0, cmd_sn, null, null);

                if (cmd_sn.getException().getCodeError() != 0)
                    throw cmd_sn.getException();

                var cmd_afl = new CmdAddFirstLogin(((Player)pd._session).m_pi.uid, 1);

                NormalManagerDB.add(0, cmd_afl, null, null);

                if (cmd_afl.getException().getCodeError() != 0)
                    throw cmd_afl.getException();

                message_pool.push(new message("[packet_func::packet006][Log] salvou o nick: " + wnick + ", do player: "
                        + (((Player)pd._session).m_pi.uid) + " com sucesso.", type_msg.CL_FILE_LOG_AND_CONSOLE));

                // Aqui colocar para verificar se ele já fez o first set, se não envia o pacote do first set, se não success_login
                var cmd_fsc = new CmdFirstSetCheck(((Player)pd._session).m_pi.uid);

                NormalManagerDB.add(0, cmd_fsc, null, null);

                if (cmd_fsc.getException().getCodeError() != 0)
                    throw cmd_fsc.getException();

                if (!cmd_fsc.getLastCheck())
                {   // Verifica se fez o primeiro set do character

                    // FIRST_SET 
                    session_send(pacote001(((Player)pd._session), 0xD9), ((Player)pd._session), 1);

                    message_pool.push(new message("[packet_func::packet006][Log] Primeira vez que o player escolhe um character padrao. player[UID="
                            + (((Player)pd._session).m_pi.uid) + ", ID=" + ((Player)pd._session).m_pi.id) + "]", type_msg.CL_FILE_LOG_AND_CONSOLE);

                }
                else
                    SUCCESS_LOGIN("packet006", param, ((Player)pd._session));

            }
            catch (exception e)
            {

                session_send(pacote00E(((Player)pd._session), wnick, 1/*UNKNOWN ERROR*/), ((Player)pd._session), 1);

                message_pool.push(new message("[packet_func::packet006][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }

            return 0;
        }

        public static int packet007(object param, ParamDispatch pd)
        {
            NICK_CHECK nc = NICK_CHECK.SUCCESS;
            uint error_info = 0;
            string wnick = "";

            Player _session = (Player)pd._session;
            try
            {
                wnick = pd._packet.ReadString();
                message_pool.push(new message($"[packet_func::packet007][Log] Check Nickname: {wnick}", type_msg.CL_FILE_LOG_AND_CONSOLE));

                if (wnick.Equals(_session.m_pi.id, StringComparison.Ordinal))
                {
                    nc = NICK_CHECK.SAME_NICK_USED;
                    message_pool.push(new message($"[packet_func::packet007][Error] O nick igual ao ID nao pode. Nick: {wnick} Player: {_session.m_pi.uid}", type_msg.CL_FILE_LOG_AND_CONSOLE));
                }

                if (nc == NICK_CHECK.SUCCESS && !(_session.m_pi.m_cap >= 4) && Regex.IsMatch(wnick, "(.*GM.*)|(.*ADM.*)", RegexOptions.IgnoreCase))
                {
                    nc = NICK_CHECK.HAVE_BAD_WORD;
                    message_pool.push(new message($"[packet_func::packet007][Error] O nick contem palavras inapropriadas: {wnick} Player: {_session.m_pi.uid}", type_msg.CL_FILE_LOG_AND_CONSOLE));
                }

                if (nc == NICK_CHECK.SUCCESS && wnick.Contains(" "))
                {
                    nc = NICK_CHECK.EMPETY_ERROR;
                    message_pool.push(new message($"[packet_func::packet007][Error] O nick contem espaco em branco: {wnick} Player: {_session.m_pi.uid}", type_msg.CL_FILE_LOG_AND_CONSOLE));
                }

                if (nc == NICK_CHECK.SUCCESS && (wnick.Length < 4 || Regex.IsMatch(wnick, "[\\^$,\\?`´~\\|\\\"@#¨'%*!\\]")))
                {
                    nc = NICK_CHECK.INCORRECT_NICK;
                    message_pool.push(new message($"[packet_func::packet007][Error] O nick eh menor que 4 letras ou tem caracteres que nao pode: {wnick} Player: {_session.m_pi.uid}", type_msg.CL_FILE_LOG_AND_CONSOLE));
                }

                if (nc == NICK_CHECK.SUCCESS)
                {
                    CmdVerifyNick cmd_vn = new CmdVerifyNick(wnick);

                    NormalManagerDB.add(0, cmd_vn, null, null);

                    if (cmd_vn.getException().getCodeError() != 0)
                        throw cmd_vn.getException();

                    if (cmd_vn.getLastCheck())
                    {
                        nc = NICK_CHECK.NICK_IN_USE;
                        message_pool.push(new message($"[packet_func::packet007][Error] O nick ja esta em uso: {wnick} Player: {_session.m_pi.uid}", type_msg.CL_FILE_LOG_AND_CONSOLE));
                    }
                }
            }
            catch (exception e)
            {

                message_pool.push(new message("[packet_func::packet007][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if (ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) == (uint)STDA_ERROR_TYPE.PANGYA_DB)
                    nc = NICK_CHECK.ERROR_DB;
                else
                    nc = NICK_CHECK.UNKNOWN_ERROR;

            }


            session_send(pacote00E(_session, wnick, (int)nc, error_info), _session, 1);

            return 0;
        }


        public static int packet008(object param, ParamDispatch pd)
        {


            packet p;

            try
            {

                uint _typeid = pd._packet.ReadUInt32();
                var default_hair = pd._packet.ReadUInt8();
                var default_shirts = pd._packet.ReadUInt8();

                message_pool.push(new message("[packet_func::packet008][Log] Character Type: " + (_typeid), type_msg.CL_FILE_LOG_AND_CONSOLE));
                message_pool.push(new message("[packet_func::packet008][Log] Default hair: " + (default_hair), type_msg.CL_FILE_LOG_AND_CONSOLE));
                message_pool.push(new message("[packet_func::packet008][Log] Default shirts: " + (default_shirts), type_msg.CL_FILE_LOG_AND_CONSOLE));

                // Verifica se session está varrizada para executar esse ação, 
                // se ele não fez o login com o Server ele não pode fazer nada até que ele faça o login
                // CHECK_SESSION_IS_AUTHORIZED("packet008");

                if (sIff.getInstance().findCharacter(_typeid) == null)
                    throw new exception("[packet_func::packet008][Error] typeid character: " + (_typeid) + " is worng.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PACKET_FUNC_LS, 21, 0));

                if (default_hair > 9)
                    throw new exception("[packet_func::packet008][Error] default_hair: " + (default_hair) + " is wrong. character: " + (_typeid), ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PACKET_FUNC_SV, 22, 0));

                if (default_shirts != 0)
                    throw new exception("[packet_func::packet008][Error] default_shirts: " + (default_shirts) + " is wrong. character: " + (_typeid), ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PACKET_FUNC_LS, 23, 0));

                CharacterInfo ci = new CharacterInfo();

                ci.id = uint.MaxValue;
                ci._typeid = _typeid;
                ci.default_hair = default_hair;
                ci.default_shirts = default_shirts;

                // Default Parts
                ci.initComboDef();

                var cmd_ac = new CmdAddCharacter(((Player)pd._session).m_pi.uid, ci, 0, 1);
                var cmd_afs = new CmdAddFirstSet(((Player)pd._session).m_pi.uid);

                NormalManagerDB.add(0, cmd_ac, null, null);
                NormalManagerDB.add(0, cmd_afs, null, null);

                if (cmd_ac.getException().getCodeError() != 0)
                    throw cmd_ac.getException();

                // Info Character Add com o Id gerado no banco de dados
                ci = cmd_ac.getInfo();

                if (cmd_afs.getException().getCodeError() != 0)
                    throw cmd_afs.getException();

                // Update Character Equipado no banco de dados
                var cmd_uce = new CmdUpdateCharacterEquiped(((Player)pd._session).m_pi.uid, (int)ci.id);

                NormalManagerDB.add(0, cmd_uce, null, null);

                if (cmd_uce.getException().getCodeError() != 0)
                    throw cmd_uce.getException();

                message_pool.push(new message("[packet_func::packet008][Log] First Character Set with success! to player: " + (((Player)pd._session).m_pi.uid), type_msg.CL_FILE_LOG_AND_CONSOLE));

                // Ok
                session_send(pacote011(1), ((Player)pd._session));

                // Success Login
                SUCCESS_LOGIN("packet008", param, ((Player)pd._session));

            }
            catch (exception e)
            {
                // Erro na hora de salvar o character 
                session_send(pacote011(),((Player)pd._session));

                session_send(pacote00E(((Player)pd._session), "", 12, 500051), ((Player)pd._session), 1);

                message_pool.push(new message("[packet_func::packet008][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }

            return 0;
        }

        public static int packet00B(object param, ParamDispatch pd)
        {


            try
            {

                sls.ls.getInstance().requestTryReLogin(((Player)pd._session), pd._packet);

            }
            catch (exception e)
            {

                message_pool.push(new message("[packet_func::packet00B][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if (ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) != (uint)STDA_ERROR_TYPE.LOGIN_SERVER)
                    throw;
            }

            return 0;
        }

        public static int packet_sv003(object param, ParamDispatch pd)
        {


            // Delete player "Desconnecta player"
            //pp.m_pw._session_pool.deleteSession(pp.m_session);
            // Parece que ele desconectar sozinho já
            //::shutdown(pp.m_session.m_sock, SD_RECEIVE);

            return 0;
        }

        public static int packet_sv006(object param, ParamDispatch pd)
        {


            // message_pool.push(new message("[packet_func::packet_sv006][Log] Time: " + ((std::clock() - ((Player)pd._session).m_time_start) / (double)CLOCKS_PER_SEC), type_msg.CL_ONLY_FILE_TIME_LOG));

            //message_pool.push(new message("[packet_func::packet_sv006][Log] Send SUCCESS LOGIN Time: " + ((std::clock() - ((Player)pd._session).m_tick_bot) / (double)CLOCKS_PER_SEC), type_msg.CL_ONLY_FILE_TIME_LOG));

            return 0;
        }

        public static int packet_svFazNada(object param, ParamDispatch pd)
        {


            // Faz Nada

            return 0;
        }

        public static int packet_svDisconectPlayerBroadcast(object param, ParamDispatch pd)
        {


            /*if (pp.m_pw != null)
                delete pp.m_pw;*/
            // faz nada aqui

            return 0;
        }

        public static int packet_as001(object param, ParamDispatch pd)
        {
            try
            {

                // Log Teste
                message_pool.push(new message("[packet_func::packet_as001][Log] Teste, so para deixar aqui, quando for usar um dia.", type_msg.CL_FILE_LOG_AND_CONSOLE));

            }
            catch (exception e)
            {

                message_pool.push(new message("[packet_func::packet_as001][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }

            return 0;
        }

        public static packet pacote001(Player _session, byte option = 0, int sub_opt = 0)
        {
            var subID = (SubLoginCode)option;
            packet p = new packet();

            p.init_plain(0x001);

            p.AddByte(option);  // OPTION 1 SENHA OU ID ERRADO

            message_pool.push($"[packet_func::pacote001][Log] IdSub=> {subID}", type_msg.CL_FILE_LOG_AND_CONSOLE);

            switch (option)
            {
                case 0:
                    p.AddString(_session.m_pi.id);
                    p.AddUInt32(_session.m_pi.uid);
                    p.AddUInt32(_session.m_pi.m_cap);
                    p.AddUInt16(_session.m_pi.level);            // 1 level, 1 pc bang(ACHO), com base no S4
                    p.AddInt32(0);                              // valor 0 Unknown
                    p.AddInt32(5);                              // valor 5 Unknown
                    p.AddTime(_session.m_pi.login_time);   // Time Build Login Server (ACHO)							- JP S9 ler mais ignora ele
                    p.AddZero(3);   // Time Build Login Server (ACHO)							- JP S9 ler mais ignora ele
                    p.AddString(_session.m_pi.acess_code);                      // Alguma AuthKey aleatória para minha conta que eu não sei - JP S9 ler mais ignora ele
                    p.AddUInt32(0);                             // Unknown valor - JP S9 ler mais ignora ele
                    p.AddUInt32(0);                             // Unknown valor - JP S9 ler mais ignora ele
                    p.AddString(_session.m_pi.nickname);
                    p.AddInt16(0);
                    break;
                case 1:
                    p.AddInt32(0);  // add 4 bytes vazios
                    break;
                case 0xD8:
                    // First Login
                    p.AddInt32(-1);
                    p.AddInt16(0);
                    break;
                case 0xD9:
                    p.AddInt16(0);
                    break;
                case 0x0c:
                case 0xE2:
                case 16:
                    p.AddInt32(sub_opt);
                    break;
                case 7:

                    var tempo = _session.m_pi.block_flag.m_id_state.block_time / 60 / 60/*Hora*/; // Hora

                    p.AddInt32(_session.m_pi.block_flag.m_id_state.block_time == -1 || tempo == 0 ? 1/*Menos de uma hora*/ : tempo);   // Block Por Tempo
                    // Aqui pode ter uma  com mensagem que o pangya exibe
                    //p.AddString("ola");
                    break;

                default:
                    break;
            }
            return p;
        }
         
        public static packet pacote002(List<ServerInfo> v_element)
        {

            var p = new packet();
            p.init_plain(0x002);

            p.AddByte((byte)(v_element.Count & 0xFF)); // 1 Game Server online

            for (int i = 0; i < v_element.Count; i++)
                p.AddBytes(v_element[i].Build());

            return p;
        }

        public static packet pacote003(string AuthKeyLogin, int option = 0)
        {

            var p = new packet();
            p.init_plain(0x003);

            p.AddInt32(option);

            p.AddString(AuthKeyLogin);

            return p;
        }

        public static packet pacote006(chat_macro_user _mu)
        {
            var p = new packet();
            p.init_plain(0x006);

            p.AddBuffer(_mu, Marshal.SizeOf(new chat_macro_user()));

            return p;
        }

        public static packet pacote009(List<ServerInfo> v_element)
        {

            var p = new packet();
            p.init_plain(0x009);

            p.AddByte((byte)(v_element.Count & 0xFF)); // nenhum Msn Server on

            for (int i = 0; i < v_element.Count; i++)
                p.AddBytes(v_element[i].Build());

            return p;
        }


        public static packet pacote00E(Player _session, string nick, int option = 0, uint error = 0)
        {
            var p = new packet();
            p.init_plain(0x00E);

            p.AddInt32(option);

            if (option == 0)
                p.AddString(nick);
            else if (option == 12)
                p.AddUInt32(error);

            return p;
        }

        // Mensagem do Tutorial
        public static packet pacote00F(Player _session, int option = 0)
        {
            var p = new packet();
            p.init_plain(0x0F);

            p.AddByte((sbyte)option);

            p.AddString(_session.m_pi.id);

            p.AddUInt32(0);                             // valor 0 Unknown
            p.AddUInt32(5);                             // valor 5 Unknown
            p.AddTime(_session.m_pi.login_time);   // Time Build Login Server (ACHO)							- JP S9 ler mais ignora ele
            p.AddZero(3);   // Time Build Login Server (ACHO)							- JP S9 ler mais ignora ele
            p.AddString(_session.m_pi.acess_code);                      // Alguma AuthKey aleatória para minha conta que eu não sei		- JP S9 ler mais ignora ele

            return p;
        }

        public static packet pacote010(string AuthKey)
        {

            var p = new packet();
            p.init_plain(0x10);

            p.AddString(AuthKey);

            return p;
        }

        public static packet pacote011(int option = 0)
        {

            var p = new packet();
            p.init_plain(0x11);

            p.AddUInt16((ushort)option);

            return p;
        }

        public static void succes_login(object _arg, Player _session, int option = 0)
        {
            List<ServerInfo> sis = new List<ServerInfo>(), msns = new List<ServerInfo>();
            chat_macro_user _cmu = new chat_macro_user();
            string auth_key_login = "";

            /* OPTION
            *  0 PRIMEIRO LOGIN
            *  1 RELOGA DEPOIS QUE CAIU DO GAME SERVER, COM A AUTH KEY
            */

            try
            {

                var cmd_server_list = new CmdServerList(TYPE_SERVER.GAME);

                NormalManagerDB.add(0, cmd_server_list, null, null);

                if (cmd_server_list.getException().getCodeError() != 0)
                    throw cmd_server_list.getException();

                sis = cmd_server_list.getServerList();

                cmd_server_list.setType(TYPE_SERVER.GAME);

                var cmd_auth_key_login = new CmdAuthKeyLogin((int)_session.m_pi.uid);

                NormalManagerDB.add(0, cmd_server_list, null, null);
                NormalManagerDB.add(0, cmd_auth_key_login, null, null);

                if (cmd_server_list.getException().getCodeError() != 0)
                    throw cmd_server_list.getException();

                msns = cmd_server_list.getServerList();

                if (cmd_auth_key_login.getException().getCodeError() != 0)
                    throw cmd_auth_key_login.getException();

                auth_key_login = cmd_auth_key_login.getAuthKey();

                message_pool.push(new message("[packet_func::succes_login][Log] AuthKeyLogin: " + auth_key_login
                        + ", do player: " + (_session.m_pi.uid), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if (option == 0)
                {
                    var cmd_macro_user = new CmdChatMacroUser(_session.m_pi.uid);

                    NormalManagerDB.add(0, cmd_macro_user, null, null);

                    if (cmd_macro_user.getException().getCodeError() != 0)
                        throw cmd_macro_user.getException();

                    _cmu = cmd_macro_user.getMacroUser();
                }

                var ip = (_session.getIP());

                // RegisterLogin do Player
                var cmd_rpl = new CmdRegisterPlayerLogin(_session.m_pi.uid, ip, ls.getInstance().getUID());

                NormalManagerDB.add(0, cmd_rpl, null, null);

                if (cmd_rpl.getException().getCodeError() != 0)
                    throw cmd_rpl.getException();

            }
            catch (exception e)
            {

                message_pool.push(new message("[packet_func::succes_login][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if (ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) == (uint)STDA_ERROR_TYPE.EXEC_QUERY)
                {
                    if (ExceptionError.STDA_ERROR_DECODE(e.getCodeError()) != 7/*getServerList*/ && ExceptionError.STDA_ERROR_DECODE(e.getCodeError()) != 9/*MacroUser*/
               && ExceptionError.STDA_ERROR_DECODE(e.getCodeError()) != 8/*getMsnList*/ && ExceptionError.STDA_ERROR_DECODE(e.getCodeError()) != 5/*AuthKey*/)
                        throw;
                }
                else
                    throw;
            }

            var p = pacote010(auth_key_login);
            session_send(p, _session, 1);

            if (option == 0)
            {
                p = pacote001(_session);
                session_send(p, _session, 1);
            }

            p = pacote002(sis);
            session_send(p, _session, 1);

            p = pacote009(msns);
            session_send(p, _session, 1);

            if (option == 0)
            {
                p = pacote006(_cmu);
                session_send(p, _session, 1);
            }
        }

        public static void session_send(packet p, Player _session, int _debug = 1)
        { 
            MAKE_SEND_BUFFER(p, _session);
        } 
    }
}
