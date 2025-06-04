//using Pangya_GameServer.GameType;
//using System;

//// Arquivo cmd_update_guild_update_activity.cpp
//// Criado em 30/11/2019 as 17:34 por Acrisio
//// Implementa��o da classe CmdUpdateGuildUpdateActivity

//#if _WIN32
//// C++ TO C# CONVERTER TASK: There is no equivalent to most C++ 'pragma' directives in C#:
////#pragma pack(1)
//#endif

//// Arquivo cmd_update_guild_update_activity.hpp
//// Criado em 30/11/2019 as 17:27 por Acrisio
//// Defini��o da classe CmdUpdateGuildUpdateActivity


//// C++ TO C# CONVERTER WARNING: The following #include directive was ignored:
////#include "../../Projeto IOCP/PANGYA_DB/pangya_db.h"

//namespace Pangya_GameServer.Cmd
//{
//	public class CmdUpdateGuildUpdateActiviy : Pangya_DB
//	{
//			public CmdUpdateGuildUpdateActiviy()
//			{
//				this.m_index = 0Ul;
//			}

//			public CmdUpdateGuildUpdateActiviy(uint64_t _index)
//			{
//
// this.m_index = _index;
//				this.m_index.CopyFrom(_index);
//			}

//			public virtual void Dispose()
//			{
//			}

//			public uint64_t getIndex()
//			{
//				return 64_t(m_index);
//			}

//			public void setIndex(uint64_t _index)
//			{
//
// m_index = _index;
//				m_index.CopyFrom(_index);
//			}

//			protected override void lineResult(ctx_res _result, uint _index_result)
//			{

//				// N�o usa por que � um UPDATE
//				return;
//			}

//			protected override Response prepareConsulta()
//			{

//				if(m_index == 0Ul)
//				{
//					throw new exception("[CmdUpdateGuildUpdateActivity::prepareConsulta][Error] m_index is invalid(zero).", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PANGYA_DB,
//						4, 0));
//				}

//				var r = consulta(_db, m_szConsulta + Convert.ToString(m_index));

//				checkResponse(r, "nao conseguiu atualizar o guild update activity[INDEX=" + Convert.ToString(m_index) + "]");

//				return r;
//			}

//			protected override string _getName()
//			{
//				return "CmdUpdateGuildUpdateActivity";
//			}
//			protected override string _wgetName()
//			{
//				return "CmdUpdateGuildUpdateActivity";
//			}

//			private uint64_t m_index = new uint64_t();

//			private const string m_szConsulta = "UPDATE pangya.pangya_guild_update_activity SET STATE = 1 WHERE INDEX = ";
//	}
//}
