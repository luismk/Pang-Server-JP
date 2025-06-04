//using Pangya_GameServer.GameType;
//using System;

//// Arquivo cmd_update_guild_points.cpp
//// Criado em 29/12/2019 as 12:40 por Acrisio
//// Implementa��o da classe CmdUpdateGuildPoints

//#if _WIN32
//// C++ TO C# CONVERTER TASK: There is no equivalent to most C++ 'pragma' directives in C#:
////#pragma pack(1)
//#endif

//// Arquivo cmd_update_guild_points.hpp
//// Criado em 29/12/2019 as 12:24 por Acrisio
//// Defini��o da classe CmdUpdateGuildPoints


//// C++ TO C# CONVERTER WARNING: The following #include directive was ignored:
////#include "../../Projeto IOCP/PANGYA_DB/pangya_db.h"

//namespace Pangya_GameServer.Cmd
//{
//	public class CmdUpdateGuildPoints : Pangya_DB
//	{
//			public CmdUpdateGuildPoints()
//			{
//				this.m_gp = new GuildPoints(0u);
//			}

//			public CmdUpdateGuildPoints(GuildPoints _gp)
//			{
//				this.m_gp = new GuildPoints(_gp);
//			}

//			public virtual void Dispose()
//			{
//			}

//			public GuildPoints getInfo()
//			{
//// C++ TO C# CONVERTER TASK: The following line was determined to contain a copy constructor call - this should be verified and a copy constructor should be created:
// return m_gp;
//				return new GuildPoints(m_gp);
//			}

//			public void setInfo(GuildPoints _gp)
//			{
//
// m_gp = _gp;
//				m_gp.CopyFrom(_gp);
//			}

//			protected override void lineResult(ctx_res _result, uint _index_result)
//			{

//				// N�o usa por que � um UPDATE
//				return;
//			}

//			protected override Response prepareConsulta()
//			{

//				if(m_gp.uid == 0u)
//				{
//					throw new exception("[CmdUpdateGuildPoints::prepareConsulta][Error] m_gp.uid is invalid(zero). Bug.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PANGYA_DB,
//						4, 0));
//				}

//				var r = procedure(
//					m_szConsulta,
//					Convert.ToString(m_gp.uid) + ", " + Convert.ToString(m_gp.point) + ", " + Convert.ToString(m_gp.pang) + ", " + Convert.ToString((ushort)m_gp.win));

//				checkResponse(r, "nao conseguiu atualizar os Pontos[POINT=" + Convert.ToString(m_gp.point) + ", PANG=" + Convert.ToString(m_gp.pang) + "] da Guild[UID=" + Convert.ToString(m_gp.uid) + ", WIN=" + Convert.ToString((ushort)m_gp.win) + "]");

//				return r;
//			}

//			protected override string _getName()
//			{
//				return "CmdUpdateGuildPoints";
//			}
//			protected override string _wgetName()
//			{
//				return "CmdUpdateGuildPoints";
//			}

//			private GuildPoints m_gp = new GuildPoints();

//			private const string m_szConsulta = "pangya.ProcUpdateGuildPoints";
//	}
//}
