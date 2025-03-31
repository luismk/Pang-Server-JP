using LoginServer.GameType;
using PangyaAPI.Network.PangyaSession;

namespace LoginServer.Session
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
         
    }
}
