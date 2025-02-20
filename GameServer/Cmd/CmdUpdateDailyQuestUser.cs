 
//namespace GameServer.Cmd
//{
//	public class CmdUpdateDailyQuestUser : Pangya_DB
//	{
//			public CmdUpdateDailyQuestUser()
//			{
//				this.m_uid = 0u;
//				this.m_dqiu = new DailyQuestInfoUser(0);
//			}

//			public CmdUpdateDailyQuestUser(uint _uid,
//				DailyQuestInfoUser _dqiu)
//				{
//// C++ TO C# CONVERTER TASK: The following line was determined to be a copy assignment (rather than a reference assignment) - this should be verified and a 'CopyFrom' method should be created:
// this.m_uid = _uid;
//				//this.
//				this.m_dqiu = new DailyQuestInfoUser(_dqiu);
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

//			public DailyQuestInfoUser getInfo()
//			{
//// C++ TO C# CONVERTER TASK: The following line was determined to contain a copy constructor call - this should be verified and a copy constructor should be created:
// return m_dqiu;
//				return new DailyQuestInfoUser(m_dqiu);
//			}

//			public void setInfo(DailyQuestInfoUser _dqiu)
//			{
//// C++ TO C# CONVERTER TASK: The following line was determined to be a copy assignment (rather than a reference assignment) - this should be verified and a 'CopyFrom' method should be created:
// m_dqiu = _dqiu;
//				m_dqiu.CopyFrom(_dqiu);
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
//					throw new exception("[CmdUpdateDailyQuestUser][Error] m_uid is invalid(zero).", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PANGYA_DB,
//						4, 0));
//				}

//				string accept_dt = "null";
//				string today_dt = "null";

//				if(m_dqiu.accept_date != 0)
//				{
//					accept_dt = _db.makeText(formatDateLocal(m_dqiu.accept_date));
//				}

//				if(m_dqiu.current_date != 0)
//				{
//					today_dt = _db.makeText(formatDateLocal(m_dqiu.current_date));
//				}

//				var r = procedure(
//					m_szConsulta,
//					Convert.ToString(m_uid) + ", " + accept_dt + ", " + today_dt);

//				checkResponse(r, "nao conseguiu Atualizar o DailyQuest[ACCEPT_DT=" + accept_dt + ", TODAY_DT=" + today_dt + "] do player[UID=" + Convert.ToString(m_uid) + "]");

//				return r;
//			}

//			protected override string _getName()
//			{
//				return "CmdUpdateDailyQuestUser";
//			}
//			protected override string _wgetName()
//			{
//				return "CmdUpdateDailyQuestUser";
//			}

//			private uint m_uid = new uint();
//			private DailyQuestInfoUser m_dqiu = new DailyQuestInfoUser();

//			private const string m_szConsulta = "pangya.ProcUpdateDailyQuestUser";
//	}
//}
