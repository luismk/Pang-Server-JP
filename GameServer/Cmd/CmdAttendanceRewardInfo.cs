using GameServer.GameType;
using System;
using PangyaAPI.SQL;

namespace GameServer.Cmd
{
    internal class CmdAttendanceRewardInfo : Pangya_DB
    {
        public CmdAttendanceRewardInfo()
        {
            this.m_uid = 0;
            this.m_ari = new AttendanceRewardInfoEx();
        }

        public CmdAttendanceRewardInfo(uint _uid)
        {
            this.m_uid = _uid;
            this.m_ari = new AttendanceRewardInfoEx();
        }


        public uint getUID()
        {
            return m_uid;
        }

        public void setUID(uint _uid)
        {
            m_uid = _uid;
        }

        public AttendanceRewardInfoEx getInfo()
        {
            return m_ari;
        }

        protected override void lineResult(ctx_res _result, uint _index_result)
        {

            checkColumnNumber(6);

            m_ari.counter = IFNULL(_result.data[0]);
            m_ari.now._typeid = IFNULL(_result.data[1]);
            m_ari.now.qntd = IFNULL(_result.data[2]);
            m_ari.after._typeid = IFNULL(_result.data[3]);
            m_ari.after.qntd = IFNULL(_result.data[4]);

            if (_result.data[5] != null)
                m_ari.last_login.CreateTime(_translateDate(_result.data[5]));
        }

        protected override Response prepareConsulta()
        {
            var r = procedure(
                m_szConsulta,
                Convert.ToString(m_uid));

            checkResponse(r, "nao conseguiu pegar attendance reward info do player: " + Convert.ToString(m_uid));

            return r;
        }

        private uint m_uid;
        private AttendanceRewardInfoEx m_ari = new AttendanceRewardInfoEx();

        private const string m_szConsulta = "pangya.ProcGetAttendanceReward";
    }
}