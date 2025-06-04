using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Pangya_GameServer.Cmd;
using Pangya_GameServer.Game.System;
using Pangya_GameServer.GameType;
using PangyaAPI.IFF.JP.Extensions;
using PangyaAPI.IFF.JP.Models.Data;
using PangyaAPI.Utilities.Log;
using PangyaAPI.Utilities;
using PangyaAPI.SQL.Manager;
using MAP_COURSE_COIN_CUBE = System.Collections.Generic.Dictionary<byte, System.Collections.Generic.Dictionary<byte, System.Collections.Generic.List<Pangya_GameServer.GameType.CubeEx>>>;
using System.Runtime.CompilerServices;
using static Pangya_GameServer.GameType._Define;
using System.Collections.Concurrent;
using Pangya_GameServer.UTIL;
namespace Pangya_GameServer.Game.Manager
{

    public class CoinCubeLocationUpdateSystem
    {

        private BlockingCollection<CalculeCoinCubeUpdateOrder> m_pedidos = new BlockingCollection<CalculeCoinCubeUpdateOrder>();

        private bool m_load;

        private Thread m_thread;

        private int m_continue_translate = new int();

        private long m_update_location_time = 0; // Atualiza os spawn de coin e cube que foram gerados no tempo determinado

        private MAP_COURSE_COIN_CUBE m_course_coin_cube = new MAP_COURSE_COIN_CUBE(); // Esse � o que ele est� gerando das tacadas dos jogadores
        private MAP_COURSE_COIN_CUBE m_course_coin_cube_current = new MAP_COURSE_COIN_CUBE(); // Esse � o atual que ele est� spawnando no course
         
        public CoinCubeLocationUpdateSystem()
        {
            this.m_pedidos = new BlockingCollection<CalculeCoinCubeUpdateOrder>();
            this.m_course_coin_cube = new MAP_COURSE_COIN_CUBE();
            this.m_course_coin_cube_current = new MAP_COURSE_COIN_CUBE();
            this.m_thread = null;
            this.m_update_location_time = 0;
            m_continue_translate = 1; 
            // Inicializa
            initialize(); 
            m_thread = new Thread(translateOrder);
            m_thread.Start();

        }

