//using GameServer.GameType;
//using PangyaAPI.Utilities;
//using System.Collections.Generic;
//using System.Linq;
//using static GameServer.GameType._Define;
//using System;
//using _smp = PangyaAPI.Utilities.Log;
//using GameServer.Session;
//using PangyaAPI.Utilities.BinaryModels;
//using PangyaAPI.Utilities.Log;
//using GameServer.Game.System;
//using GameServer.Game.Manager;
//using GameServer.PangyaEnums;
//using GameServer.PacketFunc;
//using PangyaAPI.Network.PangyaPacket;
//using PangyaAPI.Network.Pangya_St;
//using PangyaAPI.SQL.Manager;
//using GameServer.Cmd;
//using System.Runtime.InteropServices;
//using System.Reflection;
//using GameServer.Game.Utils;

//namespace GameServer.Game
//{

//    public class room
//    {
//        protected List<Player> v_sessions = new List<Player>();
//        protected Dictionary<Player, PlayerRoomInfoEx> m_Player_info = new Dictionary<Player, PlayerRoomInfoEx>();
//        protected Dictionary<uint, bool> m_Player_kicked = new Dictionary<uint, bool>();

//        protected PersonalShopManager m_personal_shop;

//        protected List<Team> m_teans = new List<Team>();

//        protected GuildRoomManager m_guild_manager = new GuildRoomManager();

//        protected List<InviteChannelInfo> v_invite = new List<InviteChannelInfo>();


//        protected RoomInfoEx m_ri = new RoomInfoEx();

//        protected byte m_channel_owner; // Id do Canal dono da sala

//        protected bool m_bot_tourney; // Bot para começa o Modo tourney só com 1 jogador
//        private int m_lock_spin_state;
//        protected bool m_destroying;

//        protected Game m_pGame;
//        // Room Tipo Lounge
//        protected byte m_weather_lounge;
//        public room(byte _channel_owner, RoomInfoEx _ri)
//        {
//            this.m_ri = _ri;
//            this.m_pGame = null;
//            this.m_channel_owner = _channel_owner;
//            this.m_teans = new List<Team>();
//            this.m_weather_lounge = 0;
//            this.m_destroying = false;
//            this.m_bot_tourney = false;
//            this.m_lock_spin_state = 0;
//            this.m_personal_shop = new PersonalShopManager(m_ri);

//            geraSecurityKey();

//            // Calcula chuva(weather) se o tipo da sala for lounge
//            calcRainLounge();

//            // Atualiza tipo da sala
//            setTipo(m_ri.tipo);

//            // Att Exp rate, e Pang rate, que criou a sala, att ele também quando começa o jogo
//            //if (sgs::gs != nullptr) {
//            m_ri.rate_exp = (uint)Program.gs.getInfo().rate.exp;
//            m_ri.rate_pang = (uint)Program.gs.getInfo().rate.pang;
//            m_ri.angel_event = Program.gs.getInfo().rate.angel_event == 1 ? true : false;
//            //}
//        }

//        public void destroy()
//        {

//            // Leave All Players
//            leaveAll(0);



//            if (m_pGame != null)
//            {
//                m_pGame = null;
//            }

//            m_pGame = null;

//            m_channel_owner = 255;

//            m_weather_lounge = 0;

//            if (v_sessions.Any())
//            {
//                v_sessions.Clear();
//            }

//            if (m_Player_info.Any())
//            {
//                m_Player_info.Clear();
//            }

//            clear_invite();

//            clear_Player_kicked();

//            clear_teans();

//            m_bot_tourney = false;



//            m_personal_shop.destroy();

//            // Destruindo a sala
//            try
//            {

//                @lock();

//                m_destroying = true;

//                unlock();

//            }
//            catch (exception e)
//            {

//                if (!ExceptionError.STDA_ERROR_CHECK_SOURCE_AND_ERROR_TYPE(e.getCodeError(),
//                    STDA_ERROR_TYPE.ROOM, 150))
//                {

//                    unlock();

//                    _smp.message_pool.push(new message("[room::destroy][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
//                }
//            }
//        }

//        public void enter(Player _session)
//        {

//            if (!_session.GetState())
//            {
//                throw new exception("[room::enter][Error] Player nao esta connectado.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                    4, 0));
//            }



//            if (isFull())
//            {



//                throw new exception("[room::enter][Error] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou entrar na a sala[NUMERO=" + Convert.ToString(m_ri.numero) + "], mas a sala ja esta cheia.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                    2, 0));
//            }

//            if (_session.m_pi.mi.sala_numero != ushort.MaxValue)
//            {



//                throw new exception("[room::enter][Error] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] sala[NUMERO=" + Convert.ToString(m_ri.numero) + "], ja esta em outra sala[NUMERO=" + Convert.ToString(_session.m_pi.mi.sala_numero) + "], nao pode entrar em outra. Hacker ou Bug.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                    120, 0));
//            }

//            if (m_ri.tipo == (byte)RoomInfo.TIPO.GUILD_BATTLE
//                && m_ri.guilds.guild_1_uid != 0
//                && m_ri.guilds.guild_2_uid != 0
//                && m_ri.guilds.guild_1_uid != _session.m_pi.gi.uid
//                && m_ri.guilds.guild_2_uid != _session.m_pi.gi.uid)
//            {



//                throw new exception("[room::enter][Error] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] sala[NUMERO=" + Convert.ToString(m_ri.numero) + "], ja tem duas guild e o Player que quer entrar nao eh de nenhum delas. Hacker ou Bug.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                    11000, 0));
//            }

//            try
//            {

//                _session.m_pi.mi.sala_numero = m_ri.numero;

//                // Update Place Player
//                if (m_ri.tipo == (byte)RoomInfo.TIPO.PRACTICE || m_ri.tipo == (byte)RoomInfo.TIPO.GRAND_ZODIAC_PRACTICE)
//                {
//                    _session.m_pi.place = 2;
//                }
//                else
//                {
//                    _session.m_pi.place = 0;
//                }

//                v_sessions.Add(_session);

//                ++m_ri.num_Player;

//                // Update Trofel
//                if (m_ri.trofel > 0)
//                {
//                    updateTrofel();
//                }

//                // Acabou de criar a sala
//                if (m_ri.master == _session.m_pi.uid && m_ri.tipo != (byte)RoomInfo.TIPO.GRAND_PRIX)
//                {

//                    // Update Trofel
//                    if (_session.m_pi.m_cap.game_master)
//                    { // GM

//                        if ((m_ri.max_Player > 30 && m_ri.tipo == (byte)RoomInfo.TIPO.TOURNEY) || (m_ri.tipo >= (byte)RoomInfo.TIPO.GRAND_ZODIAC_INT && m_ri.tipo <= (byte)RoomInfo.TIPO.GRAND_ZODIAC_ADV))
//                        {

//                            m_ri.flag_gm = 1;

//                            m_ri.state_flag = 0x100;

//                            m_ri.trofel = TROFEL_GM_EVENT_TYPEID;

//                        }
//                        else if (m_ri.tipo == (byte)RoomInfo.TIPO.TOURNEY || m_ri.tipo >= (byte)RoomInfo.TIPO.GRAND_ZODIAC_INT)
//                        {
//                            updateTrofel();
//                        }

//                    }
//                    else if (m_ri.tipo == (byte)RoomInfo.TIPO.TOURNEY || m_ri.tipo >= (byte)RoomInfo.TIPO.GRAND_ZODIAC_INT)
//                    {
//                        updateTrofel();
//                    }

//                }
//                else if (m_ri.tipo == (byte)RoomInfo.TIPO.GRAND_PRIX)
//                {
//                    updateTrofel();
//                }

//                // Update Master
//                // Só trocar o master da sala se não tiver nenhum jogo inicializado
//                if (m_pGame == null
//                    && v_sessions.Count > 0
//                    && _session.m_pi.m_cap.game_master
//                    && m_ri.state_flag != 0x100
//                    && m_ri.tipo != (byte)RoomInfo.TIPO.SPECIAL_SHUFFLE_COURSE
//                    && m_ri.tipo != (byte)RoomInfo.TIPO.GRAND_PRIX)
//                {
//                    updateMaster(_session);
//                }

//                // Add o Player ao jogo
//                if (m_pGame != null)
//                {

//                    m_pGame.addPlayer(_session);

//                    if (m_ri.trofel > 0)
//                    {
//                        updateTrofel();
//                    }
//                }

//                try
//                {

//                    // Make Info Room Player
//                    makePlayerInfo(_session);

//                }
//                catch (exception e)
//                {

//                    _smp.message_pool.push(new message("[room::enter][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
//                }

//                if (m_ri.tipo == (byte)RoomInfo.TIPO.GUILD_BATTLE)
//                {
//                    updateGuild(_session);
//                }

//            }
//            catch (exception e)
//            {

//                _smp.message_pool.push(new message("[room::enter][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
//            }


//        }

//        public int leave(Player _session, int _option)
//        {

//            //if (!_session.GetState() /*&& _option != 3/*Force*/)
//            //throw new exception("[room::leave][Error] Player nao esta connectado.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE::ROOM, 4, 0));



//            try
//            {
//                int index = findIndexSession(_session);

//                if (index == (int)~0)
//                {


//                    throw new exception("[room::leave][Error] session[UID=" + Convert.ToString(_session.m_pi.uid) + "] nao existe no vector de sessions da sala[NUMERO=" + Convert.ToString(m_ri.numero) + "].", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                        5, 0));
//                }

//                if (_option != 0
//                    && _option != 1
//                    && _option != 0x800
//                    && _option != 10)
//                {
//                    addPlayerKicked(_session.m_pi.uid);
//                }

//                // Verifica se ele está em um jogo e tira ele
//                try
//                {

//                    if (m_pGame != null)
//                    {
//                        if (m_pGame.deletePlayer(_session, _option) && m_pGame.finish_game(_session, 2))
//                        {
//                            finish_game();
//                        }
//                    }

//                }
//                catch (exception e)
//                {

//                    _smp.message_pool.push(new message("[room::leave][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
//                }

//                v_sessions.RemoveAt(index);

//                if ((m_ri.num_Player - 1) > 0 || v_sessions.Count == 0)
//                {
//                    --m_ri.num_Player;
//                }

//                // Sai do Team se for Match
//                if (m_ri.tipo == (byte)RoomInfo.TIPO.MATCH)
//                {

//                    if (m_teans.Count < 2)
//                    {
//                        throw new exception("[room::leave][Error] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou sair da sala[NUMERO=" + Convert.ToString(m_ri.numero) + "], mas a sala nao tem os 2 teans(times). Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                            1502, 0));
//                    }

//                    var pPri = getPlayerInfo(_session);

//                    if (pPri == null)
//                    {
//                        throw new exception("[room::leave][Error] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou sair da sala[NUMERO=" + Convert.ToString(m_ri.numero) + "], mas a sala nao encontrou o info do Player. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                            1503, 0));
//                    }

//                    m_teans[pPri.state_flag.team].deletePlayer(_session, _option);

//                }
//                else if (m_ri.tipo == (byte)RoomInfo.TIPO.GUILD_BATTLE)
//                {

//                    var pPri = getPlayerInfo(_session);

//                    if (pPri == null)
//                    {
//                        throw new exception("[room::leave][Error] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou sair da sala[NUMERO=" + Convert.ToString(m_ri.numero) + "], mas a sala nao encontrou o info do Player. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                            1503, 0));
//                    }

//                    var guild = m_guild_manager.findGuildByPlayer(_session);

//                    if (guild == null)
//                    {
//                        throw new exception("[room::leave][Error] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou sair da sala[NUMERO=" + Convert.ToString(m_ri.numero) + "], mas o Player nao esta em nenhuma guild da sala. Hacker ou Bug.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                            1504, 0));
//                    }

//                    // Deleta o Player da guild  da sala
//                    guild.deletePlayer(_session);

//                    // Deleta Player do team
//                    m_teans[pPri.state_flag.team].deletePlayer(_session, _option);

//                    // Limpa o team do Player
//                    pPri.state_flag.team = 0;

//                    // Limpa guild
//                    if (guild.numPlayers() == 0)
//                    {

//                        if (guild.getTeam() == Guild.eTEAM.RED)
//                        {

//                            // Red
//                            m_ri.guilds.guild_1_uid = 0;
//                            m_ri.guilds.guild_1_index_mark = 0;

//                        }
//                        else
//                        {

//                            // Blue
//                            m_ri.guilds.guild_2_uid = 0;
//                            m_ri.guilds.guild_2_index_mark = 0;

//                        }

//                        //delete Guild
//                        m_guild_manager.deleteGuild(guild);
//                    }
//                }

//                // Delete Player Info
//                m_Player_info.Remove(_session);

//                // reseta(default) o número da sala no info do Player
//                _session.m_pi.mi.sala_numero = ushort.MaxValue;
//                _session.m_pi.place = 0;

//                // Excluiu personal shop do Player se ele estiver com shop aberto
//                m_personal_shop.destroyShop(_session);

//                updatePosition();

//                updateTrofel();

//                // Isso é para o cliente saber que ele foi kickado pelo server sem ação de outro Player
//                if (_option == 0x800 || (_option != 0 && _option != 1 && _option != 3))
//                {

//                    uint opt_kick = 0x800;

//                    switch (_option)
//                    {
//                        case 1:
//                            opt_kick = 4;
//                            break;
//                        case 2:
//                            opt_kick = 2;
//                            break;
//                        default:
//                            opt_kick = (uint)_option;
//                            break;
//                    }

//                    PangyaBinaryWriter p = new PangyaBinaryWriter((ushort)0x7E);

//                    p.WriteUInt32(opt_kick);

//                    packet_func_sv.session_send(p,
//                        _session, 1);
//                }

//                if (m_ri.tipo == (byte)RoomInfo.TIPO.LOUNGE)
//                { // Zera State lounge of Player

//                    _session.m_pi.state = 0;
//                    _session.m_pi.state_lounge = 0;
//                }

//                // Update Players State On Room
//                if (v_sessions.Count > 0)
//                {
//                    sendUpdate();

//                    sendCharacter(_session, 2);
//                }
//                // Fim Update Players State

//                if ((m_pGame == null && m_ri.tipo == (byte)RoomInfo.TIPO.SPECIAL_SHUFFLE_COURSE && _session.m_pi.uid == m_ri.master) || (_session.m_pi.m_cap.game_master && m_ri.master == _session.m_pi.uid && m_ri.tipo != (byte)RoomInfo.TIPO.LOUNGE && m_ri.trofel == TROFEL_GM_EVENT_TYPEID))
//                {



//                    return 0x801; // deleta todos da sala

//                }
//                else if (m_pGame == null)
//                {
//                    updateMaster(null); // update Master
//                }

//            }
//            catch (exception e)
//            {

//                _smp.message_pool.push(new message("[room::leave][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
//            }



//            return (v_sessions.Count > 0 || (m_ri.master == -2 && (!isDropRoom() || m_ri.tipo >= (byte)RoomInfo.TIPO.GRAND_ZODIAC_INT && m_ri.tipo <= (byte)RoomInfo.TIPO.GRAND_ZODIAC_ADV))) ? 0 : 1;
//        }

//        protected void calcRainLounge()
//        {

//            // Só calcRainLounge se for lounge
//            if (m_ri.tipo == (byte)RoomInfo.TIPO.LOUNGE)
//            {

//                m_weather_lounge = 0; // Good Weather

//                short rate_rain = Program.gs.getInfo().rate.chuva;

//                Lottery loterry = new Lottery(0);

//                uint rate_good_weather = (uint)((rate_rain <= 0) ? 1000 : ((rate_rain < 1000) ? 1000 - rate_rain : 1));

//                loterry.Push(rate_good_weather, 0);
//                loterry.Push(rate_good_weather, 0);
//                loterry.Push(rate_good_weather, 0);
//                loterry.Push((uint)rate_rain, 2);

//                var lc = loterry.SpinRoleta();

//                if (lc != null)
//                {
//                    m_weather_lounge = (byte)lc.Value;
//                }
//            }
//        }


//        protected void clear_teans()
//        {
//            if (m_teans.Any())
//            {
//                m_teans.Clear();
//            }
//        }

//        // Add Bot Tourney Visual to Room
//        protected void addBotVisual(Player _session)
//        {
//            if (!_session.GetState())
//            {
//                throw new exception("[room::" + "addBotVisual" + "][Error] Player nao esta connectado", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                    12, 0));
//            }

//            // Add Bot
//            List<PlayerRoomInfoEx> v_element = new List<PlayerRoomInfoEx>();
//            PlayerRoomInfoEx pri = new PlayerRoomInfoEx();
//            PlayerRoomInfoEx tmp_pri = null;

//            try
//            {


//                v_sessions.ForEach(_el =>
//                {
//                    tmp_pri = getPlayerInfo(_el);
//                    if (tmp_pri != null)
//                    {
//                        v_element.Add(tmp_pri);
//                    }
//                });


//                if (v_element.Count == 0)
//                {
//                    throw new exception("[room::makeBot][Error] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou criar Bot na sala[NUMERO=" + Convert.ToString(m_ri.numero) + ", MASTER=" + Convert.ToString(m_ri.master) + "], mas nao nenhum Player na sala. Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                        1, 5000));
//                }

//                // Inicializa os dados do Bot
//                pri.uid = _session.m_pi.uid;
//                pri.oid = _session.m_oid;
//                pri.position = 0; // 0 Que é para ele ficar em primeiro e parece que tbm não deixa kick(ACHO)
//                pri.state_flag.ready = 1;
//                pri.char_typeid = 0x4000000; // Nuri
//                pri.title = 0x39800013; // Title Helper
//                pri.nickname = "\\1Bot";
//                // Add o Bot a sala, só no visual
//                v_element.Add(pri);

//                // Packet
//                PangyaBinaryWriter p = new PangyaBinaryWriter();

//                // Option 0, passa todos que estão na sala
//                packet_func_sv.room_broadcast(this, packet_func_sv.pacote048(_session, v_element, 0x100), 1);

//                // Option 1, passa só o Player que entrou na sala, nesse caso foi o Bot
//                packet_func_sv.room_broadcast(this, packet_func_sv.pacote048(_session, new List<PlayerRoomInfoEx> { pri }, 0x101), 1);


//                // Criou Bot com sucesso
//                m_bot_tourney = true;

//                // Log
//                _smp.message_pool.push(new message("[room::addBotVisual][Log] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] Room[NUMBER=" + Convert.ToString(m_ri.numero) + ", MASTER=" + Convert.ToString(m_ri.master) + "] Bot criado com sucesso.", type_msg.CL_FILE_LOG_AND_CONSOLE));



//            }
//            catch (exception e)
//            {

//                // Relança
//                throw;
//            }
//        }

//        // Para as classes filhas, empedir que exclua a sala depLastLasto do se tem Player ou não na sala
//        protected virtual bool isDropRoom()
//        {
//            return true; // class room normal é sempre true
//        }

//        // protected porque é um método inseguro (sem thread safety)
//        protected uint _getRealNumPlayersWithoutInvited()
//        {
//            return (uint)v_sessions.Count(_el =>
//            {
//                if (_el == null)
//                    return false;

//                return m_Player_info.TryGetValue(_el, out var playerInfo) && !(playerInfo.convidado == 1);
//            });
//        }


//        // protected por que é o método unsave(inseguro), sem thread safe
//        protected bool _haveInvited()
//        {
//            return v_sessions.Any(_el =>
//            {
//                if (_el == null)
//                    return false;

//                return m_Player_info.TryGetValue(_el, out var playerInfo) && playerInfo.convidado == 1;
//            });
//        }


//        // Game
//        protected virtual void finish_game()
//        {

//            if (m_pGame != null)
//            {
//                PangyaBinaryWriter p = new PangyaBinaryWriter();

//                // Deleta o jogo
//                m_pGame = null;

//                // Zera Player Flags
//                foreach (var el in m_Player_info)
//                {

//                    // Update Place Player
//                    if (m_ri.tipo == (byte)RoomInfo.TIPO.PRACTICE || m_ri.tipo == (byte)RoomInfo.TIPO.GRAND_ZODIAC_PRACTICE)
//                    {
//                        el.Value.place.ulPlace = 2;
//                    }
//                    else
//                    {
//                        el.Value.place.ulPlace = 0;
//                    }

//                    el.Value.state_flag.away = 0;

//                    // Aqui só zera quem não é Master da sala, o master deixa sempre ready
//                    if (m_ri.master == el.Key.m_pi.uid)
//                    {
//                        el.Value.state_flag.ready = 1;
//                    }
//                    else
//                    {
//                        el.Value.state_flag.ready = 0;
//                    }

//                    // Update Player info
//                    updatePlayerInfo(el.Key);

//                    // SLast update on room
//                    sendCharacter(el.Key, 3);
//                }

//                // Atualiza flag da sala, só não atualiza se for GM evento ou GZ Event e SSC
//                if (!(m_ri.trofel == TROFEL_GM_EVENT_TYPEID || (m_ri.tipo == (byte)RoomInfo.TIPO.SPECIAL_SHUFFLE_COURSE || m_ri.master == -2))
//                        {
//                    m_ri.state = 1; //em espera
//                }

//                // Att Exp rate, e Pang rate, que criou a sala, att ele também quando começa o jogo
//                //if (sgs::gs != nullptr) {
//                m_ri.rate_exp = (uint)Program.gs.getInfo().rate.exp;
//                m_ri.rate_pang = (uint)Program.gs.getInfo().rate.pang;
//                m_ri.angel_event = Program.gs.getInfo().rate.angel_event.IsTrue();
//                //}

//                // Update Course of Hole
//                if (m_ri.course >= RoomInfo.eCOURSE.UNK) // Random Course With Course already draw
//                {
//                    m_ri.course = RoomInfo.eCOURSE.UNK; // Random Course standard
//                }

//                // Update Master da sala
//                updateMaster(null);

//                if (m_ri.master == -2)
//                {
//                    m_ri.master = -1; // pode deletar a sala quando sair todos
//                }

//                if (v_sessions.Count > 0)
//                {
//                    // Atualiza info da sala para quem está na sala
//                    packet_func_sv.pacote04A(p,
//                        m_ri, -1);
//                    packet_func_sv.room_broadcast(this,
//                        p, 1);
//                }

//                // limpa lista de Player kikados
//                clear_Player_kicked();

//                // Verifica se o Bot Tourney está ativo, kika bot e limpa a flag
//                if (m_bot_tourney)
//                {

//                    var pMaster = findMaster();

//                    if (pMaster != null)
//                    {

//                        try
//                        {
//                            // Kick Bot
//                            // Atualiza os Player que estão na sala que o Bot sai por que ele é só visual
//                            sendCharacter(pMaster, 0);

//                        }
//                        catch (exception e)
//                        {

//                            _smp.message_pool.push(new message("[room::finish_game::KickBotTourney][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
//                        }
//                    }

//                    m_bot_tourney = false;
//                }

//                // Terminou o jogo
//                m_pGame = null;
//            }
//        }

//        // Invite
//        protected void clear_invite()
//        {

//            if (v_invite.Any())
//            {
//                v_invite.Clear();
//            }
//        }

//        // Team
//        protected void init_teans()
//        {

//            // Limpa teans, se tiver teans inicilizados já
//            clear_teans();

//            // Init Teans
//            m_teans.Add(new Team(0));
//            m_teans.Add(new Team(1));

//            PlayerRoomInfo pPri = null;

//            // Add Players All Seus Respectivos teans
//            foreach (var el in v_sessions)
//            {

//                if ((pPri = getPlayerInfo(el)) == null)
//                {
//                    throw new exception("[room::init_teans][Error] nao encontrou o info do Player[UID=" + Convert.ToString(el.m_pi.uid) + "] na sala. Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                        1504, 0));
//                }

//                m_teans[pPri.state_flag.team].addPlayer(el);
//            }

//        }


//        public int leaveAll(int _option)
//        {

//            while (!v_sessions.empty())
//            {

//                try
//                {
//                    leave(*(*v_sessions.begin()), _option);
//                }
//                catch (exception e)
//                {

//                    _smp.message_pool.push(new message("[room::leaveAll][Error] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
//                }
//            }

//            return 0;
//        }

//        public bool isInvited(Player _session)
//        {

//            var it = m_Player_info.find(_session);

//            return (it != null && it.Value.convidado);
//        }

//        public InviteChannelInfo addInvited(uint _uid_has_invite, Player _session)
//        {

//            if (!_session.GetState())
//            {
//                throw new exception("[room::addInvited][Error] Player nao esta connectado.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                    4, 0));
//            }

//            if (isFull())
//            {
//                throw new exception("[room::addInvited][Error] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou entrar na a sala[NUMERO=" + Convert.ToString(m_ri.numero) + "], mas a sala ja esta cheia.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                    2, 0));
//            }

//            if (findIndexSession(_uid_has_invite) == (int)~0)
//            {
//                throw new exception("[room::addInvited][Error] quem convidou[UID=" + Convert.ToString(_uid_has_invite) + "] o Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] para a sala nao esta na sala. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                    2010, 0));
//            }

//            var s = findSessionByUID(_session.m_pi.uid);

//            if (s != null)
//            {
//                throw new exception("[room::addInvited][Error] Player[UID=" + Convert.ToString(_uid_has_invite) + "] tentou adicionar o convidado[UID=" + Convert.ToString(_session.m_pi.uid) + "] a sala, mas ele ja esta na sala. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                    2001, 0));
//            }



//            _session.m_pi.mi.sala_numero = m_ri.numero;

//            _session.m_pi.place = 70; // Está sendo convidado

//            v_sessions.Add(_session);

//            ++m_ri.num_Player;

//            PlayerRoomInfoEx pri = null;

//            try
//            {

//                // Make Info Room Player Invited
//                pri = makePlayerInvitedInfo(_session);

//            }
//            catch (exception e)
//            {

//                _smp.message_pool.push(new message("[room::addInvited][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
//            }

//            if (pri == null)
//            {

//                // Pop_back
//                v_sessions.Remove(v_sessions.Last());



//                throw new exception("[[room::addInvited][Error] Player[UID=" + Convert.ToString(_uid_has_invite) + "] tentou adicionar o convidado[UID=" + Convert.ToString(_session.m_pi.uid) + "] a sala, nao conseguiu criar o Player Room Info Invited do Player. Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                    2002, 0));
//            }

//            // Add Invite Channel Info
//            InviteChannelInfo ici = new InviteChannelInfo();

//            ici.room_number = m_ri.numero;

//            ici.invite_uid = _uid_has_invite;
//            ici.invited_uid = _session.m_pi.uid;

//            ici.time.CreateTime();

//            v_invite.Add(ici);
//            // End Add Invite Channel Info

//            // Update Char Invited ON ROOM
//            PangyaBinaryWriter p = new PangyaBinaryWriter((ushort)0x48);

//            p.WriteByte(1);
//            p.WriteInt16(-1);

//            p.WriteBytes(pri.Build());

//            p.WriteByte(0); // Final Packet

//            packet_func_sv.room_broadcast(this,
//                p, 1);



//            return ici;
//        }

//        public InviteChannelInfo deleteInvited(Player _session)
//        {

//            // Por que se o Player não estiver mais online não pode deletar o convidado
//            //if (!_session.GetState())
//            //throw new exception("[room::deleteInvited][Error] Player nao esta connectado.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE::ROOM, 4, 0));

//            var it = m_Player_info.First(c => c.Key == _session);

//            if (it == m_Player_info.Last())
//            {
//                throw new exception("[room::deleteInvited][Error] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou deletar convidado," + " mas nao tem o info do convidado na sala. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                    2003, 0));
//            }



//            int index = findIndexSession(_session);

//            if (index == (int)~0)
//            {


//                throw new exception("[room::deleteInvited][Error] session[UID=" + Convert.ToString(_session.m_pi.uid) + "] nao existe no vector de sessions da sala.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                    5, 0));
//            }

//            _session.m_pi.mi.sala_numero = ushort.MaxValue;

//            _session.m_pi.place = 0; // Limpa Está sendo convidado

//            v_sessions.RemoveAt(index);

//            --m_ri.num_Player;

//            m_Player_info.Remove(it);

//            // Update Position all Players
//            updatePosition();

//            // Delete Invite Channel Info
//            InviteChannelInfo ici = new InviteChannelInfo();

//            // C++ TO C# CONVERTER TASK: Lambda expressions cannot be assigned to 'var':
//            var itt = v_invite.FirstOrDefault(_el =>
//            {
//                return (_el.room_number == m_ri.numero && _el.invited_uid == _session.m_pi.uid);
//            });

//            if (itt != v_invite.Last())
//            {

//                ici = itt;

//                v_invite.Remove(itt);

//            }
//            else
//            {
//                _smp.message_pool.push(new message("[room::deleteInvited][WARNING] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] nao tem um convite.", type_msg.CL_FILE_LOG_AND_CONSOLE));
//            }

//            // End Delete Invite Channel Info

//            // Resposta Delete Convidado
//            PangyaBinaryWriter p = new PangyaBinaryWriter((ushort)0x130);

//            p.WriteUInt32(_session.m_pi.uid);

//            packet_func_sv.room_broadcast(this,
//                p, 1);

//            _smp.message_pool.push(new message("[room::deleteInvited][Log] Deleteou um convite[Convidado=" + Convert.ToString(_session.m_pi.uid) + "] na Sala[NUMERO=" + Convert.ToString(m_ri.numero) + "]", type_msg.CL_FILE_LOG_AND_CONSOLE));


//            return ici;
//        }

//        public InviteChannelInfo deleteInvited(uint _uid)
//        {

//            if (_uid == 0u)
//            {
//                throw new exception("[room::deleteInvited][Error] _uid is invalid(zero). Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                    2005, 0));
//            }

//            // C++ TO C# CONVERTER TASK: Lambda expressions cannot be assigned to 'var':
//            var it = std::find_if(m_Player_info.begin(), m_Player_info.Last(), (_el) =>
//            {
//                return (_el.Value.convidado && _el.Value.uid == _uid);
//            });

//            if (it == m_Player_info.Last())
//            {
//                throw new exception("[room::deleteInvited][Error] Player[UID=" + Convert.ToString(_uid) + "] tentou deletar convidado," + " mas nao tem o info do convidado na sala. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                    2003, 0));
//            }



//            int index = findIndexSession(_uid);

//            if (index == (int)~0)
//            {


//                throw new exception("[room::deleteInvited][Error] session[UID=" + Convert.ToString(_uid) + "] nao existe no vector de sessions da sala.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                    5, 0));
//            }

//            v_sessions.RemoveAt(index);

//            --m_ri.num_Player;

//            m_Player_info.Remove(it);

//            // Update Position all Players
//            updatePosition();

//            // Delete Invite Channel Info
//            InviteChannelInfo ici = new InviteChannelInfo();

//            // C++ TO C# CONVERTER TASK: Lambda expressions cannot be assigned to 'var':
//            var itt = v_invite.FirstOrDefault(_el => _el.room_number == m_ri.numero && _el.invited_uid == _uid);


//            if (itt != null)
//            {

//                ici = itt;

//                v_invite.Remove(itt);

//            }
//            else
//            {
//                _smp.message_pool.push(new message("[room::deleteInvited][WARNING] Player[UID=" + Convert.ToString(_uid) + "] nao tem um convite.", type_msg.CL_FILE_LOG_AND_CONSOLE));
//            }

//            // End Delete Invite Channel Info

//            // Resposta Delete Convidado
//            PangyaBinaryWriter p = new PangyaBinaryWriter((ushort)0x130);

//            p.WriteUInt32(_uid);

//            packet_func_sv.room_broadcast(this,
//                p, 1);

//            _smp.message_pool.push(new message("[room::deleteInvited][Log] Deleteou um convite[Convidado=" + Convert.ToString(_uid) + "] na Sala[NUMERO=" + Convert.ToString(m_ri.numero) + "]", type_msg.CL_FILE_LOG_AND_CONSOLE));



//            return ici;
//        }

//        public RoomInfoEx getInfo()
//        {
//            return m_ri;
//        }

//        // Gets
//        public byte getChannelOwenerId()
//        {
//            return m_channel_owner;
//        }

//        public short getNumero()
//        {
//            return m_ri.numero;
//        }

//        public uint getMaster()
//        {
//            return m_ri.master;
//        }

//        public uint getNumPlayers()
//        {
//            return m_ri.num_Player;
//        }

//        public uint getPosition(Player _session)
//        {
//            var position = ~0;



//            for (var i = 0; i < v_sessions.Count; ++i)
//            {
//                if (v_sessions[i] == _session)
//                {
//                    position = i;
//                    break;
//                }
//            }



//            return position;
//        }

//        public PlayerRoomInfoEx getPlayerInfo(Player _session)
//        {

//            if (_session == null)
//            {
//                throw new exception("Error _session is nullptr. Em room::getPlayerInfo()", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                    7, 0));
//            }

//            PlayerRoomInfoEx pri = null;

//            // C++ TO C# CONVERTER TASK: Iterators are only converted within the context of 'while' and 'for' loops:
//            if ((pri = m_Player_info.Values.First(c => c.uid == _session.getUID())) != null)
//            {
//                // C++ TO C# CONVERTER TASK: Iterators are only converted within the context of 'while' and 'for' loops:
//                pri = i.Current.Value;
//            }



//            return pri;
//        }

//        public List<Player> getSessions(Player _session = null, bool _with_invited = true)
//        {
//            List<Player> v_session = new List<Player>();



//            foreach (var el in v_sessions)
//            {
//                if (el != null
//                    && el.GetState()
//                    && el.m_pi.mi.sala_numero != ushort.MaxValue
//                    && (_session == null || _session != el)
//                    && (_with_invited || !isInvited(*el)))
//                {
//                    v_session.Add(el);
//                }
//            }



//            return new List<Player>(v_session);
//        }

//        public uint getRealNumPlayersWithoutInvited()
//        {

//            uint num = 0;



//            try
//            {

//                num = _getRealNumPlayersWithoutInvited();

//            }
//            catch (exception e)
//            {

//                _smp.message_pool.push(new message("[room::getRealNumPlayerWithoutInvited][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
//            }



//            return (num);
//        }

//        public bool haveInvited()
//        {

//            bool question = false;



//            try
//            {

//                question = _haveInvited();

//            }
//            catch (exception e)
//            {

//                _smp.message_pool.push(new message("[room::haveInvited][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
//            }



//            return question;
//        }

//        // Sets
//        public void setNome(string _nome)
//        {

//            if (_nome.Length == 0)
//            {
//                throw new exception("Error _nome esta vazio. Em room::setNome()", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                    6, 0));
//            }
//            m_ri.nome = _nome;
//        }

//        public void setSenha(string _senha)
//        {

//            if (_senha.Length == 0)
//            {
//                if (!m_ri.senha_flag)
//                {
//                    // C++ TO C# CONVERTER TASK: The memory management function 'memset' has no equivalent in C#:
//                    m_ri.senha = "";

//                    m_ri.senha_flag = 1;
//                }
//            }
//            else
//            {
//                m_ri.senha = _senha;

//                m_ri.senha_flag = 0;
//            }
//        }

//        public void setTipo(byte _tipo)
//        {

//            if (_tipo == (byte)(byte)RoomInfo.TIPO.MATCH || _tipo == (byte)(byte)RoomInfo.TIPO.GUILD_BATTLE)
//            {
//                init_teans();
//            }
//            else if (_tipo != (byte)(byte)RoomInfo.TIPO.MATCH && m_ri.tipo == (byte)RoomInfo.TIPO.MATCH)
//            {
//                clear_teans();
//            }

//            m_ri.tipo = _tipo;

