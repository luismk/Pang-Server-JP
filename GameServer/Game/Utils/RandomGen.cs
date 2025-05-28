using System;
using PangyaAPI.Utilities;

namespace Pangya_GameServer.Game.Utils
{
    public class RandomGen : IDisposable
    {
        private bool m_state;
        private Random m_rd;
        private Random m_ibe_mt19937_64;

        public RandomGen()
        {
            this.m_rd = null;
            this.m_ibe_mt19937_64 = null;
            this.m_state = false;
        }

        public bool IsGood()
        {
            return m_state && m_rd != null && m_ibe_mt19937_64 != null;
        }

        private bool Init()
        {
            // Inicialização do random device
            try
            {
                m_rd = new Random();
                m_ibe_mt19937_64 = new Random();
                m_state = true;
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("[RandomGen::Init] Error: " + e.Message);
                return false;
            }
        }

        private ulong _rDevice()
        {
            if (!IsGood() && !Init())
            {
                throw new Exception("[RandomGen::_rDevice] Não conseguiu inicializar.");
            }

            ulong dice = 0;
            try
            {
                dice = (ulong)m_rd.Next();
            }
            catch (Exception e)
            {
                Console.WriteLine("[RandomGen::_rDevice] Error: " + e.Message);
            }

            return dice;
        }

        private ulong _rIbeMt19937_64_chrono()
        {
            if (!IsGood() && !Init())
            {
                throw new Exception("[RandomGen::_rIbeMt19937_64_chrono] Não conseguiu inicializar.");
            }

            ulong dice = 0;
            try
            {
                m_ibe_mt19937_64 = new Random();
                dice = (ulong)m_ibe_mt19937_64.Next();
            }
            catch (Exception e)
            {
                Console.WriteLine("[RandomGen::_rIbeMt19937_64_chrono] Error: " + e.Message);
            }

            return dice;
        }

        private ulong _rIbeMt19937_64_rdevice()
        {
            if (!IsGood() && !Init())
            {
                throw new Exception("[RandomGen::_rIbeMt19937_64_rdevice] Não conseguiu inicializar.");
            }

            ulong dice = 0;
            try
            {
                dice = (ulong)m_rd.Next();
            }
            catch (Exception e)
            {
                Console.WriteLine("[RandomGen::_rIbeMt19937_64_rdevice] Error: " + e.Message);
            }

            return dice;
        }

        private ulong _rDeviceRange(ulong min, ulong max)
        {
            if (!IsGood() && !Init())
            {
                throw new Exception("[RandomGen::_rDeviceRange] Não conseguiu inicializar.");
            }

            ulong dice = 0;
            try
            {
                dice = (ulong)(min + ((ulong)m_rd.Next() % (max - min)));
            }
            catch (Exception e)
            {
                Console.WriteLine("[RandomGen::_rDeviceRange] Error: " + e.Message);
            }

            return dice;
        }
        // Random indenpendent_bits_engine mt19937_64 chrono
        public ulong rIbeMt19937_64_chrono()
        {

            ulong dice = 0Ul;
            bool ok = false;

            try
            {

                dice = _rIbeMt19937_64_chrono();
                ok = true;

            }
            catch (exception e)
            {
            }

            // Não conseguiu com o chronos
            if (!ok)
            {
                return (ulong)dice;
            }

            // Conseguiu com o chronos
            return dice;
        }

        private ulong _rIbeMt19937_64_chronoRange(ulong min, ulong max)
        {
            if (!IsGood() && !Init())
            {
                throw new Exception("[RandomGen::_rIbeMt19937_64_chronoRange] Não conseguiu inicializar.");
            }

            ulong dice = 0;
            try
            {
                dice = (ulong)(min + ((ulong)m_ibe_mt19937_64.Next() % (max - min)));
            }
            catch (Exception e)
            {
                Console.WriteLine("[RandomGen::_rIbeMt19937_64_chronoRange] Error: " + e.Message);
            }

            return dice;
        }

        private ulong _rIbeMt19937_64_rdeviceRange(ulong min, ulong max)
        {
            if (!IsGood() && !Init())
            {
                throw new Exception("[RandomGen::_rIbeMt19937_64_rdeviceRange] Não conseguiu inicializar.");
            }

            ulong dice = 0;
            try
            {
                dice = (ulong)(min + ((ulong)m_rd.Next() % (max - min)));
            }
            catch (Exception e)
            {
                Console.WriteLine("[RandomGen::_rIbeMt19937_64_rdeviceRange] Error: " + e.Message);
            }

            return dice;
        }

        public ulong RDevice()
        {
            ulong dice = 0;
            bool ok = false;

            try
            {
                dice = _rDevice();
                ok = true;
            }
            catch (Exception e)
            {
                Console.WriteLine("[RandomGen::RDevice] Error: " + e.Message);
            }

            if (!ok)
            {
                dice = _rIbeMt19937_64_chrono();
            }

            return dice;
        }

        public ulong RIbeMt19937_64Chrono()
        {
            ulong dice = 0;
            bool ok = false;

            try
            {
                dice = _rIbeMt19937_64_chrono();
                ok = true;
            }
            catch (Exception e)
            {
                Console.WriteLine("[RandomGen::RIbeMt19937_64Chrono] Error: " + e.Message);
            }

            if (!ok)
            {
                dice = _rDevice();
            }

            return dice;
        }

        public ulong RIbeMt19937_64RDevice()
        {
            ulong dice = 0;
            bool ok = false;

            try
            {
                dice = _rIbeMt19937_64_rdevice();
                ok = true;
            }
            catch (Exception e)
            {
                Console.WriteLine("[RandomGen::RIbeMt19937_64RDevice] Error: " + e.Message);
            }

            if (!ok)
            {
                dice = _rIbeMt19937_64_chrono();
            }

            return dice;
        }

        public ulong RDeviceRange(ulong min, ulong max)
        {
            ulong dice = 0;
            bool ok = false;

            try
            {
                dice = _rDeviceRange(min, max);
                ok = true;
            }
            catch (Exception e)
            {
                Console.WriteLine("[RandomGen::RDeviceRange] Error: " + e.Message);
            }

            if (!ok)
            {
                dice = (ulong)(min + ((ulong)new Random().Next() % (max - min)));
            }

            return dice;
        }

        public ulong RIbeMt19937_64ChronoRange(ulong min, ulong max)
        {
            ulong dice = 0;
            bool ok = false;

            try
            {
                dice = _rIbeMt19937_64_chronoRange(min, max);
                ok = true;
            }
            catch (Exception e)
            {
                Console.WriteLine("[RandomGen::RIbeMt19937_64ChronoRange] Error: " + e.Message);
            }

            if (!ok)
            {
                dice = (ulong)(min + ((ulong)new Random().Next() % (max - min)));
            }

            return dice;
        }

        public ulong RIbeMt19937_64RDeviceRange(ulong min, ulong max)
        {
            ulong dice = 0;
            bool ok = false;

            try
            {
                dice = _rIbeMt19937_64_rdeviceRange(min, max);
                ok = true;
            }
            catch (Exception e)
            {
                Console.WriteLine("[RandomGen::RIbeMt19937_64RDeviceRange] Error: " + e.Message);
            }

            if (!ok)
            {
                dice = (ulong)(min + ((ulong)new Random().Next() % (max - min)));
            }

            return dice;
        }

        public void Dispose()
        {
            // Libera recursos se necessário
        }
    }
    public class sRandomGen : Singleton<RandomGen>
    { }
}
