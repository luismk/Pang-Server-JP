using System;
using System.Collections.Generic;
using System.Linq;
using Pangya_GameServer.Cmd;
using Pangya_GameServer.Game.Utils;
using Pangya_GameServer.GameType;
using Pangya_GameServer.Session;
using PangyaAPI.IFF.JP.Extensions;
using PangyaAPI.SQL.Manager;
using PangyaAPI.Utilities;
using PangyaAPI.Utilities.Log;

namespace Pangya_GameServer.Game.System
{
    public class MemorialSystem
    {

        /*static*/
        protected void initialize()
        {

            try
            {
                // Carrega as Coin e os Itens
                var coins = sIff.getInstance().getMemorialShopCoinItem();
                var rares = sIff.getInstance().getMemorialShopRareItem();

                ctx_coin c = new ctx_coin();
                ctx_coin_item_ex ci = new ctx_coin_item_ex();

                try
                {
                    foreach (var el in coins)
                    {
                        c.tipo = (MEMORIAL_COIN_TYPE)el.type;
                        c._typeid = el.ID;
                        c.probabilidade = el.Probabilities;

                        foreach (var el2 in rares)
                        {

                            if (!el.gacha_range.empty() && !el.gacha_range.isBetweenGacha(el2.gacha.Number))
                            {
                                continue;
                            }

                            if (el.emptyFilter())
                            {
                                ci.clear();

                                ci.tipo = (int)el2.RareType;
                                ci._typeid = el2.ID;
                                ci.probabilidade = el2.Probabilities;
                                ci.gacha_number = (int)el2.gacha.Number;
                                ci.qntd = 1;

                                c.item.Add(ci);
                            }
                            else
                            {
                                for (var i = 0u; i < el2.filter.Length; ++i)
                                {

                                    if (el.hasFilter(el2.filter[i]))
                                    {
                                        ci.clear();

                                        ci.tipo = (int)el2.RareType;
                                        ci._typeid = el2.ID;
                                        ci.probabilidade = el2.Probabilities;
                                        ci.gacha_number = (int)el2.gacha.Number;
                                        ci.qntd = 1;

                                        c.item.Add(ci);

                                        break; // Sai do Loop de Filters
                                    }
                                } // Fim do loop de Filters
                            }
                        } // Fim do loop de Rare Item       
                        var it = m_coin.ContainsKey(c._typeid);
                        if (!it) // Se não existir, adiciona
                            m_coin.Add(c._typeid, c);

                        c = new ctx_coin();
                    } // Fim do loop de Coin Item
                }
                catch (exception e)
                {
                    throw e;
                }
            }
            catch (exception e)
            {
                throw e;
            }

            // Add os Itens Padr�es, para quando n�o ganha o rare item
            var cmd_mnii = new CmdMemorialNormalItemInfo(); // Waiter

            NormalManagerDB.add(0,
                cmd_mnii, null, null);


            if (cmd_mnii.getException().getCodeError() != 0)
                throw cmd_mnii.getException();

            m_consolo_premio = cmd_mnii.getInfo();

            // Levels
            var cmd_mli = new CmdMemorialLevelInfo(); // Waiter

            NormalManagerDB.add(0,
                cmd_mli, null, null);

            if (cmd_mli.getException().getCodeError() != 0)
                throw cmd_mli.getException();


            m_level = cmd_mli.getInfo();

            //#ifdef _DEBUG
            message_pool.push(new message("[MemorialSystem::initialize][Log] Memorial System Carregado com sucesso!", type_msg.CL_FILE_LOG_AND_CONSOLE));
            //#else
            //_smp::message_pool::getInstance().Push(new message("[MemorialSystem::initialize][Log] Memorial System Carregado com sucesso!", type_msg.CL_ONLY_FILE_LOG));
            //#endif // _DEBUG

            // Carregado com sucesso
            m_load = true;

        }

        /*static*/
        public bool isLoad()
        {

            bool isLoad = false;
            // + 1 no MEMORIAL_LEVEL_MAX por que � do 0 a 24, da 25 Levels
            isLoad = (m_load && m_coin.Any() && m_level.Any() && m_level.Count == (MEMORIAL_LEVEL_MAX) && m_consolo_premio.Any());

            return isLoad;
        }

        /*static*/
        public void load()
        {

            if (isLoad())
                clear();

            initialize();
        }