//            // Atualizar tipo da sala
//            if (m_ri.tipo > (byte)RoomInfo.TIPO.GRAND_ZODIAC_PRACTICE)
//            {
//                m_ri.tipo_show = 4;
//            }
//            else if (m_ri.tipo == (byte)RoomInfo.TIPO.GRAND_ZODIAC_ADV || m_ri.tipo == (byte)RoomInfo.TIPO.GRAND_ZODIAC_PRACTICE)
//            {
//                m_ri.tipo_show = (byte)(byte)RoomInfo.TIPO.GRAND_ZODIAC_INT;
//            }
//            else
//            {
//                m_ri.tipo_show = m_ri.tipo;
//            }

//            if (m_ri.tipo >= (byte)RoomInfo.TIPO.GRAND_ZODIAC_INT)
//            {
//                m_ri.tipo_ex = m_ri.tipo;
//            }
//            else
//            {
//                m_ri.tipo_ex = 255;
//            }

//            // Atualiza Trofel se for Tourney
//            if (m_ri.tipo == (byte)RoomInfo.TIPO.TOURNEY || (m_ri.master != -2 && m_ri.tipo >= (byte)RoomInfo.TIPO.GRAND_ZODIAC_INT && m_ri.tipo <= (byte)RoomInfo.TIPO.GRAND_ZODIAC_ADV))
//            {

//                if ((m_ri.max_Player > 30 && m_ri.tipo == (byte)RoomInfo.TIPO.TOURNEY) || (m_ri.tipo >= (byte)RoomInfo.TIPO.GRAND_ZODIAC_INT && m_ri.tipo <= (byte)RoomInfo.TIPO.GRAND_ZODIAC_ADV))
//                {

//                    m_ri.flag_gm = 1;

//                    m_ri.state_flag = 0x100;

//                    m_ri.trofel = TROFEL_GM_EVENT_TYPEID;

//                }
//                else if (m_ri.tipo == (byte)RoomInfo.TIPO.TOURNEY || m_ri.tipo >= (byte)RoomInfo.TIPO.GRAND_ZODIAC_INT)
//                {
//                    updateTrofel();
//                }

//            }
//            else
//            {
//                m_ri.trofel = 0;
//            }
//        }

//        public void setCourse(byte _course)
//        {
//            m_ri.course = (RoomInfo.eCOURSE)_course;
//        }

//        public void setQntdHole(byte _qntd_hole)
//        {
//            m_ri.qntd_hole = _qntd_hole;
//        }

//        public void setModo(byte _modo)
//        {
//            m_ri.modo = _modo;
//        }

//        public void setTempoVS(uint _tempo)
//        {
//            m_ri.time_vs = _tempo;
//        }

//        public void setMaxPlayer(byte _max_Player)
//        {

//            if (v_sessions.Count > _max_Player)
//            {
//                throw new exception("[room::setMaxPlayer][Error] MASTER[UID=" + Convert.ToString(m_ri.master) + "] _max_Player[VALUE=" + Convert.ToString(_max_Player) + "] eh menor que o numero de jogadores[VALUE=" + Convert.ToString(v_sessions.Count) + "] na sala.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                    250, 0x588000));
//            }

//            // New Max Player room
//            m_ri.max_Player = _max_Player;

//            // Atualiza Trofeu se for Tourney
//            if (m_ri.tipo == (byte)RoomInfo.TIPO.TOURNEY || (m_ri.tipo >= (byte)RoomInfo.TIPO.GRAND_ZODIAC_INT && m_ri.tipo <= (byte)RoomInfo.TIPO.GRAND_ZODIAC_ADV))
//            {

//                if ((m_ri.max_Player > 30 && m_ri.tipo == (byte)RoomInfo.TIPO.TOURNEY) || (m_ri.tipo >= (byte)RoomInfo.TIPO.GRAND_ZODIAC_INT && m_ri.tipo <= (byte)RoomInfo.TIPO.GRAND_ZODIAC_ADV))
//                {

//                    m_ri.flag_gm = 1;

//                    m_ri.trofel = TROFEL_GM_EVENT_TYPEID;

//                }
//                else if (m_ri.tipo == (byte)RoomInfo.TIPO.TOURNEY || m_ri.tipo >= (byte)RoomInfo.TIPO.GRAND_ZODIAC_INT)
//                {
//                    updateTrofel();
//                }

//            }
//        }

//        public void setTempo30S(uint _tempo)
//        {
//            m_ri.time_30s = _tempo;
//        }

//        public void setHoleRepeat(byte _hole_repeat)
//        {
//            m_ri.hole_repeat = _hole_repeat;
//        }

//        public void setFixedHole(uint _fixed_hole)
//        {
//            m_ri.fixed_hole = _fixed_hole;
//        }

//        public void setArtefato(uint _artefato)
//        {
//            m_ri.artefato = _artefato;
//        }

//        public void setNatural(uint _natural)
//        {
//            m_ri.natural.ulNaturalAndShortGame = _natural;
//        }

//        public void setState(byte _state)
//        {
//            m_ri.state = _state;
//        }

//        public void setFlag(byte _flag)
//        {
//            m_ri.flag = _flag;
//        }

//        public void setStateAFK(byte _state_afk)
//        {
//            m_ri.state_afk = _state_afk;
//        }

//        // Checks
//        public bool checkPass(string _pass)
//        {

//            if (!isLocked())
//            {
//                throw new exception("[Room::checkPass][Error] sala nao tem senha", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                    1, 0));
//            }

//            return string.Compare(m_ri.senha, _pass) == 0;
//        }

//        // Verifica se o Player tem um loja aberta no lounge e se o item está à vLasta nela
//        public bool checkPersonalShopItem(Player _session, int32_t _item_id)
//        {
//            return m_personal_shop.isItemForSale(_session, _item_id);
//        }

//        // States
//        public bool isLocked()
//        {
//            return !m_ri.senha_flag;
//        }

//        public bool isFull()
//        {
//            return m_ri.num_Player >= m_ri.max_Player;
//        }

//        public bool isGaming()
//        {
//            return m_pGame != null;
//        }

//        public bool isGamingBefore(uint _uid)
//        {

//            if (_uid == 0u)
//            {
//                throw new exception("[room::isGamingBefore][Error] _uid is invalid(zero)", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                    1000, 0));
//            }

//            if (m_pGame == null)
//            {
//                throw new exception("[room::isGamingBefore][Error] a sala[NUMERO=" + Convert.ToString(m_ri.numero) + "] nao tem um jogo inicializado. Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                    1001, 0));
//            }

//            return m_pGame.isGamingBefore(_uid);
//        }

//        public bool isKickedPlayer(uint _uid)
//        {
//            return m_Player_kicked.First(el => el.Key == _uid).Key != m_Player_kicked.Last().Key;
//        }

//        public virtual bool isAllReady()
//        {

//            var master = findMaster();

//            if (master == null)
//            {
//                return false;
//            }

//            // Bot Tourney, Short Game and Special Shuffle Course
//            if (m_bot_tourney
//                && v_sessions.Count == 1
//                && m_ri.tipo == (byte)RoomInfo.TIPO.TOURNEY || m_ri.tipo == (byte)RoomInfo.TIPO.SPECIAL_SHUFFLE_COURSE)
//            {
//                return true;
//            }

//            // se a sala for Practice, CHIP-IN Practice, e GRAND_PRIX_NOVICE não precisa o Player está pronto
//            if (m_ri.tipo == (byte)RoomInfo.TIPO.PRACTICE
//                || m_ri.tipo == (byte)RoomInfo.TIPO.GRAND_ZODIAC_PRACTICE
//                || m_ri.tipo == (byte)RoomInfo.TIPO.GRAND_PRIX)
//            {
//                return true;
//            }

//            // Se o master for GM então não precisar todos está ready(prontos)
//            if (master.m_pi.m_cap.game_master && !_haveInvited())
//            {
//                return true;
//            }

//            // C++ TO C# CONVERTER TASK: Lambda expressions cannot be assigned to 'var':
//            var count = v_sessions.Count(_el =>
//            {
//                var pri = getPlayerInfo(_el);
//                return (pri != null && pri.state_flag.ready == 1);
//            });

//            // Conta com o master por que o master sempre está pronto(ready)
//            return (count == v_sessions.Count);
//        }

//        // Updates
//        public void updatePlayerInfo(Player _session)
//        {
//            PlayerRoomInfoEx pri = new PlayerRoomInfoEx();
//            PlayerRoomInfoEx _pri = null;

//            if ((_pri = getPlayerInfo(_session)) == null)
//            {
//                throw new exception("[room::updatePlayerInfo][Error] nao tem o Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] info dessa session na sala.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                    8, 0));
//            }

//            // Copia do que esta no map
//            pri = _pri;

//            // Player Room Info Update
//            pri.oid = _session.m_oid;

//            // C++ TO C# CONVERTER TASK: The memory management function 'memcpy_s' has no equivalent in C#:
//            pri.position = (byte)((byte)getPosition(_session) + 1); // posição na sala
//            pri.capability = _session.m_pi.m_cap;
//            pri.title = _session.m_pi.ue.skin_typeid[5];

//            if (_session.m_pi.ei.char_info != null)
//            {
//                pri.char_typeid = _session.m_pi.ei.char_info._typeid;
//            }


//            pri.skin[4] = 0; // Aqui tem que ser zero, se for outro valor não mostra a imagem do character equipado

//            if (getMaster() == _session.m_pi.uid)
//            {
//                pri.state_flag.master = 1;
//                pri.state_flag.ready = 1; // Sempre está pronto(ready) o master
//            }
//            else
//            {

//                // Só troca o estado de pronto dele na sala, se anterior mente ele era Master da sala ou não estiver pronto
//                if (pri.state_flag.master == 1 || !(pri.state_flag.ready == 1))
//                {
//                    pri.state_flag.ready = 0;
//                }

//                pri.state_flag.master = 0;
//            }

//            pri.state_flag.sexo = _session.m_pi.mi.sexo;

//            // Update Team se for Match
//            if (m_ri.tipo == (byte)RoomInfo.TIPO.MATCH)
//            {

//                // Verifica se o Player está em algum team para atualizar o team dele se ele não estiver em nenhum
//                var Player_team = pri.state_flag.team;
//                Player p_seg_team = null;

//                // atualizar o team do Player a flag de team dele não bate com o team dele
//                if (m_teans[Player_team].findPlayerByUID(pri.uid) == null && (p_seg_team = m_teans[~Player_team].findPlayerByUID(pri.uid)) == null)
//                {

//                    // Player não está em nenhum team
//                    if (v_sessions.Count > 1)
//                    {

//                        if (m_teans[0].getCount() >= 2 && m_teans[1].getCount() >= 2)
//                        {
//                            throw new exception("[room::updatePlayerInfo][Error] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou entrar em time para todos os times da sala estao cheios. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                                1500, 0));
//                        }
//                        else if (m_teans[0].getCount() >= 2)
//                        {
//                            pri.state_flag.team = 1; // Blue
//                        }
//                        else if (m_teans[1].getCount() >= 2)
//                        {
//                            pri.state_flag.team = 0; // Red
//                        }
//                        else
//                        {

//                            var pPri = getPlayerInfo((v_sessions.Count == 2) ? v_sessions.FirstOrDefault() : (v_sessions.Count > 2 ? (v_sessions.rbegin() += 1) : null));

//                            if (pPri == null)
//                            {
//                                throw new exception("[room::updatePlayerInfo][Error] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou entrar em um time, mas o ultimo Player da sala, nao tem um info no sala. Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                                    1501, 0));
//                            }

//                            pri.state_flag.team = (byte)~pPri.state_flag.team;
//                        }

//                    }
//                    else
//                    {
//                        pri.state_flag.team = 0;
//                    }

//                    m_teans[pri.state_flag.team].addPlayer(_session);

//                }
//                else if (p_seg_team != null)
//                {

//                    // a flag de team do Player está errada, ele está no outro team, ajeita
//                    pri.state_flag.team = ~Player_team;
//                }

//            }
//            else if (m_ri.tipo != (byte)RoomInfo.TIPO.GUILD_BATTLE) // O Guild Battle tem sua própria função para inicializar e atualizar o team e os dados da guild
//            {
//                pri.state_flag.team = (pri.position - 1) % 2;
//            }

//            // Só faz calculo de Quita rate depois que o Player
//            // estiver no level Beginner E e jogado 50 games
//            if (_session.m_pi.level >= 6 && _session.m_pi.ui.jogado >= 50)
//            {
//                float rate = _session.m_pi.ui.getQuitRate();

//                if (rate < GOOD_PLAYER_ICON)
//                {
//                    pri.state_flag.azinha = 1;
//                }
//                else if (rate >= QUITER_ICON_1 && rate < QUITER_ICON_2)
//                {
//                    pri.state_flag.quiter_1 = 1;
//                }
//                else if (rate >= QUITER_ICON_2)
//                {
//                    pri.state_flag.quiter_2 = 1;
//                }
//            }

//            pri.level = (byte)_session.m_pi.mi.level;

//            if (_session.m_pi.ei.char_info != null && _session.m_pi.ui.getQuitRate() < GOOD_PLAYER_ICON)
//            {
//                pri.icon_angel = _session.m_pi.ei.char_info.AngelEquiped();
//            }
//            else
//            {
//                pri.icon_angel = 0;
//            }

//            pri.place.ulPlace = 10; // 0x0A dec"10" _session.m_pi.place
//            pri.guild_uid = _session.m_pi.gi.uid;

//            pri.uid = _session.m_pi.uid;
//            pri.state_lounge = _session.m_pi.state_lounge;
//            pri.usUnknown_flg = 0; // Ví Players com valores 2 e 4 e 0
//            pri.state = _session.m_pi.state;
//            pri.location = new PlayerRoomInfo.stLocation() { x = _session.m_pi.location.x, z = _session.m_pi.location.z, r = _session.m_pi.location.r };

//            // Personal Shop
//            pri.shop = m_personal_shop.getPersonShop(_session);

//            if (_session.m_pi.ei.mascot_info != null)
//            {
//                pri.mascot_typeid = _session.m_pi.ei.mascot_info._typeid;
//            }

//            pri.flag_item_boost = _session.m_pi.checkEquipedItemBoost();
//            pri.ulUnknown_flg = 0;
//            //pri.id_NT não estou usando ainda
//            //pri.ucUnknown106

//            // Só atualiza a flag de convidado se for diferente de 1, por que 1 ele é convidado
//            if (pri.convidado != 1)
//            {
//                pri.convidado = 0; // Flag Convidado, [Não sei bem por que os que entra na sala normal tem valor igual aqui, já que é flag de convidado waiting], Valor constante da sala para os Players(ACHO)
//            }

//            pri.avg_score = _session.m_pi.ui.getMediaScore();
//            //pri.ucUnknown3

//            if (_session.m_pi.ei.char_info != null)
//            {
//                pri.ci = _session.m_pi.ei.char_info;
//            }

//            // Salva novamente
//            // C++ TO C# CONVERTER TASK: The following line was determined to be a copy assignment (rather than a reference assignment) - this should be verified and a '' method should be created:
//            _pri = pri;
//        }

//        // Finds
//        public Player findSessionByOID(uint _oid)
//        {
//            var i = v_sessions.FirstOrDefault(_el =>
//                _el.m_oid == _oid);



//            if (i != v_sessions.Last())
//            {
//                return i;
//            }

//            return null;
//        }

//        public Player findSessionByUID(uint _uid)
//        {

//            var i = v_sessions.FirstOrDefault(_el =>
//                _el.m_pi.uid == _uid);



//            if (i != v_sessions.Last())
//            {
//                return i;
//            }
//            return null;
//        }

//        public Player findMaster()
//        {

//            Player master = null;

//            var pMaster = v_sessions.FirstOrDefault(_el =>
//            {
//                return _el.m_pi.uid == m_ri.master;
//            });

//            if (pMaster != v_sessions.Last())
//            {
//                master = pMaster;
//            }
//            return master;
//        }

//        // Bot Tourney, Short Game and Special Shuffle Course
//        public void makeBot(Player _session)
//        {
//            if (!_session.GetState())
//            {
//                throw new exception("[room::" + "makeBot" + "][Error] Player nao esta connectado", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                    12, 0));
//            }

//            PangyaBinaryWriter p = new PangyaBinaryWriter();

//            try
//            {

//                // Bot Ticket TypeId

//                // Premium User Não precisa de ticket não
//                if (_session.m_pi.m_cap.premium_user)
//                {

//                    // Add Bot Tourney Visual para a sala
//                    addBotVisual(_session);

//                    // SLast Message
//                    p.init_plain((ushort)0x40); // Msg to Chat of Player

//                    p.WriteByte(7); // Notice

//                    p.WritePStr("@SuperSS");
//                    p.WritePStr("[ \\2Premium ] \\c0xff00ff00\\cBot was created.");

//                    packet_func_sv.session_send(p,
//                        _session, 1);

//                }
//                else
//                {

//                    // Verifica se ele tem o ticket para criar o Bot se não manda mensagem dizenho que ele não tem ticket para criar o bot
//                    var pWi = _session.m_pi.findWarehouseItemByTypeid(TICKET_BOT_TYPEID);

//                    if (pWi != null && pWi.c[0] > 1)
//                    {

//                        stItem item = new stItem();

//                        item.type = 2;
//                        item.id = (int)pWi.id;
//                        item._typeid = pWi._typeid;
//                        item.qntd = 1;
//                        item.c[0] = (ushort)(item.qntd * -1);

//                        if (item_manager.removeItem(item, _session) > 0)
//                        {

//                            // Atualiza o item no Jogo e Add o Bot e manda a mensagem que o bot foi add
//                            p.init_plain((ushort)0x216);

//                            p.WriteUInt32((uint)UtilTime.GetSystemTimeAsUnix());
//                            p.WriteUInt32(1); // Count;

//                            p.WriteByte(item.type);
//                            p.WriteUInt32(item._typeid);
//                            p.WriteInt32(item.id);
//                            p.WriteUInt32(item.flag_time);
//                            p.WriteStruct(item.stat, Marshal.SizeOf(new stItem.item_stat()));
//                            p.WriteUInt32((item.c[3] > 0) ? item.c[3] : item.c[0]);
//                            p.WriteZero(25);

//                            packet_func_sv.session_send(p,
//                                _session, 1);

//                            // Add Bot
//                            addBotVisual(_session);

//                            // SLast Message
//                            p.init_plain((ushort)0x40); // Msg to Chat of Player

//                            p.WriteByte(7); // Notice

//                            p.WritePStr("@SuperSS");
//                            p.WritePStr("\\c0xff00ff00\\cBot was created 1 ticket has been consumed.");

//                            packet_func_sv.session_send(p,
//                                _session, 1);

//                        }
//                        else
//                        {

//                            _smp.message_pool.push(new message("[room::makeBot][Error] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] nao conseguiu deletar o TICKET_BOT[TYPEID=" + Convert.ToString(TICKET_BOT_TYPEID) + ", ID=" + Convert.ToString(item.id) + "]", type_msg.CL_FILE_LOG_AND_CONSOLE));

//                            // SLast Message
//                            p.init_plain((ushort)0x40); // Msg to Chat of Player

//                            p.WriteByte(7); // Notice

//                            p.WritePStr("@SuperSS");
//                            p.WritePStr("\\c0xffff0000\\cError creating Bot.");

//                            packet_func_sv.session_send(p,
//                                _session, 1);
//                        }

//                    }
//                    else
//                    {

//                        // Não tem ticket bot suficiente, manda mensagem
//                        // SLast Message
//                        p.init_plain((ushort)0x40); // Msg to Chat of Player

//                        p.WriteByte(7); // Notice

//                        p.WritePStr("@SuperSS");
//                        p.WritePStr("\\c0xffff0000\\cYou do not have enough ticket to create the Bot.");

//                        packet_func_sv.session_send(p,
//                            _session, 1);
//                    }
//                }

//            }
//            catch (exception e)
//            {

//                _smp.message_pool.push(new message("[room::makeBot][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

//                // SLast Message
//                p.init_plain((ushort)0x40); // Msg to Chat of Player

//                p.WriteByte(7); // Notice

//                p.WritePStr("@SuperSS");
//                p.WritePStr("\\c0xffff0000\\cError creating Bot.");

//                packet_func_sv.session_send(p,
//                    _session, 1);
//            }
//        }

//        // Info Room
//        public bool requestChangeInfoRoom(Player _session, Packet _packet)
//        {
//            if (!_session.GetState())
//            {
//                throw new exception("[room::" + (("request" + "ChangeInfoRoom")) + "][Error] Player nao esta connectado", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                    12, 0));
//            }
//            if (_packet == null)
//            {
//                throw new exception("[room::request" + "ChangeInfoRoom" + "][Error] _packet is nullptr", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                    12, 0));
//            }

//            bool ret = false;

//            try
//            {

//                byte num_info;
//                short flag;

//                if (m_ri.master != _session.m_pi.uid)
//                {
//                    throw new exception("[room::requestChangeInfoRoom][Error] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou trocar o info da sala[NUMERO=" + Convert.ToString(m_ri.numero) + ", MASTER=" + Convert.ToString(m_ri.master) + "], mas nao pode trocar o info da sala sem ser master.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                        11, 0));
//                }

//                flag = _packet.ReadInt16();

//                num_info = _packet.ReadByte();

//                if (num_info <= 0)
//                {
//                    throw new exception("[room::requestChangeInfoRoom][Error] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou trocar o info da sala[NUMERO=" + Convert.ToString(m_ri.numero) + ", MASTER=" + Convert.ToString(m_ri.master) + "], mas nao tem nenhum info para trocar do buffer do cliente.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                        8, 0));
//                }

//                for (var i = 0; i < num_info; ++i)
//                {

//                    switch ((RoomInfo.INFO_CHANGE)_packet.ReadByte())
//                    {
//                        case RoomInfo.INFO_CHANGE.NAME:
//                            setNome(_packet.ReadPStr());
//                            break;
//                        case RoomInfo.INFO_CHANGE.SENHA:
//                            setSenha(_packet.ReadPStr());
//                            break;
//                        case RoomInfo.INFO_CHANGE.TIPO:
//                            setTipo(_packet.ReadByte());
//                            break;
//                        case RoomInfo.INFO_CHANGE.COURSE:
//                            setCourse(_packet.ReadByte());
//                            break;
//                        case RoomInfo.INFO_CHANGE.QNTD_HOLE:
//                            setQntdHole(_packet.ReadByte());
//                            break;
//                        case RoomInfo.INFO_CHANGE.MODO:
//                            setModo(_packet.ReadByte());
//                            break;
//                        case RoomInfo.INFO_CHANGE.TEMPO_VS: // Passa em Segundos
//                            setTempoVS((uint)_packet.ReadUInt16() * 1000);
//                            break;
//                        case RoomInfo.INFO_CHANGE.MAX_PLAYER:
//                            setMaxPlayer(_packet.ReadByte());
//                            break;
//                        case RoomInfo.INFO_CHANGE.TEMPO_30S: // Passa em Minutos
//                            setTempo30S((uint)_packet.ReadByte() * 60000);
//                            break;
//                        case RoomInfo.INFO_CHANGE.STATE_FLAG:
//                            // Esse não usa mais
//                            // Aqui posso usar para começar o jogo, se a sala estiver(AFK) => "isso acontece quando o master está AFK"
//                            // Então vou salver esse valor aqui
//                            setStateAFK(_packet.ReadByte());
//                            break;
//                        case RoomInfo.INFO_CHANGE.HOLE_REPEAT:
//                            setHoleRepeat(_packet.ReadByte());
//                            break;
//                        case RoomInfo.INFO_CHANGE.FIXED_HOLE:
//                            setFixedHole(_packet.ReadUInt32());
//                            break;
//                        case RoomInfo.INFO_CHANGE.ARTEFATO:
//                            setArtefato(_packet.ReadUInt32());
//                            break;
//                        case RoomInfo.INFO_CHANGE.NATURAL:
//                            {
//                                var natural = new NaturalAndShortGame((uint)_packet.ReadUInt32());

//                                if (Program.gs.getInfo().propriedade.natural) // Natural não deixa desabilitar o Natural da sala, por que o server é natural
//                                {
//                                    natural.natural = 1;
//                                }

//                                setNatural(natural.ulNaturalAndShortGame);

//                                break;
//                            }
//                        default:
//                            throw new exception("[room::requestChangeInfoRoom][Error] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou trocar info da sala[NUMERO=" + Convert.ToString(m_ri.numero) + ", MASTER=" + Convert.ToString(m_ri.master) + "], mas info change eh desconhecido.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                                9, 0));
//                    }
//                }

//                // send to clients update room info
//                sendUpdate();

//                ret = true; // Trocou o info da sala com sucesso

//            }
//            catch (exception e)
//            {

//                _smp.message_pool.push(new message("[room::requestChangeInfoRoom][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

//                // Resposta para o cliente
//                PangyaBinaryWriter p = new PangyaBinaryWriter();

//                packet_func_sv.pacote04A(p,
//                    m_ri, 25);
//                packet_func_sv.session_send(p,
//                    _session, 1);
//            }

//            return ret;
//        }

//        // Chat Team
//        public void requestChatTeam(Player _session, Packet _packet)
//        {
//            if (!_session.GetState())
//            {
//                throw new exception("[room::" + (("request" + "ChatTeam")) + "][Error] Player nao esta connectado", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                    12, 0));
//            }
//            if (_packet == null)
//            {
//                throw new exception("[room::request" + "ChatTeam" + "][Error] _packet is nullptr", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                    12, 0));
//            }

//            PangyaBinaryWriter p = new PangyaBinaryWriter();

//            try
//            {

//                var msg = _packet.ReadPStr();

//                // Verifica a mensagem com palavras proibida e manda para o log e bloquea o chat dele
//                _smp.message_pool.push(new message("[room::requestChatTeam][Log] Player[UID=" + Convert.ToString(_session.m_pi.uid) + ", MESSAGE=" + msg + "]", CL_ONLY_FILE_LOG));

//                if (msg.empty())
//                {
//                    throw new exception("[room::requestChatTeam][Error] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou mandar messsage[MSG=" + msg + "] no chat do team na sala[NUMERO=" + Convert.ToString(m_ri.numero) + "], mas a msg esta vazia. Hacker ou Bug.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                        2000, 0));
//                }

//                if (m_ri.tipo != (byte)RoomInfo.TIPO.MATCH && m_ri.tipo != (byte)RoomInfo.TIPO.GUILD_BATTLE)
//                {
//                    throw new exception("[room::requestChatTeam][Error] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou mandar messsage[MSG=" + msg + "] no chat do team na sala[NUMERO=" + Convert.ToString(m_ri.numero) + "], mas a sala nao eh MATCH ou GUILD_BATTLE. Hacker ou Bug.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                        2001, 0));
//                }

//                if (m_teans.empty())
//                {
//                    throw new exception("[room::requestChatTeam][Error] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou mandar messsage[MSG=" + msg + "] no chat do team na sala[NUMERO=" + Convert.ToString(m_ri.numero) + "], mas a sala nao tem nenhum team. Hacker ou Bug.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                        2002, 0));
//                }

//                var pri = getPlayerInfo(_session);

//                if (pri == null)
//                {
//                    throw new exception("[room::requetChatTeam][Error] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou mandar messsage[MSG=" + msg + "] no chat do team na sala[NUMERO=" + Convert.ToString(m_ri.numero) + "], mas a sala nao tem o info dele. Hacker ou Bug.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                        2003, 0));
//                }

//                var team = m_teans[pri.state_flag.team];

//                if (team.findPlayerByUID(_session.m_pi.uid) == null)
//                {
//                    throw new exception("[room::requestChatTeam][Error] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou mandar messsage[MSG=" + msg + "] no chat do team na sala[NUMERO=" + Convert.ToString(m_ri.numero) + "], mas ele nao esta no team que a flag de team dele diz. Hacker ou Bug.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                        2004, 0));
//                }

//                // LOG GM
//                // Envia para todo os GM do server essa message
//                var c = Program.gs.findChannel(_session.m_pi.channel);

//                if (c != null)
//                {

//                    var gm = Program.gs.findAllGM();

//                    if (!gm.empty())
//                    {

//                        string msg_gm = "\\5" + (_session.m_pi.nickname) + ": '" + msg + "'";
//                        string from = "\\1[Channel=" + (c.getInfo().name) + ", \\1ROOM=" + Convert.ToString(_session.m_pi.mi.sala_numero) + "][Team" + (!pri.state_flag.team ? "R" : "B") + "]";

//                        var index = from.IndexOf(' ');

//                        if (index != -1)
//                        {
//                            from = from.Remove(index, 1).Insert(index, " \\1");
//                        }

//                        foreach (var el in gm)
//                        {

//                            if (((el.m_gi.channel != 0 && el.m_pi.channel == c.getInfo().id) || el.m_gi.whisper || el.m_gi.isOpenPlayerWhisper(_session.m_pi.uid)) && (el.m_pi.channel != _session.m_pi.channel || el.m_pi.mi.sala_numero != _session.m_pi.mi.sala_numero || team.findPlayerByUID(el.m_pi.uid) == null))
//                            {

//                                // Responde no chat do Player
//                                p.init_plain((ushort)0x40);

//                                p.WriteByte(0);

//                                p.WritePStr(from); // Nickname

//                                p.WritePStr(msg_gm); // Message

//                                packet_func_sv.session_send(p,
//                                    el, 1);
//                            }
//                        }
//                    }

//                }
//                else
//                {
//                    _smp.message_pool.push(new message("[room::requestChatTeam][WARNING] Log GM nao encontrou o Channel[ID=" + Convert.ToString((ushort)_session.m_pi.channel) + "] no server. Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));
//                }

//                // Manda message para o team da sala
//                p.init_plain((ushort)0xB0);

//                p.WritePStr(_session.m_pi.nickname);
//                p.WritePStr(msg);

//                foreach (var el in team.getPlayers())
//                {
//                    el.SLast(p);
//                }

//            }
//            catch (exception e)
//            {

//                _smp.message_pool.push(new message("[room::requestChatTeam][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
//            }
//        }

//        // Change Item Equiped of Player
//        public virtual void requestChangePlayerItemRoom(Player _session, ChangePlayerItemRoom _cpir)
//        {
//            if (!_session.GetState())
//            {
//                throw new exception("[room::" + "ChangePlayerItemRoom" + "][Error] Player nao esta connectado", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                    12, 0));
//            }

//            PangyaBinaryWriter p = new PangyaBinaryWriter();

//            try
//            {

//                int error = 0;

//                switch (_cpir.type)
//                {
//                    case ChangePlayerItemRoom.TYPE_CHANGE.TC_CADDIE: // Caddie
//                        {
//                            CaddieInfoEx pCi = null;

//                            // Caddie
//                            if (_cpir.caddie != 0
//                                && (pCi = _session.m_pi.findCaddieById(_cpir.caddie)) != null
//                                && sIff.getInstance().getItemGroupIdentify(pCi._typeid) == sIff.getInstance().CADDIE)
//                            {

//                                // Check if item is in map of update item
//                                var v_it = _session.m_pi.findUpdateItemByTypeidAndId(pCi._typeid, pCi.id);

//                                if (v_it.Any())
//                                {

//                                    foreach (var el in v_it)
//                                    {

//                                        if (el.Value.type == UpdateItem.UI_TYPE.CADDIE)
//                                        {

//                                            // Desequipa o caddie
//                                            _session.m_pi.ei.cad_info = null;
//                                            _session.m_pi.ue.caddie_id = 0;

//                                            _cpir.caddie = 0;

//                                        }
//                                        else if (el.Value.type == UpdateItem.UI_TYPE.CADDIE_PARTS)
//                                        {

//                                            // Limpa o caddie Parts
//                                            pCi.parts_typeid = 0;
//                                            pCi.parts_end_date_unix = 0;
//                                            pCi.end_parts_date = new PangyaTime();

//                                            _session.m_pi.ei.cad_info = pCi;
//                                            _session.m_pi.ue.caddie_id = _cpir.caddie;
//                                        }

//                                        // Tira esse Update Item do map
//                                        _session.m_pi.mp_ui.Remove(el);
//                                    }

//                                }
//                                else
//                                {

//                                    // Caddie is Good, Update caddie equiped ON SERVER AND DB
//                                    _session.m_pi.ei.cad_info = pCi;
//                                    _session.m_pi.ue.caddie_id = _cpir.caddie;

//                                    // Verifica se o Caddie pode ser equipado
//                                    if (_session.checkCaddieEquiped(_session.m_pi.ue))
//                                    {
//                                        _cpir.caddie = _session.m_pi.ue.caddie_id;
//                                    }

//                                }

//                                // Update ON DB
//                                NormalManagerDB.add(0,
//                                    new CmdUpdateCaddieEquiped(_session.m_pi.uid, (int)_cpir.caddie),
//                                    SQLDBResponse, this);

//                            }
//                            else if (_session.m_pi.ue.caddie_id > 0 && _session.m_pi.ei.cad_info != null)
//                            { // Desequipa Caddie

//                                error = (_cpir.caddie == 0) ? 1 : (pCi == null ? 2 : 3);

//                                if (error > 1)
//                                {
//                                    _smp.message_pool.push(new message("[room::requestChangePlayerItemRoom][Log][WARNING] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou trocar o Caddie[ID=" + Convert.ToString(_cpir.caddie) + "] para comecar o jogo, mas deu Error[VALUE=" + Convert.ToString(error) + "], desequipando o caddie. Hacker ou Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));
//                                }

//                                // Check if item is in map of update item
//                                var v_it = _session.m_pi.findUpdateItemByTypeidAndId(_session.m_pi.ei.cad_info._typeid, _session.m_pi.ei.cad_info.id);

//                                if (v_it.Any())
//                                {

//                                    foreach (var el in v_it)
//                                    {

//                                        // Caddie já vai se desequipar, só verifica o parts
//                                        if (el.Value.type == UpdateItem.UI_TYPE.CADDIE_PARTS)
//                                        {

//                                            // Limpa o caddie Parts
//                                            _session.m_pi.ei.cad_info.parts_typeid = 0;
//                                            _session.m_pi.ei.cad_info.parts_end_date_unix = 0;
//                                            _session.m_pi.ei.cad_info.end_parts_date = new PangyaTime();
//                                        }

//                                        // Tira esse Update Item do map
//                                        _session.m_pi.mp_ui.Remove(el.Key);
//                                    }

//                                }

//                                _session.m_pi.ei.cad_info = null;
//                                _session.m_pi.ue.caddie_id = 0;

//                                _cpir.caddie = 0;

//                                // Zera o Error para o cliente desequipar o caddie que o server desequipou
//                                error = 0;

//                                // Att No DB
//                                NormalManagerDB.add(0,
//                                    new CmdUpdateCaddieEquiped(_session.m_pi.uid, _cpir.caddie),
//                                    SQLDBResponse, this);
//                            }
//                            packet_func_sv.room_broadcast(this, packet_func_sv.pacote04B(_session, (byte)_cpir.type, error), 1);
//                            break;
//                        }
//                    case ChangePlayerItemRoom.TYPE_CHANGE.TC_BALL: // Ball
//                        {
//                            WarehouseItemEx pWi = null;

//                            if (_cpir.ball != 0
//                                && (pWi = _session.m_pi.findWarehouseItemByTypeid(_cpir.ball)) != null
//                                && sIff.getInstance().getItemGroupIdentify(pWi._typeid) == sIff.getInstance().BALL)
//                            {

//                                _session.m_pi.ei.comet = pWi;
//                                _session.m_pi.ue.ball_typeid = _cpir.ball; // Ball(Comet) é o typeid que o cliente passa

//                                // Verifica se a bola pode ser equipada
//                                if (_session.checkBallEquiped(_session.m_pi.ue))
//                                {
//                                    _cpir.ball = _session.m_pi.ue.ball_typeid;
//                                }

