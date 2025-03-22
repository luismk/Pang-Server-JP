using PangyaAPI.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using PangyaAPI.Network.PangyaServer;
using System.Threading;

namespace PangyaAPI.Network.PangyaSession
{
    public class SessionManager
    {
        public readonly uint m_max_session;
        public readonly List<SessionBase> m_sessions = new List<SessionBase>();
        private readonly List<SessionBase> m_session_del = new List<SessionBase>();
        private uint _ttl;
        public uint m_count = 0;
        private static bool _isInit = false;
        private readonly IniHandle m_reader_ini;
        public readonly object _lockObject = new object();

        public SessionManager(uint maxSession)
        {
            m_max_session = maxSession;
            m_reader_ini = new IniHandle("server.ini");
            loadini();
            _isInit = true;
        }

        public void loadini()
        {
            _ttl = m_reader_ini.ReadUInt32("OPTION", "TTL", 0);
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

        public SessionBase AddSession(Server _server,TcpClient socket, IPEndPoint address, byte key)
        {
            if (socket == null || !socket.Connected)
            {
                throw new InvalidOperationException("[SessionManager::AddSession] _client is invalid.");
            }

            SessionBase session = null;
            lock (_lockObject)
            {
                uint index = findSessionFree();
                if (index == uint.MaxValue)
                {
                    throw new InvalidOperationException("[SessionManager::AddSession] Already reached session limit.");
                }
                //socket.ReceiveTimeout = 60000; // 20 segundos
                //socket.SendTimeout = 60000;
                session = m_sessions[(int)index];
                session._client = socket;
                session.Server = _server;// server! 
                session.Address = address;
                session.m_key = key;
                session.m_oid = index;
                session.m_start_time = Environment.TickCount;
                session.m_tick = Environment.TickCount;

                session.SetState(true);

                m_count++;
            }

            return session;
        }

        public virtual bool DeleteSession(SessionBase session)
        {
            if (session == null)
            {
                throw new ArgumentNullException(nameof(session), "[SessionManager::DeleteSession] session is null.");
            }

            bool ret = true;
            lock (_lockObject)
            {
                if (!session.getState() && session._client == null)
                {
                    throw new InvalidOperationException("[SessionManager::DeleteSession] SessionBase is not connected.");
                }

                session.Lock();
                                                
                if (ret = session.Clear())
                {                                            
                    m_count--; 
                }
                session.Unlock();
            }

            return ret;
        }


        public List<SessionBase> findAllGM()
        {
            List<SessionBase> v_gm = new List<SessionBase>();

            foreach (SessionBase el in m_sessions)
            {
                if ((el.getCapability() & 4) != 0 || (el.getCapability() & 128 ) != 0)    // GM
                    v_gm.Add(el);
            }

            return v_gm;
        }
        public virtual SessionBase FindSessionByOid(uint oid)
        {
            SessionBase session = null;
            lock (_lockObject)
            {
                foreach (var el in m_sessions.Where(el=> el._client != null))
                {
                    if (el.m_oid == oid)
                        session = el;    
                }
            }
            return session;
        }

        public virtual SessionBase findSessionByUID(uint uid)
        {
            SessionBase session = null;
            lock (_lockObject)
            {
                session = m_sessions.FirstOrDefault(el => el._client != null && el.getUID() == uid);
            }
            return session;
        }

        public virtual List<SessionBase> FindAllSessionByUid(uint uid)
        {
            List<SessionBase> sessions = new List<SessionBase>();
            lock (_lockObject)
            {
                sessions = m_sessions.Where(el => el._client != null && el.getUID() == uid).ToList();
            }
            return sessions;
        }

        public virtual SessionBase FindSessionByNickname(string nickname)
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
            if (session != null && m_session_del.Remove(session))
                return session;
            else
                return null;
        }

        public void CheckSessionLive()
        {
            foreach (var session in m_sessions)
            {
                if (session._client != null)
                {
                    int connTime = session._client.GetConnectTime();
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
            uint currOnline;
            lock (_lockObject)
            {
                currOnline = m_count;
            }
            return currOnline;
        }

        public bool IsInit()
        {
            return _isInit;
        }

        public virtual uint findSessionFree()
        {           
            uint i = 0;
            foreach (var _session in m_sessions)
            {
                if (_session.m_oid == uint.MaxValue)
                {
                    return i;
                }
                i++;
            }
            return uint.MaxValue;
        }
    }
}
