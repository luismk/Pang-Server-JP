using PangyaAPI.Network.PangyaSession;
using PangyaAPI.Network.PangyaUtil;
using PangyaAPI.Utilities;
using PangyaAPI.Utilities.BinaryModels;
using PangyaAPI.Utilities.Log;
using System.Collections.Generic;
using Session = PangyaAPI.Network.PangyaSession.Session;
namespace PangyaAPI.Network.PangyaPacket
{
    public class packet_func_base
    {
        public static func_arr funcs = new func_arr();      // Cliente
        public static func_arr funcs_sv = new func_arr();   // Server (Retorno)
        public static func_arr funcs_as = new func_arr(); // Auth Server

        public static void MakeBeginPacket(object arg)
        {
            var pd = (ParamDispatch)arg;
            message_pool.push(new message($"Trata pacote {pd._packet.getTipo()}(0x{pd._packet.getTipo():X})", type_msg.CL_FILE_LOG_AND_CONSOLE));
        }

        public static void MakeBeginSplitPacket(int packetId, Session session, int elementSize, int maxPacket, List<byte[]> elements, bool debug)
        {
            int porPacket = (maxPacket - 100) > elementSize ? (maxPacket - 100) / elementSize : 1;
            int total = elements.Count;
            int index = 0;

            foreach (var element in elements)
            {
                var p = new PangyaBinaryWriter();
                p.init_plain((ushort)packetId);

                p.WriteInt16((short)total);
                p.WriteInt16((short)(total > porPacket ? porPacket : total));

                for (int i = 0; i < porPacket && index < elements.Count; i++, index++)
                {
                    p.WriteBuffer(element, elementSize);
                }
             }
        }

        public static void MAKE_SEND_BUFFER(packet _packet, Session _session)
        {
            _packet.encrypt(_session.m_key);
            var mb = _packet.getBuffer();
            try
            {

                _session.usa();

                _session.requestSendBuffer(mb.buf, mb.len);
                
                if (_session.devolve())
                    _session.Disconnect(); 
            }
            catch (exception e)
            {

                if (!ExceptionError.STDA_ERROR_CHECK_SOURCE_AND_ERROR_TYPE(e.getCodeError(), STDA_ERROR_TYPE.SESSION, 6/*n�o pode usa session*/))
                    if (_session.devolve()) 
                        _session.Disconnect();

                if (ExceptionError.STDA_ERROR_CHECK_SOURCE_AND_ERROR_TYPE(e.getCodeError(), STDA_ERROR_TYPE.SESSION, 2))
                    throw;
            }       
        }
    }
}
