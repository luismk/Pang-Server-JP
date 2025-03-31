using LoginServer.GameType;
using PangyaAPI.SQL;
using PangyaAPI.Utilities;
using System;

namespace LoginServer.Cmd
{
	public class CmdPlayerInfo : Pangya_DB
	{
			public CmdPlayerInfo()
			{
				this.m_uid = 0;
				this.m_pi = new stdA.player_info();
			}

			public CmdPlayerInfo(uint _uid, bool _waiter = false) : base(_waiter)
			{
// C++ TO C# CONVERTER TASK: The following line was determined to be a copy assignment (rather than a reference assignment) - this should be verified and a 'CopyFrom' method should be created:
this.m_uid = _uid;
				this.m_uid.CopyFrom(_uid);
				this.m_pi = new stdA.player_info();
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

			public player_info getInfo()
			{
// C++ TO C# CONVERTER TASK: The following line was determined to contain a copy constructor call - this should be verified and a copy constructor should be created:
return m_pi;
				return new stdA.player_info(m_pi);
			}

			public void updateInfo(player_info _pi)
			{
// C++ TO C# CONVERTER TASK: The following line was determined to be a copy assignment (rather than a reference assignment) - this should be verified and a 'CopyFrom' method should be created:
m_pi = _pi;
				m_pi.CopyFrom(_pi);
			}

			protected override void lineResult(ctx_res _result, uint _index_result)
			{

				checkColumnNumber(8, (uint)_result.cols);

				// Aqui faz as coisas
				m_pi.uid = IFNULL(atoi, _result.data[0]);
				if(is_valid_c_string(_result.data[1]))
				{
					STRCPY_TO_MEMORY_FIXED_SIZE(m_pi.id,
						sizeof(char), _result.data[1]);
				}
				if(is_valid_c_string(_result.data[2]))
				{
					STRCPY_TO_MEMORY_FIXED_SIZE(m_pi.nickname,
						sizeof(char), _result.data[2]);
				}
				if(is_valid_c_string(_result.data[3]))
				{
					STRCPY_TO_MEMORY_FIXED_SIZE(m_pi.pass,
						sizeof(char), _result.data[3]);
				}
				m_pi.m_cap = IFNULL(atoi, _result.data[4]);
				m_pi.level = (ushort)IFNULL(atoi, _result.data[5]);
				m_pi.block_flag.setIDState((uint64_t)IFNULL(atoll, _result.data[6]));
				m_pi.block_flag.m_id_state.block_time = IFNULL(atoi, _result.data[7]);
				// Fim

				if(m_pi.uid != m_uid)
				{
					throw exception("[CmdPlayerInfo::lineResult][Error] UID do player info nao e igual ao requisitado. UID Req: " + Convert.ToString(m_uid) + " != " + Convert.ToString(m_pi.uid), STDA_MAKE_ERROR(STDA_ERROR_TYPE.PANGYA_DB,
						3, 0));
				}
			}

			protected override response prepareConsulta()
			{

				m_pi.clear();

				var r = procedure(m_szConsulta,
					Convert.ToString(m_uid));

				checkResponse(r, "nao conseguiu pegar o info do player: " + Convert.ToString(m_uid));

				return r;
			}

			protected override string _getName()
			{
				return "CmdPlayerInfo";
			}
			protected override string _wgetName()
			{
				return "CmdPlayerInfo";
			}

			protected player_info m_pi = new player_info();
			protected uint m_uid = new uint();

			private const string m_szConsulta = "pangya.ProcGetPlayerInfoLogin";
	}
}


