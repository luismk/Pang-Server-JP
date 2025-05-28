using System;
using Pangya_GameServer.GameType;
using PangyaAPI.SQL;
using PangyaAPI.Utilities;

namespace Pangya_GameServer.Cmd
{
    public class CmdUpdateCounterItem : Pangya_DB
    {
        public CmdUpdateCounterItem()
        {
            this.m_uid = 0;
            this.m_cii = new CounterItemInfo(0);
        }
        public CmdUpdateCounterItem(uint _uid,
            CounterItemInfo _cii)
        {
            // C++ TO C# CONVERTER TASK: The following line was determined to be a copy assignment (rather than a reference assignment) - this should be verified and a 'CopyFrom' method should be created:
            this.m_uid = _uid;
            //this.
            this.m_cii = (_cii);
        }

        public uint getUID()
        {
            return (m_uid);
        }

        public void setUID(uint _uid)
        {
            // C++ TO C# CONVERTER TASK: The following line was determined to be a copy assignment (rather than a reference assignment) - this should be verified and a 'CopyFrom' method should be created:
            m_uid = _uid;

        }

        public CounterItemInfo getInfo()
        {
            // C++ TO C# CONVERTER TASK: The following line was determined to contain a copy constructor call - this should be verified and a copy constructor should be created:
            return m_cii;
        }

        public void setInfo(CounterItemInfo _cii)
        {
            // C++ TO C# CONVERTER TASK: The following line was determined to be a copy assignment (rather than a reference assignment) - this should be verified and a 'CopyFrom' method should be created:
            m_cii = _cii;
        }

        protected override void lineResult(ctx_res _result, uint _index_result)
        {

            // N�o aqui por que � um UPDATE
            return;
        }

        protected override Response prepareConsulta()
        {

            if (m_uid == 0)
            {
                throw new exception("[CmdUpdateCounterItem::prepareConsulta][Error] m_uid is invalid", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PANGYA_DB,
                    4, 0));
            }

            if (m_cii.id <= 0 || m_cii._typeid == 0)
            {
                throw new exception("[CmdUpdateCounterItem::prepareConsulta][Error] CounterItemInfo m_cii is invalid", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PANGYA_DB,
                    4, 0));
            }

            var r = _update(m_szConsulta[0] + Convert.ToString(m_cii.value) + m_szConsulta[1] + Convert.ToString(m_uid) + m_szConsulta[2] + Convert.ToString(m_cii.id));

            checkResponse(r, "nao conseguiu atualizar o Counter Item[ID=" + Convert.ToString(m_cii.id) + "] do player: " + Convert.ToString(m_uid));

            return r;
        }

        private uint m_uid = new uint();
        private CounterItemInfo m_cii = new CounterItemInfo();

        private string[] m_szConsulta = { "UPDATE pangya.pangya_counter_item SET count_num_item = ", " WHERE UID = ", " AND count_id = " };
    }
}
