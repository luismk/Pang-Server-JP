using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Pangya_GameServer.Game.System;
using Pangya_GameServer.Game.Utils;
using Pangya_GameServer.GameType;
using Pangya_GameServer.PacketFunc;
using Pangya_GameServer.Session;
using Pangya_GameServer.UTIL;
using PangyaAPI.IFF.JP.Extensions;
using PangyaAPI.IFF.JP.Models.Data;
using PangyaAPI.Network.PangyaPacket;
using PangyaAPI.Utilities;
using PangyaAPI.Utilities.BinaryModels;
using PangyaAPI.Utilities.Log;
using static System.Net.WebRequestMethods;
using static Pangya_GameServer.Game.Manager.RoomBotGMEvent;
using static Pangya_GameServer.GameType._Define;
using Timer = System.Timers.Timer;

namespace Pangya_GameServer.Game.Manager
{
    public class m_cs_instancia : Singleton<CriticalSectionInstancia>
    {
    }
    public class m_instancias : Singleton<List<RoomBotGMEventInstanciaCtx>>
    {
    }
    public partial class RoomBotGMEvent : room
    {

        protected stStateRoomBotGMEventSync m_state_rbge = new stStateRoomBotGMEventSync();

        protected PangyaTime m_create_room = new PangyaTime(); // Data que a sala foi criada

        protected PangyaTimer m_timer_count_down;
        protected ThreadManager m_wait_time_start;

        protected EventWaitHandle m_hEvent_wait_start;
        protected EventWaitHandle m_hEvent_wait_start_pulse;

        protected List<stReward> m_rewards = new List<stReward>();

        public enum eSTATE_ROOM_BOT_GM_EVENT_SYNC : byte
        {
            WAIT_TIME_START,
            WAIT_10_SECONDS_START,
            WAIT_END_GAME
        }



        public class stStateRoomBotGMEventSync
        {
            public stStateRoomBotGMEventSync()
            {
                this.m_state = eSTATE_ROOM_BOT_GM_EVENT_SYNC.WAIT_TIME_START;
            }

            public void @lock()
            {
                Monitor.Enter(m_cs);
            }

            public void unlock()
            {
                Monitor.Exit(m_cs);
            }

            public eSTATE_ROOM_BOT_GM_EVENT_SYNC getState()
            {
                return m_state;
            }

            public void setState(eSTATE_ROOM_BOT_GM_EVENT_SYNC _state)
            {

                m_state = _state;
            }

            public void setStateWithLock(eSTATE_ROOM_BOT_GM_EVENT_SYNC _state)
            {

                @lock();

                m_state = _state;

                unlock();
            }


            protected eSTATE_ROOM_BOT_GM_EVENT_SYNC m_state;

            protected object m_cs = new object();
        }

        // Static Instance vector strunct
        public class RoomBotGMEventInstanciaCtx
        {
            public enum eSTATE : byte
            {
                GOOD,
                DESTROYING,
                DESTROYED
            }

            public RoomBotGMEventInstanciaCtx(RoomBotGMEvent _rbge, eSTATE _state)
            {
                this.m_rbge = _rbge;
                this.m_state = (_state);
            }

            public RoomBotGMEvent m_rbge { get; set; }
            public eSTATE m_state { get; set; }
        }

        public class CriticalSectionInstancia
        {
            public CriticalSectionInstancia()
            {
                this.m_state = false;
                this.m_lock = false;

                init();

            }
            public void init()
            {

                if (!m_state)
                {
                }

                m_state = true;
            }
            public void @lock()
            {

                if (!m_state)
                {
                    init();
                }

                Monitor.Enter(m_cs);
                // Est  bloqueado
                m_lock = true;

            }
            public void unlock()
            {

                if (!m_lock)
                {
                    return; // N o est  bloqueado
                }

                // Desbloquea
                m_lock = false;

                Monitor.Exit(m_cs);
            }
            public object m_cs { get; set; } = new object();
            public bool m_state { get; set; }
            public bool m_lock { get; set; }
        }

