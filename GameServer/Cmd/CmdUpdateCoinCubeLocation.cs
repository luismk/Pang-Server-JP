//using Pangya_GameServer.GameType;
//using PangyaAPI.Network.Pangya_St;
//using PangyaAPI.SQL;
//using PangyaAPI.Utilities;
//using System;
//namespace Pangya_GameServer.Cmd
//{

//	public class CmdUpdateCoinCubeLocation : Pangya_DB
//	{

//		public CmdUpdateCoinCubeLocation()
//		{
//			this.m_ccu = new < type missing > ();
//		}

//		public CmdUpdateCoinCubeLocation(CoinCubeUpdate _ccu)
//		{
//			this.m_ccu = _ccu;
//		}

//		public virtual void Dispose()
//		{
//		}

//		public CoinCubeUpdate getInfo()
//		{
//			return m_ccu;
//		}

//		public void setInfo(CoinCubeUpdate _ccu)
//		{
//			m_ccu = _ccu;
//		}

//		protected override void lineResult(ctx_res _result, uint _index)
//		{
//			// N�o usa por que � UPDATE e INSERT
//			return;
//		}

//		protected override Response prepareConsulta()
//		{

//			if (m_ccu.hole_number < 1 || m_ccu.hole_number > 18)
//			{
//				throw new exception("[CmdUpdateCoinCubeLocation::prepareConsulta][Error] m_ccu.hole_number(" + Convert.ToString((ushort)m_ccu.hole_number) + ") invalid", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PANGYA_DB,
//					4, 0));
//			}

//			// Prote��o contra os jogos random & 0x7F
//			if (sIff.getInstance().findCourse(iff.COURSE << 0x1A | (m_ccu.course_id & 0x7Fu)) == null)
//			{
//				throw new exception("[CmdUpdateCoinCubeLocation::prepareConsulta][Error] m_ccu.course_id(" + Convert.ToString((ushort)m_ccu.course_id) + ") not exists in IFF_STRUCT", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PANGYA_DB,
//					4, 0));
//			}

//			response r = null;

//			if (m_ccu.type == CoinCubeUpdate.eTYPE.UPDATE)
//			{

//				if (m_ccu.cube.id == 0u)
//				{
//					throw new exception("[CmdUpdateCoinCubeLocation::prepareConsulta][Error] invalid coin/cube id(" + Convert.ToString(m_ccu.cube.id) + ") to Update in Database", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PANGYA_DB,
//						4, 0));
//				}

//				r = procedure(_db,
//					m_szConsulta[1],
//					Convert.ToString(m_ccu.cube.id) + ", " + Convert.ToString((ushort)m_ccu.course_id) + ", " + Convert.ToString((ushort)m_ccu.hole_number) + ", " + Convert.ToString(m_ccu.cube.tipo) + ", " + Convert.ToString(m_ccu.cube.flag_location) + ", " + Convert.ToString(m_ccu.cube.rate) + ", " + Convert.ToString(m_ccu.cube.location.x) + ", " + Convert.ToString(m_ccu.cube.location.y) + ", " + Convert.ToString(m_ccu.cube.location.z));

//				checkResponse(r, "Nao conseguiu atualizar o Coin/Cube[ID=" + Convert.ToString(m_ccu.cube.id) + ", COURSE_ID=" + Convert.ToString((ushort)m_ccu.course_id) + ", HOLE=" + Convert.ToString((ushort)m_ccu.hole_number) + ", TIPO=" + Convert.ToString(m_ccu.cube.tipo) + ", TIPO_LOCATION=" + Convert.ToString(m_ccu.cube.flag_location) + ", RATE=" + Convert.ToString(m_ccu.cube.rate) + ", X=" + Convert.ToString(m_ccu.cube.location.x) + ", Y=" + Convert.ToString(m_ccu.cube.location.y) + ", Z=" + Convert.ToString(m_ccu.cube.location.z) + "]");

//			}
//			else
//			{

//				// Add new Coin/Cube Location
//				r = procedure(_db,
//					m_szConsulta[0],
//					Convert.ToString((ushort)m_ccu.course_id) + ", " + Convert.ToString((ushort)m_ccu.hole_number) + ", " + Convert.ToString(m_ccu.cube.tipo) + ", " + Convert.ToString(m_ccu.cube.flag_location) + ", " + Convert.ToString(m_ccu.cube.rate) + ", " + Convert.ToString(m_ccu.cube.location.x) + ", " + Convert.ToString(m_ccu.cube.location.y) + ", " + Convert.ToString(m_ccu.cube.location.z));

//				checkResponse(r, "Nao conseguiu adicionar o Coin/Cube[COURSE_ID=" + Convert.ToString((ushort)m_ccu.course_id) + ", HOLE=" + Convert.ToString((ushort)m_ccu.hole_number) + ", TIPO=" + Convert.ToString(m_ccu.cube.tipo) + ", TIPO_LOCATION=" + Convert.ToString(m_ccu.cube.flag_location) + ", RATE=" + Convert.ToString(m_ccu.cube.rate) + ", X=" + Convert.ToString(m_ccu.cube.location.x) + ", Y=" + Convert.ToString(m_ccu.cube.location.y) + ", Z=" + Convert.ToString(m_ccu.cube.location.z) + "]");
//			}

//			return r;
//		}

//		private CoinCubeUpdate m_ccu = new CoinCubeUpdate();

//		private string[] m_szConsulta = { "pangya.ProcInsertCoinCubeLocation", "pangya.ProcUpdateCoinCubeLocation" };
//	}
//}
