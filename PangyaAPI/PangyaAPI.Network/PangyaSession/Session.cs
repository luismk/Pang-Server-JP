using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using PangyaAPI.Network.Cryptor;
using PangyaAPI.Network.Pangya_St;
using PangyaAPI.Network.PangyaPacket;
using PangyaAPI.Network.PangyaServer;
using PangyaAPI.Network.PangyaUtil;
using PangyaAPI.Utilities;
using PangyaAPI.Utilities.Log;

namespace PangyaAPI.Network.PangyaSession
{    // Estrutura para sincronizar o uso de buff, para não limpar o socket(Session) antes dele ser liberado
    public class stUseCtx
    {

        private object m_cs = new object();
        protected int m_active = new int();
        protected bool m_quit;
        public stUseCtx()
        {
            clear();
        }
        public void Dispose()
        {

            clear();
            m_cs = new object();
        }
        public void clear()
        {
            Monitor.Enter(m_cs);
            m_active = 0;
            m_quit = false;
            Monitor.Exit(m_cs);
        }
        public bool isQuit()
        {

            var quit = false;
            Monitor.Enter(m_cs);
            quit = m_quit;
            Monitor.Exit(m_cs);

            return quit;
        }
        public int usa()
        {

            var spin = 0;
            Monitor.Enter(m_cs);
            spin = ++m_active;
            Monitor.Exit(m_cs);

            return spin;
        }
        public bool devolve()
        {
            Monitor.Enter(m_cs);
            --m_active;
            Monitor.Exit(m_cs);
            return m_active <= 0 && m_quit; // pode excluir(limpar) a Session
        }
        // Verifica se pode excluir a Session, se não seta a flag quit para o prox method que devolver excluir ela
        public bool checkCanQuit()
        {

            var can = false;
            Monitor.Enter(m_cs);

            if (m_active <= 0)
            {
                can = true;
            }
            else
            {
                m_quit = true;
            }

            Monitor.Exit(m_cs);
            return can;
        }
    }

    public abstract partial class Session
    {

        public int m_time_start = new int();
        public int m_tick = new int();
        public int m_tick_bot = new int();

        // Session autorizada pelo server, fez o login corretamente
        public bool m_is_authorized;

        // Marca na Session que o socket, levou DC, chegou ao limit de retramission do TCP para transmitir os dados
        // TCP sockets is that the maximum retransmission count and timeout have been reached on a bad(or broken) link
        public bool m_connection_timeout;
        public bool m_is_connected;
        public TcpClient m_sock = new TcpClient();
        public IPEndPoint m_addr;
        public byte m_key;

        public string m_ip = "0.0.0.0";
        public bool m_ip_maked;

        public int m_oid = -1;

        // Contexto de sincronização
        private readonly object m_cs = new object();
        private readonly object m_cs_lock_other = new object(); // Usado para bloquear outras coisas (sincronizar os pacotes, por exemplo)

        private bool m_state;
        private bool m_connected;
        private bool m_connected_to_send;

        private stUseCtx m_use_ctx = new stUseCtx();

        public Server Server { get; internal set; }

        public Session(TcpClient client)
        { this.m_sock = client; }
        public Session()
        {
            this.m_use_ctx = new stUseCtx();
            m_key = 0;
            m_addr = null;

            m_time_start = 0;
            m_tick = 0;
            m_tick_bot = 0;

            m_ip_maked = false;

            m_oid = ~0;

            m_is_authorized = false;

            m_connection_timeout = false;

            m_state = true;
            m_connected = false;
            m_connected_to_send = false;
        }

        public Session(SOCKET _sock, IPEndPoint _addr, byte _key)
        {
            this.m_sock = _sock;
            this.m_addr = _addr;
            this.m_key = _key;
            this.m_use_ctx = new stUseCtx();

            m_ip_maked = false;

            m_oid = -1;

            m_is_authorized = false;

            m_connection_timeout = false;

            m_ip = "";

            make_ip();

            m_state = true;
        }

        public virtual bool clear()
        {
            m_state = false;
            m_connected = false;
            m_connected_to_send = false;

            m_key = 0;

            m_time_start = 0;
            m_tick = 0;
            m_tick_bot = 0;

            m_oid = ~0;

            m_is_authorized = false;

            m_connection_timeout = false;

            m_ip = "";
            m_ip_maked = false;

            m_use_ctx.clear();
            m_sock.Dispose();
            return true;
        }

        public string getIP()
        {

            if (!m_ip_maked || (m_addr.Port != 0 && string.Compare(m_ip, "0.0.0.0") == 0))
            {
                make_ip();
            }

            return m_ip;
        }

