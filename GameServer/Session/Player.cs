using GameServer.Cmd;
using GameServer.Game.Manager;
using GameServer.GameServerTcp;
using GameServer.GameType;
using PangLib.IFF.JP.Models.Data;
using PangyaAPI.Network.Pangya_St;
using PangyaAPI.Network.PangyaSession;
using PangyaAPI.SQL.Manager;
using PangyaAPI.Utilities;
using PangyaAPI.Utilities.BinaryModels;
using PangyaAPI.Utilities.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

namespace GameServer.Session
{
    public class Player : SessionBase
    {
        public PlayerInfo m_pi { get; set; }
        public GMInfo m_gi { get; set; }
        public Player()
        {
            m_pi = new PlayerInfo();
            m_gi = new GMInfo();
        }
        public override string getNickname()
        {
            return m_pi.nickname;
        }

        public override uint getUID()
        {
            return m_pi.uid;
        }

        public override string getID()
        {
            return m_pi.id;
        }

        public override uint getCapability() { return (uint)m_pi.m_cap.ulCapability; }

        public override bool Clear()
        {
            bool ret;
            if ((ret = base.Clear()))
            {

                // Player Info
                m_pi.clear();

                // Game Master Info
                m_gi.clear();
            }
            return ret;
        }

        #region Send Packets
        public void Send(List<PangyaBinaryWriter> packet, bool debug_log = false)
        {
            for (int i = 0; i < packet.Count; i++)
                base.Send(packet[i], debug_log);
        }
        public override void Send(PangyaBinaryWriter packet, bool debug_log = false)
        {
            base.Send(packet, debug_log);
        }

        public override void Send(byte[] Data, bool debug_log = false)
        {
            base.Send(Data, debug_log);
        }

        public void SendChannel_broadcast(byte[] p)
        {
            List<Player> channel_session = Program.gs.findChannel(m_pi.channel).getSessions();

            for (var i = 0; i < channel_session.Count; ++i)
            {
                channel_session[i].Send(p);
            }
        }

        public void SendLobby_broadcast(byte[] p)
        {
            List<Player> channel_session = Program.gs.findChannel(m_pi.channel).getSessions();

            for (var i = 0; i < channel_session.Count; ++i)
            {
                if (channel_session[i].m_pi.mi.sala_numero == ushort.MaxValue)
                    channel_session[i].Send(p);
            }
        }

        public void SendChannel_broadcast(List<PangyaBinaryWriter> p)
        {
            List<Player> channel_session = Program.gs.findChannel(m_pi.channel).getSessions();

            for (var i = 0; i < channel_session.Count; ++i)
            {
                channel_session[i].Send(p);
            }
        }

        public void SendLobby_broadcast(List<PangyaBinaryWriter> p)
        {
            List<Player> channel_session = Program.gs.findChannel(m_pi.channel).getSessions();

            for (var i = 0; i < channel_session.Count; ++i)
            {
                if (channel_session[i].m_pi.mi.sala_numero == ushort.MaxValue)
                    channel_session[i].Send(p);//@!errado
            }
        }
        #endregion
        public void checkAllItemEquiped(UserEquip _ue)
        {

            //    if (checkSkinEquiped(_ue))
            //        NormalManagerDB.add(0, new CmdUpdateSkinEquiped(m_pi.m_uid, _ue), player::SQLDBResponse, this);

            //    if (checkPosterEquiped(_ue))
            //        NormalManagerDB.add(0, new CmdUpdatePosterEquiped(m_pi.m_uid, _ue), player::SQLDBResponse, this);

            //    if (checkCharacterEquiped(_ue))
            //        NormalManagerDB.add(0, new CmdUpdateCharacterEquiped(m_pi.m_uid, _ue.character_id), player::SQLDBResponse, this);

            //    if (checkCaddieEquiped(_ue))
            //        NormalManagerDB.add(0, new CmdUpdateCaddieEquiped(m_pi.m_uid, _ue.caddie_id), player::SQLDBResponse, this);

            //    if (checkMascotEquiped(_ue))
            //        NormalManagerDB.add(0, new CmdUpdateMascotEquiped(m_pi.m_uid, _ue.mascot_id), player::SQLDBResponse, this);

            //    if (checkItemEquiped(_ue))
            //        NormalManagerDB.add(0, new CmdUpdateItemSlot(m_pi.m_uid, (uint*)_ue.item_slot), player::SQLDBResponse, this);

            //    if (checkClubSetEquiped(_ue))
            //        NormalManagerDB.add(0, new CmdUpdateClubsetEquiped(m_pi.m_uid, _ue.clubset_id), player::SQLDBResponse, this);

            //    if (checkBallEquiped(_ue))
            //        NormalManagerDB.add(0, new CmdUpdateBallEquiped(m_pi.m_uid, _ue.ball_typeid), player::SQLDBResponse, this);
        }

