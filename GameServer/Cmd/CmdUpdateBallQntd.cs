//using GameServer.GameType;
//using System;

//// Arquivo cmd_update_ball_qntd.cpp
//// Criado em 31/05/2018 as 09:30 por Acrisio
//// Implementa��o da classe CmdUpdateBallQntd

//#if _WIN32
//// C++ TO C# CONVERTER TASK: There is no equivalent to most C++ 'pragma' directives in C#:
////#pragma pack(1)
//#endif

//// Arquivo cmd_update_ball_qntd.hpp
//// Criado em 31/05/2018 as 09:21 por Acrisio
//// Defini��o da classe CmdAddBallQntd


//// C++ TO C# CONVERTER WARNING: The following #include directive was ignored:
////#include "../../Projeto IOCP/PANGYA_DB/pangya_db.h"

//namespace GameServer.Cmd
//{
//	public class CmdUpdateBallQntd : Pangya_DB
//	{
//			public CmdUpdateBallQntd()
//			{
//				this.m_uid = 0u;
//				this.m_id = -1;
//				this.m_qntd = 0u;
//			}

//			public CmdUpdateBallQntd(uint _uid,
//				int _id, uint _qntd)
//				{
//// C++ TO C# CONVERTER TASK: The following line was determined to be a copy assignment (rather than a reference assignment) - this should be verified and a 'CopyFrom' method should be created:
// this.m_uid = _uid;
//				//this.
//// C++ TO C# CONVERTER TASK: The following line was determined to be a copy assignment (rather than a reference assignment) - this should be verified and a 'CopyFrom' method should be created:
// this.m_id = _id;
//				this.m_id.CopyFrom(_id);
//// C++ TO C# CONVERTER TASK: The following line was determined to be a copy assignment (rather than a reference assignment) - this should be verified and a 'CopyFrom' method should be created:
// this.m_qntd = _qntd;						   
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

//			public uint getQntd()
//			{
//				return (m_qntd);
//			}

//			public void setQntd(uint _qntd)
//			{
//// C++ TO C# CONVERTER TASK: The following line was determined to be a copy assignment (rather than a reference assignment) - this should be verified and a 'CopyFrom' method should be created:
// m_qntd = _qntd;
//				m_qntd.CopyFrom(_qntd);
//			}

//			protected override void lineResult(ctx_res result, uint _index_result)
//			{

//				// N�o usa por que � um UPDATE
//			}

//			protected override Response prepareConsulta()
//			{

//				if(m_id <= 0)
//				{
//					throw new exception("[CmdUpdateBallQntd][Error] Ball id[value=" + Convert.ToString(m_id) + "] is invalid", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PANGYA_DB,
//						4, 0));
//				}

//				var r = _update(m_szConsulta[0] + Convert.ToString(m_qntd) + m_szConsulta[1] + Convert.ToString(m_uid) + m_szConsulta[2] + Convert.ToString(m_id));

//				checkResponse(r, "nao conseguiu atualizar quantidade[value=" + Convert.ToString(m_qntd) + "] da Ball[ID=" + Convert.ToString(m_id) + "] do player[UID=" + Convert.ToString(m_uid) + "]");

//				return r;
//			}

//			// get Class name
//			protected override string _getName()
//			{
//				return "CmdUpdateBallQntd";
//			}
//			protected override string _wgetName()
//			{
//				return "CmdUpdateBallQntd";
//			}

//			private uint m_uid = new uint();
//			private int m_id = new int();
//			private uint m_qntd = new uint();

//			private string[] m_szConsulta = { "UPDATE pangya.pangya_item_warehouse SET C0 = ", " WHERE UID = ", " AND item_id =" };
//	}
//}
