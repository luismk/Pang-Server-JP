using GameServer.GameType;
using GameServer.Cmd;
using GameServer.Session;
using PangyaAPI.SQL;
using PangyaAPI.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using PangyaAPI.SQL.Manager;
using _smp = PangyaAPI.Utilities.Log;
using PangyaAPI.Utilities.Log;
using GameServer.Game.Utils;
namespace GameServer.Game.System
{
    public class PapelShopSystem
    {

        const int PAPEL_SHOP_MIN_BALL = 1;
        const int PAPEL_SHOP_MAX_BALL = 5;
        const int PAPEL_SHOP_BIG_BALL = 10;

        const int PAPEL_SHOP_ITEM_MIN_QNTD = 1;
        const int PAPEL_SHOP_ITEM_MAX_QNTD = 3;
        // Papel Shop com redução do limite de vezes que poder jogar no dia
        // 1 ON, 0 OFF
        const int _PS_COM_REDUCE_LIMIT = 1;
        /*static*/
        private List<ctx_papel_shop_item> m_ctx_psi = new List<ctx_papel_shop_item>();
        /*static*/
        private Dictionary<uint, ctx_papel_shop_coupon> m_ctx_psc = new Dictionary<uint, ctx_papel_shop_coupon>();

        /*static*/
        private ctx_papel_shop m_ctx_ps = new ctx_papel_shop();

        /*static*/
        private bool m_load;

        public PapelShopSystem()
        {
            this.m_load = false;
            this.m_ctx_ps = new ctx_papel_shop();
            this.m_ctx_psc = new Dictionary<uint, ctx_papel_shop_coupon>();
            this.m_ctx_psi = new List<ctx_papel_shop_item>();
            // Inicializa
            initialize();
        }

        private void initialize()
        {
            // Load Config
            var cmd_psc = new CmdPapelShopConfig(); // Waiter

            NormalManagerDB.add(0, cmd_psc, null, null);

            if (cmd_psc.getException().getCodeError() != 0)
                throw cmd_psc.getException();

            m_ctx_ps = cmd_psc.getInfo();

            // Laod Coupon(s)
            var cmd_psCoupon = new CmdPapelShopCoupon(); // Waiter

            NormalManagerDB.add(0, cmd_psCoupon, null, null);

            if (cmd_psCoupon.getException().getCodeError() != 0)
                throw cmd_psCoupon.getException();

            m_ctx_psc = cmd_psCoupon.getInfo();

            // Load Item(s)
            CmdPapelShopItem cmd_psi = new CmdPapelShopItem(); // Waiter

            NormalManagerDB.add(0, cmd_psi, null, null);

            if (cmd_psi.getException().getCodeError() != 0)
                throw cmd_psi.getException();

            m_ctx_psi = cmd_psi.getInfo();

            //#ifdef _DEBUG
            _smp.message_pool.push(new message("[PapelShopSystem::initialize][Log] Papel Shop System Carregado com sucesso!", type_msg.CL_FILE_LOG_AND_CONSOLE));
            //#else
            //_smp::message_pool::push(new message("[PapelShopSystem::initialize][Log] Papel Shop System Carregado com sucesso!", type_msg.CL_ONLY_FILE_LOG));
            //#endif

            // Carregado com sucesso!
            m_load = true;
            //if (!sIff.getInstance().isLoad())
            //    sIff.getInstance().load();

            //for (int i = 0; i < m_ctx_psi.Count; i++)
            //{
            //   TesteDropBall();         
            //}
        }

        public void load()
        {
            if (isLoad())
                clear();

            initialize();
        }

        /*static*/
        public bool isLoad()
        {
            bool isLoad = m_load && m_ctx_psi.Any() && m_ctx_psc.Any();
            return isLoad;
        }

        /*static*/
        public bool isLimittedPerDay()
        {
            bool limited_per_day = m_ctx_ps.limitted_per_day == 1 ? true : false;
            return limited_per_day;
        }

        // Initialize Papel Shop Count Info
        /*static*/
        public void init_player_papel_shop_info(Player _session)
        {
            if (!_session.getState())
            {
                throw new exception("[PapelShopSystem::" + "init_player_papel_shop_info" + "][Error] player is not connected.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PAPEL_SHOP_SYSTEM,
                    2, 0));
            }

            if (!isLimittedPerDay())
            {
                _session.m_pi.mi.papel_shop = new PlayerPapelShopInfo();
            }
            else
            {

                // Limitted Per Day
                if (checkUpdate(_session.m_pi.mi.papel_shop_last_update))
                {

                    // Update Papel Shop Last Day Update of Player
                    updateDiaPlayer(_session);

                }
            }
        }

