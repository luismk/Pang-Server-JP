using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;

namespace PangyaAPI.Utilities.Log
{
    public enum type_msg : int
    {
        CL_ONLY_CONSOLE,
        CL_FILE_TIME_LOG_AND_CONSOLE,
        CL_FILE_LOG_AND_CONSOLE,
        CL_ONLY_FILE_LOG,
        CL_ONLY_FILE_TIME_LOG,
        CL_ONLY_FILE_LOG_IO_DATA,
        CL_FILE_LOG_IO_DATA_AND_CONSOLE,
        CL_ONLY_FILE_LOG_TEST,
        CL_FILE_LOG_TEST_AND_CONSOLE,
    };
    public class message : IDisposable
    {
        public message()
        { }

        public message(string s, type_msg _tipo = 0)
        {
            m_message = s;
            m_tipo = _tipo;
            var time = DateTime.Now.ToString("[yyyy/MM/dd HH:mm:ss]");
            m_message = time + " " + m_message;
        }

        public void append(string s)
        {
            m_message += s;
        }
        public void set(string s)
        {
            m_message = s;
        }

        public string get() => m_message;
        public type_msg getTipo() => m_tipo;


        private string m_message;
        type_msg m_tipo;

        public void Dispose()
        {
            if (!string.IsNullOrEmpty(m_message))
            {
                m_message = "";
            }
        }
    }

    public static class message_pool
    {
        static readonly List<message> m_message = new List<message>();
        static DateTime date { get; set; }
        private static string _file;
        private static readonly object _lock = new object();

        private static void InitializeLogFile()
        {
            lock (_lock)
            {
                if (string.IsNullOrEmpty(_file))
                {
                    var logDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Log");
                    if (!Directory.Exists(logDirectory))
                    {
                        Directory.CreateDirectory(logDirectory);
                    }

                    date = DateTime.Now;
                    _file = Path.Combine(logDirectory, $"log_{date.ToString("ddMMyyyyHHmmss")}.log");
                }
            }
        }

        private static void LogOnly()
        {
            InitializeLogFile();
            var m = getMessage();
            lock (_lock)
            {
                using (var writer = new StreamWriter(_file, append: true))
                {
                    writer.WriteLine(m.get());
                }
            }
        }

        private static void LogAndConsole()
        {
            LogOnly();
            console_log();
        }


        static void console_log()
        {
            message m = getMessage();

            if (m != null)
            {
                Console.WriteLine(m.get(), ConsoleColor.Cyan);       
            }
            else
                throw new Exception("message is null. message_pool::console_log()");
        }
        public static void push(string s, type_msg _tipo = type_msg.CL_FILE_LOG_AND_CONSOLE)
        {
            if (date == DateTime.MinValue)
            {
                date = DateTime.Now;
            }
            push(new message(s, _tipo));
        }
        static string GetExceptionDetails(Exception ex)
        {
            var log = ($"[message: {ex.Message},");
            log += ($"Source: {ex.Source},");
            log += ($"Method: {ex.TargetSite},");
            log += ($"StackTrace: {ex.StackTrace}]");
            return log.ToString();
        }
        public static void push(string s, Exception exception, type_msg _tipo = type_msg.CL_FILE_LOG_AND_CONSOLE)
        {

            var log = GetExceptionDetails(exception);
            // Verificar se há exceções internas
            if (exception.InnerException != null)
            {
                log += ("------ Inner Exception ------");
                log += "\n" + (GetExceptionDetails(exception.InnerException));
            }
            s += " [ErrorSystem]: \n" + log;
            if (date == DateTime.MinValue)
            {
                date = DateTime.Now;
            }
            push(new message(s, _tipo));
        }

        public static void push(int msg2, string msg, int _code = 0, type_msg _tipo = 0)
        {
            if (date == DateTime.MinValue)
            {
                date = DateTime.Now;
            }
            var s = string.Format("[{0}] : {1}/Line({2})", msg, msg2, _code);
            push(new message(s, _tipo));
        }
        public static void push(string msg2, string msg, int _code = 0, type_msg _tipo = 0)
        {
            if (date == DateTime.MinValue)
            {
                date = DateTime.Now;
            }
            var s = string.Format("[{0}] : {1}/Line({2})", msg, msg2, _code);
            push(new message(s, _tipo));
        }
        public static void push(message m)
        {
            m_message.Add(m);
            LogAndConsole();
            m_message.Clear();
        }
        static message getMessage() { return getFirstMessage(); }

        static message getFirstMessage() { return m_message[0]; }

        public static bool checkUpdateDayLog()
        {

            bool ret = false;

            var ti_day = new DateTime();
            // Criar novos Logs que trocou o Dia do Log
            if (ti_day.Year < DateTime.Now.Year || ti_day.Month < DateTime.Now.Month || ti_day.Day < DateTime.Now.Day)
            {

                // Criou novos logs, trocou o dia do log
                if (DateTime.Now.Hour == 0 && DateTime.Now.Minute == 0 && DateTime.Now.Second == 0 && date.Millisecond == 0)
                {
                    ret = true; date = DateTime.Now;
                }
            }    
            return ret;
        }
    }
}
