using System;
using Pangya_GameServer.GameType;
using PangyaAPI.SQL;
using PangyaAPI.Utilities;
namespace Pangya_GameServer.Cmd
{
    public class CmdUpdatePapelShopInfo : Pangya_DB
    {
        public CmdUpdatePapelShopInfo(uint _uid,
            PlayerPapelShopInfo _ppsi,
            PangyaTime _last_update)
        {
            this.m_uid = _uid;
            this.m_ppsi = (_ppsi);
            this.m_last_update = _last_update;
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
            // C++ TO C# CONVERTER TASK: The following line was determined to be a copy assignment (rather than a reference assignment) - this should be verified and a 'CopyFrom' method should be created:
            m_uid = _uid;

        }

        public PangyaTime getLastUpdate()
        {
            return m_last_update;
        }

        public void setLastUpdate(PangyaTime _last_update)
        {
            m_last_update = _last_update;
        }

        public PlayerPapelShopInfo getInfo()
        {
            return m_ppsi;
        }

        public void setInfo(PlayerPapelShopInfo _ppsi)
        {
            m_ppsi = _ppsi;
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
                throw new exception("[CmdUpdatePapelShopInfo::prepareConsulta][Error] m_uid is invalid(zero)", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PANGYA_DB,
                    4, 0));
            }

            string last_update_dt = "null";

            if (!m_last_update.IsEmpty)
            {
                last_update_dt = _db.makeText(_formatDate(m_last_update.ConvertTime()));
            }

            var r = procedure(
                m_szConsulta,
                Convert.ToString(m_uid) + ", " + Convert.ToString(m_ppsi.current_count) + ", " + Convert.ToString(m_ppsi.remain_count) + ", " + Convert.ToString(m_ppsi.limit_count) + ", " + last_update_dt);

            checkResponse(r, "nao conseguiu atualizar o Papel Shop Info[current_cnt=" + Convert.ToString(m_ppsi.current_count) + ", remain_cnt=" + Convert.ToString(m_ppsi.remain_count) + ", limit_cnt=" + Convert.ToString(m_ppsi.limit_count) + ", last_update=" + last_update_dt + "] do player[UID=" + Convert.ToString(m_uid) + "]");

            return r;
        }

        private uint m_uid = new uint();
        private PlayerPapelShopInfo m_ppsi = new PlayerPapelShopInfo();
        private PangyaTime m_last_update = new PangyaTime();

        private const string m_szConsulta = "pangya.ProcUpdatePapelShopInfo";
    }
}
