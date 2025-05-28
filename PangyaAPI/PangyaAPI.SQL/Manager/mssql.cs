using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using PangyaAPI.Utilities;
using _smp = PangyaAPI.Utilities.Log;
using response = PangyaAPI.SQL.Response;
using result_set = PangyaAPI.SQL.Result_Set;

namespace PangyaAPI.SQL.Manager
{
    public class mssql : database
    {

        ~mssql()
        {
            init();
        }
        /// <summary>
        ///recria dados da conexao em uma unica linha de string
        /// </summary>
        /// <returns></returns>

        public override void init()
        {
            m_ctx.hDbc = new SqlConnection();
            m_ctx.hEnv = new SqlCommand();
            m_ctx.hStmt = new DataSet();
            m_state = true;
        }

        public virtual void destroy()
        {

            if (is_connected())
                disconnect();

            if (m_ctx.hDbc != null)
                m_ctx.hDbc = null;

            if (m_ctx.hEnv != null)
                m_ctx.hEnv = null;

            m_state = false;
        }


        public override bool hasGoneAway()
        {
            return false;
        }


        public override void connect()
        {
            try
            {

                if (!is_valid())
                    init();

                if (is_connected())
                    throw new exception("[mssql::connect][Error] Ja esta connectado.");

                if (m_error)
                    throw new exception(m_error_string);

                if (m_ctx.hDbc != null && m_ctx.hDbc.State == ConnectionState.Closed)
                {
                    m_ctx.hDbc.ConnectionString = m_ctx_db.CreateStrConnection();
                    m_ctx.hDbc.Open();
                }

                m_connected = true;
            }
            catch (Exception ex)
            {
                _smp.message_pool.push("[mssql::Connect][Error] " + ex.Message + "]", _smp.type_msg.CL_FILE_LOG_AND_CONSOLE);
                m_connected = false;
            }
        }

        public override void reconnect()
        {
            disconnect();
            connect();
        }

        public override void disconnect()
        {
            if (is_connected())
            {

                if (m_ctx.hDbc != null)
                    m_ctx.hDbc.Close();
            }

            m_connected = false;
        }


        public override response ExecQuery(string _query)
        {
            response res = new response();
            result_set result = null;
            uint numResults = 0;
            int numRows;
            int i;
            try
            {
                HandleDiagnosticRecord(_query);
                if (m_ctx.hStmt != null)
                {
                    var _data = m_ctx.hStmt.Tables[m_ctx_db.db_name];
                    if (_data == null)
                    {
                        res = new response();
                        res.addResultSet(new result_set((uint)result_set.STATE_TYPE._NO_DATA, 0, 0, null));
                        return res;
                    }
                    if (_data.Rows.Count == 1)
                    {
                        numResults = 1;
                    }
                    if (_data.Rows.Count > 1)
                    {
                        numResults = (uint)_data.Rows.Count - 1;
                    }
                    numRows = _data.Columns.Count;
                    res.setRowsAffected(numRows);
                    var ret = 0;
                    if (numResults > 0)
                    {
                        while (ret < numResults)
                        {
                            for (i = 0; i < numResults; i++)
                            {
                                result = new result_set((uint)result_set.STATE_TYPE.HAVE_DATA, numResults, (uint)numRows, _data.Rows[i]);

                                res.addResultSet(result);
                            }
                            ret++;
                        }
                        res.addResultSet(result);
                    }
                }
                return res;
            }
            catch (Exception ex)
            {

                // Montar a string de comando para execução do procedimento
                var commandText = $"{_query}";

                // A mensagem completa da exceção
                string mensagemErro = string.Format(
                    "[mssql::ExecQuery][Error]: {0}, [Query]: {1}",
                    ex.Message, commandText
                );

                // Enviar a mensagem para o message_pool
                _smp.message_pool.push(mensagemErro, _smp.type_msg.CL_FILE_LOG_AND_CONSOLE); return res;
            }
        }
        public override response ExecProc(string _proc_name, string valor = null)
        {
            response res = new response();
            uint numResults = 0;
            int numRows = 0;
            try
            {
                HandleDiagnosticRecord(_proc_name, valor);
                if (m_ctx.hStmt != null && m_ctx.hStmt.Tables[m_ctx_db.db_name] != null)
                {
                    var _data = m_ctx.hStmt.Tables[m_ctx_db.db_name];
                    if (_data != null && _data.Rows.Count == 1)
                    {
                        numResults = 1;
                    }
                    if (_data.Rows.Count > 1)
                    {
                        numResults = (uint)_data.Rows.Count - 1;
                    }
                    numRows = _data.Columns.Count;
                    res.setRowsAffected(numRows);
                    if (numResults > 0)
                    {
                        foreach (DataRow item in _data.Rows)
                        {
                            res.addResultSet(new result_set((uint)result_set.STATE_TYPE.HAVE_DATA, numResults, (uint)numRows, item));
                        }
                    }
                }
                return res;
            }
            catch (Exception ex)
            {
                // Montar a string de comando para execução do procedimento
                var commandText = $"EXEC {m_ctx_db.db_name}.{_proc_name} ";

                if (!string.IsNullOrEmpty(valor))
                {
                    // Divide os valores com base na vírgula
                    var valorArray = valor.Split(',')
                                            .Select(v => v.Trim()) // Remove espaços em branco
                                            .Select(v => $"'{v}'") // Adiciona aspas simples
                                            .ToArray();

                    // Junta os valores formatados de volta em uma string
                    commandText += string.Join(", ", valorArray);
                }

                // A mensagem completa da exceção
                string mensagemErro = string.Format(
                    "[mssql::ExecProc][Error]: {0}, [Query]: {1}",
                    ex.Message, commandText
                );

                // Enviar a mensagem para o message_pool
                _smp.message_pool.push(mensagemErro, _smp.type_msg.CL_FILE_LOG_AND_CONSOLE);
                return res;
            }
        }
        public override string makeText(string _value)
        {
            return "N'" + _value + "'";
        }
        public override string makeEscapeKeyword(string _value)
        {
            return "[" + _value + "]";
        }

