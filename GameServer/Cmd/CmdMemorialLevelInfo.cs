﻿using GameServer.GameType;
using PangyaAPI.SQL;
using PangyaAPI.Utilities;
using System.Collections.Generic;
using System;
using System.Linq;
using PangyaAPI.IFF.JP.Models.Data;
using GameServer.PangyaEnums;
using PangyaAPI.Utilities.Log;

namespace GameServer.Cmd
{
    public class CmdMemorialLevelInfo : Pangya_DB
    {

        public CmdMemorialLevelInfo()
        {
            this.m_level = new Dictionary<uint, ctx_memorial_level>();
        }


        public Dictionary<uint, ctx_memorial_level> getInfo()
        {
            return m_level;
        }
        protected override void lineResult(ctx_res _result, uint _index_result)
        {

            checkColumnNumber(2, (uint)_result.cols);

            ctx_memorial_level ml = new ctx_memorial_level();

            ml.level = IFNULL(_result.data[0]);    
            var it = m_level.Any(c => c.Key == ml.level);
            if (!it)
            {
                ml.gacha_number = (uint)IFNULL(_result.data[1]);
                m_level.Add(ml.level, ml);
            }   
        }
        protected override Response prepareConsulta()
        {

            var r = consulta(m_szConsulta);

            checkResponse(r, "nao conseguiu pegar Memorial Level Info");

            return r;
        }
        private Dictionary<uint, ctx_memorial_level> m_level = new Dictionary<uint, ctx_memorial_level>();

        private const string m_szConsulta = "SELECT level, gacha_end FROM pangya.pangya_new_memorial_level";
    }
}