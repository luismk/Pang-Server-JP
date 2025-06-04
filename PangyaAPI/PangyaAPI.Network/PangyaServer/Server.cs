using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using PangyaAPI.Network.Cmd;
using PangyaAPI.Network.Cryptor;
using PangyaAPI.Network.Pangya_St;
using PangyaAPI.Network.PangyaPacket;
using PangyaAPI.Network.PangyaSession;
using PangyaAPI.Network.PangyaUnit;
using PangyaAPI.Network.PangyaUtil;
using PangyaAPI.SQL;
using PangyaAPI.SQL.Manager;
using PangyaAPI.Utilities;
using PangyaAPI.Utilities.Log;
using _smp = PangyaAPI.Utilities.Log;

namespace PangyaAPI.Network.PangyaServer
{
    public enum ServerState
    {
        Uninitialized,
        Good,
        GoodWithWarning,
        Initialized,
        Failure
    }
    public abstract class Server : pangya_packet_handle, IUnitAuthServer
    {
        #region Fields
        private IpDdosFilter _ipFilter;
        private AntiDdosConfig _ddosConfig;

        // Shutdown timer
        public System.Threading.Timer m_shutdown;

        public ServerState m_state;
        //DECRYPT FIELDS

        private List<string> v_mac_ban_list;
        private List<IPBan> v_ip_ban_list;
        public SessionManager m_session_manager;
        public ServerInfoEx m_si = new ServerInfoEx();
        private int m_Bot_TTL; // Anti-bot Time-to-live
        private bool m_chatDiscord;
        public bool _isRunning => m_state == ServerState.Good;
        public IniHandle m_reader_ini { get; set; }
        public List<TableMac> ListBlockMac { get; set; } = new List<TableMac>();
        public List<ServerInfo> m_server_list { get; set; } = new List<ServerInfo>();
        public IntPtr EventMoreAccept { get; private set; }

        public ServerInfoEx getInfo() => m_si;
        public uint getUID() => (uint)(m_si?.uid);
        public TcpListener _server;
        #endregion

        #region Abstract Methods
        public abstract void OnStart();
        /// <summary>
        /// call methods
        /// </summary>
        public abstract void OnHeartBeat();
        /// <summary>
        /// check packet, packet is real
        /// </summary>
        /// <param name="session">client</param>
        /// <param name="_packet">packet read</param>
        /// <param name="opt">0 = server, 1 = client</param>
        /// <returns></returns>
        public abstract bool CheckPacket(Session session, packet _packet, int opt = 0);
        /// <summary>
        /// disconnect players !
        /// </summary>
        /// <param name="_session"></param>
        public abstract void onDisconnected(Session _session);

        /// <summary>
        /// Send Key
        /// </summary>
        /// <param name="_session"></param>
        protected abstract void onAcceptCompleted(Session _session);

        #endregion

        #region Constructor
        public Server(SessionManager manager)
        {
            try
            {
                m_session_manager = manager;

                m_state = ServerState.Uninitialized;

                _ipFilter = new IpDdosFilter();

                config_init();
                // Inicializa o Unit_Connect, que conecta com o Auth Server
                /// m_unit_connect = new unit_auth_server_connect(this, m_si);
            }
            catch (exception e)
            {
                _smp::message_pool.push(new message("[server::construtor][Error] " + e.getFullMessageError(), type_msg.CL_ONLY_CONSOLE));
            }
        }
 
        #endregion

        #region Private Methods    

