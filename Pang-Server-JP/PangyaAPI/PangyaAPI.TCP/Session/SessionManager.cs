using PangyaAPI.TCP.Util;
using PangyaAPI.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;   
namespace PangyaAPI.TCP.Session
{
    public class SessionManager
    {                                              
        public readonly uint m_max_session;
        public readonly List<SessionBase> m_sessions = new List<SessionBase>();
        private readonly List<SessionBase> m_session_del = new List<SessionBase>();
        private uint _ttl;
        public uint m_count = 0;
        private static bool _isInit = false;
        private readonly IniHandle _configReader;
        private readonly object _lockObject = new object();

        public SessionManager( uint maxSession)
        {                                            
            m_max_session = maxSession;
            _configReader = new IniHandle("server.ini");
            ConfigInit();
            _isInit = true;
        }

        public void ConfigInit()
        {
            _ttl = _configReader.ReadUInt32("OPTION", "TTL", 0);
        }

        public void Clear()
        {
            lock (_lockObject)
            {
                m_session_del.Clear();
                foreach (var session in m_sessions)
                {
                    session?.Dispose();
                }
                m_sessions.Clear();
            }
        }

        public SessionBase AddSession(TcpClient socket, IPEndPoint address, byte key)
        {
            if (socket == null || !socket.Connected)
            {
                throw new InvalidOperationException("[SessionManager::AddSession] _client is invalid.");
            }

            SessionBase session = null;
            lock (_lockObject)
            {
                int index = FindSessionFree();
                if (index == -1)
                {
                    throw new InvalidOperationException("[SessionManager::AddSession] Already reached session limit.");
                }

                session = m_sessions[index];
                session._client = socket;
                session.Address = address;
                session.m_key = key;
                session.m_oid = (uint)index;
                session.m_start_time = Environment.TickCount;
                session.m_tick = Environment.TickCount;

                session.SetState(true);
                session.SetConnected(true);
                m_count++;
            }

            return session;
        }

        public bool DeleteSession(SessionBase session)
        {
            if (session == null)
            {
                throw new ArgumentNullException(nameof(session), "[SessionManager::DeleteSession] session is null.");
            }

            bool ret = true;
            lock (_lockObject)
            {
                if (!session.GetState() && session._client == null)
                {
                    throw new InvalidOperationException("[SessionManager::DeleteSession] SessionBase is not connected.");
                }

                session.Lock();

                if ((ret = session.Clear()))
                {
                    m_count--;
                }

                session.Unlock();
            }

            return ret;
        }

        public uint GetNumSessionOnline()
        {
            uint currOnline;
            lock (_lockObject)
            {
                currOnline = m_count;
            }
            return currOnline;
        }

        public SessionBase FindSessionByOid(uint oid)
        {
            SessionBase session = null;
            lock (_lockObject)
            {
                session = m_sessions.FirstOrDefault(el => el._client != null && el.m_oid == oid);
            }
            return session;
        }

        public SessionBase FindSessionByUid(uint uid)
        {
            SessionBase session = null;
            lock (_lockObject)
            {
                session = m_sessions.FirstOrDefault(el => el._client != null && el.Uid == uid);
            }
            return session;
        }

        public List<SessionBase> FindAllSessionByUid(uint uid)
        {
            List<SessionBase> sessions = new List<SessionBase>();
            lock (_lockObject)
            {
                sessions = m_sessions.Where(el => el._client != null && el.Uid == uid).ToList();
            }
            return sessions;
        }

        public SessionBase FindSessionByNickname(string nickname)
        {
            SessionBase session = null;
            lock (_lockObject)
            {
                session = m_sessions.FirstOrDefault(el => el._client != null && el.Nickname == nickname);
            }
            return session;
        }

        public SessionBase GetSessionToDelete(int timeoutMs)
        {
            SessionBase session = null;
            try
            {
                session = m_session_del.FirstOrDefault(); // Simulate the `get` functionality here with a simple select.
            }
            catch (Exception e)
            {
                if (e is TimeoutException)
                {
                    session = null;
                }
            }
            return session;
        }                 
        
         public void CheckSessionLive()
        {
            foreach (var session in m_sessions)
            {
                if (session._client != null)
                {
                    int connTime = SocketUtils.GetConnectTime(session._client);
                    if (!session.IsCreated())
                    {
                        m_session_del.Add(session);
                    }
                    else if (connTime < 0 || !session.m_is_authorized)
                    {
                        m_session_del.Add(session);
                    }
                    else if (_ttl > 0 && (Environment.TickCount - session.m_tick) > (_ttl / 1000.0))
                    {
                        m_session_del.Add(session);
                    }
                }
            }
        }

        public bool IsFull()
        {
            bool isFull;
            lock (_lockObject)
            {
                isFull = m_sessions.Count(session => session._client != null) == m_sessions.Count;
            }
            return isFull;
        }

        public uint NumSessionConnected()
        {
            return m_count;
        }

        public bool IsInit()
        {
            return _isInit;
        }

        private int FindSessionFree()
        {
            for (int i = 0; i < m_sessions.Count; i++)
            {
                if (m_sessions[i]._client == null)
                {
                    return i;
                }
            }
            return -1;
        }
    }
      
}
