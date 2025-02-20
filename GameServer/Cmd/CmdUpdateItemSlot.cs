using GameServer.GameType;
using PangyaAPI.SQL;
using PangyaAPI.Utilities;
using System;

namespace GameServer.Cmd
{
    public class CmdUpdateItemSlot : Pangya_DB
    {
        public CmdUpdateItemSlot(uint _uid, uint[] _slot)
        {
            this.m_uid = _uid;
            m_slot[0] = 0;

            if (_slot == null)
            {
                throw new exception("[CmdUpdateItemSlot::CmdUpdateItemSlot][Error] _slot is nullptr", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PANGYA_DB,
                    6, 0));
            }
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

        public uint[] getSlot()
        {
            return m_slot;
        }

        public void setSlot(uint[] _slot)
        {

            if (_slot == null)
            {
                throw new exception("[CmdUpdateItemSlot::setSlot][Error] _slot is nullptr", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PANGYA_DB,
                    6, 0));
            }
            m_slot = _slot;
        }

        protected override void lineResult(ctx_res _result, uint _index_result)
        {

            // N�o usa por que � um update
            return;
        }

        protected override Response prepareConsulta()
        {

            var r = _update(m_szConsulta[0] + Convert.ToString(m_slot[0]) + m_szConsulta[1] + Convert.ToString(m_slot[1]) + m_szConsulta[2] + Convert.ToString(m_slot[2]) + m_szConsulta[3] + Convert.ToString(m_slot[3]) + m_szConsulta[4] + Convert.ToString(m_slot[4]) + m_szConsulta[5] + Convert.ToString(m_slot[5]) + m_szConsulta[6] + Convert.ToString(m_slot[6]) + m_szConsulta[7] + Convert.ToString(m_slot[7]) + m_szConsulta[8] + Convert.ToString(m_slot[8]) + m_szConsulta[9] + Convert.ToString(m_slot[9]) + m_szConsulta[10] + Convert.ToString(m_uid));

            checkResponse(r, "nao conseguiud atualizar o item slot do player: " + Convert.ToString(m_uid));

            return r;
        }

        private uint m_uid = new uint();
        private uint[] m_slot = new uint[10];

        private string[] m_szConsulta = { "UPDATE pangya.pangya_user_equip SET item_slot_1 = ", ", item_slot_2 = ", ", item_slot_3 = ", ", item_slot_4 = ", ", item_slot_5 = ", ", item_slot_6 = ", ", item_slot_7 = ", ", item_slot_8 = ", ", item_slot_9 = ", ", item_slot_10 = ", " WHERE uid = " };
    }
}
