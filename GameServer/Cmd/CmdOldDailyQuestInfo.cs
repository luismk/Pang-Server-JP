using Pangya_GameServer.GameType;
using System.Collections.Generic;
using System;
using PangyaAPI.SQL;

namespace Pangya_GameServer.Cmd
{
    public class CmdOldDailyQuestInfo : Pangya_DB
    {
        public CmdOldDailyQuestInfo(bool _waiter = false) : base(_waiter)
        {
            this.m_uid = 0;
            this.v_rdqu = new List<RemoveDailyQuestUser>();
        }

        public CmdOldDailyQuestInfo(uint _uid, bool _waiter = false) : base(_waiter)
        {
            this.m_uid = _uid; 
            this.v_rdqu = new List<RemoveDailyQuestUser>();
        }

        public virtual void Dispose()
        {
        }

        public List<RemoveDailyQuestUser> getInfo()
        {
            return new List<RemoveDailyQuestUser>(v_rdqu);
        }

        public uint getUID()
        {
            return (m_uid);
        }

        public void setUID(uint _uid)
        {
            m_uid = _uid;
        }

        protected override void lineResult(ctx_res _result, uint _index_result)
        {
            checkColumnNumber(2);

            v_rdqu.Add(new RemoveDailyQuestUser() { id = (int)IFNULL(_result.data[0]), _typeid = (uint)IFNULL(_result.data[1]) });
        }

        protected override Response prepareConsulta()
        {

            v_rdqu.Clear();
             
            var r = consulta(m_szConsulta + Convert.ToString(m_uid));

            checkResponse(r, "nao conseguiu pegar o(s) old daily quest do player: " + Convert.ToString(m_uid));

            return r;
        }
 
        private uint m_uid = new uint();
        private List<RemoveDailyQuestUser> v_rdqu = new List<RemoveDailyQuestUser>();

        private const string m_szConsulta = "SELECT ID_ACHIEVEMENT, typeid FROM pangya.pangya_achievement WHERE status = 1 AND UID = ";

    }
}