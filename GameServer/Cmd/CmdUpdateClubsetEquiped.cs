using System;
using PangyaAPI.SQL;

// Arquivo cmd_update_clubset_equiped.cpp
// Criado em 25/03/2018 as 11:14 por Acrisio
// Implementa��o da classe CmdUpdateClubsetEquiped

#if _WIN32
// C++ TO C# CONVERTER TASK: There is no equivalent to most C++ 'pragma' directives in C#:
//#pragma pack(1)
#endif

// Arquivo cmd_update_clubset_equiped.hpp
// Criado em 25/03/2018 as 11:10 por Acrisio
// Defini��o da classe CmdUpdateClubsetEquiped


// C++ TO C# CONVERTER WARNING: The following #include directive was ignored:
//#include "../../Projeto IOCP/PANGYA_DB/pangya_db.h"

namespace Pangya_GameServer.Cmd
{
    public class CmdUpdateClubsetEquiped : Pangya_DB
    {
        public CmdUpdateClubsetEquiped()
        {
            this.m_uid = 0;
            this.m_clubset_id = 0;
        }

        public CmdUpdateClubsetEquiped(uint _uid,
            int _clubset_id)
        {
            // C++ TO C# CONVERTER TASK: The following line was determined to be a copy assignment (rather than a reference assignment) - this should be verified and a 'CopyFrom' method should be created:
            this.m_uid = _uid;
            //this.
            // C++ TO C# CONVERTER TASK: The following line was determined to be a copy assignment (rather than a reference assignment) - this should be verified and a 'CopyFrom' method should be created:
            this.m_clubset_id = _clubset_id;
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

        public int getClubsetID()
        {
            return (m_clubset_id);
        }

        public void setClubsetID(int _clubset_id)
        {
            // C++ TO C# CONVERTER TASK: The following line was determined to be a copy assignment (rather than a reference assignment) - this should be verified and a 'CopyFrom' method should be created:
            m_clubset_id = _clubset_id;
        }

        protected override void lineResult(ctx_res _result, uint _index_result)
        {

            // N�o usa por que � um UPDATE
            return;
        }

        protected override Response prepareConsulta()
        {

            var r = procedure(
                m_szConsulta,
                Convert.ToString(m_uid) + ", " + Convert.ToString(m_clubset_id));

            checkResponse(r, "nao conseguiu atualizar o clubset[ID=" + Convert.ToString(m_clubset_id) + "] equipado do player: " + Convert.ToString(m_uid));

            return r;
        }

        private uint m_uid = new uint();
        private int m_clubset_id = new int();

        private const string m_szConsulta = "pangya.USP_FLUSH_CLUB";
    }
}
