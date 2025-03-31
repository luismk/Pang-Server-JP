using LoginServer.GameType;
using PangyaAPI.SQL;
using PangyaAPI.Utilities;
using System;

namespace LoginServer.Cmd
{
	public class CmdFirstLoginCheck : Pangya_DB
	{
			public CmdFirstLoginCheck()
			{
				this.m_uid = 0;
				this.m_check = false;
			}

			public CmdFirstLoginCheck(uint _uid, bool _waiter = false) : base(_waiter)
			{
// C++ TO C# CONVERTER TASK: The following line was determined to be a copy assignment (rather than a reference assignment) - this should be verified and a 'CopyFrom' method should be created:
this.m_uid = _uid;
				this.m_uid.CopyFrom(_uid);
				this.m_check = false;
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
// C++ TO C# CONVERTER TASK: The following line was determined to be a copy assignment (rather than a reference assignment) - this should be verified and a 'CopyFrom' method should be created:
m_uid = _uid;
				m_uid.CopyFrom(_uid);
			}

			public bool getLastCheck()
			{
				return m_check;
			}

			protected override void lineResult(ctx_res _result, uint _index_result)
			{

				checkColumnNumber(1, (uint)_result.cols);

				m_check = (IFNULL(atoi, _result.data[0]) == 1 ? true : false);
			}

			protected override response prepareConsulta()
			{

				m_check = false;

				var r = consulta(_db, m_szConsulta + Convert.ToString(m_uid));

				checkResponse(r, "nao conseguiu verificar o first login do player: " + Convert.ToString(m_uid));

				return r;
			}

			protected override string _getName()
			{
				return "CmdFirstLoginCheck";
			}
			protected override string _wgetName()
			{
				return "CmdFirstLoginCheck";
			}

			private uint m_uid = new uint();
			private bool m_check;

			private const string m_szConsulta = "SELECT FIRST_LOGIN FROM pangya.account WHERE uid = ";
	}
}

#endif // !_STDA_CMD_FIRST_LOGIN_CHECK_HPP
