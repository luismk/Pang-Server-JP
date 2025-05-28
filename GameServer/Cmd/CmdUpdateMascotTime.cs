//using Pangya_GameServer.GameType;
//using System;

//// Arquivo cmd_update_mascot_time.cpp
//// Criado em 29/05/2018 as 21:45 por Acrisio
//// Implementa��o da classe CmdUpdateMascotTime

//#if _WIN32
//// C++ TO C# CONVERTER TASK: There is no equivalent to most C++ 'pragma' directives in C#:
////#pragma pack(1)
//#endif

//// Arquivo cmd_update_mascot_time.hpp
//// Criado em 29/05/2018 as 21:23 por Acrisio
//// Definie��o da classe CmdUpdateMascotTime


//// C++ TO C# CONVERTER WARNING: The following #include directive was ignored:
////#include "../../Projeto IOCP/PANGYA_DB/pangya_db.h"
//namespace Pangya_GameServer.Cmd
//{
//	public class CmdUpdateMascotTime : Pangya_DB
//	{
//			public CmdUpdateMascotTime()
//			{
//				this.m_uid = 0u;
//				this.m_id = -1;
//				this.m_time = new string();
//			}

//			public CmdUpdateMascotTime(uint _uid,
//				int _id, string _time)
//				{
//// C++ TO C# CONVERTER TASK: The following line was determined to be a copy assignment (rather than a reference assignment) - this should be verified and a 'CopyFrom' method should be created:
// this.m_uid = _uid;
//				//this.
//// C++ TO C# CONVERTER TASK: The following line was determined to be a copy assignment (rather than a reference assignment) - this should be verified and a 'CopyFrom' method should be created:
// this.m_id = _id;
//				this.m_id.CopyFrom(_id);
//				this.m_time = _time;
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

//			public int getID()
//			{
//				return (m_id);
//			}

//			public void setID(int _id)
//			{
//// C++ TO C# CONVERTER TASK: The following line was determined to be a copy assignment (rather than a reference assignment) - this should be verified and a 'CopyFrom' method should be created:
// m_id = _id;
//				m_id.CopyFrom(_id);
//			}

//			public string getTime()
//			{
//				return m_time;
//			}

//			public void setTime(string _time)
//			{
//				m_time = _time;
//			}

//			protected override void lineResult(ctx_res _result, uint _index_result)
//			{

//				// N�o usa por que � um UPDATE
//			}

//			protected override Response prepareConsulta()
//			{

//				if(m_id <= 0)
//				{
//					throw new exception("[CmdUpdateMascotTime::prepareConsulta][Error] mascot id[value=" + Convert.ToString(m_id) + "] is invalid", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PANGYA_DB,
//						4, 0));
//				}

//				if(m_time.Length == 0)
//				{
//					throw new exception("[CmdUpdateMascotTime::prepareConsulta][Error] time is empty", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PANGYA_DB,
//						4, 0));
//				}

//				var r = procedure(
//					m_szConsulta,
//					Convert.ToString(m_uid) + ", " + Convert.ToString(m_id) + ", " + _db.makeText(m_time));

//				checkResponse(r, "nao conseguiu atualizar o tempo do mascot[ID=" + Convert.ToString(m_id) + "] do player[UID=" + Convert.ToString(m_uid) + "]");

//				return r;
//			}

//			// get Class name
//			protected override string _getName()
//			{
//				return "CmdUpdateMascotTime";
//			}
//			protected override string _wgetName()
//			{
//				return "CmdUpdateMascotTime";
//			}

//			private uint m_uid = new uint();
//			private int m_id = new int();
//			private string m_time = "";

//			private const string m_szConsulta = "pangya.ProcUpdateMascotTime";
//	}
//}
