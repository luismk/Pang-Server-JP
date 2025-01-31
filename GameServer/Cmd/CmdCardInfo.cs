using GameServer.PangType;
using System;
using System.Collections.Generic;
using PangyaAPI.SQL;
using GameServer.Game.Manager;

namespace GameServer.Cmd
{
    public class CmdCardInfo : Pangya_DB
    {
        public enum TYPE : byte
        {
            ALL,
            ONE
        }

        public CmdCardInfo()
        {
            m_uid = 0;
            m_type = TYPE.ALL;
            m_card_id = 0;
            v_ci = new CardManager();
        }

        public CmdCardInfo(uint uid, TYPE type, uint cardId = 0)
        {
            m_uid = uid;
            m_type = type;
            m_card_id = cardId;
            v_ci = new CardManager();
        }

        public CardManager getInfo()
        {
            return v_ci;
        }

        public uint getUID()
        {
            return m_uid;
        }

        public void setUID(uint uid)
        {
            m_uid = uid;
        }

        public TYPE getType()
        {
            return m_type;
        }

        public void setType(TYPE type)
        {
            m_type = type;
        }

        public uint getCardID()
        {
            return m_card_id;
        }

        public void setCardID(uint cardId)
        {
            m_card_id = cardId;
        }

        protected override void lineResult(ctx_res result, uint indexResult)
        {
            checkColumnNumber(11);

            var ci = new CardInfo
            {
                id = IFNULL(result.data[0]),
                _typeid = IFNULL(result.data[2]),
                slot = IFNULL(result.data[3]),
                efeito = IFNULL(result.data[4]),
                efeito_qntd = IFNULL(result.data[5]),
                qntd = IFNULL(result.data[6]),
                type = (byte)IFNULL(result.data[9]),
                use_yn = (byte)IFNULL(result.data[10])
            };

            if (result.IsEmptyObject(7))
                ci.use_date.CreateTime(_translateDate(result.data[7]));

            if (result.IsEmptyObject(8))
                ci.end_date.CreateTime(_translateDate(result.data[8]));

            v_ci.Add(ci.id, ci);
        }
         
        protected override Response prepareConsulta()
        {
            v_ci.Clear();

            var query = m_type == TYPE.ALL ? m_szConsulta[0] : m_szConsulta[1];
            var parameters = m_type == TYPE.ONE ? $", {m_card_id}" : string.Empty;

            var response = procedure(query, $"{m_uid}{parameters}");

            checkResponse(response, $"Não conseguiu pegar card info do player: {m_uid}");

            return response;
        }

        private uint m_uid;
        private TYPE m_type;
        private uint m_card_id;
        private CardManager v_ci;

        private readonly string[] m_szConsulta = { "pangya.ProcGetCardInfo", "pangya.ProcGetCardInfo_One" };
    }
}
