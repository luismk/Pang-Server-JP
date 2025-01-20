//// C++ TO C# CONVERTER WARNING: The following #include directive was ignored:
////#include "../../Projeto IOCP/UTIL/util_time.h"

//using GameServer.Cmd;
//using System;

//// Arquivo cmd_add_caddie.cpp
//// Criado em 25/03/2018 as 18:58 por Acrisio
//// Implementa��o da classe CmdAddCaddie

//#if _WIN32
//// C++ TO C# CONVERTER TASK: There is no equivalent to most C++ 'pragma' directives in C#:
////#pragma pack(1)
//#endif

//// Arquivo cmd_add_caddie.hpp
//// Criado em 25/03/2018 as 18:36 por Acrisio
//// Defini��o da classe CmdAddCaddie


//// C++ TO C# CONVERTER WARNING: The following #include directive was ignored:
////#include "../../Projeto IOCP/PANGYA_DB/cmd_add_item_base.hpp"
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
//	public class CmdAddCaddie : CmdAddItemBase, System.IDisposable
//	{
//			public CmdAddCaddie()
//			{
//				this.m_ci = new GameServer.Cmd.CaddieInfoEx(0);
//			}

//			public CmdAddCaddie(uint _uid,
//				CaddieInfoEx _ci,
//				byte _purchase,
//				byte _gift_flag,
//				bool _waiter = false) : base(_uid,
//					_purchase, _gift_flag, _waiter)
//					{
//				this.m_ci = new GameServer.Cmd.CaddieInfoEx(_ci);
//					}

//			public virtual void Dispose()
//			{
//			}

//			public CaddieInfoEx getInfo()
//			{
//// C++ TO C# CONVERTER TASK: The following line was determined to contain a copy constructor call - this should be verified and a copy constructor should be created:
//// ORIGINAL LINE: return m_ci;
//				return new GameServer.Cmd.CaddieInfoEx(m_ci);
//			}

//			public void setInfo(CaddieInfoEx _ci)
//			{
//// C++ TO C# CONVERTER TASK: The following line was determined to be a copy assignment (rather than a reference assignment) - this should be verified and a 'CopyFrom' method should be created:
//// ORIGINAL LINE: m_ci = _ci;
//				m_ci.CopyFrom(_ci);
//			}

//			protected override void lineResult(ctx_res _result, uint _index_result)
//			{

//				checkColumnNumber(2);

//				m_ci.id = IFNULL(_result.data[0]);

//				//m_ci.end_date_unix = (unsigned short)IFNULL(_result->data[1]);
//				if(_result.data[1] != null)
//				{
//					_translateDate(_result.data[1], m_ci.end_date);
//				}
//			}

//			protected override Response prepareConsulta()
//			{

//				if(m_uid == 0u)
//				{
//					throw new exception("[CmdAddCaddie::prepareConsulta][Error] m_uid is invalid(zero)", ExceptionError.GameServer.Cmd_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PANGYA_DB,
//						4, 0));
//				}

//				var r = procedure(
//					m_szConsulta,
//					Convert.ToString(m_uid) + ", " + Convert.ToString(m_ci.id) + ", " + Convert.ToString(m_ci._typeid) + ", " + Convert.ToString((ushort)m_gift_flag) + ", " + Convert.ToString((ushort)m_purchase) + ", " + Convert.ToString((ushort)m_ci.rent_flag) + ", " + Convert.ToString(m_ci.end_date_unix));

//				checkResponse(r, "nao conseguiu adicionar o caddie[TYPEID=" + Convert.ToString(m_ci._typeid) + "] para o player: " + Convert.ToString(m_uid));

//				return r;
//			}

//			// get Class db_name
//			protected override string _getName()
//			{
//				return "CmdAddCaddie";
//			}
//			protected override string _wgetName()
//			{
//				return "CmdAddCaddie";
//			}

//			private CaddieInfoEx m_ci = new CaddieInfoEx();

//			private const string m_szConsulta = "pangya.ProcAddCaddie";
//	}
//}
