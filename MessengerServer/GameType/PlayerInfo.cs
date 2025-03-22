using MessengerServer.Manager;
using System;

namespace MessengerServer.GameType
{
    public class PlayerInfo : player_info
    {
        public PlayerInfo() : base(0)
        {
            this.m_friend_manager = new FriendManager();
            clear();
        }
                        
        public new void clear()
        {

            base.clear();

            m_state = 0;   

            m_cpi.clear();

            m_friend_manager.clear();
        }
                    

        public byte m_state;
        public int m_logout = new int(); // Verifica se j� mandou pacote de deslogar

        public ChannelPlayerInfo m_cpi = new ChannelPlayerInfo();

        public FriendManager m_friend_manager = new FriendManager();
    }
}
