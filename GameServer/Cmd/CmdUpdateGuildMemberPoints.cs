//using GameServer.GameType;
//using System;

//// Arquivo cmd_update_guild_member_points.cpp
//// Criado em 29/12/2019 as 13:09 por Acrisio
//// Implementa��o da classe CmdUpdateGuildMemberPoints

//#if _WIN32
//// C++ TO C# CONVERTER TASK: There is no equivalent to most C++ 'pragma' directives in C#:
////#pragma pack(1)
//#endif

//// Arquivo cmd_update_guild_member_points.hpp
//// Criado em 29/12/2019 as 13:04 por Acrisio
//// Defini��o da classe CmdUpdateGuildMemberPoints


//// C++ TO C# CONVERTER WARNING: The following #include directive was ignored:
////#include "../../Projeto IOCP/PANGYA_DB/pangya_db.h"

//namespace GameServer.Cmd
//{
//	public class CmdUpdateGuildMemberPoints : Pangya_DB
//	{
//			public CmdUpdateGuildMemberPoints()
//			{
//				this.m_gmp = new GuildMemberPoints(0u);
//			}

//			public CmdUpdateGuildMemberPoints(GuildMemberPoints _gmp, bool _waiter = false) : base(_waiter)
//			{
//				this.m_gmp = new GuildMemberPoints(_gmp);
//			}

//			public virtual void Dispose()
//			{
//			}

//			public GuildMemberPoints getInfo()
//			{
//// C++ TO C# CONVERTER TASK: The following line was determined to contain a copy constructor call - this should be verified and a copy constructor should be created:
// return m_gmp;
//				return new GuildMemberPoints(m_gmp);
//			}

//			public void setInfo(GuildMemberPoints _gmp)
//			{
//// C++ TO C# CONVERTER TASK: The following line was determined to be a copy assignment (rather than a reference assignment) - this should be verified and a 'CopyFrom' method should be created:
// m_gmp = _gmp;
//				m_gmp.CopyFrom(_gmp);
//			}

//			protected override void lineResult(ctx_res _result, uint _index_result)
//			{

//				// N�o usa por que � um UPDATE
//				return;
//			}

//			protected override Response prepareConsulta()
//			{

//				if(m_gmp.guild_uid == 0u)
//				{
//					throw new exception("[CmdUpdateGuildMemberPoints::prepareConsulta][Error] m_gmp.guild_uid is invalid(zero). Bug.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PANGYA_DB,
//						4, 0));
//				}

//				if(m_gmp.member_uid == 0u)
//				{
//					throw new exception("[CmdUpdateGuildMemberPoints::prepareConsulta][Error] m_gmp.member_uid is invalid(zero). Bug.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PANGYA_DB,
//						4, 0));
//				}

//				var r = procedure(
//					m_szConsulta,
//					Convert.ToString(m_gmp.guild_uid) + ", " + Convert.ToString(m_gmp.member_uid) + ", " + Convert.ToString(m_gmp.point) + ", " + Convert.ToString(m_gmp.pang));

//				checkResponse(r, "nao conseguiu atualizar o Guild[UID=" + Convert.ToString(m_gmp.guild_uid) + "] POINTS[POINT=" + Convert.ToString(m_gmp.point) + ", PANG=" + Convert.ToString(m_gmp.pang) + "] do player[UID=" + Convert.ToString(m_gmp.member_uid) + "]");

//				return r;
//			}

//			protected override string _getName()
//			{
//				return "CmdUpdateGuildMemberPoints";
//			}
//			protected override string _wgetName()
//			{
//				return "CmdUpdateGuildMemberPoints";
//			}

//			private GuildMemberPoints m_gmp = new GuildMemberPoints();

//			private const string m_szConsulta = "pangya.ProcUpdateGuildMemberPoints";
//	}
//}
