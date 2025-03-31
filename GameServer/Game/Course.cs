//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Net.Http.Headers;
//using System.Text;
//using System.Threading.Tasks;
//using GameServer.GameType;

//namespace GameServer.Game
//{
//    public class Course
//    {
//        public class Sequencia
//        {
//            public Sequencia(uint _ul = 0u)
//            {
//                clear();
//            }
//            public Sequencia(ushort _hole)
//            {
//                clear();

//                m_hole = _hole;
//            }
//            public Sequencia(byte _course, ushort _hole)
//            {
//                this.m_course = _course;
//                this.m_hole = _hole;
//            }
//            public void clear()
//            {
                
//            }
//            public byte m_course;
//            public ushort m_hole;
//        }

//        public Course(RoomInfoEx _ri,
//            byte _channel_rookie,
//            float _star,
//            uint _rate_rain,
//            byte _rain_persist_flag)
//        {
//            this.m_ri = _ri;
//            this.m_channel_rookie = _channel_rookie;
//            this.m_star = _star;
//            this.m_rate_rain = _rate_rain;
//            this.m_rain_persist_flag = _rain_persist_flag;
//            this.m_hole = new SortedDictionary<ushort, Hole>();
//            this.m_seq = new List<Sequencia>();
//            this.m_seed_rand_game = 0u;
//            this.m_flag_cube_coin = 1;
//            this.m_wind_flag = 0;
//            this.m_wind_range = new ushort[9];
//            this.m_chr = new ConsecutivosHolesRain();
//            this.m_holes_rain = new HolesRain() ;
//            this.m_grand_prix_special_hole = false;

//            init_seq();

//            init_hole();

//            init_dados_rain(); // Inicializar os dados de chuva no course, para ser usado no achievement

//            // Deixa esse s� com int16(short), por que s� vejo n�mero baixo, n�o passa do valor m�ximo do int16
//            m_seed_rand_game = (ushort)sRandomGen.getInstance().rIbeMt19937_64_chrono();
//        }

//        public virtual void Dispose()
//        {

//            if (!m_hole.empty())
//            {
//                m_hole.clear();
//            }

//            if (!m_seq.empty())
//            {
//                m_seq.clear();
//                m_seq.shrink_to_fit();
//            }
//        }

//        // Get
//        public uint getSeedRandGame()
//        {
//            return m_seed_rand_game;
//        }

//        public ushort getFlagCubeCoin()
//        {
//            return m_flag_cube_coin;
//        }

//        public float getStar()
//        {
//            return m_star;
//        }

//        /// Finders

//        // Find Hole, se n�o achar retorna um ponteiro nulo
//        public Hole findHole(ushort _number)
//        {

//            if ((short)_number < 0)
//            {
//                return null;
//            }

//            var it = m_hole.end();

//            if ((it = std::find_if(m_hole.begin(), m_hole.end(), (_el) =>
//            {
//                return _el.second.getNumero() == _number;
//            })) != m_hole.end())
//            {
//                return it.second;
//            }

//            return null;
//        }

//        public Hole findHoleBySeq(ushort _seq)
//        {

//            if ((short)_seq <= 0 || _seq > m_hole.size())
//            {
//                return null;
//            }

//            var it = m_hole.find(_seq);

//            if (it == m_hole.end())
//            {
//                _smp.message_pool.getInstance().push(new message("[Course::findHoleBySeq][WARNIG] nao encontrou a seq[value=" + Convert.ToString(_seq) + "] no map de hole. Bug", CL_FILE_LOG_AND_CONSOLE));
//            }

//            return (it != m_hole.end()) ? it.second : null;
//        }

//        // Find Hole Sequ�ncia
//        public ushort findHoleSeq(ushort _number)
//        {

//            if ((short)_number < 0)
//            {
//                return (ushort)~0; // Error
//            }

//            var it = m_hole.end();

//            if ((it = std::find_if(m_hole.begin(), m_hole.end(), (_el) =>
//            {
//                return _el.second.getNumero() == _number ? 1 : 0;
//            })) != m_hole.end())
//            {
//                return it.first;
//            }

//            return (ushort)0;
//        }

