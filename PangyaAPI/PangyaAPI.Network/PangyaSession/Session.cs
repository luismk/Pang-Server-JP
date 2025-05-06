using PangyaAPI.Network.PangyaPacket;
using PangyaAPI.Utilities.Log;
using PangyaAPI.Utilities;
using System;
using PangyaAPI.Network.Pangya_St;
using System.Net;
using PangyaAPI.Network.PangyaUtil;
using System.Threading;
using PangyaAPI.Network.PangyaServer;
using System.Net.Sockets;
using System.Xml;

namespace PangyaAPI.Network.PangyaSession
{

    // Tempo em millisegundos que a Session pode ficar conectada sem logar-se, receber a autorização

    // estrutura que guarda os dados do player, de request de packet
    public class ctx_check_packet
    {
        public ctx_check_packet()
        {
            clear();
        }
        public void clear()
        {
        }
        public LARGE_INTEGER gettick()
        {
            LARGE_INTEGER r = new LARGE_INTEGER();
            r.LowPart = (uint)TimeSpec.GetTick().tv_sec;
            r.HighPart = (uint)TimeSpec.GetTick().tv_nsec;
            return r;
        }
        public bool checkPacketId(ushort _packet_id)
        {
            long DiffTick(long a, long b, long c)
            {
                return a - b / c / 1000000;
            }
            var tick = gettick();
            TimeSpec frequency = new TimeSpec();

            for (var i = 0; i < Global.CHK_PCKT_NUM_PCKT_MRY; ++i)
            {
                if (ctx[i].packet_id == _packet_id)
                {
                    // C++ TO C# CONVERTER TASK: The #define macro 'DIFF_TICK' was defined in multiple preprocessor conditionals and cannot be replaced in-line:
                    if (DiffTick(tick.HighPart,
                        ctx[i].tick.LowPart, frequency.tv_sec) <= Global.CHK_PCKT_INTERVAL_LIMIT)
                    {

                        last_index = (byte)i;

                        // att tick
                        ctx[last_index].tick = tick;

                        return true;
                    }
                    else
                    {
                        ctx[i].tick = tick;
                        ctx[i].count = 0;

                        return false;
                    }
                }
                else if ((i + 1) == Global.CHK_PCKT_NUM_PCKT_MRY)
                {
                    // Rotate array to the left
                    var first = ctx[0];
                    for (int j = 0; j < Global.CHK_PCKT_NUM_PCKT_MRY - 1; j++)
                        ctx[j] = ctx[j + 1];
                    ctx[Global.CHK_PCKT_NUM_PCKT_MRY - 1] = first;


                    // Insert And Clean
                    ctx[Global.CHK_PCKT_NUM_PCKT_MRY - 1].packet_id = _packet_id;
                    ctx[Global.CHK_PCKT_NUM_PCKT_MRY - 1].tick = tick;
                    ctx[Global.CHK_PCKT_NUM_PCKT_MRY - 1].count = 0;
                }
            }

            return false;
        }
        public uint incrementCount()
        {
            return ctx[last_index].count++;
        }
        public void clearLast()
        {
            ctx[last_index].clear();
        }
        protected partial class stCtx
        {
            public void clear()
            {

            }

            public LARGE_INTEGER tick = new LARGE_INTEGER();

            public uint count = new uint();
            public ushort packet_id;
        }
        protected stCtx[] ctx = Tools.InitializeWithDefaultInstances<stCtx>(Global.CHK_PCKT_NUM_PCKT_MRY);

        private byte last_index;
    }

    // Estrutura para sincronizar o uso de buff, para não limpar o socket(Session) antes dele ser liberado
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

        public ctx_check_packet m_check_packet = new ctx_check_packet();

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

        public buff_ctx m_buff_s = new buff_ctx();
        public buff_ctx m_buff_r = new buff_ctx();

        public pangya_packet_handle_base m_threadpool;

        public packet m_packet_s = new packet();
        public packet m_packet_r = new packet();

        // Contexto de sincronização
        private readonly object m_cs = new object();
        private readonly object m_cs_lock_other = new object(); // Usado para bloquear outras coisas (sincronizar os pacotes, por exemplo)

        private long m_request_recv = new long(); // Requests recv buff

        private bool m_state;
        private bool m_connected;
        private bool m_connected_to_send;

        private stUseCtx m_use_ctx = new stUseCtx();

        public Server Server { get; internal set; }

        public class buff_ctx : IDisposable
        {
            public PangyaBuffer buff = new PangyaBuffer();

