using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Timers;
using Pangya_GameServer.Game.Manager;
using Pangya_GameServer.Game.System;
using Pangya_GameServer.GameType;
using Pangya_GameServer.PacketFunc;
using Pangya_GameServer.Session;
using Pangya_GameServer.UTIL;
using PangyaAPI.IFF.JP.Extensions;
using PangyaAPI.IFF.JP.Models.Data;
using PangyaAPI.IFF.JP.Models.Flags;
using PangyaAPI.Network.PangyaPacket;
using PangyaAPI.Utilities;
using PangyaAPI.Utilities.BinaryModels;
using PangyaAPI.Utilities.Log;
using static Pangya_GameServer.GameType._Define;
using static Pangya_GameServer.GameType.RoomInfo;
namespace Pangya_GameServer.Game.Base
{
    public abstract class TourneyBase : GameBase
    {
        public uint m_max_player = 255u;
        public int m_entra_depois_flag;

        public TicketReportInfo m_tri = new TicketReportInfo();

        public Medal[] m_medal = new Medal[12];
        public TourneyBase(List<Player> _players, RoomInfoEx _ri, RateValue _rv, bool _channel_rookie) : base(_players, _ri, _rv, _channel_rookie)
        {
            this.m_tri = new TicketReportInfo();
            this.m_max_player = 255u;
            this.m_entra_depois_flag = -1;
            m_medal = new Medal[12];

            for (var i = 0u; i < 12; ++i)
                m_medal[i] = new Medal();
        }

        ~TourneyBase()
        {
            //_smp.message_pool.getInstance().push(new message("[TourneyBase::~TourneyBase][Log] TourneyBase destroyed on Room[Number=" + (m_ri.numero) + "]", type_msg.CL_FILE_LOG_AND_CONSOLE));
        }

        public abstract void changeHole(Player _session);
        public abstract void finishHole(Player _session);
        public abstract void timeIsOver();

