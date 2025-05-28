using System;
namespace Pangya_GameServer.GameType
{
    public class stRangeTime : System.IDisposable
    {
        public stRangeTime(uint _ul = 0)
        {
            this.m_channel_id = 255;
            this.m_sended_message = false;
        }
        public stRangeTime(ushort _hour_start,
            ushort _min_start,
            ushort _sec_start,
            ushort _hour_end,
            ushort _min_end,
            ushort _sec_end,
            byte _channel_id)
        {
            this.m_start = new PangyaTime(0,
                0, 0, 0, _hour_start,
                _min_start, _sec_start, 0);
            this.m_end = new PangyaTime(0,
                0, 0, 0, _hour_end,
                _min_end, _sec_end, 0);
            this.m_channel_id = _channel_id;
            this.m_sended_message = false;
        }
        public stRangeTime(PangyaTime _start,
            PangyaTime _end,
            byte _channel_id)
        {
            this.m_start = _start;
            this.m_end = _end;
            this.m_channel_id = _channel_id;
            this.m_sended_message = false;
        }
        public virtual void Dispose()
        {
            clear();
        }
        public void clear()
        {
            m_start = new PangyaTime();
            m_end = new PangyaTime();
            m_channel_id = 255;

            m_sended_message = false;
        }

        public bool isBetweenTime(PangyaTime _st)
        {

            // Garante que os dados da data est�o zerados, gera eles por preven��o
            _st.Day = 0;
            _st.DayOfWeek = 0;
            _st.Month = 0;
            _st.Year = 0;

            return intoStartTime(_st) && intoEndTime(_st);
        }

        public bool isBetweenTime(ushort _hour,
            ushort _min, ushort _sec,
            ushort _milli = 0)
        {

            PangyaTime st = new PangyaTime(0,
                0, 0, 0, _hour, _min, _sec,
                _milli);

            return isBetweenTime(st);
        }

        public uint getDiffInterval()
        {
            return timeToMilliseconds(m_end) - timeToMilliseconds(m_start);
        }

        protected bool intoStartTime(PangyaTime _st)
        {
            return timeToMilliseconds(m_start) <= timeToMilliseconds(_st);
        }

        protected bool intoEndTime(PangyaTime _st)
        {
            return timeToMilliseconds(_st) < timeToMilliseconds(m_end);
        }

        protected uint timeToMilliseconds(PangyaTime _st)
        {
            return (uint)((_st.Hour * 60 * 60 * 1000) + (_st.Minute * 60 * 1000) + (_st.Second * 1000) + _st.MilliSecond);
        }

        public PangyaTime m_start = new PangyaTime();
        public PangyaTime m_end = new PangyaTime();
        public byte m_channel_id;

        public bool m_sended_message; // Flag que guarda se o intervalo j� enviou a mensagem
    }

    // Reward
    public class stReward
    {
        public stReward(uint _ul = 0)
        {
            this._typeid = 0;
            this.qntd = 0;
            this.qntd_time = 0;
            this.rate = 100;
        }
        public stReward(uint __typeid,
            uint _qntd,
            uint _qntd_time,
            uint _rate = 100)
        {
            this._typeid = __typeid;
            this.qntd = _qntd;
            this.qntd_time = _qntd_time;
            this.rate = _rate;
        }

        public void clear()
        {
        }

        public string toString()
        {
            return "TYPEID=" + Convert.ToString(_typeid) + ", QNTD=" + Convert.ToString(qntd) + ", QNTD_TIME=" + Convert.ToString(qntd_time) + ", RATE=" + Convert.ToString(rate);
        }

        public uint _typeid = new uint();
        public uint qntd = new uint();
        public uint qntd_time = new uint();
        public uint rate = new uint();
    }
}