        /*static*/
        public ctx_coin findCoin(uint _typeid)
        {
            var it = m_coin.Find(_typeid);

            if (it.Any())
            {
                return m_coin.GetValue(_typeid);
            }
            return null;
        }

        /*static*/
        protected void clear()
        {

            if (m_coin.Any())
            {
                m_coin.Clear();
            }

            if (m_level.Any())
            {
                m_level.Clear();
            }

            if (m_consolo_premio.Any())
            {
                m_consolo_premio.Clear();
            }

            m_load = false;
        }

        /*static*/
        protected uint calculeMemorialLevel(uint _achievement_pontos)
        {

            if (_achievement_pontos == 0)
            {
                return 0u; // Level 0
            }

            var level = ((_achievement_pontos - 1) / 300);

            return level > MEMORIAL_LEVEL_MAX ? (uint)MEMORIAL_LEVEL_MAX : level;
        }/*static*/
        public List<ctx_coin_item_ex> Test(ctx_coin _ctx_c)
        {
            {
                if (!isLoad())
                {
                    throw new exception("[MemorialSystem::" + "drawCoin" + "][Error] Memorial System not loadded, please call load method first.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.MEMORIAL_SYSTEM,
                        2, 0));
                }
                if (_ctx_c._typeid == 0)
                {
                    throw new exception("[MemorialSystem::" + "drawCoin" + "][Error] coin _typeid is invalid(zero)", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.MEMORIAL_SYSTEM,
                        3, 0));
                }
                if (_ctx_c.item.Count == 0)
                {
                    throw new exception("[MemorialSystem::" + "drawCoin" + "][Error] coin is empty.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.MEMORIAL_SYSTEM,
                        4, 0));
                }
            };

            List<ctx_coin_item_ex> v_item = new List<ctx_coin_item_ex>();


            Lottery lottery = new Lottery();

            ctx_coin_item_ex ci = null;
            ctx_coin_set_item csi = null;

            // Calcula Memorial Level Pelos Achievement Pontos
            uint level = 1/* calculeMemorialLevel(_session.m_pi.mgr_achievement.getPontos())*/;

            // Initialize Rare Item e add � roleta
            foreach (var el in _ctx_c.item)
            {
                switch (_ctx_c.tipo)
                {
                    case MEMORIAL_COIN_TYPE.MCT_NORMAL:
                        if (el.gacha_number < 0 || (uint)el.gacha_number <= m_level[level].gacha_number)
                        {
                            lottery.Push(el.probabilidade, el);
                        }
                        break;
                    case MEMORIAL_COIN_TYPE.MCT_PREMIUM:
                        if (el.gacha_number < 0 || (uint)el.gacha_number <= m_level[MEMORIAL_LEVEL_MAX - 1].gacha_number)
                        {
                            lottery.Push(el.probabilidade, el);
                        }
                        break;
                    case MEMORIAL_COIN_TYPE.MCT_SPECIAL: // Special n�o tem limite de level, ele pega todos
                        lottery.Push(el.probabilidade, el);
                        break;
                    default:
                        throw new exception("[MemorialSystem::drawCoin][Error] Memorial Coin[TYPE=" + Convert.ToString(_ctx_c.tipo) + "] desconhecido. Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.MEMORIAL_SYSTEM,
                            6, 0));
                }
            }

            // Init Common Itens
            var limit_prob = lottery.GetLimitProbilidade();


            var count_item = m_consolo_premio.Values.Count(el => el.tipo == (_ctx_c.tipo == MEMORIAL_COIN_TYPE.MCT_PREMIUM ? 1 : 0));


            var rate_memorial = (float)(sgs.gs.getInstance().getInfo().rate.memorial_shop) / 100.0f;

            // Rate A+ da Coin
            if (_ctx_c.probabilidade > 0)
            {
                rate_memorial += (float)(_ctx_c.probabilidade * 4 / 100.0f); // 100 * 4 / 100 4 / 4 50% coin premium
            }

            limit_prob = (ulong)(limit_prob * (4 / rate_memorial)); // Padr�o 75% do limite de probabilidade consolo, 25% normal

            if (count_item > 0)
            {
                count_item = (int)((uint)limit_prob / count_item);
            }

            // Add Common Itens � roleta
            foreach (var el in m_consolo_premio.Values)
            {
                if (el.tipo == (_ctx_c.tipo == MEMORIAL_COIN_TYPE.MCT_PREMIUM ? 1 : 0))
                {
                    lottery.Push((uint)count_item, el);
                }
            }

            Lottery.LotteryCtx lc = null;
            uint count = 1; // Qntd de pr�mios sorteados

            while (count > 0)
            {


                lc = lottery.SpinRoleta(); // Remove os Item que j� foi sorteado

                if (lc == null)
                {
                    throw new exception("[MemorialSystem::drawCoin][Error] nao conseguiu sortear um item. erro na hora de rodar a roleta", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.MEMORIAL_SYSTEM,
                        5, 0));
                }

                if (lc.Value == null)
                {
                    throw new exception("[MemorialSystem::drawCoin][Error] nao conseguiu sortear um item. lc->value is invalid(0).", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.MEMORIAL_SYSTEM,
                        5, 1));
                }

                // Tempor�rio Coin Item

                // Verifica se � SetItem ou Item
                bool is_set = lc.Value is ctx_coin_item_ex ? false : true;
                if (is_set)
                { // SetItem
                    csi = (ctx_coin_set_item)lc.Value;

                    foreach (var el in csi.item)
                    {
                        // Contianua que o player j� tem esse item, e n�o pode ter duplicatas dele
                        //if ((!sIff.getInstance().IsCanOverlapped(el._typeid) || sIff.getInstance().getItemGroupIdentify(el._typeid) == sIff.getInstance().CAD_ITEM) && _session.m_pi.ownerItem(el._typeid))
                        //{
                        //    continue;
                        //}

                        v_item.Add(el);
                    }
                }
                else
                { // Item
                    ci = (ctx_coin_item_ex)lc.Value;

                    // Contianua que o player j� tem esse item, e n�o pode ter duplicatas dele
                    //if ((!sIff.getInstance().IsCanOverlapped(ci._typeid) || sIff.getInstance().getItemGroupIdentify(ci._typeid) == sIff.getInstance().CAD_ITEM) && _session.m_pi.ownerItem(ci._typeid))
                    //{
                    //    continue;
                    //}

                    v_item.Add(ci);
                }

                // Decrementa o count, que 1 item voi sorteado
                count = 0;

            }

            return v_item;
        }

        /*static*/
        public List<ctx_coin_item_ex> drawCoin(Player _session, ctx_coin _ctx_c)
        {
            {
                {
                    if (!_session.getState()
                        || !_session.isConnected())
                    {
                        throw new exception("[MemorialSystem::" + (("drawCoin")) + "][Error] session is not connected", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.MEMORIAL_SYSTEM,
                            1, 0));
                    }
                };
                if (!isLoad())
                {
                    throw new exception("[MemorialSystem::" + "drawCoin" + "][Error] Memorial System not loadded, please call load method first.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.MEMORIAL_SYSTEM,
                        2, 0));
                }
                if (_ctx_c._typeid == 0)
                {
                    throw new exception("[MemorialSystem::" + "drawCoin" + "][Error] coin _typeid is invalid(zero)", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.MEMORIAL_SYSTEM,
                        3, 0));
                }
                if (_ctx_c.item.Count == 0)
                {
                    throw new exception("[MemorialSystem::" + "drawCoin" + "][Error] coin is empty.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.MEMORIAL_SYSTEM,
                        4, 0));
                }
            };

            List<ctx_coin_item_ex> v_item = new List<ctx_coin_item_ex>();


            Lottery lottery = new Lottery();

            ctx_coin_item_ex ci = null;
            ctx_coin_set_item csi = null;

            // Calcula Memorial Level Pelos Achievement Pontos
            uint level = 1/* calculeMemorialLevel(_session.m_pi.mgr_achievement.getPontos())*/;

            // Initialize Rare Item e add � roleta
            foreach (var el in _ctx_c.item)
            {
                switch (_ctx_c.tipo)
                {
                    case MEMORIAL_COIN_TYPE.MCT_NORMAL:
                        if (el.gacha_number < 0 || (uint)el.gacha_number <= m_level[level].gacha_number)
                        {
                            lottery.Push(el.probabilidade, el);
                        }
                        break;
                    case MEMORIAL_COIN_TYPE.MCT_PREMIUM:
                        if (el.gacha_number < 0 || (uint)el.gacha_number <= m_level[MEMORIAL_LEVEL_MAX - 1].gacha_number)
                        {
                            lottery.Push(el.probabilidade, el);
                        }
                        break;
                    case MEMORIAL_COIN_TYPE.MCT_SPECIAL: // Special n�o tem limite de level, ele pega todos
                        lottery.Push(el.probabilidade, el);
                        break;
                    default:
                        throw new exception("[MemorialSystem::drawCoin][Error] Memorial Coin[TYPE=" + Convert.ToString(_ctx_c.tipo) + "] desconhecido. Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.MEMORIAL_SYSTEM,
                            6, 0));
                }
            }

            // Init Common Itens
            var limit_prob = lottery.GetLimitProbilidade();


            var count_item = m_consolo_premio.Values.Count(el => el.tipo == (_ctx_c.tipo == MEMORIAL_COIN_TYPE.MCT_PREMIUM ? 1 : 0));


            var rate_memorial = (float)(sgs.gs.getInstance().getInfo().rate.memorial_shop) / 100.0f;

            // Rate A+ da Coin
            if (_ctx_c.probabilidade > 0)
            {
                rate_memorial += (float)(_ctx_c.probabilidade * 4 / 100.0f); // 100 * 4 / 100 4 / 4 50% coin premium
            }

            limit_prob = (ulong)(limit_prob * (4 / rate_memorial)); // Padr�o 75% do limite de probabilidade consolo, 25% normal

            if (count_item > 0)
            {
                count_item = (int)((uint)limit_prob / count_item);
            }

            // Add Common Itens � roleta
            foreach (var el in m_consolo_premio.Values)
            {
                if (el.tipo == (_ctx_c.tipo == MEMORIAL_COIN_TYPE.MCT_PREMIUM ? 1 : 0))
                {
                    lottery.Push((uint)count_item, el);
                }
            }

            Lottery.LotteryCtx lc = null;
            uint count = 1; // Qntd de pr�mios sorteados

            while (count > 0)
            {

                {
                    if (!_session.getState()
                       || !_session.isConnected())
                    {
                        throw new exception("[MemorialSystem::" + "drawCoin" + "][Error] session is not connected", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.MEMORIAL_SYSTEM,
                            1, 0));
                    }
                };

                lc = lottery.SpinRoleta(true); // Remove os Item que j� foi sorteado

                if (lc == null)
                {
                    throw new exception("[MemorialSystem::drawCoin][Error] nao conseguiu sortear um item. erro na hora de rodar a roleta", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.MEMORIAL_SYSTEM,
                        5, 0));
                }

                if (lc.Value == null)
                {
                    throw new exception("[MemorialSystem::drawCoin][Error] nao conseguiu sortear um item. lc->value is invalid(0).", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.MEMORIAL_SYSTEM,
                        5, 1));
                }

                // Tempor�rio Coin Item

                // Verifica se � SetItem ou Item
                bool is_set = lc.Value is ctx_coin_item_ex ? false : true;
                if (is_set)
                { // SetItem
                    csi = (ctx_coin_set_item)lc.Value;

                    foreach (var el in csi.item)
                    {
                        // Contianua que o player j� tem esse item, e n�o pode ter duplicatas dele
                        if ((!sIff.getInstance().IsCanOverlapped(el._typeid) || sIff.getInstance().getItemGroupIdentify(el._typeid) == sIff.getInstance().CAD_ITEM) && _session.m_pi.ownerItem(el._typeid))
                        {
                            continue;
                        }

                        v_item.Add(el);
                    }
                }
                else
                { // Item
                    ci = (ctx_coin_item_ex)lc.Value;

                    // Contianua que o player j� tem esse item, e n�o pode ter duplicatas dele
                    if ((!sIff.getInstance().IsCanOverlapped(ci._typeid) || sIff.getInstance().getItemGroupIdentify(ci._typeid) == sIff.getInstance().CAD_ITEM) && _session.m_pi.ownerItem(ci._typeid))
                    {
                        continue;
                    }

                    v_item.Add(ci);
                }

                // Decrementa o count, que 1 item voi sorteado
                count = 0;

            }

            return v_item;
        }


        private MultiMap<uint, ctx_coin> m_coin = new MultiMap<uint, ctx_coin>();
        private Dictionary<uint, ctx_memorial_level> m_level = new Dictionary<uint, ctx_memorial_level>();
        private Dictionary<uint, ctx_coin_set_item> m_consolo_premio = new Dictionary<uint, ctx_coin_set_item>();

        uint MEMORIAL_LEVEL_MAX = 24;

        /*static*/
        private bool m_load = false;
    }


    public class sMemorialSystem : Singleton<MemorialSystem>
    { }
}
