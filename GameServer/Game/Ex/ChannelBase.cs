using GameServer.Session;
using GameServer.PangType;
using PangyaAPI.Utilities;
using System.Collections.Generic;
namespace GameServer.Game.Ex
{
    public class ChannelBase
    {
       protected enum ESTADO : byte
        {
            UNITIALIZED,
            INITIALIZED
        }

        protected enum LEAVE_ROOM_STATE : int
        {
            DO_NOTHING = -1,        // Faz nada
            SEND_UPDATE_CLIENT = 0, // bug arm g++
            ROOM_DESTROYED,
        }

        protected ChannelInfo m_ci;
        //RoomManager m_rm;

        protected int m_type;           // Type GrandPrix, Natural, Normal

        protected int m_state;

        protected List<Player> v_sessions;
        protected Dictionary<Player, PlayerCanalInfoEx> m_player_info;

        protected List<InviteChannelInfo> v_invite;
        public ChannelBase(ChannelInfo _ci, int _type)
        {
            m_ci = _ci;
            m_state = (int)ESTADO.INITIALIZED;
            v_sessions = new List<Player>();
            m_player_info = new Dictionary<Player, PlayerCanalInfoEx>();
            v_invite = new List<InviteChannelInfo>();
        }
    }
}
