using PangyaAPI.SQL;


namespace PangyaAPI.TCP.Cmd
{
    public class CmdAddItemBase: Pangya_DB
    {
       public int m_uid = -1;
      public  byte m_purchase;
       public byte m_gift_flag;
        public CmdAddItemBase(int _uid, byte _purchase, byte _gift_flag)
        {
            m_purchase = _purchase;
            m_gift_flag = _gift_flag;
            m_uid = _uid;
        }
      
       public int getUID()
        {
            return m_uid;
        }

        public void setUID(int _uid)
        {
            m_uid = _uid;
        }

       public byte getPurchase()
        {
            return m_purchase;
        }

        public void setPurchase(byte _purchase)
        {
            m_purchase = _purchase;
        }

       public byte getGiftFlag()
        {
            return m_gift_flag;
        }

        public void setGiftFlag(byte _gift_flag)
        {
            m_gift_flag = _gift_flag;
        }

        protected override void lineResult(ctx_res _result, uint _index_result)
        {
        }

        protected override Response prepareConsulta()
        {
            return null;
        }
    }
}