        ~CoinCubeLocationUpdateSystem()
        {

            clear();

            try
            {

                Interlocked.Exchange(ref m_continue_translate, 0);

                if (m_thread != null)
                {

                    m_thread.Abort();
                    m_thread = null;
                }

            }
            catch (exception e)
            {

                message_pool.push(new message("[CoinCubeLocationUpdateSystem::~CoinCubeLocationUpdateSystem][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }

        }

        public void load()
        {

            if (isLoad())
            {
                clear();
            }

            initialize();
        }

        public bool isLoad()
        {

            return m_load;
        }

        public void pushOrderToCalcule(CalculeCoinCubeUpdateOrder _cccuo)
        {

            m_pedidos.Add(_cccuo);
        }
        public void forceUpdate()
        {
            update_spawn_location();
        }

        protected void initialize()
        {


            byte course_id = 0;

            foreach (var el in sIff.getInstance().getCourse())
            {

                course_id = (byte)sIff.getInstance().getItemIdentify(el.ID);


                CmdCoinCubeLocationInfo cmd_ccli = new CmdCoinCubeLocationInfo(course_id); 

                NormalManagerDB.add(0,
                    cmd_ccli, null, null);

                if (cmd_ccli.getException().getCodeError() != 0)
                {
                    throw cmd_ccli.getException();
                }

                var it = m_course_coin_cube_current.find(course_id);

                if (it.Key != 0 && it.Value != null)
                {
                    var getiinfo = cmd_ccli.getInfo();
                    m_course_coin_cube_current.Add(course_id, getiinfo);
                }
                else
                {
 
                    m_course_coin_cube_current[course_id] = cmd_ccli.getInfo();
                }
            }

            // Initialize time to update
            m_update_location_time = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            // Log
            message_pool.push(new message("[CoinCubeLocationUpdateSystem::initialize][Log] Cube Coin Location Update System carregado com sucesso!", type_msg.CL_FILE_LOG_AND_CONSOLE));

            // Carregado com sucesso
            m_load = true;

        }

        protected void clear()
        {

            if (m_pedidos.Count > 0)
            {
                m_pedidos = null;
            }

            if (!m_course_coin_cube.empty())
            {
                m_course_coin_cube.Clear();
            }

            if (!m_course_coin_cube_current.empty())
            {
                m_course_coin_cube_current.Clear();
            }

            m_update_location_time = 0;

            m_load = false;
        }

        protected void calculeShotCube(CalculeCoinCubeUpdateOrder _cccuo)
        {
            // Wiz City n�o calcula
            if ((_cccuo.course & 0x7Fu) != 19/*RoomInfo.eCOURSE.WIZ_CITY*/)
            {

#if _VERSION_PACKET_CALCULE_12F && _VERSION_PACKET_CALCULE_12
                Console.WriteLine("Defina apenas uma das duas: _VERSION_PACKET_CALCULE_12F ou _VERSION_PACKET_CALCULE_12");
#endif

#if _VERSION_PACKET_CALCULE_12F                            

                QuadTree3D qt = new UTIL.QuadTree3D();
                Ball3D ball = new UTIL.Ball3D();

                Vector3D wind = new Vector3D(_cccuo.shot_data_for_cube.wind_influence.x,
                    _cccuo.shot_data_for_cube.wind_influence.y,
                    _cccuo.shot_data_for_cube.wind_influence.z);

                var distance = (float)_cccuo.pin.diffXZ(_cccuo.shot_data_for_cube.location) * DIVIDE_SCALE_PANGYA;

                uint club_index = _cccuo.shot_data_for_cube.taco;

                if (club_index >= 3 && club_index < 13)
                {
                    club_index -= 2;
                }
                else if (club_index == 13)
                {
                    club_index = 12;
                }
                else if (club_index == 14)
                {
                    club_index = 11;
                }
                else if (club_index > 14)
                {
                    club_index -= 2;
                }

                Club3D club = new Club3D(sAllClubInfo3D.getInstance().m_clubs[(int)club_index], calculeTypeDistance(distance));

                if (club.m_club_info.m_type == eCLUB_TYPE.PT)
                {
                    return;
                }

                // Pelo pacote12F funciona s� em tournament base
                qt.init_shot(ball,
                    club, wind,
                    _cccuo.shot_data_for_cube,
                    distance);

                do
                {

                    qt.ballProcess(STEP_TIME); // S� vai at� o max height

                } while (ball.m_num_max_height < 0 && ball.m_count < 5000);

                message_pool.push(new message("[CoinCubeLocationUpdateSystem::calculeShotCube][Log] Power(" + Convert.ToString(_cccuo.shot_data_for_cube.power_club) + "y) Cube Location[X=" + Convert.ToString(ball.m_position.m_x) + ", Y=" + Convert.ToString(ball.m_position.m_y) + ", Z=" + Convert.ToString(ball.m_position.m_z) + "]", type_msg.CL_FILE_LOG_TEST_AND_CONSOLE));

                // C++ TO C# CONVERTER TASK: C# does not allow setting or comparing #define constants:
#elif _VERSION_PACKET_CALCULE

					QuadTree3D qt = new QuadTree3D();
					Ball3D ball = new Ball3D();

					var distance = (float)_last_position.diffXZ(_pin) * DIVIDE_SCALE_PANGYA;

					Club3D club = new Club3D(sAllClubInfo3D.getInstance().m_clubs[_pgi.shot_data.club], calculeTypeDistance(distance));

					// N�o Calcula Putt
					if(club.m_club_info.m_type == eCLUB_TYPE.PT)
					{
						return;
					}

					var deg_360 = (uint)(_pgi.degree / 255.0f * 360.0f);

					Vector3D wind = new Vector3D(_wind * (float)Math.Sin(deg_360 * PI / 180.0f) * -1.0f,
						0.0f,
						_wind * (float)Math.Cos(deg_360 * PI / 180.0f));

					var extra_power = _pi.getExtraPower();

					options3D options = new options3D(_pgi.shot_data.special_shot,
						new Vector3D(_last_position.x,
							_last_position.y,
							_last_position.z),
						extra_power,
						(_pgi.shot_sync.state_shot.shot.stShot.power_shot ? ePOWER_SHOT_FACTORY.ONE_POWER_SHOT : (_pgi.shot_sync.state_shot.shot.stShot.double_power_shot ? ePOWER_SHOT_FACTORY.TWO_POWER_SHOT : ePOWER_SHOT_FACTORY.NO_POWER_SHOT)),
						distance,
						(float)_pi.getSlotPower(),
						(_pgi.shot_data.bar_point[0] - 360.f) / 140.0f,
						_pgi.shot_data.ball_effect[1],
						_pgi.shot_data.ball_effect[0],
						_pgi.shot_data.mira);

					// Pelo pacote 12, funciona em todos os modos
					qt.init_shot(ball,
						club, wind, options);

					do
					{

						qt.ballProcess(STEP_TIME); // S� vai at� o max height

					} while(ball.m_num_max_height < 0 && ball.m_count < 5000);

#if DEBUG
					message_pool.push(new message("[CoinCubeLocationUpdateSystem::calculeShotCube][Log] Power(" + club.getRange(options.m_extra_power,
						options.m_power_slot,
						options.m_power_shot)) + "y) Cube Location[X=" + Convert.ToString(ball.m_position.m_x) + ", Y=" + Convert.ToString(ball.m_position.m_y) + ", Z=" + Convert.ToString(ball.m_position.m_z) + "]", type_msg.CL_FILE_LOG_TEST_AND_CONSOLE));
#endif // _DEBUG

#else
Console.WriteLine("Nenhuma versão definida: defina _VERSION_PACKET_CALCULE_12F ou _VERSION_PACKET_CALCULE_12"); 
#endif// _VERSION_PACKET_CALCULE

                // Verifica se ele atingiu uma altura boa para o spinning cube spawnar
                if ((ball.m_position.m_y - _cccuo.last_location.y) < ALTURA_MIN_TO_CUBE_SPAWN) // Altura menor que 70 n�o coloca o spinning cube para os spawns poss�veis
                {
                    return; // Sai
                }

                // Verifica se o cube pode ser um poss�vel spawn e se sim adiciona ele para a lista<map>
                checkAndAddCoinCube((byte)(_cccuo.course & 0x7Fu),
                    _cccuo.hole,
                    new CubeEx(0,
                        Cube.eTYPE.CUBE, 0,
                        Cube.eFLAG_LOCATION.AIR,
                        ball.m_position.m_x,
                        ball.m_position.m_y,
                        ball.m_position.m_z, 1U));
            }

        }

        protected void calculeShotCoin(CalculeCoinCubeUpdateOrder _cccuo)
        {

            // Verifica se a coin pode ser um poss�vel spawn e se sim adiciona ela para a lista<map>
            checkAndAddCoinCube((byte)(_cccuo.course & 0x7Fu),
                _cccuo.hole,
                new CubeEx(0,
                    Cube.eTYPE.COIN, 0,
                    Cube.eFLAG_LOCATION.GROUND,
                    _cccuo.last_location.x,
                    _cccuo.last_location.y,
                    _cccuo.last_location.z, 1U));
        }

        protected void checkAndAddCoinCube(byte _course_id,
            byte _hole_number,
            CubeEx _cube)
        {


            var it_course = m_course_coin_cube.find(_course_id & 0x7Fu);

            if (it_course.Key != 0 && it_course.Value != null)
            {

                var it_hole = it_course.Value.find(_hole_number);

                if (it_hole.Key != 0 && it_hole.Value != null)
                {

                    // Verifica se o cube est� no raio de outro cube se n�o add o cube ou aumenta o rate do outro cube
                    Location lc = new Location() { x = _cube.location.x, y = _cube.location.y, z = _cube.location.z };

                    var it = it_hole.Value.FirstOrDefault(_el =>
                    {
                        return _el.tipo == _cube.tipo && Math.Abs(lc.diff(_el.location)) <= 5 * SCALE_PANGYA;
                    });

                    if (it != null)
                    {
                        it.rate++;
                    }
                    else
                    {
                        it_hole.Value.Add(_cube);
                    }

                }
                else
                {

                    var ret = it_course.Value.insert(Tuple.Create(_hole_number, new List<CubeEx>() { _cube }));

                    if (!ret.Value.empty() && ret.Value.Count == 0)
                    {
                        message_pool.push(new message("[CoinCubeLocationUpdateSystem::checkAndAddCoinCube][WARNING] nao conseguiu adicionar o map de hole com o Cube/Coin[TYPE=" + Convert.ToString(_cube.tipo) + ", X=" + Convert.ToString(_cube.location.x) + ", Y=" + Convert.ToString(_cube.location.y) + ", Z=" + Convert.ToString(_cube.location.z) + "] no Course[ID=" + Convert.ToString((ushort)_course_id) + ", HOLE=" + Convert.ToString((ushort)_hole_number) + "]", type_msg.CL_FILE_LOG_AND_CONSOLE));
                    }
                }

            }
            else
            {

                var ret = m_course_coin_cube.insert(
       Tuple.Create(
           (byte)(_course_id & 0x7Fu),
           new Dictionary<byte, List<CubeEx>> {
            {
                _hole_number, new List<CubeEx> { _cube }
            }
           }
       )
   );


                if (!ret.Value.empty())
                {
                    message_pool.push(new message("[CoinCubeLocationUpdateSystem::checkAndAddCoinCube][WARNING] nao conseguiu adicionar o map de course com o Cube/Coin[TYPE=" + Convert.ToString(_cube.tipo) + ", X=" + Convert.ToString(_cube.location.x) + ", Y=" + Convert.ToString(_cube.location.y) + ", Z=" + Convert.ToString(_cube.location.z) + "] no Course[ID=" + Convert.ToString((ushort)_course_id) + ", HOLE=" + Convert.ToString((ushort)_hole_number) + "]", type_msg.CL_FILE_LOG_AND_CONSOLE));
                }
            }

        }

        protected void update_spawn_location()
        {
            // lambda 
            var sort_per_rate = new Comparison<CubeEx>((a, b) => b.rate.CompareTo(a.rate)); // Descendente



            // CoinCubeUpdate para atualiza no banco de dados
            List<CoinCubeUpdate> v_coin_cube_update = new List<CoinCubeUpdate>();
            Map.stCtx map_ctx = null;
            Location lc = new Location() { x = 0 };
            uint count_new_add_all = 0;
            uint count_new_add_hole = 0;
            uint limit = 0;

            foreach (var el_course in m_course_coin_cube)
            {

                // Wiz City n�o atualiza nada
                if ((el_course.Key & 0x7Fu) == (byte)RoomInfo.eCOURSE.WIZ_CITY)
                {
                    continue;
                }

                Dictionary<byte, List<CubeEx>> it_c;
                if (!m_course_coin_cube_current.TryGetValue((byte)(el_course.Key & 0x7Fu), out it_c))
                    continue;

                map_ctx = sMap.getInstance().getMap((byte)(el_course.Key & 0x7Fu));

                if (map_ctx == null)
                {

                    // Log
                    message_pool.push(new message("[CoinCubeLocationUpdateSyatem::update_spawn_location][WARNING] nao encontrou o Course[ID=" + Convert.ToString((ushort)el_course.Key) + "] no singleton de Course(sMap).", type_msg.CL_FILE_LOG_AND_CONSOLE));

                    // Continua
                    continue;
                }

                foreach (var el_hole in el_course.Value)
                {

                    if (el_hole.Key < 1 || el_hole.Key > 18)
                    {

                        // Log
                        message_pool.push(new message("[CoinCubeLocationUpdateSystem::update_spawn_location][WARNING] hole[NUMERO=" + Convert.ToString((ushort)el_hole.Key) + "] invalido no Course[ID=" + Convert.ToString((ushort)el_course.Key) + "]", type_msg.CL_FILE_LOG_AND_CONSOLE));

                        // Continua
                        continue;
                    }

                    // Get limit per hole
                    limit = getLimitCoinCubePerParHole((byte)map_ctx.range_score.par[el_hole.Key - 1]);

                    // [upt] Count all add
                    count_new_add_all += count_new_add_hole;

                    // reset
                    count_new_add_hole = 0;

                    // Adiciona os cube/coin que conseguiu esse hole n�o tem nenhum cube/coin
                    List<CubeEx> it_h;
                    if (!it_c.TryGetValue(el_hole.Key, out it_h))
                    {

                        foreach (var el_cube in el_hole.Value)
                        {

                            // Adiciona se tiver espa�o
                            if (count_new_add_hole < limit)
                            {

                                // Add new Cube/Coin Location
                                count_new_add_hole++;

                                v_coin_cube_update.Add(new CoinCubeUpdate(CoinCubeUpdate.eTYPE.INSERT, (byte)(el_course.Key & 0x7Fu), el_hole.Key, el_cube));
                            }
                        }

                        // Continua
                        continue;
                    }

                    // Sort cubes por rate
                    it_h.Sort(sort_per_rate);
                    el_hole.Value.Sort(sort_per_rate);

                    foreach (var el_cube in el_hole.Value)
                    {

                        lc.x = el_cube.location.x;
                        lc.y = el_cube.location.y;
                        lc.z = el_cube.location.z;

                        // Atualiza o rate da mesma location - no que j� est�o sendo atualizados
                        var it_upt_cc = v_coin_cube_update.Find(_el =>
                      (_el.course_id & 0x7Fu) == (el_course.Key & 0x7Fu) &&
                      _el.hole_number == el_hole.Key &&
                      _el.cube.tipo == el_cube.tipo &&
                      Math.Abs(lc.diff(_el.cube.location)) <= 5 * SCALE_PANGYA
                  );


                        if (it_upt_cc != v_coin_cube_update.end())
                        {

                            // Update
                            it_upt_cc.cube.rate += el_cube.rate;

                            // Continua
                            continue;
                        }

                        // Atualiza o rate da mesma location
                        var it_upt = it_h.Find(_el =>
                        _el.tipo == el_cube.tipo &&
                        Math.Abs(lc.diff(_el.location)) <= 5 * SCALE_PANGYA
                    );


                        if (it_upt != null)
                        {
                            el_cube.id = it_upt.id;
                            v_coin_cube_update.Add(new CoinCubeUpdate(CoinCubeUpdate.eTYPE.UPDATE, (byte)(el_course.Key & 0x7Fu), el_hole.Key, el_cube));
                        }

                        // Adiciona se tiver espa�o 
                        if ((it_h.Count + count_new_add_hole) < limit)
                        {
                            count_new_add_hole++;
                            v_coin_cube_update.Add(new CoinCubeUpdate(CoinCubeUpdate.eTYPE.INSERT, (byte)(el_course.Key & 0x7Fu), el_hole.Key, el_cube));
                            continue;
                        }


                        // Verifica se o rate passou do outro que est� nos atualizados e substitui
                        it_upt_cc = v_coin_cube_update.Find(_el =>
                         (_el.course_id & 0x7Fu) == (el_course.Key & 0x7Fu) &&
                         _el.hole_number == el_hole.Key &&
                         _el.cube.tipo == el_cube.tipo &&
                         _el.cube.rate < el_cube.rate
                     );

                        if (it_upt_cc != null)
                        {

                            if (it_upt_cc.type == CoinCubeUpdate.eTYPE.UPDATE)
                            {
                                el_cube.id = it_upt_cc.cube.id;
                            }

                            it_upt_cc.cube = el_cube;

                            // Continua;
                            continue;
                        }

                        // Verifica se o rate passou do outro e substitui
                        it_upt = it_h.Find(_el =>
                       _el.tipo == el_cube.tipo &&
                       _el.rate < el_cube.rate
                   );
                        // Update
                        if (it_upt != null)
                        {

                            // Atualiza a location do Cube/Coin
                            el_cube.id = it_upt.id;

                            v_coin_cube_update.Add(new CoinCubeUpdate(CoinCubeUpdate.eTYPE.UPDATE, (byte)(el_course.Key & 0x7Fu), el_hole.Key, el_cube));
                        }
                    }
                }
            }

            // Finish update count all
            if (count_new_add_hole > 0)
            {
                // [upt] Count all add
                count_new_add_all += count_new_add_hole;
            }

            // reset
            count_new_add_hole = 0;

            // Adiciona no baco de dados
            if (v_coin_cube_update.Count > 0)
            {

                CmdUpdateCoinCubeLocation cmd_uccl = new Cmd.CmdUpdateCoinCubeLocation(); // Waiter;

                foreach (var el in v_coin_cube_update)
                {

                    cmd_uccl.setInfo(el);

                    NormalManagerDB.add(0,
                        cmd_uccl, null, null);

                    if (cmd_uccl.getException().getCodeError() != 0)
                    {
                        message_pool.push(new message("[CoinCubeLocationUpdateSystem::update_spawn_location][Error] " + cmd_uccl.getException().getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
                    }
                }
            }

            // Atualizou o Spwan location da coin e do cube
            m_update_location_time = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            // Atualiza os current e esse
            load();

            // Atualiza o sistema de cube e coin
            sCubeCoinSystem.getInstance().load();

            // Log
            message_pool.push(new message("[CoinCubeLocationUpdateSystem::update_spawn_location][Log] Adicionou " + Convert.ToString(count_new_add_all) + " e Atualizou " + Convert.ToString(v_coin_cube_update.Count - count_new_add_all) + " spawn(s) location da coin e cube com sucesso.", type_msg.CL_FILE_LOG_AND_CONSOLE));

        }

        protected uint getLimitCoinCubePerParHole(byte _par_hole)
        {

            uint limit = 0;

            switch (_par_hole)
            {
                case 3:
                    limit = LIMIT_LOCATION_COIN_CUBE_PER_HOLE_PAR_3;
                    break;
                case 4:
                    limit = LIMIT_LOCATION_COIN_CUBE_PER_HOLE_PAR_4;
                    break;
                case 5:
                    limit = LIMIT_LOCATION_COIN_CUBE_PER_HOLE_PAR_5;
                    break;
            }

            return (limit);
        }


        protected void translateOrder()
        {
            try
            {

                CalculeCoinCubeUpdateOrder pedido = new CalculeCoinCubeUpdateOrder();

                message_pool.push(new message("[CoinCubeLocationUpdateSystem::translateOrder][Log] translateOrder iniciado com sucesso!"));

                while (Interlocked.CompareExchange(ref m_continue_translate,
                    1, 1) == 1)
                { 
                    try
                    {

                        // Verifica se j� est� na hora de atualizar os spawn location da coin e do cube
                        if (m_update_location_time == 0)
                        {
                            m_update_location_time = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                        }
                        else if ((m_update_location_time + (UPDATE_TIME_INTERVAL_HOUR /* 24 horas */) * 60 * 60) < DateTimeOffset.UtcNow.ToUnixTimeSeconds())
                        {
                            update_spawn_location();
                        }

                        // Verifica se tem pedido para calcular tacada
                        pedido = GetFirst(5 * 1000); // Espera 5 segundos por um pedido

                        if (pedido != null)
                        {
                            switch (pedido.type)
                            {
                                case CalculeCoinCubeUpdateOrder.eTYPE.COIN:
                                    // Calcule coin location
                                    calculeShotCoin(pedido);
                                    break;
                                case CalculeCoinCubeUpdateOrder.eTYPE.CUBE:
                                    // Calcule cube location
                                    calculeShotCube(pedido);
                                    break;
                            }
                        }

                        Thread.Sleep(1000);
                    }
                    catch (exception e)
                    {

                        if (ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) != (byte)STDA_ERROR_TYPE.LIST_ASYNC)
                        {
                            throw;
                        }

                        if (ExceptionError.STDA_ERROR_DECODE(e.getCodeError()) != 2)
                        {
                            message_pool.push(new message("[CoinCubeLocationUpdateSystem::translateOrder][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
                        }
                    }
                }

            }
            catch (exception e)
            {
                message_pool.push(new message("[CoinCubeLocationUpdateSystem::translateOrder][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
            catch (Exception e)
            {
                message_pool.push(new message("[CoinCubeLocationUpdateSystem::translateOrder][ErrorSystem] " + e.Message, type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
            
            message_pool.push(new message("Saindo de translateOrder()..."));
        } 

    public CalculeCoinCubeUpdateOrder GetFirst(int timeoutMilliseconds)
        {
            CalculeCoinCubeUpdateOrder pedido;
            if (m_pedidos.TryTake(out pedido, timeoutMilliseconds))
                return pedido;  // item retirado com sucesso dentro do timeout
            else
                return null;    // timeout sem receber item
        }
    }

    public class sCoinCubeLocationUpdateSystem: Singleton<CoinCubeLocationUpdateSystem>
    { }
}