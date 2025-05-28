//using PangyaAPI.Network.Pangya_St;
//using PangyaAPI.Network.Cmd;
//using PangyaAPI.Network.PangyaPacket;
//using PangyaAPI.Network.PangyaSession;
//using PangyaAPI.SQL.Manager;
//using PangyaAPI.Utilities;
//using PangyaAPI.Utilities.BinaryModels;
//using PangyaAPI.Utilities.Log;
//using System;
//using System.Runtime.InteropServices;

//namespace PangyaAPI.Network.PangyaUnit
//{

//    public class unit_auth_server_connect : unit_connect_base
//    {
//        public unit_auth_server_connect(IUnitAuthServer _owner_server, ServerInfoEx _si) : base(_si)
//        {
//            this.m_owner_server = _owner_server;

//            if (m_state == STATE.FAILURE)
//            {
//                message_pool.push(new message("[unit_auth_server_connect::unit_auth_server_connect][Error] na inicializacao unit auth server connect", type_msg.CL_FILE_LOG_AND_CONSOLE));
//                return;
//            }

//            try
//            {

//                // Inicializar Config do arquivo ini
//                config_init();

//                /// ---------- Packets ---------

//                // Packet000
//                funcs.addPacketCall(0x0, (object _arg1, ParamDispatch _arg2) =>
//                {
//                    ParamDispatchAS pd = (ParamDispatchAS)(_arg2);
//                    unit_auth_server_connect uc = (unit_auth_server_connect)(_arg1);
//                    try
//                    {
//                        ;
//                        uc.requestFirstPacketKey(pd._session, pd._packet);
//                    }
//                    catch (exception e)
//                    {
//                        message_pool.push(new message("[packet_func::packet" + "000" + "][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
//                    }
//                    return 0;
//                }, this);

//                // Packet001
//                funcs.addPacketCall((0x1), (object _arg1, ParamDispatch _arg2) =>
//                {
//                    ParamDispatchAS pd = (ParamDispatchAS)(_arg2);
//                    unit_auth_server_connect uc = (unit_auth_server_connect)(_arg1);
//                    try
//                    {
//                        ;
//                        uc.requestAskLogin(pd._session, pd._packet);
//                    }
//                    catch (exception e)
//                    {
//                        message_pool.push(new message("[packet_func::packet" + "001" + "][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
//                    }
//                    return 0;
//                }, this);

//                // Packet002
//                funcs.addPacketCall((0x2), (object _arg1, ParamDispatch _arg2) =>
//                {
//                    ParamDispatchAS pd = (ParamDispatchAS)(_arg2);
//                    unit_auth_server_connect uc = (unit_auth_server_connect)(_arg1);
//                    try
//                    {
//                        ;
//                        uc.requestShutdownServer(pd._session, pd._packet);
//                    }
//                    catch (exception e)
//                    {
//                        message_pool.push(new message("[packet_func::packet" + "002" + "][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
//                    }
//                    return 0;
//                }, this);

//                // Packet003
//                funcs.addPacketCall((0x3), (object _arg1, ParamDispatch _arg2) =>
//                {
//                    ParamDispatchAS pd = (ParamDispatchAS)(_arg2);
//                    unit_auth_server_connect uc = (unit_auth_server_connect)(_arg1);
//                    try
//                    {
//                        ;
//                        uc.requestBroadcastNotice(pd._session, pd._packet);
//                    }
//                    catch (exception e)
//                    {
//                        message_pool.push(new message("[packet_func::packet" + "003" + "][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
//                    }
//                    return 0;
//                }, this);

//                // Packet004
//                funcs.addPacketCall((0x4), (object _arg1, ParamDispatch _arg2) =>
//                {
//                    ParamDispatchAS pd = (ParamDispatchAS)(_arg2);
//                    unit_auth_server_connect uc = (unit_auth_server_connect)(_arg1);
//                    try
//                    {
//                        ;
//                        uc.requestBroadcastTicker(pd._session, pd._packet);
//                    }
//                    catch (exception e)
//                    {
//                        message_pool.push(new message("[packet_func::packet" + "004" + "][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
//                    }
//                    return 0;
//                }, this);

//                // Packet005
//                funcs.addPacketCall((0x5), (object _arg1, ParamDispatch _arg2) =>
//                {
//                    ParamDispatchAS pd = (ParamDispatchAS)(_arg2);
//                    unit_auth_server_connect uc = (unit_auth_server_connect)(_arg1);
//                    try
//                    {
//                        ;
//                        uc.requestBroadcastCubeWinRare(pd._session, pd._packet);
//                    }
//                    catch (exception e)
//                    {
//                        message_pool.push(new message("[packet_func::packet" + "005" + "][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
//                    }
//                    return 0;
//                }, this);

//                // Packet006
//                funcs.addPacketCall((0x6), (object _arg1, ParamDispatch _arg2) =>
//                {
//                    ParamDispatchAS pd = (ParamDispatchAS)(_arg2);
//                    unit_auth_server_connect uc = (unit_auth_server_connect)(_arg1);
//                    try
//                    {
//                        ;
//                        uc.requestDisconnectPlayer(pd._session, pd._packet);
//                    }
//                    catch (exception e)
//                    {
//                        message_pool.push(new message("[packet_func::packet" + "006" + "][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
//                    }
//                    return 0;
//                }, this);

