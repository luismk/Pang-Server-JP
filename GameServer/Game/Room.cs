using GameServer.Session;
using GameServer.GameType;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameServer.Game.Manager;
using GameServer.Game.Utils;

namespace GameServer.Game
{
    public class Room
    {
        protected List<Player> v_sessions = new List<Player>();
        protected Dictionary<Player, PlayerRoomInfoEx> m_player_info = new Dictionary<Player, PlayerRoomInfoEx>();
        protected Dictionary<uint, bool> m_Player_kicked = new Dictionary<uint, bool>();


        PersonalShopManager m_personal_shop;

        //List<Team> m_teans;

        Manager.GuildRoomManager m_guild_manager;

        List<InviteChannelInfo> v_invite;

        RoomInfoEx m_ri;

        byte m_channel_owner;  // Id do Canal dono da sala

        bool m_bot_tourney;     // Bot para começa o Modo tourney só com 1 jogador

        bool m_destroying;

        Game m_pGame;                                                                  

        protected int m_lock_spin_state = new int(); // Estado do spin(count) do bloquea da sala
                                                     // Room Tipo Lounge
        protected byte m_weather_lounge;
        public Room(byte _channel_owner, RoomInfoEx _ri)
        {
            this.m_ri = _ri;
            this.m_pGame = null;
            this.m_channel_owner = _channel_owner;
           // this.m_teans = new < type missing > ();
            this.m_weather_lounge = 0;
            this.m_destroying = false;
            this.m_bot_tourney = false;
            this.m_lock_spin_state = 0;
         //   this.m_personal_shop = m_ri;

           // geraSecurityKey();

            // Calcula chuva(weather) se o tipo da sala for lounge
            calcRainLounge();

            // Atualiza tipo da sala
          //  setTipo(m_ri.tipo);

            // Att Exp rate, e Pang rate, que criou a sala, att ele também quando começa o jogo
            //if (sgs::gs != nullptr) {
            m_ri.rate_exp = (uint)Program.gs.getInfo().rate.exp;
            m_ri.rate_pang = (uint)Program.gs.getInfo().rate.pang;
            m_ri.angel_event = Program.gs.getInfo().rate.angel_event ==  1?true:false;
            //} }	  
        }

        protected void calcRainLounge()
        {
           
        }     
    }
}