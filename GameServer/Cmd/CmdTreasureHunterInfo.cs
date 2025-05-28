using System.Collections.Generic;
using Pangya_GameServer.GameType;
using PangyaAPI.SQL;

namespace Pangya_GameServer.Cmd
{
    public class CmdTreasureHunterInfo : Pangya_DB
    {
        public CmdTreasureHunterInfo()
        {
            this.v_thi = new List<TreasureHunterInfo>();
        }

        protected override void lineResult(ctx_res _result, uint _index)
        {

            checkColumnNumber(2, (uint)_result.cols);

            TreasureHunterInfo thi = new TreasureHunterInfo(); // treasure hunter info

            thi.course = (byte)IFNULL(_result.data[0]);
            thi.point = IFNULL(_result.data[1]);

            v_thi.Add(thi);
        }
        protected override Response prepareConsulta()
        {

            v_thi.Clear();
            var r = procedure(
                m_szConsulta, "");

            checkResponse(r, "nao conseguiu pegar Treasure Hunter do server");

            return r;
        }
        public List<TreasureHunterInfo> getInfo()
        {
            return v_thi;
        }

        List<TreasureHunterInfo> v_thi;          // Treasure Hunter Info = THI

        string m_szConsulta = "pangya.ProcGetCourseReward";
    }
}