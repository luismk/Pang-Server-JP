using _smp = PangyaAPI.Utilities.Log;
using PangyaAPI.SQL;
using System;
using System.Collections.Generic;
using System.Linq;
using PangyaAPI.Utilities;
using PangyaAPI.Network.PangyaSession;
using GameServer.GameType;

namespace GameServer.Session
{
    public class player_manager : SessionManager
    {
        class uIndexOID
        {

            public byte ucFlag;
            public struct stFlag
            {
                public byte busy;
                public byte block;
            }
            public stFlag flag;

            public byte getFlag()
            { return ucFlag; }
        }

        SortedList<uint, uIndexOID> m_indexes;		// Index de OID

        public player_manager(uint _max_session) : base(_max_session)
        {
            if (_max_session != 0)
            {
                m_indexes = new SortedList<uint, uIndexOID>();
                for (var i = 0u; i < _max_session; ++i)
                    m_sessions.Add(new Player() { m_oid = uint.MaxValue });
            }
        }


        public new void Clear()
        {
            base.Clear();
            if (m_indexes != null && m_indexes.Count > 0)
                m_indexes.Clear();
        }

        public Player findPlayer(uint? _uid, bool _oid = true)
        {

            foreach (Player el in m_sessions)
            {
                if ((_oid ? el.getUID() : el.m_oid) == _uid)
                {
                    return el;
                }
            }


            return null;
        }
             
        public override SessionBase FindSessionByOid(uint oid)
        {
            SessionBase session = null;
            foreach (var el in m_sessions.Where(el => el._client != null))
            {
                if (el.m_oid == oid)
                    session = el;
            }
            return session;
        }

        public override SessionBase FindSessionByUid(uint uid)
        {
            SessionBase session = null;
            lock (_lockObject)
            {
                session = m_sessions.FirstOrDefault(el => el._client != null && el.getUID() == uid);
            }
            return session;
        }

        public override List<SessionBase> FindAllSessionByUid(uint uid)
        {
            List<SessionBase> sessions = new List<SessionBase>();
            lock (_lockObject)
            {
                sessions = m_sessions.Where(el => el._client != null && el.getUID() == uid).ToList();
            }
            return sessions;
        }

        public override SessionBase FindSessionByNickname(string nickname)
        {
            SessionBase session = null;
            lock (_lockObject)
            {
                session = m_sessions.FirstOrDefault(el => el._client != null && el.Nickname == nickname);
            }
            return session;
        }
        // Override methods
        public override bool DeleteSession(SessionBase _session)
        {

            if (_session == null)
                throw new exception("[player_manager::deleteSession][ERR_SESSION] _session is null.");



            // Block SessionBase

            uint tmp_oid = _session.m_oid;

            bool ret;
            if ((ret = _session.Clear()))
            {

                // Libera OID
                freeOID(tmp_oid/*_session.m_oid*/);

                m_count--;
            }
            return ret;
        }

        public void checkPlayersItens() { }

        public void blockOID(uint _oid)
        {
            var it = m_indexes.Where(c => c.Key == _oid).FirstOrDefault();

            if (it.Value != null)
                it.Value.flag.block = 1;	// Block

        }

        public void unblockOID(uint _oid)
        {
            var it = m_indexes.Where(c => c.Key == _oid).FirstOrDefault();

            if (it.Value != null)
                it.Value.flag.block = 1;	// unblock
        }