        public void equipDefaultCharacter(UserEquip _ue)
        {

            // Valor padrão caso o adicionar Character de error
            var tmp_id = _ue.character_id;

            _ue.character_id = 0;
            m_pi.ei.char_info = null;

            if (m_pi.mp_ce.Count() > 0)
            {
                m_pi.ei.char_info = m_pi.mp_ce.Values.First();
                _ue.character_id = m_pi.ei.char_info.id;

            }
            //else
            //{

            //    BuyItem bi = new BuyItem();
            //    stItem item = new stItem();
            //    int item_id = 0;

            //    bi.id = -1;
            //    bi._typeid = IFF::CHARACTER << 26;  // Nuri
            //    bi.qntd = 1;

            //    item_manager.initItemFromBuyItem(m_pi, item, bi, false, 0, 0, 1/*Não verifica o Level*/);

            //    if (item._typeid != 0)
            //    {

            //        // Add Item já atualiza o Character equipado
            //        if ((item_id = (int)item_manager.addItem(item, this, 2/*Padrão Item*/, 0)) == item_manager.RetAddItem.TYPE.T_ERROR)
            //            throw new exception("[player::equipDefaultCharacter][Log][WARNING] player[UID=" + std::to_string(m_pi.m_uid)
            //                    + "] nao conseguiu adicionar o Character[TYPEID=" + std::to_string(item._typeid) + "] padrao para ele. Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE::PLAYER, 2500, 2));

            //    }
            //    else
            //        throw new exception("[player::equipDefaultCharacter][Log][WARNING][Error] player[UID=" + std::to_string(m_pi.m_uid)
            //                + "] nao conseguiu inicializar o Character[TYPEID=" + std::to_string(bi._typeid) + "] padrao para ele. Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE::PLAYER, 2500, 1));
            //}
        }


        public bool checkCharacterEquipedCutin(CharacterInfo ci)
        {

            bool upt_on_db = false;


            for (var i = 0u; i < 4; ++i)
            {

                if (ci.cut_in[i] != 0)
                {

                    var pCutin = m_pi.findWarehouseItemById(ci.cut_in[i]);

                    if (pCutin == null)
                    {

                        // Zera (Desequipa)
                        ci.cut_in[i] = 0;

                        upt_on_db = true;


                    }
                    else
                    {

                        var cutin = sIff.getInstance().findSkin(pCutin._typeid);

                        if (cutin != null && !cutin.Level.GoodLevel((byte)m_pi.level))
                        {

                            // Zera (Desequipa)
                            ci.cut_in[i] = 0;

                            upt_on_db = true;
                        }
                        else if (cutin == null)
                        {

                            // Zera (Desequipa)
                            ci.cut_in[i] = 0;

                            upt_on_db = true;

                        }
                    }
                }
            }

            return upt_on_db;
        }

