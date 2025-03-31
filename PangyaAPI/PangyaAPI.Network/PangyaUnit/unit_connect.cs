using PangyaAPI.Network.Pangya_St;
using PangyaAPI.Network.PangyaPacket;
using PangyaAPI.Network.PangyaSession;
using PangyaAPI.Utilities;
using PangyaAPI.Utilities.BinaryModels;
using PangyaAPI.Utilities.Log;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading;

namespace PangyaAPI.Network.PangyaUnit
{
    public class ParamDispatchAS : ParamDispatch
    {
        public new UnitPlayer _session;
    }

    public class UnitPlayer : PangyaSession.SessionBase
    {
        public struct player_info
        {
            public string nickname;
            public string id;
            public uint uid;
            public uint tipo;
            public byte m_state;
        }
        public byte getStateLogged() => m_pi.m_state;
        public override uint getUID() => m_pi.uid;
        public override uint getCapability() => m_pi.tipo;
        public override string getNickname() => m_pi.nickname;
        public override string getID() => m_pi.id;
        public ServerInfoEx m_si;
        public player_info m_pi;
        public UnitPlayer(ServerInfoEx serverInfo)
        {
            m_si = serverInfo;
            m_pi = new player_info();
        }

        public byte[] RequestRecvBuffer()
        {
            try
            {
                byte[] buffer = new byte[500000];
                int bytesRead = _client.GetStream().Read(buffer, 0, buffer.Length);

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
    }

    public abstract class unit_connect_base
    {
        public unit_connect_base(ServerInfoEx _si)
        {
            m_session = new UnitPlayer(_si);
            funcs = new func_arr();
            funcs_sv = new func_arr();
            m_unit_ctx = new stUnitCtx();
            m_reader_ini = new IniHandle("Server.ini");
        }
        public enum STATE : byte { UNINITIALIZED, GOOD, GOOD_WITH_WARNING, INITIALIZED, FAILURE }
        public enum ThreadType { WORKER_IO, WORKER_IO_SEND, WORKER_IO_RECV, WORKER_LOGICAL, WORKER_SEND, TT_CONSOLE, TT_ACCEPT, TT_ACCEPTEX, TT_ACCEPTEX_IO, TT_RECV, TT_SEND, TT_JOB, TT_DB_NORMAL, TT_MONITOR, TT_SEND_MSG_TO_LOBBY }
        public enum OperationType { SEND_RAW_REQUEST, SEND_RAW_COMPLETED, RECV_REQUEST, RECV_COMPLETED, SEND_REQUEST, SEND_COMPLETED, DISPACH_PACKET_SERVER, DISPACH_PACKET_CLIENT, ACCEPT_COMPLETED }

        public struct stUnitCtx
        {
            public bool state;
            public string ip;
            public int port;

            public void Clear()
            {
                ip = string.Empty;
                port = 0;
                state = false;
            }
        }
        public virtual void DisconnectSession(SessionBase _session)
        {
            if (_session != null && _session.getConnected())
            {

                // on Disconnect
                onDisconnect();


                _session.Clear();
            }
        }

        public virtual bool isLive()
        {
            return (m_session.getState() && m_session.getConnected());
        }

        protected abstract void onHeartBeat();
        protected abstract void onConnected();
        protected abstract void onDisconnect();
        public void ConnectAndAssoc()
        {
            if (!m_unit_ctx.state)
            {
                throw new Exception("[UnitConnectBase::ConnectAndAssoc][Error] A configuração do unit_connect não foi carregada com sucesso.");
            }

            var _client = new TcpClient();
            try
            {
                _client.NoDelay = true; // Desabilita o Nagle Algorithm

                // Conecta ao IP e Porta fornecidos
                _client.Connect(m_unit_ctx.ip, m_unit_ctx.port);

                m_session._client = _client;
                m_session.SetState(true);
                HandleSession();
            }
            catch (Exception ex)
            {
                m_session.Lock();
                m_session.Clear();
                m_session.Unlock();

                _client?.Close();
                _client = null;

                throw new Exception("[UnitConnectBase::ConnectAndAssoc][Error] Falha ao conectar.", ex);
            }
              
            // On Connected
            onConnected();


            Thread thread = new Thread(() =>
            {
                 Monitor();
            });

            thread.Start(); // Inicia a thread de verificação
        }

        protected void Monitor()
        {
            message_pool.push(new message("monitor iniciado com sucesso!"));

            while (true)
            {
                try
                {

                    // Evento de heartbeat
                    if(isLive())
                        onHeartBeat();

                }
                catch (exception e) // Exceção específica da aplicação
                {
                    message_pool.push(new message(
                        $"[unit.Monitor][ErrorSystem] {e.GetType().Name}: {e.getFullMessageError()}\nStack Trace: {e.getStackTrace()}",
                        type_msg.CL_FILE_LOG_AND_CONSOLE));
                }
                catch (Exception ex) // Exceções gerais do .NET
                {
                    message_pool.push(new message(
                        $"[unit.Monitor][ErrorSystem] {ex.GetType().Name}: {ex.Message}\nStack Trace: {ex.StackTrace}",
                        type_msg.CL_FILE_LOG_AND_CONSOLE));
                }
                Thread.Sleep(5000);
            }
        }

        protected void HandleSession()
        { 

            message_pool.push(new message("[server::HandleSession][Log] New Player Connected [IP: " + m_session.getIP() + ", Key: " + m_session.m_key + "]", type_msg.CL_FILE_LOG_AND_CONSOLE));
             
            while (m_session.getConnected())
            {
                try
                {
                    if (!m_session.getConnected())
                    {
                        DisconnectSession(m_session);
                        break;
                    }

                    byte[] message = m_session.RequestRecvBuffer();

                    DispatchPacketSameThread(m_session, new Packet(message)); // Processa o pacote recebido

                }
                catch (exception erro)
                {
                    try
                    {
                        if (erro.Message.ToUpper().Contains("FOI FORÇADO O CANCELAMENTO DE UMA CONEXÃO EXISTENTE PELO HOST REMOTO."))
                        {
                            DisconnectSession(m_session);
                            return;
                        }
                        message_pool.push(new message("[server::HandleSession][IOError] " + erro.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
                    }
                    catch { }
                }
            }
            DisconnectSession(m_session);
        }

        protected void DispatchPacketSameThread(SessionBase session, Packet packet)
        {
            if (session == null || session.getConnected() == false || packet == null)
            {
                return;//nao esta mais conectado!
            }

            func_arr.func_arr_ex func = null;

            try
            {
                // Obtém a função correspondente ao tipo de pacote
                func = funcs.getPacketCall(packet.Id);
            }
            catch (exception e)
            {
                message_pool.push(new message($"[Server.DispatchPacketSameThread][ErrorSystem] {e.Message}, {e.getStackTrace()}"));
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
                    try
                    {
                        if (func != null && func.ExecCmd(pd) != 1)
                        {
                            message_pool.push(new message($"[Server.DispatchPacketSameThread][Error][MY] Ao tratar o pacote. ID: {packet.Id}(0x{packet.Id:X})," + pd._packet.Log(), type_msg.CL_FILE_LOG_AND_CONSOLE));
                            DisconnectSession(session);
                        }
                    }

                    catch (exception e)
                    {
                        message_pool.push(new message($"[Server.DispatchPacketSameThread][Error][MY] {e.getFullMessageError()}"));

                        DisconnectSession(session);
                    }
                }
            }
            catch (exception e)
            {
                message_pool.push(new message($"[Server.DispatchPacketSameThread][Error][MY] {e.Message}"));

                DisconnectSession(session);
            }
        }

        protected bool CheckPacket(SessionBase session, Packet packet)
        {
            return true;
        }



        public void start()
        {
            _eventTryConnect.Set();

            this.ConnectAndAssoc();
        }

        public func_arr funcs;
        public func_arr funcs_sv;
        public UnitPlayer m_session;
        public STATE m_state;
        public stUnitCtx m_unit_ctx;
        public IniHandle m_reader_ini;
        private AutoResetEvent _eventTryConnect = new AutoResetEvent(false);

        public class packet_func_as
        {
            public static void session_send(PangyaBinaryWriter p, UnitPlayer s, byte _debug)
            {
                s.Send(p, _debug == 1);
            }
            public static void session_send(List<PangyaBinaryWriter> v_p, UnitPlayer s, byte _debug) { s.Send(v_p); }
        }

    }
}
