//using Pangya_GameServer.GameType;
//using System;

//// Arquivo cmd_update_last_player_game.cpp
//// Criado em 28/10/2018 as 14:24 por Acrisio
//// Implementa��o da classe CmdUpdateLastPlayerGame

//#if _WIN32
//// C++ TO C# CONVERTER TASK: There is no equivalent to most C++ 'pragma' directives in C#:
////#pragma pack(1)
//#endif

//// Arquivo cmd_update_last_player_game.hpp
//// Criado em 28/10/2018 as 14:18 por Acrisio
//// Defini��o da classe CmdUpdateLastPlayerGame


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

//namespace Pangya_GameServer.Cmd
//{
//	public class CmdUpdateLastPlayerGame : Pangya_DB
//	{
//			public CmdUpdateLastPlayerGame()
//			{
//				this.m_uid = 0u;
//				this.m_l5pg = new Last5PlayersGame(0);
//			}

//			public CmdUpdateLastPlayerGame(uint _uid,
//				Last5PlayersGame _l5pg)
//				{
//// C++ TO C# CONVERTER TASK: The following line was determined to be a copy assignment (rather than a reference assignment) - this should be verified and a 'CopyFrom' method should be created:
// this.m_uid = _uid;
//				//this.
//				this.m_l5pg = new Last5PlayersGame(_l5pg);
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

//			public Last5PlayersGame getInfo()
//			{
//// C++ TO C# CONVERTER TASK: The following line was determined to contain a copy constructor call - this should be verified and a copy constructor should be created:
// return m_l5pg;
//				return new Last5PlayersGame(m_l5pg);
//			}

//			public void setInfo(Last5PlayersGame _l5pg)
//			{
//// C++ TO C# CONVERTER TASK: The following line was determined to be a copy assignment (rather than a reference assignment) - this should be verified and a 'CopyFrom' method should be created:
// m_l5pg = _l5pg;
//				m_l5pg.CopyFrom(_l5pg);
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
//					throw new exception("[CmdUpdateLastPlayerGame::prepareConsulta][Error] uid is invalid(0)", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PANGYA_DB,
//						4, 0));
//				}

//				string param = "";

//// C++ TO C# CONVERTER WARNING: This 'sizeof' ratio was replaced with a direct reference to the array length:
// for (auto i = 0u; i < (sizeof(m_l5pg.players) / sizeof(m_l5pg.players[0])); ++i)
//				for(var i = 0u; i < (m_l5pg.players.Length); ++ i)
//				{

//					if(m_l5pg.players[i].uid == 0u) // n�o tem Player nesse passa null pra o DB
//					{
//						param += ", null, null, null, null";
//					} else
//					{
//						param += ", " + Convert.ToString(m_l5pg.players[i].uid) + ", " + Convert.ToString(m_l5pg.players[i].sex);
//						param += (std::empty(m_l5pg.players[i].id) ? ", null" : ", " + _db.makeText(m_l5pg.players[i].id));
//						param += (std::empty(m_l5pg.players[i].nick) ? ", null" : ", " + _db.makeText(m_l5pg.players[i].nick));
//					}
//				}

//				var r = procedure(
//					m_szConsulta,
//					Convert.ToString(m_uid) + param);

//				checkResponse(r, "nao conseguiu atualizar o Last 5 Player Game do player[UID=" + Convert.ToString(m_uid) + "]");

//				return r;
//			}

//			protected override string _getName()
//			{
//				return "CmdUpdateLastPlayerGame";
//			}
//			protected override string _wgetName()
//			{
//				return "CmdUpdateLastPlayerGame";
//			}

//			private uint m_uid = new uint();
//			private Last5PlayersGame m_l5pg = new Last5PlayersGame();

//			private const string m_szConsulta = "pangya.ProcUpdateLast5PlayerGame";
//	}
//}