//        // Find intervalo de hole do n�mero fornecido at� o ultimo do map
//        public Tuple<SortedDictionary<ushort, Hole>.Enumerator, SortedDictionary<ushort, Hole>.Enumerator> findRange(ushort _number)
//        {

//            if ((short)_number >= 0)
//            {

//                // C++ TO C# CONVERTER TASK: Lambda expressions cannot be assigned to 'var':
//                var find = std::find_if(m_hole.begin(), m_hole.end(), (_el) =>
//                {
//                    return _el.second.getNumero() == _number;
//                });

//                return Tuple.Create(find, m_hole.end());
//            }

//            return Tuple.Create(m_hole.end(), m_hole.end());
//        }

//        // Random Wind and Degree
//        public stHoleWind shuffleWind(uint _seed = 777u)
//        {

//            stHoleWind wind = new stHoleWind();

//            if (m_wind_flag != 0)
//            {

//                do
//                {

//                    // Update seed
//                    wind.wind = (byte)(m_wind_range[0] + (sRandomGen.getInstance().rIbeMt19937_64_rdevice() % (m_wind_range[1] - m_wind_range[0])));

//                } while (((m_wind_flag == 2) ? ((wind.wind + 1) % 2) : !((wind.wind + 1) % 2)) != 0);

//            }
//            else
//            {

//                // Update seed
//                wind.wind = (byte)(m_wind_range[0] + (sRandomGen.getInstance().rIbeMt19937_64_rdevice() % (m_wind_range[1] - m_wind_range[0])));
//            }

//            wind.degree.setDegree(sRandomGen.getInstance().rIbeMt19937_64_chrono() % stdA.Globals.LIMIT_DEGREE);

//            // C++ TO C# CONVERTER TASK: The following line was determined to contain a copy constructor call - this should be verified and a copy constructor should be created:
//            // ORIGINAL LINE: return wind;
//            return new stdA.stHoleWind(wind);
//        }

//        // Random wind next hole(s)
//        public void shuffleWindNextHole(ushort _number)
//        {

//            if ((short)_number < 0)
//            {
//                throw exception("[Course::shuffleWindNextHole][Error] _number[VALUE=" + Convert.ToString((short)_number) + "] is invalid", STDA_MAKE_ERROR(STDA_ERROR_TYPE.COURSE,
//                    1, 0));
//            }

//            // C++ TO C# CONVERTER TASK: Lambda expressions cannot be assigned to 'var':
//            var it = std::find_if(m_hole.begin(), m_hole.end(), (_el) =>
//            {
//                return _el.second.getNumero() == _number;
//            });

//            if (it == m_hole.end())
//            {
//                throw exception("[Course::shuffleWindNextHole][Error] nao conseguiu encontrar o hole[NUMERO=" + Convert.ToString(_number) + "]", STDA_MAKE_ERROR(STDA_ERROR_TYPE.COURSE,
//                    2, 0));
//            }

//            var wind = shuffleWind((uint)(size_t)this);

//            for (; it != m_hole.end(); ++it)
//            {
//                it.second.setWind(wind);
//            }
//        }

//        // Make Packet Buffer Hole(s) Info
//        public void makePacketHoleInfo(packet _p, int _option = 0)
//        {

//            // Hole(s) Info
//            foreach (var el in m_hole)
//            {
//                _p.addUint32(el.second.getId());
//                _p.addUint8(el.second.getPin());

//                if (_option == 0)
//                {
//                    _p.addUint8(el.second.getCourse());
//                }

//                _p.addUint8((byte)el.second.getNumero());
//            }

//            // Course Seed Random
//            _p.addUint32(m_seed_rand_game);

//            // Hole(s) Spinning Cube / Coin Info
//            makePacketHoleSpinningCubeInfo(_p);
//        }

//        // Make Packet Buffer Hole(s) Spinning Cube(s) Info
//        public void makePacketHoleSpinningCubeInfo(packet _p)
//        {

//            foreach (var el in m_hole)
//            {

//                _p.addUint8((byte)el.second.getCubes().size()); // Size