            private readonly object cs = new object();
            private readonly ManualResetEventSlim cv_send = new ManualResetEventSlim(true);
            private readonly ManualResetEventSlim cv_write = new ManualResetEventSlim(true);

            protected long request_send_count = 0;

            protected bool state_send;
            protected bool state_write;
            protected bool state_wr_send;

            public buff_ctx()
            {
                init();
            }

            public void Dispose()
            {
                destroy();
            }

            public void init()
            {
                state_send = false;
                state_write = false;
                state_wr_send = true;
                request_send_count = 0;

                cv_send.Set();
                cv_write.Set();
            }

            public void destroy()
            {
                clear();

                cv_send.Dispose();
                cv_write.Dispose();
            }

            public void clear()
            {
                @lock();

                buff.reset();

                state_send = false;
                state_write = false;
                state_wr_send = true;

                request_send_count = 0;

                cv_send.Set();
                cv_write.Set();

                unlock();
            }

            public void @lock()
            {
                Monitor.Enter(cs);
            }

            public void unlock()
            {
                Monitor.Exit(cs);
            }

            public bool isWritable()
            {
                while (true)
                {
                    @lock();
                    if (!state_write)
                    {
                        unlock();
                        return true;
                    }
                    unlock();

                    if (!cv_write.Wait(TimeSpan.FromMilliseconds(Timeout.Infinite)))
                        return false;
                }
            }

            public bool readyToWrite()
            {
                while (state_send || state_wr_send)
                {
                    @lock();

                    if (!cv_send.Wait(TimeSpan.FromMilliseconds(Timeout.Infinite)))
                        return false;

                    unlock();
                    return true;
                }
                return true;
            }

            public bool isSendable()
            {
                while (true)
                {
                    @lock();
                    if (!state_send)
                    {
                        unlock();
                        return true;
                    }
                    unlock();

                    if (!cv_send.Wait(TimeSpan.FromMilliseconds(Timeout.Infinite)))
                        return false;
                }
            }

            public bool readyToSend()
            {
                while (true)
                {
                    @lock();
                    if (state_wr_send || !state_write)
                    {
                        unlock();
                        return true;
                    }
                    unlock();

                    if (!cv_write.Wait(TimeSpan.FromMilliseconds(Timeout.Infinite)))
                        return false;
                }
            }

            public bool isSetedToSend() => state_send;
            public bool isSetedToWrite() => state_write;
            public bool isSetedToPartial() => state_wr_send;
            public bool isSetedToSendOrPartialSend() => state_send || state_wr_send;

            public void setWrite()
            {
                @lock();
                state_write = true;
                cv_write.Reset();
                unlock();
            }

            public void setSend()
            {
                @lock();
                state_send = true;
                cv_send.Reset();
                unlock();
            }

            public void setPartialSend()
            {
                @lock();
                state_wr_send = true;
                unlock();
            }

            public void releaseWrite()
            {
                @lock();
                state_write = false;
                cv_write.Set();
                unlock();
            }

            public void releaseSend()
            {
                @lock();
                state_send = false;
                cv_send.Set();
                unlock();
            }

            public void releasePartial()
            {
                @lock();
                state_wr_send = true;
                cv_send.Set();
                unlock();
            }

            public void releaseSendAndPartialSend()
            {
                @lock();

                if (state_wr_send && buff.getUsed() <= 0)
                    state_wr_send = true;

                state_send = false;
                cv_send.Set();

                unlock();
            }

            public long increseRequestSendCount()
            {
                return Interlocked.Increment(ref request_send_count);
            }

            public long decreaseRequestSendCount()
            {
                return Interlocked.Decrement(ref request_send_count);
            }
        }
        public Session(TcpClient client)
        { this.m_sock = client; }
        public Session(pangya_packet_handle_base _threadpool)
        {
            this.m_threadpool = _threadpool;
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

            m_request_recv = 0;

            m_check_packet.clear();



            m_buff_r.buff.init(0);
            m_buff_s.buff.init(0);

            setPacketS(null);
            setPacketR(null);

            m_state = false;
            m_connected = false;
            m_connected_to_send = false;
        }

        public Session(pangya_packet_handle_base _threadpool, SOCKET _sock, IPEndPoint _addr, byte _key)
        {
            this.m_threadpool = _threadpool;
            this.m_sock = _sock;
            this.m_addr = _addr;
            this.m_key = _key;
            this.m_use_ctx = new stUseCtx();

            m_ip_maked = false;

            m_oid = -1;

            m_is_authorized = false;

            m_connection_timeout = false;

            m_request_recv = 0;

            m_check_packet.clear();

            m_ip = "";

            make_ip();

            m_buff_r.buff.init(0);
            m_buff_s.buff.init(0);

            setPacketS(null);
            setPacketR(null);
            if (m_sock.Connected)
                m_state = true;
        }