        // Construtor e destrutor
        public RoomBotGMEvent(byte channel_owner, RoomInfoEx ri, List<stReward> _rewards)
      : base(channel_owner, ri)
        {

            this.m_state_rbge = new stStateRoomBotGMEventSync();
            this.m_create_room = new PangyaTime();  // Supondo que PangyaTime seja equivalente a um tipo DateTime customizado
            this.m_rewards = _rewards;
            this.m_wait_time_start = null;
            this.m_timer_count_down = null;

            // Coloca a instância no vetor estático
            push_instancia(this);

            // Data que a sala foi criada
            m_create_room.CreateTime();

            try
            {
                // Cria o evento que a thread de sincronização do tempo vai esperar
                m_hEvent_wait_start = new EventWaitHandle(false, EventResetMode.ManualReset);

                // Cria o evento que vai pulsar a thread para responder mais rápido a mudanças de jogadores
                m_hEvent_wait_start_pulse = new EventWaitHandle(false, EventResetMode.AutoReset);
            }
            catch (Exception ex)
            {
                throw new Exception("[RoomBotGMEvent::RoomBotGMEvent][Error] Falha ao criar eventos de sincronização do Bot GM Event.", ex);
            }

            // Cria a thread que vai sincronizar o tempo de início do Bot GM Event
            m_wait_time_start = new ThreadManager(new Thread(() => _waitTimeStart(this)))
            {
                Name = "RoomBotGMEvent::WaitTimeStart"
            };


            // Ajusta flags e troféu da sala
            m_ri.flag_gm = 1;
            m_ri.state_flag = 0x100;
            m_ri.trofel = TROFEL_GM_EVENT_TYPEID;
        }

        public override bool isAllReady()
        {

            // é sempre true porque quem começa o jogo nessa sala é sempre o server
            // O cliente da erro na hora de começar se tiver convidado na sala
            // então verifica se não tem nenhum convidado na sala
            return !_haveInvited();
        }
        public bool startGame()
        {

            var p = new PangyaBinaryWriter();

            bool ret = true;

            try
            {

                // Verifica se j  tem um jogo inicializado e lan a error se tiver, para o cliente receber uma resposta
                if (m_pGame != null)
                {
                    throw new exception("[RoomBotGMEvent::startGame][Error] Server tentou comecar o jogo na sala[NUMERO=" + Convert.ToString(m_ri.numero) + "], mas ja tem um jogo inicializado. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM_BOT_GM_EVENT,
                        8, 0x5900202));
                }

                // Verifica se todos est o prontos se n o da erro
                if (!isAllReady())
                {
                    throw new exception("[RoomBotGMEvent::startGame][Error] Server tentou comecar o jogo na sala[NUMERO=" + Convert.ToString(m_ri.numero) + ", MASTER=" + Convert.ToString(m_ri.master) + "], mas nem todos jogadores estao prontos. Hacker ou Bug.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM_BOT_GM_EVENT,
                    8, 0x5900202));
                }

                if (m_ri.course >= RoomInfo.eCOURSE.UNK)
                {

                    // Special Shuffle Course
                    if (m_ri.getTipo() == RoomInfo.TIPO.SPECIAL_SHUFFLE_COURSE && m_ri.modo == (byte)Hole.eMODO.M_SHUFFLE_COURSE)
                    {

                        m_ri.course = (RoomInfo.eCOURSE)(0x80 | (byte)RoomInfo.eCOURSE.CHRONICLE_1_CHAOS);

                    }
                    else
                    { // Random normal

                        Lottery lottery = new Lottery();

                        foreach (var el in sIff.getInstance().getCourse())
                        {

                            var course_id = sIff.getInstance().getItemIdentify(el.ID);

                            if (course_id != 17 && course_id != 0x40)
                            {
                                lottery.Push(100, course_id);
                            }
                        }

                        var lc = lottery.SpinRoleta();

                        if (lc != null)
                        {
                            m_ri.course = (RoomInfo.eCOURSE)(0x80u | (byte)lc.Value);
                        }
                    }
                }