//                                // Update ON DB
//                                NormalManagerDB.add(0,
//                                    new CmdUpdateBallEquiped(_session.m_pi.uid, _cpir.ball),
//                                    SQLDBResponse, this);

//                            }
//                            else if (_cpir.ball == 0)
//                            { // Bola 0 coloca a bola padrão para ele, se for premium user coloca a bola de premium user

//                                // Zera para equipar a bola padrão
//                                _session.m_pi.ei.comet = null;
//                                _session.m_pi.ue.ball_typeid = 0;

//                                // Verifica se a Bola pode ser equipada (Coloca para equipar a bola padrão
//                                if (_session.checkBallEquiped(_session.m_pi.ue))
//                                {
//                                    _cpir.ball = _session.m_pi.ue.ball_typeid;
//                                }

//                                // Update ON DB
//                                NormalManagerDB.add(0,
//                                    new CmdUpdateBallEquiped(_session.m_pi.uid, _cpir.ball),
//                                    SQLDBResponse, this);

//                            }
//                            else
//                            {

//                                error = (pWi == null ? 2 : 3);

//                                pWi = _session.m_pi.findWarehouseItemByTypeid(DEFAULT_COMET_TYPEID);

//                                if (pWi != null)
//                                {

//                                    _smp.message_pool.push(new message("[room::requestChangePlayerItemRoom][Log][WARNING] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou trocar a Ball[TYPEID=" + Convert.ToString(_cpir.ball) + "] para comecar o jogo, mas deu Error[VALUE=" + Convert.ToString(error) + "], colocando a Ball Padrao do Player. Hacker ou Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));

//                                    _session.m_pi.ei.comet = pWi;
//                                    _cpir.ball = _session.m_pi.ue.ball_typeid = pWi._typeid;

//                                    // Zera o Error para o cliente equipar a Ball Padrão que o server equipou
//                                    error = 0;

//                                    // Update ON DB
//                                    NormalManagerDB.add(0,
//                                        new CmdUpdateBallEquiped(_session.m_pi.uid, _cpir.ball),
//                                        SQLDBResponse, this);

//                                }
//                                else
//                                {

//                                    _smp.message_pool.push(new message("[room::requestChangePlayerItemRoom][Log][WARNING] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou trocar a Ball[TYPEID=" + Convert.ToString(_cpir.ball) + "] para comecar o jogo, mas deu Error[VALUE=" + Convert.ToString(error) + "], ele nao tem a Ball Padrao, adiciona a Ball pardrao para ele. Hacker ou Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));

//                                    BuyItem bi = new BuyItem();
//                                    stItem item = new stItem();

//                                    bi.id = -1;
//                                    bi._typeid = DEFAULT_COMET_TYPEID;
//                                    bi.qntd = 1;

//                                    item_manager.initItemFromBuyItem(_session.m_pi,
//                                      refref item, bi, false, 0, 0, 1);

//                                    if (item._typeid != 0)
//                                    {

//                                        if ((_cpir.ball = item_manager.addItem(item,
//                                            _session, 2, 0)) != item_manager.RetAddItem.TYPE.T_ERROR)
//                                        {

//                                            // Equipa a Ball padrao
//                                            pWi = _session.m_pi.findWarehouseItemById(_cpir.ball);

//                                            if (pWi != null)
//                                            {

//                                                _session.m_pi.ei.comet = pWi;
//                                                _session.m_pi.ue.ball_typeid = pWi._typeid;

//                                                // Zera o Error para o cliente equipar a Ball Padrão que o server equipou
//                                                error = 0;

//                                                // Update ON DB
//                                                NormalManagerDB.add(0,
//                                                    new CmdUpdateBallEquiped(_session.m_pi.uid, _cpir.ball),
//                                                    SQLDBResponse, this);

//                                                // Update ON GAME
//                                                p.init_plain((ushort)0x216);

//                                                p.WriteUInt32((uint)UtilTime.GetSystemTimeAsUnix());
//                                                p.WriteUInt32(1); // Count

//                                                p.WriteByte(item.type);
//                                                p.WriteUInt32(item._typeid);
//                                                p.WriteInt32(item.id);
//                                                p.WriteUInt32(item.flag_time);
//                                                p.WriteStruct(item.stat, Marshal.SizeOf(new stItem.item_stat()));
//                                                p.WriteUInt32((item.c[3] > 0) ? item.c[3] : item.c[0]);
//                                                p.WriteZero(25);

//                                                packet_func_sv.session_send(p,
//                                                    _session, 1);

//                                            }
//                                            else
//                                            {
//                                                _smp.message_pool.push(new message("[room::requestChangePlayerItemRoom][Log][WARNING] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] nao conseguiu achar a Ball[ID=" + Convert.ToString(item.id) + "] que acabou de adicionar para ele. Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));
//                                            }

//                                        }
//                                        else
//                                        {
//                                            _smp.message_pool.push(new message("[room::requestChangePlayerItemRoom][Log][WARNING] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] nao conseguiu adicionar a Ball[TYPEID=" + Convert.ToString(item._typeid) + "] para ele. Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));
//                                        }

//                                    }
//                                    else
//                                    {
//                                        _smp.message_pool.push(new message("[room::requestChangePlayerItemRoom][Log][WARNING] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] nao conseguiu inicializar a Ball[TYPEID=" + Convert.ToString(bi._typeid) + "] para ele. Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));
//                                    }
//                                }
//                            }

//                            packet_func_sv.room_broadcast(this, packet_func_sv.pacote04B(_session, (byte)_cpir.type, error), 1);
//                            break;
//                        }
//                    case ChangePlayerItemRoom.TYPE_CHANGE.TC_CLUBSET: // ClubSet
//                        {
//                            WarehouseItemEx pWi = null;

//                            // ClubSet
//                            if (_cpir.clubset != 0
//                                && (pWi = _session.m_pi.findWarehouseItemById(_cpir.clubset)) != null
//                                && sIff.getInstance().getItemGroupIdentify(pWi._typeid) == sIff.getInstance().CLUBSET)
//                            {

//                                var c_it = _session.m_pi.findUpdateItemByTypeidAndType(_cpir.clubset, UpdateItem.UI_TYPE.WAREHOUSE);

//                                if (c_it != null)
//                                {

//                                    _session.m_pi.ei.clubset = pWi;

//                                    // Esse C do WarehouseItem, que pega do DB, não é o ja updado inicial da taqueira é o que fica tabela enchant,
//                                    // que no original fica no warehouse msm, eu só confundi quando fiz
//                                    _session.m_pi.ei.csi.setValues(pWi.id, pWi._typeid, pWi.c);

//                                    var cs = sIff.getInstance().findClubSet(pWi._typeid);

//                                    if (cs != null)
//                                    {

//                                        for (var j = 0; j < 5; ++j)
//                                        {
//                                            _session.m_pi.ei.csi.enchant_c[j] = (short)(cs.Stats.getSlot()[j] + pWi.clubset_workshop.c[j]);
//                                        }

//                                        _session.m_pi.ue.clubset_id = _cpir.clubset;

//                                        // Verifica se o ClubSet pode ser equipado
//                                        // if (_session.checkClubSetEquiped(_session.m_pi.ue))
//                                        //{
//                                        _cpir.clubset = _session.m_pi.ue.clubset_id;
//                                        //}

//                                        //// Update ON DB
//                                        //NormalManagerDB.add(0,
//                                        //    new CmdUpdateClubsetEquiped(_session.m_pi.uid, _cpir.clubset),
//                                        //    SQLDBResponse, this);

//                                    }
//                                    else
//                                    {

//                                        error = 5;

//                                        _smp.message_pool.push(new message("[room::requestChangePlayerItemRoom][Error] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou Atualizar Clubset[TYPEID=" + Convert.ToString(pWi._typeid) + ", ID=" + Convert.ToString(pWi.id) + "] equipado, mas ClubSet Not exists on IFF structure. Equipa o ClubSet padrao. Hacker ou Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));

//                                        // Coloca o ClubSet CV1 no lugar do ClubSet que acabou o tempo
//                                        pWi = _session.m_pi.findWarehouseItemByTypeid(AIR_KNIGHT_SET);

//                                        if (pWi != null)
//                                        {

//                                            _smp.message_pool.push(new message("[room::requestChangePlayerItemRoom][Log][WARNING] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou trocar o ClubSet[ID=" + Convert.ToString(_cpir.clubset) + "] para comecar o jogo, mas acabou o tempo do ClubSet[ID=" + Convert.ToString(_cpir.clubset) + "], colocando o ClubSet Padrao\"CV1\" do Player. Hacker ou Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));

//                                            // Esse C do WarehouseItem, que pega do DB, não é o ja updado inicial da taqueira é o que fica tabela enchant,
//                                            // que no original fica no warehouse msm, eu só confundi quando fiz
//                                            _session.m_pi.ei.csi.setValues(pWi.id, pWi._typeid, pWi.c);

//                                            cs = sIff.getInstance().findClubSet(pWi._typeid);

//                                            if (cs != null)
//                                            {

//                                                for (var j = 0; j < 5; ++j)
//                                                {
//                                                    _session.m_pi.ei.csi.enchant_c[j] = (short)(cs.Stats.getSlot()[j] + pWi.clubset_workshop.c[j]);
//                                                }

//                                            }

//                                            _session.m_pi.ei.clubset = pWi;
//                                            _cpir.clubset = _session.m_pi.ue.clubset_id = pWi.id;

//                                            // Zera o Error para o cliente equipar a "CV1" que o server equipou
//                                            error = 0;

//                                            //// Update ON DB
//                                            //NormalManagerDB.add(0,
//                                            //    new CmdUpdateClubsetEquiped(_session.m_pi.uid, _cpir.clubset),
//                                            //    SQLDBResponse, this);

//                                        }
//                                        else
//                                        {

//                                            _smp.message_pool.push(new message("[room::requestChangePlayerItemRoom][Log][WARNING] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou trocar o ClubSet[ID=" + Convert.ToString(_cpir.clubset) + "] para comecar o jogo, mas acabou o tempo do ClubSet[ID=" + Convert.ToString(_cpir.clubset) + "], ele nao tem o ClubSet Padrao\"CV1\", adiciona o ClubSet pardrao\"CV1\" para ele. Hacker ou Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));

//                                            BuyItem bi = new BuyItem();
//                                            stItem item = new stItem();

//                                            bi.id = -1;
//                                            bi._typeid = AIR_KNIGHT_SET;
//                                            bi.qntd = 1;

//                                            item_manager.initItemFromBuyItem(_session.m_pi,
//                                                refref item, bi, false, 0, 0, 1);

//                                            if (item._typeid != 0)
//                                            {

//                                                if ((_cpir.clubset = item_manager.addItem(item,
//                                                    _session, 2, 0)) != item_manager.RetAddItem.TYPE.T_ERROR)
//                                                {

//                                                    // Equipa o ClubSet CV1
//                                                    pWi = _session.m_pi.findWarehouseItemById(_cpir.clubset);

//                                                    if (pWi != null)
//                                                    {

//                                                        // Esse C do WarehouseItem, que pega do DB, não é o ja updado inicial da taqueira é o que fica tabela enchant,
//                                                        // que no original fica no warehouse msm, eu só confundi quando fiz
//                                                        _session.m_pi.ei.csi.setValues(pWi.id, pWi._typeid, pWi.c);

//                                                        cs = sIff.getInstance().findClubSet(pWi._typeid);

//                                                        if (cs != null)
//                                                        {

//                                                            for (var j = 0; j < 5; ++j)
//                                                            {
//                                                                _session.m_pi.ei.csi.enchant_c[j] = (short)(cs.Stats.getSlot()[j] + pWi.clubset_workshop.c[j]);
//                                                            }

//                                                        }
//                                                        _session.m_pi.ei.clubset = pWi;
//                                                        _session.m_pi.ue.clubset_id = pWi.id;

//                                                        // Zera o Error para o cliente equipar a "CV1" que o server equipou
//                                                        error = 0;

//                                                        // Update ON DB
//                                                        NormalManagerDB.add(0,
//                                                            new CmdUpdateClubsetEquiped(_session.m_pi.uid, _cpir.clubset),
//                                                            SQLDBResponse, this);

//                                                        // Update ON GAME
//                                                        p.init_plain((ushort)0x216);

//                                                        p.WriteUInt32((uint)UtilTime.GetSystemTimeAsUnix());
//                                                        p.WriteUInt32(1); // Count

//                                                        p.WriteByte(item.type);
//                                                        p.WriteUInt32(item._typeid);
//                                                        p.WriteInt32(item.id);
//                                                        p.WriteUInt32(item.flag_time);
//                                                        p.WriteStruct(item.stat, Marshal.SizeOf(new stItem.item_stat()));
//                                                        p.WriteUInt32((item.c[3] > 0) ? item.c[3] : item.c[0]);
//                                                        p.WriteZero(25);

//                                                        packet_func_sv.session_send(p,
//                                                            _session, 1);

//                                                    }
//                                                    else
//                                                    {
//                                                        _smp.message_pool.push(new message("[room::requestChangePlayerItemRoom][Log][WARNING] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] nao conseguiu achar o ClubSet\"CV1\"[ID=" + Convert.ToString(item.id) + "] que acabou de adicionar para ele. Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));
//                                                    }

//                                                }
//                                                else
//                                                {
//                                                    _smp.message_pool.push(new message("[room::requestChangePlayerItemRoom][Log][WARNING] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] nao conseguiu adicionar o ClubSet[TYPEID=" + Convert.ToString(item._typeid) + "] para ele. Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));
//                                                }

//                                            }
//                                            else
//                                            {
//                                                _smp.message_pool.push(new message("[room::requestChangePlayerItemRoom][Log][WARNING] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] nao conseguiu inicializar o ClubSet[TYPEID=" + Convert.ToString(bi._typeid) + "] para ele. Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));
//                                            }
//                                        }
//                                    }

//                                }
//                                else
//                                { // ClubSet Acabou o tempo

//                                    error = 6; // Acabou o tempo do item

//                                    // Coloca o ClubSet CV1 no lugar do ClubSet que acabou o tempo
//                                    pWi = _session.m_pi.findWarehouseItemByTypeid(AIR_KNIGHT_SET);

//                                    if (pWi != null)
//                                    {

//                                        _smp.message_pool.push(new message("[room::requestChangePlayerItemRoom][Log][WARNING] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou trocar o ClubSet[ID=" + Convert.ToString(_cpir.clubset) + "] para comecar o jogo, mas acabou o tempo do ClubSet[ID=" + Convert.ToString(_cpir.clubset) + "], colocando o ClubSet Padrao\"CV1\" do Player. Hacker ou Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));

//                                        // Esse C do WarehouseItem, que pega do DB, não é o ja updado inicial da taqueira é o que fica tabela enchant,
//                                        // que no original fica no warehouse msm, eu só confundi quando fiz
//                                        _session.m_pi.ei.csi.setValues(pWi.id, pWi._typeid, pWi.c);

//                                        cs = sIff.getInstance().findClubSet(pWi._typeid);

//                                        if (cs != null)
//                                        {

//                                            for (var j = 0; j < 5; ++j)
//                                            {
//                                                _session.m_pi.ei.csi.enchant_c[j] = (short)(cs.Stats.getSlot()[j] + pWi.clubset_workshop.c[j]);
//                                            }

//                                        }
//                                        _session.m_pi.ei.clubset = pWi;
//                                        _cpir.clubset = _session.m_pi.ue.clubset_id = pWi.id;

//                                        // Zera o Error para o cliente equipar a "CV1" que o server equipou
//                                        error = 0;

//                                        // Update ON DB
//                                        NormalManagerDB.add(0,
//                                            new CmdUpdateClubsetEquiped(_session.m_pi.uid, _cpir.clubset),
//                                            SQLDBResponse, this);

//                                    }
//                                    else
//                                    {

//                                        _smp.message_pool.push(new message("[room::requestChangePlayerItemRoom][Log][WARNING] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou trocar o ClubSet[ID=" + Convert.ToString(_cpir.clubset) + "] para comecar o jogo, mas acabou o tempo do ClubSet[ID=" + Convert.ToString(_cpir.clubset) + "], ele nao tem o ClubSet Padrao\"CV1\", adiciona o ClubSet pardrao\"CV1\" para ele. Hacker ou Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));

//                                        BuyItem bi = new BuyItem();
//                                        stItem item = new stItem();

//                                        bi.id = -1;
//                                        bi._typeid = AIR_KNIGHT_SET;
//                                        bi.qntd = 1;

//                                        item_manager.initItemFromBuyItem(_session.m_pi,
//                                           ref item, bi, false, 0, 0, 1);

//                                        if (item._typeid != 0)
//                                        {

//                                            if ((_cpir.clubset = item_manager.addItem(item,
//                                                _session, 2, 0)) != item_manager.RetAddItem.TYPE.T_ERROR)
//                                            {

//                                                // Equipa o ClubSet CV1
//                                                pWi = _session.m_pi.findWarehouseItemById(_cpir.clubset);

//                                                if (pWi != null)
//                                                {

//                                                    // Esse C do WarehouseItem, que pega do DB, não é o ja updado inicial da taqueira é o que fica tabela enchant,
//                                                    // que no original fica no warehouse msm, eu só confundi quando fiz
//                                                    _session.m_pi.ei.csi.setValues(pWi.id, pWi._typeid, pWi.c);

//                                                    cs = sIff.getInstance().findClubSet(pWi._typeid);

//                                                    if (cs != null)
//                                                    {

//                                                        for (var j = 0; j < 5; ++j)
//                                                        {
//                                                            _session.m_pi.ei.csi.enchant_c[j] = (short)(cs.Stats.getSlot()[j] + pWi.clubset_workshop.c[j]);
//                                                        }

//                                                    }

//                                                    _session.m_pi.ei.clubset = pWi;
//                                                    _session.m_pi.ue.clubset_id = pWi.id;

//                                                    // Zera o Error para o cliente equipar a "CV1" que o server equipou
//                                                    error = 0;

//                                                    // Update ON DB
//                                                    NormalManagerDB.add(0,
//                                                        new CmdUpdateClubsetEquiped(_session.m_pi.uid, _cpir.clubset),
//                                                        SQLDBResponse, this);

//                                                    // Update ON GAME
//                                                    p.init_plain((ushort)0x216);

//                                                    p.WriteUInt32((uint)UtilTime.GetSystemTimeAsUnix());
//                                                    p.WriteUInt32(1); // Count

//                                                    p.WriteByte(item.type);
//                                                    p.WriteUInt32(item._typeid);
//                                                    p.WriteInt32(item.id);
//                                                    p.WriteUInt32(item.flag_time);
//                                                    p.WriteStruct(item.stat, Marshal.SizeOf(new stItem.item_stat()));
//                                                    p.WriteUInt32((item.c[3] > 0) ? item.c[3] : item.c[0]);
//                                                    p.WriteZero(25);

//                                                    packet_func_sv.session_send(p,
//                                                        _session, 1);

//                                                }
//                                                else
//                                                {
//                                                    _smp.message_pool.push(new message("[room::requestChangePlayerItemRoom][Log][WARNING] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] nao conseguiu achar o ClubSet\"CV1\"[ID=" + Convert.ToString(item.id) + "] que acabou de adicionar para ele. Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));
//                                                }

//                                            }
//                                            else
//                                            {
//                                                _smp.message_pool.push(new message("[room::requestChangePlayerItemRoom][Log][WARNING] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] nao conseguiu adicionar o ClubSet[TYPEID=" + Convert.ToString(item._typeid) + "] para ele. Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));
//                                            }

//                                        }
//                                        else
//                                        {
//                                            _smp.message_pool.push(new message("[room::requestChangePlayerItemRoom][Log][WARNING] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] nao conseguiu inicializar o ClubSet[TYPEID=" + Convert.ToString(bi._typeid) + "] para ele. Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));
//                                        }
//                                    }
//                                }

//                            }
//                            else
//                            {

//                                error = (_cpir.clubset == 0) ? 1 : (pWi == null ? 2 : 3);

//                                pWi = _session.m_pi.findWarehouseItemByTypeid(AIR_KNIGHT_SET);

//                                if (pWi != null)
//                                {

//                                    _smp.message_pool.push(new message("[room::requestChangePlayerItemRoom][Log][WARNING] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou trocar o ClubSet[ID=" + Convert.ToString(_cpir.clubset) + "] para comecar o jogo, mas deu Error[VALUE=" + Convert.ToString(error) + "], colocando o ClubSet Padrao\"CV1\" do Player. Hacker ou Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));

//                                    // Esse C do WarehouseItem, que pega do DB, não é o ja updado inicial da taqueira é o que fica tabela enchant,
//                                    // que no original fica no warehouse msm, eu só confundi quando fiz
//                                    _session.m_pi.ei.csi.setValues(pWi.id, pWi._typeid, pWi.c);

//                                    cs = sIff.getInstance().findClubSet(pWi._typeid);

//                                    if (cs != null)
//                                    {

//                                        for (var j = 0; j < 5; ++j)
//                                        {
//                                            _session.m_pi.ei.csi.enchant_c[j] = (short)(cs.Stats.getSlot()[j] + pWi.clubset_workshop.c[j]);
//                                        }

//                                    }

//                                    _session.m_pi.ei.clubset = pWi;
//                                    _cpir.clubset = _session.m_pi.ue.clubset_id = pWi.id;

//                                    // Zera o Error para o cliente equipar a "CV1" que o server equipou
//                                    error = 0;

//                                    // Update ON DB
//                                    NormalManagerDB.add(0,
//                                        new CmdUpdateClubsetEquiped(_session.m_pi.uid, _cpir.clubset),
//                                        SQLDBResponse, this);

//                                }
//                                else
//                                {

//                                    _smp.message_pool.push(new message("[room::requestChangePlayerItemRoom][Log][WARNING] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou trocar o ClubSet[ID=" + Convert.ToString(_cpir.clubset) + "] para comecar o jogo, mas deu Error[VALUE=" + Convert.ToString(error) + "], ele nao tem o ClubSet Padrao\"CV1\", adiciona o ClubSet pardrao\"CV1\" para ele. Hacker ou Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));

//                                    BuyItem bi = new BuyItem();
//                                    stItem item = new stItem();

//                                    bi.id = -1;
//                                    bi._typeid = AIR_KNIGHT_SET;
//                                    bi.qntd = 1;

//                                    item_manager.initItemFromBuyItem(_session.m_pi,
//                                       ref item, bi, false, 0, 0, 1);

//                                    if (item._typeid != 0)
//                                    {

//                                        if ((_cpir.clubset = item_manager.addItem(item,
//                                            _session, 2, 0)) != item_manager.RetAddItem.TYPE.T_ERROR)
//                                        {

//                                            // Equipa o ClubSet CV1
//                                            pWi = _session.m_pi.findWarehouseItemById(_cpir.clubset);

//                                            if (pWi != null)
//                                            {

//                                                // Esse C do WarehouseItem, que pega do DB, não é o ja updado inicial da taqueira é o que fica tabela enchant,
//                                                // que no original fica no warehouse msm, eu só confundi quando fiz
//                                                _session.m_pi.ei.csi.setValues(pWi.id, pWi._typeid, pWi.c);

//                                                var cs = sIff.getInstance().findClubSet(pWi._typeid);

//                                                if (cs != null)
//                                                {
//                                                    // C++ TO C# CONVERTER WARNING: This 'sizeof' ratio was replaced with a direct reference to the array length:
//                                                    for (var j = 0; j < 5; ++j)
//                                                    {
//                                                        _session.m_pi.ei.csi.enchant_c[j] = cs.Stats.getSlot[j] + pWi.clubset_workshop.c[j];
//                                                    }
//                                                }

//                                                _session.m_pi.ei.clubset = pWi;
//                                                _session.m_pi.ue.clubset_id = pWi.id;

//                                                // Zera o Error para o cliente equipar a "CV1" que o server equipou
//                                                error = 0;

//                                                // Update ON DB
//                                                NormalManagerDB.add(0,
//                                                    new CmdUpdateClubsetEquiped(_session.m_pi.uid, _cpir.clubset),
//                                                    SQLDBResponse, this);

//                                                // Update ON GAME
//                                                p.init_plain((ushort)0x216);

//                                                p.WriteUInt32((uint)UtilTime.GetSystemTimeAsUnix());
//                                                p.WriteUInt32(1); // Count

//                                                p.WriteByte(item.type);
//                                                p.WriteUInt32(item._typeid);
//                                                p.WriteInt32(item.id);
//                                                p.WriteUInt32(item.flag_time);
//                                                p.WriteStruct(item.stat, Marshal.SizeOf(new stItem.item_stat()));
//                                                p.WriteUInt32((item.c[3] > 0) ? item.c[3] : item.c[0]);
//                                                p.WriteZero(25);

//                                                packet_func_sv.session_send(p,
//                                                    _session, 1);

//                                            }
//                                            else
//                                            {
//                                                _smp.message_pool.push(new message("[room::requestChangePlayerItemRoom][Log][WARNING] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] nao conseguiu achar o ClubSet\"CV1\"[ID=" + Convert.ToString(item.id) + "] que acabou de adicionar para ele. Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));
//                                            }

//                                        }
//                                        else
//                                        {
//                                            _smp.message_pool.push(new message("[room::requestChangePlayerItemRoom][Log][WARNING] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] nao conseguiu adicionar o ClubSet[TYPEID=" + Convert.ToString(item._typeid) + "] para ele. Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));
//                                        }

//                                    }
//                                    else
//                                    {
//                                        _smp.message_pool.push(new message("[room::requestChangePlayerItemRoom][Log][WARNING] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] nao conseguiu inicializar o ClubSet[TYPEID=" + Convert.ToString(bi._typeid) + "] para ele. Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));
//                                    }
//                                }
//                            }

//                            packet_func_sv.pacote04B(_session, _cpir.type, error);
//                            packet_func_sv.room_broadcast(this,
//                                p, 1);
//                            break;
//                        }
//                    case ChangePlayerItemRoom.TYPE_CHANGE.TC_CHARACTER: // Character
//                        {
//                            CharacterInfo pCe = null;

//                            if (_cpir.character != 0
//                                && (pCe = _session.m_pi.findCharacterById(_cpir.character)) != null
//                                && sIff.getInstance().getItemGroupIdentify(pCe._typeid) == sIff.getInstance().CHARACTER)
//                            {

//                                _session.m_pi.ei.char_info = pCe;
//                                _session.m_pi.ue.character_id = _cpir.character;

//                                // Update ON DB
//                                NormalManagerDB.add(0,
//                                    new CmdUpdateCharacterEquiped(_session.m_pi.uid, _cpir.character),
//                                    SQLDBResponse, this);

//                                // Update Player Info Channel and Room
//                                updatePlayerInfo(_session);

//                                PlayerRoomInfoEx pri = getPlayerInfo(_session);

//                                if ((RoomInfo.TIPO)getInfo().tipo != (byte)RoomInfo.TIPO.PRACTICE && (RoomInfo.TIPO)getInfo().tipo != (byte)RoomInfo.TIPO.GRAND_ZODIAC_PRACTICE)
//                                {
//                                    packet_func_sv.room_broadcast(this, packet_func_sv.pacote048(new List<PlayerRoomInfoEx>((pri == null) ? new PlayerRoomInfoEx() : pri), 0x103), 0);
//                                }

//                                if ((RoomInfo.TIPO)getInfo().tipo == (byte)RoomInfo.TIPO.LOUNGE)
//                                {

//                                    var it = (_session.m_pi.ei.char_info == null) ? _session.m_pi.mp_scl.Last() : _session.m_pi.mp_scl.FirstOrDefault(c => c.Key == _session.m_pi.ei.char_info.id);

//                                    if (it.Value != null)
//                                    {

//                                        _smp.message_pool.push(new message("[room::requestChangePlayerItemRoom][Error] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] nao tem os estados do character na lounge. Criando um novo para ele. Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));

//                                        // Add New State Character Lounge
//                                        _session.m_pi.mp_scl.Add(_session.m_pi.ei.char_info.id, new StateCharacterLounge());
//                                        var pair = _session.m_pi.mp_scl.FirstOrDefault(c => c.Key == _session.m_pi.ei.char_info.id);
//                                        it = pair;
//                                    }

//                                    p.init_plain((ushort)0x196);

//                                    p.WriteUInt32(_session.m_oid);

//                                    p.WriteStruct(it.Value, Marshal.SizeOf(new StateCharacterLounge()));

//                                    packet_func_sv.room_broadcast(this,
//                                        p, 0);
//                                }

//                            }
//                            else
//                            {

//                                error = (_cpir.character == 0) ? 1 : (pCe == null ? 2 : 3);

//                                if (_session.m_pi.mp_ce.Count > 0)
//                                {

//                                    _smp.message_pool.push(new message("[room::requestChangePlayerItemRoom][Log][WARNING] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou trocar o Character[ID=" + Convert.ToString(_cpir.character) + "] para comecar o jogo, mas deu Error[VALUE=" + Convert.ToString(error) + "], colocando o primeiro character do Player. Hacker ou Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));

//                                    _session.m_pi.ei.char_info = _session.m_pi.mp_ce.begin().Value;
//                                    _cpir.character = _session.m_pi.ue.character_id = _session.m_pi.ei.char_info.id;

//                                    // Zera o Error para o cliente equipar o Primeiro Character do map de character do Player, que o server equipou
//                                    error = 0;

//                                    // Update ON DB
//                                    NormalManagerDB.add(0,
//                                        new CmdUpdateCharacterEquiped(_session.m_pi.uid, _cpir.character),
//                                        SQLDBResponse, this);

//                                    // Update Player Info Channel and Room
//                                    updatePlayerInfo(_session);

//                                    PlayerRoomInfoEx pri = getPlayerInfo(_session);

//                                    if ((RoomInfo.TIPO)getInfo().tipo != (byte)RoomInfo.TIPO.PRACTICE && (RoomInfo.TIPO)getInfo().tipo != (byte)RoomInfo.TIPO.GRAND_ZODIAC_PRACTICE)
//                                    {
//                                        if (packet_func_sv.pacote048(p,
//                                            _session,
//                                            new List<PlayerRoomInfoEx>((pri == null) ? new PlayerRoomInfoEx() : pri),
//                                        0x103))
//                                        {
//                                            packet_func_sv.room_broadcast(this,
//                                                p, 0);
//                                        }
//                                    }

//                                    if ((RoomInfo.TIPO)getInfo().tipo == (byte)RoomInfo.TIPO.LOUNGE)
//                                    {

//                                        var it = (_session.m_pi.ei.char_info == null) ? _session.m_pi.mp_scl.Last() : _session.m_pi.mp_scl.FirstOrDefault(c => c.Key == _session.m_pi.ei.char_info.id);

//                                        if (it == _session.m_pi.mp_scl.Last())
//                                        {

//                                            _smp.message_pool.push(new message("[room::requestChangePlayerItemRoom][Error] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] nao tem os estados do character na lounge. Criando um novo para ele. Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));

//                                            // Add New State Character Lounge
//                                            _session.m_pi.mp_scl.Add(_session.m_pi.ei.char_info.id, new StateCharacterLounge());
//                                            var pair = _session.m_pi.mp_scl.FirstOrDefault(c => c.Key == _session.m_pi.ei.char_info.id);
//                                            it = pair;
//                                        }

//                                        p.init_plain((ushort)0x196);

//                                        p.WriteUInt32(_session.m_oid);

//                                        p.WriteStruct(it.Value, Marshal.SizeOf(new StateCharacterLounge());

//                                        packet_func_sv.room_broadcast(this,
//                                            p, 0);
//                                    }

//                                }
//                                else
//                                {

//                                    _smp.message_pool.push(new message("[room::requestChangePlayerItemRoom][Log][WARNING] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou trocar o Character[ID=" + Convert.ToString(_cpir.character) + "] para comecar o jogo, mas deu Error[VALUE=" + Convert.ToString(error) + "], ele nao tem nenhum character, adiciona o Nuri para ele. Hacker ou Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));

//                                    BuyItem bi = new BuyItem();
//                                    stItem item = new stItem();

//                                    bi.id = -1;
//                                    bi._typeid = sIff.getInstance().CHARACTER << 26; // Nuri
//                                    bi.qntd = 1;

//                                    item_manager.initItemFromBuyItem(_session.m_pi,
//                                        refref item, bi, false, 0, 0, 1);

//                                    if (item._typeid != 0)
//                                    {

//                                        // Add Item já atualiza o Character equipado
//                                        if ((_cpir.character = item_manager.addItem(item,
//                                            _session, 2, 0)) != item_manager.RetAddItem.TYPE.T_ERROR)
//                                        {

//                                            // Zera o Error para o cliente equipar o Nuri que o server equipou
//                                            error = 0;

//                                            // Update ON GAME
//                                            p.init_plain((ushort)0x216);

//                                            p.WriteUInt32((uint)UtilTime.GetSystemTimeAsUnix());
//                                            p.WriteUInt32(1); // Count

//                                            p.WriteByte(item.type);
//                                            p.WriteUInt32(item._typeid);
//                                            p.WriteInt32(item.id);
//                                            p.WriteUInt32(item.flag_time);
//                                            p.WriteStruct(item.stat, Marshal.SizeOf(new stItem.item_stat()));
//                                            p.WriteUInt32((item.c[3] > 0) ? item.c[3] : item.c[0]);
//                                            p.WriteZero(25);

//                                            packet_func_sv.session_send(p,
//                                                _session, 1);

//                                            // Update Player Info Channel and Room
//                                            updatePlayerInfo(_session);

//                                            PlayerRoomInfoEx pri = getPlayerInfo(_session);

//                                            if ((RoomInfo.TIPO)getInfo().tipo != (byte)RoomInfo.TIPO.PRACTICE && (RoomInfo.TIPO)getInfo().tipo != (byte)RoomInfo.TIPO.GRAND_ZODIAC_PRACTICE)
//                                            {
//                                                if (packet_func_sv.pacote048(p,
//                                                    _session,
//                                                    new List<PlayerRoomInfoEx>()((pri == null) ? new PlayerRoomInfoEx() : pri),
//                                                0x103))
//                                                {
//                                                    packet_func_sv.room_broadcast(this,
//                                                        p, 0);
//                                                }
//                                            }

//                                            if ((RoomInfo.TIPO)getInfo().tipo == (byte)RoomInfo.TIPO.LOUNGE)
//                                            {

//                                                var it = (_session.m_pi.ei.char_info == null) ? _session.m_pi.mp_scl.Last() : _session.m_pi.mp_scl.FirstOrDefault(c => c.Key == _session.m_pi.ei.char_info.id);

//                                                if (it != null)
//                                                {

//                                                    _smp.message_pool.push(new message("[room::requestChangePlayerItemRoom][Error] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] nao tem os estados do character na lounge. Criando um novo para ele. Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));

//                                                    // Add New State Character Lounge
//                                                    _session.m_pi.mp_scl.Add(_session.m_pi.ei.char_info.id, new StateCharacterLounge());
//                                                    var pair = _session.m_pi.mp_scl.FirstOrDefault(c => c.Key == _session.m_pi.ei.char_info.id);
//                                                    it = pair;
//                                                }

//                                                p.init_plain((ushort)0x196);

//                                                p.WriteUInt32(_session.m_oid);

//                                                p.WriteStruct(it.Value, Marshal.SizeOf(new StateCharacterLounge());

//                                                packet_func_sv.room_broadcast(this,
//                                                    p, 0);
//                                            }

//                                        }
//                                        else
//                                        {
//                                            _smp.message_pool.push(new message("[room::requestChangePlayerItemRoom][Log][WARNING] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] nao conseguiu adicionar o Character[TYPEID=" + Convert.ToString(item._typeid) + "] para ele. Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));
//                                        }

//                                    }
//                                    else
//                                    {
//                                        _smp.message_pool.push(new message("[room::requestChangePlayerItemRoom][Log][WARNING] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] nao conseguiu inicializar o Character[TYPEID=" + Convert.ToString(bi._typeid) + "] para ele. Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));
//                                    }
//                                }
//                            }

