using GameServer.PangType;
using GameServer.Session;
using PangyaAPI.Utilities;
using PangyaAPI.Utilities.BinaryModels;
using System;
using System.Collections.Generic;
using System.Linq;
using static GameServer.PangType._Define;
namespace GameServer.Game.System
{
    public class PremiumSystem
    {
        public void checkEndTimeTicket(Player _session)
        {
            if (!_session.GetState())
            {

            }

            try
            {

                if (isPremiumTicket(_session.m_pi.pt._typeid)
                    && _session.m_pi.pt.id != 0
                    && _session.m_pi.pt.unix_sec_date <= 0)
                {

                    WarehouseItemEx ticket = new WarehouseItemEx();

                    var it = _session.m_pi.findWarehouseItemByTypeid(_session.m_pi.pt._typeid);

                    if (it == _session.m_pi.mp_wi.Last().Value)
                    {

                        ////ticket = item_manager._ownerItem(_session.m_pi.uid, _session.m_pi.pt._typeid);

                        //if (ticket.id <= 0)
                        //{
                        //   // _smp.message_pool.getInstance().push(new message("[PremiumSystem::checkEndTimeTicket][Error] player[UID=" + Convert.ToString(_session.m_pi.uid) + "] nao tem o item Ticket Premium. Bug", CL_FILE_LOG_AND_CONSOLE));

                        //    return;
                        //}

                        //// Add o Ticket Premium User para o map do player, para poder excluir ele
                        //_session.m_pi.mp_wi.insert(Tuple.Create(ticket.id, ticket));

                    }
                    else
                    {
                        ticket = it;
                    }

                    stItem item = new stItem();

                    item.type = 2;
                    item.id = ticket.id;
                    item._typeid = ticket._typeid;
                    item.qntd = (uint)ticket.c[0];
                    item.c[0] = Convert.ToInt16(item.qntd * -1);

                    // UPDATE ON SERVER AND DB
                    //if (item_manager.removeItem(item, _session) <= 0)
                    //{
                    //    throw exception("[PremiumSystem::checkEndTimeTicket][Error] player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou excluir ticket premium user, mas nao conseguiu deletar ele. Bug", STDA_MAKE_ERROR(STDA_ERROR_TYPE.PREMIUM_SYSTEM,
                    //        10000, 0));
                    //}

                    // Log
                    //// _smp.message_pool.getInstance().push(new message("[PremiumSystem::checkEndTimeTicket][Log] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "].\tExcluiu ticket premium do player.", CL_ONLY_FILE_LOG));

                    var p = new PangyaBinaryWriter();

                    ////// UPDATE ON GAME
                    //packet_func.pacote26D(p,
                    //    _session,
                    //    _session.m_pi.pt.unix_end_date);
                    //packet_func.session_send(p,
                    //    _session, 0);

                    // Zera o Premium User Ticket que ele j� n�o tem mais
                    // _session.m_pi.pt.Cl();
                }

            }
            catch (exception e)
            {

                // _smp.message_pool.getInstance().push(new message("[PremiumSystem::checkEndTimeTicket][ErrorSystem] " + e.getFullMessageError(), CL_FILE_LOG_AND_CONSOLE));
            }
        }