//                foreach (var el2 in el.second.getCubes())
//                {
//                    _p.addUint32(el2.tipo);
//                    _p.addUint32(el2.id);
//                    _p.addUint32(el2.flag_unknown);
//                    _p.addUint32(el.second.getCourse());
//                    _p.addUint8((byte)(el.second.getModo() == Hole.M_REPEAT ? el.second.getHoleRepeat() : el.second.getNumero()));
//                    _p.addUint8((byte)el.first - 1); // Index
//                    _p.addUint16(m_flag_cube_coin);
//                    _p.addBuffer(el2.location, sizeof(el2.location));
//                    _p.addUint32(el2.flag_location);
//                }
//            }
//        }

//        public uint countHolesRain()
//        {
//            return m_holes_rain.getCountHolesRain();
//        }

//        public uint countHolesRainBySeq(uint _seq)
//        {
//            return m_holes_rain.getCountHolesRainBySeq(_seq);
//        }

//        // retorna Media de tacadas do course para fazer par em todos os holes
//        public float getMediaAllParHoles()
//        {

//            if (m_hole.empty())
//            {
//                return 1.0f; // N�o tem nenhum hole inicializado
//            }

//            uint count = 0u;

//            foreach (var el in m_hole)
//            {
//                count += el.second.getPar().par;
//            }

//            return (float)(count / (float)m_hole.size());
//        }

//        public float getMediaAllParHolesBySeq(uint _seq)
//        {

//            if (_seq <= 0 || _seq > m_hole.size())
//            {
//                return 1.0f; // Sequ�ncia inv�lida
//            }

//            if (m_hole.empty())
//            {
//                return 1.0f; // N�o tem nenhum hole inicializado
//            }

//            uint count = 0u;

//            for (var it = m_hole.begin(); it != m_hole.end() && it.first <= _seq; ++it)
//            {
//                count += it.second.getPar().par;
//            }

//            return (float)(count / (float)_seq);
//        }

//        public ConsecutivosHolesRain getConsecutivesHolesRain()
//        {
//            return m_chr;
//        }

//        protected void init_seq()
//        {

//            // Verifica se � Grand Prix e se tem Special Hole
//            if (m_ri.grand_prix.active && m_ri.grand_prix.dados_typeid > 0)
//            {

//                // Grand Prix Special Hole
//                var sh = sIff.getInstance().findGrandPrixSpecialHole(m_ri.grand_prix.rank_typeid);

//                if (!sh.empty())
//                {

//                    // Sort Sequencie
//                    // Ordena do menor para o maior por sequ�ncia do hole
//                    std::sort(sh.begin(), sh.end(), (_1, _2) =>
//                    {
//                        return _1.seq < _2.seq;
//                    });

//                    foreach (var el in sh)
//                    {
//                        m_seq.push_back(new Sequencia((byte)el.course, (ushort)el.hole));
//                    }

//                    // Completa os 18 Holes
//                    if (m_seq.size() < 18)
//                    {

//                        for (var i = m_seq.size() + 1; i <= 18u; ++i)
//                        {
//                            m_seq.push_back(new Sequencia((byte)m_ri.course, (ushort)i));
//                        }
//                    }

//                    // Tem Special Hole
//                    m_grand_prix_special_hole = true;

//                    // Sai da fun��o por que os holes speciais do Prand Prix
//                    return;
//                }
//            }

//            // Normal
//            switch (m_ri.modo)
//            {
//                case Hole.eMODO.M_FRONT:
//                case Hole.eMODO.M_REPEAT:
//                    // !@ Teste
//#if DEBUG
//                    for (var i = 1u; i <= 18u; i++)
//                    {
//                        m_seq.push_back(new Sequencia((ushort)(i)));
//                    }
//#else
//					for(var i = 1u; i <= 18u; ++ i)
//					{
//						m_seq.push_back(new Sequencia((ushort)i));
//					}
//#endif
//                    break;
//                case Hole.eMODO.M_BACK:
//                    {
//                        var i = 10u;
//                        for (; i <= 18; ++i)
//                        {
//                            m_seq.push_back(new Sequencia((ushort)i));
//                        }
//                        for (i = 1u; i < 10; ++i)
//                        {
//                            m_seq.push_back(new Sequencia((ushort)i));
//                        }
//                        break;
//                    }
//                case Hole.eMODO.M_RANDOM:
//                    {
//                        ushort rand = (sRandomGen.getInstance().rIbeMt19937_64_chrono() % 18) + 1;
//                        for (var i = rand; i <= 18; ++i)
//                        {
//                            m_seq.push_back(new Sequencia((ushort)i));
//                        }
//                        if (rand > 1)
//                        {
//                            for (var i = 1u; i < rand; ++i)
//                            {
//                                m_seq.push_back(new Sequencia((ushort)i));
//                            }
//                        }
//                        break;
//                    }
//                case Hole.eMODO.M_SHUFFLE:
//                    {
//                        Lottery lottery = new Lottery((uint64_t)this);

