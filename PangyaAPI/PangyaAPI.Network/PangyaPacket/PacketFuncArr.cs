using PangyaAPI.Network.PangyaSession;
using PangyaAPI.Utilities;
using PangyaAPI.Utilities.Log;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace PangyaAPI.Network.PangyaPacket
{
    /// <summary>
    /// get packet and session
    /// </summary>
    public class ParamDispatch
    {
        public SessionBase _session { get; set; }
        public Packet _packet { get; set; }
    }

    public class func_arr
    {
        static int MAX_CALL_FUNC_ARR = 10000; // Era 500, era 1000
        protected func_arr_ex[] m_func; // pacotes de recv
        public func_arr()
        {
            m_func = new func_arr_ex[MAX_CALL_FUNC_ARR];
            for (int i = 0; i < MAX_CALL_FUNC_ARR; i++)
            {
                m_func.SetValue(new func_arr_ex(), i);
            }

        }
        public class func_arr_ex
        {
            public func_arr_ex()
            {                                
            }
            public Func<ParamDispatch, int> cf;

            public int ExecCmd(ParamDispatch pd)
            {
                // Captura o nome do método atual
                string methodName = cf != null ? cf.Method.Name : "NULL";

                // Cria um stopwatch para medir o tempo
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start(); // Inicia a contagem do tempo

                try
                {
                    if (cf == null)
                    {
                        return 1; // Retorna 1 se o ID não existir (cf é nulo).
                    }

                    // Invoca a função callback e pega o resultado
                    int result = cf.Invoke(pd);

                    // Parar o stopwatch
                    stopwatch.Stop();

                    // Exibe o nome da função e o tempo que demorou para ser executada
                    Console.WriteLine($"Function: {methodName}, Execution Time: {stopwatch.ElapsedMilliseconds} ms");

                    return result; // Retorna o resultado do callback
                }
                catch (Exception e)
                {
                    // Parar o stopwatch em caso de exceção
                    stopwatch.Stop();

                    // Exibe o erro e o tempo gasto até a exceção
                    Console.WriteLine($"Function: {methodName}, Error: {e.Message}, Execution Time: {stopwatch.ElapsedMilliseconds} ms");

                    throw; // Re-throw the exception
                }
            }
        }

        /// <summary>
        /// adiciona o pacote e chama a sua devida funcao
        /// </summary>
        /// <param db_name="_tipo">id do pacote</param>
        /// <param db_name="_func"> funcao a ser chamada</param>
        public void addPacketCall(short _tipo,
            Func<ParamDispatch, int> _func)
        {
            m_func[_tipo].cf = _func;
        }

        public func_arr_ex getPacketCall(short _tipo)
        {
            if (m_func[_tipo] != null)
            {
                return m_func[_tipo];
            }
            else
            {
                throw new Exception("[new func_arr().getPacketCall][Error] Tipo: " + Convert.ToString(_tipo) + "(0x" + _tipo + "), desconhecido ou nao implementado.");
            }
        }
    }
}