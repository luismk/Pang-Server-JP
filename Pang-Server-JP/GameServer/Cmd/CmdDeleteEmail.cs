using PangyaAPI.SQL;
using PangyaAPI.Utilities;
using System;

namespace GameServer.Cmd
{
    public class CmdDeleteEmail : Pangya_DB
    {
        public CmdDeleteEmail()
        {
            this.m_uid = 0;
            this.m_email_id = null;
            this.m_count = 0u;
        }

        public CmdDeleteEmail(uint _uid,
            uint[] _email_id,
            uint _count)
        {
            this.m_uid = _uid;
            this.m_email_id = null;
            this.m_count = 0u;

            if (_email_id == null || _count == 0u)
            {
                return;
            }

            // Alloc memory
            m_email_id = new uint[_count];
            m_count = _count;
            // copy
#if _WIN32
// C++ TO C# CONVERTER TASK: The memory management function 'memcpy_s' has no equivalent in C#:
				memcpy_s(m_email_id,
					_count * sizeof(uint),
					_email_id,
					_count * sizeof(uint));
#elif __linux__
// C++ TO C# CONVERTER TASK: The memory management function 'memcpy' has no equivalent in C#:
				memcpy(m_email_id,
					_email_id,
					_count * sizeof(uint));
#endif
        }

        public uint getUID()
        {
            return m_uid;
        }

        public void setUID(uint _uid)
        {
            m_uid = _uid;
        }

        public uint[] getEmailID()
        {
            return m_email_id;
        }

        public void setEmailID(uint[] _email_id, uint _count)
        {

            if (_email_id == null || _count == 0u)
            {

                if (m_email_id != null)
                {
                    m_email_id = null;
                }

                m_email_id = null;
                m_count = 0u;

                return;
            }

            // realoca se for maior
            if (m_email_id != null && _count > m_count)
            {

                m_email_id = null;

                // Alloc memory
                m_email_id = new uint[_count];

            }
            else if (m_email_id == null) // Alloc memory
            {
                m_email_id = new uint[_count];
            }

            m_count = _count;
            m_email_id = _email_id; 
        }

        public uint getCount()
        {
            return m_count;
        }

        protected override void lineResult(ctx_res _result, uint _index_result)
        {
            // UPDATE n�o usa o result
            // mas caso algum dia eu queira usar o result, depois de deletar um email eu mexo aqui
            return;
        }

        protected override Response prepareConsulta()
        {

            if (m_count > 0u && m_email_id != null)
            {
                string ids = "";

                for (uint i = 0u; i < m_count; ++i)
                {
                    if (i == 0u)
                    {
                        ids += Convert.ToString(m_email_id[i]);
                    }
                    else
                    {
                        ids += ", " + Convert.ToString(m_email_id[i]);
                    }
                }

                var r = _update(m_szConsulta[0] + Convert.ToString(m_uid) + m_szConsulta[1] + ids + m_szConsulta[2]);

                checkResponse(r, "nao conseguiu deletar o email(s) do player: " + Convert.ToString(m_uid));

                return r;

            }
            else
            {
                throw new exception("[CmdDeleteEmail][Error] nao pode deletar Email(s) sem id(s)");
            }
        }


        private uint m_uid = 0;
        private uint[] m_email_id;
        private uint m_count =0;

        private string[] m_szConsulta = { "UPDATE pangya.pangya_gift_table SET valid = 0 WHERE uid = ", " AND Msg_ID IN(", ")" };
    }
}