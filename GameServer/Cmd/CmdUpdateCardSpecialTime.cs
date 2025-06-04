using System;
using Pangya_GameServer.GameType;
using PangyaAPI.SQL;
using PangyaAPI.Utilities;
namespace Pangya_GameServer.Cmd
{
    public class CmdUpdateCardSpecialTime : Pangya_DB
    {
        public CmdUpdateCardSpecialTime()
        {
            this.m_uid = 0u;
        }

        public CmdUpdateCardSpecialTime(uint _uid,
            CardEquipInfoEx _cei)
        {
            
            this.m_uid = _uid;
            //this.
            this.m_cei = (_cei);
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

        public CardEquipInfoEx getInfo()
        {
            // C++ TO C# CONVERTER TASK: The following line was determined to contain a copy constructor call - this should be verified and a copy constructor should be created:
            return m_cei;
        }

        public void setInfo(CardEquipInfoEx _cei)
        {
            
            m_cei = _cei;
        }

        protected override void lineResult(ctx_res _result, uint _index_result)
        {

            // N�o usa por que � um UPDATE
            return;
        }

        protected override Response prepareConsulta()
        {

            if (m_uid == 0)
            {
                throw new exception("[CmdUpdateCardSpecialTime::prepareConsulta][Error] m_uid is invalid(zero)", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PANGYA_DB,
                    4, 0));
            }

            if (m_cei.id <= 0 || m_cei._typeid == 0)
            {
                throw new exception("[CmdUpdateCardSpecialTime::prepareConsulta][Error] m_cei[index=" + Convert.ToString(m_cei.id) + ", TYPEID=" + Convert.ToString(m_cei._typeid) + "] is invalid", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PANGYA_DB,
                    4, 0));
            }

            var r = procedure(
                m_szConsulta,
                Convert.ToString(m_uid) + ", " + Convert.ToString(m_cei.id) + ", " + Convert.ToString(m_cei._typeid) + ", " + Convert.ToString(m_cei.efeito) + ", " + Convert.ToString(m_cei.efeito_qntd) + ", " + Convert.ToString(m_cei.tipo) + ", " + _db.makeText(UtilTime.FormatDate(m_cei.end_date.ConvertTime())));

            checkResponse(r, "nao conseguiu atualizar tempo do Card Special[index=" + Convert.ToString(m_cei.id) + ", TYPEID=" + Convert.ToString(m_cei._typeid) + ", EFEITO{TYPE: " + Convert.ToString(m_cei.efeito) + ", QNTD: " + Convert.ToString(m_cei.efeito_qntd) + "}, TIPO=" + Convert.ToString(m_cei.tipo) + ", DATE{REG_DT: " + _formatDate(m_cei.use_date.ConvertTime()) + ", END_DT: " + _formatDate(m_cei.end_date.ConvertTime()) + "}] do player[UID=" + Convert.ToString(m_uid) + "]");

            return r;
        }

        private uint m_uid = new uint();
        private CardEquipInfoEx m_cei = new CardEquipInfoEx();

        private const string m_szConsulta = "pangya.ProcUpdateCardSpecialTime";
    }
}
