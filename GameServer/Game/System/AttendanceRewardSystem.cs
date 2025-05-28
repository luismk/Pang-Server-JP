using System;
using System.Collections.Generic;
using System.Linq;
using Pangya_GameServer.Cmd;
using Pangya_GameServer.Game.Manager;
using Pangya_GameServer.Game.Utils;
using Pangya_GameServer.GameType;
using Pangya_GameServer.PacketFunc;
using Pangya_GameServer.Session;
using PangyaAPI.Network.PangyaPacket;
using PangyaAPI.SQL;
using PangyaAPI.SQL.Manager;
using PangyaAPI.Utilities;
using PangyaAPI.Utilities.BinaryModels;
using PangyaAPI.Utilities.Log;
using static Pangya_GameServer.GameType._Define;

namespace Pangya_GameServer.Game.System
{
    public class AttendanceRewardSystem
    {
        // Campos privados
        private List<AttendanceRewardItemCtx> v_item;
        private bool m_load;
        private readonly object m_cs; // Usado para sincronização (equivalente ao CRITICAL_SECTION ou pthread_mutex_t)

        // Construtor
        public AttendanceRewardSystem()
        {
            m_cs = new object();
            v_item = new List<AttendanceRewardItemCtx>();
            m_load = false;
            initialize();

        }

        // Destrutor/Finalizador (caso necessário para liberação de recursos)
        ~AttendanceRewardSystem()
        {
            // Código de limpeza, se necessário.
            clear();
        }

        // Método para carregar o sistema de recompensa
        public void load()
        {
            if (isLoad())
                clear();

            initialize();
        }

        // Retorna se o sistema foi carregado
        public bool isLoad()
        {

            bool isLoad = false;

            isLoad = (m_load && v_item.Any());

            return isLoad;
        }

