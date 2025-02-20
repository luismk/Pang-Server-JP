//// C++ TO C# CONVERTER WARNING: The following #include directive was ignored:
////#include "../../Projeto IOCP/UTIL/util_time.h"

//using GameServer.GameType;
//using System;

//// Arquivo cmd_update_login_reward_player.cpp
//// Criado em 27/10/2020 as 20:21 por Acrisio
//// Implementa��o da classe CmdUpdateLoginRewardPlayer

//#if _WIN32
//// C++ TO C# CONVERTER TASK: There is no equivalent to most C++ 'pragma' directives in C#:
////#pragma pack(1)
//#endif
//// Arquivo cmd_update_login_reward_player.hpp
//// Criado em 27/10/2020 as 20:11 por Acrisio
//// Defini��o da classe CmdUpdateLoginRewardPlayer


//// C++ TO C# CONVERTER WARNING: The following #include directive was ignored:
////#include "../../Projeto IOCP/PANGYA_DB/pangya_db.h"

//namespace GameServer.Cmd
//{

//	public class CmdUpdateLoginRewardPlayer : Pangya_DB
//	{

//			public CmdUpdateLoginRewardPlayer(stPlayerState _ps)
//			{
//				this.m_ps = new stPlayerState(_ps);
//			}

//			public CmdUpdateLoginRewardPlayer()
//			{
//				this.m_ps = new stPlayerState(0u);
//			}

//			public virtual void Dispose()
//			{
//			}

//			public stPlayerState getPlayerState()
//			{
//// C++ TO C# CONVERTER TASK: The following line was determined to contain a copy constructor call - this should be verified and a copy constructor should be created:
// return m_ps;
//				return new stPlayerState(m_ps);
//			}

//			public void setPlayerState(stPlayerState _ps)
//			{
//// C++ TO C# CONVERTER TASK: The following line was determined to be a copy assignment (rather than a reference assignment) - this should be verified and a 'CopyFrom' method should be created:
// m_ps = _ps;
//				m_ps.CopyFrom(_ps);
//			}

//			protected override void lineResult(ctx_res _result, uint _index_result)
//			{

//				// N�o usa por que � um UPDATE
//				return;
//			}

//			protected override Response prepareConsulta()
//			{

//				if(m_ps.id == 0Ul)
//				{
//					throw new exception("[CmdUpdateLoginRewardPlayer::prepareConsulta][Error] m_ps.id is invalid(" + Convert.ToString(m_ps.id) + ")", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PANGYA_DB,
//						4, 0));
//				}

//				if(m_ps.uid == 0u)
//				{
//					throw new exception("[CmdUpdateLoginRewardPlayer::prepareConsulta][Error] m_ps.uid is invalid(" + Convert.ToString(m_ps.uid) + ")", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PANGYA_DB,
//						4, 0));
//				}

//				var r = procedure(
//					m_szConsulta,
//					Convert.ToString(m_ps.id) + ", " + Convert.ToString(m_ps.uid) + ", " + Convert.ToString(m_ps.count_days) + ", " + Convert.ToString(m_ps.count_seq) + ", " + new string(m_ps.is_clear ? "1" : "0") + ", " + _db.makeText(_formatDate(m_ps.update_date)));

//				checkResponse(r, "nao conseguiu atualizar o Player[" + m_ps.toString() + "]");

//				return r;
//			}

//			protected override string _getName()
//			{
//				return "CmdUpdateLoginRewardPlayer";
//			}
//			protected override string _wgetName()
//			{
//				return "CmdUpdateLoginRewardPlayer";
//			}

//			private stPlayerState m_ps = new stPlayerState();

//			private const string m_szConsulta = "pangya.procUpdateLoginRewardPlayer";
//	}
//}
