using GameServer.Cmd;
using GameServer.PacketFunc;
using GameServer.PangType;
using GameServer.Session;
using PangyaAPI.Network.Cmd;
using PangyaAPI.Network.Pangya_St;
using PangyaAPI.Network.PangyaPacket;
using PangyaAPI.Utilities;
using PangyaAPI.Utilities.Log;
using _smp = PangyaAPI.Utilities.Log;
using snmdb = PangyaAPI.SQL.Manager;
namespace GameServer.PangSystem
{
    /// <summary>
    /// Class manipulation login player!
    /// checks and funcs
    /// </summary>
    public static class LoginSystem
    {  
        /// <summary>
        /// Handle login 
        /// </summary>
        /// <param db_name="_Packet">bits recevied by projectg</param>
        /// <param db_name="_session">client = session</param>
        public static void requestLogin(Player _session, Packet _Packet)
        {
            Packet p;

            try
            {

                uint Packet_version = 0;

                KeysOfLogin kol = new KeysOfLogin();
                AuthKeyInfo akli = new AuthKeyInfo();
                AuthKeyGameInfo akgi = new AuthKeyGameInfo();

                string client_version;

                // Temp que vai guarda os dados que o cliente enviou para fazer o login com o gs
                PlayerInfo _pi = new PlayerInfo();

                // Player info da session e vai guardar os valores recuperados do banco de dados
                PlayerInfo pi = (_session.m_pi);

                //////////// ----------------------- Começa a ler o Packet que o cliente enviou ------------------------- \\\\\\\\\\\
                // Read Packet Client request
                _pi.id = _Packet.ReadString();
                _pi.uid = _Packet.ReadUInt32();
                var ntKey = _Packet.ReadUInt32(); // ntKey
                var Command = _Packet.ReadUInt16();
                kol.keys[0] = _Packet.ReadString();
                client_version = _Packet.ReadString();
                Packet_version = _Packet.ReadUInt32();
                string mac_address = _Packet.ReadString();
                kol.keys[1] = _Packet.ReadString();
                // -------------- Finished reading the Packet sent by the client ---------------


                ////////////----------------------- Terminou a leitura do Packet que o cliente enviou -------------------------\\\\\\\\\\\/

                // Verifica aqui se o IP/MAC ADDRESS do player está bloqueado
                if (Program.gs.haveBanList(_session.getIP(), mac_address, !mac_address.empty()))
                    throw new exception("[game_server::requestLogin][Error] Player[UID=" + (_pi.uid) + ", IP="
                            + _session.getIP() + ", MAC=" + mac_address + "] esta bloqueado por regiao IP/MAC Addrress.");

                // Aqui verifica se recebeu os dados corretos
                if (_pi.id[0] == '\0')
                    throw new exception("[game_server::requestLogin][Error] Player[UID=" + (_pi.uid)
                            + ", IP=" + _session.getIP() + "] id que o player enviou eh invalido. id: " + (_pi.id));

                // Verifica se o gs está mantle, se tiver verifica se o player tem capacidade para entrar
                var cmd_pi = new CmdPlayerInfo(_pi.uid); // Waiter

                snmdb.NormalManagerDB.add(0, cmd_pi, null, null);

                cmd_pi.ExecCmd();

                if (cmd_pi.getException().getCodeError() != 0)
                    throw cmd_pi.getException();

                pi.uid = cmd_pi.getInfo().uid;
                pi.level = cmd_pi.getInfo().level;
                pi.block_flag = cmd_pi.getInfo().block_flag;
                pi.nickname = cmd_pi.getInfo().nickname;
                pi.pass = cmd_pi.getInfo().pass;  
                _session.m_pi = _pi;

                if (pi.uid <= 0)
                    throw new exception("[game_server::requestLogin][Error] player[UID=" + (_pi.uid) + "] nao existe no banco de dados");

                // UID de outro player ou enviou o ID errado mesmo (essa parte é anti-hack ou bot)
                if (string.Compare(pi.id, _pi.id) ==0 )
                    throw new exception("[game_server::requestLogin][Error] Player[UID=" + (pi.uid) + ", REQ_UID="
                            + (_pi.uid) + "] Player ID nao bate : client send ID : " + (_pi.id) + "\t player DB ID : "
                            + (pi.id));
                // Verifica aqui se a conta do player está bloqueada
                if (pi.block_flag.m_id_state.id_state.ull_IDState != 0)
                {

                    if (pi.block_flag.m_id_state.id_state.st_IDState.L_BLOCK_TEMPORARY && (pi.block_flag.m_id_state.block_time == -1 || pi.block_flag.m_id_state.block_time > 0))
                    {

                        throw new exception("[game_server::requestLogin][Log] Bloqueado por tempo[Time="
                                + (pi.block_flag.m_id_state.block_time == -1 ? ("indeterminado") : ((pi.block_flag.m_id_state.block_time / 60)
                                + "min " + (pi.block_flag.m_id_state.block_time % 60) + "sec"))
                                + "]. player [UID=" + (pi.uid) + ", ID=" + (pi.id) + "]");

                    }
                    else if (pi.block_flag.m_id_state.id_state.st_IDState.L_BLOCK_FOREVER)
                    {

                        throw new exception("[game_server::requestLogin][Log] Bloqueado permanente. player [UID=" + (pi.uid)
                                + ", ID=" + (pi.id) + "]");
                    }
                    else if (pi.block_flag.m_id_state.id_state.st_IDState.L_BLOCK_ALL_IP)
                    {

                        // Bloquea todos os IP que o player logar e da error de que a area dele foi bloqueada

                        // Add o ip do player para a lista de ip banidos
                        snmdb.NormalManagerDB.add(9, new CmdInsertBlockIp(_session.getIP(), "255.255.255.255"), Program.gs.SQLDBResponse, Program.gs);

                        // Resposta
                        throw new exception("[game_server::requestLogin][Log] Player[UID=" + (pi.uid) + ", IP=" + (_session.getIP())
                                + "] Block ALL IP que o player fizer login.");
                    }
                    else if (pi.block_flag.m_id_state.id_state.st_IDState.L_BLOCK_MAC_ADDRESS)
                    {

                        // Bloquea o MAC Address que o player logar e da error de que a area dele foi bloqueada

                        // Add o MAC Address do player para a lista de MAC Address banidos
                        snmdb.NormalManagerDB.add(10, new CmdInsertBlockMac(mac_address), Program.gs.SQLDBResponse, Program.gs);

                        // Resposta
                        throw new exception("[game_server::requestLogin][Log] Player[UID=" + (pi.uid)
                                + ", IP=" + (_session.getIP()) + ", MAC=" + mac_address + "] Block MAC Address que o player fizer login.");

                    }
                }

                // Check Packet version
                _Packet.Version_Decrypt(@Packet_version);


                //// Se a flag do canSameIDLogin estiver ativo, não verifica Packet
                //if (!m_login_manager.canSameIDLogin() && Packet_version != m_si.Packet_version)
                //{
                //	// Error no login, set falso o autoriza o player a continuar conectado com o Game Server
                //	_session.m_is_authorized = 0;

                //	// Error Sistema
                //	Packet p((unsigned short)0x44);

                //	// Pronto agora sim, mostra o erro que eu quero
                //	p.addInt32(0x0B);

                //	Packet_func.session_send(p,_session, 1);

                //	// Disconnect

                //}
                // Verifica o Auth Key do player
                var cmd_akli = new CmdAuthKeyLoginInfo((int)pi.uid); // Waiter

                snmdb.NormalManagerDB.add(0, cmd_akli, null, null);

                cmd_akli.ExecCmd();

                if (cmd_akli.getException().getCodeError() != 0)
                    throw cmd_akli.getException();
                //false  = true, true = false
                //	// ### Isso aqui é uma falha de segurança faltal, muito grande nunca posso deixar isso ligado depois que colocar ele online
                //	if (!m_login_manager.canSameIDLogin() && !cmd_akli.getInfo().valid)
                //		throw exception("[game_server::requestLogin][Error] Player[UID=" + (pi.uid) + "].\tAuthKey ja foi utilizada antes.", STDA_MAKE_ERROR(STDA_ERROR_TYPE::GAME_SERVER, 1056, 0));

                //	// ### Isso aqui é uma falha de segurança faltal, muito grande nunca posso deixar isso ligado depois que colocar ele online
                //	if (!m_login_manager.canSameIDLogin() &&
                //string.Compare(kol.keys[0], cmd_akli.getInfo().key) != 0
                //	)
                //		throw new exception("[game_server::requestLogin][Error] Player[UID=" + (pi.uid) + "].\tAuthKey no bate(no match).", STDA_MAKE_ERROR(STDA_ERROR_TYPE::GAME_SERVER, 1057, 0));

                //	ClientVersion cv_side_sv = ClientVersion::make_version(const_cast <  &> (m_login_manager.getClientVersionSideServer()));
                //	auto cv_side_c = ClientVersion::make_version(client_version);

                //	if (cv_side_c.flag == ClientVersion::COMPLETE_VERSION && strcmp(cv_side_c.region, cv_side_sv.region) == 0
                //			&& strcmp(cv_side_c.season, cv_side_sv.season) == 0)
                //	{

                //		if (cv_side_c.high != cv_side_sv.high || cv_side_c.low < cv_side_sv.low)
                //		{
                //			_smp.message_pool.push(("[game_server::requestLogin][WARNING] Player[UID=" + (pi.uid) + "].\tClient Version not match. Server: "
                //					+ (m_login_manager.getClientVersionSideServer()) + " == Client: " + cv_side_c.toString(), CL_ONLY_FILE_LOG));

                //			pi.block_flag.m_flag.stBit.all_game = 1;// |= BLOCK_PLAY_ALL;
                //		}

                //	}
                //	else if (cv_side_c.high != cv_side_sv.high || cv_side_c.low < cv_side_sv.low)
                //	{

                //		_smp.message_pool.push(("[game_server::requestLogin][WARNING] Player[UID=" + (pi.uid) + "].\tClient Version not match. Server: "
                //				+ (m_login_manager.getClientVersionSideServer()) + " == Client: " + cv_side_c.toString(), CL_ONLY_FILE_LOG));

                //		pi.block_flag.m_flag.stBit.all_game = 1;// |= BLOCK_PLAY_ALL;
                //	}

                // Member Info
                var cmd_mi = new CmdMemberInfo(pi.uid);    // Waiter

                snmdb.NormalManagerDB.add(0, cmd_mi, null, null);

                cmd_mi.ExecCmd();

                if (cmd_mi.getException().getCodeError() != 0)
                    throw cmd_mi.getException();

                _session.m_pi.mi = cmd_mi.getInfo();
                // Passa o Online ID para a estrutura MemberInfo, para não da erro depois
                pi.mi.oid = _session.m_oid;
                pi.mi.state_flag.stFlagBit.visible = 1;
                pi.mi.state_flag.stFlagBit.whisper = pi.whisper;
                pi.mi.state_flag.stFlagBit.channel = (byte)~pi.whisper;

                if (pi.m_cap.stBit.game_master.IsTrue())
                {
                    _session.m_gi.setGMUID(pi.uid);    // Set o UID do GM dados

                    pi.mi.state_flag.stFlagBit.visible = _session.m_gi.visible;
                    pi.mi.state_flag.stFlagBit.whisper = _session.m_gi.whisper;
                    pi.mi.state_flag.stFlagBit.channel = _session.m_gi.channel;
                }

                // Verifica se o player tem a capacidade e level para entrar no gs
                if (Program.gs.m_si.propriedade.stBit.only_rookie.IsTrue() && pi.level >= 6/*Beginner E maior*/)
                    throw new exception("[game_server::requestLogin][Error] Player[UID=" + (pi.uid) + ", LEVEL="
                            + ((short)pi.level) + "] nao pode entrar no gs por que o gs eh so para rookie.");
                /*Nega ele não pode ser nenhum para lançar o erro*/
                if (Program.gs.m_si.propriedade.stBit.mantle.IsTrue() && !(pi.m_cap.stBit.mantle.IsTrue() || pi.m_cap.stBit.game_master.IsTrue()))
                    throw new exception("[game_server::requestLogin][Error] Player[UID=" + (pi.uid) + ", CAP=" + (pi.m_cap.ulCapability)
                            + "] nao tem a capacidade para entrar no gs mantle.");
                // Verifica se o Player já está logado
                var player_logado = Program.gs.HasLoggedWithOuterSocket(_session);

                if (player_logado != null)
                {
                    //if (!m_login_manager.canSameIDLogin())
                    //{
                    //	_smp.message_pool.push(("[game_server::requestLogin][Log] Player[UID=" + (_pi.uid) + ", OID="
                    //		+ (_session.m_oid) + ", IP=" + _session.getIP() + "] que esta logando agora, ja tem uma outra session com o mesmo UID logado, desloga o outro Player[UID="
                    //		+ (player_logado.getUID()) + ", OID=" + (player_logado.m_oid) + ", IP=" + player_logado.getIP() + "]", CL_FILE_LOG_AND_CONSOLE));

                    ////	if (!DisconnectSession(player_logado))
                    ////		throw new exception("[game_server::requestLogin][Error] Nao conseguiu disconnectar o player[UID=" + (player_logado.getUID())
                    ////			+ ", OID=" + (player_logado.m_oid) + ", IP=" + player_logado.getIP() + "], ele pode esta com o bug do oid bloqueado, ou SessionBase::UsaCtx bloqueado.");
                    //}
                }

                // Junta Flag de block do gs, ao do player
                pi.block_flag.m_flag.ullFlag |= Program.gs.m_si.flag.ullFlag;

                // Authorized a ficar online no gs por tempo indeterminado
                _session.m_is_authorized = true;

                // Registra no Banco de dados que o player está logado no Game Server
                snmdb.NormalManagerDB.add(5, new CmdRegisterLogon(pi.uid, 0/*Logou*/), Program.gs.SQLDBResponse, Program.gs);

                // Resgistra o Login do Player no gs
                snmdb.NormalManagerDB.add(7, new CmdRegisterLogonServer(pi.uid, Program.gs.m_si.uid), Program.gs.SQLDBResponse, Program.gs);

                _smp.message_pool.push("[game_server::requestLogin][Log] Player[OID=" + (_session.m_oid) + ", UID=" + (pi.uid) + ", NICKNAME="
                        + (pi.nickname) + "] Autenticou com sucesso.");

                //// Verifica se o papel tem limite por dia, se não anula o papel shop do player
                //sPapelShopSystem.init_player_papel_shop_info(_session);

                //snmdb.NormalManagerDB.add(11, new CmdFirstAnniversary(), Program.gs.SQLDBResponse, this);


                // Cria o login manager para carregar o cache das informações e itens completo do player
               Program.gs.m_login_manager.createTask(ref _session, kol, _pi/*esses valores não vai usar mais se ficar tudo bem aqui no game_server*/, Program.gs);


                // Entra com sucesso
                p = new Packet();

                packet_func.pacote044(ref p, _session, Program.gs.m_si, 0xD3);

                // Entra com sucesso
                packet_func.session_send(ref p, _session, 0);

            }
            catch (exception ex)
            {
                _smp.message_pool.push(new message(
             $"[LoginSystem::requestLogin][ErrorSt] {ex.Message}\nStack Trace: {ex.StackTrace}",
             type_msg.CL_FILE_LOG_AND_CONSOLE));

                // Error no login, set falso o autoriza o player a continuar conectado com o Game Server
                _session.m_is_authorized = false;

                // Error Sistema
                p = new Packet();
                p.Write(new byte[] { 0x44, 0x00 });
                // Pronto agora sim, mostra o erro que eu quero
                p.WriteInt32(300);

                packet_func.session_send(ref p,_session, 1);

                // Disconnect

                Program.gs.onDisconnected(_session);
            }
        }
    }
}
