using PangyaAPI.Network.PangyaSession;
using PangyaAPI.Utilities.Log;
using PangyaAPI.Utilities;
using System;
using System.Threading.Tasks;
using PangyaAPI.Network.PangyaUtil;
using System.Net.Sockets;
using System.Diagnostics;
using System.Configuration;

namespace PangyaAPI.Network.PangyaPacket
{
    public abstract class pangya_packet_handle : pangya_packet_handle_base
    {
        protected abstract void accept_completed(TcpClient client);
        protected abstract void dispach_packet_same_thread(Session _session, packet _packet);
        protected abstract void dispach_packet_sv_same_thread(Session _session, packet _packet);
        public abstract bool DisconnectSession(Session _session);

        protected abstract void translate_packet(Session _session, ref PangyaBuffer lpBuffer, uint dwIOsize, uint operation);

        protected void translate_operation(Session _session, uint dwIOsize, PangyaBuffer lpBuffer, uint _operation)
        { 
             try
            {
                _session.usa();
  
                switch ((OP_TYPE)_operation)
                {
                    case OP_TYPE.STDA_OT_SEND_RAW_REQUEST:
                    case OP_TYPE.STDA_OT_SEND_REQUEST://send server response final packet
                        send_new(_session, ref lpBuffer, _operation + 1);
                        break;
                    case OP_TYPE.STDA_OT_RECV_REQUEST://read client packet
                        recv_new(_session, ref lpBuffer, _operation + 1);
                        break;
                    case OP_TYPE.STDA_OT_SEND_COMPLETED:
                    case OP_TYPE.STDA_OT_SEND_RAW_COMPLETED:
                    case OP_TYPE.STDA_OT_RECV_COMPLETED://translation packet and encrypt/decrypt
                        translate_packet(_session, ref lpBuffer, dwIOsize, _operation);
                        break;
                    case OP_TYPE.STDA_OT_DISPACH_PACKET_SERVER: // decript packet send
                        dispach_packet_sv_same_thread(_session, lpBuffer.getPacket());
                        break;
                    case OP_TYPE.STDA_OT_DISPACH_PACKET_CLIENT: // read packet
                        dispach_packet_same_thread(_session, lpBuffer.getPacket());
                        break;  
                    default: // Operation invalid disconnect Session
                        {
                            Debug.WriteLine("[packet_handle::operation][Error] OPT[VALUE=" + _operation + "] invalid");
                            
                            break;
                        }
                }
            }
            catch (Exception e)
            {
            }
        }

        protected void send_new(Session _session, ref PangyaBuffer lpBuffer, uint operation)
        {
            try
            {
                _session.usa();
                _session.setSend();

                // Seta operação
                lpBuffer.setOperation(operation);

                if (_session.devolve())
                {
                    _session.releaseSend();

                    if (_session.isCreated())
                        translate_operation(_session, 0, lpBuffer, 0);
                    return;
                }
            }
            catch (exception e)
            {
                if (!ExceptionError.STDA_ERROR_CHECK_SOURCE_AND_ERROR_TYPE(e.getCodeError(), STDA_ERROR_TYPE.SESSION, 6))
                {
                    if (_session.devolve())
                    {
                        if (_session.isCreated())
                            translate_operation(_session, 0, lpBuffer, 0);
                        return;
                    }
                }

                if (ExceptionError.STDA_ERROR_CHECK_SOURCE_AND_ERROR_TYPE(e.getCodeError(), STDA_ERROR_TYPE.SESSION, 2))
                    throw;

                return;
            }

            try
            {
                var payloadData = lpBuffer.getWSABufToSend();


                if (!_session.m_sock.Send(payloadData.buf, (int)payloadData.len))
                {
                    _session.@lock();
                    _session.setConnectedToSend(false);
                    _session.unlock();

                    try
                    {
                        _session.releaseSend();
                    }
                    catch (exception e)
                    {
                        message_pool.push(new message("[threadpool::send_new][Error] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
                    }

                    if (_session.isCreated())
                        translate_operation(_session, 0, lpBuffer, 0);
                }
                else
                {
                    translate_operation(_session, 1, lpBuffer, 2);
                }
            }
            catch (exception e)
            {
                throw e;
            }
        }

        protected void recv_new(Session _session, ref PangyaBuffer lpBuffer, uint operation)
        {
            try
            {

                lpBuffer.setOperation(operation);

                if (!_session.isConnected() || !_session.m_sock.Connected)
                    return;

                var result = _session.m_sock.Read();
                if (result.check)
                {
                    lpBuffer.init(result._buffer, result.len, 0);

                    if (_session.isCreated())
                    {
                        translate_operation(_session, (uint)result.len, lpBuffer, (uint)OP_TYPE.STDA_OT_RECV_COMPLETED);

                        if (_session.m_sock.Connected)
                            recv_new(_session, ref lpBuffer, operation);
                    }
                }
                else
                    DisconnectSession(_session);
            }
            catch (exception e)
            {
                throw e;
            }
        }

        void pangya_packet_handle_base.postIoOperation(Session _session, PangyaBuffer lpBuffer, uint dwIOsize, uint operation) => Task.Run(() => translate_operation(_session, dwIOsize, lpBuffer, operation));
        void pangya_packet_handle_base.postIoOperation(Session _session, PangyaBuffer lpBuffer, uint dwIOsize, OP_TYPE operation) => Task.Run(() => translate_operation(_session, dwIOsize, lpBuffer, (uint)operation));
    }
}