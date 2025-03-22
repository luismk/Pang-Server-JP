using PangyaAPI.Utilities.BinaryModels;
using PangyaAPI.Utilities.Log;
using System.Collections.Generic;
using Session = PangyaAPI.Network.PangyaSession.SessionBase;
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
            message_pool.push(new message($"Trata pacote {pd._packet.Id}(0x{pd._packet.Id:X})", type_msg.CL_FILE_LOG_AND_CONSOLE));
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
                session.Send(p, debug); 
            }
        }
    }             
}
