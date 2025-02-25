using PangyaAPI.SQL;
using PangyaAPI.Network.Pangya_St;
using PangyaAPI.Utilities;
using System.Collections.Generic;
using System.Linq;
using _smp = PangyaAPI.Utilities.Log;                   
using static GameServer.GameType._Define;
using GameServer.Cmd;
using GameServer.Session;
using static GameServer.GameType.PlayerInfo;
using GameServer.GameType;
using System;
using PangyaAPI.Utilities.Log;
using PangyaAPI.Network.PangyaPacket;
using PangyaAPI.SQL.Manager;
using PangyaAPI.Utilities.BinaryModels;
using PangyaAPI.Network.Cmd;                               
using System.Diagnostics;
using GameServer.Game.System;
using System.Threading;
using GameServer.Game.Manager;
using GameServer.PacketFunc;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace GameServer.Game.System
{
    /// <summary>
    /// Class manipulation login player!
    /// checks and funcs
    /// </summary>
    public class LoginSystem
    {
        uint m_count;
        /// <summary>
        /// requestCommonCmdGM login 
        /// </summary>
        /// <param db_name="_Packet">bits recevied by projectg</param>
        /// <param db_name="_session">client = session</param>
        public void requestLogin(Player _session, Packet _packet)
        {
            PangyaBinaryWriter p;

            try
            {

                uint Packet_version = 0;

                KeysOfLogin kol = new KeysOfLogin();
                AuthKeyInfo akli = new AuthKeyInfo();
                AuthKeyGameInfo akgi = new AuthKeyGameInfo();

                string client_version;
                                      
                // Player info da session e vai guardar os valores recuperados do banco de dados
                PlayerInfo _pi = (_session.m_pi);

                //////////// ----------------------- Começa a ler o Packet que o cliente enviou ------------------------- \\\\\\\\\\\
                // Read Packet Client request
                _pi.id = _packet.ReadString();
                _pi.uid = _packet.ReadUInt32();
                var ntKey = _packet.ReadUInt32(); // ntKey
                var Command = _packet.ReadUInt16();
                kol.keys[0] = _packet.ReadString();
                client_version = _packet.ReadString();
                Packet_version = _packet.ReadUInt32();
                string mac_address = _packet.ReadString();
                kol.keys[1] = _packet.ReadString();


                // -------------- Finished reading the Packet sent by the client ---------------


                ////////////----------------------- Terminou a leitura do Packet que o cliente enviou -------------------------\\\\\\\\\\\/

                // Verifica aqui se o IP/MAC ADDRESS do player está bloqueado
                //if (Program.gs.haveBanList(_session.getIP(), mac_address, !mac_address.empty()))
                //    throw new exception("Player[UID=" + (_pi.m_uid) + ", IP="
                //            + _session.getIP() + ", MAC=" + mac_address + "] esta bloqueado por regiao IP/MAC Addrress.");

                // Aqui verifica se recebeu os dados corretos
                if (_pi.id[0] == '\0')
                {  
                    throw new exception("Player[UID=" + (_pi.uid)
                            + ", IP=" + _session.getIP() + "] id que o player enviou eh invalido. id: " + (_pi.id));
                }
                // Verifica se o gs está mantle, se tiver verifica se o player tem capacidade para entrar
                var cmd_pi = new CmdPlayerInfo(_pi.uid); // Waiter

                NormalManagerDB.add(0, cmd_pi, null, null);

                if (cmd_pi.getException().getCodeError() != 0)
                    throw cmd_pi.getException();

                //set info player!
                _pi.SetInfo(cmd_pi.getInfo());

                _session.m_pi = _pi; //fez ficar correto agora!

                if (_pi.uid <= 0)
                    throw new exception("player[UID=" + (_pi.uid) + "] nao existe no banco de dados");

                // UID de outro player ou enviou o ID errado mesmo (essa parte é anti-hack ou bot)
                if (string.Compare(cmd_pi.getInfo().id, _pi.id) != 0)
                    throw new exception("Player[UID=" + (_pi.uid) + ", REQ_UID="
                            + (_pi.uid) + "] Player ID nao bate : client send ID : " + (_pi.id) + "\t player DB ID : "
                            + (_pi.id));
                // Verifica aqui se a conta do player está bloqueada
                if (_pi.block_flag.m_id_state.ull_IDState != 0)
                {

                    if (_pi.block_flag.m_id_state.L_BLOCK_TEMPORARY && (_pi.block_flag.m_id_state.block_time == -1 || _pi.block_flag.m_id_state.block_time > 0))
                    {

                        throw new exception("[LoginSystem::requestLogin][Log] Bloqueado por tempo[Time="
                                + (_pi.block_flag.m_id_state.block_time == -1 ? ("indeterminado") : ((_pi.block_flag.m_id_state.block_time / 60)
                                + "min " + (_pi.block_flag.m_id_state.block_time % 60) + "sec"))
                                + "]. player [UID=" + (_pi.uid) + ", ID=" + (_pi.id) + "]");

                    }
                    else if (_pi.block_flag.m_id_state.L_BLOCK_FOREVER)
                    {

                        throw new exception("[LoginSystem::requestLogin][Log] Bloqueado permanente. player [UID=" + (_pi.uid)
                                + ", ID=" + (_pi.id) + "]");
                    }
                    //else if (_pi.block_flag.m_id_state.L_BLOCK_ALL_IP)
                    //{

                    //    // Bloquea todos os IP que o player logar e da error de que a area dele foi bloqueada

                    //    // Add o ip do player para a lista de ip banidos
                    //    NormalManagerDB.add(9, new CmdInsertBlockIp(_session.getIP(), "255.255.255.255"), Program.gs.SQLDBResponse, Program.gs);

                    //    // Resposta
                    //    throw new exception("[LoginSystem::requestLogin][Log] Player[UID=" + (_pi.m_uid) + ", IP=" + (_session.getIP())
                    //            + "] Block ALL IP que o player fizer login.");
                    //}
                    //else if (_pi.block_flag.m_id_state.L_BLOCK_MAC_ADDRESS)
                    //{

                    //    // Bloquea o MAC Address que o player logar e da error de que a area dele foi bloqueada

                    //    // Add o MAC Address do player para a lista de MAC Address banidos
                    //    NormalManagerDB.add(10, new CmdInsertBlockMac(mac_address), Program.gs.SQLDBResponse, Program.gs);

                    //    // Resposta
                    //    throw new exception("[LoginSystem::requestLogin][Log] Player[UID=" + (_pi.m_uid)
                    //            + ", IP=" + (_session.getIP()) + ", MAC=" + mac_address + "] Block MAC Address que o player fizer login.");

                    //}
                }

                // Check Packet version
                _packet.Version_Decrypt(ref Packet_version);


                //// Se a flag do canSameIDLogin estiver ativo, não verifica Packet
                //if (!Program.gs.canSameIDLogin() && Packet_version != Program.gs.getInfo().packet_version)
                //{
                //    // Error no login, set falso o varriza o player a continuar conectado com o Game Server
                //    _session.m_is_authorized = false;

                //    // Error Sistema
                //    p = new PangyaBinaryWriter(0x44);

                //    // Pronto agora sim, mostra o erro que eu quero
                //    p.WriteByte(0x0B);

                //    _session.Send(p);
                     
                //    _session.Disconnect();
                //    return;
                //}
                // Verifica o Auth Key do player
                var cmd_akli = new CmdAuthKeyLoginInfo((int)_pi.uid); // Waiter

                NormalManagerDB.add(0, cmd_akli, null, null);  

                if (cmd_akli.getException().getCodeError() != 0)
                    throw cmd_akli.getException();
                //false  = true, true = false
                //	// ### Isso aqui é uma falha de segurança faltal, muito grande nunca posso deixar isso ligado depois que colocar ele online
                //	if (!m_login_manager.canSameIDLogin() && !cmd_akli.getInfo().valid)
                //		throw new exception("Player[UID=" + (_pi.m_uid) + "].\tAuthKey ja foi utilizada antes.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.GAME_SERVER, 1056, 0));

                //	// ### Isso aqui é uma falha de segurança faltal, muito grande nunca posso deixar isso ligado depois que colocar ele online
                //	if (!m_login_manager.canSameIDLogin() &&
                //string.Compare(kol.keys[0], cmd_akli.getInfo().key) != 0
                //	)
                //		throw new exception("Player[UID=" + (_pi.m_uid) + "].\tAuthKey no bate(no match).", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.GAME_SERVER, 1057, 0));

                //	ClientVersion cv_side_sv = ClientVersion::make_version(const_cast <  &> (m_login_manager.getClientVersionSideServer()));
                //	var cv_side_c = ClientVersion::make_version(client_version);

                //	if (cv_side_c.flag == ClientVersion::COMPLETE_VERSION && strcmp(cv_side_c.region, cv_side_sv.region) == 0
                //			&& strcmp(cv_side_c.season, cv_side_sv.season) == 0)
                //	{

                //		if (cv_side_c.high != cv_side_sv.high || cv_side_c.low < cv_side_sv.low)
                //		{
                //			_smp.message_pool.push(("[LoginSystem::requestLogin][WARNING] Player[UID=" + (_pi.m_uid) + "].\tClient Version not match. Server: "
                //					+ (m_login_manager.getClientVersionSideServer()) + " == Client: " + cv_side_c.toString(), CL_ONLY_FILE_LOG));

                //			_pi.block_flag.m_flag.stBit.all_game = 1;// |= BLOCK_PLAY_ALL;
                //		}

                //	}
                //	else if (cv_side_c.high != cv_side_sv.high || cv_side_c.low < cv_side_sv.low)
                //	{

                //		_smp.message_pool.push(("[LoginSystem::requestLogin][WARNING] Player[UID=" + (_pi.m_uid) + "].\tClient Version not match. Server: "
                //				+ (m_login_manager.getClientVersionSideServer()) + " == Client: " + cv_side_c.toString(), CL_ONLY_FILE_LOG));

                //		_pi.block_flag.m_flag.stBit.all_game = 1;// |= BLOCK_PLAY_ALL;
                //	}

                // Member Info
                var cmd_mi = new CmdMemberInfo(_pi.uid);    // Waiter

                NormalManagerDB.add(0, cmd_mi, null, null);              

                if (cmd_mi.getException().getCodeError() != 0)
                    throw cmd_mi.getException();

                _session.m_pi.mi = cmd_mi.getInfo();
                // Passa o Online ID para a estrutura MemberInfo, para não da erro depois
                _pi.mi.oid = _session.m_oid;
                _pi.mi.state_flag.visible = true;
                _pi.mi.state_flag.whisper = _pi.whisper.IsTrue();
                _pi.mi.state_flag.channel = false;

                if (_pi.m_cap.game_master)
                {
                    _session.m_gi.setGMUID(_pi.uid);    // Set o UID do GM dados

                    _pi.mi.state_flag.visible = _session.m_gi.visible;
                    _pi.mi.state_flag.whisper = _session.m_gi.whisper;
                    _pi.mi.state_flag.channel = _session.m_gi.channel;
                }

                // Verifica se o player tem a capacidade e level para entrar no gs
                if (Program.gs.m_si.propriedade.only_rookie && _pi.level >= 6/*Beginner E maior*/)
                    throw new exception("Player[UID=" + (_pi.uid) + ", LEVEL="
                            + ((short)_pi.level) + "] nao pode entrar no gs por que o gs eh so para rookie.");
                /*Nega ele não pode ser nenhum para lançar o erro*/
                if (Program.gs.m_si.propriedade.mantle && !(_pi.m_cap.mantle|| _pi.m_cap.game_master))
                    throw new exception("Player[UID=" + (_pi.uid) + ", CAP=" + (_pi.m_cap.ulCapability)
                            + "] nao tem a capacidade para entrar no gs mantle.");
                // Verifica se o Player já está logado
                var player_logado = Program.gs.HasLoggedWithOuterSocket(_session);

                if (player_logado != null)
                {
                    //if (!m_login_manager.canSameIDLogin())
                    //{
                    //	_smp.message_pool.push(("[LoginSystem::requestLogin][Log] Player[UID=" + (_pi.m_uid) + ", OID="
                    //		+ (_session.m_oid) + ", IP=" + _session.getIP() + "] que esta logando agora, ja tem uma outra session com o mesmo UID logado, desloga o outro Player[UID="
                    //		+ (player_logado.getUID()) + ", OID=" + (player_logado.m_oid) + ", IP=" + player_logado.getIP() + "]", CL_FILE_LOG_AND_CONSOLE));

                    ////	if (!DisconnectSession(player_logado))
                    ////		throw new exception("Nao conseguiu disconnectar o player[UID=" + (player_logado.getUID())
                    ////			+ ", OID=" + (player_logado.m_oid) + ", IP=" + player_logado.getIP() + "], ele pode esta com o bug do oid bloqueado, ou SessionBase::UsaCtx bloqueado.");
                    //}
                }

                // Junta Flag de block do gs, ao do player
                _pi.block_flag.m_flag.ullFlag |= Program.gs.m_si.flag.ullFlag;
                _pi.m_cap = _pi.mi.capability;//seta cap
                // Authorized a ficar online no gs por tempo indeterminado
                _session.m_is_authorized = true;

                // Registra no Banco de dados que o player está logado no Game Server
                NormalManagerDB.add(5, new CmdRegisterLogon(_pi.uid, 0/*Logou*/), Program.gs.SQLDBResponse, Program.gs);

                // Resgistra o Login do Player no gs
                NormalManagerDB.add(7, new CmdRegisterLogonServer(_pi.uid, Program.gs.m_si.uid), Program.gs.SQLDBResponse, Program.gs);

                _smp.message_pool.push("[LoginSystem::requestLogin][Log] Player[OID=" + (_session.m_oid) + ", UID=" + (_pi.uid) + ", NICK="
                        + (_pi.nickname) + "] Sucess.");

                //// Verifica se o papel tem limite por dia, se não anula o papel shop do player
                sPapelShopSystem.getInstance().init_player_papel_shop_info(_session);

                //NormalManagerDB.add(11, new CmdFirstAnniversary(), Program.gs.SQLDBResponse, this);  

                NormalManagerDB.add(2, new CmdUserEquip(_session.m_pi.uid), SQLDBResponse, _session);
                                                                           
                // Entra com sucesso
                _session.Send(packet_func_sv.pacote044(Program.gs.m_si, 0xD3));

            }
            catch (exception ex)
            {
                _smp.message_pool.push(new message(
             $"[LoginSystem::requestLogin][ErrorSt] {ex.Message}\nStack Trace: {ex.StackTrace}",
             type_msg.CL_FILE_LOG_AND_CONSOLE));

                // Error no login, set falso o varriza o player a continuar conectado com o Game Server
                _session.m_is_authorized = false;

                // Error Sistema
                p = new PangyaBinaryWriter();
                p.Write(new byte[] { 0x44, 0x00 });
                // Pronto agora sim, mostra o erro que eu quero
                p.Write(300);

                _session.Send(p.GetBytes);                                  

                // Disconnect

                Program.gs.DisconnectSession(_session);
            }
        }

        void SQLDBResponse(int _msg_id, Pangya_DB _pangya_db, object _arg)
        {
            if (_arg == null)
            {
                _smp.message_pool.push("[LoginSystem.SQLDBResponse][Error] _arg is null na msg_id = " + (_msg_id));
                return;
            }
            // if (_arg is LoginTask && (_session = (LoginTask)_arg) != null)

            var _session = (Player)_arg;

            try
            {
                // Verifica se a session ainda é valida, essas funções já é thread-safe
                if (_session == null || !_session.getConnected())
                    throw new exception("[SQLDBResponse][Error] session is invalid, para tratar o pangya_db");

                // Por Hora só sai, depois faço outro tipo de tratamento se precisar
                if (_pangya_db.getException().getCodeError() != 0)
                    throw new exception(_pangya_db.getException().getFullMessageError());

                switch (_msg_id)
                {
                    case 0: // Info Player
                        {

                            break;
                        }
                    case 1: // Key Login
                        {


                            break;
                        }
                    case 2: // Member Info - User Equip
                        {

                            var pi = _session.m_pi;

                            _session.m_pi.ue = ((CmdUserEquip)_pangya_db).getInfo();

                            // Verifica se tem o Pacote de verificação de bots ativado
                            int ttl = Program.gs.getBotTTL(); //10000÷1.000=10s

                            _session.Send(packet_func_sv.pacote1A9(ttl/*milliseconds*/)); // Tempo para enviar um pacote, ant Bot

                            NormalManagerDB.add(5, new CmdTutorialInfo(pi.uid), SQLDBResponse, _session);

                            NormalManagerDB.add(6, new CmdCouponGacha(pi.uid), SQLDBResponse, _session);

                            NormalManagerDB.add(7, new CmdUserInfo(pi.uid), SQLDBResponse, _session);

                            NormalManagerDB.add(8, new CmdGuildInfo(pi.uid, 0), SQLDBResponse, _session);

                            NormalManagerDB.add(9, new CmdDolfiniLockerInfo(pi.uid), SQLDBResponse, _session);

                            NormalManagerDB.add(10, new CmdCookie(pi.uid), SQLDBResponse, _session);

                            NormalManagerDB.add(11, new CmdTrofelInfo(pi.uid, CmdTrofelInfo.TYPE_SEASON.CURRENT), SQLDBResponse, _session);

                            // Esses que estavam aqui coloquei no resposta do CmdUserEquip, por que eles precisam da resposta do User Equip

                            NormalManagerDB.add(16, new CmdMyRoomConfig(pi.uid), SQLDBResponse, _session);

                            NormalManagerDB.add(18, new CmdCheckAchievement(_session.m_pi.uid), SQLDBResponse, _session);

                            NormalManagerDB.add(20, new CmdDailyQuestInfoUser(pi.uid, CmdDailyQuestInfoUser.TYPE.GET), SQLDBResponse, _session);

                            NormalManagerDB.add(21, new CmdCardInfo(pi.uid, CmdCardInfo.TYPE.ALL), SQLDBResponse, _session);

                            NormalManagerDB.add(22, new CmdCardEquipInfo(pi.uid), SQLDBResponse, _session);

                            NormalManagerDB.add(23, new CmdTrophySpecial(pi.uid, CmdTrophySpecial.TYPE_SEASON.CURRENT, CmdTrophySpecial.TYPE.NORMAL), SQLDBResponse, _session);

                            NormalManagerDB.add(24, new CmdTrophySpecial(pi.uid, CmdTrophySpecial.TYPE_SEASON.CURRENT, CmdTrophySpecial.TYPE.GRAND_PRIX), SQLDBResponse, _session);

                            break;
                        }
                    case 3: // User Equip - Desativa
                        {
                            break;
                        }
                    case 4: // Premium Ticket
                        {
                            var pi = _session.m_pi;

                            pi.pt = ((CmdPremiumTicketInfo)(_pangya_db)).getInfo();

                            ///Att Capability do player
                            ///Verifica se tem premium ticket para mandar o pacote do premium user e a comet
                            if (sPremiumSystem.getInstance().isPremiumTicket(pi.pt._typeid) && pi.pt.id != 0 && pi.pt.unix_sec_date > 0)
                            {

                                sPremiumSystem.getInstance().updatePremiumUser(_session);

                                _smp.message_pool.push("[SQLDBResponse][Log] Player[UID=" + (pi.uid) + "] is Premium User");
                            }

                            break;
                        }
                    case 5: // Tutorial Info
                        {

                            _session.m_pi.TutoInfo = ((CmdTutorialInfo)(_pangya_db)).getInfo();  
                            // Manda pacote do tutorial aqui
                            _session.Send(packet_func_sv.pacote11F(_session.m_pi, 3/*tutorial info, 3 add do zero init*/));

                            break;
                        }
                    case 6: // Coupon Gacha
                        {
                            _session.m_pi.cg = ((CmdCouponGacha)(_pangya_db)).getCouponGacha();  

                            // Não sei se o que é esse pacote, então não sei o que ele busca no banco de dados, mas depois descubro
                            // Deixar ele enviando aqui por enquanto

                            _session.Send(packet_func_sv.pacote101());// pacote novo do JP

                            break;
                        }
                    case 7: // User Info
                        {

                            var pi = _session.m_pi;

                            pi.ui = ((CmdUserInfo)(_pangya_db)).getInfo();    // cmd_ui.getInfo();

                            NormalManagerDB.add(26, new CmdMapStatistics(_session.m_pi.uid, CmdMapStatistics.TYPE_SEASON.CURRENT, CmdMapStatistics.TYPE.NORMAL, CmdMapStatistics.TYPE_MODO.M_NORMAL), SQLDBResponse, _session);

                            NormalManagerDB.add(27, new CmdMapStatistics(_session.m_pi.uid, CmdMapStatistics.TYPE_SEASON.CURRENT, CmdMapStatistics.TYPE.ASSIST, CmdMapStatistics.TYPE_MODO.M_NORMAL), SQLDBResponse, _session);

                            NormalManagerDB.add(28, new CmdMapStatistics(_session.m_pi.uid, CmdMapStatistics.TYPE_SEASON.CURRENT, CmdMapStatistics.TYPE.NORMAL, CmdMapStatistics.TYPE_MODO.M_NATURAL), SQLDBResponse, _session);

                            NormalManagerDB.add(29, new CmdMapStatistics(_session.m_pi.uid, CmdMapStatistics.TYPE_SEASON.CURRENT, CmdMapStatistics.TYPE.ASSIST, CmdMapStatistics.TYPE_MODO.M_NATURAL), SQLDBResponse, _session);

                            NormalManagerDB.add(30, new CmdMapStatistics(_session.m_pi.uid, CmdMapStatistics.TYPE_SEASON.CURRENT, CmdMapStatistics.TYPE.NORMAL, CmdMapStatistics.TYPE_MODO.M_GRAND_PRIX), SQLDBResponse, _session);

                            NormalManagerDB.add(31, new CmdMapStatistics(_session.m_pi.uid, CmdMapStatistics.TYPE_SEASON.CURRENT, CmdMapStatistics.TYPE.ASSIST, CmdMapStatistics.TYPE_MODO.M_GRAND_PRIX), SQLDBResponse, _session);

                            NormalManagerDB.add(36, new CmdChatMacroUser(_session.m_pi.uid), SQLDBResponse, _session);

                            NormalManagerDB.add(38, new CmdFriendInfo(_session.m_pi.uid), SQLDBResponse, _session);

                            break;
                        }
                    case 8: // Guild Info
                        {
                            _session.m_pi.gi = ((CmdGuildInfo)(_pangya_db)).getInfo();   // cmd_gi.getInfo();
                            break;
                        }
                    case 9:     // Donfini Locker Info
                        {
                            _session.m_pi.df = ((CmdDolfiniLockerInfo)(_pangya_db)).getInfo();   // cmd_df.getInfo();
                            break;
                        }
                    case 10:    // Cookie
                        {
                            _session.m_pi.cookie = ((CmdCookie)(_pangya_db)).getCookie();    // cmd_cookie.getCookie();

                            NormalManagerDB.add(32, new CmdMailBoxInfo2(_session.m_pi.uid), SQLDBResponse, _session);

                            NormalManagerDB.add(33, new CmdCaddieInfo(_session.m_pi.uid, CmdCaddieInfo.TYPE.FERIAS), SQLDBResponse, _session);

                            NormalManagerDB.add(34, new CmdMsgOffInfo(_session.m_pi.uid), SQLDBResponse, _session);

                            NormalManagerDB.add(35, new CmdItemBuffInfo(_session.m_pi.uid), SQLDBResponse, _session);

                            NormalManagerDB.add(37, new CmdLastPlayerGameInfo(_session.m_pi.uid), SQLDBResponse, _session);

                            NormalManagerDB.add(39, new CmdAttendanceRewardInfo(_session.m_pi.uid), SQLDBResponse, _session);

                            NormalManagerDB.add(42, new CmdGrandPrixClear(_session.m_pi.uid), SQLDBResponse, _session);

                            NormalManagerDB.add(43, new CmdGrandZodiacPontos(_session.m_pi.uid, CmdGrandZodiacPontos.eCMD_GRAND_ZODIAC_TYPE.CGZT_GET), SQLDBResponse, _session);

                            NormalManagerDB.add(44, new CmdLegacyTikiShopInfo(_session.m_pi.uid), SQLDBResponse, _session);

                            break;
                        }
                    case 11:    // Trofel Info atual
                        {
                            _session.m_pi.ti_current_season = ((CmdTrofelInfo)(_pangya_db)).getInfo();   // cmd_ti.getInfo();

                            NormalManagerDB.add(12, new CmdCharacterInfo(_session.m_pi.uid, CmdCharacterInfo.TYPE.ALL), SQLDBResponse, _session);

                            NormalManagerDB.add(13, new CmdCaddieInfo(_session.m_pi.uid, CmdCaddieInfo.TYPE.ALL), SQLDBResponse, _session);

                            NormalManagerDB.add(14, new CmdMascotInfo(_session.m_pi.uid, CmdMascotInfo.TYPE.ALL), SQLDBResponse, _session);

                            NormalManagerDB.add(15, new CmdWarehouseItem(_session.m_pi.uid, CmdWarehouseItem.TYPE.ALL), SQLDBResponse, _session);

                            break;
                        }
                    case 12:    // Character Info
                        {

                            var pi = _session.m_pi;

                            pi.mp_ce = ((CmdCharacterInfo)(_pangya_db)).getAllInfo(); // cmd_ci.getAllInfo();

                            pi.ei.char_info = null;

                            // Add Structure de estado do lounge para cada character do player
                            foreach (var el in pi.mp_ce)
                            {
                                pi.mp_scl.Add(el.Value.id, new StateCharacterLounge());
                            }

                            // Att Character Equipado que não tem nenhum character o player
                            if (pi.ue.character_id == 0 || pi.mp_ce.Count() <= 0)
                                pi.ue.character_id = 0;
                            else
                            { // Character Info(CharEquip)

                                // É um Map, então depois usa o find com a Key, que é mais rápido que rodar ele em um loop
                                var it = pi.mp_ce.Where(c => c.Key == pi.ue.character_id);

                                if (it.Any())
                                    pi.ei.char_info = it.First().Value;
                            }

                            // teste Calcula a condição do player e o sexo
                            // Só faz calculo de Quita rate depois que o player
                            // estiver no level Beginner E e jogado 50 games
                            if (pi.level >= 6 && pi.ui.jogado >= 50)
                            {
                                float rate = pi.ui.getQuitRate();

                                if (rate < GOOD_PLAYER_ICON)
                                    pi.mi.state_flag.azinha = true;
                                else if (rate >= QUITER_ICON_1 && rate < QUITER_ICON_2)
                                    pi.mi.state_flag.quiter_1 = true;
                                else if (rate >= QUITER_ICON_2)
                                    pi.mi.state_flag.quiter_2 = true;
                            }

                            if (pi.ei.char_info != null && pi.ui.getQuitRate() < GOOD_PLAYER_ICON)
                                pi.mi.state_flag.icon_angel = pi.ei.char_info.AngelEquiped() == 1? true: false;
                            else
                                pi.mi.state_flag.icon_angel = false;

                            pi.mi.state_flag.sexo = pi.mi.sexo == 1 ? true: false;

                            break;
                        }
                    case 13:    // Caddie Info
                        {

                            var pi = _session.m_pi;

                            pi.mp_ci = ((CmdCaddieInfo)(_pangya_db)).getInfo();   // cmd_cadi.getInfo();

                            // Check Caddie Times
                            player_manager.checkCaddie(_session);
                           
                            pi.ei.cad_info = null;

                            // Att Caddie Equipado que não tem nenhum caddie o player
                            if (pi.ue.caddie_id == 0 || pi.mp_ci.Count() <= 0)
                                pi.ue.caddie_id = 0;
                            else
                            { // Caddie Info

                                // É um Map, então depois usa o find com a Key, qui é mais rápido que rodar ele em um loop
                                var it = pi.mp_ci.Where(c => c.Key == pi.ue.caddie_id);

                                if (it.Any())
                                    pi.ei.cad_info = it.First().Value;
                            }
                            break;
                        }
                    case 14:    // Mascot Info
                        {

                            var pi = _session.m_pi;

                            pi.mp_mi = ((CmdMascotInfo)(_pangya_db)).getInfo(); // cmd_mi.getInfo();

                            // Check Mascot Times
                            player_manager.checkMascot(_session);

                            // Att Mascot Equipado que não tem nenhum mascot o player
                            if (pi.ue.mascot_id == 0 || pi.mp_mi.Count() <= 0)
                                pi.ue.mascot_id = 0;
                            else
                            { // Mascot Info

                                // É um Map, então depois usa o find com a Key, qui é mais rápido que rodar ele em um loop
                                var it = pi.mp_mi.Where(c => c.Key == pi.ue.mascot_id);

                                if (it.Any())
                                    pi.ei.mascot_info = it.First().Value;
                            }
                            break;
                        }
                    case 15:    // Warehouse Item
                        {

                            var pi = _session.m_pi;

                            pi.mp_wi = ((CmdWarehouseItem)(_pangya_db)).getInfo();    // cmd_wi.getInfo();

                            // Check Warehouse Item Times
                            player_manager.checkWarehouse(_session);

                            // Iterator
                            Dictionary<stIdentifyKey, UpdateItem> ui_ticket_report_scroll;

                            //Verifica se tem Ticket Report Scroll no update item para abrir ele e excluir. Todos que estiver, não só 1
                            while ((ui_ticket_report_scroll = pi.findUpdateItemByTypeidAndType(TICKET_REPORT_SCROLL_TYPEID, UpdateItem.UI_TYPE.WAREHOUSE)).Count > 0)
                            {

                                try
                                {

                                    //var pWi = pi.findWarehouseItemById(ui_ticket_report_scroll.FirstOrDefault().Value.id);

                                    //if (pWi != null)
                                    //    item_manager.openTicketReportScroll(_session, pWi.id, (uint)((pWi.c[1] * 0x800) | pWi.c[2]));

                                }
                                catch (exception e)
                                {

                                    _smp.message_pool.push("[checkWarehouse][ErrorSystem] " + e.getFullMessageError());    
                                    //if (e.getCodeError() == STDA_ERROR_TYPE._ITEM_MANAGER)
                                    //    throw new exception("[SQLDBResponse][Error] " + e.getFullMessageError(), STDA_ERROR_TYPE.LOGIN_MANAGER);
                                    //else
                                    //    throw;  // Relança
                                }
                            }


                            var it = pi.findWarehouseItemById(pi.ue.clubset_id);

                            // Att ClubSet Equipado que não tem nenhum clubset o player
                            if (pi.ue.clubset_id != 0 && it != null)
                            { // ClubSet Info

                                pi.ei.clubset = it;

                                // Esse C do WarehouseItem, que pega do DB, não é o ja updado inicial da taqueira é o que fica tabela enchant, 
                                // que no original fica no warehouse msm, eu só confundi quando fiz
                                // [AJEITEI JA] (tem que ajeitar na hora que coloca no DB e no DB isso)
                                pi.ei.csi.setValues(it.id, it._typeid, it.c);

                                var cs = sIff.getInstance().findClubSet(it._typeid);

                                if (cs != null)
                                {

                                    for (var i = 0u; i < 5; ++i)
                                        pi.ei.csi.enchant_c[i] = (short)(cs.Stats.getSlot()[i] + it.clubset_workshop.c[i]);

                                }
                                else
                                    _smp.message_pool.push("[SQLDBResponse][Erro] player[UID=" + (pi.uid) + "] tentou inicializar ClubSet[TYPEID="
                                            + (it._typeid) + ", ID=" + (it.id) + "] equipado, mas ClubSet Not exists on IFF_STRUCT do Server. Bug");

                            }
                            else
                            {

                                it = pi.findWarehouseItemByTypeid(AIR_KNIGHT_SET);

                                if (it == null)
                                {

                                    pi.ue.clubset_id = it.id;
                                    pi.ei.clubset = it;

                                    //// Esse C do WarehouseItem, que pega do DB, não é o ja updado inicial da taqueira é o que fica tabela enchant, 
                                    //// que no original fica no warehouse msm, eu só confundi quando fiz
                                    //// [AJEITEI JA] (tem que ajeitar na hora que coloca no DB e no DB isso)
                                    pi.ei.csi.setValues(it.id, it._typeid, it.c);   

                                    var cs = sIff.getInstance().findClubSet(it._typeid);

                                    if (cs != null)
                                    {                          
                                        for (var i = 0u; i < 5; ++i)
                                            pi.ei.csi.enchant_c[i] = (short)(cs.Stats.getSlot()[i] + it.clubset_workshop.c[i]);

                                    }
                                    else
                                        _smp.message_pool.push("[SQLDBResponse][Erro] player[UID=" + (pi.uid) + "] tentou inicializar ClubSet[TYPEID="
                                                + (it._typeid) + ", ID=" + (it.id) + "] equipado, mas ClubSet Not exists on IFF_STRUCT do Server. Bug");


                                }
                                else
                                {   // Não tem add o ClubSet padrão para ele(CV1)

                                    _smp.message_pool.push("[SQLDBResponse][WARNING] Player[UID=" + (pi.uid)
                                            + "] nao tem o ClubSet[TYPEID=" + (AIR_KNIGHT_SET) + "] padrao.");

                                    BuyItem bi = new BuyItem();
                                    stItem item = new stItem();

                                    bi.id = -1;
                                    bi._typeid = AIR_KNIGHT_SET;
                                    bi.qntd = 1;

                                    item_manager.initItemFromBuyItem(pi, ref @item, bi, false, 0, 0, 1/*Não verifica o Level*/);

                                    //if (item._typeid != 0 && (item.id = (uint)item_manager.addItem(ref item, _session, 2/*Padrão item*/, 0)) != -4
                                    //    && (it = pi.findWarehouseItemById(item.id)) != null)
                                    //{

                                    //    pi.ue.clubset_id = it.id;
                                    //    pi.ei.clubset = it;

                                    //    // Esse C do WarehouseItem, que pega do DB, não é o ja updado inicial da taqueira é o que fica tabela enchant, 
                                    //    // que no original fica no warehouse msm, eu só confundi quando fiz
                                    //    // [AJEITEI JA] (tem que ajeitar na hora que coloca no DB e no DB isso)
                                    //    pi.ei.csi.setValues(it.id, it._typeid, it.c);

                                    //    var cs = sIff.getInstance().findClubSet(it._typeid);

                                    //    if (cs != null)
                                    //    {

                                    //        for (var i = 0u; i < 5; ++i)
                                    //            pi.ei.csi.enchant_c[i] = (short)(cs.Stats.getSlot()[i] + it.clubset_workshop.c[i]);

                                    //    }
                                    //    else
                                    //        _smp.message_pool.push("[SQLDBResponse][Erro] player[UID=" + (pi.m_uid) + "] tentou inicializar ClubSet[TYPEID="
                                    //            + (it._typeid) + ", ID=" + (it.id) + "] equipado, mas ClubSet Not exists on IFF_STRUCT do Server. Bug");


                                    //}
                                    //else
                                    //    throw new exception("[SQLDBResponse][Error] Player[UID=" + (pi.m_uid)
                                    //            + "] nao conseguiu adicionar o ClubSet[TYPEID=" + (AIR_KNIGHT_SET) + "] padrao para ele. Bug");
                                
                                } 
                            }

                            // Atualiza Comet(Ball) Equipada
                            var it_ball = pi.findWarehouseItemByTypeid(pi.ue.ball_typeid);
                            if (pi.ue.ball_typeid != 0)
                            {
                                pi.ei.comet = it_ball;
                            }
                            else
                            { // Default Ball

                                pi.ue.ball_typeid = DEFAULT_COMET_TYPEID;

                                it = pi.findWarehouseItemByTypeid(DEFAULT_COMET_TYPEID);

                                if (it != pi.mp_wi.Last().Value)
                                {
                                    pi.ei.comet = it;
                                }
                                else
                                {   // não tem add a bola padrão para ele

                                    //_smp.message_pool.push("[SQLDBResponse][WARNING] Player[UID=" + (_pi.m_uid)
                                    //        + "] nao tem a Comet(Ball)[TYPEID=" + (DEFAULT_COMET_TYPEID) + "] padrao.");

                                    //BuyItem bi;
                                    //stItem item;

                                    //bi.id = -1;
                                    //bi._typeid = DEFAULT_COMET_TYPEID;
                                    //bi.qntd = 1;

                                    //item_manager.initItemFromBuyItem(*_pi, item, bi, false, 0, 0, 1/*Não verifica o Level*/);

                                    //if (true)
                                    //{

                                    //    _pi.ei.comet = &it.second;

                                    //}
                                    //else
                                    //{
                                    //    throw new exception("[SQLDBResponse][Error] Player[UID=" + (_pi.m_uid)
                                    //            + "] nao conseguiu adicionar a Comet(Ball)[TYPEID=" + (DEFAULT_COMET_TYPEID) + "] padrao para ele. Bug");
                                    //}

                                }
                            }

                            // Premium Ticket Tem que ser chamado depois que o Warehouse Item ja foi carregado
                            NormalManagerDB.add(4, new Cmd.CmdPremiumTicketInfo(_session.m_pi.uid), SQLDBResponse, _session);

                            break;
                        }
                    case 16:    // Config MyRoom
                        {

                            _session.m_pi.mrc = ((CmdMyRoomConfig)(_pangya_db)).getMyRoomConfig();   // cmd_mrc.getMyRoomConfig();

                            NormalManagerDB.add(17, new Cmd.CmdMyRoomItem(_session.m_pi.uid, CmdMyRoomItem.TYPE.ALL), SQLDBResponse, _session);
                            break;
                        }
                    case 17:    // MyRoom Item Info
                        {
                            _session.m_pi.v_mri = ((CmdMyRoomItem)(_pangya_db)).getMyRoomItem(); // cmd_mri.getMyRoomItem();

                            break;
                        }
                    case 18:    // Check if have Achievement
                        {
                            // --------------------- AVISO ----------------------
                            // esse aqui os outros tem que depender dele para, não ir sem ele
                            var cmd_cAchieve = (CmdCheckAchievement)(_pangya_db);

                            // Cria Achievements do player
                            if (!cmd_cAchieve.getLastState())
                            {
                                _session.m_pi.mgr_achievement.initAchievement(_session.m_pi.uid, true/*Create sem verifica se o player tem achievement, por que aqui ele já verificou*/);
                                
                                // Add o Task + 1 por que não pede o achievement do db, porque criou ele aqui e salvo no DB
                                incremenetCount();

                            }
                            else
                            {
                                NormalManagerDB.add(19, new CmdAchievementInfo(_session.m_pi.uid), SQLDBResponse, _session);
                            }

                        }
                        break;
                    case 19:    // Achievement Info
                        {
                            var cmd_ai = ((CmdAchievementInfo)(_pangya_db));

                            // Inicializa o Achievement do player
                            _session.m_pi.mgr_achievement.initAchievement(_session.m_pi.uid, cmd_ai.GetInfo());

                            break;
                        }
                    case 20:    // Daily Quest User Info
                        {
                            _session.m_pi.dqiu = ((CmdDailyQuestInfoUser)(_pangya_db)).GetInfo();    // cmd_dqiu.getInfo();
                                                                                                     //                                                              // fim daily quest info player

                            break;
                        }
                    case 21:    // Card Info
                        {
                            _session.m_pi.v_card_info = ((CmdCardInfo)(_pangya_db)).getInfo();   // cmd_cardi.getInfo();

                            break;
                        }
                    case 22:    // Card Equipped Info
                        {
                            _session.m_pi.v_cei = ((CmdCardEquipInfo)(_pangya_db)).getInfo();    // cmd_cei.getInfo();

                            // Check Card Special Times
                            player_manager.checkCardSpecial(_session);

                            break;
                        }
                    case 23:    // Trofel especial normal atual
                        {
                            _session.m_pi.v_tsi_current_season = ((CmdTrophySpecial)(_pangya_db)).getInfo();

                            break;
                        }
                    case 24:    // Trofel especial grand prix atual
                        {
                            _session.m_pi.v_tgp_current_season = ((CmdTrophySpecial)_pangya_db).getInfo(); // cmd_tei.getInfo();

                            break;
                        }
                    case 26:    // MapStatistics normal, atual
                        {
                            var v_ms = ((CmdMapStatistics)(_pangya_db)).getMapStatistics(); // cmd_ms.getMapStatistics();

                            try
                            {
                                foreach (var i in v_ms)
                                {
                                    _session.m_pi.a_ms_normal[i.course] = i;
                                }

                            }
                            catch (Exception ex)
                            {           
                                throw ex;
                            }
                            break;
                        }
                    case 27:    // MapStatistics Normal, assist, atual
                        {
                            var v_ms = ((CmdMapStatistics)(_pangya_db)).getMapStatistics(); // cmd_ms.getMapStatistics();

                            foreach (var i in v_ms)
                            {
                                _session.m_pi.a_msa_normal[i.course] = i;
                            }

                            break;
                        }
                    case 28:    // MapStatistics Natural, atual
                        {
                            var v_ms = ((CmdMapStatistics)(_pangya_db)).getMapStatistics(); // cmd_ms.getMapStatistics();

                            foreach (var i in v_ms)
                            {
                                _session.m_pi.a_ms_natural[i.course] = i;
                            }

                            break;
                        }
                    case 29:    // MapStatistics Natural, assist, atual
                        {

                            var v_ms = ((CmdMapStatistics)(_pangya_db)).getMapStatistics(); // cmd_ms.getMapStatistics();

                            foreach (var i in v_ms)
                            {
                                _session.m_pi.a_msa_natural[i.course] = i;
                            }

                            break;
                        }
                    case 30:    // MapStatistics GrandPrix, atual
                        {
                            var v_ms = ((CmdMapStatistics)(_pangya_db)).getMapStatistics(); // cmd_ms.getMapStatistics();


                            foreach (var i in v_ms)
                            {
                                _session.m_pi.a_ms_grand_prix[i.course] = i;
                            }

                            break;
                        }
                    case 31:    // MapStatistics GrandPrix, Assist, atual
                        {
                            var v_ms = ((CmdMapStatistics)(_pangya_db)).getMapStatistics(); // cmd_ms.getMapStatistics();

                            foreach (var i in v_ms)
                            {
                                _session.m_pi.a_msa_grand_prix[i.course] = i;
                            }
                            break;
                        }
                    case 32:    // [MailBox] New Email(s), Agora é a inicialização do Cache do Mail Box
                        {
                            var cmd_mbi2 = ((CmdMailBoxInfo2)(_pangya_db));

                            _session.m_pi.m_mail_box.init(cmd_mbi2.getInfo(), _session.m_pi.uid);

                            var v_mb = _session.m_pi.m_mail_box.getAllUnreadEmail();

                            _session.Send(packet_func_sv.pacote210(v_mb));
                            break;
                        }
                    case 33:    // Aviso Caddie Ferias
                        {
                            var v_cif = ((CmdCaddieInfo)(_pangya_db)).getInfo();    // cmd_cadi.getInfo();

                            if (v_cif.Any())
                            {

                                _session.Send(packet_func_sv.pacote0D4(v_cif));         
                            }
                            break;
                        }
                    case 34:    // Msg Off Info
                        {
                            var v_moi = ((CmdMsgOffInfo)(_pangya_db)).GetInfo();    // cmd_moi.getInfo();

                            if (!v_moi.Any())
                            {

                                _session.Send(packet_func_sv.pacote0B2(v_moi));         

                            }

                            break;
                        }
                    case 35:    // YamEquipedInfo ItemBuff(item que da um efeito, por tempo)
                        {
                            _session.m_pi.v_ib = ((CmdItemBuffInfo)(_pangya_db)).GetInfo();  // cmd_yei.getInfo();

                            //// Check Item Buff Times
                            player_manager.checkItemBuff(_session);

                            break;
                        }
                    case 36:    // Chat Macro User
                        {
                            _session.m_pi.cmu = ((CmdChatMacroUser)(_pangya_db)).getMacroUser();
                            break;
                        }
                    case 37:    // Last 5 Player Game Info
                        {
                            _session.m_pi.l5pg = ((CmdLastPlayerGameInfo)(_pangya_db)).getInfo();
                            break;
                        }
                    case 38:    // Friend List
                        {
                            _session.m_pi.mp_fi = ((CmdFriendInfo)(_pangya_db)).getInfo();
                            break;
                        }
                    case 39:    // Attendance Reward Info
                        {
                            _session.m_pi.ari = ((CmdAttendanceRewardInfo)(_pangya_db)).getInfo();
                            break;
                        }
                    case 40:    // Register Player Logon ON DB
                        {
                            // Não usa por que é um UPDATE
                            break;
                        }
                    case 41:    // Register Logon of player on Server in DB
                        {
                            // Não usa por que é um UPDATE
                            break;
                        }
                    case 42:    // Grand Prix Clear
                        {
                            _session.m_pi.v_gpc = ((CmdGrandPrixClear)(_pangya_db)).getInfo();

                            break;
                        }
                    case 43: // Grand Zodiac Pontos
                        {
                            _session.m_pi.grand_zodiac_pontos = ((CmdGrandZodiacPontos)(_pangya_db)).getPontos();

                            break;
                        }
                    case 44: // Legacy Tiki Shop(PointShop)
                        {
                            _session.m_pi.m_legacy_tiki_pts = ((CmdLegacyTikiShopInfo)(_pangya_db)).getInfo();

                            break;
                        }
                    default:
                        break;
                }

                // Incrementa o contador
                incremenetCount();

                if (getCount() == 39) // 44 - 5 (38 deixei o 1, 2, 3, 40 e 41 para o game server)
                {
                    sendCompleteData(_session);//send packets for login....
                }
                else if (getCount() > 0)
                {
                    _session.Send(packet_func_sv.pacote044(Program.gs.m_si, 0xD2, _session.m_pi, _msg_id == 10? _msg_id + 2 : _msg_id + 1)); // send bar loading server!
                }                
               
            }
            catch (Exception ex)
            {
                _smp.message_pool.push(new message(
              $"[LoginSystem::SQLDBResponse][ErrorSystem] {ex.Message}\nStack Trace: {ex.StackTrace}",
              type_msg.CL_FILE_LOG_AND_CONSOLE));
                if (_session != null && _session.getConnected())
                    Program.gs.DisconnectSession(_session);
            }
        }

        void sendCompleteData(Player _session)
        {
            //// Verifica se a session ainda é valida, essas funções já é thread-safe
            if (!_session.getConnected())
            {

                _smp.message_pool.push("[LoginSystem.sendCompleteData][Error] session is invalid.");
                _session.Disconnect();
                return;
            }

            try
            {

                var stopwatch = Stopwatch.StartNew();

                //// Check All Character All Item Equiped is on Warehouse Item of Player
                foreach (var el in _session.m_pi.mp_ce)
                {
                    // Check Parts of Character e Check Aux Part of Character
                    _session.checkCharacterAllItemEquiped(el.Value);
                }

                // Check All Item Equiped
                _session.checkAllItemEquiped(_session.m_pi.ue);


                var pi = _session.m_pi;
                // Envia todos pacotes aqui, alguns envia antes, por que agora estou usando o jeito o pangya original   
                _session.Send(packet_func_sv.pacote044(Program.gs.m_si, 0, pi));
                                               
                _session.Send(packet_func_sv.pacote070(pi.mp_ce)); // characters
                                                              
                _session.Send(packet_func_sv.pacote071(pi.mp_ci)); //caddies   

               _session.Send(pi.mp_wi.Build()); //inventory(warehouse)   

                _session.Send(packet_func_sv.pacote0E1(pi.mp_mi)); //mascots

               _session.Send(packet_func_sv.pacote072(pi.ue)); // equip selected                     

                Program.gs.sendChannelListToSession(_session);

                _session.Send(packet_func_sv.pacote102(pi));        // Pacote novo do JP, passa os coupons do Gacha JP

                // Treasure Hunter Info
                //_session.Send(packet_func_sv.pacote131());

                _session.m_pi.mgr_achievement.sendCounterItemToPlayer(_session);

                _session.m_pi.mgr_achievement.sendAchievementToPlayer(_session);

                _session.Send(packet_func_sv.pacote0F1());

                _session.Send(packet_func_sv.pacote144());		// Pacote novo do JP

                _session.Send(packet_func_sv.pacote135()); 

                _session.Send(packet_func_sv.pacote138(pi.v_card_info)); 

                _session.Send(packet_func_sv.pacote136()); 

                _session.Send(packet_func_sv.pacote137(pi.v_cei)); 

                _session.Send(packet_func_sv.pacote13F()); 

                _session.Send(packet_func_sv.pacote181(pi.v_ib));

                _session.Send(packet_func_sv.pacote096(pi));

                _session.Send(packet_func_sv.pacote169(pi.ti_current_season, 5/*season atual*/));


                _session.Send(packet_func_sv.pacote169(pi.ti_rest_season));


                _session.Send(packet_func_sv.pacote0B4(pi.v_tsi_current_season, 5/*season atual*/));


                _session.Send(packet_func_sv.pacote0B4(pi.v_tsi_rest_season));


                _session.Send(packet_func_sv.pacote158(pi.uid, pi.ui, 0));
                // Total de season, 5 atual season

                _session.Send(packet_func_sv.pacote25D(pi.v_tgp_current_season, 5/*season atual*/));


                _session.Send(packet_func_sv.pacote25D(pi.v_tgp_rest_season, 0));

                var p = new PangyaBinaryWriter(0x1B1);
                ///UCC COMPRESS
                p.WriteUInt64(0x190132DC55);
                p.WriteUInt64(0x19);
                p.WriteZero(13);    
                p.WriteUInt32(0x1100);//@@@@@ aqui diz que esta compresss
                _session.Send(p);

                //// Login Reward System - verifica se o player ganhou algum item por logar
                //if (sgs::gs::getInstance().getInfo().rate.login_reward_event)
                //    sLoginRewardSystem::getInstance().checkRewardLoginAndSend(_session);  
                stopwatch.Stop();
                _smp.message_pool.push(new message($"[LoginSystem.sendCompleteData][Log] Function executed in {stopwatch.ElapsedMilliseconds}ms", type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
            catch (Exception ex)
            {
                _smp.message_pool.push(new message($"[LoginSystem.sendCompleteData][ErrorSystem] {ex.Message}\nStack Trace: {ex.StackTrace}", type_msg.CL_FILE_LOG_AND_CONSOLE));
            }

        }

        uint getCount()
        {
            return m_count;
        }

        void incremenetCount()
        {
            ++m_count;
        }
    }
}
