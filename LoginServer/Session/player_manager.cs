using System.Collections.Generic;
using PangyaAPI.Network.PangyaSession;
using LoginServer.GameType;
using PangyaAPI.Network.PangyaPacket;

namespace LoginServer.Session
{
    public class player_manager : SessionManager
    {
        public player_manager(pangya_packet_handle_base _threadpool, uint _max_session) : base(_threadpool, _max_session)
        {
            if (_max_session != 0)
            {
                for (var i = 0u; i < _max_session; ++i)
                    m_sessions.Add(new Player(m_threadpool) { m_oid = int.MaxValue });
            }
        }

        public new void Clear()
        {
            base.Clear();
        }

        public Player findPlayer(uint? _uid, bool _oid = true)
        {

            foreach (var el in m_sessions)
            {
                if ((_oid ? el.getUID() : (uint)el.m_oid) == _uid)
                {
                    return (Player)el;
                }
            }


            return null;
        }

        public Player FindPlayer(uint uid, bool oid)
        {
            Player p = null;
            foreach (var el in m_sessions)
            {
                if (el.m_sock != null && ((!oid) ? el.getUID() : (uint)el.m_oid) == uid)
                {
                    p = (Player)el;
                    break;
                }
            }

            return p;
        }

        public List<Player> FindAllGM()
        {
            var gmList = new List<Player>();

            foreach (var el in m_sessions)
            {
                if (el.m_sock != null && ((el.getCapability() & 4) != 0 || (el.getCapability() & 128) != 0))
                {
                    gmList.Add((Player)el);
                }
            }

            return gmList;
        }
         
    }
}