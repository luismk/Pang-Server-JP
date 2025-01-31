using PangyaAPI.Network.Pangya_St;
using PangyaAPI.SQL;

using System;
using System.Collections.Generic;
using System.Data;

namespace PangyaAPI.Network.Cmd
{
    public class CmdUpdateRateConfigInfo: Pangya_DB
    {
        int m_server_uid = -1;
        bool m_error = false;
        RateConfigInfo m_rci;
        protected override string _getName { get; } = "CmdUpdateRateConfigInfo";
       

        public CmdUpdateRateConfigInfo(int _uid, RateConfigInfo _rate)
        {
            m_server_uid = _uid;
            m_rci = _rate;
        }
        public CmdUpdateRateConfigInfo()
        {
        }

        protected override void lineResult(ctx_res _result, uint _index_result)
        {
              //somente update!
        }

        protected override Response prepareConsulta()
        {

            if (m_server_uid == 0u)
                throw new Exception("[CmdUpdateRateConfigInfo][Error] server_uid[VALUE=" + (m_server_uid) + "] is invalid.");

            var r = procedure("pangya.ProcUpdateRateConfigInfo", m_server_uid.ToString() + ", " + m_rci.grand_zodiac_event_time.ToString()  + ", " + m_rci.scratchy.ToString()  + ", " + m_rci.papel_shop_rare_item.ToString()  + ", " + m_rci.papel_shop_cookie_item.ToString()  + ", " + m_rci.treasure.ToString()  + ", " + m_rci.pang.ToString()  + ", " + m_rci.exp.ToString()  + ", " + m_rci.club_mastery.ToString()  + ", " + m_rci.chuva.ToString()  + ", " + m_rci.memorial_shop.ToString()  + ", " + m_rci.angel_event.ToString()  + ", " + m_rci.grand_prix_event.ToString()  + ", " + m_rci.golden_time_event.ToString()  + ", " + m_rci.login_reward_event.ToString()  + ", " + m_rci.bot_gm_event.ToString()  + ", " + m_rci.smart_calculator.ToString()  + ", " + ParameterDirection.Input);

            checkResponse(r, "nao conseguiu atualizar o Rate Config Info[SERVER_UID=" + (m_server_uid) + ", " + m_rci.ToString() + "]");
            return r;
        }

        public RateConfigInfo GetInfo()
        {
            return this.m_rci;
        }


        public bool isError()
        {
            return m_error;
        }
    }
}
