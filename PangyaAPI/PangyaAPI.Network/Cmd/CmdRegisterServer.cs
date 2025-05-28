using PangyaAPI.Network.Pangya_St;
using PangyaAPI.SQL;
namespace PangyaAPI.Network.Cmd
{
    public class CmdRegisterServer : Pangya_DB
    {
        ServerInfoEx m_si;
        protected override string _getName { get; } = "CmdRegisterServer";

        public CmdRegisterServer(ServerInfoEx _si)
        {
            m_si = _si;
        }

        protected override void lineResult(ctx_res _result, uint _index_result)
        {

        }

        protected override Response prepareConsulta()
        {
            var r = procedure("pangya.ProcRegServer_New", (m_si.uid).ToString() + ", " + (m_si.nome).ToString() + ", " + (m_si.ip).ToString()
               + ", " + (m_si.port).ToString() + ", " + (m_si.tipo).ToString() + ", " + (m_si.max_user).ToString()
               + ", " + (m_si.curr_user).ToString() + ", " + (m_si.rate.pang).ToString() + ", " + (m_si.version).ToString()
               + ", " + (m_si.version_client).ToString() + ", " + (m_si.propriedade.ulProperty).ToString() + ", " + (m_si.angelic_wings_num).ToString()
               + ", " + (m_si.event_flag.usEventFlag).ToString() + ", " + (m_si.rate.exp).ToString() + ", " + (m_si.img_no).ToString()
               + ", " + (m_si.rate.scratchy).ToString() + ", " + (m_si.rate.club_mastery).ToString() + ", " + (m_si.rate.treasure).ToString()
               + ", " + (m_si.rate.papel_shop_rare_item).ToString() + ", " + (m_si.rate.papel_shop_cookie_item).ToString() + ", " + (m_si.rate.chuva).ToString());

            checkResponse(r, "nao conseguiu registrar o server[GUID=" + (m_si.uid) + ", PORT=" + (m_si.port) + ", NOME=" + (m_si.nome) + "] no banco de dados");
            return r;
        }

        public ServerInfoEx getServerList()
        {
            return this.m_si;
        }


        public void setInfo(ServerInfoEx _si)
        {
            m_si = _si;
        }
    }
}
