using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Pangya_GameServer.Cmd;
using Pangya_GameServer.Game.Utils;
using Pangya_GameServer.GameType;
using PangyaAPI.IFF.JP.Extensions;
using PangyaAPI.SQL.Manager;
using PangyaAPI.Utilities;
using PangyaAPI.Utilities.Log;

namespace Pangya_GameServer.Game.System
{
    public class CardSystem
    {
        public CardSystem()
        {
            this.m_load = false;
            // Inicializa
            //initialize();
        }


        // Load
        /*static*/
        public void load()
        {

            if (isLoad())
            {
                clear();
            }

            initialize();
        }

        /*static*/
        public bool isLoad()
        {

            bool isLoad = false;

            isLoad = (m_load && m_card_pack.Count > 0 && m_box_card_pack.Count > 0);

            return isLoad;
        }

        // finders
        /*static*/
        public CardPack findCardPack(uint _typeid)
        {

            if (!isLoad())
            {
                throw new exception("[CardSystem::findCardPack][Error] Card System nao esta carregado, carregue ele primeiro antes de procurar um Card Pack.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.CARD_SYSTEM,
                    5, 0));
            }

            if (_typeid == 0)
            {
                throw new exception("[CardSystem::findCardPack][Error] _typeid is invalid(zero)", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.CARD_SYSTEM,
                1, 0));
            }


            var it = m_card_pack.Where(c => c.Key == _typeid);