//                // Packet007
//                funcs.addPacketCall((0x7), (object _arg1, ParamDispatch _arg2) =>
//                {
//                    ParamDispatchAS pd = (ParamDispatchAS)(_arg2);
//                    unit_auth_server_connect uc = (unit_auth_server_connect)(_arg1);
//                    try
//                    {
//                        ;
//                        uc.requestConfirmDisconnectPlayer(pd._session, pd._packet);
//                    }
//                    catch (exception e)
//                    {
//                        message_pool.push(new message("[packet_func::packet" + "007" + "][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
//                    }
//                    return 0;
//                }, this);

//                // Packet008
//                funcs.addPacketCall((0x8), (object _arg1, ParamDispatch _arg2) =>
//                {
//                    ParamDispatchAS pd = (ParamDispatchAS)(_arg2);
//                    unit_auth_server_connect uc = (unit_auth_server_connect)(_arg1);
//                    try
//                    {
//                        ;
//                        uc.requestNewMailArrivedMailBox(pd._session, pd._packet);
//                    }
//                    catch (exception e)
//                    {
//                        message_pool.push(new message("[packet_func::packet" + "008" + "][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
//                    }
//                    return 0;
//                }, this);

//                // Packet009
//                funcs.addPacketCall((0x9), (object _arg1, ParamDispatch _arg2) =>
//                {
//                    ParamDispatchAS pd = (ParamDispatchAS)(_arg2);
//                    unit_auth_server_connect uc = (unit_auth_server_connect)(_arg1);
//                    try
//                    {
//                        ;
//                        uc.requestNewRate(pd._session, pd._packet);
//                    }
//                    catch (exception e)
//                    {
//                        message_pool.push(new message("[packet_func::packet" + "009" + "][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
//                    }
//                    return 0;
//                }, this);

//                // Packet00A
//                funcs.addPacketCall((0xA), (object _arg1, ParamDispatch _arg2) =>
//                {
//                    ParamDispatchAS pd = (ParamDispatchAS)(_arg2);
//                    unit_auth_server_connect uc = (unit_auth_server_connect)(_arg1);
//                    try
//                    {
//                        ;
//                        uc.requestReloadSystem(pd._session, pd._packet);
//                    }
//                    catch (exception e)
//                    {
//                        message_pool.push(new message("[packet_func::packet" + "00A" + "][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
//                    }
//                    return 0;
//                }, this);

//                // Packet00B
//                funcs.addPacketCall((0xB), (object _arg1, ParamDispatch _arg2) =>
//                {
//                    ParamDispatchAS pd = (ParamDispatchAS)(_arg2);
//                    unit_auth_server_connect uc = (unit_auth_server_connect)(_arg1);
//                    try
//                    {
//                        ;
//                        uc.requestInfoPlayerOnline(pd._session, pd._packet);
//                    }
//                    catch (exception e)
//                    {
//                        message_pool.push(new message("[packet_func::packet" + "00B" + "][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
//                    }
//                    return 0;
//                }, this);

//                // Packet00C
//                funcs.addPacketCall((0xC), (object _arg1, ParamDispatch _arg2) =>
//                {
//                    ParamDispatchAS pd = (ParamDispatchAS)(_arg2);
//                    unit_auth_server_connect uc = (unit_auth_server_connect)(_arg1);
//                    try
//                    {
//                        ;
//                        uc.requestConfirmSendInfoPlayerOnline(pd._session, pd._packet);
//                    }
//                    catch (exception e)
//                    {
//                        message_pool.push(new message("[packet_func::packet" + "00C" + "][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
//                    }
//                    return 0;
//                }, this);

//                // Packet00D
//                funcs.addPacketCall((0xD), (object _arg1, ParamDispatch _arg2) =>
//                {
//                    ParamDispatchAS pd = (ParamDispatchAS)(_arg2);
//                    unit_auth_server_connect uc = (unit_auth_server_connect)(_arg1);
//                    try
//                    {
//                        ;
//                        uc.requestSendCommandToOtherServer(pd._session, pd._packet);
//                    }
//                    catch (exception e)
//                    {
//                        message_pool.push(new message("[packet_func::packet" + "00D" + "][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
//                    }
//                    return 0;
//                }, this);

//                // Packet00E
//                funcs.addPacketCall((0xE), (object _arg1, ParamDispatch _arg2) =>
//                {
//                    ParamDispatchAS pd = (ParamDispatchAS)(_arg2);
//                    unit_auth_server_connect uc = (unit_auth_server_connect)(_arg1);
//                    try
//                    {   
//                        uc.requestSendReplyToOtherServer(pd._session, pd._packet);
//                    }
//                    catch (exception e)
//                    {
//                        message_pool.push(new message("[packet_func::packet" + "00E" + "][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
//                    }
//                    return 0;
//                }, this);

//                /// ----------- Pacotes -----------

