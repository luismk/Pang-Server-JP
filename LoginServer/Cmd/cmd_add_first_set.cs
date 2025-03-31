using LoginServer.GameType;
using PangyaAPI.SQL;
using PangyaAPI.Utilities;
using System;

namespace LoginServer.Cmd
{
	public class CmdAddFirstSet : Pangya_DB
	{
			public CmdAddFirstSet()
			{
				this.m_uid = 0;
			}

			public CmdAddFirstSet(uint _uid, bool _waiter = false) : base(_waiter)
			{
// C++ TO C# CONVERTER TASK: The following line was determined to be a copy assignment (rather than a reference assignment) - this should be verified and a 'CopyFrom' method should be created:
this.m_uid = _uid;
				this.m_uid.CopyFrom(_uid);
			}

			public void Dispose()
			{
			}

			public uint getUID()
			{
				return new uint(m_uid);
			}

			public void setUID(uint _uid)
			{
				m_uid;
			}

			protected override void lineResult(ctx_res _result, uint _index_result)
			{

				// N�o usa por que � INSERT e UPDATE
				return;
			}

			protected override response prepareConsulta()
			{

				var r = procedure(m_szConsulta,
					Convert.ToString(m_uid));

				checkResponse(r, "nao conseguiu add first set do player: " + Convert.ToString(m_uid));

				return r;
			}

			protected override string _getName()
			{
				return "CmdAddFirstSet";
			}
			protected override string _wgetName()
			{
				return "CmdAddFirstSet";
			}

			private uint m_uid = new uint();

			private const string m_szConsulta = "pangya.ProcFirstSet";
	}
}
