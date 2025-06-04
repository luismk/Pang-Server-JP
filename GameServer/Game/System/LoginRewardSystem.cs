using Pangya_GameServer.Cmd;
using Pangya_GameServer.Game.Manager;
using Pangya_GameServer.GameType;
using Pangya_GameServer.Session;
using PangyaAPI.IFF.JP.Extensions;
using PangyaAPI.SQL;
using PangyaAPI.SQL.Manager;
using PangyaAPI.Utilities;
using PangyaAPI.Utilities.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pangya_GameServer.Game.System
{
    public class LoginRewardSystem
    {

        public LoginRewardSystem()
        {
            // Inicializa
            initialize();
        }

        public void load()
        {

            if (isLoad())
            {
                clear();
            }

            initialize();
        }

        public bool isLoad()
        {

            bool isLoad = false;

            isLoad = (m_load);

            return isLoad;
        }

        public void checkRewardLoginAndSend(Player _session)
        {
            if (!_session.getState())
            {
                throw new exception("[LoginRewardSystem::" + "checkRewardLoginAndSend" + "][Error] player nao esta connectado.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.LOGIN_REWARD_SYSTEM,
                    1, 0));
            }


            // Carrega lista de player do evento
            CmdLoginRewardPlayerInfo cmd_lrpi = new Cmd.CmdLoginRewardPlayerInfo(_session.m_pi.uid); // Waiter

            // Check All Event Enabled
            foreach (var el_e in m_events)
            {

                if (el_e.is_end)
                {
                    continue;
                }

                // Se tiver data e passou dela, encerra esse evento
                if (!(el_e.end_date.IsEmpty) && UtilTime.GetLocalDateDiff(el_e.end_date.ConvertTime()) > 0)
                {

                    el_e.is_end = true;

                    // Atualiza aqui no banco de dados o evento
                    NormalManagerDB.add(1,
                        new CmdUpdateLoginReward(el_e.id, el_e.is_end),
                        SQLDBResponse,
                        this);

                    // Continua
                    continue;
                }

                // Pega info do player no banco de dados
                cmd_lrpi.setId(el_e.id);

                NormalManagerDB.add(0,
                    cmd_lrpi, null, null);

                if (cmd_lrpi.getException().getCodeError() != 0)
                {

                    // Log
                    message_pool.push(new message("[LoginRewardSystem::checkRewardLoginAndSend][Error][WARNING] " + cmd_lrpi.getException().getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                    // Continua
                    continue;
                }

                var p = (stPlayerState)cmd_lrpi.getInfo();
                // ---- end get player info event

                if (p.id == 0 && p.uid == 0)
                { // N�o tem, cria um novo

                    p = new stPlayerState(0,
                        _session.m_pi.uid, 1u, 0, new PangyaTime());

                    if (p.update_date.IsEmpty)
                    {
                        p.update_date.CreateTime();
                    }

                    // Add o player ao banco de dados aqui
                    CmdAddLoginRewardPlayer cmd_alrp = new Cmd.CmdAddLoginRewardPlayer(el_e.id, // Waiter
                        p);

                    NormalManagerDB.add(0,
                        cmd_alrp, null, null);

                    // Error ao adicionar o player no banco de dados
                    if (cmd_alrp.getException().getCodeError() != 0)
                    {

                        // Log
                        message_pool.push(new message("[LoginRewardSystem::checkRewardLoginAndSend][Error][WARNING] " + cmd_alrp.getException().getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                        // Continua
                        continue;
                    }

                    if (!cmd_alrp.isGood())
                    {

                        // Log
                        message_pool.push(new message("[LoginRewardSystem::checkRewardLoginAndSend][Error][WARINIG] nao conseguiu adicionar o Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] no Login Reward[ID=" + Convert.ToString(el_e.id) + "] no banco de dados, nao retornou o id do player criado.", type_msg.CL_FILE_LOG_AND_CONSOLE));

                        // Continua
                        continue;
                    }

                    // Pega o Id que foi gerado quando adicionou no banco de dados
                    p = cmd_alrp.getPlayerState();

                    // Log
                    message_pool.push(new message("[LoginRewardSystem::checkRewardLoginAndSend][Log] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] Primeira participacao do player[" + p.toString() + "] no Login Reward Event[" + el_e.toString() + "]", type_msg.CL_FILE_LOG_AND_CONSOLE));

                }
                else
                {

                    // Continua o player j� terminou esse evento
                    if (p.is_clear)
                    {
                        continue;
                    }

                    // Ainda n�o � outro dia
                    if (UtilTime.GetLocalDateDiff(p.update_date.ConvertTime()) <= 0)
                    {
                        continue;
                    }

                    p.count_days++; // Update count

                    // Update time
                    p.update_date.CreateTime();

                    // Atualiza aqui o state do player no banco de dados
                    NormalManagerDB.add(2,
                        new Cmd.CmdUpdateLoginRewardPlayer(p),
                        SQLDBResponse,
                        this);
                }

                // Verifica quantas vezes tem que logar para receber o pr�mio

                // Player n�o tem o n�mero de login's necess�rios
                if (p.count_days < el_e.days_to_gift)
                {
                    continue;
                }

                p.count_seq++;

                if (el_e.type == stLoginReward.eTYPE.N_TIME)
                {

                    if (p.count_seq < el_e.n_times_gift)
                    {
                        p.count_days = 0;
                    }
                    else
                    {
                        p.is_clear = true;
                    }

                }
                else if (el_e.type == stLoginReward.eTYPE.FOREVER)
                {
                    p.count_days = 0;
                }

                // Atualiza aqui o state do plauer no banco de dados
                NormalManagerDB.add(2,
                    new CmdUpdateLoginRewardPlayer(p),
                    SQLDBResponse,
                    this);

                // Log
                message_pool.push(new message("[LoginRewardSystem::checkRewardLoginAndSend][Log] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] ganhou item[" + el_e.item_reward.toString() + "] no Login Reward[" + el_e.toString() + "] com [DAYS=" + Convert.ToString(p.count_days) + ", SEQ=" + Convert.ToString(p.count_seq) + ", IS_CLEAR=" + (p.is_clear ? "TRUE" : "FALSE") + "]", type_msg.CL_FILE_LOG_AND_CONSOLE));

                // Send Gift
                sendGiftToPlayer(_session, el_e);
            }

        }

        public void updateLoginReward()
        {
            foreach (var el_e in m_events)
            {

                if (el_e.is_end)
                {
                    continue;
                }

                if (!(el_e.end_date.IsEmpty) && UtilTime.GetLocalDateDiff(el_e.end_date.ConvertTime()) > 0)
                {

                    el_e.is_end = true;

                    // Atualiza aqui no banco de dados o evento
                    NormalManagerDB.add(1,
                        new CmdUpdateLoginReward(el_e.id, el_e.is_end),
                        SQLDBResponse,
                        this);
                }
            }
        }

        protected void initialize()
        {
            // Carrega a lista de eventos
            CmdLoginRewardInfo cmd_lri = new Cmd.CmdLoginRewardInfo(); // Waiter

            NormalManagerDB.add(0,
                cmd_lri, null, null);

            if (cmd_lri.getException().getCodeError() != 0)
            {
                throw cmd_lri.getException();
            }

            m_events = cmd_lri.getInfo();

            foreach (var el_e in m_events)
            {

                // Verifica se j� venceu
                if (!(el_e.end_date.IsEmpty) && UtilTime.GetLocalDateDiff(el_e.end_date.ConvertTime()) > 0)
                {

                    el_e.is_end = true;

                    // Atualiza aqui no banco de dados o evento
                    NormalManagerDB.add(1,
                        new CmdUpdateLoginReward(el_e.id, el_e.is_end),
                        SQLDBResponse,
                        this);
                }
            }
                          
            //#ifdef _DEBUG
            message_pool.push(new message("[LoginRewardSystem::initialize][Log] Login Reward System carregado com sucesso!", type_msg.CL_FILE_LOG_AND_CONSOLE));
            //#else
            //_smp::message_pool::getInstance().push(new message("[LoginRewardSystem::initialize][Log] Login Reward System carregado com sucesso!", type_msg.CL_ONLY_FILE_LOG));
            //#endif // _DEBUG

            // Carregado com sucesso
            m_load = true;
        }

        protected void clear()
        {
            if (!m_events.empty())
            {
                m_events.Clear();
            }

            m_load = false;
        }

        protected void sendGiftToPlayer(Player _session, stLoginReward _lr)
        {
            if (!_session.getState())
            {
                throw new exception("[LoginRewardSystem::" + "sendGiftToPlayer" + "][Error] player nao esta connectado.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.LOGIN_REWARD_SYSTEM,
                    1, 0));
            }

            Func<uint, string> getItemName = delegate (uint _typeid)
            {
                var @base = sIff.getInstance().findCommomItem(_typeid);
                return @base != null ? @base.Name : string.Empty;
            };



            try
            {

                stItem item = new stItem();
                BuyItem bi = new BuyItem();

                // Limpa
                bi.clear();
                item.clear();

                // Initialize
                bi.id = -1;
                bi._typeid = _lr.item_reward._typeid;
                bi.qntd = _lr.item_reward.qntd;
                bi.time = (short)_lr.item_reward.qntd_time;

                item_manager.initItemFromBuyItem(_session.m_pi,
                   item, bi, false, 0, 0, 1);

                if (item._typeid == 0)
                {
                    message_pool.push(new message("[LoginRewardSystem::sendGiftToPlayer][Error][WARNING] tentou enviar o reward para o player[UID=" + Convert.ToString(_session.m_pi.uid) + "] o Item[" + _lr.item_reward.toString() + "], mas nao conseguiu inicializar o item. Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));
                }

                var msg = ("Login Reward System - \"" + _lr.getName() + "\": item[ " + getItemName(_lr.item_reward._typeid) + " ]");

                if (MailBoxManager.sendMessageWithItem(0,
                    _session.m_pi.uid, msg, item) <= 0)
                {
                    message_pool.push(new message("[LoginRewardSystem::sendGiftToPlayer][Error][WARNING] tentou enviar reward para o player[UID=" + Convert.ToString(_session.m_pi.uid) + "] o Item[" + _lr.item_reward.toString() + "], mas nao conseguiu colocar o item no mail box dele. Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));
                }

            }
            catch (exception e)
            {

                message_pool.push(new message("[LoginRewardSystem::sendGiftToPlayer][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
        }

        protected static void SQLDBResponse(int _msg_id,
Pangya_DB _pangya_db,
object _arg)
        {

            if (_arg == null)
            {
#if DEBUG
                // Static class
                message_pool.push(new message("[LoginRewardSystem::SQLDBResponse][WARNING] _arg is nullptr na msg_id = " + Convert.ToString(_msg_id), type_msg.CL_FILE_LOG_AND_CONSOLE));
#endif // _DEBUG
                return;
            }

            // Por Hora s� sai, depois fa�o outro tipo de tratamento se precisar
            if (_pangya_db.getException().getCodeError() != 0)
            {
                message_pool.push(new message("[LoginRewardSystem::SQLDBResponse][Error] " + _pangya_db.getException().getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
                return;
            }

            var gts = Tools.reinterpret_cast<LoginRewardSystem>(_arg);

            switch (_msg_id)
            {
                case 1: // Update Login Reward
                    {

                        var cmd_ulr = Tools.reinterpret_cast<Cmd.CmdUpdateLoginReward>(_pangya_db);

                        // Log
                        message_pool.push(new message("[LoginRewardSystem::SQLDBResponse][Log] Atualizaou Login Reward[ID=" + Convert.ToString(cmd_ulr.getId()) + ", IS_END=" + (cmd_ulr.getIsEnd() ? "TRUE" : "FALSE") + "]", type_msg.CL_FILE_LOG_AND_CONSOLE));

                        break;
                    }
                case 2: // Update Login Reward Player
                    {

                        var cmd_ulrp = Tools.reinterpret_cast<CmdUpdateLoginRewardPlayer>(_pangya_db);

                        // Log
                        message_pool.push(new message("[LoginRewardSystem::SQLDBResponse][Log] Atualizou o Player[" + cmd_ulrp.getPlayerState().toString() + "] do Login Reward.", type_msg.CL_FILE_LOG_AND_CONSOLE));

                        break;
                    }
                case 0:
                default:
                    break;
            }
        }

        private bool m_load; 
        private List<stLoginReward> m_events = new List<stLoginReward>(); 
    }
    public class sLoginRewardSystem : Singleton<LoginRewardSystem>
    {
    }
}
