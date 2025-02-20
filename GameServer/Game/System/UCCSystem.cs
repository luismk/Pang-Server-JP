using GameServer.Cmd;
using GameServer.GameType;
using PangyaAPI.Network.PangyaPacket;
using PangyaAPI.SQL.Manager;
using PangyaAPI.Utilities.Log;
using PangyaAPI.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameServer.Session;
using PangyaAPI.Utilities.BinaryModels;
using _smp = PangyaAPI.Utilities.Log;
namespace GameServer.Game.System
{
    public static class UCCSystem
    {
        public static void HandleUCC(this Player _session, Packet _packet)
        {
            var p = new PangyaBinaryWriter();
            try
            {

                byte opt = _packet.ReadUInt8();

                // Verifica se session está varrizada para executar esse ação, 
                // se ele não fez o login com o Server ele não pode fazer nada até que ele faça o login
                //CHECK_SESSION_IS_AUTHORIZED("UCCSystem");

                switch (opt)
                {
                    case 0: // Salva para sempre[definitivo]
                        {
                            uint ucc_typeid = _packet.ReadUInt32();
                            string ucc_idx = _packet.ReadString();
                            string ucc_name = _packet.ReadString();

                            // INICIO CHECK UCC VALID FOR SERVER
                            if (sIff.getInstance().getItemGroupIdentify(ucc_typeid) != 2)
                                throw new exception("[GameServer.requestUCCSystem][Error] player[UID=" + (_session.m_pi.uid) + "] tentou salvar definitivo a UCC[TYPEID="
                                    + (ucc_typeid) + ", IDX=" + ucc_idx + "], mas o UCC nao eh um part valido. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.GAME_SERVER, 13, 0x5200113));

                            var part = sIff.getInstance().findPart(ucc_typeid);

                            if (part == null)
                                throw new exception("[GameServer.requestUCCSystem][Error] player[UID=" + (_session.m_pi.uid) + "] tentou salvar definitivo a UCC[TYPEID="
                                    + (ucc_typeid) + ", IDX=" + ucc_idx + "], mas nao tem a UCC no IFF_STRUCT do Server. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.GAME_SERVER, 11, 0x5200111));

                            if (!part.IsUCC())
                                throw new exception("[GameServer.requestUCCSystem][Error] player[UID=" + (_session.m_pi.uid) + "] tentou salvar definitivo a UCC[TYPEID="
                                    + (ucc_typeid) + ", IDX=" + ucc_idx + "], mas nao eh uma UCC valida. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.GAME_SERVER, 12, 0x5200112));
                            // FIM CHECK UCC VALID FOR SERVER

                            if (ucc_typeid == 0)
                                throw new exception("[GameServer.requestUCCSystem][Error] player[UID=" + (_session.m_pi.uid) + "] tentou salvar definitivo a UCC[TYPEID="
                                    + (ucc_typeid) + ", IDX=" + ucc_idx + "], mas o typeid is invalid. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.GAME_SERVER, 4, 0x5200104));

                            if (ucc_idx.empty())
                                throw new exception("[GameServer.requestUCCSystem][Error] player[UID=" + (_session.m_pi.uid) + "] tentou salvar definitivo a UCC[TYPEID="
                                    + (ucc_typeid) + ", IDX=" + ucc_idx + "], mas o idx eh invalido. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.GAME_SERVER, 6, 0x5200106));

                            if (ucc_name.empty())
                                throw new exception("[GameServer.requestUCCSystem][Error] player[UID=" + (_session.m_pi.uid) + "] tentou salvar definitivo a UCC[TYPEID="
                                        + (ucc_typeid) + ", IDX=" + ucc_idx + "], mas o name eh invalido. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.GAME_SERVER, 7, 0x5200107));

                            // Save definitivo UCC

                            // UPDATE ON SERVER
                            var it = _session.m_pi.mp_wi.FirstOrDefault(el => el.Value._typeid == ucc_typeid &&
                           (string.IsNullOrEmpty(el.Value.ucc.name) || el.Value.ucc.name == "0") &&
                           el.Value.ucc.idx == ucc_idx);

                            if (it.Value == null)
                                throw new Exception($"[GameServer.requestUCCSystem][Error] player[UID={_session.m_pi.uid}] tentou salvar definitivo a UCC[TYPEID={ucc_typeid}, IDX={ucc_idx}], mas ele não tem essa UCC. Hacker ou Bug");

                            // TEMPORARY 2, FOREVER 1
                            it.Value.ucc.status = 1; // Definitivo
                            it.Value.ucc.name = ucc_name;
                            it.Value.ucc.copier_nick = _session.m_pi.nickname;
                            it.Value.ucc.copier = _session.m_pi.uid;

                            // Date
                            DateTime si = DateTime.Now;

                            // UPDATE ON DB
                            var cmd_uu = new CmdUpdateUCC(_session.m_pi.uid, it.Value, new PangyaTime(1), CmdUpdateUCC.T_UPDATE.FOREVER);   // Waiter

                            NormalManagerDB.add(0, cmd_uu, null, null);

                            if (cmd_uu.getException().getCodeError() != 0)
                                throw cmd_uu.getException();

                            // Log
                            _smp::message_pool.push(new message("[UCC::Self Design System][Log] player[UID=" + (_session.m_pi.uid) + "] salvo definitivo a UCC[TYPEID="
                                + (it.Value._typeid) + ", ID=" + (it.Value.id) + ", IDX=" + (it.Value.ucc.idx) + ", NAME=" + (it.Value.ucc.name) + "]", type_msg.CL_FILE_LOG_AND_CONSOLE));

                            // UPDATE ON GAME
                            p.init_plain(0x12E);

                            p.WriteByte(opt);

                            p.WriteByte(1);    // no outro fala que � op��o de erro, mas n�o sei n�o

                            p.WriteUInt32(it.Value.id);
                            p.WriteUInt32(it.Value._typeid);
                            p.WritePStr(it.Value.ucc.idx);
                            p.WritePStr(it.Value.ucc.name);
                            _session.Send(p);
                            break;
                        }
                    case 1: // Info
                        {
                            var ucc_id = _packet.ReadUInt32();
                            var owner = _packet.ReadUInt8();  // acho que 1 � do pr�prio player, 0 de outro player
                                                             
                            if (ucc_id <= 0)
                                throw new exception("[GameServer.requestUCCSystem][Error] player[UID=" + (_session.m_pi.uid) + "] tentou ver info da UCC[ID="
                                        + (ucc_id) + "], mas o id da ucc eh invalido. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.GAME_SERVER, 2, 0x5200102));

                            var pWi = _session.m_pi.findWarehouseItemById(ucc_id);

                            // N�o achou o UCC no Player, tenta no DB para ver se � de outro player
                            // Por Hora envia uma Exception
                            if (pWi == null)
                            {

                                var cmd_fu = new CmdFindUCC(ucc_id);    // Waiter

                                NormalManagerDB.add(0, cmd_fu, null, null);

                                if (cmd_fu.getException().getCodeError() != 0)
                                    throw cmd_fu.getException();

                                if (cmd_fu.getInfo().id <= 0)
                                    throw new exception("[GameServer.requestUCCSystem][Error] player[UID=" + (_session.m_pi.uid) + "] tentou ver info da UCC[ID="
                                            + (ucc_id) + "], mas nao encontrou essa UCC. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.GAME_SERVER, 3, 0x5200103));

                                pWi = cmd_fu.getInfo();            
                            }

                            // Log      
                            _smp::message_pool.push(new message("[UCCSystem.HandleUCC][Log] player[UID=" + (_session.m_pi.uid) + "] pediu info da ucc[TYPEID="
                                    + (pWi._typeid) + ", ID=" + (pWi.id) + "]" + ", UCCID=" + (ucc_id) + "]", type_msg.CL_FILE_LOG_AND_CONSOLE));

                            // UPDATE ON GAME
                            p.init_plain(0x12E);

                            p.WriteByte(opt);

                            p.WriteUInt32(pWi._typeid);
                            p.WritePStr(pWi.ucc.idx);
                            p.WriteByte(owner);

                            p.WriteBytes(pWi.Build());
                            _session.Send(p);

                            break;
                        }
                    case 2: // C�piar
                        {
                            uint ucc_typeid = _packet.ReadUInt32();
                            string ucc_idx = _packet.ReadString();
                            ushort seq = _packet.ReadUInt16();
                            uint cpy_id = _packet.ReadUInt32();

                            // INICIO CHECK UCC VALID FOR SERVER
                            if (sIff.getInstance().getItemGroupIdentify(ucc_typeid) != 2)
                                throw new exception("[GameServer.requestUCCSystem][Error] player[UID=" + (_session.m_pi.uid) + "] tentou copiar a UCC[TYPEID="
                                    + (ucc_typeid) + ", IDX=" + ucc_idx + "] para UCC_CPY[ID=" + (cpy_id) + "], mas o UCC nao eh um part valido. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.GAME_SERVER, 13, 0x5200113));

                            var part = sIff.getInstance().findPart(ucc_typeid);

                            if (part == null)
                                throw new exception("[GameServer.requestUCCSystem][Error] player[UID=" + (_session.m_pi.uid) + "] tentou copiar a UCC[TYPEID="
                                    + (ucc_typeid) + ", IDX=" + ucc_idx + "] para UCC_CPY[ID=" + (cpy_id) + "], mas nao tem a UCC no IFF_STRUCT do Server. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.GAME_SERVER, 11, 0x5200111));

                            if (!part.IsUCC())
                                throw new exception("[GameServer.requestUCCSystem][Error] player[UID=" + (_session.m_pi.uid) + "] tentou copiar a UCC[TYPEID="
                                    + (ucc_typeid) + ", IDX=" + ucc_idx + "] para UCC_CPY[ID=" + (cpy_id) + "], mas nao eh uma UCC valida. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.GAME_SERVER, 12, 0x5200112));
                            // FIM CHECK UCC VALID FOR SERVER

                            if (ucc_typeid == 0)
                                throw new exception("[GameServer.requestUCCSystem][Error] player[UID=" + (_session.m_pi.uid) + "] tentou copiar a UCC[TYPEID="
                                    + (ucc_typeid) + ", IDX=" + ucc_idx + "] para UCC_CPY[ID=" + (cpy_id) + "], mas o typeid is invalid. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.GAME_SERVER, 4, 0x5200104));

                            if (ucc_idx.empty())
                                throw new exception("[GameServer.requestUCCSystem][Error] player[UID=" + (_session.m_pi.uid) + "] tentou copiar a UCC[TYPEID="
                                    + (ucc_typeid) + ", IDX=" + ucc_idx + "] para UCC_CPY[ID=" + (cpy_id) + "], mas o idx eh invalido. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.GAME_SERVER, 6, 0x5200106));

                            if (seq == 0)
                                throw new exception("[GameServer.requestUCCSystem][Error] player[UID=" + (_session.m_pi.uid) + "] tentou copiar a UCC[TYPEID="
                                        + (ucc_typeid) + ", IDX=" + ucc_idx + "] para UCC_CPY[ID=" + (cpy_id) + "], mas seq[value=" + (seq) + "] is invalid. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.GAME_SERVER, 8, 0x5200108));

                            if (cpy_id <= 0)
                                throw new exception("[GameServer.requestUCCSystem][Error] player[UID=" + (_session.m_pi.uid) + "] tentou copiar a UCC[TYPEID="
                                        + (ucc_typeid) + ", IDX=" + ucc_idx + "] para UCC_CPY[ID=" + (cpy_id) + "], mas o copy_id is invalid. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.GAME_SERVER, 9, 0x5200109));

                            var pWi = _session.m_pi.findWarehouseItemById(cpy_id);

                            if (pWi == null)
                                throw new exception("[GameServer.requestUCCSystem][Error] player[UID=" + (_session.m_pi.uid) + "] tentou copiar a UCC[TYPEID="
                                        + (ucc_typeid) + ", IDX=" + ucc_idx + "] para UCC_CPY[ID=" + (cpy_id) + "], mas o ele nao tem a UCC_CPY, Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.GAME_SERVER, 10, 0x5200110));

                            // INICIO CHECK UCC VALID FOR SERVER
                            if (sIff.getInstance().getItemGroupIdentify(pWi._typeid) != 2)
                                throw new exception("[GameServer.requestUCCSystem][Error] player[UID=" + (_session.m_pi.uid) + "] tentou copiar a UCC[TYPEID="
                                        + (ucc_typeid) + ", IDX=" + ucc_idx + "] para UCC_CPY[ID=" + (cpy_id) + "], mas o UCC nao eh um part valido. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.GAME_SERVER, 13, 0x5200113));

                            part = sIff.getInstance().findPart(pWi._typeid);

                            if (part == null)
                                throw new exception("[GameServer.requestUCCSystem][Error] player[UID=" + (_session.m_pi.uid) + "] tentou copiar a UCC[TYPEID="
                                        + (ucc_typeid) + ", IDX=" + ucc_idx + "] para UCC_CPY[ID=" + (cpy_id) + "], mas nao tem a UCC no IFF_STRUCT do Server. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.GAME_SERVER, 11, 0x5200111));

                            if (!part.IsUCC())
                                throw new exception("[GameServer.requestUCCSystem][Error] player[UID=" + (_session.m_pi.uid) + "] tentou copiar a UCC[TYPEID="
                                        + (ucc_typeid) + ", IDX=" + ucc_idx + "] para UCC_CPY[ID=" + (cpy_id) + "], mas nao eh uma UCC valida. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.GAME_SERVER, 12, 0x5200112));
                            // FIM CHECK UCC VALID FOR SERVER

                            // Copiar UCC

                            // UPDATE ON SERVER
                            var it = _session.m_pi.mp_wi.FirstOrDefault(el => el.Value._typeid == ucc_typeid &&
                         (string.IsNullOrEmpty(el.Value.ucc.name) || el.Value.ucc.name == "0") &&
                         el.Value.ucc.idx == ucc_idx);

                            if (it.Value == null)
                                throw new exception("[GameServer.requestUCCSystem][Error] player[UID=" + (_session.m_pi.uid) + "] tentou copiar a UCC[TYPEID="
                                        + (ucc_typeid) + ", IDX=" + ucc_idx + "], mas ele nao tem essa UCC. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.GAME_SERVER, 5, 0x5200105));


                            // Copia permanente
                            pWi.ucc.status = 1;
                            pWi.ucc.idx = it.Value.ucc.idx;
                            pWi.ucc.name = it.Value.ucc.name;
                            pWi.ucc.copier_nick = _session.m_pi.nickname;
                            pWi.ucc.copier = _session.m_pi.uid;

                            // Date
                            PangyaTime draw_dt = new PangyaTime();

                            draw_dt.CreateTime();

                            // UPDATE ON DB
                            var cmd_uu = new CmdUpdateUCC(_session.m_pi.uid, pWi, draw_dt, CmdUpdateUCC.T_UPDATE.COPY);   // Waiter

                            NormalManagerDB.add(0, cmd_uu, null, null);

                            if (cmd_uu.getException().getCodeError() != 0)
                                throw cmd_uu.getException();

                            pWi = cmd_uu.getInfo();

                            // Log
                            _smp::message_pool.push(new message("[UCC::Self Design System][Log] player[UID=" + (_session.m_pi.uid) + "] fez um copia da UCC[TYPEID="
                                    + (it.Value._typeid) + ", ID=" + (it.Value.id) + ", IDX=" + (it.Value.ucc.idx) + "] na UCC_CPY[TYPEID="
                                    + (pWi._typeid) + ", ID=" + (pWi.id) + "]", type_msg.CL_FILE_LOG_AND_CONSOLE));

                            // UPDATE ON GAME
                            p.init_plain(0x12E);

                            p.WriteByte(opt);

                            p.WriteUInt32(it.Value._typeid);
                            p.WritePStr(it.Value.ucc.idx);
                            p.WriteUInt16(it.Value.ucc.seq);

                            p.WriteUInt32(pWi.id);
                            p.WriteUInt32(pWi.id);
                            p.WriteUInt32(pWi._typeid);
                            p.WritePStr(pWi.ucc.idx);
                            p.WriteUInt16(pWi.ucc.seq);

                            p.WriteByte(1);    // no outro fala que � op��o de erro, mas n�o sei n�o
                            _session.Send(p);
                            break;
                        }
                    case 3: // Salve tempor�rio
                        {
                            uint ucc_typeid = _packet.ReadUInt32();
                            string ucc_idx = _packet.ReadString();

                            // INICIO CHECK UCC VALID FOR SERVER
                            if (sIff.getInstance().getItemGroupIdentify(ucc_typeid) != 2)
                                throw new exception("[GameServer.requestUCCSystem][Error] player[UID=" + (_session.m_pi.uid) + "] tentou salvar temporario a UCC[TYPEID="
                                    + (ucc_typeid) + ", IDX=" + ucc_idx + "], mas o UCC nao eh um part valido. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.GAME_SERVER, 13, 0x5200113));

                            var part = sIff.getInstance().findPart(ucc_typeid);

                            if (part == null)
                                throw new exception("[GameServer.requestUCCSystem][Error] player[UID=" + (_session.m_pi.uid) + "] tentou salvar temporario a UCC[TYPEID="
                                    + (ucc_typeid) + ", IDX=" + ucc_idx + "], mas nao tem a UCC no IFF_STRUCT do Server. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.GAME_SERVER, 11, 0x5200111));

                            if (!part.IsUCC())
                                throw new exception("[GameServer.requestUCCSystem][Error] player[UID=" + (_session.m_pi.uid) + "] tentou salvar temporario a UCC[TYPEID="
                                    + (ucc_typeid) + ", IDX=" + ucc_idx + "], mas nao eh uma UCC valida. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.GAME_SERVER, 12, 0x5200112));
                            // FIM CHECK UCC VALID FOR SERVER

                            if (ucc_typeid == 0)
                                throw new exception("[GameServer.requestUCCSystem][Error] player[UID=" + (_session.m_pi.uid) + "] tentou salvar temporario a UCC[TYPEID="
                                        + (ucc_typeid) + ", IDX=" + ucc_idx + "], mas o typeid is invalid. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.GAME_SERVER, 4, 0x5200104));

                            if (ucc_idx.empty())
                                throw new exception("[GameServer.requestUCCSystem][Error] player[UID=" + (_session.m_pi.uid) + "] tentou salvar temporario a UCC[TYPEID="
                                        + (ucc_typeid) + ", IDX=" + ucc_idx + "], mas o idx eh invalido. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.GAME_SERVER, 6, 0x5200106));

                            // Save tempor�rio UCC

                            // UPDATE ON SERVER
                            var it = _session.m_pi.mp_wi.FirstOrDefault(el => el.Value._typeid == ucc_typeid &&
                       (string.IsNullOrEmpty(el.Value.ucc.name) || el.Value.ucc.name == "0") &&
                       el.Value.ucc.idx == ucc_idx);

                            if (it.Value == null)
                                throw new exception("[GameServer.requestUCCSystem][Error] player[UID=" + (_session.m_pi.uid) + "] tentou salvar temporario a UCC[TYPEID="
                                        + (ucc_typeid) + ", IDX=" + ucc_idx + "], mas ele nao tem essa UCC. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.GAME_SERVER, 5, 0x5200105));

                            // TEMPORARY 2, FOREVER 1
                            it.Value.ucc.status = 2;       // Tempor�rio
                            it.Value.ucc.name = "0";
                            // UPDATE ON DB
                            var cmd_uu = new Cmd.CmdUpdateUCC(_session.m_pi.uid, it.Value, new PangyaTime(1), CmdUpdateUCC.T_UPDATE.TEMPORARY);   // Waiter

                            NormalManagerDB.add(0, cmd_uu, null, null);

                            if (cmd_uu.getException().getCodeError() != 0)
                                throw cmd_uu.getException();

                            // Log
                            _smp::message_pool.push(new message("[UCC::Self Design System][Log] player[UID=" + (_session.m_pi.uid) + "] salvo temporario a UCC[TYPEID="
                                    + (it.Value._typeid) + ", ID=" + (it.Value.id) + ", IDX=" + (it.Value.ucc.idx) + "]", type_msg.CL_FILE_LOG_AND_CONSOLE));

                            // UPDATE ON GAME
                            p.init_plain(0x12E);

                            p.WriteByte(opt);

                            p.WriteUInt32(it.Value._typeid);
                            p.WritePStr(it.Value.ucc.idx);
                            p.WriteByte(1);    // no outro fala que � op��o de erro, mas n�o sei n�o

                            _session.Send(p);

                            break;
                        }
                    default:
                        throw new exception("[GameServer.requestUCCSystem][Error] player[UID=" + (_session.m_pi.uid) + "] tentou usar UCC System, mas forneceu uma option desconhecida. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.GAME_SERVER, 1, 0x5200101));
                }
            }
            catch (exception e)
            {

                _smp::message_pool.push(new message("[GameServer.requestUCCSystem][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                p.init_plain(0x12E);

                p.WriteByte(-1);    // Error

                _session.Send(p);
            }
        }
    }
}