        public void addPremiumUser(Player _session,
            WarehouseItemEx _ticket,
            uint _time)
        {
            if (!_session.GetState())
            {

            }

            try
            {

                // // Inicializa o PremiumTicket estrutura do player
                // _session.m_pi.pt.id = _ticket.id;
                // _session.m_pi.pt._typeid = _ticket._typeid;
                // _session.m_pi.pt.unix_end_date = _ticket.end_date_unix_local;
                //// _session.m_pi.pt.unix_sec_date = _ticket.end_date_unix_local - (uint)GetLocalTimeAsUnix(); // Difer�ncia em segundo, quanto tempo ainda tem para acabar o ticket premium

                // // add Comet para o player
                //// _smp.message_pool.getInstance().push(new message("[PremiumSystem::addPremiumUser][Log] Add Comet Premium e set Capability do player[UID=" + Convert.ToString(_session.m_pi.uid) + "]", CL_FILE_LOG_AND_CONSOLE));
                //// _smp.message_pool.getInstance().push(new message("[PremiumSystem::addPremiumUser][Log] Agora o Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] eh um Premium User por (" + Convert.ToString(_time) + ") Dias", CL_FILE_LOG_AND_CONSOLE));

                // // Add comet e outros itens e atualizar no SERVER, DB e GAME
                // List<stItem> add_itens = new List<stItem>();

                // // Flag Premium User
                // _session.m_pi.m_cap.stBit.premium_user = 1u;

                // // Add Ball
                // var new_ball = addPremiumBall(_session);

                // if (new_ball._typeid == 0u)
                // {
                //     throw exception("[PremiumSystem::addPremiumUser][Error] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] nao conseguiu adicionar a Premium Ball.", STDA_MAKE_ERROR(STDA_ERROR_TYPE.PREMIUM_SYSTEM,
                //         300, 0));
                // }

                // // Add Ball para o jogo
                // add_itens.Add(new_ball);

                // if (isPremium2(_session.m_pi.pt._typeid))
                // {

                //     // Add ClubSet
                //     var new_clubset = addPremiumClubSet(_session, _time);

                //     if (new_clubset._typeid == 0u)
                //     {
                //         throw exception("[PremiumSystem::addPremiumUser][Error] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] nao conseguiu adicionar o Premium ClubSet.", STDA_MAKE_ERROR(STDA_ERROR_TYPE.PREMIUM_SYSTEM,
                //             300, 0));
                //     }

                //     // o ClubSet atualiza com o pacote073

                //     // Add Mascot
                //     var new_mascot = addPremiumMascot(_session, _time);

                //     if (new_mascot._typeid == 0u)
                //     {
                //         throw exception("[PremiumSystem::addPremiumUser][Error] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] nao conseguiu adicionar o Premium Mascot.", STDA_MAKE_ERROR(STDA_ERROR_TYPE.PREMIUM_SYSTEM,
                //             300, 0));
                //     }

                //     // Add Mascot para o jogo
                //     add_itens.Add(new_mascot);

                //     packet pp = new packet();

                //     if (packet_func.pacote073(pp,
                //         _session, _session.m_pi.mp_wi))
                //     {
                //         packet_func.session_send(pp,
                //             _session, 1);
                //     }
                // }

                // // Atualiza Capability do player
                // packet p = new packet((ushort)0x9A);

                // p.addUint32(_session.m_pi.m_cap.ulCapability);

                // packet_func.session_send(p,
                //     _session, 1);

                // if (add_itens.Count != 0)
                // {

                //     p.init_plain((ushort)0x216);

                //     p.addUint32((uint)GetSystemTimeAsUnix());
                //     p.addUint32((uint)add_itens.Count); // Count

                //     foreach (var el in add_itens)
                //     {

                //         p.addUint8(el.type);
                //         p.addUint32(el._typeid);
                //         p.addUint32(el.id);
                //         p.addUint32(el.flag_time);
                //         p.addBuffer(el.stat, sizeof(stItem.item_stat));
                //         p.addUint32((el.flag_time == 0) ? el.c[0] : el.c[3]);
                //         p.addZeroByte(25);
                //     }

                //     packet_func.session_send(p,
                //         _session, 1);
                // }

            }
            catch (exception e)
            {

                // _smp.message_pool.getInstance().push(new message("[PremiumSystem::addPremiumUser][ErrorSystem] " + e.getFullMessageError(), CL_FILE_LOG_AND_CONSOLE));
            }
        }

        public void removePremiumUser(Player _session)
        {
            if (!_session.GetState())
            {
                //throw exception("[PremiumSystem" + "removePremiumUser" + "][Error] player nao esta connectado.", STDA_MAKE_ERROR(STDA_ERROR_TYPE.PREMIUM_SYSTEM,
                //    1, 0));
            }

            try
            {

                //packet p = new packet();

                //// Remove Premium Ball
                //removePremiumBall(_session);

                //// Tira capacidade de premium user do player
                //_session.m_pi.m_cap.stBit.premium_user = 0u;

                //packet_func.pacote09A(p,
                //    _session, _session.m_pi);
                //packet_func.session_send(p,
                //    _session, 1);

                //// UPDATE ON GAME - Mostra a mensagem que acabou o tempo do ticket premium
                //packet_func.pacote26D(p,
                //    _session,
                //    _session.m_pi.pt.unix_end_date);
                //packet_func.session_send(p,
                //    _session, 0);

                // Zera o Premium User Ticket que ele j� n�o tem mais
                // _session.m_pi.pt.clear();

                // Log
                // _smp.message_pool.getInstance().push(new message("[PremiumSystem::removePremiumUser][Log] player[UID=" + Convert.ToString(_session.m_pi.uid) + "] removeu o Premium User do Player, acabou o tempo do ticket, tirando a capacidade e a Comet(Ball)", CL_FILE_LOG_AND_CONSOLE));

            }
            catch (exception e)
            {

                // _smp.message_pool.getInstance().push(new message("[PremiumSystem::removePremiumUser][ErrorSystem] " + e.getFullMessageError(), CL_FILE_LOG_AND_CONSOLE));
            }
        }

