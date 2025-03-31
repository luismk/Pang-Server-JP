//using System.Collections.Generic;

//namespace GameServer.Game
//{
//    public class Hole
//    {
//        public enum eMODO : byte
//        {
//            M_FRONT,
//            M_BACK,
//            M_RANDOM,
//            M_SHUFFLE,
//            M_REPEAT,
//            M_SHUFFLE_COURSE
//        }

//        public Hole(byte _course, ushort _numero, byte _pin, eMODO _modo, byte _hole_repeat, byte _weather, byte _wind, ushort _degree, uCubeCoinFlag _cube_coin)
//        {
//            m_course = _course;
//            m_numero = _numero;
//            m_pin = _pin;
//            m_modo = _modo;
//            m_hole_repeat = _hole_repeat;
//            m_weather = _weather;
//            m_wind = new stHoleWind(_wind, _degree);
//            m_cube_coin = _cube_coin;
//            init_cube_coin();
//            init_from_IFF_STRUCT();
//        }

//        public void init(stXZLocation _tee, stXZLocation _pin)
//        {
//            m_tee_location = new Location(_tee);
//            m_pin_location = new Location(_pin);
//        }

//        public void init(Location _tee, Location _pin)
//        {
//            m_tee_location = _tee;
//            m_pin_location = _pin;
//        }

//        public bool isGood() => m_good;

//        // Getters
//        public uint getId() => m_id;
//        public ushort getNumero() => m_numero;
//        public byte getTipo() => m_tipo;
//        public ref stHoleWind getWind() => ref m_wind;
//        public ref stHolePar getPar() => ref m_par;
//        public byte getPin() => m_pin;
//        public byte getWeather() => m_weather;
//        public byte getCourse() => m_course;
//        public ref uCubeCoinFlag getCubeCoin() => ref m_cube_coin;
//        public eMODO getModo() => m_modo;
//        public byte getHoleRepeat() => m_hole_repeat;
//        public Location getPinLocation() => m_pin_location;
//        public Location getTeeLocation() => m_tee_location;
//        public List<CubeEx> getCubes() => m_cube;

//        // Setters
//        public void setWeather(byte _weather) => m_weather = _weather;
//        public void setWind(byte _wind, ushort _degree) => m_wind = new stHoleWind(_wind, _degree);
//        public void setWind(stHoleWind _wind) => m_wind = _wind;

//        // Finders
//        public CubeEx findCubeCoin(uint _id) => m_cube.Find(c => c.Id == _id);

//        // Internal Methods
//        protected void init_cube_coin() { /* Implementation */ }
//        protected void init_from_IFF_STRUCT() { /* Implementation */ }

//        // Fields
//        protected Location m_pin_location;
//        protected Location m_tee_location;
//        protected List<CubeEx> m_cube = new();

//        protected uint m_id;
//        protected ushort m_numero;
//        protected byte m_tipo;
//        protected stHoleWind m_wind;
//        protected stHolePar m_par;
//        protected byte m_pin;
//        protected byte m_weather;
//        protected byte m_course;
//        protected uCubeCoinFlag m_cube_coin;
//        protected eMODO m_modo;
//        protected byte m_hole_repeat;
//        protected bool m_good;
//    }
//}