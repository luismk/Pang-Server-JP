using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Pangya_GameServer.Game.Utils;
using Pangya_GameServer.GameType;
using PangyaAPI.IFF.JP.Extensions;
using PangyaAPI.Utilities;
using PangyaAPI.Utilities.BinaryModels;
using PangyaAPI.Utilities.Log;
using static Pangya_GameServer.GameType._Define;
using _smp = PangyaAPI.Utilities.Log;
using uint16_t = System.UInt16;

namespace Pangya_GameServer.Game
{
    public class Course
    {
        public class Sequencia
        {
            public Sequencia(uint _ul = 0u)
            {
                clear();
            }
            public Sequencia(ushort _hole)
            {
                clear();

                m_hole = _hole;
            }
            public Sequencia(byte _course, ushort _hole)
            {
                this.m_course = _course;
                this.m_hole = _hole;
            }
            public void clear()
            {

            }
            public byte m_course;
            public ushort m_hole;
        }

        public Course(RoomInfoEx _ri,
            bool _channel_rookie,
            float _star,
            uint _rate_rain,
            byte _rain_persist_flag)
        {
            this.m_ri = _ri;
            this.m_channel_rookie = _channel_rookie;
            this.m_star = _star;
            this.m_rate_rain = _rate_rain;
            this.m_rain_persist_flag = _rain_persist_flag;
            this.m_hole = new Dictionary<ushort, Hole>();
            this.m_seq = new List<Sequencia>();
            this.m_seed_rand_game = 0;
            this.m_flag_cube_coin = 1;
            this.m_wind_flag = 0;
            this.m_wind_range = new ushort[9];
            this.m_chr = new ConsecutivosHolesRain();
            this.m_holes_rain = new HolesRain();
            this.m_grand_prix_special_hole = false;

            init_seq();

            init_hole();

            init_dados_rain(); // Inicializar os dados de chuva no course, para ser usado no achievement

            // Deixa esse s� com int16(short), por que s� vejo n�mero baixo, n�o passa do valor m�ximo do int16
            m_seed_rand_game = (uint)new Random().Next(1, short.MaxValue);
        }

        ~Course()
        { 
            if (!m_hole.empty())
            {
                m_hole.Clear();
            }

            if (!m_seq.empty())
            {
                m_seq.Clear();
            }
        }

        // Get
        public uint getSeedRandGame()
        {
            return m_seed_rand_game;
        }

        public ushort getFlagCubeCoin()
        {
            return m_flag_cube_coin;
        }

        public float getStar()
        {
            return m_star;
        }

        /// Finders

        // Find Hole, se n�o achar retorna um ponteiro nulo
        public Hole findHole(ushort _number)
        {
            if (_number < 0)
                return null;

            foreach (var it in m_hole)
            {
                if (it.Value.getNumero() == _number)
                    return it.Value;
            }

            return null;
        }

