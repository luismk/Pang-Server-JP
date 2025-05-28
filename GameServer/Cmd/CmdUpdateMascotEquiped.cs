using System;
using PangyaAPI.SQL;

// Arquivo cmd_update_mascot_equiped.cpp
// Criado em 25/03/2018 as 11:46 por Acrisio
// Implementa��o da classe CmdUpdateMascotEquiped

#if _WIN32
// C++ TO C# CONVERTER TASK: There is no equivalent to most C++ 'pragma' directives in C#:
//#pragma pack(1)
#endif

// Arquivo cmd_update_mascot_equiped.hpp
// Criado em 25/03/2018 as 11:42 por Acrisio
// Defini��o da classe CmdUpdateMascotEquiped


// C++ TO C# CONVERTER WARNING: The following #include directive was ignored:
//#include "../../Projeto IOCP/PANGYA_DB/pangya_db.h"

namespace Pangya_GameServer.Cmd
{
    public class CmdUpdateMascotEquiped : Pangya_DB
    {
        public CmdUpdateMascotEquiped()
        {
            this.m_uid = 0;
            this.m_mascot_id = -1;
        }

        public CmdUpdateMascotEquiped(uint _uid,
            int _mascot_id)
        {
            // C++ TO C# CONVERTER TASK: The following line was determined to be a copy assignment (rather than a reference assignment) - this should be verified and a 'CopyFrom' method should be created:
            this.m_uid = _uid;
            //this.
            // C++ TO C# CONVERTER TASK: The following line was determined to be a copy assignment (rather than a reference assignment) - this should be verified and a 'CopyFrom' method should be created:
            this.m_mascot_id = _mascot_id;
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

        public int getMascotID()
        {
            return (m_mascot_id);
        }

        public void setMascotID(int _mascot_id)
        {
            // C++ TO C# CONVERTER TASK: The following line was determined to be a copy assignment (rather than a reference assignment) - this should be verified and a 'CopyFrom' method should be created:
            m_mascot_id = _mascot_id;
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
                Convert.ToString(m_uid) + ", " + Convert.ToString(m_mascot_id));

            checkResponse(r, "nao conseguiu atualizar o mascot[ID=" + Convert.ToString(m_mascot_id) + "] equipado do player: " + Convert.ToString(m_uid));

            return r;
        }


        private uint m_uid = new uint();
        private int m_mascot_id = new int();

        private const string m_szConsulta = "pangya.USP_FLUSH_MASCOT";
    }
}
