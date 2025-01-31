//// C++ TO C# CONVERTER WARNING: The following #include directive was ignored:
////#include "../../Projeto IOCP/UTIL/util_time.h"

//using GameServer.Cmd;
//using GameServer.PangType;
//using PangyaAPI.SQL;
//using PangyaAPI.Utilities;
//using System;

//// Arquivo cmd_update_ucc.cpp
//// Criado em 14/07/2018 as 20:16 por Acrisio
//// Implementa��o da classe CmdUpdateUCC

//#if _WIN32
//// C++ TO C# CONVERTER TASK: There is no equivalent to most C++ 'pragma' directives in C#:
////#pragma pack(1)
//#endif

//// Arquivo cmd_update_ucc.hpp
//// Criado em 14/07/2018 as 20:009 por Acrisio
//// Defini��o da classe CmdUpdateUCC

//#if ! _GameServer.Cmd_CMD_UPDATE_UCC_HPP
//#define GameServer.Cmd_CMD_UPDATE_UCC_HPP

//// C++ TO C# CONVERTER WARNING: The following #include directive was ignored:
////#include "../../Projeto IOCP/PANGYA_DB/pangya_db.h"
//#define CLEAR_10_DAILY_QUEST_TYPEID
//#define ASSIST_ITEM_TYPEID
//#define GRAND_PRIX_TICKET
//#define LIMIT_GRAND_PRIX_TICKET
//#define MULLIGAN_ROSE_TYPEID
//#define DEFAULT_COMET_TYPEID
//#define AIR_KNIGHT_SET
//#define CLUB_PATCHER_TYPEID
//#define MILAGE_POINT_TYPEID
//#define TIKI_POINT_TYPEID
//#define SPECIAL_SHUFFLE_COURSE_TICKET_TYPEID
//#define PANG_POUCH_TYPEID
//#define EXP_POUCH_TYPEID
//#define CP_POUCH_TYPEID
//#define DECREASE_COMBO_VALUE
//#define MEDIDA_PARA_YARDS
//// C++ TO C# CONVERTER TASK: #define macros defined in multiple preprocessor conditionals can only be replaced within the scope of the preprocessor conditional:
//// C++ TO C# CONVERTER NOTE: The following #define macro was replaced in-line:
//// ORIGINAL LINE: #define m_title skin_typeid[5]
//#define m_title // Titulo Typeid
//#define MS_NUM_MAPS
//// C++ TO C# CONVERTER TASK: #define macros defined in multiple preprocessor conditionals can only be replaced within the scope of the preprocessor conditional:
//// C++ TO C# CONVERTER NOTE: The following #define macro was replaced in-line:
//// ORIGINAL LINE: #define GameServer.Cmd_C_ITEM_QNTD c[0]
//#define GameServer.Cmd_C_ITEM_QNTD
//// C++ TO C# CONVERTER TASK: #define macros defined in multiple preprocessor conditionals can only be replaced within the scope of the preprocessor conditional:
//// C++ TO C# CONVERTER NOTE: The following #define macro was replaced in-line:
//// ORIGINAL LINE: #define GameServer.Cmd_C_ITEM_TICKET_REPORT_ID_HIGH c[1]
//#define GameServer.Cmd_C_ITEM_TICKET_REPORT_ID_HIGH
//// C++ TO C# CONVERTER TASK: #define macros defined in multiple preprocessor conditionals can only be replaced within the scope of the preprocessor conditional:
//// C++ TO C# CONVERTER NOTE: The following #define macro was replaced in-line:
//// ORIGINAL LINE: #define GameServer.Cmd_C_ITEM_TICKET_REPORT_ID_LOW c[2]
//#define GameServer.Cmd_C_ITEM_TICKET_REPORT_ID_LOW
//// C++ TO C# CONVERTER TASK: #define macros defined in multiple preprocessor conditionals can only be replaced within the scope of the preprocessor conditional:
//// C++ TO C# CONVERTER NOTE: The following #define macro was replaced in-line:
//// ORIGINAL LINE: #define GameServer.Cmd_C_ITEM_TIME c[3]
//#define GameServer.Cmd_C_ITEM_TIME
//#define GameServer.Cmd_INVITE_TIME_MILLISECONDS