//                funcs_sv.addPacketCall((0x1), (object _arg1, ParamDispatch _arg2) =>
//                {
//                    return 0;
//                }, this);

//                // Pacote002
//                funcs_sv.addPacketCall((0x2), (object _arg1, ParamDispatch _arg2) =>
//                {
//                    return 0;
//                }, this);

//                // Pacote003
//                funcs_sv.addPacketCall((0x3), (object _arg1, ParamDispatch _arg2) =>
//                {
//                    return 0;
//                }, this);

//                // Pacote004
//                funcs_sv.addPacketCall((0x4), (object _arg1, ParamDispatch _arg2) =>
//                {
//                    return 0;
//                }, this);

//                // Pacote005
//                funcs_sv.addPacketCall((0x5), (object _arg1, ParamDispatch _arg2) =>
//                {
//                    return 0;
//                }, this);

//                // Pacote005
//                funcs_sv.addPacketCall((0x6), (object _arg1, ParamDispatch _arg2) =>
//                {
//                    return 0;
//                }, this);

//                // Pacote005
//                funcs_sv.addPacketCall((0x7), (object _arg1, ParamDispatch _arg2) =>
//                {
//                    return 0;
//                }, this);

//                // Initialized complete
//                m_state = STATE. INITIALIZED;

//            }
//            catch (exception e)
//            {

//                m_state = STATE.FAILURE;

//                message_pool.push(new message("[unit_auth_server_connect::unit_auth_server_connect][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
//            }
//        }

//        public virtual void requestFirstPacketKey(UnitPlayer _session, packet _packet)
//        {
//            if (!_session.getState())
//            {
//                throw new exception("[unit_auth_server_connect::request" + "FirstPacketKey" + "][Error] player nao esta connectado.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.UNIT_AUTH_SERVER_CONNECT,
//                    1, 0));
//            }
//            if (_packet == null)
//            {
//                throw new exception("[unit_auth_server_connect::request" + "FirstPacketKey" + "][Error] _packet is nullptr", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.UNIT_AUTH_SERVER_CONNECT,
//                    6, 0));
//            }

//            try
//            {

//                _session.m_key = (byte)_packet.ReadUInt32();

//                var server_guid = _packet.ReadUInt32();

//                CmdNewAuthServerKey cmd_nask = new CmdNewAuthServerKey(_session.m_si.uid); // Waiter

//                NormalManagerDB.add(0,
//                    cmd_nask, null, null);

//                if (cmd_nask.getException().getCodeError() != 0)
//                {
//                    throw cmd_nask.getException();
//                }

//                // Resposta para o Auth Server
//                var p = new packet((ushort)0x1);

//                p.WriteUInt32((uint)_session.m_si.tipo);
//                p.WriteUInt32((uint)_session.m_si.uid);
//                p.WriteString(_session.m_si.nome);
//                p.WriteString(cmd_nask.getInfo());

//                packet_func_as.session_send(p,
//                    _session, 1);

//            }
//            catch (exception e)
//            {

//                message_pool.push(new message("[unit_auth_server_connect::requestFirstPacketKey][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
//            }
//        }

//        public virtual void requestAskLogin(UnitPlayer _session, packet _packet)
//        {
//            if (!_session.getState())
//            {
//                throw new exception("[unit_auth_server_connect::request" + "AskLogin" + "][Error] player nao esta connectado.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.UNIT_AUTH_SERVER_CONNECT,
//                    1, 0));
//            }
//            if (_packet == null)
//            {
//                throw new exception("[unit_auth_server_connect::request" + "AskLogin" + "][Error] _packet is nullptr", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.UNIT_AUTH_SERVER_CONNECT,
//                    6, 0));
//            }

//            try
//            {

//                int oid = _packet.ReadInt32();

//                if (oid > -1)
//                {

//                    _session.m_oid = (uint)oid;

//                    // Log
//                    message_pool.push(new message("[unit_auth_server_connect::requestAskLogin][Log] Logou com o Auth Server[OID=" + Convert.ToString(_session.m_oid) + "]", type_msg.CL_FILE_LOG_AND_CONSOLE));
//                }
//                else
//                {
//                    message_pool.push(new message("[unit_auth_server_connect::requestAskLogin][Log] Nao conseguiu logar com o Auth Server.", type_msg.CL_FILE_LOG_AND_CONSOLE));
//                }

//            }
//            catch (exception e)
//            {

//                message_pool.push(new message("[unit_auth_server_connect::requestAskLogin][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
//            }
//        }

//        public virtual void requestShutdownServer(UnitPlayer _session, packet _packet)
//        {
//            if (!_session.getState())
//            {
//                throw new exception("[unit_auth_server_connect::request" + "ShutdownServer" + "][Error] player nao esta connectado.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.UNIT_AUTH_SERVER_CONNECT,
//                    1, 0));
//            }
//            if (_packet == null)
//            {
//                throw new exception("[unit_auth_server_connect::request" + "ShutdownServer" + "][Error] _packet is nullptr", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.UNIT_AUTH_SERVER_CONNECT,
//                    6, 0));
//            }

//            try
//            {

//                // Time In tv_sec for Shutdown
//                int time = _packet.ReadInt32();

