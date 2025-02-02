using GameServer.Game;
using GameServer.PangType;
using PangyaAPI.Network.PangyaSession;
using PangyaAPI.Utilities.BinaryModels;
using System.Collections.Generic;

namespace GameServer.Session
{
    public class Player : SessionBase
    {
        public PlayerInfo m_pi { get; set; }
        public GMInfo m_gi { get; set; }                               
        public Player()
        {
            m_pi = new PlayerInfo();
            m_gi = new GMInfo();
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

        public override uint getCapability() { return m_pi.m_cap.ulCapability; }

        public bool IsGM => m_pi.m_cap.ulCapability == 4;

        public override void Send(PangyaBinaryWriter packet, bool debug = false)
        {
            base.Send(packet, debug);
        }

        public override void Send(byte[] Data, bool debug = false)
        {
            base.Send(Data, debug);
        }

        public void SendChannel_broadcast(byte[] p)
        {
            List<Player> channel_session = Program.gs.findChannel(m_pi.channel).getSessions();

            for (var i = 0; i < channel_session.Count; ++i)
            {
                channel_session[i].Send(p);
            }
        }

        public void SendLobby_broadcast(byte[] p)
        {
            List<Player> channel_session = Program.gs.findChannel(m_pi.channel).getSessions();

            for (var i = 0; i < channel_session.Count; ++i)
            {
                if (channel_session[i].m_pi.mi.sala_numero == ushort.MaxValue)
                    channel_session[i].Send(p);//@!errado
            }                                                          
        }
    }
}
