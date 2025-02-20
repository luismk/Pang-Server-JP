using GameServer.GameType;
using PangyaAPI.SQL;
using PangyaAPI.Utilities;
using System;  
namespace GameServer.Cmd
{
	public class CmdUpdateAttendanceReward : Pangya_DB
	{				    

			public CmdUpdateAttendanceReward(uint _uid,
				AttendanceRewardInfoEx _ari)
				{
// C++ TO C# CONVERTER TASK: The following line was determined to be a copy assignment (rather than a reference assignment) - this should be verified and a 'CopyFrom' method should be created:
 this.m_uid = _uid;
				//this.
				this.m_ari = (_ari);
				}

			public virtual void Dispose()
			{
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

			public AttendanceRewardInfoEx getInfo()
			{
// C++ TO C# CONVERTER TASK: The following line was determined to contain a copy constructor call - this should be verified and a copy constructor should be created:
 return m_ari;
			}

			public void setInfo(AttendanceRewardInfoEx _ari)
			{
// C++ TO C# CONVERTER TASK: The following line was determined to be a copy assignment (rather than a reference assignment) - this should be verified and a 'CopyFrom' method should be created:
 m_ari = _ari;
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
					throw new exception("[CmdUpdateAttendanceReward::prepareConsulta][Error] m_uid is invalid(zero).", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PANGYA_DB,
						4, 0));
				}

				string last_login = "null";

				if(m_ari.last_login.Year != 0
					&& m_ari.last_login.Month != 0
					&& m_ari.last_login.Day != 0)
				{
					last_login = _db.makeText(_formatDate(m_ari.last_login.ConvertTime()));
				}

				var r = procedure(
					m_szConsulta,
					Convert.ToString(m_uid) + ", " + Convert.ToString(m_ari.counter) + ", " + Convert.ToString(m_ari.now._typeid) + ", " + Convert.ToString(m_ari.now.qntd) + ", " + Convert.ToString(m_ari.after._typeid) + ", " + Convert.ToString(m_ari.after.qntd) + ", " + last_login);

				checkResponse(r, "nao conseguiu Atualizar o Attendance Reward[COUNTER=" + Convert.ToString(m_ari.counter) + ", NOW_TYPEID=" + Convert.ToString(m_ari.now._typeid) + ", NOW_QNTD=" + Convert.ToString(m_ari.now.qntd) + ", AFTER_TYPEID=" + Convert.ToString(m_ari.after._typeid) + ", AFTER_QNTD=" + Convert.ToString(m_ari.after.qntd) + ", LAST_LOGIN=" + last_login + "] do player[UID=" + Convert.ToString(m_uid) + "]");

				return r;
			}			  
			private uint m_uid = new uint();
			private AttendanceRewardInfoEx m_ari = new AttendanceRewardInfoEx();

			private const string m_szConsulta = "pangya.ProcUpdateAttendanceReward";
	}
}