//                m_owner_server.authCmdShutdown(time);

//            }
//            catch (exception e)
//            {

//                message_pool.push(new message("[unit_auth_server_connect::requestShutdownServer][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
//            }
//        }

//        public virtual void requestBroadcastNotice(UnitPlayer _session, packet _packet)
//        {
//            if (!_session.getState())
//            {
//                throw new exception("[unit_auth_server_connect::request" + "BroadcastNotice" + "][Error] player nao esta connectado.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.UNIT_AUTH_SERVER_CONNECT,
//                    1, 0));
//            }
//            if (_packet == null)
//            {
//                throw new exception("[unit_auth_server_connect::request" + "BroadcastNotice" + "][Error] _packet is nullptr", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.UNIT_AUTH_SERVER_CONNECT,
//                    6, 0));
//            }

//            try
//            {

//                var notice = _packet.ReadString();

//                m_owner_server.authCmdBroadcastNotice(notice);

//            }
//            catch (exception e)
//            {

//                message_pool.push(new message("[unit_auth_server_connect::requestBroadcastNotice][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
//            }
//        }

//        public virtual void requestBroadcastTicker(UnitPlayer _session, packet _packet)
//        {
//            if (!_session.getState())
//            {
//                throw new exception("[unit_auth_server_connect::request" + "BroadcastTicker" + "][Error] player nao esta connectado.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.UNIT_AUTH_SERVER_CONNECT,
//                    1, 0));
//            }
//            if (_packet == null)
//            {
//                throw new exception("[unit_auth_server_connect::request" + "BroadcastTicker" + "][Error] _packet is nullptr", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.UNIT_AUTH_SERVER_CONNECT,
//                    6, 0));
//            }

//            try
//            {

//                var nickname = _packet.ReadString();
//                var msg = _packet.ReadString();

//                m_owner_server.authCmdBroadcastTicker(nickname, msg);

//            }
//            catch (exception e)
//            {

//                message_pool.push(new message("[unit_auth_server_connect::requestBroadcastTicker][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
//            }
//        }

//        public virtual void requestBroadcastCubeWinRare(UnitPlayer _session, packet _packet)
//        {
//            if (!_session.getState())
//            {
//                throw new exception("[unit_auth_server_connect::request" + "BroadcastCubeWinRare" + "][Error] player nao esta connectado.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.UNIT_AUTH_SERVER_CONNECT,
//                    1, 0));
//            }
//            if (_packet == null)
//            {
//                throw new exception("[unit_auth_server_connect::request" + "BroadcastCubeWinRare" + "][Error] _packet is nullptr", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.UNIT_AUTH_SERVER_CONNECT,
//                    6, 0));
//            }

//            try
//            {

//                uint option = _packet.ReadUInt32();
//                var msg = _packet.ReadString();

//                m_owner_server.authCmdBroadcastCubeWinRare(msg, (option));

//            }
//            catch (exception e)
//            {

//                message_pool.push(new message("[unit_auth_server_connect::requestBroadcastCubeWinRare][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
//            }
//        }

//        public virtual void requestDisconnectPlayer(UnitPlayer _session, packet _packet)
//        {
//            if (!_session.getState())
//            {
//                throw new exception("[unit_auth_server_connect::request" + "DisconnectPlayer" + "][Error] player nao esta connectado.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.UNIT_AUTH_SERVER_CONNECT,
//                    1, 0));
//            }
//            if (_packet == null)
//            {
//                throw new exception("[unit_auth_server_connect::request" + "DisconnectPlayer" + "][Error] _packet is nullptr", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.UNIT_AUTH_SERVER_CONNECT,
//                    6, 0));
//            }

//            try
//            {

//                uint player_uid = _packet.ReadUInt32();
//                uint server_uid = _packet.ReadUInt32();
//                byte force = _packet.ReadUInt8(); // Flag que força a disconectar o usuário

//                m_owner_server.authCmdDisconnectPlayer((server_uid),
//                    (player_uid),
//                    force);

//            }
//            catch (exception e)
//            {

//                message_pool.push(new message("[unit_auth_server_connect::requestDisconnectPlayer][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
//            }
//        }

//        public virtual void requestConfirmDisconnectPlayer(UnitPlayer _session, packet _packet)
//        {
//            if (!_session.getState())
//            {
//                throw new exception("[unit_auth_server_connect::request" + "ConfirmDisconnectPlayer" + "][Error] player nao esta connectado.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.UNIT_AUTH_SERVER_CONNECT,
//                    1, 0));
//            }
//            if (_packet == null)
//            {
//                throw new exception("[unit_auth_server_connect::request" + "ConfirmDisconnectPlayer" + "][Error] _packet is nullptr", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.UNIT_AUTH_SERVER_CONNECT,
//                    6, 0));
//            }

//            try
//            {

//                uint player_uid = _packet.ReadUInt32();

//                m_owner_server.authCmdConfirmDisconnectPlayer((player_uid));

//            }
//            catch (exception e)
//            {

//                message_pool.push(new message("[unit_auth_server_connect::requestConfirmDisconnectPlayer][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
//            }
//        }

