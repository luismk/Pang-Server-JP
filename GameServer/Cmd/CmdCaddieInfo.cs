using GameServer.Game.Manager;
using GameServer.GameType;
using PangyaAPI.SQL;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using _smp = PangyaAPI.Utilities.Log;

namespace GameServer.Cmd
{
    public class CmdCaddieInfo : Pangya_DB
    {
        uint m_uid = uint.MaxValue;
        public enum TYPE : int
        {
            ALL,
            ONE,
            FERIAS
        }
        TYPE m_type;
        int m_caddie_id;
        CaddieManager v_ci;
        protected override string _getName { get; } = "CmdCaddieInfo";

        public CmdCaddieInfo(uint _uid, TYPE _type, int _caddie_id = -1)
        {
            m_uid = _uid;
            m_type = _type;
            m_caddie_id = _caddie_id;
            v_ci = new CaddieManager();
        }

        public CaddieManager getInfo()
        {
            return v_ci;
        }

        public uint getUID()
        {
            return m_uid;
        }

        public void setUID(uint _uid)
        {
            m_uid = _uid;
        }

        public TYPE getType()
        {
            return m_type;
        }

        public void setType(TYPE _type)
        {
            m_type = _type;
        }

        public int getItemID()
        {
            return m_caddie_id;
        }

        public void setItemID(int _caddie_id)
        {
            m_caddie_id = _caddie_id;
        }
        protected override void lineResult(ctx_res _result, uint _index_result)
        { 
			checkColumnNumber(11);

            try
            {
                if (_result.data_row == null || _result.data_row.ItemArray.Length == 0)    
                    return;

                CaddieInfoEx ci = new CaddieInfoEx
                {
                    id = _result.GetUInt32(0),
                    _typeid = _result.GetUInt32(2),
                    parts_typeid = _result.GetUInt32(3),
                    level = _result.GetByte(4),
                    exp = _result.GetUInt32(5),
                    rent_flag = _result.GetByte(6)
                };

                if (_result.IsEmptyObject(7))
                    ci.end_date.CreateTime(_result.GetDateTime(7));

                ci.purchase = _result.GetByte(8);
				
                if (_result.IsEmptyObject(9))
                    ci.end_parts_date.CreateTime(_result.GetDateTime(9));
				
                ci.check_end = _result.GetByte(10);
				
				v_ci.Add(ci.id, ci);
                
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);  
            }
        }

        protected override Response prepareConsulta()
        {
            var r = procedure((m_type == TYPE.ALL) ? m_szConsulta[0] : ((m_type == TYPE.ONE) ? m_szConsulta[1] : m_szConsulta[2]),
                    Convert.ToString(m_uid) + (m_type == TYPE.ONE ? ", " + Convert.ToString(m_caddie_id) : ""));
            checkResponse(r, "nao conseguiu pegar o member info do player: " + (m_uid));
            return r;
        }
        private string[] m_szConsulta = { "pangya.ProcGetCaddieInfo", "pangya.ProcGetCaddieInfo_One", "pangya.ProcGetCaddieFerias" };
    }
}
