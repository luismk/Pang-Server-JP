using GameServer.PangType;
using PangyaAPI.SQL;
using System;
using System.Data;

namespace GameServer.Cmd
{
    public class CmdTrofelInfo : Pangya_DB
    {
        private uint m_uid;
        TYPE_SEASON m_season;
        TrofelInfo m_ti = new TrofelInfo();
        public  enum TYPE_SEASON : byte
        {
            ALL,        // Todas SEASON
            ONE,        // 1
            TWO,        // 2
            THREE,      // 3
            FOUR,       // 4
            CURRENT     // Atual
        }
        protected override string _getName { get; } = "CmdTrofelInfo";
        public CmdTrofelInfo(uint _uid, TYPE_SEASON _season)
        {
            m_uid = (_uid);
            m_season = (_season);
            m_ti = new TrofelInfo();
        }
        public CmdTrofelInfo(uint _uid, uint _season)
        {
            m_uid = (_uid);
            m_season = (TYPE_SEASON)(_season);
            m_ti = new TrofelInfo();
        }

		protected override void lineResult(ctx_res _result, uint _index_result)
		{
			checkColumnNumber(39);
			try
			{
				int i =0;
                // AMA 6~1

                for (i = 0; i < 3; i++)
                    m_ti.ama_6[i] = (short)Convert.ToUInt32(_result.data[0 + i]);   // 0 + 3
                for (i = 0; i < 3; i++)
                    m_ti.ama_5[i] = (short)Convert.ToUInt32(_result.data[3 + i]);   // 3 + 3
                for (i = 0; i < 3; i++)
                    m_ti.ama_4[i] = (short)Convert.ToUInt32(_result.data[6 + i]);   // 6 + 3
                for (i = 0; i < 3; i++)
                    m_ti.ama_3[i] = (short)Convert.ToUInt32(_result.data[9 + i]);   // 9 + 3
                for (i = 0; i < 3; i++)
                    m_ti.ama_2[i] = (short)Convert.ToUInt32(_result.data[12 + i]);  // 12 + 3
                for (i = 0; i < 3; i++)
                    m_ti.ama_1[i] = (short)Convert.ToUInt32(_result.data[15 + i]);  // 15 + 3
                for (i = 0; i < 3; i++)
                    m_ti.pro_1[i] = (short)Convert.ToUInt32(_result.data[18 + i]);  // 18 + 3
                for (i = 0; i < 3; i++)
                    m_ti.pro_2[i] = (short)Convert.ToUInt32(_result.data[21 + i]);  // 21 + 3
                for (i = 0; i < 3; i++)
                    m_ti.pro_3[i] = (short)Convert.ToUInt32(_result.data[24 + i]);  // 24 + 3
                for (i = 0; i < 3; i++)
                    m_ti.pro_4[i] = (short)Convert.ToUInt32(_result.data[27 + i]);  // 27 + 3
                for (i = 0; i < 3; i++)
                    m_ti.pro_5[i] = (short)Convert.ToUInt32(_result.data[30 + i]);  // 30 + 3
                for (i = 0; i < 3; i++)
                    m_ti.pro_6[i] = (short)Convert.ToUInt32(_result.data[33 + i]);  // 33 + 3
                for (i = 0; i < 3; i++)
                    m_ti.pro_7[i] = (short)Convert.ToUInt32(_result.data[36 + i]);  // 36 + 3

            }
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);

			}
		}

		protected override Response prepareConsulta()
		{                                               
            var r = procedure("pangya.ProcGetTrofel ", m_uid.ToString()+ ", "+((int)m_season).ToString());
			checkResponse(r, "nao conseguiu pegar o member info do player: " + (m_uid));
			return r;
		}


		public TrofelInfo getInfo()
		{
            return m_ti;
		}



		public uint getUID()
		{
			return m_uid;
		}

		public void setUID(uint _uid)
		{
			m_uid = _uid;
		}

		public TYPE_SEASON getSeason()
		{
			return m_season;
		}

		public void getSeason(TYPE_SEASON _type)
		{
			m_season = _type;
		}

	}
}