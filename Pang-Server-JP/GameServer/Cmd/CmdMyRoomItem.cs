using GameServer.PangType;
using PangyaAPI.SQL;
using PangyaAPI.Utilities;
using System.Collections.Generic;
using System;

namespace GameServer.Cmd
{
    internal class CmdMyRoomItem : Pangya_DB
    {
        public enum TYPE : byte
        {
            ALL,
            ONE
        }

        public CmdMyRoomItem()
        {
            this.m_uid = 0u;
            this.m_item_id = uint.MaxValue;
            this.m_type = TYPE.ALL;
            this.v_mri = new List<MyRoomItem>();
        }

        public CmdMyRoomItem(uint _uid,
            TYPE _type,
            uint _item_id = uint.MaxValue)
        {
             this.m_uid = _uid;         
            this.m_type = (_type);
           this.m_item_id = _item_id;          
            this.v_mri = new List<MyRoomItem>();
        }
           
        public List<MyRoomItem> getMyRoomItem()
        {
            return new List<MyRoomItem>(v_mri);
        }

        public uint getUID()
        {
            return m_uid;
        }

        public void setUID(uint _uid)
        {
        m_uid = _uid;
        }

        public uint getItemID()
        {
            return m_item_id;
        }

        public void setItemID(uint _item_id)
        {
             m_item_id = _item_id;           
        }

        public CmdMyRoomItem.TYPE getType()
        {
            return m_type;
        }

        public void setType(TYPE _type)
        {
            m_type = _type;
        }

        protected override void lineResult(ctx_res _result, uint _index_result)
        {

            checkColumnNumber(9);

            MyRoomItem mri = new MyRoomItem();
            uint uid_req = 0u;

            mri.id = IFNULL(_result.data[0]);
            uid_req = IFNULL(_result.data[1]);
            mri._typeid = IFNULL(_result.data[2]);
            mri.number = (short)IFNULL(_result.data[3]);
            mri.location.x = (float)IFNULL(_result.data[4]);
            mri.location.y = (float)IFNULL(_result.data[5]);
            mri.location.z = (float)IFNULL(_result.data[6]);
            mri.location.r = (float)IFNULL(_result.data[7]);
            mri.equiped = (byte)IFNULL(_result.data[8]);

            v_mri.Add(mri);

            if (uid_req != m_uid)
            {
                throw new exception("[CmdMyRoomItem::lineResult][Error] o uid do my room item requisitado do player e diferente. UID_req: " + Convert.ToString(uid_req) + " != " + Convert.ToString(m_uid));
            }
        }

        protected override Response prepareConsulta()
        {

            v_mri.Clear();             

            var r = procedure(
                (m_type == (CmdMyRoomItem.TYPE)TYPE.ALL) ? m_szConsulta[0] : m_szConsulta[1],
                Convert.ToString(m_uid) + (m_type == TYPE.ONE ? ", " + Convert.ToString(m_item_id) : ""));

            checkResponse(r, "nao conseguiu pegar o(s) item(ns) do my room do player: " + Convert.ToString(m_uid));

            return r;
        }
                          
        private uint m_uid = new uint();
        private uint m_item_id = new uint();
        private TYPE m_type;
        private List<MyRoomItem> v_mri = new List<MyRoomItem>();

        private string[] m_szConsulta = { "pangya.ProcGetRoom", "pangya.ProcGetMyRoom_One" };
    }
}