        /*static*/
        public void updateDia()
        {

            if (!isLoad())
            {
                throw new exception("[PapelShopSystem::updateDia][Error] O Papel Shop System nao foi carregado ainda, carregue ele primeiro antes de tentar atualizar o dia", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PAPEL_SHOP_SYSTEM,
                    3, 0));
            }

            if (checkUpdate())
            {

                // Update Dia do Papel Shop
                m_ctx_ps.update_date.CreateTime();

                CmdUpdatePapelShopConfig cmd_upsc = new CmdUpdatePapelShopConfig(m_ctx_ps); // Waiter

                NormalManagerDB.add(0,
                    cmd_upsc,
                    SQLDBResponse, null);

                if (cmd_upsc.getException().getCodeError() != 0)
                {
                    _smp.message_pool.push(new message("[PapelShopSystem::updateDia][ErrorSystem] " + cmd_upsc.getException().getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                    return; // Error Sai da função
                }

                // Update Aqui por que outro sistema atualizou primeiro no banco de dados
                if (cmd_upsc.isUpdated())
                {
                    _smp.message_pool.push(new message("[PapelShopSystem::updateDia][Log] Atualizou Papel Shop Config[" + cmd_upsc.getInfo().toString() + "] com sucesso", type_msg.CL_FILE_LOG_AND_CONSOLE));
                }
                else
                {

                    m_ctx_ps = cmd_upsc.getInfo();

                    _smp.message_pool.push(new message("[PapelShopSystem::updateDia][Log] Atualizou Papel Shop Config[" + cmd_upsc.getInfo().toString() + "] com os dados do DB, mas quem atualizou no DB foi outro sistema.", type_msg.CL_FILE_LOG_AND_CONSOLE));
                }
            }
        }

        /*static*/
        public void updateDiaPlayer(Player _session)
        {
            if (!_session.getState())
            {
                throw new exception("[PapelShopSystem::" + "updateDaiPlayer" + "][Error] player is not connected.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PAPEL_SHOP_SYSTEM,
                    2, 0));
            }

            // Update Time                          
            _session.m_pi.mi.papel_shop_last_update.CreateTime();
            // C++ TO C# CONVERTER TASK: C# does not allow setting or comparing const int constants:
            if (_PS_COM_REDUCE_LIMIT == 1)
            {
                // Atualiza o contador de quantas vez o player pode jogar no dia
                _session.m_pi.mi.papel_shop.remain_count = _session.m_pi.mi.papel_shop.limit_count;

                // Só aumenta o limite, se o player não jogou todo o seu limite
                if (_session.m_pi.mi.papel_shop.current_count < _session.m_pi.mi.papel_shop.limit_count)
                {

                    if (_session.m_pi.mi.papel_shop.limit_count < 50)
                    {
                        _session.m_pi.mi.papel_shop.limit_count = 50;
                    }
                    else if (_session.m_pi.mi.papel_shop.limit_count < 100)
                    {
                        _session.m_pi.mi.papel_shop.limit_count = 100;
                    }
                }
            }
            else
                _session.m_pi.mi.papel_shop.remain_count = _session.m_pi.mi.papel_shop.limit_count = 100;
            // Reseta o Atual Contador do player
            _session.m_pi.mi.papel_shop.current_count = 0;

            // Log
            _smp.message_pool.push(new message("[PapelShopSystem::updateDiaPlayer][Log] player[UID=" + Convert.ToString(_session.m_pi.uid) + "] atualizou Papel Shop Limit[REMAIN=" + Convert.ToString(_session.m_pi.mi.papel_shop.remain_count) + ", CURRENT=" + Convert.ToString(_session.m_pi.mi.papel_shop.current_count) + ", LIMIT=" + Convert.ToString(_session.m_pi.mi.papel_shop.limit_count) + ", LAST_UPDATE=" + UtilTime.FormatDate(_session.m_pi.mi.papel_shop_last_update.ConvertTime()) + "]", type_msg.CL_FILE_LOG_AND_CONSOLE));

            // UPDATE ON DB
            NormalManagerDB.add(2,
                new CmdUpdatePapelShopInfo(_session.m_pi.uid,
                    _session.m_pi.mi.papel_shop,
                    _session.m_pi.mi.papel_shop_last_update),
                SQLDBResponse, null);
        }

