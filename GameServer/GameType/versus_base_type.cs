using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pangya_GameServer.GameType
{
    public enum STATE_VERSUS : byte
    {
        WAIT_HIT_SHOT,
        SHOTING,
        END_SHOT,
        LOAD_HOLE,
        WAIT_END_GAME,
    }

    public class stStateVersus
    {

        private object m_cs;
        private STATE_VERSUS m_state;

        public stStateVersus()
        {
            m_state = STATE_VERSUS.WAIT_HIT_SHOT;
            m_cs = new object();
        }

        ~stStateVersus()
        {
            m_state = STATE_VERSUS.WAIT_HIT_SHOT;
            m_cs = null;
        }

        public void @lock()
        {
            Monitor.Enter(m_cs);
        }

        public void unlock()
        {
            Monitor.Exit(m_cs);
        }

        public STATE_VERSUS getState()
        {
            return m_state;
        }

        public void setState(STATE_VERSUS state)
        {
            m_state = state;
        }

        public void setStateWithLock(STATE_VERSUS _state)
        {
            @lock();

            m_state = _state;

            unlock();
        }
    }
}
