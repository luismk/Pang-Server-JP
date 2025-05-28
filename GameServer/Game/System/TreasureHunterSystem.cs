using System;
using System.Collections.Generic;
using System.Threading;
using Pangya_GameServer.Cmd;
using Pangya_GameServer.Game.Utils;
using Pangya_GameServer.GameType;
using PangyaAPI.IFF.JP.Extensions;
using PangyaAPI.SQL;
using PangyaAPI.SQL.Manager;
using PangyaAPI.Utilities;
using PangyaAPI.Utilities.Log;
using static Pangya_GameServer.GameType._Define;
namespace Pangya_GameServer.Game.System
{
    public class TreasureHunterSystem
    {
        public TreasureHunterSystem()
        {
            this.m_thItem = new List<TreasureHunterItem>();
            this.m_load = false;
            this.m_time = DateTime.Now;
            // Inicializa
            initialize();
        }

        /*static */
        public void load()
        {

            if (isLoad())
            {
                clear();
            }

            initialize();
        }

        /*static */
        public bool isLoad()
        {

            bool isLoad = false;

            Monitor.Enter(m_cs);


            isLoad = (m_load && m_thItem.Count > 0);

            Monitor.Exit(m_cs);


            return isLoad;
        }

        /*static */
        public TreasureHunterInfo[] getAllCoursePoint()
        {

            lock (m_cs)
            {
                return m_thi; // cópia superficial
            }
        }

        /*static */
        public TreasureHunterInfo findCourse(byte _course)
        {

            // Prote��o contra o Random Map, que usa o negatico do 'char'
            if ((_course & 0x7F) >= MS_NUM_MAPS)
            {
                throw new exception("[TreasureHunterSystem::findCourse][Error] _course is invalid[VALUE=" + Convert.ToString((ushort)_course & 0x7F) + "]", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.TREASURE_HUNTER_SYSTEM,
                    2, 0));
            }

            TreasureHunterInfo thi = null;

            Monitor.Enter(m_cs);


            // Prote��o contra o Random Map, que usa o negatico do 'char'
            thi = m_thi[_course & 0x7F];

            Monitor.Exit(m_cs);


            return thi;
        }

        /*static */
        public uint calcPointNormal(uint _tacada, sbyte _par_hole)
        {

            if (_tacada == 1) // Hole In One(HIO)
            {
                return 100;
            }

            uint point = 0u;

            switch ((sbyte)(_tacada - _par_hole))
            {
                case (sbyte)-3: // Albatross
                    point = 100;
                    break;
                case (sbyte)-2: // Eagle
                    point = 50;
                    break;
                case (sbyte)-1: // Birdie
                    point = 30;
                    break;
                case 0: // Par
                    point = 15;
                    break;
                case 1: // Bogey
                    point = 10;
                    break;
                case 2: // Double Bogey
                    point = 7; break;
                case 3: // Triple Bogey
                    point = 4; break;
                case 4: // +4
                    point = 1; break;
                    break;
            }

            return point;
        }

        /*static */
        public uint calcPointSSC(uint _tacada, sbyte _par_hole)
        {

            if (_tacada == 1) // Hole In One(HIO)
            {
                return 30;
            }

            uint point = 0u;

            switch ((sbyte)(_tacada - _par_hole))
            {
                case -3: // Albatross
                    point = 1;
                    break;
                case -2: // Eagle
                    point = 4;
                    break;
                case -1: // Birdie
                    point = 7;
                    break;
                case 0: // Par
                    point = 10;
                    break;
                case 1: // Bogey
                    point = 15;
                    break;
                case 2: // Double Bogey
                    point = 30;
                    break;
                case 3: // Triple Bogey
                    point = 50;
                    break;
                case 4: // +4
                    point = 100;
                    break;
            }

            return point;
        }

