using GameServer.GameType;
using PangyaAPI.Utilities;
using System.Collections.Generic;
using System.Linq;
using static GameServer.GameType._Define;
using System;
using _smp = PangyaAPI.Utilities.Log;
using GameServer.Session;
using PangyaAPI.Utilities.BinaryModels;
using PangyaAPI.Utilities.Log;
using GameServer.Game.System;
using GameServer.Game.Manager;
using GameServer.PangyaEnums;
using GameServer.PacketFunc;
using PangyaAPI.Network.PangyaPacket;
using PangyaAPI.Network.Pangya_St;
using PangyaAPI.SQL.Manager;
using GameServer.Cmd;
using System.Runtime.InteropServices;
using System.Reflection;
using GameServer.Game.Utils;
using PangyaAPI.SQL;
using System.Runtime.Remoting.Channels;

namespace GameServer.Game
{

    public class room
    {
        protected List<Player> v_sessions = new List<Player>();
        protected Dictionary<Player, PlayerRoomInfoEx> m_Player_info = new Dictionary<Player, PlayerRoomInfoEx>();
        protected Dictionary<uint, bool> m_Player_kicked = new Dictionary<uint, bool>();

        protected PersonalShopManager m_personal_shop;

        protected List<Team> m_teans = new List<Team>();

        protected GuildRoomManager m_guild_manager = new GuildRoomManager();

        protected List<InviteChannelInfo> v_invite = new List<InviteChannelInfo>();


        protected RoomInfoEx m_ri = new RoomInfoEx();

        protected byte m_channel_owner; // Id do Canal dono da sala

        protected bool m_bot_tourney; // Bot para começa o Modo tourney só com 1 jogador
        private int m_lock_spin_state;
        protected bool m_destroying;

        protected Game m_pGame;
        // Room Tipo Lounge
        protected byte m_weather_lounge;
        public room(byte _channel_owner, RoomInfoEx _ri)
        {
            this.m_ri = _ri;
            this.m_pGame = null;
            this.m_channel_owner = _channel_owner;
            this.m_teans = new List<Team>();
            this.m_weather_lounge = 0;
            this.m_destroying = false;
            this.m_bot_tourney = false;
            this.m_lock_spin_state = 0;
            this.m_personal_shop = new PersonalShopManager(m_ri);

            geraSecurityKey();

            // Calcula chuva(weather) se o tipo da sala for lounge
            calcRainLounge();

            // Atualiza tipo da sala
            setTipo(m_ri.tipo);

            // Att Exp rate, e Pang rate, que criou a sala, att ele também quando começa o jogo
            //if (sgs::gs != nullptr) {
            m_ri.rate_exp = (uint)sgs.gs.getInstance().getInfo().rate.exp;
            m_ri.rate_pang = (uint)sgs.gs.getInstance().getInfo().rate.pang;
            m_ri.angel_event = sgs.gs.getInstance().getInfo().rate.angel_event == 1 ? true : false;
            //}
        }

