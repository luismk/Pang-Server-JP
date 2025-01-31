//// C++ TO C# CONVERTER WARNING: The following #include directive was ignored:
////#include "../../Projeto IOCP/UTIL/util_time.h"

//using GameServer.Cmd;
//using System;

//// Arquivo cmd_update_papel_shop_config.cpp
//// Criado em 08/12/2018 as 15:16 por Acrisio
//// Implementa��o da classe CmdUpdatePapelShopConfig

//#if _WIN32
//// C++ TO C# CONVERTER TASK: There is no equivalent to most C++ 'pragma' directives in C#:
////#pragma pack(1)
//#endif

//// Arquivo cmd_udpdate_papel_shop_config.hpp
//// Criado em 08/12/2018 as 15:09 por Acrisio
//// Defini��o da classe CmdUpdatePapelShopConfig


//// C++ TO C# CONVERTER WARNING: The following #include directive was ignored:
////#include "../../Projeto IOCP/PANGYA_DB/pangya_db.h"

//namespace GameServer.Cmd
//{
//	public class CmdUpdatePapelShopConfig : Pangya_DB
//	{
//			public CmdUpdatePapelShopConfig()
//			{
//				this.m_ps = new GameServer.Cmd.ctx_papel_shop(0);
//				this.m_updated = false;
//			}

//			public CmdUpdatePapelShopConfig(ctx_papel_shop _ps, )
//			{
//				this.m_ps = new GameServer.Cmd.ctx_papel_shop(_ps);
//				this.m_updated = false;
//			}

//			public virtual void Dispose()
//			{
//			}

//			public ctx_papel_shop getInfo()
//			{
//// C++ TO C# CONVERTER TASK: The following line was determined to contain a copy constructor call - this should be verified and a copy constructor should be created:
//// ORIGINAL LINE: return m_ps;
//				return new GameServer.Cmd.ctx_papel_shop(m_ps);
//			}

//			public void setInfo(ctx_papel_shop _ps)
//			{
//// C++ TO C# CONVERTER TASK: The following line was determined to be a copy assignment (rather than a reference assignment) - this should be verified and a 'CopyFrom' method should be created:
//// ORIGINAL LINE: m_ps = _ps;
//				m_ps.CopyFrom(_ps);
//			}

//			public bool isUpdated()
//			{
//				return m_updated;
//			}

//			protected override void lineResult(ctx_res _result, uint _index_result)
//			{

//				checkColumnNumber(6, (uint)_result.cols);

//				// Update ON DB
//				m_updated = IFNULL(_result.data[0]) == 1 ? true : false;

//				if(! m_updated)
//				{ // Não atualizou, pega os valores atualizados do banco de dados

//					m_ps.numero = IFNULL(_result.data[1]);
//					m_ps.price_normal = IFNULL(atoll, _result.data[2]);
//					m_ps.price_big = IFNULL(atoll, _result.data[3]);
//					m_ps.limitted_per_day = (byte)IFNULL(_result.data[4]);

//					if(_result.data[5] != null)
//					{
//						_translateDate(_result.data[5], m_ps.update_date);
//					}
//				}

//				return;
//			}

//			protected override response prepareConsulta(database _db)
//			{

//				m_updated = false;

//				string upt_dt = "null";

//				if(! isEmpty(m_ps.update_date))
//				{
//					upt_dt = _db.makeText(_formatDate(m_ps.update_date));
//				}

//				var r = procedure(_db,
//					m_szConsulta,
//					Convert.ToString(m_ps.numero) + ", " + Convert.ToString(m_ps.price_normal) + ", " + Convert.ToString(m_ps.price_big) + ", " + Convert.ToString((ushort)m_ps.limitted_per_day) + ", " + upt_dt);

//				checkResponse(r, "nao conseguiu atualizar o Papel Shop Config[" + m_ps.toString() + "]");

//				return r;
//			}

//			protected override string _getName()
//			{
//				return "CmdUpdatePapelShopConfig";
//			}
//			protected override string _wgetName()
//			{
//				return "CmdUpdatePapelShopConfig";
//			}

//			private ctx_papel_shop m_ps = new ctx_papel_shop();
//			private bool m_updated;

//			private const string m_szConsulta = "pangya.ProcUpdatePapelShopConfig";
//	}
//}