        public override void sendInitialData(Player _session)
        {
            try
            {
                // m_players.Count representa o tamanho da lista de jogadores
                if (Interlocked.Increment(ref m_sync_send_init_data) == m_players.Count)
                {
                    // Zera a variável atômica
                    Interlocked.Exchange(ref m_sync_send_init_data, 0);

                    var p = new PangyaBinaryWriter();

                    // Game Data Init
                    p.init_plain(0x76);

                    p.WriteByte(m_ri.tipo_show);
                    p.WriteUInt32(1);

                    p.WriteTime(m_start_time);//escreve um tempo

                    packet_func.game_broadcast(this,
                        p, 1);

                    // Course
                    // Send Individual Packet to all players in game
                    foreach (var el in m_players)
                    {
                        base.sendInitialData(el);
                    }
                }
            }
            catch (exception e)
            {
                //_smp.message_pool.getInstance().push(new message("[TourneyBase::sendInitialData][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
        }
        public override void sendInitialDataAfter(Player _session)
        {

            var p = new PangyaBinaryWriter();

            try
            {

                // Send Initial Data of Game
                p.init_plain(0x113);

                p.WriteByte(4);
                p.WriteUInt32(3);

                p.WriteTime(m_start_time);

                packet_func.session_send(p,
                    _session, 1);

                // Course
                p.init_plain(0x113);

                p.WriteByte(4);
                p.WriteByte(4);

                p.WriteByte((byte)m_ri.course);
                p.WriteByte(m_ri.tipo_show);
                p.WriteByte(m_ri.modo);
                p.WriteByte(m_ri.qntd_hole);
                p.WriteUInt32(m_ri.trofel);
                p.WriteUInt32(m_ri.time_vs);
                p.WriteUInt32(m_ri.time_30s);

                // Hole Info, Hole Spinning Cube, end Seed Random Course
                m_course.makePacketHoleInfo(p, 1);

                packet_func.session_send(p,
                    _session, 1);

            }
            catch (exception e)
            {

                //_smp.message_pool.getInstance().push(new message("[TourneyBase::sendInitialDataBefore][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
        }

        public override void requestInitHole(Player _session, packet _packet)
        {
            //REQUEST_BEGIN("InitHole");

            var p = new PangyaBinaryWriter();

            try
            {

                stInitHole ctx_hole = _packet.Read<stInitHole>();

                var hole = m_course.findHole(ctx_hole.numero);

                if (hole == null)
                {
                    throw new exception("[TourneyBase::requestInitHole][Error] course->findHole nao encontrou o hole retonou null, o server esta com erro no init course do tourney_base.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.TOURNEY_BASE,
                        2555, 0));
                }

                hole.init(ctx_hole.tee, ctx_hole.pin);

                INIT_PLAYER_INFO("requestInitHole",
                    "tentou inicializar o hole[NUMERO = " + Convert.ToString(hole.getNumero()) + "] no jogo",
                    _session, out PlayerGameInfo pgi);

                // Update Location Player in Hole
                pgi.location.x = ctx_hole.tee.x;
                pgi.location.z = ctx_hole.tee.z;

                // Número do hole atual, que o player está jogandp
                pgi.hole = ctx_hole.numero;

                // Flag que marca se o player já inicializou o primeiro hole do jogo
                if (!pgi.init_first_hole)
                {
                    pgi.init_first_hole = true;
                }

                // Gera degree para o player ou pega o degree sem gerar que é do modo do hole repeat
                pgi.degree = (m_ri.getModo() == RoomInfo.eMODO.M_REPEAT) ? hole.getWind().degree.getDegree() : hole.getWind().degree.getShuffleDegree();

                // Resposta de tempo do hole
                p.init_plain(0x9E);

                p.WriteUInt16(hole.getWeather());
                p.WriteByte(0); // Option do tempo, sempre peguei zero aqui dos pacotes que vi

                packet_func.session_send(p,
                    _session, 1);

                var wind_flag = initCardWindPlayer(pgi, hole.getWind().wind);

                // Resposta do vento do hole
                p.init_plain(0x5B);

                p.WriteByte(hole.getWind().wind + wind_flag);
                p.WriteByte((wind_flag < 0) ? 1 : 0); // Flag de card de vento, aqui é a qnd diminui o vento, 1 Vento azul
                p.WriteUInt16(pgi.degree);
                p.WriteByte(1); // Flag do vento, 1 Reseta o Vento, 0 soma o vento que nem o comando gm \wind do pangya original

                packet_func.session_send(p,
                    _session, 1);

                // envia tempo decorrido do Tourney
                // Resposta tempo percorrido do Tourney
                sendRemainTime(_session);

            }
            catch (exception e)
            {
                //_smp.message_pool.getInstance().push(new message("[TourneyBase::requestInitHole][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
        }

        public override bool requestFinishLoadHole(Player _session, packet _packet)
        {
            //REQUEST_BEGIN("FinishLoadHole");

            var p = new PangyaBinaryWriter();

            // Esse aqui é para Trocar Info da Sala
            // para colocar a sala no modo que pode entrar depois de ter começado
            bool ret = false;

            try
            {

                INIT_PLAYER_INFO("requestFinishLoadHole",
                    "tentou finalizar carregamento do hole no jogo",
                    _session, out PlayerGameInfo pgi);

                pgi.finish_load_hole = 1;

                if (pgi.enter_after_started == 1)
                {
                    // Add Player Score
                    p.init_plain(0x113);

                    p.WriteByte(9);
                    p.WriteByte(0);

                    p.WriteInt32(_session.m_oid);
                    p.WriteUInt32((uint)m_players.Count());

                    packet_func.game_broadcast(this,
                        p, 1);
                }

                // Resposta passa o oid do player que vai começa o Hole
                p.init_plain(0x53);

                p.WriteInt32(_session.m_oid);

                packet_func.session_send(p,
                    _session, 1);

            }
            catch (exception e)
            {

                //_smp.message_pool.getInstance().push(new message("[TourneyBase::requestFinishLoadHole][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }

            return ret;
        }

        public override void requestFinishCharIntro(Player _session, packet _packet)
        {
            //REQUEST_BEGIN("FinishCharIntro");

            var p = new PangyaBinaryWriter();

            try
            {

                INIT_PLAYER_INFO("requestFinishCharIntro",
                    "tentou finalizar intro do char no jogo",
                    _session, out PlayerGameInfo pgi);

                pgi.finish_char_intro = 1;

                // Zera todas as tacada num dos players se for camp normal se for short game coloca o n mero de tacadas inicial
                if (m_ri.natural.short_game == 1)
                { // Short Game

                    var hole = m_course.findHole(pgi.hole);

                    if (hole == null)
                    {
                        throw new exception("[TourneyBase::requestFinishCharIntro][Error] player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou finalizar intro do char, mas nao conseguiu encontrar o hole[NUMERO=" + Convert.ToString(pgi.hole) + "] no course do jogo. Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.TOURNEY_BASE,
                            30, 0));
                    }

                    switch (hole.getPar().par)
                    {
                        case 5:
                            pgi.data.tacada_num = 2;
                            break;
                        case 4:
                            pgi.data.tacada_num = 1;
                            break;
                        case 3:
                        default:
                            pgi.data.tacada_num = 0;
                            break;
                    }
                }
                else
                {
                    pgi.data.tacada_num = 0;
                }

                // Giveup Flag
                pgi.data.giveup = 0;

            }
            catch (exception e)
            {

                //_smp.message_pool.getInstance().push(new message("[TourneyBase::requestFinishCharIntro][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
        }

        public override void requestFinishHoleData(Player _session, packet _packet)
        {
            //REQUEST_BEGIN("FinishHoleData");

            try
            {

                UserInfoEx ui = new UserInfoEx();

                _packet.ReadBuffer(ref ui, Marshal.SizeOf(new UserInfo()));

                // aqui o cliente passa mad_conduta com o hole_in, trocados, mad_conduto <-> hole_in

                INIT_PLAYER_INFO("requestFinishHoleData",
                    "tentou finalizar hole dados no jogo",
                    _session, out PlayerGameInfo pgi);

                pgi.ui = ui;

                if (!(pgi.shot_sync.state_shot.display.acerto_hole))
                { // Terminou o Hole sem acerta ele, Give Up

                    // Ainda não colocara o give up, o outro pacote, coloca nesse(muito difícil, n o colocar só se estiver com bug)
                    if (!(pgi.data.giveup == 1))
                    {
                        pgi.data.giveup = 1;

                        // Incrementa o Bad Condute
                        pgi.data.bad_condute++;
                    }
                }

                // Aqui Salva os dados do Pgi, os best Chipin, Long putt e best drive(max distância)
                // Não sei se precisa de salvar aqui, já que estou salvando no pgi User Info
                pgi.progress.best_chipin = ui.best_chip_in;
                pgi.progress.best_long_puttin = ui.best_long_putt;
                pgi.progress.best_drive = ui.best_drive;

            }
            catch (exception e)
            {
                //_smp.message_pool.getInstance().push(new message("[TourneyBase::requestFinishHoleData][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
        }

        public override void requestInitShot(Player _session, packet _packet)
        {
            try
            {

                ShotDataEx sd = new ShotDataEx();

                // Power Shot
                #region Read Shot Sync Data
                sd.option = _packet.ReadUInt16();

                if (sd.option == 1)
                {
                    sd.power_shot.option = _packet.ReadByte();
                    sd.power_shot.decrease_power_shot = _packet.ReadInt32();
                    sd.power_shot.increase_power_shot = _packet.ReadInt32();
                }

                //READ SHOTDataBase primeiro, primeiro se lê a classe base, e depois a classe que herda.

                sd.bar_point[0] = _packet.ReadSingle();
                sd.bar_point[1] = _packet.ReadSingle();

                sd.ball_effect[0] = _packet.ReadSingle();
                sd.ball_effect[1] = _packet.ReadSingle();

                sd.acerto_pangya_flag = _packet.ReadByte();
                sd.special_shot.ulSpecialShot = _packet.ReadUInt32();
                sd.time_hole_sync = _packet.ReadUInt32();
                sd.mira = _packet.ReadSingle();

                sd.time_shot = _packet.ReadUInt32();
                sd.bar_point1 = _packet.ReadSingle();
                sd.club = _packet.ReadByte();

                sd.fUnknown[0] = _packet.ReadSingle();
                sd.fUnknown[1] = _packet.ReadSingle();

                sd.impact_zone_pixel = _packet.ReadSingle();

                sd.natural_wind[0] = _packet.ReadInt32();
                sd.natural_wind[1] = _packet.ReadInt32();

                //READ SHOTDATA
                sd.spend_time_game = _packet.ReadSingle();

                #endregion
                INIT_PLAYER_INFO("requestInitShot",
                    "tentou iniciar tacada no jogo",
                    _session, out PlayerGameInfo pgi);

                pgi.shot_data = sd;

            }
            catch (exception e)
            {

                //_smp.message_pool.getInstance().push(new message("[TourneyBase::requestInitShot][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
        }

        public override void requestSyncShot(Player _session, packet _packet)
        {
            //REQUEST_BEGIN("SyncShot");

            var p = new PangyaBinaryWriter();

            try
            {

                ShotSyncData ssd = new ShotSyncData();

                // game read request sync shot
                base.requestReadSyncShotData(_session,
                    _packet, ref ssd);

                // Request Calcule Shot Spinning Cube
                requestCalculeShotSpinningCube(_session, ssd); // esse não precisa verificar o usuário, por que em tourney só o próprio player que envia

                // Request Calcule Shot Coin
                requestCalculeShotCoin(_session, ssd); // esse não precisa verificar o usuário, por que em tourney só o próprio player que envia

                requestTranslateSyncShotData(_session, ssd);

                requestReplySyncShotData(_session);

            }
            catch (exception e)
            {

                //_smp.message_pool.getInstance().push(new message("[TourneyBase::requestSyncShot][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
        }

        public override void requestInitShotArrowSeq(Player _session, packet _packet)
        {
            //REQUEST_BEGIN("InitShotArrowSeq");

            var p = new PangyaBinaryWriter();

            try
            {

                byte count_seta = _packet.ReadUInt8();

                if (count_seta == 0)
                {
                    throw new exception("[TourneyBase::requestInitShotArrowSeq][Error] player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou inicializar as sequencia de setas, mas nao enviou nenhuma seta. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.TOURNEY_BASE,
                        5, 0));
                }

                List<uArrow> setas = new List<uArrow>();

                for (var i = 0u; i < count_seta; ++i)
                {
                    setas.Add(new uArrow(_packet.ReadUInt32()));
                }

            }
            catch (exception e)
            {

                //_smp.message_pool.getInstance().push(new message("[TourneyBase::requestInitShotArrowSeq][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
        }

        public override void requestShotEndData(Player _session, packet _packet)
        {
            //REQUEST_BEGIN("requestShotEndData");

            var p = new PangyaBinaryWriter();

            try
            {

                // ----------------- LEMBRETE --------------
                // Aqui vou usar para as tacadas do spinning cube que gera no course
                // --- Já estou usando o pacote no sync, por que preciso verificar uns valores lá ---
                var arrays = _packet.GetRemainingData;

                ShotEndLocationData seld = new ShotEndLocationData(_packet);

                INIT_PLAYER_INFO("requestShotEndData",
                    "tentou finalizar local da tacada no jogo",
                    _session, out PlayerGameInfo pgi);

                pgi.shot_data_for_cube = seld;

                // Resposta para Shot End Data
                p.init_plain(0x1F7);

                p.WriteUInt32(pgi.oid);
                p.WriteByte(pgi.hole);

                p.WriteBytes(arrays);

                packet_func.game_broadcast(this,
                    p, 1);
            }
            catch (exception e)
            {

                //_smp.message_pool.getInstance().push(new message("[TourneyBase::requestShotEndData][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
        }

        public override RetFinishShot requestFinishShot(Player _session, packet _packet)
        {
            //REQUEST_BEGIN("FinishShot");

            RetFinishShot ret = new RetFinishShot();

            try
            {

                // Request Init Cube Coin
                var cube = requestInitCubeCoin(_session, _packet);

                // Resposta para Finish Shot
                sendEndShot(_session, cube);

                ret.ret = checkEndShotOfHole(_session);

                if (ret.ret == 2)
                    ret.p = _session;

                INIT_PLAYER_INFO("requestFinishShot",
                    "tentou finalizar a tacada",
                   _session, out PlayerGameInfo pgi);

                // Limpa dados que usa para cada tacada
                clearDataEndShot(pgi);


            }
            catch (exception e)
            {

                //_smp.message_pool.getInstance().push(new message("[TourneyBase::requestFinishShot][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }

            return (ret);
        }

        public override void requestChangeMira(Player _session, packet _packet)
        {
            //REQUEST_BEGIN("ChangeMira");

            var p = new PangyaBinaryWriter();

            try
            {

                float mira = _packet.ReadFloat();

                INIT_PLAYER_INFO("requestChangeMira",
                    "tentou mudar a mira[MIRA=" + Convert.ToString(mira) + "] no jogo",
                   _session, out PlayerGameInfo pgi);

                pgi.location.r = mira;

                // Resposta para o Change mira
                p.init_plain(0x56);

                p.WriteUInt32(pgi.oid);
                p.WriteFloat(pgi.location.r);

                packet_func.session_send(p,
                    _session, 1);

            }
            catch (exception e)
            {

                //_smp.message_pool.getInstance().push(new message("[TourneyBase::requestChangeMira][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
        }

        public override void requestChangeStateBarSpace(Player _session, packet _packet)
        {
            //REQUEST_BEGIN("ChangeStateBarSpace");

            var p = new PangyaBinaryWriter();

            try
            {

                byte state = _packet.ReadUInt8();
                float point = _packet.ReadFloat();

                INIT_PLAYER_INFO("requestChangeStateBarSpace",
                    "tentou mudar o estado[STATE=" + Convert.ToString((ushort)state) + ", POINT=" + Convert.ToString(point) + "] da barra de espaco no jogo",
                   _session, out PlayerGameInfo pgi);

                if (!pgi.bar_space.setStateAndPoint(state, point))
                {
                    throw new exception("[TourneyBase::requestChangeStateBarSpace][Error] player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou mudar o estado da barra de espaco[STATE=" + Convert.ToString((ushort)state) + " POINT=" + Convert.ToString(point) + "] no jogo, mas o estado eh desconhecido, Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.TOURNEY_BASE,
                        5, 0));
                }

                if (state == 3)
                {
                    pgi.bar_space.setState(0); // Volta para 1 depois que taca, era esse meu comentário no antigo

                    pgi.tempo = 0; // Reseta o tempo
                }

            }
            catch (exception e)
            {

                //_smp.message_pool.getInstance().push(new message("[TourneyBase::requestChangeStateBarSpace][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
        }

        public override void requestActivePowerShot(Player _session, packet _packet)
        {
            //REQUEST_BEGIN("ActivePowerShot");

            var p = new PangyaBinaryWriter();

            try
            {

                byte ps = _packet.ReadUInt8();

                INIT_PLAYER_INFO("requestActivePowerShot",
                    "tentou ativar power shot, no jogo",
                   _session, out PlayerGameInfo pgi);

                pgi.power_shot = ps;

                if (ps == 1)//ps1
                {
                    Debug.WriteLine($"[TourneyBase::requestActivePowerShot] [Log]: ativou 1x power shot gauge");
                }
                else if (ps == 2)//ps2
                {
                    Debug.WriteLine($"[TourneyBase::requestActivePowerShot] [Log]: ativou 2x power shot gauge");
                }
                else
                 if (ps == 0)//desativado
                {
                    Debug.WriteLine($"[TourneyBase::requestActivePowerShot] [Log]: desativou power shot gauge");
                }
                // Resposta para Active Power Shot
                p.init_plain(0x58);

                p.WriteInt32(_session.m_oid);
                p.WriteByte(pgi.power_shot);

                packet_func.session_send(p,
                    _session, 1);

            }
            catch (exception e)
            {

                //_smp.message_pool.getInstance().push(new message("[TourneyBase::requestActivePowerShot][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
        }

        public override void requestChangeClub(Player _session, packet _packet)
        {
            //REQUEST_BEGIN("ChangeClub");

            var p = new PangyaBinaryWriter();

            try
            {

                byte club = _packet.ReadUInt8();

                INIT_PLAYER_INFO("requestChangeClub",
                    "tentou trocar taco no jogo",
                   _session, out PlayerGameInfo pgi);

                pgi.club = club;

                // Resposta para Change Club
                p.init_plain(0x59);

                p.WriteInt32(_session.m_oid);
                p.WriteByte(pgi.club);

                packet_func.session_send(p,
                    _session, 1);

            }
            catch (exception e)
            {

                //_smp.message_pool.getInstance().push(new message("[TourneyBase::requestChangeClub][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
        }

        public override void requestUseActiveItem(Player _session, packet _packet)
        {
            //REQUEST_BEGIN("UseActiveItem");

            var p = new PangyaBinaryWriter();

            try
            {

                uint item_typeid = _packet.ReadUInt32();

                INIT_PLAYER_INFO("requestUseActiveItem",
                    "tentou usar item ativo no jogo",
                   _session, out PlayerGameInfo pgi);

                if (item_typeid == 0)
                {
                    throw new exception("[TourneyBase::requestActiveItem][Error] player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou usar active item[TYPEID=" + Convert.ToString(item_typeid) + "] no jogo, mas o item_typeid eh invalido(zero). Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.TOURNEY_BASE,
                        7, 0));
                }

                var iffItem = sIff.getInstance().findCommomItem(item_typeid);

                if (iffItem == null)
                {
                    throw new exception("[TourneyBase::requestActiveItem][Error] Player[UID=" + Convert.ToString(_session.m_pi.uid) + " tentou usar active item[TYPEID=" + Convert.ToString(item_typeid) + "] no jogo, mas o item nao tem no IFF_STRUCT. Hacker ou Bug.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.TOURNEY_BASE,
                        77, 0));
                }

                if (sIff.getInstance().getItemGroupIdentify(item_typeid) != sIff.getInstance().ITEM || !sIff.getInstance().IsItemEquipable(item_typeid))
                {
                    throw new exception("[TourneyBase::requestActiveItem][Error] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou usar active item[TYPEID=" + Convert.ToString(item_typeid) + "] no jogo, mas o item nao eh equipavel(usar). Hacker ou Bug.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.TOURNEY_BASE,
                        78, 0));
                }

                // Verifica se tem algum card de tempo equipado com efeito de mulligan rose
                if (item_typeid == MULLIGAN_ROSE_TYPEID)
                {

                    // Card Special - Efeito mulligan rose == 32
                    if (_session.m_pi.v_cei.Count(_el =>
                    {
                        return (_el.parts_typeid == 0 && _el.parts_typeid == 0 && sIff.getInstance().getItemSubGroupIdentify22(_el._typeid) == 2 && _el.efeito == 32);
                    }) > 0)
                    {
                        var rand = new Random();
                        // Resposta para o Use Active Item
                        p.init_plain(0x5A);

                        p.WriteUInt32(item_typeid);
                        p.WriteInt32(rand.Next()); // Seed Rand Failure Active Item
                        p.WriteInt32(_session.m_oid);

                        packet_func.game_broadcast(this,
                            p, 1);

                        // Sai
                        return;

                    }

                }

                // Verifica se o player tem o item para usar
                var pWi = _session.m_pi.findWarehouseItemByTypeid(item_typeid);

                if (pWi == null)
                {
                    throw new exception("[TourneyBase::requestActiveItem][Error] player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou usar active item[TYPEID=" + Convert.ToString(item_typeid) + "] no jogo, mas ele nao tem esse item. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.TOURNEY_BASE,
                        8, 0));
                }

                var it = pgi.used_item.v_active.find(pWi._typeid);

                if (it.Key == 0)
                {
                    throw new exception("[TourneyBase::requestActiveItem][Error] player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou usar active item[TYPEID=" + Convert.ToString(item_typeid) + "] no jogo, mas ele nao equipou esse item. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.TOURNEY_BASE,
                        9, 0));
                }

                if (it.Value.count >= it.Value.v_slot.Count())
                {
                    throw new exception("[TourneyBase::requestActiveItem][Error] player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou usar active item[TYPEID=" + Convert.ToString(item_typeid) + "] no jogo, mas ele ja usou todos os item desse que ele equipou. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.TOURNEY_BASE,
                        10, 0));
                }

                // Add +1 ou countador
                it.Value.count++;

                // item que foi usado na tacada
                pgi.item_active_used_shot = pWi._typeid;

                // Resposta para o Use Active Item
                p.init_plain(0x5A);

                p.WriteUInt32(pWi._typeid);
                p.WriteInt32(new Random().Next()); // Seed Rand Failure Active Item
                p.WriteInt32(_session.m_oid);

                packet_func.game_broadcast(this,
                    p, 1);

            }
            catch (exception e)
            {

                //_smp.message_pool.getInstance().push(new message("[TourneyBase::requestUseActiveItem][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
        }

        public override void requestChangeStateTypeing(Player _session, packet _packet)
        {
            //REQUEST_BEGIN("ChangeStateTypeing");

            var p = new PangyaBinaryWriter();

            try
            {

                short typeing = _packet.ReadInt16();

                INIT_PLAYER_INFO("requestChangeStateTypeing",
                    "tentou mudar o estado de escrevendo no jogo",
                   _session, out PlayerGameInfo pgi);

                pgi.typeing = typeing;

                // Resposta para Change State Typeing 
                p.init_plain(0x5D);

                p.WriteInt32(_session.m_oid);
                p.WriteInt16(pgi.typeing);

                packet_func.session_send(p,
                    _session, 1);

            }
            catch (exception e)
            {

                //_smp.message_pool.getInstance().push(new message("[TourneyBase::requestChangeStateTypeing][Error] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
        }

        public override void requestMoveBall(Player _session, packet _packet)
        {
            //REQUEST_BEGIN("MoveBall");

            var p = new PangyaBinaryWriter();

            try
            {

                float x = _packet.ReadFloat();
                float y = _packet.ReadFloat();
                float z = _packet.ReadFloat();

                INIT_PLAYER_INFO("requestMoveBall",
                    "tentou recolocar a bola no jogo",
                   _session, out PlayerGameInfo pgi);

                pgi.location.x = x;
                pgi.location.y = y;
                pgi.location.z = z;

                // Add + 1 a tacada, já que ele recolocou em vez de tacar
                pgi.data.tacada_num++;

                // Resposta para Move Ball
                p.init_plain(0x60);

                p.WriteFloat(pgi.location.x);
                p.WriteFloat(pgi.location.y);
                p.WriteFloat(pgi.location.z);

                packet_func.session_send(p,
                    _session, 1);

            }
            catch (exception e)
            {

                //_smp.message_pool.getInstance().push(new message("[TourneyBase::requestMoveBall][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
        }

        public override void requestChangeStateChatBlock(Player _session, packet _packet)
        {
            //REQUEST_BEGIN("ChangeStateChatBlock");

            var p = new PangyaBinaryWriter();

            try
            {

                byte chat_block = _packet.ReadUInt8();

                INIT_PLAYER_INFO("requestChangeStateChatBlock",
                    "tentou mudar estado do chat block no jogo",
                   _session, out PlayerGameInfo pgi);

                pgi.chat_block = chat_block;

                // Resposta para Chat Block
                p.init_plain(0xAC);

                p.WriteInt32(_session.m_oid);
                p.WriteByte(pgi.chat_block);

                packet_func.session_send(p,
                    _session, 1);

            }
            catch (exception e)
            {

                //_smp.message_pool.getInstance().push(new message("[TourneyBase::requestChangeStateChatBlock][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
        }

        public override void requestActiveBooster(Player _session, packet _packet)
        {
            //REQUEST_BEGIN("ActiveBooster");

            var p = new PangyaBinaryWriter();

            try
            {

                float velocidade = _packet.ReadFloat();

                INIT_PLAYER_INFO("requestActiveBooster",
                    "tentou ativar Time Booster no jogo",
                   _session, out PlayerGameInfo pgi);

                if ((_session.m_pi.m_cap.premium_user))
                { // (não é)!PREMIUM USER

                    var pWi = _session.m_pi.findWarehouseItemByTypeid(TIME_BOOSTER_TYPEID);

                    if (pWi == null)
                    {
                        throw new exception("[TourneyBase::requestActiveBooster][Error] player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou ativar time booster, mas ele nao tem o item passive. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.TOURNEY_BASE,
                            11, 0));
                    }

                    if (pWi.STDA_C_ITEM_QNTD <= 0)
                    {
                        throw new exception("[TourneyBase::requestActiveBooster][Error] player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou ativar time booster, mas ele nao tem quantidade suficiente[VALUE=" + Convert.ToString(pWi.STDA_C_ITEM_QNTD) + ", REQUEST=1] do item de time booster.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.TOURNEY_BASE,
                            12, 0));
                    }

                    var it = pgi.used_item.v_passive.find(pWi._typeid);

                    if (it.Key == 0)
                    {
                        throw new exception("[TourneyBase::requestActiveBooster][Error] player[UID = " + Convert.ToString(_session.m_pi.uid) + "] tentou ativar time booster, mas ele nao tem ele no item passive usados do server. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.TOURNEY_BASE,
                            13, 0));
                    }

                    if ((short)it.Value.count >= pWi.STDA_C_ITEM_QNTD)
                    {
                        throw new exception("[TourneyBase::requestActiveBooster][Error] player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou ativar time booster, mas ele ja usou todos os time booster. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.TOURNEY_BASE,
                            14, 0));
                    }

                    // Add +1 ao item passive usado
                    it.Value.count++;

                }
                else
                { // Soma +1 no contador de counter item do booster do player e passive item

                    pgi.sys_achieve.incrementCounter(0x6C400075u);

                    pgi.sys_achieve.incrementCounter(0x6C400050u);
                }

                // Resposta para Active Booster
                p.init_plain(0xC7);

                p.WriteFloat(velocidade);
                p.WriteInt32(_session.m_oid);

                packet_func.session_send(p,
                    _session, 1);

            }
            catch (exception e)
            {
                //_smp.message_pool.getInstance().push(new message("[TourneyBase::requestActiveBooster][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
        }

        public override void requestActiveReplay(Player _session, packet _packet)
        {
            //REQUEST_BEGIN("ActiveReplay");

            var p = new PangyaBinaryWriter();

            try
            {

                uint _typeid = _packet.ReadUInt32();

                if (_typeid == 0)
                {
                    throw new exception("[TourneyBase::requestActiveReplay][Error] player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou ativar Replay[TYPEID=" + Convert.ToString(_typeid) + "], mas o typeid eh invalido(zero). Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.TOURNEY_BASE,
                        200, 0));
                }

                var pWi = _session.m_pi.findWarehouseItemByTypeid(_typeid);

                if (pWi == null)
                {
                    throw new exception("[TourneyBase::requestActiveReplay][Error] player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou ativar Replay[TYPEID=" + Convert.ToString(_typeid) + "], mas ele nao tem o item. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.TOURNEY_BASE,
                        201, 0));
                }

                if (pWi.STDA_C_ITEM_QNTD <= 0)
                {
                    throw new exception("[TourneyBase::requestActiveReplay][Error] player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou ativar Replay[TYPEID=" + Convert.ToString(_typeid) + "], mas ele nao tem quantidade suficiente[VALUE=" + Convert.ToString(pWi.STDA_C_ITEM_QNTD) + ", REQUEST=1] do item. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.TOURNEY_BASE,
                        202, 0));
                }

                // UPDATE ON SERVER AND DB
                stItem item = new stItem();

                item.type = 2;
                item._typeid = pWi._typeid;
                item.id = (int)pWi.id;
                item.qntd = 1;
                item.STDA_C_ITEM_QNTD = (ushort)(item.qntd * -1);

                if (item_manager.removeItem(item, _session) <= 0)
                {
                    throw new exception("[TourneyBase::requestActiveReplay][Error] player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou ativar Replay[TYPEID=" + Convert.ToString(_typeid) + "], nao conseguiu deletar ou atualizar qntd do item[TYPEID=" + Convert.ToString(item._typeid) + ", ID=" + Convert.ToString(item.id) + "]", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.TOURNEY_BASE,
                        203, 0));
                }

#if DEBUG
                //_smp.message_pool.getInstance().push(new message("[TourneyBase::requestActiveReplay][Log] player[UID=" + Convert.ToString(_session.m_pi.uid) + "] ativou replay e ficou com " + Convert.ToString(item.stat.qntd_dep) + " + fita(s)", type_msg.CL_FILE_LOG_AND_CONSOLE));
#endif // DEBUG

                // UPDATE ON GAME
                // Resposta para o Active Replay
                p.init_plain(0xA4);

                p.WriteUInt16((ushort)item.stat.qntd_dep);

                packet_func.session_send(p,
                    _session, 1);

            }
            catch (exception e)
            {

                //_smp.message_pool.getInstance().push(new message("[TourneyBase::requestActiveReplay][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
        }

        public override void requestActiveCutin(Player _session, packet _packet)
        {
            //REQUEST_BEGIN("ActiveCutin");

            var p = new PangyaBinaryWriter();

            try
            {

                stActiveCutin ac = _packet.Read<stActiveCutin>();

                Player s = null;

                if (ac.uid != _session.m_pi.uid || (s = findSessionByUID(ac.uid)) == null)
                {
                    throw new exception("[TourneyBase::requestActiveCutin][Error] player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou activar cutin[CHAR_TYPEID=" + Convert.ToString(ac.char_typeid) + ", TIPO=" + Convert.ToString(ac.tipo) + ", OPT=" + Convert.ToString(ac.opt) + ",  ACTIVE=" + Convert.ToString(ac.active) + "] de um player[UID=" + Convert.ToString(ac.uid) + "], mas o jogador nao esta no jogo. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.TOURNEY_BASE,
                        1, 0x5200101));
                }

                if (s.m_pi.ei.char_info == null)
                {
                    throw new exception("[TourneyBase::requestActiveCutin][Error] player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou activar cutin[CHAR_TYPEID=" + Convert.ToString(ac.char_typeid) + ", TIPO=" + Convert.ToString(ac.tipo) + ", OPT=" + Convert.ToString(ac.opt) + ",  ACTIVE=" + Convert.ToString(ac.active) + "] de um player[UID=" + Convert.ToString(ac.uid) + "], mas o jogador nao tem um character equipado. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.TOURNEY_BASE,
                        2, 0x5200102));
                }

                CutinInformation pCutin = null;

                // Cutin Padrão que o player equipa, quando o cliente envia o cutin type é que é efeito por roupas equipadas
                if (sIff.getInstance().getItemGroupIdentify(ac.char_typeid) == sIff.getInstance().CHARACTER && ac.active == 1)
                {

                    if (s.m_pi.ei.char_info._typeid != ac.char_typeid)
                    {
                        throw new exception("[TourneyBase::requestActiveCutin][Error] player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou activar cutin[CHAR_TYPEID=" + Convert.ToString(ac.char_typeid) + ", TIPO=" + Convert.ToString(ac.tipo) + ", OPT=" + Convert.ToString(ac.opt) + ",  ACTIVE=" + Convert.ToString(ac.active) + "] de um player[UID=" + Convert.ToString(ac.uid) + "], mas o character typeid passado nao eh igual ao equipado do player. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.TOURNEY_BASE,
                            4, 0x5200104));
                    }

                    WarehouseItemEx pWi = null;

                    var end = (s.m_pi.ei.char_info.cut_in.Length);

                    for (var i = 0u; i < end; ++i)
                    {

                        if (s.m_pi.ei.char_info.cut_in[i] > 0)
                        {

                            if ((pWi = _session.m_pi.findWarehouseItemById((int)s.m_pi.ei.char_info.cut_in[i])) != null)
                            {

                                if ((pCutin = sIff.getInstance().findCutinInfomation(pWi._typeid)) == null)
                                {
                                    throw new exception("[TourneyBase::requestActiveCutin][Error] player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou activar cutin[CHAR_TYPEID=" + Convert.ToString(ac.char_typeid) + ", TIPO=" + Convert.ToString(ac.tipo) + ", OPT=" + Convert.ToString(ac.opt) + ", ACTIVE=" + Convert.ToString(ac.active) + "] de um player[UID=" + Convert.ToString(ac.uid) + "], mas o jogador nao tem esse cutin[TYPEID=" + Convert.ToString(pWi._typeid) + ", ID=" + Convert.ToString(pWi.id) + "]. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.TOURNEY_BASE,
                                        3, 0x5200103));
                                }

                                if (pCutin.tipo.ulCondition == ac.tipo)
                                {
                                    break;
                                }
                                else if ((i + 1) == end)
                                {
                                    throw new exception("[TourneyBase::requestActiveCutin][Error] player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou activar cutin[CHAR_TYPEID=" + Convert.ToString(ac.char_typeid) + ", TIPO=" + Convert.ToString(ac.tipo) + ", OPT=" + Convert.ToString(ac.opt) + ",  ACTIVE=" + Convert.ToString(ac.active) + "] de um player[UID=" + Convert.ToString(ac.uid) + "], mas o jogador nao tem esse cutin[TYPEID=" + Convert.ToString(pWi._typeid) + ", ID=" + Convert.ToString(pWi.id) + "]. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.TOURNEY_BASE,
                                        3, 0x5200103));
                                }
                            }
                        }
                    }

                }
                else if (sIff.getInstance().getItemGroupIdentify(ac.char_typeid) == sIff.getInstance().SKIN && ac.active == 0)
                {

                    // Verificar se ele tem os itens para ativar esse Cutin

                    if ((pCutin = sIff.getInstance().findCutinInfomation(ac.char_typeid)) == null)
                    {
                        throw new exception("[TourneyBase::requestActiveCutin][Error] player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou activar cutin[CHAR_TYPEID=" + Convert.ToString(ac.char_typeid) + ", TIPO=" + Convert.ToString(ac.tipo) + ", OPT=" + Convert.ToString(ac.opt) + ",  ACTIVE=" + Convert.ToString(ac.active) + "] de um player[UID=" + Convert.ToString(ac.uid) + "], mas o jogador nao tem esse cutin[TYPEID=" + Convert.ToString(ac.char_typeid) + "]. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.TOURNEY_BASE,
                            3, 0x5200103));
                    }

