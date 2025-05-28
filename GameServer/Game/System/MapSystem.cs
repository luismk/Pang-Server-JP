using System;
using System.Collections.Generic;
using Pangya_GameServer.GameType;
using PangyaAPI.IFF.JP.Extensions;
using PangyaAPI.Utilities;
using PangyaAPI.Utilities.Log;

namespace Pangya_GameServer.Game.System
{
    public class Map
    {
        Dictionary<byte, stCtx> m_map = new Dictionary<byte, stCtx>();
        bool m_load = false;

        public Map()
        {
            initialize();
        }

        ~Map()
        {
            clear();
        }

        public bool isLoad()
        {
            return m_load && m_map.Count > 0;
        }

        public void load()
        {
            if (isLoad())
                clear();

            initialize();
        }

        public stCtx getMap(byte course)
        {
            if (m_map.TryGetValue((byte)(course & 0x7F), out var ctx))
                return ctx;

            return null;
        }

        public uint calculeClearVS(stCtx ctx, uint num_player, uint qntd_hole)
        {
            return ctx.clear_bonus * qntd_hole * (num_player - 1);
        }

        public uint calculeClearMatch(stCtx ctx, uint qntd_hole)
        {
            return ctx.clear_bonus * qntd_hole;
        }

        public uint calculeClear30s(stCtx ctx, uint qntd_hole)
        {
            if (ctx.clear_bonus == 0 || qntd_hole == 0)
                return 0;

            return (ctx.clear_bonus * qntd_hole) / 2;
        }

        public uint calculeClearSSC(stCtx ctx)
        {
            return ctx.clear_bonus;
        }

        private void initialize()
        {
            try
            {
                if (!sIff.getInstance().isLoad())
                    sIff.getInstance().load();

                var courses = sIff.getInstance().getCourse();

                stCtx ctx;
                foreach (var el in courses)
                {
                    ctx = new stCtx
                    {
                        name = el.Name
                    };
                    ctx.range_score.par = el.Par_Hole;
                    ctx.star = 1f + (el.Star / 10f);
                    // Bonus por curso (substituir com enums se necessário)
                    var tipo = (RoomInfo.eCOURSE)(el.ID & 0xFF);
                    var course = (byte)(el.ID & 0xFF);
                    switch (tipo)
                    {
                        case RoomInfo.eCOURSE.BLUE_LAGOON: ctx.clear_bonus = 20u; break;
                        case RoomInfo.eCOURSE.BLUE_WATER: ctx.clear_bonus = 50u; break;
                        case RoomInfo.eCOURSE.BLUE_MOON: ctx.clear_bonus = 50u; break;
                        case RoomInfo.eCOURSE.SEPIA_WIND: ctx.clear_bonus = 55u; break;
                        case RoomInfo.eCOURSE.PINK_WIND: ctx.clear_bonus = 20u; break;
                        case RoomInfo.eCOURSE.WIND_HILL: ctx.clear_bonus = 80u; break;
                        case RoomInfo.eCOURSE.WIZ_WIZ: ctx.clear_bonus = 65u; break;
                        case RoomInfo.eCOURSE.WHITE_WIZ: ctx.clear_bonus = 55u; break;
                        case RoomInfo.eCOURSE.WEST_WIZ: ctx.clear_bonus = 24u; break;
                        case RoomInfo.eCOURSE.WIZ_CITY: ctx.clear_bonus = 40u; break;
                        case RoomInfo.eCOURSE.DEEP_INFERNO: ctx.clear_bonus = 80u; break;
                        case RoomInfo.eCOURSE.ICE_SPA: ctx.clear_bonus = 20u; break;
                        case RoomInfo.eCOURSE.ICE_CANNON: ctx.clear_bonus = 40u; break;
                        case RoomInfo.eCOURSE.ICE_INFERNO: ctx.clear_bonus = 70u; break;
                        case RoomInfo.eCOURSE.SILVIA_CANNON: ctx.clear_bonus = 70u; break;
                        case RoomInfo.eCOURSE.SHINNING_SAND: ctx.clear_bonus = 40u; break;
                        case RoomInfo.eCOURSE.EASTERN_VALLEY: ctx.clear_bonus = 40u; break;
                        case RoomInfo.eCOURSE.LOST_SEAWAY: ctx.clear_bonus = 20u; break;
                        case RoomInfo.eCOURSE.GRAND_ZODIAC: ctx.clear_bonus = 0u; break;
                        case RoomInfo.eCOURSE.CHRONICLE_1_CHAOS: ctx.clear_bonus = 360u; break;
                        case RoomInfo.eCOURSE.ABBOT_MINE: ctx.clear_bonus = 40u; break;
                        case RoomInfo.eCOURSE.MYSTIC_RUINS: ctx.clear_bonus = 40u; break;
                    } 
                    m_map[course] = (ctx);
                }

                message_pool.push(new message("[Map::initialize][Log] Map Dados Estaticos carregado com sucesso!", type_msg.CL_FILE_LOG_AND_CONSOLE));

                m_load = true;
            }
            catch (Exception e)
            {
                message_pool.push(new message("[Map::initialize][ErrorSystem] " + e.Message, type_msg.CL_FILE_LOG_AND_CONSOLE));
                throw;
            }
        }

        private void clear()
        {
            m_map.Clear();
            m_load = false;
        }

        public class stCtx
        {
            public stCtx(uint ul = 0u)
            {
                clear();
            }

            public void clear()
            {
                name = "";
                range_score = new stParRangeScore();
                clear_bonus = 0u;
                star = 0f;
            }

            public class stParRangeScore
            {
                public sbyte[] par = new sbyte[18];
                public sbyte[] min = new sbyte[18];
                public sbyte[] max = new sbyte[18];

                public void clear()
                {
                    Array.Clear(par, 0, par.Length);
                    Array.Clear(min, 0, min.Length);
                    Array.Clear(max, 0, max.Length);
                }
            }

            public string name;
            public uint clear_bonus;
            public float star;
            public stParRangeScore range_score = new stParRangeScore();
        }

    }

    public class sMap : Singleton<Map>
    {
    }
}