//                        for (var i = 1u; i <= 18; ++i)
//                        {
//                            lottery.push(1000, i);
//                        }

//                        Lottery.LotteryCtx lc = null;

//                        for (var i = 0u; i < 18; ++i)
//                        {
//                            if ((lc = lottery.spinRoleta(true)) != null)
//                            {
//                                m_seq.push_back(new Sequencia((ushort)lc.value));
//                            }
//                        }

//                        break;
//                    }
//                case Hole.eMODO.M_SHUFFLE_COURSE:
//                    {

//                        ushort hole_ssc = (sRandomGen.getInstance().rIbeMt19937_64_chrono() % 2) + 1;

//                        Lottery lottery = new Lottery((uint64_t)this);

//                        for (var i = 1u; i <= 18; ++i)
//                        {
//                            lottery.push(1000, i);
//                        }

//                        Lottery.LotteryCtx lc = null;

//                        // 17 Holes S�
//                        for (var i = 0u; i < 18; ++i)
//                        {
//                            if ((lc = lottery.spinRoleta(true)) != null && lc.value != hole_ssc)
//                            {
//                                m_seq.push_back(new Sequencia((ushort)lc.value));
//                            }
//                        }

//                        // ultimo Hole � do SSC
//                        m_seq.push_back(new Sequencia((ushort)hole_ssc));

//                        break;
//                    } // End Case M_SHUFFLE_COURSE
//            } // End Switch

//        }

//        protected void init_hole()
//        {

//            uCubeCoinFlag cube_coin = new uCubeCoinFlag();

//            // Enable Coin e Cube in Course Default
//            cube_coin.stFlag.enable = 1u;
//            cube_coin.stFlag.enable_coin = 1u;

//            // Type Cube Game Mode
//            if (m_ri.modo == Hole.eMODO.M_REPEAT)
//            {
//                cube_coin.stFlag.type = 1u;
//            }
//            else if (m_ri.tipo == RoomInfo.TIPO.STROKE || m_ri.tipo == RoomInfo.TOURNEY || m_ri.tipo == RoomInfo.GUILD_BATTLE || m_ri.tipo == RoomInfo.MATCH || m_ri.tipo == RoomInfo.PRACTICE || m_ri.tipo == RoomInfo.SPECIAL_SHUFFLE_COURSE)
//            {
//                cube_coin.stFlag.type = 2u;
//            }

//            switch (m_ri.artefato)
//            {
//                case DefineConstants.ORCHID_BLOSSOM_ART: // 1 a 8m
//                    m_wind_range[1] = 8;
//                    break;
//                case DefineConstants.PENNE_ABACUS_ART: // Wind Impar
//                    m_wind_flag = 1;
//                    break;
//                case DefineConstants.TITAN_WINDMILL_ART: // Wind Par
//                    m_wind_flag = 2;
//                    break;
//            }

//            if (m_ri.grand_prix.active && m_ri.grand_prix.dados_typeid > 0)
//            {

//                // Grand Prix n�o tem cube
//                cube_coin.stFlag.enable = 0u;

//                try
//                {

//                    var gp = sIff.getInstance().findGrandPrixData(m_ri.grand_prix.dados_typeid);

//                    // Grand Prix Data -> Rule
//                    if (gp != null)
//                    {