                    // Esses que passa o cutin typeid, pode ativar com tipo 1 e 2, 1 PS e 2 PS

                }

                if (pCutin == null)
                {
                    throw new exception("[TourneyBase::requestActiveCutin][Error] player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou activar cutin[CHAR_TYPEID=" + Convert.ToString(ac.char_typeid) + ", TIPO=" + Convert.ToString(ac.tipo) + ", OPT=" + Convert.ToString(ac.opt) + ",  ACTIVE=" + Convert.ToString(ac.active) + "] de um player[UID=" + Convert.ToString(ac.uid) + "], mas o cution nao foi encontrado[TYPEID=" + Convert.ToString(ac.char_typeid) + "]. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.TOURNEY_BASE,
                        4, 0x5200104));
                }

                // Resposta para Active Cutin
                p.init_plain(0x18D);

                p.WriteByte(1); // OK

                p.WriteUInt32(pCutin._typeid);
                p.WriteUInt32(pCutin.sector);
                p.WriteUInt32(pCutin.tipo.ulCondition);

                p.WriteUInt32(pCutin.img[0].tipo);
                p.WriteUInt32(pCutin.img[1].tipo);
                p.WriteUInt32(pCutin.img[2].tipo);
                p.WriteUInt32(pCutin.img[3].tipo);

                p.WriteUInt32(pCutin.tempo);

                p.WriteStr(pCutin.img[0].sprite, 40);
                p.WriteStr(pCutin.img[1].sprite, 40);
                p.WriteStr(pCutin.img[2].sprite, 40);
                p.WriteStr(pCutin.img[3].sprite, 40);

                packet_func.session_send(p,
                    _session, 1);

                // No Modo GrandZodic, não envia Cutin, então envia o pacote18D com option 0(Uint8), e valor 3(Uint16)

            }
            catch (exception e)
            {

                //_smp.message_pool.getInstance().push(new message("[TourneyBase::requestActiveCutin][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                p.init_plain(0x18D);

                p.WriteByte(0); // OPT

                p.WriteUInt16(1); // Error

                packet_func.session_send(p,
                    _session, 1);
            }
        }

        public override void requestActiveRing(Player _session, packet _packet)
        {
            //REQUEST_BEGIN("ActiveRing");

            var p = new PangyaBinaryWriter();

            try
            {

                stRing r = _packet.Read<stRing>();

                if (r._typeid == 0)
                {
                    throw new exception("[TourneyBase::requestActiveRing][Error] player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou ativar Anel[TYPEID=" + Convert.ToString(r._typeid) + "], mas o typeid eh invalido(zero). Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.TOURNEY_BASE,
                        30, 0x330001));
                }

                var pWi = _session.m_pi.findWarehouseItemByTypeid(r._typeid);

                if (pWi == null)
                {
                    throw new exception("[TourneyBase::requestActiveRing][Error] player[UID = " + Convert.ToString(_session.m_pi.uid) + "] tentou ativar Anel[TYPEID = " + Convert.ToString(r._typeid) + "], mas ele nao tem o anel. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.TOURNEY_BASE,
                        31, 0x330002));
                }

                if (_session.m_pi.ei.char_info == null)
                {
                    throw new exception("[TourneyBase::requestActiveRing][Error] player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou ativar Anel[TYPEID=" + Convert.ToString(r._typeid) + "], mas ele nao esta com um Character equipado. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.TOURNEY_BASE,
                        32, 0x330003));
                }

                if (!_session.m_pi.ei.char_info.auxparts.Any(c => c == r._typeid))
                {
                    throw new exception("[TourneyBase::requestActiveRing][Error] player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou ativar Anel[TYPEID=" + Convert.ToString(r._typeid) + "], mas ele nao esta equipado com o anel. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.TOURNEY_BASE,
                        33, 0x330004));
                }

                // Adiciona o efeito que foi ativado
                checkEffectItemAndSet(_session, r._typeid);

                // Resposta para o cliente
                p.init_plain(0x237);

                p.WriteUInt32(0); // OK

                p.WriteUInt32(_session.m_pi.uid);

                p.WriteUInt32(r._typeid);
                p.WriteByte(r.efeito);

                packet_func.session_send(p,
                    _session, 1);

            }
            catch (exception e)
            {

                //_smp.message_pool.getInstance().push(new message("[TourneyBase::requestActiveRing][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                // Resposta Error
                p.init_plain(0x237);

                p.WriteUInt32((ExceptionError.STDA_SOURCE_ERROR_DECODE_TYPE(e.getCodeError()) == STDA_ERROR_TYPE.TOURNEY_BASE) ? ExceptionError.STDA_SYSTEM_ERROR_DECODE(e.getCodeError()) : 0x330000);

                packet_func.session_send(p,
                    _session, 1);
            }
        }

        public override void requestActiveRingGround(Player _session, packet _packet)
        {
            //REQUEST_BEGIN("ActiveRingGround");

            var p = new PangyaBinaryWriter();

            try
            {

                stRingGround rg = _packet.Read<stRingGround>();

                // Log para saber qual é o efeito 31(0x1F)
                if ((AbilityEffect)(rg.efeito) == AbilityEffect.UNKNOWN_31)
                {
                    //_smp.message_pool.getInstance().push(new message("[TourneyBase::requestActiveRingGround][Log] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] ativou o efeito 0x1F(31) com os itens[TYPEID_1=" + Convert.ToString(rg.ring[0]) + ", TYPEID_2=" + Convert.ToString(rg.ring[1]) + "] e OPTION=" + Convert.ToString(rg.option), type_msg.CL_FILE_LOG_AND_CONSOLE));
                }

                if (!rg.isValid())
                {
                    throw new exception("[TourneyBase::requestActiveRingGround][Error] player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou ativar Anel de Terreno[TYPE=" + Convert.ToString(rg.efeito) + ", RING[0]=" + Convert.ToString(rg.ring[0]) + ", RING[1]=" + Convert.ToString(rg.ring[1]) + ", OPTION=" + Convert.ToString(rg.option) + "], mas os typeid's nao sao validos. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.TOURNEY_BASE,
                        50, 0x340001));
                }

                if (_session.m_pi.ei.char_info == null)
                {
                    throw new exception("[TourneyBase::requestActiveRingGround][Error] player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou ativar Anel de Terreno[TYPE=" + Convert.ToString(rg.efeito) + ", RING[0]=" + Convert.ToString(rg.ring[0]) + ", RING[1]=" + Convert.ToString(rg.ring[1]) + ", OPTION=" + Convert.ToString(rg.option) + "], mas ele nao esta com um Character equipado. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.TOURNEY_BASE,
                        51, 0x340002));
                }

                if (sIff.getInstance().getItemGroupIdentify(rg.ring[0]) == sIff.getInstance().AUX_PART)
                { // Anel

                    var pRing = _session.m_pi.findWarehouseItemByTypeid(rg.ring[0]);

                    if (pRing == null)
                    {
                        throw new exception("[TourneyBase::requestActiveRingGround][Error] player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou ativar Anel de Terreno[TYPE=" + Convert.ToString(rg.efeito) + ", RING[0]=" + Convert.ToString(rg.ring[0]) + ", RING[1]=" + Convert.ToString(rg.ring[1]) + ", OPTION=" + Convert.ToString(rg.option) + "], mas ele nao tem o Anel[0]. hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.TOURNEY_BASE,
                            52, 0x340002));
                    }

                    if (!_session.m_pi.ei.char_info.auxparts.Any(c => c == rg.ring[0]))
                    {
                        throw new exception("[TourneyBase::requestActiveRingGround][Error] player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou ativar Anel de Terreno[TYPE=" + Convert.ToString(rg.efeito) + ", RING[0]=" + Convert.ToString(rg.ring[0]) + ", RING[1]=" + Convert.ToString(rg.ring[1]) + ", OPTION=" + Convert.ToString(rg.option) + "], mas ele nao esta com o Anel[0] equipado", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.TOURNEY_BASE,
                            53, 0x340003));
                    }

                    if (rg.ring[0] != rg.ring[1])
                    { // Ativou Habilidade em conjunto 2 aneis

                        var pRing2 = _session.m_pi.findWarehouseItemByTypeid(rg.ring[1]);

                        if (pRing2 == null)
                        {
                            throw new exception("[TourneyBase::requestActiveRingGround][Error] player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou ativar Anel de Terreno[TYPE=" + Convert.ToString(rg.efeito) + ", RING[0]=" + Convert.ToString(rg.ring[0]) + ", RING[1]=" + Convert.ToString(rg.ring[1]) + ", OPTION=" + Convert.ToString(rg.option) + "], mas ele nao tem o Anel[1]. hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.TOURNEY_BASE,
                                52, 0x340002));
                        }

                        if (!_session.m_pi.ei.char_info.auxparts.Any(c => c == rg.ring[1]))
                        {
                            throw new exception("[TourneyBase::requestActiveRingGround][Error] player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou ativar Anel de Terreno[TYPE=" + Convert.ToString(rg.efeito) + ", RING[0]=" + Convert.ToString(rg.ring[0]) + ", RING[1]=" + Convert.ToString(rg.ring[1]) + ", OPTION=" + Convert.ToString(rg.option) + "], mas ele nao esta com o Anel[1] equipado", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.TOURNEY_BASE,
                                53, 0x340003));
                        }
                    }

                }
                else if (sIff.getInstance().getItemGroupIdentify(rg.ring[0]) == sIff.getInstance().PART)
                { // Part

                    var pRing = _session.m_pi.findWarehouseItemByTypeid(rg.ring[0]);

                    if (pRing == null)
                    {
                        throw new exception("[TourneyBase::requestActiveRingGround][Error] player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou ativar Anel de Terreno[TYPE=" + Convert.ToString(rg.efeito) + ", RING[0]=" + Convert.ToString(rg.ring[0]) + ", RING[1]=" + Convert.ToString(rg.ring[1]) + ", OPTION=" + Convert.ToString(rg.option) + "], mas ele nao tem o Part[0]. hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.TOURNEY_BASE,
                            52, 0x340002));
                    }

                    if (!_session.m_pi.ei.char_info.auxparts.Any(c => c == rg.ring[0]))
                    {
                        throw new exception("[TourneyBase::requestActiveRingGround][Error] player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou ativar Anel de Terreno[TYPE=" + Convert.ToString(rg.efeito) + ", RING[0]=" + Convert.ToString(rg.ring[0]) + ", RING[1]=" + Convert.ToString(rg.ring[1]) + ", OPTION=" + Convert.ToString(rg.option) + "], mas ele nao esta com o Part[0] equipado", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.TOURNEY_BASE,
                            53, 0x340003));
                    }

                    if (rg.ring[0] != rg.ring[1])
                    { // Ativou Habilidade em conjunto 2 aneis

                        var pRing2 = _session.m_pi.findWarehouseItemByTypeid(rg.ring[1]);

                        if (pRing2 == null)
                        {
                            throw new exception("[TourneyBase::requestActiveRingGround][Error] player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou ativar Anel de Terreno[TYPE=" + Convert.ToString(rg.efeito) + ", RING[0]=" + Convert.ToString(rg.ring[0]) + ", RING[1]=" + Convert.ToString(rg.ring[1]) + ", OPTION=" + Convert.ToString(rg.option) + "], mas ele nao tem o Part[1]. hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.TOURNEY_BASE,
                                52, 0x340002));
                        }

                        if (!_session.m_pi.ei.char_info.auxparts.Any(c => c == rg.ring[1]))
                        {
                            throw new exception("[TourneyBase::requestActiveRingGround][Error] player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou ativar Anel de Terreno[TYPE=" + Convert.ToString(rg.efeito) + ", RING[0]=" + Convert.ToString(rg.ring[0]) + ", RING[1]=" + Convert.ToString(rg.ring[1]) + ", OPTION=" + Convert.ToString(rg.option) + "], mas ele nao esta com o Part[1] equipado", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.TOURNEY_BASE,
                                53, 0x340003));
                        }
                    }

                }
                else if (sIff.getInstance().getItemGroupIdentify(rg.ring[0]) == sIff.getInstance().MASCOT)
                {

                    var pMascot = _session.m_pi.findMascotByTypeid(rg.ring[0]);

                    if (pMascot == null)
                    {
                        throw new exception("[TourneyBase::requestActiveRingGround][Error] player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou ativar Anel de Terreno[TYPE=" + Convert.ToString(rg.efeito) + ", RING[0]=" + Convert.ToString(rg.ring[0]) + ", RING[1]=" + Convert.ToString(rg.ring[1]) + ", OPTION=" + Convert.ToString(rg.option) + "], mas ele nao tem o Mascot[0]. hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.TOURNEY_BASE,
                            52, 0x340002));
                    }

                    if (rg.ring[0] != rg.ring[1])
                    { // Ativou Habilidade em conjunto 2 aneis

                        var pPart2 = _session.m_pi.findWarehouseItemByTypeid(rg.ring[1]);

                        if (pPart2 == null)
                        {
                            throw new exception("[TourneyBase::requestActiveRingGround][Error] player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou ativar Anel de Terreno[TYPE=" + Convert.ToString(rg.efeito) + ", RING[0]=" + Convert.ToString(rg.ring[0]) + ", RING[1]=" + Convert.ToString(rg.ring[1]) + ", OPTION=" + Convert.ToString(rg.option) + "], mas ele nao tem o Part[1]. hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.TOURNEY_BASE,
                                52, 0x340002));
                        }

                        if (!_session.m_pi.ei.char_info.auxparts.Any(c => c == rg.ring[1]))
                        {
                            throw new exception("[TourneyBase::requestActiveRingGround][Error] player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou ativar Anel de Terreno[TYPE=" + Convert.ToString(rg.efeito) + ", RING[0]=" + Convert.ToString(rg.ring[0]) + ", RING[1]=" + Convert.ToString(rg.ring[1]) + ", OPTION=" + Convert.ToString(rg.option) + "], mas ele nao esta com o Part[1] equipado", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.TOURNEY_BASE,
                                53, 0x340003));
                        }
                    }
                }

                // Adiciona o efeito que foi ativado
                //checkEffectItemAndSet(_session, rg.ring[0]);
                setEffectActiveInShot(_session, enumToBitValue((AbilityEffect)rg.efeito));

                // Resposta para o Active Ring Terreno
                p.init_plain(0x266);

                p.WriteUInt32(0); // OK

                p.WriteBytes(rg.ToArray());

                p.WriteUInt32(_session.m_pi.uid);

                packet_func.session_send(p,
                    _session, 1);

            }
            catch (exception e)
            {

                //_smp.message_pool.getInstance().push(new message("[TourneyBase::requestActiveRingGround][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                // Resposta Error
                p.init_plain(0x266);

                p.WriteUInt32((ExceptionError.STDA_SOURCE_ERROR_DECODE_TYPE(e.getCodeError()) == STDA_ERROR_TYPE.TOURNEY_BASE) ? ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) : 0x340000);

                packet_func.session_send(p,
                    _session, 1);
            }
        }

        public override void requestActiveRingPawsRainbowJP(Player _session, packet _packet)
        {
            //REQUEST_BEGIN("ActiveRingPawsRainbowJP");

            var p = new PangyaBinaryWriter();

            try
            {

                // Efeito patinha não passa o TYPEID do item que ativou
                setEffectActiveInShot(_session, enumToBitValue(AbilityEffect.PAWS_ACCUMULATE));

                // Resposta para o Active Ring Paws Rainbow JP
                p.init_plain(0x27E);

                p.WriteUInt32(_session.m_pi.uid);

                packet_func.session_send(p,
                    _session, 1);

            }
            catch (exception e)
            {

                //_smp.message_pool.getInstance().push(new message("[TourneyBase::requestActiveRingPawsRainbowJP][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
        }

        public override void requestActiveRingPawsRingSetJP(Player _session, packet _packet)
        {
            //REQUEST_BEGIN("ActiveRingPawsRingSetJP");

            var p = new PangyaBinaryWriter();

            try
            {

                // Efeito patinha não passa o TYPEID do item que ativou
                setEffectActiveInShot(_session, enumToBitValue(AbilityEffect.PAWS_NOT_ACCUMULATE));

                // Resposta para o Active Ring Paws Ring Set JP
                p.init_plain(0x281);

                p.WriteUInt32(_session.m_pi.uid);

                packet_func.session_send(p,
                    _session, 1);

            }
            catch (exception e)
            {

                //_smp.message_pool.getInstance().push(new message("[TourneyBase::requestActiveRingPawsRingSetJP][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
        }

        public override void requestActiveRingPowerGagueJP(Player _session, packet _packet)
        {
            //REQUEST_BEGIN("ActiveRingPowerGagueJP");

            var p = new PangyaBinaryWriter();

            try
            {

                stRingPowerGagueJP rpg = _packet.Read<stRingPowerGagueJP>();

                if (!rpg.isValid())
                {
                    throw new exception("[TourneyBase::requestActiveRingPowerGagueJP][Error] player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou ativar Anel de Barra de PS [JP] [TYPE=" + Convert.ToString(rpg.efeito) + ", RING[0]=" + Convert.ToString(rpg.ring[0]) + ", RING[1]=" + Convert.ToString(rpg.ring[1]) + ", OPTION=" + Convert.ToString(rpg.option) + "], mas os typeid's nao sao validos. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.TOURNEY_BASE,
                        150, 0x390001));
                }

                if (_session.m_pi.ei.char_info == null)
                {
                    throw new exception("[TourneyBase::requestActiveRingPowerGagueJP][Error] player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou ativar Anel de Barra de PS [JP] [TYPE=" + Convert.ToString(rpg.efeito) + ", RING[0]=" + Convert.ToString(rpg.ring[0]) + ", RING[1]=" + Convert.ToString(rpg.ring[1]) + ", OPTION=" + Convert.ToString(rpg.option) + "], mas ele nao esta com um Character equipado. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.TOURNEY_BASE,
                        151, 0x390002));
                }

                var pRing = _session.m_pi.findWarehouseItemByTypeid(rpg.ring[0]);

                if (pRing == null)
                {
                    throw new exception("[TourneyBase::requestActiveRingPowerGagueJP][Error] player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou ativar Anel de Barra de PS [JP] [TYPE=" + Convert.ToString(rpg.efeito) + ", RING[0]=" + Convert.ToString(rpg.ring[0]) + ", RING[1]=" + Convert.ToString(rpg.ring[1]) + ", OPTION=" + Convert.ToString(rpg.option) + "], mas ele nao tem o Anel[0]. hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.TOURNEY_BASE,
                        152, 0x390002));
                }

                if (!_session.m_pi.ei.char_info.auxparts.Any(c => c == rpg.ring[0]))
                {
                    throw new exception("[TourneyBase::requestActiveRingPowerGagueJP][Error] player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou ativar Anel de Barra de PS [JP] [TYPE=" + Convert.ToString(rpg.efeito) + ", RING[0]=" + Convert.ToString(rpg.ring[0]) + ", RING[1]=" + Convert.ToString(rpg.ring[1]) + ", OPTION=" + Convert.ToString(rpg.option) + "], mas ele nao esta com o Anel[0] equipado", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.TOURNEY_BASE,
                        153, 0x390003));
                }

                if (rpg.ring[0] != rpg.ring[1])
                { // Ativou Habilidade em conjunto 2 aneis

                    var pRing2 = _session.m_pi.findWarehouseItemByTypeid(rpg.ring[1]);

                    if (pRing2 == null)
                    {
                        throw new exception("[TourneyBase::requestActiveRingPowerGagueJP][Error] player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou ativar Anel de Barra de PS [JP] [TYPE=" + Convert.ToString(rpg.efeito) + ", RING[0]=" + Convert.ToString(rpg.ring[0]) + ", RING[1]=" + Convert.ToString(rpg.ring[1]) + ", OPTION=" + Convert.ToString(rpg.option) + "], mas ele nao tem o Anel[1]. hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.TOURNEY_BASE,
                            152, 0x390002));
                    }

                    if (!_session.m_pi.ei.char_info.auxparts.Any(c => c == rpg.ring[1]))
                    {
                        throw new exception("[TourneyBase::requestActiveRingPowerGagueJP][Error] player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou ativar Anel de Barra de PS [JP] [TYPE=" + Convert.ToString(rpg.efeito) + ", RING[0]=" + Convert.ToString(rpg.ring[0]) + ", RING[1]=" + Convert.ToString(rpg.ring[1]) + ", OPTION=" + Convert.ToString(rpg.option) + "], mas ele nao esta com o Anel[1] equipado", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.TOURNEY_BASE,
                            153, 0x390003));
                    }
                }

                // Effect
                setEffectActiveInShot(_session, enumToBitValue(AbilityEffect.POWER_GAUGE_FREE));

                // Resposta para o Active Ring Power Gague JP
                p.init_plain(0x27F);

                p.WriteUInt32(_session.m_pi.uid);

                packet_func.session_send(p,
                    _session, 1);

            }
            catch (exception e)
            {

                //_smp.message_pool.getInstance().push(new message("[TourneyBase::requestActiveRingPowerGagueJP][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
        }

        public override void requestActiveRingMiracleSignJP(Player _session, packet _packet)
        {
            //REQUEST_BEGIN("ActiveRingMiracleSign");

            var p = new PangyaBinaryWriter();

            try
            {

                uint _typeid = _packet.ReadUInt32();

                if (_typeid == 0)
                {
                    throw new exception("[TourneyBase::requestActiveRingMiracleSignJP][Error] player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou ativar 'Anel'[TYPEID=" + Convert.ToString(_typeid) + "] Olho Magico JP, mas o typeid eh invalido(zero)", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.TOURNEY_BASE,
                        70, 0x350001));
                }

                WarehouseItemEx pWi = _session.m_pi.findWarehouseItemByTypeid(_typeid);

                if (pWi == null)
                {
                    throw new exception("[TourneyBase::requestActiveRingMiracleSignJP][Error] player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou ativar 'Anel'[TYPEID=" + Convert.ToString(_typeid) + "] Olho Magico JP, mas ele nao tem o 'Anel'. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.TOURNEY_BASE,
                        71, 0x350002));
                }

                if (_session.m_pi.ei.char_info == null)
                {
                    throw new exception("[TourneyBase::requestActiveRingMiracleSignJP][Error] player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou ativar 'Anel'[TYPEID=" + Convert.ToString(_typeid) + "] Olho Magico JP, mas ele nao esta com um Character equipado. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.TOURNEY_BASE,
                        72, 0x350003));
                }

                if (sIff.getInstance().getItemGroupIdentify(_typeid) == sIff.getInstance().AUX_PART)
                { // Anel

                    if (!_session.m_pi.ei.char_info.auxparts.Any(c => c == _typeid))
                    {
                        throw new exception("[TourneyBase::requestActiveRingMiracleSignJP][Error] player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou ativar 'Anel'[TYPEID=" + Convert.ToString(_typeid) + "] Olho Magico JP, mas ele nao esta com o Anel equipado. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.TOURNEY_BASE,
                            0x73, 0x350004));
                    }

                }
                else if (sIff.getInstance().getItemGroupIdentify(_typeid) == sIff.getInstance().PART)
                { // Part

                    if (_session.m_pi.ei.char_info.parts_typeid.Any(c => c == _typeid))
                    {
                        throw new exception("[TourneyBase::requestActiveRingMiracleSignJP][Error] player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou ativar 'Anel'[TYPEID=" + Convert.ToString(_typeid) + "] Olho Magico JP, mas ele nao esta com a Part equipado. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.TOURNEY_BASE,
                            74, 0x350005));
                    }

                } // else Item Passive, o item do assist, mas acho que ele n o chame esse, ele chama o proprio pacote dele

                // Effect
                setEffectActiveInShot(_session, enumToBitValue(AbilityEffect.MIRACLE_SIGN_RANDOM));

                // Resposta para o Active Ring Miracle Sign JP
                p.init_plain(0x280);

                p.WriteUInt32(0); // OK;

                p.WriteUInt32(_typeid);
                p.WriteUInt32(_session.m_pi.uid);

                packet_func.session_send(p,
                    _session, 1);

            }
            catch (exception e)
            {

                //_smp.message_pool.getInstance().push(new message("[TourneyBase::requestActiveRingMiracleSign][ErroSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                // Resposta Error
                p.init_plain(0x280);

                p.WriteUInt32((ExceptionError.STDA_SOURCE_ERROR_DECODE_TYPE(e.getCodeError()) == STDA_ERROR_TYPE.TOURNEY_BASE) ? ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) : 0x350000);

                packet_func.session_send(p,
                    _session, 1);
            }
        }

        public override void requestActiveWing(Player _session, packet _packet)
        {
            //REQUEST_BEGIN("ActiveWing");

            var p = new PangyaBinaryWriter();

            try
            {

                uint _typeid = _packet.ReadUInt32();

                if (_typeid == 0)
                {
                    throw new exception("[TourneyBase::ActiveWing][Error] player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou ativar Asa[TYPEID=" + Convert.ToString(_typeid) + "], mas o typeid eh invalido(zero)", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.TOURNEY_BASE,
                        90, 0x360001));
                }

                var pWi = _session.m_pi.findWarehouseItemByTypeid(_typeid);

                if (pWi == null)
                {
                    throw new exception("[TourneyBase::ActiveWing][Error] player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou ativar Asa[TYPEID=" + Convert.ToString(_typeid) + "], mas ele nao tem esse item 'Asa', Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.TOURNEY_BASE,
                        91, 0x360002));
                }

                if (_session.m_pi.ei.char_info == null)
                {
                    throw new exception("[TourneyBase::ActiveWing][Error] player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou ativar Asa[TYPEID=" + Convert.ToString(_typeid) + "], mas ele nao esta com um Character equipado. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.TOURNEY_BASE,
                        92, 0x360003));
                }

                if (_session.m_pi.ei.char_info.parts_typeid.Any(c => c == _typeid))
                {
                    throw new exception("[TourneyBase::ActiveWing][Error] player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou ativar Asa[TYPEID=" + Convert.ToString(_typeid) + "], mas ele nao esta com o item 'Asa' equipado. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.TOURNEY_BASE,
                        93, 0x360004));
                }

                // Adiciona o efeito que foi ativado
                checkEffectItemAndSet(_session, _typeid);

                // Resposta para o Active Wing
                p.init_plain(0x203);

                p.WriteUInt32(_session.m_pi.uid);

                p.WriteUInt32(_typeid);

                packet_func.session_send(p,
                    _session, 1);

            }
            catch (exception e)
            {

                //_smp.message_pool.getInstance().push(new message("[TourneyBase::ActiveWing][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
        }

        public override void requestActivePaws(Player _session, packet _packet)
        {
            //REQUEST_BEGIN("ActivePaws");

            var p = new PangyaBinaryWriter();

            try
            {

                // Efeito patinha não passa o TYPEID do item que ativou, Animal Ring(Anel) ou Patinha
                setEffectActiveInShot(_session, enumToBitValue(AbilityEffect.PAWS_NOT_ACCUMULATE));

                // Resposta para o Active Paws
                p.init_plain(0x236);

                p.WriteUInt32(_session.m_pi.uid);

                packet_func.session_send(p,
                    _session, 1);

            }
            catch (exception e)
            {

                //_smp.message_pool.getInstance().push(new message("[TourneyBase::requestActivePaws][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
        }

        public override void requestActiveGlove(Player _session, packet _packet)
        {
            //REQUEST_BEGIN("ActiveGlove");

            var p = new PangyaBinaryWriter();

            try
            {

                uint _typeid = _packet.ReadUInt32();

                if (_typeid == 0)
                {
                    throw new exception("[TourneyBase::requestActiveGlove][Error] player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou ativar Luva[TYPEID=" + Convert.ToString(_typeid) + "], mas o typeid eh invalido(zero). Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.TOURNEY_BASE,
                        110, 0x370001));
                }

                var pWi = _session.m_pi.findWarehouseItemByTypeid(_typeid);

                if (pWi == null)
                {
                    throw new exception("[TourneyBase::requestActiveGlove][Error] player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou ativar Luva[TYPEID=" + Convert.ToString(_typeid) + "], mas ele nao tem esse item 'Luva'. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.TOURNEY_BASE,
                        111, 0x370002));
                }

                if (_session.m_pi.ei.char_info == null)
                {
                    throw new exception("[TourneyBase::requestActiveGlove][Error] player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou ativar Luva[TYPEID=" + Convert.ToString(_typeid) + "], mas ele nao esta com um Character equipado. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.TOURNEY_BASE,
                        112, 0x370003));
                }

                if (sIff.getInstance().getItemGroupIdentify(_typeid) == sIff.getInstance().PART)
                { // Luva

                    if (!_session.m_pi.ei.char_info.parts_typeid.Any(c => c == _typeid))
                    {
                        throw new exception("[TourneyBase::requestActiveGlove][Error] player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou ativar Luva[TYPEID=" + Convert.ToString(_typeid) + "], mas ele nao tem a Luva equipada. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.TOURNEY_BASE,
                            113, 0x370004));
                    }

                }
                else if (sIff.getInstance().getItemGroupIdentify(_typeid) == sIff.getInstance().AUX_PART)
                { // Anel
                    if (!_session.m_pi.ei.char_info.auxparts.Any(c => c == _typeid))
                    {
                        throw new exception("[TourneyBase::requestActiveGlove][Error] player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou ativar Luva[TYPEID=" + Convert.ToString(_typeid) + "], mas ele nao tem o Anel equipado. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.TOURNEY_BASE,
                            114, 0x370005));
                    }
                }

                // Adiciona o efeito que foi ativado
                checkEffectItemAndSet(_session, _typeid);

                // Resposta para o Active Glove
                p.init_plain(0x265);

                p.WriteUInt32(0); // OK

                p.WriteUInt32(_typeid);

                p.WriteUInt32(_session.m_pi.uid);

                packet_func.session_send(p,
                    _session, 1);

            }
            catch (exception e)
            {

                //_smp.message_pool.getInstance().push(new message("[TourneyBase::requestActiveGlove][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                // Resposta Error
                p.init_plain(0x265);

                p.WriteUInt32((ExceptionError.STDA_SOURCE_ERROR_DECODE_TYPE(e.getCodeError()) == STDA_ERROR_TYPE.TOURNEY_BASE) ? ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) : 0x370000);

                packet_func.session_send(p,
                    _session, 1);
            }
        }

        public override void requestActiveEarcuff(Player _session, packet _packet)
        {
            //REQUEST_BEGIN("ActiveEarcuff");

            var p = new PangyaBinaryWriter();

            try
            {

                stEarcuff ec = _packet.Read<stEarcuff>();

                if (ec._typeid == 0)
                {
                    throw new exception("[TourneyBase::ActiveEarcuff][Error] player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou ativar Earcuff'Mascot'[TYPEID=" + Convert.ToString(ec._typeid) + ", ANGLE_SENTIDO=" + Convert.ToString((ushort)ec.angle) + ", X_ANGLE=" + Convert.ToString(ec.x_point_angle) + "], mas o typeid eh invalido. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.TOURNEY_BASE,
                        130, 0x380001));
                }

                if (sIff.getInstance().getItemGroupIdentify(ec._typeid) == sIff.getInstance().PART)
                { // Earcuff

                    if (_session.m_pi.ei.char_info == null)
                    {
                        throw new exception("[TourneyBase::ActiveEarcuff][Error] player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou ativar Earcuff[TYPEID=" + Convert.ToString(ec._typeid) + ", ANGLE_SENTIDO=" + Convert.ToString((ushort)ec.angle) + ", X_ANGLE=" + Convert.ToString(ec.x_point_angle) + "], mas ele nao esta com um Character equipado. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.TOURNEY_BASE,
                            131, 0x380002));
                    }

                    var pWi = _session.m_pi.findWarehouseItemByTypeid(ec._typeid);

                    if (pWi == null)
                    {
                        throw new exception("[TourneyBase::ActiveEarcuff][Error] player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou ativar Earcuff[TYPEID=" + Convert.ToString(ec._typeid) + ", ANGLE_SENTIDO=" + Convert.ToString((ushort)ec.angle) + ", X_ANGLE=" + Convert.ToString(ec.x_point_angle) + "], mas ele nao tem o Part. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.TOURNEY_BASE,
                            132, 0x380003));
                    }
                    if (!_session.m_pi.ei.char_info.parts_typeid.Any(c => c == ec._typeid))
                    {
                        throw new exception("[TourneyBase::ActiveEarcuff][Error] player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou ativar Earcuff[TYPEID=" + Convert.ToString(ec._typeid) + ", ANGLE_SENTIDO=" + Convert.ToString((ushort)ec.angle) + ", X_ANGLE=" + Convert.ToString(ec.x_point_angle) + "], mas ele nao esta com o Part equipado. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.TOURNEY_BASE,
                            133, 0x380004));
                    }

                }
                else if (sIff.getInstance().getItemGroupIdentify(ec._typeid) == sIff.getInstance().MASCOT)
                { // Mascot Dragon

                    var pMi = _session.m_pi.findMascotByTypeid(ec._typeid);

                    if (pMi == null)
                    {
                        throw new exception("[TourneyBase::ActiveEarcuff][Error] player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou ativar Earcuff[TYPEID=" + Convert.ToString(ec._typeid) + ", ANGLE_SENTIDO=" + Convert.ToString((ushort)ec.angle) + ", X_ANGLE=" + Convert.ToString(ec.x_point_angle) + "], mas ele nao tem esse Mascot. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.TOURNEY_BASE,
                            134, 0x380005));
                    }

                    if (_session.m_pi.ei.mascot_info == null)
                    {
                        throw new exception("[TourneyBase::ActiveEarcuff][Error] player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou ativar Earcuff'Mascot'[TYPEID=" + Convert.ToString(ec._typeid) + ", ANGLE_SENTIDO=" + Convert.ToString((ushort)ec.angle) + ", X_ANGLE=" + Convert.ToString(ec.x_point_angle) + "], mas ele nao esta com o Mascot equipado. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.TOURNEY_BASE,
                            135, 0x380006));
                    }
                }

