﻿using LoginServer.GameType;
using PangyaAPI.SQL;
using PangyaAPI.Utilities;
using System;

namespace LoginServer.Cmd
{
	public class CmdRegisterPlayerLogin : Pangya_DB
	{
			public CmdRegisterPlayerLogin()
			{
				this.m_uid = 0;
				this.m_ip = new string();
				this.m_server_uid = 0;
			}

			public CmdRegisterPlayerLogin(uint _uid,
				string _ip,
				uint _server_uid)
				{
this.m_uid = _uid;
				this.m_ip = _ip;
this.m_server_uid = _server_uid;
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
m_uid = _uid;
				m_uid.CopyFrom(_uid);
			}

			public string getIP()
			{
				return m_ip;
			}

			public void setIP(string _ip)
			{
				m_ip = _ip;
			}

			public uint getServerUID()
			{
				return new uint(m_server_uid);
			}

			public void setServerUID(uint _server_uid)
			{
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

				if(m_ip.Length == 0)
				{
					throw exception("[CmdRegisterPlayerLogin::prepareConsulta][Error] ip is invalid", STDA_MAKE_ERROR(STDA_ERROR_TYPE.PANGYA_DB,
						4, 0));
				}

				var r = procedure(m_szConsulta,
					Convert.ToString(m_uid) + ", " +(m_ip) + ", " + Convert.ToString(m_server_uid));

				checkResponse(r, "nao conseguiu registrar o login do player: " + Convert.ToString(m_uid) + ", IP: " + m_ip);

				return r;
			}

			protected override string _getName()
			{
				return "CmdRegisterPlayerLogin";
			}
			protected override string _wgetName()
			{
				return "CmdRegisterPlayerLogin";
			}

			private uint m_uid = new uint();
			private uint m_server_uid = new uint();
			private string m_ip = "";

			private const string m_szConsulta = "pangya.ProcRegisterLogin";
	}
}
