using System;
using PangyaAPI.SQL;
using PangyaAPI.Utilities;

namespace Pangya_GameServer.Cmd
{
    public class CmdUpdateBallQntd : Pangya_DB
    {
        public CmdUpdateBallQntd()
        {
            this.m_uid = 0u;
            this.m_id = -1;
            this.m_qntd = 0u;
        }

        public CmdUpdateBallQntd(uint _uid,
            int _id, uint _qntd)
        {
            this.m_uid = _uid;
            this.m_id = _id;
            this.m_qntd = _qntd;
        }

        public virtual void Dispose()
        {
        }

        public uint getUID()
        {
            return (m_uid);
        }

        public void setUID(uint _uid)
        {
            m_uid = _uid;

        }

        public int getID()
        {
            return (m_id);
        }

        public void setID(int _id)
        {
            m_id = _id;
        }

        public uint getQntd()
        {
            return (m_qntd);
        }

        public void setQntd(uint _qntd)
        {
            m_qntd = _qntd;
        }

        protected override void lineResult(ctx_res result, uint _index_result)
        {

            // N�o usa por que � um UPDATE
        }

        protected override Response prepareConsulta()
        {

            if (m_id <= 0)
            {
                throw new exception("[CmdUpdateBallQntd][Error] Ball id[value=" + Convert.ToString(m_id) + "] is invalid", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PANGYA_DB,
                    4, 0));
            }

            var r = _update(m_szConsulta[0] + Convert.ToString(m_qntd) + m_szConsulta[1] + Convert.ToString(m_uid) + m_szConsulta[2] + Convert.ToString(m_id));

            checkResponse(r, "nao conseguiu atualizar quantidade[value=" + Convert.ToString(m_qntd) + "] da Ball[ID=" + Convert.ToString(m_id) + "] do player[UID=" + Convert.ToString(m_uid) + "]");

            return r;
        }

        private uint m_uid = new uint();
        private int m_id = new int();
        private uint m_qntd = new uint();

        private string[] m_szConsulta = { "UPDATE pangya.pangya_item_warehouse SET C0 = ", " WHERE UID = ", " AND item_id =" };
    }
}
