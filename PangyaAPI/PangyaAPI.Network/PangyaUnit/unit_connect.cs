using PangyaAPI.Network.Pangya_St;
using PangyaAPI.Network.PangyaPacket;
using PangyaAPI.Utilities;
using PangyaAPI.Utilities.BinaryModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static PangyaAPI.Network.PangyaUnit.unit_connect_base;
using static System.Collections.Specialized.BitVector32;

namespace PangyaAPI.Network.PangyaUnit
{
    public class ParamDispatchAS : ParamDispatch
    {
        public new UnitPlayer _session;
    }

    public class UnitPlayer : PangyaSession.SessionBase
    {
        public struct player_info
        {
            public string nickname;
            public string id;
            public uint uid;
            public uint tipo;
            public byte m_state;
        }
        public byte getStateLogged() => m_pi.m_state;
        public override uint getUID() => m_pi.uid;
        public override uint getCapability() => m_pi.tipo;
        public override string getNickname() => m_pi.nickname;
        public override string getID() => m_pi.id;
        public ServerInfoEx m_si;
        public player_info m_pi;
    }

    public abstract class unit_connect_base
    {
        public unit_connect_base(ServerInfoEx _si)
        { }
        public enum STATE : byte { UNINITIALIZED, GOOD, GOOD_WITH_WARNING, INITIALIZED, FAILURE }
        public enum ThreadType { WORKER_IO, WORKER_IO_SEND, WORKER_IO_RECV, WORKER_LOGICAL, WORKER_SEND, TT_CONSOLE, TT_ACCEPT, TT_ACCEPTEX, TT_ACCEPTEX_IO, TT_RECV, TT_SEND, TT_JOB, TT_DB_NORMAL, TT_MONITOR, TT_SEND_MSG_TO_LOBBY }
        public enum OperationType { SEND_RAW_REQUEST, SEND_RAW_COMPLETED, RECV_REQUEST, RECV_COMPLETED, SEND_REQUEST, SEND_COMPLETED, DISPACH_PACKET_SERVER, DISPACH_PACKET_CLIENT, ACCEPT_COMPLETED }

        public struct stUnitCtx
        {
            public bool state;
            public string ip;
            public int port;

            public void Clear()
            {
                ip = string.Empty;
                port = 0;
                state = false;
            }
        }

        public abstract void WaitForAllThreadsFinish(int milliseconds);
        public abstract void PostIoOperation(PangyaSession.SessionBase session, byte[] buffer, int ioSize, int operation);
        public abstract void DisconnectSession(PangyaSession.SessionBase session);
        public abstract void onStart();
        public abstract bool isLive();

        protected abstract void onHeartBeat();
        protected abstract void onConnected();
        protected abstract void onDisconnect();
        protected abstract void config_init();

        public func_arr funcs;
        public func_arr funcs_sv;

        public UnitPlayer m_session;
        public STATE m_state;
        public stUnitCtx m_unit_ctx;
        public IniHandle m_reader_ini;
        public class packet_func_as
        {
            public static void session_send(PangyaBinaryWriter p, UnitPlayer s, byte _debug) { }
            public static void session_send(List<PangyaBinaryWriter> v_p, UnitPlayer s, byte _debug) { }
        }

    }
}
