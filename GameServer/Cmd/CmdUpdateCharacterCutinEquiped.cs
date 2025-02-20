﻿using GameServer.GameType;
using PangyaAPI.Network.Pangya_St;
using PangyaAPI.SQL;
using PangyaAPI.Utilities;
using System;

namespace GameServer.Cmd
{
	public class CmdUpdateCharacterCutinEquiped : Pangya_DB
	{		    
			public CmdUpdateCharacterCutinEquiped(uint _uid,
				CharacterInfo _ci)
				{
// C++ TO C# CONVERTER TASK: The following line was determined to be a copy assignment (rather than a reference assignment) - this should be verified and a 'CopyFrom' method should be created:
 this.m_uid = _uid;
				//this.
// C++ TO C# CONVERTER TASK: The following line was determined to be a copy assignment (rather than a reference assignment) - this should be verified and a 'CopyFrom' method should be created:
 this.m_ci = _ci;						 
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

			public CharacterInfo getInfo()
			{
				return (m_ci);
			}

			public void setInfo(CharacterInfo _ci)
			{
// C++ TO C# CONVERTER TASK: The following line was determined to be a copy assignment (rather than a reference assignment) - this should be verified and a 'CopyFrom' method should be created:
 m_ci = _ci;
			}

			protected override void lineResult(ctx_res _result, uint _index_result)
			{

				// N�o usa por que � um UPDATE
				return;
			}

			protected override Response prepareConsulta()
			{

				var r = procedure(
					m_szConsulta,
					Convert.ToString(m_uid) + ", " + Convert.ToString(m_ci.id) + ", " + Convert.ToString(m_ci.cut_in[0]) + ", " + Convert.ToString(m_ci.cut_in[1]) + ", " + Convert.ToString(m_ci.cut_in[2]) + ", " + Convert.ToString(m_ci.cut_in[3]));

				checkResponse(r, "nao conseguiu atualizar o character[ID=" + Convert.ToString(m_ci.id) + "] cutin equipado do player: " + Convert.ToString(m_uid));

				return r;
			}

		 

			private uint m_uid = new uint();
			private CharacterInfo m_ci = new CharacterInfo();

			private const string m_szConsulta = "pangya.USP_FLUSHCHARACTERCUTIN";
	}
}