//namespace GameServer.Cmd
//{
//	public class CmdUpdateUCC : Pangya_DB
//	{
//			public enum T_UPDATE : byte
//			{
//				TEMPORARY,
//				FOREVER,
//				COPY
//			}

//			public CmdUpdateUCC()
//			{
//				this.m_uid = 0u;
//				this.m_wi = WarehouseItemEx();
//				this.m_dt_draw =  0 ;
//				this.m_type = T_UPDATE.TEMPORARY;
//			}

//			public CmdUpdateUCC(uint _uid,
//				WarehouseItemEx _wi,
//				PangyaTime _si, T_UPDATE _type
//				)
//				{	 this.m_uid = _uid;		 
//				this.m_wi = _wi;	
//			this.m_dt_draw = _si;			   
//				this.m_type = _type;
//				}
							 

//			public uint getUID()
//			{
//				return m_uid;
//			}

//			public void setUID(uint _uid)
//			{	    m_uid = _uid;
// 			}

//			public PangyaTime getDrawDate()
//			{
//				return m_dt_draw;
//			}

//			public void setDrawDate(PangyaTime _si)
//			{		 m_dt_draw = _si;
//			}

//			public CmdUpdateUCC.T_UPDATE getType()
//			{
//				return m_type;
//			}

//			public void setType(T_UPDATE _type)
//			{
//				m_type = _type;
//			}

//			public WarehouseItemEx getInfo()
//			{		 return m_wi;
//			}

//			public void setInfo(WarehouseItemEx _wi)
//			{	    m_wi = _wi;
//			}

//			protected override void lineResult(ctx_res _result, uint _index_result)
//			{

//				checkColumnNumber(1);

//				if(m_type == T_UPDATE.COPY)
//				{
//					m_wi.ucc.seq = IFNULL<short>(_result.data[0]);
//				}

//				// Ignora o retorno dos outros tipos

//				return;
//			}

//			protected override Response prepareConsulta()
//			{

//				if(m_uid == 0)
//				{
//					throw new exception("[CmdSaveUCC::prepareConsulta][Error] m_uid is invalid(zero)", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PANGYA_DB,
//						4, 0));
//				}

//				if(m_wi._typeid == 0)
//				{
//					throw new exception("[CmdSaveUCC::prepareConsulta][Error] m_typeid is invalid(zero)", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PANGYA_DB,
//						4, 0));
//				}

//				if(m_wi.ucc.idx[0] == '\0' || m_wi.ucc.db_name[0] == '\0')
//				{
//					throw new exception("[CmdSaveUCC::prepareConsulta][Error] UCC[IDX=" + m_wi.ucc.idx + ", NAME=" + m_wi.ucc.db_name + "] is invalid", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PANGYA_DB,
//						4, 0));
//				}

//				var r = procedure(
//					m_szConsulta,
//					Convert.ToString(m_uid) + ", " + Convert.ToString(m_wi.id) + ", " + _db.makeText(m_wi.ucc.idx) + ", " + _db.makeText(m_wi.ucc.db_name) + ", " + ((m_dt_draw.Year == 0) ? "NULL" : _db.makeText("") + ", " + ((m_wi.ucc.copier_nick[0] == '\0') ? "NULL" : _db.makeText(m_wi.ucc.copier_nick)) + ", " + Convert.ToString(m_wi.ucc.copier) + ", " + Convert.ToString((ushort)m_wi.ucc.status) + ", " + _db.makeText(((m_type == T_UPDATE.TEMPORARY) ? "T" : "Y")) + ", " + Convert.ToString(m_type));

//				checkResponse(r, "nao conseguiu salvar o UCC[TYPEID=" + Convert.ToString(m_wi._typeid) + ", ID=" + Convert.ToString(m_wi.id) + ", UCCIDX=" + m_wi.ucc.idx + ", NAME=" + m_wi.ucc.db_name + "] do player[UID=" + Convert.ToString(m_uid) + "]");

//				return r;
//			}
																							 

//			private uint m_uid = new uint();
//			private WarehouseItemEx m_wi = new WarehouseItemEx();
//			private PangyaTime m_dt_draw = new PangyaTime();
//			private T_UPDATE m_type;

//			private const string m_szConsulta = "pangya.ProcUpdateUCC";
//	}
//}

//#endif // !_GameServer.Cmd_CMD_UPDATE_UCC_HPP