        public void Dispose()
        {

            clear();
        }
        public virtual bool clear()
        {

            if (!m_use_ctx.checkCanQuit())
            {

                message_pool.push(new message("[Session::clear][WARNING] [Session OID=" + Convert.ToString(m_oid) + "] o buffer esta sendo usada, espera, o proximo thread que esta usando tem que limpar a Session.", type_msg.CL_ONLY_FILE_LOG));
             
                m_sock.Close();

                return true;
            }

            m_state = false;
            m_connected = false;
            m_connected_to_send = false;
            m_sock.Dispose();

            m_key = 0;

            m_time_start = 0;
            m_tick = 0;
            m_tick_bot = 0;

            m_oid = ~0;

            m_is_authorized = false;

            m_connection_timeout = false;

            m_request_recv = 0;

            m_check_packet.clear();
            m_ip = "";
            m_ip_maked = false;

            m_buff_s.clear();
            m_buff_r.clear();

            setPacketS(null);

            setPacketR(null);

            m_use_ctx.clear();

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

        public void requestSendBuffer(packet _packet, int _size, bool _raw = false)
        {
            if (_packet == null)
            {
                throw new exception("Error _packet is nullptr. Session::requestSendBuffer()", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.SESSION,
                    3, 0));
            }

            byte[] _buff = _packet.getBuffer().buf;

            if (_buff == null)
            {
                throw new exception("Error _buff is nullptr. Session::requestSendBuffer()", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.SESSION,
                    3, 0));
            }