        // Solicita verificação de presença (attendance)
        public void requestCheckAttendance(Player _session, packet _packet)
        {
            try
            {

                if (passedOneDay(_session))
                {   // Passou 1 dia depois que o player logou no pangya

                    _session.m_pi.ari.login = 0;

                    // Troca o item after para now
                    _session.m_pi.ari.now = _session.m_pi.ari.after;

                    // Limpa o After
                    _session.m_pi.ari.after.clear();

                    // Atualiza no banco de dados
                    NormalManagerDB.add(1, new CmdUpdateAttendanceReward(_session.m_pi.uid, _session.m_pi.ari), SQLDBResponse, null);

                }
                else
                    _session.m_pi.ari.login = 1;   // Ainda n�o passou 1 dia desde que ele logou no pangya


                packet_func.session_send(packet_func.pacote248(_session.m_pi.ari), _session);
            }
            catch (exception e)
            {
                message_pool.push(new message("[AttendanceRewardSystem::checkAttendance][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                using (var p = new PangyaBinaryWriter())
                {
                    p.init_plain(0x248);

                    p.WriteUInt32(ExceptionError.STDA_SYSTEM_ERROR_DECODE_TYPE(e.getCodeError()) == (int)STDA_ERROR_TYPE.ATTENDANCE_REWARD_SYSTEM ? ExceptionError.STDA_SYSTEM_ERROR_DECODE_TYPE(e.getCodeError()) : ~0u);

                    packet_func.session_send(p, _session);
                }
                throw e;
            }

        }

        // Solicita atualização do contador de login
        public void requestUpdateCountLogin(Player _session, packet _packet)
        {

            try
            {
                AttendanceRewardItemCtx reward_item;

                if (_session.m_pi.ari.login == 1u)
                    throw new exception("[AttendanceRewardSystem::requestUpdateCountLogin][Error] player[UID=" + (_session.m_pi.uid)
                            + "] tentou pegar o premio do dia logado, mas ele ja pegou. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ATTENDANCE_REWARD_SYSTEM, 8, 0));

                if (!passedOneDay(_session))
                    throw new exception("[AttendanceRewardSystem::requestUpdateCountLogin][Error] player[UID=" + (_session.m_pi.uid)
                            + "] tentou pegar o premio do dia logado, mas ele ja pegou. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ATTENDANCE_REWARD_SYSTEM, 8, 1));

                // Verifica se � o primeiro dia que ele loga, ai tem que criar os 2 item, que ele vai ganha o now e after
                // Incrementa o count de login do player e d� o reward dele
                if (_session.m_pi.ari.counter++ == 0 || _session.m_pi.ari.now._typeid == 0u)
                {

                    reward_item = drawReward(1);

                    if (reward_item == null)
                        throw new exception("[AttendanceRewardSystem::requestUpdateCountLogin][Error] nao conseguiu sortear um item para o player. Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ATTENDANCE_REWARD_SYSTEM, 7, 0));

                    _session.m_pi.ari.now._typeid = reward_item._typeid;
                    _session.m_pi.ari.now.qntd = reward_item.qntd;
                }

                _session.m_pi.ari.last_login.CreateTime();

                // Zera as Horas deixa s� a date
                _session.m_pi.ari.last_login.MilliSecond = _session.m_pi.ari.last_login.Second = _session.m_pi.ari.last_login.Minute = _session.m_pi.ari.last_login.Hour = 0;

                // Evento de Login do dia concluido
                _session.m_pi.ari.login = 1;

                // Reward
                stItem item = new stItem();

                item.type = 2;
                item.id = -1;
                item._typeid = _session.m_pi.ari.now._typeid;
                item.qntd = _session.m_pi.ari.now.qntd;
                item.STDA_C_ITEM_QNTD = (ushort)item.qntd;

                string msg = "Your Attendance rewards have arrived";

                MailBoxManager.sendMessageWithItem(0, _session.m_pi.uid, msg, item);

                // Sortea o Pr�ximo Item que ele vai ganhar
                reward_item = drawReward((byte)(((_session.m_pi.ari.counter + 1) % 10 == 0) ? 2/*Tipo 2 Papel Box*/ : 1/*Item Normal*/));

                if (reward_item == null)
                    throw new exception("[AttendanceRewardSystem::requestUpdateCountLogin][Error] nao conseguiu sortear um item para o player. Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ATTENDANCE_REWARD_SYSTEM, 7, 0));

                _session.m_pi.ari.after._typeid = reward_item._typeid;
                _session.m_pi.ari.after.qntd = reward_item.qntd;

                // Atualiza no Banco de dados
                NormalManagerDB.add(1, new CmdUpdateAttendanceReward(_session.m_pi.uid, _session.m_pi.ari), SQLDBResponse, null);

                // D� 3 Grand Prix Ticket, por que � a primeira vez que o player loga no dia
                sendGrandPrixTicket(_session);

                // Log
                message_pool.push(new message("[AttendanceRewardSystem::requestUpdateCountLogin][Log] player[UID=" + (_session.m_pi.uid) + "] Atualizou seu Count de Login, e ganhou o Item[TYPEID="
                         + (reward_item._typeid) + ", QNTD=" + (reward_item.qntd) + "]", type_msg.CL_FILE_LOG_AND_CONSOLE));

                //// UPDATE Achievement ON SERVER, DB and GAME
                //SysAchievement sys_achieve;

                //sys_achieve.incrementCounter(0x6C4000A0u/*Login Count por dia, 1 por dia*/);


                packet_func.session_send(packet_func.pacote249(_session.m_pi.ari), _session);

                // UPDATE Achievement ON SERVER, DB and GAME
                ///sys_achieve.finish_and_update(_session);
            }
            catch (exception e)
            {
                message_pool.push(new message("[AttendanceRewardSystem::requestUpdateCountLogin][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                using (var p = new PangyaBinaryWriter())
                {
                    p.init_plain(0x249);

                    p.WriteUInt32(ExceptionError.STDA_SYSTEM_ERROR_DECODE_TYPE(e.getCodeError()) == (int)STDA_ERROR_TYPE.ATTENDANCE_REWARD_SYSTEM ? ExceptionError.STDA_SYSTEM_ERROR_DECODE_TYPE(e.getCodeError()) : ~0u);

                    packet_func.session_send(p, _session);
                }
                throw e;
            }
        }

        // Métodos protegidos

        // Inicializa o sistema
        protected void initialize()
        {

            // Carrega os Itens do Attendance Reward
            var cmd_aric = new Cmd.CmdAttendanceRewardItemInfo(); // Waiter

            NormalManagerDB.add(0, cmd_aric, null, null);

            if (cmd_aric.getException().getCodeError() != 0)
                throw cmd_aric.getException();

            v_item = cmd_aric.getInfo();

            //#ifdef _DEBUG
            if (v_item.Count > 0) 
            message_pool.push(new message("[AttendanceRewardSystem::initialize][Log] Attendance Reward System Carregado com sucesso.", type_msg.CL_FILE_LOG_AND_CONSOLE));
            //#else
            //message_pool.push(new message("[AttendanceRewardSystem::initialize][Log] Attendance Reward System Carregado com sucesso.", type_msg.CL_ONLY_FILE_LOG));
            //#endif // _DEBUG

            // Carregou com sucesso
            m_load = true;

        }

        // Limpa os dados do sistema
        protected void clear()
        {
            // Implementação da limpeza dos dados

            if (v_item.Any())
            {
                v_item.Clear();
            }

            m_load = false;
        }

        /// <summary>
        /// Dá 3 Grand Prix Ticket para o Player por ele ter logado a primeira vez no dia,
        /// mas só dá se ele não atingiu o limite de grand prix ticket.
        /// </summary>
        protected void sendGrandPrixTicket(Player _session)
        {
            // Implementação do envio de Grand Prix Ticket
            //next version add item +++
        }

        // Retorna um objeto de recompensa com base no tipo informado
        public AttendanceRewardItemCtx drawReward(byte _tipo)
        {
            try
            {
                if (!isLoad())
                    throw new exception("[AttendanceRewardSystem::drawReward][Error] Attendance Reward not load, please call load method first.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ATTENDANCE_REWARD_SYSTEM, 4, 0));

                AttendanceRewardItemCtx aric = null;


                var lottery = new Lottery();

                if (v_item.Any(el => el.tipo == _tipo))
                {
                    var collection = v_item.Where(el => el.tipo == _tipo).ToList();
                    foreach (var item in collection)
                        lottery.Push(400, item);
                }
                else
                {
                    var collection = v_item.ToList();//nao tem o de cima, vou pegar do tipo '0'
                    foreach (var item in collection)
                        lottery.Push(400, item);
                }
                var lc = lottery.SpinRoleta();

                if (lc == null)
                    throw new exception("[AttendanceRewardSystem::drawReward][Error] nao conseguiu rodar a roleta. falhou ao sortear o item. Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ATTENDANCE_REWARD_SYSTEM, 5, 0));

                aric = (AttendanceRewardItemCtx)lc.Value;

                return aric;
            }
            catch (Exception e)
            {

                throw e;
            }
        }

        // Verifica se passou um dia após o Player ter logado no Pangya
        public bool passedOneDay(Player _session)
        {
            try
            {
                // Define o início do dia atual (0h, 0min, 0seg, 0ms)
                var st = DateTime.Now;

                // Converte o horário do último login do player (supondo que ConvertTime retorne um DateTime)
                DateTime lastLogin = _session.m_pi.ari.last_login.ConvertTime(); 

                // Calcula a diferença de tempo utilizando o método utilitário (assume que diff seja compatível com STDA_10_MICRO_PER_DAY)
                var diff = UtilTime.GetTimeDiff(st, lastLogin);

                var res = (diff / STDA_10_MICRO_PER_DAY) >= 1;
                // Retorna verdadeiro se a diferença, em unidades definidas por STDA_10_MICRO_PER_DAY, for de pelo menos 1
                return res;
            }
            catch (Exception e)
            {

                throw e;
            }
        }

        // Método estático para resposta de banco de dados
        protected static void SQLDBResponse(int _msg_id, Pangya_DB _pangya_db, object _arg)
        {
            if (_arg == null)
            {
                message_pool.push(new message("[AttendanceRewardSystem::SQLDBResponse][WARNING] _arg is nullptr com msg_id = " + (_msg_id), type_msg.CL_FILE_LOG_AND_CONSOLE));
                return;
            }

            // Por Hora s� sai, depois fa�o outro tipo de tratamento se precisar
            if (_pangya_db.getException().getCodeError() != 0)
            {
                message_pool.push(new message("[AttendanceRewardSystem::SQLDBResponse][Error] " + _pangya_db.getException().getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
                return;
            }

            switch (_msg_id)
            {
                case 1: // Update Attendance Reward Player
                    {
                        var cmd_uar = (CmdUpdateAttendanceReward)(_pangya_db);
                        message_pool.push(new message("[AttendanceRewardSystem::SQLDBResponse][Log] player[UID=" + (cmd_uar.getUID()) + "] Atualizou Attendance Reward com sucesso.", type_msg.CL_FILE_LOG_AND_CONSOLE));

                        break;
                    }
                case 0:
                default:
                    break;
            }

        }
    }

    // Implementação do padrão Singleton
    public class sAttendanceRewardSystem : Singleton<AttendanceRewardSystem>
    {
    }
}