                RateValue rv = new RateValue();

                rv.exp = m_ri.rate_exp = (uint)sgs.gs.getInstance().getInfo().rate.exp;
                rv.pang = m_ri.rate_pang = (uint)sgs.gs.getInstance().getInfo().rate.pang;

                // Angel Event
                m_ri.angel_event = sgs.gs.getInstance().getInfo().rate.angel_event == 1;

                rv.clubset = (uint)sgs.gs.getInstance().getInfo().rate.club_mastery;
                rv.rain = (uint)sgs.gs.getInstance().getInfo().rate.chuva;
                rv.treasure = (uint)sgs.gs.getInstance().getInfo().rate.treasure;

                rv.persist_rain = 0; // Persist rain flag isso   feito na classe game

                switch (m_ri.getTipo())
                {
                    case RoomInfo.TIPO.TOURNEY:
                        m_pGame = new Tourney(v_sessions,
                            m_ri, rv, m_ri.channel_rookie);
                        break;
                    default:
                        throw new exception("[RoomBotGMEvent::startGame][Error] Server tentou comecar o jogo na sala[NUMERO=" + Convert.ToString(m_ri.numero) + ", MASTER=" + Convert.ToString(m_ri.master) + "], mas o tipo da sala nao eh Tourney. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM_BOT_GM_EVENT,
                            9, 0x5900202));
                }

                // Verifica se tem s  1 player na sala, se tiver cria um Bot para o player poder jogar
                if (v_sessions.Count() == 1u)
                {
                    addBotVisual(v_sessions.First());
                }

                // Update Room State
                m_ri.state = 0; // IN GAME

                p.init_plain((ushort)0x230);

                packet_func.room_broadcast(this,
                    p, 1);

                p.init_plain((ushort)0x231);

                packet_func.room_broadcast(this,
                    p, 1);

                uint rate_pang = (uint)sgs.gs.getInstance().getInfo().rate.pang;

                p.init_plain((ushort)0x77);

                p.WriteUInt32(rate_pang); // Rate Pang

                packet_func.room_broadcast(this,
                    p, 1);

