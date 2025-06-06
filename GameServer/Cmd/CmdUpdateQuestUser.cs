﻿using System;
using Pangya_GameServer.GameType;
using PangyaAPI.SQL;
using PangyaAPI.Utilities;

namespace Pangya_GameServer.Cmd
{
    public class CmdUpdateQuestUser : Pangya_DB
    {
        public CmdUpdateQuestUser(uint _uid,
            QuestStuffInfo _qsi)
        {
            this.m_uid = _uid;
            this.m_qsi = _qsi;
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

        public QuestStuffInfo getInfo()
        {
            return m_qsi;
        }

        public void setInfo(QuestStuffInfo _qsi)
        {
            m_qsi = _qsi;
        }

        protected override void lineResult(ctx_res _result, uint _index_result)
        {

            // N�o usa por que � um UPDATE
            return;
        }

        protected override Response prepareConsulta()
        {

            if (m_qsi.id <= 0
                || m_qsi._typeid == 0
                || m_qsi.counter_item_id <= 0)
            {
                throw new exception("[CmdUpdateQuestUser::prepareConsulta][Error] QuestStuffInfoEx m_qsi is invalid", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PANGYA_DB,
                    4, 0));
            }

            string clear_dt = "NULL";
             
            if (m_qsi.clear_date_unix != 0)
                clear_dt = "'" + DateTimeOffset.FromUnixTimeSeconds(m_qsi.clear_date_unix)
                                               .ToString("yyyy-MM-dd HH:mm:ss.fffffff") + "'";

            var query = string.Format(
        m_szConsulta,
        Convert.ToString(m_qsi.counter_item_id),  // {0}
        clear_dt,                                 // {1}
        Convert.ToString(m_uid),                  // {2}
        Convert.ToString(m_qsi.id)                // {3}
    );

            var r = _update(query);


            checkResponse(r, "nao conseguiu atualizar a quest[ID=" + Convert.ToString(m_qsi.id) + "] do player: " + Convert.ToString(m_uid));

            return r;
        }
        private uint m_uid = new uint();
        private QuestStuffInfo m_qsi = new QuestStuffInfo();
        private const string m_szConsulta = "UPDATE pangya.pangya_quest SET counter_item_id = {0}, [date] = {1} WHERE UID = {2} AND id = {3};";
    }
}