//                            packet_func_sv.pacote04B(_session, (byte)_cpir.type, error);
//                            packet_func_sv.room_broadcast(this,
//                                p, 1);
//                            break;
//                        }
//                    case ChangePlayerItemRoom.TYPE_CHANGE.TC_MASCOT: // Mascot
//                        {
//                            MascotInfoEx pMi = null;

//                            if (_cpir.mascot != 0)
//                            {

//                                if ((pMi = _session.m_pi.findMascotById(_cpir.mascot)) != null && sIff.getInstance().getItemGroupIdentify(pMi._typeid) == sIff.getInstance().MASCOT)
//                                {

//                                    var m_it = _session.m_pi.findUpdateItemByTypeidAndType(_session.m_pi.ue.mascot_id, UpdateItem.UI_TYPE.MASCOT);

//                                    if (m_it != _session.m_pi.mp_ui.Last())
//                                    {

//                                        // Desequipa o Mascot que acabou o tempo dele
//                                        _session.m_pi.ei.mascot_info = null;
//                                        _session.m_pi.ue.mascot_id = 0;

//                                        _cpir.mascot = 0;

//                                    }
//                                    else
//                                    {

//                                        // Mascot is good, update on server, DB and game
//                                        _session.m_pi.ei.mascot_info = pMi;
//                                        _session.m_pi.ue.mascot_id = _cpir.mascot;

//                                        // Verifica se o mascot pode equipar
//                                        if (_session.checkMascotEquiped(_session.m_pi.ue))
//                                        {
//                                            _cpir.mascot = _session.m_pi.ue.mascot_id;
//                                        }

//                                    }

//                                    // Update ON DB
//                                    NormalManagerDB.add(0,
//                                        new CmdUpdateMascotEquiped(_session.m_pi.uid, _cpir.mascot),
//                                        SQLDBResponse, this);

//                                }
//                                else
//                                {

//                                    error = (_cpir.mascot == 0) ? 1 : (pMi == null ? 2 : 3);

//                                    if (error > 1)
//                                    {
//                                        _smp.message_pool.push(new message("[room::requestChangePlayerItemRoom][Log][WARNING] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou trocar o Mascot[ID=" + Convert.ToString(_cpir.mascot) + "] para comecar o jogo, mas deu Error[VALUE=" + Convert.ToString(error) + "], desequipando o Mascot. Hacker ou Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));
//                                    }

//                                    _session.m_pi.ei.mascot_info = null;
//                                    _session.m_pi.ue.mascot_id = 0;

//                                    _cpir.mascot = 0;

//                                    // Att No DB
//                                    NormalManagerDB.add(0,
//                                        new CmdUpdateMascotEquiped(_session.m_pi.uid, _cpir.mascot),
//                                        SQLDBResponse, this);
//                                }

//                            }
//                            else if (_session.m_pi.ue.mascot_id > 0 && _session.m_pi.ei.mascot_info != null)
//                            { // Desequipa Mascot

//                                _session.m_pi.ei.mascot_info = null;
//                                _session.m_pi.ue.mascot_id = 0;

//                                _cpir.mascot = 0;

//                                // Att No DB
//                                NormalManagerDB.add(0,
//                                    new CmdUpdateMascotEquiped(_session.m_pi.uid, _cpir.mascot),
//                                    SQLDBResponse, this);

//                            } // else Não tem nenhum mascot equipado, para desequipar, então o cliente só quis atualizar o estado


//                            packet_func_sv.pacote04B(_session, (byte)_cpir.type, error);
//                            packet_func_sv.room_broadcast(this,
//                                p, 1);
//                            break;
//                        }
//                    case ChangePlayerItemRoom.TYPE_CHANGE.TC_ITEM_EFFECT_LOUNGE: // Itens Active, Jester x2 velocidade no lounge, e Harmes tamanho da cabeça
//                        {
//                            // ignora o item_id por que ele envia 0

//                            // Valor 1 Cabeca
//                            // Valor 2 Velocidade
//                            // Valor 3 Twilight

//                            if (!sIff.getInstance().isLoad())
//                            {
//                                sIff.getInstance().load();
//                            }

//                            if (_session.m_pi.ei.char_info == null)
//                            {
//                                throw new exception("[room::requestChangePlayerItemRoom][Error] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] nao tem nenhum character equipado. Hacker ou Bug.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.CHANNEL,
//                                    1000, 0x57007));
//                            }

//                            if (_cpir.effect_lounge.effect != ChangePlayerItemRoom.stItemEffectLounge.TYPE_EFFECT.TE_TWILIGHT)
//                            {

//                                var it = (_session.m_pi.ei.char_info == null) ? _session.m_pi.mp_scl.Last() : _session.m_pi.mp_scl.FirstOrDefault(c => c.Key == _session.m_pi.ei.char_info.id);

//                                if (it == _session.m_pi.mp_scl.Last())
//                                {

//                                    _smp.message_pool.push(new message("[room::requestChangePlayerItemRoom][Error] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] nao tem os estados do character na lounge. Criando um novo para ele. Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));

//                                    // Add New State Character Lounge
//                                    _session.m_pi.mp_scl.Add(_session.m_pi.ei.char_info.id, new StateCharacterLounge());
//                                    var pair = _session.m_pi.mp_scl.FirstOrDefault(c => c.Key == _session.m_pi.ei.char_info.id);
//                                    it = pair;
//                                }

//                                switch (_cpir.effect_lounge.effect)
//                                {
//                                    case ChangePlayerItemRoom.stItemEffectLounge.TYPE_EFFECT.TE_BIG_HEAD: // Jester (Big head)
//                                        {

//                                            // C++ TO C# CONVERTER TASK: Lambda expressions cannot be assigned to 'var':
//                                            var ccj = std::find_if(Globals.cadie_cauldron_Jester_item_typeid, LAST_ELEMENT_IN_ARRAY(Globals.cadie_cauldron_Jester_item_typeid), (el) =>
//                                            {
//                                                return sIff.getInstance().getItemCharIdentify(el) == (_session.m_pi.ei.char_info._typeid & 0x000000FF);
//                                            });

//                                            if (ccj == LAST_ELEMENT_IN_ARRAY(Globals.cadie_cauldron_Jester_item_typeid))
//                                            {
//                                                throw new exception("[room::requestChangePlayerItemRoom][Error] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] o Character[TYPEID=" + Convert.ToString(_session.m_pi.ei.char_info._typeid) + "] equipado nao tem o item Jester no server. Hacker ou Bug.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.CHANNEL,
//                                                    1001, 0x57008));
//                                            }

//                                            if (!_session.m_pi.ei.char_info.isPartEquiped(*ccj))
//                                            {
//                                                throw new exception("[room::requestChangePlayerItemRoom][Error] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] o Character[TYPEID=" + Convert.ToString(_session.m_pi.ei.char_info._typeid) + "] nao esta com o item[TYPEID=" + Convert.ToString(*ccj) + "] Jester equipado. Hacker ou Bug.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.CHANNEL,
//                                                    1002, 0x57009));
//                                            }

//                                            it.Value.scale_head = (it.Value.scale_head > 1.0f) ? 1.0f : 2.0f;

//                                            break;
//                                        }
//                                    case ChangePlayerItemRoom.stItemEffectLounge.TYPE_EFFECT.TE_FAST_WALK: // Hermes (Velocidade x2)
//                                        {

//                                            // C++ TO C# CONVERTER TASK: Lambda expressions cannot be assigned to 'var':
//                                            var cch = std::find_if(Globals.cadie_cauldron_Hermes_item_typeid, LAST_ELEMENT_IN_ARRAY(Globals.cadie_cauldron_Hermes_item_typeid), (el) =>
//                                            {
//                                                return sIff.getInstance().getItemCharIdentify(el) == (_session.m_pi.ei.char_info._typeid & 0x000000FF);
//                                            });

//                                            if (cch == LAST_ELEMENT_IN_ARRAY(Globals.cadie_cauldron_Hermes_item_typeid))
//                                            {
//                                                throw new exception("[room::requestChangePlayerItemRoom][Error] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] o Character[TYPEID=" + Convert.ToString(_session.m_pi.ei.char_info._typeid) + "] equipado nao tem o item Hermes no server. Hacker ou Bug.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.CHANNEL,
//                                                    1001, 0x57008));
//                                            }

//                                            if (!_session.m_pi.ei.char_info.isPartEquiped(*cch))
//                                            {
//                                                throw new exception("[room::requestChangePlayerItemRoom][Error] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] o Character[TYPEID=" + Convert.ToString(_session.m_pi.ei.char_info._typeid) + "] nao esta com o item[TYPEID=" + Convert.ToString(*cch) + "] Hermes equipado. Hacker ou Bug.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.CHANNEL,
//                                                    1002, 0x57009));
//                                            }

//                                            it.Value.walk_speed = (it.Value.walk_speed > 1.0f) ? 1.0f : 2.0f;

//                                            break;
//                                        }
//                                } // End Switch

//                            }
//                            else
//                            {
//                                // else == 3 // Twilight (Fogos de artifícios em cima da cabeça do Player)
//                                // Valor 1 pass para fazer o fogos

//                                // C++ TO C# CONVERTER TASK: Lambda expressions cannot be assigned to 'var':
//                                var cct = std::find_if(Globals.cadie_cauldron_Twilight_item_typeid, LAST_ELEMENT_IN_ARRAY(Globals.cadie_cauldron_Twilight_item_typeid), (el) =>
//                                {
//                                    return sIff.getInstance().getItemCharIdentify(el) == (_session.m_pi.ei.char_info._typeid & 0x000000FF);
//                                });

//                                if (cct == LAST_ELEMENT_IN_ARRAY(Globals.cadie_cauldron_Twilight_item_typeid))
//                                {
//                                    throw new exception("[room::requestChangePlayerItemRoom][Error] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] o Character[TYPEID=" + Convert.ToString(_session.m_pi.ei.char_info._typeid) + "] equipado nao tem o item Twilight no server. Hacker ou Bug.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.CHANNEL,
//                                        1001, 0x57008));
//                                }

//                                if (!_session.m_pi.ei.char_info.isPartEquiped(*cct))
//                                {
//                                    throw new exception("[room::requestChangePlayerItemRoom][Error] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] o Character[TYPEID=" + Convert.ToString(_session.m_pi.ei.char_info._typeid) + "] nao esta com o item[TYPEID=" + Convert.ToString(*cct) + "] Twilight equipado. Hacker ou Bug.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.CHANNEL,
//                                        1002, 0x57009));
//                                }
//                            }

//                            packet_func_sv.pacote04B(p,
//                                _session, _cpir.type, error,
//                                _cpir.effect_lounge.effect);
//                            packet_func_sv.room_broadcast(this,
//                                p, 1);
//                            break;
//                        }
//                    case ChangePlayerItemRoom.TYPE_CHANGE.TC_ALL: // Todos
//                        {
//                            // Aqui se não tiver os itens, algum hacker, gera Log, e coloca item padrão ou nenhum
//                            CharacterInfo pCe = null;
//                            CaddieInfoEx pCi = null;
//                            WarehouseItemEx pWi = null;
//                            uint error = 0;

//                            // Character
//                            if (_cpir.character != 0
//                                && (pCe = _session.m_pi.findCharacterById(_cpir.character)) != null
//                                && sIff.getInstance().getItemGroupIdentify(pCe._typeid) == sIff.getInstance().CHARACTER)
//                            {

//                                _session.m_pi.ei.char_info = pCe;
//                                _session.m_pi.ue.character_id = _cpir.character;

//                                // Update ON DB
//                                NormalManagerDB.add(0,
//                                    new CmdUpdateCharacterEquiped(_session.m_pi.uid, _cpir.character),
//                                    SQLDBResponse, this);

//                            }
//                            else
//                            {

//                                error = (_cpir.character == 0) ? 1 : (pCe == null ? 2 : 3);

//                                if (_session.m_pi.mp_ce.Count > 0)
//                                {

//                                    _smp.message_pool.push(new message("[room::requestChangePlayerItemRoom][Log][WARNING] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou trocar o Character[ID=" + Convert.ToString(_cpir.character) + "] para comecar o jogo, mas deu Error[VALUE=" + Convert.ToString(error) + "], colocando o primeiro character do Player. Hacker ou Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));

//                                    _session.m_pi.ei.char_info = _session.m_pi.mp_ce.begin().Value;
//                                    _cpir.character = _session.m_pi.ue.character_id = _session.m_pi.ei.char_info.id;

//                                    // Update ON DB
//                                    NormalManagerDB.add(0,
//                                        new CmdUpdateCharacterEquiped(_session.m_pi.uid, _cpir.character),
//                                        SQLDBResponse, this);

//                                }
//                                else
//                                {

//                                    _smp.message_pool.push(new message("[room::requestChangePlayerItemRoom][Log][WARNING] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou trocar o Character[ID=" + Convert.ToString(_cpir.character) + "] para comecar o jogo, mas deu Error[VALUE=" + Convert.ToString(error) + "], ele nao tem nenhum character, adiciona o Nuri para ele. Hacker ou Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));

//                                    BuyItem bi = new BuyItem();
//                                    stItem item = new stItem();

//                                    bi.id = -1;
//                                    bi._typeid = sIff.getInstance().CHARACTER << 26; // Nuri
//                                    bi.qntd = 1;

//                                    item_manager.initItemFromBuyItem(_session.m_pi,
//                                       ref item, bi, false, 0, 0, 1);

//                                    if (item._typeid != 0)
//                                    {

//                                        // Add Item já atualiza o Character equipado
//                                        if ((_cpir.character = item_manager.addItem(item,
//                                            _session, 2, 0)) != item_manager.RetAddItem.TYPE.T_ERROR)
//                                        {

//                                            // Update ON GAME
//                                            p.init_plain((ushort)0x216);

//                                            p.WriteUInt32((uint)UtilTime.GetSystemTimeAsUnix());
//                                            p.WriteUInt32(1); // Count

//                                            p.WriteByte(item.type);
//                                            p.WriteUInt32(item._typeid);
//                                            p.WriteInt32(item.id);
//                                            p.WriteUInt32(item.flag_time);
//                                            p.WriteStruct(item.stat, Marshal.SizeOf(new stItem.item_stat()));
//                                            p.WriteUInt32((item.c[3] > 0) ? item.c[3] : item.c[0]);
//                                            p.WriteZero(25);

//                                            packet_func_sv.session_send(p,
//                                                _session, 1);

//                                        }
//                                        else
//                                        {
//                                            _smp.message_pool.push(new message("[room::requestChangePlayerItemRoom][Log][WARNING] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] nao conseguiu adicionar o Character[TYPEID=" + Convert.ToString(item._typeid) + "] para ele. Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));
//                                        }

//                                    }
//                                    else
//                                    {
//                                        _smp.message_pool.push(new message("[room::requestChangePlayerItemRoom][Log][WARNING] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] nao conseguiu inicializar o Character[TYPEID=" + Convert.ToString(bi._typeid) + "] para ele. Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));
//                                    }
//                                }
//                            }

//                            // Caddie
//                            if (_cpir.caddie != 0
//                                && (pCi = _session.m_pi.findCaddieById(_cpir.caddie)) != null
//                                && sIff.getInstance().getItemGroupIdentify(pCi._typeid) == sIff.getInstance().CADDIE)
//                            {

//                                // Check if item is in map of update item
//                                var v_it = _session.m_pi.findUpdateItemByTypeidAndId(pCi._typeid, pCi.id);

//                                if (v_it.Any())
//                                {

//                                    foreach (var el in v_it)
//                                    {

//                                        if (el.Value.type == UpdateItem.UI_TYPE.CADDIE)
//                                        {

//                                            // Desequipa o caddie
//                                            _session.m_pi.ei.cad_info = null;
//                                            _session.m_pi.ue.caddie_id = 0;

//                                            _cpir.caddie = 0;

//                                        }
//                                        else if (el.Value.type == UpdateItem.UI_TYPE.CADDIE_PARTS)
//                                        {

//                                            // Limpa o caddie Parts
//                                            pCi.parts_typeid = 0;
//                                            pCi.parts_end_date_unix = 0;
//                                            pCi.end_parts_date = new PangyaTime();

//                                            _session.m_pi.ei.cad_info = pCi;
//                                            _session.m_pi.ue.caddie_id = _cpir.caddie;
//                                        }

//                                        // Tira esse Update Item do map
//                                        _session.m_pi.mp_ui.Remove(el.Key);
//                                    }

//                                }
//                                else
//                                {

//                                    // Caddie is Good, Update caddie equiped ON SERVER AND DB
//                                    _session.m_pi.ei.cad_info = pCi;
//                                    _session.m_pi.ue.caddie_id = _cpir.caddie;

//                                    // Verifica se o Caddie pode equipar
//                                    if (_session.checkCaddieEquiped(_session.m_pi.ue))
//                                    {
//                                        _cpir.caddie = _session.m_pi.ue.caddie_id;
//                                    }

//                                }

//                                // Update ON DB
//                                NormalManagerDB.add(0,
//                                    new CmdUpdateCaddieEquiped(_session.m_pi.uid, _cpir.caddie),
//                                    SQLDBResponse, this);

//                            }
//                            else if (_session.m_pi.ue.caddie_id > 0 && _session.m_pi.ei.cad_info != null)
//                            { // Desequipa Caddie

//                                error = (_cpir.caddie == 0) ? 1 : (pCi == null ? 2 : 3);

//                                if (error > 1)
//                                {
//                                    _smp.message_pool.push(new message("[room::requestChangePlayerItemRoom][Log][WARNING] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou trocar o Caddie[ID=" + Convert.ToString(_cpir.caddie) + "] para comecar o jogo, mas deu Error[VALUE=" + Convert.ToString(error) + "], desequipando o caddie. Hacker ou Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));
//                                }

//                                // Check if item is in map of update item
//                                var v_it = _session.m_pi.findUpdateItemByTypeidAndId(_session.m_pi.ei.cad_info._typeid, _session.m_pi.ei.cad_info.id);

//                                if (v_it.Any())
//                                {

//                                    foreach (var el in v_it)
//                                    {

//                                        // Caddie já vai se desequipar, só verifica o parts
//                                        if (el.Value.type == UpdateItem.UI_TYPE.CADDIE_PARTS)
//                                        {

//                                            // Limpa o caddie Parts
//                                            _session.m_pi.ei.cad_info.parts_typeid = 0;
//                                            _session.m_pi.ei.cad_info.parts_end_date_unix = 0;
//                                            _session.m_pi.ei.cad_info.end_parts_date = { 0 };
//                                        }

//                                        // Tira esse Update Item do map
//                                        _session.m_pi.mp_ui.Remove(el.Key);
//                                    }

//                                }

//                                _session.m_pi.ei.cad_info = null;
//                                _session.m_pi.ue.caddie_id = 0;

//                                _cpir.caddie = 0;

//                                // Att No DB
//                                NormalManagerDB.add(0,
//                                    new CmdUpdateCaddieEquiped(_session.m_pi.uid, _cpir.caddie),
//                                    SQLDBResponse, this);
//                            }

//                            // ClubSet
//                            if (_cpir.clubset != 0
//                                && (pWi = _session.m_pi.findWarehouseItemById(_cpir.clubset)) != null
//                                && sIff.getInstance().getItemGroupIdentify(pWi._typeid) == sIff.getInstance().CLUBSET)
//                            {

//                                var c_it = _session.m_pi.findUpdateItemByTypeidAndType(_cpir.clubset, UpdateItem.UI_TYPE.WAREHOUSE);

//                                if (c_it == _session.m_pi.mp_ui.Last())
//                                {

//                                    _session.m_pi.ei.clubset = pWi;

//                                    // Esse C do WarehouseItem, que pega do DB, não é o ja updado inicial da taqueira é o que fica tabela enchant,
//                                    // que no original fica no warehouse msm, eu só confundi quando fiz
//                                    _session.m_pi.ei.csi.setValues(pWi.id, pWi._typeid, pWi.c);

//                                    var cs = sIff.getInstance().findClubSet(pWi._typeid);

//                                    if (cs != null)
//                                    {

//                                        // C++ TO C# CONVERTER WARNING: This 'sizeof' ratio was replaced with a direct reference to the array length:
//                                        for (var j = 0; j < 5; ++j)
//                                        {
//                                            _session.m_pi.ei.csi.enchant_c[j] = cs.Stats.getSlot[j] + pWi.clubset_workshop.c[j];
//                                        }

//                                        _session.m_pi.ue.clubset_id = _cpir.clubset;

//                                        // Verifica se o ClubSet pode equipar
//                                        //if (_session.checkClubSetEquiped(_session.m_pi.ue))
//                                        {
//                                            _cpir.clubset = _session.m_pi.ue.clubset_id;
//                                            // }

//                                            // Update ON DB
//                                            //NormalManagerDB.add(0,
//                                            //    new CmdUpdateClubsetEquiped(_session.m_pi.uid, _cpir.clubset),
//                                            //    SQLDBResponse, this);

//                                        }
//                                    else
//                                        {

//                                            error = 5;

//                                            _smp.message_pool.push(new message("[room::requestChangePlayerItemRoom][Error] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou Atualizar Clubset[TYPEID=" + Convert.ToString(pWi._typeid) + ", ID=" + Convert.ToString(pWi.id) + "] equipado, mas ClubSet Not exists on IFF structure. Equipa o ClubSet padrao. Hacker ou Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));

//                                            // Coloca o ClubSet CV1 no lugar do ClubSet que acabou o tempo
//                                            pWi = _session.m_pi.findWarehouseItemByTypeid(AIR_KNIGHT_SET);

//                                            if (pWi != null)
//                                            {

//                                                _smp.message_pool.push(new message("[room::requestChangePlayerItemRoom][Log][WARNING] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou trocar o ClubSet[ID=" + Convert.ToString(_cpir.clubset) + "] para comecar o jogo, mas acabou o tempo do ClubSet[ID=" + Convert.ToString(_cpir.clubset) + "], colocando o ClubSet Padrao\"CV1\" do Player. Hacker ou Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));

//                                                // Esse C do WarehouseItem, que pega do DB, não é o ja updado inicial da taqueira é o que fica tabela enchant,
//                                                // que no original fica no warehouse msm, eu só confundi quando fiz
//                                                _session.m_pi.ei.csi.setValues(pWi.id, pWi._typeid, pWi.c);

//                                                cs = sIff.getInstance().findClubSet(pWi._typeid);

//                                                if (cs != null)
//                                                {
//                                                    // C++ TO C# CONVERTER WARNING: This 'sizeof' ratio was replaced with a direct reference to the array length:
//                                                    for (var j = 0; j < 5; ++j)
//                                                    {
//                                                        _session.m_pi.ei.csi.enchant_c[j] = cs.Stats.getSlot()[j] + pWi.clubset_workshop.c[j];
//                                                    }
//                                                }

//                                                _session.m_pi.ei.clubset = pWi;
//                                                _cpir.clubset = _session.m_pi.ue.clubset_id = pWi.id;

//                                                // Update ON DB
//                                                NormalManagerDB.add(0,
//                                                    new CmdUpdateClubsetEquiped(_session.m_pi.uid, _cpir.clubset),
//                                                    SQLDBResponse, this);

//                                            }
//                                            else
//                                            {

//                                                _smp.message_pool.push(new message("[channel::requestChangePlayerItemRoom][Log][WARNING] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou trocar o ClubSet[ID=" + Convert.ToString(_cpir.clubset) + "] para comecar o jogo, mas acabou o tempo do ClubSet[ID=" + Convert.ToString(_cpir.clubset) + "], ele nao tem o ClubSet Padrao\"CV1\", adiciona o ClubSet pardrao\"CV1\" para ele. Hacker ou Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));

//                                                BuyItem bi = new BuyItem();
//                                                stItem item = new stItem();

//                                                bi.id = -1;
//                                                bi._typeid = AIR_KNIGHT_SET;
//                                                bi.qntd = 1;

//                                                item_manager.initItemFromBuyItem(_session.m_pi,
//                                                   ref item, bi, false, 0, 0, 1);

//                                                if (item._typeid != 0)
//                                                {

//                                                    if ((_cpir.clubset = item_manager.addItem(item,
//                                                        _session, 2, 0)) != item_manager.RetAddItem.TYPE.T_ERROR)
//                                                    {

//                                                        // Equipa o ClubSet CV1
//                                                        pWi = _session.m_pi.findWarehouseItemById(_cpir.clubset);

//                                                        if (pWi != null)
//                                                        {

//                                                            // Esse C do WarehouseItem, que pega do DB, não é o ja updado inicial da taqueira é o que fica tabela enchant,
//                                                            // que no original fica no warehouse msm, eu só confundi quando fiz
//                                                            _session.m_pi.ei.csi.setValues(pWi.id, pWi._typeid, pWi.c);

//                                                            var cs = sIff.getInstance().findClubSet(pWi._typeid);

//                                                            if (cs != null)
//                                                            {
//                                                                // C++ TO C# CONVERTER WARNING: This 'sizeof' ratio was replaced with a direct reference to the array length:
//                                                                for (var j = 0; j < 5; ++j)
//                                                                    for (var j = 0; j < 5; ++j)
//                                                                    {
//                                                                        _session.m_pi.ei.csi.enchant_c[j] = cs.slot[j] + pWi.clubset_workshop.c[j];
//                                                                    }
//                                                            }

//                                                            _session.m_pi.ei.clubset = pWi;
//                                                            _session.m_pi.ue.clubset_id = pWi.id;

//                                                            // Update ON DB
//                                                            NormalManagerDB.add(0,
//                                                                new CmdUpdateClubsetEquiped(_session.m_pi.uid, _cpir.clubset),
//                                                                SQLDBResponse, this);

//                                                            // Update ON GAME
//                                                            p.init_plain((ushort)0x216);

//                                                            p.WriteUInt32((uint)UtilTime.GetSystemTimeAsUnix());
//                                                            p.WriteUInt32(1); // Count

//                                                            p.WriteByte(item.type);
//                                                            p.WriteUInt32(item._typeid);
//                                                            p.WriteInt32(item.id);
//                                                            p.WriteUInt32(item.flag_time);
//                                                            p.WriteStruct(item.stat, Marshal.SizeOf(new stItem.item_stat()));
//                                                            p.WriteUInt32((item.c[3] > 0) ? item.c[3] : item.c[0]);
//                                                            p.WriteZero(25);

//                                                            packet_func_sv.session_send(p,
//                                                                _session, 1);

//                                                        }
//                                                        else
//                                                        {
//                                                            _smp.message_pool.push(new message("[room::requestChangePlayerItemRoom][Log][WARNING] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] nao conseguiu achar o ClubSet\"CV1\"[ID=" + Convert.ToString(item.id) + "] que acabou de adicionar para ele. Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));
//                                                        }

//                                                    }
//                                                    else
//                                                    {
//                                                        _smp.message_pool.push(new message("[room::requestChangePlayerItemRoom][Log][WARNING] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] nao conseguiu adicionar o ClubSet[TYPEID=" + Convert.ToString(item._typeid) + "] para ele. Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));
//                                                    }

//                                                }
//                                                else
//                                                {
//                                                    _smp.message_pool.push(new message("[room::requestChangePlayerItemRoom][Log][WARNING] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] nao conseguiu inicializar o ClubSet[TYPEID=" + Convert.ToString(bi._typeid) + "] para ele. Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));
//                                                }
//                                            }
//                                        }

//                                    }
//                                    else
//                                    { // ClubSet Acabou o tempo

//                                        // Coloca o ClubSet CV1 no lugar do ClubSet que acabou o tempo
//                                        pWi = _session.m_pi.findWarehouseItemByTypeid(AIR_KNIGHT_SET);

//                                        if (pWi != null)
//                                        {

//                                            _smp.message_pool.push(new message("[room::requestChangePlayerItemRoom][Log][WARNING] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou trocar o ClubSet[ID=" + Convert.ToString(_cpir.clubset) + "] para comecar o jogo, mas acabou o tempo do ClubSet[ID=" + Convert.ToString(_cpir.clubset) + "], colocando o ClubSet Padrao\"CV1\" do Player. Hacker ou Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));

//                                            // Esse C do WarehouseItem, que pega do DB, não é o ja updado inicial da taqueira é o que fica tabela enchant,
//                                            // que no original fica no warehouse msm, eu só confundi quando fiz
//                                            _session.m_pi.ei.csi.setValues(pWi.id, pWi._typeid, pWi.c);

//                                            var cs = sIff.getInstance().findClubSet(pWi._typeid);

//                                            if (cs != null)
//                                            {
//                                                // C++ TO C# CONVERTER WARNING: This 'sizeof' ratio was replaced with a direct reference to the array length:
//                                                for (var j = 0; j < 5; ++j)
//                                                    for (var j = 0; j < 5; ++j)
//                                                    {
//                                                        _session.m_pi.ei.csi.enchant_c[j] = cs.slot[j] + pWi.clubset_workshop.c[j];
//                                                    }
//                                            }

//                                            _session.m_pi.ei.clubset = pWi;
//                                            _cpir.clubset = _session.m_pi.ue.clubset_id = pWi.id;

//                                            // Update ON DB
//                                            NormalManagerDB.add(0,
//                                                new CmdUpdateClubsetEquiped(_session.m_pi.uid, _cpir.clubset),
//                                                SQLDBResponse, this);

//                                        }
//                                        else
//                                        {

//                                            _smp.message_pool.push(new message("[room::requestChangePlayerItemRoom][Log][WARNING] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou trocar o ClubSet[ID=" + Convert.ToString(_cpir.clubset) + "] para comecar o jogo, mas acabou o tempo do ClubSet[ID=" + Convert.ToString(_cpir.clubset) + "], ele nao tem o ClubSet Padrao\"CV1\", adiciona o ClubSet pardrao\"CV1\" para ele. Hacker ou Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));

//                                            BuyItem bi = new BuyItem();
//                                            stItem item = new stItem();

//                                            bi.id = -1;
//                                            bi._typeid = AIR_KNIGHT_SET;
//                                            bi.qntd = 1;

//                                            item_manager.initItemFromBuyItem(_session.m_pi,
//                                               ref item, bi, false, 0, 0, 1);

//                                            if (item._typeid != 0)
//                                            {

//                                                if ((_cpir.clubset = item_manager.addItem(item,
//                                                    _session, 2, 0)) != item_manager.RetAddItem.TYPE.T_ERROR)
//                                                {

//                                                    // Equipa o ClubSet CV1
//                                                    pWi = _session.m_pi.findWarehouseItemById(_cpir.clubset);

//                                                    if (pWi != null)
//                                                    {

//                                                        // Esse C do WarehouseItem, que pega do DB, não é o ja updado inicial da taqueira é o que fica tabela enchant,
//                                                        // que no original fica no warehouse msm, eu só confundi quando fiz
//                                                        _session.m_pi.ei.csi.setValues(pWi.id, pWi._typeid, pWi.c);

//                                                        var cs = sIff.getInstance().findClubSet(pWi._typeid);

//                                                        if (cs != null)
//                                                        {
//                                                            // C++ TO C# CONVERTER WARNING: This 'sizeof' ratio was replaced with a direct reference to the array length:
//                                                            for (var j = 0; j < 5; ++j)
//                                                                for (var j = 0; j < 5; ++j)
//                                                                {
//                                                                    _session.m_pi.ei.csi.enchant_c[j] = cs.slot[j] + pWi.clubset_workshop.c[j];
//                                                                }
//                                                        }

//                                                        _session.m_pi.ei.clubset = pWi;
//                                                        _session.m_pi.ue.clubset_id = pWi.id;

//                                                        // Update ON DB
//                                                        NormalManagerDB.add(0,
//                                                            new CmdUpdateClubsetEquiped(_session.m_pi.uid, _cpir.clubset),
//                                                            SQLDBResponse, this);

//                                                        // Update ON GAME
//                                                        p.init_plain((ushort)0x216);

//                                                        p.WriteUInt32((uint)UtilTime.GetSystemTimeAsUnix());
//                                                        p.WriteUInt32(1); // Count

//                                                        p.WriteByte(item.type);
//                                                        p.WriteUInt32(item._typeid);
//                                                        p.WriteInt32(item.id);
//                                                        p.WriteUInt32(item.flag_time);
//                                                        p.WriteStruct(item.stat, Marshal.SizeOf(new stItem.item_stat()));
//                                                        p.WriteUInt32((item.c[3] > 0) ? item.c[3] : item.c[0]);
//                                                        p.WriteZero(25);

//                                                        packet_func_sv.session_send(p,
//                                                            _session, 1);

//                                                    }
//                                                    else
//                                                    {
//                                                        _smp.message_pool.push(new message("[room::requestChangePlayerItemRoom][Log][WARNING] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] nao conseguiu achar o ClubSet\"CV1\"[ID=" + Convert.ToString(item.id) + "] que acabou de adicionar para ele. Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));
//                                                    }

//                                                }
//                                                else
//                                                {
//                                                    _smp.message_pool.push(new message("[room::requestChangePlayerItemRoom][Log][WARNING] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] nao conseguiu adicionar o ClubSet[TYPEID=" + Convert.ToString(item._typeid) + "] para ele. Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));
//                                                }

//                                            }
//                                            else
//                                            {
//                                                _smp.message_pool.push(new message("[room::requestChangePlayerItemRoom][Log][WARNING] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] nao conseguiu inicializar o ClubSet[TYPEID=" + Convert.ToString(bi._typeid) + "] para ele. Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));
//                                            }
//                                        }
//                                    }

//                                }
//                                else
//                                {

//                                    error = (_cpir.clubset == 0) ? 1 : (pWi == null ? 2 : 3);

//                                    pWi = _session.m_pi.findWarehouseItemByTypeid(AIR_KNIGHT_SET);

//                                    if (pWi != null)
//                                    {

//                                        _smp.message_pool.push(new message("[room::requestChangePlayerItemRoom][Log][WARNING] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou trocar o ClubSet[ID=" + Convert.ToString(_cpir.clubset) + "] para comecar o jogo, mas deu Error[VALUE=" + Convert.ToString(error) + "], colocando o ClubSet Padrao\"CV1\" do Player. Hacker ou Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));

//                                        // Esse C do WarehouseItem, que pega do DB, não é o ja updado inicial da taqueira é o que fica tabela enchant,
//                                        // que no original fica no warehouse msm, eu só confundi quando fiz
//                                        _session.m_pi.ei.csi.setValues(pWi.id, pWi._typeid, pWi.c);

//                                        var cs = sIff.getInstance().findClubSet(pWi._typeid);

//                                        if (cs != null)
//                                        {
//                                            // C++ TO C# CONVERTER WARNING: This 'sizeof' ratio was replaced with a direct reference to the array length:
//                                            for (var j = 0; j < 5; ++j)
//                                                for (var j = 0; j < 5; ++j)
//                                                {
//                                                    _session.m_pi.ei.csi.enchant_c[j] = cs.slot[j] + pWi.clubset_workshop.c[j];
//                                                }
//                                        }

//                                        _session.m_pi.ei.clubset = pWi;
//                                        _cpir.clubset = _session.m_pi.ue.clubset_id = pWi.id;

//                                        // Update ON DB
//                                        NormalManagerDB.add(0,
//                                            new CmdUpdateClubsetEquiped(_session.m_pi.uid, _cpir.clubset),
//                                            SQLDBResponse, this);

//                                    }
//                                    else
//                                    {

//                                        _smp.message_pool.push(new message("[room::requestChangePlayerItemRoom][Log][WARNING] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou trocar o ClubSet[ID=" + Convert.ToString(_cpir.clubset) + "] para comecar o jogo, mas deu Error[VALUE=" + Convert.ToString(error) + "], ele nao tem o ClubSet Padrao\"CV1\", adiciona o ClubSet pardrao\"CV1\" para ele. Hacker ou Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));

