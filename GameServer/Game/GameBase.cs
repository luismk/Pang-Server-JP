// Arquivo Game.cs
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Threading;
using System.Timers;
using Pangya_GameServer.Cmd;
using Pangya_GameServer.Game.Manager;
using Pangya_GameServer.Game.System;
using Pangya_GameServer.Game.Utils;
using Pangya_GameServer.GameType;
using Pangya_GameServer.PacketFunc;
using Pangya_GameServer.Session;
using Pangya_GameServer.UTIL;
using PangyaAPI.IFF.JP.Extensions;
using PangyaAPI.IFF.JP.Models.Flags;
using PangyaAPI.Network.PangyaPacket;
using PangyaAPI.SQL;
using PangyaAPI.SQL.Manager;
using PangyaAPI.Utilities;
using PangyaAPI.Utilities.BinaryModels;
using PangyaAPI.Utilities.Log;
using static Pangya_GameServer.GameType._Define;
using _smp = PangyaAPI.Utilities.Log;
using int32_t = System.Int32;
using int64_t = System.Int64;
using size_t = System.Int32;
using uint32_t = System.UInt32;
using uint64_t = System.UInt64;

namespace Pangya_GameServer.Game
{

    public abstract class GameBase
    {
        protected List<Player> m_players;
        protected Dictionary<Player, PlayerGameInfo> m_player_info;
        protected List<PlayerGameInfo> m_player_order;
        protected Dictionary<uint, uint> m_player_report_game;
        protected RoomInfoEx m_ri;
        protected RateValue m_rv;
        protected int m_game_init_state;
        protected bool m_state;
        protected DateTime m_start_time;
        protected PangyaTimer m_timer;
        protected bool m_channel_rookie;
        protected volatile int m_sync_send_init_data;
        protected Course m_course;

        public object m_cs = new object();// criar o codigo
        public object m_cs_sync_finish_game = new object(); // criar o codigo
                                                            // 
        #region Abstract Methods 
        // Game
        public abstract bool requestFinishGame(Player _session, packet _packet);

        // Inicializa Jogo e Finaliza Jogo
        protected abstract bool init_game();

        // Trata Shot Sync Data
        protected abstract void requestTranslateSyncShotData(Player _session, ShotSyncData _ssd);
        protected abstract void requestReplySyncShotData(Player _session);

        // Metôdos do Game.Course.Hole
        public abstract void requestInitHole(Player _session, packet _packet);
        public abstract bool requestFinishLoadHole(Player _session, packet _packet);
        public abstract void requestFinishCharIntro(Player _session, packet _packet);
        public abstract void requestFinishHoleData(Player _session, packet _packet);

        // São implementados na suas classe base

        // Esses 2 Aqui é do modo VS
        //virtual void changeHole() = 0;
        //virtual void finishHole() = 0;

        // Esses 2 Aqui é do Modo Tourney
        //virtual void changeHole(Player& _session) = 0;
        //virtual void finishHole(Player& _session) = 0;

        // Server enviou a resposta do InitShot para o cliente
        // Esse aqui é exclusivo do VersusBase
        // C++ TO C# CONVERTER TASK: The implementation of the following method could not be found:
        //		virtual void requestInitShotSended(Player _session, packet _packet);

        public abstract void requestInitShot(Player _session, packet _packet);
        public abstract void requestSyncShot(Player _session, packet _packet);
        public abstract void requestInitShotArrowSeq(Player _session, packet _packet);
        public abstract void requestShotEndData(Player _session, packet _packet);
        public abstract RetFinishShot requestFinishShot(Player _session, packet _packet);

        public abstract void requestChangeMira(Player _session, packet _packet);
        public abstract void requestChangeStateBarSpace(Player _session, packet _packet);
        public abstract void requestActivePowerShot(Player _session, packet _packet);
        public abstract void requestChangeClub(Player _session, packet _packet);
        public abstract void requestUseActiveItem(Player _session, packet _packet);
        public abstract void requestChangeStateTypeing(Player _session, packet _packet); // Escrevendo
        public abstract void requestMoveBall(Player _session, packet _packet);
        public abstract void requestChangeStateChatBlock(Player _session, packet _packet);
        public abstract void requestActiveBooster(Player _session, packet _packet);
        public abstract void requestActiveReplay(Player _session, packet _packet);
        public abstract void requestActiveCutin(Player _session, packet _packet);

        // Hability Item
        public abstract void requestActiveRing(Player _session, packet _packet);
        public abstract void requestActiveRingGround(Player _session, packet _packet);
        public abstract void requestActiveRingPawsRainbowJP(Player _session, packet _packet);
        public abstract void requestActiveRingPawsRingSetJP(Player _session, packet _packet);
        public abstract void requestActiveRingPowerGagueJP(Player _session, packet _packet);
        public abstract void requestActiveRingMiracleSignJP(Player _session, packet _packet);
        public abstract void requestActiveWing(Player _session, packet _packet);
        public abstract void requestActivePaws(Player _session, packet _packet);
        public abstract void requestActiveGlove(Player _session, packet _packet);
        public abstract void requestActiveEarcuff(Player _session, packet _packet);

        #endregion

        #region Virtual 

        public virtual bool finish_game(Player _session, int option = 0) { return false; }


        public virtual void DECRYPT16(ref byte[] _buffer)
        {
            if (_buffer.Length > 0 && (_buffer) != null && m_ri.key != null)
                for (var i = 0; i < (_buffer.Length); ++i)
                    (_buffer)[i] ^= (m_ri.key)[i % 16];
        }
        public virtual void INIT_PLAYER_INFO(string _method, string _msg, Player __session, out PlayerGameInfo pgi)
        {
            pgi = getPlayerInfo((__session));
            if (pgi == null)
                throw new exception($"[{GetType().Name}::" + ((_method)) + "][Error] player[UID=" + ((__session).m_pi.uid) + "] " + ((_msg)) + ", mas o game nao tem o info dele guardado. Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.GAME, 1, 4));
        }
        #endregion
        public GameBase(List<Player> _players,
            RoomInfoEx _ri, RateValue _rv,
            bool _channel_rookie)
        {
            this.m_players = _players;
            this.m_ri = _ri;
            this.m_rv = _rv;
            this.m_channel_rookie = _channel_rookie;
            this.m_start_time = DateTime.MinValue;
            this.m_player_info = new Dictionary<Player, PlayerGameInfo>();
            this.m_course = null;
            this.m_game_init_state = -1;
            this.m_state = false;
            this.m_player_order = new List<PlayerGameInfo>();
            this.m_timer = null;
            this.m_player_report_game = new Dictionary<uint, uint>();

            m_cs = new object();
            m_cs_sync_finish_game = new object();
            Interlocked.Exchange(ref m_sync_send_init_data, 0);


            // Inicializar Artefact Info Of Game
            initArtefact();

            // Inicializar o rate chuva dos itens equipado dos players no jogo
            initPlayersItemRainRate();

            // Inicializa a flag persist rain next hole
            initPlayersItemRainPersistNextHole();

            // Map Dados Estáticos
            if (!sMap.getInstance().isLoad())
            {
                sMap.getInstance().load();
            }

            var map = sMap.getInstance().getMap((byte)((int)m_ri.course & 0x7F));

            if (map == null)
            {
                _smp.message_pool.push(new message("[Game::Game][Error][WARNING] tentou pegar o Map dados estaticos do course[COURSE=" + Convert.ToString((ushort)((int)m_ri.course & 0x7F)) + "], mas nao conseguiu encontra na classe do Server.", type_msg.CL_FILE_LOG_AND_CONSOLE));
            }

            // Cria Course
            m_course = new Course(m_ri,
                _channel_rookie,
                ((map == null) ? 1.0f : map.star),
                m_rv.rain, m_rv.persist_rain);

        }

        protected virtual void clear_player_order()
        {

            if (!m_player_order.empty())
            {
                m_player_order.Clear();
            }
        }
        ~GameBase()
        {

            if (m_course != null)
                m_course = null;


            clear_player_order();

            clearAllPlayerInfo();

            clear_time();

            if (!m_player_report_game.empty())
            {
                m_player_report_game.Clear();
            }
              
            _smp.message_pool.push(new message("[Game::~Game][Log] Game destroyed on Room[Number=" + Convert.ToString(m_ri.numero) + "]", type_msg.CL_FILE_LOG_AND_CONSOLE));
        }


        protected virtual void clear_time()
        {

            // Garantir que qualquer exception derrube o server
            try
            {

                if (m_timer != null && m_timer.getState() == PangyaTimer.STATE_TIME.RUN)
                    m_timer.stop();  
            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[Game::clear_time][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }

            m_timer = null;
        }

        public virtual void sendInitialData(Player _session)
        {

            PangyaBinaryWriter p = new PangyaBinaryWriter();

            try
            {

                // Course
                p.init_plain((ushort)0x52);

                p.WriteByte((byte)m_ri.course);
                p.WriteByte(m_ri.tipo_show);
                p.WriteByte(m_ri.modo);
                p.WriteByte(m_ri.qntd_hole);
                p.WriteUInt32(m_ri.trofel);
                p.WriteUInt32(m_ri.time_vs);
                p.WriteUInt32(m_ri.time_30s);//2100000 
                // Hole Info, Hole Spinning Cube, end Seed Random Course
                m_course.makePacketHoleInfo(p);//problema aqui@@@@@@!!!!!!!! use o fix ;)

                packet_func.session_send(p,
                    _session, 1);

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[Game::sendInitialData][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }

        }

        // Envia os dados iniciais para quem entra depois no Game
        public virtual void sendInitialDataAfter(Player _session)
        {
            // UNREFERENCED_PARAMETER(_session);
        }

        protected Player findSessionByOID(int _oid)
        {

            Monitor.Enter(m_cs);


            var it = m_players.Where(el => el.m_oid == _oid);


            Monitor.Exit(m_cs);


            return (it.Any() ? it.FirstOrDefault() : null);
        }

        protected Player findSessionByUID(uint32_t _uid)
        {

            Monitor.Enter(m_cs);

            var it = m_players.Where(el => el.m_pi.uid == _uid);

            Monitor.Exit(m_cs);


            return (it.Any() ? it.FirstOrDefault() : null);
        }

        protected Player findSessionByNickname(string _nickname)
        {

            Monitor.Enter(m_cs);


            var it = m_players.Where(el =>
            {
                return (string.CompareOrdinal(_nickname, el.m_pi.nickname) == 0);
            });

            Monitor.Exit(m_cs);


            return (it.Any() ? it.FirstOrDefault() : null);
        }

        protected Player findSessionByPlayerGameInfo(PlayerGameInfo _pgi)
        {

            if (_pgi == null)
            {
                _smp.message_pool.push(new message("[Game::findSessionByPlayerGameInfo][Error] PlayerGameInfo* _pgi is invalid(null)", type_msg.CL_FILE_LOG_AND_CONSOLE));

                return null;
            }

            Monitor.Enter(m_cs);


            var it = m_player_info.Where(_el =>
            {
                return _el.Value == _pgi;
            });

            Monitor.Exit(m_cs);


            return (it.Any() ? it.FirstOrDefault().Key : null);
        }

        public PlayerGameInfo getPlayerInfo(Player _session)
        {

            if (_session == null)
            {
                throw new exception("[Game::getPlayerInfo][Error] _session is null", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.GAME,
                    1, 0));
            }

            PlayerGameInfo pgi = null;

            Monitor.Enter(m_cs);

            pgi = m_player_info.Where(c => c.Key == _session).FirstOrDefault().Value;

            Monitor.Exit(m_cs);


            return pgi;
        }



        // Se _session for diferente de null retorna todas as session, menos a que foi passada no _session
        public List<Player> getSessions(Player _session = null)
        {

            List<Player> v_sessions = new List<Player>();

            Monitor.Enter(m_cs);

            // Se _session for diferente de null retorna todas as session, menos a que foi passada no _session
            foreach (var el in m_players)
            {
                if (el != null
                    && el.getState()
                    && el.m_pi.mi.sala_numero != ushort.MaxValue
                    && (_session == null || _session != el))
                {
                    v_sessions.Add(el);
                }
            }

            Monitor.Exit(m_cs);


            return new List<Player>(v_sessions);
        }

        public virtual DateTime getTimeStart()
        {
            return m_start_time;
        }

        public virtual void addPlayer(Player _session)
        {
            m_players.Add(_session);

            makePlayerInfo(_session);
        }
        public virtual bool deletePlayer(Player _session, int _option)
        {
            // ignore : UNREFERENCED_PARAMETER(_option);

            if (_session == null)
            {
                throw new exception("[Game::deletePlayer][Error] tentou deletar um player, mas o seu endereco eh null.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.GAME,
                    50, 0));
            }

            Monitor.Enter(m_cs);

            var it = m_players.Any(c => c == _session);

            if (it)
            {
                m_players.Remove(_session);//limpar ou deletar o jogador da lista
            }
            else
            {
                _smp.message_pool.push(new message("[Game::deletePlayer][WARNING] player ja foi excluido do game.", type_msg.CL_FILE_LOG_AND_CONSOLE));
            }

            Monitor.Exit(m_cs);

            return false;
        }

        public virtual void requestActiveAutoCommand(Player _session, packet _packet)
        {
            if (!_session.getState())
            {
                throw new exception("[Game::" + (("request" + "ActiveAutoCommand")) + "][Error] player nao esta connectado.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.GAME,
                    1, 0));
            }

            if (_packet == null)
            {
                throw new exception("[Game::request" + "ActiveAutoCommand" + "][Error] _packet is null", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.GAME,
                    6, 0));
            }

            PangyaBinaryWriter p = new PangyaBinaryWriter();

            try
            {

                var pgi = getPlayerInfo((_session));
                if (pgi == null)
                {
                    throw new exception("[Game::" + "requestActiveAutoCommand" + "][Error] player[UID=" + Convert.ToString((_session).m_pi.uid) + "] " + "tentou ativar Auto Command no jogo" + ", mas o game nao tem o info dele guardado. Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.GAME,
                        1, 4));
                }

                if (!(pgi.premium_flag == 1))
                { // (não é)!PREMIUM USER

                    var pWi = _session.m_pi.findWarehouseItemByTypeid(AUTO_COMMAND_TYPEID);

                    if (pWi == null)
                    {
                        throw new exception("[Game::requestActiveAutoCommand][Error] player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou ativar o Auto Command Item[TYPEID=" + Convert.ToString(AUTO_COMMAND_TYPEID) + "], mas ele nao tem o item. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.GAME, 1, 0x550001));
                    }

                    if (pWi.STDA_C_ITEM_QNTD < 1)
                    {
                        throw new exception("[Game::requestActiveAutoCommand][Error] player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou ativar o Auto Command Item[TYPEID=" + Convert.ToString(AUTO_COMMAND_TYPEID) + "], mas ele nao tem quantidade suficiente do item[QNTD=" + Convert.ToString(pWi.STDA_C_ITEM_QNTD) + ", QNTD_REQ=1]. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.GAME,
                            2, 0x550002));
                    }

                    var it = pgi.used_item.v_passive.find(pWi._typeid);

                    if (it.Value != null)//(it.Key == pgi.used_item.v_passive.end().Key)
                    {
                        throw new exception("[Game::requestActiveAutoCommand][Error] player[UID = " + Convert.ToString(_session.m_pi.uid) + "] tentou ativar Auto Command, mas ele nao tem ele no item passive usados do server. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.TOURNEY_BASE,
                            13, 0));
                    }

                    if ((short)it.Value.count >= pWi.STDA_C_ITEM_QNTD)
                    {
                        throw new exception("[Game::requestActiveAutoCommand][Error] player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou ativar Auto Command, mas ele ja usou todos os Auto Command. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.TOURNEY_BASE,
                            14, 0));
                    }

