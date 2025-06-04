//Convertion By LuisMK
using Pangya_GameServer.GameType;
using PangyaAPI.SQL;
using PangyaAPI.Utilities;
using System;

namespace Pangya_GameServer.Cmd
{
    public class CmdTicketReportDadosInfo : Pangya_DB
    {
        public CmdTicketReportDadosInfo(bool _waiter = false) : base(_waiter)
        {
            this.m_ticket_report_id = -1;
            this.m_trsi = new TicketReportScrollInfo();
        }

        public CmdTicketReportDadosInfo(int _ticket_report_id, bool _waiter = false) : base(_waiter)
        {
            this.m_ticket_report_id = _ticket_report_id;
            this.m_trsi = new TicketReportScrollInfo();
        }
                         

        public int getTicketReportId()
        {
            return (m_ticket_report_id);
        }

        public void setTicketReportId(int _ticket_report_id)
        {
            
             m_ticket_report_id = _ticket_report_id;
            m_ticket_report_id = _ticket_report_id;
        }

        public TicketReportScrollInfo getInfo()
        {
             return m_trsi;
         }

        public void setInfo(TicketReportScrollInfo _trsi)
        {
            m_trsi = _trsi;
        }

        protected override void lineResult(ctx_res _result, uint _index_result)
        {

            checkColumnNumber(22);

            if (m_trsi.id < 0)
            {
                m_trsi.id = IFNULL<int>(_result.data[0]);

                if (_result.IsNotNull(3))
                {
                    m_trsi.date.CreateTime(_translateDate(_result.data[3]));
                }
            }

            TicketReportScrollInfo.stPlayerDados pd = new TicketReportScrollInfo.stPlayerDados();

            
             pd.tipo = (uint)IFNULL(_result.data[1]);
             
             pd.trofel_typeid = (uint)IFNULL(_result.data[2]);
            
             pd.uid = (uint)IFNULL(_result.data[4]);
            pd.score = (sbyte)IFNULL(_result.data[5]);
            pd.medalha.ucMedal = (byte)IFNULL(_result.data[6]);
            pd.trofel = (byte)IFNULL(_result.data[7]);
            
             pd.pang = (ulong)IFNULL( _result.data[8]);
              pd.bonus_pang = (ulong)IFNULL( _result.data[9]);
            
             pd.exp = (uint)IFNULL(_result.data[10]);
              pd.mascot_typeid = (uint)IFNULL(_result.data[11]);
             pd.state = (byte)IFNULL(_result.data[12]);
            pd.item_boost = (byte)IFNULL(_result.data[13]);
            pd.premium_user = (byte)IFNULL(_result.data[14]);
            
             pd.level = (uint)IFNULL(_result.data[15]);
             if (is_valid_c_string(_result.data[16]))
            {
                pd.id = _result.GetString(16);  
            }
            if (is_valid_c_string(_result.data[17]))
            {
                pd.nickname = _result.GetString(17);
            }

            pd.guild_uid = (uint)IFNULL(_result.data[18]);
             if (is_valid_c_string(_result.data[19]))
            {                                          
                pd.guild_mark_img = _result.GetString(19);    
            }


            pd.mark_index = (uint)IFNULL(_result.data[20]);
             if (_result.IsNotNull(21))
            {
                pd.finish_time.CreateTime(_translateDate(_result.data[21]));
            }

            // Add ao vector do Ticket Report Scroll Info
            m_trsi.v_players.Add(pd);

            if (m_trsi.id < 0)
            {
                throw new exception("[CmdTicketReportDadosInfo::lineResult][Error] m_trsi[request_id=" + Convert.ToString(m_ticket_report_id) + ", return_id=" + Convert.ToString(m_trsi.id) + "] is wrong not match.", STDA_MAKE_ERROR(STDA_ERROR_TYPE.PANGYA_DB,
                    3, 0));
            }

        }

        protected override Response prepareConsulta()
        {

            if (m_ticket_report_id < 0)
            {
                throw new exception("[CmdTicketReportDadosInfo::prepareConsulta][Error] m_ticket_report_id[VALUE=" + Convert.ToString(m_ticket_report_id) + "] is invalid", STDA_MAKE_ERROR(STDA_ERROR_TYPE.PANGYA_DB,
                    4, 0));
            }

            m_trsi.id = -1;

            var r = procedure(
                m_szConsulta,
                Convert.ToString(m_ticket_report_id));

            checkResponse(r, "nao conseguiu pegar os dados do Ticket Report[ID=" + Convert.ToString(m_ticket_report_id) + "]");

            return r;
        }
                   
        private int m_ticket_report_id = new int();
        private TicketReportScrollInfo m_trsi = new TicketReportScrollInfo();

        private const string m_szConsulta = "pangya.ProcGetTicketReportDados";
    }
}