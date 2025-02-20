//using GameServer.Cmd;
//using System;

//// Arquivo cmd_update_poster_equiped.cpp
//// Criado em 25/03/2018 as 13:01 por Acrisio
//// Implementa��o da classe CmdUpdatePosterEquiped

//#if _WIN32
//// C++ TO C# CONVERTER TASK: There is no equivalent to most C++ 'pragma' directives in C#:
////#pragma pack(1)
//#endif

//// Arquivo cmd_update_poster_equiped.hpp
//// Criado em 25/03/2018 as 12:56 por Acrisio
//// Defini��o da classe CmdUpdatePosterEquiped


//// C++ TO C# CONVERTER WARNING: The following #include directive was ignored:
////#include "../../Projeto IOCP/PANGYA_DB/pangya_db.h"
//// C++ TO C# CONVERTER NOTE: The following #define macro was replaced in-line:
// #define m_title skin_typeid[5]
//// C++ TO C# CONVERTER NOTE: The following #define macro was replaced in-line:
// #define GameServer.Cmd_C_ITEM_QNTD c[0]
//// C++ TO C# CONVERTER NOTE: The following #define macro was replaced in-line:
// #define GameServer.Cmd_C_ITEM_TICKET_REPORT_ID_HIGH c[1]
//// C++ TO C# CONVERTER NOTE: The following #define macro was replaced in-line:
// #define GameServer.Cmd_C_ITEM_TICKET_REPORT_ID_LOW c[2]
//// C++ TO C# CONVERTER NOTE: The following #define macro was replaced in-line:
// #define GameServer.Cmd_C_ITEM_TIME c[3]

//namespace GameServer.Cmd
//{
//	public class CmdUpdatePosterEquiped : Pangya_DB
//	{
//			public CmdUpdatePosterEquiped()
//			{
//				this.m_uid = 0;
//				this.m_ue = new GameServer.Cmd.UserEquip(0);
//			}

//			public CmdUpdatePosterEquiped(uint _uid,
//				UserEquip _ue,
//				)
//				{
//// C++ TO C# CONVERTER TASK: The following line was determined to be a copy assignment (rather than a reference assignment) - this should be verified and a 'CopyFrom' method should be created:
// this.m_uid = _uid;
//				//this.
//				this.m_ue = new GameServer.Cmd.UserEquip(_ue);
//				}

//			public virtual void Dispose()
//			{
//			}

//			public uint getUID()
//			{
//				return (m_uid);
//			}

//			public void setUID(uint _uid)
//			{
//// C++ TO C# CONVERTER TASK: The following line was determined to be a copy assignment (rather than a reference assignment) - this should be verified and a 'CopyFrom' method should be created:
// m_uid = _uid;
//				
//			}

//			public UserEquip getInfo()
//			{
//// C++ TO C# CONVERTER TASK: The following line was determined to contain a copy constructor call - this should be verified and a copy constructor should be created:
// return m_ue;
//				return new GameServer.Cmd.UserEquip(m_ue);
//			}

//			public void setInfo(UserEquip _ue)
//			{
//// C++ TO C# CONVERTER TASK: The following line was determined to be a copy assignment (rather than a reference assignment) - this should be verified and a 'CopyFrom' method should be created:
// m_ue = _ue;
//				m_ue.CopyFrom(_ue);
//			}

//			protected override void lineResult(ctx_res _result, uint _index_result)
//			{

//				// N�o usa aqui por que � UPDATE
//				return;
//			}

//			protected override Response prepareConsulta()
//			{

//				var r = procedure(
//					m_szConsulta,
//					Convert.ToString(m_uid) + ", " + Convert.ToString(m_ue.poster[0]) + ", " + Convert.ToString(m_ue.poster[1]));

//				checkResponse(r, "nao conseguiu atualizar o poster[ID=" + Convert.ToString(m_ue.poster[0]) + ", " + Convert.ToString(m_ue.poster[1]) + "] equipado do player: " + Convert.ToString(m_uid));

//				return r;
//			}

//			// get Class db_name
//			protected override string _getName()
//			{
//				return "CmdUpdatePosterEquiped";
//			}
//			protected override string _wgetName()
//			{
//				return "CmdUpdatePosterEquiped";
//			}

//			private uint m_uid = new uint();
//			private UserEquip m_ue = new UserEquip();

//			private const string m_szConsulta = "pangya.USP_FLUSH_EQUIP_POSTER";
//	}
//}
