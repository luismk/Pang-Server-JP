using PangyaAPI.Network.PangyaSession;
using PangyaAPI.Utilities.Log;
using System;
using System.Collections.Generic;
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
                Param = new object();
            }
            public Action<ParamDispatch> cf;

            public object Param { get; set; }

            public int ExecCmd(ParamDispatch pd)
            {
                try
                {
                    if (cf == null)
                    {
                        return 1; // Retorna 1 se o ID não existir (cf é nulo).
                    }
                    if (pd._session.getConnected())
                    {
                        cf.Invoke(pd);
                        return 0;
                    }
                    else
                    {
                        return 1; // Retorna 1 se ocorrer uma exceção.
                    }
                }
                catch
                {
                    return 1; // Retorna 1 se ocorrer uma exceção.
                }
            }
        }

        /// <summary>
        /// adiciona o pacote e chama a sua devida funcao
        /// </summary>
        /// <param db_name="_tipo">id do pacote</param>
        /// <param db_name="_func"> funcao a ser chamada</param>
        public void addPacketCall(short _tipo,
            Action<ParamDispatch> _func)
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