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
        #endregion

        #region Constructor
        // Construtores
        public SessionBase()
        {
            m_connected = false;
            m_state = true;
            m_connectedToSend = false;                        
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
            if (_client.Connected && m_connected)
            {
                _client.GetStream().Write(buffer, 0, buffer.Length);
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
                    _client.Dispose();
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
     
}
