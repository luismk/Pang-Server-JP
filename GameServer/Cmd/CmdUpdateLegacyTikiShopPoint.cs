//using GameServer.GameType;
//using System;

//// Arquivo cmd_update_lagacy_tiki_shop_point.cpp
//// Criado em 26/10/2020 as 15:33 por Acrisio
//// Implementa��o da classe CmdUpdateLegacyTikiShopPoint

//#if _WIN32
//// C++ TO C# CONVERTER TASK: There is no equivalent to most C++ 'pragma' directives in C#:
////#pragma pack(1)
//#endif
//// Arquivo cmd_update_legacy_tiki_shop_point.hpp
//// Criado em 26/10/2020 as 15:27 por Acrisio
//// Defini��o da classe CmdUpdateLegacyTikiShopPoint


//// C++ TO C# CONVERTER WARNING: The following #include directive was ignored:
////#include "../../Projeto IOCP/PANGYA_DB/pangya_db.h"

//namespace GameServer.Cmd
//{

//	public class CmdUpdateLegacyTikiShopPoint : Pangya_DB
//	{

//			public CmdUpdateLegacyTikiShopPoint(uint _uid,
//				uint64_t _tiki_pts)
//				{
//// C++ TO C# CONVERTER TASK: The following line was determined to be a copy assignment (rather than a reference assignment) - this should be verified and a 'CopyFrom' method should be created:
// this.m_uid = _uid;
//				//this.
//// C++ TO C# CONVERTER TASK: The following line was determined to be a copy assignment (rather than a reference assignment) - this should be verified and a 'CopyFrom' method should be created:
// this.m_tiki_shop_point = _tiki_pts;
//				this.m_tiki_shop_point.CopyFrom(_tiki_pts);
//				}

//			public CmdUpdateLegacyTikiShopPoint()
//			{
//				this.m_uid = 0Ul;
//				this.m_tiki_shop_point = 0Ul;
//			}

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

//			public uint64_t getTikiShopPoint()
//			{
//				return 64_t(m_tiki_shop_point);
//			}

//			public void setTikiShopPoint(uint64_t _tiki_pts)
//			{
//// C++ TO C# CONVERTER TASK: The following line was determined to be a copy assignment (rather than a reference assignment) - this should be verified and a 'CopyFrom' method should be created:
// m_tiki_shop_point = _tiki_pts;
//				m_tiki_shop_point.CopyFrom(_tiki_pts);
//			}

//			protected override void lineResult(ctx_res _result, uint _index_result)
//			{

//				// N�o usa por que � um UPDATE
//				return;
//			}

//			protected override Response prepareConsulta()
//			{

//				if(m_uid == 0u)
//				{
//					throw new exception("[CmdUpdateLegacyTikiShopPoint::prepareConsulta][Error] m_uid is invalid(" + Convert.ToString(m_uid) + ")", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PANGYA_DB,
//						4, 0));
//				}

//				var r = consulta(_db, m_szConsulta[0] + Convert.ToString(m_tiki_shop_point) + m_szConsulta[1] + Convert.ToString(m_uid));

//				checkResponse(r, "Nao conseguiu atualizar o Legacy Tiki Shop Point[POINT=" + Convert.ToString(m_tiki_shop_point) + "] do Player[UID=" + Convert.ToString(m_uid) + "]");

//				return r;
//			}

//			protected override string _getName()
//			{
//				return "CmdUpdateLegacyTikiShopPoint";
//			}
//			protected override string _wgetName()
//			{
//				return "CmdUpdateLegacyTikiShopPoint";
//			}

//			private uint m_uid = new uint();
//			private uint64_t m_tiki_shop_point = new uint64_t();

//			private string[] m_szConsulta = { "UPDATE pangya.pangya_tiki_points SET Tiki_Points = ", ", MOD_DATE = CURRENT_TIMESTAMP WHERE UID = " };
//	}
//}