        protected class ctx_db
        {
            public SqlCommand hEnv;
            public SqlConnection hDbc;
            public DataSet hStmt;
        }

        protected ctx_db m_ctx = new ctx_db();
        protected void HandleDiagnosticRecord(string query)
        {

            try
            {
                if (m_ctx.hDbc != null)
                {
                    m_ctx.hDbc = new SqlConnection(m_ctx_db.CreateStrConnection());
                    m_ctx.hDbc.Open();
                    var da = new SqlDataAdapter(query, m_ctx.hDbc);
                    da.Fill(m_ctx.hStmt, m_ctx_db.db_name);
                    m_ctx.hDbc.Close();
                }
            }
            catch (exception ex)
            {
                _smp.message_pool.push("[mssql::HandleDiagnosticQuery][Error] " + ex.getFullMessageError() + "]", _smp.type_msg.CL_FILE_LOG_AND_CONSOLE);
            }
        }

        protected void HandleDiagnosticRecord(string _proc_name, string valores = null)
        {
            try
            {
                m_ctx.hDbc = new SqlConnection(m_ctx_db.CreateStrConnection());
                m_ctx.hDbc.Open();
                // Montar a string de comando para execução do procedimento
                var commandText = $"EXEC {m_ctx_db.db_name}.{_proc_name} ";

                if (!string.IsNullOrEmpty(valores))
                {
                    // Verifica se os valores estão no formato de uma sequência separada por '|'
                    if (valores.Contains("|"))
                    {
                        var valorArray = valores.Split(',') // Dividindo corretamente pelos pipes
                                              .Select(v => v.Trim()) // Remover espaços em branco
                                              .Select(v => $"'{v}'") // Adiciona aspas simples     
                                              .ToArray();

                        commandText += string.Join(", ", valorArray);
                    }
                    else
                    {
                        // Divide os valores com base na vírgula
                        var valorArray = valores.Split(',')
                                                .Select(v => v.Trim()) // Remove espaços em branco
                                                .Select(v => $"'{v}'") // Adiciona aspas simples
                                                .ToArray();

                        // Junta os valores formatados de volta em uma string
                        commandText += string.Join(", ", valorArray);
                    }
                }


                m_ctx.hEnv = new SqlCommand(commandText, m_ctx.hDbc);
                var da = new SqlDataAdapter(m_ctx.hEnv);
                da.Fill(m_ctx.hStmt, m_ctx_db.db_name);
            }
            catch (exception ex)
            {
                _smp.message_pool.push("[mssql::HandleDiagnosticQuery][Error] " + ex.getFullMessageError() + "]", _smp.type_msg.CL_FILE_LOG_AND_CONSOLE);
            }
        }
    }
}
