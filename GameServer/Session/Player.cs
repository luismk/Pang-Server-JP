using GameServer.PangType;
using PangyaAPI.TCP.Session;   
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
        public void Close()
        {
            Program.gs.DisconnectSession(this);
        }
    }
}