                    // Add +1 ao item passive usado
                    it.Value.count++;
                }

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[Game::requestActiveAutoCommand][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                // !@ Não sei o que esse pacote faz, não encontrei no meu antigo pangya
                // Resposta Error
                p.init_plain((ushort)0x22B);

                var errorCode = ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) == (uint)STDA_ERROR_TYPE.GAME ? ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) : 0x550001;
                p.WriteUInt32(errorCode);

                packet_func.session_send(p,
                    _session, 1);
            }
        }


        public virtual void requestActiveAssistGreen(Player _session, packet _packet)
        {
            if (!_session.getState())
            {
                throw new exception("[Game::" + (("request" + "ActiveAssistGreen")) + "][Error] player nao esta connectado.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.GAME,
                    1, 0));
            }
            ;
            if (_packet == null)
            {
                throw new exception("[Game::request" + "ActiveAssistGreen" + "][Error] _packet is null", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.GAME,
                    6, 0));
            }

            PangyaBinaryWriter p = new PangyaBinaryWriter();

            try
            {

                uint32_t item_typeid = _packet.ReadUInt32();

                if (item_typeid == 0)
                {
                    throw new exception("[Game::requestActiveAssistGreen][Error] player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou ativar Assist[TYPEID=" + Convert.ToString(item_typeid) + "] do Green, mas o item_typeid is invalid(zero). Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.GAME,
                        1, 0x5200101));
                }


                if (!(item_typeid == 0x1BE00016))
                {
                    throw new exception("[Game::requestActiveAssistGreen][Error] player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou ativar Assist[TYPEID=" + Convert.ToString(item_typeid) + "] do Green, mas o item_typeid esta errado. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.GAME,
                           1, 0x5200101));
                }
                 
                var pWi = _session.m_pi.findWarehouseItemByTypeid(item_typeid);

                if (pWi == null)
                {
                    throw new exception("[Game::requestActiveAssistGreen][Error] player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou ativar Assist[TYPEID=" + Convert.ToString(item_typeid) + "] do Green, mas o Assist Mode do player nao esta ligado. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.GAME,
                        2, 0x5200102));
                }
                if (_session.m_pi.assist_flag)
                {

                    // Resposta para Active Assist Green
                    p.init_plain((ushort)0x26B);

                    p.WriteUInt32(0); // OK

                    p.WriteUInt32(pWi._typeid);
                    p.WriteUInt32(_session.m_pi.uid);

                    packet_func.session_send(p,
                        _session, 1);
                }
                else
                    throw new exception("[Game::requestActiveAssistGreen][Error] player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou ativar Assist[TYPEID=" + Convert.ToString(item_typeid) + "] do Green, mas o Assist Mode do player nao esta ligado. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.GAME,
                       2, 0x5200102));
            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[Game::requestActiveAssistGreen][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                p.init_plain((ushort)0x26B);

                p.WriteUInt32((ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) == (uint)STDA_ERROR_TYPE.GAME) ? ExceptionError.STDA_SYSTEM_ERROR_DECODE(e.getCodeError()) : 0x5200100);

                packet_func.session_send(p,
                    _session, 1);
            }
        }
        // Esse Aqui só tem no VersusBase e derivados dele
        public virtual void requestMarkerOnCourse(Player _session, packet _packet)
        {
            // ignore : UNREFERENCED_PARAMETER(_session);
            // ignore : UNREFERENCED_PARAMETER(_packet);
        }

        public virtual void requestLoadGamePercent(Player _session, packet _packet)
        {
            // ignore : UNREFERENCED_PARAMETER(_session);
            // ignore : UNREFERENCED_PARAMETER(_packet);
        }

        public virtual void requestStartTurnTime(Player _session, packet _packet)
        {
            // ignore : UNREFERENCED_PARAMETER(_session);
            // ignore : UNREFERENCED_PARAMETER(_packet);
        }

        public virtual void requestUnOrPause(Player _session, packet _packet)
        {
            // ignore : UNREFERENCED_PARAMETER(_session);
            // ignore : UNREFERENCED_PARAMETER(_packet);
        }

        // Common Command GM Change Wind Versus
        public virtual void requestExecCCGChangeWind(Player _session, packet _packet)
        {
            // ignore : UNREFERENCED_PARAMETER(_session);
            // ignore : UNREFERENCED_PARAMETER(_packet);
        }

        public virtual void requestExecCCGChangeWeather(Player _session, packet _packet)
        {
            // ignore : UNREFERENCED_PARAMETER(_session);
            // ignore : UNREFERENCED_PARAMETER(_packet);
        }

        // Continua o versus depois que o player saiu no 3 hole pra cima e se for de 18h o game
        public virtual void requestReplyContinue()
        {
        }

        // Esse Aqui só tem no TourneyBase e derivados dele
        public virtual bool requestUseTicketReport(Player _session, packet _packet)
        {
            // ignore : UNREFERENCED_PARAMETER(_session);
            // ignore : UNREFERENCED_PARAMETER(_packet);

            return false;
        }

        // Apenas no Practice que ele é implementado
        public virtual void requestChangeWindNextHoleRepeat(Player _session, packet _packet)
        {
            // ignore : UNREFERENCED_PARAMETER(_session);
            // ignore : UNREFERENCED_PARAMETER(_packet);
        }

        // Exclusivo do Modo Tourney(nao sei o que é, mas vou usar o thread)
        public virtual void requestStartAfterEnter(Thread _job)
        {
            _job.Start();
            // ignore : UNREFERENCED_PARAMETER(_job);
        }

        public virtual void requestEndAfterEnter()
        {
        }

        public virtual void requestUpdateTrofel()
        {
        }

        // Excluviso do Modo Match
        public virtual void requestTeamFinishHole(Player _session, packet _packet)
        {
            // ignore : UNREFERENCED_PARAMETER(_session);
            // ignore : UNREFERENCED_PARAMETER(_packet);
        }
        // Pede o Hole que o player está, 
        // se eles estiver jogando ou 0 se ele não está jogando
        public virtual byte requestPlace(Player _session)
        {

            if (!_session.getState())
            {
                throw new exception("[Game::requestPlace][Error] player nao esta connectado.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.TOURNEY_BASE,
                    1, 0));
            }

            // Valor padrão
            ushort hole = 0;

            var pgi = getPlayerInfo((_session));
            if (pgi == null)
            {
                throw new exception("[Game::" + "requestPlace" + "][Error] player[UID=" + Convert.ToString((_session).m_pi.uid) + "] " + "tentou pegar o lugar[Hole] do player no jogo" + ", mas o game nao tem o info dele guardado. Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.GAME,
                    1, 4));
            }

            if (pgi.hole != 255)
            {

                hole = m_course.findHoleSeq(pgi.hole);

                if (hole == ushort.MaxValue)
                {

                    // Valor padrão
                    hole = 0;

                    _smp.message_pool.push(new message("[Game::requestPlace][Error] player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou pegar a sequencia do hole[NUMERO=" + Convert.ToString(pgi.hole) + "], mas ele nao encontrou no course do game na sala[NUMERO=" + Convert.ToString(m_ri.numero) + "]", type_msg.CL_FILE_LOG_AND_CONSOLE));
                }

            }
            else if (pgi.init_first_hole) // Só cria mensagem de log se o player já inicializou o primeiro hole do jogo e tem um valor inválido no pgi->hole (não é uma sequência de hole válida)
            {
                _smp.message_pool.push(new message("[Game::requesPlace][Error] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou pegar o hole[NUMERO=" + Convert.ToString(pgi.hole) + "] em que o player esta na sala[NUMERO=" + Convert.ToString(m_ri.numero) + "], mas ele esta carregando o course ou tem algum error.", type_msg.CL_FILE_LOG_AND_CONSOLE));
            }

            return (byte)hole;
        }

        // Verifica se o player já esteve na sala
        public virtual bool isGamingBefore(uint32_t _uid)
        {

            if (_uid == 0u)
            {
                throw new exception("[Game::isGamingBefore][Error] _uid is invalid(zero)", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.GAME,
                    1000, 0));
            }

            return m_player_info.FirstOrDefault(_el =>
            {
                return _el.Value.uid == _uid;
            }).Value != m_player_info.end().Value;
        }



        // Exclusivo do Modo Tourney
        public virtual void requestSendTimeGame(Player _session)
        {
            // ignore : UNREFERENCED_PARAMETER(_session);
        }

        public virtual void requestUpdateEnterAfterStartedInfo(Player _session, EnterAfterStartInfo _easi)
        {
            // ignore : UNREFERENCED_PARAMETER(_session);
            // ignore : UNREFERENCED_PARAMETER(_easi);
        }

        // Exclusivo do Grand Zodiac Modo
        public virtual void requestStartFirstHoleGrandZodiac(Player _session, packet _packet)
        {
            // ignore : UNREFERENCED_PARAMETER(_session);
            // ignore : UNREFERENCED_PARAMETER(_packet);
        }

        public virtual void requestReplyInitialValueGrandZodiac(Player _session, packet _packet)
        {
            // ignore : UNREFERENCED_PARAMETER(_session);
            // ignore : UNREFERENCED_PARAMETER(_packet);
        }

        public virtual void requestReadSyncShotData(Player _session,
            packet _packet,
            ref ShotSyncData _ssd)
        {
            if (!_session.getState())
            {
                throw new exception("[Game::" + (("request" + "readSyncShotData")) + "][Error] player nao esta connectado.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.GAME,
                    1, 0));
            }
            ;
            if (_packet == null)
            {
                throw new exception("[Game::request" + "readSyncShotData" + "][Error] _packet is null", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.GAME,
                    6, 0));
            }

            try
            {

                var decrypted = _packet.GetRemainingData;

                DECRYPT16(ref decrypted);

                _packet.SetReader(new PangyaBinaryReader(new MemoryStream(decrypted)));

                _ssd = _packet.Read<ShotSyncData>(); 
                if (_ssd.pang > 37000u)
                {
                    _smp.message_pool.push(new message("[Game::requestReadSyncShotDate][WARNING] player[UID=" + Convert.ToString(_session.m_pi.uid) + "] pode esta usando hack, PANG[" + Convert.ToString(_ssd.pang) + "] maior que 40k. Hacker ou Bug.", type_msg.CL_FILE_LOG_AND_CONSOLE));
                    sgs.gs.getInstance().authCmdBroadcastNotice("[NOTICE_BROAD] PLAYER [" + (_session.m_pi.id) + " ] using hacker Pangs");
                    sgs.gs.getInstance().DisconnectSession(_session);
                }
                if (_ssd.bonus_pang > 10000u)
                {
                    _smp.message_pool.push(new message("[Game::requestReadSyncShotDate][WARNING] player[UID=" + Convert.ToString(_session.m_pi.uid) + "] pode esta usando hack, BONUS PANG[" + Convert.ToString(_ssd.bonus_pang) + "] maior que 10k. Hacker ou Bug.", type_msg.CL_FILE_LOG_AND_CONSOLE));
                    sgs.gs.getInstance().authCmdBroadcastNotice("[NOTICE_BROAD] PLAYER [" + (_session.m_pi.id) + " ] using hacker P-Bonus");
                    sgs.gs.getInstance().DisconnectSession(_session);
                }
            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[Game::requestReadSyncShotData][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
        }

        // Usa o Padrão delas
        public virtual bool stopTime()
        {
            clear_time();


            _smp.message_pool.push(new message("[Game::stopTime][Log] Parou o Timer[Tempo=" + Convert.ToString(m_ri.time_30s > 0 ? m_ri.time_30s : m_ri.time_vs) + (m_timer != null ? "" + "]" : "]"), type_msg.CL_FILE_LOG_AND_CONSOLE));


            return true;
        }

        public virtual bool pauseTime()
        {

            if (m_timer != null)
            {
                m_timer.pause();


                _smp.message_pool.push(new message("[Game::pauseTime][Log] pausou o Timer[Tempo=" + Convert.ToString(m_ri.time_30s > 0 ? m_ri.time_30s : m_ri.time_vs) + "" + "]", type_msg.CL_FILE_LOG_AND_CONSOLE));


                return true;
            }

            return false;
        }

        public virtual bool resumeTime()
        {

            if (m_timer != null)
            {
                m_timer.start();


                _smp.message_pool.push(new message("[Game::resumerTime][Log] Retomou o Timer[Tempo=" + Convert.ToString(m_ri.time_30s > 0 ? m_ri.time_30s : m_ri.time_vs) + "" + "]", type_msg.CL_FILE_LOG_AND_CONSOLE));


                return true;
            }

            return false;
        }

        // Report Game
        public virtual void requestPlayerReportChatGame(Player _session, packet _packet)
        {
            if (!_session.getState())
            {
                throw new exception("[Game::" + (("request" + "PlayerReportChatGame")) + "][Error] player nao esta connectado.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.GAME,
                    1, 0));
            }
            ;
            if (_packet == null)
            {
                throw new exception("[Game::request" + "PlayerReportChatGame" + "][Error] _packet is null", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.GAME,
                    6, 0));
            }

            PangyaBinaryWriter p = new PangyaBinaryWriter();

            try
            {

                // Verifica se o player já reportou o jogo
                var it = m_player_report_game.find(_session.m_pi.uid);

                if (it.Key != 0)//(it.Key != m_player_report_game.end().Key)
                {

                    // Player já reportou o jogo
                    p.init_plain((ushort)0x94);

                    p.WriteByte(1); // Player já reportou o jogo

                    packet_func.session_send(p,
                        _session, 1);

                }
                else
                { // Primeira vez que o palyer report o jogo

                    // add ao mapa de uid de player que reportaram o jogo
                    m_player_report_game[_session.m_pi.uid] = _session.m_pi.uid;

                    // Faz Log de quem está na sala, quando pangya, o update enviar o chat log verifica o chat
                    // por que parece que o pangya não envia o chat, ele só cria um arquivo, acho que quem envia é o update
                    string log = "";

                    foreach (var el in m_players)
                    {
                        if (el != null)
                        {
                            log = log + "UID: " + Convert.ToString(_session.m_pi.uid) + "\tID: " + el.m_pi.id + "\tNICKNAME: " + el.m_pi.nickname + "\n";
                        }
                    }

                    // Log
                    _smp.message_pool.push(new message("[Game::requestPlayerReportChatGame][Log] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] reportou o chat do jogo na sala[NUMERO=" + Convert.ToString(m_ri.numero) + "] Log{" + log + "}", type_msg.CL_FILE_LOG_AND_CONSOLE));

                    // Reposta para o cliente
                    p.init_plain((ushort)0x94);

                    p.WriteByte(0); // Sucesso

                    packet_func.session_send(p,
                        _session, 1);
                }

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[Game::requestPlayerReportChatGame][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                p.init_plain((ushort)0x94);

                p.WriteByte(1); // 1 já foi feito report do jogo por esse player

                packet_func.session_send(p,
                    _session, 1);
            }
        }

        protected virtual void initPlayersItemRainRate()
        {

            // Characters Equip
            foreach (var s in m_players)
            {
                if (s.getState() && s.isConnected())
                { // Check Player Connected

                    if (s.m_pi.ei.char_info == null)
                    { // Player não está com character equipado, kika dele do jogo
                        _smp.message_pool.push(new message("[Game::initPlayersItemRainRate][Log] player[UID=" + Convert.ToString(s.m_pi.uid) + "] nao esta com Character equipado. kika ele do jogo. pode ser Bug.", type_msg.CL_FILE_LOG_AND_CONSOLE));
                        continue; // Kika aqui "deletePlayer(s);"
                    }

                    // Devil Wings
                    if (s.m_pi.ei.char_info.parts_typeid.Any(_element =>
      devil_wings.Contains(_element)))
                    {

                        _smp.message_pool.push(new message("[Game::initPlayersItemRainRate][Log] player[UID=" + Convert.ToString(s.m_pi.uid) + "] esta equipado com Devil Wings no Character[TYPEID=" + Convert.ToString(s.m_pi.ei.char_info._typeid) + ", ID=" + Convert.ToString(s.m_pi.ei.char_info.id) + "]", type_msg.CL_FILE_LOG_AND_CONSOLE));

                        m_rv.rain += 10;
                    }

                    // Obsidian Wings
                    if (s.m_pi.ei.char_info.parts_typeid.Any(_element =>
      obsidian_wings.Contains(_element)))
                    {

                        _smp.message_pool.push(new message("[Game::initPlayersItemRainRate][Log] player[UID=" + Convert.ToString(s.m_pi.uid) + "] esta equipado com Obsidian Wings no Character[TYPEID=" + Convert.ToString(s.m_pi.ei.char_info._typeid) + ", ID=" + Convert.ToString(s.m_pi.ei.char_info.id) + "]", type_msg.CL_FILE_LOG_AND_CONSOLE));

                        m_rv.rain += 10;
                    }

                    // Corrupt Wings
                    if (s.m_pi.ei.char_info.parts_typeid.Any(_element =>
    corrupt_wings.Contains(_element)))
                    {

                        _smp.message_pool.push(new message("[Game::initPlayersItemRainRate][Log] player[UID=" + Convert.ToString(s.m_pi.uid) + "] esta equipado com Corrupt Wings no Character[TYPEID=" + Convert.ToString(s.m_pi.ei.char_info._typeid) + ", ID=" + Convert.ToString(s.m_pi.ei.char_info.id) + "]", type_msg.CL_FILE_LOG_AND_CONSOLE));

                        m_rv.rain += 15;
                    }

                    // Hasegawa Chirain
                    if (s.m_pi.ei.char_info.parts_typeid.Any(_element =>
      hasegawa_chirain.Contains(_element)))
                    {

                        _smp.message_pool.push(new message("[Game::initPlayersItemRainRate][Log] player[UID=" + Convert.ToString(s.m_pi.uid) + "] esta equipado com Hasegawa Chirain Item Part no Character[TYPEID=" + Convert.ToString(s.m_pi.ei.char_info._typeid) + ", ID=" + Convert.ToString(s.m_pi.ei.char_info.id) + "]", type_msg.CL_FILE_LOG_AND_CONSOLE));

                        m_rv.rain += 10;
                    }

                    // Hat Spooky Halloween -- Esse aqui "tenho que colocar a regra para funcionar só na epoca do halloween"
                    if (s.m_pi.ei.char_info.parts_typeid.Any(_element =>
      hat_spooky_halloween.Contains(_element)))
                    {

                        _smp.message_pool.push(new message("[Game::initPlayersItemRainRate][Log] player[UID=" + Convert.ToString(s.m_pi.uid) + "] esta equipado com Hat Spooky Halloween no Character[TYPEID=" + Convert.ToString(s.m_pi.ei.char_info._typeid) + ", ID=" + Convert.ToString(s.m_pi.ei.char_info.id) + "]", type_msg.CL_FILE_LOG_AND_CONSOLE));

                        m_rv.rain += 10;
                    }

                    // Card Efeito 19 rate chuva
                    var it = s.m_pi.v_cei.FirstOrDefault(_el =>
                    {
                        return sIff.getInstance().getItemSubGroupIdentify22(_el._typeid) == 2 && _el.efeito == 19;
                    });

                    if (it != null)
                    {

                        _smp.message_pool.push(new message("[Game::initPlayersItemRainRate][Log] player[UID=" + Convert.ToString(s.m_pi.uid) + "] esta equipado com Card[TYPEID=" + Convert.ToString(it._typeid) + ", EFEITO=" + Convert.ToString(it.efeito) + ", EFEITO_QNTD=" + Convert.ToString(it.efeito_qntd) + "] especial", type_msg.CL_FILE_LOG_AND_CONSOLE));

                        if (it.efeito_qntd > 0)
                        {
                            m_rv.rain += it.efeito_qntd;
                        }
                    }

                    // Mascot Poltergeist -- Esse aqui "tenho que colocar a regra para funcionar só na epoca do halloween"
                    if (s.m_pi.ei.mascot_info != null && s.m_pi.ei.mascot_info._typeid == 0x40000029)
                    {

                        _smp.message_pool.push(new message("[Game::initPlayersItemRainRate][Log] player[UID=" + Convert.ToString(s.m_pi.uid) + "] esta equipado com Mascot Poltergeist[TYPEID=" + Convert.ToString(s.m_pi.ei.mascot_info._typeid) + ", ID=" + Convert.ToString(s.m_pi.ei.mascot_info.id) + "]", type_msg.CL_FILE_LOG_AND_CONSOLE));

                        m_rv.rain += 10;
                    }

                    // Caddie Big Black Papel
                    if (s.m_pi.ei.cad_info != null && s.m_pi.ei.cad_info._typeid == 0x1C00000E)
                    {

                        _smp.message_pool.push(new message("[Game::initPlayersItemRainRate][Log] player[UID=" + Convert.ToString(s.m_pi.uid) + "] esta equipado com Caddie Big Black Papel[TYPEID=" + Convert.ToString(s.m_pi.ei.cad_info._typeid) + ", ID=" + Convert.ToString(s.m_pi.ei.cad_info.id) + "]", type_msg.CL_FILE_LOG_AND_CONSOLE));

                        m_rv.rain += 10;
                    }
                }
            }
        }

        protected virtual void initPlayersItemRainPersistNextHole()
        {

            // Characters Equip
            foreach (var s in m_players)
            {
                if (s.getState() && s.isConnected())
                { // Check Player Connected

                    if (s.m_pi.ei.char_info == null)
                    { // Player não está com character equipado, kika dele do jogo
                        _smp.message_pool.push(new message("[Game::initPlayersItemRainPersistNextHole][Log] player[UID=" + Convert.ToString(s.m_pi.uid) + "] nao esta com Character equipado. kika ele do jogo. pode ser Bug.", type_msg.CL_FILE_LOG_AND_CONSOLE));
                        continue; // Kika aqui "deletePlayer(s);"
                    }

                    // Devil Wings
                    if (s.m_pi.ei.char_info.parts_typeid.Any(_element =>
      devil_wings.Contains(_element)))
                    {

                        _smp.message_pool.push(new message("[Game::initPlayersItemRainPersistNextHole][Log] player[UID=" + Convert.ToString(s.m_pi.uid) + "] esta equipado com Devil Wings no Character[TYPEID=" + Convert.ToString(s.m_pi.ei.char_info._typeid) + ", ID=" + Convert.ToString(s.m_pi.ei.char_info.id) + "]", type_msg.CL_FILE_LOG_AND_CONSOLE));

                        // sai por que só precisa que 1 player tenha o item para valer para o game todo
                        m_rv.persist_rain = 1;
                        return;
                    }

                    // Obsidian Wings
                    if (s.m_pi.ei.char_info.parts_typeid.Any(_element =>
     obsidian_wings.Contains(_element)))

                    {

                        _smp.message_pool.push(new message("[Game::initPlayersItemRainPersistNextHole][Log] player[UID=" + Convert.ToString(s.m_pi.uid) + "] esta equipado com Obsidian Wings no Character[TYPEID=" + Convert.ToString(s.m_pi.ei.char_info._typeid) + ", ID=" + Convert.ToString(s.m_pi.ei.char_info.id) + "]", type_msg.CL_FILE_LOG_AND_CONSOLE));

                        // sai por que só precisa que 1 player tenha o item para valer para o game todo
                        m_rv.persist_rain = 1;
                        return;
                    }

                    // Corrupt Wings
                    if (s.m_pi.ei.char_info.parts_typeid.Any(_element =>
     corrupt_wings.Contains(_element)))

                    {

                        _smp.message_pool.push(new message("[Game::initPlayersItemRainPersistNextHole][Log] player[UID=" + Convert.ToString(s.m_pi.uid) + "] esta equipado com Corrupt Wings no Character[TYPEID=" + Convert.ToString(s.m_pi.ei.char_info._typeid) + ", ID=" + Convert.ToString(s.m_pi.ei.char_info.id) + "]", type_msg.CL_FILE_LOG_AND_CONSOLE));

                        // sai por que só precisa que 1 player tenha o item para valer para o game todo
                        m_rv.persist_rain = 1;
                        return;
                    }

                    // Hasegawa Chirain
                    if (s.m_pi.ei.char_info.parts_typeid.Any(_element =>
     hasegawa_chirain.Contains(_element)))
                    {

                        _smp.message_pool.push(new message("[Game::initPlayersItemRainPersistNextHole][Log] player[UID=" + Convert.ToString(s.m_pi.uid) + "] esta equipado com Hasegawa Chirain Item Part no Character[TYPEID=" + Convert.ToString(s.m_pi.ei.char_info._typeid) + ", ID=" + Convert.ToString(s.m_pi.ei.char_info.id) + "]", type_msg.CL_FILE_LOG_AND_CONSOLE));

                        // sai por que só precisa que 1 player tenha o item para valer para o game todo
                        m_rv.persist_rain = 1;
                        return;
                    }

                    // Hat Spooky Halloween -- Esse aqui "tenho que colocar a regra para funcionar só na epoca do halloween"
                    if (s.m_pi.ei.char_info.parts_typeid.Any(_element =>
     hat_spooky_halloween.Contains(_element)))
                    {

                        _smp.message_pool.push(new message("[Game::initPlayersItemRainPersistNextHole][Log] player[UID=" + Convert.ToString(s.m_pi.uid) +
                            "] esta equipado com Hat Spooky Halloween no Character[TYPEID=" + Convert.ToString(s.m_pi.ei.char_info._typeid) +
                            ", ID=" + Convert.ToString(s.m_pi.ei.char_info.id) + "]", type_msg.CL_FILE_LOG_AND_CONSOLE));


                        m_rv.persist_rain = 1;
                        return;
                    }


                    // Card Efeito 31 Persist chuva para o proximo hole

                    var it = s.m_pi.v_cei.FirstOrDefault(_el =>
                    {
                        return sIff.getInstance().getItemSubGroupIdentify22(_el._typeid) == 2 && _el.efeito == 31;
                    });

                    if (it != null)
                    {

                        _smp.message_pool.push(new message("[Game::initPlayersItemRainPersistNextHole][Log] player[UID=" + Convert.ToString(s.m_pi.uid) + "] esta equipado com Card[TYPEID=" + Convert.ToString(it._typeid) + ", EFEITO=" + Convert.ToString(it.efeito) + ", EFEITO_QNTD=" + Convert.ToString(it.efeito_qntd) + "] especial", type_msg.CL_FILE_LOG_AND_CONSOLE));

                        // sai por que só precisa que 1 player tenha o item para valer para o game todo
                        m_rv.persist_rain = 1;
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// gerar o item do artefato, pode dar exp, pang, e etc...
        /// </summary>
        protected virtual void initArtefact()
        {

            switch (m_ri.artefato)
            {
                // Artefact of EXP
                case ART_LUMINESCENT_CORAL:
                    m_rv.exp += 2;
                    break;
                case ART_TROPICAL_TREE:
                    m_rv.exp += 4;
                    break;
                case ART_TWIN_LUNAR_MIRROR:
                    m_rv.exp += 6;
                    break;
                case ART_MACHINA_WRENCH:
                    m_rv.exp += 8;
                    break;
                case ART_SILVIA_MANUAL:
                    m_rv.exp += 10;
                    break;
                // End
                // Artefact of Rain Rate
                case ART_SCROLL_OF_FOUR_GODS:
                    m_rv.rain += 5;
                    break;
                case ART_ZEPHYR_TOTEM:
                    m_rv.rain += 10;
                    break;
                case ART_DRAGON_ORB:
                    m_rv.rain += 20;
                    break;
                    // End
            }
        }

        protected virtual PlayerGameInfo.eCARD_WIND_FLAG getPlayerWindFlag(Player _session)
        {

            if (_session.m_pi.ei.char_info == null)
            { // Player n�o est� com character equipado, kika dele do jogo
                _smp.message_pool.push(new message("[Game::getPlayerWindFlag][Log] player[UID=" + Convert.ToString(_session.m_pi.uid) + "] nao esta com Character equipado. kika ele do jogo. pode ser Bug.", type_msg.CL_FILE_LOG_AND_CONSOLE));
                return PlayerGameInfo.eCARD_WIND_FLAG.NONE; // Kika aqui "deletePlayer(s);"
            }

            // 3 R, 17 SR, 13 SC, 12 N

            var it = _session.m_pi.v_cei.FirstOrDefault(_el =>
            {
                return (_session.m_pi.ei.char_info.id == _el.parts_id && _session.m_pi.ei.char_info._typeid == _el.parts_typeid) && sIff.getInstance().getItemSubGroupIdentify22(_el._typeid) == 1 && (_el.efeito == 3 || _el.efeito == 17 || _el.efeito == 13 || _el.efeito == 12);
            });

            if (it != null)//(it.Key != _session.m_pi.v_cei.end().Key)
            {

                _smp.message_pool.push(new message("[Game::getPlayerWindFlag][Log] player[UID=" + Convert.ToString(_session.m_pi.uid) + "] esta equipado com Card[TYPEID=" + Convert.ToString(it._typeid) + ", EFEITO=" + Convert.ToString(it.efeito) + ", EFEITO_QNTD=" + Convert.ToString(it.efeito_qntd) + "] Caddie", type_msg.CL_FILE_LOG_AND_CONSOLE));


                switch (it.efeito)
                {
                    case 3:
                        return PlayerGameInfo.eCARD_WIND_FLAG.RARE;
                    case 12:
                        return PlayerGameInfo.eCARD_WIND_FLAG.NORMAL;
                    case 13:
                        return PlayerGameInfo.eCARD_WIND_FLAG.SECRET;
                    case 17:
                        return PlayerGameInfo.eCARD_WIND_FLAG.SUPER_RARE;
                }
            }

            return PlayerGameInfo.eCARD_WIND_FLAG.NONE;
        }

        protected virtual int initCardWindPlayer(PlayerGameInfo _pgi, byte _wind)
        {

            if (_pgi == null)
            {
                throw new exception("[Game::initCardWindPlayer][Error] PlayerGameInfo* _pgi is invalid(null). Ao tentar inicializar o card wind player no jogo. Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.GAME,
                    1, 4));
            }

            switch (_pgi.card_wind_flag)
            {
                case PlayerGameInfo.eCARD_WIND_FLAG.NORMAL:
                    if (_wind == 8) // 9m Wind
                    {
                        return -1;
                    }
                    break;
                case PlayerGameInfo.eCARD_WIND_FLAG.RARE:
                    if (_wind > 0) // All Wind
                    {
                        return -1;
                    }
                    break;
                case PlayerGameInfo.eCARD_WIND_FLAG.SUPER_RARE:
                    if (_wind >= 5) // High(strong) Wind
                    {
                        return -2;
                    }
                    break;
                case PlayerGameInfo.eCARD_WIND_FLAG.SECRET:
                    if (_wind >= 5) // High(strong) Wind
                    {
                        return -2;
                    }
                    else if (_wind > 0) // Low(weak) Wind, 1m não precisa diminuir
                    {
                        return -1;
                    }
                    break;
            }

            return 0;
        }

        protected virtual PlayerGameInfo.stTreasureHunterInfo getPlayerTreasureInfo(Player _session)
        {

            PlayerGameInfo.stTreasureHunterInfo pti = new PlayerGameInfo.stTreasureHunterInfo();

            if (_session.m_pi.ei.char_info == null)
            { // Player não está com character equipado, kika dele do jogo
                _smp.message_pool.push(new message("[Game::getPlayerTreasureInfo][Log] player[UID=" + Convert.ToString(_session.m_pi.uid) + "] nao esta com Character equipado. kika ele do jogo. pode ser Bug.", type_msg.CL_FILE_LOG_AND_CONSOLE));
                return pti; // Kika aqui "deletePlayer(s);"
            }

            List<CardEquipInfoEx> v_cei = new List<CardEquipInfoEx>();

            // 9 N, 10 R, 14 SR por Score. 8 N, R, SR todos score
            _session.m_pi.v_cei.ToList().ForEach(el =>
             {
                 var _el = el;
                 if ((_session.m_pi.ei.char_info.id == _el.parts_id && _session.m_pi.ei.char_info._typeid == _el.parts_typeid)
                     && sIff.getInstance().getItemSubGroupIdentify22(_el._typeid) == 1
                     && (_el.efeito == 8 || _el.efeito == 9 || _el.efeito == 10 || _el.efeito == 14))
                 {
                     v_cei.Add((CardEquipInfoEx)el);
                 }
             });

            if (v_cei.Count > 0)
            {
                foreach (var el in v_cei)
                {

                    _smp.message_pool.push(new message("[Game::getPlayerTreasureInfo][Log] player[UID=" + Convert.ToString(_session.m_pi.uid) + "] esta equipado com Card[TYPEID=" + Convert.ToString(el._typeid) + ", EFEITO=" + Convert.ToString(el.efeito) + ", EFEITO_QNTD=" + Convert.ToString(el.efeito_qntd) + "] Caddie", type_msg.CL_FILE_LOG_AND_CONSOLE));


                    switch (el.efeito)
                    {
                        case 8: // Todos Score
                            pti.all_score = (byte)el.efeito_qntd;
                            break;
                        case 9: // Par
                            pti.par_score = (byte)el.efeito_qntd;
                            break;
                        case 10: // Birdie
                            pti.birdie_score = (byte)el.efeito_qntd;
                            break;
                        case 14: // Eagle
                            pti.eagle_score = (byte)el.efeito_qntd;
                            break;
                    }
                }
            }

            // Card Efeito 18 Aumenta o treasure point para qualquer score por 2 horas

            var it = _session.m_pi.v_cei.FirstOrDefault(_el =>
            {
                return sIff.getInstance().getItemSubGroupIdentify22(_el._typeid) == 2 && _el.efeito == 18;
            });

            if (it != null)//(it.Key != _session.m_pi.v_cei.end().Key)
            {

                _smp.message_pool.push(new message("[Game::getPlayerTreasureInfo][Log] player[UID=" + Convert.ToString(_session.m_pi.uid) + "] esta equipado com Card[TYPEID=" + Convert.ToString(it._typeid) + ", EFEITO=" + Convert.ToString(it.efeito) + ", EFEITO_QNTD=" + Convert.ToString(it.efeito_qntd) + "] especial", type_msg.CL_FILE_LOG_AND_CONSOLE));


                pti.all_score += (byte)it.efeito_qntd;
            }

            /// Todos que dão Drop Rate da treasue hunter point, então aonde dá o drop rate já vai dá o treasure point
            /// Angel Wings deixa que ela é uma excessão não tem os valores no IFF, é determinado pelo server e o ProjectG
            // Passarinho gordo aumenta 30 treasure hunter point para todos scores
            //if (_session.m_pi.ei.mascot_info != null && _session.m_pi.ei.mascot_info->_typeid == MASCOT_FAT_BIRD)
            //pti.all_score += 30;	// +30 all score

            // Verifica se está com asa de anjo equipada (shop ou gacha), aumenta 30 treasure hunter point para todos scores
            if (_session.m_pi.ei.char_info.AngelEquiped() == 1 && _session.m_pi.ui.getQuitRate() < GOOD_PLAYER_ICON)
            {
                pti.all_score += 30; // +30 all score
            }

            return pti;
        }

        protected virtual void updatePlayerAssist(Player _session)
        {

            var pgi = getPlayerInfo((_session));
            if (pgi == null)
            {
                throw new exception("[Game::" + "updatePlayerAssist" + "][Error] player[UID=" + Convert.ToString((_session).m_pi.uid) + "] " + "tentou atualizar assist pang no jogo" + ", mas o game nao tem o info dele guardado. Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.GAME,
                    1, 4));
            }

            if (pgi.assist_flag == 1 && pgi.level > 10)
            {
                pgi.data.pang = (uint64_t)(pgi.data.pang * 0.7f); // - 30% dos pangs
            }

        }

        protected virtual void initGameTime()
        {
            UtilTime.GetLocalTime(out m_start_time);
        }

        protected virtual uint32_t getRankPlace(Player _session)
        {

            var pgi = getPlayerInfo((_session));
            if (pgi == null)
            {
                throw new exception("[Game::" + "getRankPlace" + "][Error] player[UID=" + Convert.ToString((_session).m_pi.uid) + "] " + "tentou pegar o lugar no rank do jogo" + ", mas o game nao tem o info dele guardado. Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.GAME,
                    1, 4));
            }

            int index = m_player_order.IndexOf(pgi);

            return (index != -1) ? (uint)index : ~0u;
        }

        protected virtual DropItemRet requestInitDrop(Player _session)
        {

            if (!sDropSystem.getInstance().isLoad())
            {
                sDropSystem.getInstance().load();
            }

            DropItemRet dir = new DropItemRet();

            var pgi = getPlayerInfo((_session));
            if (pgi == null)
            {
                throw new exception("[Game::" + "requestInitDrop" + "][Error] player[UID=" + Convert.ToString((_session).m_pi.uid) + "] " + "tentou inicializar drop do hole no jogo" + ", mas o game nao tem o info dele guardado. Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.GAME,
                    1, 4));
            }

            DropSystem.stCourseInfo ci = new DropSystem.stCourseInfo();

            var hole = m_course.findHole(pgi.hole);

            if (hole == null)
            {
                throw new exception("[Game::requestInitDrop][Error] player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou inicializar Drop System do hole[NUMERO=" + Convert.ToString(pgi.hole) + "] no jogo, mas nao encontrou o hole no course do game. Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.GAME,
                    200, 0));
            }

            // Init Course Info Drop System
            ci.artefact = m_ri.artefato;
            ci.char_motion = pgi.char_motion_item;
            ci.course = (byte)(hole.getCourse() & 0x7F); // Course do Hole, Por que no SSC, cada hole é um course
            ci.hole = pgi.hole;
            ci.seq_hole = (byte)m_course.findHoleSeq(pgi.hole);
            ci.qntd_hole = m_ri.qntd_hole;
            ci.rate_drop = pgi.used_item.rate.drop;

            if (_session.m_pi.ei.char_info != null && _session.m_pi.ui.getQuitRate() < GOOD_PLAYER_ICON)
            {
                ci.angel_wings = _session.m_pi.ei.char_info.AngelEquiped();
            }
            else
            {
                ci.angel_wings = 0;
            }

            // Artefact Pang Drop
            if (m_ri.qntd_hole == ci.seq_hole && m_ri.qntd_hole == 18)
            { // Ultimo Hole, de 18h Game
                var art_pang = sDropSystem.getInstance().drawArtefactPang(ci, (uint32_t)m_players.Count());

                if (art_pang._typeid != 0)
                { // Dropou

                    dir.v_drop.Add(art_pang);

                    if (art_pang.qntd >= 30)
                    { // Envia notice que o player ganhou jackpot

                        PangyaBinaryWriter p = new PangyaBinaryWriter((ushort)0x40);

                        p.WriteByte(10); // JackPot

                        p.WriteString(_session.m_pi.nickname);

                        p.WriteUInt16(0); // size Msg

                        p.WriteInt32(art_pang.qntd * 500);

                        packet_func.game_broadcast(this,
                            p.GetBytes, 1);
                    }
                }
            }

            // Drop Event Course
            var course = sDropSystem.getInstance().findCourse((byte)(ci.course & 0x7F));

            if (course != null)
            { // tem Drop nesse Course
                var drop_event = sDropSystem.getInstance().drawCourse(course, ci);

                if (!drop_event.empty()) // Dropou
                {
                    dir.v_drop.AddRange(dir.v_drop);
                }
            }

            // Drop Mana Artefact
            var mana_drop = sDropSystem.getInstance().drawManaArtefact(ci);

            if (mana_drop._typeid != 0) // Dropou
            {
                dir.v_drop.Add(mana_drop);
            }

            // Drop Grand Prix Ticket, não drop no Grand Prix
            if (m_ri.qntd_hole == ci.seq_hole && m_ri.getTipo() != RoomInfo.TIPO.GRAND_PRIX)
            {
                var gp_ticket = sDropSystem.getInstance().drawGrandPrixTicket(ci, _session);

                if (gp_ticket._typeid != 0) // Dropou
                {
                    dir.v_drop.Add(gp_ticket);
                }
            }

            // SSC Ticket
            var ssc = sDropSystem.getInstance().drawSSCTicket(ci);

            if (!ssc.empty())
            {
                dir.v_drop.AddRange(ssc);

                // SSC Ticket Achievement
                //    pgi.sys_achieve.incrementCounter(0x6C400053u, (int)ssc.Count);
            }

            // Adiciona para a lista de drop's do player
            if (!dir.v_drop.empty())
            {
                pgi.drop_list.v_drop.AddRange(dir.v_drop);

            }

            return (dir);
        }


        protected virtual void requestSaveDrop(Player _session)
        {

            var pgi = getPlayerInfo((_session));
            if (pgi == null)
            {
                throw new exception("[Game::" + "requestSaveDrop" + "][Error] player[UID=" + Convert.ToString((_session).m_pi.uid) + "] " + "tentou salvar drop item no jogo" + ", mas o game nao tem o info dele guardado. Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.GAME,
                    1, 4));
            }

            if (!pgi.drop_list.v_drop.empty())
            {

                List<stItem> v_item = new List<stItem>();
                stItem item = new stItem();

                foreach (var el in pgi.drop_list.v_drop)
                {
                    item.clear();

                    item.type = 2;
                    item._typeid = el._typeid;
                    item.qntd = (uint)((el.type == DropItem.eTYPE.QNTD_MULTIPLE_500) ? el.qntd * 500 : el.qntd);
                    item.STDA_C_ITEM_QNTD = (ushort)item.qntd;

                    if (!v_item.Any(c=>c._typeid== item._typeid))
                    {
                        // Novo item
                        v_item.Add(item);
                    }
                    else
                    { // J� tem
                        v_item.First(c => c._typeid == item._typeid).qntd += item.qntd;
                        v_item.First(c => c._typeid == item._typeid).STDA_C_ITEM_QNTD = (ushort)v_item.First(c => c._typeid == item._typeid).qntd;
                    }
                }
                var rai = item_manager.addItem(v_item, _session, 0, 0); 

                if (rai.fails.Count() > 0 && rai.type != item_manager.RetAddItem.TYPE.T_SUCCESS_PANG_AND_EXP_AND_CP_POUCH)
                {
                    _smp.message_pool.push(new message("[Game:requestSaveDrop][WARNIG] nao conseguiu adicionar os drop itens. Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));
                }

                PangyaBinaryWriter p = new PangyaBinaryWriter((ushort)0x216);

                p.WriteUInt32((uint32_t)UtilTime.GetSystemTimeAsUnix());
                p.WriteUInt32((uint32_t)v_item.Count);

                foreach (var el in v_item)
                {
                    p.WriteByte(el.type);
                    p.WriteUInt32(el._typeid);
                    p.WriteInt32(el.id);
                    p.WriteUInt32(el.flag_time);
                    p.WriteBytes(el.stat.ToArray());
                    p.WriteUInt32((el.STDA_C_ITEM_TIME > 0) ? el.STDA_C_ITEM_TIME : el.STDA_C_ITEM_QNTD);
                    p.WriteZeroByte(25);
                }

                packet_func.session_send(p,
                    _session, 1);
            }
        }



        protected virtual DropItemRet requestInitCubeCoin(Player _session, packet _packet)
        {
            if (!_session.getState())
            {
                throw new exception("[Game::" + (("request" + "InitCubeCoin")) + "][Error] player nao esta connectado.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.GAME,
                    1, 0));
            }
            ;
            if (_packet == null)
            {
                throw new exception("[Game::request" + "InitCubeCoin" + "][Error] _packet is null", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.GAME,
                    6, 0));
            }

            try
            {

                byte opt = _packet.ReadByte();
                byte count = _packet.ReadByte();

                // Player que tacou e tem drops (Coin ou Cube)
                if (opt == 1 && count > 0)
                {

                    DropItemRet dir = new DropItemRet();

                    var pgi = getPlayerInfo((_session));
                    if (pgi == null)
                    {
                        throw new exception("[Game::" + "initCubeCoin" + "][Error] player[UID=" + Convert.ToString((_session).m_pi.uid) + "] " + "tentou terninar o hole no jogo" + ", mas o game nao tem o info dele guardado. Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.GAME,
                            1, 4));
                    }

                    var hole = m_course.findHole(pgi.hole);

                    if (hole == null)
                    {
                        throw new exception("[Game::requestInitCubeCoin][Error] player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou terminar hole[NUMERO=" + Convert.ToString((ushort)pgi.hole) + "], mas no course nao tem esse hole. Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.GAME,
                            250, 0));
                    }

                    uint32_t tipo = 0;
                    uint32_t id = 0;

                    CubeEx pCube = null;

                    for (var i = 0; i < count; ++i)
                    {

                        tipo = _packet.ReadByte();
                        id = _packet.ReadUInt32();

                        pCube = hole.findCubeCoin(id);

                        if (pCube == null)
                        {
                            throw new exception("[Game::requestInitCubeCoin][Error] player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou terminar hole[NUMERO=" + Convert.ToString((ushort)pgi.hole) + "], mas o cliente forneceu um cube/coin id[ID=" + Convert.ToString(id) + "] invalido. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.GAME,
                                251, 0));
                        }
                        if (tipo == 0)
                        // Coin
                        {
                            // Tipo 3 Coin da borda do green ganha menos pangs ganha de 1 a 50, Tipo 4 Coin no chão qualquer lugar ganha mais pang de 1 a 200
                            dir.v_drop.Add(new DropItem(
                                COIN_TYPEID,
                                (byte)hole.getCourse(),
                                (byte)hole.getNumero(),
                                (short)((sRandomGen.getInstance().rIbeMt19937_64_chrono() % ((uint)(pCube.flag_location == 0 ? 50 : 200))) + 1),
                                (pCube.flag_location == 0) ? DropItem.eTYPE.COIN_EDGE_GREEN : DropItem.eTYPE.COIN_GROUND
                            ));

                        }
                        if (tipo == 1) // Cube
                        {
                            dir.v_drop.Add(new DropItem(SPINNING_CUBE_TYPEID, (byte)hole.getCourse(), (byte)hole.getNumero(), 1, DropItem.eTYPE.CUBE));
                        }
                    }

                    // Add os Cube Coin para o player list drop
                    pgi.drop_list.v_drop.AddRange(pgi.drop_list.v_drop);

                    return (dir);
                } 
            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[Game::requestInitCubeCoin][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }

            return new DropItemRet();
        }

        public virtual void requestCalculePang(Player _session)
        {

            var pgi = getPlayerInfo((_session));
            if (pgi == null)
            {
                throw new exception("[Game::" + "requestCalculePang" + "][Error] player[UID=" + Convert.ToString((_session).m_pi.uid) + "] " + "tentou calcular o pang do player no jogo" + ", mas o game nao tem o info dele guardado. Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.GAME,
                    1, 4));
            }

            // Course Rate of Pang
            var course = sIff.getInstance().findCourse((uint)((int)m_ri.course & 0x7F) | 0x28000000u);

            // Rate do course, tem uns que é 10% a+ tem outros que é 30% a mais que o pangya JP deixou
            float course_rate = (course != null && course.RatePang >= 1.0f) ? course.RatePang : 1.0f;
            float pang_rate = 0.0f;

            pang_rate = TRANSF_SERVER_RATE_VALUE((int)pgi.used_item.rate.pang) * TRANSF_SERVER_RATE_VALUE((int)(m_rv.pang + (m_ri.modo == (byte)RoomInfo.eMODO.M_SHUFFLE ? 10 : 0))) * course_rate;

            pgi.data.bonus_pang = (uint64_t)(((pgi.data.pang * pang_rate) - pgi.data.pang) + (pgi.data.bonus_pang * pang_rate));
        }

        protected virtual void requestSaveInfo(Player _session, int option)
        {

            var pgi = getPlayerInfo((_session));
            if (pgi == null)
            {
                throw new exception("[Game::" + "requestSaveInfo" + "][Error] player[UID=" + Convert.ToString((_session).m_pi.uid) + "] " + "tentou salvar o info dele no jogo" + ", mas o game nao tem o info dele guardado. Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.GAME,
                    1, 4));
            }

            try
            {

                // Aqui dados do jogo ele passa o holein no lugar do mad_conduta <-> holein, agora quando ele passa o info user é invertido(Normal)
                // Inverte para salvar direito no banco de dados
                var tmp_holein = pgi.ui.hole_in;


                _smp.message_pool.push(new message("[Game::requestSaveInfo][Log] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] UserInfo[" + pgi.ui.ToString() + "]", type_msg.CL_FILE_LOG_AND_CONSOLE));


                pgi.ui.hole_in = pgi.ui.mad_conduta;
                pgi.ui.mad_conduta = tmp_holein;

                if (option == 0)
                { // Terminou VS

                    // Verifica se o Angel Event está ativo de tira 1 quit do player que concluí o jogo
                    if (m_ri.angel_event)
                    {

                        pgi.ui.quitado = -1;

                        _smp.message_pool.push(new message("[Game::requestSaveInfo][Log][AngelEvent] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] vai reduzir o quit em " + Convert.ToString(pgi.ui.quitado * -1) + " unidade(s).", type_msg.CL_FILE_LOG_AND_CONSOLE));
                    }

                    pgi.ui.exp = 0;
                    pgi.ui.combo = 1;
                    pgi.ui.jogado = 1;
                    pgi.ui.media_score = pgi.data.score;

                    // Os valores que eu não colocava
                    pgi.ui.jogados_disconnect = 1; // Esse aqui é o contador de jogos que o player começou é o mesmo do jogado, só que esse aqui usa para o disconnect

                    var diff = UtilTime.GetLocalDateDiff(m_start_time);

                    if (diff > 0)
                    {
                        diff /= STDA_10_MICRO_PER_SEC; // NanoSeconds To Seconds
                    }

                    pgi.ui.tempo = (int32_t)diff;

                }
                else if (option == 1)
                { // Quitou ou tomou DC

                    // Quitou ou saiu não ganha pangs
                    pgi.data.pang = 0;
                    pgi.data.bonus_pang = 0;

                    pgi.ui.exp = 0;
                    pgi.ui.combo = (int)(DECREASE_COMBO_VALUE * -1);
                    pgi.ui.jogado = 1;

                    // Verifica se tomou DC ou Quitou, ai soma o membro certo
                    if (!_session.m_connection_timeout)
                    {
                        pgi.ui.quitado = 1;
                    }
                    else
                    {
                        pgi.ui.disconnect = 1;
                    }

                    // Os valores que eu não colocava
                    pgi.ui.jogados_disconnect = 1; // Esse aqui é o contador de jogos que o player começou é o mesmo do jogado, só que esse aqui usa para o disconnect

                    pgi.ui.media_score = pgi.data.score;

                    var diff = UtilTime.GetLocalDateDiff(m_start_time);

                    if (diff > 0)
                    {
                        diff /= STDA_10_MICRO_PER_SEC; // NanoSeconds To Seconds
                    }

                    pgi.ui.tempo = (int32_t)diff;

                }
                else if (option == 2)
                { // Não terminou o hole 1, alguem saiu ai volta para sala sem contar o combo, só conta o jogo que começou

                    pgi.data.pang = 0;
                    pgi.data.bonus_pang = 0;

                    pgi.ui.exp = 0;
                    pgi.ui.jogado = 1;

                    // Os valores que eu não colocava
                    pgi.ui.jogados_disconnect = 1; // Esse aqui é o contador de jogos que o player começou é o mesmo do jogado, só que esse aqui usa para o disconnect

                    var diff = UtilTime.GetLocalDateDiff(m_start_time);

                    if (diff > 0)
                    {
                        diff /= STDA_10_MICRO_PER_SEC; // NanoSeconds To Seconds
                    }

                    pgi.ui.tempo = (int32_t)diff;

                }
                else if (option == 4)
                { // SSC

                    pgi.ui.clear();

                    // Verifica se o Angel Event está ativo de tira 1 quit do player que concluí o jogo
                    if (m_ri.angel_event)
                    {

                        pgi.ui.quitado = -1;

                        _smp.message_pool.push(new message("[Game::requestSaveInfo][Log][AngelEvent] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] vai reduzir o quit em " + Convert.ToString(pgi.ui.quitado * -1) + " unidade(s).", type_msg.CL_FILE_LOG_AND_CONSOLE));
                    }

                    pgi.ui.exp = 0;
                    pgi.ui.combo = 1;
                    pgi.ui.jogado = 1;
                    pgi.ui.media_score = 0;

                    // Os valores que eu não colocava
                    pgi.ui.jogados_disconnect = 1; // Esse aqui é o contador de jogos que o player começou é o mesmo do jogado, só que esse aqui usa para o disconnect

                    var diff = UtilTime.GetLocalDateDiff(m_start_time);

                    if (diff > 0)
                    {
                        diff /= STDA_10_MICRO_PER_SEC;
                    }

                    pgi.ui.tempo = (int32_t)diff;

                }
                else if (option == 5)
                {

                    // Quitou ou saiu não ganha pangs
                    pgi.data.pang = 0;
                    pgi.data.bonus_pang = 0;

                    pgi.ui.exp = 0;
                    pgi.ui.jogado = 1;
                    pgi.ui.media_score = pgi.data.score;

                    // Os valores que eu não colocava
                    pgi.ui.jogados_disconnect = 1; // Esse aqui é o contador de jogos que o player começou é o mesmo do jogado, só que esse aqui usa para o disconnect

                    var diff = UtilTime.GetLocalDateDiff(m_start_time);

                    if (diff > 0)
                    {
                        diff /= STDA_10_MICRO_PER_SEC; // NanoSeconds To Seconds
                    }

                    pgi.ui.tempo = (int32_t)diff;
                }

                // Achievement Records
                records_player_achievement(_session);

                // Pode tirar pangs
                int64_t total_pang = (long)(pgi.data.pang + pgi.data.bonus_pang);

                // UPDATE ON SERVER AND DB
                _session.m_pi.addUserInfo(pgi.ui, (ulong)total_pang); // add User Info

                if (total_pang > 0)
                {
                    _session.m_pi.addPang((ulong)total_pang); // add Pang
                }
                else if (total_pang < 0)
                {
                    _session.m_pi.consomePang((ulong)(total_pang * -1)); // consome Pangs
                }

                // Game Combo
                if (_session.m_pi.ui.combo > 0)
                {
                    // pgi.sys_achieve.incrementCounter(0x6C40004Bu, _session.m_pi.ui.combo);
                }

            }
            catch (exception e)
            {
                _smp.message_pool.push(new message("[Game::requestSaveInfo][Error] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
        }

        protected virtual void requestUpdateItemUsedGame(Player _session)
        {

            var pgi = getPlayerInfo((_session));
            if (pgi == null)
            {
                throw new exception("[Game::" + "requestUpdateItemUsedGame" + "][Error] player[UID=" + Convert.ToString((_session).m_pi.uid) + "] " + "tentou atualizar itens usado no jogo" + ", mas o game nao tem o info dele guardado. Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.GAME,
                    1, 4));
            }

            var ui = pgi.used_item;

            // Club Mastery // ((int)((int)m_ri.course & 0x7F) == RoomInfo.TIPO.SPECIAL_SHUFFLE_COURSE ? 1.5f : 1.f), SSSC sobrecarrega essa função para colocar os valores dele
            ui.club.count += (uint32_t)(1.0f * 10.0f * ui.club.rate * TRANSF_SERVER_RATE_VALUE((int)m_rv.clubset) * TRANSF_SERVER_RATE_VALUE((int)ui.rate.club));

            // Passive Item exceto Time Booster e Auto Command, que soma o contador por uso, o cliente passa o pacote, dizendo que usou o item
            foreach (var el in ui.v_passive)
            {

                // Verica se é o ultimo hole, terminou o jogo, ai tira soma 1 ao count do pirulito que consome por jogo
                if (CHECK_PASSIVE_ITEM(el.Value._typeid)
                    && el.Value._typeid != TIME_BOOSTER_TYPEID
                    && el.Value._typeid != AUTO_COMMAND_TYPEID)
                {

                    // Item de Exp Boost que só consome 1 Por Jogo, só soma no requestFinishItemUsedGame
                    if (passive_item_exp_1perGame.Contains(el.Value._typeid))
                    {
                        el.Value.count++;
                    }

                }
                else if (sIff.getInstance().getItemGroupIdentify(el.Value._typeid) == sIff.getInstance().BALL || sIff.getInstance().getItemGroupIdentify(el.Value._typeid) == sIff.getInstance().AUX_PART) //AuxPart(Anel)
                {
                    el.Value.count++;
                }
            }
        }

        protected virtual void requestFinishItemUsedGame(Player _session)
        {

            List<stItemEx> v_item = new List<stItemEx>();
            stItemEx item = new stItemEx();

            var pgi = getPlayerInfo((_session));
            if (pgi == null)
            {
                throw new exception("[Game::" + "requestFinishItemUsedGame" + "][Error] player[UID=" + Convert.ToString((_session).m_pi.uid) + "] " + "tentou finalizar itens usado no jogo" + ", mas o game nao tem o info dele guardado. Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.GAME,
                    1, 4));
            }

            // Player já finializou os itens usados, verifica para não finalizar dua vezes os itens do player
            if (pgi.finish_item_used == 1)
            {

                _smp.message_pool.push(new message("[Game::requestFinishItemUsedGame][WARNING] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] ja finalizou os itens. Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));

                return;
            }

            var ui = pgi.used_item;

            uint32_t tmp_counter_typeid = 0;

            // Add +1 ao itens que consome 1 só por jogo
            // Item de Exp Boost que só consome 1 Por Jogo
            foreach (var _el in ui.v_passive)
            {
                if (passive_item_exp_1perGame.Contains((uint)_el.Value._typeid))
                {
                    _el.Value.count++;
                }
            }


            // Verifica se é premium 2 e se ele tem o var caliper para poder somar no Achievement
            if (_session.m_pi.m_cap.premium_user && sPremiumSystem.getInstance().isPremium(_session.m_pi.pt._typeid))
            {

                var it_ac = ui.v_passive.find(AUTO_CALIPER_TYPEID);

                if (it_ac.Key == ui.v_passive.end().Key)
                {

                    uint32_t qntd = m_course.findHoleSeq(pgi.hole);

                    if (unchecked(qntd == (ushort)~0u))
                    {
                        qntd = m_ri.qntd_hole;
                    }

                    // Adiciona Auto Caliper para ser contado no Achievement
                    var it_p_ac = ui.v_passive.insert(AUTO_CALIPER_TYPEID,
                        new UsedItem.Passive(typeid: AUTO_CALIPER_TYPEID, _count: qntd));

                    if (!(it_p_ac.Value != null) && it_p_ac.Key == ui.v_passive.end().Key)
                    {
                        // Log
                        _smp.message_pool.push(new message("[Game::requestFinishItemUsedGame][Error][WARNING] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] nao conseguiu adicionar o Auto Caliper passive item para adicionar no contador do Achievement, por que ele eh premium user 2", type_msg.CL_FILE_LOG_AND_CONSOLE));
                    }
                }
            }

            // Passive Item
            foreach (var el in ui.v_passive)
            {

                if (el.Value.count > 0u)
                {

                    // Item Aqui tem o Achievemente de passive item
                    if (sIff.getInstance().getItemGroupIdentify(el.Value._typeid) == sIff.getInstance().ITEM && !sIff.getInstance().IsItemEquipable(el.Value._typeid))
                    {

                        //pgi.sys_achieve.incrementCounter(0x6C400075u, el.Value.count);

                        //if ((tmp_counter_typeid = SysAchievement.getPassiveItemCounterTypeId(el.Value._typeid)) > 0)
                        //{
                        //    pgi.sys_achieve.incrementCounter(tmp_counter_typeid, el.Value.count);
                        //}
                    }

                    // Só atualiza o Auto Caliper se não for Premium 2
                    if (!_session.m_pi.m_cap.premium_user
                        || !sPremiumSystem.getInstance().isPremium(_session.m_pi.pt._typeid)
                        || el.Value._typeid != AUTO_CALIPER_TYPEID)
                    {

                        // Tira todos itens passivo, antes estava Item e AuxPart, não ia Ball por que eu fiz errado, só preciso verifica se é item e passivo para somar o achievement
                        // Para tirar os itens, tem que tirar(atualizar) todos.
                        var pWi = _session.m_pi.findWarehouseItemByTypeid(el.Value._typeid);

                        if (pWi != null)
                        {

                            // Init Item
                            item.clear();

                            item.type = 2;
                            item._typeid = el.Value._typeid;
                            item.id = (int)pWi.id;
                            item.qntd = el.Value.count;
                            item.STDA_C_ITEM_QNTD = (ushort)((short)item.qntd * -1);

                            // Add On Vector
                            v_item.Add(item);

                        }
                        else
                        {
                            _smp.message_pool.push(new message("[Game::requestFinishItemUsedGame][WARNING] player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou atualizar item[TYPEID=" + Convert.ToString(el.Value._typeid) + "] que ele nao possui. Hacker ou Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));
                        }
                    }
                }
            }

            // Active Item
            foreach (var el in ui.v_active)
            {

                if (el.Value.count > 0u)
                {

                    // Aqui tem achievement de Item Active
                    if (sIff.getInstance().getItemGroupIdentify(el.Value._typeid) == sIff.getInstance().ITEM && sIff.getInstance().IsItemEquipable(el.Value._typeid))
                    {

                        //pgi.sys_achieve.incrementCounter(0x6C40004Fu, el.Value.count);

                        //if ((tmp_counter_typeid = SysAchievement.getActiveItemCounterTypeId(el.Value._typeid)) > 0)
                        //{
                        //    pgi.sys_achieve.incrementCounter(tmp_counter_typeid, el.Value.count);
                        //}
                    }

                    // Só tira os itens Active se a sala não estiver com o artefact Frozen Flame,
                    // se ele estiver com artefact Frozen Flame ele mantém os Itens Active, não consome e nem desequipa do inventório do player
                    if (m_ri.artefato != ART_FROZEN_FLAME)
                    {

                        // Limpa o Item Slot do player, dos itens que foram usados(Ativados) no jogo
                        if (el.Value.count <= el.Value.v_slot.Count)
                        {

                            for (var i = 0; i < el.Value.count; ++i)
                            {
                                _session.m_pi.ue.item_slot[el.Value.v_slot[i]] = 0;
                            }
                        }

                        var pWi = _session.m_pi.findWarehouseItemByTypeid(el.Value._typeid);

                        if (pWi != null)
                        {
                            // Init Item
                            item.clear();

                            item.type = 2;
                            item._typeid = el.Value._typeid;
                            item.id = (int)pWi.id;
                            item.qntd = el.Value.count;
                            item.STDA_C_ITEM_QNTD = (ushort)(item.qntd * -1);

                            // Add On Vector
                            v_item.Add(item);

                        }
                        else
                        {
                            _smp.message_pool.push(new message("[Game::requestFinishItemUsedGame][WARNING] player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou atualizar item[TYPEID=" + Convert.ToString(el.Value._typeid) + "] que ele nao possui. Hacker ou Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));
                        }
                    }
                }
            }

            // Update Item Equiped Slot ON DB
            NormalManagerDB.add(25,
                new CmdUpdateItemSlot(_session.m_pi.uid, _session.m_pi.ue.item_slot),
                SQLDBResponse, this);

            // Se for o Master da sala e ele estiver com artefato tira o mana dele
            // Antes tirava assim que começava o jogo, mas aí o cliente atualizava a sala tirando o artefact aí no final não tinha como ver se o frozen flame estava equipado
            // e as outras pessoas que estão na lobby não sabe qual artefect que está na sala, por que o master mesmo mando o pacote pra tirar da sala quando o server tira o mana dele no init game
            if (m_ri.artefato != 0 && m_ri.master == _session.m_pi.uid)
            {

                // Tira Artefact Mana do master da sala
                var pWi = _session.m_pi.findWarehouseItemByTypeid(m_ri.artefato + 1);

                if (pWi != null)
                {

                    item.clear();

                    item.type = 2;
                    item.id = (int)pWi.id;
                    item._typeid = pWi._typeid;
                    item.qntd = (uint)((pWi.STDA_C_ITEM_QNTD <= 0) ? 1 : pWi.STDA_C_ITEM_QNTD);
                    item.STDA_C_ITEM_QNTD = (ushort)(item.qntd * -1);

                    // Add on Vector Update Itens
                    v_item.Add(item);

                }
                else
                {
                    _smp.message_pool.push(new message("[Game::requestFinishItemUsedGame][WARNING] Master[UID=" + Convert.ToString(_session.m_pi.uid) + "] do jogo nao tem Mana do Artefect[TYPEID=" + Convert.ToString(m_ri.artefato) + ", MANA=" + Convert.ToString(m_ri.artefato + 1) + "] e criou e comecou um jogo com artefact sem mana. Hacker ou Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));
                }
            }

            // Update Item ON Server AND DB
            if (v_item.Count > 0)
            {

                if (item_manager.removeItem(v_item, _session) <= 0)
                {
                    _smp.message_pool.push(new message("[Game::requestFinishItemUsedGame][WARNING] player[UID=" + Convert.ToString(_session.m_pi.uid) + "] nao conseguiu deletar os item do player. Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));
                }
            }

            // Club Mastery
            if (ui.club.count > 0u && ui.club._typeid > 0u)
            {

                var pClub = _session.m_pi.findWarehouseItemByTypeid(ui.club._typeid);

                if (pClub != null)
                {

                    pClub.clubset_workshop.mastery += ui.club.count;

                    item.clear();

                    item.type = 0xCC;
                    item.id = (int)pClub.id;
                    item._typeid = pClub._typeid;

                    item.clubset_workshop.c
                        = Tools.CopyShortToUShort(pClub.clubset_workshop.c);

                    item.clubset_workshop.level = (byte)pClub.clubset_workshop.level;
                    item.clubset_workshop.mastery = pClub.clubset_workshop.mastery;
                    item.clubset_workshop.rank = pClub.clubset_workshop.rank;
                    item.clubset_workshop.recovery = pClub.clubset_workshop.recovery_pts;

                    NormalManagerDB.add(12,
                        new CmdUpdateClubSetWorkshop(_session.m_pi.uid,
                            pClub,
                            CmdUpdateClubSetWorkshop.FLAG.F_TRANSFER_MASTERY_PTS),
                        SQLDBResponse, this);

                    v_item.Add(item);

                }
                else
                {
                    _smp.message_pool.push(new message("[Game::requestFinishItemUsedGame][WARNING] player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou salvar mastery do ClubSet[TYPEID=" + Convert.ToString(ui.club._typeid) + "] que ele nao tem. Hacker ou Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));
                }
            }

            // Flag de que o palyer já finalizou os itens usados no jogo, para não finalizar duas vezes
            pgi.finish_item_used = 1;

            // Atualiza ON Jogo
            if (v_item.Count > 0)
            {
                PangyaBinaryWriter p = new PangyaBinaryWriter((ushort)0x216);

                p.WriteUInt32((uint32_t)UtilTime.GetSystemTimeAsUnix());
                p.WriteUInt32((uint32_t)v_item.Count);

                foreach (var el in v_item)
                {
                    p.WriteByte(el.type);
                    p.WriteUInt32(el._typeid);
                    p.WriteInt32(el.id);
                    p.WriteUInt32(el.flag_time);
                    p.WriteBytes(el.stat.ToArray());
                    p.WriteUInt32((el.STDA_C_ITEM_TIME > 0) ? el.STDA_C_ITEM_TIME : el.STDA_C_ITEM_QNTD);
                    p.WriteZeroByte(25); // 10 PCL[C0~C4] 2 Bytes cada, 15 bytes desconhecido
                    if (el.type == 0xCC)
                    {
                        p.WriteBytes(el.clubset_workshop.ToArray());
                    }
                }

                packet_func.session_send(p,
                    _session, 1);
            }
        }

        /// <summary>
        /// precisa corrigir@@@@
        /// </summary>
        /// <param name="_session"></param>
        /// <param name="option"></param>
        /// <exception cref="exception"></exception>
        protected virtual void requestFinishHole(Player _session, int option)
        {

            var pgi = getPlayerInfo((_session));
            if (pgi == null)
            {
                throw new exception("[Game::" + "requestFinishHole" + "][Error] player[UID=" + Convert.ToString((_session).m_pi.uid) + "] " + "tentou finalizar o dados do hole do player no jogo" + ", mas o game nao tem o info dele guardado. Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.GAME,
                    1, 4));
            }

            if (pgi.hole == 255)
                return;

            var hole = m_course.findHole(pgi.hole);

            if (hole == null)
            {
                throw new exception("[Game::finishHole][Error] player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou finalizar hole[NUMERO=" + Convert.ToString((ushort)pgi.hole) + "] no jogo, mas o numero do hole is invalid. Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.GAME,
                    20, 0));
            }

            sbyte score_hole = 0;//negativo
            uint tacada_hole = 0;//negativo

            // Finish Hole Dados
            if (option == 0) // melhorar depois@@@@@
            {

                pgi.data.total_tacada_num += pgi.data.tacada_num;

                // Score do hole
                score_hole = Convert.ToSByte(pgi.data.tacada_num - hole.getPar().par);

                // Tacadas do hole
                tacada_hole = pgi.data.tacada_num;
                pgi.data.score = 0;
                pgi.data.score += score_hole;  // SEM alterar o sinal, conforme original

                //// Achievement Score
                //var tmp_counter_typeid = SysAchievement.getScoreCounterTypeId(tacada_hole, hole.getPar().par);

                //if (tmp_counter_typeid > 0)
                //{
                //    pgi.sys_achieve.incrementCounter(tmp_counter_typeid);
                //}


                _smp.message_pool.push(new message("[Game::requestFinishHole][Log] player[UID=" + Convert.ToString(_session.m_pi.uid) + "] terminou o hole[COURSE=" + Convert.ToString(hole.getCourse()) + ", NUMERO=" + Convert.ToString(hole.getNumero()) + ", PAR=" + Convert.ToString(hole.getPar().par) + ", SHOT=" + Convert.ToString(tacada_hole) + ", SCORE=" + Convert.ToString(score_hole) + ", TOTAL_SHOT=" + Convert.ToString(pgi.data.total_tacada_num) + ", TOTAL_SCORE=" + Convert.ToString(pgi.data.score) + "]", type_msg.CL_FILE_LOG_AND_CONSOLE));



                // Zera dados
                pgi.data.time_out = 0;

                // Giveup Flag
                pgi.data.giveup = 0;

                // Zera as penalidades do hole
                pgi.data.penalidade = 0;

            }
            else if (option == 1)
            { // Não acabou o hole então faz os calculos para o jogo todo

                var range = m_course.findRange(pgi.hole);  // findRange deve retornar IEnumerable<KeyValuePair<int, Tipo>>

                foreach (var kv in range)
                {
                    if (kv.Key > m_ri.qntd_hole)
                    {
                        break;  // Igual à condição it->first <= m_ri.qntd_hole
                    }

                    pgi.data.total_tacada_num += kv.Value.getPar().total_shot;
                    pgi.data.score += kv.Value.getPar().range_score[1];  // Max Score
                }


                // Zera dados
                pgi.data.time_out = 0;

                pgi.data.tacada_num = 0;

                // Giveup Flag
                pgi.data.giveup = 0;

                // Zera as penalidades do hole do player
                pgi.data.penalidade = 0;
            }

            // Aqui tem que atualiza o PGI direitinho com outros dados
            pgi.progress.hole = (short)m_course.findHoleSeq(pgi.hole);

            // Dados Game Progress do Player
            if (option == 0)
            {

                if (pgi.progress.hole > 0)
                {

                    if (pgi.shot_sync.state_shot.display.acerto_hole)
                    {
                        pgi.progress.finish_hole[pgi.progress.hole - 1] = 1; // Terminou o hole
                    }

                    pgi.progress.par_hole[pgi.progress.hole - 1] = hole.getPar().par;
                    pgi.progress.score[pgi.progress.hole - 1] = score_hole;
                    pgi.progress.tacada[pgi.progress.hole - 1] = tacada_hole;
                }

            }
            else
            {

                var range = m_course.findRange(pgi.hole);

                foreach (var kv in range)
                {
                    int index = kv.Key - 1;

                    pgi.progress.finish_hole[index] = 0; // não terminou

                    pgi.progress.par_hole[index] = kv.Value.getPar().par;

                    pgi.progress.score[index] = (sbyte)kv.Value.getPar().range_score[1]; // Max Score

                    pgi.progress.tacada[index] = kv.Value.getPar().total_shot;
                }

            }
        }

        protected virtual void requestSaveRecordCourse(Player _session,
            int game, int option)
        {

            var pgi = getPlayerInfo((_session));
            if (pgi == null)
            {
                throw new exception("[Game::" + "requestSaveRecordCourse" + "][Error] player[UID=" + Convert.ToString((_session).m_pi.uid) + "] " + "tentou salvar record do course do player no jogo" + ", mas o game nao tem o info dele guardado. Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.GAME,
                    1, 4));
            }

            if (_session.m_pi.ei.char_info == null)
            { // Player não está com character equipado, kika dele do jogo
                _smp.message_pool.push(new message("[Game::requestSaveRecordCourse][Log] player[UID=" + Convert.ToString(_session.m_pi.uid) + "] nao esta com Character equipado. kika ele do jogo. pode ser Bug.", type_msg.CL_FILE_LOG_AND_CONSOLE));
                return; // Kika aqui "deletePlayer(s);"
            }

            MapStatistics pMs = null;

            if (pgi.assist_flag == 1)
            { // Assist

                if (game == 52)
                {
                    pMs = _session.m_pi.a_msa_grand_prix[(int)((int)m_ri.course & 0x7F)];
                }
                else if (m_ri.natural.natural == 1)
                { // Natural
                    pMs = _session.m_pi.a_msa_natural[(int)((int)m_ri.course & 0x7F)];

                    game = 51; // Natural
                }
                else
                { // Normal
                    pMs = _session.m_pi.a_msa_normal[(int)((int)m_ri.course & 0x7F)];
                }

            }
            else
            { // Sem Assist

                if (game == 52)
                {
                    pMs = _session.m_pi.a_ms_grand_prix[(int)((int)m_ri.course & 0x7F)];
                }
                else if (m_ri.natural.natural == 1)
                { // Natural
                    pMs = _session.m_pi.a_ms_natural[(int)((int)m_ri.course & 0x7F)];

                    game = 51; // Natural
                }
                else
                { // Normal
                    pMs = _session.m_pi.a_ms_normal[(int)((int)m_ri.course & 0x7F)];
                }
            }

            bool make_record = false;

            // UPDATE ON SERVER
            if (option == 1)
            { // 18h pode contar record

                // Fez Record
                if (pMs.best_score == 127
                    || pgi.data.score < (int)pMs.best_score
                    || pgi.data.pang > pMs.best_pang)
                {

                    // Update Best Score Record
                    if (pgi.data.score < pMs.best_score)
                    {
                        pMs.best_score = (sbyte)pgi.data.score;
                    }

                    // Update Best Pang Record
                    if (pgi.data.pang > pMs.best_pang)
                    {
                        pMs.best_pang = pgi.data.pang;
                    }

                    // Update Character Record
                    pMs.character_typeid = _session.m_pi.ei.char_info._typeid;

                    make_record = true;
                }
            }

            // Salva os dados normais
            pMs.tacada += (uint)pgi.ui.tacada;
            pMs.putt += (uint)pgi.ui.putt;
            pMs.hole += (uint)pgi.ui.hole;
            pMs.fairway += (uint)pgi.ui.fairway;
            pMs.hole_in += (uint)pgi.ui.hole_in;
            pMs.putt_in += (uint)pgi.ui.putt_in;
            pMs.total_score += pgi.data.score;
            pMs.event_score = 0;

            MapStatisticsEx ms = new MapStatisticsEx(pMs);

            ms.tipo = (byte)game;

            // UPDATE ON DB
            NormalManagerDB.add(5,
                new CmdUpdateMapStatistics(_session.m_pi.uid,
                    ms, pgi.assist_flag),
                SQLDBResponse, this);

            // UPDATE ON GAME, se ele fez record, e add 1000 para ele
            if (make_record)
            {

                // Log
                _smp.message_pool.push(new message("[Game::requestSaveRecordCourse][Log] player[UID=" + Convert.ToString(_session.m_pi.uid) + "] fez record no Map[COURSE=" + Convert.ToString((ushort)(int)((int)m_ri.course & 0x7F)) + " (" + Convert.ToString((ushort)pMs.course) + "), SCORE=" + Convert.ToString((short)pMs.best_score) + ", PANG=" + Convert.ToString(pMs.best_pang) + ", CHARACTER=" + Convert.ToString(pMs.character_typeid) + "]", type_msg.CL_FILE_LOG_AND_CONSOLE));

                // Add 1000 pang por ele ter quebrado o  record dele
                _session.m_pi.addPang(1000);

                // Resposta para make record
                PangyaBinaryWriter p = new PangyaBinaryWriter((ushort)0xB9);

                p.WriteByte(((int)m_ri.course) & 0x7F);

                packet_func.session_send(p,
                    _session, 1);
            }
        }

        protected virtual void requestInitItemUsedGame(Player _session, PlayerGameInfo _pgi)
        {

            //INIT_PLAYER_INFO("requestInitItemUsedGame", "tentou inicializar itens usado no jogo", _session, out PlayerGameInfo pgi);

            // Characters Equip
            if (_session.getState() && _session.isConnected())
            { // Check Player Connected

                if (_session.m_pi.ei.char_info == null)
                { // Player não está com character equipado, kika dele do jogo
                    _smp.message_pool.push(new message("[Game::requestInitItemUsedGame][Log] player[UID=" + Convert.ToString(_session.m_pi.uid) + "] nao esta com Character equipado. kika ele do jogo. pode ser Bug.", type_msg.CL_FILE_LOG_AND_CONSOLE));
                    return; // Kika aqui "deletePlayer(s);"
                }

                if (_session.m_pi.ei.comet == null)
                { // Player não está com Comet(Ball) equipado, kika dele do jogo
                    _smp.message_pool.push(new message("[Game::requestInitItemUsedGame][Log] player[UID=" + Convert.ToString(_session.m_pi.uid) + "] nao esta com Ball equipado. kika ele do jogo. pode ser Bug.", type_msg.CL_FILE_LOG_AND_CONSOLE));
                    return; // Kika aqui "deletePlayer(s);"
                }

                var ui = _pgi.used_item;

                // Zera os Itens usados
                ui.clear();

                /// ********** Itens Usado **********

                // Passive Item Equipado
                _session.m_pi.mp_wi.ToList().ForEach(_el =>
                {
                    if (passive_item.Any(c => c == _el.Value._typeid))
                    {
                        ui.v_passive.insert(Tuple.Create((uint32_t)_el.Value._typeid, new UsedItem.Passive(_el.Value._typeid, 0u)));
                    }
                });
                // Ball Equiped 
                if (_session.m_pi.ei.comet._typeid != DEFAULT_COMET_TYPEID && (!_session.m_pi.m_cap.premium_user || _session.m_pi.ei.comet._typeid != sPremiumSystem.getInstance().getPremiumBallByTicket(_session.m_pi.pt._typeid)))
                {
                    ui.v_passive.insert(Tuple.Create((uint32_t)_session.m_pi.ei.comet._typeid, new UsedItem.Passive(_session.m_pi.ei.comet._typeid, 0u)));
                }

                // AuxParts
                for (var i = 0; i < (_session.m_pi.ei.char_info.auxparts.Length); ++i)
                {
                    if (_session.m_pi.ei.char_info.auxparts[i] >= 0x70000000 && _session.m_pi.ei.char_info.auxparts[i] < 0x70010000)
                    {
                        ui.v_passive.insert(Tuple.Create((uint32_t)_session.m_pi.ei.char_info.auxparts[i], new UsedItem.Passive(_session.m_pi.ei.char_info.auxparts[i], 0u)));
                    }
                }

                // Item Active Slot 
                for (var i = 0; i < (_session.m_pi.ue.item_slot.Length); ++i)
                {

                    // Diferente de 0 item está equipado
                    if (_session.m_pi.ue.item_slot[i] != 0)
                    {
                        if (!ui.v_active.ContainsKey(_session.m_pi.ue.item_slot[i])) // Não tem add o novo
                        {
                            ui.v_active.insert(Tuple.Create((uint32_t)_session.m_pi.ue.item_slot[i], new UsedItem.Active(_session.m_pi.ue.item_slot[i], 0u, new List<byte> { (byte)i })));
                        }

                        else // Já tem add só o slot
                        {
                            ui.v_active[(uint32_t)_session.m_pi.ue.item_slot[i]].v_slot.Add((byte)i); // Slot
                        }
                    }
                }

                // ClubSet For ClubMastery
                ui.club._typeid = _session.m_pi.ei.csi._typeid;
                ui.club.count = 0;
                ui.club.rate = 1.0f;

                var club = sIff.getInstance().findClubSet(ui.club._typeid);

                if (club != null)
                {
                    ui.club.rate = club.work_shop.rate;
                }
                else
                {
                    _smp.message_pool.push(new message("[Game::requestIniItemUsedGame][WARNING] player[UID=" + Convert.ToString(_session.m_pi.uid) + "] esta equipado com um ClubSet[TYPEID=" + Convert.ToString(_session.m_pi.ei.csi._typeid) + ", ID=" + Convert.ToString(_session.m_pi.ei.csi.id) + "] que nao tem no IFF_STRUCT do Server. Hacker ou Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));
                }

                /// ********** Itens Usado **********

                /// ********** Itens Exp/Pang Rate **********
                // Item Buff
                var time_limit_item = sIff.getInstance().getTimeLimitItem();

                _session.m_pi.v_ib.ForEach(_el =>
                {
                    var item_ = time_limit_item.FirstOrDefault(_el2 => _el2.Value._typeid == _el._typeid);

                    if (item_.Value != null)
                    {
                        switch ((ItemBuff.eTYPE)item_.Value.type)
                        {
                            case ItemBuff.eTYPE.YAM_AND_GOLD:
                                ui.rate.exp += item_.Value.percent;
                                break;

                            case ItemBuff.eTYPE.RAINBOW:
                            case ItemBuff.eTYPE.RED:
                                ui.rate.exp += (item_.Value.percent > 0) ? item_.Value.percent : 100;
                                ui.rate.pang += (item_.Value.percent > 0) ? item_.Value.percent : 100;
                                break;

                            case ItemBuff.eTYPE.GREEN:
                                ui.rate.exp += (item_.Value.percent > 0) ? item_.Value.percent : 100;
                                break;

                            case ItemBuff.eTYPE.YELLOW:
                                ui.rate.pang += (item_.Value.percent > 0) ? item_.Value.percent : 100;
                                break;
                        }
                    }
                });

                // Card Equipado, Special, NPC, e Caddie
                _session.m_pi.v_cei.ToList().ForEach(_el =>
{
    if (_el.parts_id == _session.m_pi.ei.char_info.id
        && _el.parts_typeid == _session.m_pi.ei.char_info._typeid
        && sIff.getInstance().getItemSubGroupIdentify22(_el._typeid) == 5)
    {
        if (_el.efeito == 2)
        {
            ui.rate.exp += _el.efeito_qntd;
        }
        else if (_el.efeito == 1)
        {
            ui.rate.pang += _el.efeito_qntd;
        }
    }
    else if (_el.parts_id == 0 && _el.parts_typeid == 0 && sIff.getInstance().getItemSubGroupIdentify22(_el._typeid) == 2)
    {
        if (_el.efeito == 3)
        {
            ui.rate.exp += _el.efeito_qntd;
        }
        else if (_el.efeito == 2)
        {
            ui.rate.pang += _el.efeito_qntd;
        }
        else if (_el.efeito == 34)
        {
            ui.rate.club += _el.efeito_qntd;
        }
    }
});

                // Item Passive Boost Exp, Pang and Club Mastery

                // Pang
                ui.v_passive.ToList().ForEach(_el =>
                {
                    if (Array.IndexOf(passive_item_pang_x2, _el.Value._typeid) != passive_item_pang_x2.Length - 1)
                    {
                        ui.rate.pang += 200;
                        _pgi.boost_item_flag.pang = 1;
                    }

                    if (Array.IndexOf(passive_item_pang_x4, _el.Value._typeid) != passive_item_pang_x4.Length - 1)
                    {
                        ui.rate.pang += 400;
                        _pgi.boost_item_flag.pang_nitro = 1;
                    }

                    if (Array.IndexOf(passive_item_pang_x1_5, _el.Value._typeid) != passive_item_pang_x1_5.Length - 1)
                    {
                        ui.rate.pang += 50;
                        _pgi.boost_item_flag.pang = 1;
                    }

                    if (Array.IndexOf(passive_item_pang_x1_4, _el.Value._typeid) != passive_item_pang_x1_4.Length - 1)
                    {
                        ui.rate.pang += 40;
                        _pgi.boost_item_flag.pang = 1;
                    }

                    if (Array.IndexOf(passive_item_pang_x1_2, _el.Value._typeid) != passive_item_pang_x1_2.Length - 1)
                    {
                        ui.rate.pang += 20;
                        _pgi.boost_item_flag.pang = 1;
                    }
                });


                // Exp
                ui.v_passive.ToList().ForEach(_el =>
                {
                    if (Array.IndexOf(passive_item_exp, _el.Value._typeid) != -1)
                    {
                        ui.rate.exp += 200;
                    }
                });

                // Club Mastery Boost
                ui.v_passive.ToList().ForEach(_el =>
                {
                    if (Array.IndexOf(passive_item_club_boost, _el.Value._typeid) != -1)
                    {
                        ui.rate.club += 200;
                    }
                });

                // Character Parts Equipado
                if (_session.m_pi.ei.char_info.parts_typeid.Any(_element =>
                    Array.IndexOf(hat_birthday, _element) != -1))
                {

                    _smp.message_pool.push(new message("[Game::requestInitItemUsedGame][Log] player[UID=" + Convert.ToString(_session.m_pi.uid) + "] esta equipado com Hat Birthday no Character[TYPEID=" + Convert.ToString(_session.m_pi.ei.char_info._typeid) + ", ID=" + Convert.ToString(_session.m_pi.ei.char_info.id) + "]", type_msg.CL_FILE_LOG_AND_CONSOLE));


                    ui.rate.exp += 20; // 20% Hat Birthday
                }

                // Hat Lua e Sol
                if (_session.m_pi.ei.char_info.parts_typeid.Any(_element =>
                    Array.IndexOf(hat_lua_sol, _element) != -1))
                {

                    _smp.message_pool.push(new message("[Game::requestInitItemUsedGame][Log] player[UID=" + Convert.ToString(_session.m_pi.uid) + "] esta equipado com Hat Lua e Sol no Character[TYPEID=" + Convert.ToString(_session.m_pi.ei.char_info._typeid) + ", ID=" + Convert.ToString(_session.m_pi.ei.char_info.id) + "]", type_msg.CL_FILE_LOG_AND_CONSOLE));


                    ui.rate.exp += 20;  // 20% Hat Lua e Sol
                    ui.rate.pang += 20; // 20% Hat Lua e Sol
                }

                // Kurafaito Ring Club Mastery
                if (Array.IndexOf(_session.m_pi.ei.char_info.auxparts, KURAFAITO_RING_CLUBMASTERY) != -1)
                {

                    _smp.message_pool.push(new message("[Game::requestInitItemUsedGame][Log] player[UID=" + Convert.ToString(_session.m_pi.uid) + "] esta equipado com Anel (Kurafaito) que da Club Mastery +1.1% no Character[TYPEID=" + Convert.ToString(_session.m_pi.ei.char_info._typeid) + ", ID=" + Convert.ToString(_session.m_pi.ei.char_info.id) + "]", type_msg.CL_FILE_LOG_AND_CONSOLE));


                    ui.rate.club += 10; // Kurafaito Ring da + 10% no Club Mastery
                }

                // Character AuxParts Equipado
                // Aux parts tem seus próprios valores de rate no iff
                foreach (var _el in _session.m_pi.ei.char_info.auxparts)
                {
                    if (_el != 0 && sIff.getInstance().getItemGroupIdentify(_el) == sIff.getInstance().AUX_PART)
                    {
                        var auxpart = sIff.getInstance().findAuxPart(_el);
                        if (auxpart != null)
                        {
                            if (auxpart.Pang_Rate > 100)
                            {
                                ui.rate.pang += (uint)(auxpart.Pang_Rate - 100);
                            }
                            else if (auxpart.Pang_Rate > 0)
                            {
                                ui.rate.pang += auxpart.Pang_Rate;
                            }

                            if (auxpart.Exp_Rate > 100)
                            {
                                ui.rate.exp += (uint)(auxpart.Exp_Rate - 100);
                            }
                            else if (auxpart.Exp_Rate > 0)
                            {
                                ui.rate.exp += auxpart.Exp_Rate;
                            }

                            if (auxpart.Drop_Rate > 100)
                            {
                                ui.rate.drop += (uint)(auxpart.Drop_Rate - 100);
                            }
                            else if (auxpart.Drop_Rate > 0)
                            {
                                ui.rate.drop += auxpart.Drop_Rate;
                            }

                            _pgi.thi.all_score += 15;
                        }
                    }
                }

                // Mascot Equipado Rate Exp And Pang, Drop item e Treasure Hunter rate
                if (_session.m_pi.ei.mascot_info != null && _session.m_pi.ei.mascot_info._typeid > 0)
                {

                    var mascot = sIff.getInstance().findMascot(_session.m_pi.ei.mascot_info._typeid);

                    if (mascot != null)
                    {

                        // Pang
                        if (mascot.efeito.pang_rate > 100)
                        {
                            ui.rate.pang += (uint)(mascot.efeito.pang_rate - 100);
                        }
                        else if (mascot.efeito.pang_rate > 0)
                        {
                            ui.rate.pang += (uint)mascot.efeito.pang_rate;
                        }

                        // Exp
                        if (mascot.efeito.exp_rate > 100)
                        {
                            ui.rate.exp += (uint)(mascot.efeito.exp_rate - 100);
                        }
                        else if (mascot.efeito.exp_rate > 0)
                        {
                            ui.rate.exp += (uint)mascot.efeito.exp_rate;
                        }

                        // Drop item, aqui ele add os 120% e no Drop System ele trata isso direito
                        // Todos itens que dá drop rate da treasure hunter point
                        if (mascot.efeito.drop_rate > 100)
                        {

                            if (mascot.efeito.drop_rate > 100)
                            {
                                ui.rate.drop += (uint)(mascot.efeito.drop_rate - 100);
                            }
                            else if (mascot.efeito.drop_rate > 0)
                            {
                                ui.rate.drop += (uint)mascot.efeito.drop_rate;
                            }

                            // Passaro gordo que usa isso aqui, mas pode adicionar mais mascot que dé drop rate e treasure hunter point
                            _pgi.thi.all_score += 15; // Add +15 ao all score
                        }
                    }
                    else
                    {
                        _smp.message_pool.push(new message("[Game::requestInitItemUsedGame][WARNING] player[UID=" + Convert.ToString(_session.m_pi.uid) + "] esta equipado com um mascot[TYPEID=" + Convert.ToString(_session.m_pi.ei.mascot_info._typeid) + ", ID=" + Convert.ToString(_session.m_pi.ei.mascot_info.id) + "] que nao tem no IFF_STRUCT do Server. Hacker ou Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));
                    }
                }

                /// ********** Premium User +10% EXP and PANG *********************

                if (_pgi.premium_flag == 1)
                {
                    var rate_premium = sPremiumSystem.getInstance().getExpPangRateByTicket(_session.m_pi.pt._typeid);
                    ui.rate.exp += rate_premium;
                    ui.rate.pang += rate_premium;
                }

                /// ********** Itens Exp/Pang Rate **********
            }
        }

        protected virtual void requestSendTreasureHunterItem(Player _session)
        {

            var pgi = getPlayerInfo((_session));
            if (pgi == null)
            {
                throw new exception("[Game::" + "requestSendTreasureHunterItem" + "][Error] player[UID=" + Convert.ToString((_session).m_pi.uid) + "] " + "tentou enviar os itens ganho no Treasure Hunter do jogo" + ", mas o game nao tem o info dele guardado. Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.GAME,
                    1, 4));
            }

            List<stItem> v_item = new List<stItem>();
            stItem item = new stItem();
            BuyItem bi = new BuyItem();

            if (!pgi.thi.v_item.empty())
            {

                foreach (var el in pgi.thi.v_item)
                {

                    bi = new BuyItem();
                    item.clear();

                    bi.id = -1;
                    bi._typeid = el._typeid;
                    bi.qntd = el.qntd;

                    item_manager.initItemFromBuyItem(_session.m_pi,
                        item, bi, false, 0, 0, 1);

                    if (item._typeid == 0)
                    {
                        _smp.message_pool.push(new message("[Game::requestSendTreasureHunterItem][Error] player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou inicializar item[TYPEID=" + Convert.ToString(bi._typeid) + "], mas nao consgeuiu. Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));

                        continue;
                    }

                    v_item.Add(item);
                }

                // Add Item, se tiver Item
                if (v_item.Count > 0)
                {

                    var rai = item_manager.addItem(v_item,
                        _session, 0, 0);

                    if (rai.fails.Count > 0 && rai.type != item_manager.RetAddItem.TYPE.T_SUCCESS_PANG_AND_EXP_AND_CP_POUCH)
                    {
                        _smp.message_pool.push(new message("[Game::requestSendTreasureHunterItem][Error] player[UID=" + Convert.ToString(_session.m_pi.uid) + "] nao conseguiu adicionar os itens que ele ganhou no Treasure Hunter. Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));
                    }
                }
            }

            // UPDATE ON GAME
            PangyaBinaryWriter p = new PangyaBinaryWriter((ushort)0x134);

            p.WriteByte((byte)v_item.Count);

            foreach (var el in v_item)
            {
                p.WriteUInt32(_session.m_pi.uid);

                p.WriteUInt32(el._typeid);
                p.WriteInt32(el.id);
                p.WriteUInt32(el.qntd);
                p.WriteByte(0); // Opt Acho, mas nunca vi diferente de 0

                p.WriteUInt16((ushort)(el.stat.qntd_dep / 0x8000));
                p.WriteUInt16((ushort)(el.stat.qntd_dep % 0x8000));
            }

            packet_func.session_send(p,
                _session, 1);
        }

        protected virtual byte checkCharMotionItem(Player _session)
        {

            // Characters Equip
            if (_session.getState() && _session.isConnected())
            { // Check Player Connected

                if (_session.m_pi.ei.char_info == null)
                { // Player não está com character equipado, kika dele do jogo
                    _smp.message_pool.push(new message("[Game::checkCharMotionItem][Log] player[UID=" + Convert.ToString(_session.m_pi.uid) + "] nao esta com Character equipado. kika ele do jogo. pode ser Bug.", type_msg.CL_FILE_LOG_AND_CONSOLE));
                    // Kika aqui "deletePlayer(s);"

                    return 0;
                }

                // Motion Item
                if (_session.m_pi.ei.char_info.parts_typeid.Any(_element =>
    motion_item.Contains(_element)))
                {

                    _smp.message_pool.push(new message(
                        "[Game::checkCharMotionItem][Log] player[UID=" + Convert.ToString(_session.m_pi.uid) +
                        "] esta equipado com Motion Item no Character[TYPEID=" + Convert.ToString(_session.m_pi.ei.char_info._typeid) +
                        ", ID=" + Convert.ToString(_session.m_pi.ei.char_info.id) + "]",
                        type_msg.CL_FILE_LOG_AND_CONSOLE));

                    return 1;
                }

            }

            return 0;
        }

        // Atualiza o Info do usuario, Info Trofel e Map Statistics do Course
        // Opt 0 Envia tudo, -1 não envia o map statistics
        protected virtual void sendUpdateInfoAndMapStatistics(Player _session, int _option)
        {

            PangyaBinaryWriter p = new PangyaBinaryWriter((ushort)0x45);

            p.WriteBytes(_session.m_pi.ui.ToArray());

            p.WriteBytes(_session.m_pi.ti_current_season.ToArray());

            // Ainda tenho que ajeitar esses Map Statistics no Pacote Principal, No Banco de dados e no player_info class
            if (_option == -1)
            {

                // -1 12 Bytes, os 2 tipos de dados do Map Statistics
                p.WriteInt64(-1);
                p.WriteInt32(-1);

            }
            else
            {
                // Normal essa season
                if (_session.m_pi.a_ms_normal[(int)((int)m_ri.course & 0x7F)].course != ((int)m_ri.course & 0x7F))
                {
                    p.WriteSByte(-1); // Não tem
                }
                else
                {
                    p.WriteByte((char)m_ri.course & 0x7F);
                    p.WriteBytes(_session.m_pi.a_ms_normal[(int)((int)m_ri.course & 0x7F)].ToArray());
                }

                // Normal rest season
                // tem que fazer o map statistics soma de todas season
                //p.WriteByte((char)m_ri.course & 0x7F);
                //p.WriteBuffer(_session, out PlayerGameInfo pgi.m_pi.aa_ms_normal_todas_season[0][(int)((int)m_ri.course & 0x7F)],Marshal.SizeOf(new MapStatistics()));
                p.WriteSByte(-1); // Não tem

                // Natural essa season
                if (_session.m_pi.a_ms_natural[(int)((int)m_ri.course & 0x7F)].course != ((int)m_ri.course & 0x7F))
                {
                    p.WriteSByte(-1); // N�o tem
                }
                else
                {
                    p.WriteByte((char)m_ri.course & 0x7F);
                    p.WriteBytes(_session.m_pi.a_ms_natural[(int)((int)m_ri.course & 0x7F)].ToArray());
                }

                // Natural rest season
                // tem que fazer o map statistics soma de todas season
                //p.WriteByte((char)m_ri.course & 0x7F);
                //p.WriteBuffer(_session, out PlayerGameInfo pgi.m_pi.aa_ms_normal_todas_season[0][(int)((int)m_ri.course & 0x7F)],Marshal.SizeOf(new MapStatistics()));
                p.WriteSByte(-1); // Não tem

                // Normal Assist essa season
                if (_session.m_pi.a_msa_normal[(int)((int)m_ri.course & 0x7F)].course != ((int)m_ri.course & 0x7F))
                {
                    p.WriteSByte(-1); // Não tem
                }
                else
                {
                    p.WriteByte((char)m_ri.course & 0x7F);
                    p.WriteBytes(_session.m_pi.a_msa_normal[(int)((int)m_ri.course & 0x7F)].ToArray());
                }

                // Normal Assist rest season
                // tem que fazer o map statistics soma de todas season
                //p.WriteByte((char)m_ri.course & 0x7F);
                //p.WriteBuffer(_session, out PlayerGameInfo pgi.m_pi.aa_ms_normal_todas_season[0][(int)((int)m_ri.course & 0x7F)],Marshal.SizeOf(new MapStatistics()));
                p.WriteSByte(-1); // Não tem

                // Natural Assist essa season
                if (_session.m_pi.a_msa_natural[(int)((int)m_ri.course & 0x7F)].course != ((int)m_ri.course & 0x7F))
                {
                    p.WriteSByte(-1); // Não tem
                }
                else
                {
                    p.WriteByte((char)m_ri.course & 0x7F);
                    p.WriteBytes(_session.m_pi.a_msa_natural[(int)((int)m_ri.course & 0x7F)].ToArray());
                }

                // Natural Assist rest season
                // tem que fazer o map statistics soma de todas season
                //p.WriteByte((char)m_ri.course & 0x7F);
                //p.WriteBuffer(_session, out PlayerGameInfo pgi.m_pi.aa_ms_normal_todas_season[0][(int)((int)m_ri.course & 0x7F)],Marshal.SizeOf(new MapStatistics()));
                p.WriteSByte(-1); // Não tem

                // Grand Prix essa season
                if (_session.m_pi.a_ms_grand_prix[(int)((int)m_ri.course & 0x7F)].course != ((int)m_ri.course & 0x7F))
                {
                    p.WriteSByte(-1); // Não tem
                }
                else
                {
                    p.WriteByte((char)m_ri.course & 0x7F);
                    p.WriteBytes(_session.m_pi.a_ms_grand_prix[(int)((int)m_ri.course & 0x7F)].ToArray());
                }

                // Grand Prix rest season
                // tem que fazer o map statistics soma de todas season
                //p.WriteByte((char)m_ri.course & 0x7F);
                //p.WriteBuffer(_session, out PlayerGameInfo pgi.m_pi.aa_ms_normal_todas_season[0][(int)((int)m_ri.course & 0x7F)],Marshal.SizeOf(new MapStatistics()));
                p.WriteSByte(-1); // Não tem

                // Grand Prix Assist essa season
                if (_session.m_pi.a_msa_grand_prix[(int)((int)m_ri.course & 0x7F)].course != ((int)m_ri.course & 0x7F))
                {
                    p.WriteSByte(-1); // Não tem
                }
                else
                {
                    p.WriteByte((char)m_ri.course & 0x7F);
                    p.WriteBytes(_session.m_pi.a_msa_grand_prix[(int)((int)m_ri.course & 0x7F)].ToArray());
                }

                // Grand Prix Assist rest season
                // tem que fazer o map statistics soma de todas season
                //p.WriteByte((char)m_ri.course & 0x7F);
                //p.WriteBuffer(_session, out PlayerGameInfo pgi.m_pi.aa_ms_normal_todas_season[0][(int)((int)m_ri.course & 0x7F)],Marshal.SizeOf(new MapStatistics()));
                p.WriteSByte(-1); // Não tem
            }

            packet_func.session_send(p,
                _session, 1);
        }

        // Envia a message no char para todos player do Game que o player terminou o jogo
        protected virtual void sendFinishMessage(Player _session)
        {

            var pgi = getPlayerInfo((_session));
            if (pgi == null)
            {
                throw new exception("[Game::" + "sendFinishMessage" + "][Error] player[UID=" + Convert.ToString((_session).m_pi.uid) + "] " + "tentou enviar message no chat que o player terminou o jogo" + ", mas o game nao tem o info dele guardado. Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.GAME,
                    1, 4));
            }

            PangyaBinaryWriter p = new PangyaBinaryWriter((ushort)0x40);

            p.WriteByte(16); // Msg que terminou o game

            p.WriteString(_session.m_pi.nickname);
            p.WriteUInt16(0); // Size Msg

            p.WriteInt32(pgi.data.score);
            p.WriteUInt64(pgi.data.pang);
            p.WriteByte(pgi.assist_flag);

            packet_func.game_broadcast(this,
                p.GetBytes, 1);
        }
        protected virtual void requestCalculeRankPlace()
        {
            if (m_player_order.Count > 0)
            {
                m_player_order.Clear();
            }

            foreach (var el in m_player_info)
            {
                if (el.Value.flag != PlayerGameInfo.eFLAG_GAME.QUIT) // menos os que quitaram
                {
                    m_player_order.Add(el.Value);
                }
            }

            m_player_order.Sort(sort_player_rank);
        }

        public int sort_player_rank(PlayerGameInfo _pgi1, PlayerGameInfo _pgi2)
        {
            if (_pgi1.data.score == _pgi2.data.score)
                return _pgi2.data.pang.CompareTo(_pgi1.data.pang); // ordem decrescente de pang

            return _pgi1.data.score.CompareTo(_pgi2.data.score); // ordem crescente de score
        }

        // Set Flag Game and finish_game flag
        protected virtual void setGameFlag(PlayerGameInfo _pgi, PlayerGameInfo.eFLAG_GAME _fg)
        {

            if (_pgi == null)
            {

                _smp.message_pool.push(new message("[Game::setGameFlag][Error] PlayerGameInfo* _pgi is invalid(null).", type_msg.CL_FILE_LOG_AND_CONSOLE));

                return;
            }

            Monitor.Enter(m_cs_sync_finish_game);
            _pgi.flag = _fg;

            Monitor.Exit(m_cs_sync_finish_game);
        }

        protected virtual void setFinishGameFlag(PlayerGameInfo _pgi, byte _finish_game)
        {

            if (_pgi == null)
            {

                _smp.message_pool.push(new message("[Game::setFinishGameFlag][Error] PlayerGameInfo* _pgi is invlaid(null).", type_msg.CL_FILE_LOG_AND_CONSOLE));

                return;
            }

            Monitor.Enter(m_cs_sync_finish_game);

            _pgi.finish_game = _finish_game;

            Monitor.Exit(m_cs_sync_finish_game);
        }

        // Check And Clear
        protected virtual bool AllCompleteGameAndClear()
        {

            uint32_t count = 0;
            bool ret = false;

            Monitor.Enter(m_cs_sync_finish_game);

            // Da error Aqui
            foreach (var el in m_players)
            {

                try
                {

                    var pgi = getPlayerInfo((el));
                    if (pgi == null)
                    {
                        throw new exception("[Game::" + "PlayersCompleteGameAndClear" + "][Error] player[UID=" + Convert.ToString((el).m_pi.uid) + "] " + "tentou verificar se o player terminou o jogo" + ", mas o game nao tem o info dele guardado. Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.GAME,
                            1, 4));
                    }

                    if (pgi.flag != PlayerGameInfo.eFLAG_GAME.PLAYING)
                    {
                        count++;
                    }

                }
                catch (exception e)
                {

                    _smp.message_pool.push(new message("[Game::AllCompleteGameAndClear][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
                }
            }

            ret = (count == m_players.Count);

            Monitor.Exit(m_cs_sync_finish_game);

            return ret;
        }

        protected virtual bool PlayersCompleteGameAndClear()
        {

            uint32_t count = 0;
            bool ret = false;

            Monitor.Enter(m_cs_sync_finish_game);

            // Da error Aqui
            foreach (var el in m_players)
            {

                try
                {

                    var pgi = getPlayerInfo((el));
                    if (pgi == null)
                    {
                        throw new exception("[Game::" + "PlayersCompleteGameAndClear" + "][Error] player[UID=" + Convert.ToString((el).m_pi.uid) + "] " + "tentou verificar se o player terminou o jogo" + ", mas o game nao tem o info dele guardado. Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.GAME,
                            1, 4));
                    }

                    if (pgi.finish_game == 1)
                    {
                        count++;
                    }

                }
                catch (exception e)
                {

                    _smp.message_pool.push(new message("[GamePlayersCompleteGameAndClear][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
                }
            }

            ret = (count == m_players.Count);

            Monitor.Exit(m_cs_sync_finish_game);

            return ret;
        }

        // Verifica se é o ultimo hole feito
        protected virtual bool checkEndGame(Player _session)
        {

            var pgi = getPlayerInfo((_session));
            if (pgi == null)
            {
                throw new exception("[Game::" + "checkEndGame" + "][Error] player[UID=" + Convert.ToString((_session).m_pi.uid) + "] " + "tentou verificar se eh o final do jogo" + ", mas o game nao tem o info dele guardado. Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.GAME,
                1, 4));
            }

            return (m_course.findHoleSeq(pgi.hole) == m_ri.qntd_hole);
        }

        // Retorna todos os player que entrou no jogo, exceto os que quitaram
        protected virtual uint32_t getCountPlayersGame()
        {

            size_t count = 0;

            count = m_player_info.Count(_el =>
            {
                return _el.Value.flag != PlayerGameInfo.eFLAG_GAME.QUIT;
            });

            return (uint32_t)count;
        }
        protected virtual void initAchievement(Player _session)
        {

            var pgi = getPlayerInfo((_session));
            if (pgi == null)
            {
                throw new exception("[Game::" + "initAchievement" + "][Error] player[UID=" + Convert.ToString((_session).m_pi.uid) + "] " + "tentou inicializar o achievemento do player no jogo" + ", mas o game nao tem o info dele guardado. Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.GAME,
                    1, 4));
            }


            try
            {

                // Initialize Achievement Player
                //pgi.sys_achieve.incrementCounter(0x6C400002u/*Normal Game*/);

                //if (m_ri.natural.stBit.short_game/* & 2 /*Short Game*/)
                //    pgi.sys_achieve.incrementCounter(0x6C4000BBu/*Short Game*/);

                //if (m_ri.master == _session.m_pi.uid)
                //{
                //    pgi.sys_achieve.incrementCounter(0x6C400098u/*Master da Sala*/);

                //    if (m_ri.artefato > 0)
                //        pgi.sys_achieve.incrementCounter(0x6C400099u/*Master da Sala com Artefact*/);
                //}

                //if (_session.m_pi.ei.char_info != null)
                //{

                //    auto ctc = SysAchievement::getCharacterCounterTypeId(_session.m_pi.ei.char_info._typeid);

                //    if (ctc > 0u)
                //        pgi.sys_achieve.incrementCounter(ctc/*Character Counter Typeid*/);
                //}

                //if (_session.m_pi.ei.cad_info != null)
                //{

                //    auto ctc = SysAchievement::getCaddieCounterTypeId(_session.m_pi.ei.cad_info._typeid);

                //    if (ctc > 0u)
                //        pgi.sys_achieve.incrementCounter(ctc/*Caddie Counter Typeid*/);
                //}

                //if (_session.m_pi.ei.mascot_info != null)
                //{

                //    auto ctm = SysAchievement::getMascotCounterTypeId(_session.m_pi.ei.mascot_info._typeid);

                //    if (ctm > 0u)
                //        pgi.sys_achieve.incrementCounter(ctm/*Mascot Counter Typeid*/);
                //}

                //auto ct = SysAchievement::getCourseCounterTypeId(m_ri.course & 0x7F);

                //if (ct > 0u)
                //    pgi.sys_achieve.incrementCounter(ct/*Course Counter Item*/);

                //ct = SysAchievement::getQntdHoleCounterTypeId(m_ri.qntd_hole);

                //if (ct > 0u)
                //    pgi.sys_achieve.incrementCounter(ct/*Qntd Hole Counter Item*/);

                //// Fim do inicializa o Achievement

            }
            catch (exception e)
            {

                _smp::message_pool.push(new message("[Game::initAchievement][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if (ExceptionError.STDA_SOURCE_ERROR_DECODE_TYPE(e.getCodeError()) != STDA_ERROR_TYPE.SYS_ACHIEVEMENT)
                    throw;  // relança exception
            }
        }
        public virtual void records_player_achievement(Player _session)
        {
            //CHECK_SESSION("records_players_achievement");

            INIT_PLAYER_INFO("records_player_achievement", "tentou atualizar os achievement de records do player no jogo", _session, out PlayerGameInfo pgi);

            try
            {

                //if (pgi.ui.ob > 0)
                //    pgi.sys_achieve.incrementCounter(0x6C40004Cu/*OB*/, pgi.ui.ob);

                //if (pgi.ui.bunker > 0)
                //    pgi.sys_achieve.incrementCounter(0x6C40004Eu/*Bunker*/, pgi.ui.bunker);

                //if (pgi.ui.tacada > 0 || pgi.ui.putt > 0)
                //    pgi.sys_achieve.incrementCounter(0x6C400055u/*Shots*/, pgi.ui.tacada + pgi.ui.putt);

                //if (pgi.ui.hole > 0)
                //    pgi.sys_achieve.incrementCounter(0x6C400005u/*Holes*/, pgi.ui.hole);

                //if (pgi.ui.total_distancia > 0)
                //    pgi.sys_achieve.incrementCounter(0x6C400056u/*Yards*/, pgi.ui.total_distancia);

                //// Bug o valor é 0 por que (int)0.9f é 0 ele trunca não arredondo, e tem que truncar mesmo
                //// Para fixa esse bug é só fazer >= 1.f sempre vai ser (int) >= 1(truncado)
                //if (pgi.ui.best_drive >= 1.f)
                //    pgi.sys_achieve.incrementCounter(0x6C400057u/*Best Drive*/, (int)pgi.ui.best_drive);

                //if (pgi.ui.best_chip_in >= 1.f)
                //    pgi.sys_achieve.incrementCounter(0x6C400058u/*Best Chip-in*/, (int)pgi.ui.best_chip_in);

                //if (pgi.ui.best_long_putt >= 1.f)
                //    pgi.sys_achieve.incrementCounter(0x6C400077u/*Best Long-putt*/, (int)pgi.ui.best_long_putt);

                //if (pgi.ui.acerto_pangya > 0)
                //    pgi.sys_achieve.incrementCounter(0x6C40000Bu/*Acerto PangYa*/, pgi.ui.acerto_pangya);

                //if (pgi.data.pang > 0)
                //    pgi.sys_achieve.incrementCounter(0x6C40000Du/*Pangs Ganho em 1 jogo*/, (int)pgi.data.pang);

                //if (pgi.data.score != 0)
                //    pgi.sys_achieve.incrementCounter(0x6C40000Cu/*Score*/, pgi.data.score);

            }
            catch (exception e)
            {

                _smp::message_pool.push(new message("[Game::records_player_achievement][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if (ExceptionError.STDA_SOURCE_ERROR_DECODE_TYPE(e.getCodeError()) != STDA_ERROR_TYPE.SYS_ACHIEVEMENT)
                    throw;  // relança exception
            }
        }

        public virtual void update_sync_shot_achievement(Player _session, Location _last_location)
        {
            //CHECK_SESSION("update_sync_shot_achievement");

            INIT_PLAYER_INFO("update_sync_shot_achievement", "tentou atualizar o achievement de Desafios no jogo", _session, out PlayerGameInfo pgi);

            try
            {

                // Só conta se o player acertou o hole
                if (pgi.shot_sync.state_shot.display.acerto_hole)
                {

                    // Long-putt
                    if (pgi.shot_sync.state_shot.display.long_putt && pgi.shot_sync.state_shot.shot.club_putt == 1)
                    {
                        var diff = pgi.location.diffXZ(_last_location) * MEDIDA_PARA_YARDS;

                        //if (diff >= 30.0f)
                        //    pgi.sys_achieve.incrementCounter(0x6C400035u/*Long Putt 30y+*/);

                        //if (diff >= 25.0f)
                        //    pgi.sys_achieve.incrementCounter(0x6C400034u/*Long Putt 25y+*/);

                        //if (diff >= 20.0f)
                        //    pgi.sys_achieve.incrementCounter(0x6C400033u/*Long Putt 20y+*/);

                        //if (diff >= 17.0f)
                        //    pgi.sys_achieve.incrementCounter(0x6C400032u/*Long Putt 17y+*/);
                    }

                    // Fez o hole de Beam Impact
                    //if (pgi.shot_sync.state_shot.display.beam_impact == 1)
                    //    pgi.sys_achieve.incrementCounter(0x6C40006Fu/*Beam Impact*/);

                    //// Fez o hole com
                    //if (pgi.shot_sync.state_shot.shot.spin_front == 1)
                    //    pgi.sys_achieve.incrementCounter(0x6C400064u/*Spin Front*/);

                    //if (pgi.shot_sync.state_shot.shot.spin_back == 1)
                    //    pgi.sys_achieve.incrementCounter(0x6C400065u/*Spin Back*/);

                    //if (pgi.shot_sync.state_shot.shot.curve_left || pgi.shot_sync.state_shot.shot.curve_right)
                    //    pgi.sys_achieve.incrementCounter(0x6C400066u/*Curve*/);

                    //if (pgi.shot_sync.state_shot.shot.tomahawk)
                    //    pgi.sys_achieve.incrementCounter(0x6C400067u/*Tomahawk*/);

                    //if (pgi.shot_sync.state_shot.shot.spike)
                    //    pgi.sys_achieve.incrementCounter(0x6C400068u/*Spike*/);

                    //if (pgi.shot_sync.state_shot.shot.cobra)
                    //    pgi.sys_achieve.incrementCounter(0x6C40006Eu/*Cobra*/);

                    // Fez sem usar power shot
                    //if (pgi.shot_sync.state_shot.display.stDisplay.chip_in_without_special_shot && !pgi.shot_sync.state_shot.display.stDisplay.special_shot/*Nega*/)
                    //    pgi.sys_achieve.incrementCounter(0x6C40005Bu/*Fez sem usar power shot*/);

                    //// o pacote12 passa primeiro depois que o server response ele passa esse pacote1B, então esse valor sempre vai está certo
                    //// Fez Errando pangya
                    //if (pgi.shot_data.acerto_pangya_flag & 2/*Errou pangya*/ && !pgi.shot_sync.state_shot.shot.stShot.club_putt/*Nega*/)
                    //    pgi.sys_achieve.incrementCounter(0x6C400059u/*Fez errando pangya*/);
                }

                //// Tacada Power Shot ou Double Power Shot
                //if (pgi.shot_sync.state_shot.shot.stShot.power_shot)
                //    pgi.sys_achieve.incrementCounter(0x6C400051u/*Power Shot*/);

                //if (pgi.shot_sync.state_shot.shot.stShot.double_power_shot)
                //    pgi.sys_achieve.incrementCounter(0x6C400052u/*Double Power Shot*/);

            }
            catch (exception e)
            {

                _smp::message_pool.push(new message("[Game::update_sync_shot_achievement][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if (ExceptionError.STDA_SOURCE_ERROR_DECODE_TYPE(e.getCodeError()) != STDA_ERROR_TYPE.SYS_ACHIEVEMENT)
                    throw;  // relança exception
            }
        }

        public virtual void rain_hole_consecutivos_count(Player _session)
        {

            var chr = m_course.getConsecutivesHolesRain();

            INIT_PLAYER_INFO("rain_hole_consecutivos_count", "tentou atualizar o achievement count de chuva em holes consecutivos do player no jogo", _session, out PlayerGameInfo pgi);

            try
            {

                uint32_t count = 0;

                var seq = m_course.findHoleSeq(pgi.hole);

                if (chr.isValid())
                {

                    // 2 Holes consecutivos
                    //if ((count = chr._2_count.getCountHolesRainBySeq(seq)) > 0u)
                    //    pgi.sys_achieve.incrementCounter(0x6C40009Bu/*2 Holes consecutivos*/, count);

                    //if ((count = chr._3_count.getCountHolesRainBySeq(seq)) > 0u)
                    //    pgi.sys_achieve.incrementCounter(0x6C40009Cu/*3 Holes consecutivos*/, count);

                    //if ((count = chr._4_pluss_count.getCountHolesRainBySeq(seq)) > 0u)
                    //    pgi.sys_achieve.incrementCounter(0x6C40009Du/*4 ou mais Holes consecutivos*/, count);
                }

            }
            catch (exception e)
            {

                _smp::message_pool.push(new message("[Game::rain_hole_consecutivos_count][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if (ExceptionError.STDA_SOURCE_ERROR_DECODE_TYPE(e.getCodeError()) != STDA_ERROR_TYPE.SYS_ACHIEVEMENT)
                    throw;  // relança exception
            }
        }

        public virtual void score_consecutivos_count(Player _session)
        {

            int32_t score = -2, last_score = -2; 

            INIT_PLAYER_INFO("rain_score_consecutivos", "tentou atualizar o achievement contador de score consecutivos do player no jogo", _session, out PlayerGameInfo pgi);

            try
            {

                for (var i = 0; i < m_ri.qntd_hole; ++i)
                {
                    //score = SysAchievement::getScoreNum(pgi.progress.tacada[i], pgi.progress.par_hole[i]);

                    // Change Score, Soma o Count do Score	
                    //if ((score != last_score || i == (m_ri.qntd_hole - 1)/*Ultimo hole*/) && last_score != -2/*Primeiro Hole*/)
                    //{

                    //    // 1 == 2, 2 ou mais Holes com o mesmo score
                    //   // if (count >= 1u && last_score >= 0/*Scores que tem no achievement*/)
                    //    //{

                    //    //    switch (last_score)
                    //    //    {
                    //    //        case 0: // HIO
                    //    //            pgi.sys_achieve.incrementCounter(0x6C400063u/*HIO*/);
                    //    //            break;
                    //    //        case 1: // Alba
                    //    //            pgi.sys_achieve.incrementCounter(0x6C400062u/*Alba*/);
                    //    //            break;
                    //    //        case 2: // Eagle
                    //    //            pgi.sys_achieve.incrementCounter(0x6C400061u/*Eagle*/);
                    //    //            break;
                    //    //        case 3: // Birdie
                    //    //            pgi.sys_achieve.incrementCounter(0x6C40005Du/*Birdie*/);
                    //    //            break;
                    //    //        case 4: // Par
                    //    //            pgi.sys_achieve.incrementCounter(0x6C40005Eu/*Par*/);
                    //    //            break;
                    //    //        case 5: // Bogey
                    //    //            pgi.sys_achieve.incrementCounter(0x6C40005Fu/*Bogey*/);
                    //    //            break;
                    //    //        case 6: // Double Bogey
                    //    //            pgi.sys_achieve.incrementCounter(0x6C400060u/*Double Bogey*/);
                    //    //            break;
                    //    //    }
                    //    //}

                    //    // Reseta o count
                    //    count = 0;

                    //}
                    //else if (score == last_score)
                    //    count++;

                    // Update Last Score
                    last_score = score;
                }
            }

            catch (exception e)
            {

                _smp::message_pool.push(new message("[Game::score_consecutivos_count][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if (ExceptionError.STDA_SOURCE_ERROR_DECODE_TYPE(e.getCodeError()) != STDA_ERROR_TYPE.SYS_ACHIEVEMENT)
                    throw;  // relança exception
            }
        }

        public virtual void rain_count(Player _session)
        {

            try
            {

                // Recovery, Chuva, Neve/*Tempo Ruim*/
                if (m_course.countHolesRain() > 0)
                {

                    uint32_t count = 0;

                    INIT_PLAYER_INFO("rain_count_players", "tentou atualizar o achievement contador de chuva do player no jogo", _session, out PlayerGameInfo pgi);

                    // Pega pela quantidade de holes jogados
                    var seq = m_course.findHoleSeq(pgi.hole);

                    // if ((count = m_course.countHolesRainBySeq(seq)) > 0u)
                    //pgi.sys_achieve.incrementCounter(0x6C40009Au/*Chuva*/, count);
                }

            }
            catch (exception e)
            {

                _smp::message_pool.push(new message("[Game::rain_count][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if (ExceptionError.STDA_SOURCE_ERROR_DECODE_TYPE(e.getCodeError()) != STDA_ERROR_TYPE.SYS_ACHIEVEMENT)
                    throw;  // relança exception
            }
        }

        public virtual void setEffectActiveInShot(Player _session, uint64_t _effect)
        {
            //CHECK_SESSION("setEffectActiveInShot");

            try
            {

                INIT_PLAYER_INFO("setEffectActiveInShot", "tentou setar o efeito ativado na tacada", _session, out PlayerGameInfo pgi);

                pgi.effect_flag_shot.ullFlag |= _effect;

            }
            catch (exception e)
            {

                _smp::message_pool.push(new message("[Game::setEffectActiveInShot][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
        }
        // Limpa os dados que são usados para cada tacada, reseta ele para usar na próxima tacada 
        public virtual void clearDataEndShot(PlayerGameInfo _pgi)
        {

            if (_pgi == null)
                throw new exception("[Game::clearDataEndShot][Error] PlayerGameInfo *_pgi is invalid(null). Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.GAME, 100, 0));

            try
            {

                _pgi.effect_flag_shot.clear();
                _pgi.item_active_used_shot = 0;
                _pgi.earcuff_wind_angle_shot = 0.0f;

            }
            catch (exception e)
            {

                _smp::message_pool.push(new message("[Game::clearDataEndShot][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
        }
        public virtual void checkEffectItemAndSet(Player _session, uint32_t _typeid)
        {
            //CHECK_SESSION("checkEffectitemAndSet");

            try
            {

                var ability = sIff.getInstance().findAbility(_typeid);

                if (ability != null)
                {

                    for (var i = 0; i < ability.Efeito.Type.Length; ++i)
                    {

                        if (ability.Efeito.Type[i] == 0u)
                            continue;

                        if (ability.Efeito.Type[i] == (uint32_t)AbilityEffect.COMBINE_ITEM_EFFECT)
                        {

                            // find item setEffectTable
                            var effectTable = sIff.getInstance().findSetEffectTable((uint32_t)ability.Efeito.Rate[i]);

                            if (effectTable != null)
                            {

                                for (var j = 0; j < effectTable.effect.effect.Length; ++j)
                                {

                                    if (effectTable.effect.effect[j] == 0u || effectTable.effect.effect[j] < 4u)
                                        continue;

                                    switch ((eEFFECT)effectTable.effect.effect[j])
                                    {
                                        case eEFFECT.PIXEL:
                                            setEffectActiveInShot(_session, enumToBitValue(AbilityEffect.PIXEL));
                                            break;
                                        case eEFFECT.ONE_ALL_STATS:
                                            setEffectActiveInShot(_session, enumToBitValue(AbilityEffect.ONE_IN_ALL_STATS));
                                            break;
                                        case eEFFECT.WIND_DECREASE:
                                            setEffectActiveInShot(_session, enumToBitValue(AbilityEffect.DECREASE_1M_OF_WIND));
                                            break;
                                        case eEFFECT.PATINHA:
                                            setEffectActiveInShot(_session, enumToBitValue(AbilityEffect.PAWS_NOT_ACCUMULATE));
                                            break;
                                    }
                                }
                            }

                        }
                        else
                            setEffectActiveInShot(_session, enumToBitValue((AbilityEffect)(ability.Efeito.Type[i])));
                    }
                }

            }
            catch (exception e)
            {

                _smp::message_pool.push(new message("[Game::checkEffectitemAndSet][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
        }

        public virtual void SQLDBResponse(int _msg_id, Pangya_DB _pangya_db, object _arg)
        {

            if (_arg == null)
            {
                _smp::message_pool.push(new message("[Game::SQLDBResponse][WARNING] _arg is null com msg_id = " + (_msg_id), type_msg.CL_FILE_LOG_AND_CONSOLE));
                return;
            }

            // Por Hora só sai, depois faço outro tipo de tratamento se precisar
            if (_pangya_db.getException().getCodeError() != 0)
            {
                _smp::message_pool.push(new message("[Game::SQLDBResponse][Error] " + _pangya_db.getException().getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
                return;
            }

            var game = (GameBase)(_arg);

            switch (_msg_id)
            {
                case 12:    // Update ClubSet Workshop
                    {
                        var cmd_ucw = (CmdUpdateClubSetWorkshop)(_pangya_db);

                        _smp::message_pool.push(new message("[Game::SQLDBResponse][Log] player[UID=" + (cmd_ucw.getUID()) + "] Atualizou ClubSet[TYPEID=" + (cmd_ucw.getInfo()._typeid) + ", ID="
                                + (cmd_ucw.getInfo().id) + "] Workshop[C0=" + (cmd_ucw.getInfo().clubset_workshop.c[0]) + ", C1=" + (cmd_ucw.getInfo().clubset_workshop.c[1]) + ", C2="
                                + (cmd_ucw.getInfo().clubset_workshop.c[2]) + ", C3=" + (cmd_ucw.getInfo().clubset_workshop.c[3]) + ", C4=" + (cmd_ucw.getInfo().clubset_workshop.c[4])
                                + ", Level=" + (cmd_ucw.getInfo().clubset_workshop.level) + ", Mastery=" + (cmd_ucw.getInfo().clubset_workshop.mastery) + ", Rank="
                                + (cmd_ucw.getInfo().clubset_workshop.rank) + ", Recovery=" + (cmd_ucw.getInfo().clubset_workshop.recovery_pts) + "] Flag=" + (cmd_ucw.getFlag()) + "", type_msg.CL_FILE_LOG_AND_CONSOLE));
                        break;
                    }
                case 1: // Insert Ticket Report Dados
                    {
                        break;
                    }
                case 0:
                default:    // 25 é update item equipado slot
                    break;
            }
        }

        public virtual void makePlayerInfo(Player _session)
        {
            try
            {
                PlayerGameInfo pgi = makePlayerInfoObject(_session);

                // Bloqueia o OID para ninguém pegar ele até o torneio acabar
                sgs.gs.getInstance().blockOID((uint)_session.m_oid);

                // Update Place player
                _session.m_pi.place = 0;   // Jogando

                pgi.uid = _session.m_pi.uid;
                pgi.oid = (uint)_session.m_oid;
                pgi.level = (byte)_session.m_pi.mi.level;

                // Entrou no Jogo depois de ele ter começado
                if (m_state)
                    pgi.enter_after_started = 1;

                // Typeid do Mascot Equipado
                if (_session.m_pi.ei.mascot_info != null && _session.m_pi.ei.mascot_info._typeid > 0)
                    pgi.mascot_typeid = _session.m_pi.ei.mascot_info._typeid;

                // Premium User
                if (_session.m_pi.m_cap.premium_user/* & (1 << 14)/*Premium User*/)
                    pgi.premium_flag = 1;

                // Card Wind Flag
                pgi.card_wind_flag = getPlayerWindFlag(_session);

                // Treasure Hunter Points Card Player Initialize Data
                // Não pode ser chamado depois do Init Item Used Game, por que ele vai add os pontos dos itens que dá Drop rate e treasure hunter point
                pgi.thi = getPlayerTreasureInfo(_session);

                // Flag Assist
                var pWi = _session.m_pi.findWarehouseItemByTypeid(ASSIST_ITEM_TYPEID);

                if (pWi != null)
                    pgi.assist_flag = 1;

                // Verifica se o player está com o motion item equipado
                pgi.char_motion_item = checkCharMotionItem(_session);

                // Motion Item da Treasure Hunter Point também
                if (pgi.char_motion_item == 1)
                    pgi.thi.all_score += 20;    // +20 all score

                pgi.data.clear();
                pgi.location.clear();
                if (!m_player_info.ContainsKey(_session)) // ainda nao
                {
                    m_player_info.Add(_session, pgi);
                }
                else //ja tem ele 
                {
                    try
                    {

                        // pega o antigo PlayerGameInfo para usar no Log
                        var pgi_ant = m_player_info[_session];

                        // Novo PlayerGameInfo
                        m_player_info[_session] = pgi;

                        // Log de que trocou o PlayerGameInfo da session
                        _smp::message_pool.push(new message("[Game::makePlayerInfo][WARNING][Log] Player[UID=" + (_session.m_pi.uid)
                                + "] esta trocando o PlayerGameInfo[UID=" + (pgi_ant.uid) + "] do player anterior que estava conectado com essa session, pelo o PlayerGameInfo[UID="
                                + (pgi.uid) + "] do player atual da session.", type_msg.CL_FILE_LOG_AND_CONSOLE));


                    }
                    catch (ArgumentOutOfRangeException e)
                    {
                        _smp::message_pool.push(new message("[Game::makePlayerInfo][Error][WARNING] Player[UID=" + (_session.m_pi.uid)
                                + "], nao conseguiu atualizar o PlayerGameInfo da session para o novo PlayerGameInfo do player atual da session. Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));
                    }
                }

                // Init Item Used Game(Dados)
                requestInitItemUsedGame(_session, pgi);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public virtual void clearAllPlayerInfo()
        {

            Monitor.Enter(m_cs);

            foreach (var el in m_player_info)
            {

                if (el.Value != null)
                {

                    sgs.gs.getInstance().unblockOID(el.Value.oid);   // Desbloqueia o OID

                }
            }

            m_player_info.Clear();


            Monitor.Exit(m_cs);

        }

        public virtual void initAllPlayerInfo()
        {

            foreach (var el in m_players)
                makePlayerInfo(el);
        }

        // Make Object Player Info Polimofirsmo
        public virtual PlayerGameInfo makePlayerInfoObject(Player _session)
        {
            // ignore : UNREFERENCED_PARAMETER(_session); == ignore

            return new PlayerGameInfo();
        }
        /// <summary>
        /// metodo é ignorante
        /// </summary>
        /// <param name="_session"></param>
        /// <param name="_packet"></param>
        public virtual void requestInitShotSended(Player _session, packet _packet)
        {

        }
    }
}