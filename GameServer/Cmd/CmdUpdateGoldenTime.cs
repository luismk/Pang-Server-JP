//using Pangya_GameServer.GameType;
//using System;

//// Arquivo cmd_update_golden_time.cpp
//// Criado em 24/10/2020 as 03:32 por Acrisio
//// Implementa��o da classe CmdUpdateGoldenTime

//#if _WIN32
//// C++ TO C# CONVERTER TASK: There is no equivalent to most C++ 'pragma' directives in C#:
////#pragma pack(1)
//#endif
//// Arquivo cmd_update_golden_time.hpp
//// Criado em 24/10/2020 as 03:25 por Acrisio
//// Deifini��o da classe CmdUpdateGoldenTime


//// C++ TO C# CONVERTER WARNING: The following #include directive was ignored:
////#include "../../Projeto IOCP/PANGYA_DB/pangya_db.h"

//namespace Pangya_GameServer.Cmd
//{

//	public class CmdUpdateGoldenTime : Pangya_DB
//	{

//			public CmdUpdateGoldenTime()
//			{
//				this.m_id = 0u;
//				this.m_is_end = false;
//			}

//			public CmdUpdateGoldenTime(uint _id,
//				bool _is_end)
//				{
//// C++ TO C# CONVERTER TASK: The following line was determined to be a copy assignment (rather than a reference assignment) - this should be verified and a 'CopyFrom' method should be created:
// this.m_id = _id;
//				this.m_id.CopyFrom(_id);
//				this.m_is_end = _is_end;
//				}

//			public virtual void Dispose()
//			{
//			}

//			public uint getId()
//			{
//				return (m_id);
//			}

//			public void setId(uint _id)
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

//				if(m_id == 0u)
//				{
//					throw new exception("[CmdUpdateGoldenTime::prepareConsulta][Error] m_id is invalid(" + Convert.ToString(m_id) + ")", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PANGYA_DB,
//						4, 0));
//				}

//				var r = consulta(_db, m_szConsulta[0] + Convert.ToString(m_is_end ? 1 : 0) + m_szConsulta[1] + Convert.ToString(m_id));

//				checkResponse(r, "nao conseguiu atualizar o Golden Time[ID=" + Convert.ToString(m_id) + ", IS_END=" + new string(m_is_end ? "TRUE" : "FALSE") + "]");

//				return r;
//			}

//			protected override string _getName()
//			{
//				return "CmdUpdateGoldenTime";
//			}
//			protected override string _wgetName()
//			{
//				return "CmdUpdateGoldenTime";
//			}

//			private uint m_id = new uint();
//			private bool m_is_end;

//			private string[] m_szConsulta = { "UPDATE pangya.pangya_golden_time_info SET is_end = ", " WHERE index = " };
//	}
//}