//                        // Aqui inicializa as regras do Grand Prix de vento
//                        switch (gp.rule)
//                        {
//                            case DefineConstants.ONLY_1M_RULE:
//                                m_wind_range[1] = 1;
//                                break;
//                            case DefineConstants.SUPER_WIND_RULE:
//                                m_wind_range[0] = 9;
//                                m_wind_range[1] = 15;
//                                break;
//                            case DefineConstants.HOLE_CUP_MAGNET_RULE: // Ainda n�o sei esses aqui, como funciona
//                            case DefineConstants.NO_TURNING_BACK_RULE: // Ainda n�o sei esses aqui, como funciona
//                                break;
//                            case DefineConstants.WIND_3M_A_5M_RULE:
//                                m_wind_range[0] = 2;
//                                m_wind_range[1] = 5;
//                                break;
//                            case DefineConstants.WIND_7M_A_9M_RULE:
//                                m_wind_range[0] = 6;
//                                break;
//                        }

//                    }
//                    else
//                    {
//                        _smp.message_pool.getInstance().push(new message("[Course::init_hole][Error] tentou pegar o Grand Prix[TYPEID=" + Convert.ToString(m_ri.grand_prix.dados_typeid) + "] no IFF_STRUCT do server mais ele nao existe. Bug", CL_FILE_LOG_AND_CONSOLE));
//                    }

//                }
//                catch (exception e)
//                {

//                    _smp.message_pool.getInstance().push(new message("[Course::init_hole][ErrorSystem] " + e.getFullMessageError(), CL_FILE_LOG_AND_CONSOLE));
//                }

//            }
//            else if (m_channel_rookie == 1)
//            {
//                m_wind_range[1] = 5;
//            }

//            byte new_course = (byte)(m_ri.course & 0x7F);
//            byte pin = 0u;
//            byte weather = 0u;

//            byte persist_rain = 0u;

//            stHoleWind wind = new stHoleWind();

//            // Lottery Wind
//            Lottery loterry = new Lottery((uint64_t)this);

//            var rate_good_weather = (m_rate_rain <= 0) ? 1000 : ((m_rate_rain < 1000) ? 1000 - m_rate_rain : 1);

//            // Coloquei 4 pra 1, antes estava 3 pra 1
//            loterry.push(rate_good_weather, 0);
//            loterry.push(rate_good_weather, 0);
//            loterry.push(rate_good_weather, 0);
//            loterry.push(rate_good_weather, 0);
//            loterry.push(m_rate_rain, 2);

//            // Lottery Course
//            Lottery lottery_map = new Lottery((uint64_t)this);

//            uint course_id = 0u;

//            foreach (var el in sIff.getInstance().getCourse())
//            {

//                course_id = sIff.getInstance().getItemIdentify(el.second._typeid);

//                if (course_id != 17 && course_id != 0x40)
//                {
//                    lottery_map.push(100, course_id);
//                }
//            }

//            for (var i = 1u; i <= 18u; ++i)
//            {

//                // Reseta flag cube
//                cube_coin.stFlag.enable_cube = 0u;
//                cube_coin.stFlag.enable_coin = 0u;

//                if (i <= m_ri.qntd_hole)
//                {

//                    if (m_ri.modo == Hole.eMODO.M_REPEAT && i == 1)
//                    {
//                        wind = shuffleWind(i);
//                    }
//                    else if (m_ri.modo != Hole.eMODO.M_REPEAT)
//                    {
//                        wind = shuffleWind(i);
//                    }

//                    if (m_ri.fixed_hole == 7 && i == 1)
//                    {
//                        pin = sRandomGen.getInstance().rIbeMt19937_64_rdevice() % 3;
//                    }
//                    else if (m_ri.fixed_hole != 7)
//                    {
//                        pin = sRandomGen.getInstance().rIbeMt19937_64_rdevice() % 3;
//                    }

//                    weather = 0u;

//                    var lc = loterry.spinRoleta();

//                    if (lc != null)
//                    {
//                        weather = (byte)lc.value;
//                    }

//                    if (persist_rain != 0 || weather == 2)
//                    {

//                        if (persist_rain == 0
//                            && weather == 2
//                            && m_rain_persist_flag)
//                        {
//                            persist_rain = 1;
//                        }
//                        else if (persist_rain)
//                        {
//                            weather = 2;
//                            persist_rain = 0;
//                        }