        public Hole findHoleBySeq(ushort _seq)
        {

            if ((short)_seq <= 0 || _seq > m_hole.Count)
            {
                return null;
            }

            var it = m_hole.find(_seq);

            if (it.Key == m_hole.end().Key)
            {
                _smp.message_pool.push(new message("[Course::findHoleBySeq][WARNIG] nao encontrou a seq[value=" + Convert.ToString(_seq) + "] no map de hole. Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));
            }

            return (it.Key != m_hole.end().Key) ? it.Value : null;
        }

        // Find Hole Sequ�ncia
        public ushort findHoleSeq(ushort _number)
        {
            if (_number < 0)
                return ushort.MaxValue; // Erro

            foreach (var it in m_hole)
            {
                if (it.Value.getNumero() == _number)
                    return it.Key;
            }

            return 0; // Não encontrado
        }

        // Find intervalo de hole do n�mero fornecido at� o ultimo do map
        public IEnumerable<KeyValuePair<ushort, Hole>> findRange(ushort _number)
        {
            if (_number >= 0)
                return m_hole.Where(kv => kv.Value.getNumero() == _number);

            return Enumerable.Empty<KeyValuePair<ushort, Hole>>();
        }



        // Random Wind and Degree
        public stHoleWind shuffleWind(uint _seed = 777u)
        {
            stHoleWind wind = new stHoleWind();

            Random rand = new Random((int)_seed);  // Use sempre a mesma seed para consistência

            if (m_wind_flag != 0)
            {
                do
                {
                    wind.wind = (byte)(m_wind_range[0] + rand.Next(m_wind_range[1] - m_wind_range[0] + 1));
                } while (m_wind_flag == 2 ? ((wind.wind + 1) % 2 == 1) : ((wind.wind + 1) % 2 == 0));
            }
            else
            {
                wind.wind = (byte)(m_wind_range[0] + rand.Next(m_wind_range[1] - m_wind_range[0] + 1));
            }

            wind.degree.setDegree((byte)(rand.Next(LIMIT_DEGREE)));

            return wind;
        }


        // Random wind next hole(s)
        public void shuffleWindNextHole(ushort _number)
        {

            if ((short)_number < 0)
            {
                throw new exception("[Course::shuffleWindNextHole][Error] _number[VALUE=" + Convert.ToString((short)_number) + "] is invalid", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.COURSE,
                    1, 0));
            }

            // C++ TO C# CONVERTER TASK: Lambda expressions cannot be assigned to 'var':
            var it = m_hole.FirstOrDefault(_el =>
            {
                return _el.Value.getNumero() == _number;
            });

            if (it.Key == m_hole.end().Key)
            {
                throw new exception("[Course::shuffleWindNextHole][Error] nao conseguiu encontrar o hole[NUMERO=" + Convert.ToString(_number) + "]", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.COURSE,
                    2, 0));
            }

            var wind = shuffleWind();

            foreach (var _it in m_hole)
            {
                _it.Value.setWind(wind);
            }
        }

        // Make Packet Buffer Hole(s) Info
        public void makePacketHoleInfo(PangyaBinaryWriter _p, int _option = 0)
        {

            // Hole(s) Info
            foreach (var el in m_hole)//nao tem esses dados@@@
            {
                _p.WriteUInt32(el.Value.getId());
                _p.WriteByte(el.Value.getPin());

                if (_option == 0)
                    _p.WriteByte((int)el.Value.getCourse());

                _p.WriteByte((byte)el.Value.getNumero());
            }
            // Course Seed Random
            _p.WriteUInt32(m_seed_rand_game);

            // Hole(s) Spinning Cube / Coin Info
              makePacketHoleSpinningCubeInfo(_p);
        }

        // Make Packet Buffer Hole(s) Spinning Cube(s) Info
        public void makePacketHoleSpinningCubeInfo(PangyaBinaryWriter _p)
        {

            foreach (var el in m_hole)
            {

                _p.WriteByte((byte)el.Value.getCubes().Count); // Size

                foreach (var el2 in el.Value.getCubes())
                {
                    _p.WriteUInt32((uint)el2.tipo);
                    _p.WriteUInt32(el2.id);
                    _p.WriteUInt32(el2.flag_unknown);
                    _p.WriteUInt32(el.Value.getCourse());
                    _p.WriteByte((byte)(el.Value.getModo() == Hole.eMODO.M_REPEAT ? el.Value.getHoleRepeat() : el.Value.getNumero()));
                    _p.WriteByte(el.Key - 1); // Index
                    _p.WriteUInt16(m_flag_cube_coin);
                    _p.Write(el2.location.x);//float
                    _p.Write(el2.location.y);//float
                    _p.Write(el2.location.z);//float
                    _p.WriteUInt32((uint)el2.flag_location);
                }
            }
        }

        public uint countHolesRain()
        {
            return m_holes_rain.getCountHolesRain();
        }

        public uint countHolesRainBySeq(uint _seq)
        {
            return m_holes_rain.getCountHolesRainBySeq(_seq);
        }

