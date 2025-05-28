using System.Diagnostics;
using System.Net.Sockets;
using PangyaAPI.Network.Cryptor;
using PangyaAPI.Network.PangyaSession;
using PangyaAPI.Network.PangyaUtil;
using PangyaAPI.Utilities;
using PangyaAPI.Utilities.Log;

namespace PangyaAPI.Network.PangyaPacket
{
    public abstract class pangya_packet_handle : pangya_packet_handle_base
    {
        private ToServerBuffer ToServerBuffer = new ToServerBuffer();
        protected abstract void accept_completed(TcpClient client);
        protected abstract void dispach_packet_same_thread(Session _session, packet _packet);
        public abstract void dispach_packet_sv_same_thread(Session _session, packet _packet);
        public abstract bool DisconnectSession(Session _session);

        protected bool recv_new(Session _session)
        {
            try
            {
                if (!_session.isConnected() || !_session.m_sock.Connected)
                    return false;//falso pq deu errado

                var result = _session.m_sock.Read();


                if (result.check)
                {
                    if (_session.isCreated() && result.len >= 5)
                    {
                        var decryptedPackets = ToServerBuffer.getPackets(result._buffer, _session.m_key); //interpreta packets
                        foreach (var _packet in decryptedPackets)
                        {
                            dispach_packet_same_thread(_session, _packet);//ler e cuida com packets
                        }
                        return true; //true se caso deu certo
                    }
                    else
                    {
                        Debug.WriteLine("[pangya_packet_handle::recv_new] [Log] " + result.len);
                        return false;//falso pq deu errado
                    }
                }
                else
                {
                    Debug.WriteLine("[pangya_packet_handle::recv_new] [Log] " + result.len);
                    DisconnectSession(_session);//desconecta pq deu errado
                }
            }
            catch (exception e)
            {
                throw e;//falso pq deu errado
            }
            return false;//falso pq deu errado
        }

        void pangya_packet_handle_base.postIoOperation(Session _session, PangyaBuffer lpBuffer, uint dwIOsize, uint operation) { }
        void pangya_packet_handle_base.postIoOperation(Session _session, PangyaBuffer lpBuffer, uint dwIOsize, OP_TYPE operation) { }
    }
}