using System;
using System.Collections.Generic;
using System.Linq;

namespace Pangya_GameServer.Game.Utils
{
    public class BroadcastList
    {
        public enum RET_TYPE : byte
        {
            NO_NOTICE, // Não tem notice na lista
            OK,
            WAIT
        }

        public enum TYPE : byte
        {
            GM_NOTICE,
            CUBE_WIN_RARE,
            TICKER
        }

        public class NoticeCtx
        {

            // NoticeCtx
            public NoticeCtx(uint _ul = 0u)
            {
                clear();
            }

            public NoticeCtx(uint _time_second,
                string _notice, TYPE _type)
            {
                this.nickname = "";
                this.option = 0u;

                type = _type;
                time_second = _time_second;

                notice = _notice;
            }

            public NoticeCtx(uint _time_second,
                string _notice,
                uint _option, TYPE _type)
            {
                this.nickname = "";

                type = _type;
                option = _option;
                time_second = _time_second;
                notice = _notice;
            }

            public NoticeCtx(uint _time_second,
                string _nickname,
                string _notice, TYPE _type)
            {
                this.option = 0u;

                type = _type;
                time_second = _time_second;

                nickname = _nickname;
                notice = _notice;
            }

            public void clear()
            {

                type = TYPE.GM_NOTICE;
                time_second = 0u;

                if (notice.Length != 0)
                {
                    notice = "";
                }
            }

            public TYPE type;
            public uint time_second = new uint();
            public uint option = new uint();
            public string nickname = "";
            public string notice = "";
        }

        public class RetNoticeCtx
        {

            // RetNotice
            public RetNoticeCtx(uint _ul = 0u)
            {
                clear();
            }

            public void clear()
            {

                ret = RET_TYPE.NO_NOTICE;

                nc.clear();
            }

            public RET_TYPE ret;
            public NoticeCtx nc = new NoticeCtx();
        }

        public BroadcastList(uint _interval_time_second)
        {
            this.m_interval = _interval_time_second;
        }

        public void push_back(uint _time,
            string _notice, TYPE _type)
        {

            NoticeCtx nc = new NoticeCtx((uint)((_time < 0) ? 0 : _time),
                _notice, _type);
            push_back(nc);
        }

        public void push_back(uint _time,
            string _notice,
            uint _option, TYPE _type)
        {

            NoticeCtx nc = new NoticeCtx((uint)((_time < 0) ? 0 : _time),
                _notice, _option, _type);
            push_back(nc);
        }

        public void push_back(uint _time,
            string _nickname,
            string _notice, TYPE _type)
        {

            NoticeCtx nc = new NoticeCtx((uint)((_time < 0) ? 0 : _time),
                _nickname, _notice, _type);
            push_back(nc);
        }

        public void push_back(NoticeCtx _nc)
        {
            m_list.Add(_nc.time_second, _nc);
        }

        // C++ TO C# CONVERTER TASK: The memory management function 'peek' has no equivalent in C#:
        public RetNoticeCtx peek()
        {

            RetNoticeCtx rnc = new RetNoticeCtx();

            if (!m_list.Any())
            {

                if ((DateTimeOffset.UtcNow.ToUnixTimeSeconds() - m_last_peek) >= m_interval)
                {

                    if (m_list.First().Value.time_second == 0u || m_list.First().Value.time_second <= DateTimeOffset.UtcNow.ToUnixTimeSeconds())
                    {

                        // Peek new Notice
                        rnc.nc = m_list.First().Value;

                        // Remove o primeiro da lista
                        m_list.Remove(m_list.First().Key);

                        rnc.ret = RET_TYPE.OK;

                        m_last_peek = (uint)DateTimeOffset.UtcNow.ToUnixTimeSeconds();

                    }
                    else // Ainda n�o deu o tempo de mostrar o notice no tempo dele
                    {
                        rnc.ret = RET_TYPE.WAIT;
                    }

                }
                else // Ainda n�o deu o intervalo para pegar a pr�xima notice da fila
                {
                    rnc.ret = RET_TYPE.WAIT;
                }

            }
            else // A List est� vazia
            {
                rnc.ret = RET_TYPE.NO_NOTICE;
            }

            return rnc;
        }

        public uint getSize()
        {
            var count = m_list.Count();
            return (uint)count;
        }

        protected Dictionary<uint, NoticeCtx> m_list = new Dictionary<uint, NoticeCtx>();
        protected uint m_last_peek = new uint();

        // Interval time to peek next Notice
        protected uint m_interval = new uint();

    }
}
