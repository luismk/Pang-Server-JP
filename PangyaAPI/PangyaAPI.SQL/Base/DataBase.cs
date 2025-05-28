using System;
using PangyaAPI.Utilities;
using _smp = PangyaAPI.Utilities.Log;
using response = PangyaAPI.SQL.Response;

namespace PangyaAPI.SQL
{
    public abstract class database
    {
        public enum ERROR_TYPE : uint
        {
            INVALID_HANDLE,
            INVALID_PARAMETER,
            ALLOC_HANDLE_FAIL_ENV,
            ALLOC_HANDLE_FAIL_DBC,
            ALLOC_HANDLE_FAIL_STMT,
            SET_ATTR_ENV_FAIL,
            CONNECT_DRIVER_FAIL,
            EXEC_QUERY_FAIL,
            FETCH_QUERY_FAIL,
            MORE_RESULTS,
            GERAL_ERROR,
            HAS_CONNECT
        }
        public database()
        {
            loadIni();
            this.m_state = false;
            this.m_connected = false;
        }
        public database(ctx_db _m_ctx_db)
        {
            m_ctx_db = _m_ctx_db;

            this.m_state = false;
            this.m_connected = false;

        }

        public bool loadIni()
        {
            IniHandle ini = new IniHandle("server.ini");
            try
            {
                m_ctx_db.engine = ini.ReadString("NORMAL_DB", "DBENGINE");//tipo de conexao 
                m_ctx_db.ip = ini.ReadString("NORMAL_DB", "DBIP");
                m_ctx_db.db_name = ini.ReadString("NORMAL_DB", "DBNAME");
                m_ctx_db.user = ini.ReadString("NORMAL_DB", "DBUSER");
                m_ctx_db.pass = ini.ReadString("NORMAL_DB", "DBPASS");
                m_ctx_db.port = ini.ReadUInt32("NORMAL_DB", "DBPORT", 1433);
            }
            catch (Exception ex)
            {
                m_error_string = ex.Message;
                m_error = false;
                _smp.message_pool.push("[database::loadIni][Error] " + ex.Message + "]", _smp.type_msg.CL_FILE_LOG_AND_CONSOLE);
            }
            return true;
        }
        public virtual void Dispose()
        {
        }

        public abstract void init();

        public bool is_valid()
        {
            return m_state;
        }
        public bool is_connected()
        {
            return m_connected;
        }

        public abstract bool hasGoneAway();

        public abstract void connect();
        public abstract void reconnect();
        public abstract void disconnect();

        public abstract response ExecQuery(string _query);
        public abstract response ExecProc(string _proc_name, string valor = null);
        public abstract string makeText(string _value);

        public abstract string makeEscapeKeyword(string _value); 


        public bool m_error = false;
        public string m_error_string = "";
        protected bool m_state;
        protected bool m_connected;
        protected ctx_db m_ctx_db = new ctx_db();
    } 
}