        // retorna Media de tacadas do course para fazer par em todos os holes
        public float getMediaAllParHoles()
        {

            if (m_hole.empty())
            {
                return 1.0f; // N�o tem nenhum hole inicializado
            }

            int count = 0;

            foreach (var el in m_hole)
            {
                count += el.Value.getPar().par;
            }

            return (float)(count / (float)m_hole.Count);
        }

        public float getMediaAllParHolesBySeq(uint _seq)
        {

            if (_seq <= 0 || _seq > m_hole.Count)
            {
                return 1.0f; // Sequ�ncia inv�lida
            }

            if (m_hole.empty())
            {
                return 1.0f; // N�o tem nenhum hole inicializado
            }

            int count = 0;

            foreach (var it in m_hole)
            {
                if (it.Key > _seq)
                    break;

                count += it.Value.getPar().par;
            }

            return (float)(count / (float)_seq);
        }

        public ConsecutivosHolesRain getConsecutivesHolesRain()
        {
            return m_chr;
        }

        protected void init_seq()
        {

            // Verifica se � Grand Prix e se tem Special Hole
            if (m_ri.grand_prix.active == 1 && m_ri.grand_prix.dados_typeid > 0)
            {

                // Grand Prix Special Hole
                var sh = sIff.getInstance().findGrandPrixSpecialHole(m_ri.grand_prix.rank_typeid);

                if (!sh.empty())
                {

                    // Sort Sequencie
                    // Ordena do menor para o maior por sequ�ncia do hole
                    sh.Sort((_1, _2) => (int)Tools.IfCompare<uint>(_1.Hole > _2.Hole, _1.Hole, _2.Hole));


                    foreach (var el in sh)
                    {
                        m_seq.Add(new Sequencia((byte)el.Map, (ushort)el.Hole));
                    }

                    // Completa os 18 Holes
                    if (m_seq.Count < 18)
                    {

                        for (var i = m_seq.Count + 1; i <= 18; ++i)
                        {
                            m_seq.Add(new Sequencia((byte)m_ri.course, (ushort)i));
                        }
                    }

                    // Tem Special Hole
                    m_grand_prix_special_hole = true;

                    // Sai da fun��o por que os holes speciais do Prand Prix
                    return;
                }
            }

            // Normal
            switch ((Hole.eMODO)m_ri.modo)
            {
                case Hole.eMODO.M_FRONT:
                case Hole.eMODO.M_REPEAT:
                    for (var i = 1; i <= 18; ++i)
                    {
                        m_seq.Add(new Sequencia((ushort)i));
                    }
                    break;
                case Hole.eMODO.M_BACK:
                    {
                        var i = 10;
                        for (; i <= 18; ++i)
                        {
                            m_seq.Add(new Sequencia((ushort)i));
                        }
                        for (i = 1; i < 10; ++i)
                        {
                            m_seq.Add(new Sequencia((ushort)i));
                        }
                        break;
                    }
                case Hole.eMODO.M_RANDOM:
                    {
                        ushort rand = (ushort)((new Random().Next(1, 17)) + 1);
                        for (var i = rand; i <= 18; ++i)
                        {
                            m_seq.Add(new Sequencia(i));
                        }
                        if (rand > 1)
                        {
                            for (var i = 1; i < rand; ++i)
                            {
                                m_seq.Add(new Sequencia((ushort)i));
                            }
                        }
                        break;
                    }
                case Hole.eMODO.M_SHUFFLE:
                    {
                        Lottery lottery = new Lottery();

                        for (var i = 1; i <= 18; ++i)
                        {
                            lottery.Push(1000, i);
                        }

                        Lottery.LotteryCtx lc = null;

                        for (var i = 0; i < 18; ++i)
                        {
                            if ((lc = lottery.SpinRoleta(true)) != null)
                            {
                                m_seq.Add(new Sequencia(Convert.ToUInt16(lc.Value)));
                            }
                        }

                        break;
                    }
                case Hole.eMODO.M_SHUFFLE_COURSE:
                    {

                        ushort hole_ssc = (ushort)((sRandomGen.getInstance().rIbeMt19937_64_chrono() % 2) + 1);

                        Lottery lottery = new Lottery();

                        for (var i = 1; i <= 18; ++i)
                        {
                            lottery.Push(1000, i);
                        }

                        Lottery.LotteryCtx lc = null;

                        // 17 Holes S�
                        for (var i = 0; i < 18; ++i)
                        {
                            if ((lc = lottery.SpinRoleta(true)) != null && (uint16_t)lc.Value != hole_ssc)
                            {
                                m_seq.Add(new Sequencia(Convert.ToUInt16(lc.Value)));
                            }
                        }

                        // ultimo Hole � do SSC
                        m_seq.Add(new Sequencia((ushort)hole_ssc));

                        break;
                    } // End Case M_SHUFFLE_COURSE
            } // End Switch

        }