            if (it.Any())
            {
                return it.First().Value;
            }
            return null;
        }

        /*static*/
        public CardPack findBoxCardPack(uint _typeid)
        {

            if (!isLoad())
            {
                throw new exception("[CardSystem::findBoxCardPack][Error] Card System nao esta carregado, carregue ele primeiro antes de procurar um Box Card Pack.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.CARD_SYSTEM,
                    5, 0));
            }

            if (_typeid == 0)
            {
                throw new exception("[CardSystem::findBoxCardPack][Error] _typeid is invalid(zero)", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.CARD_SYSTEM,
                    1, 0));
            }

            var it = m_box_card_pack.Where(c => c.Key == _typeid);

            if (it.Any())
            {
                return it.First().Value;
            }
            return null;
        }

        /*static*/
        public Card findCard(uint _typeid)
        {

            if (!isLoad())
            {
                throw new exception("[CardSystem::findCard][Error] Card System nao esta carregado, carregue ele primeiro antes de procurar um Card.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.CARD_SYSTEM,
                    5, 0));
            }

            if (_typeid == 0)
            {
                throw new exception("[CardSystem::findCard][Error] _typeid is invalid(zeror)", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.CARD_SYSTEM,
                    1, 0));
            }

            var it = m_card.Where(c => c._typeid == _typeid);

            if (it.Any())
            {
                return it.First();
            }
            return null;

        }

        /*static*/
        public List<Card> draws(CardPack _cp)
        {

            if (_cp._typeid == 0
                || _cp.num == 0
                || _cp.card.Count == 0)
            {
                throw new exception("[CardSystem::findCardPack][Error] CardPack[TYPEID=" + Convert.ToString(_cp._typeid) + ", NUM=" + Convert.ToString(_cp.num) + ", card(s)=" + Convert.ToString(_cp.card.Count) + "] is invalid", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.CARD_SYSTEM,
                    1, 0));
            }

            List<Card> v_card = new List<Card>();

            Lottery lottery = new Lottery();

            foreach (var el in _cp.card)
            {
                lottery.Push((uint)(el.prob * (double)((el.tipo > CARD_TYPE.T_SECRET ? 1.0f : _cp.rate.value[(int)el.tipo] / 100.0f))), el);
            }

            for (var i = 0u; i < _cp.num; ++i)
            {
                var lc = lottery.SpinRoleta();

                if (lc == null)
                {
                    throw new exception("[CardSystem::draws][ErrorSystem] nao conseguiu sortear um card", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.CARD_SYSTEM,
                        2, 0));
                }

                if ((Card)lc.Value == null)
                {
                    throw new exception("[CardSystem::draws][ErrorSystem] valor retornado do sorteio eh invalido(nullptr)", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.CARD_SYSTEM,
                        3, 0));
                }

                v_card.Add((Card)lc.Value);
            }

            return v_card;
        }

        /*static*/
        public Card drawsLoloCardCompose(LoloCardComposeEx _lcc)
        {

            Card card = new Card();


            Lottery lottery = new Lottery();

            uint prob = 0u;

            for (var i = 0u; i < (_lcc._typeid.Length); ++i)
            {
                prob += (uint)((_lcc.tipo + 1) * 20);
            }

            for (var i = 1; i <= 5; ++i)
            {
                var it = m_card_pack.Values.FirstOrDefault(c => c.volume == i);


                if (it != null)
                {
                    foreach (var el in it.card)
                    {
                        lottery.Push((el.tipo > CARD_TYPE.T_NORMAL ? el.prob + prob : el.prob), el);
                    }
                }
            }

            var lc = lottery.SpinRoleta();

            if (lc == null)
            {
                throw new exception("[CardSystem::drawsLoloCardCompose][ErrorSystem] nao conseguiu sortear um card", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.CARD_SYSTEM,
                    2, 0));
            }

            if ((Card)lc.Value == null)
            {
                throw new exception("[CardSystem::drawsLoloCardCompose][ErrorSystem] valor retornado do sorteio eh invalido(nullptr)", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.CARD_SYSTEM,
                    3, 0));
            }

            card = (Card)lc.Value;

            return card;
        }

        /*static*/
        protected void initialize()
        {
            // Load Card from IFF_STRUCT
            var card = sIff.getInstance().getCard();

            foreach (var el in card)
            {
                switch (sIff.getInstance().getItemSubGroupIdentify22(el.ID))
                {
                    case 0: // Character
                    case 1: // Caddie
                    case 2: // Special
                    case 5: // NPC
                        m_card.Add(new Card() { _typeid = el.ID, prob = 0, tipo = (CARD_TYPE)(el.Rarity) });
                        break;
                    case 4: // Box Card Pack
                        {
                            m_box_card_pack[el.ID] = new CardPack(el.ID,
                                    3, (byte)el.Volumn);
                            break;
                        }
                    case 3: // Card Pack
                            // N�o usa esses, por que � card pack os dois
                        break;
                    default:
                        throw new exception("[CardSystem::initialize][Error] Card Group Type Is invalid.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.CARD_SYSTEM,
                            4, 0));
                }
            }

            // Load Card Pack Map
            CmdCardPack cmd_cp = new Cmd.CmdCardPack(); // Waiter

            NormalManagerDB.add(0,
                cmd_cp, null,
                null);

            if (cmd_cp.getException().getCodeError() != 0)
                throw cmd_cp.getException();

            m_card_pack = cmd_cp.getCardPack();

            // Load Box Card Pack
            foreach (var el in m_box_card_pack.Values)
            {
                var it = m_card_pack.Values.FirstOrDefault(el2 => el2.volume == el.volume);

                if (it != null)
                {
                    el.card = new List<Card>(it.card);
                }
            }
            // Carregado com sucesso
            m_load = true;

            message_pool.push(new message("[CardSystem::initialize][Log] Carregou os CardPack/Box e Card com sucesso.", type_msg.CL_FILE_LOG_AND_CONSOLE));

        }

        /*static*/
        protected void clear()
        {

            Monitor.Enter(m_cs);

            if (m_card.Count > 0)
            {
                m_card.Clear();
            }

            if (m_card_pack.Count > 0)
            {
                m_card_pack.Clear();
            }

            m_load = false;

            Monitor.Exit(m_cs);
        }
        /*static*/
        private List<Card> m_card = new List<Card>(); // Todos os Card
        /*static*/
        private Dictionary<uint, CardPack> m_card_pack = new Dictionary<uint, CardPack>(); // Todos os Card Pack
        /*static*/
        private Dictionary<uint, CardPack> m_box_card_pack = new Dictionary<uint, CardPack>(); // Todos os Box Card Pack

        /*static*/
        private bool m_load; // Load CardSystem
        private object m_cs = new object();
    }
    public class sCardSystem : Singleton<CardSystem>
    {
    }
}
