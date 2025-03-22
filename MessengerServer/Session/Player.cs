using MessengerServer.Cmd;
using MessengerServer.Manager;
using MessengerServer.MessengerServerTcp;
using MessengerServer.GameType;
using PangLib.IFF.JP.Models.Data;
using PangyaAPI.Network.Pangya_St;
using PangyaAPI.Network.PangyaSession;
using PangyaAPI.SQL.Manager;
using PangyaAPI.Utilities;
using PangyaAPI.Utilities.BinaryModels;
using PangyaAPI.Utilities.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

namespace MessengerServer.Session
{
    public class Player : SessionBase
    {
        public PlayerInfo m_pi { get; set; }
         public Player()
        {
            m_pi = new PlayerInfo();
         }
        public override string getNickname()
        {
            return m_pi.nickname;
        }

        public override uint getUID()
        {
            return m_pi.uid;
        }

        public override string getID()
        {
            return m_pi.id;
        }

        public override uint getCapability() { return (uint)m_pi.m_cap; }

        public override bool Clear()
        {
            bool ret;
            if ((ret = base.Clear()))
            {

                // Player Info
                m_pi.clear();
                                 
            }
            return ret;
        }

        #region Send Packets
        public void Send(List<PangyaBinaryWriter> packet, bool debug_log = false)
        {
            for (int i = 0; i < packet.Count; i++)
                base.Send(packet[i], debug_log);
        }
        public override void Send(PangyaBinaryWriter packet, bool debug_log = false)
        {
            base.Send(packet, debug_log);
        }

        public override void Send(byte[] Data, bool debug_log = false)
        {
            base.Send(Data, debug_log);
        }

        
        #endregion
         
    }
}