        public stItem addPremiumBall(Player _session)
        {
            if (!_session.GetState())
            {
                //throw exception("[PremiumSystem" + "addPremiumBall" + "][Error] player nao esta connectado.", STDA_MAKE_ERROR(STDA_ERROR_TYPE.PREMIUM_SYSTEM,
                //    1, 0));
            }

            stItem item = new stItem() { id = 0u };

            //try
            //{

            //    uint ball = getPremiumBallByTicket(_session.m_pi.pt._typeid);

            //    // Add Ball
            //    WarehouseItemEx new_wi = new WarehouseItemEx();
            //    new_wi.id = -1;
            //    new_wi.ano = -1;
            //    new_wi._typeid = ball; // Premium Ball
            //    new_wi.c[0] = 1;
            //    new_wi.type = 0x6A; // Item time Premium
            //    new_wi.clubset_workshop.level = -1;

            //    var it = _session.m_pi.mp_wi.insert(Tuple.Create(new_wi.id, new_wi));

            //    // Coloca a premium ball nos itens equipados
            //    _session.m_pi.ue.ball_typeid = ball;

            //    // Warehouse Item on Map Player
            //    _session.m_pi.ei.comet = &it.second;

            //    // Initialize Item
            //    item.type = 2;
            //    // C++ TO C# CONVERTER TASK: The following line was determined to be a copy assignment (rather than a reference assignment) - this should be verified and a 'CopyFrom' method should be created:
            //    // ORIGINAL LINE: item.id = new_wi.id;
            //    item.id.CopyFrom(new_wi.id);
            //    item._typeid = new_wi._typeid;
            //    item.flag_time = new_wi.type;
            //    item.stat.qntd_ant = 0u;
            //    item.stat.qntd_dep = 1u;
            //    item.qntd = 1u;
            //    item.c[0] = (short)item.qntd;

            //}
            //catch (exception e)
            //{

            //   // _smp.message_pool.getInstance().push(new message("[PremiumSystem::addPremiumBall][ErrorSystem] " + e.getFullMessageError(), CL_FILE_LOG_AND_CONSOLE));
            //}
            return item;
        }

        public stItem addPremiumClubSet(Player _session, uint _time)
        {
            //if (!_session.GetState())
            //{
            //    throw exception("[PremiumSystem" + "addPremiumClubSet" + "][Error] player nao esta connectado.", STDA_MAKE_ERROR(STDA_ERROR_TYPE.PREMIUM_SYSTEM,
            //        1, 0));
            //}

            stItem item = new stItem() { id = 0u };

            //try
            //{

            //    uint clubset = getPremiumClubSetByTicket(_session.m_pi.pt._typeid);

            //    // Add ClubSet
            //    // Aqui add com o item_manager::addItem
            //    BuyItem bi = new BuyItem() { id = 0u };

            //    bi.id = -1;
            //    // C++ TO C# CONVERTER TASK: The following line was determined to be a copy assignment (rather than a reference assignment) - this should be verified and a 'CopyFrom' method should be created:
            //    // ORIGINAL LINE: bi._typeid = clubset;
            //    bi._typeid.CopyFrom(clubset);
            //    bi.qntd = 1;
            //    bi.time = (ushort)_time;

            //    item_manager.initItemFromBuyItem(_session.m_pi,
            //        item, bi, false, 0, 0, 1);

            //    if (item._typeid == 0u)
            //    {
            //        throw exception("[PremiumSystem::addPremiumClubSet][Error] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] nao conseguiu inicializar o item[TYPEID=" + Convert.ToString(bi._typeid) + "]", STDA_MAKE_ERROR(STDA_ERROR_TYPE.PREMIUM_SYSTEM,
            //            400, 0));
            //    }

            //    if (item_manager.addItem(item,
            //        _session, 0, 0) < 0)
            //    {
            //        throw exception("[PremiumSystem::addPremiumClubSet][Error] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] nao conseguiu adicionar o item[TYPEID=" + Convert.ToString(item._typeid) + "]", STDA_MAKE_ERROR(STDA_ERROR_TYPE.PREMIUM_SYSTEM,
            //            401, 0));
            //    }

            //    var new_wi = _session.m_pi.findWarehouseItemById(item.id);

            //    new_wi.c[3] = 0;

            //}
            //catch (exception e)
            //{

            //   // _smp.message_pool.getInstance().push(new message("[PremiumSystem::addPremiumClubSet][ErrorSystem] " + e.getFullMessageError(), CL_FILE_LOG_AND_CONSOLE));
            //}

            //// C++ TO C# CONVERTER TASK: The following line was determined to contain a copy constructor call - this should be verified and a copy constructor should be created:
            //// ORIGINAL LINE:
            return item;

        }

