//using GameServer.Cmd;
//using System;
				  
//namespace GameServer.Cmd
//{
//	public class CmdAddBall : Pangya_DB
//	{
//			public CmdAddBall()
//			{
//				this.m_uid = 0u;
//				this.m_purchase = 0;
//				this.m_gift_flag = 0;
//				this.m_wi = new GameServer.Cmd.WarehouseItemEx(0);
//			}

//			public CmdAddBall(uint _uid,
//				WarehouseItemEx _wi,
//				byte _purchase,
//				byte _gift_flag,
//				)
//				{
//// C++ TO C# CONVERTER TASK: The following line was determined to be a copy assignment (rather than a reference assignment) - this should be verified and a 'CopyFrom' method should be created:
//// ORIGINAL LINE: this.m_uid = _uid;
//				this.m_uid.CopyFrom(_uid);
//				this.m_purchase = _purchase;
//				this.m_gift_flag = _gift_flag;
//				this.m_wi = new GameServer.Cmd.WarehouseItemEx(_wi);
//				}

//			public virtual void Dispose()
//			{
//			}

//			public uint getUID()
//			{
//				return new uint(m_uid);
//			}

//			public void setUID(uint _uid)
//			{
//// C++ TO C# CONVERTER TASK: The following line was determined to be a copy assignment (rather than a reference assignment) - this should be verified and a 'CopyFrom' method should be created:
//// ORIGINAL LINE: m_uid = _uid;
//				m_uid.CopyFrom(_uid);
//			}

//			public byte getPurchase()
//			{
//				return m_purchase;
//			}

//			public void setPurchase(byte _purchase)
//			{
//				m_purchase = _purchase;
//			}

//			public byte getGiftFlag()
//			{
//				return m_gift_flag;
//			}

//			public void setGiftFlag(byte _gift_flag)
//			{
//				m_gift_flag = _gift_flag;
//			}

//			public WarehouseItemEx getInfo()
//			{
//// C++ TO C# CONVERTER TASK: The following line was determined to contain a copy constructor call - this should be verified and a copy constructor should be created:
//// ORIGINAL LINE: return m_wi;
//				return new GameServer.Cmd.WarehouseItemEx(m_wi);
//			}

//			public void setInfo(WarehouseItemEx _wi)
//			{
//// C++ TO C# CONVERTER TASK: The following line was determined to be a copy assignment (rather than a reference assignment) - this should be verified and a 'CopyFrom' method should be created:
//// ORIGINAL LINE: m_wi = _wi;
//				m_wi.CopyFrom(_wi);
//			}

//			protected override void lineResult(ctx_res _result, uint _index_result)
//			{

//				checkColumnNumber(1);

//				m_wi.id = IFNULL(_result.data[0]);
//			}

//			protected override Response prepareConsulta()
//			{

//				if(m_wi._typeid == 0)
//				{
//					throw new exception("[CmdAddBall::prepareConsulta][Error] ball is invalid", ExceptionError.GameServer.Cmd_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PANGYA_DB,
//						4, 0));
//				}

//				var r = procedure(
//					m_szConsulta,
//					Convert.ToString(m_uid) + ", " + Convert.ToString((ushort)m_gift_flag) + ", " + Convert.ToString((ushort)m_purchase) + ", " + Convert.ToString(m_wi.id) + ", " + Convert.ToString(m_wi._typeid) + ", " + Convert.ToString((ushort)m_wi.flag) + ", " + Convert.ToString(m_wi.c[3]) + ", " + Convert.ToString(m_wi.c[0]) + ", " + Convert.ToString(m_wi.c[1]) + ", " + Convert.ToString(m_wi.c[2]) + ", " + Convert.ToString(m_wi.c[3]) + ", " + Convert.ToString(m_wi.c[4]));

//				checkResponse(r, "nao conseguiu adicionar Ball[TYPEID=" + Convert.ToString(m_wi._typeid) + "] para o player[UID=" + Convert.ToString(m_uid) + "]");

//				return r;
//			}		  
//			private uint m_uid = new uint();
//			private byte m_purchase;
//			private byte m_gift_flag;
//			private WarehouseItemEx m_wi = new WarehouseItemEx();

//			private const string m_szConsulta = "pangya.ProcAddBall";
//	}
//}
