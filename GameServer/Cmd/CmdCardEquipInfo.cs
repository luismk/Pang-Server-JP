using GameServer.Game.Manager;
using GameServer.GameType;
using PangyaAPI.SQL;
using System;
using System.Collections.Generic;

namespace GameServer.Cmd
{
    public class CmdCardEquipInfo : Pangya_DB
    {
        public CmdCardEquipInfo()
        {
            this.m_uid = 0;
            this.v_cei = new CardEquipManager();
        }

        public CmdCardEquipInfo(uint _uid)
        {
            this.m_uid = _uid;
        }                       

        public CardEquipManager getInfo()
        {
            return v_cei;
        }

        public uint getUID()
        {
            return (m_uid);
        }

        public void setUID(uint _uid)
        {
            m_uid = _uid;
        }

        protected override void lineResult(ctx_res _result, uint _index_reuslt)
        {

            checkColumnNumber(13, (uint)_result.cols);

            CardEquipInfoEx cei = new CardEquipInfoEx
            {
                index = IFNULL(_result.data[0]),
                _typeid = IFNULL(_result.data[1]),
                parts_typeid = IFNULL(_result.data[3]),
                parts_id = IFNULL(_result.data[4]),
                efeito = IFNULL(_result.data[5]),
                efeito_qntd = IFNULL(_result.data[6]),
                slot = IFNULL(_result.data[7])
            };
            if (_result.IsNotNull(8))
            {
                cei.use_date.CreateTime(_translateDate(_result.data[8]));
            }
            if (_result.IsNotNull(9))
            {
                cei.end_date.CreateTime(_translateDate(_result.data[9]));
            }
            cei.tipo = IFNULL(_result.data[11]);
            cei.use_yn = (byte)IFNULL(_result.data[12]);

            v_cei.Add(cei.id, cei);
        }

        protected override Response prepareConsulta()
        {

            v_cei.Clear();

            var r = procedure(
                m_szConsulta,
                Convert.ToString(m_uid) + ", 0");

            checkResponse(r, "nao conseguiu pegar o card equip info do player: " + Convert.ToString(m_uid));

            return r;
        }


        private uint m_uid = new uint();
        private CardEquipManager v_cei = new CardEquipManager();

        private const string m_szConsulta = "pangya.ProcGetCardEquip";
    }
    
}