        public void destroy()
        {

            // Leave All Players
            leaveAll(0);



            if (m_pGame != null)
            {
                m_pGame = null;
            }

            m_pGame = null;

            m_channel_owner = 255;

            m_weather_lounge = 0;

            if (v_sessions.Any())
            {
                v_sessions.Clear();
            }

            if (m_Player_info.Any())
            {
                m_Player_info.Clear();
            }

            clear_invite();

            clear_Player_kicked();

            clear_teans();

            m_bot_tourney = false;



            m_personal_shop.destroy();

            // Destruindo a sala
            try
            {

                @lock();

                m_destroying = true;

                unlock();

            }
            catch (exception e)
            {

                if (!ExceptionError.STDA_ERROR_CHECK_SOURCE_AND_ERROR_TYPE(e.getCodeError(),
                    STDA_ERROR_TYPE.ROOM, 150))
                {

                    unlock();

                    _smp.message_pool.push(new message("[room::destroy][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
                }
            }
        }

        public void enter(Player _session)
        {

            if (!_session.getState())
            {
                throw new exception("[room::enter][Error] Player nao esta connectado.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                    4, 0));
            }



            if (isFull())
            {
                throw new exception("[room::enter][Error] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou entrar na a sala[NUMERO=" + Convert.ToString(m_ri.numero) + "], mas a sala ja esta cheia.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                    2, 0));
            }

            if (_session.m_pi.mi.sala_numero != ushort.MaxValue)
            {
                throw new exception("[room::enter][Error] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] sala[NUMERO=" + Convert.ToString(m_ri.numero) + "], ja esta em outra sala[NUMERO=" + Convert.ToString(_session.m_pi.mi.sala_numero) + "], nao pode entrar em outra. Hacker ou Bug.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                    120, 0));
            }

            if (m_ri.tipo == (byte)RoomInfo.TIPO.GUILD_BATTLE
                && m_ri.guilds.guild_1_uid != 0
                && m_ri.guilds.guild_2_uid != 0
                && m_ri.guilds.guild_1_uid != _session.m_pi.gi.uid
                && m_ri.guilds.guild_2_uid != _session.m_pi.gi.uid)
            {
                throw new exception("[room::enter][Error] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] sala[NUMERO=" + Convert.ToString(m_ri.numero) + "], ja tem duas guild e o Player que quer entrar nao eh de nenhum delas. Hacker ou Bug.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                    11000, 0));
            }

            try
            {

                _session.m_pi.mi.sala_numero = m_ri.numero;

                // Update Place Player
                if (m_ri.tipo == (byte)RoomInfo.TIPO.PRACTICE || m_ri.tipo == (byte)RoomInfo.TIPO.GRAND_ZODIAC_PRACTICE)
                {
                    _session.m_pi.place = 2;
                }
                else
                {
                    _session.m_pi.place = 0;
                }

                v_sessions.Add(_session);

                ++m_ri.num_Player;

                // Update Trofel
                if (m_ri.trofel > 0)
                {
                    updateTrofel();
                }

                // Acabou de criar a sala
                if (m_ri.master == _session.m_pi.uid && m_ri.tipo != (byte)RoomInfo.TIPO.GRAND_PRIX)
                {
                    // Update Trofel
                    if (_session.m_pi.m_cap.game_master)
                    { // GM

                        if ((m_ri.max_Player > 30 && m_ri.tipo == (byte)RoomInfo.TIPO.TOURNEY) || (m_ri.tipo >= (byte)RoomInfo.TIPO.GRAND_ZODIAC_INT && m_ri.tipo <= (byte)RoomInfo.TIPO.GRAND_ZODIAC_ADV))
                        {

                            m_ri.flag_gm = 1;

                            m_ri.state_flag = 0x100;

                            m_ri.trofel = TROFEL_GM_EVENT_TYPEID;

                        }
                        else if (m_ri.tipo == (byte)RoomInfo.TIPO.TOURNEY || m_ri.tipo >= (byte)RoomInfo.TIPO.GRAND_ZODIAC_INT)
                        {
                            updateTrofel();
                        }

                    }
                    else if (m_ri.tipo == (byte)RoomInfo.TIPO.TOURNEY || m_ri.tipo >= (byte)RoomInfo.TIPO.GRAND_ZODIAC_INT)
                    {
                        updateTrofel();
                    }

                }
                else if (m_ri.tipo == (byte)RoomInfo.TIPO.GRAND_PRIX)
                {
                    updateTrofel();
                }

                // Update Master
                // Só trocar o master da sala se não tiver nenhum jogo inicializado
                if (m_pGame == null
                    && v_sessions.Count > 0
                    && _session.m_pi.m_cap.game_master
                    && m_ri.state_flag != 0x100
                    && m_ri.tipo != (byte)RoomInfo.TIPO.SPECIAL_SHUFFLE_COURSE
                    && m_ri.tipo != (byte)RoomInfo.TIPO.GRAND_PRIX)
                {
                    updateMaster(_session);
                }

                // Add o Player ao jogo
                if (m_pGame != null)
                {
                    m_pGame.addPlayer(_session);

                    if (m_ri.trofel > 0)
                    {
                        updateTrofel();
                    }
                }

                try
                {
                    // Make Info Room Player
                    makePlayerInfo(_session);

                }
                catch (exception e)
                {
                    _smp.message_pool.push(new message("[room::enter][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
                }

                if (m_ri.tipo == (byte)RoomInfo.TIPO.GUILD_BATTLE)
                {
                    updateGuild(_session);
                }

            }
            catch (exception e)
            {
                _smp.message_pool.push(new message("[room::enter][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
        }

        public int leave(Player _session, int _option)
        {

            //if (!_session.getState() /*&& _option != 3/*Force*/)
            //throw new exception("[room::leave][Error] Player nao esta connectado.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE::ROOM, 4, 0));

            try
            {
                int index = findIndexSession(_session);

                if (index == (int)~0)
                {


                    throw new exception("[room::leave][Error] session[UID=" + Convert.ToString(_session.m_pi.uid) + "] nao existe no vector de sessions da sala[NUMERO=" + Convert.ToString(m_ri.numero) + "].", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                        5, 0));
                }

                if (_option != 0
                    && _option != 1
                    && _option != 0x800
                    && _option != 10)
                {
                    addPlayerKicked(_session.m_pi.uid);
                }

                // Verifica se ele está em um jogo e tira ele
                try
                {

                    if (m_pGame != null)
                    {
                        if (m_pGame.deletePlayer(_session, _option) && m_pGame.finish_game(_session, 2))
                        {
                            finish_game();
                        }
                    }

                }
                catch (exception e)
                {
                    _smp.message_pool.push(new message("[room::leave][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
                }

                v_sessions.RemoveAt(index);

                if ((m_ri.num_Player - 1) > 0 || v_sessions.Count == 0)
                {
                    --m_ri.num_Player;
                }

                // Sai do Team se for Match
                if (m_ri.tipo == (byte)RoomInfo.TIPO.MATCH)
                {

                    if (m_teans.Count < 2)
                    {
                        throw new exception("[room::leave][Error] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou sair da sala[NUMERO=" + Convert.ToString(m_ri.numero) + "], mas a sala nao tem os 2 teans(times). Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                            1502, 0));
                    }

                    var pPri = getPlayerInfo(_session);

                    if (pPri == null)
                    {
                        throw new exception("[room::leave][Error] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou sair da sala[NUMERO=" + Convert.ToString(m_ri.numero) + "], mas a sala nao encontrou o info do Player. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                            1503, 0));
                    }

                    m_teans[pPri.state_flag.team].deletePlayer(_session, _option);
                }
                else if (m_ri.tipo == (byte)RoomInfo.TIPO.GUILD_BATTLE)
                {

                    var pPri = getPlayerInfo(_session);

                    if (pPri == null)
                    {
                        throw new exception("[room::leave][Error] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou sair da sala[NUMERO=" + Convert.ToString(m_ri.numero) + "], mas a sala nao encontrou o info do Player. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                            1503, 0));
                    }

                    var guild = m_guild_manager.findGuildByPlayer(_session);

                    if (guild == null)
                    {
                        throw new exception("[room::leave][Error] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou sair da sala[NUMERO=" + Convert.ToString(m_ri.numero) + "], mas o Player nao esta em nenhuma guild da sala. Hacker ou Bug.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                            1504, 0));
                    }

                    // Deleta o Player da guild  da sala
                    guild.deletePlayer(_session);

                    // Deleta Player do team
                    m_teans[pPri.state_flag.team].deletePlayer(_session, _option);

                    // Limpa o team do Player
                    pPri.state_flag.team = 0;

                    // Limpa guild
                    if (guild.numPlayers() == 0)
                    {

                        if (guild.getTeam() == Guild.eTEAM.RED)
                        {

                            // Red
                            m_ri.guilds.guild_1_uid = 0;
                            m_ri.guilds.guild_1_index_mark = 0;

                        }
                        else
                        {

                            // Blue
                            m_ri.guilds.guild_2_uid = 0;
                            m_ri.guilds.guild_2_index_mark = 0;

                        }

                        //delete Guild
                        m_guild_manager.deleteGuild(guild);
                    }
                }

                // Delete Player Info
                m_Player_info.Remove(_session);

                // reseta(default) o número da sala no info do Player
                _session.m_pi.mi.sala_numero = ushort.MaxValue;
                _session.m_pi.place = 0;

                // Excluiu personal shop do Player se ele estiver com shop aberto
                m_personal_shop.destroyShop(_session);

                updatePosition();

                updateTrofel();

                // Isso é para o cliente saber que ele foi kickado pelo server sem ação de outro Player
                if (_option == 0x800 || (_option != 0 && _option != 1 && _option != 3))
                {

                    uint opt_kick = 0x800;

                    switch (_option)
                    {
                        case 1:
                            opt_kick = 4;
                            break;
                        case 2:
                            opt_kick = 2;
                            break;
                        default:
                            opt_kick = (uint)_option;
                            break;
                    }

                    PangyaBinaryWriter p = new PangyaBinaryWriter((ushort)0x7E);

                    p.WriteUInt32(opt_kick);

                    packet_func_sv.session_send(p,
                        _session, 1);
                }

                if (m_ri.tipo == (byte)RoomInfo.TIPO.LOUNGE)
                { // Zera State lounge of Player

                    _session.m_pi.state = 0;
                    _session.m_pi.state_lounge = 0;
                }

                // Update Players State On Room
                if (v_sessions.Count > 0)
                {
                    sendUpdate();

                    sendCharacter(_session, 2);
                }
                // Fim Update Players State

                if ((m_pGame == null && m_ri.tipo == (byte)RoomInfo.TIPO.SPECIAL_SHUFFLE_COURSE && _session.m_pi.uid == m_ri.master) || (_session.m_pi.m_cap.game_master && m_ri.master == _session.m_pi.uid && m_ri.tipo != (byte)RoomInfo.TIPO.LOUNGE && m_ri.trofel == TROFEL_GM_EVENT_TYPEID))
                {



                    return 0x801; // deleta todos da sala

                }
                else if (m_pGame == null)
                {
                    updateMaster(null); // update Master
                }

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[room::leave][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }



            return (v_sessions.Count > 0 || (m_ri.master == -2 && (!isDropRoom() || m_ri.tipo >= (byte)RoomInfo.TIPO.GRAND_ZODIAC_INT && m_ri.tipo <= (byte)RoomInfo.TIPO.GRAND_ZODIAC_ADV))) ? 0 : 1;
        }

        protected void calcRainLounge()
        {

            // Só calcRainLounge se for lounge
            if (m_ri.tipo == (byte)RoomInfo.TIPO.LOUNGE)
            {

                m_weather_lounge = 0; // Good Weather

                short rate_rain = sgs.gs.getInstance().getInfo().rate.chuva;

                Lottery loterry = new Lottery();

                uint rate_good_weather = (uint)((rate_rain <= 0) ? 1000 : ((rate_rain < 1000) ? 1000 - rate_rain : 1));

                loterry.Push(rate_good_weather, 0);
                loterry.Push(rate_good_weather, 0);
                loterry.Push(rate_good_weather, 0);
                loterry.Push((uint)rate_rain, 2);

                var lc = loterry.SpinRoleta();

                if (lc != null)
                {
                    m_weather_lounge = (byte)lc.Value;
                }
            }
        }


        protected void clear_teans()
        {
            if (m_teans.Any())
            {
                m_teans.Clear();
            }
        }

        // Add Bot Tourney Visual to Room
        protected void addBotVisual(Player _session)
        {
            if (!_session.getState())
            {
                throw new exception("[room::" + "addBotVisual" + "][Error] Player nao esta connectado", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                    12, 0));
            }

            // Add Bot
            List<PlayerRoomInfoEx> v_element = new List<PlayerRoomInfoEx>();
            PlayerRoomInfoEx pri = new PlayerRoomInfoEx();
            PlayerRoomInfoEx tmp_pri = null;

            try
            {


                v_sessions.ForEach(_el =>
                {
                    tmp_pri = getPlayerInfo(_el);
                    if (tmp_pri != null)
                    {
                        v_element.Add(tmp_pri);
                    }
                });


                if (v_element.Count == 0)
                {
                    throw new exception("[room::makeBot][Error] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou criar Bot na sala[NUMERO=" + Convert.ToString(m_ri.numero) + ", MASTER=" + Convert.ToString(m_ri.master) + "], mas nao nenhum Player na sala. Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                        1, 5000));
                }

                // Inicializa os dados do Bot
                pri.uid = _session.m_pi.uid;
                pri.oid = _session.m_oid;
                pri.position = 0; // 0 Que é para ele ficar em primeiro e parece que tbm não deixa kick(ACHO)
                pri.state_flag.ready = 1;
                pri.char_typeid = 0x4000000; // Nuri
                pri.title = 0x39800013; // Title Helper
                pri.nickname = "\\1Bot";
                // Add o Bot a sala, só no visual
                v_element.Add(pri);

                // Packet
                PangyaBinaryWriter p = new PangyaBinaryWriter();

                // Option 0, passa todos que estão na sala
                packet_func_sv.room_broadcast(this, packet_func_sv.pacote048(_session, v_element, 0x100), 1);

                // Option 1, passa só o Player que entrou na sala, nesse caso foi o Bot
                packet_func_sv.room_broadcast(this, packet_func_sv.pacote048(_session, new List<PlayerRoomInfoEx> { pri }, 0x101), 1);


                // Criou Bot com sucesso
                m_bot_tourney = true;

                // Log
                _smp.message_pool.push(new message("[room::addBotVisual][Log] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] Room[NUMBER=" + Convert.ToString(m_ri.numero) + ", MASTER=" + Convert.ToString(m_ri.master) + "] Bot criado com sucesso.", type_msg.CL_FILE_LOG_AND_CONSOLE));



            }
            catch (exception e)
            {

                // Relança
                throw;
            }
        }

        // Para as classes filhas, empedir que exclua a sala depLastLasto do se tem Player ou não na sala
        protected virtual bool isDropRoom()
        {
            return true; // class room normal é sempre true
        }

        // protected porque é um método inseguro (sem thread safety)
        protected uint _getRealNumPlayersWithoutInvited()
        {
            return (uint)v_sessions.Count(_el =>
            {
                if (_el == null)
                    return false;

                return m_Player_info.TryGetValue(_el, out var playerInfo) && !(playerInfo.convidado == 1);
            });
        }


        // protected por que é o método unsave(inseguro), sem thread safe
        protected bool _haveInvited()
        {
            return v_sessions.Any(_el =>
            {
                if (_el == null)
                    return false;

                return m_Player_info.TryGetValue(_el, out var playerInfo) && playerInfo.convidado == 1;
            });
        }


        // Game
        protected virtual void finish_game()
        {

            if (m_pGame != null)
            {
                PangyaBinaryWriter p = new PangyaBinaryWriter();

                // Deleta o jogo
                m_pGame = null;

                // Zera Player Flags
                foreach (var el in m_Player_info)
                {

                    // Update Place Player
                    if (m_ri.tipo == (byte)RoomInfo.TIPO.PRACTICE || m_ri.tipo == (byte)RoomInfo.TIPO.GRAND_ZODIAC_PRACTICE)
                    {
                        el.Value.place.ulPlace = 2;
                    }
                    else
                    {
                        el.Value.place.ulPlace = 0;
                    }

                    el.Value.state_flag.away = 0;

                    // Aqui só zera quem não é Master da sala, o master deixa sempre ready
                    if (m_ri.master == el.Key.m_pi.uid)
                    {
                        el.Value.state_flag.ready = 1;
                    }
                    else
                    {
                        el.Value.state_flag.ready = 0;
                    }

                    // Update Player info
                    updatePlayerInfo(el.Key);

                    // SLast update on room
                    sendCharacter(el.Key, 3);
                }

                // Atualiza flag da sala, só não atualiza se for GM evento ou GZ Event e SSC
                if (!(m_ri.trofel == TROFEL_GM_EVENT_TYPEID || (m_ri.tipo == (byte)RoomInfo.TIPO.SPECIAL_SHUFFLE_COURSE || m_ri.master == -2)))
                {
                    m_ri.state = 1; //em espera
                }

                // Att Exp rate, e Pang rate, que criou a sala, att ele também quando começa o jogo
                //if (sgs::gs != nullptr) {
                m_ri.rate_exp = (uint)sgs.gs.getInstance().getInfo().rate.exp;
                m_ri.rate_pang = (uint)sgs.gs.getInstance().getInfo().rate.pang;
                m_ri.angel_event = sgs.gs.getInstance().getInfo().rate.angel_event.IsTrue();
                //}

                // Update Course of Hole
                if (m_ri.course >= RoomInfo.eCOURSE.UNK) // Random Course With Course already draw
                {
                    m_ri.course = RoomInfo.eCOURSE.UNK; // Random Course standard
                }

                // Update Master da sala
                updateMaster(null);

                if (m_ri.master == -2)
                {
                    m_ri.master = -1; // pode deletar a sala quando sair todos
                }

                if (v_sessions.Count > 0)
                {
                    // Atualiza info da sala para quem está na sala
                    packet_func_sv.pacote04A(p,
                        m_ri, -1);
                    packet_func_sv.room_broadcast(this,
                        p, 1);
                }

                // limpa lista de Player kikados
                clear_Player_kicked();

                // Verifica se o Bot Tourney está ativo, kika bot e limpa a flag
                if (m_bot_tourney)
                {

                    var pMaster = findMaster();

                    if (pMaster != null)
                    {

                        try
                        {
                            // Kick Bot
                            // Atualiza os Player que estão na sala que o Bot sai por que ele é só visual
                            sendCharacter(pMaster, 0);

                        }
                        catch (exception e)
                        {

                            _smp.message_pool.push(new message("[room::finish_game::KickBotTourney][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
                        }
                    }

                    m_bot_tourney = false;
                }

                // Terminou o jogo
                m_pGame = null;
            }
        }

        // Invite
        protected void clear_invite()
        {

            if (v_invite.Any())
            {
                v_invite.Clear();
            }
        }

        // Team
        protected void init_teans()
        {

            // Limpa teans, se tiver teans inicilizados já
            clear_teans();

            // Init Teans
            m_teans.Add(new Team(0));
            m_teans.Add(new Team(1));

            PlayerRoomInfo pPri = null;

            // Add Players All Seus Respectivos teans
            foreach (var el in v_sessions)
            {

                if ((pPri = getPlayerInfo(el)) == null)
                {
                    throw new exception("[room::init_teans][Error] nao encontrou o info do Player[UID=" + Convert.ToString(el.m_pi.uid) + "] na sala. Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                        1504, 0));
                }

                m_teans[pPri.state_flag.team].addPlayer(el);
            }

        }


        public int leaveAll(int _option)
        {

            while (!v_sessions.empty())
            {

                try
                {
                    leave(v_sessions.begin(), _option);
                }
                catch (exception e)
                {

                    _smp.message_pool.push(new message("[room::leaveAll][Error] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
                }
            }

            return 0;
        }

        public bool isInvited(Player _session)
        {

            var it = m_Player_info.find(_session);

            return (it.Value != null && it.Value.convidado == 1);
        }

        public InviteChannelInfo addInvited(uint _uid_has_invite, Player _session)
        {

            if (!_session.getState())
            {
                throw new exception("[room::addInvited][Error] Player nao esta connectado.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                    4, 0));
            }

            if (isFull())
            {
                throw new exception("[room::addInvited][Error] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou entrar na a sala[NUMERO=" + Convert.ToString(m_ri.numero) + "], mas a sala ja esta cheia.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                    2, 0));
            }

            if (findIndexSession(_uid_has_invite) == (int)~0)
            {
                throw new exception("[room::addInvited][Error] quem convidou[UID=" + Convert.ToString(_uid_has_invite) + "] o Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] para a sala nao esta na sala. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                    2010, 0));
            }

            var s = findSessionByUID(_session.m_pi.uid);

            if (s != null)
            {
                throw new exception("[room::addInvited][Error] Player[UID=" + Convert.ToString(_uid_has_invite) + "] tentou adicionar o convidado[UID=" + Convert.ToString(_session.m_pi.uid) + "] a sala, mas ele ja esta na sala. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                    2001, 0));
            }



            _session.m_pi.mi.sala_numero = m_ri.numero;

            _session.m_pi.place = 70; // Está sendo convidado

            v_sessions.Add(_session);

            ++m_ri.num_Player;

            PlayerRoomInfoEx pri = null;

            try
            {

                // Make Info Room Player Invited
                pri = makePlayerInvitedInfo(_session);

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[room::addInvited][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }

            if (pri == null)
            {

                // Pop_back
                v_sessions.Remove(v_sessions.Last());



                throw new exception("[[room::addInvited][Error] Player[UID=" + Convert.ToString(_uid_has_invite) + "] tentou adicionar o convidado[UID=" + Convert.ToString(_session.m_pi.uid) + "] a sala, nao conseguiu criar o Player Room Info Invited do Player. Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                    2002, 0));
            }

            // Add Invite Channel Info
            InviteChannelInfo ici = new InviteChannelInfo();

            ici.room_number = m_ri.numero;

            ici.invite_uid = _uid_has_invite;
            ici.invited_uid = _session.m_pi.uid;

            ici.time.CreateTime();

            v_invite.Add(ici);
            // End Add Invite Channel Info

            // Update Char Invited ON ROOM
            PangyaBinaryWriter p = new PangyaBinaryWriter((ushort)0x48);

            p.WriteByte(1);
            p.WriteInt16(-1);

            p.WriteBytes(pri.Build());

            p.WriteByte(0); // Final Packet

            packet_func_sv.room_broadcast(this,
                p, 1);



            return ici;
        }

        public InviteChannelInfo deleteInvited(Player _session)
        {

            // Por que se o Player não estiver mais online não pode deletar o convidado
            //if (!_session.getState())
            //throw new exception("[room::deleteInvited][Error] Player nao esta connectado.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE::ROOM, 4, 0));

            var it = m_Player_info.First(c => c.Key == _session);

            if (it.Value == m_Player_info.Last().Value)
            {
                throw new exception("[room::deleteInvited][Error] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou deletar convidado," + " mas nao tem o info do convidado na sala. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                    2003, 0));
            }



            int index = findIndexSession(_session);

            if (index == (int)~0)
            {


                throw new exception("[room::deleteInvited][Error] session[UID=" + Convert.ToString(_session.m_pi.uid) + "] nao existe no vector de sessions da sala.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                    5, 0));
            }

            _session.m_pi.mi.sala_numero = ushort.MaxValue;

            _session.m_pi.place = 0; // Limpa Está sendo convidado

            v_sessions.RemoveAt(index);

            --m_ri.num_Player;

            m_Player_info.Remove(it.Key);

            // Update Position all Players
            updatePosition();

            // Delete Invite Channel Info
            InviteChannelInfo ici = new InviteChannelInfo();

            
            var itt = v_invite.FirstOrDefault(_el =>
            {
                return (_el.room_number == m_ri.numero && _el.invited_uid == _session.m_pi.uid);
            });

            if (itt != v_invite.Last())
            {

                ici = itt;

                v_invite.Remove(itt);

            }
            else
            {
                _smp.message_pool.push(new message("[room::deleteInvited][WARNING] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] nao tem um convite.", type_msg.CL_FILE_LOG_AND_CONSOLE));
            }

            // End Delete Invite Channel Info

            // Resposta Delete Convidado
            PangyaBinaryWriter p = new PangyaBinaryWriter((ushort)0x130);

            p.WriteUInt32(_session.m_pi.uid);

            packet_func_sv.room_broadcast(this,
                p, 1);

            _smp.message_pool.push(new message("[room::deleteInvited][Log] Deleteou um convite[Convidado=" + Convert.ToString(_session.m_pi.uid) + "] na Sala[NUMERO=" + Convert.ToString(m_ri.numero) + "]", type_msg.CL_FILE_LOG_AND_CONSOLE));


            return ici;
        }

        public InviteChannelInfo deleteInvited(uint _uid)
        {

            if (_uid == 0u)
            {
                throw new exception("[room::deleteInvited][Error] _uid is invalid(zero). Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                    2005, 0));
            }

            var it = m_Player_info.FirstOrDefault(_el =>
           {
               return (_el.Value.convidado == 1 && _el.Value.uid == _uid);
           });

            if (it.Key == m_Player_info.Last().Key)
            {
                throw new exception("[room::deleteInvited][Error] Player[UID=" + Convert.ToString(_uid) + "] tentou deletar convidado," + " mas nao tem o info do convidado na sala. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                    2003, 0));
            }



            int index = findIndexSession(_uid);

            if (index == (int)~0)
            {


                throw new exception("[room::deleteInvited][Error] session[UID=" + Convert.ToString(_uid) + "] nao existe no vector de sessions da sala.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                    5, 0));
            }

            v_sessions.RemoveAt(index);

            --m_ri.num_Player;

            m_Player_info.Remove(it.Key);

            // Update Position all Players
            updatePosition();

            // Delete Invite Channel Info
            InviteChannelInfo ici = new InviteChannelInfo();

            var itt = v_invite.FirstOrDefault(_el => _el.room_number == m_ri.numero && _el.invited_uid == _uid);


            if (itt != null)
            {

                ici = itt;

                v_invite.Remove(itt);

            }
            else
            {
                _smp.message_pool.push(new message("[room::deleteInvited][WARNING] Player[UID=" + Convert.ToString(_uid) + "] nao tem um convite.", type_msg.CL_FILE_LOG_AND_CONSOLE));
            }

            // End Delete Invite Channel Info

            // Resposta Delete Convidado
            PangyaBinaryWriter p = new PangyaBinaryWriter((ushort)0x130);

            p.WriteUInt32(_uid);

            packet_func_sv.room_broadcast(this,
                p, 1);

            _smp.message_pool.push(new message("[room::deleteInvited][Log] Deleteou um convite[Convidado=" + Convert.ToString(_uid) + "] na Sala[NUMERO=" + Convert.ToString(m_ri.numero) + "]", type_msg.CL_FILE_LOG_AND_CONSOLE));

            return ici;
        }

        public RoomInfoEx getInfo()
        {
            return m_ri;
        }

        // Gets
        public byte getChannelOwenerId()
        {
            return m_channel_owner;
        }

        public short getNumero()
        {
            return (short)m_ri.numero;
        }

        public uint getMaster()
        {
            return (uint)m_ri.master;
        }

        public uint getNumPlayers()
        {
            return m_ri.num_Player;
        }

        public uint getPosition(Player _session)
        {
            var position = ~0;

            for (var i = 0; i < v_sessions.Count; ++i)
            {
                if (v_sessions[i] == _session)
                {
                    position = i;
                    break;
                }
            }
            return (uint)position;
        }

        public PlayerRoomInfoEx getPlayerInfo(Player _session)
        {

            if (_session == null)
            {
                throw new exception("Error _session is nullptr. Em room::getPlayerInfo()", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                    7, 0));
            }

            PlayerRoomInfoEx pri = m_Player_info.Values.First(c => c.uid == _session.getUID());

            if (pri == null)
            {
                throw new exception("Error pri is nullptr. Em room::getPlayerInfo()", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                    7, 0));
            }


            return pri;
        }

        public List<Player> getSessions(Player _session = null, bool _with_invited = true)
        {
            List<Player> v_session = new List<Player>();



            foreach (var el in v_sessions)
            {
                if (el != null
                    && el.getState()
                    && el.m_pi.mi.sala_numero != ushort.MaxValue
                    && (_session == null || _session != el)
                    && (_with_invited || !isInvited(el)))
                {
                    v_session.Add(el);
                }
            }



            return new List<Player>(v_session);
        }

        public uint getRealNumPlayersWithoutInvited()
        {

            uint num = 0;



            try
            {

                num = _getRealNumPlayersWithoutInvited();

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[room::getRealNumPlayerWithoutInvited][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }



            return (num);
        }

        public bool haveInvited()
        {

            bool question = false;



            try
            {

                question = _haveInvited();

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[room::haveInvited][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }



            return question;
        }

        // Sets
        public void setNome(string _nome)
        {

            if (_nome.Length == 0)
            {
                throw new exception("Error _nome esta vazio. Em room::setNome()", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                    6, 0));
            }
            m_ri.nome = _nome;
        }

        public void setSenha(string _senha)
        {

            if (_senha.Length == 0)
            {
                if (!(m_ri.senha_flag == 1))
                {
                    m_ri.senha = "";
                    m_ri.senha_flag = 1;
                }
            }
            else
            {
                m_ri.senha = _senha;
                m_ri.senha_flag = 0;
            }
        }

        public void setTipo(byte _tipo)
        {

            if (_tipo == (byte)(byte)RoomInfo.TIPO.MATCH || _tipo == (byte)(byte)RoomInfo.TIPO.GUILD_BATTLE)
            {
                init_teans();
            }
            else if (_tipo != (byte)(byte)RoomInfo.TIPO.MATCH && m_ri.tipo == (byte)RoomInfo.TIPO.MATCH)
            {
                clear_teans();
            }

            m_ri.tipo = _tipo;

            // Atualizar tipo da sala
            if (m_ri.tipo > (byte)RoomInfo.TIPO.GRAND_ZODIAC_PRACTICE)
            {
                m_ri.tipo_show = 4;
            }
            else if (m_ri.tipo == (byte)RoomInfo.TIPO.GRAND_ZODIAC_ADV || m_ri.tipo == (byte)RoomInfo.TIPO.GRAND_ZODIAC_PRACTICE)
            {
                m_ri.tipo_show = (byte)(byte)RoomInfo.TIPO.GRAND_ZODIAC_INT;
            }
            else
            {
                m_ri.tipo_show = m_ri.tipo;
            }

            if (m_ri.tipo >= (byte)RoomInfo.TIPO.GRAND_ZODIAC_INT)
            {
                m_ri.tipo_ex = m_ri.tipo;
            }
            else
            {
                m_ri.tipo_ex = 255;
            }

            // Atualiza Trofel se for Tourney
            if (m_ri.tipo == (byte)RoomInfo.TIPO.TOURNEY || (m_ri.master != -2 && m_ri.tipo >= (byte)RoomInfo.TIPO.GRAND_ZODIAC_INT && m_ri.tipo <= (byte)RoomInfo.TIPO.GRAND_ZODIAC_ADV))
            {

                if ((m_ri.max_Player > 30 && m_ri.tipo == (byte)RoomInfo.TIPO.TOURNEY) || (m_ri.tipo >= (byte)RoomInfo.TIPO.GRAND_ZODIAC_INT && m_ri.tipo <= (byte)RoomInfo.TIPO.GRAND_ZODIAC_ADV))
                {

                    m_ri.flag_gm = 1;

                    m_ri.state_flag = 0x100;

                    m_ri.trofel = TROFEL_GM_EVENT_TYPEID;

                }
                else if (m_ri.tipo == (byte)RoomInfo.TIPO.TOURNEY || m_ri.tipo >= (byte)RoomInfo.TIPO.GRAND_ZODIAC_INT)
                {
                    updateTrofel();
                }

            }
            else
            {
                m_ri.trofel = 0;
            }
        }

        public void setCourse(byte _course)
        {
            m_ri.course = (RoomInfo.eCOURSE)_course;
        }

        public void setQntdHole(byte _qntd_hole)
        {
            m_ri.qntd_hole = _qntd_hole;
        }

        public void setModo(byte _modo)
        {
            m_ri.modo = _modo;
        }

        public void setTempoVS(uint _tempo)
        {
            m_ri.time_vs = _tempo;
        }

        public void setMaxPlayer(byte _max_Player)
        {

            if (v_sessions.Count > _max_Player)
            {
                throw new exception("[room::setMaxPlayer][Error] MASTER[UID=" + Convert.ToString(m_ri.master) + "] _max_Player[VALUE=" + Convert.ToString(_max_Player) + "] eh menor que o numero de jogadores[VALUE=" + Convert.ToString(v_sessions.Count) + "] na sala.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                    250, 0x588000));
            }

            // New Max Player room
            m_ri.max_Player = _max_Player;

            // Atualiza Trofeu se for Tourney
            if (m_ri.tipo == (byte)RoomInfo.TIPO.TOURNEY || (m_ri.tipo >= (byte)RoomInfo.TIPO.GRAND_ZODIAC_INT && m_ri.tipo <= (byte)RoomInfo.TIPO.GRAND_ZODIAC_ADV))
            {

                if ((m_ri.max_Player > 30 && m_ri.tipo == (byte)RoomInfo.TIPO.TOURNEY) || (m_ri.tipo >= (byte)RoomInfo.TIPO.GRAND_ZODIAC_INT && m_ri.tipo <= (byte)RoomInfo.TIPO.GRAND_ZODIAC_ADV))
                {

                    m_ri.flag_gm = 1;

                    m_ri.trofel = TROFEL_GM_EVENT_TYPEID;

                }
                else if (m_ri.tipo == (byte)RoomInfo.TIPO.TOURNEY || m_ri.tipo >= (byte)RoomInfo.TIPO.GRAND_ZODIAC_INT)
                {
                    updateTrofel();
                }

            }
        }

        public void setTempo30S(uint _tempo)
        {
            m_ri.time_30s = _tempo;
        }

        public void setHoleRepeat(byte _hole_repeat)
        {
            m_ri.hole_repeat = _hole_repeat;
        }

        public void setFixedHole(uint _fixed_hole)
        {
            m_ri.fixed_hole = _fixed_hole;
        }

        public void setArtefato(uint _artefato)
        {
            m_ri.artefato = _artefato;
        }

        public void setNatural(uint _natural)
        {
            m_ri.natural.ulNaturalAndShortGame = _natural;
        }

        public void setState(byte _state)
        {
            m_ri.state = _state;
        }

        public void setFlag(byte _flag)
        {
            m_ri.flag = _flag;
        }

        public void setStateAFK(byte _state_afk)
        {
            m_ri.state_afk = _state_afk;
        }

        // Checks
        public bool checkPass(string _pass)
        {

            if (!isLocked())
            {
                throw new exception("[Room::checkPass][Error] sala nao tem senha", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                    1, 0));
            }

            return string.Compare(m_ri.senha, _pass) == 0;
        }

        // Verifica se o Player tem um loja aberta no lounge e se o item está à vLasta nela
        public bool checkPersonalShopItem(Player _session, int _item_id)
        {
            return m_personal_shop.isItemForSale(_session, _item_id);
        }

        // States
        public bool isLocked()
        {
            return !(m_ri.senha_flag == 1);
        }

        public bool isFull()
        {
            return m_ri.num_Player >= m_ri.max_Player;
        }

        public bool isGaming()
        {
            return m_pGame != null;
        }

        public bool isGamingBefore(uint _uid)
        {

            if (_uid == 0u)
            {
                throw new exception("[room::isGamingBefore][Error] _uid is invalid(zero)", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                    1000, 0));
            }

            if (m_pGame == null)
            {
                throw new exception("[room::isGamingBefore][Error] a sala[NUMERO=" + Convert.ToString(m_ri.numero) + "] nao tem um jogo inicializado. Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                    1001, 0));
            }

            return m_pGame.isGamingBefore(_uid);
        }

        public bool isKickedPlayer(uint _uid)
        {
            return m_Player_kicked.First(el => el.Key == _uid).Key != m_Player_kicked.Last().Key;
        }

        public virtual bool isAllReady()
        {

            var master = findMaster();

            if (master == null)
            {
                return false;
            }

            // Bot Tourney, Short Game and Special Shuffle Course
            if (m_bot_tourney
                && v_sessions.Count == 1
                && m_ri.tipo == (byte)RoomInfo.TIPO.TOURNEY || m_ri.tipo == (byte)RoomInfo.TIPO.SPECIAL_SHUFFLE_COURSE)
            {
                return true;
            }

            // se a sala for Practice, CHIP-IN Practice, e GRAND_PRIX_NOVICE não precisa o Player está pronto
            if (m_ri.tipo == (byte)RoomInfo.TIPO.PRACTICE
                || m_ri.tipo == (byte)RoomInfo.TIPO.GRAND_ZODIAC_PRACTICE
                || m_ri.tipo == (byte)RoomInfo.TIPO.GRAND_PRIX)
            {
                return true;
            }

            // Se o master for GM então não precisar todos está ready(prontos)
            if (master.m_pi.m_cap.game_master && !_haveInvited())
            {
                return true;
            }

            
            var count = v_sessions.Count(_el =>
            {
                var pri = getPlayerInfo(_el);
                return (pri != null && pri.state_flag.ready == 1);
            });

            // Conta com o master por que o master sempre está pronto(ready)
            return (count == v_sessions.Count);
        }

        // Updates
        public void updatePlayerInfo(Player _session)
        {
            PlayerRoomInfoEx pri = new PlayerRoomInfoEx();
            PlayerRoomInfoEx _pri = null;

            if ((_pri = getPlayerInfo(_session)) == null)
            {
                throw new exception("[room::updatePlayerInfo][Error] nao tem o Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] info dessa session na sala.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                    8, 0));
            }

            // Copia do que esta no map
            pri = _pri;

            // Player Room Info Update
            pri.oid = _session.m_oid;

            // C++ TO C# CONVERTER TASK: The memory management function 'memcpy_s' has no equivalent in C#:
            pri.position = (byte)((byte)getPosition(_session) + 1); // posição na sala
            pri.capability = _session.m_pi.m_cap;
            pri.title = _session.m_pi.ue.skin_typeid[5];

            if (_session.m_pi.ei.char_info != null)
            {
                pri.char_typeid = _session.m_pi.ei.char_info._typeid;
            }


            pri.skin[4] = 0; // Aqui tem que ser zero, se for outro valor não mostra a imagem do character equipado

            if (getMaster() == _session.m_pi.uid)
            {
                pri.state_flag.master = 1;
                pri.state_flag.ready = 1; // Sempre está pronto(ready) o master
            }
            else
            {

                // Só troca o estado de pronto dele na sala, se anterior mente ele era Master da sala ou não estiver pronto
                if (pri.state_flag.master == 1 || !(pri.state_flag.ready == 1))
                {
                    pri.state_flag.ready = 0;
                }

                pri.state_flag.master = 0;
            }

            pri.state_flag.sexo = _session.m_pi.mi.sexo;

            // Update Team se for Match
            if (m_ri.tipo == (byte)RoomInfo.TIPO.MATCH)
            {

                // Verifica se o Player está em algum team para atualizar o team dele se ele não estiver em nenhum
                var Player_team = pri.state_flag.team;
                Player p_seg_team = null;

                // atualizar o team do Player a flag de team dele não bate com o team dele
                if (m_teans[Player_team].findPlayerByUID(pri.uid) == null && (p_seg_team = m_teans[~Player_team].findPlayerByUID(pri.uid)) == null)
                {

                    // Player não está em nenhum team
                    if (v_sessions.Count > 1)
                    {

                        if (m_teans[0].getCount() >= 2 && m_teans[1].getCount() >= 2)
                        {
                            throw new exception("[room::updatePlayerInfo][Error] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou entrar em time para todos os times da sala estao cheios. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                                1500, 0));
                        }
                        else if (m_teans[0].getCount() >= 2)
                        {
                            pri.state_flag.team = 1; // Blue
                        }
                        else if (m_teans[1].getCount() >= 2)
                        {
                            pri.state_flag.team = 0; // Red
                        }
                        else
                        {

                            var pPri = getPlayerInfo((v_sessions.Count == 2) ? v_sessions.FirstOrDefault() : (v_sessions.Count > 2 ? (v_sessions.Skip(1).FirstOrDefault()) : null));

                            if (pPri == null)
                            {
                                throw new exception("[room::updatePlayerInfo][Error] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou entrar em um time, mas o ultimo Player da sala, nao tem um info no sala. Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                                    1501, 0));
                            }

                            pri.state_flag.team = (byte)~pPri.state_flag.team;
                        }

                    }
                    else
                    {
                        pri.state_flag.team = 0;
                    }

                    m_teans[pri.state_flag.team].addPlayer(_session);

                }
                else if (p_seg_team != null)
                {

                    // a flag de team do Player está errada, ele está no outro team, ajeita
                    pri.state_flag.team = (byte)~Player_team;
                }

            }
            else if (m_ri.tipo != (byte)RoomInfo.TIPO.GUILD_BATTLE) // O Guild Battle tem sua própria função para inicializar e atualizar o team e os dados da guild
            {
                pri.state_flag.team = Convert.ToByte((pri.position - 1) % 2);
            }

            // Só faz calculo de Quita rate depois que o Player
            // estiver no level Beginner E e jogado 50 games
            if (_session.m_pi.level >= 6 && _session.m_pi.ui.jogado >= 50)
            {
                float rate = _session.m_pi.ui.getQuitRate();

                if (rate < GOOD_PLAYER_ICON)
                {
                    pri.state_flag.azinha = 1;
                }
                else if (rate >= QUITER_ICON_1 && rate < QUITER_ICON_2)
                {
                    pri.state_flag.quiter_1 = 1;
                }
                else if (rate >= QUITER_ICON_2)
                {
                    pri.state_flag.quiter_2 = 1;
                }
            }

            pri.level = (byte)_session.m_pi.mi.level;

            if (_session.m_pi.ei.char_info != null && _session.m_pi.ui.getQuitRate() < GOOD_PLAYER_ICON)
            {
                pri.icon_angel = _session.m_pi.ei.char_info.AngelEquiped();
            }
            else
            {
                pri.icon_angel = 0;
            }

            pri.place.ulPlace = 10; // 0x0A dec"10" _session.m_pi.place
            pri.guild_uid = _session.m_pi.gi.uid;

            pri.uid = _session.m_pi.uid;
            pri.state_lounge = _session.m_pi.state_lounge;
            pri.usUnknown_flg = 0; // Ví Players com valores 2 e 4 e 0
            pri.state = _session.m_pi.state;
            pri.location = new PlayerRoomInfo.stLocation() { x = _session.m_pi.location.x, z = _session.m_pi.location.z, r = _session.m_pi.location.r };

            // Personal Shop
            pri.shop = m_personal_shop.getPersonShop(_session);

            if (_session.m_pi.ei.mascot_info != null)
            {
                pri.mascot_typeid = _session.m_pi.ei.mascot_info._typeid;
            }

            pri.flag_item_boost = _session.m_pi.checkEquipedItemBoost();
            pri.ulUnknown_flg = 0;
            //pri.id_NT não estou usando ainda
            //pri.ucUnknown106

            // Só atualiza a flag de convidado se for diferente de 1, por que 1 ele é convidado
            if (pri.convidado != 1)
            {
                pri.convidado = 0; // Flag Convidado, [Não sei bem por que os que entra na sala normal tem valor igual aqui, já que é flag de convidado waiting], Valor constante da sala para os Players(ACHO)
            }

            pri.avg_score = _session.m_pi.ui.getMediaScore();
            //pri.ucUnknown3

            if (_session.m_pi.ei.char_info != null)
            {
                pri.ci = _session.m_pi.ei.char_info;
            }

            // Salva novamente
            // C++ TO C# CONVERTER TASK: The following line was determined to be a copy assignment (rather than a reference assignment) - this should be verified and a '' method should be created:
            _pri = pri;
        }

        // Finds
        public Player findSessionByOID(uint _oid)
        {
            var i = v_sessions.FirstOrDefault(_el =>
                _el.m_oid == _oid);



            if (i != v_sessions.Last())
            {
                return i;
            }

            return null;
        }

        public Player findSessionByUID(uint _uid)
        {

            var i = v_sessions.FirstOrDefault(_el =>
                _el.m_pi.uid == _uid);



            if (i != v_sessions.Last())
            {
                return i;
            }
            return null;
        }

        public Player findMaster()
        {

            Player master = null;

            var pMaster = v_sessions.FirstOrDefault(_el =>
            {
                return _el.m_pi.uid == m_ri.master;
            });

            if (pMaster != v_sessions.Last())
            {
                master = pMaster;
            }
            return master;
        }

        // Bot Tourney, Short Game and Special Shuffle Course
        public void makeBot(Player _session)
        {
            if (!_session.getState())
            {
                throw new exception("[room::" + "makeBot" + "][Error] Player nao esta connectado", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                    12, 0));
            }

            PangyaBinaryWriter p = new PangyaBinaryWriter();

            try
            {

                // Bot Ticket TypeId

                // Premium User Não precisa de ticket não
                if (_session.m_pi.m_cap.premium_user)
                {

                    // Add Bot Tourney Visual para a sala
                    addBotVisual(_session);

                    // SLast Message
                    p.init_plain((ushort)0x40); // Msg to Chat of Player

                    p.WriteByte(7); // Notice

                    p.WritePStr("@SuperSS");
                    p.WritePStr("[ \\2Premium ] \\c0xff00ff00\\cBot was created.");

                    packet_func_sv.session_send(p,
                        _session, 1);

                }
                else
                {

                    // Verifica se ele tem o ticket para criar o Bot se não manda mensagem dizenho que ele não tem ticket para criar o bot
                    var pWi = _session.m_pi.findWarehouseItemByTypeid(TICKET_BOT_TYPEID);

                    if (pWi != null && pWi.c[0] > 1)
                    {

                        stItem item = new stItem();

                        item.type = 2;
                        item.id = (int)pWi.id;
                        item._typeid = pWi._typeid;
                        item.qntd = 1;
                        item.c[0] = (ushort)(item.qntd * -1);

                        if (item_manager.removeItem(item, _session) > 0)
                        {

                            // Atualiza o item no Jogo e Add o Bot e manda a mensagem que o bot foi add
                            p.init_plain((ushort)0x216);

                            p.WriteUInt32((uint)UtilTime.GetSystemTimeAsUnix());
                            p.WriteUInt32(1); // Count;

                            p.WriteByte(item.type);
                            p.WriteUInt32(item._typeid);
                            p.WriteInt32(item.id);
                            p.WriteUInt32(item.flag_time);
                            p.WriteStruct(item.stat, Marshal.SizeOf(new stItem.item_stat()));
                            p.WriteUInt32((item.c[3] > 0) ? item.c[3] : item.c[0]);
                            p.WriteZero(25);

                            packet_func_sv.session_send(p,
                                _session, 1);

                            // Add Bot
                            addBotVisual(_session);

                            // SLast Message
                            p.init_plain((ushort)0x40); // Msg to Chat of Player

                            p.WriteByte(7); // Notice

                            p.WritePStr("@SuperSS");
                            p.WritePStr("\\c0xff00ff00\\cBot was created 1 ticket has been consumed.");

                            packet_func_sv.session_send(p,
                                _session, 1);

                        }
                        else
                        {

                            _smp.message_pool.push(new message("[room::makeBot][Error] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] nao conseguiu deletar o TICKET_BOT[TYPEID=" + Convert.ToString(TICKET_BOT_TYPEID) + ", ID=" + Convert.ToString(item.id) + "]", type_msg.CL_FILE_LOG_AND_CONSOLE));

                            // SLast Message
                            p.init_plain((ushort)0x40); // Msg to Chat of Player

                            p.WriteByte(7); // Notice

                            p.WritePStr("@SuperSS");
                            p.WritePStr("\\c0xffff0000\\cError creating Bot.");

                            packet_func_sv.session_send(p,
                                _session, 1);
                        }

                    }
                    else
                    {

                        // Não tem ticket bot suficiente, manda mensagem
                        // SLast Message
                        p.init_plain((ushort)0x40); // Msg to Chat of Player

                        p.WriteByte(7); // Notice

                        p.WritePStr("@SuperSS");
                        p.WritePStr("\\c0xffff0000\\cYou do not have enough ticket to create the Bot.");

                        packet_func_sv.session_send(p,
                            _session, 1);
                    }
                }

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[room::makeBot][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                // SLast Message
                p.init_plain((ushort)0x40); // Msg to Chat of Player

                p.WriteByte(7); // Notice

                p.WritePStr("@SuperSS");
                p.WritePStr("\\c0xffff0000\\cError creating Bot.");

                packet_func_sv.session_send(p,
                    _session, 1);
            }
        }

        // Info Room
        public bool requestChangeInfoRoom(Player _session, Packet _packet)
        {
            if (!_session.getState())
            {
                throw new exception("[room::" + (("request" + "ChangeInfoRoom")) + "][Error] Player nao esta connectado", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                    12, 0));
            }
            if (_packet == null)
            {
                throw new exception("[room::request" + "ChangeInfoRoom" + "][Error] _packet is nullptr", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                    12, 0));
            }

            bool ret = false;

            try
            {

                byte num_info;
                short flag;

                if (m_ri.master != _session.m_pi.uid)
                {
                    throw new exception("[room::requestChangeInfoRoom][Error] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou trocar o info da sala[NUMERO=" + Convert.ToString(m_ri.numero) + ", MASTER=" + Convert.ToString(m_ri.master) + "], mas nao pode trocar o info da sala sem ser master.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                        11, 0));
                }

                flag = _packet.ReadInt16();

                num_info = _packet.ReadByte();

                if (num_info <= 0)
                {
                    throw new exception("[room::requestChangeInfoRoom][Error] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou trocar o info da sala[NUMERO=" + Convert.ToString(m_ri.numero) + ", MASTER=" + Convert.ToString(m_ri.master) + "], mas nao tem nenhum info para trocar do buffer do cliente.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                        8, 0));
                }

                for (var i = 0; i < num_info; ++i)
                {

                    switch ((RoomInfo.INFO_CHANGE)_packet.ReadByte())
                    {
                        case RoomInfo.INFO_CHANGE.NAME:
                            setNome(_packet.ReadPStr());
                            break;
                        case RoomInfo.INFO_CHANGE.SENHA:
                            setSenha(_packet.ReadPStr());
                            break;
                        case RoomInfo.INFO_CHANGE.TIPO:
                            setTipo(_packet.ReadByte());
                            break;
                        case RoomInfo.INFO_CHANGE.COURSE:
                            setCourse(_packet.ReadByte());
                            break;
                        case RoomInfo.INFO_CHANGE.QNTD_HOLE:
                            setQntdHole(_packet.ReadByte());
                            break;
                        case RoomInfo.INFO_CHANGE.MODO:
                            setModo(_packet.ReadByte());
                            break;
                        case RoomInfo.INFO_CHANGE.TEMPO_VS: // Passa em Segundos
                            setTempoVS((uint)_packet.ReadUInt16() * 1000);
                            break;
                        case RoomInfo.INFO_CHANGE.MAX_PLAYER:
                            setMaxPlayer(_packet.ReadByte());
                            break;
                        case RoomInfo.INFO_CHANGE.TEMPO_30S: // Passa em Minutos
                            setTempo30S((uint)_packet.ReadByte() * 60000);
                            break;
                        case RoomInfo.INFO_CHANGE.STATE_FLAG:
                            // Esse não usa mais
                            // Aqui posso usar para começar o jogo, se a sala estiver(AFK) => "isso acontece quando o master está AFK"
                            // Então vou salver esse valor aqui
                            setStateAFK(_packet.ReadByte());
                            break;
                        case RoomInfo.INFO_CHANGE.HOLE_REPEAT:
                            setHoleRepeat(_packet.ReadByte());
                            break;
                        case RoomInfo.INFO_CHANGE.FIXED_HOLE:
                            setFixedHole(_packet.ReadUInt32());
                            break;
                        case RoomInfo.INFO_CHANGE.ARTEFATO:
                            setArtefato(_packet.ReadUInt32());
                            break;
                        case RoomInfo.INFO_CHANGE.NATURAL:
                            {
                                var natural = new NaturalAndShortGame((uint)_packet.ReadUInt32());

                                if (sgs.gs.getInstance().getInfo().propriedade.natural) // Natural não deixa desabilitar o Natural da sala, por que o server é natural
                                {
                                    natural.natural = 1;
                                }

                                setNatural(natural.ulNaturalAndShortGame);

                                break;
                            }
                        default:
                            throw new exception("[room::requestChangeInfoRoom][Error] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou trocar info da sala[NUMERO=" + Convert.ToString(m_ri.numero) + ", MASTER=" + Convert.ToString(m_ri.master) + "], mas info change eh desconhecido.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                                9, 0));
                    }
                }

                // send to clients update room info
                SendUpdate();

                ret = true; // Trocou o info da sala com sucesso

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[room::requestChangeInfoRoom][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                // Resposta para o cliente
                PangyaBinaryWriter p = new PangyaBinaryWriter();

                packet_func_sv.pacote04A(p,
                    m_ri, 25);
                packet_func_sv.session_send(p,
                    _session, 1);
            }

            return ret;
        }

        // Chat Team
        public void requestChatTeam(Player _session, Packet _packet)
        {
            if (!_session.getState())
            {
                throw new exception("[room::" + (("request" + "ChatTeam")) + "][Error] Player nao esta connectado", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                    12, 0));
            }
            if (_packet == null)
            {
                throw new exception("[room::request" + "ChatTeam" + "][Error] _packet is nullptr", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                    12, 0));
            }

            PangyaBinaryWriter p = new PangyaBinaryWriter();

            try
            {

                var msg = _packet.ReadPStr();

                // Verifica a mensagem com palavras proibida e manda para o log e bloquea o chat dele
                _smp.message_pool.push(new message("[room::requestChatTeam][Log] Player[UID=" + Convert.ToString(_session.m_pi.uid) + ", MESSAGE=" + msg + "]", type_msg.CL_ONLY_FILE_LOG));

                if (msg.empty())
                {
                    throw new exception("[room::requestChatTeam][Error] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou mandar messsage[MSG=" + msg + "] no chat do team na sala[NUMERO=" + Convert.ToString(m_ri.numero) + "], mas a msg esta vazia. Hacker ou Bug.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                        2000, 0));
                }

                if (m_ri.tipo != (byte)RoomInfo.TIPO.MATCH && m_ri.tipo != (byte)RoomInfo.TIPO.GUILD_BATTLE)
                {
                    throw new exception("[room::requestChatTeam][Error] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou mandar messsage[MSG=" + msg + "] no chat do team na sala[NUMERO=" + Convert.ToString(m_ri.numero) + "], mas a sala nao eh MATCH ou GUILD_BATTLE. Hacker ou Bug.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                        2001, 0));
                }

                if (m_teans.empty())
                {
                    throw new exception("[room::requestChatTeam][Error] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou mandar messsage[MSG=" + msg + "] no chat do team na sala[NUMERO=" + Convert.ToString(m_ri.numero) + "], mas a sala nao tem nenhum team. Hacker ou Bug.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                        2002, 0));
                }

                var pri = getPlayerInfo(_session);

                if (pri == null)
                {
                    throw new exception("[room::requetChatTeam][Error] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou mandar messsage[MSG=" + msg + "] no chat do team na sala[NUMERO=" + Convert.ToString(m_ri.numero) + "], mas a sala nao tem o info dele. Hacker ou Bug.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                        2003, 0));
                }

                var team = m_teans[pri.state_flag.team];

                if (team.findPlayerByUID(_session.m_pi.uid) == null)
                {
                    throw new exception("[room::requestChatTeam][Error] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou mandar messsage[MSG=" + msg + "] no chat do team na sala[NUMERO=" + Convert.ToString(m_ri.numero) + "], mas ele nao esta no team que a flag de team dele diz. Hacker ou Bug.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                        2004, 0));
                }

                // LOG GM
                // Envia para todo os GM do server essa message
                var c = sgs.gs.getInstance().findChannel(_session.m_pi.channel);

                if (c != null)
                {

                    var gm = sgs.gs.getInstance().FindAllGM();

                    if (!gm.empty())
                    {

                        string msg_gm = "\\5" + (_session.m_pi.nickname) + ": '" + msg + "'";
                        string from = "\\1[Channel=" + (c.getInfo().name) + ", \\1ROOM=" + Convert.ToString(_session.m_pi.mi.sala_numero) + "][Team" + (!(pri.state_flag.team == 1) ? "R" : "B") + "]";

                        var index = from.IndexOf(' ');

                        if (index != -1)
                        {
                            from = from.Remove(index, 1).Insert(index, " \\1");
                        }

                        foreach (Player el in gm)
                        {
                            if (((el.m_gi.channel && el.m_pi.channel == c.getInfo().id) || el.m_gi.whisper || el.m_gi.isOpenPlayerWhisper(_session.m_pi.uid)) && (el.m_pi.channel != _session.m_pi.channel || el.m_pi.mi.sala_numero != _session.m_pi.mi.sala_numero || team.findPlayerByUID(el.m_pi.uid) == null))
                            {

                                // Responde no chat do Player
                                p.init_plain((ushort)0x40);

                                p.WriteByte(0);

                                p.WritePStr(from); // Nickname

                                p.WritePStr(msg_gm); // Message

                                packet_func_sv.session_send(p,
                                    el, 1);
                            }
                        }
                    }
                }
                else
                {

                }
                {
                    _smp.message_pool.push(new message("[room::requestChatTeam][WARNING] Log GM nao encontrou o Channel[ID=" + Convert.ToString((ushort)_session.m_pi.channel) + "] no server. Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));
                }

                // Manda message para o team da sala
                p.init_plain((ushort)0xB0);

                p.WritePStr(_session.m_pi.nickname);
                p.WritePStr(msg);

                foreach (var el in team.getPlayers())
                {
                    el.Send(p);
                }

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[room::requestChatTeam][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
        }

        // Change Item Equiped of Player
        public virtual void requestChangePlayerItemRoom(Player _session, ChangePlayerItemRoom _cpir)
        {
            if (!_session.getState())
            {
                throw new exception("[room::" + "ChangePlayerItemRoom" + "][Error] Player nao esta connectado", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                    12, 0));
            }

            PangyaBinaryWriter p = new PangyaBinaryWriter();

            try
            {

                int error = 0/*SUCCESS*/;
                switch (_cpir.type)
                {
                    case ChangePlayerItemRoom.TYPE_CHANGE.TC_CADDIE:
                        break;
                    case ChangePlayerItemRoom.TYPE_CHANGE.TC_BALL:
                        break;
                    case ChangePlayerItemRoom.TYPE_CHANGE.TC_CLUBSET:
                        break;
                    case ChangePlayerItemRoom.TYPE_CHANGE.TC_CHARACTER:
                        break;
                    case ChangePlayerItemRoom.TYPE_CHANGE.TC_MASCOT:
                        break;
                    case ChangePlayerItemRoom.TYPE_CHANGE.TC_ITEM_EFFECT_LOUNGE:  // Itens Active, Jester x2 velocidade no lounge, e Harmes tamanho da cabeça
                        {
                            // ignora o item_id por que ele envia 0

                            // Valor 1 Cabeca
                            // Valor 2 Velocidade
                            // Valor 3 Twilight

                            if (!sIff.getInstance().isLoad())
                            {
                                sIff.getInstance().load();
                            }

                            if (_session.m_pi.ei.char_info == null)
                            {
                                throw new exception("[room::requestChangePlayerItemRoom][Error] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] nao tem nenhum character equipado. Hacker ou Bug.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.CHANNEL,
                                    1000, 0x57007));
                            }

                            if (_cpir.effect_lounge.effect != ChangePlayerItemRoom.stItemEffectLounge.TYPE_EFFECT.TE_TWILIGHT)
                            {

                                var it = (_session.m_pi.ei.char_info == null) ? _session.m_pi.mp_scl.Last() : _session.m_pi.mp_scl.FirstOrDefault(c => c.Key == _session.m_pi.ei.char_info.id);

                                if (it.Key == _session.m_pi.mp_scl.Last().Key)
                                {

                                    _smp.message_pool.push(new message("[room::requestChangePlayerItemRoom][Error] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] nao tem os estados do character na lounge. Criando um novo para ele. Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));

                                    // Add New State Character Lounge
                                    _session.m_pi.mp_scl.Add(_session.m_pi.ei.char_info.id, new StateCharacterLounge());
                                    var pair = _session.m_pi.mp_scl.FirstOrDefault(c => c.Key == _session.m_pi.ei.char_info.id);
                                    it = pair;
                                }

                                switch (_cpir.effect_lounge.effect)
                                {
                                    case ChangePlayerItemRoom.stItemEffectLounge.TYPE_EFFECT.TE_BIG_HEAD: // Jester (Big head)
                                        {

                                             var ccj = cadie_cauldron_Jester_item_typeid.FirstOrDefault(el =>
                                            {
                                                return sIff.getInstance().getItemCharIdentify(el) == (_session.m_pi.ei.char_info._typeid & 0x000000FF);
                                            });

                                            if (ccj <= 0)
                                            {
                                                throw new exception("[room::requestChangePlayerItemRoom][Error] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] o Character[TYPEID=" + Convert.ToString(_session.m_pi.ei.char_info._typeid) + "] equipado nao tem o item Jester no server. Hacker ou Bug.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.CHANNEL,
                                                    1001, 0x57008));
                                            }

                                            if (!_session.m_pi.ei.char_info.isPartEquiped(ccj))
                                            {
                                                throw new exception("[room::requestChangePlayerItemRoom][Error] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] o Character[TYPEID=" + Convert.ToString(_session.m_pi.ei.char_info._typeid) + "] nao esta com o item[TYPEID=" + Convert.ToString(ccj) + "] Jester equipado. Hacker ou Bug.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.CHANNEL, 1002, 0x57009));
                                            }

                                            it.Value.scale_head = (it.Value.scale_head > 1.0f) ? 1.0f : 2.0f;

                                            break;
                                        }
                                    case ChangePlayerItemRoom.stItemEffectLounge.TYPE_EFFECT.TE_FAST_WALK: // Hermes (Velocidade x2)
                                        {

                                            
                                            var cch = cadie_cauldron_Hermes_item_typeid.FirstOrDefault(el =>
                                            {
                                                return sIff.getInstance().getItemCharIdentify(el) == (_session.m_pi.ei.char_info._typeid & 0x000000FF);
                                            });

                                            if (cch <= 0)
                                            {
                                                throw new exception("[room::requestChangePlayerItemRoom][Error] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] o Character[TYPEID=" + Convert.ToString(_session.m_pi.ei.char_info._typeid) + "] equipado nao tem o item Hermes no server. Hacker ou Bug.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.CHANNEL,
                                                    1001, 0x57008));
                                            }

                                            if (!_session.m_pi.ei.char_info.isPartEquiped(cch))
                                            {
                                                throw new exception("[room::requestChangePlayerItemRoom][Error] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] o Character[TYPEID=" + Convert.ToString(_session.m_pi.ei.char_info._typeid) + "] nao esta com o item[TYPEID=" + Convert.ToString(cch) + "] Hermes equipado. Hacker ou Bug.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.CHANNEL, 1002, 0x57009));
                                            }

                                            it.Value.walk_speed = (it.Value.walk_speed > 1.0f) ? 1.0f : 2.0f;

                                            break;
                                        }
                                } // End Switch

                            }
                            else
                            {
                                // else == 3 // Twilight (Fogos de artifícios em cima da cabeça do Player)
                                // Valor 1 pass para fazer o fogos                                              
                                var cct = cadie_cauldron_Twilight_item_typeid.FirstOrDefault (el =>
                                {
                                    return sIff.getInstance().getItemCharIdentify(el) == (_session.m_pi.ei.char_info._typeid & 0x000000FF);
                                });

                                if (cct <= 0)
                                {
                                    throw new exception("[room::requestChangePlayerItemRoom][Error] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] o Character[TYPEID=" + Convert.ToString(_session.m_pi.ei.char_info._typeid) + "] equipado nao tem o item Twilight no server. Hacker ou Bug.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.CHANNEL,
                                        1001, 0x57008));
                                }

                                if (!_session.m_pi.ei.char_info.isPartEquiped(cct))
                                {
                                    throw new exception("[room::requestChangePlayerItemRoom][Error] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] o Character[TYPEID=" + Convert.ToString(_session.m_pi.ei.char_info._typeid) + "] nao esta com o item[TYPEID=" + Convert.ToString(cct) + "] Twilight equipado. Hacker ou Bug.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.CHANNEL,
                                        1002, 0x57009));
                                }
                            }
                                        
                            packet_func_sv.room_broadcast(this,
                                packet_func_sv.pacote04B(
                                _session, _cpir.type, error,
                                _cpir.effect_lounge.effect), 1);            
                        }
                        break;
                    case ChangePlayerItemRoom.TYPE_CHANGE.TC_ALL:
                        {

                            // Aqui se não tiver os itens, algum hacker, gera Log, e coloca item padrão ou nenhum
                            CharacterInfo pCe = null;
                            CaddieInfoEx pCi = null;
                            WarehouseItemEx pWi = null;
                            uint _error = 0;

                            // Character
                            if (_cpir.character != 0
                                && (pCe = _session.m_pi.findCharacterById(_cpir.character)) != null
                                && sIff.getInstance().getItemGroupIdentify(pCe._typeid) == sIff.getInstance().CHARACTER)
                            {

                                _session.m_pi.ei.char_info = pCe;
                                _session.m_pi.ue.character_id = _cpir.character;

                                // Update ON DB
                                NormalManagerDB.add(0,
                                    new CmdUpdateCharacterEquiped(_session.m_pi.uid, (int)_cpir.character),
                                    SQLDBResponse, this);

                            }
                            else
                            {

                                _error = (uint)((_cpir.character == 0) ? 1 : (pCe == null ? 2 : 3));

                                if (_session.m_pi.mp_ce.Count > 0)
                                {

                                    _smp.message_pool.push(new message("[room::requestChangePlayerItemRoom][Log][WARNING] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou trocar o Character[ID=" + Convert.ToString(_cpir.character) + "] para comecar o jogo, mas deu Error[VALUE=" + Convert.ToString(error) + "], colocando o primeiro character do Player. Hacker ou Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));

                                    _session.m_pi.ei.char_info = _session.m_pi.mp_ce.begin().Value;
                                    _cpir.character = _session.m_pi.ue.character_id = _session.m_pi.ei.char_info.id;

                                    // Update ON DB
                                    NormalManagerDB.add(0,
                                        new CmdUpdateCharacterEquiped(_session.m_pi.uid, (int)_cpir.character),
                                        SQLDBResponse, this);

                                }
                                else
                                {

                                    _smp.message_pool.push(new message("[room::requestChangePlayerItemRoom][Log][WARNING] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou trocar o Character[ID=" + Convert.ToString(_cpir.character) + "] para comecar o jogo, mas deu Error[VALUE=" + Convert.ToString(error) + "], ele nao tem nenhum character, adiciona o Nuri para ele. Hacker ou Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));

                                    BuyItem bi = new BuyItem();
                                    stItem item = new stItem();

                                    bi.id = -1;
                                    bi._typeid = (uint)(sIff.getInstance().CHARACTER << 26); // Nuri
                                    bi.qntd = 1;

                                    item_manager.initItemFromBuyItem(_session.m_pi,
                                       ref item, bi, false, 0, 0, 1);

                                    if (item._typeid != 0)
                                    {

                                        // Add Item já atualiza o Character equipado
                                        if ((_cpir.character = item_manager.addItem(item,
                                            _session, 2, 0)) != item_manager.RetAddItem.TYPE.T_ERROR)
                                        {

                                            // Update ON GAME
                                            p.init_plain((ushort)0x216);

                                            p.WriteUInt32((uint)UtilTime.GetSystemTimeAsUnix());
                                            p.WriteUInt32(1); // Count

                                            p.WriteByte(item.type);
                                            p.WriteUInt32(item._typeid);
                                            p.WriteInt32(item.id);
                                            p.WriteUInt32(item.flag_time);
                                            p.WriteStruct(item.stat, Marshal.SizeOf(new stItem.item_stat()));
                                            p.WriteUInt32((item.c[3] > 0) ? item.c[3] : item.c[0]);
                                            p.WriteZero(25);

                                            packet_func_sv.session_send(p,
                                                _session, 1);

                                        }
                                        else
                                        {
                                            _smp.message_pool.push(new message("[room::requestChangePlayerItemRoom][Log][WARNING] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] nao conseguiu adicionar o Character[TYPEID=" + Convert.ToString(item._typeid) + "] para ele. Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));
                                        }

                                    }
                                    else
                                    {
                                        _smp.message_pool.push(new message("[room::requestChangePlayerItemRoom][Log][WARNING] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] nao conseguiu inicializar o Character[TYPEID=" + Convert.ToString(bi._typeid) + "] para ele. Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));
                                    }
                                }
                            }

                            // Caddie
                            if (_cpir.caddie != 0
                                && (pCi = _session.m_pi.findCaddieById(_cpir.caddie)) != null
                                && sIff.getInstance().getItemGroupIdentify(pCi._typeid) == sIff.getInstance().CADDIE)
                            {

                                // Check if item is in map of update item
                                var v_it = _session.m_pi.findUpdateItemByTypeidAndId(pCi._typeid, pCi.id);

                                if (v_it.Any())
                                {

                                    foreach (var el in v_it)
                                    {

                                        if (el.Value.type == UpdateItem.UI_TYPE.CADDIE)
                                        {

                                            // Desequipa o caddie
                                            _session.m_pi.ei.cad_info = null;
                                            _session.m_pi.ue.caddie_id = 0;

                                            _cpir.caddie = 0;

                                        }
                                        else if (el.Value.type == UpdateItem.UI_TYPE.CADDIE_PARTS)
                                        {

                                            // Limpa o caddie Parts
                                            pCi.parts_typeid = 0;
                                            pCi.parts_end_date_unix = 0;
                                            pCi.end_parts_date = new PangyaTime();

                                            _session.m_pi.ei.cad_info = pCi;
                                            _session.m_pi.ue.caddie_id = _cpir.caddie;
                                        }

                                        // Tira esse Update Item do map
                                        _session.m_pi.mp_ui.Remove(el.Key);
                                    }

                                }
                                else
                                {

                                    // Caddie is Good, Update caddie equiped ON SERVER AND DB
                                    _session.m_pi.ei.cad_info = pCi;
                                    _session.m_pi.ue.caddie_id = _cpir.caddie;

                                    // Verifica se o Caddie pode equipar
                                    if (_session.checkCaddieEquiped(_session.m_pi.ue))
                                    {
                                        _cpir.caddie = _session.m_pi.ue.caddie_id;
                                    }

                                }

                                // Update ON DB
                                NormalManagerDB.add(0,
                                    new CmdUpdateCaddieEquiped(_session.m_pi.uid, (int)_cpir.caddie),
                                    SQLDBResponse, this);

                            }
                            else if (_session.m_pi.ue.caddie_id > 0 && _session.m_pi.ei.cad_info != null)
                            { // Desequipa Caddie

                                _error = (uint)((_cpir.caddie == 0) ? 1 : (pCi == null ? 2 : 3));

                                if (_error > 1)
                                {
                                    _smp.message_pool.push(new message("[room::requestChangePlayerItemRoom][Log][WARNING] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou trocar o Caddie[ID=" + Convert.ToString(_cpir.caddie) + "] para comecar o jogo, mas deu Error[VALUE=" + Convert.ToString(error) + "], desequipando o caddie. Hacker ou Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));
                                }

                                // Check if item is in map of update item
                                var v_it = _session.m_pi.findUpdateItemByTypeidAndId(_session.m_pi.ei.cad_info._typeid, _session.m_pi.ei.cad_info.id);

                                if (v_it.Any())
                                {
                                    foreach (var el in v_it)
                                    {
                                        // Caddie já vai se desequipar, só verifica o parts
                                        if (el.Value.type == UpdateItem.UI_TYPE.CADDIE_PARTS)
                                        {

                                            // Limpa o caddie Parts
                                            _session.m_pi.ei.cad_info.clear();
                                        }

                                        // Tira esse Update Item do map
                                        _session.m_pi.mp_ui.Remove(el.Key);
                                    }

                                }
                                _session.m_pi.ei.cad_info = null;
                                _session.m_pi.ue.caddie_id = 0;

                                _cpir.caddie = 0;

                                // Att No DB
                                NormalManagerDB.add(0,
                                    new CmdUpdateCaddieEquiped(_session.m_pi.uid, (int)_cpir.caddie),
                                    SQLDBResponse, this);
                            }

                            // ClubSet
                            if (_cpir.clubset != 0
                                && (pWi = _session.m_pi.findWarehouseItemById(_cpir.clubset)) != null
                                && sIff.getInstance().getItemGroupIdentify(pWi._typeid) == sIff.getInstance().CLUBSET)
                            {

                                var c_it = _session.m_pi.findUpdateItemByTypeidAndType(_cpir.clubset, UpdateItem.UI_TYPE.WAREHOUSE);

                                if (c_it.Equals(_session.m_pi.mp_ui.Last()))
                                {

                                    _session.m_pi.ei.clubset = pWi;

                                    // Esse C do WarehouseItem, que pega do DB, não é o ja updado inicial da taqueira é o que fica tabela enchant,
                                    // que no original fica no warehouse msm, eu só confundi quando fiz
                                    _session.m_pi.ei.csi.setValues(pWi.id, pWi._typeid, pWi.c);

                                    var cs = sIff.getInstance().findClubSet(pWi._typeid);

                                    if (cs != null)
                                    {

                                        // C++ TO C# CONVERTER WARNING: This 'sizeof' ratio was replaced with a direct reference to the array length:
                                        for (var j = 0; j < 5; ++j)
                                        {
                                            _session.m_pi.ei.csi.enchant_c[j] = (short)(cs.Stats.getSlot[j] + pWi.clubset_workshop.c[j]);
                                        }

                                        _session.m_pi.ue.clubset_id = _cpir.clubset;

                                        // Verifica se o ClubSet pode equipar
                                        if (_session.checkClubSetEquiped(_session.m_pi.ue))
                                        {
                                            _cpir.clubset = _session.m_pi.ue.clubset_id;
                                            // }

                                            // Update ON DB
                                            //NormalManagerDB.add(0,
                                            //    new CmdUpdateClubsetEquiped(_session.m_pi.uid, _cpir.clubset),
                                            //    SQLDBResponse, this);

                                        }
                                        else
                                        {

                                            _error = 5;

                                            _smp.message_pool.push(new message("[room::requestChangePlayerItemRoom][Error] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou Atualizar Clubset[TYPEID=" + Convert.ToString(pWi._typeid) + ", ID=" + Convert.ToString(pWi.id) + "] equipado, mas ClubSet Not exists on IFF structure. Equipa o ClubSet padrao. Hacker ou Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));

                                            // Coloca o ClubSet CV1 no lugar do ClubSet que acabou o tempo
                                            pWi = _session.m_pi.findWarehouseItemByTypeid(AIR_KNIGHT_SET);

                                            if (pWi != null)
                                            {

                                                _smp.message_pool.push(new message("[room::requestChangePlayerItemRoom][Log][WARNING] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou trocar o ClubSet[ID=" + Convert.ToString(_cpir.clubset) + "] para comecar o jogo, mas acabou o tempo do ClubSet[ID=" + Convert.ToString(_cpir.clubset) + "], colocando o ClubSet Padrao\"CV1\" do Player. Hacker ou Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));

                                                // Esse C do WarehouseItem, que pega do DB, não é o ja updado inicial da taqueira é o que fica tabela enchant,
                                                // que no original fica no warehouse msm, eu só confundi quando fiz
                                                _session.m_pi.ei.csi.setValues(pWi.id, pWi._typeid, pWi.c);

                                                cs = sIff.getInstance().findClubSet(pWi._typeid);

                                                if (cs != null)
                                                {
                                                    // C++ TO C# CONVERTER WARNING: This 'sizeof' ratio was replaced with a direct reference to the array length:
                                                    for (var j = 0; j < 5; ++j)
                                                    {
                                                        _session.m_pi.ei.csi.enchant_c[j] = (short)(cs.Stats.getSlot[j] + pWi.clubset_workshop.c[j]);
                                                    }
                                                }

                                                _session.m_pi.ei.clubset = pWi;
                                                _cpir.clubset = _session.m_pi.ue.clubset_id = pWi.id;

                                                // Update ON DB
                                                NormalManagerDB.add(0,
                                                    new CmdUpdateClubsetEquiped(_session.m_pi.uid, (int)_cpir.clubset),
                                                    SQLDBResponse, this);

                                            }
                                            else
                                            {

                                                _smp.message_pool.push(new message("[channel::requestChangePlayerItemRoom][Log][WARNING] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou trocar o ClubSet[ID=" + Convert.ToString(_cpir.clubset) + "] para comecar o jogo, mas acabou o tempo do ClubSet[ID=" + Convert.ToString(_cpir.clubset) + "], ele nao tem o ClubSet Padrao\"CV1\", adiciona o ClubSet pardrao\"CV1\" para ele. Hacker ou Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));

                                                BuyItem bi = new BuyItem();
                                                stItem item = new stItem();

                                                bi.id = -1;
                                                bi._typeid = AIR_KNIGHT_SET;
                                                bi.qntd = 1;

                                                item_manager.initItemFromBuyItem(_session.m_pi,
                                                   ref item, bi, false, 0, 0, 1);

                                                if (item._typeid != 0)
                                                {

                                                    if ((_cpir.clubset = item_manager.addItem(item,
                                                        _session, 2, 0)) != item_manager.RetAddItem.TYPE.T_ERROR)
                                                    {

                                                        // Equipa o ClubSet CV1
                                                        pWi = _session.m_pi.findWarehouseItemById(_cpir.clubset);

                                                        if (pWi != null)
                                                        {

                                                            // Esse C do WarehouseItem, que pega do DB, não é o ja updado inicial da taqueira é o que fica tabela enchant,
                                                            // que no original fica no warehouse msm, eu só confundi quando fiz
                                                            _session.m_pi.ei.csi.setValues(pWi.id, pWi._typeid, pWi.c);

                                                            cs = sIff.getInstance().findClubSet(pWi._typeid);

                                                            if (cs != null)
                                                            {
                                                                for (var j = 0; j < 5; ++j)
                                                                {
                                                                    _session.m_pi.ei.csi.enchant_c[j] = (short)(cs.Stats.getSlot[j] + pWi.clubset_workshop.c[j]);
                                                                }
                                                            }

                                                            _session.m_pi.ei.clubset = pWi;
                                                            _session.m_pi.ue.clubset_id = pWi.id;

                                                            // Update ON DB
                                                            NormalManagerDB.add(0,
                                                                new CmdUpdateClubsetEquiped(_session.m_pi.uid, (int)_cpir.clubset),
                                                                SQLDBResponse, this);

                                                            // Update ON GAME
                                                            p.init_plain((ushort)0x216);

                                                            p.WriteUInt32((uint)UtilTime.GetSystemTimeAsUnix());
                                                            p.WriteUInt32(1); // Count

                                                            p.WriteByte(item.type);
                                                            p.WriteUInt32(item._typeid);
                                                            p.WriteInt32(item.id);
                                                            p.WriteUInt32(item.flag_time);
                                                            p.WriteStruct(item.stat, Marshal.SizeOf(new stItem.item_stat()));
                                                            p.WriteUInt32((item.c[3] > 0) ? item.c[3] : item.c[0]);
                                                            p.WriteZero(25);

                                                            packet_func_sv.session_send(p,
                                                                _session, 1);

                                                        }
                                                        else
                                                        {
                                                            _smp.message_pool.push(new message("[room::requestChangePlayerItemRoom][Log][WARNING] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] nao conseguiu achar o ClubSet\"CV1\"[ID=" + Convert.ToString(item.id) + "] que acabou de adicionar para ele. Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));
                                                        }

                                                    }
                                                    else
                                                    {
                                                        _smp.message_pool.push(new message("[room::requestChangePlayerItemRoom][Log][WARNING] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] nao conseguiu adicionar o ClubSet[TYPEID=" + Convert.ToString(item._typeid) + "] para ele. Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));
                                                    }

                                                }
                                                else
                                                {
                                                    _smp.message_pool.push(new message("[room::requestChangePlayerItemRoom][Log][WARNING] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] nao conseguiu inicializar o ClubSet[TYPEID=" + Convert.ToString(bi._typeid) + "] para ele. Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));
                                                }
                                            }
                                        }

                                    }
                                    else
                                    { // ClubSet Acabou o tempo

                                        // Coloca o ClubSet CV1 no lugar do ClubSet que acabou o tempo
                                        pWi = _session.m_pi.findWarehouseItemByTypeid(AIR_KNIGHT_SET);

                                        if (pWi != null)
                                        {

                                            _smp.message_pool.push(new message("[room::requestChangePlayerItemRoom][Log][WARNING] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou trocar o ClubSet[ID=" + Convert.ToString(_cpir.clubset) + "] para comecar o jogo, mas acabou o tempo do ClubSet[ID=" + Convert.ToString(_cpir.clubset) + "], colocando o ClubSet Padrao\"CV1\" do Player. Hacker ou Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));

                                            // Esse C do WarehouseItem, que pega do DB, não é o ja updado inicial da taqueira é o que fica tabela enchant,
                                            // que no original fica no warehouse msm, eu só confundi quando fiz
                                            _session.m_pi.ei.csi.setValues(pWi.id, pWi._typeid, pWi.c);

                                            cs = sIff.getInstance().findClubSet(pWi._typeid);

                                            if (cs != null)
                                            {
                                                for (var j = 0; j < 5; ++j)
                                                {
                                                    _session.m_pi.ei.csi.enchant_c[j] = (short)(cs.Stats.getSlot[j] + pWi.clubset_workshop.c[j]);
                                                }
                                            }

                                            _session.m_pi.ei.clubset = pWi;
                                            _cpir.clubset = _session.m_pi.ue.clubset_id = pWi.id;

                                            // Update ON DB
                                            NormalManagerDB.add(0,
                                                new CmdUpdateClubsetEquiped(_session.m_pi.uid, (int)_cpir.clubset),
                                                SQLDBResponse, this);

                                        }
                                        else
                                        {

                                            _smp.message_pool.push(new message("[room::requestChangePlayerItemRoom][Log][WARNING] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou trocar o ClubSet[ID=" + Convert.ToString(_cpir.clubset) + "] para comecar o jogo, mas acabou o tempo do ClubSet[ID=" + Convert.ToString(_cpir.clubset) + "], ele nao tem o ClubSet Padrao\"CV1\", adiciona o ClubSet pardrao\"CV1\" para ele. Hacker ou Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));

                                            BuyItem bi = new BuyItem();
                                            stItem item = new stItem();

                                            bi.id = -1;
                                            bi._typeid = AIR_KNIGHT_SET;
                                            bi.qntd = 1;

                                            item_manager.initItemFromBuyItem(_session.m_pi,
                                               ref item, bi, false, 0, 0, 1);

                                            if (item._typeid != 0)
                                            {

                                                if ((_cpir.clubset = item_manager.addItem(item,
                                                    _session, 2, 0)) != item_manager.RetAddItem.TYPE.T_ERROR)
                                                {

                                                    // Equipa o ClubSet CV1
                                                    pWi = _session.m_pi.findWarehouseItemById(_cpir.clubset);

                                                    if (pWi != null)
                                                    {

                                                        // Esse C do WarehouseItem, que pega do DB, não é o ja updado inicial da taqueira é o que fica tabela enchant,
                                                        // que no original fica no warehouse msm, eu só confundi quando fiz
                                                        _session.m_pi.ei.csi.setValues(pWi.id, pWi._typeid, pWi.c);

                                                        cs = sIff.getInstance().findClubSet(pWi._typeid);

                                                        if (cs != null)
                                                        {
                                                            for (var j = 0; j < 5; ++j)
                                                            {
                                                                _session.m_pi.ei.csi.enchant_c[j] = (short)(cs.Stats.getSlot[j] + pWi.clubset_workshop.c[j]);
                                                            }
                                                        }

                                                        _session.m_pi.ei.clubset = pWi;
                                                        _session.m_pi.ue.clubset_id = pWi.id;

                                                        // Update ON DB
                                                        NormalManagerDB.add(0,
                                                            new CmdUpdateClubsetEquiped(_session.m_pi.uid, (int)_cpir.clubset),
                                                            SQLDBResponse, this);

                                                        // Update ON GAME
                                                        p.init_plain((ushort)0x216);

                                                        p.WriteUInt32((uint)UtilTime.GetSystemTimeAsUnix());
                                                        p.WriteUInt32(1); // Count

                                                        p.WriteByte(item.type);
                                                        p.WriteUInt32(item._typeid);
                                                        p.WriteInt32(item.id);
                                                        p.WriteUInt32(item.flag_time);
                                                        p.WriteStruct(item.stat, Marshal.SizeOf(new stItem.item_stat()));
                                                        p.WriteUInt32((item.c[3] > 0) ? item.c[3] : item.c[0]);
                                                        p.WriteZero(25);

                                                        packet_func_sv.session_send(p,
                                                            _session, 1);

                                                    }
                                                    else
                                                    {
                                                        _smp.message_pool.push(new message("[room::requestChangePlayerItemRoom][Log][WARNING] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] nao conseguiu achar o ClubSet\"CV1\"[ID=" + Convert.ToString(item.id) + "] que acabou de adicionar para ele. Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));
                                                    }

                                                }
                                                else
                                                {
                                                    _smp.message_pool.push(new message("[room::requestChangePlayerItemRoom][Log][WARNING] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] nao conseguiu adicionar o ClubSet[TYPEID=" + Convert.ToString(item._typeid) + "] para ele. Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));
                                                }

                                            }
                                            else
                                            {
                                                _smp.message_pool.push(new message("[room::requestChangePlayerItemRoom][Log][WARNING] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] nao conseguiu inicializar o ClubSet[TYPEID=" + Convert.ToString(bi._typeid) + "] para ele. Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));
                                            }
                                        }
                                    }

                                }
                                else
                                {

                                    _error = (uint)((_cpir.clubset == 0) ? 1 : (pWi == null ? 2 : 3));

                                    pWi = _session.m_pi.findWarehouseItemByTypeid(AIR_KNIGHT_SET);

                                    if (pWi != null)
                                    {

                                        _smp.message_pool.push(new message("[room::requestChangePlayerItemRoom][Log][WARNING] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou trocar o ClubSet[ID=" + Convert.ToString(_cpir.clubset) + "] para comecar o jogo, mas deu Error[VALUE=" + Convert.ToString(error) + "], colocando o ClubSet Padrao\"CV1\" do Player. Hacker ou Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));

                                        // Esse C do WarehouseItem, que pega do DB, não é o ja updado inicial da taqueira é o que fica tabela enchant,
                                        // que no original fica no warehouse msm, eu só confundi quando fiz
                                        _session.m_pi.ei.csi.setValues(pWi.id, pWi._typeid, pWi.c);

                                        var cs = sIff.getInstance().findClubSet(pWi._typeid);

                                        if (cs != null)
                                        {
                                            for (var j = 0; j < 5; ++j)
                                            {
                                                _session.m_pi.ei.csi.enchant_c[j] = (short)(cs.Stats.getSlot[j] + pWi.clubset_workshop.c[j]);
                                            }
                                        }

                                        _session.m_pi.ei.clubset = pWi;
                                        _cpir.clubset = _session.m_pi.ue.clubset_id = pWi.id;

                                        // Update ON DB
                                        NormalManagerDB.add(0,
                                            new CmdUpdateClubsetEquiped(_session.m_pi.uid, (int)_cpir.clubset),
                                            SQLDBResponse, this);

                                    }
                                    else
                                    {

                                        _smp.message_pool.push(new message("[room::requestChangePlayerItemRoom][Log][WARNING] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou trocar o ClubSet[ID=" + Convert.ToString(_cpir.clubset) + "] para comecar o jogo, mas deu Error[VALUE=" + Convert.ToString(error) + "], ele nao tem o ClubSet Padrao\"CV1\", adiciona o ClubSet pardrao\"CV1\" para ele. Hacker ou Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));

                                        BuyItem bi = new BuyItem();
                                        stItem item = new stItem();

                                        bi.id = -1;
                                        bi._typeid = AIR_KNIGHT_SET;
                                        bi.qntd = 1;

                                        item_manager.initItemFromBuyItem(_session.m_pi,
                                           ref item, bi, false, 0, 0, 1);

                                        if (item._typeid != 0)
                                        {

                                            if ((_cpir.clubset = item_manager.addItem(item,
                                                _session, 2, 0)) != item_manager.RetAddItem.TYPE.T_ERROR)
                                            {

                                                // Equipa o ClubSet CV1
                                                pWi = _session.m_pi.findWarehouseItemById(_cpir.clubset);

                                                if (pWi != null)
                                                {

                                                    // Esse C do WarehouseItem, que pega do DB, não é o ja updado inicial da taqueira é o que fica tabela enchant,
                                                    // que no original fica no warehouse msm, eu só confundi quando fiz
                                                    _session.m_pi.ei.csi.setValues(pWi.id, pWi._typeid, pWi.c);

                                                    var cs = sIff.getInstance().findClubSet(pWi._typeid);

                                                    if (cs != null)
                                                    {
                                                        for (var j = 0; j < 5; ++j)
                                                        {
                                                            _session.m_pi.ei.csi.enchant_c[j] = (short)(cs.Stats.getSlot[j] + pWi.clubset_workshop.c[j]);
                                                        }
                                                    }

                                                    _session.m_pi.ei.clubset = pWi;
                                                    _session.m_pi.ue.clubset_id = pWi.id;

                                                    // Update ON DB
                                                    NormalManagerDB.add(0,
                                                        new CmdUpdateClubsetEquiped(_session.m_pi.uid, (int)_cpir.clubset),
                                                        SQLDBResponse, this);

                                                    // Update ON GAME
                                                    p.init_plain((ushort)0x216);

                                                    p.WriteUInt32((uint)UtilTime.GetSystemTimeAsUnix());
                                                    p.WriteUInt32(1); // Count

                                                    p.WriteByte(item.type);
                                                    p.WriteUInt32(item._typeid);
                                                    p.WriteInt32(item.id);
                                                    p.WriteUInt32(item.flag_time);
                                                    p.WriteStruct(item.stat, Marshal.SizeOf(new stItem.item_stat()));
                                                    p.WriteUInt32((item.c[3] > 0) ? item.c[3] : item.c[0]);
                                                    p.WriteZero(25);

                                                    packet_func_sv.session_send(p,
                                                        _session, 1);

                                                }
                                                else
                                                {
                                                    _smp.message_pool.push(new message("[room::requestChangePlayerItemRoom][Log][WARNING] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] nao conseguiu achar o ClubSet\"CV1\"[ID=" + Convert.ToString(item.id) + "] que acabou de adicionar para ele. Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));
                                                }

                                            }
                                            else
                                            {
                                                _smp.message_pool.push(new message("[room::requestChangePlayerItemRoom][Log][WARNING] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] nao conseguiu adicionar o ClubSet[TYPEID=" + Convert.ToString(item._typeid) + "] para ele. Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));
                                            }

                                        }
                                        else
                                        {
                                            _smp.message_pool.push(new message("[room::requestChangePlayerItemRoom][Log][WARNING] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] nao conseguiu inicializar o ClubSet[TYPEID=" + Convert.ToString(bi._typeid) + "] para ele. Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));
                                        }
                                    }
                                }

                                // Ball(Comet)
                                if (_cpir.ball != 0
                                    && (pWi = _session.m_pi.findWarehouseItemByTypeid(_cpir.ball)) != null
                                    && sIff.getInstance().getItemGroupIdentify(pWi._typeid) == sIff.getInstance().BALL)
                                {

                                    _session.m_pi.ei.comet = pWi;
                                    _session.m_pi.ue.ball_typeid = _cpir.ball; // Ball(Comet) é o typeid que o cliente passa

                                    // Verifica se a Bola pode ser equipada
                                    if (_session.checkBallEquiped(_session.m_pi.ue))
                                    {
                                        _cpir.ball = _session.m_pi.ue.ball_typeid;
                                    }

                                    // Update ON DB
                                    NormalManagerDB.add(0,
                                        new CmdUpdateBallEquiped(_session.m_pi.uid, _cpir.ball),
                                        SQLDBResponse, this);

                                }
                                else if (_cpir.ball == 0)
                                { // Bola 0 coloca a bola padrão para ele, se for premium user coloca a bola de premium user

                                    // Zera para equipar a bola padrão
                                    _session.m_pi.ei.comet = null;
                                    _session.m_pi.ue.ball_typeid = 0;

                                    // Verifica se a Bola pode ser equipada (Coloca para equipar a bola padrão
                                    if (_session.checkBallEquiped(_session.m_pi.ue))
                                    {
                                        _cpir.ball = _session.m_pi.ue.ball_typeid;
                                    }

                                    // Update ON DB
                                    NormalManagerDB.add(0,
                                        new CmdUpdateBallEquiped(_session.m_pi.uid, _cpir.ball),
                                        SQLDBResponse, this);

                                }
                                else
                                {

                                    _error = ((uint)(pWi == null ? 2 : 3));

                                    pWi = _session.m_pi.findWarehouseItemByTypeid(DEFAULT_COMET_TYPEID);

                                    if (pWi != null)
                                    {

                                        _smp.message_pool.push(new message("[room::requestChangePlayerItemRoom][Log][WARNING] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou trocar a Ball[TYPEID=" + Convert.ToString(_cpir.ball) + "] para comecar o jogo, mas deu Error[VALUE=" + Convert.ToString(error) + "], colocando a Ball Padrao do Player. Hacker ou Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));

                                        _session.m_pi.ei.comet = pWi;
                                        _cpir.ball = _session.m_pi.ue.ball_typeid = pWi._typeid;

                                        // Update ON DB
                                        NormalManagerDB.add(0,
                                            new CmdUpdateBallEquiped(_session.m_pi.uid, _cpir.ball),
                                            SQLDBResponse, this);

                                    }
                                    else
                                    {

                                        _smp.message_pool.push(new message("[room::requestChangePlayerItemRoom][Log][WARNING] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou trocar a Ball[TYPEID=" + Convert.ToString(_cpir.ball) + "] para comecar o jogo, mas deu Error[VALUE=" + Convert.ToString(error) + "], ele nao tem a Ball Padrao, adiciona a Ball pardrao para ele. Hacker ou Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));

                                        BuyItem bi = new BuyItem();
                                        stItem item = new stItem();

                                        bi.id = -1;
                                        bi._typeid = DEFAULT_COMET_TYPEID;
                                        bi.qntd = 1;

                                        item_manager.initItemFromBuyItem(_session.m_pi,
                                           ref item, bi, false, 0, 0, 1);

                                        if (item._typeid != 0)
                                        {

                                            if ((_cpir.ball = item_manager.addItem(item,
                                                _session, 2, 0)) != item_manager.RetAddItem.TYPE.T_ERROR)
                                            {

                                                // Equipa a Ball padrao
                                                pWi = _session.m_pi.findWarehouseItemById(_cpir.ball);

                                                if (pWi != null)
                                                {

                                                    _session.m_pi.ei.comet = pWi;
                                                    _session.m_pi.ue.ball_typeid = pWi._typeid;

                                                    // Update ON DB
                                                    NormalManagerDB.add(0,
                                                        new CmdUpdateBallEquiped(_session.m_pi.uid, _cpir.ball),
                                                        SQLDBResponse, this);

                                                    // Update ON GAME
                                                    p.init_plain((ushort)0x216);

                                                    p.WriteUInt32((uint)UtilTime.GetSystemTimeAsUnix());
                                                    p.WriteUInt32(1); // Count

                                                    p.WriteByte(item.type);
                                                    p.WriteUInt32(item._typeid);
                                                    p.WriteInt32(item.id);
                                                    p.WriteUInt32(item.flag_time);
                                                    p.WriteStruct(item.stat, Marshal.SizeOf(new stItem.item_stat()));
                                                    p.WriteUInt32((item.c[3] > 0) ? item.c[3] : item.c[0]);
                                                    p.WriteZero(25);

                                                    packet_func_sv.session_send(p,
                                                        _session, 1);

                                                }
                                                else
                                                {
                                                    _smp.message_pool.push(new message("[room::requestChangePlayerItemRoom][Log][WARNING] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] nao conseguiu achar a Ball[ID=" + Convert.ToString(item.id) + "] que acabou de adicionar para ele. Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));
                                                }

                                            }
                                            else
                                            {
                                                _smp.message_pool.push(new message("[room::requestChangePlayerItemRoom][Log][WARNING] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] nao conseguiu adicionar a Ball[TYPEID=" + Convert.ToString(item._typeid) + "] para ele. Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));
                                            }

                                        }
                                        else
                                        {
                                            _smp.message_pool.push(new message("[channel::requestChangePlayerItemRoom][Log][WARNING] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] nao conseguiu inicializar a Ball[TYPEID=" + Convert.ToString(bi._typeid) + "] para ele. Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));
                                        }
                                    }
                                }

                                // Verifica se o Mascot Equipado acabou o tempo
                                if (_session.m_pi.ue.mascot_id != 0 && _session.m_pi.ei.mascot_info != null)
                                {

                                    var m_it = _session.m_pi.findUpdateItemByTypeidAndType(_session.m_pi.ue.mascot_id, UpdateItem.UI_TYPE.MASCOT);

                                    if (m_it != null)
                                    {

                                        // Desequipa o Mascot que acabou o tempo dele
                                        _session.m_pi.ei.mascot_info = null;
                                        _session.m_pi.ue.mascot_id = 0;

                                        //NormalManagerDB.add(0,
                                        //    new CmdUpdateMascotEquiped(_session.m_pi.uid, 0),
                                        //    SQLDBResponse, this);

                                        // Update on GAME se não o cliente continua com o mascot equipado
                                        packet_func_sv.session_send(packet_func_sv.pacote04B(_session,
                                          (byte)ChangePlayerItemRoom.TYPE_CHANGE.TC_MASCOT,
                                           0),
                                           _session, 0);

                                    }
                                }

                                // Começa jogo
                                startGame(_session);
                            }
                        }
                        break;
                    default:
                        throw new exception("[room::requestChangePlayerItemRoom][Error] sala[NUMERO=" + Convert.ToString(getNumero()) + "] type desconhecido.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.CHANNEL,
                           13, 1));
                }
            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[room::requestChangePlayerItemRoom][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                packet_func_sv.pacote04B(_session, (byte)_cpir.type,
                    (int)(ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) == (uint)STDA_ERROR_TYPE.ROOM ? ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) : 1));
                packet_func_sv.session_send(p,
                    _session, 0);
            }
        }

        public static void SQLDBResponse(int _msg_id,
                Pangya_DB _pangya_db,
                object _arg)
        {

            if (_arg == null)
            {
                _smp.message_pool.push(new message("[room::SQLDBResponse][WARNING] _arg is nullptr com msg_id = " + Convert.ToString(_msg_id), type_msg.CL_FILE_LOG_AND_CONSOLE));
                return;
            }

            // Por Hora só sai, depois faço outro tipo de tratamento se precisar
            if (_pangya_db.getException().getCodeError() != 0)
            {
                _smp.message_pool.push(new message("[room::SQLDBResponse][Error] " + _pangya_db.getException().getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
                return;
            }

            var _channel = (Channel)(_arg);

            switch (_msg_id)
            {
                case 7: // Update Character PCL
                    {
                        var cmd_ucp = (CmdUpdateCharacterPCL)(_pangya_db);
                        break;
                    }
                case 8: // Update ClubSet Stats
                    {
                        var cmd_ucss = (CmdUpdateClubSetStats)(_pangya_db);

                        break;
                    }
                case 9: // Update Character Mastery
                    {
                        var cmd_ucm = (CmdUpdateCharacterMastery)(_pangya_db);

                        _smp.message_pool.push(new message("[room::SQLDBResponse][Log] Atualizou Character[TYPEID=" + Convert.ToString(cmd_ucm.getInfo()._typeid) + ", ID=" + Convert.ToString(cmd_ucm.getInfo().id) + "] Mastery[value=" + Convert.ToString(cmd_ucm.getInfo().mastery) + "] do player[UID=" + Convert.ToString(cmd_ucm.getUID()) + "]", type_msg.CL_FILE_LOG_AND_CONSOLE));
                        break;
                    }
                case 12: // Update ClubSet Workshop
                    {
                        var cmd_ucw = (CmdUpdateClubSetWorkshop)(_pangya_db);

                        _smp.message_pool.push(new message("[room::SQLDBResponse][Log] player[UID=" + Convert.ToString(cmd_ucw.getUID()) + "] Atualizou ClubSet[TYPEID=" + Convert.ToString(cmd_ucw.getInfo()._typeid) + ", ID=" + Convert.ToString(cmd_ucw.getInfo().id) + "] Workshop[C0=" + Convert.ToString(cmd_ucw.getInfo().clubset_workshop.c[0]) + ", C1=" + Convert.ToString(cmd_ucw.getInfo().clubset_workshop.c[1]) + ", C2=" + Convert.ToString(cmd_ucw.getInfo().clubset_workshop.c[2]) + ", C3=" + Convert.ToString(cmd_ucw.getInfo().clubset_workshop.c[3]) + ", C4=" + Convert.ToString(cmd_ucw.getInfo().clubset_workshop.c[4]) + ", Level=" + Convert.ToString(cmd_ucw.getInfo().clubset_workshop.level) + ", Mastery=" + Convert.ToString(cmd_ucw.getInfo().clubset_workshop.mastery) + ", Rank=" + Convert.ToString(cmd_ucw.getInfo().clubset_workshop.rank) + ", Recovery=" + Convert.ToString(cmd_ucw.getInfo().clubset_workshop.recovery_pts) + "] Flag=" + Convert.ToString(cmd_ucw.getFlag()) + "", type_msg.CL_FILE_LOG_AND_CONSOLE));
                        break;
                    }
                case 26: // Update Mascot Info
                    {

                        var cmd_umi = (CmdUpdateMascotInfo)(_pangya_db);

                        _smp.message_pool.push(new message("[room::SQLDBResponse][Log] Player[UID=" + Convert.ToString(cmd_umi.getUID()) + "] Atualizar Mascot Info[TYPEID=" + Convert.ToString(cmd_umi.getInfo()._typeid) + ", ID=" + Convert.ToString(cmd_umi.getInfo().id) + ", LEVEL=" + Convert.ToString((ushort)cmd_umi.getInfo().level) + ", EXP=" + Convert.ToString(cmd_umi.getInfo().exp) + ", FLAG=" + Convert.ToString((ushort)cmd_umi.getInfo().flag) + ", TIPO=" + Convert.ToString(cmd_umi.getInfo().tipo) + ", IS_CASH=" + Convert.ToString((ushort)cmd_umi.getInfo().is_cash) + ", PRICE=" + Convert.ToString(cmd_umi.getInfo().price) + ", MESSAGE=" + cmd_umi.getInfo().message + ", END_DT=" + UtilTime.FormatDate(cmd_umi.getInfo().data.ConvertTime()) + "]", type_msg.CL_FILE_LOG_AND_CONSOLE));

                        break;
                    }
                case 0:
                default: // 25 é update item equipado slot
                    break;
            }
        }

    }
}