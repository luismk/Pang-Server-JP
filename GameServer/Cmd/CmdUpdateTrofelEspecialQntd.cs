//using GameServer.Cmd;
//using PangyaAPI.SQL;
//using System;					  

//namespace GameServer.Cmd
//{
//	public class CmdUpdateTrofelEspecialQntd : Pangya_DB
//	{
//			public enum eTYPE : byte
//			{
//				ESPECIAL,
//				GRAND_PRIX
				
//			}

//			public CmdUpdateTrofelEspecialQntd()
//			{
//				this.m_uid = 0u;
//				this.m_id = -1;
//				this.m_qntd = 0u;
//				this.m_type = eTYPE.ESPECIAL;
//			}

//			public CmdUpdateTrofelEspecialQntd(uint _uid,
//				uint _id, uint _qntd,
//				eTYPE _type
//				)
//				{   this.m_uid = _uid;		 
//			this.m_id = _id;   
//			this.m_qntd = _qntd;			   
//				this.m_type =(_type);
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
//				
//			}

//			public int getId()
//			{
//				return (m_id);
//			}

//			public void setId(int _id)
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

//			public CmdUpdateTrofelEspecialQntd.eTYPE getType()
//			{
//				return m_type;
//			}

//			public void setType(eTYPE _type)
//			{
//				m_type = _type;
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
//					throw new exception("[CmdUpdateTrofelEspecialQntd::prepareConsulta][Error] m_uid is invalid(zero)", ExceptionError.GameServer.Cmd_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PANGYA_DB,
//						4, 0));
//				}

//				if(m_id <= 0)
//				{
//					throw new exception("[CmdUpdateTrofelEspecialQntd::prepareConsulta][Error] m_id[VALUE=" + Convert.ToString(m_id) + "] is invalid", ExceptionError.GameServer.Cmd_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PANGYA_DB,
//						4, 0));
//				}

//				if(m_type > eTYPE.GRAND_PRIX)
//				{
//					throw new exception("[CmdUpdateTrofelEspecialQntd::prepareConsulta][Error] m_type[VALUE=" + Convert.ToString((ushort)m_type) + "] is invalid", ExceptionError.GameServer.Cmd_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PANGYA_DB,
//						4, 0));
//				}

//				var r = consulta( m_szConsulta[(int)m_type][0] + Convert.ToString(m_qntd) + m_szConsulta[(int)m_type][1] + Convert.ToString(m_uid) + m_szConsulta[(int)m_type][2] + Convert.ToString(m_id));

//				checkResponse(r, "nao conseguiu Atualizar quantidade do Trofel Especial(" + new string(m_type == eTYPE.GRAND_PRIX ? "Grand Prix" : "") + ")[ID=" + Convert.ToString(m_id) + ", QNTD=" + Convert.ToString(m_qntd) + "] do Player[UID=" + Convert.ToString(m_uid) + "]");

//				return r;
//			}

//			protected override string _getName()
//			{
//				return "CmdUpdateTrofelEspecialQntd";
//			}
//			protected override string _wgetName()
//			{
//				return "CmdUpdateTrofelEspecialQntd";
//			}

//			private uint m_uid = new uint();
//			private uint m_qntd = new uint();
//			private int m_id = new int();
//			private eTYPE m_type;

//			private const string m_szConsulta = new char({ "UPDATE pangya.pangya_trofel_especial SET qntd = ", " WHERE UID = ", " AND item_id = " }, { "UPDATE pangya.pangya_trofel_grandprix SET qntd = ", " WHERE UID = ", " AND item_id = " });
//	}
//}
