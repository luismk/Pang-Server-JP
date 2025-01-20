using PangyaAPI.Cryptor.HandlePacket;
using PangyaAPI.Utilities.BinaryModels;
using PangyaAPI.Utilities.Interface;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using PangyaAPI.Utilities;
using System.Threading;
using PangyaAPI.Network.PangyaServer;

namespace PangyaAPI.Network.PangyaSession
{
    public abstract partial class SessionBase : IDisposeable
    {
        #region Public Fields
                                               

        /// <summary>
        /// Servidor em que o cliente está conectado
        /// </summary>
        public Server Server { get; set; }

        /// <summary>
        /// Conexão do cliente
        /// </summary>
        public TcpClient Tcp { get; set; }
        /// <summary>
        /// Chave de criptografia e decriptografia
        /// </summary>
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

        public PangyaBinaryWriter Response { get; set; }
        // Sincronização do uso da session
        private stUseCtx m_use_ctx;                               
        #endregion

        #region Constructor
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
            if (m_connected)
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

        public void Disconnect()
        {
            Server.DisconnectSession(this);
        }
        #endregion

        #region Player Send Packets 
        public void Send(PangyaBinaryWriter packet)
        {
            var buffer = packet.GetBytes.ServerEncrypt(m_key);

            SendBytes(buffer);
        }

        public void Write(PangyaBinaryWriter Data)
        {
            Send(Data);
        }

        public void Write(byte[] Data)
        {
            Send(Data);
        }
        public void SendResponse(List<byte[]> Data)
        {
            Response = new PangyaBinaryWriter();
            Data.ForEach(item => Response.Write(item));
            SendResponse();
        }
        public void SendResponse(byte[] Data)
        {
            Send(Data);
        }
        public void SendToAll(byte[] Data)
        {
            Server.SendToAll(Data);
        }

        public void SendResponse(byte[] Header, byte[] Data)
        {
            Response.Write(Header);
            Response.Write(Data);
            Send(Response.GetBytes);
            Response.Clear();
        }
        public void SendResponse()
        {
            var buffer = Response.GetBytes.ServerEncrypt(m_key);

            SendBytes(buffer);
            if (Response.GetSize > 0)
            {
                Response.Clear();
            }
        }
        public void SendResponse(PangyaBinaryWriter packet)
        {
            Send(packet);
            if (packet.GetSize > 0)
            {
                Response.Clear();
            }
        }
        public void Send(byte[] Data)
        {
            var buffer = Data.ServerEncrypt(m_key);

            SendBytes(buffer);
        }

        public void SendBytes(byte[] buffer)
        {
            if (Tcp.Connected && m_connected)
            {
                Tcp.GetStream().Write(buffer, 0, buffer.Length);
            }
        }

        #endregion

        #region Dispose

        // booleano para controlar se
        // o método Dispose já foi chamado
        public bool Disposed { get; set; }

        // método privado para controle
        // da liberação dos recursos
        private void Dispose(bool disposing)
        {
            // Verifique se Dispose já foi chamado.
            if (!this.Disposed)
            {
                if (disposing)
                {
                    // Liberando recursos gerenciados
                    this.m_connected = false;   
                    Tcp.Dispose();
                }

                // Seta a variável booleana para true,
                // indicando que os recursos já foram liberados
                Disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }



        /// <summary>
        /// Destrutor
        /// </summary>
        ~SessionBase()
        {
            Dispose(false);
        }


        #endregion
    }
    // A classe stUseCtx é integrada aqui, ela controla o acesso sincronizado à session
    public class stUseCtx
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

    }
}
