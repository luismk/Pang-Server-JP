using GameServer.Game.Manager;
using GameServer.PangType;
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
        }
        TYPE m_type;
        uint m_item_id;
        CaddieManager v_ci;
        protected override string _getName { get; } = "CmdCaddieInfo";

        public CmdCaddieInfo(uint _uid, TYPE _type, uint _item_id = 0)
        {
            m_uid = _uid;
            m_type = _type;
            m_item_id = _item_id;
            v_ci = new CaddieManager();

        }
        /// <summary>
        /// inicia a consulta
        /// </summary>
        /// <param db_name="_uid">player id</param>
        /// <param db_name="_type">todos os item = 0 </param>
        /// <param db_name="_item_id"></param>
        public CmdCaddieInfo(uint _uid, int _type, uint _item_id)
        {
            m_uid = _uid;
            m_type = (TYPE)_type;
            m_item_id = _item_id;
            v_ci = new CaddieManager();
        }

        protected override void lineResult(ctx_res _result, uint _index_result)
        { 
            try
            {
                if (_result.cols <= 1)
                {
                    return;
                }
                CaddieInfoEx ci = new CaddieInfoEx();   
                ci.id = _result.GetUInt32(0);
                ci._typeid = _result.GetUInt32(1);
                ci.parts_typeid = _result.GetUInt32(3);
                ci.level = _result.GetByte(4);
                ci.exp = _result.GetUInt32(5);
                ci.rent_flag = _result.GetByte(6);
                if (!_result.IsNotNull(7))
                    ci.end_date.CreateTime(_result.GetDateTime(7));

                ci.purchase = _result.GetByte(8);
                if (!(_result.IsNotNull(9)))
                    ci.end_parts_date.CreateTime(_result.GetDateTime(9));
                ci.check_end = _result.GetByte(10);

                var it = v_ci.Where(c => c.Key == ci.id);

                if (it.FirstOrDefault().Value == null || (it.Count() == 1 && it.FirstOrDefault().Value._typeid != ci._typeid))
                    v_ci.Add(ci.id, ci);
                else if (v_ci.Where(c => c.Key == ci.id).Count() > 1)
                {

                    var er = v_ci.Where(c => c.Key == ci.id);

                    it = er.Where(c => c.Value._typeid == ci._typeid);

                    // N�o tem um igual add um novo
                    if (it == er/*End*/)
                    {

                        v_ci.Add(ci.id, ci);

                        _smp.message_pool.push("[CmdCaddieInfoInfo::lineResult][WARNING] player[UID=" + (m_uid) + "] adicionou CaddieInfo[TYPEID="
                                + (ci._typeid) + ", ID=" + (ci.id) + "], com mesmo id e typeid diferente de outro CaddieInfoEx que tem no multimap");
                    }
                    else
                    {
                        // Tem um CaddieInfoEx com o mesmo ID e TYPEID (DUPLICATA)
                        _smp.message_pool.push("[CmdCaddieInfoInfo::lineResult][WARNING] player[UID=" + (m_uid) + "] tentou adicionar no multimap um CaddieInfo[TYPEID="
                                + (it.First().Value._typeid) + ", ID=" + (it.First().Value.id) + "] com o mesmo ID e TYPEID, DUPLICATA");

                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

            }
        }

        protected override Response prepareConsulta()
        {
            var m_szConsulta = new string[] { "pangya.ProcGetCaddieInfo ", "pangya.ProcGetCaddieInfo_One " };  
            var r = procedure(m_type == TYPE.ALL ? m_szConsulta[0] + m_uid.ToString() : m_szConsulta[1] + m_uid.ToString() + ", "+ m_item_id.ToString());
            checkResponse(r, "nao conseguiu pegar o member info do player: " + (m_uid));
            return r;
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

        public uint getItemID()
        {
            return m_item_id;
        }

        public void setItemID(uint _item_id)
        {
            m_item_id = _item_id;
        }
    }
}
