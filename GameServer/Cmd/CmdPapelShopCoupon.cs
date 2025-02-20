using GameServer.Game.Manager;
using GameServer.GameType;
using PangyaAPI.SQL;
using PangyaAPI.Utilities.Log;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using _smp = PangyaAPI.Utilities.Log;
namespace GameServer.Cmd
{
    public class CmdPapelShopCoupon : Pangya_DB
    {
        public CmdPapelShopCoupon()
        {
            this.m_ctx_psc = new Dictionary<uint, ctx_papel_shop_coupon>();
        }               

        public Dictionary<uint, ctx_papel_shop_coupon> getInfo()
        {
            return new Dictionary<uint, ctx_papel_shop_coupon>(m_ctx_psc);
        }

        protected override void lineResult(ctx_res _result, uint _index_result)
        {

            checkColumnNumber(2);

            ctx_papel_shop_coupon ctx_psc = new ctx_papel_shop_coupon
            {
                _typeid = IFNULL(_result.data[0]),
                active = (byte)IFNULL(_result.data[1])
            };

            var it = m_ctx_psc.FirstOrDefault(c => c.Value._typeid == ctx_psc._typeid);

            if (it.Value == null) // N�o tem add um novo coupon
                m_ctx_psc.Add(ctx_psc._typeid, ctx_psc);
        }

        protected override Response prepareConsulta()
        {

            var r = consulta(m_szConsulta);

            checkResponse(r, "nao conseguiu pegar os papel shop coupon(s).");

            return r;
        }

        private Dictionary<uint, ctx_papel_shop_coupon> m_ctx_psc = new Dictionary<uint, ctx_papel_shop_coupon>();

        private const string m_szConsulta = "SELECT typeid, active FROM pangya.pangya_papel_shop_coupon";
    }
}