#if DEBUG
                //_smp.message_pool.getInstance().push(new message("[TourneyBase::requestActiveEarcuff][Log] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] Typeid=" + Convert.ToString(ec._typeid) + ", ANG_DIRECTION=" + Convert.ToString((ushort)ec.angle) + ", ANG=" + Convert.ToString(ec.x_point_angle), type_msg.CL_FILE_LOG_AND_CONSOLE));
#endif // DEBUG

                INIT_PLAYER_INFO("requestActiveEarcuff",
                    "tentou ativar o efeito earcuff de direcao de vento",
                   _session, out PlayerGameInfo pgi);

                // Effect
                setEffectActiveInShot(_session, enumToBitValue(AbilityEffect.EARCUFF_DIRECTION_WIND));


                // Resposta para o Active Earcuff
                p.init_plain(0x24C);

                p.WriteUInt32(0); // OK

                p.WriteUInt32(ec._typeid);

                p.WriteUInt32(_session.m_pi.uid);

                p.WriteByte(ec.angle);

                p.WriteFloat(ec.x_point_angle);

                packet_func.session_send(p,
                    _session, 1);

            }
            catch (exception e)
            {

                //_smp.message_pool.getInstance().push(new message("[TourneyBase::requestActiveEarcuff][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                // Resposta Error
                p.init_plain(0x24C);

                p.WriteUInt32((ExceptionError.STDA_SOURCE_ERROR_DECODE_TYPE(e.getCodeError()) == STDA_ERROR_TYPE.TOURNEY_BASE) ? ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) : 0x380000);

                packet_func.session_send(p,
                    _session, 1);
            }
        }

        public override void requestUpdateTrofel()
        {

            uint soma = 0u;

            m_player_info.ToList().ForEach(_el =>
            {
                if (_el.Key != null)
                {
                    soma += (_el.Value.level > 60) ? 60 : (uint)(_el.Value.level > 0 ? _el.Value.level - 1 : 0);
                }
            });

            uint new_trofel = STDA_MAKE_TROFEL(soma, (int)m_player_info.Count());

            // Check se o trofeu anterior era o GM e se o novo não é mais, aí tira a flag de GM da sala
            if (m_ri.trofel == TROFEL_GM_EVENT_TYPEID && new_trofel != TROFEL_GM_EVENT_TYPEID)
            {
                m_ri.flag_gm = 0;
            }

            if (new_trofel > 0 && new_trofel != m_ri.trofel)
            {
                m_ri.trofel = new_trofel;
            }
        }

        public override void requestSendTimeGame(Player _session)
        {
            //CHECK_SESSION_BEGIN("requestSendTimeGame");

            var p = new PangyaBinaryWriter();

            try
            {

                if (isGamingBefore(_session.m_pi.uid))
                {
                    throw new exception("[TourneyBase::requestSendTimeGame][Error] player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou entrar na sala[NUMERO=" + Convert.ToString(m_ri.numero) + "] ja em jogo, mas o player ja tinha jogado nessa sala e saiu, e nao pode mais entrar.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.TOURNEY_BASE,
                        2703, 6));
                }

                p = new PangyaBinaryWriter((ushort)0x113);

                p.WriteByte(3); // Remain Time of Game
                p.WriteByte(0);

                p.WriteUInt16(m_ri.numero);

                var remain_time = UtilTime.GetLocalDateDiff(m_start_time);

                if (remain_time > 0)
                {
                    remain_time /= STDA_10_MICRO_PER_MILLI; // Miliseconds
                }

                p.WriteUInt32((uint)remain_time);
                p.WriteUInt32(m_ri.time_30s);

                p.WriteBytes(m_ri.ToArray());

                packet_func.session_send(p,
                    _session, 0);

            }
            catch (exception e)
            {

                //_smp.message_pool.getInstance().push(new message("[TourneyBase::requestSendTimeGame][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                // Resposta erro
                p.init_plain(0x113);

                p.WriteByte(6); // Option Error

                // Error Code
                p.WriteByte((byte)((ExceptionError.STDA_SOURCE_ERROR_DECODE_TYPE(e.getCodeError()) == STDA_ERROR_TYPE.TOURNEY_BASE) ? ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) : 1));

                packet_func.session_send(p,
                    _session, 1);
            }
        }
        public override void requestUpdateEnterAfterStartedInfo(Player _session, EnterAfterStartInfo _easi)
        {
            //CHECK_SESSION_BEGIN("requestUpdateEnterAfterStartedInfo");

            var p = new PangyaBinaryWriter();

            try
            {

                if (_session.m_oid != _easi.owner_oid)
                {
                    throw new exception("[TourneyBase::requestUpdateEnterAfterStartedInfo][Error] player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou atualizar info depois de entrar na sala[NUMERO=" + Convert.ToString(m_ri.numero) + "] que ja tinha comecado, mas os oid[owner=" + Convert.ToString(_session.m_oid) + ", owner=" + Convert.ToString(_easi.owner_oid) + "] nao bate. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.TOURNEY_BASE,
                        2708, 1));
                }

                var s = findSessionByOID(_easi.request_oid);

                if (s == null)
                {
                    throw new exception("[TourneyBase::requestUpdateEnterAfterStartedInfo][Error] player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou atualizar info depois de entrar na sala[NUMERO=" + Convert.ToString(m_ri.numero) + "] que ja tinha comecado, mas o player[OID=" + Convert.ToString(_easi.request_oid) + "] nao esta no jogo. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.TOURNEY_BASE,
                        2709, 1));
                }

                INIT_PLAYER_INFO("requestUpdateEnterAfterStartedInfo",
                    "tentou atualizar info depois de entar na sala[NUMERO=" + Convert.ToString(m_ri.numero) + "] no jogo",
                   _session, out PlayerGameInfo pgi);

                p = new PangyaBinaryWriter((ushort)0x113);

                p.WriteByte(10); // Update Info Scores
                p.WriteByte(0);

                p.WriteInt32(_session.m_oid);
                p.WriteByte((byte)pgi.data.total_tacada_num);
                p.WriteByte(pgi.hole);
                p.WriteInt32(pgi.data.score);
                p.WriteUInt64(pgi.data.pang);

                p.WriteBytes(_easi.ToArray());

                packet_func.session_send(p,
                    _session, 1);
                packet_func.session_send(p,
                    s, 1);

            }
            catch (exception e)
            {

                //_smp.message_pool.getInstance().push(new message("[TourneyBase::requestUpdateEnterAfterStartedInfo][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                // Resposta erro
                p.init_plain(0x113);

                p.WriteByte(6); // Option Error

                // Error Code
                p.WriteByte((byte)((ExceptionError.STDA_SOURCE_ERROR_DECODE_TYPE(e.getCodeError()) == STDA_ERROR_TYPE.TOURNEY_BASE) ? ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) : 1));

                packet_func.session_send(p,
                    _session, 1);
            }
        }
        public override bool requestFinishGame(Player _session, packet _packet)
        {
            //REQUEST_BEGIN("FinishGame");

            var p = new PangyaBinaryWriter();

            bool ret = false;

            try
            {

                UserInfoEx ui = new UserInfoEx();
                _packet.ReadBuffer(ref ui, Marshal.SizeOf(new UserInfo()));
                // aqui o cliente passa mad_conduta com o hole_in, trocados, mad_conduto <-> hole_in

                INIT_PLAYER_INFO("requestFinishGame",
                    "tentou terminar o jogo",
                   _session, out PlayerGameInfo pgi);

                pgi.ui = ui;

                // Packet06
                ret = finish_game(_session, 6); 
            }
            catch (exception e)
            {

                //_smp.message_pool.getInstance().push(new message("[TourneyBase::requestFinishGame][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }

            return ret;
        }

        public void startTime()
        {
            try
            {
                if (m_timer != null)
                    stopTime();

                m_timer = new PangyaTimer(m_ri.time_30s); // tempo em ms
                //m_timer.OnTimeFinish = () => end_time(this, null);
                //m_timer.Start();

                var minutos = m_ri.time_30s > 0 ? m_ri.time_30s / 60000 : 0;
                //_smp.message_pool.getInstance().push(new message(
                    //$"[TourneyBase::startTime][Log] Criou o Timer [Tempo={minutos}min, STATE={m_timer.getState()}]",
                    //type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
            catch (Exception e)
            {
                //_smp.message_pool.getInstance().push(new message(
                    //$"[TourneyBase::startTime][ErrorSystem] {e.Message}",
                    //type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
        }

        protected override void requestTranslateSyncShotData(Player _session, ShotSyncData _ssd)
        {
            //CHECK_SESSION_BEGIN("requestTranslateSyncShotData");

            try
            {

                var s = findSessionByOID(_ssd.oid);

                if (s == null)
                {
                    throw new exception("[TourneyBase::requestTranslateSyncShotData][Error] player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou sincronizar tacada do player[OID=" + Convert.ToString(_ssd.oid) + "], mas o player nao existe nessa jogo. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.VERSUS_BASE,
                        200, 0));
                }

                // Update Sync Shot Player
                if (_session.m_pi.uid == s.m_pi.uid)
                {

                    INIT_PLAYER_INFO("requestTranslateSyncShotData",
                        "tentou sincronizar a tacada no jogo",
                       _session, out PlayerGameInfo pgi);

                    pgi.shot_sync = _ssd;

                    // Last Location Player
                    var last_location = pgi.location;

                    // Update Location Player
                    pgi.location.x = _ssd.location.x;
                    pgi.location.z = _ssd.location.z;

                    // Update Pang and Bonus Pang
                    pgi.data.pang = _ssd.pang;
                    pgi.data.bonus_pang = _ssd.bonus_pang;

                    // Já só na função que come a o tempo do player do turno
                    pgi.data.tacada_num++;

                    if (_ssd.state == ShotSyncData.SHOT_STATE.OUT_OF_BOUNDS || _ssd.state == ShotSyncData.SHOT_STATE.UNPLAYABLE_AREA)
                    {
                        pgi.data.tacada_num++;
                    }

                    var hole = m_course.findHole(pgi.hole);

                    if (hole == null)
                    {
                        throw new exception("[TourneyBase::requestTranslateSyncShotData][Error] player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou sincronizar tacada no hole[NUMERO=" + Convert.ToString((ushort)pgi.hole) + "], mas o numero do hole is invalid. Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.VERSUS_BASE,
                            12, 0));
                    }

                    // Conta já a próxima tacada, no give up
                    if (!(_ssd.state_shot.display.acerto_hole) && hole.getPar().total_shot <= (pgi.data.tacada_num + 1))
                    {

                        // +1 que é giveup, só add se n o passou o número de tacadas
                        if (pgi.data.tacada_num < hole.getPar().total_shot)
                        {
                            pgi.data.tacada_num++;
                        }

                        pgi.data.giveup = 1;

                        // Soma +1 no Bad Condute
                        pgi.data.bad_condute++;
                    }

                    // aqui os achievement de power shot int32_t putt beam impact e etc
                    update_sync_shot_achievement(_session, last_location);
                }

            }
            catch (exception e)
            {

                //_smp.message_pool.getInstance().push(new message("[TourneyBase::requestTranslateSyncShotData][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
        }

        protected override void requestReplySyncShotData(Player _session)
        {
            //CHECK_SESSION_BEGIN("requestReplySyncShotData");

            try
            {

                drawDropItem(_session);

                // Resposta Sync Shot
                sendSyncShot(_session);

            }
            catch (exception e)
            {

                //_smp.message_pool.getInstance().push(new message("[TourneyBase::requestReplySyncShotData][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
        }

        public virtual void sendRemainTime(Player _session)
        {

            var remain_time = UtilTime.GetLocalDateDiff(m_start_time);

            if (remain_time > 0)
            {
                remain_time /= STDA_10_MICRO_PER_MILLI;
            }

            // Resposta tempo percorrido do Tourney
            PangyaBinaryWriter p = new PangyaBinaryWriter((ushort)0x8D);

            p.WriteUInt32((uint)13000);

            packet_func.session_send(p,
                _session, 1);
        }
        public virtual void updateFinishHole(Player _session, int _option)
        {

            INIT_PLAYER_INFO("updateFinishHole",
                "tentou terminar o hole no jogo",
               _session, out PlayerGameInfo pgi);

            var p = new PangyaBinaryWriter((ushort)0x6D);

            p.WriteInt32(_session.m_oid);
            p.WriteByte(pgi.hole);
            p.WriteByte((byte)pgi.data.total_tacada_num);
            p.WriteInt32(pgi.data.score);
            p.WriteUInt64(pgi.data.pang);
            p.WriteUInt64(pgi.data.bonus_pang);
            p.WriteByte((byte)_option); // 1 - Terminou o Hole, 0 - N o terminou o Hole

            packet_func.game_broadcast(this,
                p, 1);
        }
        public void updateTreasureHunterPoint(Player _session)
        {

            INIT_PLAYER_INFO("updateTreasureHunterPoint",
                "tentou atualizar os pontos do Treasure Hunter no jogo",
               _session, out PlayerGameInfo pgi);


            if (!sTreasureHunterSystem.getInstance().isLoad())
            {
                sTreasureHunterSystem.getInstance().load();
            }

            var hole = m_course.findHole(pgi.hole);

            if (hole == null)
            {
                throw new exception("[TourneyBase::updateTreasureHunterPoint][Error] player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou atualizar os pontos do Treasure Hunter no hole[NUMERO=" + Convert.ToString((ushort)pgi.hole) + "], mas o hole nao existe. Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.TOURNEY_BASE,
                    30, 0));
            }

            // Calcule Treasure Pontos
            if (m_ri.getTipo() == RoomInfo.TIPO.SPECIAL_SHUFFLE_COURSE)
            {

                pgi.thi.treasure_point += sTreasureHunterSystem.getInstance().calcPointSSC(pgi.data.tacada_num, hole.getPar().par) + pgi.thi.getPoint(pgi.data.tacada_num, hole.getPar().par);
            }
            else
            {
                pgi.thi.treasure_point += sTreasureHunterSystem.getInstance().calcPointNormal(pgi.data.tacada_num, hole.getPar().par) + pgi.thi.getPoint(pgi.data.tacada_num, hole.getPar().par);
            }

            // Mostra score board
            var p = new PangyaBinaryWriter((ushort)0x132);

            p.WriteUInt32(pgi.thi.treasure_point);

            // No Modo Match passa outro valor tbm

            packet_func.session_send(p,
                _session, 1);
        }

        public virtual void requestDrawTreasureHunterItem(Player _session)
        {

            // Sorteia os itens ganho do Treasure ponto do player

            if (!sTreasureHunterSystem.getInstance().isLoad())
            {
                sTreasureHunterSystem.getInstance().load();
            }

            INIT_PLAYER_INFO("requestDrawTreasureHunterItem",
                "tentou sortear os item(ns) do Treasure Hunter do jogo",
               _session, out PlayerGameInfo pgi);

            // Guarda os item(ns) ganho no treasure hunter system, no Player Game Info, para poder consultar ele depois

            pgi.thi.v_item = sTreasureHunterSystem.getInstance().drawItem(pgi.thi.treasure_point, (byte)((int)m_ri.course & 0x7F));

            if (pgi.thi.v_item.empty())
            {
                //_smp.message_pool.getInstance().push(new message("[TourneyBase::requestDrawTreasureHunterItem][WARNING] player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou sortear os item(ns) do Treasure Hunter do jogo," + "mas o Treasure Hunter Item nao conseguiu sortear nenhum item", type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
        }

        public void sendSyncShot(Player _session)
        {

            INIT_PLAYER_INFO("sendSyncShot",
                "tentou sincronizar a tacada do jogador no jogo",
               _session, out PlayerGameInfo pgi);

            var p = new PangyaBinaryWriter((ushort)0x6E);

            p.WriteInt32(pgi.shot_sync.oid);

            p.WriteByte(pgi.hole);

            p.WriteFloat(pgi.location.x);
            p.WriteFloat(pgi.location.z);

            p.WriteUInt32(pgi.shot_sync.state_shot.shot.ulState);

            // No Modo de jogo Approach aqui tem mais 2 uint32_t, com tempo da tacada e a dist ncia do hole

            p.WriteUInt16(pgi.shot_sync.tempo_shot);

            packet_func.game_broadcast(this,
                p, 1);
        }
        public void sendEndShot(Player _session, DropItemRet _cube)
        {

            var p = new PangyaBinaryWriter((ushort)0xCC);

            p.WriteInt32(_session.m_oid);

            // Count, Coin/Cube "Drop"
            p.WriteByte(_cube.v_drop.Count());

            if (!_cube.v_drop.empty())
            {
                foreach (var el in _cube.v_drop)
                {
                    p.WriteBytes(el.ToArray());
                }

                // Aqui o server passa 128 itens de drop, os que dropou e o resto vazio
                if (_cube.v_drop.Count() < 128)
                {
                    p.WriteZeroByte((128 - _cube.v_drop.Count()) * 16);
                }
            }

            packet_func.session_send(p,
                _session, 1);
        }

        public void sendUpdateState(Player _session, int _option)
        {

            var p = new PangyaBinaryWriter((ushort)0x6C);

            p.WriteInt32(_session.m_oid);

            p.WriteByte((byte)_option); // 2 Terminou, 3 Saiu

            packet_func.game_broadcast(this,
                p, 1);
        }
        public void sendDropItem(Player _session)
        {

            INIT_PLAYER_INFO("sendDropItem",
                "tentou enviar os itens dropado do player no jogo",
               _session, out PlayerGameInfo pgi);

            var p = new PangyaBinaryWriter((ushort)0xCE);

            p.WriteByte(0); // OK

            p.WriteUInt16((ushort)pgi.drop_list.v_drop.Count());

            foreach (var el in pgi.drop_list.v_drop)
            {
                p.WriteUInt32(el._typeid);
            }

            packet_func.session_send(p,
                _session, 1);
        }
        public virtual void sendPlacar(Player _session)
        {

            INIT_PLAYER_INFO("sendPlacar",
                "tentou enviar o placar do jogo",
               _session, out PlayerGameInfo pgi);

            var p = new PangyaBinaryWriter((ushort)0x79);

            p.WriteUInt32(pgi.data.exp);

            p.WriteUInt32(m_ri.trofel);

            p.WriteByte(pgi.trofel); // Trofel Que o Player Ganhou
            p.WriteByte((byte)pgi.team); // Team Win, 0 - vermelho, 1 - Azul, 2 nenhum

            // Medalhas 
            for (var i = 0u; i < (m_medal.Length); ++i)
            {
                p.WriteBytes(m_medal[i].ToArray());
            }

            // N o sei se   a geral ou se   s  a do Tourney, (DEIXEI A GERAL) todas as medalhas que ele tem
            p.WriteBytes(_session.m_pi.ui.medal.ToArray());

            packet_func.session_send(p,
                _session, 1);
        }
        public void sendTreasureHunterItemDrawGUI(Player _session)
        {

            INIT_PLAYER_INFO("sendTreasureHunterItemDrawGUI",
                "tentou enviar os itens ganho no Treasure Hunter(so o Visual) do jogo",
               _session, out PlayerGameInfo pgi);

            var p = new PangyaBinaryWriter((ushort)0x133);

            p.WriteByte((byte)pgi.thi.v_item.Count());

            // No VS aqui os itens s o dividido entres os players do versus
            foreach (var el in pgi.thi.v_item)
            {
                p.WriteUInt32(pgi.uid); // UID do player que ganhou o item
                p.WriteUInt32(el._typeid);
                p.WriteUInt16((ushort)el.qntd);
                p.WriteByte(0); // Acho que sej  op  o ou dizendo que acabou o struct de Treasure Hunter Item Draw
            }

            packet_func.session_send(p,
                _session, 1);
        }
        public void sendTimeIsOver(Player _session)
        {

            var p = new PangyaBinaryWriter((ushort)0x8C);

            packet_func.session_send(p,
                _session, 1);
        }

        public virtual int checkEndShotOfHole(Player _session)
        {

            // Agora verifica o se ele acabou o hole e essas coisas
            INIT_PLAYER_INFO("checkEndShotOfHole",
                "tentou verificar a ultima tacada do hole no jogo",
               _session, out PlayerGameInfo pgi);

            if (pgi.shot_sync.state_shot.display.acerto_hole || pgi.data.giveup == 1)
            {

                if (pgi.data.bad_condute >= 3)
                { // Kika player deu 3 give up

                    // !!@@@
                    // Tira o player da sala
                    return 2;
                }

                // Verifica se o player terminou jogo, fez o ultimo hole
                if (m_course.findHoleSeq(pgi.hole) == m_ri.qntd_hole)
                {

                    // Resposta para o player que terminou o ultimo hole do Game
                    var p = new PangyaBinaryWriter((ushort)0x199);

                    packet_func.session_send(p,
                        _session, 1);

                    // Fez o Ultimo Hole, Calcula Clear Bonus para o player
                    if (pgi.shot_sync.state_shot.display.clear_bonus)
                    {

                        if (!sMap.getInstance().isLoad())
                        {
                            sMap.getInstance().load();
                        }

                        var map = sMap.getInstance().getMap((byte)((int)m_ri.course & 0x7F));

                        if (map == null)
                        {
                            //_smp.message_pool.getInstance().push(new message("[TourneyBase::checkEndShotOfHole][Error][WARNING] tentou pegar o Map dados estaticos do course[COURSE=" + Convert.ToString((ushort)((int)m_ri.course & 0x7F)) + "], mas nao conseguiu encontra na classe do Server.", type_msg.CL_FILE_LOG_AND_CONSOLE));
                        }
                        else
                        {
                            pgi.data.bonus_pang += sMap.getInstance().calculeClear30s(map, m_ri.qntd_hole);
                        }
                    }
                }

                finishHole(_session);

                changeHole(_session);

            }

            return 0;
        }

        public virtual void drawDropItem(Player _session)
        {

            INIT_PLAYER_INFO("drawDropItem",
                "tentou sortear item drop para o jogador no jogo",
               _session, out PlayerGameInfo pgi);

            if (pgi.shot_sync.state_shot.display.acerto_hole)
            {
                var drop = requestInitDrop(_session);

                if (!drop.v_drop.empty())
                {
                    var p = new PangyaBinaryWriter((ushort)0xCC);

                    p.WriteInt32(_session.m_oid);

                    // Count, Coin/Cube "Drop"
                    p.WriteByte((byte)drop.v_drop.Count());

                    if (!drop.v_drop.empty())
                    {
                        foreach (var el in drop.v_drop)
                        {
                            p.WriteBytes(el.ToArray());
                        }

                        // Aqui o server passa 128 itens de drop, os que dropou e o resto vazio
                        if (drop.v_drop.Count() < 128)
                        {
                            p.WriteZeroByte((128 - drop.v_drop.Count()) * 16);
                        }
                    }

                    packet_func.session_send(p,
                        _session, 1);
                }
            }
        }
        public void achievement_top_3_1st(Player _session)
        {
            //CHECK_SESSION_BEGIN("achievement_top_3_1st");

            var rank = getRankPlace(_session);

            if (rank != ~0u)
            {

                if (rank < 3)
                {

                    INIT_PLAYER_INFO("achievement_top_3_1st",
                        "tentou atualizar achievement contador de top 3 rank do player no jogo",
                       _session, out PlayerGameInfo pgi);

                    pgi.sys_achieve.incrementCounter(0x6C4000B6u);

                    if (rank == 0u)
                    {
                        pgi.sys_achieve.incrementCounter(0x6C4000AFu);
                    }
                }
            }
        }

        public void calcule_shot_to_spinning_cube(Player _session, ShotSyncData _ssd)
        {
            //CHECK_SESSION_BEGIN("calcule_shot_to_spinning_cube");

            try
            {

                INIT_PLAYER_INFO("calcule_shot_to_spinning_cube",
                    "tentou calcular a tacada para o spinning cube",
                   _session, out PlayerGameInfo pgi);

                var hole = m_course.findHole(pgi.hole);

                if (hole == null)
                {
                    return;
                }

                if (_ssd.state != ShotSyncData.SHOT_STATE.PLAYABLE_AREA && _ssd.state != ShotSyncData.SHOT_STATE.INTO_HOLE)
                {
                    return; // Sai
                }

                // Bogey+ ou errou pangya ou bunker não calcula
                if (pgi.data.tacada_num > hole.getPar().par
                    || pgi.shot_data.acerto_pangya_flag != 4
                    || _ssd.bunker_flag != 0)
                {
                    return; // Sai, tacada bogey não calcula spinning cube
                }

                // Calcule Shot Cube
                sCoinCubeLocationUpdateSystem.getInstance().pushOrderToCalcule(new CalculeCoinCubeUpdateOrder(CalculeCoinCubeUpdateOrder.eTYPE.CUBE, _session.m_pi.uid, pgi.location, hole.getPinLocation(), pgi.shot_data_for_cube, (byte)(m_ri.getMap()), (byte)(m_ri.modo == (byte)RoomInfo.eMODO.M_REPEAT ? hole.getHoleRepeat() : hole.getNumero())));
            }
            catch (exception e)
            {

                //_smp.message_pool.getInstance().push(new message("[TourneyBase::calcule_shot_to_spinning_cube][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
        }
        public void calcule_shot_to_coin(Player _session, ShotSyncData _ssd)
        {
            //CHECK_SESSION_BEGIN("calcule_shot_to_coin");

            try
            {

                const float MIN_DISTANCE_TO_HOLE_TO_SPAWN_COIN = 70.0f * SCALE_PANGYA; // 70y

                INIT_PLAYER_INFO("calcule_shot_to_coin",
                    "tentou verificar a tacada para a coin",
                   _session, out PlayerGameInfo pgi);

                var hole = m_course.findHole(pgi.hole);

                if (hole == null)
                {
                    return;
                }

                if (_ssd.state != ShotSyncData.SHOT_STATE.PLAYABLE_AREA && _ssd.state != ShotSyncData.SHOT_STATE.INTO_HOLE)
                {
                    return; // Sai
                }

                // Bogey+ ou errou pangya ou bunker não calcula
                if (pgi.data.tacada_num > hole.getPar().par
                    || pgi.shot_data.acerto_pangya_flag != 4
                    || _ssd.bunker_flag != 0)
                {
                    return; // Sai, tacada bogey não calcula coin
                }

                if (Math.Abs(hole.getPinLocation().diffXZ(_ssd.location)) <= MIN_DISTANCE_TO_HOLE_TO_SPAWN_COIN)
                {
                    return; // Sai, muito perto do hole para spawnar uma coin
                }

                // Calcule Shot Coin
                sCoinCubeLocationUpdateSystem.getInstance().pushOrderToCalcule(new CalculeCoinCubeUpdateOrder(CalculeCoinCubeUpdateOrder.eTYPE.COIN, _session.m_pi.uid, _ssd.location, hole.getPinLocation(), pgi.shot_data_for_cube, m_ri.getMap(), (byte)(m_ri.getModo() == RoomInfo.eMODO.M_REPEAT ? hole.getHoleRepeat() : hole.getNumero())));

            }
            catch (exception e)
            {

                //_smp.message_pool.getInstance().push(new message("[TourneyBase::calcule_shot_to_coin][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
        }
        public virtual void requestCalculeShotSpinningCube(Player _session, ShotSyncData _ssd)
        {
            //UNREFERENCED_PARAMETER(_session);
            //UNREFERENCED_PARAMETER(_ssd);

            // o Calculo da tacada do spinning cube só é calculado no Tourney(normal) e no Grand Prix(Normal)
        }

        public virtual void requestCalculeShotCoin(Player _session, ShotSyncData _ssd)
        {
            //UNREFERENCED_PARAMETER(_session);
            //UNREFERENCED_PARAMETER(_ssd);

            // o Calculo da tacada da coin só é calculado no Tourney(normal) e no Grand Prix(Normal)
        }

        public override void requestExecCCGChangeWeather(Player _session, packet _packet)
        {
            try
            {

                var weather = _packet.ReadByte();

                // Log
                //_smp.message_pool.getInstance().push(new message("[VersusBase::requestExecCCGChangeWeather][Log] [GM] player[UID=" + (_session.m_pi.uid) + "] trocou o tempo(weather) da sala[NUMERO="
                        // + (m_ri.numero) + ", WEATHER=" + (weather) + "]", type_msg.CL_FILE_LOG_AND_CONSOLE));

                // UPDATE ON GAME
                PangyaBinaryWriter p = new PangyaBinaryWriter((ushort)0x9E);

                p.WriteUInt16(weather);
                p.WriteByte(1); // Acho que seja flag, não sei, vou deixar 1 por ser o GM que mudou

                packet_func.game_broadcast(this,
                    p, 1);

            }
            catch (exception e)
            {

                //_smp.message_pool.getInstance().push(new message("[VersusBase::requestExecCCGChangeWeather][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                throw;
            }
        }
        public static int end_time(object _arg1, object _arg2)
        {
            var game = Tools.reinterpret_cast<TourneyBase>(_arg1);

            try
            {

                // Tempo Acabou 
                    game.timeIsOver();

            }
            catch (exception e)
            {

                //_smp.message_pool.getInstance().push(new message("[TourneyBase::end_time][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }

            return 0;
        }
    }
}
