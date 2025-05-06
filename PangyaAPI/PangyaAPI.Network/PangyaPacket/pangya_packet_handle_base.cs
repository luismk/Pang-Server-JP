using PangyaAPI.Network.PangyaSession;
using PangyaAPI.Network.PangyaUtil;                         
namespace PangyaAPI.Network.PangyaPacket
{
    public enum TT
    { // TYPE THREAD
        TT_WORKER_IO,
        TT_WORKER_IO_SEND,
        TT_WORKER_IO_RECV,
        TT_WORKER_LOGICAL,
        TT_WORKER_SEND,
        TT_CONSOLE,
        TT_ACCEPT,
        TT_ACCEPTEX,
        TT_ACCEPTEX_IO,
        TT_RECV,
        TT_SEND,
        TT_JOB,
        TT_DB_NORMAL,
        TT_MONITOR,
        TT_SEND_MSG_TO_LOBBY,
        TT_SCAN_CMD,
        TT_DISCONNECT_SESSION,
        TT_REGISTER_SERVER,
        TT_COIN_CUBE_LOCATION_TRANSLATE
    }
    public enum OP_TYPE
    {  // Operation Type
        STDA_OT_SEND_RAW_REQUEST,
        STDA_OT_SEND_RAW_COMPLETED,
        STDA_OT_RECV_REQUEST,
        STDA_OT_RECV_COMPLETED,
        STDA_OT_SEND_REQUEST,
        STDA_OT_SEND_COMPLETED,
        STDA_OT_DISPACH_PACKET_SERVER,
        STDA_OT_DISPACH_PACKET_CLIENT,
        STDA_OT_ACCEPT_COMPLETED,
        STDA_OT_DISCONNECT_PLAYER
    };

    public interface pangya_packet_handle_base
    {
        void postIoOperation(Session _session,
            PangyaBuffer lpBuffer, uint dwIOsize,
            uint operation);
        void postIoOperation(Session _session,
           PangyaBuffer lpBuffer, uint dwIOsize,
           OP_TYPE operation);

    }
}
