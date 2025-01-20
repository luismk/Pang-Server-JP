
using PangyaAPI.SQL;

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PangyaAPI.TCP.Cmd
{
    public class CmdVerifyNick: Pangya_DB
    {
        int m_uid = -1;
        string m_nick = "";
        bool m_check = false;
       
        public CmdVerifyNick(string nick)
        {
            m_nick = nick;
            m_check = false;
        }

        protected override void lineResult(ctx_res _result, uint _index_result)
        {
            checkColumnNumber(1);
            try
            {
                m_uid = int.Parse(_result.data[0].ToString());
                m_check = m_uid > 0;

                if (!m_check)
                    throw new Exception("[CmdVerify::prepareConsulta][Error] Nick invalid");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

            }
        }

        protected override Response prepareConsulta()
        {
            if (string.IsNullOrEmpty(m_nick))
                throw new Exception("[CmdVerify::prepareConsulta][Error] Nick invalid");

            m_check = false;
            m_uid = 0;

            var r = procedure("pangya.ProcVerifyNickname", m_nick);

            checkResponse(r, "nao conseguiu verificar se existe o nick: " + m_nick);
            return r;
        }

        public bool getLastCheck() => m_check;

    }
}
