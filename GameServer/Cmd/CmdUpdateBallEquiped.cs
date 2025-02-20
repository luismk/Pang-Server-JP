using GameServer.GameType;
using PangyaAPI.SQL;
using System;								 

namespace GameServer.Cmd
{
	public class CmdUpdateBallEquiped : Pangya_DB
	{
			public CmdUpdateBallEquiped()
			{
				this.m_uid = 0;
				this.m_ball_typeid = 0;
			}

			public CmdUpdateBallEquiped(uint _uid, uint _ball_typeid)
				{	  this.m_uid = _uid; 
			this.m_ball_typeid = _ball_typeid;		 
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

			public uint getBallTypeid()
			{
				return (m_ball_typeid);
			}

			public void setBallTypeid(uint _ball_typeid)
			{
  m_ball_typeid = _ball_typeid;
			}

			protected override void lineResult(ctx_res _result, uint _index_result)
			{

				// N�o usa por que � um update
				return;
			}

			protected override Response prepareConsulta()
			{

				var r = procedure(
					m_szConsulta,
					Convert.ToString(m_uid) + ", " + Convert.ToString(m_ball_typeid));

				checkResponse(r, "nao conseguiu atualizar a bola[TYPEID=" + Convert.ToString(m_ball_typeid) + "] equipada do player: " + Convert.ToString(m_uid));

				return r;
			}		 
			private uint m_uid = new uint();
			private uint m_ball_typeid = new uint();

			private const string m_szConsulta = "pangya.USP_FLUSH_COMET";
	}
}
