using System;
using System.Collections.Generic;
using System.Linq;
using PangyaAPI.Utilities;
using _smp = PangyaAPI.Utilities.Log;
namespace PangyaAPI.SQL.Manager
{
    public static class NormalManagerDB
    {
        static NormalManager normalManager = new NormalManager();
        public static void add(int _id,
            Pangya_DB _pangya_db,
            Action<int, Pangya_DB, object> _callback_response,
            object _arg)
        {
            normalManager.add(_id, ref _pangya_db, _callback_response, _arg);
        }
        public static void create(uint _db_instance_num = 26)
        {
            normalManager.create(_db_instance_num);
        }
    }
    public partial class NormalManager
    {
        protected List<NormalDB> m_dbs = new List<NormalDB>();

        protected bool m_state;
        uint m_db_instance_num;
        public NormalManager()
        {
            this.m_state = false;
            this.m_dbs = new List<NormalDB>();
            this.m_db_instance_num = 26;

            for (int i = 0; i < m_db_instance_num; i++)
            {
                m_dbs.Add(new NormalDB());
            }
        }
        public void create(uint _db_instance_num = 26)
        {

            try
            {

                if (!m_state)
                {

                    m_db_instance_num = _db_instance_num;

                    // Verifica DB Instance Number
                    checkDBInstanceNumAndFix();

                    // Create Data base Instance and initialize
                    for (var i = 0; i < m_db_instance_num; ++i)
                    {

                        // Make New Normal DB Instance if not reach of limit of instances
                        if (m_dbs.Count() == 0 || m_dbs.Count() < m_db_instance_num)
                        {
                            m_dbs.Add(new NormalDB());
                        }

                        // Initialize DB Instance
                        if (m_dbs[i] != null)
                        {
                            m_dbs[i].Init();
                        }
                    }

                    // Initialize with Success
                    m_state = true;
                }

            }
            catch (exception e)
            {

                // Failed to initialize
                m_state = false;

                _smp.message_pool.push("[NormalManagerDB::create][ErrorSystem] " + e.getFullMessageError());
            }
        }

        private void checkDBInstanceNumAndFix()
        {
            // check m_db_instance_num is valid
            if ((int)m_db_instance_num <= 0)
                m_db_instance_num = 26;
        }


        public int add(NormalDB.msg_t _msg)
        {
            _msg.execQuery();
            _msg.execFunc();
            return 0;
        }
        public int add(int _id,
            ref Pangya_DB _pangya_db,
        Action<int, Pangya_DB, object> _callback_response,
            object _arg)
        {

            add(new NormalDB.msg_t(_id, _pangya_db, _callback_response, _arg));

            return 0;
        }
    }
}