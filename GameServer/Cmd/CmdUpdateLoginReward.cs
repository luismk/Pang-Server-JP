//using GameServer.GameType;
//using System;

//// Arquivo cmd_update_login_reward.cpp
//// Criado em 27/10/2020 as 20:05 por Acrisio
//// Implementa��o da classe CmdUpdateLoginReward

//#if _WIN32
//// C++ TO C# CONVERTER TASK: There is no equivalent to most C++ 'pragma' directives in C#:
////#pragma pack(1)
//#endif
//// Arquivo cmd_update_login_reward.hpp
//// Criado em 27/10/2020 as 19:59 por Acrisio
//// Defini��o da classe CmdUpdateLoginReward


//// C++ TO C# CONVERTER WARNING: The following #include directive was ignored:
////#include "../../Projeto IOCP/PANGYA_DB/pangya_db.h"

//namespace GameServer.Cmd
//{

//	public class CmdUpdateLoginReward : Pangya_DB
//	{

//			public CmdUpdateLoginReward(uint64_t _id,
//				bool _is_end)
//				{
//// C++ TO C# CONVERTER TASK: The following line was determined to be a copy assignment (rather than a reference assignment) - this should be verified and a 'CopyFrom' method should be created:
// this.m_id = _id;
//				this.m_id.CopyFrom(_id);
//				this.m_is_end = _is_end;
//				}

//			public CmdUpdateLoginReward()
//			{
//				this.m_id = 0Ul;
//				this.m_is_end = false;
//			}

//			public virtual void Dispose()
//			{
//			}

//			public uint64_t getId()
//			{
//				return 64_t(m_id);
//			}

//			public void setId(uint64_t _id)
//			{
//// C++ TO C# CONVERTER TASK: The following line was determined to be a copy assignment (rather than a reference assignment) - this should be verified and a 'CopyFrom' method should be created:
// m_id = _id;
//				m_id.CopyFrom(_id);
//			}

//			public bool getIsEnd()
//			{
//				return m_is_end;
//			}

//			public void setIsEnd(bool _is_end)
//			{
//				m_is_end = _is_end;
//			}

//			protected override void lineResult(ctx_res _result, uint _index_result)
//			{

//				// N�o usa por que � um UPDATE
//				return;
//			}

//			protected override Response prepareConsulta()
//			{

//				if(m_id == 0Ul)
//				{
//					throw new exception("[CmdUpdateLoginReward::prepareConsulta][Error] m_id is invalid(" + Convert.ToString(m_id) + ")", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PANGYA_DB,
//						4, 0));
//				}

//				var r = consulta(_db, m_szConsulta[0] + new string(m_is_end ? "1" : "0") + m_szConsulta[1] + Convert.ToString(m_id));

//				checkResponse(r, "nao conseguiu atualizar o Login Reward[ID=" + Convert.ToString(m_id) + ", IS_END=" + new string(m_is_end ? "TRUE" : "FALSE") + "]");

//				return r;
//			}

//			protected override string _getName()
//			{
//				return "CmdUpdateLoginReward";
//			}
//			protected override string _wgetName()
//			{
//				return "CmdUpdateLoginReward";
//			}

//			private uint64_t m_id = new uint64_t();

//			private bool m_is_end;

//			private string[] m_szConsulta = { "UPDATE pangya.pangya_login_reward SET is_end = ", " WHERE index = " };
//	}
//}