//                        try
//                        {
//                            if (i > 1 && m_hole.at(i - 1).getWeather() == 0)
//                            {
//                                m_hole.at(i - 1).setWeather(1);
//                            }
//                        }
//                        catch (System.IndexOutOfRangeException e)
//                        {
//                            UNREFERENCED_PARAMETER(e);
//                        }
//                    }

//                    if (m_ri.tipo == RoomInfo.SPECIAL_SHUFFLE_COURSE && m_ri.modo == Hole.eMODO.M_SHUFFLE_COURSE)
//                    {

//                        if (i == 18) // Ultimo Hole � do SSC
//                        {
//                            new_course = RoomInfo.CHRONICLE_1_CHAOS;
//                        }
//                        else
//                        {

//                            var lc = lottery_map.spinRoleta();

//                            if (lc != null)
//                            {
//                                new_course = (byte)lc.value;
//                            }
//                        }
//                    }

//                    // Cube a cada 3 hole
//                    if (i % 3 == 0 != null)
//                    {
//                        cube_coin.stFlag.enable_cube = 1u;
//                    }

//                    // Coin todos os holes
//                    if (cube_coin.stFlag.enable)
//                    {
//                        cube_coin.stFlag.enable_coin = 1u;
//                    }

//                    if (m_ri.grand_prix.active
//                        && m_ri.grand_prix.dados_typeid > 0
//                        && m_grand_prix_special_hole)
//                    {

//                        // A fun��o init_seq j� inicializa a sequ�ncia se for Grand Prix e se ele tiver Special Hole
//                        m_hole.insert(Tuple.Create((ushort)i, new Hole(m_seq[i - 1].m_course,
//                            m_seq[i - 1].m_hole, pin,
//                            Hole.eMODO(m_ri.modo),
//                            (byte)m_ri.hole_repeat,
//                            weather, wind.wind,
//                            wind.degree.getDegree(),
//                            cube_coin)));

//                    }
//                    else
//                    {
//                        m_hole.insert(Tuple.Create((ushort)i, new Hole(new_course,
//                            m_seq[i - 1].m_hole, pin,
//                            Hole.eMODO(m_ri.modo),
//                            (byte)m_ri.hole_repeat,
//                            weather, wind.wind,
//                            wind.degree.getDegree(),
//                            cube_coin)));
//                    }

//                }
//                else
//                {
//                    m_hole.insert(Tuple.Create((ushort)i, new Hole(new_course,
//                        m_seq[i - 1].m_hole,
//                        sRandomGen.getInstance().rIbeMt19937_64_rdevice() % 3,
//                        Hole.eMODO(m_ri.modo),
//                        (byte)m_ri.hole_repeat,
//                        weather, wind.wind,
//                        wind.degree.getDegree(),
//                        cube_coin)));
//                }
//            }

//        }

//        protected void init_dados_rain()
//        {

//            // Inicializa dados de chuva em holes consecutivos
//            m_chr.clear();

//            // Inicializa dados do n�mero de holes com chuva
//            m_holes_rain.clear();

//            uint count = 0u;

//            foreach (var el in m_hole)
//            {

//                // Quantidade de holes que tem o Game
//                if (el.first <= m_ri.qntd_hole)
//                {

//                    if (el.second.getWeather() == 2)
//                    {

//                        // Chuva
//                        m_holes_rain.setRain(el.first - 1, 1u);

//                        count++;
//                    }

//                    // �ltimo hole ou acabou a sequ�ncia de chuva consecutivas
//                    if (count > 1u && (el.second.getWeather() != 2 || el.first == m_ri.qntd_hole))
//                    {

//                        if (count >= 4) // 4 ou mais Holes consecutivos
//                        {
//                            m_chr._4_pluss_count.setRain(el.first - 1, 1u);
//                        }
//                        else if (count == 3) // 3 Holes consecutivos
//                        {
//                            m_chr._3_count.setRain(el.first - 1, 1u);
//                        }
//                        else // 2 Holes consecutivos
//                        {
//                            m_chr._2_count.setRain(el.first - 1, 1u);
//                        }

