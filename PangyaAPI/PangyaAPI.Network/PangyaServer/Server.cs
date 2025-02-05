using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using _smp = PangyaAPI.Utilities.Log;
using PangyaAPI.Network.Pangya_St;
using System.Threading;
using PangyaAPI.Utilities.Log;
using PangyaAPI.Utilities;
using PangyaAPI.Network.PangyaPacket;
using PangyaAPI.SQL.Manager;
using PangyaAPI.SQL;
using PangyaAPI.Network.Cmd;
using PangyaAPI.Network.PangyaSession;
using System.IO;
using System.Linq;
using System.Diagnostics;
using static System.Collections.Specialized.BitVector32;

namespace PangyaAPI.Network.PangyaServer
{
    public abstract partial class Server
    {
        #region Fields
        enum ServerState
        {
            Uninitialized,
            Good,
            GoodWithWarning,
            Initialized,
            Failure
        }

        ServerState m_state;
        private List<string> v_mac_ban_list;
        private List<IPBan> v_ip_ban_list;
        private readonly SessionManager m_session_manager;
        public ServerInfoEx m_si;
        private uint m_Bot_TTL; // Anti-bot Time-to-live
        private bool m_chatDiscord;
        private volatile bool _continueAccept;
        public bool _isRunning => m_state == ServerState.Good;
        public IniHandle m_reader_ini { get; set; }
        public List<TableMac> ListBlockMac { get; set; }
        public List<ServerInfo> m_server_list { get; set; }
        protected func_arr funcs { get; set; }
        public ServerInfoEx getInfo() => m_si;
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
        /// <param db_name="session"></param>
        /// <param db_name="packet"></param>
        /// <returns></returns>
        public abstract bool CheckPacket(SessionBase session, Packet packet);
        /// <summary>
        /// disconnect players !
        /// </summary>
        /// <param db_name="_session"></param>
        public abstract void onDisconnected(SessionBase _session);

        /// <summary>
        /// Send Key
        /// </summary>
        /// <param name="_session"></param>
        protected abstract void onAcceptCompleted(SessionBase _session);

        #endregion

        #region Constructor
        public Server(SessionManager sessionManager)
           : base()
        {
            this.m_session_manager = sessionManager;
            m_server_list = new List<ServerInfo>();
            m_state = ServerState.Uninitialized;
            funcs = new func_arr();
            m_reader_ini = new IniHandle("server.ini");
            ConfigInit();
        }

        #endregion

        #region Private Methods    

        public virtual void ConfigInit()
        {
            m_reader_ini = new IniHandle("server.ini");
            m_si = new ServerInfoEx
            {
                version = m_reader_ini.ReadString("SERVERINFO", "VERSION", "Pangya Server Csharp 1.0"),
                version_client = m_reader_ini.ReadString("SERVERINFO", "CLIENTVERSION", "JP.R7.962.00"),
                nome = m_reader_ini.ReadString("SERVERINFO", "NAME", "Pangya Server Csharp"),
                uid = m_reader_ini.ReadInt32("SERVERINFO", "GUID", 10103),
                port = m_reader_ini.ReadInt32("SERVERINFO", "PORT", 10103),
                ip = m_reader_ini.ReadString("SERVERINFO", "IP", "127.0.0.1"),
                max_user = m_reader_ini.ReadInt32("SERVERINFO", "MAXUSER", 2001),
                propriedade = new uProperty(m_reader_ini.ReadUInt32("SERVERINFO", "PROPERTY", 2048)),
                rate = new RateConfigInfo(),
                event_flag = new uEventFlag(),
                flag = new uFlag(0)
            };
            try
            {
                m_Bot_TTL = m_reader_ini.ReadUInt32("OPTION", "ANTIBOTTTL", 1000);
                m_si.packet_version = m_reader_ini.ReadUInt32("SERVERINFO", "PACKETVERSION");
            }
            catch (exception e)
            {
                _smp::message_pool.push(new message("[server::config_init][Error] " + e.getFullMessageError(), type_msg.CL_ONLY_CONSOLE));
                m_Bot_TTL = 1000u; // Usa o valor padrão do anti bot TTL
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

                    // Inicia Escuta de novas conexões (Quando player se conecta).

                    TcpClient newClient = _server.AcceptTcpClient();

                    newClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);

                    // Cliente conectado
                    // Cria uma Thread para manusear a comunicação (uma thread por cliente)
                    Thread t = new Thread(new ParameterizedThreadStart(HandleSession));
                    t.Start(newClient);

                }

