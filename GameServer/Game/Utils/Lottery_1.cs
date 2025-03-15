using System;
using System.Collections.Generic;
using System.Linq;

namespace GameServer.Game.Utils
{
    public class Lottery
    {
        private readonly List<LotteryCtx> m_ctx = new List<LotteryCtx>();
        private ulong m_probLimit;
        private Dictionary<ulong, LotteryCtx> m_roleta;
        private List<ulong> m_randValues;

        public Lottery()
        {
            m_probLimit = 0;
            m_randValues = new List<ulong>();
            m_ctx = new List<LotteryCtx>();
            m_roleta = new Dictionary<ulong, LotteryCtx>();
 
            // Simulando o preenchimento com 5 valores randômicos
             for (var i = 0u; i < 5u; ++i)
                m_randValues.Add(sRandomGen.getInstance().rIbeMt19937_64_chrono());

            ShuffleValuesRand();
        }
        public void Push(LotteryCtx lc)
        {
            m_ctx.Add(lc);
        }

        public void Push(uint prob, object value, uint typeid = 0)
        {
            Push(new LotteryCtx
            {
                Active = true,
                Prob = prob,
                Value = value,
                _typeid = typeid,
                Penalty = 2.0f

            });
        }

        public void FillRoleta()
        {
            if (!m_ctx.Any())
                throw new Exception("[Lottery::fill_roleta][Error] nao tem lottery ctx, por favor popule o lottery primeiro.");

            // Limpa a roleta
            ClearRoleta();

            // Embaralha a lista m_ctx usando o algoritmo Fisher-Yates
            var rand = new Random((int)sRandomGen.getInstance().rIbeMt19937_64_chrono());  // Gerador baseado no chrono
            int n = m_ctx.Count;
            for (int i = n - 1; i > 0; i--)
            {
                // Escolhe um índice aleatório entre 0 e i
                int j = rand.Next(n-1);

                // Troca os elementos m_ctx[i] e m_ctx[j]
                var temp = m_ctx[i];
                m_ctx[i] = m_ctx[j];
                m_ctx[j] = temp;
            }

            // Embaralha a lista m_ctx                                    
            m_probLimit = 0;

            // Preenche a roleta
            foreach (var el in m_ctx)
            {
                if (el.Active)
                {
                    el.Offset[0] = m_probLimit == 0 ? m_probLimit : m_probLimit + 1;
                    el.Offset[1] = m_probLimit += (el.Prob <= 0) ? 100 : el.Prob;

                    m_roleta[el.Offset[0]] = el;
                    m_roleta[el.Offset[1]] = el;
                }
            }
        }

        public void ClearRoleta()
        {
            if (m_roleta.Any())
            {
                m_roleta.Clear();
            }
        }


        private void ShuffleValuesRand()
        {
            Random rng = new Random();
            m_randValues = m_randValues.OrderBy(x => sRandomGen.getInstance().RDevice()).ToList();
        }
        // Retorna a soma total das probabilidades dos itens ativos
        public ulong GetLimitProbilidade()
        {
            FillRoleta();
            return m_probLimit;
        }


        // Executa o sorteio aplicando penalidade nos itens raros/lendários
        public LotteryCtx SpinRoleta(bool removeItemDraw = false)
        {
            FillRoleta();
            // Filtra itens ativos e que ainda não atingiram o limite de sorteios
             
            trycode:
            // Calcula total de probabilidade
            var totalProbability = GetLimitProbilidade();
            Random random = new Random();
            int roll = random.Next(1, (int)totalProbability);
            
            LotteryCtx lc = null;
            var high = m_roleta.OrderByDescending(c => c.Key).FirstOrDefault().Key;//ideia é pega a chave maior
            var low = m_roleta.OrderBy(c => c.Key).FirstOrDefault().Key; //pegar a chave menor   
            ulong lucky = m_randValues[(int)sRandomGen.getInstance().RIbeMt19937_64ChronoRange(0, 4)] * sRandomGen.getInstance().RDevice() % (m_probLimit == 0 ? 1 : m_probLimit + 1);

            // Encontra o item que corresponde ao intervalo sorteado  
            lc = m_roleta.FirstOrDefault(kvp => lucky >= kvp.Key && lucky < kvp.Key + (kvp.Value.Prob <= 0 ? 100 : kvp.Value.Prob)).Value;
             if (lc == null)
            {
                goto trycode;
            }

            // Aplica a penalidade (aumento ou diminuição da probabilidade)
            lc.Prob = (uint)(lc.Prob / lc.Penalty);

            // Garante que a probabilidade não caia para zero, e não seja absurdamente alta
            if (lc.Prob < 1 || lc.Prob > 10000)
                lc.Prob = 1; //reinicia
                  
            if (removeItemDraw)
            {
                RemoveDrawItem(lc);
            }
            // Incrementa o contador de sorteios
            lc.DrawCount++;
            return lc;
        }

        public void RemoveDrawItem(LotteryCtx lc)
        {
            if (lc != null)
            {
                lc.Active = false;
            }
        }
        // Método para contar itens ativos
        public int GetCountItem()
        {
            return m_ctx.Count;
        } 
        public class LotteryCtx
        {
            public bool Active { get; set; }
            public object Value { get; set; }
            public uint Prob { get; set; }
            public ulong[] Offset { get; set; } = new ulong[2];//[0] base, [1] current
            public uint _typeid { get; set; }              
            public float Penalty { get; set; }
            public int DrawCount { get; set; }  // Contador de quantas vezes o item foi sorteado
            public int MaxDraws { get; }  // Quantidade máxima de sorteios permitidos    
        }
    }   
}