//                        // Zera
//                        count = 0u;
//                    }
//                }
//            }

//#if DEBUG
//            // teste
//            var c = m_holes_rain.getCountHolesRainBySeq(15);
//            var c2 = m_holes_rain.getCountHolesRainBySeq(18);
//            var c3 = m_holes_rain.getCountHolesRainBySeq(3);
//            var c4 = m_holes_rain.getCountHolesRain();

//            _smp.message_pool.getInstance().push(new message("[Course::init_dados_rain][Log] Count Holes Rain: SEQ(15)[VALUE=" + Convert.ToString(c) + "], SEQ(18)[VALUE=" + Convert.ToString(c2) + "], SEQ(3)[VALUE=" + Convert.ToString(c3) + "] e SEQ(1~18)[VALUE=" + Convert.ToString(c4) + "]", CL_FILE_LOG_AND_CONSOLE));

//            var cc2 = m_chr._2_count.getCountHolesRainBySeq(15);
//            var cc3 = m_chr._3_count.getCountHolesRainBySeq(15);
//            var cc4 = m_chr._4_pluss_count.getCountHolesRainBySeq(15);

//            var cd2 = m_chr._2_count.getCountHolesRainBySeq(18);
//            var cd3 = m_chr._3_count.getCountHolesRainBySeq(18);
//            var cd4 = m_chr._4_pluss_count.getCountHolesRainBySeq(18);

//            var ce2 = m_chr._2_count.getCountHolesRainBySeq(3);
//            var ce3 = m_chr._3_count.getCountHolesRainBySeq(3);
//            var ce4 = m_chr._4_pluss_count.getCountHolesRainBySeq(3);

//            var cf2 = m_chr._2_count.getCountHolesRain();
//            var cf3 = m_chr._3_count.getCountHolesRain();
//            var cf4 = m_chr._4_pluss_count.getCountHolesRain();

//            _smp.message_pool.getInstance().push(new message("[Course::init_dados_rain][Log] Count Consecutives Holes Rain: SEQ(15)[C2=" + Convert.ToString(cc2) + ", C3=" + Convert.ToString(cc3) + ", C4=" + Convert.ToString(cc4) + "], SEQ(18)[C2=" + Convert.ToString(cd2) + ", C3=" + Convert.ToString(cd3) + ", C4=" + Convert.ToString(cd4) + "], SEQ(3)[C2=" + Convert.ToString(ce2) + ", C3=" + Convert.ToString(ce3) + ", C4=" + Convert.ToString(ce4) + "] e SEQ(1~18)[C2=" + Convert.ToString(cf2) + ", C3=" + Convert.ToString(cf3) + ", C4=" + Convert.ToString(cf4) + "]", CL_FILE_LOG_AND_CONSOLE));
//#endif // _DEBUG


//            //m_holes_rain = std::count_if(m_hole.begin(), m_hole.end(), [&](auto& el) {
//            //	return (el.first <= m_ri.qntd_hole/*Quantidade de holes que tem o Game*/ && el.second.getWeather() == 2);
//            //});
//        }

//        protected SortedDictionary<ushort, Hole> m_hole = new SortedDictionary<ushort, Hole>();
//        protected List<Sequencia> m_seq = new List<Sequencia>();

//        protected byte m_channel_rookie;
//        protected uint m_rate_rain = new uint();
//        protected byte m_rain_persist_flag;

//        protected float m_star;

//        protected ushort[] m_wind_range = new ushort[2];
//        protected ushort m_wind_flag;

//        protected uint m_seed_rand_game = new uint();

//        protected RoomInfoEx m_ri;

//        protected HolesRain m_holes_rain = new HolesRain(); // N�mero de holes que est� chovendo no course
//        protected ConsecutivosHolesRain m_chr = new ConsecutivosHolesRain(); // N�mero de chuva em holes consecutivos, 2, 3 e 4+

//        protected bool m_grand_prix_special_hole; // Flag de special hole Grand Prix, true tem special hole, false n�o tem

//        private ushort m_flag_cube_coin; // 1 Tem Cube e Coin, 0 sem
//    }
//}