//                                        BuyItem bi = new BuyItem();
//                                        stItem item = new stItem();

//                                        bi.id = -1;
//                                        bi._typeid = AIR_KNIGHT_SET;
//                                        bi.qntd = 1;

//                                        item_manager.initItemFromBuyItem(_session.m_pi,
//                                           ref item, bi, false, 0, 0, 1);

//                                        if (item._typeid != 0)
//                                        {

//                                            if ((_cpir.clubset = item_manager.addItem(item,
//                                                _session, 2, 0)) != item_manager.RetAddItem.TYPE.T_ERROR)
//                                            {

//                                                // Equipa o ClubSet CV1
//                                                pWi = _session.m_pi.findWarehouseItemById(_cpir.clubset);

//                                                if (pWi != null)
//                                                {

//                                                    // Esse C do WarehouseItem, que pega do DB, não é o ja updado inicial da taqueira é o que fica tabela enchant,
//                                                    // que no original fica no warehouse msm, eu só confundi quando fiz
//                                                    _session.m_pi.ei.csi.setValues(pWi.id, pWi._typeid, pWi.c);

//                                                    var cs = sIff.getInstance().findClubSet(pWi._typeid);

//                                                    if (cs != null)
//                                                    {
//                                                        // C++ TO C# CONVERTER WARNING: This 'sizeof' ratio was replaced with a direct reference to the array length:
//                                                        for (var j = 0; j < 5; ++j)
//                                                            for (var j = 0; j < 5; ++j)
//                                                            {
//                                                                _session.m_pi.ei.csi.enchant_c[j] = cs.slot[j] + pWi.clubset_workshop.c[j];
//                                                            }
//                                                    }

//                                                    _session.m_pi.ei.clubset = pWi;
//                                                    _session.m_pi.ue.clubset_id = pWi.id;

//                                                    // Update ON DB
//                                                    NormalManagerDB.add(0,
//                                                        new CmdUpdateClubsetEquiped(_session.m_pi.uid, _cpir.clubset),
//                                                        SQLDBResponse, this);

//                                                    // Update ON GAME
//                                                    p.init_plain((ushort)0x216);

//                                                    p.WriteUInt32((uint)UtilTime.GetSystemTimeAsUnix());
//                                                    p.WriteUInt32(1); // Count

//                                                    p.WriteByte(item.type);
//                                                    p.WriteUInt32(item._typeid);
//                                                    p.WriteInt32(item.id);
//                                                    p.WriteUInt32(item.flag_time);
//                                                    p.WriteStruct(item.stat, Marshal.SizeOf(new stItem.item_stat()));
//                                                    p.WriteUInt32((item.c[3] > 0) ? item.c[3] : item.c[0]);
//                                                    p.WriteZero(25);

//                                                    packet_func_sv.session_send(p,
//                                                        _session, 1);

//                                                }
//                                                else
//                                                {
//                                                    _smp.message_pool.push(new message("[room::requestChangePlayerItemRoom][Log][WARNING] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] nao conseguiu achar o ClubSet\"CV1\"[ID=" + Convert.ToString(item.id) + "] que acabou de adicionar para ele. Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));
//                                                }

//                                            }
//                                            else
//                                            {
//                                                _smp.message_pool.push(new message("[room::requestChangePlayerItemRoom][Log][WARNING] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] nao conseguiu adicionar o ClubSet[TYPEID=" + Convert.ToString(item._typeid) + "] para ele. Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));
//                                            }

//                                        }
//                                        else
//                                        {
//                                            _smp.message_pool.push(new message("[room::requestChangePlayerItemRoom][Log][WARNING] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] nao conseguiu inicializar o ClubSet[TYPEID=" + Convert.ToString(bi._typeid) + "] para ele. Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));
//                                        }
//                                    }
//                                }

//                                // Ball(Comet)
//                                if (_cpir.ball != 0
//                                    && (pWi = _session.m_pi.findWarehouseItemByTypeid(_cpir.ball)) != null
//                                    && sIff.getInstance().getItemGroupIdentify(pWi._typeid) == sIff.getInstance().BALL)
//                                {

//                                    _session.m_pi.ei.comet = pWi;
//                                    _session.m_pi.ue.ball_typeid = _cpir.ball; // Ball(Comet) é o typeid que o cliente passa

//                                    // Verifica se a Bola pode ser equipada
//                                    if (_session.checkBallEquiped(_session.m_pi.ue))
//                                    {
//                                        _cpir.ball = _session.m_pi.ue.ball_typeid;
//                                    }

//                                    // Update ON DB
//                                    NormalManagerDB.add(0,
//                                        new CmdUpdateBallEquiped(_session.m_pi.uid, _cpir.ball),
//                                        SQLDBResponse, this);

//                                }
//                                else if (_cpir.ball == 0)
//                                { // Bola 0 coloca a bola padrão para ele, se for premium user coloca a bola de premium user

//                                    // Zera para equipar a bola padrão
//                                    _session.m_pi.ei.comet = null;
//                                    _session.m_pi.ue.ball_typeid = 0;

//                                    // Verifica se a Bola pode ser equipada (Coloca para equipar a bola padrão
//                                    if (_session.checkBallEquiped(_session.m_pi.ue))
//                                    {
//                                        _cpir.ball = _session.m_pi.ue.ball_typeid;
//                                    }

//                                    // Update ON DB
//                                    NormalManagerDB.add(0,
//                                        new CmdUpdateBallEquiped(_session.m_pi.uid, _cpir.ball),
//                                        SQLDBResponse, this);

//                                }
//                                else
//                                {

//                                    error = (pWi == null ? 2 : 3);

//                                    pWi = _session.m_pi.findWarehouseItemByTypeid(DEFAULT_COMET_TYPEID);

//                                    if (pWi != null)
//                                    {

//                                        _smp.message_pool.push(new message("[room::requestChangePlayerItemRoom][Log][WARNING] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou trocar a Ball[TYPEID=" + Convert.ToString(_cpir.ball) + "] para comecar o jogo, mas deu Error[VALUE=" + Convert.ToString(error) + "], colocando a Ball Padrao do Player. Hacker ou Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));

//                                        _session.m_pi.ei.comet = pWi;
//                                        _cpir.ball = _session.m_pi.ue.ball_typeid = pWi._typeid;

//                                        // Update ON DB
//                                        NormalManagerDB.add(0,
//                                            new CmdUpdateBallEquiped(_session.m_pi.uid, _cpir.ball),
//                                            SQLDBResponse, this);

//                                    }
//                                    else
//                                    {

//                                        _smp.message_pool.push(new message("[room::requestChangePlayerItemRoom][Log][WARNING] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou trocar a Ball[TYPEID=" + Convert.ToString(_cpir.ball) + "] para comecar o jogo, mas deu Error[VALUE=" + Convert.ToString(error) + "], ele nao tem a Ball Padrao, adiciona a Ball pardrao para ele. Hacker ou Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));

//                                        BuyItem bi = new BuyItem();
//                                        stItem item = new stItem();

//                                        bi.id = -1;
//                                        bi._typeid = DEFAULT_COMET_TYPEID;
//                                        bi.qntd = 1;

//                                        item_manager.initItemFromBuyItem(_session.m_pi,
//                                           ref item, bi, false, 0, 0, 1);

//                                        if (item._typeid != 0)
//                                        {

//                                            if ((_cpir.ball = item_manager.addItem(item,
//                                                _session, 2, 0)) != item_manager.RetAddItem.TYPE.T_ERROR)
//                                            {

//                                                // Equipa a Ball padrao
//                                                pWi = _session.m_pi.findWarehouseItemById(_cpir.ball);

//                                                if (pWi != null)
//                                                {

//                                                    _session.m_pi.ei.comet = pWi;
//                                                    _session.m_pi.ue.ball_typeid = pWi._typeid;

//                                                    // Update ON DB
//                                                    NormalManagerDB.add(0,
//                                                        new CmdUpdateBallEquiped(_session.m_pi.uid, _cpir.ball),
//                                                        SQLDBResponse, this);

//                                                    // Update ON GAME
//                                                    p.init_plain((ushort)0x216);

//                                                    p.WriteUInt32((uint)UtilTime.GetSystemTimeAsUnix());
//                                                    p.WriteUInt32(1); // Count

//                                                    p.WriteByte(item.type);
//                                                    p.WriteUInt32(item._typeid);
//                                                    p.WriteInt32(item.id);
//                                                    p.WriteUInt32(item.flag_time);
//                                                    p.WriteStruct(item.stat, Marshal.SizeOf(new stItem.item_stat()));
//                                                    p.WriteUInt32((item.c[3] > 0) ? item.c[3] : item.c[0]);
//                                                    p.WriteZero(25);

//                                                    packet_func_sv.session_send(p,
//                                                        _session, 1);

//                                                }
//                                                else
//                                                {
//                                                    _smp.message_pool.push(new message("[room::requestChangePlayerItemRoom][Log][WARNING] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] nao conseguiu achar a Ball[ID=" + Convert.ToString(item.id) + "] que acabou de adicionar para ele. Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));
//                                                }

//                                            }
//                                            else
//                                            {
//                                                _smp.message_pool.push(new message("[room::requestChangePlayerItemRoom][Log][WARNING] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] nao conseguiu adicionar a Ball[TYPEID=" + Convert.ToString(item._typeid) + "] para ele. Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));
//                                            }

//                                        }
//                                        else
//                                        {
//                                            _smp.message_pool.push(new message("[channel::requestChangePlayerItemRoom][Log][WARNING] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] nao conseguiu inicializar a Ball[TYPEID=" + Convert.ToString(bi._typeid) + "] para ele. Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));
//                                        }
//                                    }
//                                }

//                                // Verifica se o Mascot Equipado acabou o tempo
//                                if (_session.m_pi.ue.mascot_id != 0 && _session.m_pi.ei.mascot_info != null)
//                                {

//                                    var m_it = _session.m_pi.findUpdateItemByTypeidAndType(_session.m_pi.ue.mascot_id, UpdateItem.UI_TYPE.MASCOT);

//                                    if (m_it != null)
//                                    {

//                                        // Desequipa o Mascot que acabou o tempo dele
//                                        _session.m_pi.ei.mascot_info = null;
//                                        _session.m_pi.ue.mascot_id = 0;

//                                        //NormalManagerDB.add(0,
//                                        //    new CmdUpdateMascotEquiped(_session.m_pi.uid, 0),
//                                        //    SQLDBResponse, this);

//                                        // Update on GAME se não o cliente continua com o mascot equipado
//                                        packet_func_sv.session_send(packet_func_sv.pacote04B(_session,
//                                          (byte)ChangePlayerItemRoom.TYPE_CHANGE.TC_MASCOT,
//                                           0),
//                                           _session, 0);

//                                    }
//                                }

//                                // Começa jogo
//                                startGame(_session);

//                                break;
//                            }
//                            default:
//                        throw new exception("[room::requestChangePlayerItemRoom][Error] sala[NUMERO=" + Convert.ToString(getNumero()) + "] type desconhecido.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.CHANNEL,
//                            13, 1));
//                        }

//                        updatePlayerInfo(_session);

//                }
//            catch (exception e)
//            {

//                _smp.message_pool.push(new message("[room::requestChangePlayerItemRoom][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

//                packet_func_sv.pacote04B(p,
//                    _session, _cpir.type,
//                    (STDA_SOURCE_ERROR_DECODE(e.getCodeError()) == STDA_ERROR_TYPE.ROOM ? STDA_SYSTEM_ERROR_DECODE(e.getCodeError()) : 1));
//                packet_func_sv.session_send(p,
//                    _session, 0);
//            }
//        }

//        // Personal Shop
//        public void requestOpenEditSaleShop(Player _session, Packet _packet)
//        {
//            if (!_session.GetState())
//            {
//                throw new exception("[room::" + (("request" + "OpenEditSaleShop")) + "][Error] Player nao esta connectado", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                    12, 0));
//            }
//            if (_packet == null)
//            {
//                throw new exception("[room::request" + "OpenEditSaleShop" + "][Error] _packet is nullptr", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                    12, 0));
//            }

//            PangyaBinaryWriter p = new PangyaBinaryWriter();

//            try
//            {

//                if (m_personal_shop.openShopToEdit(_session, ref p))
//                {
//                    packet_func_sv.room_broadcast(this,
//                        p, 1);
//                }

//            }
//            catch (exception e)
//            {

//                _smp.message_pool.push(new message("[room::requestOpenEditSaleShop][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
//            }
//        }

//        public void requestCloseSaleShop(Player _session, Packet _packet)
//        {
//            if (!_session.GetState())
//            {
//                throw new exception("[room::" + (("request" + "CloseSaleShop")) + "][Error] Player nao esta connectado", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                    12, 0));
//            }
//            if (_packet == null)
//            {
//                throw new exception("[room::request" + "CloseSaleShop" + "][Error] _packet is nullptr", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                    12, 0));
//            }

//            PangyaBinaryWriter p = new PangyaBinaryWriter();

//            try
//            {

//                if (m_personal_shop.closeShop(_session, ref p))
//                {
//                    packet_func_sv.room_broadcast(this,
//                        p, 1);
//                }

//            }
//            catch (exception e)
//            {

//                _smp.message_pool.push(new message("[room::requestCloseSaleShop][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
//            }
//        }

//        public void requestChangeNameSaleShop(Player _session, Packet _packet)
//        {
//            if (!_session.GetState())
//            {
//                throw new exception("[room::" + (("request" + "ChangeNameSaleShop")) + "][Error] Player nao esta connectado", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                    12, 0));
//            }
//            if (_packet == null)
//            {
//                throw new exception("[room::request" + "ChangeNameSaleShop" + "][Error] _packet is nullptr", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                    12, 0));
//            }

//            PangyaBinaryWriter p = new PangyaBinaryWriter();

//            try
//            {

//                if (m_personal_shop.changeShopName(_session,
//                    _packet.ReadPStr(), p))
//                {
//                    packet_func_sv.room_broadcast(this,
//                        p, 1);
//                }

//            }
//            catch (exception e)
//            {

//                _smp.message_pool.push(new message("[room::requestChangeNameSaleShop][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
//            }
//        }

//        public void requestOpenSaleShop(Player _session, Packet _packet)
//        {
//            if (!_session.GetState())
//            {
//                throw new exception("[room::" + (("request" + "OpenSaleShop")) + "][Error] Player nao esta connectado", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                    12, 0));
//            }
//            if (_packet == null)
//            {
//                throw new exception("[room::request" + "OpenSaleShop" + "][Error] _packet is nullptr", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                    12, 0));
//            }

//            try
//            {

//                m_personal_shop.openShop(_session, _packet);

//            }
//            catch (exception e)
//            {

//                _smp.message_pool.push(new message("[room::requestOpenSaleShop][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
//            }
//        }

//        public void requestVisitCountSaleShop(Player _session, Packet _packet)
//        {
//            if (!_session.GetState())
//            {
//                throw new exception("[room::" + (("request" + "VisitCountSaleShop")) + "][Error] Player nao esta connectado", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                    12, 0));
//            }
//            if (_packet == null)
//            {
//                throw new exception("[room::request" + "VisitCountSaleShop" + "][Error] _packet is nullptr", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                    12, 0));
//            }

//            try
//            {

//                m_personal_shop.visitCountShop(_session);

//            }
//            catch (exception e)
//            {

//                _smp.message_pool.push(new message("[room::requestVisitCount][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
//            }
//        }

//        public void requestPangSaleShop(Player _session, Packet _packet)
//        {
//            if (!_session.GetState())
//            {
//                throw new exception("[room::" + (("request" + "PangSaleShop")) + "][Error] Player nao esta connectado", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                    12, 0));
//            }
//            if (_packet == null)
//            {
//                throw new exception("[room::request" + "PangSaleShop" + "][Error] _packet is nullptr", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                    12, 0));
//            }

//            try
//            {

//                m_personal_shop.pangShop(_session);

//            }
//            catch (exception e)
//            {

//                _smp.message_pool.push(new message("[room::requestPangSaleShop][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
//            }
//        }

//        public void requestCancelEditSaleShop(Player _session, Packet _packet)
//        {
//            if (!_session.GetState())
//            {
//                throw new exception("[room::" + (("request" + "CancelEditSaleShop")) + "][Error] Player nao esta connectado", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                    12, 0));
//            }
//            if (_packet == null)
//            {
//                throw new exception("[room::request" + "CancelEditSaleShop" + "][Error] _packet is nullptr", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                    12, 0));
//            }

//            PangyaBinaryWriter p = new PangyaBinaryWriter();

//            try
//            {

//                if (m_personal_shop.cancelEditShop(_session, ref p))
//                {
//                    packet_func_sv.room_broadcast(this,
//                        p, 1);
//                }

//            }
//            catch (exception e)
//            {

//                _smp.message_pool.push(new message("[room::reuqestCancelEditSaleShop][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
//            }
//        }

//        public void requestViewSaleShop(Player _session, Packet _packet)
//        {
//            if (!_session.GetState())
//            {
//                throw new exception("[room::" + (("request" + "ViewSaleShop")) + "][Error] Player nao esta connectado", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                    12, 0));
//            }
//            if (_packet == null)
//            {
//                throw new exception("[room::request" + "ViewSaleShop" + "][Error] _packet is nullptr", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                    12, 0));
//            }

//            try
//            {

//                m_personal_shop.viewShop(_session, _packet.ReadUInt32());

//            }
//            catch (exception e)
//            {

//                _smp.message_pool.push(new message("[room::requestViewSaleShop][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
//            }
//        }

//        public void requestCloseViewSaleShop(Player _session, Packet _packet)
//        {
//            if (!_session.GetState())
//            {
//                throw new exception("[room::" + (("request" + "CloseViewSaleShop")) + "][Error] Player nao esta connectado", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                    12, 0));
//            }
//            if (_packet == null)
//            {
//                throw new exception("[room::request" + "CloseViewSaleShop" + "][Error] _packet is nullptr", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                    12, 0));
//            }

//            try
//            {

//                m_personal_shop.closeViewShop(_session, _packet.ReadUInt32());

//            }
//            catch (exception e)
//            {

//                _smp.message_pool.push(new message("[room::requestCloseViewSaleShop][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
//            }
//        }

//        public void requestBuyItemSaleShop(Player _session, Packet _packet)
//        {
//            if (!_session.GetState())
//            {
//                throw new exception("[room::" + (("request" + "BuyItemSaleShop")) + "][Error] Player nao esta connectado", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                    12, 0));
//            }
//            if (_packet == null)
//            {
//                throw new exception("[room::request" + "BuyItemSaleShop" + "][Error] _packet is nullptr", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                    12, 0));
//            }

//            try
//            {

//                m_personal_shop.buyInShop(_session, _packet);

//            }
//            catch (exception e)
//            {

//                _smp.message_pool.push(new message("[room::requestBuyItemSaleShop][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
//            }
//        }

//        // Room Wait
//        public void requestToggleAssist(Player _session, Packet _packet)
//        {
//            if (!_session.GetState())
//            {
//                throw new exception("[room::" + (("request" + "ToggleAssist")) + "][Error] Player nao esta connectado", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                    12, 0));
//            }
//            if (_packet == null)
//            {
//                throw new exception("[room::request" + "ToggleAssist" + "][Error] _packet is nullptr", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                    12, 0));
//            }

//            PangyaBinaryWriter p = new PangyaBinaryWriter();

//            try
//            {

//                stItem item = new stItem();

//                var rt = item_manager.RetAddItem.TYPE.T_INIT_VALUE;

//                item.type = 2;
//                item.id = -1;
//                item._typeid = ASSIST_ITEM_TYPEID;

//                var pWi = _session.m_pi.findWarehouseItemByTypeid(ASSIST_ITEM_TYPEID);

//                if (pWi == null)
//                { // Não tem ativa o Assist
//                    item.qntd = 1;
//                    item.c[0] = 1;

//                    if ((rt = item_manager.addItem(item,
//                        _session, 0, 0)) < 0)
//                    {
//                        throw new exception("[room::requestToggleAssist][Error] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou ativar o Assist[TYPEID=" + Convert.ToString(ASSIST_ITEM_TYPEID) + "], mas nao conseguiu adicionar o item. Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                            1, 0x5200801));
//                    }
//                }
//                else
//                { // Já tem, Desativa

//                    item.id = pWi.id;
//                    item.qntd = (pWi.c[0] <= 0) ? 1 : pWi.c[0];
//                    item.c[0] = (short)item.qntd * -1;

//                    if (item_manager.removeItem(item, _session) <= 0)
//                    {
//                        throw new exception("[room::requestToggleAssist][Error] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou desativar o Assist[TYPEID=" + Convert.ToString(ASSIST_ITEM_TYPEID) + "], mas nao conseguiu remover o item. Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                            2, 0x5200802));
//                    }

//                }

//                // UPDATE ON GAME
//                if (rt != item_manager.RetAddItem.TYPE.T_SUCCESS_PANG_AND_EXP_AND_CP_POUCH)
//                {

//                    p.init_plain((ushort)0x216);

//                    p.WriteUInt32((uint)UtilTime.GetSystemTimeAsUnix());
//                    p.WriteUInt32(1); // Count

//                    p.WriteByte(item.type);
//                    p.WriteUInt32(item._typeid);
//                    p.WriteInt32(item.id);
//                    p.WriteUInt32(item.flag_time);
//                    p.WriteStruct(item.stat, Marshal.SizeOf(new stItem.item_stat()));
//                    p.WriteUInt32((item.c[3] > 0) ? item.c[3] : item.c[0]);
//                    p.WriteZero(25);

//                    packet_func_sv.session_send(p,
//                        _session, 1);
//                }

//                // Resposta ao Toggle Assist
//                p.init_plain((ushort)0x26A);

//                p.WriteUInt32(0); // OK

//                packet_func_sv.session_send(p,
//                    _session, 1);

//            }
//            catch (exception e)
//            {

//                _smp.message_pool.push(new message("[room::requestToggleAssist][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

//                p.init_plain((ushort)0x26A);

//                p.WriteUInt32((STDA_SOURCE_ERROR_DECODE(e.getCodeError()) == STDA_ERROR_TYPE.ROOM) ? STDA_SYSTEM_ERROR_DECODE(e.getCodeError()) : 0x5200800);

//                packet_func_sv.session_send(p,
//                    _session, 1);
//            }
//        }

//        public void requestChangeTeam(Player _session, Packet _packet)
//        {
//            if (!_session.GetState())
//            {
//                throw new exception("[room::" + (("request" + "ChangeTeam")) + "][Error] Player nao esta connectado", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                    12, 0));
//            }
//            if (_packet == null)
//            {
//                throw new exception("[room::request" + "ChangeTeam" + "][Error] _packet is nullptr", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                    12, 0));
//            }

//            PangyaBinaryWriter p = new PangyaBinaryWriter();

//            try
//            {

//                byte team = _packet.ReadByte();

//                PlayerRoomInfoEx pPri = getPlayerInfo(_session);

//                if (pPri == null)
//                {
//                    throw new exception("[room::requestChangeTeam][Error] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou trocar o team(time) na sala[NUMERO=" + Convert.ToString(m_ri.numero) + "], mas a sala nao tem o info do Player. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                        1505, 0));
//                }

//                if (m_teans.Count < 2)
//                {
//                    throw new exception("[room::requestChangeTeam][Error] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou trocar o team(time) na sala[NUMERO=" + Convert.ToString(m_ri.numero) + "], mas a sala nao tem teans(times) suficiente. Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                        1506, 0));
//                }

//                // Sai do outro team(time) se ele estiver
//                try
//                {

//                    m_teans[pPri.state_flag.team].deletePlayer(_session, 3);

//                }
//                catch (exception e)
//                {

//                    _smp.message_pool.push(new message("[room::requestChangeTeam][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
//                }

//                // Add o Player ao (team)time
//                m_teans[team].addPlayer(_session);

//                pPri.state_flag.team = team; // ~pri->state_flag.team;

//                PangyaBinaryWriter p = new PangyaBinaryWriter((ushort)0x7D);

//                p.WriteUInt32(_session.m_oid);

//                p.WriteByte(team);

//                packet_func_sv.room_broadcast(this,
//                    p, 0);

//            }
//            catch (exception e)
//            {

//                _smp.message_pool.push(new message("[room::requestChangeTeam][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
//            }
//        }

//        // Request Game
//        public virtual bool requestStartGame(Player _session, Packet _packet)
//        {
//            if (!_session.GetState())
//            {
//                throw new exception("[room::" + (("request" + "StartGame")) + "][Error] Player nao esta connectado", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                    12, 0));
//            }
//            if (_packet == null)
//            {
//                throw new exception("[room::request" + "StartGame" + "][Error] _packet is nullptr", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                    12, 0));
//            }

//            PangyaBinaryWriter p = new PangyaBinaryWriter();

//            bool ret = true;

//            try
//            {

//                if (m_ri.master != _session.m_pi.uid)
//                {
//                    throw new exception("[room::requestStartGame][Error] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou comecar o jogo na sala[NUMERO=" + Convert.ToString(m_ri.numero) + "], mas ele nao eh o master da sala. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                        1, 0x5900201));
//                }

//                // Verifica se já tem um jogo inicializado e lança error se tiver, para o cliente receber uma resposta
//                if (m_pGame != null)
//                {
//                    throw new exception("[room::requestStartGame][Error] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou comecar o jogo na sala[NUMERO=" + Convert.ToString(m_ri.numero) + "], mas ja tem um jogo inicializado. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                        8, 0x5900202));
//                }

//                // Verifica se todos estão prontos se não da erro
//                if (!isAllReady())
//                {
//                    throw new exception("[room::requestStartGame][Error] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou comecar o jogo na sala[NUMERO=" + Convert.ToString(m_ri.numero) + ", MASTER=" + Convert.ToString(m_ri.master) + "], mas nem todos jogadores estao prontos. Hacker ou Bug.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                        8, 0x5900202));
//                }

//                // Coloquei para verificar se a flag de Bot tourney não está ativo verifica o resto das condições
//                if (!m_bot_tourney
//                    && v_sessions.Count == 1
//                    && m_ri.tipo != RoomInfo.PRACTICE
//                    && m_ri.tipo != RoomInfo.GRAND_PRIX
//                    && m_ri.tipo != RoomInfo.GRAND_ZODIAC_INT
//                    && m_ri.tipo != RoomInfo.GRAND_ZODIAC_ADV
//                    && m_ri.tipo != RoomInfo.GRAND_ZODIAC_PRACTICE)
//                {
//                    throw new exception("[room::requestStartGame][Error] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou comecar o jogo na sala[NUMERO=" + Convert.ToString(m_ri.numero) + "], mas nao tem quantidade de jogadores suficiente para da comecar. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                        1, 0x5900202));
//                }

//                // Match
//                if (m_ri.tipo == (byte)RoomInfo.TIPO.MATCH)
//                {

//                    if (m_teans.empty())
//                    {
//                        throw new exception("[room::requestStartGame][Error] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou comecar o Match na sala[NUMERO=" + Convert.ToString(m_ri.numero) + "], mas o vector do teans esta vazio. Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                            2, 0x5900202));
//                    }

//                    if (m_teans.Count == 1)
//                    {
//                        throw new exception("[room::requestStartGame][Error] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou comecar o Match na sala[NUMERO=" + Convert.ToString(m_ri.numero) + "], mas o vector do teans só tem um team. Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                            3, 0x5900202));
//                    }

//                    if (v_sessions.Count % 2 == 1)
//                    {
//                        throw new exception("[room::requestStartGame][Error] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou comecar o Match na sala[NUMERO=" + Convert.ToString(m_ri.numero) + "], mas o numero de jogadores na sala eh impar. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                            4, 0x5900202));
//                    }

//                    if (v_sessions.Count == 2 && (m_teans[0].getNumPlayers() == 0 || m_teans[1].getNumPlayers() == 0))
//                    {
//                        throw new exception("[room::requestStartGame][Error] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou comecar o Match na sala[NUMERO=" + Convert.ToString(m_ri.numero) + "], mas um team nao tem jogador. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                            5, 0x5900202));
//                    }

//                    if (v_sessions.Count == 4 && (m_teans[0].getNumPlayers() < 2 || m_teans[1].getNumPlayers() < 2))
//                    {
//                        throw new exception("[room::requestStartGame][Error] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou comecar o Match na sala[NUMERO=" + Convert.ToString(m_ri.numero) + "], mas um team nao tem jogador suficiente. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                            6, 0x5900202));
//                    }

//                    if (m_ri.max_Player == 4 && v_sessions.Count < 4)
//                    {
//                        throw new exception("[room::requestStartGame][Error] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou comecar o Match na sala[NUMERO=" + Convert.ToString(m_ri.numero) + "], mas o max Player sala eh 4, mas nao tem os 4 jogadores na sala. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                            7, 0x5900202));
//                    }
//                }

//                // Guild Battle
//                if (m_ri.tipo == (byte)RoomInfo.TIPO.GUILD_BATTLE)
//                {

//                    if (v_sessions.Count % 2 == 1)
//                    {
//                        throw new exception("[room::requestStartGame][Error] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou comecar o Guild Battle na sala[NUMERO=" + Convert.ToString(m_ri.numero) + "], mas o numero de jogadores na sala eh impar. Hacker ou Bug.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                            9, 0x5900202));
//                    }

//                    var error_check = m_guild_manager.isGoodToStart();

//                    if (error_check <= 0)
//                    {

//                        switch (error_check)
//                        {
//                            case 0: // Não tem duas guilds na sala
//                                throw new exception("[room::requestStartGame][Error] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou comecar o Guild Battle na sala[NUMERO=" + Convert.ToString(m_ri.numero) + "], mas nao tem guilds suficientes para comecar o jogo. Hacker ou Bug.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                                    10, 0x5900202));
//                                break;
//                            case -1: // Não tem o mesmo número de jogadores na sala as duas guilds
//                                throw new exception("[room::requestStartGame][Error] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou comecar o Guild Battle na sala[NUMERO=" + Convert.ToString(m_ri.numero) + "], mas as duas guilds nao tem o mesmo numero de jogadores na sala. Hacker ou Bug.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                                    11, 0x5900202));
//                                break;
//                            case -2: // Uma das Guilds ou as duas não tem 2 jogadores
//                                throw new exception("[room::requestStartGame][Error] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou comecar o Guild Battle na sala[NUMERO=" + Convert.ToString(m_ri.numero) + "], mas uma ou as duas guilds tem menos que 2 jogadores na sala. Hacker ou Bug.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                                    12, 0x5900202));
//                                break;
//                        }
//                    }
//                }

//                // Chip-in Practice
//                if (m_ri.tipo == (byte)RoomInfo.TIPO.GRAND_ZODIAC_PRACTICE)
//                {

//                    var pTicket = _session.m_pi.findWarehouseItemByTypeid(CHIP_IN_PRACTICE_TICKET_TYPEID);

//                    if (pTicket == null)
//                    {
//                        throw new exception("[room::requestStartGame][Error] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou comecar o Chip-in Practice na sala[NUMERO=" + Convert.ToString(m_ri.numero) + "], mas ele nao tem ticket[TYPEID=" + Convert.ToString(CHIP_IN_PRACTICE_TICKET_TYPEID) + "] do Chip-in Practice para comecar o jogo.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                            50, 500017));
//                    }

//                    if (pTicket.c[0] < 1)
//                    {
//                        throw new exception("[room::requestStartGame][Error] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou comecar o Chip-in Practice na sala[NUMERO=" + Convert.ToString(m_ri.numero) + "], mas ele nao tem ticket[TYPEID=" + Convert.ToString(CHIP_IN_PRACTICE_TICKET_TYPEID) + ", ID=" + Convert.ToString(pTicket.id) + ", QNTD=" + Convert.ToString(pTicket.c[0]) + "] do Chip-in Practice suficiente para comecar o jogo. Ticket necessario \"1\".", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                            51, 500017));
//                    }

//                    stItem item = new stItem() { id = 0u };

//                    item.type = 2;
//                    item.id = pTicket.id;
//                    item._typeid = pTicket._typeid;
//                    item.qntd = 1;
//                    item.c[0] = (short)item.qntd * -1;

//                    // UPDATE ON SERVER AND DB
//                    if (item_manager.removeItem(item, _session) <= 0)
//                    {
//                        throw new exception("[room::requestStartGame][Error] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou comecar o Chip-in Practice na sala[NUMERO=" + Convert.ToString(m_ri.numero) + "], mas nao conseguiu deletar o Ticket[TYPEID=" + Convert.ToString(item._typeid) + ", ID=" + Convert.ToString(item.id) + "]. Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                            52, 500017));
//                    }

//                    // UPDATE ON GAME
//                    p.init_plain((ushort)0x216);

//                    p.WriteUInt32((uint)UtilTime.GetSystemTimeAsUnix());
//                    p.WriteUInt32(1); // Count;

//                    p.WriteByte(item.type);
//                    p.WriteUInt32(item._typeid);
//                    p.WriteInt32(item.id);
//                    p.WriteUInt32(item.flag_time);
//                    p.WriteStruct(item.stat, Marshal.SizeOf(new stItem.item_stat()));
//                    p.WriteUInt32((item.c[3] > 0) ? item.c[3] : item.c[0]);
//                    p.WriteZero(25);

//                    packet_func_sv.session_send(p,
//                        _session, 1);
//                }

//                if (m_ri.course >= 0x7Fu)
//                {

//                    // Special Shuffle Course
//                    if (m_ri.tipo == (byte)RoomInfo.TIPO.SPECIAL_SHUFFLE_COURSE && m_ri.modo == Hole.eMODO.M_SHUFFLE_COURSE)
//                    {

//                        m_ri.course = RoomInfo.eCOURSE(0x80 | RoomInfo.eCOURSE.CHRONICLE_1_CHAOS);

//                    }
//                    else
//                    { // Random normal

//                        Lottery lottery = new Lottery((uint64_t)this);

//                        foreach (var el in sIff.getInstance().getCourse())
//                        {

//                            var course_id = sIff.getInstance().getItemIdentify(el.Value._typeid);

//                            if (course_id != 17 && course_id != 0x40)
//                            {
//                                lottery.push(100, course_id);
//                            }
//                        }

//                        var lc = lottery.spinRoleta();

//                        if (lc != null)
//                        {
//                            m_ri.course = RoomInfo.eCOURSE(0x80u | (byte)lc.value);
//                        }
//                    }
//                }

//                RateValue rv = new RateValue();

//                // Att Exp rate, e Pang rate, que começou o jogo
//                //if (sgs::gs != nullptr) {

//                rv.exp = m_ri.rate_exp = Program.gs.getInfo().rate.exp;
//                rv.pang = m_ri.rate_pang = Program.gs.getInfo().rate.pang;

//                // Angel Event
//                m_ri.angel_event = Program.gs.getInfo().rate.angel_event;

//                rv.clubset = Program.gs.getInfo().rate.club_mastery;
//                rv.rain = Program.gs.getInfo().rate.chuva;
//                rv.treasure = Program.gs.getInfo().rate.treasure;

//                rv.persist_rain = 0; // Persist rain flag isso é feito na classe game
//                                     //}

