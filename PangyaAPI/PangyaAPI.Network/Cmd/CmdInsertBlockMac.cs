using System;
using PangyaAPI.SQL;

namespace PangyaAPI.Network.Cmd
{
    public class CmdInsertBlockMac : Pangya_DB
    {
        string m_mac_address;
        protected override string _getName { get; } = "CmdInsertBlockMac";

        public CmdInsertBlockMac(string _mac_address)
        {
            m_mac_address = _mac_address;
        }

        public CmdInsertBlockMac()
        {
        }

        protected override void lineResult(ctx_res _result, uint _index_result)
        {
            // é um update;
            return;
        }

        protected override Response prepareConsulta()
        {
            if (string.IsNullOrEmpty(m_mac_address))
                throw new Exception("[CmdInsertBlockMAC::prepareConsulta][Error] m_mac_address is empty.");

            var r = procedure("pangya.ProcInsertBlockMAC", m_mac_address);

            checkResponse(r, "nao conseguiu inserir o MAC ADDRESS[" + m_mac_address + "] para a lista de MAC bloqueado");
            return r;

        }

        public string getMACAddress()
        {
            return m_mac_address;
        }

        public void setMACAddress(string _mac_address)
        {
            m_mac_address = _mac_address;
        }
    }
}