        /*static */
        public List<TreasureHunterItem> drawItem(uint _point, byte _course)
        {

            List<TreasureHunterItem> v_item = new List<TreasureHunterItem>();



            float rate_course = getCourseRate(_course);

            float rate = _point * sgs.gs.getInstance().getInfo().rate.treasure * rate_course / 100.0f;

            uint box = (uint)(rate / TREASURE_HUNTER_BOX_PER_POINT);

            // Pelo menos uma box sortea
            if (box == 0)
            {
                box = 1u;
            }

            Lottery lottery = new Lottery();

            foreach (var el in m_thItem)
            {
                lottery.Push((el.probabilidade), el);
            }

            TreasureHunterItem thi = new TreasureHunterItem();
            TreasureHunterItem pThi = null;

            Lottery.LotteryCtx ctx = null;

            for (var i = 0u; i < box;)
            {

                // Sorteia item
                if ((ctx = lottery.SpinRoleta()) == null)
                {
                    message_pool.push(new message("[TreasureHunterSystem::drawItem][WARNING] nao conseguiu sortear o item. Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));
                    continue;
                }

                pThi = (TreasureHunterItem)ctx.Value;

                // Verifica se o item existe no IFF_STRUCT do server, para n�o da erro mais tarde
                if (sIff.getInstance().findCommomItem(pThi._typeid) == null)
                {
                    message_pool.push(new message("[TreasureHunterSystem::drawItem][WARNING] nao conseguiu encontrar o Item[TYPEID=" + Convert.ToString(pThi._typeid) + "] no IFF_STRUCT do server. Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));
                    continue;
                }

                // Sorteia quantidade
                if (pThi.qntd > 1 && pThi._typeid != PANG_POUCH_TYPEID)
                {
                    pThi.qntd = (uint)(1 + (sRandomGen.getInstance().rIbeMt19937_64_chrono() % pThi.qntd));
                }

#if DEBUG
                message_pool.push(new message("[TreasureHunterSystem::Sorteia Item][Log] sorteou Item[TYPEID=" + Convert.ToString(pThi._typeid) + ", QNTD=" + Convert.ToString(pThi.qntd) + ", PROBABILIDADE=" + Convert.ToString(pThi.probabilidade) + ", FLAG=" + Convert.ToString((ushort)pThi.flag) + "]", type_msg.CL_FILE_LOG_AND_CONSOLE));
#endif // _DEBUG

                v_item.Add(pThi);

                // Incrementa o index
                i++;
            }

            return v_item;
        }

        /*static */
        public List<TreasureHunterItem> drawApproachBox(uint _num_box, byte _course)
        {

            List<TreasureHunterItem> v_item = new List<TreasureHunterItem>();
            TreasureHunterItem thi = new TreasureHunterItem();
            TreasureHunterItem pThi = null;

            if (_num_box == 0)
            {
                return v_item;
            }



            float rate_course = getCourseRate(_course);

            uint box = (uint)(_num_box * sgs.gs.getInstance().getInfo().rate.treasure * rate_course / 100.0f);

            // _num box � maior que zero, box n�o pode ser 0, tem que ser pelo menos 1
            if (box == 0u)
            {
                box = 1u;
            }

            Lottery lottery = new Lottery();

            foreach (var el in m_thItem)
            {
                lottery.Push((el.probabilidade), el);
            }

            Lottery.LotteryCtx ctx = null;

            for (var i = 0u; i < box;)
            {

                // Sorteia item
                if ((ctx = lottery.SpinRoleta()) == null)
                {
                    message_pool.push(new message("[TreasureHunterSystem::drawApproachBox][WARNING] nao conseguiu sortear o item. Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));
                    continue;
                }

                pThi = (TreasureHunterItem)ctx.Value;

                // Verifica se o item existe no IFF_STRUCT do server, para n�o da erro mais tarde
                if (sIff.getInstance().findCommomItem(pThi._typeid) == null)
                {
                    message_pool.push(new message("[TreasureHunterSystem::drawApprochBox][WARNING] nao conseguiu encontrar o Item[TYPEID=" + Convert.ToString(pThi._typeid) + "] no IFF_STRUCT do server. Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));
                    continue;
                }

                // Sorteia quantidade
                if (pThi.qntd > 1 && pThi._typeid != PANG_POUCH_TYPEID)
                {
                    pThi.qntd = (uint)(1 + (sRandomGen.getInstance().rIbeMt19937_64_chrono() % pThi.qntd));
                }

#if DEBUG
                message_pool.push(new message("[TreasureHunterSystem::Sorteia Approach Box][Log] sorteou Item[TYPEID=" + Convert.ToString(pThi._typeid) + ", QNTD=" + Convert.ToString(pThi.qntd) + ", PROBABILIDADE=" + Convert.ToString(pThi.probabilidade) + ", FLAG=" + Convert.ToString((ushort)pThi.flag) + "]", type_msg.CL_FILE_LOG_AND_CONSOLE));
#endif // _DEBUG

                v_item.Add(pThi);

                // Incrementa o index
                i++;
            }
            return v_item;
        }

        // Check time update Point Course
        /*static */
        public bool checkUpdateTimePointCourse()
        {
            if ((DateTime.Now - m_time).TotalSeconds >= TREASURE_HUNTER_TIME_UPDATE)
            {

                for (var i = 0; i < MS_NUM_MAPS; i++)
                {
                    if (m_thi[i].point < TREASURE_HUNTER_LIMIT_POINT_COURSE)
                    {
                        updateCoursePoint(m_thi[i], TREASURE_HUNTER_INCREASE_POINT);
                    }
                }

                message_pool.push(new message("[TreasureHunterSystem][Log] Atualizou Pontos dos course.", type_msg.CL_FILE_LOG_AND_CONSOLE));

                m_time = DateTime.Now;

                return true;
            }
            return false;
        }

        /*static */
        public void updateCoursePoint(TreasureHunterInfo _thi, int _point)
        {



            if (_point < 0) // Decrease
            {
                _thi.point = (uint)((_thi.point + _point < 0) != null ? 0u : _thi.point + _point);
            }
            else // Increase
            {
                _thi.point = (uint)((_thi.point + _point > TREASURE_HUNTER_LIMIT_POINT_COURSE) ? TREASURE_HUNTER_LIMIT_POINT_COURSE : _thi.point + _point);
            }

            NormalManagerDB.add(1,
                new CmdUpdateTreasureHunterCoursePoint(_thi),
                TreasureHunterSystem.SQLDBResponse,
                null);

        }

        /*static */
        protected void initialize()
        {



            CmdTreasureHunterInfo cmd_thi = new Cmd.CmdTreasureHunterInfo(); // Waiter

            NormalManagerDB.add(0,
                cmd_thi, null, null);

            if (cmd_thi.getException().getCodeError() != 0)
            {
                throw cmd_thi.getException();
            }

            if (cmd_thi.getInfo().Count == 0)
            {
                throw new exception("[TreasureHunterSystem::initialize][Error] nao conseguiu pegar os treasure hunter point dos course na data base.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.TREASURE_HUNTER_SYSTEM,
                    1, 0));
            }

            var v_thi = cmd_thi.getInfo();

            foreach (TreasureHunterInfo i in v_thi)
            {
                m_thi[i.course] = i;
            }

            // Item Treasure Hunter
            CmdTreasureHunterItem cmd_thItem = new Cmd.CmdTreasureHunterItem(); // Waiter

            NormalManagerDB.add(0,
                cmd_thItem, null, null);

            if (cmd_thItem.getException().getCodeError() != 0)
            {
                throw cmd_thItem.getException();
            }

            if (cmd_thItem.getInfo().Count == 0)
            {
                throw new exception("[TreasureHunterSystem::initialize][Error] nao tem item do Treasure Hunter no banco de dados.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.TREASURE_HUNTER_SYSTEM,
                    1, 1));
            }

            m_thItem = new List<TreasureHunterItem>(cmd_thItem.getInfo());

            // Init Time
            m_time = DateTime.Now;

            //#ifdef _DEBUG
            message_pool.push(new message("[TreasureHunterSystem::initialize][Log] Treasure Hunter System carregado com sucesso!", type_msg.CL_FILE_LOG_AND_CONSOLE));
            //#else
            //_smp::message_pool::getInstance().push(new message("[TreasureHunterSystem::initialize][Log] Treasure Hunter System carregado com sucesso!", type_msg.CL_ONLY_FILE_LOG));
            //#endif // _DEBUG

            // Carregado com sucesso
            m_load = true;
        }

        /*static */
        protected void clear()
        {

            Monitor.Enter(m_cs);
            // Limpa a lista de itens do Treasure Hunter System
            // n�o faz o shrink_to_fit por que pode preencher ela novamente
            if (m_thItem.Count > 0)
            {
                m_thItem.Clear();
            }

            m_load = false;

            Monitor.Exit(m_cs);

        }

        /*static */
        protected float getCourseRate(byte _course)
        {

            float rate_course = 0.0f;



            // Prote��o contra o Random Map, que usa o negatico do 'char'
            var course = findCourse((byte)(_course & 0x7F));

            var point = (course != null ? course.point : 0u);

            if (point > 0)
            {
                rate_course = (float)point / TREASURE_HUNTER_LIMIT_POINT_COURSE; // 1000.f;
            }
            return rate_course;
        }

        protected static void SQLDBResponse(int _msg_id,
            Pangya_DB _pangya_db,
            object _arg)
        {

            if (_arg == null)
            {
#if DEBUG
                // Static class
                message_pool.push(new message("[TreasureHunterSystem::SQLDBResponse][WARNING] _arg is nullptr com msg_id = " + Convert.ToString(_msg_id), type_msg.CL_FILE_LOG_AND_CONSOLE));
#endif // _DEBUG
                return;
            }

            // Por Hora s� sai, depois fa�o outro tipo de tratamento se precisar
            if (_pangya_db.getException().getCodeError() != 0)
            {
                message_pool.push(new message("[TreasureHunterSystem::SQLDBResponse][Error] " + _pangya_db.getException().getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
                return;
            }

            var _channel = Tools.reinterpret_cast<TreasureHunterSystem>(_arg);

            switch (_msg_id)
            {
                case 1: // Update Treasure Hunter Course Point
                    {
                        var cmd_uthcp = Tools.reinterpret_cast<CmdUpdateTreasureHunterCoursePoint>(_pangya_db);

                        message_pool.push(new message("[TreasureHunterSystem::SQLDBResponse][Log] Atualizou Treasure Hunter Course Point[COURSE=" + Convert.ToString((ushort)(cmd_uthcp.getInfo().course & 0x7F)) + ", POINT=" + Convert.ToString(cmd_uthcp.getInfo().point) + "]", type_msg.CL_FILE_LOG_AND_CONSOLE));

                        break;
                    }
                case 0:
                default:
                    break;
            }

        }

        /*static */
        private TreasureHunterInfo[] m_thi = Tools.InitializeWithDefaultInstances<TreasureHunterInfo>(_Define.MS_NUM_MAPS);
        /*static */
        private List<TreasureHunterItem> m_thItem = new List<TreasureHunterItem>(); // Treasure Hunter Item

        /*static */
        private DateTime m_time = new DateTime();

        /*static */
        private bool m_load;

        private object m_cs = new object();
    }
    public class sTreasureHunterSystem : Singleton<TreasureHunterSystem>
    { }

}
