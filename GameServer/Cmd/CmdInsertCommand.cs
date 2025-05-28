using System;
using Pangya_GameServer.GameType;
using PangyaAPI.SQL;

namespace Pangya_GameServer.Cmd
{
    public class CmdInsertCommand : Pangya_DB
    {
        public CmdInsertCommand(CommandInfo _ci)
        {
            this.m_ci = _ci;
        }


        public CommandInfo getInfo()
        {

            return m_ci;
        }

        public void setInfo(CommandInfo _ci)
        {
            m_ci = _ci;
        }

        protected override void lineResult(ctx_res _result, uint _index_result)
        {

            // N�o usa por que � um INSERT
            return;
        }

        protected override Response prepareConsulta()
        {
            object reserveDate = DBNull.Value; // Default to DBNull.Value

            if (m_ci.reserveDate != null && !m_ci.reserveDate.IsEmpty)
            {
                reserveDate = _db.makeText(_formatDate(m_ci.reserveDate.ConvertTime()));
            }

            var r = procedure(
                m_szConsulta,
                Convert.ToString(m_ci.id) + ", " +
                Convert.ToString(m_ci.arg[0]) + ", " +
                Convert.ToString(m_ci.arg[1]) + ", " +
                Convert.ToString(m_ci.arg[2]) + ", " +
                Convert.ToString(m_ci.arg[3]) + ", " +
                Convert.ToString(m_ci.arg[4]) + ", " +
                Convert.ToString(m_ci.target) + ", " +
                Convert.ToString(m_ci.flag) + ", " +
                Convert.ToString((ushort)m_ci.valid) + ", " +
                reserveDate);

            checkResponse(r, "nao conseguiu adicionar o Comando[" + m_ci.ToString() + "]");

            return r;
        }



        private CommandInfo m_ci = new CommandInfo();

        private const string m_szConsulta = "pangya.ProcInsertCommand";
    }
}