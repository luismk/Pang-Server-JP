using LoginServer.GameType;
using PangyaAPI.SQL;
using PangyaAPI.Utilities;
using System;

namespace LoginServer.Cmd
{
	public class CmdRegisterLogonServer : Pangya_DB
	{
			public CmdRegisterLogonServer()
			{
				this.m_uid = 0;
				this.m_server_uid = 0;
			}

			public CmdRegisterLogonServer(uint _uid,
				uint _server_uid)
				{
// C++ TO C# CONVERTER TASK: The following line was determined to be a copy assignment (rather than a reference assignment) - this should be verified and a 'CopyFrom' method should be created:
this.m_uid = _uid;
				this.m_uid.CopyFrom(_uid);
// C++ TO C# CONVERTER TASK: The following line was determined to be a copy assignment (rather than a reference assignment) - this should be verified and a 'CopyFrom' method should be created:
this.m_server_uid = _server_uid;
				this.m_server_uid.CopyFrom(_server_uid);
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

			public uint getServerUID()
			{
				return new uint(m_server_uid);
			}

			public void setServerUID(uint _server_uid)
			{
// C++ TO C# CONVERTER TASK: The following line was determined to be a copy assignment (rather than a reference assignment) - this should be verified and a 'CopyFrom' method should be created:
m_server_uid = _server_uid;
				m_server_uid.CopyFrom(_server_uid);
			}

			protected override void lineResult(ctx_res _result, uint _index_result)
			{

				// N�o usa por que � um UPDATE
				return;
			}

			protected override response prepareConsulta()
			{

				var r = procedure(m_szConsulta,
					Convert.ToString(m_uid) + ", " + Convert.ToString(m_server_uid));

				checkResponse(r, "nao conseguiu registrar o logon no server[UID=" + Convert.ToString(m_server_uid) + "] do player: " + Convert.ToString(m_uid));

				return r;
			}

			protected override string _getName()
			{
				return "CmdRegisterLogonServer";
			}
			protected override string _wgetName()
			{
				return "CmdRegisterLogonServer";
			}

			private uint m_uid = new uint();
			private uint m_server_uid = new uint();

			private const string m_szConsulta = "pangya.ProcRegisterLogonServer";
	}
}
