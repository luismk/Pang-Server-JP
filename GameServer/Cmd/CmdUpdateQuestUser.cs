//using GameServer.Cmd;
//using System;

//// Arquivo cmd_update_quest_user.cpp
//// Criado em 14/04/2018 as 16:03 por Acrisio
//// Implementa��o da classe CmdUpdateQuestUser

//#if _WIN32
//// C++ TO C# CONVERTER TASK: There is no equivalent to most C++ 'pragma' directives in C#:
////#pragma pack(1)
//#endif

//// Arquivo cmd_update_quest_user.hpp
//// Criado em 14/04/2018 as 15:57 por Acrisio
//// Defini��o da classe CmdUpdateQuestUser


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
//	public class CmdUpdateQuestUser : Pangya_DB
//	{
//			public CmdUpdateQuestUser()
//			{
//				this.m_uid = 0;
//				this.m_qsi = new GameServer.Cmd.QuestStuffInfo();
//			}

//			public CmdUpdateQuestUser(uint _uid,
//				QuestStuffInfo _qsi,
//				)
//				{
//// C++ TO C# CONVERTER TASK: The following line was determined to be a copy assignment (rather than a reference assignment) - this should be verified and a 'CopyFrom' method should be created:
// this.m_uid = _uid;
//				//this.
//				this.m_qsi = new GameServer.Cmd.QuestStuffInfo(_qsi);
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

//			public QuestStuffInfo getInfo()
//			{
//// C++ TO C# CONVERTER TASK: The following line was determined to contain a copy constructor call - this should be verified and a copy constructor should be created:
// return m_qsi;
//				return new GameServer.Cmd.QuestStuffInfo(m_qsi);
//			}

//			public void setInfo(QuestStuffInfo _qsi)
//			{
//// C++ TO C# CONVERTER TASK: The following line was determined to be a copy assignment (rather than a reference assignment) - this should be verified and a 'CopyFrom' method should be created:
// m_qsi = _qsi;
//				m_qsi.CopyFrom(_qsi);
//			}

//			protected override void lineResult(ctx_res _result, uint _index_result)
//			{

//				// N�o usa por que � um UPDATE
//				return;
//			}

//			protected override Response prepareConsulta()
//			{

//				if(m_qsi.id <= 0
//					|| m_qsi._typeid == 0
//					|| m_qsi.counter_item_id <= 0)
//				{
//					throw new exception("[CmdUpdateQuestUser::prepareConsulta][Error] QuestStuffInfoEx m_qsi is invalid", GameServer.Cmd_MAKE_ERROR(STDA_ERROR_TYPE.PANGYA_DB,
//						4, 0));
//				}

//				string clear_dt = "null";

//				if(m_qsi.clear_date_unix != 0)
//				{
//					clear_dt = _db.makeText(formatDateLocal(m_qsi.clear_date_unix));
//				}

//				var r = procedure(
//					m_szConsulta,
//					Convert.ToString(m_uid) + ", " + Convert.ToString(m_qsi.id) + ", " + Convert.ToString(m_qsi.counter_item_id) + ", " + clear_dt);

//				checkResponse(r, "nao conseguiu atualizar a quest[ID=" + Convert.ToString(m_qsi.id) + "] do player: " + Convert.ToString(m_uid));

//				return r;
//			}

//			// get Class db_name
//			protected override string _getName()
//			{
//				return "CmdUpdateQuestUser";
//			}
//			protected override string _wgetName()
//			{
//				return "CmdUpdateQuestUser";
//			}

//			private uint m_uid = new uint();
//			private QuestStuffInfo m_qsi = new QuestStuffInfo();

//			private const string m_szConsulta = "pangya.ProcUpdateQuestUser";
//	}
//}
