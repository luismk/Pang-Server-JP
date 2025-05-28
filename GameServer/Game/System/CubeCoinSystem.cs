// CubeCoinSystem.cs - versão otimizada e compatível com C# 6
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Pangya_GameServer.Cmd;
using Pangya_GameServer.Game.Utils;
using Pangya_GameServer.GameType;
using PangyaAPI.IFF.JP.Extensions;
using PangyaAPI.SQL.Manager;
using PangyaAPI.Utilities;
using PangyaAPI.Utilities.Log;
using static Pangya_GameServer.Game.System.CubeCoinSystem;

namespace Pangya_GameServer.Game.System
{
    public class CubeCoinSystem
    {
        private ConcurrentDictionary<uint, CourseCtx> m_course = new ConcurrentDictionary<uint, CourseCtx>();
        protected CourseCtx CoursesCtx;
        private volatile bool m_load;

        public CubeCoinSystem()
        {
            m_load = false; 
        }

        public void load()
        {
            if (isLoad()) Clear();
            Initialize();
        }

        public bool isLoad() { return m_load && !m_course.IsEmpty; }

        public CourseCtx FindCourse(uint course_typeid)
        {
            if (course_typeid == 0)
                throw new exception("[CubeCoinSystem::FindCourse][Error] course_typeid is invalid (zero)", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.CUBE_COIN_SYSTEM, 1, 0));

            CourseCtx course;
            if (m_course.TryGetValue(course_typeid, out course)) return course;
            return null;
        }

        public static CoinCubeInHole getAllCoinCubeInHoleWizCity(byte number_hole)
        {
            var cbih = new CoinCubeInHole();
            switch (number_hole)
            {
                case 3:
                case 12:
                    cbih.m_all_cube = 5;
                    cbih.m_all_coin_and_cube = 60;
                    break;
                case 14:
                    cbih.m_all_cube = 2;
                    cbih.m_all_coin_and_cube = 48;
                    break;
                case 18:
                    cbih.m_all_cube = 3;
                    cbih.m_all_coin_and_cube = 33;
                    break;
                default:
                    cbih.m_all_cube = 0;
                    cbih.m_all_coin_and_cube = 20;
                    break;
            }
            return cbih;
        }

        public static CoinCubeInHole getAllCoinCubeInHole(byte course_id, byte number_hole)
        {
            var cbih = new CoinCubeInHole();

            if (number_hole == 0 || number_hole > 18)
            {
                message_pool.push(new message("[CubeCoinSystem::CoinCubeInHole][WARNING] invalid number hole(" + number_hole + ")", type_msg.CL_FILE_LOG_AND_CONSOLE));
                return cbih;
            }

            var course = sMap.getInstance().getMap(course_id);
            if (course == null)
            {
                message_pool.push(new message("[CubeCoinSystem::CoinCubeInHole][WARNING] Course=" + course_id + " not found in sMap.", type_msg.CL_FILE_LOG_AND_CONSOLE));
                return cbih;
            }

            switch (course.range_score.par[number_hole - 1])
            {
                case 3:
                    cbih.m_all_cube = 1;
                    cbih.m_all_coin_and_cube = 1;
                    break;
                case 4:
                    cbih.m_all_cube = 1;
                    cbih.m_all_coin_and_cube = 5;
                    break;
                case 5:
                    cbih.m_all_cube = 2;
                    cbih.m_all_coin_and_cube = 8;
                    break;
            }
            return cbih;
        }

        private void Initialize()
        {
            CoursesCtx = new CourseCtx();
            CmdCoinCubeInfo cmd_cci = new CmdCoinCubeInfo(); // Waiter

            NormalManagerDB.add(0, cmd_cci, null, null); 

            if (cmd_cci.getException().getCodeError() != 0u)
            {
                throw cmd_cci.getException();
            }

            var courses = sIff.getInstance().getCourse();
            foreach (var el in courses)
            {

                var it_course_info = cmd_cci.getInfo().find((byte)sIff.getInstance().getItemIdentify(el.ID));
                var check_key = m_course.ContainsKey(it_course_info.Key);
                CoursesCtx.set(el.ID, check_key ? it_course_info.Value : false);
                m_course.TryAdd(el.ID, CoursesCtx);
            }

            message_pool.push(new message("[CubeCoinSystem::Initialize][Log] Cube Coin System loaded successfully!", type_msg.CL_FILE_LOG_AND_CONSOLE));

            m_load = true;
        }

        public void Clear()
        {
            m_course.Clear();
            m_load = false;
        }
         
        public class CourseCtx
        {
            public class Hole
            {
                public Hole(byte numero, uint number_of_cube, uint max_coin_and_cube)
                {
                    this.numero = numero;
                    this.number_of_cube = number_of_cube;
                    this.max_coin_and_cube = max_coin_and_cube;
                    this.v_cube = new List<CubeEx>();
                }