        protected void init_hole()
        {
            try
            {

                uCubeCoinFlag cube_coin = new uCubeCoinFlag();

                // Enable Coin e Cube in Course Default
                cube_coin.enable = 1;
                cube_coin.enable_coin = 1;

                // Type Cube Game Mode
                if (m_ri.modo == (byte)Hole.eMODO.M_REPEAT)
                {
                    cube_coin.type = 1;
                }
                else if (m_ri.getTipo() == RoomInfo.TIPO.STROKE || m_ri.getTipo() == RoomInfo.TIPO.TOURNEY || m_ri.getTipo() == RoomInfo.TIPO.GUILD_BATTLE || m_ri.getTipo() == RoomInfo.TIPO.MATCH || m_ri.getTipo() == RoomInfo.TIPO.PRACTICE || m_ri.getTipo() == RoomInfo.TIPO.SPECIAL_SHUFFLE_COURSE)
                {
                    cube_coin.type = 2;
                }

                switch (m_ri.artefato)
                {
                    case ORCHID_BLOSSOM_ART: // 1 a 8m
                        m_wind_range[1] = 8;
                        break;
                    case PENNE_ABACUS_ART: // Wind Impar
                        m_wind_flag = 1;
                        break;
                    case TITAN_WINDMILL_ART: // Wind Par
                        m_wind_flag = 2;
                        break;
                }

                if (m_ri.grand_prix.active == 1 && m_ri.grand_prix.dados_typeid > 0)
                {

                    // Grand Prix n�o tem cube
                    cube_coin.enable = 0;

                    try
                    {

                        var gp = sIff.getInstance().findGrandPrixData(m_ri.grand_prix.dados_typeid);

                        // Grand Prix Data -> Rule
                        if (gp != null)
                        {

                            // Aqui inicializa as regras do Grand Prix de vento
                            switch (gp.rule)
                            {
                                case ONLY_1M_RULE:
                                    m_wind_range[1] = 1;
                                    break;
                                case SUPER_WIND_RULE:
                                    m_wind_range[0] = 9;
                                    m_wind_range[1] = 15;
                                    break;
                                case HOLE_CUP_MAGNET_RULE: // Ainda n�o sei esses aqui, como funciona
                                case NO_TURNING_BACK_RULE: // Ainda n�o sei esses aqui, como funciona
                                    break;
                                case WIND_3M_A_5M_RULE:
                                    m_wind_range[0] = 2;
                                    m_wind_range[1] = 5;
                                    break;
                                case WIND_7M_A_9M_RULE:
                                    m_wind_range[0] = 6;
                                    break;
                            }

                        }
                        else
                        {
                            _smp.message_pool.push(new message("[Course::init_hole][Error] tentou pegar o Grand Prix[TYPEID=" + Convert.ToString(m_ri.grand_prix.dados_typeid) + "] no IFF_STRUCT do server mais ele nao existe. Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));
                        }

                    }
                    catch (exception e)
                    {

                        _smp.message_pool.push(new message("[Course::init_hole][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
                    }

                }
                else if (m_channel_rookie)
                {
                    m_wind_range[1] = 5;
                }

                byte new_course = (byte)((byte)m_ri.course & 0x7F);
                byte pin = 0;
                byte weather = 0;

                byte persist_rain = 0;

                stHoleWind wind = new stHoleWind();

                // Lottery Wind
                Lottery loterry = new Lottery();

                var rate_good_weather = (m_rate_rain <= 0) ? 1000 : ((m_rate_rain < 1000) ? 1000 - m_rate_rain : 1);

                // Coloquei 4 pra 1, antes estava 3 pra 1
                loterry.Push(rate_good_weather, 0);
                loterry.Push(rate_good_weather, 0);
                loterry.Push(rate_good_weather, 0);
                loterry.Push(rate_good_weather, 0);
                loterry.Push(m_rate_rain, 2);

                // Lottery Course
                Lottery lottery_map = new Lottery();

                byte course_id = 0;

                foreach (var el in sIff.getInstance().getCourse())
                {

                    course_id = (byte)sIff.getInstance().getItemIdentify(el.ID);

                    if (course_id != 17 && course_id != 0x40)
                    {
                        lottery_map.Push(100, course_id);
                    }
                }

                for (uint i = 1; i <= 18; ++i)
                {

                    // Reseta flag cube
                    cube_coin.enable_cube = 0;
                    cube_coin.enable_coin = 0;

                    if (i <= m_ri.qntd_hole)
                    {

                        if (m_ri.modo == (int)Hole.eMODO.M_REPEAT && i == 1)
                        {
                            wind = shuffleWind(i);
                        }
                        else if (m_ri.modo != (int)Hole.eMODO.M_REPEAT)
                        {
                            wind = shuffleWind(i);
                        }

                        if (m_ri.fixed_hole == 7 && i == 1)
                        {
                            pin = (byte)(new Random().Next() % 3);
                        }
                        else if (m_ri.fixed_hole != 7)
                        {
                            pin = (byte)(new Random().Next() % 3);
                        }

                        weather = 0;

                        var lc = loterry.SpinRoleta();

                        if (lc?.Value != null && Convert.ToInt32(lc.Value) != 0)
                        {
                            weather = Convert.ToByte(lc.Value);
                        }

                        if (persist_rain != 0 || weather == 2)
                        {

                            if (persist_rain == 0
                                && weather == 2
                                && m_rain_persist_flag == 1)
                            {
                                persist_rain = 1;
                            }
                            else if (persist_rain == 1)
                            {
                                weather = 2;
                                persist_rain = 0;
                            }

                            try
                            {
                                if (i > 1 && m_hole[(uint16_t)(i - 1)].getWeather() == 0)
                                {
                                    m_hole[(uint16_t)(i - 1)].setWeather(1);
                                }

                            }
                            catch (IndexOutOfRangeException e)
                            {
                                Console.WriteLine(e.Message);
                            }
                        }

                        if (m_ri.getTipo() == RoomInfo.TIPO.SPECIAL_SHUFFLE_COURSE && m_ri.modo == (byte)Hole.eMODO.M_SHUFFLE_COURSE)
                        {

                            if (i == 18) // Ultimo Hole � do SSC
                            {
                                new_course = (byte)RoomInfo.eCOURSE.CHRONICLE_1_CHAOS;
                            }
                            else
                            {

                                lc = lottery_map.SpinRoleta();

                                if (lc != null)
                                {
                                    new_course = (byte)lc.Value;
                                }
                            }
                        }

                        // Cube a cada 3 hole
                        if (i % 3 == 0)
                        {
                            cube_coin.enable_cube = 1;
                        }

                        // Coin todos os holes
                        if (cube_coin.enable == 1)
                        {
                            cube_coin.enable_coin = 1;
                        }

                        if (m_ri.grand_prix.active == 1
                            && m_ri.grand_prix.dados_typeid > 0
                            && m_grand_prix_special_hole)
                        {

                            // A fun��o init_seq j� inicializa a sequ�ncia se for Grand Prix e se ele tiver Special Hole
                            m_hole.insert(Tuple.Create((ushort)i, new Hole(m_seq[(uint16_t)(i - 1)].m_course,
                                m_seq[(uint16_t)(i - 1)].m_hole, pin,
                               (Hole.eMODO)(m_ri.modo),
                                (byte)m_ri.hole_repeat,
                                weather, wind.wind,
                                wind.degree.getDegree(),
                                cube_coin)));

                        }
                        else
                        {
                            m_hole.insert(Tuple.Create((ushort)i, new Hole(new_course,
                                m_seq[(uint16_t)(i - 1)].m_hole, pin,
                               (Hole.eMODO)(m_ri.modo),
                                (byte)m_ri.hole_repeat,
                                weather, wind.wind,
                                wind.degree.getDegree(),
                                cube_coin)));
                        }

                    }
                    else
                    {
                        m_hole.insert(Tuple.Create((ushort)i, new Hole(new_course,
                            m_seq[(uint16_t)(i - 1)].m_hole,
                            (byte)(new Random().Next() % 3),
                            (Hole.eMODO)(m_ri.modo),
                            (byte)m_ri.hole_repeat,
                            weather, wind.wind,
                            wind.degree.getDegree(),
                            cube_coin)));
                    }
                }
            }
            catch (Exception e)
            { 
                throw e;
            }

        }

        protected void init_dados_rain()
        {
            try
            {
                // Inicializa dados de chuva em holes consecutivos
                m_chr.clear();

                // Inicializa dados do n�mero de holes com chuva
                m_holes_rain.clear();

                uint count = 0;

                foreach (var el in m_hole)
                {

                    // Quantidade de holes que tem o Game
                    if (el.Key <= m_ri.qntd_hole)
                    {

                        if (el.Value.getWeather() == 2)
                        {

                            // Chuva
                            m_holes_rain.setRain((uint)(el.Key - 1), 1);

                            count++;
                        }

                        // �ltimo hole ou acabou a sequ�ncia de chuva consecutivas
                        if (count > 1u && (el.Value.getWeather() != 2 || el.Key == m_ri.qntd_hole))
                        {

                            if (count >= 4) // 4 ou mais Holes consecutivos
                            {
                                m_chr._4_pluss_count.setRain((uint)(el.Key - 1), 1);
                            }
                            else if (count == 3) // 3 Holes consecutivos
                            {
                                m_chr._3_count.setRain((uint)(el.Key - 1), 1);
                            }
                            else // 2 Holes consecutivos
                            {
                                m_chr._2_count.setRain((uint)(el.Key - 1), 1);
                            }

                            // Zera
                            count = 0;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                throw e;
            } 
        }

        protected Dictionary<ushort, Hole> m_hole = new Dictionary<ushort, Hole>();
        protected List<Sequencia> m_seq = new List<Sequencia>();

        protected bool m_channel_rookie;
        protected uint m_rate_rain = new uint();
        protected byte m_rain_persist_flag;

        protected float m_star;

        protected ushort[] m_wind_range = new ushort[2];
        protected ushort m_wind_flag;

        protected uint m_seed_rand_game = new uint();

        protected RoomInfoEx m_ri;

        protected HolesRain m_holes_rain = new HolesRain(); // N�mero de holes que est� chovendo no course
        protected ConsecutivosHolesRain m_chr = new ConsecutivosHolesRain(); // N�mero de chuva em holes consecutivos, 2, 3 e 4+

        protected bool m_grand_prix_special_hole; // Flag de special hole Grand Prix, true tem special hole, false n�o tem

        private ushort m_flag_cube_coin; // 1 Tem Cube e Coin, 0 sem
    }
}
