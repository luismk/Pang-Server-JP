using GameServer.PangType;
using System;
using PangyaAPI.SQL;
using PangyaAPI.Utilities;

namespace GameServer.Cmd
{
    internal class CmdLastPlayerGameInfo : Pangya_DB
    {
        public CmdLastPlayerGameInfo()
        {
            this.m_uid = 0;
            this.m_l5pg = new Last5PlayersGame();
        }

        public CmdLastPlayerGameInfo(uint _uid)
        {
            this.m_uid = _uid; 
            this.m_l5pg = new Last5PlayersGame();
        }
         public Last5PlayersGame getInfo()
        { return m_l5pg;
        }

        public void setInfo(Last5PlayersGame _l5pg)
        {  m_l5pg = _l5pg; 
        }

        public uint getUID()
        {
            return m_uid;
        }

        public void setUID(uint _uid)
        { 
            m_uid = _uid; 
        }

        protected override void lineResult(ctx_res _result, uint _index_result)
        {

            checkColumnNumber(20);

            try
            {
                for (var i = 0; i < 5; i++)
                {
                    m_l5pg.players[i].sex = IFNULL(_result.data[i * 4]);

                    if (is_valid_c_string(_result.data[i * 4 + 1]))
                    {
                        m_l5pg.players[i].nick = _result.data[i * 4 + 1].ToString();
                    }

                    if (is_valid_c_string(_result.data[i * 4 + 2]))
                    {
                        m_l5pg.players[i].id = _result.data[i * 4 + 2].ToString();
                    }
                    m_l5pg.players[i].uid = IFNULL(_result.data[i * 4 + 3]);
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }       
        }

        protected override Response prepareConsulta()
        {

            m_l5pg = new Last5PlayersGame();

            var r = procedure( m_szConsulta,
                Convert.ToString(m_uid));

            checkResponse(r, "nao conseguiu pegar os ultimos players game info do player: " + Convert.ToString(m_uid));

            return r;
        }
         
        private uint m_uid;
        private Last5PlayersGame m_l5pg = new Last5PlayersGame();

        private const string m_szConsulta = "pangya.ProcGetLastPlayerGame";

        private class ProcGetLastPlayerGame
        {
            public Nullable<int> SEX_0 { get; set; }
            public string NICK_0 { get; set; }
            public string ID_0 { get; set; }
            public Nullable<int> UID_0 { get; set; }
            public Nullable<int> SEX_1 { get; set; }
            public string NICK_1 { get; set; }
            public string ID_1 { get; set; }
            public Nullable<int> UID_1 { get; set; }
            public Nullable<int> SEX_2 { get; set; }
            public string NICK_2 { get; set; }
            public string ID_2 { get; set; }
            public Nullable<int> UID_2 { get; set; }
            public Nullable<int> SEX_3 { get; set; }
            public string NICK_3 { get; set; }
            public string ID_3 { get; set; }
            public Nullable<int> UID_3 { get; set; }
            public Nullable<int> SEX_4 { get; set; }
            public string NICK_4 { get; set; }
            public string ID_4 { get; set; }
            public Nullable<int> UID_4 { get; set; }
        }
    }
}