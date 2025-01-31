using GameServer.PangType;
using PangyaAPI.SQL;
using System;
using System.Collections.Generic;

namespace GameServer.Cmd
{
    internal class CmdCardEquipInfo : Pangya_DB
    {
        private uint m_uid;
        private List<CardEquipInfoEx> v_cei = new List<CardEquipInfoEx>();

        private const string m_szConsulta = "pangya.ProcGetCardEquip";
        public CmdCardEquipInfo(uint uid)
        {
            this.m_uid = uid;
            v_cei = new List<CardEquipInfoEx>();
        }
        protected override void lineResult(ctx_res _result, uint _index_result)
        {
            checkColumnNumber(13);

            CardEquipInfoEx cei = new CardEquipInfoEx();

            cei.index = IFNULL(_result.data[0]);
            cei._typeid = IFNULL(_result.data[1]);
            cei.parts_typeid = IFNULL(_result.data[3]);
            cei.parts_id = IFNULL(_result.data[4]);
            cei.efeito = IFNULL(_result.data[5]);
            cei.efeito_qntd = IFNULL(_result.data[6]);
            cei.slot = IFNULL(_result.data[7]);
            if (_result.data[8] != null)
            {
                cei.use_date.CreateTime(_translateDate(_result.data[8]));
            }
            if (_result.data[9] != null)
            {
                cei.end_date.CreateTime(_translateDate(_result.data[9]));
            }
             cei.tipo = IFNULL(_result.data[11]);
            cei.use_yn = (byte)IFNULL(_result.data[12]);

            v_cei.Add(cei);
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
    }
}