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
using System.Runtime.CompilerServices;
using System.Diagnostics;
using static System.Collections.Specialized.BitVector32;
using System.Runtime.InteropServices.WindowsRuntime;

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
        public int m_tick_bot { get; set; }
        public bool m_is_authorized { get; set; }
        public uint Uid { get; set; }
        public string Nickname { get; set; }

        // Contexto de sincronização
        private readonly object _lockSync = new object();
                                           
        private bool m_state;             
        private bool disposing = false;

        #endregion

        #region Constructor
        // Construtores
        public SessionBase()
        {
            m_oid = uint.MaxValue;
            m_state = true;
        }
        #endregion

        #region Methods
        // Métodos
        public bool Clear()
        {
            // Se já foi chamado antes e não está pronto para limpar, retorna false
            if (disposing)
            {
                return false;
            }

            // Se já estava pronto, executa a limpeza
            try
            {
                disposing = true; // Marca como tentativa de limpeza
               
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

        public int GetConnectTime() => (Environment.TickCount - m_start_time);

        // Métodos de estado
        public bool GetState() => m_state;     

        public void SetState(bool state) => m_state = state;

        public bool getConnected()
        {
            try
            {
                if (_client != null && _client.Client != null && _client.Client.Connected)
                {
                    return !(_client.Client.Poll(1, SelectMode.SelectRead) && _client.Client.Available == 0);
                }
                return false;
            }
            catch (SocketException ex)
            {
                Console.WriteLine($"Erro de socket ao verificar conexão: {ex.Message} | Código: {ex.SocketErrorCode}");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro inesperado ao verificar conexão: {ex.Message}");
                return false;
            }
        }
                               
        public abstract uint getUID();

        public abstract uint getCapability();

        public abstract string getNickname();

        public abstract string getID();
              

        public void Disconnect()
        {
            try
            {    
                if (Server != null)
                {
                    Server.DisconnectSession(this);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao desconectar cliente: {ex.Message}");
            }
        }
        #endregion

        #region Player Send Packets 
        public virtual void Send(PangyaBinaryWriter packet, bool debug_log = true)
        {
            if (debug_log)
                Debug.WriteLine("[SessionBase::Send1][HexLog]: " + packet.GetBytes.HexDump() + Environment.NewLine);
            SafeSend(packet.GetBytes.ServerEncrypt(m_key));
        }

        public virtual void Send(byte[] Data, bool debug_log = true)
        {
            if (debug_log)
                Debug.WriteLine("[SessionBase::Send2][HexLog]: " + Data.HexDump() + Environment.NewLine);
                                               
            SafeSend(Data.ServerEncrypt(m_key));
        }

        protected void SendBytes(byte[] buffer)
        {
            try
            {
                if (_client != null && _client.GetStream().CanWrite)
                {
                    _client.GetStream().Write(buffer, 0, buffer.Length);
                }
                else
                {
                    throw new IOException("O stream do cliente não está disponível para escrita.");
                }
            }
            catch (Exception ex)
            {
                // Registre o erro com mais detalhes
                Console.WriteLine($"Erro ao enviar bytes: {ex.Message}");
                throw;
            }
        }

        public void SafeSend(byte[] buffer)
        {
            try
            {
                if (getConnected())
                {
                    SendBytes(buffer);
                }    
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao enviar dados: {ex.Message} | StackTrace: {ex.StackTrace}");
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
                    Server = null;
                    m_state = false;            
                    m_key = byte.MaxValue;
                    Address = null;

                    m_start_time = int.MaxValue;
                    m_tick = int.MaxValue;
                    m_oid = uint.MaxValue;
                    m_is_authorized = false;

                    this._client.Dispose();
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