        public bool checkCharacterEquipedPart(CharacterInfo ci)
        {

            uint def_part = 0u;

            // Angel Part of character 3% quit rate para equipar o Normal angel wings
            var angel_wings_typeid = Global.angel_wings.FirstOrDefault(el => sIff.getInstance().getItemCharIdentify(el) == (ci._typeid & 0x000000FF));


            int angel_wings_part_num = (Global.angel_wings.Any(el => el == angel_wings_typeid) ? -1 : (int)sIff.getInstance().getItemCharPartNumber(angel_wings_typeid));

            bool upt_on_db = false;

            // Checks Parts Equiped
            for (var i = 0u; i < 24; ++i)
            {

                if (ci.parts_typeid[i] != 0)
                {

                    if (sIff.getInstance().getItemGroupIdentify(ci.parts_typeid[i]) == 2 && (sIff.getInstance().getItemCharPartNumber(ci.parts_typeid[i]) == i || (ci.parts_typeid[i] & 0x08000400/*def part*/) == 0x8000400))
                    {

                        var part = sIff.getInstance().findPart(ci.parts_typeid[i]);

                        if (part != null && part.Active)
                        {

                            if (ci.parts_id[i] == 0)
                            {

                                def_part = ((sIff.getInstance().getItemCharPartNumber(ci.parts_typeid[i]) | (uint)(ci._typeid << 5)) << 13) | 0x8000400;

                                if ((ci.parts_typeid[i] & def_part) == def_part)
                                {
                                }
                                else
                                {   
                                    // Deseequipa o Part do character e coloca os Parts Default do Character no lugar
                                    ci.unequipPart(part);

                                    upt_on_db = true;
                                }

                            }
                            else
                            {

                                var parts = m_pi.findWarehouseItemById(ci.parts_id[i]);

                                if (parts != null/* != _session.m_pi.v_wi.end()*/)
                                {

                                    var slot = part.position_mask.getSlot((int)i);

                                    if (slot == false)
                                    {

                                        // Deseequipa o Part do character e coloca os Parts Default do Character no lugar
                                        ci.unequipPart(part);

                                        upt_on_db = true;
                                    }
                                    else if (slot)
                                    {

                                        if (part.Level.GoodLevel((byte)m_pi.level))
                                        {

                                            if (angel_wings_part_num == -1l || parts._typeid != angel_wings_typeid || m_pi.ui.getQuitRate() < 3.0)
                                            {
                                            }
                                            else
                                            {

                                                // Deseequipa o Part do character e coloca os Parts Default do Character no lugar
                                                ci.unequipPart(part);

                                                upt_on_db = true;

                                            }

                                        }
                                        else
                                        {

                                            // Deseequipa o Part do character e coloca os Parts Default do Character no lugar
                                            ci.unequipPart(part);

                                            upt_on_db = true;

                                        }
                                    }

                                }
                                else
                                {

                                    // Deseequipa o Part do character e coloca os Parts Default do Character no lugar
                                    ci.unequipPart(part);

                                    upt_on_db = true;
                                }
                            }

                        }
                        else
                        {

                            // Deseequipa o Part do character e coloca os Parts Default do Character no lugar
                            if (part != null)
                                ci.unequipPart(part);
                            else
                            {

                                part = sIff.getInstance().findPart((def_part = ((i | (uint)(ci._typeid << 5)) << 13) | 0x8000400));
                                ci.parts_typeid[i] = (part != null) ? def_part : 0;
                                ci.parts_id[i] = 0;
                            }

                            upt_on_db = true;
                        }

                    }
                    else
                    {

                        var part = sIff.getInstance().findPart(ci.parts_typeid[i]);

                        // Deseequipa o Part do character e coloca os Parts Default do Character no lugar
                        if (part != null)
                            ci.unequipPart(part);
                        else
                        {

                            part = sIff.getInstance().findPart((def_part = ((i | (uint)(ci._typeid << 5)) << 13) | 0x8000400));
                            ci.parts_typeid[i] = (part != null) ? def_part : 0;
                            ci.parts_id[i] = 0;
                        }

                        upt_on_db = true;
                    }

                }
                else
                {
                }
            }

            return upt_on_db;
        }

        public bool checkCharacterEquipedAuxPart(CharacterInfo ci)
        {

            bool upt_on_db = false;

            // Check AuxPart Equiped
            for (var i = 0u; i < 5; ++i)
            {

                if (ci.auxparts[i] != 0)
                {

                    // Esse AuxPartNumber é o 0x0 anel que consome(só mão direita), 0x1 mão direita, 0x21 mão esquerda
                    if (sIff.getInstance().getItemGroupIdentify(ci.auxparts[i]) == 4)//aux
                    {

                        var aux = sIff.getInstance().findAuxPart(ci.auxparts[i]);
                        var pAux = m_pi.findWarehouseItemByTypeid(ci.auxparts[i]);

                        if (aux != null && aux.Active && pAux != null)
                        {

                            if (aux.Level.GoodLevel((byte)m_pi.level))
                            {

                                if (aux.PowerSlot/*qntd*/ == 0 || pAux.c[0] > 0)
                                {
                                }
                                else
                                {

                                    // Desequipa
                                    ci.auxparts[i] = 0;

                                    upt_on_db = true;
                                }

                            }
                            else
                            {

                                // Desequipa
                                ci.auxparts[i] = 0;

                                upt_on_db = true;
                            }

                        }
                        else
                        {

                            // Desequipa
                            ci.auxparts[i] = 0;

                            upt_on_db = true;
                        }

                    }
                    else
                    {

                        // Desequipa
                        ci.auxparts[i] = 0;

                        upt_on_db = true;
                    }

                }
                else
                {
                }
            }

            return upt_on_db;
        }

        public void checkCharacterAllItemEquiped(CharacterInfo ci)
        {
            var ret = checkCharacterEquipedPart(ci);

            ret |= checkCharacterEquipedAuxPart(ci);

            ret |= checkCharacterEquipedCutin(ci);

            // Atualiza os parts equipados do player no banco de dados, que tinha parts errados
            if (ret)
                NormalManagerDB.add(5, new CmdUpdateCharacterAllPartEquiped(m_pi.uid, ci), null, this);

        }

        internal bool checkCaddieEquiped(UserEquip ue)
        {
            throw new NotImplementedException();
        }
    }
}
