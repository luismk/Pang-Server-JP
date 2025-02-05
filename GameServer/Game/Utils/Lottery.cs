using PangyaAPI.Utilities.Log;
using PangyaAPI.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GameServer.Game.Utils
{
    public class Lottery
    {
        public class LotteryCtx
        {
            public void clear()
            {
            }
            public uint prob = new uint(); // Probabilidade
            public object value = new object();
            public ulong[] offset = new ulong[2]; // 0 start, 1 end
            public bool active; // 0 ou 1 ativo
        }

        public Lottery(ulong _value_rand)
        {
            this.m_prob_limit = 0Ul;

            initialize(_value_rand);
        }


        public void clear()
        { // Clear Ctx

            if (m_ctx.Count > 0)
            {
                m_ctx.Clear();
            }
        }

        public void push(LotteryCtx _lc)
        {
            m_ctx.Add(_lc);
        }

        public void push(uint _prob, object _value)
        {
            LotteryCtx lc = new LotteryCtx
            {
                active = false,
                prob = _prob,
                value = _value
            };
            push(lc);
        }

        public ulong getLimitProbilidade()
        {

            // Preenche roleta, para poder pegar o limite da probabilidade
            fill_roleta();

            return m_prob_limit;
        }

        // Retorna a quantidade de itens que tem para sortear
        public uint getCountItem()
        {
            return (uint)m_ctx.Count;
        }

        // Deleta o Item Sorteado, para não sair ele de novo, se for passado true
        public Lottery.LotteryCtx spinRoleta(bool _remove_item_draw = false)
        {

            try
            {

                LotteryCtx lc = null;

                // Preencha a Roleta
                fill_roleta();

                ulong lucky = 0Ul;

                shuffle_values_rand();
                lucky = m_rand_values[Convert.ToInt32(new Random().Next(0, 4) * new Random().Next() % (int)(m_prob_limit == 0 ? 1 : m_prob_limit + 1))];

                var bound = m_roleta.Where(c => c.Key == lucky);

                lc = bound.First().Value;

                // Deleta o Item Sorteado, para n�o sair ele de novo, se for passado true
                if (_remove_item_draw)
                {
                    remove_draw_item(lc);
                }

                 return lc;

            }
            catch (exception e)
            {

                message_pool.push(new message("[Lottery::spinRoleta][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                throw;
            }

         }

        protected void initialize(ulong _value_rand)
        {

            // 5 Rands Values
            for (var i = 0u; i < 5u; ++i)
            {
                m_rand_values.Add((ulong)new Random().Next());
            }

#if DEBUG
            message_pool.push(new message("[Lottery][Test] Values RAND: L=" + Convert.ToString(m_rand_values[0]) + " R=" + Convert.ToString(m_rand_values[1]) + " T=" + Convert.ToString(m_rand_values[2]) + " E=" + Convert.ToString(m_rand_values[3]) + " S=" + Convert.ToString(m_rand_values[4]) + "", type_msg.CL_ONLY_FILE_LOG));
#endif // _DEBUG

            shuffle_values_rand();
        }

        protected void fill_roleta()
        {

            if (m_ctx.Count == 0)
            {
                throw new exception("[Lottery::fill_roleta][Error] nao tem lottery ctx, por favor popule o lottery primeiro.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.LOTTERY,
                    1, 0));
            }

            // Limpa Roleta
            clear_roleta();

            // Shuffle Ctx                                                                                  
            m_ctx.Last().prob = (uint)new Random().Next(0, 1000);
            m_prob_limit = 0Ul;

            // Preenche Roleta
            foreach (var el in m_ctx)
            {
                if (el.active)
                {
                    el.offset[0] = (m_prob_limit == 0 ? m_prob_limit : m_prob_limit + 1);
                    el.offset[1] = m_prob_limit += (el.prob <= 0) ? 100 : el.prob;
                    m_roleta[el.offset[0]] = el;
                    m_roleta[el.offset[1]] = el;
                }
            }
        }

        protected void clear_roleta()
        {

            if (m_roleta.Count > 0)
            {
                m_roleta.Clear();
            }
        }

        protected void remove_draw_item(LotteryCtx _lc)
        {

            if (_lc != null)
            {
                _lc.active = false;
                //auto it = std::find_if(m_ctx.begin(), m_ctx.end(), [](auto el) {
                //	return el.offset[0] == _lc->offset[0]  el.offset[1] == _lc->offset[1];
                //});

                //// Remove from vector
                //if (it != m_ctx.end())
                //	//m_ctx.erase(it);
                //	it->active = 0;
            }
        }

        protected void shuffle_values_rand()
        {                                                            
            m_rand_values[m_rand_values.Count - 1] = (ulong)new Random().Next(0, 1000);
        }

        private SortedDictionary<ulong, LotteryCtx> m_roleta = new SortedDictionary<ulong, LotteryCtx>();

        private List<LotteryCtx> m_ctx = new List<LotteryCtx>();
        private List<ulong> m_rand_values = new List<ulong>();

        private ulong m_prob_limit = new ulong();
    }
}