        public void @lock()
        {
            Monitor.Enter(m_cs);
        }
        public void unlock()
        {
            Monitor.Exit(m_cs);
        }

        // Usando para syncronizar outras coisas da Session, tipo pacotes
        public void lockSync()
        {
            Monitor.Enter(m_cs_lock_other);
        }

        public void unlockSync()
        {
            Monitor.Exit(m_cs_lock_other);
        }

        public void requestSendBuffer(byte[] _buff, bool _raw = false)
        {

            if (_buff == null)
            {
                throw new exception("Error _buff is nullptr. Session::requestSendBuffer()", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.SESSION,
                    3, 0));
            }
            int _size = _buff.Length;
            if (_size <= 0)
            {
                throw new exception("Error _size is less or equal the zero. Session::requestSendBuffer()", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.SESSION,
                    4, 0));
            }
            try
            {
                if (isConnectedToSend())
                {

                    var payloadData = _raw ? _buff : Cipher.ServerEncrypt(_buff, m_key, 0);

                    if (!m_sock.Send(payloadData, (int)payloadData.Length))
                    {
                        @lock();
                        setConnectedToSend(false);
                        unlock();

                        try
                        {
                            Server.DisconnectSession(this);
                        }
                        catch (exception e)
                        {
                            message_pool.push(new message("[threadpool::send_new][Error] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
                        }
                    }
                    else
                    {
                        if (_raw)
                        {
                            packet_head ph = Tools.memcpy<packet_head>(_buff);

                            _buff = _buff.Slice(4); //3size, -1 byte aleatory
                        }

                        Server.dispach_packet_sv_same_thread(this, new packet(_buff));
                    }
                }
                else
                {
                    //m_buff_s.releaseWrite();
                    return;
                }
            }
            finally
            {
                // m_buff_s.unlock();
            }
        }

        public bool isConnected()
        {
            bool ret = false;

            try
            {

                @lock();

                // getConnectTime pode lançar exception
                ret = m_connected && (getConnectTime() >= 0);

                unlock();

            }
            catch (exception e)
            {

                message_pool.push(new message("[Session::isConnected][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                unlock();
            }

            return ret;
        }

        public bool isCreated()
        {
            return m_state;
        }

        public int getConnectTime()
        {
            if (m_sock != null && m_sock.Connected && getState())
            {
                if (m_sock.Connected)
                {
                    return 1;
                }
                else
                {
                    throw new exception("[Session::getConnectTime] erro ao pegar optsock SO_CONNECT_TIME.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.SESSION,
                        50, 0));
                }
            }

            return -1;
        }


        public int usa()
        {

            if (!isConnected())
            {
                throw new exception("[Session::usa][error] nao pode usa porque o Session nao esta mais conectado.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.SESSION,
                    6, 0));
            }

            return m_use_ctx.usa();
        }

        public bool devolve()
        {
            return m_use_ctx.devolve();
        }

        public bool isQuit()
        {
            return m_use_ctx.isQuit();
        }

        public bool getState()
        {
            return m_state;
        }
        public void setState(bool _state)
        {
            m_state = _state;
        }

        public void setConnected(bool _connected)
        {

            m_connected = _connected;

            // Espelho de connected exceto quando o setConnectedToSend é chamando que vão ter outros valores,
            // esse aqui é para quando o socket WSASend retorna WSAECONNREST, que o getTimeConnect não vai detectar no mesmo estante que o socket foi resetado,
            // essa flag é para bloquea os requestSend no socket, para não gerar deadlock no buffer_send do socket
            setConnectedToSend(_connected);
        }
        public void setConnectedToSend(bool _connected_to_send)
        {
            m_connected_to_send = _connected_to_send;
        }

        public abstract byte getStateLogged();

        public abstract uint getUID();
        public abstract uint getCapability();
        public abstract string getNickname();
        public abstract string getID();

        public void make_ip()
        {

            if (!m_ip_maked || (m_addr.Port != 0 && string.Compare(m_ip, "0.0.0.0") == 0))
            {
                try
                {
                    m_ip = m_addr.Address.ToString();
                    m_ip_maked = true;
                }
                catch
                {

                    throw new exception("Erro ao converter SOCKADDR_IN para string doted mode(IP). Session::make_ip()", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.SESSION,
                        1, 0));
                }
            }
        }

        public bool isConnectedToSend()
        {

            bool ret = false;

            try
            {

                @lock();

                // getConnectTime pode lançar exception
                ret = m_connected_to_send && (m_sock.Connected);

                unlock();
            }
            catch (exception e)
            {

                message_pool.push(new message("[Session::isConnectedToSend][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                unlock();
            }

            return ret;
        }

        public void Disconnect()
        {
            Server.DisconnectSession(this);
        }
    }
}