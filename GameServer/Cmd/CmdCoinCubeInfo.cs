using System;
using System.Collections.Generic;
using System.Linq;
using PangyaAPI.IFF.JP.Extensions;
using System.Threading.Tasks;
using PangyaAPI.SQL;
using PangyaAPI.Utilities.Log;
using static Pangya_GameServer.Game.System.CubeCoinSystem;
using System.Collections.Concurrent;
using static Pangya_GameServer.GameType.UsedItem;

namespace Pangya_GameServer.Cmd
{
    public class CmdCoinCubeInfo : Pangya_DB
    {

        public CmdCoinCubeInfo()
        {
            _CourseCtx = new CourseCtx();
            this.m_course_info = new Dictionary<byte, bool>();
        } 
         
        public Dictionary<byte, bool> getInfo()
        {
            return new Dictionary<byte, bool>(m_course_info);
        }

        protected override void lineResult(ctx_res _result, uint _index)
        {

            checkColumnNumber(2);

            byte course_id = (byte)IFNULL(_result.data[0]);
            bool active = IFNULL(_result.data[1]) == 1;

            var it = m_course_info.Any(c => c.Key == course_id);
            if (sIff.getInstance().getCourse().Any(c=> c.ID == course_id))
            {
                _CourseCtx.set(course_id, active);
                CourseCtxes.TryAdd(course_id, _CourseCtx);
            }
            if (it)
            {
                m_course_info[course_id] = active;
            }
            else
            {

                m_course_info.Add(course_id, active);

                if (!m_course_info.ContainsKey(course_id))
                {
                    message_pool.push(new message("[CmdCoinCubeInfo::lineResult][WARNING] nao conseguiu adicionar o course[ID=" + Convert.ToString((ushort)course_id) + ", ACTIVE=" + Convert.ToString(active) + "] no map<>.", type_msg.CL_FILE_LOG_AND_CONSOLE));
                }
            }
        }

        protected override Response prepareConsulta()
        {

            if (m_course_info.Count > 0)
            {
                m_course_info.Clear();
            }

            var r = consulta(m_szConsulta);

            checkResponse(r, "nao conseguiu pegar coin cube info dos course");

            return r;
        }

        private Dictionary<byte, bool> m_course_info = new Dictionary<byte, bool>();
        private CourseCtx _CourseCtx;
        private ConcurrentDictionary<uint, CourseCtx> CourseCtxes = new ConcurrentDictionary<uint, CourseCtx>();
        private const string m_szConsulta = "SELECT course_id, active FROM pangya.pangya_coin_cube_info";
    }
}