        public static void checkItemBuff(Player _session) { }
        public static void checkCardSpecial(Player _session) { }
        public static void checkCaddie(Player _session)
        {
            // Caddie
            foreach (var el in _session.m_pi.mp_ci.Values)
            {      
                // Caddie por tempo
                if (el.rent_flag == 2 && UtilTime.GetLocalDateDiffDESC(el.end_date.ConvertTime()) <= 0)
                {

                    // Put Update Item on vector update item of player
                    if ((_session.m_pi.findUpdateItemByTypeidAndType(el.id, UpdateItem.UI_TYPE.CADDIE).Values) != null)
                    {
                        _session.m_pi.mp_ui.Add(new PlayerInfo.stIdentifyKey(el._typeid, el.id), new UpdateItem(UpdateItem.UI_TYPE.CADDIE, el._typeid, el.id));

                        // Verifica se o Caddie está equipado e desequipa
                        if ((_session.m_pi.ei.cad_info != null && _session.m_pi.ei.cad_info.id == el.id) || _session.m_pi.ue.caddie_id == el.id)
                        {

                            _session.m_pi.ei.cad_info = null;
                            _session.m_pi.ue.caddie_id = 0;
                        }
                    }
                }

                // Parts Caddie End Date
                if (el.parts_typeid != 0u && !el.end_parts_date.IsEmpty && UtilTime.GetLocalDateDiffDESC(el.end_parts_date.ConvertTime()) <= 0)
                {

                    // Put Update Item on vector update item of player
                    if (_session.m_pi.findUpdateItemByTypeidAndType(el.id, UpdateItem.UI_TYPE.CADDIE_PARTS) != null)
                    {

                        _session.m_pi.mp_ui.Add(new PlayerInfo.stIdentifyKey(el._typeid, el.id), new UpdateItem(UpdateItem.UI_TYPE.CADDIE_PARTS, el._typeid, el.id));

                        el.parts_typeid = 0u;
                        el.parts_end_date_unix = 0;
                        el.end_parts_date = new PangyaTime();

                        //snmdb::NormalManagerDB::getInstance().add(1, new CmdUpdateCaddieInfo(_session.m_pi.uid, el.second), player_manager::SQLDBResponse, nullptr);
                    }
                }
            }

        }
        public static void checkMascot(Player _session) { }
        public static void checkWarehouse(Player _session) { }

        // Sem proteção de sincronização, chamar ela em uma função thread safe(thread com seguranção de sincronização)
        public override uint findSessionFree()
        {
            for (var i = 0; i < m_sessions.Count; ++i)
                if (m_sessions[i].m_oid == uint.MaxValue)
                    return getNewOID();

            return uint.MaxValue;
        }

        // Sem proteção de sincronização, chamar ela em uma função thread safe(thread com seguranção de sincronização)
        public uint getNewOID()
        {
            uint oid = 0u;

            // Find a index OID FREE
            var it = m_indexes.Where(c => c.Value.ucFlag == 0).FirstOrDefault();

            if (it.Value != null)
            {   // Achei 1 index desocupado

                it.Value.flag.busy = 1; // BUSY OCUPDADO

                oid = it.Key;

            }
            else
            {   // Add um novo index no mapa de oid

                oid = (uint)m_indexes.Count;

                m_indexes[oid] = new uIndexOID() { ucFlag = 1 };
            }
            return oid;
        }

        public void freeOID(uint _oid)
        {
            var it = m_indexes.Where(c => c.Key == _oid).FirstOrDefault();

            if (it.Value != null)
            {

                it.Value.flag.busy = 0; // WAITING DESOCUPADO LIVRE

                if (Convert.ToBoolean(it.Value.flag.block))
                    _smp.message_pool.push("[player_manager::freeOID][WARNING] index[OID=" + (it.Key) + "] esta bloqueado, nao pode liberar ele agora");
            }
            else
                _smp.message_pool.push("[player_manager::freeOID][WARNING] index[OID=" + (_oid) + "] nao esta no mapa.");

        }

        public void SQLDBResponse(uint _msg_id, Pangya_DB _pangya_db, object _arg)
        {

            if (_arg == null)
            {
                // Static Functions of Class
                _smp.message_pool.push(("[player_manager::SQLDBResponse]WARNING] _arg is null"));
                return;
            }

            // Por Hora só sai, depois faço outro tipo de tratamento se precisar
            if (_pangya_db.getException().getCodeError() != 0)
            {
                _smp.message_pool.push(("[player_manager::SQLDBResponse][Error] " + _pangya_db.getException().getFullMessageError()));
                return;
            }

            //var pm = reinterpret_cast< player_manager* >(_arg);

            switch (_msg_id)
            {
                case 1: // Update Caddie Info
                    {
                        //var cmd_uci = (CmdUpdateCaddieInfo)(_pangya_db);

                        //_smp.message_pool.push(("[player_manager::SQLDBResponse][Log] player[UID=" + (cmd_uci.getUID()) + "] Atualizou Caddie Info[TYPEID="
                        //        + (cmd_uci.getInfo()._typeid) + ", ID=" + (cmd_uci.getInfo().id) + ", PARTS_TYPEID=" + (cmd_uci.getInfo().parts_typeid)
                        //        + ", END_DATE=" + _formatDate(cmd_uci.getInfo().end_date) + ", PARTS_END_DATE=" + _formatDate(cmd_uci.getInfo().end_parts_date) + "] com sucesso!", CL_FILE_LOG_AND_CONSOLE));

                        break;
                    }
                case 2: // Update All parts of Character
                    {
                        break;
                    }
                case 0:
                default:
                    break;
            }
        }
    }
}