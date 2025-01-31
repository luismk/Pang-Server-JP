//using GameServer.Cmd;
//using System;

//// Arquivo cmd_update_treasure_hunter_course_point.cpp
//// Criado em 22/09/2018 as 12:46 por Acrisio
//// Implementa��o da classe CmdTreasureHunterCoursePoint

//#if _WIN32
//// C++ TO C# CONVERTER TASK: There is no equivalent to most C++ 'pragma' directives in C#:
////#pragma pack(1)
//#endif

//// Arquivo cmd_update_treasure_hunter_course_point.hpp
//// Criado em 22/09/2018 as 12:34 por Acrisio
//// Defini��o da classe CmdUpdateTreasureHunterCoursePoint


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
//	public class CmdUpdateTreasureHunterCoursePoint : Pangya_DB
//	{
//			public CmdUpdateTreasureHunterCoursePoint()
//			{
//				this.m_thi = new GameServer.Cmd.TreasureHunterInfo(0);
//			}

//			public CmdUpdateTreasureHunterCoursePoint(TreasureHunterInfo _thi, )
//			{
//				this.m_thi = new GameServer.Cmd.TreasureHunterInfo(_thi);
//			}

//			public virtual void Dispose()
//			{
//			}

//			public TreasureHunterInfo getInfo()
//			{
//// C++ TO C# CONVERTER TASK: The following line was determined to contain a copy constructor call - this should be verified and a copy constructor should be created:
//// ORIGINAL LINE: return m_thi;
//				return new GameServer.Cmd.TreasureHunterInfo(m_thi);
//			}

//			public void setInfo(TreasureHunterInfo _thi)
//			{
//// C++ TO C# CONVERTER TASK: The following line was determined to be a copy assignment (rather than a reference assignment) - this should be verified and a 'CopyFrom' method should be created:
//// ORIGINAL LINE: m_thi = _thi;
//				m_thi.CopyFrom(_thi);
//			}

//			protected override void lineResult(ctx_res _result, uint _index_result)
//			{

//				// N�o usa por que � um UPDATE
//				return;
//			}

//			protected override Response prepareConsulta()
//			{

//				var r = consulta( m_szConsulta[0] + Convert.ToString(m_thi.point) + m_szConsulta[1] + Convert.ToString((ushort)(m_thi.course & 0x7F)));

//				checkResponse(r, "nao conseguiu atulizar o Treasure Hunter Info[COURSE=" + Convert.ToString((ushort)(m_thi.course & 0x7F)) + ", POINT=" + Convert.ToString(m_thi.point) + "]");

//				return r;
//			}

//			protected override string _getName()
//			{
//				return "CmdUpdateTreasureHunterCoursePoint";
//			}
//			protected override string _wgetName()
//			{
//				return "CmdUpdateTreasureHunterCoursePoint";
//			}

//			private TreasureHunterInfo m_thi = new TreasureHunterInfo();

//			private string[] m_szConsulta = { "UPDATE pangya.pangya_course_reward_treasure SET PANGREWARD = ", " WHERE COURSE = " };
//	}
//}
