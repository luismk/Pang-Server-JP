using _smp = PangyaAPI.Utilities.Log;
using PangyaAPI.SQL;
using System;
using System.Collections.Generic;
using System.Linq;
using PangyaAPI.Utilities;
using PangyaAPI.Network.PangyaSession;

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
                for (var i = 0u; i < _max_session; ++i)
                    m_sessions.Add(new Player() { m_oid = uint.MaxValue });
            }
        }


        public new void Clear()
        {
            base.Clear();
            if (m_indexes !=null && m_indexes.Count >0)
                m_indexes.Clear();
        }

        public Player findPlayer(uint? _uid, bool _oid = false)
        {
            Player _player = null;

            foreach (var el in m_sessions)
            {
                if ((!Convert.ToBoolean(_oid) ? (uint)el.getUID() : el.m_oid) == _uid)
                {
                    _player = (Player)el;
                    break;
                }
            }


            return _player;
        }

        public List<Player> findAllGM()
        {
            List<Player> v_gm = new List<Player>();

            foreach (var el in m_sessions)
            {
                if (Convert.ToBoolean(((Player)el).getCapability() & 4) || Convert.ToBoolean(((Player)el).getCapability() & 128/*GM Player Normal*/))    // GM
                    v_gm.Add((Player)el);
            }

            return v_gm;
        }

        // Override methods
        public bool deleteSession(Player _session)
        {
            if (_session == null)
                throw new exception("[player_manager::deleteSession][ERR_SESSION] _session is nullptr.");

            if (!_session.getConnected())
                throw new exception("[player_manager::deleteSession][ERR_SESSION] SESSION[IP=" + _session.getIP() + ", UID="
                        + ((_session).m_pi.uid) + ", OID=" + (_session.m_oid)
                        + "] _seession not connected.");

            bool ret = true;


            if (_session == null)
            {


                throw new exception("[player_manager::deleteSession][ERR_SESSION] SESSION[IP=" + _session.getIP() + ", UID="
                        + _session.m_pi.uid + ", OID=" + _session.m_oid
                        + "] _seession not connected.");
            }

            // Block SessionBase

            uint tmp_oid = _session.m_oid;
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
            var it = m_indexes.Where(c=> c.Key == _oid).FirstOrDefault();

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
        public static void checkCaddie(Player _session) { }
        public static void checkMascot(Player _session) { }
        public static void checkWarehouse(Player _session) { }

        // Sem proteção de sincronização, chamar ela em uma função thread safe(thread com seguranção de sincronização)
        public uint findSessionFree()
        {
            for (var i = 0; i < m_sessions.Count; ++i)
                if (m_sessions[i] == null)
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
                _smp.message_pool.push(("[player_manager::SQLDBResponse]WARNING] _arg is nullptr"));
                return;
            }

            // Por Hora só sai, depois faço outro tipo de tratamento se precisar
            if (_pangya_db.getException().getCodeError() != 0)
            {
                _smp.message_pool.push(("[player_manager::SQLDBResponse][Error] " + _pangya_db.getException().getFullMessageError()));
                return;
            }

            //auto pm = reinterpret_cast< player_manager* >(_arg);

            switch (_msg_id)
            {
                case 1: // Update Caddie Info
                    {
                        //var cmd_uci = (CmdUpdateCaddieInfo)(_pangya_db);

                        //_smp.message_pool.push(("[player_manager::SQLDBResponse][Log] player[UID=" + std::to_string(cmd_uci->getUID()) + "] Atualizou Caddie Info[TYPEID="
                        //        + std::to_string(cmd_uci->getInfo()._typeid) + ", ID=" + std::to_string(cmd_uci->getInfo().id) + ", PARTS_TYPEID=" + std::to_string(cmd_uci->getInfo().parts_typeid)
                        //        + ", END_DATE=" + _formatDate(cmd_uci->getInfo().end_date) + ", PARTS_END_DATE=" + _formatDate(cmd_uci->getInfo().end_parts_date) + "] com sucesso!", CL_FILE_LOG_AND_CONSOLE));

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