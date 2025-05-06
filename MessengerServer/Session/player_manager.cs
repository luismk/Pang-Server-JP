using System.Collections.Generic;
using PangyaAPI.Network.PangyaSession;
using MessengerServer.GameType;
using PangyaAPI.Network.PangyaPacket;

namespace MessengerServer.Session
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

        public Dictionary<uint, Player> findAllFriend(List<FriendInfoEx> friends)
        {
            var friendMap = new Dictionary<uint, Player>();

            foreach (var el in friends)
            {
                var player = (Player)findSessionByUID(el.uid);

                if (player != null && !friendMap.ContainsKey(player.m_pi.uid))
                {
                    friendMap[player.m_pi.uid] = player;
                }
            }

            return friendMap;
        }

        public Dictionary<uint, Player> findAllGuildMember(uint guildUid)
        {
            var guildMap = new Dictionary<uint, Player>();

            foreach (var el in m_sessions)
            {
                var player = el as Player;

                if (player != null && player.m_pi.guild_uid > 0 && player.m_pi.guild_uid == guildUid)
                {
                    if (!guildMap.ContainsKey(player.m_pi.uid))
                    {
                        guildMap[player.m_pi.uid] = player;
                    }
                }
            }

            return guildMap;
        }
    }
}