
using PangyaAPI.Network.Pangya_St;
using PangyaAPI.SQL;

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PangyaAPI.Network.Cmd
{
    public class CmdUpdateCharacterEquip: Pangya_DB
    {
        uint m_uid;
        int m_character_id;
        protected override string _getName { get; } = "CmdUpdateCharacterEquip";
        public CmdUpdateCharacterEquip(uint _uid, int character_id)
        {
            m_uid = _uid;
            m_character_id = character_id;
        }

        protected override void lineResult(ctx_res _result, uint _index_result)
        {
            //so um update
            return;
        }

        protected override Response prepareConsulta()
        {

            var r = procedure("pangya.USP_FLUSH_CHARACTER", 
                m_uid.ToString() + ", " +
                m_character_id.ToString() );
            checkResponse(r, "nao conseguiu atualizar o character[ID=" + (m_character_id) + "] equipado do player: " + (m_uid));
            return r;
        }

        public uint getUID()
        {
            return m_uid;
        }

        public void setUID(uint _uid)
        {
            m_uid = _uid;
        }

        public int getCharacterID()
        {
            return m_character_id;
        }

        public void setCharacterID(int charID)
        {
            m_character_id = charID;
        }
    }
}