        public virtual void config_init()
        {
            try
            {
                m_reader_ini = new IniHandle("server.ini");
                m_si = new ServerInfoEx
                {
                    version = m_reader_ini.ReadString("SERVERINFO", "VERSION"),
                    version_client = m_reader_ini.ReadString("SERVERINFO", "CLIENTVERSION"),
                    nome = m_reader_ini.ReadString("SERVERINFO", "NAME", "Pangya Server Csharp"),
                    uid = m_reader_ini.ReadInt32("SERVERINFO", "GUID"),
                    port = m_reader_ini.ReadInt32("SERVERINFO", "PORT"),
                    ip = m_reader_ini.ReadString("SERVERINFO", "IP"),
                    max_user = m_reader_ini.ReadInt32("SERVERINFO", "MAXUSER"),
                    propriedade = new uProperty(m_reader_ini.ReadUInt32("SERVERINFO", "PROPERTY")),
                    rate = new RateConfigInfo(),
                    event_flag = new uEventFlag(),
                    flag = new uFlag(0)
                };
            }
            catch (exception e)
            {
                _smp::message_pool.push(new message("[server::config_init][Error] " + e.getFullMessageError(), type_msg.CL_ONLY_CONSOLE));
            }

            try
            {
                m_Bot_TTL = m_reader_ini.ReadInt32("OPTION", "ANTIBOTTTL", 1000);
                m_si.packet_version = m_reader_ini.ReadUInt32("SERVERINFO", "PACKETVERSION");
            }
            catch (exception e)
            {
                _smp::message_pool.push(new message("[server::config_init][Error] " + e.getFullMessageError(), type_msg.CL_ONLY_CONSOLE));
                m_Bot_TTL = 1000; // Usa o valor padrão do anti bot TTL
            }
        }

        /// <summary>
        /// Aguarda Conexões
        /// </summary>
        private void HandleWaitConnections()
        {
            while (_isRunning)
            {
                try
                {
                    var newClient = _server.AcceptTcpClient();
                    var remoteEndPoint = newClient.Client.RemoteEndPoint as IPEndPoint;
                    string ipAddress = remoteEndPoint?.Address.ToString(); 

                    if (_ipFilter != null && _ipFilter.IsBlocked(ipAddress))
                    {
                        newClient.Close();
                        _smp.message_pool.push(new message($"[Server] Conexão de IP bloqueado: {ipAddress}", type_msg.CL_FILE_LOG_AND_CONSOLE));
                        continue;
                    }

                    _ipFilter?.OnConnect(ipAddress);

                    init_option_accepted_socket(newClient.Client);

                    // Processa o cliente utilizando o pool de threads
                    Task.Run(() => accept_completed(newClient));
                }
                catch (exception e)
                {
                    _smp.message_pool.push(new message(
                        $"[Server.HandleWaitConnections][ErrorSystem] {e.getFullMessageError()}",
                        type_msg.CL_FILE_LOG_AND_CONSOLE));
                }
            }
        }

