using System;
using PangyaAPI.SQL;

namespace PangyaAPI.Network.Cmd
{
    public class CmdInsertBlockIp : Pangya_DB
    {
        string m_ip;
        string m_mask;
        protected override string _getName { get; } = "CmdInsertBlockIp";

        public CmdInsertBlockIp(string _ip_address, string mask)
        {
            m_ip = _ip_address;
            m_mask = mask;
        }

        public CmdInsertBlockIp()
        {
        }

        protected override void lineResult(ctx_res _result, uint _index_result)
        {
            // é um update;
            return;
        }

        protected override Response prepareConsulta()
        {
            if (string.IsNullOrEmpty(m_ip) || string.IsNullOrEmpty(m_mask))
                throw new Exception("[CmdInsertBlockIP::prepareConsulta][Error] m_ip[" + m_ip + "] or m_mask["
            + m_mask + "] is invalid");

            var r = procedure("pangya.ProcInsertBlocIP", m_ip);

            checkResponse(r, "nao conseguiu inserir um Block IP[IP=" + m_ip + ", MASK=" + m_mask + "]");
            return r;
        }

        public string getIP()
        {
            return m_ip;
        }

        public void setIP(string _ip_address)
        {
            if (!string.IsNullOrEmpty(m_ip))
                m_ip = _ip_address;
        }


        public string geMask()
        {
            return m_mask;
        }

        public void setMask(string mask)
        {
            if (!string.IsNullOrEmpty(m_mask))
                m_mask = mask;
        }
    }
}
