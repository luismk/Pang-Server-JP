//using GameServer.GameType;
//using System;

//// Arquivo cmd_update_grand_prix_clear.cpp
//// Criado em 14/06/2019 as 10:41 por Acrisio
//// Implementa��o da classe CmdUpdateGrandPrixClear

//#if _WIN32
//// C++ TO C# CONVERTER TASK: There is no equivalent to most C++ 'pragma' directives in C#:
////#pragma pack(1)
//#endif

//// Arquivo cmd_update_grand_prix_clear.hpp
//// Criado em 14/06/2019 as 10:35 por Acrisio
//// Defini��o da classe CmdUpdateGrandPrixClear


//// C++ TO C# CONVERTER WARNING: The following #include directive was ignored:
////#include "../../Projeto IOCP/PANGYA_DB/pangya_db.h"
//// C++ TO C# CONVERTER NOTE: The following #define macro was replaced in-line:
// #define m_title skin_typeid[5]
//// C++ TO C# CONVERTER NOTE: The following #define macro was replaced in-line:
// #define STDA_C_ITEM_QNTD c[0]
//// C++ TO C# CONVERTER NOTE: The following #define macro was replaced in-line:
// #define STDA_C_ITEM_TICKET_REPORT_ID_HIGH c[1]
//// C++ TO C# CONVERTER NOTE: The following #define macro was replaced in-line:
// #define STDA_C_ITEM_TICKET_REPORT_ID_LOW c[2]
//// C++ TO C# CONVERTER NOTE: The following #define macro was replaced in-line:
// #define STDA_C_ITEM_TIME c[3]

//namespace GameServer.Cmd
//{
//	public class CmdUpdateGrandPrixClear : Pangya_DB
//	{
//			public CmdUpdateGrandPrixClear()
//			{
//				this.m_uid = 0u;
//				this.m_gpc = new GrandPrixClear(0);
//			}

//			public CmdUpdateGrandPrixClear(uint _uid,
//				GrandPrixClear _gpc)
//				{
//// C++ TO C# CONVERTER TASK: The following line was determined to be a copy assignment (rather than a reference assignment) - this should be verified and a 'CopyFrom' method should be created:
// this.m_uid = _uid;
//				//this.
//				this.m_gpc = new GrandPrixClear(_gpc);
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
				
//			}

//			public GrandPrixClear getInfo()
//			{
//// C++ TO C# CONVERTER TASK: The following line was determined to contain a copy constructor call - this should be verified and a copy constructor should be created:
// return m_gpc;
//				return new GrandPrixClear(m_gpc);
//			}

//			public void setInfo(GrandPrixClear _gpc)
//			{
//// C++ TO C# CONVERTER TASK: The following line was determined to be a copy assignment (rather than a reference assignment) - this should be verified and a 'CopyFrom' method should be created:
// m_gpc = _gpc;
//				m_gpc.CopyFrom(_gpc);
//			}

//			protected override void lineResult(ctx_res _result, uint _index_result)
//			{

//				// N�o usa por que � um UPDATE
//				return;
//			}

//			protected override Response prepareConsulta()
//			{

//				if(m_uid == 0u)
//				{
//					throw new exception("[CmdUpdateGrandPrixClear::prepareConsulta][Error] m_uid is invalid(zero)", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PANGYA_DB,
//						4, 0));
//				}

//				if(m_gpc._typeid == 0u)
//				{
//					throw new exception("[CmdUpdateGrandPrixClear::prepareConsulta][Error] Grand Prix Clear is invalid typeid is zero", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PANGYA_DB,
//						4, 0));
//				}

//				var r = consulta(_db, m_szConsulta[0] + Convert.ToString(m_gpc.position) + m_szConsulta[1] + Convert.ToString(m_uid) + m_szConsulta[2] + Convert.ToString(m_gpc._typeid));

//				checkResponse(r, "nao conseguiu atualizar o Grand Prix Clear[TYPEID=" + Convert.ToString(m_gpc._typeid) + ", POSITION=" + Convert.ToString(m_gpc.position) + "] do Player[UID=" + Convert.ToString(m_uid) + "]");

//				return r;
//			}

//			protected override string _getName()
//			{
//				return "CmdUpdateGrandPrixClear";
//			}
//			protected override string _wgetName()
//			{
//				return "CmdUpdateGrandPrixClear";
//			}

//			private uint m_uid = new uint();
//			private GrandPrixClear m_gpc = new GrandPrixClear();

//			private string[] m_szConsulta = { "UPDATE pangya.pangya_grandprix_clear SET flag = ", " WHERE UID = ", " AND typeid = " };
//	}
//}
