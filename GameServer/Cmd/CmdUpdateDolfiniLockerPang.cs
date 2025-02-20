//using GameServer.GameType;
//using System;

//// Arquivo cmd_update_dolfini_locker_pang.cpp
//// Criado em 02/06/2018 as 21:45 por Acrisio
//// Implementa��o da classe CmdUpdateDolfiniLockerPang

//#if _WIN32
//// C++ TO C# CONVERTER TASK: There is no equivalent to most C++ 'pragma' directives in C#:
////#pragma pack(1)
//#endif

//// Arquivo cmd_update_dolfini_locker_pang.hpp
//// Criado em 02/06/2018 as 21:39 por Acrisio
//// Defini��o da classe CmdUpdateDolfiniLockerPang


//// C++ TO C# CONVERTER WARNING: The following #include directive was ignored:
////#include "../../Projeto IOCP/PANGYA_DB/pangya_db.h"

//namespace GameServer.Cmd
//{
//	public class CmdUpdateDolfiniLockerPang : Pangya_DB
//	{
//			public CmdUpdateDolfiniLockerPang()
//			{
//				this.m_uid = 0u;
//				this.m_pang = 0Ul;
//			}

//			public CmdUpdateDolfiniLockerPang(uint _uid,
//				ulong _pang)
//				{
//// C++ TO C# CONVERTER TASK: The following line was determined to be a copy assignment (rather than a reference assignment) - this should be verified and a 'CopyFrom' method should be created:
// this.m_uid = _uid;
//				//this.
//// C++ TO C# CONVERTER TASK: The following line was determined to be a copy assignment (rather than a reference assignment) - this should be verified and a 'CopyFrom' method should be created:
// this.m_pang = _pang;
//				this.m_pang.CopyFrom(_pang);
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

//			public ulong getPang()
//			{
//				return (m_pang);
//			}

//			public void setPang(ulong _pang)
//			{
//// C++ TO C# CONVERTER TASK: The following line was determined to be a copy assignment (rather than a reference assignment) - this should be verified and a 'CopyFrom' method should be created:
// m_pang = _pang;
//				m_pang.CopyFrom(_pang);
//			}

//			protected override void lineResult(ctx_res _result, uint _index_result)
//			{

//				// N�o usa por que � um UPDATE
//			}

//			protected override Response prepareConsulta()
//			{

//				var r = _update(m_szConsulta[0] + Convert.ToString(m_pang) + m_szConsulta[1] + Convert.ToString(m_uid));

//				checkResponse(r, "nao conseguiu atualizar o pang[value=" + Convert.ToString(m_pang) + "] do Dolfini Locker do player[UID=" + Convert.ToString(m_uid) + "]");

//				return r;
//			}

//			// get Class name
//			protected override string _getName()
//			{
//				return "CmdUpdateDolfiniLockerPang";
//			}
//			protected override string _wgetName()
//			{
//				return "CmdUpdateDolfiniLockerPang";
//			}

//			private uint m_uid = new uint();
//			private ulong m_pang = new ulong();

//			private string[] m_szConsulta = { "UPDATE pangya.pangya_dolfini_locker SET pang = ", " WHERE UID = " };
//	}
//}