//        public virtual void requestNewMailArrivedMailBox(UnitPlayer _session, packet _packet)
//        {
//            if (!_session.getState())
//            {
//                throw new exception("[unit_auth_server_connect::request" + "NewMailArrivedMailBox" + "][Error] player nao esta connectado.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.UNIT_AUTH_SERVER_CONNECT,
//                    1, 0));
//            }
//            if (_packet == null)
//            {
//                throw new exception("[unit_auth_server_connect::request" + "NewMailArrivedMailBox" + "][Error] _packet is nullptr", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.UNIT_AUTH_SERVER_CONNECT,
//                    6, 0));
//            }

//            try
//            {

//                uint player_uid = _packet.ReadUInt32();
//                uint mail_id = _packet.ReadUInt32();

//                m_owner_server.authCmdNewMailArrivedMailBox((player_uid), (mail_id));

//            }
//            catch (exception e)
//            {

//                message_pool.push(new message("[unit_auth_server_connect::requestNewMailArrivedMailBox][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
//            }
//        }

//        public virtual void requestNewRate(UnitPlayer _session, packet _packet)
//        {
//            if (!_session.getState())
//            {
//                throw new exception("[unit_auth_server_connect::request" + "NewRate" + "][Error] player nao esta connectado.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.UNIT_AUTH_SERVER_CONNECT,
//                    1, 0));
//            }
//            if (_packet == null)
//            {
//                throw new exception("[unit_auth_server_connect::request" + "NewRate" + "][Error] _packet is nullptr", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.UNIT_AUTH_SERVER_CONNECT,
//                    6, 0));
//            }

//            try
//            {

//                uint tipo = _packet.ReadUInt32();
//                uint qntd = _packet.ReadUInt32();

//                m_owner_server.authCmdNewRate((tipo), (qntd));

//            }
//            catch (exception e)
//            {

//                message_pool.push(new message("[unit_auth_server_connect::requestNewRate][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
//            }
//        }

//        public virtual void requestReloadSystem(UnitPlayer _session, packet _packet)
//        {
//            if (!_session.getState())
//            {
//                throw new exception("[unit_auth_server_connect::request" + "ReloadSystem" + "][Error] player nao esta connectado.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.UNIT_AUTH_SERVER_CONNECT,
//                    1, 0));
//            }
//            if (_packet == null)
//            {
//                throw new exception("[unit_auth_server_connect::request" + "ReloadSystem" + "][Error] _packet is nullptr", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.UNIT_AUTH_SERVER_CONNECT,
//                    6, 0));
//            }

//            try
//            {

//                uint sistema = _packet.ReadUInt32();

//                m_owner_server.authCmdReloadGlobalSystem((sistema));

//            }
//            catch (exception e)
//            {

//                message_pool.push(new message("[unit_auth_server_connect::requestReloadSystem][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
//            }
//        }

//        public virtual void requestInfoPlayerOnline(UnitPlayer _session, packet _packet)
//        {
//            if (!_session.getState())
//            {
//                throw new exception("[unit_auth_server_connect::request" + "GetInfoPlayerOnline" + "][Error] player nao esta connectado.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.UNIT_AUTH_SERVER_CONNECT,
//                    1, 0));
//            }
//            if (_packet == null)
//            {
//                throw new exception("[unit_auth_server_connect::request" + "GetInfoPlayerOnline" + "][Error] _packet is nullptr", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.UNIT_AUTH_SERVER_CONNECT,
//                    6, 0));
//            }

//            try
//            {

//                uint req_server_uid = _packet.ReadUInt32();
//                uint player_uid = _packet.ReadUInt32();

//                m_owner_server.authCmdInfoPlayerOnline((req_server_uid), (player_uid));

//            }
//            catch (exception e)
//            {

//                message_pool.push(new message("[unit_auth_server_connect::requestInfoPlayerOnline][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
//            }
//        }

//        public virtual void requestConfirmSendInfoPlayerOnline(UnitPlayer _session, packet _packet)
//        {
//            if (!_session.getState())
//            {
//                throw new exception("[unit_auth_server_connect::request" + "ConfirmSendInfoOnline" + "][Error] player nao esta connectado.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.UNIT_AUTH_SERVER_CONNECT,
//                    1, 0));
//            }
//            if (_packet == null)
//            {
//                throw new exception("[unit_auth_server_connect::request" + "ConfirmSendInfoOnline" + "][Error] _packet is nullptr", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.UNIT_AUTH_SERVER_CONNECT,
//                    6, 0));
//            }

//            try
//            {

//                AuthServerPlayerInfo aspi = new AuthServerPlayerInfo();

//                uint req_server_uid = _packet.ReadUInt32();

//                aspi.option = _packet.ReadInt32();
//                aspi.uid = _packet.ReadUInt32();

//                if (aspi.option == 1)
//                {

//                    aspi.id = _packet.ReadString();
//                    aspi.ip = _packet.ReadString();

//                }

//                m_owner_server.authCmdConfirmSendInfoPlayerOnline(req_server_uid, aspi);

//            }
//            catch (exception e)
//            {

