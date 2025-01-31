//using GameServer.Cmd;
//using System;

//// Arquivo cmd_update_premium_ticket_time.cpp
//// Criado em 07/12/2019 as 13:07 por Acrisio
//// Implementa��o da classe CmdUpdatePremiumTicketTime

//#if _WIN32
//// C++ TO C# CONVERTER TASK: There is no equivalent to most C++ 'pragma' directives in C#:
////#pragma pack(1)
//#endif

//// Arquivo cmd_update_premium_ticket_time.hpp
//// Criado em 07/12/2019 as 13:01 por Acrisio
//// Defini��o da classe CmdUpdatePremiumTicketTime


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
//	public class CmdUpdatePremiumTicketTime : Pangya_DB
//	{
//			public CmdUpdatePremiumTicketTime()
//			{
//				this.m_uid = 0u;
//				this.m_wi = new GameServer.Cmd.WarehouseItemEx(0);
//			}

//			public CmdUpdatePremiumTicketTime(uint _uid,
//				WarehouseItemEx _wi,
//				)
//				{
//// C++ TO C# CONVERTER TASK: The following line was determined to be a copy assignment (rather than a reference assignment) - this should be verified and a 'CopyFrom' method should be created:
//// ORIGINAL LINE: this.m_uid = _uid;
//				this.m_uid.CopyFrom(_uid);
//				this.m_wi = new GameServer.Cmd.WarehouseItemEx(_wi);
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

//			public WarehouseItemEx getPremiumTicket()
//			{
//// C++ TO C# CONVERTER TASK: The following line was determined to contain a copy constructor call - this should be verified and a copy constructor should be created:
//// ORIGINAL LINE: return m_wi;
//				return new GameServer.Cmd.WarehouseItemEx(m_wi);
//			}

//			public void setPremiumTicket(WarehouseItemEx _wi)
//			{
//// C++ TO C# CONVERTER TASK: The following line was determined to be a copy assignment (rather than a reference assignment) - this should be verified and a 'CopyFrom' method should be created:
//// ORIGINAL LINE: m_wi = _wi;
//				m_wi.CopyFrom(_wi);
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
//					throw new exception("[CmdUpdatePremiumTicketTime::prepareConsulta][Error] m_uid is invalid(zero).", ExceptionError.GameServer.Cmd_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PANGYA_DB,
//						4, 0));
//				}

//				if(m_wi.id <= 0)
//				{
//					throw new exception("[CmdUpdatePremiumTicketTime::prepareConsulta][Error] m_wi.id is invalid[VALUE=" + Convert.ToString(m_wi.id) + "].", ExceptionError.GameServer.Cmd_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PANGYA_DB,
//						4, 0));
//				}

//				var r = procedure(
//					m_szConsulta,
//					Convert.ToString(m_uid) + ", " + Convert.ToString(m_wi.id) + ", " + Convert.ToString(m_wi.c[3]) + ", " + Convert.ToString(m_wi.c[0]) + ", " + Convert.ToString(m_wi.c[1]) + ", " + Convert.ToString(m_wi.c[2]) + ", " + Convert.ToString(m_wi.c[3]) + ", " + Convert.ToString(m_wi.c[4]));

//				checkResponse(r, "nao conseguiu atualizar Premium Ticket Time[ID=" + Convert.ToString(m_wi.id) + ", TEMPO=" + Convert.ToString(m_wi.c[3]) + ", C0=" + Convert.ToString(m_wi.c[0]) + ", C1=" + Convert.ToString(m_wi.c[1]) + ", C2=" + Convert.ToString(m_wi.c[2]) + ", C3=" + Convert.ToString(m_wi.c[3]) + ", C4=" + Convert.ToString(m_wi.c[4]) + "] do Player[UID=" + Convert.ToString(m_uid) + "]");

//				return r;
//			}

//			protected override string _getName()
//			{
//				return "CmdUpdatePremiumTicketTime";
//			}
//			protected override string _wgetName()
//			{
//				return "CmdUpdatePremiumTicketTime";
//			}

//			private uint m_uid = new uint();
//			private WarehouseItemEx m_wi = new WarehouseItemEx();

//			private const string m_szConsulta = "pangya.ProcUpdatePremiumTicketTime";
//	}
//}
