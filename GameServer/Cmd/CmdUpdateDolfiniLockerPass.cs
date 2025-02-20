using GameServer.GameType;
using PangyaAPI.SQL;
using PangyaAPI.Utilities;
using System;

// Arquivo cmd_update_dolfini_locker_pass.cpp
// Criado em 02/06/2018 as 15:24 por Acrisio
// Implementa��o da classe CmdUpdateDolfiniLockerPass

#if _WIN32
// C++ TO C# CONVERTER TASK: There is no equivalent to most C++ 'pragma' directives in C#:
//#pragma pack(1)
#endif

// Arquivo cmd_update_dolfini_locker_pass.hpp
// Criado em 02/06/2018 as 15:17 por Acrisio
// Defini��o da classe CmdUpdateDolfiniLockerPass


// C++ TO C# CONVERTER WARNING: The following #include directive was ignored:
//#include "../../Projeto IOCP/PANGYA_DB/pangya_db.h"
namespace GameServer.Cmd
{
	public class CmdUpdateDolfiniLockerPass : Pangya_DB
	{		 

			public CmdUpdateDolfiniLockerPass(uint _uid,
				string _pass)
				{
// C++ TO C# CONVERTER TASK: The following line was determined to be a copy assignment (rather than a reference assignment) - this should be verified and a 'CopyFrom' method should be created:
 this.m_uid = _uid;
				//this.
				this.m_pass = _pass;
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

			public string getPass()
			{
				return m_pass;
			}

			public void setPass(string _pass)
			{
				m_pass = _pass;
			}

			protected override void lineResult(ctx_res _result, uint _index_result)
			{

				// n�o usa por que � um UDPATE
				return;
			}

			protected override Response prepareConsulta()
			{

				if(m_pass.Length == 0)
				{
					throw new exception("[CmdUpdateDolfiniLockerPass::prepareConsulta][Error] pass is empty", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PANGYA_DB,
						4, 0));
				}

				if(m_pass.Length > 7)
				{
					throw new exception("[CmdUpdateDolfiniLockerPass::prepareConsulta][Error] pass is hight of permited", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PANGYA_DB,
						4, 0));
				}

				var r = procedure(
					m_szConsulta,
					Convert.ToString(m_uid) + ", " + _db.makeText(m_pass));

				checkResponse(r, "nao conseguiu atualizar a senha[value=" + m_pass + "] do dolfini locker do player[UID=" + Convert.ToString(m_uid) + "]");

				return r;
			}	   

			private uint m_uid = new uint();
			private string m_pass = "";

			private const string m_szConsulta = "pangya.ProcChangeDolfiniLockerPass";
	}
}
