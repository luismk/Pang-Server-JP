//// C++ TO C# CONVERTER WARNING: The following #include directive was ignored:
////#include "../../Projeto IOCP/UTIL/util_time.h"

//using GameServer.Cmd;
//using System;

//// Arquivo cmd_update_papel_shop_info.cpp
//// Criado em 09/07/2018 as 21:50 por Acrisio
//// Implementa��o da classe CmdUpdatePapelShopInfo

//#if _WIN32
//// C++ TO C# CONVERTER TASK: There is no equivalent to most C++ 'pragma' directives in C#:
////#pragma pack(1)
//#endif

//// Arquivo cmd_update_papel_shop_info.hpp
//// Criado em 09/07/2018 as 21:44 por Acrisio
//// Defini��o da classe CmdUpdatePapelShopInfo


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

//#if _WIN32
//#elif __linux__
//// C++ TO C# CONVERTER WARNING: The following #include directive was ignored:
////#include "../../Projeto IOCP/UTIL/WinPort.h"
//#endif

//namespace GameServer.Cmd
//{
//	public class CmdUpdatePapelShopInfo : Pangya_DB
//	{
//			public CmdUpdatePapelShopInfo()
//			{
//				this.m_uid = 0u;
//				this.m_ppsi = new GameServer.Cmd.PlayerPapelShopInfo(0);
//				this.m_last_update =  0 ;
//			}

//			public CmdUpdatePapelShopInfo(uint _uid,
//				PlayerPapelShopInfo _ppsi,
//				SYSTEMTIME _last_update,
//				)
//				{
//// C++ TO C# CONVERTER TASK: The following line was determined to be a copy assignment (rather than a reference assignment) - this should be verified and a 'CopyFrom' method should be created:
//// ORIGINAL LINE: this.m_uid = _uid;
//				this.m_uid.CopyFrom(_uid);
//				this.m_ppsi = new GameServer.Cmd.PlayerPapelShopInfo(_ppsi);
//// C++ TO C# CONVERTER TASK: The following line was determined to be a copy assignment (rather than a reference assignment) - this should be verified and a 'CopyFrom' method should be created:
//// ORIGINAL LINE: this.m_last_update = _last_update;
//				this.m_last_update.CopyFrom(_last_update);
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

//			public SYSTEMTIME getLastUpdate()
//			{
//				return new SYSTEMTIME(m_last_update);
//			}

//			public void setLastUpdate(SYSTEMTIME _last_update)
//			{
//// C++ TO C# CONVERTER TASK: The following line was determined to be a copy assignment (rather than a reference assignment) - this should be verified and a 'CopyFrom' method should be created:
//// ORIGINAL LINE: m_last_update = _last_update;
//				m_last_update.CopyFrom(_last_update);
//			}

//			public PlayerPapelShopInfo getInfo()
//			{
//// C++ TO C# CONVERTER TASK: The following line was determined to contain a copy constructor call - this should be verified and a copy constructor should be created:
//// ORIGINAL LINE: return m_ppsi;
//				return new GameServer.Cmd.PlayerPapelShopInfo(m_ppsi);
//			}

//			public void setInfo(PlayerPapelShopInfo _ppsi)
//			{
//// C++ TO C# CONVERTER TASK: The following line was determined to be a copy assignment (rather than a reference assignment) - this should be verified and a 'CopyFrom' method should be created:
//// ORIGINAL LINE: m_ppsi = _ppsi;
//				m_ppsi.CopyFrom(_ppsi);
//			}

//			protected override void lineResult(ctx_res _result, uint _index_result)
//			{

//				// N�o usa por que � um UPDATE
//				return;
//			}

//			protected override Response prepareConsulta()
//			{

//				if(m_uid == 0)
//				{
//					throw new exception("[CmdUpdatePapelShopInfo::prepareConsulta][Error] m_uid is invalid(zero)", ExceptionError.GameServer.Cmd_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PANGYA_DB,
//						4, 0));
//				}

//				string last_update_dt = "null";

//				if(! isEmpty(m_last_update))
//				{
//					last_update_dt = _db.makeText(_formatDate(m_last_update));
//				}

//				var r = procedure(
//					m_szConsulta,
//					Convert.ToString(m_uid) + ", " + Convert.ToString(m_ppsi.current_count) + ", " + Convert.ToString(m_ppsi.remain_count) + ", " + Convert.ToString(m_ppsi.limit_count) + ", " + last_update_dt);

//				checkResponse(r, "nao conseguiu atualizar o Papel Shop Info[current_cnt=" + Convert.ToString(m_ppsi.current_count) + ", remain_cnt=" + Convert.ToString(m_ppsi.remain_count) + ", limit_cnt=" + Convert.ToString(m_ppsi.limit_count) + ", last_update=" + last_update_dt + "] do player[UID=" + Convert.ToString(m_uid) + "]");

//				return r;
//			}

//			// get Class db_name
//			protected override string _getName()
//			{
//				return "CmdUpdatePapelShopInfo";
//			}
//			protected override string _wgetName()
//			{
//				return "CmdUpdatePapelShopInfo";
//			}

//			private uint m_uid = new uint();
//			private PlayerPapelShopInfo m_ppsi = new PlayerPapelShopInfo();
//			private SYSTEMTIME m_last_update = new SYSTEMTIME();

//			private const string m_szConsulta = "pangya.ProcUpdatePapelShopInfo";
//	}
//}