//                switch (m_ri.tipo)
//                {
//                    case (byte)RoomInfo.TIPO.STROKE:
//                        m_pGame = new Versus(v_sessions,
//                            m_ri, rv, m_ri.channel_rookie);
//                        break;
//                    case (byte)RoomInfo.TIPO.MATCH:
//                        m_pGame = new Match(v_sessions,
//                            m_ri, rv, m_ri.channel_rookie,
//                            m_teans);
//                        break;
//                    case (byte)RoomInfo.TIPO.PANG_BATTLE: // Ainda não está feio, usa o  Versus Normal
//                        m_pGame = new PangBattle(v_sessions,
//                            m_ri, rv, m_ri.channel_rookie);
//                        break;
//                    case (byte)RoomInfo.TIPO.APPROCH:
//                        m_pGame = new Approach(v_sessions,
//                            m_ri, rv, m_ri.channel_rookie);
//                        break;
//                    case (byte)RoomInfo.TIPO.PRACTICE:
//                        m_pGame = new Practice(v_sessions,
//                            m_ri, rv, m_ri.channel_rookie);
//                        break;
//                    case (byte)RoomInfo.TIPO.TOURNEY:
//                        m_pGame = new Tourney(v_sessions,
//                            m_ri, rv, m_ri.channel_rookie);
//                        break;
//                    case (byte)RoomInfo.TIPO.SPECIAL_SHUFFLE_COURSE:
//                        m_pGame = new SpecialShuffleCourse(v_sessions,
//                            m_ri, rv, m_ri.channel_rookie);
//                        break;
//                    case (byte)RoomInfo.TIPO.GUILD_BATTLE:
//                        m_pGame = new GuildBattle(v_sessions,
//                            m_ri, rv, m_ri.channel_rookie,
//                            m_guild_manager);
//                        break;
//                    case (byte)RoomInfo.TIPO.GRAND_ZODIAC_PRACTICE:
//                        m_pGame = new ChipInPractice(v_sessions,
//                            m_ri, rv, m_ri.channel_rookie);
//                        break;
//                    case (byte)RoomInfo.TIPO.GRAND_ZODIAC_INT:
//                    case (byte)RoomInfo.TIPO.GRAND_ZODIAC_ADV:
//                        m_pGame = new GrandZodiac(v_sessions,
//                            m_ri, rv, m_ri.channel_rookie);
//                        break;
//                    default:
//                        m_pGame = new Practice(v_sessions,
//                            m_ri, rv, m_ri.channel_rookie);
//                        break;
//                }

//                // Update Room State
//                m_ri.state = 0; // IN GAME

//                // Update on GAME
//                //packet_func_sv::pacote04A(p, m_ri, -1);

//                //packet_func_sv::room_broadcast(*this, p, 1);

//                p.init_plain((ushort)0x230);

//                packet_func_sv.room_broadcast(this,
//                    p, 1);

//                p.init_plain((ushort)0x231);

//                packet_func_sv.room_broadcast(this,
//                    p, 1);

//                uint rate_pang = Program.gs.getInfo().rate.pang;

//                p.init_plain((ushort)0x77);

//                p.WriteUInt32(rate_pang); // Rate Pang

//                packet_func_sv.room_broadcast(this,
//                    p, 1);

//            }
//            catch (exception e)
//            {

//                _smp.message_pool.push(new message("[room::requestStartGame][Error] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

//                // Error
//                p.init_plain((ushort)0x253);

//                p.WriteUInt32((STDA_SOURCE_ERROR_DECODE(e.getCodeError() == STDA_ERROR_TYPE.ROOM)) ? STDA_SYSTEM_ERROR_DECODE(e.getCodeError()) : 0x5900200);

//                packet_func_sv.session_send(p,
//                    _session, 1);

//                ret = false; // Error ao inicializar o Jogo
//            }

//            return ret;
//        }

//        public void requestInitHole(Player _session, Packet _packet)
//        {
//            if (!_session.GetState())
//            {
//                throw new exception("[room::" + (("request" + "InitHole")) + "][Error] Player nao esta connectado", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                    12, 0));
//            }
//            if (_packet == null)
//            {
//                throw new exception("[room::request" + "InitHole" + "][Error] _packet is nullptr", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                    12, 0));
//            }

//            try
//            {

//                if (m_pGame == null)
//                {
//                    throw new exception("[room::requestInitHole][Error] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou inicializer o hole do jogo na sala[NUMERO=" + Convert.ToString(m_ri.numero) + "], mas a sala nao tem nenhum jogo inicializado. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                        1, 0x5200201));
//                }

//                m_pGame.requestInitHole(_session, _packet);

//            }
//            catch (exception e)
//            {

//                _smp.message_pool.push(new message("[room::requestInitHole][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
//            }
//        }

//        public bool requestFinishLoadHole(Player _session, Packet _packet)
//        {
//            if (!_session.GetState())
//            {
//                throw new exception("[room::" + (("request" + "FinishLoadHole")) + "][Error] Player nao esta connectado", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                    12, 0));
//            }
//            if (_packet == null)
//            {
//                throw new exception("[room::request" + "FinishLoadHole" + "][Error] _packet is nullptr", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                    12, 0));
//            }

//            bool ret = false;

//            try
//            {

//                if (m_pGame == null)
//                {
//                    throw new exception("[room::requestFinishLoadHole][Error] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou finalizar carregamento do hole do jogo na sala[NUMERO=" + Convert.ToString(m_ri.numero) + "], mas a sala nao tem nenhum jogo inicializado. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                        1, 0x5200301));
//                }

//                ret = m_pGame.requestFinishLoadHole(_session, _packet);

//            }
//            catch (exception e)
//            {

//                _smp.message_pool.push(new message("[room::requestFinishLoadHole][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
//            }

//            return ret;
//        }

//        public void requestFinishCharIntro(Player _session, Packet _packet)
//        {
//            if (!_session.GetState())
//            {
//                throw new exception("[room::" + (("request" + "FinishCharIntro")) + "][Error] Player nao esta connectado", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                    12, 0));
//            }
//            if (_packet == null)
//            {
//                throw new exception("[room::request" + "FinishCharIntro" + "][Error] _packet is nullptr", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                    12, 0));
//            }

//            try
//            {

//                if (m_pGame == null)
//                {
//                    throw new exception("[room::requestFinishCharIntro][Error] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou finalizar Char Intro do jogo na sala[NUMEROR=" + Convert.ToString(m_ri.numero) + "], mas a sala nao tem nenhum jogo inicializado. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                        1, 0x5200401));
//                }

//                m_pGame.requestFinishCharIntro(_session, _packet);

//            }
//            catch (exception e)
//            {

//                _smp.message_pool.push(new message("[room::requestFinishCharIntro][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
//            }
//        }

//        public void requestFinishHoleData(Player _session, Packet _packet)
//        {
//            if (!_session.GetState())
//            {
//                throw new exception("[room::" + (("request" + "FinishHoleData")) + "][Error] Player nao esta connectado", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                    12, 0));
//            }
//            if (_packet == null)
//            {
//                throw new exception("[room::request" + "FinishHoleData" + "][Error] _packet is nullptr", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                    12, 0));
//            }

//            try
//            {

//                if (m_pGame == null)
//                {
//                    throw new exception("[room::requestFinishHoleData][Error] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou finalizar dados do hole, no jogo na sala[NUMERO=" + Convert.ToString(m_ri.numero) + "], mas a sala nao tem nenhum jogo inicializado. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                        1, 0x5201901));
//                }

//                m_pGame.requestFinishHoleData(_session, _packet);

//            }
//            catch (exception e)
//            {

//                _smp.message_pool.push(new message("[room::requestFinishHoleData][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
//            }
//        }

//        // Server enviou a resposta do InitShot para o cliente
//        public void requestInitShotSLasted(Player _session, Packet _packet)
//        {
//            if (!_session.GetState())
//            {
//                throw new exception("[room::" + (("request" + "InitShotSLasted")) + "][Error] Player nao esta connectado", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                    12, 0));
//            }
//            if (_packet == null)
//            {
//                throw new exception("[room::request" + "InitShotSLasted" + "][Error] _packet is nullptr", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                    12, 0));
//            }

//            try
//            {

//                if (m_pGame == null)
//                {
//                    throw new exception("[room::requestInitShotSLasted][Error] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] o server enviou o pacote de InitShot para o cliente, mas na sala[NUMERO=" + Convert.ToString(m_ri.numero) + "] nao tem mais nenhum jogo inicializado. Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                        1, 0x5905001));
//                }

//                m_pGame.requestInitShotSLasted(_session, _packet);

//            }
//            catch (exception e)
//            {

//                _smp.message_pool.push(new message("[room::requestInitShotSLasted][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
//            }
//        }

//        public void requestInitShot(Player _session, Packet _packet)
//        {
//            if (!_session.GetState())
//            {
//                throw new exception("[room::" + (("request" + "InitShot")) + "][Error] Player nao esta connectado", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                    12, 0));
//            }
//            if (_packet == null)
//            {
//                throw new exception("[room::request" + "InitShot" + "][Error] _packet is nullptr", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                    12, 0));
//            }

//            try
//            {

//                if (m_pGame == null)
//                {
//                    throw new exception("[room::requestInitShot][Error] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou inicializar o shot no jogo na sala[NUMERO=" + Convert.ToString(m_ri.numero) + "], mas a sala nao tem nenhum jogo inicializado. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                        1, 0x5201401));
//                }

//                m_pGame.requestInitShot(_session, _packet);

//            }
//            catch (exception e)
//            {

//                _smp.message_pool.push(new message("[room::requestInitShot][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
//            }
//        }

//        public void requestSyncShot(Player _session, Packet _packet)
//        {
//            if (!_session.GetState())
//            {
//                throw new exception("[room::" + (("request" + "SyncShot")) + "][Error] Player nao esta connectado", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                    12, 0));
//            }
//            if (_packet == null)
//            {
//                throw new exception("[room::request" + "SyncShot" + "][Error] _packet is nullptr", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                    12, 0));
//            }

//            try
//            {

//                if (m_pGame == null)
//                {
//                    throw new exception("[room::requestSyncShot][Error] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou sincronizar tacada no jogo na sala[NUMERO=" + Convert.ToString(m_ri.numero) + "], mas a sala nao tem nenhum jogo inicializado. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                        1, 0x5201501));
//                }

//                m_pGame.requestSyncShot(_session, _packet);

//            }
//            catch (exception e)
//            {

//                _smp.message_pool.push(new message("[room::requestSyncShot][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
//            }
//        }

//        public void requestInitShotArrowSeq(Player _session, Packet _packet)
//        {
//            if (!_session.GetState())
//            {
//                throw new exception("[room::" + (("request" + "InitShotArrowSeq")) + "][Error] Player nao esta connectado", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                    12, 0));
//            }
//            if (_packet == null)
//            {
//                throw new exception("[room::request" + "InitShotArrowSeq" + "][Error] _packet is nullptr", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                    12, 0));
//            }

//            try
//            {

//                if (m_pGame == null)
//                {
//                    throw new exception("[room::requestInitShotArrowSeq][Error] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou inicializar a sequencia de setas no jogo na sala[NUMERO=" + Convert.ToString(m_ri.numero) + "], mas a sala nao tem nenhum jogo inicializado, Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                        1, 0x5201601));
//                }

//                m_pGame.requestInitShotArrowSeq(_session, _packet);

//            }
//            catch (exception e)
//            {

//                _smp.message_pool.push(new message("[room::requestInitShotArrowSeq][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
//            }
//        }

//        public void requestShotEndData(Player _session, Packet _packet)
//        {
//            if (!_session.GetState())
//            {
//                throw new exception("[room::" + (("request" + "ShotEndData")) + "][Error] Player nao esta connectado", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                    12, 0));
//            }
//            if (_packet == null)
//            {
//                throw new exception("[room::request" + "ShotEndData" + "][Error] _packet is nullptr", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                    12, 0));
//            }

//            try
//            {

//                if (m_pGame == null)
//                {
//                    throw new exception("[room::requestShotEndData][Error] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou finalizar local da tacada no jogo na sala[NUMERO=" + Convert.ToString(m_ri.numero) + "], mas a sala nao tem nenhum jogo inicializado. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                        1, 0x5201701));
//                }

//                m_pGame.requestShotEndData(_session, _packet);

//            }
//            catch (exception e)
//            {

//                _smp.message_pool.push(new message("[room::requestShotEndData][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
//            }
//        }

//        public RetFinishShot requestFinishShot(Player _session, Packet _packet)
//        {
//            if (!_session.GetState())
//            {
//                throw new exception("[room::" + (("request" + "FinishShot")) + "][Error] Player nao esta connectado", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                    12, 0));
//            }
//            if (_packet == null)
//            {
//                throw new exception("[room::request" + "FinishShot" + "][Error] _packet is nullptr", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                    12, 0));
//            }

//            RetFinishShot rfs = new RetFinishShot();

//            try
//            {

//                if (m_pGame == null)
//                {
//                    throw new exception("[room::requestFinishShot][Error] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou finalizar tacada no jogo na sala[NUMERO=" + Convert.ToString(m_ri.numero) + "], mas a sala nao tem nenhum jogo inicializado. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                        1, 0x5201801));
//                }

//                rfs = m_pGame.requestFinishShot(_session, _packet);

//                if (rfs.ret > 0)
//                {

//                    // Acho que não usa mais isso então vou deixar ai, e o ret == 2 vou deixar no channel,
//                    // por que se ele for o ultimo da sala tem que excluir ela
//                    if (rfs.ret == 1)
//                    {
//                        finish_game();
//                    }

//                }

//            }
//            catch (exception e)
//            {

//                _smp.message_pool.push(new message("[room::requestFinishShot][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
//            }

//            return new RetFinishShot(rfs);
//        }

//        public void requestChangeMira(Player _session, Packet _packet)
//        {
//            if (!_session.GetState())
//            {
//                throw new exception("[room::" + (("request" + "ChangeMira")) + "][Error] Player nao esta connectado", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                    12, 0));
//            }
//            if (_packet == null)
//            {
//                throw new exception("[room::request" + "ChangeMira" + "][Error] _packet is nullptr", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                    12, 0));
//            }

//            try
//            {

//                if (m_pGame == null)
//                {
//                    throw new exception("[room::requestChangeMira][Error] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou trocar a mira no jogo na sala[NUMERO=" + Convert.ToString(m_ri.numero) + "], mas a sala nao tem nenhum jogo inicializado. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                        1, 0x5200501));
//                }

//                m_pGame.requestChangeMira(_session, _packet);

//            }
//            catch (exception e)
//            {

//                _smp.message_pool.push(new message("[room::requestChangeMira][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
//            }
//        }

//        public void requestChangeStateBarSpace(Player _session, Packet _packet)
//        {
//            if (!_session.GetState())
//            {
//                throw new exception("[room::" + (("request" + "ChangeStateBarSpace")) + "][Error] Player nao esta connectado", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                    12, 0));
//            }
//            if (_packet == null)
//            {
//                throw new exception("[room::request" + "ChangeStateBarSpace" + "][Error] _packet is nullptr", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                    12, 0));
//            }

//            try
//            {

//                if (m_pGame == null)
//                {
//                    throw new exception("[room::requestChangeStateBarSpace][Error] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou trocar estado da barra de espaco no jogo na sala[NUMERO=" + Convert.ToString(m_ri.numero) + "], mas a sala nao tem nenhum jogo inicializado. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                        1, 0x5200601));
//                }

//                m_pGame.requestChangeStateBarSpace(_session, _packet);

//            }
//            catch (exception e)
//            {

//                _smp.message_pool.push(new message("[room::requestChangeStateBarSpace][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
//            }
//        }

//        public void requestActivePowerShot(Player _session, Packet _packet)
//        {
//            if (!_session.GetState())
//            {
//                throw new exception("[room::" + (("request" + "ActivePowerShot")) + "][Error] Player nao esta connectado", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                    12, 0));
//            }
//            if (_packet == null)
//            {
//                throw new exception("[room::request" + "ActivePowerShot" + "][Error] _packet is nullptr", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                    12, 0));
//            }

//            try
//            {

//                if (m_pGame == null)
//                {
//                    throw new exception("[room::requestActivePowerShot][Error] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou ativar power shot no jogo na sala[NUMEROR=" + Convert.ToString(m_ri.numero) + "], mas a sala nao tem nenhum jogo inicializado. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                        1, 0x5200701));
//                }

//                m_pGame.requestActivePowerShot(_session, _packet);

//            }
//            catch (exception e)
//            {

//                _smp.message_pool.push(new message("[room::requestActivePowerShot][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
//            }
//        }

//        public void requestChangeClub(Player _session, Packet _packet)
//        {
//            if (!_session.GetState())
//            {
//                throw new exception("[room::" + (("request" + "ChangeClub")) + "][Error] Player nao esta connectado", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                    12, 0));
//            }
//            if (_packet == null)
//            {
//                throw new exception("[room::request" + "ChangeClub" + "][Error] _packet is nullptr", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                    12, 0));
//            }

//            try
//            {

//                if (m_pGame == null)
//                {
//                    throw new exception("[room::requestChangeClub][Error] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou trocar taco no jogo na sala[NUMERO=" + Convert.ToString(m_ri.numero) + "], mas a sala nao tem nenhum jogo inicializado. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                        1, 0x5200801));
//                }

//                m_pGame.requestChangeClub(_session, _packet);

//            }
//            catch (exception e)
//            {

//                _smp.message_pool.push(new message("[room::requestChangeClub][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
//            }
//        }

//        public void requestUseActiveItem(Player _session, Packet _packet)
//        {
//            if (!_session.GetState())
//            {
//                throw new exception("[room::" + (("request" + "UseActiveItem")) + "][Error] Player nao esta connectado", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                    12, 0));
//            }
//            if (_packet == null)
//            {
//                throw new exception("[room::request" + "UseActiveItem" + "][Error] _packet is nullptr", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                    12, 0));
//            }

//            try
//            {

//                if (m_pGame == null)
//                {
//                    throw new exception("[room::requestUseActiveItem][Error] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou usar active item no jogo na sala[NUMERO=" + Convert.ToString(m_ri.numero) + "], mas a sala nao tem nenhum jogo inicializado. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                        1, 0x5200901));
//                }

//                m_pGame.requestUseActiveItem(_session, _packet);

//            }
//            catch (exception e)
//            {

//                _smp.message_pool.push(new message("[room::requestUseActiveItem][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
//            }
//        }

//        public void requestChangeStateTypeing(Player _session, Packet _packet)
//        {
//            if (!_session.GetState())
//            {
//                throw new exception("[room::" + (("request" + "ChangeStateTypeing")) + "][Error] Player nao esta connectado", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                    12, 0));
//            }
//            if (_packet == null)
//            {
//                throw new exception("[room::request" + "ChangeStateTypeing" + "][Error] _packet is nullptr", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                    12, 0));
//            }

//            try
//            {

//                if (m_pGame == null)
//                {
//                    throw new exception("[room::requestChangeStateTypeing][Error] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou mudar estado do escrevLasto icon no jogo na sala[NUMERO=" + Convert.ToString(m_ri.numero) + "], mas a sala nao tem nenhum jogo inicializado. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                        1, 0x5201001));
//                }

//                m_pGame.requestChangeStateTypeing(_session, _packet);

//            }
//            catch (exception e)
//            {

//                _smp.message_pool.push(new message("[room::requestChangeStateTypeing][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
//            }
//        }

//        public void requestMoveBall(Player _session, Packet _packet)
//        {
//            if (!_session.GetState())
//            {
//                throw new exception("[room::" + (("request" + "MoveBall")) + "][Error] Player nao esta connectado", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                    12, 0));
//            }
//            if (_packet == null)
//            {
//                throw new exception("[room::request" + "MoveBall" + "][Error] _packet is nullptr", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                    12, 0));
//            }

//            try
//            {

//                if (m_pGame == null)
//                {
//                    throw new exception("[room::requestMoveBall][Error] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou recolocar a bola no jogo na sala[NUMERO=" + Convert.ToString(m_ri.numero) + "], mas a sala nao tem nenhum jogo inicializado. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                        1, 0x5201101));
//                }

//                m_pGame.requestMoveBall(_session, _packet);

//            }
//            catch (exception e)
//            {

//                _smp.message_pool.push(new message("[room::requestMoveBall][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
//            }
//        }

//        public void requestChangeStateChatBlock(Player _session, Packet _packet)
//        {
//            if (!_session.GetState())
//            {
//                throw new exception("[room::" + (("request" + "ChangeStateChatBlock")) + "][Error] Player nao esta connectado", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                    12, 0));
//            }
//            if (_packet == null)
//            {
//                throw new exception("[room::request" + "ChangeStateChatBlock" + "][Error] _packet is nullptr", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                    12, 0));
//            }

//            try
//            {

//                if (m_pGame == null)
//                {
//                    throw new exception("[room::requestChangeStateChatBlock][Error] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou mudar estado do chat block no jogo na sala[NUMERO=" + Convert.ToString(m_ri.numero) + "], mas a sala nao tem nenhum jogo inicializado. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                        1, 0x5201201));
//                }

//                m_pGame.requestChangeStateChatBlock(_session, _packet);

//            }
//            catch (exception e)
//            {

//                _smp.message_pool.push(new message("[room::requestChangeStateChatBlock][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
//            }
//        }

//        public void requestActiveBooster(Player _session, Packet _packet)
//        {
//            if (!_session.GetState())
//            {
//                throw new exception("[room::" + (("request" + "ActiveBooster")) + "][Error] Player nao esta connectado", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                    12, 0));
//            }
//            if (_packet == null)
//            {
//                throw new exception("[room::request" + "ActiveBooster" + "][Error] _packet is nullptr", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                    12, 0));
//            }

//            try
//            {

//                if (m_pGame == null)
//                {
//                    throw new exception("[room::requestActiveBooster][Error] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou ativar time booster no jogo na sala[NUMERO=" + Convert.ToString(m_ri.numero) + "], mas a sala nao tem nenhum jogo inicializado. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                        1, 0x5201301));
//                }

//                m_pGame.requestActiveBooster(_session, _packet);

//            }
//            catch (exception e)
//            {

//                _smp.message_pool.push(new message("[room::requestActiveBooster][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
//            }
//        }

//        public void requestActiveReplay(Player _session, Packet _packet)
//        {
//            if (!_session.GetState())
//            {
//                throw new exception("[room::" + (("request" + "ActiveReplay")) + "][Error] Player nao esta connectado", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                    12, 0));
//            }
//            if (_packet == null)
//            {
//                throw new exception("[room::request" + "ActiveReplay" + "][Error] _packet is nullptr", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                    12, 0));
//            }

//            try
//            {

//                if (m_pGame == null)
//                {
//                    throw new exception("[room::requestActiveReplay][Error] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou ativar Replay no jogo na sala[NUMERO=" + Convert.ToString(m_ri.numero) + "], mas a sala nao tem nenhum jogo inicializado. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                        1, 0x5301001));
//                }

//                m_pGame.requestActiveReplay(_session, _packet);

//            }
//            catch (exception e)
//            {

//                _smp.message_pool.push(new message("[room::requestActiveReplay][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
//            }
//        }

//        public void requestActiveCutin(Player _session, Packet _packet)
//        {
//            if (!_session.GetState())
//            {
//                throw new exception("[room::" + (("request" + "ActiveCutin")) + "][Error] Player nao esta connectado", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                    12, 0));
//            }
//            if (_packet == null)
//            {
//                throw new exception("[room::request" + "ActiveCutin" + "][Error] _packet is nullptr", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                    12, 0));
//            }

//            try
//            {

//                if (m_pGame == null)
//                {
//                    throw new exception("[room::requestActiveCutin][Error] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou ativar cutin no jogo na sala[NUMERO=" + Convert.ToString(m_ri.numero) + "], mas a sala nao tem nenhum jogo inicializado. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                        1, 0x5201701));
//                }

//                m_pGame.requestActiveCutin(_session, _packet);

//            }
//            catch (exception e)
//            {

//                _smp.message_pool.push(new message("[room::requestActiveCutin][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
//            }
//        }

//        public void requestActiveAutoCommand(Player _session, Packet _packet)
//        {
//            if (!_session.GetState())
//            {
//                throw new exception("[room::" + (("request" + "ActiveAutoCommand")) + "][Error] Player nao esta connectado", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                    12, 0));
//            }
//            if (_packet == null)
//            {
//                throw new exception("[room::request" + "ActiveAutoCommand" + "][Error] _packet is nullptr", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                    12, 0));
//            }

//            try
//            {

//                if (m_pGame == null)
//                {
//                    throw new exception("[room::requestActiveAutoCommand][Error] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou ativar Auto Command no jogo na sala[NUMEROR=" + Convert.ToString(m_ri.numero) + "], mas a sala nao tem nenhum jogo inicializado. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                        1, 0x550001));
//                }

//                m_pGame.requestActiveAutoCommand(_session, _packet);

//            }
//            catch (exception e)
//            {

//                _smp.message_pool.push(new message("[room::requestActiveAutoCommand][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
//            }
//        }

//        public void requestActiveAssistGreen(Player _session, Packet _packet)
//        {
//            if (!_session.GetState())
//            {
//                throw new exception("[room::" + (("request" + "ActiveAssistGreen")) + "][Error] Player nao esta connectado", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                    12, 0));
//            }
//            if (_packet == null)
//            {
//                throw new exception("[room::request" + "ActiveAssistGreen" + "][Error] _packet is nullptr", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                    12, 0));
//            }

//            try
//            {

//                if (m_pGame == null)
//                {
//                    throw new exception("[room::requestActiveAssistGreen][Error] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou ativar Assist Green no jogo na sala[NUMERO=" + Convert.ToString(m_ri.numero) + "], mas a sala nao tem nenhum jogo inicializado. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                        1, 0x5201801));
//                }

//                m_pGame.requestActiveAssistGreen(_session, _packet);

//            }
//            catch (exception e)
//            {

//                _smp.message_pool.push(new message("[room::requestActiveAssistGreen][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
//            }
//        }

//        // VersusBase
//        public void requestLoadGamePercent(Player _session, Packet _packet)
//        {
//            if (!_session.GetState())
//            {
//                throw new exception("[room::" + (("request" + "LoadGamePercent")) + "][Error] Player nao esta connectado", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                    12, 0));
//            }
//            if (_packet == null)
//            {
//                throw new exception("[room::request" + "LoadGamePercent" + "][Error] _packet is nullptr", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                    12, 0));
//            }

//            try
//            {

//                if (m_pGame == null)
//                {
//                    throw new exception("[room::requestLoadGamePercent][Error] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou mandar a porcentagem carregada do jogo na sala[NUMERO=" + Convert.ToString(m_ri.numero) + "], mas a sala nao tem nenhum jogo inicializado. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                        1, 0x551001));
//                }

//                m_pGame.requestLoadGamePercent(_session, _packet);

//            }
//            catch (exception e)
//            {

//                _smp.message_pool.push(new message("[room::requestLoadGamePercent][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
//            }
//        }

//        public void requestMarkerOnCourse(Player _session, Packet _packet)
//        {
//            if (!_session.GetState())
//            {
//                throw new exception("[room::" + (("request" + "MarkerOnCourse")) + "][Error] Player nao esta connectado", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                    12, 0));
//            }
//            if (_packet == null)
//            {
//                throw new exception("[room::request" + "MarkerOnCourse" + "][Error] _packet is nullptr", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                    12, 0));
//            }

//            try
//            {

//                if (m_pGame == null)
//                {
//                    throw new exception("[room::requestMarkerOnCourse][Error] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou marcar no course no jogo na sala[NUMERO=" + Convert.ToString(m_ri.numero) + "], mas a sala nao tem nenhum jogo inicializado. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                        1, 0x552001));
//                }

//                m_pGame.requestMarkerOnCourse(_session, _packet);

//            }
//            catch (exception e)
//            {

//                _smp.message_pool.push(new message("[room::requestMarkerOnCourse][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
//            }
//        }

//        public void requestStartTurnTime(Player _session, Packet _packet)
//        {
//            if (!_session.GetState())
//            {
//                throw new exception("[room::" + (("request" + "StartTurnTime")) + "][Error] Player nao esta connectado", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                    12, 0));
//            }
//            if (_packet == null)
//            {
//                throw new exception("[room::request" + "StartTurnTime" + "][Error] _packet is nullptr", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                    12, 0));
//            }

//            try
//            {

//                if (m_pGame == null)
//                {
//                    throw new exception("[room::requestStartTurnTime][Error] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou comecar o tempo do turno no jogo na sala[NUMERO=" + Convert.ToString(m_ri.numero) + "], mas a sala nao tem nenhum jogo inicializado. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                        1, 0x553001));
//                }

//                m_pGame.requestStartTurnTime(_session, _packet);

//            }
//            catch (exception e)
//            {

//                _smp.message_pool.push(new message("[room::requestStartTurnTime][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
//            }
//        }

//        public void requestUnOrPauseGame(Player _session, Packet _packet)
//        {
//            if (!_session.GetState())
//            {
//                throw new exception("[room::" + (("request" + "UnOrPauseGame")) + "][Error] Player nao esta connectado", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                    12, 0));
//            }
//            if (_packet == null)
//            {
//                throw new exception("[room::request" + "UnOrPauseGame" + "][Error] _packet is nullptr", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                    12, 0));
//            }

//            try
//            {

//                if (m_pGame == null)
//                {
//                    throw new exception("[room::requestUnOrPauseGame][Error] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou pausar ou despausar o jogo na sala[NUMERO=" + Convert.ToString(m_ri.numero) + "], mas a sala nao tem nenhum jogo inicializado. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                        1, 0x554001));
//                }

//                m_pGame.requestUnOrPause(_session, _packet);

//            }
//            catch (exception e)
//            {

//                _smp.message_pool.push(new message("[room::requestUnOrPauseGame][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
//            }
//        }

//        public bool requestLastPlayerFinishVersus(Player _session, Packet _packet)
//        {
//            if (!_session.GetState())
//            {
//                throw new exception("[room::" + (("request" + "LastPlayerFinishVersus")) + "][Error] Player nao esta connectado", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                    12, 0));
//            }
//            if (_packet == null)
//            {
//                throw new exception("[room::request" + "LastPlayerFinishVersus" + "][Error] _packet is nullptr", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                    12, 0));
//            }

//            bool ret = false;

//            try
//            {

//                if (m_pGame == null)
//                {
//                    throw new exception("[room::requestLastPlayerFinishVersus][Error] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou finalizar Versus na sala[NUMERO=" + Convert.ToString(m_ri.numero) + "], mas a sala nao tem nenhum jogo inicializado. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                        1, 0x555001));
//                }

//                // Finaliza o Versus
//                if (m_pGame.getSessions().Count > 0)
//                {

//                    if (m_pGame.finish_game(**m_pGame.getSessions().begin(), 2))
//                    {
//                        finish_game();
//                    }

//                }
//                else
//                {
//                    finish_game();
//                }

//                ret = true;

//            }
//            catch (exception e)
//            {

//                _smp.message_pool.push(new message("[room::requestLastPlayerFinishVersus][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
//            }

//            return ret;
//        }

//        public bool requestReplyContinueVersus(Player _session, Packet _packet)
//        {
//            if (!_session.GetState())
//            {
//                throw new exception("[room::" + (("request" + "ReplyContinueVersus")) + "][Error] Player nao esta connectado", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                    12, 0));
//            }
//            if (_packet == null)
//            {
//                throw new exception("[room::request" + "ReplyContinueVersus" + "][Error] _packet is nullptr", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                    12, 0));
//            }

//            bool ret = false;

//            try
//            {

//                byte opt = _packet.ReadByte();

//                if (m_pGame == null)
//                {
//                    throw new exception("[room::requestReplyContinueVersus][Error] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou responder se quer continuar o versus ou nao na sala[NUMERO=" + Convert.ToString(m_ri.numero) + "], mas a sala nao tem nenhum jogo inicializado. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                        1, 0x556001));
//                }

//                if (opt == 0)
//                {

//                    if (m_pGame.getSessions().Count > 0)
//                    {

//                        if (m_pGame.finish_game(**m_pGame.getSessions().begin(), 2))
//                        {
//                            finish_game();
//                        }

//                    }
//                    else
//                    {
//                        finish_game();
//                    }

//                    ret = true;

//                }
//                else if (opt == 1)
//                {
//                    m_pGame.requestReplyContinue();
//                }

//            }
//            catch (exception e)
//            {

//                _smp.message_pool.push(new message("[room::requestReplyContinueVersus][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
//            }

//            return ret;
//        }

//        // Match
//        public void requestTeamFinishHole(Player _session, Packet _packet)
//        {
//            if (!_session.GetState())
//            {
//                throw new exception("[room::" + (("request" + "TeamFinishHole")) + "][Error] Player nao esta connectado", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                    12, 0));
//            }
//            if (_packet == null)
//            {
//                throw new exception("[room::request" + "TeamFinishHole" + "][Error] _packet is nullptr", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                    12, 0));
//            }

//            try
//            {

//                if (m_pGame == null)
//                {
//                    throw new exception("[room::requestTeamFinishHole][Error] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou finalizar hole do Match na sala[NUMERO=" + Convert.ToString(m_ri.numero) + "], mas a sala nao tem nenhum jogo inicializado. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                        1, 0x562001));
//                }

//                m_pGame.requestTeamFinishHole(_session, _packet);

//            }
//            catch (exception e)
//            {

//                _smp.message_pool.push(new message("[room::requestTeamFinishHole][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
//            }
//        }

//        public void testeDegree()
//        {

//            try
//            {

//                if (m_pGame == null)
//                {
//                    throw new exception("[room::testeDegree][Error] sala[NUMERO=" + Convert.ToString(m_ri.numero) + "] nao tem um jogo inicializado. Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                        2000, 0));
//                }

//                var pgi = m_pGame.getPlayerInfo(*v_sessions.begin());

//                if (pgi == null)
//                {
//                    throw new exception("[room::testeDegree][Error] o jogo na sala[NUMERO=" + Convert.ToString(m_ri.numero) + "], nao tem o Player[UID=" + Convert.ToString((*v_sessions.begin()).m_pi.uid) + "]. Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                        2001, 0));
//                }

//                pgi.degree += 1;

//                pgi.degree %= LIMIT_DEGREE;


//                PangyaBinaryWriter p = new PangyaBinaryWriter((ushort)0x5B);

//                p.WriteByte(1);
//                p.WriteByte(0); // Flag de card de vento, aqui é a qnd diminui o vento, 1 Vento azul
//                p.WriteUInt16(pgi.degree);
//                p.WriteByte(1); // Flag do vento, 1 Reseta o Vento, 0 soma o vento que nem o comando gm \wind do pangya original

//                packet_func_sv.room_broadcast(this,
//                    p, 1);

//            }
//            catch (exception e)
//            {

//                _smp.message_pool.push(new message("[room::testeDegree][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
//            }
//        }

//        // Practice
//        public void requestLeavePractice(Player _session, Packet _packet)
//        {
//            if (!_session.GetState())
//            {
//                throw new exception("[room::" + (("request" + "LeavePractice")) + "][Error] Player nao esta connectado", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                    12, 0));
//            }
//            if (_packet == null)
//            {
//                throw new exception("[room::request" + "LeavePractice" + "][Error] _packet is nullptr", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                    12, 0));
//            }

//            try
//            {

//                if (m_pGame == null)
//                {
//                    throw new exception("[room::requestLeavePractice][Error] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou sair do Practice na sala[NUMERO=" + Convert.ToString(m_ri.numero) + "], mas a sala nao tem nenhum jogo inicializado. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                        1, 0x6202001));
//                }

//                if (m_ri.tipo != (byte)(byte)RoomInfo.TIPO.PRACTICE)
//                {
//                    throw new exception("[room::requestLeavePractice][Error] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou sair do Practice na sala[NUMERO=" + Convert.ToString(m_ri.numero) + "], mas TIPO=" + Convert.ToString((ushort)m_ri.tipo) + " de jogo da sala nao eh Practice", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                        2, 0x6202002));
//                }

//                // Acabou o tempo /*Sai do Practice*/
//                // C++ TO C# CONVERTER TASK: There is no equivalent to '' in C#:
//                //< TourneyBase > (m_pGame).timeIsOver();