                // Coloca para o thread que cria o tempo sspera o jogo acabar
                m_state_rbge.setStateWithLock(eSTATE_ROOM_BOT_GM_EVENT_SYNC.WAIT_END_GAME);

            }
            catch (exception e)
            {

                message_pool.push(new message("[RoomBotGMEvent::startGame][Error] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                ret = false; // Error ao inicializar o Jogo
            }

            return ret;
        }
        public static void initFirstInstance()
        {

            if (m_cs_instancia.getInstance().m_state && m_instancias.getInstance().empty())
            {
                message_pool.push(new message("[RoomBotGMEvent::initFirstInstance][Log] Criou primeira instance do Singleton da classe Room Bot GM Event static vector.", type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
        }
        private static void _waitTimeStart(object param)
        {
            if (!(param is RoomBotGMEvent pTP))
                return;

            try
            {
                pTP.waitTimeStart();
            }
            catch (Exception ex)
            {
                Console.WriteLine("[RoomBotGMEvent::_waitTimeStart][Error] " + ex);
            }
        }

        public void waitTimeStart()
        {
            try
            {
                message_pool.push(new message("[RoomBotGMEvent::waitTimeStart][Log] waitTimeStart iniciado com sucesso!", type_msg.CL_FILE_LOG_AND_CONSOLE));

                int retWait = WaitHandle.WaitTimeout;

                if (m_hEvent_wait_start == null || m_hEvent_wait_start_pulse == null)
                    throw new InvalidOperationException("Eventos de espera não foram inicializados.");

                WaitHandle[] waitEvents = new WaitHandle[] {
    m_hEvent_wait_start,
    m_hEvent_wait_start_pulse
};

                while ((retWait = WaitHandle.WaitAny(waitEvents, 1000)) == WaitHandle.WaitTimeout || retWait == 1)
                {
                    try
                    {
                        m_state_rbge.@lock();

                        switch (m_state_rbge.getState())
                        {
                            case eSTATE_ROOM_BOT_GM_EVENT_SYNC.WAIT_TIME_START:
                                if (m_timer_count_down == null)
                                {
                                    if ((UtilTime.GetLocalTimeDiff(m_create_room.ConvertTime()) / STDA_10_MICRO_PER_MIN) >= 2 /*&& v_sessions.Count > 0*/)
                                    {
                                        count_down(10);
                                        m_state_rbge.setState(eSTATE_ROOM_BOT_GM_EVENT_SYNC.WAIT_10_SECONDS_START);
                                    }
                                    else if (_getRealNumPlayersWithoutInvited() == m_ri.max_player)
                                    {
                                        // Broadcast de sala cheia
                                        var p = new PangyaBinaryWriter(0x40);
                                        p.WriteByte(12);    // msg
                                        p.WriteUInt16(0);     // nick vazio
                                        p.WriteUInt16(0);     // msg vazio
                                        p.WriteUInt32(10);    // tempo para iniciar (10s)

                                        packet_func.room_broadcast(this, p, 1);

                                        count_down(10);
                                        m_state_rbge.setState(eSTATE_ROOM_BOT_GM_EVENT_SYNC.WAIT_10_SECONDS_START);
                                    }
                                }
                                break;

                            case eSTATE_ROOM_BOT_GM_EVENT_SYNC.WAIT_10_SECONDS_START:
                                // Nada a fazer aqui, só aguardar countdown acabar
                                break;

                            case eSTATE_ROOM_BOT_GM_EVENT_SYNC.WAIT_END_GAME:
                                // Nada a fazer
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        message_pool.push(new message($"[RoomBotGMEvent::waitTimeStart][ErrorSystem] {ex.Message}\n{ex.StackTrace}", type_msg.CL_FILE_LOG_AND_CONSOLE));
                    }
                    finally
                    {
                        m_state_rbge.unlock();
                    }
                }
            }
            catch (Exception ex)
            {
                message_pool.push(new message($"[RoomBotGMEvent::waitTimeStart][ErrorSystem] {ex.Message}\n{ex.StackTrace}", type_msg.CL_FILE_LOG_AND_CONSOLE));
            }

#if DEBUG
            message_pool.push(new message("[RoomBotGMEvent::waitTimeStart][Log] Saindo de waitTimeStart()...", type_msg.CL_FILE_LOG_AND_CONSOLE));
#else
     message_pool.push(new message("[RoomBotGMEvent::waitTimeStart][Log] Saindo de waitTimeStart()...");
#endif
        }

        public int _count_down_time(object _arg1, object _arg2)
        {

            RoomBotGMEvent rbge = Tools.reinterpret_cast<RoomBotGMEvent>(_arg1);
            long sec_to_start = (long)(_arg2);

            try
            {

                if (rbge != null && instancia_valid(rbge))
                {
                    if (rbge.count_down(sec_to_start) == 1)
                    {
                        sgs.gs.getInstance().destroyRoom(rbge.m_channel_owner, (short)rbge.m_ri.numero); // Destroi a sala, se não tem players, ou não conseguiu inicializar
                    }
                }

            }
            catch (exception e)
            {

                message_pool.push(new message("[RoomBotGMEvent::_count_down_time][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }

            return 0;
        }
        public int count_down(long _sec_to_start)
        {

            int ret = 0;

            try
            {

                // Bloquea a sala para não dá erro de conflito
                @lock();

                if (_sec_to_start <= 0)
                { // Começa o jogo

                    // excluí o timer se ele ainda existir
                    clear_timer_count_down();

                    // Começa o jogo se tem pelo menos 1 jogador na sala
                    if (v_sessions.Count() >= 1 && startGame())
                    {
                        sgs.gs.getInstance().sendUpdateRoomInfo(this, 3); // Update Room Info
                    }
                    else if (v_sessions.Count() >= 1)
                    {
                        // Coloca para começar espera o tempo de começar o jogo de novo, por que não conseguiu criar a sala
                        // Pode ter convidado na sala, aí não pode iniciar o jogo por que o cliente trava
                        count_down(10);
                    }
                    else
                    {
                        ret = 1; // Destroi a sala
                    }

                }
                else
                {

                    uint wait = 0u;
                    int rest = 0;

                    byte type = 1;

                    uint interval = 0u;
                    float diff = 0.0f;

                    int elapsed_sec = (m_timer_count_down != null) ? (int)Math.Round(m_timer_count_down.GetElapsedMilliseconds() / 1000.0f)/* / *Mili para segundos * /*/ : 0;

                    _sec_to_start -= elapsed_sec;

                    if ((diff = ((_sec_to_start - 10/* / *10 segundos * /*/) / 30.0f /*/ *30 segundos * /*/)) >= 1.0f)
                    { // Intervalo de 30 segundos

                        if ((_sec_to_start % 30) == 0)
                        {

                            // Intervalo
                            interval = (uint)(30 * 1000); // 30 segundos

                            wait = (uint)(interval * (int)diff); // 30 * diff minutos em milisegundos

                        }
                        else
                        {

                            // Corrige o tempo para ficar no intervalo certo
                            wait = interval = (uint)((_sec_to_start % 30) * 1000);

                        }

                    }
                    else if ((diff = ((_sec_to_start - 1/* / *1 segundo * /*/) / 10.0f/* / *10 segundos * /*/)) >= 1.0f)
                    { // Intervalo de 10 segundos

                        if ((_sec_to_start % 10) == 0)
                        {

                            // Intervalo
                            interval = (uint)(10 * 1000); // 10 segundos

                            wait = (uint)(interval * (int)diff); // 10 * diff segundos em milisegundos

                        }
                        else
                        {

                            // Corrige o tempo para ficar no intervalo certo
                            wait = interval = (uint)((_sec_to_start % 10) * 1000);
                        }

                    }
                    else
                    { // Intervalo de 1 segundo

                        diff = (float)Math.Round(_sec_to_start / 1.0f, MidpointRounding.AwayFromZero);

                        // Intervalo
                        interval = 1000; // 1 segundo

                        wait = (uint)(interval * (int)diff); // 1 * diff segundos em milesegundos

                    }

                    // UPDATE ON GAME
                    var p = new PangyaBinaryWriter((ushort)0x40);

                    p.WriteByte(11); // Temporizador Grand Prix e Grand Zodiac

                    p.WriteUInt16(0u); // Nick
                    p.WriteUInt16(0u); // Msg

                    p.WriteUInt32((uint)_sec_to_start);

                    packet_func.room_broadcast(this,
                        p, 1);

                    // Make Timer
                    if (m_timer_count_down == null)
                    {
                        // Destroi timer anterior, se existir
                        if (m_timer_count_down != null)
                        {
                            clear_timer_count_down();
                        }

                        // Cria e inicia o novo timer
                        m_timer_count_down = sgs.gs.getInstance().makeTime(wait, new List<uint> { interval });
                         
                        m_timer_count_down.start();


                        // Make Time, Cria o job como thread
                        Thread _job = new Thread(() => _count_down_time(this, _sec_to_start));

                        _job.Start();
                    }
                }

                // Libera
                unlock();

            }
            catch (exception e)
            {

                // Libera
                unlock();

                message_pool.push(new message("[RoomBotGMEvent::count_down][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }

            return ret;
        }

        public void finish_thread_sync_wait_time_start()
        {

            try
            {

                if (m_wait_time_start != null)
                {
                    m_wait_time_start.SignalStart();
                    m_wait_time_start.WaitThreadFinish();
                    m_wait_time_start.Dispose();
                    m_wait_time_start = null;
                }

            }
            catch (exception e)
            {

                message_pool.push(new message("[RoomBotGMEvent::finish_thread_sync_wait_time_start][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if (m_wait_time_start != null)
                {
                    m_wait_time_start.ExitThread();
                    m_wait_time_start.Dispose();
                    m_wait_time_start = null;
                }
            }

            m_wait_time_start = null;

            if (m_hEvent_wait_start != null)
            {
                m_hEvent_wait_start.Close();

            }

            if (m_hEvent_wait_start_pulse != null)
            {
                m_hEvent_wait_start_pulse.Close();
            }

        }
        public void clear_timer_count_down()
        {

            try
            {

                if (m_timer_count_down != null)
                    m_timer_count_down = sgs.gs.getInstance().unMakeTime(m_timer_count_down);
            }
            catch (exception e)
            {

                message_pool.push(new message("[RoomBotGMEvent::clear_timer_count_down][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }

            m_timer_count_down = null;
        }

        public override void finish_game()
        {

            if (m_pGame != null)
            {

                // Envia presentes aqui, mas tem que ter jogadores no vector do game
                if (m_pGame.getSessions().empty() || m_rewards.empty())
                {

                    finish_game();

                    return;
                }

                // Envia os presentes aqui 
                Func<uint, string> getItemName = (_typeid) => {
                    string ret = "";

                    var @base = sIff.getInstance().findCommomItem(_typeid);

                    if (@base != null)
                    {
                        ret = (@base.Name);
                    }

                    return ret;
                };

                string reward_str = "{";
                foreach (var it_r in m_rewards) 
                {

                    if (it_r != m_rewards.begin())
                    {
                        reward_str += ", [";
                    }
                    else
                    {
                        reward_str += "[";
                    }

                    reward_str += it_r.toString() + "]";
                }

                reward_str += "}";

                try
                {

                    List<stItem> v_item = new List<stItem>();
                    stItem item = new stItem();
                    BuyItem bi = new BuyItem();

                    Player p = null;

                    foreach (var el_p in v_sessions)
                    {

                        // Limpa, por que   por jogador
                        v_item.Clear();

                        if (el_p == null || (p = sgs.gs.getInstance().findPlayer(el_p.m_pi.uid)) == null)
                        {

                            // Log, Player que ganhou n o est  mais online, vai ficar sem o item
                            message_pool.push(new message("[RoomBotGMEvent::finish_game][WARNING] Player[UID=" + (el_p == null ? "UNKNOWN" : Convert.ToString(el_p.m_pi.uid)) + "] ganhou o item(ns)" + reward_str + ", mas saiu antes dos pr mios ser entregues, vai ficar sem o pr mio.", type_msg.CL_FILE_LOG_AND_CONSOLE));

                            continue;
                        }

                        // Initialize itens
                        foreach (var el_r in m_rewards)
                        {

                            // Limpa
                            bi.clear();
                            item.clear();

                            // Initialize
                            bi.id = -1;
                            bi._typeid = el_r._typeid;
                            bi.qntd = el_r.qntd;
                            bi.time = (short)(ushort)el_r.qntd_time;

                            item_manager.initItemFromBuyItem(p.m_pi,
                                ref item, bi, false, 0, 0,
                                1);

                            if (item._typeid == 0)
                            {
                                message_pool.push(new message("[RoomBotGMEvent::finish_game][Error][WARNING] tentou enviar o reward para o player[UID=" + Convert.ToString(p.m_pi.uid) + "] o Item[" + el_r.toString() + "], mas nao conseguiu inicializar o item. Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));
                            }

                            v_item.Add(item);
                        }

                        var msg = ("Bot GM Event(" + m_create_room.ConvertTime() + "): item[ " + (v_item.Count == 0 ? "NONE" : getItemName(v_item[0]._typeid)) + " ]");

                        if (v_item.Count == 0 || MailBoxManager.sendMessageWithItem(0,
                            p.m_pi.uid, msg, v_item) <= 0)
                        {
                            message_pool.push(new message("[RoomBotGMEvent::finish_game][Error][WARNING] tentou enviar reward para o player[UID=" + Convert.ToString(p.m_pi.uid) + "] o Item(ns)" + reward_str + ", mas nao conseguiu colocar o item no mail box dele. Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));
                        }

                        // Log
                        message_pool.push(new message("[RoomBotGMEvent::finish_game][Log] Player[UID=" + Convert.ToString(p.m_pi.uid) + "] ganhou no Bot GM Event(" + (m_create_room.ConvertTime()) + "): Item(" + Convert.ToString(m_rewards.Count) + ")" + reward_str, type_msg.CL_FILE_LOG_AND_CONSOLE));
                    }

                }
                catch (exception e)
                {

                    message_pool.push(new message("[RoomBotGMEvent::finish_game][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
                }

                // Finaliza o jogo de verdade
               finish_game();

                // 2min para destruir a sala
                Action<object, object> destroyRoomTimer = (object _arg1, object _arg2) =>
                {
                    RoomBotGMEvent rbge = Tools.reinterpret_cast<RoomBotGMEvent>(_arg1);
                    long sec_to_start = (long)(_arg2);

                    try
                    {

                        if (rbge != null && instancia_valid(rbge))
                        {
                            sgs.gs.getInstance().destroyRoom(rbge.m_channel_owner, (short)rbge.m_ri.numero); // Destroi a sala, se n o tem players, ou n o conseguiu inicializar
                        }

                    }
                    catch (exception e)
                    {

                        message_pool.push(new message("[RoomBotGMEvent::lambda(destroyRoom)][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
                    } 
                };


                // Se o Shutdown Timer estiver criado descria e cria um novo
                if (m_timer_count_down != null)
                {
                    clear_timer_count_down();
                }

                m_timer_count_down = sgs.gs.getInstance().makeTime(2 * 60000);

                m_timer_count_down.start();

                // Make Timer
                Thread _job = new Thread(() => destroyRoomTimer(this, (object)0));

                _job.Start();
            }

        }
        public virtual bool isDropRoom()
        {
            return false; // Não drop(destroy) a sala
        }
        public void push_instancia(RoomBotGMEvent _rbge)
        {

            m_cs_instancia.getInstance().@lock();

            m_instancias.getInstance().Add(new RoomBotGMEventInstanciaCtx(_rbge, RoomBotGMEventInstanciaCtx.eSTATE.GOOD));

            m_cs_instancia.getInstance().unlock();
        }
        public void pop_instancia(RoomBotGMEvent _rbge)
        {

            m_cs_instancia.getInstance().@lock();

            var index = get_instancia_index(_rbge);

            if (index >= 0)
            {
                m_instancias.getInstance().RemoveAt(index);
            }

            m_cs_instancia.getInstance().unlock();
        }
        public void set_instancia_state(RoomBotGMEvent _rbge, RoomBotGMEventInstanciaCtx.eSTATE _state)
        {

            m_cs_instancia.getInstance().@lock();

            var index = get_instancia_index(_rbge);

            if (index >= 0)
            {
                m_instancias.getInstance()[index].m_state = _state;
            }

            m_cs_instancia.getInstance().unlock();
        }
        public int get_instancia_index(RoomBotGMEvent _rbge)
        {

            int index = -1;

            for (var i = 0; i < m_instancias.getInstance().Count; ++i)
            {

                if (m_instancias.getInstance()[i].m_rbge == _rbge)
                {

                    index = (int)i;

                    break;
                }
            }

            return index;
        }
        public bool instancia_valid(RoomBotGMEvent _rbge)
        {

            bool valid = false;

            m_cs_instancia.getInstance().@lock();

            var index = get_instancia_index(_rbge);

            if (index >= 0)
            {
                valid = (m_instancias.getInstance()[index].m_state == RoomBotGMEventInstanciaCtx.eSTATE.GOOD);
            }

            m_cs_instancia.getInstance().unlock();

            return valid;
        }
    }
}