//                message_pool.push(new message("[unit_auth_server_connect::requestConfirmSendInfoPlayerOnline][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
//            }
//        }

//        public virtual void requestSendCommandToOtherServer(UnitPlayer _session, packet _packet)
//        {
//            if (!_session.getState())
//            {
//                throw new exception("[unit_auth_server_connect::request" + "SendCommandToOtherServer" + "][Error] player nao esta connectado.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.UNIT_AUTH_SERVER_CONNECT,
//                    1, 0));
//            }
//            if (_packet == null)
//            {
//                throw new exception("[unit_auth_server_connect::request" + "SendCommandToOtherServer" + "][Error] _packet is nullptr", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.UNIT_AUTH_SERVER_CONNECT,
//                    6, 0));
//            }

//            try
//            {

//                m_owner_server.authCmdSendCommandToOtherServer(_packet);

//            }
//            catch (exception e)
//            {

//                message_pool.push(new message("[unit_auth_server_connect::requestSendCommandToOtherServer][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
//            }
//        }

//        public virtual void requestSendReplyToOtherServer(UnitPlayer _session, packet _packet)
//        {
//            if (!_session.getState())
//            {
//                throw new exception("[unit_auth_server_connect::request" + "SendReplyToOtherServer" + "][Error] player nao esta connectado.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.UNIT_AUTH_SERVER_CONNECT,
//                    1, 0));
//            }
//            if (_packet == null)
//            {
//                throw new exception("[unit_auth_server_connect::request" + "SendReplyToOtherServer" + "][Error] _packet is nullptr", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.UNIT_AUTH_SERVER_CONNECT,
//                    6, 0));
//            }

//            try
//            {

//                m_owner_server.authCmdSendReplyToOtherServer(_packet);

//            }
//            catch (exception e)
//            {

//                message_pool.push(new message("[unit_auth_server_connect::requestSendReplyToOtherServer][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
//            }
//        }

//        // Request Reply
//        public virtual void sendConfirmDisconnectPlayer(uint _server_uid, uint _player_uid)
//        {

//            if (!isLive())
//            {
//                throw new exception("[unit_auth_server_connect::sendConfirmDisconnectPlayer][Error] Nao pode enviar o comando confirm disconnect player para o Auth Server, por que nao esta conectado com ele.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.UNIT_AUTH_SERVER_CONNECT,
//                    50, 0));
//            }

//            try
//            {

//                // Log
//                message_pool.push(new message("[unit_auth_server_connect::sendConfirmDisconnectPlayer][Log] Send Confirm Disconnect Player[UID=" + Convert.ToString(_player_uid) + "] para o Auth Server enviar a resposta para o Server[UID=" + Convert.ToString(_server_uid) + "]", type_msg.CL_FILE_LOG_AND_CONSOLE));

//                var p = new packet((ushort)0x3);

//                p.WriteUInt32(_player_uid);
//                p.WriteUInt32(_server_uid);

//                packet_func_as.session_send(p,
//                    m_session, 1);

//            }
//            catch (exception e)
//            {

//                message_pool.push(new message("[unit_auth_server_connect::sendConfirmDisconnectPlayer][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
//            }
//        }

//        public virtual void sendDisconnectPlayer(uint _server_uid, uint _player_uid)
//        {

//            if (!isLive())
//            {
//                throw new exception("[unit_auth_server_connect::sendDisconnectPlayer][Error] Nao pode enviar o comando disconnect player para o Auth Server, por que nao esta conectado com ele.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.UNIT_AUTH_SERVER_CONNECT,
//                    50, 0));
//            }

//            try
//            {

//                // Log
//                message_pool.push(new message("[unit_auth_server_connect::sendDisconnectPlayer][Log] Send Disconnect Player[UID=" + Convert.ToString(_player_uid) + "] para o Auth Server enviar para o Server[UID=" + Convert.ToString(_server_uid) + "]", type_msg.CL_FILE_LOG_AND_CONSOLE));

//                var p = new packet((ushort)0x2);

//                p.WriteUInt32(_player_uid);
//                p.WriteUInt32(_server_uid);

//                packet_func_as.session_send(p,
//                    m_session, 1);

//            }
//            catch (exception e)
//            {

//                message_pool.push(new message("[unit_auth_server_connect::sendDisconnectPlayer][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
//            }
//        }

//        public virtual void sendInfoPlayerOnline(uint _server_uid, AuthServerPlayerInfo _aspi)
//        {

//            if (!isLive())
//            {
//                throw new exception("[unit_auth_server_connect::sendInfoPlayerOnline][Error] Nao pode enviar o comando disconnect player para o Auth Server, por que nao esta conectado com ele.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.UNIT_AUTH_SERVER_CONNECT,
//                    50, 0));
//            }

//            try
//            {