        public stItem addPremiumMascot(Player _session, uint _time)
        {

            stItem item = new stItem() { id = 0u };

            //try
            //{

            //    uint mascot = getPremiumMascotByTicket(_session.m_pi.pt._typeid);

            //    // Add Mascot
            //    // Aqui add com o item_manager::addItem
            //    BuyItem bi = new BuyItem() { id = 0u };

            //    bi.id = -1;
            //    // C++ TO C# CONVERTER TASK: The following line was determined to be a copy assignment (rather than a reference assignment) - this should be verified and a 'CopyFrom' method should be created:
            //    // ORIGINAL LINE: bi._typeid = mascot;
            //    bi._typeid.CopyFrom(mascot);
            //    bi.qntd = 1;
            //    bi.time = (ushort)_time;

            //    item_manager.initItemFromBuyItem(_session.m_pi,
            //        item, bi, false, 0, 0, 1);

            //    if (item._typeid == 0u)
            //    {
            //        throw exception("[PremiumSystem::addPremiumMascot][Error] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] nao conseguiu inicializar o item[TYPEID=" + Convert.ToString(bi._typeid) + "]", STDA_MAKE_ERROR(STDA_ERROR_TYPE.PREMIUM_SYSTEM,
            //            400, 0));
            //    }

            //    if (item_manager.addItem(item,
            //        _session, 0, 0) < 0)
            //    {
            //        throw exception("[PremiumSystem::addPremiumMascot][Error] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] nao conseguiu adicionar o item[TYPEID=" + Convert.ToString(item._typeid) + "]", STDA_MAKE_ERROR(STDA_ERROR_TYPE.PREMIUM_SYSTEM,
            //            401, 0));
            //    }

            //    var new_wi = _session.m_pi.findMascotById(item.id);

            //}
            //catch (exception e)
            //{

            //   // _smp.message_pool.getInstance().push(new message("[PremiumSystem::addPremiumMascot][ErrorSystem] " + e.getFullMessageError(), CL_FILE_LOG_AND_CONSOLE));
            //}

            // C++ TO C# CONVERTER TASK: The following line was determined to contain a copy constructor call - this should be verified and a copy constructor should be created:
            return item;

        }

        public void removePremiumBall(Player _session)
        {
            //if (!_session.GetState())
            //{
            //    throw exception("[PremiumSystem" + "removePremiumBall" + "][Error] player nao esta connectado.", STDA_MAKE_ERROR(STDA_ERROR_TYPE.PREMIUM_SYSTEM,
            //        1, 0));
            //}

            //try
            //{

            //    uint ball = getPremiumBallByTicket(_session.m_pi.pt._typeid);

            //    // Tira primeiro a Ball
            //    var pWi = _session.m_pi.findWarehouseItemItByTypeid(ball);

            //    // Delete Premium Ball
            //    if (pWi != _session.m_pi.mp_wi.Last())
            //    {

            //        stItem item = new stItem();

            //        item.type = 2;
            //        item.id = pWi.second.id;
            //        item._typeid = pWi.second._typeid;
            //        item.qntd = 1;
            //        item.c[0] = (short)item.qntd * -1;
            //        item.stat.qntd_ant = 1;
            //        item.stat.qntd_dep = 0;
            //        item.flag_time = 0x6A; // PREMIUM ITEM, /*0x6A expired time*/

            //        // Remove do Server, que esse item n�o tem no DB,
            //        // � s� do server um item que ganha quando � premium user quando loga
            //        // !@ Aqui pode d� erro por que ent� rodando no loob o map mp_wi, e aqui est� excluindo um iterator do map
            //        _session.m_pi.mp_wi.erase(pWi);

            //        packet p = new packet((ushort)0x216);

            //        p.addUint32((uint)GetSystemTimeAsUnix());
            //        p.addUint32(1); // Count

            //        p.addUint8(item.type);
            //        p.addUint32(item._typeid);
            //        p.addInt32(item.id);
            //        p.addUint32(item.flag_time);
            //        p.addBuffer(item.stat, sizeof(stdA.stItem.item_stat));
            //        p.addUint32((item.c[3] > 0) ? item.c[3] : item.c[0]);
            //        p.addZeroByte(25);

            //        packet_func.session_send(p,
            //            _session, 1);
            //    }

            //}
            //catch (exception e)
            //{

            //   // _smp.message_pool.getInstance().push(new message("[PremiumSystem::removePremiumBall][ErrorSystem] " + e.getFullMessageError(), CL_FILE_LOG_AND_CONSOLE));
            //}
        }

