//using GameServer.GameType;
//using System;

//// Arquivo cmd_update_dolfini_locker_mode.cpp
//// Criado em 02/06/2018 as 18:09 por Acrisio
//// Implementa��o da classe CmdUpdateDolfiniLockerMode

//#if _WIN32
//// C++ TO C# CONVERTER TASK: There is no equivalent to most C++ 'pragma' directives in C#:
////#pragma pack(1)
//#endif

//// Arquivo cmd_update_dolfini_locker_mode.hpp
//// Criado em 02/06/2018 as 18:03 por Acrisio
//// Defini��o da classe CmdUpdateDolfiniLockerMode


//// C++ TO C# CONVERTER WARNING: The following #include directive was ignored:
////#include "../../Projeto IOCP/PANGYA_DB/pangya_db.h"

//namespace GameServer.Cmd
//{
//	public class CmdUpdateDolfiniLockerMode : Pangya_DB
//	{
//			public CmdUpdateDolfiniLockerMode()
//			{
//				this.m_uid = 0u;
//				this.m_locker = 0;
//			}

//			public CmdUpdateDolfiniLockerMode(uint _uid,
//				byte _locker)
//				{
//// C++ TO C# CONVERTER TASK: The following line was determined to be a copy assignment (rather than a reference assignment) - this should be verified and a 'CopyFrom' method should be created:
// this.m_uid = _uid;
//				//this.
//				this.m_locker = _locker;
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

//			public byte getLocker()
//			{
//				return m_locker;
//			}

//			public void setLocker(byte _locker)
//			{
//				m_locker = _locker;
//			}

//			protected override void lineResult(ctx_res _result, uint _index_result)
//			{

//				// N�o usa por que � um UPDATE
//				return;
//			}

//			protected override Response prepareConsulta()
//			{

//				var r = _update(m_szConsulta[0] + Convert.ToString((ushort)m_locker) + m_szConsulta[1] + Convert.ToString(m_uid));

//				checkResponse(r, "nao conseguiu atualizar o modo[locker=" + Convert.ToString(m_locker) + "] do dolfini locker do player[UID=" + Convert.ToString(m_uid) + "]");

//				return r;
//			}

//			// get Class name
//			protected override string _getName()
//			{
//				return "CmdUpdateDolfiniLockerMode";
//			}
//			protected override string _wgetName()
//			{
//				return "CmdUpdateDolfiniLockerMode";
//			}

//			private uint m_uid = new uint();
//			private byte m_locker;

//			private string[] m_szConsulta = { "UPDATE pangya.pangya_dolfini_locker SET locker = ", " WHERE UID = " };
//	}
//}
