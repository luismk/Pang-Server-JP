using System;
using System.Linq;
using PangyaAPI.SQL.Manager;
using PangyaAPI.Utilities;
using response = PangyaAPI.SQL.Response;

namespace PangyaAPI.SQL
{
    public abstract partial class Pangya_DB
    {
        protected ctx_db m_ctx_db = new ctx_db();
        protected mssql _db = new mssql();
        public Pangya_DB()
        {
            _db = new mssql();
            _db.connect();
        }


        public virtual void exec()
        {
            uint num_result = 0;
            try
            {
                response r = null;
                if ((r = prepareConsulta()) != null)
                {
                    foreach (var _result in r.getResultSet())
                    {
                        lineResult(_result.getFirstLine(), num_result);
                        num_result++;
                    }
                    clear_response(r);
                }
                else
                {
                    Console.WriteLine("[Pangya_DB::" + _getName + "::exec][Error] return prepareConsulta is null.");
                }
            }
            catch (Exception e)
            {
                m_exception = new exception(e.Message, ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PANGYA_DB, 0, 0));
            }
        }

        public virtual exception getException()
        {
            return m_exception ?? new exception("");
        }

        public virtual response _insert(string _query)
        {
            return _db.ExecQuery(_query);
        }
        public virtual response _update(string _query) { return _db.ExecQuery(_query); }

        public virtual response _delete(string _query) { return _db.ExecQuery(_query); }

        public virtual response consulta(string _query) { return _db.ExecQuery(_query); }

        public virtual response procedure(string _name, string values = null) { return _db.ExecProc(_name, values); }

        public virtual void clear_response(response _res) { }

        public virtual void checkColumnNumber(uint _number_cols1)
        {
            if (_number_cols1 <= 0)
                throw new exception("[Pangya_DB::" + _getName + "::checkColumnNumber][Error] numero de colunas retornada pela consulta sao diferente do esperado.");
        }
        public virtual void checkColumnNumber(uint _number_cols1, uint _number_cols2)
        {
            if (_number_cols1 <= 0 || _number_cols1 != _number_cols2)
                throw new exception("[Pangya_DB::" + _getName + "::checkColumnNumber][Error] numero de colunas retornada pela consulta sao diferente do esperado.");
        }

        public virtual void checkResponse(response r, string _exception_msg)
        {
            if (r == null || (r.getNumResultSet() <= 0 && r.getRowsAffected() == -1))
                throw new exception("[Pangya_DB::" + _getName + "::checkResponse][Error] " + _exception_msg, ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PANGYA_DB, 0, 0));
        }

        protected abstract void lineResult(ctx_res _result, uint _index_result);
        protected abstract response prepareConsulta();

        protected virtual string _getName { get => GetType().Name; }

        public static string _formatDate(DateTime date)
        {
            return UtilTime.FormatDate(date);
        }
        public static string formatDateLocal(long date)
        {
            return UtilTime.FormatDateLocal(date);
        }
        public static bool is_valid_c_string(object value)
        {
            if (value == null || value is DBNull || (value is string && string.IsNullOrEmpty((string)value)))
            {
                return false;
            }
            var _ptr_c_string = Convert.ToString(value);
            return _ptr_c_string != null && _ptr_c_string[0] != 0;
        }

        public static void STRCPY_TO_MEMORY_FIXED_SIZE(ref string v1, int size, object v2)
        {
            @v1 = Convert.ToString(v2);
        }


        public uint IFNULL(object value)
        {
            if (value == null || value is DBNull)
            {
                return 0;
            }

            try
            {
                if (value is int intValue && intValue == -1)
                {
                    return uint.MaxValue;
                }

                return Convert.ToUInt32(value);
            }
            catch
            {
                throw new InvalidCastException($"[{_getName}::IFNULL][Error] The provided value cannot be converted to uint.");
            }
        }

        public T IFNULL<T>(object value)
        {
            if (value == null || value is DBNull)
            {
                return default; // Retorna o valor padrão de T (ex: 0 para int, null para string)
            }

            try
            {
                return (T)Convert.ChangeType(value, typeof(T)); // Conversão segura para o tipo T
            }
            catch (Exception ex)
            {
                throw new InvalidCastException($"[{_getName}::IFNULL][Error] The provided value cannot be converted to {typeof(T).Name}.", ex);
            }
        }
        public static DateTime? _translateDate(object value)
        {
            if (value == null)
                return null;

            string[] formatos = new[] {
        "M/d/yyyy h:mm:ss tt",    // ex: 5/6/2025 12:00:00 AM
        "M/d/yyyy hh:mm:ss tt",
        "dd/MM/yyyy HH:mm:ss",
        "HH:mm:ss"                // apenas hora
    };

            if (DateTime.TryParseExact(value.ToString(), formatos,
                                       System.Globalization.CultureInfo.InvariantCulture,
                                       System.Globalization.DateTimeStyles.None,
                                       out DateTime result))
            {
                // Se for apenas horário (sem data), assume o dia de hoje
                if (value.ToString().Length == 8 && value.ToString().Count(c => c == ':') == 2)
                {
                    return DateTime.Today.Add(result.TimeOfDay);
                }

                return result;
            }

            return null;
        }

        protected exception m__exception { get; set; }
        public exception m_exception { get => m__exception; set => m__exception = value; }
    }
}