        /*static*/
        public void updatePlayerCount(Player _session)
        {
            if (!_session.getState())
            {
                throw new exception("[PapelShopSystem::" + "updatePlayerCount" + "][Error] player is not connected.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PAPEL_SHOP_SYSTEM,
                    2, 0));
            }

            if (isLimittedPerDay())
            {

                _session.m_pi.mi.papel_shop.current_count++;

                if (_PS_COM_REDUCE_LIMIT == 1)
                {
                    if (--_session.m_pi.mi.papel_shop.remain_count == 0)
                    { // Zerou tira 50 do limite do player

                        // Não diminui o limite deixa ele em 100 por dia mesmo
                        if (_session.m_pi.mi.papel_shop.limit_count > 50)
                        {
                            _session.m_pi.mi.papel_shop.limit_count = 50;
                        }
                        if (_session.m_pi.mi.papel_shop.limit_count > 30)
                        {
                            _session.m_pi.mi.papel_shop.limit_count = 30;
                        }
                        else
                        {
                            _session.m_pi.mi.papel_shop.limit_count = 30;
                        }
                    }
                }
                else
                    --_session.m_pi.mi.papel_shop.remain_count;

                // UPDATE ON DB
                NormalManagerDB.add(2,
                    new CmdUpdatePapelShopInfo(_session.m_pi.uid,
                        _session.m_pi.mi.papel_shop,
                        _session.m_pi.mi.papel_shop_last_update),
                    SQLDBResponse, null);
            }

        }

        /*static*/
        public void updateConfig(ctx_papel_shop _ps)
        {
            m_ctx_ps = _ps;
        }

        /*static*/
        public ulong getPriceNormal()
        {                                
            return m_ctx_ps.price_normal;
        }

        /*static*/
        public ulong getPriceBig()
        {                             
            return m_ctx_ps.price_big;
        }

        // Check if he has coupon, return id or -1 if not
        /*static*/
        public WarehouseItemEx hasCoupon(Player _session)
        {
            if (!_session.getState())
            {
                throw new exception("[PapelShopSystem::" + "hasCoupon" + "][Error] player is not connected.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PAPEL_SHOP_SYSTEM,
                    2, 0));
            }


            WarehouseItemEx pWi = null;

            foreach (var el in m_ctx_psc)
            {

                if (el.Value.active == 1 && (pWi = _session.m_pi.findWarehouseItemByTypeid(el.Value._typeid)) != null)
                {

                    return pWi;
                }
            }

            return null;
        }



        /*static*/
        protected void clear()
        {
            if (m_ctx_psi.Any())
            {
                m_ctx_psi.Clear();
            }

            if (m_ctx_psc.Any())
            {
                m_ctx_psc.Clear();
            }

            m_load = false;
        }

        protected bool checkUpdate()
        {

            if (!isLoad())
            {
                throw new exception("[PapelShopSystem::checkUpdate][Error] O Papel Shop System nao foi carregado ainda, carregue ele primeiro antes de tentar verificar se pode atualizar o dia", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PAPEL_SHOP_SYSTEM,
                    3, 0));
            }

            var local = new PangyaTime();

            local.CreateTime();

            bool check = false;

            // update
            check = (m_ctx_ps.update_date.Year < local.Year || m_ctx_ps.update_date.Month < local.Month || m_ctx_ps.update_date.Day < local.Day);

            return check;
        }

        /*static*/
        protected bool checkUpdate(PangyaTime _st)
        {

            // Verifica se é vazio, se for retorna true por que é diferente
            if (_st.IsEmpty)
            {
                return true;
            }

            bool check = false;

            check = (m_ctx_ps.update_date.Year != _st.Year || m_ctx_ps.update_date.Month != _st.Month || m_ctx_ps.update_date.Day != _st.Day);

            return check;
        }

        public List<ctx_papel_shop_ball> TesteDropBall()
        {
            if (!isLoad())
            {
                throw new exception("[PapelShopSystem::dropBalls][Error] O Papel Shop System nao foi carregado ainda, carregue ele primeiro antes de tentar dropar umas Balls", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PAPEL_SHOP_SYSTEM,
                    3, 0));
            }

            List<ctx_papel_shop_ball> v_ball = new List<ctx_papel_shop_ball>();
            ctx_papel_shop_ball ctx_b = new ctx_papel_shop_ball();

            Lottery lottery = new Lottery();

            // Pega o Rate do Game Server
            var rate_cookie_server = 100 / 100.0f;
            var rate_rare_server = 100 / 100.0f;

            foreach (var el in m_ctx_psi)
            {
                if (el.active == 1 && (el.numero == -1 || el.numero == m_ctx_ps.numero))
                {
                    lottery.Push((uint)(el.probabilidade * (el.tipo == PAPEL_SHOP_TYPE.PST_COOKIE ? rate_cookie_server : (el.tipo == PAPEL_SHOP_TYPE.PST_RARE ? rate_rare_server : 1.0f))), el, el._typeid);
                }
            }
            uint _type_id = 0;
            // Número de balls
            byte num_ball = (byte)new Random().Next(PAPEL_SHOP_MIN_BALL, PAPEL_SHOP_MAX_BALL - PAPEL_SHOP_MIN_BALL + 1);
            Console.Write("Sorteou as quantidade de bolas: " + num_ball + ", Win=>");
            while (num_ball > 0)
            {
                // Sortea um valor
                Lottery.LotteryCtx lc = null;

                lc = lottery.SpinRoleta();

                if (lc == null)
                {
                    throw new exception("[PapelShopSystem::dropsBalls][Error] nao conseguiu sortear Bola. Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PAPEL_SHOP_SYSTEM,
                        1, 0));
                }

                ctx_b.clear();

                var ctx_psi = (ctx_papel_shop_item)lc.Value;

                // Player já tem o item, e nao pode ter duplicate, sortea um novo para ele

                // Color
                ctx_b.color = (PAPEL_SHOP_BALL_COLOR)(new Random().Next() % ((int)PAPEL_SHOP_BALL_COLOR.PSBC_RED + 1));

                // Raro Item Sempre é a qntd minima
                if (ctx_psi.tipo == PAPEL_SHOP_TYPE.PST_RARE)
                {
                    ctx_b.qntd = PAPEL_SHOP_ITEM_MIN_QNTD;
                }
                else
                {
                    ctx_b.qntd = (uint)(PAPEL_SHOP_ITEM_MIN_QNTD + (new Random().Next() % (PAPEL_SHOP_ITEM_MAX_QNTD - PAPEL_SHOP_ITEM_MIN_QNTD + 1)));
                }

                // Item
                ctx_b.ctx_psi = ctx_psi;

                // Add Ball ao vector de bolas dropadas
                v_ball.Add(ctx_b);
                //salva o ultimo id
                _type_id = ctx_b.ctx_psi._typeid;
                Console.WriteLine(Environment.NewLine);
                Console.WriteLine("itemID:" + ctx_b.ctx_psi._typeid + ", ItemName: " + sIff.getInstance().GetItemName(ctx_b.ctx_psi._typeid));

                // Decrementa o num_ball, que uma bola já foi dropada
                num_ball--;

            }
            Console.WriteLine(Environment.NewLine);
            return v_ball;
        }


        public List<ctx_papel_shop_ball> dropBalls(Player _session)
        {
            if (!_session.getState())
            {
                throw new exception("[PapelShopSystem::" + "dropBalls" + "][Error] player is not connected.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PAPEL_SHOP_SYSTEM,
                    2, 0));
            }
            if (!isLoad())
            {
                throw new exception("[PapelShopSystem::dropBalls][Error] O Papel Shop System nao foi carregado ainda, carregue ele primeiro antes de tentar dropar umas Balls", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PAPEL_SHOP_SYSTEM,
                    3, 0));
            }

            List<ctx_papel_shop_ball> v_ball = new List<ctx_papel_shop_ball>();
            ctx_papel_shop_ball ctx_b = new ctx_papel_shop_ball();

            Lottery lottery = new Lottery();

            // Pega o Rate do Game Server
            var rate_cookie_server = 100 / 100.0f;
            var rate_rare_server = 100 / 100.0f;

            foreach (var el in m_ctx_psi)
            {
                if (el.active == 1 && (el.numero == -1 || el.numero == m_ctx_ps.numero))
                {
                    lottery.Push((uint)(el.probabilidade * (el.tipo == PAPEL_SHOP_TYPE.PST_COOKIE ? rate_cookie_server : (el.tipo == PAPEL_SHOP_TYPE.PST_RARE ? rate_rare_server : 1.0f))), el, el._typeid);
                }
            }
            uint _type_id = 0;
            // Número de balls
            byte num_ball = (byte)new Random().Next(PAPEL_SHOP_MIN_BALL, PAPEL_SHOP_MAX_BALL - PAPEL_SHOP_MIN_BALL + 1);
            Console.Write("Sorteou as quantidade de bolas: " + num_ball + ", Win=>");
            while (num_ball > 0)
            {
                // Sortea um valor
                Lottery.LotteryCtx lc = null;


                    lc = lottery.SpinRoleta();

                if (lc == null)
                {
                    throw new exception("[PapelShopSystem::dropsBalls][Error] nao conseguiu sortear Bola. Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PAPEL_SHOP_SYSTEM,
                        1, 0));
                }

                ctx_b.clear();

                var ctx_psi = (ctx_papel_shop_item)lc.Value;


                // Player já tem o item, e nao pode ter duplicate, sortea um novo para ele
                if ((!sIff.getInstance().IsCanOverlapped(ctx_psi._typeid) || sIff.getInstance()._getItemGroupIdentify(ctx_psi._typeid) == PangLib.IFF.JP.Models.Flags.IFF_GROUP.CAD_ITEM) && _session.m_pi.ownerItem(ctx_psi._typeid))
                {
                    continue;
                }
                // Color
                ctx_b.color = (PAPEL_SHOP_BALL_COLOR)(new Random().Next(0, 3));

                // Raro Item Sempre é a qntd minima
                if (ctx_psi.tipo == PAPEL_SHOP_TYPE.PST_RARE)
                {
                    ctx_b.qntd = PAPEL_SHOP_ITEM_MIN_QNTD;
                }
                else
                {
                    ctx_b.qntd = (uint)(PAPEL_SHOP_ITEM_MIN_QNTD + (new Random().Next() % (PAPEL_SHOP_ITEM_MAX_QNTD - PAPEL_SHOP_ITEM_MIN_QNTD + 1)));
                }

                // Item
                ctx_b.ctx_psi = ctx_psi;

                // Add Ball ao vector de bolas dropadas
                v_ball.Add(ctx_b);
                //salva o ultimo id
                _type_id = ctx_b.ctx_psi._typeid;
                Console.WriteLine(Environment.NewLine);
                Console.WriteLine("itemID:" + ctx_b.ctx_psi._typeid + ", ItemName: " + sIff.getInstance().GetItemName(ctx_b.ctx_psi._typeid));

                // Decrementa o num_ball, que uma bola já foi dropada
                num_ball--;

            }
            Console.WriteLine(Environment.NewLine);
            return v_ball;
        }


        protected static void SQLDBResponse(int _msg_id,
                Pangya_DB _pangya_db,
                object _arg)
        {

#if DEBUG
            // Classe estatica não pode passar o ponteiro dela, por ser estática, então passa nullptr
            if (_arg == null)
            {
                // Static class
                _smp.message_pool.push(new message("[PapelShopSystem::SQLDBResponse][WARNING] _arg is nullptr", type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
#endif // _DEBUG

            // Por Hora só sai, depois faço outro tipo de tratamento se precisar
            if (_pangya_db.getException().getCodeError() != 0)
            {
                _smp.message_pool.push(new message("[PapelShopSystem::SQLDBResponse][Error] " + _pangya_db.getException().getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
                return;
            }

            switch (_msg_id)
            {
                case 1: // Update Papel Shop Config
                    {
                        // C++ TO C# CONVERTER TASK: There is no equivalent to 'reinterpret_cast' in C#:
                        var cmd_upsc = (CmdUpdatePapelShopConfig)(_pangya_db);

                        if (cmd_upsc.isUpdated())
                        {
                            _smp.message_pool.push(new message("[PapelShopSystem::SQLDBResponse][Log] Atualizou Papel Shop Config[" + cmd_upsc.getInfo().toString() + "] com sucesso", type_msg.CL_FILE_LOG_AND_CONSOLE));
                        }
                        else
                        {

                            // Não conseguiu atualizar papel shop config, por que outro sistema atualizou primeiro, atualiza o papel shop config com os dados retornados
                            sPapelShopSystem.getInstance().updateConfig(cmd_upsc.getInfo());

                            _smp.message_pool.push(new message("[PapelShopSystem::SQLDBResposne][Log] Atualizou Papel Shop Config[" + cmd_upsc.getInfo().toString() + "] com os dados do DB, mas quem atualizou no DB foi outro sistema.", type_msg.CL_FILE_LOG_AND_CONSOLE));
                        }

                        break;
                    }
                case 2: // Update Papel Shop Player Info
                    {
                        // Aqui não usa por que é um update
                        break;
                    }
                case 3: // Update Papel Shop Last Day Update do player
                    {
                        // Aqui não usa por que é um update
                        break;
                    }
                case 0:
                default:
                    break;
            }
        }

    }

    public class sPapelShopSystem : Singleton<PapelShopSystem>
    { }
}