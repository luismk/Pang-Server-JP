using System;
using PangyaAPI.SQL;
using PangyaAPI.Utilities;

namespace Pangya_GameServer.Cmd
{
    internal class CmdUpdateLevelAndExp : Pangya_DB
    {
        private uint m_uid = new uint();
        private byte m_level;
        private uint m_exp = new uint();

        private string[] m_szConsulta = { "UPDATE pangya.user_info SET level = ", ", Xp = ", " WHERE UID = " };
        public CmdUpdateLevelAndExp()
        {
            this.m_uid = 0u;
            this.m_level = 0;
            this.m_exp = 0u;
        }

        public CmdUpdateLevelAndExp(uint _uid,
            short _level, uint _exp)
        {
            this.m_uid = _uid;
            this.m_exp = _exp;
            this.m_level = (byte)_level;
        }

        protected override void lineResult(ctx_res _result, uint _index_result)
        {

            // N�o usa por que � um UPDATE
            return;
        }

        protected override Response prepareConsulta()
        {

            if (m_uid == 0)
            {
                throw new exception("[CmdUpdateLevelAndExp::prepareConsulta][Error] m_uid is invalid(zero)", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PANGYA_DB,
                    4, 0));
            }

            var r = _update(m_szConsulta[0] + Convert.ToString((ushort)m_level) + m_szConsulta[1] + Convert.ToString(m_exp) + m_szConsulta[2] + Convert.ToString(m_uid));

            checkResponse(r, "nao conseguiu atualizar Level[value=" + Convert.ToString((ushort)m_level) + "] Exp[value=" + Convert.ToString(m_exp) + "] do player[UID=" + Convert.ToString(m_uid) + "]");

            return r;
        }
    }
}