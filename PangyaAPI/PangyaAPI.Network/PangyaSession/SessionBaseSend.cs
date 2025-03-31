using PangyaAPI.Network.Cmd;
using PangyaAPI.Network.Cryptor;
using PangyaAPI.Utilities;
using PangyaAPI.Utilities.BinaryModels;
using System;
using System.Collections.Generic;

namespace PangyaAPI.Network.PangyaSession
{
    public abstract partial class SessionBase
    {                                                                                                            
        public virtual void Send(byte[] data, int _compress = 1)
        { 
            if (m_key != 255 && _compress == 1) // Encrypt se necessário
                data = data.ServerEncrypt(m_key, 0);

            SafeSend(data); // Adiciona à fila sem bloquear
        } 

        public virtual void Send(PangyaBinaryWriter packet, bool debug_log = true)
        {
            if (debug_log)
                Console.WriteLine("[SessionBase::Send][HexLog]: " + packet.GetBytes.HexDump() + Environment.NewLine);

            if (m_key != 255)
                SafeSend(packet.GetBytes.ServerEncrypt(m_key, 0));

        }

        public virtual void Send(List<PangyaBinaryWriter> packet)
        {
 
            if (m_key != 255)
                for (int i = 0; i < packet.Count; i++)
                    SafeSend(packet[i].GetBytes.ServerEncrypt(m_key, 0));

        }

    }
}