        public void updatePremiumUser(Player _session)
        {

            //try
            //{

            //    List<stItem> add_itens = new List<stItem>();

            // Flag Premium User
            _session.m_pi.m_cap.stBit.premium_user = 1u;

            //    // Add Ball
            //    var new_ball = addPremiumBall(_session);

            //    if (new_ball._typeid == 0u)
            //    {
            //        throw exception("[PremiumSystem::updatePremiumUser][Error] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] nao conseguiu adicionar a Premium Ball.", STDA_MAKE_ERROR(STDA_ERROR_TYPE.PREMIUM_SYSTEM,
            //            300, 0));
            //    }

            //    add_itens.Add(new_ball);

            // Atualiza Capability do player
            var p = new PangyaBinaryWriter((ushort)0x9A);

            p.WriteUInt32(_session.m_pi.m_cap.ulCapability);

            _session.Send(p);

            //    if (add_itens.Count > 0)
            //    {

            //        p.init_plain((ushort)0x216);

            //        p.addUint32((uint)GetSystemTimeAsUnix());
            //        p.addUint32((uint)add_itens.Count); // Count

            //        foreach (var el in add_itens)
            //        {

            //            p.addUint8(el.type);
            //            p.addUint32(el._typeid);
            //            p.addUint32(el.id);
            //            p.addUint32(el.flag_time);
            //            p.addBuffer(el.stat, sizeof(stItem.item_stat));
            //            p.addUint32((el.flag_time == 0) ? el.c[0] : el.c[3]);
            //            p.addZeroByte(25);
            //        }

            //        packet_func.session_send(p,
            //            _session, 1);
            //    }

            //}
            //catch (exception e)
            //{

            //   // _smp.message_pool.getInstance().push(new message("[PremiumSystem::updatePremiumUser][ErrorSystem] " + e.getFullMessageError(), CL_FILE_LOG_AND_CONSOLE));
            //}
        }

        public uint getPremiumBallByTicket(uint _typeid)
        {

            if (_typeid == PREMIUM_TICKET_TYPEID)
            {
                return PREMIUM_BALL_TYPEID;
            }

            if (_typeid == PREMIUM_2_TICKET_TYPEID)
            {
                return PREMIUM_2_BALL_TYPEID;
            }

            return 0u;
        }

        public uint getPremiumClubSetByTicket(uint _typeid)
        {

            if (_typeid == PREMIUM_2_TICKET_TYPEID)
            {
                return PREMIUM_2_CLUBSET_TYPEID;
            }

            return 0u;
        }

        public uint getPremiumMascotByTicket(uint _typeid)
        {

            if (_typeid == PREMIUM_2_TICKET_TYPEID)
            {
                return PREMIUM_2_MASCOT_TYPEID;
            }

            return 0u;
        }

        public uint getExpPangRateByTicket(uint _typeid)
        {

            if (_typeid == PREMIUM_TICKET_TYPEID)
            {
                return 10u;
            }

            if (_typeid == PREMIUM_2_TICKET_TYPEID)
            {
                return 15u;
            }

            return 0u;
        }

        public bool isPremiumTicket(uint _typeid)
        {
            return _typeid == PREMIUM_TICKET_TYPEID || _typeid == PREMIUM_2_TICKET_TYPEID;
        }

        public bool isPremiumBall(uint _typeid)
        {
            return _typeid == PREMIUM_BALL_TYPEID || _typeid == PREMIUM_2_BALL_TYPEID;
        }

        public bool isPremium1(uint _typeid)
        {
            return _typeid == PREMIUM_TICKET_TYPEID;
        }

        public bool isPremium2(uint _typeid)
        {
            return _typeid == PREMIUM_2_TICKET_TYPEID;
        }
    }
    public class sPremiumSystem : Singleton<PremiumSystem>
    {
    }
}