                public List<CubeEx> getAllCoinCubeWizCity(bool includeCubes)
                {
                    if (v_cube.Count == 0) return v_cube;

                    var cpy = new List<CubeEx>(v_cube);
                    Tools.Shuffle(cpy);

                    var ret = new List<CubeEx>();
                    var lottery = new Lottery();

                    foreach (var it in cpy)
                    {
                        if (it.tipo == Cube.eTYPE.COIN && it.flag_location == Cube.eFLAG_LOCATION.CARPET)
                        {
                            lottery.Push(it.rate, it);
                        }
                    }

                    foreach (var it in cpy)
                    {
                        if (it.flag_location == Cube.eFLAG_LOCATION.EDGE_GREEN)
                        {
                            ret.Add(it);
                        }
                    }

                    if (lottery.GetCountItem() > 0u)
                    {
                        var count_cube = Math.Min(number_of_cube, lottery.GetCountItem());

                        for (uint i = 0; i < count_cube; i++)
                        {
                            var ctx = lottery.SpinRoleta(true);
                            if (ctx != null && ctx.Value != null)
                            {
                                var dice = (CubeEx)ctx.Value;
                                ret.Add(new CubeEx(dice.id, Cube.eTYPE.CUBE, 0, dice.flag_location, dice.location.x, dice.location.y, dice.location.z, dice.rate));
                            }
                        }

                        var rest_count = Math.Min(max_coin_and_cube - (uint)ret.Count, lottery.GetCountItem());
                        for (uint i = 0; i < rest_count; i++)
                        {
                            var ctx = lottery.SpinRoleta(true);
                            if (ctx != null && ctx.Value != null) ret.Add((CubeEx)ctx.Value);
                        }
                    }

                    return ret;
                }

                public List<CubeEx> getAllCoinCube(bool includeCubes)
                {
                    if (v_cube.Count == 0) return v_cube;

                    var cpy = new List<CubeEx>(v_cube);
                    Tools.Shuffle(cpy);

                    var ret = new List<CubeEx>();
                    var lottery = new Lottery();

                    if (includeCubes)
                    {
                        foreach (var it in cpy)
                        {
                            if (it.tipo == Cube.eTYPE.CUBE && it.flag_location == Cube.eFLAG_LOCATION.AIR)
                            {
                                lottery.Push(it.rate, it);
                            }
                        }

                        var count_cube = Math.Min(number_of_cube, lottery.GetCountItem());
                        for (uint i = 0; i < count_cube; i++)
                        {
                            var ctx = lottery.SpinRoleta(true);
                            if (ctx != null && ctx.Value != null) ret.Add((CubeEx)ctx.Value);
                        }
                    }

                    lottery.Clear();

                    foreach (var it in cpy)
                    {
                        if (it.tipo == Cube.eTYPE.COIN && it.flag_location == Cube.eFLAG_LOCATION.GROUND)
                        {
                            lottery.Push(it.rate, it);
                        }
                    }

                    var rest_count = Math.Min(max_coin_and_cube - (uint)ret.Count, lottery.GetCountItem());
                    for (uint i = 0; i < rest_count; i++)
                    {
                        var ctx = lottery.SpinRoleta(true);
                        if (ctx != null && ctx.Value != null) ret.Add((CubeEx)ctx.Value);
                    }

                    return ret;
                }

                public byte numero;
                public uint number_of_cube;
                public uint max_coin_and_cube;
                public List<CubeEx> v_cube;
            }

            private uint m_typeid;
            private ConcurrentDictionary<byte, Hole> m_hole = new ConcurrentDictionary<byte, Hole>();
            private volatile bool m_active;
            private Dictionary<byte, List<CubeEx>> m_coin_cube;
            public CourseCtx()
            { 
            }
            public CourseCtx(uint typeid, bool active)
            {
                m_typeid = typeid;
                m_active = active; 
            }

            public void set(uint typeid, bool active)
            {
                m_typeid = typeid;
                m_active = active;

                Initialize();//problema aqui

                var course_id = sIff.getInstance().getItemIdentify(m_typeid);



                Parallel.For(1, 19, i =>
                {
                    CoinCubeInHole cbih;
                    if (course_id == (byte)RoomInfo.eCOURSE.WIZ_CITY)
                        cbih = getAllCoinCubeInHoleWizCity((byte)i);
                    else
                        cbih = getAllCoinCubeInHole((byte)course_id, (byte)i);

                    var hole = new Hole((byte)i, cbih.m_all_cube, cbih.m_all_coin_and_cube);
                    hole.v_cube.AddRange(getAllCoinCubeHole((byte)i));
                    m_hole.TryAdd((byte)i, hole);
                });
            }

            protected List<CubeEx> getAllCoinCubeHole(byte _hole_number)
            {

                List<CubeEx> v_coin_cube = new List<CubeEx>();

                var it = m_coin_cube.FirstOrDefault(c => c.Key == _hole_number);

                if (it.Key != 0 && it.Value != null)
                {
                    v_coin_cube = it.Value;
                }
                return new List<CubeEx>(v_coin_cube);
            }

            public bool IsActive() { return m_active; }

            public Hole FindHole(byte hole)
            {
                Hole h;
                if (m_hole.TryGetValue(hole, out h)) return h;
                return null;
            }

            private void Initialize()
            {
                var cmd_ccli = new CmdCoinCubeLocationInfo((byte)sIff.getInstance().getItemIdentify(m_typeid));

                NormalManagerDB.add(0, cmd_ccli, null, null);

                if (cmd_ccli.getException().getCodeError() != 0u)
                    throw cmd_ccli.getException();

                m_coin_cube = cmd_ccli.getInfo(); // carrega os 
            }
        }

        public class CoinCubeInHole
        {
            public uint m_all_cube;
            public uint m_all_coin_and_cube;
        }

    }

    public class sCubeCoinSystem : Singleton<CubeCoinSystem> { }
}
