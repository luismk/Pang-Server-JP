using PangyaAPI.Network.PangyaPacket;
using PangyaAPI.Network.PangyaSession;
using PangyaAPI.Network.PangyaUtil;
using PangyaAPI.Utilities.Log;
using PangyaAPI.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;

namespace PangyaAPI.Network.PangyaPacket
{

    public abstract class pangya_packet_packet_handle_server : pangya_packet_handle
    { 
        protected override void translate_packet(Session _session, ref PangyaBuffer lpBuffer, uint dwIOsize, uint operation)
        { 
            switch ((OP_TYPE)operation)
            {
                case OP_TYPE.STDA_OT_SEND_RAW_COMPLETED:

                    if (dwIOsize > 0 && lpBuffer != null)
                    {

                        try
                        {

                            lpBuffer.consume(dwIOsize);

                            _session.releaseSend();

                        }
                        catch (exception e)
                        {

                           

                            _session.releaseSend();

                            message_pool.push(new message("[threadpl_server::translate_packet][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                        }
                        catch (System.Exception e)
                        {

                           

                            _session.releaseSend();

                            message_pool.push(new message("[threadpl_server::translate_packet][ErrorSystem] " + e.Message, type_msg.CL_FILE_LOG_AND_CONSOLE));
                        }

                    }
                    else
                    {
                        _session.releaseSend();

                        try
                        {

                            // getConnectTime pode lançar exception
                            if (_session.getConnectTime() <= 0 && _session.getState())
                            {

                                message_pool.push(new message("[threadpl_server::translate_packet][Error] [STDA_OT_SEND_RAW_COMPLETED] _session[OID=" + Convert.ToString(_session.m_oid) + "] is not connected.", type_msg.CL_FILE_LOG_AND_CONSOLE));

                                // Ainda não habilitar o disconnect Session, vms aguardar as mensagens para ver se vai ter
                                DisconnectSession(_session);
                            }

                        }
                        catch (exception e)
                        {

                            message_pool.push(new message("[threadpl_server::translate_packet][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
                        }
                    }

                    break;
                case OP_TYPE.STDA_OT_SEND_COMPLETED:

                    if (dwIOsize > 0 && lpBuffer != null)
                    {

                        packet_head_client ph = new packet_head_client();
                        do
                        {

                            try
                            {
                                packet _packet = new packet();

                                _packet.encrypt(lpBuffer.getWSABufToSend().buf, _session.m_key);

                                lpBuffer = new PangyaBuffer(_packet.getBuffer().buf, 0);
                                translate_operation(_session,dwIOsize, lpBuffer, 0);
                            }
                            catch (exception e)
                            {

                                message_pool.push(new message("SEND_COMPLETED MY class exception {SESSION[IP=" + (_session.getIP()) + "]}: " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                                if (ExceptionError.STDA_SOURCE_ERROR_DECODE_TYPE(e.getCodeError()) == STDA_ERROR_TYPE.CRYPT || ExceptionError.STDA_SOURCE_ERROR_DECODE_TYPE(e.getCodeError()) == STDA_ERROR_TYPE.COMPRESS)
                                {

                                   



                                }
                                else if (ExceptionError.STDA_ERROR_CHECK_SOURCE_AND_ERROR_TYPE(e.getCodeError(),
                                    STDA_ERROR_TYPE.PACKET, 15) || ExceptionError.STDA_ERROR_CHECK_SOURCE_AND_ERROR_TYPE(e.getCodeError(),
                                        STDA_ERROR_TYPE.PACKET, 1))
                                {

                                   





                                    // Libera o send
                                    _session.releaseSend();

                                    //break;
                                    return; // Return por que o break só vai sair do while, e esse break é para sair do switch, mas como depois do switch não tem nada, então só terminar a função

                                }
                                else
                                { 
                                    // Libera o send
                                    _session.releaseSend();

                                    //break;
                                    return; // Return por que o break só vai sair do while, e esse break é para sair do switch, mas como depois do switch não tem nada, então só terminar a função
                                }

                            }
                            catch (System.Exception e)
                            {

                                message_pool.push(new message("SEND_COMPLETED std::class exception {SESSION[IP=" + (_session.getIP()) + "]}: " + (e.Message), type_msg.CL_FILE_LOG_AND_CONSOLE));

                               





                                // Libera o send
                                _session.releaseSend();

                                //break;
                                return; // Return por que o break só vai sair do while, e esse break é para sair do switch, mas como depois do switch não tem nada, então só terminar a função

                            }
                            catch
                            {
                                message_pool.push(new message("SEND_COMPLETED Exception desconhecida. {SESSION[IP=" + (_session.getIP()) + "]}", type_msg.CL_FILE_LOG_AND_CONSOLE));

                               


                                // Libera o send
                                _session.releaseSend();

                                //break;
                                return; // Return por que o break só vai sair do while, e esse break é para sair do switch, mas como depois do switch não tem nada, então só terminar a função
                            }
                        } while (dwIOsize > 0);

                        // Libera o send
                        _session.releaseSend();

                    }
                    else
                    {

                        // Libera o send
                        _session.releaseSend();

                        try
                        {

                            // getConnectTime pode lançar exception
                            if (_session.getConnectTime() <= 0 && _session.getState())
                            {

                                message_pool.push(new message("[threadpl_server::translate_packet][Error] [STDA_OT_SEND_COMPLETED] _session[OID=" + Convert.ToString(_session.m_oid) + "] is not connected.", type_msg.CL_FILE_LOG_AND_CONSOLE));

                                // Ainda não habilitar o disconnect Session, vms aguardar as mensagens para ver se vai ter
                                //DisconnectSession(_session);
                            }

                        }
                        catch (exception e)
                        {

                            message_pool.push(new message("[threadpl_server::translate_packet][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
                        }
                    }

                    break;
                case OP_TYPE.STDA_OT_RECV_COMPLETED:
                    if (dwIOsize > 0 && lpBuffer != null)
                    {
                        try
                        {

                            packet _packet = new packet();

                            _packet.decrypt(lpBuffer.getWSABufToSend().buf, _session.m_key);
                            
                            lpBuffer = new PangyaBuffer(_packet.Decrypt_Msg, 0);

                            translate_operation(_session, dwIOsize, lpBuffer, 7);
                         }
                        catch (exception e)
                        {
                            message_pool.push(new message("RECV_COMPLETED MY class exception {SESSION[IP=" + (_session.getIP()) + "]}: " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                            if (ExceptionError.STDA_SOURCE_ERROR_DECODE_TYPE(e.getCodeError()) == STDA_ERROR_TYPE.CRYPT || ExceptionError.STDA_SOURCE_ERROR_DECODE_TYPE(e.getCodeError()) == STDA_ERROR_TYPE.COMPRESS)
                            { 
                                break; 
                            }
                            else if (ExceptionError.STDA_ERROR_CHECK_SOURCE_AND_ERROR_TYPE(e.getCodeError(),
                                STDA_ERROR_TYPE.PACKET, 15) || ExceptionError.STDA_ERROR_CHECK_SOURCE_AND_ERROR_TYPE(e.getCodeError(),
                                    STDA_ERROR_TYPE.PACKET, 1))
                            { 
                                break;
                            }
                            else
                            { 
                                break;
                            }
                        }
                        catch (System.Exception e)
                        {
                            message_pool.push(new message("RECV_COMPLETED std::class exception {SESSION[IP=" + (_session.getIP()) + "]}: " + (e.Message), type_msg.CL_FILE_LOG_AND_CONSOLE));
                             
                            break;
                        }
                        catch
                        {
                            message_pool.push(new message("RECV_COMPLETED Exception desconhecida {SESSION[IP=" + (_session.getIP()) + "]}.", type_msg.CL_FILE_LOG_AND_CONSOLE));
                             
                            break;
                        }
                    }
                    else
                    {
                        if (_session != null)
                        {
                            DisconnectSession(_session);
                        }
                    }

                    break;
            }
        }

    }
}
