using GameServer.GameType;
using PangyaAPI.Network.Pangya_St;
using PangyaAPI.SQL;
using PangyaAPI.Utilities;
using System;
namespace GameServer.Cmd
{

	public class CmdUpdateClubSetTime : Pangya_DB
	{

			public CmdUpdateClubSetTime(uint _uid,
				WarehouseItemEx _wi)
				{
// C++ TO C# CONVERTER TASK: The following line was determined to be a copy assignment (rather than a reference assignment) - this should be verified and a 'CopyFrom' method should be created:
 this.m_uid = _uid;
				//this.
				this.m_wi = (_wi);
				}
				 
			public uint getUID()
			{
				return (m_uid);
			}

			public void setUID(uint _uid)
			{
// C++ TO C# CONVERTER TASK: The following line was determined to be a copy assignment (rather than a reference assignment) - this should be verified and a 'CopyFrom' method should be created:
 m_uid = _uid;
				
			}

			public WarehouseItemEx getClubSet()
			{
// C++ TO C# CONVERTER TASK: The following line was determined to contain a copy constructor call - this should be verified and a copy constructor should be created:
 return m_wi;
 			}

			public void setClubSet(WarehouseItemEx _wi)
			{
// C++ TO C# CONVERTER TASK: The following line was determined to be a copy assignment (rather than a reference assignment) - this should be verified and a 'CopyFrom' method should be created:
 m_wi = _wi;
 			}

			protected override void lineResult(ctx_res _result, uint _index_result)
			{

				// N�o usa por que � um UPDATE
				return;
			}

			protected override Response prepareConsulta()
			{

				if(m_uid == 0u)
				{
					throw new exception("[CmdUpdateClubSetTime::prepareConsulta][Error] m_uid is invalid(" + Convert.ToString(m_uid) + ")", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PANGYA_DB,
						4, 0));
				}

				if(m_wi.id <= 0)
				{
					throw new exception("[CmdUpdateClubSetTime::prepareConsulta][Error] m_wi.id is invalid(" + Convert.ToString(m_wi.id) + ")", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PANGYA_DB,
						4, 0));
				}

				if(m_wi._typeid == 0u)
				{
					throw new exception("[CmdUpdateClubSetTime::prepareConsulta][Error] m_wi._typeid is invalid(" + Convert.ToString(m_wi._typeid) + ")", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PANGYA_DB,
						4, 0));
				}

				var r = procedure(
					m_szConsulta,
					Convert.ToString(m_uid) + ", " + Convert.ToString(m_wi.id) + ", " + Convert.ToString(m_wi._typeid) + ", " + _db.makeText(formatDateLocal(m_wi.end_date_unix_local)));

				checkResponse(r, "nao conseguiu atualizar o tempo do ClubSet[ID=" + Convert.ToString(m_wi.id) + ", TYPEID=" + Convert.ToString(m_wi._typeid) + ", ENDDATE=" + formatDateLocal(m_wi.end_date_unix_local) + "] do Player[UID=" + Convert.ToString(m_uid) + "]");

				return r;
			}
									   

			private uint m_uid = new uint();
			private WarehouseItemEx m_wi = new WarehouseItemEx();

			private const string m_szConsulta = "pangya.ProcUpdateClubSetTime";
	}
}