//                // Log
//                if (_aspi.option == 1)
//                {
//                    message_pool.push(new message("[unit_auth_server_connect::sendInfoPlayerOnline][Log] Send Info Player[UID=" + Convert.ToString(_aspi.uid) + "], para o Auth Server enviar para o Server[UID=" + Convert.ToString(_server_uid) + "]", type_msg.CL_FILE_LOG_AND_CONSOLE));
//                }
//                else
//                {
//                    message_pool.push(new message("[unit_auth_server_connect::sendInfoPlayerOnline][Log] Error nao encontrou o Player[UID=" + Convert.ToString(_aspi.uid) + "] online para enviar o info dele para o Auth Server enviar para o Server[UID=" + Convert.ToString(_server_uid) + "]", type_msg.CL_FILE_LOG_AND_CONSOLE));
//                }

//                var p = new packet((ushort)0x5);

//                p.WriteUInt32(_server_uid);
//                p.WriteInt32(_aspi.option);
//                p.WriteUInt32(_aspi.uid);

//                if (_aspi.option == 1)
//                {

//                    p.WriteString(_aspi.id);
//                    p.WriteString(_aspi.ip);
//                }

//                packet_func_as.session_send(p,
//                    m_session, 1);

//            }
//            catch (exception e)
//            {

//                message_pool.push(new message("[unit_auth_server_connect::sendInfoPlayerOnline][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
//            }
//        }

//        public virtual void getInfoPlayerOnline(uint _server_uid, uint _player_uid)
//        {

//            if (!isLive())
//            {
//                throw new exception("[unit_auth_server_connect::getInfoPlayerOnline][Error] Nao pode enviar o comando disconnect player para o Auth Server, por que nao esta conectado com ele.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.UNIT_AUTH_SERVER_CONNECT,
//                    50, 0));
//            }

//            try
//            {

//                // Log
//                message_pool.push(new message("[unit_auth_server_connect::getInfoPlayerOnline][Log] Pede o Info do Player[UID=" + Convert.ToString(_player_uid) + "] Online para o Auth Server pedir para o Server[UID=" + Convert.ToString(_server_uid) + "]", type_msg.CL_FILE_LOG_AND_CONSOLE));

//                var p = new packet((ushort)0x4);

//                p.WriteUInt32(_server_uid);
//                p.WriteUInt32(_player_uid);

//                packet_func_as.session_send(p,
//                    m_session, 1);

//            }
//            catch (exception e)
//            {

//                message_pool.push(new message("[unit_auth_server_connect::getInfoPlayerOnline][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
//            }
//        }

//        public virtual void sendCommandToOtherServer(uint _server_uid, packet _packet)
//        {

//            if (!isLive())
//            {
//                throw new exception("[unit_auth_server_connect::sendCommandToOtherServer][Error] Nao pode enviar o comando[ID=" + Convert.ToString(_packet.getTipo()) + "] para o outro server[UID=" + Convert.ToString(_server_uid) + "] com o Auth Server, por que nao esta conectado com ele.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.UNIT_AUTH_SERVER_CONNECT,
//                    50, 0));
//            }

//            try
//            {

//                // Ler o command ID para verificar se está tudo ok
//                _packet.ReadUInt16();

//                if (_packet.GetSize < 2)
//                {
//                    throw new exception("[unit_auth_server_connect::sendCommandToOtherServer][Error] Tentou enviar o comando[ID=" + Convert.ToString(_packet.getTipo()) + "] para o outro server[UID=" + Convert.ToString(_server_uid) + "] com o Auth Server, mas o packet eh invalido nao tem nem o id.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.UNIT_AUTH_SERVER_CONNECT,
//                        1000, 0));
//                }

//                ushort command_buff_size = (ushort)(_packet.GetSize - 2u);

//                CommandOtherServerHeaderEx cosh = new CommandOtherServerHeaderEx();

//                cosh.send_server_uid_or_type = _server_uid;
//                cosh.command_id = _packet.getTipo();

//                // Inicializa comando buffer
//                cosh.command.init(command_buff_size);

//                if (!cosh.command.is_good())
//                {
//                    throw new exception("[unit_auth_server_connect::sendCommandToOtherServer][Error] Tentou enviar a reposta[ID=" + Convert.ToString(_packet.getTipo()) + "] para o outro server[UID=" + Convert.ToString(_server_uid) + "] com o Auth Server, mas nao conseguiu allocar memoria para o command buffer. Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.UNIT_AUTH_SERVER_CONNECT,
//                        1001, 0));
//                }

//                 cosh.command.buff = _packet.ReadBytes(cosh.command.size);

//                // Log
//                message_pool.push(new message("[unit_auth_server_connect::sendCommandToOtherServer][Log] Envia comando[ID=" + Convert.ToString(_packet.getTipo()) + "] para outro server[UID=" + Convert.ToString(_server_uid) + "] com o Auth Server.", type_msg.CL_FILE_LOG_AND_CONSOLE));

//                // Envia o comando para o Auth Server enviar para o outro server
//                var p = new packet((ushort)0x06);

//                p.WriteBuffer(cosh, Marshal.SizeOf(new CommandOtherServerHeader()));

//                if (command_buff_size > 0 && cosh.command.size > 0)
//                {
//                    p.WriteBuffer(cosh.command.buff, cosh.command.size);
//                }

//                packet_func_as.session_send(p,
//                    m_session, 1);

//            }
//            catch (exception e)
//            {