                catch (exception e) // Exceção específica da aplicação
                {
                    _smp.message_pool.push(new message(
                        $"[Server.HandleWaitConnections][ErrorSystem] {e.GetType().Name}: {e.getFullMessageError()}\nStack Trace: {e.getStackTrace()}",
                        type_msg.CL_FILE_LOG_AND_CONSOLE));
                }
            }
        }

        /// <summary>
        /// Manuseia Comunicação do Cliente
        /// </summary>
        private void HandleSession(object obj)
        {
            //Recebe cliente a partir do parâmetro
            TcpClient client = (TcpClient)obj;
            //add player
            var Session = m_session_manager.AddSession(this, client, client.Client.RemoteEndPoint as IPEndPoint, (byte)(new Random().Next() % 16));
            //
            _smp.message_pool.push(new message("[server::HandleSession][Log] New Player Connected [Ip: " + Session.getIP() + ", Key: " + Session.m_key + "]", type_msg.CL_FILE_LOG_AND_CONSOLE));

            onAcceptCompleted(Session);

            while (Session.getConnected())
            {
                try
                {
                    if (!Session.getConnected())
                    {
                        DisconnectSession(Session);
                        break;
                    }
                    
                    byte[] message = ReceivePacket(client.GetStream());

                    if (message.Length >= 5)
                    {
                        var packet = new Packet(message, Session.m_key);
                        DispatchPacketSameThread(Session, packet); // Processa o pacote recebido
                    }
                    else
                    {
                        DisconnectSession(Session);
                        Session.Dispose();
                        break;
                    }
                }
                catch (SocketException ex)
                {
                    _smp.message_pool.push(new message("[server::HandleSession][IOError] " + ex.Message, type_msg.CL_FILE_LOG_AND_CONSOLE));
                    DisconnectSession(Session);
                    break; 
                }
                catch (IOException ioEx)
                {
                    _smp.message_pool.push(new message("[server::HandleSession][IOError] " + ioEx.Message, type_msg.CL_FILE_LOG_AND_CONSOLE));
                    DisconnectSession(Session);
                    break;
                }
                catch (Exception ex)
                {
                    _smp.message_pool.push(new message("[server::HandleSession][ErrorSystem] " + ex.Message, type_msg.CL_FILE_LOG_AND_CONSOLE));
                    DisconnectSession(Session);
                    break;
                }
            }
        }

        protected byte[] ReceivePacket(NetworkStream stream)
        {
            try
            {
                byte[] buffer = new byte[1024];
                int bytesRead = stream.Read(buffer, 0, buffer.Length);

                if (bytesRead == 0)
                {
                    Debug.WriteLine("O cliente desconectou durante a leitura.");
                    return new byte[0];
                }

                return buffer.Take(bytesRead).ToArray();
            }
            catch (IOException ioEx)
            {
                Debug.WriteLine($"[ReceivePacket] Erro de leitura: {ioEx.Message}");
                return new byte[0];
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ReceivePacket] Erro inesperado: {ex.Message}");
                return new byte[0];
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
                      ///  m_session_manager.CheckSessionLive();

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
                            Console.Title = $"Messenger Server - P: {m_si.curr_user}";
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

                    // Atualiza a lista de servidores online e bloqueios de IP/MAC
                    cmdUpdateServerList();
                    //cmdUpdateListBlock_IP_MAC();

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

            cmd_lib.ExecCmd();

            if (cmd_lib.getException().getCodeError() != 0)
                throw cmd_lib.getException();

            v_ip_ban_list = cmd_lib.getListIPBan();

            // List de Mac Address Ban
            var cmd_lmb = new CmdListMacBan();    // Waiter

            NormalManagerDB.add(0, cmd_lmb, null, null);

            cmd_lmb.ExecCmd();

            if (cmd_lmb.getException().getCodeError() != 0)
                throw cmd_lmb.getException();
            v_mac_ban_list = cmd_lmb.getList();
        }

        protected void DispatchPacketSameThread(SessionBase session, Packet packet)
        {
            if (session == null || packet == null)
            {
                throw new ArgumentNullException("SessionBase or packet is null.");
            }

            func_arr.func_arr_ex func = null;

            try
            {
                // Obtém a função correspondente ao tipo de pacote
                func = funcs.getPacketCall(packet.Id);
            }
            catch (exception e)
            {
                _smp::message_pool.push(new message($"[Server.DispatchPacketSameThread][ErrorSystem] {e.Message}, {e.getStackTrace()}"));
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
                    _packet = packet
                };

                if (CheckPacket(session, packet))
                {
                    if (func != null && func.ExecCmd(pd) != 0)
                    {
                        _smp.message_pool.push(new message($"[Server.DispatchPacketSameThread][Error] Ao tratar o pacote. ID: {packet.Id}(0x{packet.Id:X}). Hex:\n\r" + pd._packet.Log(), type_msg.CL_FILE_LOG_AND_CONSOLE));
                        //pode dar dc aqui por que o pacote nao e reconhecido!
                        //@@@@
                    }
                }
            }
            catch (exception e)
            {
                _smp::message_pool.push(new message($"[Server.DispatchPacketSameThread][Error][MY] {e.Message}"));

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
                        //if (m_unit_connect != nullptr)
                        //    m_unit_connect->start();
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

        public bool StartingServer()
        {
            try
            {
                _smp::message_pool.push(new message("[server::Starting][Log] initial Server Now", type_msg.CL_FILE_LOG_AND_CONSOLE));

                Start();
                return true;
            }
            catch (exception e)
            {
                throw e;
            }
        }

        public void SendToAll(byte[] Data)
        {
            for (int i = 0; i < m_session_manager.NumSessionConnected(); i++)
            {
                m_session_manager.m_sessions[i].Send(Data);
            }
        }

        public virtual SessionBase HasLoggedWithOuterSocket(SessionBase _session)
        {
            var s = m_session_manager.FindAllSessionByUid(_session.getUID());
            foreach (var el in s)
            {
                if (el.m_oid != _session.m_oid && el.getConnected())
                    return el;
            }

            return null;
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
        public virtual SessionBase FindSession(uint _uid, bool _oid = false)
        {
            return (_oid ? m_session_manager.FindSessionByOid(_uid) : m_session_manager.FindSessionByUid(_uid));
        }

        public virtual bool DisconnectSession(SessionBase _session)
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
                _smp.message_pool.push("[Server.SQLDBResponse][WARNING] _arg is nullptr, na msg_id = " + _msg_id);
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


        public virtual void RunCommand(string[] comando)
        {

        }
        #endregion
    }
}
