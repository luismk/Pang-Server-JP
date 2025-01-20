//using GameServer.Cmd;
//using System;

//// Arquivo cmd_update_player_location.cpp
//// Criado em 11/05/2019 as 17:38 por Acrisio
//// Implementação da classe CmdUpdatePlayerLocation

//#if _WIN32
//// C++ TO C# CONVERTER TASK: There is no equivalent to most C++ 'pragma' directives in C#:
////#pragma pack(1)
//#endif

//// Arquivo cmd_update_player_location.hpp
//// Criado em 11/05/2019 as 17:32 por Acrisio
//// Definição da classe CmdUpdatePlayerLocation


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
//	public class CmdUpdatePlayerLocation : Pangya_DB
//	{
//			public CmdUpdatePlayerLocation(stPlayerLocationDB _pl, )
//			{
//				this.m_uid = 0u;
//				this.m_pl = new GameServer.Cmd.stPlayerLocationDB(_pl);
//			}

//			public CmdUpdatePlayerLocation(uint _uid,
//				stPlayerLocationDB _pl,
//				)
//				{
//// C++ TO C# CONVERTER TASK: The following line was determined to be a copy assignment (rather than a reference assignment) - this should be verified and a 'CopyFrom' method should be created:
//// ORIGINAL LINE: this.m_uid = _uid;
//				this.m_uid.CopyFrom(_uid);
//				this.m_pl = new GameServer.Cmd.stPlayerLocationDB(_pl);
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

//			public stPlayerLocationDB getInfo()
//			{
//				return m_pl;
//			}

//			protected override void lineResult(ctx_res _result, uint _index_result)
//			{

//				// Não usa por que é um UPDATE
//				return;
//			}

//			protected override Response prepareConsulta()
//			{

//				if(m_uid == 0u)
//				{
//					throw new exception("[CmdUpdatePlayerLocation::prepareConsulta][Error] Player[UID=" + Convert.ToString(m_uid) + "] is invalid.", ExceptionError.GameServer.Cmd_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PANGYA_DB,
//						4, 0));
//				}

//				var r = procedure(
//					m_szConsulta,
//					Convert.ToString(m_uid) + ", " + Convert.ToString((short)m_pl.channel) + ", " + Convert.ToString((short)m_pl.lobby) + ", " + Convert.ToString(m_pl.room) + ", " + Convert.ToString((ushort)m_pl.place));

//				checkResponse(r, "nao conseguiu atualizar Player[UID=" + Convert.ToString(m_uid) + "] Location[CHANNEL=" + Convert.ToString((short)m_pl.channel) + ", LOBBY=" + Convert.ToString((short)m_pl.lobby) + ", ROOM=" + Convert.ToString(m_pl.room) + ", PLACE=" + Convert.ToString((ushort)m_pl.place) + "]");

//				return r;
//			}

//			protected override string _getName()
//			{
//				return "CmdUpdatePlayerLocation";
//			}
//			protected override string _wgetName()
//			{
//				return "CmdUpdatePlayerLocation";
//			}

//			private stPlayerLocationDB m_pl;
//			private uint m_uid = new uint();

//			private const string m_szConsulta = "pangya.ProcUpdatePlayerLocation";
//	}
//}