        /// <summary>
        /// Manuseia Comunicação do Cliente
        /// </summary>
        protected override void accept_completed(TcpClient client)
        {
            //add player
            var _session = m_session_manager.AddSession(this, client, client.Client.RemoteEndPoint as IPEndPoint, (byte)(new Random().Next() % 16));
            //
            _smp.message_pool.push(new message("[server::HandleSession][Log] New Player Connected [IP: " + _session.getIP() + ", Key: " + _session.m_key + "]", type_msg.CL_FILE_LOG_AND_CONSOLE));

            onAcceptCompleted(_session);
            //send key

            while (_session.isConnected())
            {
                try
                {
                    if (!_session.isConnected())
                    {
                        DisconnectSession(_session);
                        break;
                    }

                    if (recv_new(_session))
                    {
                        // Processa o pacote recebido
                    }
                    else
                    {
                        DisconnectSession(_session);
                        break;
                    }
                }
                catch (IOException ioEx)
                {
                    _smp.message_pool.push(new message("[server::Handle_session][IOError] " + ioEx.Message, type_msg.CL_FILE_LOG_AND_CONSOLE));
                    DisconnectSession(_session);
                    break;
                }
                catch (exception ex)
                {
                    _smp.message_pool.push(new message("[server::Handle_session][ErrorSystem] " + ex.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
                    DisconnectSession(_session);
                    break;
                }
            }
        }



        protected void OnMonitor()
        {
            while (_isRunning)
            {
                try
                {
                    // Verifica e atualiza os arquivos de log caso o dia tenha mudado
                    if (_smp.message_pool.checkUpdateDayLog())
                    {
                        _smp.message_pool.push("[Server::Monitor::UpdateLogFiles][Log] Atualizou os arquivos de Log porque trocou de dia.");
                    }

                    try
                    {
                        m_session_manager.CheckSessionLive();

                        // Atualiza o número de sessões conectadas
                        m_si.curr_user = (int)m_session_manager.NumSessionConnected();
                        NormalManagerDB.add(0, new CmdRegisterServer(m_si), SQLDBResponse, this);
                    }
                    catch (exception e) // Exceção específica da aplicação
                    {
                        _smp.message_pool.push(new message(
                            $"[Server.Monitor][ErrorSystem] {e.GetType().Name}: {e.getFullMessageError()}\nStack Trace: {e.getStackTrace()}",
                            type_msg.CL_FILE_LOG_AND_CONSOLE));
                    }
                    // Atualiza o título da janela do console conforme o tipo do servidor
                    switch (m_si.tipo)
                    {
                        case 0:
                            Console.Title = $"Login Server - P: {m_si.curr_user}";
                            break;
                        case 1:
                            Console.Title = $"Game Server - P: {m_si.curr_user}";
                            break;
                        case 2:
                            Console.Title = $"Bird Server - P: {m_si.curr_user}";
                            break;
                        case 3:
                            Console.Title = $"Login Server - P: {m_si.curr_user}";
                            break;
                        case 4:
                            Console.Title = $"Rank Server - P: {m_si.curr_user}";
                            break;
                        case 5:
                            Console.Title = $"Auth Server - P: {m_si.curr_user}";
                            break;
                        case 6:
                            Console.Title = $"GG Auth Server - P: {m_si.curr_user}";
                            break;
                        default:
                            Console.Title = $"Unknown Server - P: {m_si.curr_user}";
                            break;
                    }
                    // pega a lista de servidores online
                    cmdUpdateServerList();
                    // Atualiza a lista de bloqueios de IP/MAC
                    cmdUpdateListBlock_IP_MAC(); 
                    // Evento de heartbeat
                    OnHeartBeat();

                }
                catch (exception e) // Exceção específica da aplicação
                {
                    _smp.message_pool.push(new message(
                        $"[Server.Monitor][ErrorSystem] {e.GetType().Name}: {e.getFullMessageError()}\nStack Trace: {e.getStackTrace()}",
                        type_msg.CL_FILE_LOG_AND_CONSOLE));
                }
                catch (Exception ex) // Exceções gerais do .NET
                {
                    _smp.message_pool.push(new message(
                        $"[Server.Monitor][ErrorSystem] {ex.GetType().Name}: {ex.Message}\nStack Trace: {ex.StackTrace}",
                        type_msg.CL_FILE_LOG_AND_CONSOLE));
                }
            }
            Thread.Sleep(2100);
        }

        protected void cmdUpdateServerList()
        {
            NormalManagerDB.add(1, new CmdServerList(TYPE_SERVER.GAME), SQLDBResponse, this);
        }

        protected void cmdUpdateListBlock_IP_MAC()
        {
            // List de IP Address Ban
            var cmd_lib = new CmdListIpBan();     // Waiter

            NormalManagerDB.add(0, cmd_lib, null, null);

            if (cmd_lib.getException().getCodeError() != 0)
                throw cmd_lib.getException();

            v_ip_ban_list = cmd_lib.getListIPBan();

            // List de Mac Address Ban
            var cmd_lmb = new CmdListMacBan();    // Waiter

            NormalManagerDB.add(0, cmd_lmb, null, null);

            if (cmd_lmb.getException().getCodeError() != 0)
                throw cmd_lmb.getException();

            v_mac_ban_list = cmd_lmb.getList();
        }

        public override void dispach_packet_sv_same_thread(Session session, packet _packet)
        {
            if (session == null || session.isConnected() == false || _packet == null)
            {
                return;//nao esta mais conectado!
            }

            func_arr.func_arr_ex func = null;

            try
            {
                // Obtém a função correspondente ao tipo de pacote
                func = packet_func_base.funcs_sv.getPacketCall(_packet.getTipo());
            }
            catch (exception e)
            {
                _smp::message_pool.push(new message($"[Server.DispatchpacketSameThread][ErrorSystem] {e.Message}, {e.getStackTrace()}"));
                // Desconecta a sessão
                DisconnectSession(session);
            }

            try
            {
                // Atualiza o tick do cliente
                session.m_tick = Environment.TickCount;

                var pd = new ParamDispatch
                {
                    _session = session,
                    _packet = _packet
                };

                if (CheckPacket(session, _packet))
                {
                    try
                    {
                        if (func != null && func.ExecCmd(pd) != 0)
                        {
                            // _smp.message_pool.push(new message($"[Server.DispatchpacketSameThread][Error][MY] Ao tratar o pacote. ID: {_packet.getTipo()}(0x{_packet.getTipo():X})," + pd._packet.Log(), type_msg.CL_FILE_LOG_AND_CONSOLE));
                            DisconnectSession(session);
                        }
                    }

                    catch (exception e)
                    {
                        _smp::message_pool.push(new message($"[Server.DispatchpacketSameThread][Error][MY] {e.getFullMessageError()}"));

                        DisconnectSession(session);
                    }
                }
            }
            catch (exception e)
            {
                _smp::message_pool.push(new message($"[Server.DispatchpacketSameThread][Error][MY] {e.Message}"));

                DisconnectSession(session);
            }
        }

        protected override void dispach_packet_same_thread(Session session, packet _packet)
        {
            if (session == null || session.isConnected() == false || _packet == null)
            {
                return;//nao esta mais conectado!
            }

            func_arr.func_arr_ex func = null;

            try
            {
                // Obtém a função correspondente ao tipo de pacote
                func = packet_func_base.funcs.getPacketCall(_packet.getTipo());
            }
            catch (exception e)
            {
                _smp::message_pool.push(new message($"[Server.DispatchpacketSameThread][ErrorSystem] {e.Message}, {e.getStackTrace()}"));
                // Desconecta a sessão
                DisconnectSession(session);
            }

            try
            {
                // Atualiza o tick do cliente
                session.m_tick = Environment.TickCount;

                var pd = new ParamDispatch
                {
                    _session = session,
                    _packet = _packet
                };

                if (CheckPacket(session, _packet, 1))
                {
                    try
                    {
                        if (func != null && func.ExecCmd(pd) != 0)
                        {
                            //  _smp.message_pool.push(new message($"[Server.DispatchpacketSameThread][Error][MY] Ao tratar o pacote. ID: {_packet.getTipo()}(0x{_packet.getTipo():X})," + pd._packet.Log(), type_msg.CL_FILE_LOG_AND_CONSOLE));
                            DisconnectSession(session);
                        }
                    }

                    catch (exception e)
                    {
                        _smp::message_pool.push(new message($"[Server.DispatchpacketSameThread][Error][MY] {e.getFullMessageError()}"));

                        DisconnectSession(session);
                    }
                }
            }
            catch (exception e)
            {
                _smp::message_pool.push(new message($"[Server.DispatchpacketSameThread][Error][MY] {e.Message}"));

                DisconnectSession(session);
            }
        }


        #endregion

        #region Public Methods

        public void Start()
        {
            try
            {
                _server = new TcpListener(IPAddress.Any, m_si.port);
                m_state = ServerState.Good;

                if (m_state != ServerState.Failure)
                {

                    try
                    {
                        _server.Start(m_si.max_user);

                        _smp::message_pool.push(new message("[server::Start][Log] Running in Port: " + m_si.port, type_msg.CL_FILE_LOG_AND_CONSOLE));

                        // Start Unit Connect for Try Connection with Auth Server
                        //if (m_unit_connect != null)
                        //    //m_unit_connect.start();

                        //Inicia Thread para exec. registrar/att o servidor    

                        Thread thread = new Thread(() =>
                        {
                            _disconnect_session();

                            OnMonitor();
                        });

                        thread.Start(); // Inicia a thread de verificação
                        // On Start
                        OnStart();

                        // Set Accept Connection for Starting Service
                        var WaitConnectionsThread = new Thread(new ThreadStart(HandleWaitConnections));
                        WaitConnectionsThread.Start();
                    }
                    catch (exception e)
                    {
                        _smp::message_pool.push(new message(e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
                    }

                }
                else
                {
                    _smp::message_pool.push(new message("[server::start][Error] Server Inicializado com falha, fechando o server.", type_msg.CL_FILE_LOG_AND_CONSOLE));
                }
            }
            catch (exception e)
            {
                _smp::message_pool.push(new message(e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
        }

        public void Stop()
        {
            m_state = ServerState.Failure;
            Console.WriteLine("Server is stopping...");
        }


        public virtual Session HasLoggedWithOuterSocket(Session _session)
        {
            var s = m_session_manager.FindAllSessionByUid(_session.getUID());
            foreach (var el in s)
            {
                if (el.m_oid != _session.m_oid && el.isConnected())
                    return el;
            }

            return null;
        }
        protected virtual void init_option_accepted_socket(in Socket _accepted)
        {
            bool tcp_nodelay = true;

            // ---------- DESEMPENHO COM OS SOCKOPT -----------  
            // COM NO_TCPDELAY                 AVG(MEDIA) 0.552
            // COM SO_SNDBUF 0                AVG(MEDIA) 0.560
            // COM SO_RCVBUF 0                AVG(MEDIA) 0.570
            // COM NO_TCPDELAY e SO_SNDBUF 0  AVG(MEDIA) 0.569
            // COM NO_TCPDELAY e SO_RCVBUF 0  AVG(MEDIA) 0.566
            // SEM NENHUM SOCKOPT             AVG(MEDIA) 0.569
            // Não tem muita diferença, vou deixar só o NO_TCPDELAY mesmo

            try
            {
                // Ativa TCP_NODELAY (desabilita Nagle)
                _accepted.NoDelay = tcp_nodelay;
            }
            catch (SocketException ex)
            {
                throw new Exception("[server::init_option_accepted_socket][Error] não conseguiu desabilitar tcp delay (nagle algorithm).", ex);
            }

            try
            {
                // KEEPALIVE: habilita + configura tempo
                byte[] keepAlive = new byte[12];
                BitConverter.GetBytes((uint)1).CopyTo(keepAlive, 0);     // onoff
                BitConverter.GetBytes((uint)20000).CopyTo(keepAlive, 4); // keepalivetime (20s)
                BitConverter.GetBytes((uint)2000).CopyTo(keepAlive, 8);  // keepaliveinterval (2s)

                _accepted.IOControl(IOControlCode.KeepAliveValues, keepAlive, null);

                message_pool.push(new message(
                    $"[server::init_option_accepted_socket][Log] socket[ID={_accepted.Handle}] KEEPALIVE[ONOFF=1, TIME=20000, INTERVAL=2000] foi ativado para esse",
                    type_msg.CL_FILE_LOG_AND_CONSOLE
                ));
            }
            catch (SocketException ex)
            {
                throw new Exception("[server::init_option_accepted_socket][Error] não conseguiu setar o socket option KEEPALIVE.", ex);
            }
        }

        public bool haveBanList(string _ip_address, string _mac_address, bool _check_mac = true)
        {
            if (_check_mac)
            {
                // Verifica primeiro se o MAC Address foi bloqueado

                // Cliente não enviou um MAC Address válido, bloquea essa conexão que é hacker que mudou o ProjectG
                if (string.IsNullOrEmpty(_mac_address))
                    return true;    // Cliente não enviou um MAC Address válido, bloquea essa conexão que é hacker que mudou o ProjectG

                foreach (var el in v_mac_ban_list)
                {
                    if (!string.IsNullOrEmpty(el) && string.Compare(el, _mac_address, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        return true;
                    }
                }
            }
            // IP Address inválido, bloquea essa conexão que é Hacker ou Bug
            if (string.IsNullOrEmpty(_ip_address))
            {
                return true;
            }
            uint ip = 0;
            if (IPAddress.TryParse(_ip_address, out IPAddress ipAddress))
            {
                byte[] ipBytes = ipAddress.GetAddressBytes();
                ip = BitConverter.ToUInt32(ipBytes, 0);
                ip = (uint)IPAddress.NetworkToHostOrder((int)ip);
            }
            foreach (IPBan el in v_ip_ban_list)
            {
                if (el.type == IPBan._TYPE.IP_BLOCK_NORMAL)
                {
                    if ((ip & el.mask) == (el.ip & el.mask))
                    {
                        return true;
                    }
                }
                else if (el.type == IPBan._TYPE.IP_BLOCK_RANGE)
                {
                    if (el.ip <= ip && ip <= el.mask)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public void Shutdown(int timeSec)
        {
            Console.WriteLine("Shutting down server...");
            Stop();
        }

        public virtual uint GetUID()
        {
            return (uint)m_si.uid;
        }
        protected void _disconnect_session()
        {
            try
            {
                if (m_session_manager.IsInit())
                {

                    var s = m_session_manager.GetSessionToDelete(1000/*1 second para a liberar o while se não tiver sessions para disconectar*/);

                    if (s != null)
                        DisconnectSession(s);

                }
                else
                    Thread.Sleep(300/*espera 300 miliseconds até o session_manager ser inicializado*/);

            }
            catch (exception e)
            {
                _smp::message_pool.push(new message("[server::disconnect_session][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
        }

        public virtual List<Session> FindAllGM()
        {
            return m_session_manager.findAllGM();
        }

        public virtual Session FindSessionByOid(uint oid)
        {
            return m_session_manager.FindSessionByOid(oid);
        }

        public virtual Session FindSessionByUid(uint uid)
        {
            return m_session_manager.findSessionByUID(uid);
        }

        public virtual List<Session> FindAllSessionByUid(uint uid)
        {
            return m_session_manager.FindAllSessionByUid(uid);
        }

        public virtual Session FindSessionByNickname(string nickname)
        {
            return m_session_manager.FindSessionByNickname(nickname);
        }

        public override bool DisconnectSession(Session _session)
        {
            if (_session == null)
            {
                Console.WriteLine("[server::DisconnectSession][Warning] Tentativa de desconectar uma sessão nula.");
                return false;
            }

            message_pool.push(new message($"[server::DisconnectSession][Log] PLAYER[IP: {_session.getIP()}, Key: {_session.m_key}, Time: {DateTime.Now}]", type_msg.CL_FILE_LOG_AND_CONSOLE));

            // Notifica que a desconexão ocorreu       
            onDisconnected(_session);

            bool result;
            try
            {
                _ipFilter?.OnDisconnect(_session.getIP());

                // Remove a sessão do gerenciador        
                result = m_session_manager.DeleteSession(_session);

            }
            catch (Exception ex)
            {
                result = false;
                Console.WriteLine($"[server::DisconnectSession][Error] Erro ao deletar sessão: {ex.Message}");
            }
            return result;
        }

        public virtual void SQLDBResponse(int _msg_id, Pangya_DB _pangya_db, object _arg)
        {
            if (_arg == null)
            {
                _smp.message_pool.push("[Server.SQLDBResponse][WARNING] _arg is null, na msg_id = " + _msg_id);
                return;
            }
            switch (_msg_id)
            {
                case 1:
                    {
                        m_server_list = ((CmdServerList)_pangya_db).getServerList();
                    }
                    break;
                default:
                    break;
            }
        }


        public abstract bool CheckCommand(Queue<string> _command);

        public int getBotTTL() => m_Bot_TTL;
        #endregion

        #region Auth                                                                           
        //  public  unit_auth_server_connect m_unit_connect;		// Ponteiro Connecta com o Auth Server                  

        public virtual void authCmdShutdown(int _time_sec)
        {
        }

        public virtual void authCmdBroadcastNotice(string _notice)
        {

        }

        public virtual void authCmdBroadcastTicker(string _nickname, string _msg)
        {

        }

        public virtual void authCmdBroadcastCubeWinRare(string _msg, uint _option)
        {

        }

        public virtual void authCmdDisconnectPlayer(uint _req_server_uid, uint _player_uid, byte _force)
        {

        }

        public virtual void authCmdConfirmDisconnectPlayer(uint _player_uid)
        {

        }

        public virtual void authCmdNewMailArrivedMailBox(uint _player_uid, uint _mail_id)
        {

        }

        public virtual void authCmdNewRate(uint _tipo, uint _qntd)
        {

        }

        public virtual void authCmdReloadGlobalSystem(uint _tipo)
        {

        }

        public virtual void authCmdInfoPlayerOnline(uint _req_server_uid, uint _player_uid)
        {
            try
            {

                var s = m_session_manager.findSessionByUID(_player_uid);

                if (s != null)
                {
                    var aspi = new AuthServerPlayerInfo(s.getUID(), s.getID(), s.getIP());

                    // UPDATE ON Auth Server
                    //m_unit_connect.sendInfoPlayerOnline(_req_server_uid, aspi);

                }
                else
                {
                    // UPDATE ON Auth Server
                    //m_unit_connect.sendInfoPlayerOnline(_req_server_uid, new AuthServerPlayerInfo(_player_uid));
                }

            }
            catch (exception e)
            {

                // UPDATE ON Auth Server - Error reply
                //m_unit_connect.sendInfoPlayerOnline(_req_server_uid, new AuthServerPlayerInfo(_player_uid));

                _smp::message_pool.push(new message("[server::authCmdInfoPlayerOnline][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
        }

        public virtual void authCmdConfirmSendInfoPlayerOnline(uint _req_server_uid, AuthServerPlayerInfo _aspi)
        {

        }

        public virtual void authCmdSendCommandToOtherServer(packet _packet)
        {

            //try
            //{

            //    func_arr.func_arr_ex func = null;

            //    uint req_server_uid = _packet.ReadUInt32();
            //    var command_id = _packet.ReadInt16();

            //    try
            //    {

            //        func = packet_func_base.funcs_as.getpacketCall(command_id);

            //        if (func != null && func.ExecCmd(new ParamDispatch(m_unit_connect.m_session, _packet)) == 1)
            //            throw new exception("[server::authCmdSendCommandToOtherServer][Error] Ao tratar o Comando. ID: " + (command_id)
            //                    + "(0x" + (command_id) + ").", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.GAME_SERVER, 5000, 0));

            //    }
            //    catch (exception e)
            //    {

            //        if (ExceptionError.STDA_SOURCE_ERROR_DECODE_TYPE(e.getCodeError()) == STDA_ERROR_TYPE.FUNC_ARR/*packet_func Erro, Warning e etc*/)
            //        {

            //            message_pool.push(new message("[server::authCmdSendCommandToOtherServer][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

            //        }
            //        else
            //            throw;
            //    }

            //}
            //catch (exception e)
            //{

            //    message_pool.push(new message("[server::authCmdSendCommandToOtherServer][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            //}
        }

        public virtual void authCmdSendReplyToOtherServer(packet _packet)
        {
            //try
            //{

            //    func_arr.func_arr_ex func = null;

            //    uint req_server_uid = _packet.ReadUInt32();
            //    var command_id = _packet.ReadInt16();

            //    try
            //    {

            //        func = packet_func_base.funcs_as.getpacketCall(command_id);

            //        if (func != null && func.ExecCmd(new ParamDispatch(m_unit_connect.m_session, _packet)) == 1)
            //            throw new exception("[server::authCmdSendCommandToOtherServer][Error] Ao tratar o Comando. ID: " + (command_id)
            //                    + "(0x" + (command_id) + ").", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.GAME_SERVER, 5000, 0));

            //    }
            //    catch (exception e)
            //    {

            //        if (ExceptionError.STDA_SOURCE_ERROR_DECODE_TYPE(e.getCodeError()) == STDA_ERROR_TYPE.FUNC_ARR/*packet_func Erro, Warning e etc*/)
            //        {

            //            message_pool.push(new message("[server::authCmdSendCommandToOtherServer][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

            //        }
            //        else
            //            throw;
            //    }

            //}
            //catch (exception e)
            //{

            //    message_pool.push(new message("[server::authCmdSendCommandToOtherServer][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            //}
        }

        public virtual void sendCommandToOtherServerWithAuthServer(packet _packet, uint _send_server_uid_or_type)
        {
            try
            {

                // Envia o comando para o outro server com o Auth Server
                //m_unit_connect.sendCommandToOtherServer(_send_server_uid_or_type, _packet);

            }
            catch (exception e)
            {

                _smp::message_pool.push(new message("[server::sendCommandToOtherServerWithAuthServer][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
        }

        public virtual void sendReplyToOtherServerWithAuthServer(packet _packet, uint _send_server_uid_or_type)
        {
            try
            {

                // Envia a resposta para o outro server com o Auth Server
                //m_unit_connect.sendReplyToOtherServer(_send_server_uid_or_type, _packet);

            }
            catch (exception e)
            {

                _smp::message_pool.push(new message("[server::sendReplyToOtherServerWithAuthServer][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
        }



        #endregion
    }

    // Server Static
    //namespace ssv
    //{
    //    public abstract partial class sv : Singleton<Server>
    //    {
    //    }
    //}
}