//            }
//            catch (exception e)
//            {

//                _smp.message_pool.push(new message("[room::requestLeavePractice][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
//            }
//        }

//        // Tourney
//        public bool requestUseTicketReport(Player _session, Packet _packet)
//        {
//            if (!_session.GetState())
//            {
//                throw new exception("[room::" + (("request" + "UseTicketReport")) + "][Error] Player nao esta connectado", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                    12, 0));
//            }
//            if (_packet == null)
//            {
//                throw new exception("[room::request" + "UseTicketReport" + "][Error] _packet is nullptr", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                    12, 0));
//            }

//            bool ret = false;

//            try
//            {

//                if (m_pGame == null)
//                {
//                    throw new exception("[room::requestUseTicketReport][Error] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou usar Ticket Report no Tourney no jogo na sala[NUMERO=" + Convert.ToString(m_ri.numero) + "], mas a sala nao tem nenhum jogo inicializado. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                        1, 0x6301001));
//                }

//                ret = m_pGame.requestUseTicketReport(_session, _packet);

//            }
//            catch (exception e)
//            {

//                _smp.message_pool.push(new message("[room::requestUseTicketReport][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
//            }

//            return ret;
//        }

//        // Grand Zodiac
//        public void requestLeaveChipInPractice(Player _session, Packet _packet)
//        {
//            if (!_session.GetState())
//            {
//                throw new exception("[room::" + (("request" + "LeaveChipInPractice")) + "][Error] Player nao esta connectado", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                    12, 0));
//            }
//            if (_packet == null)
//            {
//                throw new exception("[room::request" + "LeaveChipInPractice" + "][Error] _packet is nullptr", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                    12, 0));
//            }

//            try
//            {

//                if (m_pGame == null)
//                {
//                    throw new exception("[room::requestLeaveChipInPractice][Error] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou sair do Chip-in Practice na sala[NUMERO=" + Convert.ToString(m_ri.numero) + "], mas a sala nao tem nenhum jogo inicializado. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                        1, 0x6701001));
//                }

//                if (m_ri.tipo != (byte)RoomInfo.TIPO.GRAND_ZODIAC_PRACTICE)
//                {
//                    throw new exception("[room::requestLeaveChipInPractice][Error] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou sair do Chip-in Practice na sala[NUMERO=" + Convert.ToString(m_ri.numero) + "], mas TIPO=" + Convert.ToString((ushort)m_ri.tipo) + " de jogo da sala nao eh Chip-in Practice", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                        2, 0x6701002));
//                }

//                // Acabou o tempo /*Sai do Chip-in Practice*/
//                if (m_pGame.finish_game(_session, 2))
//                {
//                    finish_game();
//                }

//            }
//            catch (exception e)
//            {

//                _smp.message_pool.push(new message("[room::requestLeaveChipInPractice][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
//            }
//        }

//        public void requestStartFirstHoleGrandZodiac(Player _session, Packet _packet)
//        {
//            if (!_session.GetState())
//            {
//                throw new exception("[room::" + (("request" + "StartFirstHoleGrandZodiac")) + "][Error] Player nao esta connectado", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                    12, 0));
//            }
//            if (_packet == null)
//            {
//                throw new exception("[room::request" + "StartFirstHoleGrandZodiac" + "][Error] _packet is nullptr", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                    12, 0));
//            }

//            try
//            {

//                if (m_pGame == null)
//                {
//                    throw new exception("[room::requestStartFisrtHoleGrandZodiac][Error] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou comecar o primeiro hole do Grand Zodiac game na sala[NUMERO=" + Convert.ToString(m_ri.numero) + "], mas a sala nao tem nenhum jogo inicializado. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                        1, 0x6702001));
//                }

//                m_pGame.requestStartFirstHoleGrandZodiac(_session, _packet);

//            }
//            catch (exception e)
//            {

//                _smp.message_pool.push(new message("[room::requestStartFirstHoleGrandZodiac][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
//            }
//        }

//        public void requestReplyInitialValueGrandZodiac(Player _session, Packet _packet)
//        {
//            if (!_session.GetState())
//            {
//                throw new exception("[room::" + (("request" + "ReplyInitialValueGrandZodiac")) + "][Error] Player nao esta connectado", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                    12, 0));
//            }
//            if (_packet == null)
//            {
//                throw new exception("[room::request" + "ReplyInitialValueGrandZodiac" + "][Error] _packet is nullptr", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                    12, 0));
//            }

//            try
//            {

//                if (m_pGame == null)
//                {
//                    throw new exception("[room::requestReplyInitialValueGrandZodiac][Error] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou reponder o valor inicial do Grand Zodiac game na sala[NUMERO=" + Convert.ToString(m_ri.numero) + "], mas a sala nao tem nenhum jogo inicializado. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                        1, 0x6703001));
//                }

//                m_pGame.requestReplyInitialValueGrandZodiac(_session, _packet);

//            }
//            catch (exception e)
//            {

//                _smp.message_pool.push(new message("[room::requestReplyInitialValueGrandZodiac][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
//            }
//        }

//        // Ability Item
//        public void requestActiveRing(Player _session, Packet _packet)
//        {
//            if (!_session.GetState())
//            {
//                throw new exception("[room::" + (("request" + "ActiveRing")) + "][Error] Player nao esta connectado", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                    12, 0));
//            }
//            if (_packet == null)
//            {
//                throw new exception("[room::request" + "ActiveRing" + "][Error] _packet is nullptr", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                    12, 0));
//            }

//            try
//            {

//                if (m_pGame == null)
//                {
//                    throw new exception("[room::requestActiveRing][Error] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou ativar Anel no jogo na sala[NUMERO=" + Convert.ToString(m_ri.numero) + "], mas a sala nao tem nenhum jogo inicializado. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                        1, 0x6201001));
//                }

//                m_pGame.requestActiveRing(_session, _packet);

//            }
//            catch (exception e)
//            {

//                _smp.message_pool.push(new message("[room::requestActiveRing][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
//            }
//        }

//        public void requestActiveRingGround(Player _session, Packet _packet)
//        {
//            if (!_session.GetState())
//            {
//                throw new exception("[room::" + (("request" + "ActiveRingGround")) + "][Error] Player nao esta connectado", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                    12, 0));
//            }
//            if (_packet == null)
//            {
//                throw new exception("[room::request" + "ActiveRingGround" + "][Error] _packet is nullptr", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                    12, 0));
//            }

//            try
//            {

//                if (m_pGame == null)
//                {
//                    throw new exception("[room::requestActiveRingGround][Error] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou ativar Anel de Terreno no jogo na sala[NUMERO=" + Convert.ToString(m_ri.numero) + "], mas a sala nao tem nenhum jogo inicializado. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                        1, 0x6201101));
//                }

//                m_pGame.requestActiveRingGround(_session, _packet);

//            }
//            catch (exception e)
//            {

//                _smp.message_pool.push(new message("[room::requestActiveRingGround][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
//            }
//        }

//        public void requestActiveRingPawsRainbowJP(Player _session, Packet _packet)
//        {
//            if (!_session.GetState())
//            {
//                throw new exception("[room::" + (("request" + "ActiveRingPawsRainbowJP")) + "][Error] Player nao esta connectado", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                    12, 0));
//            }
//            if (_packet == null)
//            {
//                throw new exception("[room::request" + "ActiveRingPawsRainbowJP" + "][Error] _packet is nullptr", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                    12, 0));
//            }

//            try
//            {

//                if (m_pGame == null)
//                {
//                    throw new exception("[room::requestActiveRingPawsRainbowJP][Error] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou ativar Anel Patinha Arco-iris no jogo na sala[NUMERO=" + Convert.ToString(m_ri.numero) + "], mas a sala nao tem nenhum jogo inicializado. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                        1, 0x6201201));
//                }

//                m_pGame.requestActiveRingPawsRainbowJP(_session, _packet);

//            }
//            catch (exception e)
//            {

//                _smp.message_pool.push(new message("[room::requestActiveRingPawsRainbowJP][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
//            }
//        }

//        public void requestActiveRingPawsRingSetJP(Player _session, Packet _packet)
//        {
//            if (!_session.GetState())
//            {
//                throw new exception("[room::" + (("request" + "ActiveRingPawsRingSetJP")) + "][Error] Player nao esta connectado", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                    12, 0));
//            }
//            if (_packet == null)
//            {
//                throw new exception("[room::request" + "ActiveRingPawsRingSetJP" + "][Error] _packet is nullptr", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                    12, 0));
//            }

//            try
//            {

//                if (m_pGame == null)
//                {
//                    throw new exception("[room::requestActiveRingPawsRingSetJP][Error] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou ativar Anel de Patinha de conjunto de Aneis [JP] no jogo na sala[NUMERO=" + Convert.ToString(m_ri.numero) + "], mas a sala n ao tem nenhum jogo inicializado. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                        1, 0x6201301));
//                }

//                m_pGame.requestActiveRingPawsRingSetJP(_session, _packet);

//            }
//            catch (exception e)
//            {

//                _smp.message_pool.push(new message("[room::requestActiveRingPawsRingSetJP][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
//            }
//        }

//        public void requestActiveRingPowerGagueJP(Player _session, Packet _packet)
//        {
//            if (!_session.GetState())
//            {
//                throw new exception("[room::" + (("request" + "ActiveRingPowerGagueJP")) + "][Error] Player nao esta connectado", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                    12, 0));
//            }
//            if (_packet == null)
//            {
//                throw new exception("[room::request" + "ActiveRingPowerGagueJP" + "][Error] _packet is nullptr", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                    12, 0));
//            }

//            try
//            {

//                if (m_pGame == null)
//                {
//                    throw new exception("[room::requestActiveRingPowerGagueJP][Error] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou ativar Anel Barra de PS [JP] no jogo na sala[NUMERO=" + Convert.ToString(m_ri.numero) + "], mas a sala nao tem nenhum jogo inicializado. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                        1, 0x6201401));
//                }

//                m_pGame.requestActiveRingPowerGagueJP(_session, _packet);

//            }
//            catch (exception e)
//            {

//                _smp.message_pool.push(new message("[room::requestActiveRingPowerGagueJP][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
//            }
//        }

//        public void requestActiveRingMiracleSignJP(Player _session, Packet _packet)
//        {
//            if (!_session.GetState())
//            {
//                throw new exception("[room::" + (("request" + "ActiveRingMiracleSignJP")) + "][Error] Player nao esta connectado", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                    12, 0));
//            }
//            if (_packet == null)
//            {
//                throw new exception("[room::request" + "ActiveRingMiracleSignJP" + "][Error] _packet is nullptr", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                    12, 0));
//            }

//            try
//            {

//                if (m_pGame == null)
//                {
//                    throw new exception("[room::requestActiveRingMiracleSignJP][Error] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou ativar Anel Olho Magico [JP] no jogo na sala[NUMERO=" + Convert.ToString(m_ri.numero) + "], mas a sala nao tem nenhum jogo inicializado. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                        1, 0x6201501));
//                }

//                m_pGame.requestActiveRingMiracleSignJP(_session, _packet);

//            }
//            catch (exception e)
//            {

//                _smp.message_pool.push(new message("[room::requestActiveRingMiracleSignJP][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
//            }
//        }

//        public void requestActiveWing(Player _session, Packet _packet)
//        {
//            if (!_session.GetState())
//            {
//                throw new exception("[room::" + (("request" + "ActiveWing")) + "][Error] Player nao esta connectado", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                    12, 0));
//            }
//            if (_packet == null)
//            {
//                throw new exception("[room::request" + "ActiveWing" + "][Error] _packet is nullptr", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                    12, 0));
//            }

//            try
//            {

//                if (m_pGame == null)
//                {
//                    throw new exception("[room::requestActiveWing][Error] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou ativar Asa no joga na sala[NUMERO=" + Convert.ToString(m_ri.numero) + "], mas a sala nao tem nenhum jogo inicializado. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                        1, 0x6201601));
//                }

//                m_pGame.requestActiveWing(_session, _packet);

//            }
//            catch (exception e)
//            {

//                _smp.message_pool.push(new message("[room::requestActiveWing][ErrorSystem] " + e.getMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
//            }
//        }

//        public void requestActivePaws(Player _session, Packet _packet)
//        {
//            if (!_session.GetState())
//            {
//                throw new exception("[room::" + (("request" + "ActivePaws")) + "][Error] Player nao esta connectado", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                    12, 0));
//            }
//            if (_packet == null)
//            {
//                throw new exception("[room::request" + "ActivePaws" + "][Error] _packet is nullptr", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                    12, 0));
//            }

//            try
//            {

//                if (m_pGame == null)
//                {
//                    throw new exception("[room::requestActivePaws][Error] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou ativar Patinha no jogo na sala[NUMERO=" + Convert.ToString(m_ri.numero) + "], mas a sala nao tem nenhum jogo inicializado. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                        1, 0x6201701));
//                }

//                m_pGame.requestActivePaws(_session, _packet);

//            }
//            catch (exception e)
//            {

//                _smp.message_pool.push(new message("[room::requestActivePaws][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
//            }
//        }

//        public void requestActiveGlove(Player _session, Packet _packet)
//        {
//            if (!_session.GetState())
//            {
//                throw new exception("[room::" + (("request" + "ActiveGlove")) + "][Error] Player nao esta connectado", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                    12, 0));
//            }
//            if (_packet == null)
//            {
//                throw new exception("[room::request" + "ActiveGlove" + "][Error] _packet is nullptr", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                    12, 0));
//            }

//            try
//            {

//                if (m_pGame == null)
//                {
//                    throw new exception("[room::requestActiveGlove][Error] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou ativat Luva 1m no jogo na sala[NUMERO=" + Convert.ToString(m_ri.numero) + "], mas a sala nao tem nenhum jogo inicializado. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                        1, 0x6201801));
//                }

//                m_pGame.requestActiveGlove(_session, _packet);

//            }
//            catch (exception e)
//            {

//                _smp.message_pool.push(new message("[room::requestActiveGlove][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
//            }
//        }

//        public void requestActiveEarcuff(Player _session, Packet _packet)
//        {
//            if (!_session.GetState())
//            {
//                throw new exception("[room::" + (("request" + "ActiveEarcuff")) + "][Error] Player nao esta connectado", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                    12, 0));
//            }
//            if (_packet == null)
//            {
//                throw new exception("[room::request" + "ActiveEarcuff" + "][Error] _packet is nullptr", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                    12, 0));
//            }

//            try
//            {

//                if (m_pGame == null)
//                {
//                    throw new exception("[room::requestActiveEarcuff][Error] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou ativar Earcuff no jogo na sala[NUMERO=" + Convert.ToString(m_ri.numero) + "], mas a sala nao tem nenhum jogo inicializado. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                        1, 0x6201901));
//                }

//                m_pGame.requestActiveEarcuff(_session, _packet);

//            }
//            catch (exception e)
//            {

//                _smp.message_pool.push(new message("[room::requestActiveEarcuff][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
//            }
//        }

//        public void requestSLastTimeGame(Player _session)
//        {
//            if (!_session.GetState())
//            {
//                throw new exception("[room::" + "requestSLastTimeGame" + "][Error] Player nao esta connectado", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                    12, 0));
//            }

//            PangyaBinaryWriter p = new PangyaBinaryWriter();

//            try
//            {

//                if (isKickedPlayer(_session.m_pi.uid))
//                {
//                    throw new exception("[room::requestSLastTimeGame][Error] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou entrar na sala[NUMERO=" + Convert.ToString(m_ri.numero) + "] ja em jogo, mas o Player foi chutado da sala antes de comecar o jogo.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                        2704, 7));
//                }

//                if (m_pGame == null)
//                {
//                    throw new exception("[room::requestSLastTimeGame][Error] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou pegar o tempo do tourney que comecou na sala[NUMERO=" + Convert.ToString(m_ri.numero) + "], mas a sala nao tem nenhum jogo inicializado. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                        2705, 1));
//                }

//                m_pGame.requestSLastTimeGame(_session);

//            }
//            catch (exception e)
//            {

//                _smp.message_pool.push(new message("[room::requestSLastTimeGame][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

//                // Resposta erro
//                p.init_plain((ushort)0x113);

//                p.WriteByte(6); // Option Error

//                // Error Code
//                p.WriteByte((byte)((STDA_SOURCE_ERROR_DECODE(e.getCodeError()) == STDA_ERROR_TYPE.ROOM) ? STDA_SYSTEM_ERROR_DECODE(e.getCodeError()) : 1));

//                packet_func_sv.session_send(p,
//                    _session, 1);
//            }
//        }

//        public bool requestEnterGameAfterStarted(Player _session)
//        {
//            if (!_session.GetState())
//            {
//                throw new exception("[room::" + "requestEnterGameAfterStarted" + "][Error] Player nao esta connectado", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                    12, 0));
//            }

//            PangyaBinaryWriter p = new PangyaBinaryWriter();

//            bool ret = false;

//            try
//            {

//                if (isKickedPlayer(_session.m_pi.uid))
//                {
//                    throw new exception("[room::requestEnterGameAfterStarted][WARNING] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou entrar na sala[NUMERO=" + Convert.ToString(m_ri.numero) + "] ja em jogo, mas o Player foi chutado da sala antes de comecar o jogo.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                        2704, 7));
//                }

//                if (m_pGame == null)
//                {
//                    throw new exception("[room::requestEnterGameAfterStarted][Error] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou entrar na sala[NUMERO=" + Convert.ToString(m_ri.numero) + "] ja em jogo, mas a sala nao tem nenhum jogo inicializado. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                        2705, 1));
//                }

//                if (isGamingBefore(_session.m_pi.uid))
//                {
//                    throw new exception("[room::requestEnterGameAfterStarted][WARNING] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou entrar na sala[NUMERO=" + Convert.ToString(m_ri.numero) + "] ja em jogo, mas o Player ja tinha jogado nessa sala e saiu, e nao pode mais entrar.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                        2703, 6));
//                }

//                int64_t tempo = (m_ri.qntd_hole == 18) ? 10 * 60000 : 5 * 60000;

//                var remain = getLocalTimeDsIff.getInstance()(m_pGame.getTimeStart());

//                if (remain > 0)
//                {
//                    remain /= STDA_10_MICRO_PER_MILLI; // miliValues
//                }

//                if (remain >= tempo)
//                {
//                    throw new exception("[room::requestEnterGameAfrerStarted][WARNING] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou entrar na sala[NUMERO=" + Convert.ToString(m_ri.numero) + "] ja em jogo, mas o tempo de entrar no tourney acabou.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM, // Acabou o tempo de entrar na sala
//                        2706, 2));
//                }

//                // Add Player a sala
//                enter(_session);

//                ret = true;

//            }
//            catch (exception e)
//            {

//                _smp.message_pool.push(new message("[room::requestEnterGameAfterStarted][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

//                // Excluí Player da sala se adicionou ele antes
//                if (findSessionByUID(_session.m_pi.uid) != null)
//                {
//                    leave(_session, 0);
//                }

//                // Resposta erro
//                p.init_plain((ushort)0x113);

//                p.WriteByte(6); // Option Error

//                // Error Code
//                p.WriteByte((byte)((STDA_SOURCE_ERROR_DECODE(e.getCodeError()) == STDA_ERROR_TYPE.ROOM) ? STDA_SYSTEM_ERROR_DECODE(e.getCodeError()) : 1));

//                packet_func_sv.session_send(p,
//                    _session, 1);
//            }

//            return ret;
//        }

//        public void requestUpdateEnterAfterStartedInfo(Player _session, EnterAfterStartInfo _easi)
//        {
//            if (!_session.GetState())
//            {
//                throw new exception("[room::" + "requestUpdateEnterAfterStartedInfo" + "][Error] Player nao esta connectado", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                    12, 0));
//            }

//            PangyaBinaryWriter p = new PangyaBinaryWriter();

//            try
//            {

//                if (m_pGame == null)
//                {
//                    throw new exception("[room::requestUpdateEnterAfterStartedInfo][Error] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou atualizar info do Player que entrou depois na sala[NUMERO=" + Convert.ToString(m_ri.numero) + "] ja em jogo, mas a sala nao tem nenhum jogo inicializado. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                        2705, 1));
//                }

//                m_pGame.requestUpdateEnterAfterStartedInfo(_session, _easi);

//            }
//            catch (exception e)
//            {

//                _smp.message_pool.push(new message("[room::requestUpdateEnterAfterStartedInfo][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

//                // Resposta erro
//                p.init_plain((ushort)0x113);

//                p.WriteByte(6); // Option Error

//                // Error Code
//                p.WriteByte((byte)((STDA_SOURCE_ERROR_DECODE(e.getCodeError()) == STDA_ERROR_TYPE.ROOM) ? STDA_SYSTEM_ERROR_DECODE(e.getCodeError()) : 1));

//                packet_func_sv.session_send(p,
//                    _session, 1);
//            }
//        }

//        public bool requestFinishGame(Player _session, Packet _packet)
//        {
//            if (!_session.GetState())
//            {
//                throw new exception("[room::" + (("request" + "FinishGame")) + "][Error] Player nao esta connectado", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                    12, 0));
//            }
//            if (_packet == null)
//            {
//                throw new exception("[room::request" + "FinishGame" + "][Error] _packet is nullptr", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                    12, 0));
//            }

//            bool ret = false;

//            try
//            {

//                if (m_pGame == null)
//                {
//                    throw new exception("[room::requestFinishGame][Error] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou finalizar jogo na sala[NUMERO=" + Convert.ToString(m_ri.numero) + "], mas a sala nao tem nenhum jogo inicializado. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                        1, 0x5202101));
//                }

//                if (m_pGame.requestFinishGame(_session, _packet))
//                { // Terminou o Jogo

//                    finish_game();

//                    ret = true;
//                }

//            }
//            catch (exception e)
//            {

//                _smp.message_pool.push(new message("[room::requestFinishGame][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
//            }

//            return ret;
//        }

//        public void requestChangeWindNextHoleRepeat(Player _session, Packet _packet)
//        {
//            if (!_session.GetState())
//            {
//                throw new exception("[room::" + (("request" + "ChangeWindNextHoleRepeat")) + "][Error] Player nao esta connectado", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                    12, 0));
//            }
//            if (_packet == null)
//            {
//                throw new exception("[room::request" + "ChangeWindNextHoleRepeat" + "][Error] _packet is nullptr", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                    12, 0));
//            }

//            try
//            {

//                if (m_pGame == null)
//                {
//                    throw new exception("[room::requestChangeWindNextHoleRepeat][Error] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou trocar vento dos proximos holes repeat no jogo na sala[NUMERO=" + Convert.ToString(m_ri.numero) + "], mas a sala nao tem nenhum jogo inicializado. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                        1, 0x5202201));
//                }

//                m_pGame.requestChangeWindNextHoleRepeat(_session, _packet);

//            }
//            catch (exception e)
//            {

//                _smp.message_pool.push(new message("[room::requestChangeWindNextHoleRepeat][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
//            }
//        }

//        public void requestPlayerReportChatGame(Player _session, Packet _packet)
//        {
//            if (!_session.GetState())
//            {
//                throw new exception("[room::" + (("request" + "PlayerReportChatGame")) + "][Error] Player nao esta connectado", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                    12, 0));
//            }
//            if (_packet == null)
//            {
//                throw new exception("[room::request" + "PlayerReportChatGame" + "][Error] _packet is nullptr", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                    12, 0));
//            }

//            try
//            {

//                if (m_pGame == null)
//                {
//                    throw new exception("[room::requestPlayerReportChatGame][Error] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou reporta o chat do jogo na sala[NUMERO=" + Convert.ToString(m_ri.numero) + "], mas nao tem nenhum jogo inicializado na sala. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                        0x580200, 0));
//                }

//                // Report Chat Game
//                m_pGame.requestPlayerReportChatGame(_session, _packet);

//            }
//            catch (exception e)
//            {

//                _smp.message_pool.push(new message("[room::requestPlayerReportChatGame][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

//                if (STDA_SOURCE_ERROR_DECODE(e.getCodeError()) != STDA_ERROR_TYPE.GAME)
//                {
//                    throw;
//                }
//            }
//        }

//        // Common Command GM
//        public void requestExecCCGChangeWindVersus(Player _session, Packet _packet)
//        {
//            if (!_session.GetState())
//            {
//                throw new exception("[room::" + (("request" + "ExecCCGChangeWindVersus")) + "][Error] Player nao esta connectado", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                    12, 0));
//            }
//            if (_packet == null)
//            {
//                throw new exception("[room::request" + "ExecCCGChangeWindVersus" + "][Error] _packet is nullptr", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                    12, 0));
//            }

//            try
//            {

//                if (!(m_ri.tipo == (byte)RoomInfo.TIPO.STROKE || (m_ri.tipo == (byte)RoomInfo.TIPO.MATCH))
//                {
//                    throw new exception("[room::requestExecCCGChangeWindVersus][Error] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou executar o comando de troca de vento na sala[NUMERO=" + Convert.ToString(m_ri.numero) + ", TIPO=" + Convert.ToString(m_ri.tipo) + "], mas o tipo da sala nao eh Stroke ou Match modo. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                        1, 0x5700100));
//                }

//                if (m_pGame == null)
//                {
//                    throw new exception("[room::requestExecCCGChangeWindVersus][Error] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou executar o comando de troca de vento na sala[NUMERO=" + Convert.ToString(m_ri.numero) + "], mas a sala nao tem nenhum jogo inicializado. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                        2, 0x5700100));
//                }

//                m_pGame.requestExecCCGChangeWind(_session, _packet);

//            }
//            catch (exception e)
//            {

//                _smp.message_pool.push(new message("[room::requestExecCCGChangeWindVersus][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

//                throw;
//            }
//        }

//        public void requestExecCCGChangeWeather(Player _session, Packet _packet)
//        {
//            if (!_session.GetState())
//            {
//                throw new exception("[room::" + (("request" + "ExecCCGChangeWeather")) + "][Error] Player nao esta connectado", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                    12, 0));
//            }
//            if (_packet == null)
//            {
//                throw new exception("[room::request" + "ExecCCGChangeWeather" + "][Error] _packet is nullptr", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                    12, 0));
//            }

//            try
//            {

//                // Update on Flag Lounge or Game Course->hole->weather
//                if (m_ri.tipo == (byte)RoomInfo.TIPO.LOUNGE)
//                {

//                    m_weather_lounge = _packet.ReadByte();

//                    // UPDATE ON GAME
//                    PangyaBinaryWriter p = new PangyaBinaryWriter((ushort)0x9E);

//                    p.WriteUInt16(m_weather_lounge);
//                    p.WriteByte(1); // Acho que seja flag, não sei, vou deixar 1 por ser o GM que mudou

//                    packet_func_sv.room_broadcast(this,
//                        p, 1);

//                }
//                else if (m_pGame != null)
//                {

//                    m_pGame.requestExecCCGChangeWeather(_session, _packet);

//                }
//                else
//                {
//                    throw new exception("[room::requestExecCCGChangeWeather][Error] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou executar o comando de troca de tempo(weather) na sala[NUMERO=" + Convert.ToString(m_ri.numero) + "], mas a sala nao eh lounge ou nao tem um jogo iniclializado. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                        3, 0x5700100));
//                }

//            }
//            catch (exception e)
//            {

//                _smp.message_pool.push(new message("[room::requestExecCCGChangeWeather][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

//                throw;
//            }
//        }

//        public void requestExecCCGGoldenBell(Player _session, Packet _packet)
//        {
//            if (!_session.GetState())
//            {
//                throw new exception("[room::" + (("request" + "ExecCCGGoldenBell")) + "][Error] Player nao esta connectado", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                    12, 0));
//            }
//            if (_packet == null)
//            {
//                throw new exception("[room::request" + "ExecCCGGoldenBell" + "][Error] _packet is nullptr", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                    12, 0));
//            }

//            try
//            {

//                uint item_typeid = _packet.ReadUInt32();
//                uint item_qntd = _packet.ReadUInt32();

//                if (item_typeid == 0)
//                {
//                    throw new exception("[room::requestExecCCGGoldenBell][Error] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou enviar presente para todos da sala[NUMERO=" + Convert.ToString(m_ri.numero) + "] o Item[TYPEID=" + Convert.ToString(item_typeid) + "QNTD = " + Convert.ToString(item_qntd) + "], mas item is invalid. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.GAME_SERVER,
//                        3, 0x5700100));
//                }

//                if (item_qntd > 20000u)
//                {
//                    throw new exception("[room::requestExecCCGGoldenBell][Error] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou enviar presente para todos da sala[NUMERO=" + Convert.ToString(m_ri.numero) + "] o Item[TYPEID=" + Convert.ToString(item_typeid) + "QNTD = " + Convert.ToString(item_qntd) + "], mas a quantidade passa de 20mil. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.GAME_SERVER,
//                        4, 0x5700100));
//                }

//                var @base = sIff.getInstance().findCommomItem(item_typeid);

//                if (@base == null)
//                {
//                    throw new exception("[room::requestExecCCGGoldenBell][Error] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou enviar presente para todos da sala[NUMERO=" + Convert.ToString(m_ri.numero) + "] o Item[TYPEID=" + Convert.ToString(item_typeid) + "QNTD = " + Convert.ToString(item_qntd) + "], mas o item nao existe no IFF_STRUCT do Server. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.GAME_SERVER,
//                        6, 0));
//                }

//                stItem item = new stItem();
//                BuyItem bi = new BuyItem();

//                bi.id = -1;
//                bi._typeid = item_typeid;
//                bi.qntd = item_qntd;

//                var msg = ("GM enviou um item para voce: item[ " + (@base.Name) + " ]");

//                foreach (var el in v_sessions)
//                {

//                    // Limpa item
//                    item.clear();

//                    item_manager.initItemFromBuyItem(el.m_pi,
//                        refref item, bi, false, 0, 0, 1);

//                    if (item._typeid == 0)
//                    {
//                        throw new exception("[room::requestExecCCGGoldenBell][Error] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou enviar presente para todos da sala[NUMERO=" + Convert.ToString(m_ri.numero) + "] o Item[TYPEID=" + Convert.ToString(item_typeid) + "QNTD = " + Convert.ToString(item_qntd) + "], mas nao conseguiu inicializar o item. Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.GAME_SERVER,
//                            5, 0));
//                    }

//                    if (MailBoxManager.sendMessageWithItem(0,
//                        el.m_pi.uid, msg, item) <= 0)
//                    {
//                        throw new exception("[room::requestExecCCGGoldenBell][Error] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou enviar presente para o Player[UID=" + Convert.ToString(el.m_pi.uid) + "] o Item[TYPEID=" + Convert.ToString(item_typeid) + ", QNTD=" + Convert.ToString(item_qntd) + "], mas nao conseguiu colocar o item no mail box dele. Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.GAME_SERVER,
//                            7, 0));
//                    }

//                    // Log
//                    _smp.message_pool.push(new message("[room::requestExecCCGGoldenBell][Log] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] enviou um Item[TYPEID=" + Convert.ToString(item_typeid) + ", QNTD=" + Convert.ToString(item_qntd) + "] para o Player[UID=" + Convert.ToString(el.m_pi.uid) + "]", type_msg.CL_FILE_LOG_AND_CONSOLE));
//                }

//            }
//            catch (exception e)
//            {

//                _smp.message_pool.push(new message("[room::requestExecCCGGoldenBell][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

//                throw;
//            }
//        }

//        // Smart Calculator Command
//        public virtual bool execSmartCalculatorCmd(Player _session,
//            string _msg,
//            eTYPE_CALCULATOR_CMD _type)
//        {
//            if (!_session.GetState())
//            {
//                throw new exception("[room::" + "execSmartCalculatorCmd" + "][Error] Player nao esta connectado", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                    12, 0));
//            }

//            bool ret = false;

//            try
//            {

//                if (!isGaming())
//                {
//                    throw new exception("[room::execSmartCalculatorCmd][Error] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] Channel[ID=" + Convert.ToString((ushort)m_channel_owner) + "] tentou executar Smart Calculator Command na sala[NUMERO=" + Convert.ToString(m_ri.numero) + "], mas ele nao esta em jogo.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                        10000, 0));
//                }

//                //ret = m_pGame.execSmartCalculatorCmd(_session,
//                //    _msg, _type);

//            }
//            catch (exception e)
//            {

//                _smp.message_pool.push(new message("[room::execSmartCalculatorCmd][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

//                ret = false;
//            }

//            return ret;
//        }

//        // Pede o Hole que o Player está, 
//        // se eles estiver jogando ou 0 se ele não está jogando
//        public byte requestPlace(Player _session)
//        {

//            try
//            {

//                if (m_pGame != null)
//                {
//                    return m_pGame.requestPlace(_session);
//                }

//            }
//            catch (exception e)
//            {

//                _smp.message_pool.push(new message("[room::requestPlacePlayer][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
//            }

//            return 0;
//        }

//        public virtual void startGame(Player _session)
//        {

//            try
//            {

//                if (m_pGame == null)
//                {
//                    throw new exception("[room::startGame][Error] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou comecar o jogo na sala[NUMERO=" + Convert.ToString(m_ri.numero) + "], mas a sala nao tem nenhum jogo iniciado. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                        1, 0x5200101));
//                }

//                if (m_ri.flag == 0)
//                {

//                    m_pGame.sendInitialData(_session);

//                    if (m_ri.tipo == (byte)RoomInfo.TIPO.STROKE || (m_ri.tipo == (byte)RoomInfo.TIPO.MATCH)
//                    {
//                        sendCharacter(_session, 0x103);
//                    }

//                }
//                else
//                { // Entra depois

//                    try
//                    {

//                        List<PlayerRoomInfoEx> v_element = new List<PlayerRoomInfoEx>();
//                        PlayerRoomInfoEx pri = null;

//                        v_sessions.(_el) =>
//                        {
//                            if ((pri = getPlayerInfo(_el)) != null)
//                            {
//                                v_element.Add(pri);
//                            }
//                        });

//                        // SLast Make Room
//                        PangyaBinaryWriter p = new PangyaBinaryWriter((ushort)0x113);

//                        p.WriteByte(4); // Cria sala
//                        p.WriteByte(0);

//                        p.WriteStruct(m_ri, Marshal.SizeOf(new RoomInfo());

//                        packet_func_sv.session_send(p,
//                            _session, 1);

//                        // SLast All Player Of Room
//                        p.init_plain((ushort)0x113);

//                        p.WriteByte(4);
//                        p.WriteByte(1);

//                        p.WriteByte((byte)v_element.Count);

//                        for (var i = 0; i < v_element.Count; i++)
//                        {
//                            p.WriteStruct(v_element[i], Marshal.SizeOf(new PlayerRoomInfo));
//                        }

//                        packet_func_sv.session_send(p,
//                            _session, 1);

//                        // Rate Pang
//                        p.init_plain((ushort)0x113);

//                        p.WriteByte(4);
//                        p.WriteByte(2);

//                        p.WriteUInt32(m_ri.rate_pang);

//                        packet_func_sv.session_send(p,
//                            _session, 1);

//                        // SLast Initial of Game
//                        m_pGame.sendInitialDataAfter(_session);

//                        // Add Player ON GAME to ALL Players
//                        p.init_plain((ushort)0x113);

//                        p.WriteByte(7);
//                        p.WriteByte(0);

//                        p.WritePStr(_session.m_pi.nickname);

//                        p.WriteStruct(m_ri, Marshal.SizeOf(new RoomInfo());

//                        p.WriteByte((byte)v_element.Count);

//                        p.WriteUInt32((uint)v_sessions.Count);

//                        for (var i = 0; i < v_element.Count; i++)
//                        {
//                            p.WriteStruct(v_element[i], Marshal.SizeOf(new PlayerRoomInfo));
//                        }

