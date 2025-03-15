using PangyaAPI.SQL;

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using _smp = PangyaAPI.Utilities.Log;
using GameServer.GameType;
using GameServer.Game.Manager;

namespace GameServer.Cmd
{
    public class CmdMascotInfo : Pangya_DB
    {
        uint m_uid = uint.MaxValue;
        public enum TYPE : int
        {
            ALL,
            ONE,
        }
        TYPE m_type;
        uint m_item_id;
        MascotManager v_mi;
        protected override string _getName { get; } = "CmdMascotInfo";

        public CmdMascotInfo(uint _uid, TYPE _type, uint _item_id = 0)
        {
            m_uid = _uid;
            m_type = _type;
            m_item_id = _item_id;
            v_mi = new MascotManager();

        }

        public CmdMascotInfo(uint _uid, int _type, uint _item_id)
        {
            m_uid = _uid;
            m_type = (TYPE)_type;
            m_item_id = _item_id;
            v_mi = new MascotManager();

        }

        protected override void lineResult(ctx_res _result, uint _index_result)
        {
            checkColumnNumber(45);
            try
            {

                MascotInfoEx mi = new MascotInfoEx
                {
                    id = Convert.ToUInt32(_result.data[0]),
                    _typeid = Convert.ToUInt32(_result.data[2]),
                    level = (byte)Convert.ToUInt32(_result.data[3]),
                    exp = Convert.ToUInt32(_result.data[4]),
                    flag = (byte)Convert.ToUInt32(_result.data[5])
                };
                if (_result.IsNotNull(6))
                    mi.message = _result.data[6].ToString();
                mi.tipo = (short)Convert.ToUInt32(_result.data[7]);
                mi.is_cash = (byte)Convert.ToUInt32(_result.data[8]);
                mi.data.CreateTime(_translateDate(_result.data[9]));


                var it = v_mi.Where(c => c.Key == mi.id);

                if (it.FirstOrDefault().Value == null || (it.Count() == 1 && it.FirstOrDefault().Value._typeid != mi._typeid))
                    v_mi.Add(mi.id, mi);
                else if (v_mi.Where(c => c.Key == mi.id).Count() > 1)
                {

                    var er = v_mi.Where(c => c.Key == mi.id);

                    it = er.Where(c => c.Value._typeid == mi._typeid);

                    // N�o tem um igual add um novo
                    if (it == er/*End*/)
                    {

                        v_mi.Add(mi.id, mi);

                        _smp.message_pool.push("[CmdMascotInfoInfo::lineResult][WARNING] player[UID=" + (m_uid) + "] adicionou MascotInfo[TYPEID="
                                + (mi._typeid) + ", ID=" + (mi.id) + "], com mesmo id e typeid diferente de outro MascotInfoEx que tem no multimap");
                    }
                    else
                    {
                        // Tem um MascotInfoEx com o mesmo ID e TYPEID (DUPLICATA)
                        _smp.message_pool.push("[CmdMascotInfoInfo::lineResult][WARNING] player[UID=" + (m_uid) + "] tentou adicionar no multimap um MascotInfo[TYPEID="
                                + (it.First().Value._typeid) + ", ID=" + (it.First().Value.id) + "] com o mesmo ID e TYPEID, DUPLICATA");

                    }
                }
                else
                    // Tem um MascotInfoEx com o mesmo ID e TYPEID (DUPLICATA)
                    _smp.message_pool.push("[CmdMascotInfoInfo::lineResult][WARNING] player[UID=" + (m_uid) + "] tentou adicionar no multimap um MascotInfo[TYPEID="
                            + (it.First().Value._typeid) + ", ID=" + (it.First().Value.id) + "] com o mesmo ID e TYPEID, DUPLICATA");

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

            }
        }

        protected override Response prepareConsulta()
        {
            var m_szConsulta = new string[] { "pangya.ProcGetMascotInfo " + m_uid.ToString(), "pangya.ProcGetMascotInfo_One " + m_uid.ToString() + ", " + m_item_id.ToString() };

            var r = procedure(m_type == TYPE.ALL ? m_szConsulta[0] : m_szConsulta[1]);
            checkResponse(r, "nao conseguiu pegar o member info do player: " + (m_uid));
            return r;
        }


        public MascotManager getInfo()
        {
            return v_mi;
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
