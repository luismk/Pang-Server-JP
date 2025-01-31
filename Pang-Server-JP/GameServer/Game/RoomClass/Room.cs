using GameServer.Session;
using GameServer.PangType;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Game.RoomClass
{
    public class Room
    {
		List<Player> v_sessions;
		SortedList<Player, PlayerRoomInfoEx> m_player_info;
		SortedList<uint/*UID*/, bool> m_player_kicked;

		Manager.PersonalShopManager m_personal_shop;

		//List<Team> m_teans;

		Manager.GuildRoomManager m_guild_manager;

		List<InviteChannelInfo> v_invite;

		RoomInfoEx m_ri;

		byte m_channel_owner;  // Id do Canal dono da sala

		bool m_bot_tourney;     // Bot para começa o Modo tourney só com 1 jogador

		bool m_destroying;

		RoomClass.Game m_pGame;

		public	Room(byte _channel_owner, RoomInfoEx _ri)
		{ }

	}
}
