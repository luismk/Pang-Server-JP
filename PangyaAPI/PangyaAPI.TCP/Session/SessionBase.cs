using PangyaAPI.Utilities;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace PangyaAPI.TCP.Session
{

    // Placeholder class definitions
    public abstract class SessionBase : IDisposable
    {
        // Propriedades e campos
        public TcpClient _client { get; set; }
        public IPEndPoint Address { get; set; }
        public byte m_key { get; set; }
        public uint m_oid { get; set; }
        public int m_start_time { get; set; }
        public int m_tick { get; set; }
        public bool m_is_authorized { get; set; }
        public uint Uid { get; set; }
        public string Nickname { get; set; }            

        // Contexto de sincronização
        private readonly object _lockSync = new object();

        private bool m_connected;
        private bool m_state;
        private bool m_connectedToSend;
        private string m_ip;

        // Sincronização do uso da session
        private stUseCtx m_use_ctx;
        ThreadPool.MyThreadPool m_threadpool;
        // Construtores
        public SessionBase()
        {
            m_connected = false;
            m_state = true;
            m_connectedToSend = false;
            m_use_ctx = new stUseCtx();
        }

        public SessionBase(TcpClient sock, IPEndPoint addr, byte key)
            : this()
        {
            _client = sock;
            Address = addr;
            m_key = key;
            m_use_ctx = new stUseCtx();
        }

        // Métodos
        public bool Clear()
        {
            try
            {          
                m_state = false;
                m_connected = false;
                m_connectedToSend = false;

                m_key = 0;
                Address = null;

                m_start_time = 0;
                m_tick = 0;

                m_oid = 0;

                m_is_authorized = false;


                m_use_ctx.Clear();
                Dispose();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public string getIP()
        {
            return Address?.Address.ToString();
        }

        // Métodos de bloqueio
        public void Lock()
        {
            Monitor.Enter(_lockSync);
        }

        public void Unlock()
        {
            Monitor.Exit(_lockSync);
        }

        // Usado para sincronizar outras coisas da sessão, como pacotes
        public void LockSync()
        {
            Monitor.Enter(_lockSync);
        }

        public void UnlockSync()
        {
            Monitor.Exit(_lockSync);
        }

        public bool SendResponse(byte[] buff, bool raw = false)
        {
            return RequestSendBuffer(buff, raw);
        }

        public bool SendBuffer(byte[] buff, bool raw = false)
        {
            return RequestSendBuffer(buff, raw);
        }

        // Métodos de envio e recebimento
        protected bool RequestSendBuffer(byte[] buff, bool raw = false)
        {
            try
            {
                // Validação do buffer
                if (buff == null || buff.Length == 0)
                {
                    throw new Exception("Error: Buffer is null or empty. session::RequestSendBuffer()");
                }

                // Encriptação, se necessário
                byte[] _buff = raw ? buff : Cryptor.HandlePacket.Pang.ServerEncrypt(buff, m_key);

                // Verificação do socket e envio dos dados
                if (m_connected)
                {
                    _client.GetStream().Write(_buff, 0, _buff.Length);

                    return true;
                }
                else
                {
                    throw new Exception("_client is not connected or valid.");
                }
            }
            catch (Exception ex)
            {
                // Log para rastrear o erro
                Console.WriteLine($"Error in RequestSendBuffer: {ex.Message}");
                return false;
            }
        }

        public bool IsConnected() => (bool)(_client?.Connected);
        public bool IsCreated()
        {
            bool ret = false;

            Lock();

            ret = m_state;

            Unlock();

            return ret;
        }

        public int GetConnectTime()
        {
            return (Environment.TickCount - m_start_time);
        }

        // Métodos de estado
        public bool GetState() => m_state;

        public bool getConnected()
        {
            return m_connected;
        }

        public void SetState(bool state)
        {
            m_state = state;
        }

        public void SetConnected(bool connected)
        {
            m_connected = connected;
        }

        public void SetConnectedToSend(bool connectedToSend)
        {
            m_connectedToSend = connectedToSend;
        }


        public abstract uint getUID();

        public abstract uint getCapability();

        public abstract string getNickname();

        public abstract string getID();

        // Métodos privados
        public void MakeIP()
        {
            if (Address != null)
            {
                m_ip = Address.Address.ToString();
            }
        }

        private bool IsConnectedToSend()
        {
            return m_connectedToSend;
        }


        public int usa()
        {

            if (!IsConnected())
                throw new exception("[session::usa][error] nao pode usa porque o session nao esta mais conectado.");

            return m_use_ctx.Usa();
        }

        public bool devolve()
        {
            return m_use_ctx.Devolve();
        }

        public bool isQuit()
        {
            return m_use_ctx.IsQuit();
        }

        // Método de descarte de recursos
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
				m_connected = false;
                // Dispose de recursos gerenciados
                _client.Close();
            }

            // Dispose de recursos não gerenciados
        }

        // Finalizador
        ~SessionBase()
        {
            Dispose(false);
        }

        // Implementação do IDisposable
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }     
    }
    // A classe stUseCtx é integrada aqui, ela controla o acesso sincronizado à session
    public class stUseCtx : IDisposable
    {
        private readonly object _lock = new object();
        private int m_active;
        private bool m_quit;

        public stUseCtx()
        {
            m_active = 0;
            m_quit = false;
            Clear();
        }

        public void Clear()
        {
            lock (_lock)
            {
                m_active = 0;
                m_quit = false;
            }
        }

        public bool IsQuit()
        {
            bool quit;
            lock (_lock)
            {
                quit = m_quit;
            }
            return quit;
        }

        public int Usa()
        {
            int spin;
            lock (_lock)
            {
                spin = ++m_active;
            }
            return spin;
        }

        public bool Devolve()
        {
            bool canDevolve;
            lock (_lock)
            {
                --m_active;
                canDevolve = m_active <= 0 && m_quit;
            }
            return canDevolve;
        }

        public bool CheckCanQuit()
        {
            bool canQuit;
            lock (_lock)
            {
                if (m_active <= 0)
                    canQuit = true;
                else
                {
                    m_quit = true;
                    canQuit = false;
                }
            }
            return canQuit;
        }

        public void Dispose()
        {
            // Dispose recursos, se necessário
        }
    }
}