            if (_size <= 0)
            {
                throw new exception("Error _size is less or equal the zero. Session::requestSendBuffer()", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.SESSION,
                    4, 0));
            }

            m_buff_s.@lock();

            if (isConnectedToSend()
                && m_buff_s.isWritable()
                && isConnectedToSend())
            {

                m_buff_s.setWrite();

                int sz_write = 0;

                do
                {

                    if (isConnectedToSend()
                        && m_buff_s.readyToWrite()
                        && isConnectedToSend())
                    {

                        if (_raw
                            && m_buff_s.buff.getUsed() > 0
                            && m_buff_s.buff.getOperation() != (uint)OP_TYPE.STDA_OT_SEND_RAW_REQUEST
                            && m_buff_s.buff.getOperation() != (uint)OP_TYPE.STDA_OT_SEND_RAW_COMPLETED)
                        {

                            m_buff_s.setPartialSend();

                            m_threadpool.postIoOperation(this,
                                m_buff_s.buff, 0,
                                (uint)OP_TYPE.STDA_OT_SEND_REQUEST);

                        }
                        else
                        {

                            sz_write += m_buff_s.buff.write(_buff.Slice((uint)sz_write), _size - sz_write);

                            // Buffer já chegou ao seu limite, libera o parcial send, para o buffer ser esvaziado(Enviado)
                            if (sz_write < _size)
                            {
                                m_buff_s.setPartialSend();
                            }

                            m_threadpool.postIoOperation(this,
                                m_buff_s.buff, 0,
                                (_raw ? (uint)OP_TYPE.STDA_OT_SEND_RAW_REQUEST : (uint)OP_TYPE.STDA_OT_SEND_REQUEST));

                            m_buff_s.buff.init(0);
                        }

                    }
                    else
                    { // Não conseguiu entrar para escrever ou a Session não está mais conectada, libera o state write

                        m_buff_s.releaseWrite();

                        m_buff_s.unlock();

                        return;
                    }

                } while (sz_write < _size);

                // Libera o State Write
                m_buff_s.releaseWrite();
            }

            m_buff_s.unlock();
        }

        public void requestSendBuffer(byte[] _buff, int _size, bool _raw = false)
        {

            if (_buff == null)
            {
                throw new exception("Error _buff is nullptr. Session::requestSendBuffer()", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.SESSION,
                    3, 0));
            }

            if (_size <= 0)
            {
                throw new exception("Error _size is less or equal the zero. Session::requestSendBuffer()", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.SESSION,
                    4, 0));
            }

            m_buff_s.@lock();

            m_buff_s.clear();
            if (isConnectedToSend())
            {
                int sz_write = m_buff_s.buff.write(_buff.Slice((uint)0), _size);

                // Buffer já chegou ao seu limite, libera o parcial send, para o buffer ser esvaziado(Enviado)
                if (sz_write < _size)
                {
                    m_buff_s.setPartialSend();
                }

                m_threadpool.postIoOperation(this,
                    m_buff_s.buff, 0,
                    (_raw ? (uint)OP_TYPE.STDA_OT_SEND_RAW_REQUEST : (uint)OP_TYPE.STDA_OT_SEND_REQUEST));

            }
            else
            {
                // Não conseguiu entrar para escrever ou a Session não está mais conectada, libera o state write

                m_buff_s.releaseWrite();

                m_buff_s.unlock();

                return;
            }

            m_buff_s.unlock();
        }

        public void requestRecvBuffer()
        {
            while (isConnected() && m_sock.Connected)

            {
                m_threadpool.postIoOperation(this,
                    m_buff_r.buff, 0,
                   (int)OP_TYPE.STDA_OT_RECV_REQUEST);
            }
        }

        public void setRecv()
        {

            m_buff_r.@lock();

            if (m_request_recv <= 0L)
            {

                m_request_recv = 1L;

                m_threadpool.postIoOperation(this,
                    m_buff_r.buff, 0,
                  (uint)OP_TYPE.STDA_OT_RECV_REQUEST);

            }
            else
            {
                m_request_recv++;
            }

            m_buff_r.unlock();
        }
        public void releaseRecv()
        {

            m_buff_r.@lock();

            m_threadpool.postIoOperation(this,
                m_buff_r.buff, 0,
               (uint)OP_TYPE.STDA_OT_RECV_REQUEST);
        }

        public void setSend()
        {

            m_buff_s.@lock();

            // Verifica antes e depois se a Session ainda está conectada
            if (isConnectedToSend()
                && m_buff_s.isSendable()
                && isConnectedToSend())
            {

                if (m_buff_s.buff.getUsed() <= 0)
                {

                    m_buff_s.unlock();

                    throw new exception("[Session::setSend][Error] nao tem nada no buffer para ser enviado[SIZE_BUFF=" + Convert.ToString(m_buff_s.buff.getUsed()) + "].", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.SESSION,
                        5, 0));
                }

                if (isConnectedToSend()
                    && m_buff_s.readyToSend()
                    && isConnectedToSend())
                {

                    m_buff_s.setSend();

                    m_buff_s.increseRequestSendCount();

                }
                else
                {

                    m_buff_s.releaseSendAndPartialSend();

                    m_buff_s.unlock();

                    // Lança o erro para ser exibido e não chamar o WSASend para não da error, pois a Session já não é mais valida para enviar dados
                    throw new exception("[Session::setSend][Error] nao conseguiu set o Send por que a Session nao esta mais conectada.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.SESSION,
                        2, 0));
                }

            }
            else
            {

                m_buff_s.unlock();

                // Lança o erro para ser exibido e não chamar o WSASend para não da error, pois a Session já não é mais valida para enviar dados
                throw new exception("[Session::setSend][Error] nao conseguiu set o Send por que a Session nao esta mais conectada.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.SESSION,
                    2, 0));
            }

            m_buff_s.unlock();
        }
        public void setSendPartial()
        {

            m_buff_s.@lock();

            // Verifica antes e depois se a Session ainda está conectada
            if (m_buff_s.isSetedToSend() && m_buff_s.buff.getUsed() > 0)
            {

                m_buff_s.setPartialSend();

                // Post new request to send buff, to send rest of buff
                m_threadpool.postIoOperation(this,
                    m_buff_s.buff, 0,
                    m_buff_s.buff.getOperation());
            }

            m_buff_s.unlock();
        }
        public void releaseSend()
        {

            m_buff_s.@lock();

            if (m_buff_s.decreaseRequestSendCount() >= 0 || m_buff_s.isSetedToSendOrPartialSend())
            {
                m_buff_s.releaseSendAndPartialSend();
            }
            else
            {
                message_pool.push(new message("[Session::releaseSend][WARNING] todos os request send ja foram liberados.", type_msg.CL_FILE_LOG_AND_CONSOLE));
            }

            m_buff_s.unlock();
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
            bool ret = false;

            @lock();

            ret = m_state;

            unlock();

            return ret;
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

        public packet getPacketS() => m_packet_s;

        public void setPacketS(packet _packet)
        {
            if (_packet == null)
                m_packet_s = new packet();
        }

        public packet getPacketR() => m_packet_r;

        public void setPacketR(packet _packet)
        {
            if (_packet == null)
                m_packet_r = new packet();
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