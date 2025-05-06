using PangyaAPI.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using PangyaAPI.Network.PangyaServer; 
using PangyaAPI.Network.PangyaPacket;

namespace PangyaAPI.Network.PangyaSession
{
    public class SessionManager
    {
        public readonly uint m_max_session;
        public readonly List<Session> m_sessions = new List<Session>();
        private readonly List<Session> m_session_del = new List<Session>();
        private uint m_ttl;
        public uint m_count = 0;
        private static bool m_is_init = false;
        protected IniHandle m_reader_ini;
        public readonly object _lockObject = new object();
        public pangya_packet_handle_base m_threadpool;
        public SessionManager(pangya_packet_handle_base _threadpool, uint _max_session)
        {
            m_threadpool = _threadpool;
            m_max_session = _max_session;
            m_ttl = 0u;
            // Carrega as config do arquivo server.ini
            config_init();
            m_is_init = true;
        }

        public void config_init()
        {
            try
            {
                m_reader_ini = new IniHandle("server.ini");
//read file
                m_ttl = m_reader_ini.ReadUInt32("OPTION", "TTL", 0);
            }
            catch (Exception e)
            { 
                throw;
            }
        }

        public virtual void Clear()
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

        public Session AddSession(Server _server, TcpClient socket, IPEndPoint address, byte key)
        {
            if (socket == null || !socket.Connected)
            {
                throw new InvalidOperationException("[SessionManager::AddSession] m_sock is invalid.");
            }

            Session pSession = null;
            lock (_lockObject)
            {
                int index = findSessionFree();
                if (index == int.MaxValue)
                {
                    throw new InvalidOperationException("[SessionManager::AddSession] Already reached Session limit.");
                }
                //socket.ReceiveTimeout = 60000; // 20 segundos
                //socket.SendTimeout = 60000;
                pSession = m_sessions[index];   
                pSession.m_sock = socket;
                pSession.Server = _server;
                pSession.m_addr = address;
                pSession.m_key = key;
                pSession.m_oid = index;
                pSession.m_time_start = pSession.m_tick = Environment.TickCount;

                pSession.setState(true);
                pSession.setConnected(true);

                m_count++;   
            }

            return pSession;
        }   

        public virtual bool DeleteSession(Session session)
        {
            if (session == null)
            {
                throw new ArgumentNullException(nameof(session), "[SessionManager::DeleteSession] Session is null.");
            }

            bool ret = true;
            lock (_lockObject)
            {
                if (!session.getState() && session.m_sock == null)
                {
                    throw new InvalidOperationException("[SessionManager::DeleteSession] Session is not connected.");
                }

                session.@lock();

                if (ret = session.clear())
                    m_count--;

                session.unlock();
            }
            if (m_sessions.Count(c => c.isConnected() == true) != m_count)
                m_count = (uint)m_sessions.Count(c => c.isConnected() == true);
            return ret;
        }


        public List<Session> findAllGM()
        {
            List<Session> v_gm = new List<Session>();

            foreach (Session el in m_sessions)
            {
                if ((el.getCapability() & 4) != 0 || (el.getCapability() & 128) != 0)    // GM
                    v_gm.Add(el);
            }

            return v_gm;
        }
        public virtual Session FindSessionByOid(uint oid)
        {
            Session session = null;
            lock (_lockObject)
            {
                foreach (var el in m_sessions.Where(el => el.m_sock != null))
                {
                    if (el.m_oid == oid)
                        session = el;
                }
            }
            return session;
        }

        public virtual Session findSessionByUID(uint uid)
        {
            Session session = null;
            lock (_lockObject)
            {
                session = m_sessions.FirstOrDefault(el => el.m_sock != null && el.getUID() == uid);
            }
            return session;
        }

        public virtual List<Session> FindAllSessionByUid(uint uid)
        {
            List<Session> sessions = new List<Session>();
            lock (_lockObject)
            {
                sessions = m_sessions.Where(el => el.m_sock != null && el.getUID() == uid).ToList();
            }
            return sessions;
        }

        public virtual Session FindSessionByNickname(string nickname)
        {
            Session session = null;
            lock (_lockObject)
            {
                session = m_sessions.FirstOrDefault(el => el.m_sock != null && el.getNickname() == nickname);
            }
            return session;
        }

        public Session GetSessionToDelete(int timeoutMs)
        {
            Session session = null;
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
                if (session.m_sock != null)
                {
                    int connTime = session.m_sock.GetConnectTime();
                    if (!session.isCreated())
                    {
                        m_session_del.Add(session);
                    }
                    else if (connTime < 0 || !session.m_is_authorized)
                    {
                        m_session_del.Add(session);
                    }
                    else if (m_ttl > 0 && (Environment.TickCount - session.m_tick) > (m_ttl / 1000.0))
                    {
                        m_session_del.Add(session);
                    }
                }
            }
        }

        public bool isFull()
        {
            bool isFull;
            lock (_lockObject)
            {
                isFull = m_sessions.Count(session => session.m_sock != null) == m_sessions.Count;
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
            return m_is_init;
        }

        public virtual int findSessionFree()
        {
            int i = 0;
            foreach (var _session in m_sessions)
            {
                if (_session.m_oid == int.MaxValue)
                {
                    return i;
                }
                i++;
            }
            return int.MaxValue;
        }

        public bool HasSessionWithIP(string ip)
        {
            return m_sessions.Any(s => s.isConnected() && s.getIP() == ip);
        }

        public Session findSessionByIP(string ip)
        {
            return m_sessions.FirstOrDefault(s => s.isConnected() && s.getIP() == ip);
        }

        public List<Session> findAllSessionByIP(string ip)
        {
            return m_sessions.Where(s => s.isConnected() && s.getIP() == ip).ToList();
        }

    }
}
