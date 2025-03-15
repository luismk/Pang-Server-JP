using PangyaAPI.SQL;
using System;

namespace GameServer.Cmd
{
    public class CmdCheckAchievement : Pangya_DB
    {
        private uint m_uid = new uint();
        private bool m_check = false;

        private const string m_szConsulta = "pangya.ProcCheckAchievement";

        public CmdCheckAchievement(uint uid)
        {
            this.m_uid = uid;
        }

        public uint getUID()
        {
            return m_uid;
        }

        public void setUID(uint _uid)
        {
            m_uid = _uid;
        }

        public bool getLastState()
        {
            return m_check;
        }

        protected override void lineResult(ctx_res _result, uint _index_result)
        {

            checkColumnNumber(1);

            m_check = _result.GetBoolean(0);
        }

        protected override Response prepareConsulta()
        {

            m_check = false;

            var r = procedure(m_szConsulta,
                Convert.ToString(m_uid));

            checkResponse(r, "nao conseguiu verificar o achievement do player: " + Convert.ToString(m_uid));

            return r;
        }
    }
}