//                        // SLast ALL Players of room exceto ele
//                        packet_func_sv.vector_send(p,
//                            getSessions(_session), 1);

//                    }
//                    catch (exception e)
//                    {


//                        throw;
//                    }
//                }

//            }
//            catch (exception e)
//            {

//                _smp.message_pool.push(new message("[room::startGame][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
//            }
//        }

//        // Time Tourney After Enter IN GAME
//        public void requestStartAfterEnter(job _job)   //ver depois!!!!!@@@@
//        {

//            try
//            {

//                if (m_pGame == null)
//                {
//                    throw new exception("[room::requestStartAfterEnter][Error] tentou comecar o tempo que pode entrar no jogo depois que ele comecou na sala[NUMERO=" + Convert.ToString(m_ri.numero) + "], mas a sala nao tem nenhum jogo iniciado. Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                        1200, 0));
//                }

//                m_pGame.requestStartAfterEnter(_job);

//            }
//            catch (exception e)
//            {

//                _smp.message_pool.push(new message("[room::requestStartAfterEnter][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
//            }
//        }

//        public void requestEndAfterEnter()
//        {

//            try
//            {

//                if (m_pGame == null)
//                {
//                    throw new exception("[room::requestEndAfterEnter][Error] tentou terminar o tempo que pode entrar no jogo depois que ele comecou na sala[NUMERO=" + Convert.ToString(m_ri.numero) + "], mas a sala nao tem nenhum jogo iniciado. Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                        1201, 0));
//                }

//                m_pGame.requestEndAfterEnter();

//            }
//            catch (exception e)
//            {

//                _smp.message_pool.push(new message("[room::requestEndAfterEnter][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
//            }
//        }

//        // senders
//        public void sendMake(Player _session)
//        {

//            PangyaBinaryWriter p = new PangyaBinaryWriter();

//            packet_func_sv.pacote049(p,
//                this, 0);
//            packet_func_sv.session_send(p,
//                _session, 0);

//        }

//        public void sendUpdate()
//        {

//            PangyaBinaryWriter p = new PangyaBinaryWriter();

//            packet_func_sv.pacote04A(p,
//                m_ri, -1);
//            packet_func_sv.room_broadcast(this,
//                p, 0);
//        }

//        public void sendCharacter(Player _session, int _option)
//        {

//            int option = !((byte)RoomInfo.TIPO.STROKE == m_ri.tipo || (byte)RoomInfo.TIPO.MATCH == m_ri.tipo || (byte)RoomInfo.TIPO.LOUNGE == m_ri.tipo || (byte)RoomInfo.TIPO.PANG_BATTLE == m_ri.tipo) ? 0x100 : 0;

//            option += _option;

//            if (option == 0 && m_ri.tipo == (byte)RoomInfo.TIPO.LOUNGE)
//            {
//                option = 7;
//            }

//            List<PlayerRoomInfoEx> v_element = new List<PlayerRoomInfoEx>();
//            PlayerRoomInfoEx pri = null;

//            try
//            {
//                v_sessions.ForEach(_el =>
//                {
//                    pri = getPlayerInfo(_el);
//                    if (pri != null)
//                    {
//                        v_element.Add(pri);
//                    }
//                });


//                pri = getPlayerInfo(_session);

//                if (pri == null && _option != 2)
//                {
//                    throw new exception("[room::sendCharacter][Error] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou pegar o info do Player na sala[NUMERO=" + Convert.ToString(m_ri.numero) + "], mas nao tem o info dele na sala. Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                        1, 5000));
//                }

//                PangyaBinaryWriter p = new PangyaBinaryWriter();

//                if (packet_func_sv.pacote048(p,
//                    _session,
//                    ((_option == 1 || _option == 4 || _option == 0x103) ? new List<PlayerRoomInfoEx>({ pri }) : v_element),
//						option))
//					{
//                    packet_func_sv.room_broadcast(this,
//                        p, 1);
//                }



//            }
//            catch (exception e)
//            {




//                // Relança
//                throw;
//            }
//        }

//        public void sendCharacterStateLounge(Player _session)
//        {

//            if (m_ri.tipo == (byte)RoomInfo.TIPO.LOUNGE)
//            {
//                var it = (_session.m_pi.ei.char_info == null) ? _session.m_pi.mp_scl.Last() : _session.m_pi.mp_scl.FirstOrDefault(c => c.Key == _session.m_pi.ei.char_info.id);

//                if (it == _session.m_pi.mp_scl.Last())
//                {
//                    throw new exception("[room::sendCharacterStateLounge][Error] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] nao tem os estados do character na lounge.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.CHANNEL,
//                        13, 0));
//                }

//                PangyaBinaryWriter p = new PangyaBinaryWriter((ushort)0x196);

//                p.WriteUInt32(_session.m_oid);

//                p.WriteStruct(it.Value, Marshal.SizeOf(new StateCharacterLounge());

//                packet_func_sv.room_broadcast(this,
//                    p, 0);
//            }
//        }

//        public void sendWeatherLounge(Player _session)
//        {

//            if (m_ri.tipo == (byte)RoomInfo.TIPO.LOUNGE)
//            {

//                // Envia o tempo(weather) do lounge só se ele for diferente de tempo bom
//                if (m_weather_lounge != 0u)
//                {

//                    PangyaBinaryWriter p = new PangyaBinaryWriter((ushort)0x9E);

//                    p.WriteUInt16(m_weather_lounge);
//                    p.WriteByte(0); // Flag (acho), vou colocar 0 o padrão, colocou 1 aqui só quando eu mudou com o comando GM

//                    packet_func_sv.session_send(p,
//                        _session, 1);
//                }
//            }
//        }

//        // Locker
//        public void @lock()
//        {

//            if (m_destroying)
//            {

//                throw new exception("[room::lock][Error] room esta no estado para ser destruida, nao pode bloquear ela.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                    150, 0));
//            }

//            m_lock_spin_state++; // Bloqueado
//        }

//        public bool tryLock()
//        {

//            bool ret = false;


//            if ((ret = TryEnterCriticalSection(m_lock_cs)))
//            {

//                int err_ret = 0; // provável erro é EBUSY, mas pode falhar com outros também, mas muito menos provável

//                if ((err_ret = pthread_mutex_trylock(m_lock_cs)) == 0)
//                {

//                    ret = true;

//                    if (m_destroying)
//                    {

//                        throw new exception("[room::tryLock][Error] room esta no estado para ser destruida, nao pode bloquear ela.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                            150, 0));
//                    }

//                    m_lock_spin_state++; // Bloqueado
//                }

//                return ret;
//            }

//            public void unlock()
//            {

//                if (--m_lock_spin_state < 0)
//                {

//                    _smp.message_pool.push(new message("[room::unlock][WARNING] a sala[NUMERO=" + Convert.ToString(m_ri.numero) + "] ja esta desbloqueada.", type_msg.CL_FILE_LOG_AND_CONSOLE));

//                    //return;
//                }
//            }

//            public List<InviteChannelInfo> getAllInvite()
//            {
//                return v_invite;
//            }

//            public void setDestroying()
//            {

//                // Destruindo a sala
//                m_destroying = true;
//            }

//            public static void SQLDBResponse(int _msg_id,
//                Pangya_DB _pangya_db,
//                object _arg)
//            {

//                if (_arg == null)
//                {
//                    _smp.message_pool.push(new message("[room::SQLDBResponse][WARNING] _arg is nullptr com msg_id = " + Convert.ToString(_msg_id), type_msg.CL_FILE_LOG_AND_CONSOLE));
//                    return;
//                }

//                // Por Hora só sai, depois faço outro tipo de tratamento se precisar
//                if (_pangya_db.getException().getCodeError() != 0)
//                {
//                    _smp.message_pool.push(new message("[room::SQLDBResponse][Error] " + _pangya_db.getException().getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
//                    return;
//                }

//                // C++ TO C# CONVERTER TASK: There is no equivalent to '' in C#:
//                var _channel = (Channel)(_arg);

//                switch (_msg_id)
//                {
//                    case 7: // Update Character PCL
//                        {

//                            var cmd_ucp = (CmdUpdateCharacterPCL)(_pangya_db);

//                            break;
//                        }
//                    case 8: // Update ClubSet Stats
//                        {

//                            var cmd_ucss = (CmdUpdateClubSetStats)(_pangya_db);

//                            break;
//                        }
//                    case 9: // Update Character Mastery
//                        {

//                            var cmd_ucm = (CmdUpdateCharacterMastery)(_pangya_db);

//                            _smp.message_pool.push(new message("[room::SQLDBResponse][Log] Atualizou Character[TYPEID=" + Convert.ToString(cmd_ucm.getInfo()._typeid) + ", ID=" + Convert.ToString(cmd_ucm.getInfo().id) + "] Mastery[value=" + Convert.ToString(cmd_ucm.getInfo().mastery) + "] do Player[UID=" + Convert.ToString(cmd_ucm.getUID()) + "]", type_msg.CL_FILE_LOG_AND_CONSOLE));
//                            break;
//                        }
//                    case 12: // Update ClubSet Workshop
//                        {

//                            var cmd_ucw = (CmdUpdateClubSetWorkshop)(_pangya_db);

//                            break;
//                        }
//                    case 26: // Update Mascot Info
//                        {


//                            var cmd_umi = (CmdUpdateMascotInfo)(_pangya_db);


//                            break;
//                        }
//                    case 0:
//                    default: // 25 é update item equipado slot
//                        break;
//                }
//            }

//            protected int findIndexSession(Player _session)
//            {

//                if (_session == null)
//                {
//                    throw new exception("[room::findIndexSession][Error] _session is nullptr.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                        3, 0));
//                }

//                int index = ~0;



//                for (var i = 0; i < v_sessions.Count; ++i)
//                {
//                    if (v_sessions[i] == _session)
//                    {
//                        index = i;
//                        break;
//                    }
//                }



//                return new int(index);
//            }

//            protected int findIndexSession(uint _uid)
//            {

//                if (_uid == 0u)
//                {
//                    throw new exception("[room::findIndexSession][Error] _uid is invalid(zero).", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                        3, 0));
//                }

//                int index = ~0;



//                for (var i = 0; i < v_sessions.Count; ++i)
//                {
//                    if (v_sessions[i].m_pi.uid == _uid)
//                    {
//                        index = i;
//                        break;
//                    }
//                }



//                return new int(index);
//            }

//            protected PlayerRoomInfoEx makePlayerInfo(Player _session)
//            {

//                PlayerRoomInfoEx pri = new PlayerRoomInfoEx();

//                // Player Room Info Init
//                pri.oid = _session.m_oid;

//                // C++ TO C# CONVERTER TASK: The memory management function 'memcpy_s' has no equivalent in C#:
//                memcpy_s(pri.nickname,
//                    Marshal.SizeOf(new char),
//                    _session.m_pi.nickname,
//                    Marshal.SizeOf(new char));
//                // C++ TO C# CONVERTER TASK: The memory management function 'memcpy_s' has no equivalent in C#:
//                memcpy_s(pri.guild_name,
//                    Marshal.SizeOf(new char),
//                    _session.m_pi.gi.name,
//                    Marshal.SizeOf(new char));

//                // C++ TO C# CONVERTER TASK: The memory management function 'memcpy' has no equivalent in C#:
//                memcpy(pri.nickname,
//                    _session.m_pi.nickname,
//                    Marshal.SizeOf(new char));
//                // C++ TO C# CONVERTER TASK: The memory management function 'memcpy' has no equivalent in C#:
//                memcpy(pri.guild_name,
//                    _session.m_pi.gi.name,
//                    Marshal.SizeOf(new char));
//                pri.position = (byte)((byte)getPosition(_session) + 1); // posição na sala
//                pri.capability = _session.m_pi.m_cap;
//                pri.title = _session.m_pi.ue.skin_typeid[5];

//                if (_session.m_pi.ei.char_info != null)
//                {
//                    pri.char_typeid = _session.m_pi.ei.char_info._typeid;
//                }


//                // C++ TO C# CONVERTER TASK: The memory management function 'memcpy_s' has no equivalent in C#:
//                memcpy_s(pri.skin,
//                    Marshal.SizeOf(new uint),
//                    _session.m_pi.ue.skin_typeid,
//                    Marshal.SizeOf(new uint));

//                // C++ TO C# CONVERTER TASK: The memory management function 'memcpy' has no equivalent in C#:
//                memcpy(pri.skin,
//                    _session.m_pi.ue.skin_typeid,
//                    Marshal.SizeOf(new uint));
//                pri.skin[4] = 0; // Aqui tem que ser zero, se for outro valor não mostra a imagem do character equipado

//                if (getMaster() == _session.m_pi.uid)
//                {
//                    pri.state_flag.master = 1;
//                    pri.state_flag.ready = 1; // Sempre está pronto(ready) o master
//                }

//                pri.state_flag.sexo = _session.m_pi.mi.sexo;

//                // Update Team se for Match
//                if (m_ri.tipo == (byte)RoomInfo.TIPO.MATCH)
//                {

//                    if (v_sessions.Count > 1)
//                    {

//                        if (m_teans[0].getCount() >= 2 && m_teans[1].getCount() >= 2)
//                        {
//                            throw new exception("[room::makePlayerInfo][Error] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou entrar em time para todos os times da sala estao cheios. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                                1500, 0));
//                        }
//                        else if (m_teans[0].getCount() >= 2)
//                        {
//                            pri.state_flag.team = 1; // Blue
//                        }
//                        else if (m_teans[1].getCount() >= 2)
//                        {
//                            pri.state_flag.team = 0; // Red
//                        }
//                        else
//                        {

//                            var pPri = getPlayerInfo(
//                                v_sessions.Count == 2 ? v_sessions.First() :
//                                (v_sessions.Count > 2 ? v_sessions.Skip(1).FirstOrDefault() : null)
//                            );

//                            if (pPri == null)
//                            {
//                                throw new exception("[room::makePlayerInfo][Error] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou entrar em um time, mas o ultimo Player da sala, nao tem um info no sala. Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                                    1501, 0));
//                            }

//                            pri.state_flag.team = (byte)~pPri.state_flag.team;
//                        }

//                    }
//                    else
//                    {
//                        pri.state_flag.team = 0;
//                    }

//                    m_teans[pri.state_flag.team].addPlayer(_session);

//                }
//                else if (m_ri.tipo != (byte)RoomInfo.TIPO.GUILD_BATTLE) // O Guild Battle tem sua própria função para inicializar e atualizar o team e os dados da guild
//                {
//                    pri.state_flag.team = (pri.position - 1) % 2;
//                }

//                // Só faz calculo de Quit rate depois que o Player
//                // estiver no level Beginner E e jogado 50 games
//                if (_session.m_pi.level >= 6 && _session.m_pi.ui.jogado >= 50)
//                {
//                    float rate = _session.m_pi.ui.getQuitRate();

//                    if (rate < GOOD_PLAYER_ICON)
//                    {
//                        pri.state_flag.azinha = 1;
//                    }
//                    else if (rate >= QUITER_ICON_1 && rate < QUITER_ICON_2)
//                    {
//                        pri.state_flag.quiter_1 = 1;
//                    }
//                    else if (rate >= QUITER_ICON_2)
//                    {
//                        pri.state_flag.quiter_2 = 1;
//                    }
//                }

//                pri.level = (byte)_session.m_pi.mi.level;

//                if (_session.m_pi.ei.char_info != null && _session.m_pi.ui.getQuitRate() < GOOD_PLAYER_ICON)
//                {
//                    pri.icon_angel = _session.m_pi.ei.char_info.AngelEquiped();
//                }
//                else
//                {
//                    pri.icon_angel = 0;
//                }

//                pri.place.ulPlace = 10; // 0x0A dec"10" _session.m_pi.place, pode ser lugar[place]
//                pri.guild_uid = _session.m_pi.gi.uid;

//                // C++ TO C# CONVERTER TASK: The memory management function 'memcpy_s' has no equivalent in C#:
//                memcpy_s(pri.guild_mark_img,
//                    Marshal.SizeOf(new char),
//                    _session.m_pi.gi.mark_emblem,
//                    Marshal.SizeOf(new char));

//                // C++ TO C# CONVERTER TASK: The memory management function 'memcpy' has no equivalent in C#:
//                memcpy(pri.guild_mark_img,
//                    _session.m_pi.gi.mark_emblem,
//                    Marshal.SizeOf(new char));
//                pri.guild_mark_index = _session.m_pi.gi.index_mark_emblem;
//                pri.uid = _session.m_pi.uid;
//                pri.state_lounge = _session.m_pi.state_lounge;
//                pri.usUnknown_flg = 0; // Ví Players com valores 2 e 4 e 0
//                pri.state = _session.m_pi.state;
//                pri.location = new PlayerRoomInfo.stLocation() { x = _session.m_pi.location.x, z = _session.m_pi.location.z, r = _session.m_pi.location.r };

//                // Personal Shop
//                pri.shop = m_personal_shop.getPersonShop(_session);

//                if (_session.m_pi.ei.mascot_info != null)
//                {
//                    pri.mascot_typeid = _session.m_pi.ei.mascot_info._typeid;
//                }

//                pri.flag_item_boost = _session.m_pi.checkEquipedItemBoost();
//                pri.ulUnknown_flg = 0;
//                //pri.id_NT não estou usando ainda
//                //pri.ucUnknown106
//                pri.convidado = 0; // Flag Convidado, [Não sei bem por que os que entra na sala normal tem valor igual aqui, já que é flag de convidado waiting], Valor constante da sala para os Players(ACHO)
//                pri.avg_score = _session.m_pi.ui.getMediaScore();
//                //pri.ucUnknown3

//                if (_session.m_pi.ei.char_info != null)
//                {
//                    pri.ci = *_session.m_pi.ei.char_info;
//                }

//                var it = m_Player_info.insert((_session, pri));

//                // Check inset pair in map of room Player info
//                if (!it.Value)
//                {

//                    if (it.first != null
//                        && it.Value.first != null
//                        && it.Value.first == (_session))
//                    {

//                        if (it.Value.Value.uid != _session.m_pi.uid)
//                        {

//                            // Add novo PlayerRoomInfo para a (session*), que tem um novo Player conectado na session.
//                            // Isso pode acontecer quando chama essa função 2x com a mesma session e o mesmo Player

//                            try
//                            {

//                                // pega o antigo PlayerRoomInfo para usar no Log
//                                var pri_ant = m_Player_info.at(_session);

//                                // Novo PlayerRoomInfo
//                                m_Player_info.at(_session) = pri;

//                                // Log de que trocou o PlayerChannelInfo da session
//                                _smp.message_pool.push(new message("[room::makePlayerInfo][WARNING][Log] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] esta trocando o PlayerRoomInfo[UID=" + Convert.ToString(pri_ant.uid) + "] do Player anterior que estava conectado com essa session, pelo o PlayerRoomInfo[UID=" + Convert.ToString(pri.uid) + "] do Player atual da session.", type_msg.CL_FILE_LOG_AND_CONSOLE));

//                            }
//                            catch (System.IndexOutOfRangeException e)
//                            {


//                                _smp.message_pool.push(new message("[room::makePlayerInfo][Error][WARNING] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "], nao conseguiu atualizar o PlayerRoomInfo da session para o novo PlayerRoomInfo do Player atual da session. Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));
//                            }

//                        }
//                        else
//                        {
//                            _smp.message_pool.push(new message("[room::makePlayerInfo][Log] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] nao conseguiu adicionar o PlayerRoomInfo da session, por que ja tem o mesmo PlayerRoomInfo no map.", type_msg.CL_FILE_LOG_AND_CONSOLE));
//                        }

//                    }
//                    else
//                    {
//                        _smp.message_pool.push(new message("[room::makePlayerInfo][Error] nao conseguiu inserir o pair de PlayerInfo do Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] no map de Player info do room. Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));
//                    }
//                }

//                return ((it.Value || (it.first != null && it.Value.first != null)) ? &it.Value.Value : null);
//            }

//            protected PlayerRoomInfoEx makePlayerInvitedInfo(Player _session)
//            {

//                PlayerRoomInfoEx pri = new PlayerRoomInfoEx();

//                // Player Room Info Init
//                pri.oid = _session.m_oid;
//                pri.position = (byte)(getPosition(_session) + 1); // posição na sala

//                pri.place.ulPlace = 10; // 0x0A dec"10" _session.m_pi.place, pode ser lugar[place]

//                pri.uid = _session.m_pi.uid;

//                pri.convidado = 1; // Flag Convidado, [Não sei bem por que os que entra na sala normal tem valor igual aqui, já que é flag de convidado waiting], Valor constante da sala para os Players(ACHO)

//                m_Player_info.Add(_session, pri);
//                var it = m_Player_info[_session];
//                // Check inset pair in map of room Player info
//                if (it != null)
//                {
//                    if (it.uid != _session.m_pi.uid)
//                    {

//                        // Add novo PlayerRoomInfo para a (session*), que tem um novo Player conectado na session.
//                        // Isso pode acontecer quando chama essa função 2x com a mesma session e o mesmo Player

//                        try
//                        {

//                            // pega o antigo PlayerRoomInfo para usar no Log
//                            var pri_ant = m_Player_info[_session];

//                            // Novo PlayerRoomInfo
//                            m_Player_info[_session] = pri;

//                            // Log de que trocou o PlayerChannelInfo da session
//                            _smp.message_pool.push(new message("[room::makePlayerInfo][WARNING][Log] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] esta trocando o PlayerRoomInfo[UID=" + Convert.ToString(pri_ant.uid) + "] do Player anterior que estava conectado com essa session, pelo o PlayerRoomInfo[UID=" + Convert.ToString(pri.uid) + "] do Player atual da session.", type_msg.CL_FILE_LOG_AND_CONSOLE));

//                        }
//                        catch (IndexOutOfRangeException e)
//                        {


//                            _smp.message_pool.push(new message("[room::makePlayerInfo][Error][WARNING] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "], nao conseguiu atualizar o PlayerRoomInfo da session para o novo PlayerRoomInfo do Player atual da session. Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));
//                        }

//                    }
//                    else
//                    {
//                        _smp.message_pool.push(new message("[room::makePlayerInfo][Log] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] nao conseguiu adicionar o PlayerRoomInfo da session, por que ja tem o mesmo PlayerRoomInfo no map.", type_msg.CL_FILE_LOG_AND_CONSOLE));
//                    }

//                    else
//                    {
//                        _smp.message_pool.push(new message("[room::makePlayerInfo][Error] nao conseguiu inserir o pair de PlayerInfo do Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] no map de Player info do room. Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));
//                    }
//                }

//                return it;
//            }

//            protected void geraSecurityKey()
//            {
//=
//                for (var i = 0; i < 16; ++i)
//                {
//                    m_ri.key[i] = (byte)((sRandomGen.getInstance().rIbeMt19937_64_chrono() % 195) + 60);
//                }
//            }

//            protected void updatePosition()
//            {



//                Dictionary<Player, PlayerRoomInfoEx>.Enumerator it;

//                for (var i = 0; i < v_sessions.Count; ++i)
//                {
//                    // C++ TO C# CONVERTER TASK: Iterators are only converted within the context of 'while' and 'for' loops:
//                    if ((it = m_Player_info.find(v_sessions[i])) != null)
//                    {
//                        // C++ TO C# CONVERTER TASK: Iterators are only converted within the context of 'while' and 'for' loops:
//                        it.Value.position = (byte)i + 1;
//                    }
//                }


//            }

//            protected void updateTrofel()
//            {

//                if (v_sessions.Count > 0
//                    && (m_ri.trofel != TROFEL_GM_EVENT_TYPEID || m_ri.max_Player <= 30)
//                    && (m_ri.time_30s > 0 && m_ri.tipo != (byte)RoomInfo.TIPO.GUILD_BATTLE)
//                    && m_ri.master != -2
//                    || (m_ri.tipo == (byte)RoomInfo.TIPO.GRAND_PRIX && m_ri.grand_prix.dados_typeid > 0))
//                {

//                    if (m_pGame != null)
//                    {
//                        m_pGame.requestUpdateTrofel();
//                    }
//                    else
//                    {

//                        uint soma = 0;
//                        v_sessions.ForEach(_el =>
//                        {
//                            if (_el != null)
//                            {
//                                soma += (_el.m_pi.level > 60) ? 60 : (_el.m_pi.level > 0 ? _el.m_pi.level - 1 : 0);
//                            }
//                        });


//                        uint new_trofel = ((((uint)v_sessions.Count) != 0) ? (sIff.getInstance().MATCH << 26) | (((((soma) + (5 - ((soma) % 5)) - 5) / ((uint)v_sessions.Count)) / 5) << 16) : 0);

//                        if (new_trofel > 0 && new_trofel != m_ri.trofel)
//                        {

//                            // Check se o trofeu anterior era o GM e se o novo não é mais, aí tira a flag de GM da sala
//                            if (m_ri.trofel == TROFEL_GM_EVENT_TYPEID && new_trofel != TROFEL_GM_EVENT_TYPEID)
//                            {
//                                m_ri.flag_gm = 0;
//                            }

//                            if (m_ri.trofel > 0)
//                            {

//                                m_ri.trofel = new_trofel;

//                                PangyaBinaryWriter p = new PangyaBinaryWriter((ushort)0x97);

//                                p.WriteUInt32(m_ri.trofel);

//                                packet_func_sv.room_broadcast(this,
//                                    p, 1);

//                            }
//                            else
//                            {
//                                m_ri.trofel = new_trofel;
//                            }
//                        }
//                    }
//                }
//            }

//            protected void updateMaster(Player _session)
//            {

//                Player master = findSessionByUID(m_ri.master);

//                if (_session != null
//                    && _session.m_pi.m_cap.game_master
//                    && m_ri.master != -2)
//                { // GM Entrou na sala

//                    // Só troca o master se ele saiu da sala ou se ele não for GM
//                    if (master == null || !(master.m_pi.m_cap.game_master))
//                    {

//                        PangyaBinaryWriter p = new PangyaBinaryWriter();

//                        m_ri.master = _session.m_pi.uid;

//                        m_ri.state_flag = 0x100; // GM

//                        if (master != null)
//                        {

//                            updatePlayerInfo(master);

//                            // State Old Master Change
//                            p.init_plain((ushort)0x78);

//                            p.WriteUInt32(master.m_oid);

//                            p.WriteByte(!getPlayerInfo(master).state_flag.ready);

//                            packet_func_sv.room_broadcast(this,
//                                p, 1);
//                        }

//                        // Master Change
//                        p.init_plain((ushort)0x7C);

//                        p.WriteUInt32(_session.m_oid);

//                        p.WriteInt16(0); // option ACHO

//                        packet_func_sv.room_broadcast(this,
//                            p, 0);
//                    }

//                }
//                else if (master == null && v_sessions.Count > 0 && m_ri.master != -2)
//                { // Master saiu da sala

//                    //só troca se for diferente de SSC e GRANDPRIX e GZ Event
//                    if (m_ri.tipo != (byte)RoomInfo.TIPO.SPECIAL_SHUFFLE_COURSE && m_ri.tipo != (byte)RoomInfo.TIPO.GRAND_PRIX)
//                    {

//                        var i = VECTOR_FIND_PTR_ITEM(v_sessions,
//                            m_pi.m_cap.game_master,               ==, 1);



//                        PangyaBinaryWriter p = new PangyaBinaryWriter((ushort)0x7C);

//                        if (i != v_sessions.Last())
//                        {
//                            master = *i;
//                        }
//                        else
//                        {
//                            master = v_sessions.First();
//                        }

//                        m_ri.master = master.m_pi.uid;

//                        if (master.m_pi.m_cap.game_master)
//                        {
//                            m_ri.state_flag = 0x100; // GM
//                        }
//                        else
//                        {
//                            m_ri.state_flag = 0; // Normal Player
//                        }

//                        updatePlayerInfo(master);

//                        p.WriteUInt32(master.m_oid);

//                        p.WriteInt16(0); // option ACHO

//                        packet_func_sv.room_broadcast(this,
//                            p, 0);
//                    }
//                }
//            }

//            protected void updateGuild(Player _session)
//            {

//                if (_session.m_pi.gi.uid == ~0u)
//                {
//                    throw new exception("[channel::UpdateGuild][Error] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] Player nao esta em uma guild.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                        9, 0));
//                }

//                PlayerRoomInfoEx pri = getPlayerInfo(_session);

//                if (pri == null)
//                {
//                    throw new exception("[channel::UpdateGuild][Error] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] nao tem o info do Player na sala[NUMERO=" + Convert.ToString(m_ri.numero) + "]. Hacker ou Bug.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                        10, 0));
//                }

//                Guild guild = null;

//                if (m_ri.guilds.guild_1_uid == 0 && m_ri.guilds.guild_2_uid != _session.m_pi.gi.uid)
//                {

//                    m_ri.guilds.guild_1_uid = _session.m_pi.gi.uid;
//                    m_ri.guilds.guild_1_nome = _session.m_pi.gi.name;
//                    m_ri.guilds.guild_1_mark = _session.m_pi.gi.mark_emblem;


//                    // Team Red
//                    pri.state_flag.team = 0;

//                    // Add a guild Vermelha
//                    guild = m_guild_manager.addGuild(Guild.eTEAM.RED, m_ri.guilds.guild_1_uid);

//                }
//                else if (m_ri.guilds.guild_1_uid == _session.m_pi.gi.uid)
//                {

//                    // Team Red
//                    pri.state_flag.team = 0;

//                    // Find Guild Vermelha
//                    guild = m_guild_manager.findGuildByTeam(Guild.eTEAM.RED);

//                }
//                else if (m_ri.guilds.guild_2_uid == 0)
//                {

//                    m_ri.guilds.guild_2_uid = _session.m_pi.gi.uid;
//                    m_ri.guilds.guild_2_nome = _session.m_pi.gi.name;
//                    m_ri.guilds.guild_2_mark = _session.m_pi.gi.mark_emblem;
//                    // Team Blue
//                    pri.state_flag.team = 1;

//                    // Add a guild Azul
//                    guild = m_guild_manager.addGuild(Guild.eTEAM.BLUE, m_ri.guilds.guild_2_uid);

//                }
//                else
//                {

//                    // Team Blue
//                    pri.state_flag.team = 1;

//                    // Find Guild Azul
//                    guild = m_guild_manager.findGuildByTeam(Guild.eTEAM.BLUE);
//                }

//                if (guild != null)
//                {
//                    guild.addPlayer(_session);
//                }
//                else
//                {
//                    _smp.message_pool.push(new message("[room::updateGuild][WARNING] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou entrar em uma guild da sala[NUMERO=" + Convert.ToString(m_ri.numero) + "], mas nao conseguiu criar ou achar nenhum guild na sala. Bug.", type_msg.CL_FILE_LOG_AND_CONSOLE));
//                }

//                // Add Player ao team
//                m_teans[pri.state_flag.team].addPlayer(_session);
//            }

//            // Jogador Chutado da sala
//            protected void clear_Player_kicked()
//            {

//                if (m_Player_kicked.Any())
//                {
//                    m_Player_kicked.Clear();
//                }
//            }

//            protected void addPlayerKicked(uint _uid)
//            {

//                if (isKickedPlayer(_uid))
//                {
//                    _smp.message_pool.push(new message("[room::addPlayerKicked][Error][WARNING] Player[UID=" + Convert.ToString(_uid) + "] ja foi chutado da sala[NUMERO=" + Convert.ToString(m_ri.numero) + "]", type_msg.CL_FILE_LOG_AND_CONSOLE));
//                }
//                else
//                {
//                    m_Player_kicked[_uid] = true;
//                }
//            }

//// Game
//protected virtual void finish_game()
//        {

//            if (m_pGame != null)
//            {
//                PangyaBinaryWriter p = new PangyaBinaryWriter();

//                // Deleta o jogo
//                m_pGame = null;

//                // Zera Player Flags
//                foreach (var el in m_Player_info)
//                {

//                    // Update Place Player
//                    if (m_ri.tipo == (byte)RoomInfo.TIPO.PRACTICE || m_ri.tipo == (byte)RoomInfo.TIPO.GRAND_ZODIAC_PRACTICE)
//                    {
//                        el.Value.place.ulPlace = 2;
//                    }
//                    else
//                    {
//                        el.Value.place.ulPlace = 0;
//                    }

//                    el.Value.state_flag.away = 0;

//                    // Aqui só zera quem não é Master da sala, o master deixa sempre ready
//                    if (m_ri.master == el.Key.m_pi.uid)
//                    {
//                        el.Value.state_flag.ready = 1;
//                    }
//                    else
//                    {
//                        el.Value.state_flag.ready = 0;
//                    }

//                    // Update Player info
//                    updatePlayerInfo(el.Key);

//                    // SLast update on room
//                    sendCharacter(el.Key, 3);
//                }

//                // Atualiza flag da sala, só não atualiza se for GM evento ou GZ Event e SSC
//                if (!(m_ri.trofel == TROFEL_GM_EVENT_TYPEID || (m_ri.tipo == RoomInfo.SPECIAL_SHUFFLE_COURSE || m_ri.master == -2))
//                        {
//                    m_ri.state = 1; //em espera
//                }

//                // Att Exp rate, e Pang rate, que criou a sala, att ele também quando começa o jogo
//                //if (sgs::gs != nullptr) {
//                m_ri.rate_exp = (int)Program.gs.getInfo().rate.exp;
//                m_ri.rate_pang = (int)Program.gs.getInfo().rate.pang;
//                m_ri.angel_event = Program.gs.getInfo().rate.angel_event.IsTrue();
//                //}

//                // Update Course of Hole
//                if (m_ri.course >= RoomInfo.eCOURSE.UNK) // Random Course With Course already draw
//                {
//                    m_ri.course = RoomInfo.eCOURSE.UNK; // Random Course standard
//                }

//                // Update Master da sala
//                updateMaster(null);

//                if (m_ri.master == -2)
//                {
//                    m_ri.master = -1; // pode deletar a sala quando sair todos
//                }

//                if (v_sessions.Count > 0)
//                {
//                    // Atualiza info da sala para quem está na sala
//                    packet_func_sv.pacote04A(p,
//                        m_ri, -1);
//                    packet_func_sv.room_broadcast(this,
//                        p, 1);
//                }

//                // limpa lista de Player kikados
//                clear_Player_kicked();

//                // Verifica se o Bot Tourney está ativo, kika bot e limpa a flag
//                if (m_bot_tourney)
//                {

//                    var pMaster = findMaster();

//                    if (pMaster != null)
//                    {

//                        try
//                        {
//                            // Kick Bot
//                            // Atualiza os Player que estão na sala que o Bot sai por que ele é só visual
//                            sendCharacter(pMaster, 0);

//                        }
//                        catch (exception e)
//                        {

//                            _smp.message_pool.push(new message("[room::finish_game::KickBotTourney][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
//                        }
//                    }

//                    m_bot_tourney = false;
//                }

//                // Terminou o jogo
//                m_pGame = null;
//            }
//        }

//        // Invite
//        protected void clear_invite()
//        {

//            if (v_invite.Any())
//            {
//                v_invite.Clear();
//            }
//        }

//        // Team
//        protected void init_teans()
//        {

//            // Limpa teans, se tiver teans inicilizados já
//            clear_teans();

//            // Init Teans
//            m_teans.Add(new Team(0));
//            m_teans.Add(new Team(1));

//            PlayerRoomInfo pPri = null;

//            // Add Players All Seus Respectivos teans
//            foreach (var el in v_sessions)
//            {

//                if ((pPri = getPlayerInfo(el)) == null)
//                {
//                    throw new exception("[room::init_teans][Error] nao encontrou o info do Player[UID=" + Convert.ToString(el.m_pi.uid) + "] na sala. Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
//                        1504, 0));
//                }

//                m_teans[pPri.state_flag.team].addPlayer(el);
//            }
//        }
//    }
//}