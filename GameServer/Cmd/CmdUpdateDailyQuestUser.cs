
using System;
using Pangya_GameServer.GameType;
using PangyaAPI.SQL;
using PangyaAPI.Utilities;

namespace Pangya_GameServer.Cmd
{
    public class CmdUpdateDailyQuestUser : Pangya_DB
    {
        public CmdUpdateDailyQuestUser()
        {
            this.m_uid = 0u;
            this.m_dqiu = new DailyQuestInfoUser(0);
        }

        public CmdUpdateDailyQuestUser(uint _uid,
            DailyQuestInfoUser _dqiu)
        {
            this.m_uid = _uid;
            this.m_dqiu = new DailyQuestInfoUser(_dqiu);
        }

        public virtual void Dispose()
        {
        }

        public uint getUID()
        {
            return (m_uid);
        }

        public void setUID(uint _uid)
        {
            m_uid = _uid;
        }

        public DailyQuestInfoUser getInfo()
        {
            return m_dqiu;
        }

        public void setInfo(DailyQuestInfoUser _dqiu)
        {
            m_dqiu = _dqiu;
        }

        protected override void lineResult(ctx_res _result, uint _index_result)
        {

            // N�o usa por que � um UPDATE
            return;
        }

        protected override Response prepareConsulta()
        {

            if (m_uid == 0u)
            {
                throw new exception("[CmdUpdateDailyQuestUser][Error] m_uid is invalid(zero).", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PANGYA_DB,
                    4, 0));
            }

            string accept_dt = "null";
            string today_dt = "null";

            if (m_dqiu.accept_date != 0)
            {
                accept_dt = _db.makeText(formatDateLocal(m_dqiu.accept_date));
            }

            if (m_dqiu.current_date != 0)
            {
                today_dt = _db.makeText(formatDateLocal(m_dqiu.current_date));
            }

            var r = procedure(
                m_szConsulta,
                Convert.ToString(m_uid) + ", " + accept_dt + ", " + today_dt);

            checkResponse(r, "nao conseguiu Atualizar o DailyQuest[ACCEPT_DT=" + accept_dt + ", TODAY_DT=" + today_dt + "] do player[UID=" + Convert.ToString(m_uid) + "]");

            return r;
        }


        private uint m_uid = new uint();
        private DailyQuestInfoUser m_dqiu = new DailyQuestInfoUser();

        private const string m_szConsulta = "pangya.ProcUpdateDailyQuestUser";
    }
}