//                message_pool.push(new message("[unit_auth_server_connect::sendCommandToOtherServer][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
//            }
//        }

//        public virtual void sendReplyToOtherServer(uint _server_uid, packet _packet)
//        {

//            if (!isLive())
//            {
//                throw new exception("[unit_auth_server_connect::sendReplyToOtherServer][Error] Nao pode enviar a resposta[ID=" + Convert.ToString(_packet.getTipo()) + "] para o outro server[UID=" + Convert.ToString(_server_uid) + "] com o Auth Server, por que nao esta conectado com ele.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.UNIT_AUTH_SERVER_CONNECT,
//                    50, 0));
//            }

//            try
//            {


//                // Ler a Resposta ID para verificar se está tudo ok
//                _packet.ReadUInt16();

//                if (_packet.GetSize < 2)
//                {
//                    throw new exception("[unit_auth_server_connect::sendReplyToOtherServer][Error] Tentou enviar a reposta[ID=" + Convert.ToString(_packet.getTipo()) + "] para o outro server[UID=" + Convert.ToString(_server_uid) + "] com o Auth Server, mas o packet eh invalido nao tem nem o id.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.UNIT_AUTH_SERVER_CONNECT,
//                        1000, 0));
//                }

//                ushort command_buff_size = (ushort)(_packet.GetSize - 2u);

//                CommandOtherServerHeaderEx cosh = new CommandOtherServerHeaderEx();

//                cosh.send_server_uid_or_type = _server_uid;
//                cosh.command_id = _packet.Id;

//                // Inicializa comando buffer
//                cosh.command.init(command_buff_size);

//                if (!cosh.command.is_good())
//                {
//                    throw new exception("[unit_auth_server_connect::sendReplyToOtherServer][Error] Tentou enviar a reposta[ID=" + Convert.ToString(_packet.getTipo()) + "] para o outro server[UID=" + Convert.ToString(_server_uid) + "] com o Auth Server, mas nao conseguiu allocar memoria para o command buffer. Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.UNIT_AUTH_SERVER_CONNECT,
//                        1001, 0));
//                }

//                cosh.command.buff = _packet.ReadBytes(cosh.command.size);

//                // Log
//                message_pool.push(new message("[unit_auth_server_connect::sendReplyToOtherServer][Log] Envia resposta[ID=" + Convert.ToString(_packet.getTipo()) + "] para o outro server[UID=" + Convert.ToString(_server_uid) + "] com o Auth Server.", type_msg.CL_FILE_LOG_AND_CONSOLE));

//                // Envia a resposta para o Auth Server enviar para o outro server
//                var p = new packet((ushort)0x07);

//                p.WriteBuffer(cosh, Marshal.SizeOf(new CommandOtherServerHeader()));

//                if (command_buff_size > 0 && cosh.command.size > 0)
//                {
//                    p.WriteBuffer(cosh.command.buff, cosh.command.size);
//                }

//                packet_func_as.session_send(p,
//                    m_session, 1);

//            }
//            catch (exception e)
//            {

//                message_pool.push(new message("[unit_auth_server_connect::sendReplyToOtherServer][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
//            }
//        }

//        protected override void onHeartBeat()
//        {

//            // Aqui Faz a verificação se está connectado com o auth server
//            try
//            {

//                if (m_state != STATE.INITIALIZED)
//                {
//                    return;
//                } 

//                if (!m_session.isConnected())
//                {
//                    ConnectAndAssoc();
//                 }
//                else 
//                {
//                 message_pool.push(new message("[unit_auth_server_connect::onHeartBeat][Error] tentou esperar pelo evento de tentar conectar com o Auth Server, mas deu error", type_msg.CL_FILE_LOG_AND_CONSOLE));
//                }
//            }
//            catch (exception e)
//            {

//                message_pool.push(new message("[unit_auth_server_connect::onHearBeat][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
//            }
//        }

//        protected override void onConnected()
//        {

//            try
//            {

//                message_pool.push(new message("[unit_auth_server_connect::onConnected][Log] Connectou com o Auth Server: " + m_unit_ctx.ip + ":" + Convert.ToString(m_unit_ctx.port), type_msg.CL_ONLY_CONSOLE));

//            }
//            catch (exception e)
//            {

//                message_pool.push(new message("[unit_auth_server_connect::onConnected][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
//            }
//        }

//        protected override void onDisconnect()
//        {

//            try
//            {

//                // Log
//                message_pool.push(new message("[unit_auth_server_connect::onDisconnect][Log] Desconectou do Auth Server.", type_msg.CL_FILE_LOG_AND_CONSOLE));

//            }
//            catch (exception e)
//            {

//                message_pool.push(new message("[unit_auth_server_connect::onDisconnect][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
//            }
//        }

//        protected void config_init()
//        {
//            m_unit_ctx.ip = m_reader_ini.ReadString("AUTHSERVER", "IP");
//            m_unit_ctx.port = m_reader_ini.readInt("AUTHSERVER", "PORT");

//            // Carregou com sucesso
//            m_unit_ctx.state = true;
//        }

//        protected IUnitAuthServer m_owner_server;
//    }
//}
