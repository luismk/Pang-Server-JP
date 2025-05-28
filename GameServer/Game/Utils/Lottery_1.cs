// Refatoração completa da classe Lottery com correções de aleatoriedade e estrutura de roleta
using System;
using System.Collections.Generic;
using System.Linq;

namespace Pangya_GameServer.Game.Utils
{
    public class Lottery
    {
        private readonly List<LotteryCtx> m_ctx = new List<LotteryCtx>();
        private readonly List<LotteryCtx> m_roleta = new List<LotteryCtx>();
        private ulong m_probLimit;
        private bool m_roletaValid;

        private readonly Random rand = new Random();

        public void Push(LotteryCtx lc) => m_ctx.Add(lc);

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
            m_roleta.Clear();
            m_probLimit = 0;

            foreach (var el in m_ctx.Where(e => e.Active))
            {
                el.Offset[0] = m_probLimit;
                el.Offset[1] = m_probLimit + (el.Prob > 0 ? el.Prob : 100) - 1;
                m_probLimit = el.Offset[1] + 1;
                m_roleta.Add(el);
            }

            m_roletaValid = true;
        }

        public ulong GetLimitProbilidade()
        {
            if (!m_roletaValid)
                FillRoleta();
            return m_probLimit;
        }

        public LotteryCtx SpinRoleta(bool removeItemDraw = false)
        {
            if (!m_roletaValid || m_probLimit == 0)//refaz a prob
                FillRoleta();

            if (m_probLimit == 0)
                 throw new Exception("[Lottery::SpinRoleta][Error] Nenhuma probabilidade acumulada válida.");

            ulong lucky = (ulong)rand.Next(0, (int)m_probLimit);

            var result = m_roleta.FirstOrDefault(el => lucky >= el.Offset[0] && lucky <= el.Offset[1]);

            if (result == null)
                return SpinRoleta(removeItemDraw); // Retry (raro)

            result.Prob = (uint)Math.Max(1, result.Prob / result.Penalty);

            if (removeItemDraw)
                RemoveDrawItem(result);

            result.DrawCount++;
            m_roletaValid = false; // roleta precisa ser atualizada no próximo giro
            return result;
        }

        public void RemoveDrawItem(LotteryCtx lc)
        {
            if (lc != null)
                lc.Active = false;
        }

        public int GetCountItem() => m_ctx.Count;

        public void Clear()
        {
            if (m_roleta.Any())
            {
                m_roleta.Clear();
            }
        }
        public class LotteryCtx
        {
            public bool Active { get; set; }
            public object Value { get; set; }
            public uint Prob { get; set; }
            public ulong[] Offset { get; set; } = new ulong[2]; // [0]: início, [1]: fim
            public uint _typeid { get; set; }
            public float Penalty { get; set; } = 2.0f;
            public int DrawCount { get; set; }
        }
    }
}
