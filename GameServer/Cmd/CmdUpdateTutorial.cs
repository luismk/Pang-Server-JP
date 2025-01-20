//using GameServer.Cmd;
//using System;

//// Arquivo cmd_update_tutorial.cpp
//// Criado em 28/06/2018 as 22:34 por Acrisio
//// Implementa��o da classe CmdUpdateTutorial


//#if _WIN32
//// C++ TO C# CONVERTER TASK: There is no equivalent to most C++ 'pragma' directives in C#:
////#pragma pack(1)
//#endif

//// Arquivo cmd_update_tutorial.hpp
//// Criado em 28/06/2018 as 22:27 por Acrisio
//// Defini��o da classe CmdUpdateTutorial


//// C++ TO C# CONVERTER WARNING: The following #include directive was ignored:
////#include "../../Projeto IOCP/PANGYA_DB/pangya_db.h"
//// C++ TO C# CONVERTER NOTE: The following #define macro was replaced in-line:
//// ORIGINAL LINE: #define m_title skin_typeid[5]
//// C++ TO C# CONVERTER NOTE: The following #define macro was replaced in-line:
//// ORIGINAL LINE: #define GameServer.Cmd_C_ITEM_QNTD c[0]
//// C++ TO C# CONVERTER NOTE: The following #define macro was replaced in-line:
//// ORIGINAL LINE: #define GameServer.Cmd_C_ITEM_TICKET_REPORT_ID_HIGH c[1]
//// C++ TO C# CONVERTER NOTE: The following #define macro was replaced in-line:
//// ORIGINAL LINE: #define GameServer.Cmd_C_ITEM_TICKET_REPORT_ID_LOW c[2]
//// C++ TO C# CONVERTER NOTE: The following #define macro was replaced in-line:
//// ORIGINAL LINE: #define GameServer.Cmd_C_ITEM_TIME c[3]

//namespace GameServer.Cmd
//{
//	public class CmdUpdateTutorial : Pangya_DB
//	{
//			public CmdUpdateTutorial()
//			{
//				this.m_uid = 0u;
//				this.m_ti = new GameServer.Cmd.TutorialInfo(0);
//			}

//			public CmdUpdateTutorial(uint _uid,
//				TutorialInfo _ti,
//				)
//				{
//// C++ TO C# CONVERTER TASK: The following line was determined to be a copy assignment (rather than a reference assignment) - this should be verified and a 'CopyFrom' method should be created:
//// ORIGINAL LINE: this.m_uid = _uid;
//				this.m_uid.CopyFrom(_uid);
//				this.m_ti = new GameServer.Cmd.TutorialInfo(_ti);
//				}

//			public virtual void Dispose()
//			{
//			}

//			public uint getUID()
//			{
//				return new uint(m_uid);
//			}

//			public void setUID(uint _uid)
//			{
//// C++ TO C# CONVERTER TASK: The following line was determined to be a copy assignment (rather than a reference assignment) - this should be verified and a 'CopyFrom' method should be created:
//// ORIGINAL LINE: m_uid = _uid;
//				m_uid.CopyFrom(_uid);
//			}

//			public TutorialInfo getInfo()
//			{
//// C++ TO C# CONVERTER TASK: The following line was determined to contain a copy constructor call - this should be verified and a copy constructor should be created:
//// ORIGINAL LINE: return m_ti;
//				return new GameServer.Cmd.TutorialInfo(m_ti);
//			}

//			public void setInfo(TutorialInfo _ti)
//			{
//// C++ TO C# CONVERTER TASK: The following line was determined to be a copy assignment (rather than a reference assignment) - this should be verified and a 'CopyFrom' method should be created:
//// ORIGINAL LINE: m_ti = _ti;
//				m_ti.CopyFrom(_ti);
//			}

//			protected override void lineResult(ctx_res _result, uint _index_result)
//			{

//				// N�o usa por que � um UPDATE
//				return;
//			}

//			protected override response prepareConsulta(database _db)
//			{

//				if(m_uid == 0)
//				{
//					throw new exception("[CmdUpdateTutorial::prepareConsulta][Error] m_uid is invalid(zero)", ExceptionError.GameServer.Cmd_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PANGYA_DB,
//						4, 0));
//				}

//				var r = _update(_db, m_szConsulta[0] + Convert.ToString(m_ti.rookie) + m_szConsulta[1] + Convert.ToString(m_ti.beginner) + m_szConsulta[2] + Convert.ToString(m_ti.advancer) + m_szConsulta[3] + Convert.ToString(m_uid));

//				checkResponse(r, "nao conseguiu Atualizar o Tutorial[Rookie=" + Convert.ToString(m_ti.rookie) + ", Beginner=" + Convert.ToString(m_ti.beginner) + ", Advancer=" + Convert.ToString(m_ti.advancer) + "] do player[UID=" + Convert.ToString(m_uid) + "]");

//				return r;
//			}

//			// get Class db_name
//			protected override string _getName()
//			{
//				return "CmdUpdateTutorial";
//			}
//			protected override string _wgetName()
//			{
//				return "CmdUpdateTutorial";
//			}

//			private uint m_uid = new uint();
//			private TutorialInfo m_ti = new TutorialInfo();

//			private string[] m_szConsulta = { "UPDATE pangya.tutorial SET Rookie = ", ", Beginner = ", ", Advancer = ", " WHERE UID = " };
//	}
//}
