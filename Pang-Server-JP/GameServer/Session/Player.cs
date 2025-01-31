using GameServer.Game;
using GameServer.PangType;
using PangyaAPI.Network.PangyaSession;
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
            return ((uint)m_pi.uid);
        }

        public override string getID()
        {
            return m_pi.id;
        }
             
        public override uint getCapability() { return m_pi.m_cap.ulCapability; }      

        public void SendChannel_broadcast(Channel _channel, byte[] p)
        {

            List<Player> channel_session = _channel.getSessions();  

            for (var i = 0; i < channel_session.Count; ++i)
            {
                channel_session[i].Send(p);//alguma coisa ta dando errado aqui :/
            }
        }

        public void SendLobby_broadcast(Channel _channel, byte[] p)
        {

            List<Player> channel_session = _channel.getSessions();  //gs->getSessionPool().getChannelSessions(s->m_channel);

            for (var i = 0; i < channel_session.Count; ++i)
            {
                if (channel_session[i].m_pi.mi.sala_numero == ushort.MaxValue)
                    channel_session[i].Send(p);//@!errado
            }

            //delete p;
        }
    }
}
