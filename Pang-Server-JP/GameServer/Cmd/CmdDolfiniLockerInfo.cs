using GameServer.PangType;

using PangyaAPI.SQL;

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Cmd
{
    public class CmdDolfiniLockerInfo : Pangya_DB
    {
        readonly uint m_uid = uint.MaxValue;
        DolfiniLocker m_df = new DolfiniLocker();
        protected override string _getName { get; } = "CmdDolfiniLockerInfo";

        public CmdDolfiniLockerInfo(uint _uid)
        {
            m_uid = _uid;
        }

        protected override void lineResult(ctx_res _result, uint _index_result)
        {
            checkColumnNumber(_result.cols);
            try
            {
                uint uid_req = 0u;


            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

            }
        }

        protected override Response prepareConsulta()
        {
            var m_szConsulta = new string[] { "pangya.ProcGetDolfiniLockerInfo", "pangya.ProcGetDolfiniLockerItem" };

            var r = procedure(m_szConsulta[0], m_uid.ToString());

            checkResponse(r, "nao conseguiu pegar o dolfini locker info do player: " + (m_uid));

            var r2 = procedure(m_szConsulta[1], m_uid.ToString());

            checkResponse(r2, "nao conseguiu pegar o dolfini locker item(ns) do player: " + (m_uid));

           
            return r;
        }


        public DolfiniLocker getInfo()
        {
            return m_df;
        }

    }
}
