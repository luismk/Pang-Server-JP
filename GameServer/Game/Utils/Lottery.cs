//using PangyaAPI.Utilities.Log;
//using PangyaAPI.Utilities;
//using System;
//using System.Collections.Generic;
//using System.Linq;

//namespace GameServer.Game.Utils
//{
//    public class Lottery
//    {
//        private ulong m_probLimit;
//        private List<ulong> m_randValues;
//        private List<LotteryCtx> m_ctx;
//        private Dictionary<ulong, LotteryCtx> m_roleta;

//        public Lottery(ulong valueRand)
//        {
//            m_probLimit = 0;
//            m_randValues = new List<ulong>();
//            m_ctx = new List<LotteryCtx>();
//            m_roleta = new Dictionary<ulong, LotteryCtx>();

//            Initialize(valueRand);
//        }

//        public void Initialize(ulong valueRand)
//        {
//            // Simulando o preenchimento com 5 valores randômicos
//            Random random = new Random();
//            for (var i = 0u; i < 5u; ++i)
//                m_randValues.Add(sRandomGen.getInstance().rIbeMt19937_64_chrono());

//            ShuffleValuesRand();
//        }

//        public void FillRoleta()
//        {
//            if (!m_ctx.Any())
//                throw new Exception("[Lottery::fill_roleta][Error] nao tem lottery ctx, por favor popule o lottery primeiro.");

//            // Limpa a roleta
//            ClearRoleta();

//            // Embaralha a lista m_ctx usando o algoritmo Fisher-Yates
//            var rand = new Random((int)sRandomGen.getInstance().rIbeMt19937_64_chrono());  // Gerador baseado no chrono
//            int n = m_ctx.Count;
//            for (int i = n - 1; i > 0; i--)
//            {
//                // Escolhe um índice aleatório entre 0 e i
//                int j = rand.Next(i + 1);

//                // Troca os elementos m_ctx[i] e m_ctx[j]
//                var temp = m_ctx[i];
//                m_ctx[i] = m_ctx[j];
//                m_ctx[j] = temp;
//            }

//            // Embaralha a lista m_ctx                                    
//            m_probLimit = 0;

//            // Preenche a roleta
//            foreach (var el in m_ctx)
//            {
//                if (el.Active)
//                {
//                    el.Offset[0] = (m_probLimit == 0 ? m_probLimit : m_probLimit + 1);
//                    el.Offset[1] = m_probLimit += (el.Prob <= 0) ? 100 : el.Prob;

//                    m_roleta[el.Offset[0]] = el;
//                    m_roleta[el.Offset[1]] = el;
//                }
//            }
//        }

//        public void ClearRoleta()
//        {
//            if (m_roleta.Any())
//            {
//                m_roleta.Clear();
//            }
//        }

//        public void Push(LotteryCtx lc)
//        {
//            m_ctx.Add(lc);
//        }

//        public void Push(uint prob, object value, uint typeid = 0)
//        {     
//            Push(new LotteryCtx
//            {
//                Active = true,
//                Prob = prob,
//                Value = value, _typeid = typeid
                
//            });
//        }      
//        public ulong GetLimitProbilidade()
//        {
//            FillRoleta();
//            return m_probLimit;
//        }

//        public int GetCountItem()
//        {
//            return m_ctx.Count;
//        }
                                      
//        public void RemoveDrawItem(LotteryCtx lc)
//        {
//            if (lc != null)
//            {
//                lc.Active = false;
//            }
//        }

//        private void ShuffleValuesRand()
//        {
//            Random rng = new Random();
//            m_randValues = m_randValues.OrderBy(x => sRandomGen.getInstance().RDevice()).ToList();
//        }

//        public LotteryCtx SpinRoleta(uint typeid = 0 ,bool removeItemDraw = false)
//        {
//            try
//            {
//                LotteryCtx lc = null;

//                // Preenche a roleta
//                FillRoleta();
//                // Gera um número aleatório entre 0 e m_probLimit
//                var high = m_roleta.OrderByDescending(c => c.Key).FirstOrDefault().Key;//ideia é pega a chave maior
//                var low = m_roleta.OrderBy(c => c.Key).FirstOrDefault().Key; //pegar a chave menor
//            trycode:
//                ulong lucky = m_randValues[(int)sRandomGen.getInstance().RIbeMt19937_64ChronoRange(0, 4)] * sRandomGen.getInstance().RDevice() % (m_probLimit == 0 ? 1 : m_probLimit + 1);

//                // Encontra o item que corresponde ao intervalo sorteado  
//                lc = m_roleta.FirstOrDefault(kvp => lucky >= kvp.Key && lucky < kvp.Key + (kvp.Value.Prob <= 0 ? 100 : kvp.Value.Prob)).Value;
//                if (typeid != 0 && lc != null && lc._typeid == typeid)
//                    goto trycode;
//                // Se removeItemDraw for true, deleta o item sorteado
//                if (removeItemDraw && lc != null)
//                {
//                    RemoveDrawItem(lc);
//                }               
//                return lc;
//            }
//            catch (Exception e)
//            {
//                throw new Exception("[Lottery::SpinRoleta][ErrorSystem] " + e.Message);
//            }
//        }


//        public class LotteryCtx
//        {
//            public bool Active { get; set; }
//            public uint Prob { get; set; }
//            public uint _typeid { get; set; }
//            public object Value { get; set; }
//            public ulong[] Offset { get; set; } = new ulong[2];
//        }

//    }
//}
