using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using _smp = PangyaAPI.Utilities.Log;
using PangyaAPI.Utilities;
using PangyaAPI.SQL.Manager;
namespace PangyaAPI.SQL
{
    public class NormalDB
    {

        public enum TT_DB : uint
        {
            TT_NORMAL_EXEC_QUERY,
            TT_NORMAL_RESPONSE
        }

        public class msg_t 
        {
            public msg_t(int _id,  Pangya_DB __pangya_db, Action<int, Pangya_DB, object> _callback_response, object _arg)
            {
                this.id = _id;
                this._pangya_db = __pangya_db;
                this.func = _callback_response;
                this.arg = _arg;
            }
           
            public void execFunc()
            {
                if (func == null)
                {
                    return;
                }

                try
                {
                    if (_pangya_db == null)
                    {
                        throw new System.Exception("_pangya_db is null");
                    }

                    func.Invoke(id, _pangya_db, arg);
                    sucess = true;
                }
                catch (exception e)
                {
                    _smp.message_pool.push("[NormalDB::mgs_t::execFunc][Error] " + e.getFullMessageError());
                }
            }

            public void execQuery()
            {                    
                try
                {
                    if (_pangya_db == null)
                    {
                        throw new System.Exception("[NormalDB::mgs_t::execQuery][Error] _pangya_db is null");
                    }
                    sucess = false;
                    _pangya_db.exec();
                }
                catch (Exception ex)
                {            
                    throw ex;
                }
            }
         
            protected int id; // ID da msg
            protected Pangya_DB _pangya_db;
            protected Action<int, Pangya_DB, object> func;
            protected object arg;
            public bool sucess;
        }

        public NormalDB()
        {
            m_unExec = new List<msg_t>();
            m_unResponse = new List<msg_t>();
        }

        public void Init()
        {                
            _db = new mssql();         
            _db.connect();

            m_pExec = new Thread(new ThreadStart(RunExecQuery));
            m_pResponse = new Thread(new ThreadStart(RunResponse));
            m_pExec.Start();
            m_pResponse.Start();
        }

        public void RunExecQuery()
        {
            for (int i = 0; i < m_unExec.Count; i++)
            {
                msg_t _msg = m_unExec[i];

                if (_msg != null && m_unResponse.Any(c => c == _msg) == false)
                {
                    m_unResponse.Add(_msg);
                }

                if (_msg != null && m_unResponse.Any(c => c == _msg))
                {
                    m_unResponse.Remove(_msg);
                    m_unExec.Remove(_msg);
                }
            }
        }

        void RunResponse()
        {
            if (m_unResponse.Count > 0)
            {
                for (int i = 0; i < m_unResponse.Count; i++)
                {
                    msg_t _msg = m_unResponse[i];

                    if (_msg != null)
                        _msg.execFunc();
                }
            }

        }
                                                                                                 

        protected Thread m_pExec;
        protected Thread m_pResponse;                
        protected bool m_state;
        protected uint m_continue_exec;
        protected uint m_continue_response;
        protected uint m_free_all_waiting;
        List<msg_t> m_unExec = new List<msg_t>();
        List<msg_t> m_unResponse = new List<msg_t>();
        private mssql _db;
    }
}
