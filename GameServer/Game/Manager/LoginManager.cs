using GameServer.PangSystem;
using PangyaAPI.SQL;
using PangyaAPI.Network.Pangya_St;
using PangyaAPI.Utilities;
using System.Collections.Generic;
using System.Linq;
using _smp = PangyaAPI.Utilities.Log;
using snmdb = PangyaAPI.SQL.Manager;
using packet = PangyaAPI.Network.PangyaPacket.Packet;
using packet_func = GameServer.PacketFunc.packet_func;
using static GameServer.PangType._Define;
using GameServer.Cmd;
using GameServer.Session;
using static GameServer.PangType.PlayerInfo;
using GameServer.GameServerTcp;
using GameServer.PangType;
using System;
using PangyaAPI.Utilities.Log;          
using System.Threading;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace GameServer.Game
{
    public class LoginManager
    {
        protected List<LoginTask> v_task;
        static bool m_same_id_login;
        static string m_client_version;
        public LoginManager()
        {
            v_task = new List<LoginTask>();
            loadIni();
            //_checkTaskFinish();
        }
        public void _checkTaskFinish()
        {
            for (var i = 0; i < getSize(); ++i)
            {
                if (v_task[i] != null && v_task[i].isFinished())
                {
                    var _session = v_task[i].getSession;
                    _smp.message_pool.push(new message($"[LoginManager::checkTaskFinish][Log] Player [UID: {_session.getUID()}, ID: {_session.getID()}] task finished.", type_msg.CL_FILE_LOG_AND_CONSOLE));
                    v_task[i] = null;
                }
            }
            Thread.Sleep(1000);
        }

        public LoginTask createTask(ref Player _session, KeysOfLogin _kol, PlayerInfo _pi, object _gs)
        {
            if (getSize() == 2000)
                throw new exception("[createTask][Error] Chegou ao limite task de login ao mesmo tempo");

            var task = new LoginTask(_session, _kol, _pi, _gs);
            task.exec();

            v_task.Add(task);

            return task;
        }

        public void deleteTask(LoginTask _task)
        {                             
            var it = v_task.Where(c => c.getSession.getUID() == _task.getSession.getUID());

            if (it.Any() && _task.isFinished())
            {
                v_task.Remove(_task);
            }
        }

        public static void SQLDBResponse(int _msg_id, Pangya_DB _pangya_db, object _arg)
        {
            if (_arg == null)
            {
                _smp.message_pool.push("[SQLDBResponse][Error] _arg is null na msg_id = " + (_msg_id));
                return;
            }
           // if (_arg is LoginTask && (task = (LoginTask)_arg) != null)
            
            var task = (LoginTask)(_arg);

            try
            {
                if (task.isFinished())
                    return;

                // Verifica se a session ainda é valida, essas funções já é thread-safe
                if (!task.getSession.getConnected())
                    throw new exception("[SQLDBResponse][Error] session is invalid, para tratar o pangya_db");

                // Por Hora só sai, depois faço outro tipo de tratamento se precisar
                if (_pangya_db.getException().getCodeError() != 0)
                    throw new exception(_pangya_db.getException().getFullMessageError());

                switch (_msg_id)
                {
                    case 0: // Info Player
                        {

                            break;
                        }
                    case 1: // Key Login
                        {


                            break;
                        }
                    case 2: // Member Info - User Equip
                        {

                            var pi = task.getSession.m_pi;

                            task.getSession.m_pi.ue = ((CmdUserEquip)(_pangya_db)).getInfo();
                                              
                            // Verifica se tem o Pacote de verificação de bots ativado
                            //uint ttl = Program.gs.getBotTTL();

                           var p = packet_func.pacote1A9(10000/*milliseconds*/);
                            packet_func.session_send(p, task.getSession, 1); // Tempo para enviar um pacote, ant Bot

                            snmdb.NormalManagerDB.add(5, new CmdTutorialInfo(pi.uid), SQLDBResponse, task);

                            snmdb.NormalManagerDB.add(6, new CmdCouponGacha(pi.uid), SQLDBResponse, task);

                            snmdb.NormalManagerDB.add(7, new CmdUserInfo(pi.uid), SQLDBResponse, task);

                            snmdb.NormalManagerDB.add(8, new CmdGuildInfo(pi.uid, 0), SQLDBResponse, task);

                            snmdb.NormalManagerDB.add(9, new CmdDolfiniLockerInfo(pi.uid), SQLDBResponse, task);

                            snmdb.NormalManagerDB.add(10, new CmdCookie(pi.uid), SQLDBResponse, task);

                            snmdb.NormalManagerDB.add(11, new CmdTrofelInfo(pi.uid, CmdTrofelInfo.TYPE.CURRENT), SQLDBResponse, task);

                            // Esses que estavam aqui coloquei no resposta do CmdUserEquip, por que eles precisam da resposta do User Equip

                            snmdb.NormalManagerDB.add(16, new CmdMyRoomConfig(pi.uid), SQLDBResponse, task);

                            snmdb.NormalManagerDB.add(18, new CmdCheckAchievement(task.getSession.m_pi.uid), SQLDBResponse, task);

                            snmdb.NormalManagerDB.add(20, new CmdDailyQuestInfoUser(pi.uid, CmdDailyQuestInfoUser.TYPE.GET), SQLDBResponse, task);

                            snmdb.NormalManagerDB.add(21, new CmdCardInfo(pi.uid, CmdCardInfo.TYPE.ALL), SQLDBResponse, task);

                            snmdb.NormalManagerDB.add(22, new CmdCardEquipInfo(pi.uid), SQLDBResponse, task);

                            snmdb.NormalManagerDB.add(23, new CmdTrophySpecial(pi.uid, CmdTrophySpecial.TYPE_SEASON.CURRENT, CmdTrophySpecial.TYPE.NORMAL), SQLDBResponse, task);

                            snmdb.NormalManagerDB.add(24, new CmdTrophySpecial(pi.uid, CmdTrophySpecial.TYPE_SEASON.CURRENT, CmdTrophySpecial.TYPE.GRAND_PRIX), SQLDBResponse, task);

                            break;
                        }
                    case 3: // User Equip - Desativa
                        {
                            break;
                        }
                    case 4: // Premium Ticket
                        {
                            var pi = task.getSession.m_pi;

                            pi.pt = ((CmdPremiumTicketInfo)(_pangya_db)).getInfo();

                            ///Att Capability do player
                            ///Verifica se tem premium ticket para mandar o pacote do premium user e a comet
                            //if (sPremiumSystem.isPremiumTicket(pi.pt._typeid) && pi.pt.id != 0 && pi.pt.unix_sec_date > 0)
                            //{

                            //    sPremiumSystem.updatePremiumUser(task.getSession);

                            //    _smp.message_pool.push("[SQLDBResponse][Log] Player[UID=" + (pi.uid) + "] is Premium User");
                            //}

                            break;
                        }
                    case 5: // Tutorial Info
                        {

                            task.getSession.m_pi.TutoInfo = ((CmdTutorialInfo)(_pangya_db)).getInfo();  // cmd_tti.getInfo();
                                                                                                        //pi.TutoInfo = pangya_db.getTutorialInfo(pi.uid);

                            // Manda pacote do tutorial aqui
                            var p =  packet_func.pacote11F(task.getSession.m_pi, 3/*tutorial info, 3 add do zero init*/);
                            packet_func.session_send(p, task.getSession, 1);

                            break;
                        }
                    case 6: // Coupon Gacha
                        {
                            task.getSession.m_pi.cg = ((CmdCouponGacha)(_pangya_db)).getCouponGacha();  // cmd_cg.getCouponGacha();
                                                                                                        //pi.cg = pangya_db.getCouponGacha(pi.uid);

                            // Não sei se o que é esse pacote, então não sei o que ele busca no banco de dados, mas depois descubro
                            // Deixar ele enviando aqui por enquanto
                                                            
                            var p = packet_func.pacote101();
                            packet_func.session_send(p, task.getSession,1);   // pacote novo do JP

                            break;
                        }
                    case 7: // User Info
                        {

                            var pi = task.getSession.m_pi;

                            pi.ui = ((CmdUserInfo)(_pangya_db)).getInfo();    // cmd_ui.getInfo();

                            snmdb.NormalManagerDB.add(26, new CmdMapStatistics(task.getSession.m_pi.uid, CmdMapStatistics.TYPE_SEASON.CURRENT, CmdMapStatistics.TYPE.NORMAL, CmdMapStatistics.TYPE_MODO.M_NORMAL), SQLDBResponse, task);

                            snmdb.NormalManagerDB.add(27, new CmdMapStatistics(task.getSession.m_pi.uid, CmdMapStatistics.TYPE_SEASON.CURRENT, CmdMapStatistics.TYPE.ASSIST, CmdMapStatistics.TYPE_MODO.M_NORMAL), SQLDBResponse, task);

                            snmdb.NormalManagerDB.add(28, new CmdMapStatistics(task.getSession.m_pi.uid, CmdMapStatistics.TYPE_SEASON.CURRENT, CmdMapStatistics.TYPE.NORMAL, CmdMapStatistics.TYPE_MODO.M_NATURAL), SQLDBResponse, task);

                            snmdb.NormalManagerDB.add(29, new CmdMapStatistics(task.getSession.m_pi.uid, CmdMapStatistics.TYPE_SEASON.CURRENT, CmdMapStatistics.TYPE.ASSIST, CmdMapStatistics.TYPE_MODO.M_NATURAL), SQLDBResponse, task);

                            snmdb.NormalManagerDB.add(30, new CmdMapStatistics(task.getSession.m_pi.uid, CmdMapStatistics.TYPE_SEASON.CURRENT, CmdMapStatistics.TYPE.NORMAL, CmdMapStatistics.TYPE_MODO.M_GRAND_PRIX), SQLDBResponse, task);

                            snmdb.NormalManagerDB.add(31, new CmdMapStatistics(task.getSession.m_pi.uid, CmdMapStatistics.TYPE_SEASON.CURRENT, CmdMapStatistics.TYPE.ASSIST, CmdMapStatistics.TYPE_MODO.M_GRAND_PRIX), SQLDBResponse, task);

                            snmdb.NormalManagerDB.add(36, new CmdChatMacroUser(task.getSession.m_pi.uid), SQLDBResponse, task);

                            snmdb.NormalManagerDB.add(38, new CmdFriendInfo(task.getSession.m_pi.uid), SQLDBResponse, task);

                            break;
                        }
                    case 8: // Guild Info
                        {
                            task.getSession.m_pi.gi = ((CmdGuildInfo)(_pangya_db)).getInfo();   // cmd_gi.getInfo();
                            break;
                        }
                    case 9:     // Donfini Locker Info
                        {
                            task.getSession.m_pi.df = ((CmdDolfiniLockerInfo)(_pangya_db)).getInfo();   // cmd_df.getInfo();
                            break;
                        }
                    case 10:    // Cookie
                        {
                            task.getSession.m_pi.cookie = ((CmdCookie)(_pangya_db)).getCookie();    // cmd_cookie.getCookie();

                            snmdb.NormalManagerDB.add(32, new CmdMailBoxInfo2(task.getSession.m_pi.uid), SQLDBResponse, task);

                            snmdb.NormalManagerDB.add(33, new CmdCaddieInfo(task.getSession.m_pi.uid, CmdCaddieInfo.TYPE.ONE), SQLDBResponse, task);

                            snmdb.NormalManagerDB.add(34, new CmdMsgOffInfo(task.getSession.m_pi.uid), SQLDBResponse, task);

                            snmdb.NormalManagerDB.add(35, new CmdItemBuffInfo(task.getSession.m_pi.uid), SQLDBResponse, task);

                            snmdb.NormalManagerDB.add(37, new CmdLastPlayerGameInfo(task.getSession.m_pi.uid), SQLDBResponse, task);

                            snmdb.NormalManagerDB.add(39, new CmdAttendanceRewardInfo(task.getSession.m_pi.uid), SQLDBResponse, task);

                            snmdb.NormalManagerDB.add(42, new CmdGrandPrixClear(task.getSession.m_pi.uid), SQLDBResponse, task);

                            snmdb.NormalManagerDB.add(43, new CmdGrandZodiacPontos(task.getSession.m_pi.uid, CmdGrandZodiacPontos.eCMD_GRAND_ZODIAC_TYPE.CGZT_GET), SQLDBResponse, task);

                            snmdb.NormalManagerDB.add(44, new CmdLegacyTikiShopInfo(task.getSession.m_pi.uid), SQLDBResponse, task);

                            break;
                        }
                    case 11:    // Trofel Info atual
                        {
                            task.getSession.m_pi.ti_current_season = ((CmdTrofelInfo)(_pangya_db)).getInfo();   // cmd_ti.getInfo();

                            snmdb.NormalManagerDB.add(12, new CmdCharacterInfo(task.getSession.m_pi.uid, CmdCharacterInfo.TYPE.ALL), SQLDBResponse, task);

                            snmdb.NormalManagerDB.add(13, new CmdCaddieInfo(task.getSession.m_pi.uid, CmdCaddieInfo.TYPE.ALL), SQLDBResponse, task);

                            snmdb.NormalManagerDB.add(14, new CmdMascotInfo(task.getSession.m_pi.uid, CmdMascotInfo.TYPE.ALL), SQLDBResponse, task);

                            snmdb.NormalManagerDB.add(15, new CmdWarehouseItem(task.getSession.m_pi.uid, CmdWarehouseItem.TYPE.ALL), SQLDBResponse, task);

                            break;
                        }
                    case 12:    // Character Info
                        {

                            var pi = task.getSession.m_pi;

                            pi.mp_ce = ((CmdCharacterInfo)(_pangya_db)).getAllInfo(); // cmd_ci.getAllInfo();

                            pi.ei.char_info = null;

                            // Add Structure de estado do lounge para cada character do player
                            foreach (var el in pi.mp_ce)
                            {
                                pi.mp_scl.Add(el.Value.id, new StateCharacterLounge());
                            }

                            // Att Character Equipado que não tem nenhum character o player
                            if (pi.ue.character_id == 0 || pi.mp_ce.Count() <= 0)
                                pi.ue.character_id = 0;
                            else
                            { // Character Info(CharEquip)

                                // É um Map, então depois usa o find com a Key, que é mais rápido que rodar ele em um loop
                                var it = pi.mp_ce.Where(c => c.Key == pi.ue.character_id);

                                if (it.Any())
                                    pi.ei.char_info = it.First().Value;
                            }

                            // teste Calcula a condição do player e o sexo
                            // Só faz calculo de Quita rate depois que o player
                            // estiver no level Beginner E e jogado 50 games
                            if (pi.level >= 6 && pi.ui.jogado >= 50)
                            {
                                float rate = pi.ui.getQuitRate();

                                if (rate < GOOD_PLAYER_ICON)
                                    pi.mi.state_flag.stFlagBit.azinha = 1;
                                else if (rate >= QUITER_ICON_1 && rate < QUITER_ICON_2)
                                    pi.mi.state_flag.stFlagBit.quiter_1 = 1;
                                else if (rate >= QUITER_ICON_2)
                                    pi.mi.state_flag.stFlagBit.quiter_2 = 1;
                            }

                            if (pi.ei.char_info != null && pi.ui.getQuitRate() < GOOD_PLAYER_ICON)
                                pi.mi.state_flag.stFlagBit.icon_angel = 0; // pi.ei.char_info.AngelEquiped();
                            else
                                pi.mi.state_flag.stFlagBit.icon_angel = 0;

                            pi.mi.state_flag.stFlagBit.sexo = pi.mi.sexo;

                            break;
                        }
                    case 13:    // Caddie Info
                        {

                            var pi = task.getSession.m_pi;

                            pi.mp_ci = ((CmdCaddieInfo)(_pangya_db)).getInfo();   // cmd_cadi.getInfo();

                            // Check Caddie Times
                            player_manager.checkCaddie(task.getSession);

                            // Att Caddie Equipado que não tem nenhum caddie o player
                            if (pi.ue.caddie_id == 0 || pi.mp_ci.Count() <= 0)
                                pi.ue.caddie_id = 0;
                            else
                            { // Caddie Info

                                // É um Map, então depois usa o find com a Key, qui é mais rápido que rodar ele em um loop
                                var it = pi.mp_ci.Where(c => c.Key == pi.ue.caddie_id);

                                if (it.Any())
                                    pi.ei.cad_info = it.First().Value;
                            }
                            break;
                        }
                    case 14:    // Mascot Info
                        {

                            var pi = task.getSession.m_pi;

                            pi.mp_mi = ((CmdMascotInfo)(_pangya_db)).getInfo(); // cmd_mi.getInfo();

                            // Check Mascot Times
                            player_manager.checkMascot(task.getSession);

                            // Att Mascot Equipado que não tem nenhum mascot o player
                            if (pi.ue.mascot_id == 0 || pi.mp_mi.Count() <= 0)
                                pi.ue.mascot_id = 0;
                            else
                            { // Mascot Info

                                // É um Map, então depois usa o find com a Key, qui é mais rápido que rodar ele em um loop
                                var it = pi.mp_mi.Where(c => c.Key == pi.ue.mascot_id);

                                if (it.Any())
                                    pi.ei.mascot_info = it.First().Value;
                            }
                            break;
                        }
                    case 15:    // Warehouse Item
                        {

                            var pi = task.getSession.m_pi;

                            pi.mp_wi = ((CmdWarehouseItem)(_pangya_db)).getInfo();    // cmd_wi.getInfo();

                            // Check Warehouse Item Times
                            player_manager.checkWarehouse(task.getSession);

                            // Iterator
                            SortedList<stIdentifyKey, UpdateItem> ui_ticket_report_scroll;

                            //Verifica se tem Ticket Report Scroll no update item para abrir ele e excluir. Todos que estiver, não só 1
                            //while ((ui_ticket_report_scroll = pi.findUpdateItemByTypeidAndType(TICKET_REPORT_SCROLL_TYPEID, UpdateItem.UI_TYPE.WAREHOUSE)) != pi.mp_ui.end())
                            //{

                            //    try
                            //    {

                            //        var pWi = pi.findWarehouseItemById(ui_ticket_report_scroll.Values.id);

                            //        if (pWi != null)
                            //            item_manager.openTicketReportScroll(task.getSession, pWi.id, ((pWi.c[1] * 0x800) | pWi.c[2]));

                            //    }
                            //    catch (exception e)
                            //    {

                            //        _smp.message_pool.push("[checkWarehouse][ErrorSystem] " + e.getFullMessageError());

                            //        SessionBase inválida
                            //        if (e.getCodeError() == STDA_ERROR_TYPE._ITEM_MANAGER)
                            //            throw new exception("[SQLDBResponse][Error] " + e.getFullMessageError(), STDA_ERROR_TYPE.LOGIN_MANAGER);
                            //        else
                            //            throw;  // Relança
                            //    }
                            //}


                            var it = pi.findWarehouseItemById((int)pi.ue.clubset_id);

                            // Att ClubSet Equipado que não tem nenhum clubset o player
                            if (pi.ue.clubset_id != 0 && it != null)
                            { // ClubSet Info

                                pi.ei.clubset = it;

                                // Esse C do WarehouseItem, que pega do DB, não é o ja updado inicial da taqueira é o que fica tabela enchant, 
                                // que no original fica no warehouse msm, eu só confundi quando fiz
                                // [AJEITEI JA] (tem que ajeitar na hora que coloca no DB e no DB isso)
                                pi.ei.csi.setValues(it.id, it._typeid, it.c);

                                //IFF.ClubSet* cs = sIff.findClubSet(it.second._typeid);

                                //if (cs != null)
                                //{

                                //    for (var i = 0u; i < (Marshal.SizeOf(pi.ei.csi.enchant_c) / Marshal.SizeOf(pi.ei.csi.enchant_c[0])); ++i)
                                //        pi.ei.csi.enchant_c[i] = cs.slot[i] + it.second.clubset_workshop.c[i];

                                //}
                                //else
                                //    _smp.message_pool.push("[SQLDBResponse][Erro] player[UID=" + (pi.uid) + "] tentou inicializar ClubSet[TYPEID="
                                //            + (it.second._typeid) + ", ID=" + (it.second.id) + "] equipado, mas ClubSet Not exists on IFF_STRUCT do Server. Bug"));

                            }
                            else
                            {

                                it = pi.findWarehouseItemByTypeid((int)AIR_KNIGHT_SET);

                                if (it != pi.mp_wi.end())
                                {

                                    pi.ue.clubset_id = it.id;
                                    pi.ei.clubset = it;

                                    //// Esse C do WarehouseItem, que pega do DB, não é o ja updado inicial da taqueira é o que fica tabela enchant, 
                                    //// que no original fica no warehouse msm, eu só confundi quando fiz
                                    //// [AJEITEI JA] (tem que ajeitar na hora que coloca no DB e no DB isso)
                                    pi.ei.csi.setValues(it.id, it._typeid, it.c);

                                    //IFF.ClubSet* cs = sIff.findClubSet(it.second._typeid);

                                    //if (cs != null)
                                    //{

                                    //    for (var i = 0u; i < (Marshal.SizeOf(pi.ei.csi.enchant_c) / Marshal.SizeOf(pi.ei.csi.enchant_c[0])); ++i)
                                    //        pi.ei.csi.enchant_c[i] = cs.slot[i] + it.second.clubset_workshop.c[i];

                                    //}
                                    //else
                                    //    _smp.message_pool.push("[SQLDBResponse][Erro] player[UID=" + (pi.uid) + "] tentou inicializar ClubSet[TYPEID="
                                    //        + (it.second._typeid) + ", ID=" + (it.second.id) + "] equipado, mas ClubSet Not exists on IFF_STRUCT do Server. Bug"));


                                }
                                else
                                {   // Não tem add o ClubSet padrão para ele(CV1)

                                    //_smp.message_pool.push("[SQLDBResponse][WARNING] Player[UID=" + (pi.uid)
                                    //        + "] nao tem o ClubSet[TYPEID=" + (AIR_KNIGHT_SET) + "] padrao.");

                                    //BuyItem bi;
                                    //stItem item;

                                    //bi.id = -1;
                                    //bi._typeid = AIR_KNIGHT_SET;
                                    //bi.qntd = 1;

                                    //item_manager.initItemFromBuyItem(*pi, item, bi, false, 0, 0, 1/*Não verifica o Level*/);

                                    //if (item._typeid != 0 && (item.id = item_manager.addItem(item, task.getSession, 2/*Padrão item*/, 0)) != item_manager.RetAddItem.T_ERROR
                                    //    && (it = pi.findWarehouseItemItById(item.id)) != pi.mp_wi.end())
                                    //{

                                    //    pi.ue.clubset_id = it.second.id;
                                    //    pi.ei.clubset = &it.second;

                                    //    // Esse C do WarehouseItem, que pega do DB, não é o ja updado inicial da taqueira é o que fica tabela enchant, 
                                    //    // que no original fica no warehouse msm, eu só confundi quando fiz
                                    //    // [AJEITEI JA] (tem que ajeitar na hora que coloca no DB e no DB isso)
                                    //    pi.ei.csi = { it.second.id, it.second._typeid, it.second.c };

                                    //    IFF.ClubSet* cs = sIff.findClubSet(it.second._typeid);

                                    //    if (cs != null)
                                    //    {

                                    //        for (var i = 0u; i < (Marshal.SizeOf(pi.ei.csi.enchant_c) / Marshal.SizeOf(pi.ei.csi.enchant_c[0])); ++i)
                                    //            pi.ei.csi.enchant_c[i] = cs.slot[i] + it.second.clubset_workshop.c[i];

                                    //    }
                                    //    else
                                    //        _smp.message_pool.push("[SQLDBResponse][Erro] player[UID=" + (pi.uid) + "] tentou inicializar ClubSet[TYPEID="
                                    //            + (it.second._typeid) + ", ID=" + (it.second.id) + "] equipado, mas ClubSet Not exists on IFF_STRUCT do Server. Bug"));


                                    //}
                                    //else
                                    //    throw new exception("[SQLDBResponse][Error] Player[UID=" + (pi.uid)
                                    //            + "] nao conseguiu adicionar o ClubSet[TYPEID=" + (AIR_KNIGHT_SET) + "] padrao para ele. Bug");
                                }

                            }

                            // Atualiza Comet(Ball) Equipada
                            var it_ball = pi.findWarehouseItemItByTypeid(pi.ue.ball_typeid);
                            if (pi.ue.ball_typeid != 0 && it_ball.Any())
                            {
                                pi.ei.comet = it_ball.First().Value.First();
                            }
                            else
                            { // Default Ball

                                pi.ue.ball_typeid = DEFAULT_COMET_TYPEID;

                                it = pi.findWarehouseItemByTypeid((int)DEFAULT_COMET_TYPEID);

                                if (it != pi.mp_wi.Last().Value.First())
                                {
                                    pi.ei.comet = it;
                                }
                                else
                                {   // não tem add a bola padrão para ele

                                    //_smp.message_pool.push("[SQLDBResponse][WARNING] Player[UID=" + (pi.uid)
                                    //        + "] nao tem a Comet(Ball)[TYPEID=" + (DEFAULT_COMET_TYPEID) + "] padrao.");

                                    //BuyItem bi;
                                    //stItem item;

                                    //bi.id = -1;
                                    //bi._typeid = DEFAULT_COMET_TYPEID;
                                    //bi.qntd = 1;

                                    //item_manager.initItemFromBuyItem(*pi, item, bi, false, 0, 0, 1/*Não verifica o Level*/);

                                    //if (true)
                                    //{

                                    //    pi.ei.comet = &it.second;

                                    //}
                                    //else
                                    //{
                                    //    throw new exception("[SQLDBResponse][Error] Player[UID=" + (pi.uid)
                                    //            + "] nao conseguiu adicionar a Comet(Ball)[TYPEID=" + (DEFAULT_COMET_TYPEID) + "] padrao para ele. Bug");
                                    //}

                                }
                            }

                            // Premium Ticket Tem que ser chamado depois que o Warehouse Item ja foi carregado
                            snmdb.NormalManagerDB.add(4, new Cmd.CmdPremiumTicketInfo(task.getSession.m_pi.uid), SQLDBResponse, task);

                            break;
                        }
                    case 16:    // Config MyRoom
                        {

                            task.getSession.m_pi.mrc = ((CmdMyRoomConfig)(_pangya_db)).getMyRoomConfig();   // cmd_mrc.getMyRoomConfig();

                            snmdb.NormalManagerDB.add(17, new Cmd.CmdMyRoomItem(task.getSession.m_pi.uid, CmdMyRoomItem.TYPE.ALL), SQLDBResponse, task);
                            break;
                        }
                    case 17:    // MyRoom Item Info
                        {
                            task.getSession.m_pi.v_mri = ((CmdMyRoomItem)(_pangya_db)).getMyRoomItem(); // cmd_mri.getMyRoomItem();

                            break;
                        }
                    case 18:    // Check if have Achievement
                        {
                            // --------------------- AVISO ----------------------
                            // esse aqui os outros tem que depender dele para, não ir sem ele
                            //var cmd_cAchieve = ((CmdCheckAchievement)(_pangya_db));

                            //// Cria Achievements do player
                            //if (!cmd_cAchieve.getLastState())
                            //{

                            //    // Aqui pode lançar uma excession esse block dentro do if
                            //    var pi = task.getSession.m_pi;

                            //    pi.mgr_achievement.initAchievement(pi.uid, true/*Create sem verifica se o player tem achievement, por que aqui ele já verificou);

                            //    // Add o Task + 1 por que não pede o achievement do db, porque criou ele aqui e salvo no DB
                            //}
                            //else
                            //{

                            //}

                           snmdb.NormalManagerDB.add(19, new CmdAchievementInfo(task.getSession.m_pi.uid), SQLDBResponse, task);
                            task.incremenetCount();
                            break;
                        }
                    case 19:    // Achievement Info
                        {
                            //var cmd_ai = ((CmdAchievementInfo)(_pangya_db));
                            //var pi = task.getSession.m_pi;

                            //// Inicializa o Achievement do player
                            //pi.mgr_achievement.initAchievement(pi.uid, cmd_ai.getInfo());

                            break;
                        }
                    case 20:    // Daily Quest User Info
                        {
                            task.getSession.m_pi.dqiu = ((CmdDailyQuestInfoUser)(_pangya_db)).GetInfo();    // cmd_dqiu.getInfo();
                                                                                                            //                                                              // fim daily quest info player

                            break;
                        }
                    case 21:    // Card Info
                        {
                            task.getSession.m_pi.v_card_info = ((CmdCardInfo)(_pangya_db)).getInfo();   // cmd_cardi.getInfo();

                            break;
                        }
                    case 22:    // Card Equipped Info
                        {
                            //task.getSession.m_pi.v_cei = ((CmdCardEquipInfo)(_pangya_db)).getInfo();    // cmd_cei.getInfo();

                            //// Check Card Special Times
                            //player_manager.checkCardSpecial(task.getSession);

                            break;
                        }
                    case 23:    // Trofel especial normal atual
                        {
                            task.getSession.m_pi.v_tsi_current_season = ((CmdTrophySpecial)(_pangya_db)).getInfo();

                            break;
                        }
                    case 24:    // Trofel especial grand prix atual
                        {
                            task.getSession.m_pi.v_tgp_current_season = ((CmdTrophySpecial)(_pangya_db)).getInfo(); // cmd_tei.getInfo();

                            break;
                        }
                    case 26:    // MapStatistics normal, atual
                        {
                            var v_ms = ((CmdMapStatistics)(_pangya_db)).getMapStatistics(); // cmd_ms.getMapStatistics();

                            foreach (var i in v_ms)
                            {
                                task.getSession.m_pi.a_ms_normal[i.course] = i;
                            }

                            break;
                        }
                    case 27:    // MapStatistics Normal, assist, atual
                        {
                            var v_ms = ((CmdMapStatistics)(_pangya_db)).getMapStatistics(); // cmd_ms.getMapStatistics();

                            foreach (var i in v_ms)
                            {
                                task.getSession.m_pi.a_msa_normal[i.course] = i;
                            }

                            break;
                        }
                    case 28:    // MapStatistics Natural, atual
                        {
                            var v_ms = ((CmdMapStatistics)(_pangya_db)).getMapStatistics(); // cmd_ms.getMapStatistics();

                            foreach (var i in v_ms)
                            {
                                task.getSession.m_pi.a_ms_natural[i.course] = i;
                            }

                            break;
                        }
                    case 29:    // MapStatistics Natural, assist, atual
                        {

                            var v_ms = ((CmdMapStatistics)(_pangya_db)).getMapStatistics(); // cmd_ms.getMapStatistics();

                            foreach (var i in v_ms)
                            {
                                task.getSession.m_pi.a_msa_natural[i.course] = i;
                            }

                            break;
                        }
                    case 30:    // MapStatistics GrandPrix, atual
                        {
                            var v_ms = ((CmdMapStatistics)(_pangya_db)).getMapStatistics(); // cmd_ms.getMapStatistics();


                            foreach (var i in v_ms)
                            {
                                task.getSession.m_pi.a_ms_grand_prix[i.course] = i;
                            }

                            break;
                        }
                    case 31:    // MapStatistics GrandPrix, Assist, atual
                        {
                            var v_ms = ((CmdMapStatistics)(_pangya_db)).getMapStatistics(); // cmd_ms.getMapStatistics();

                            foreach (var i in v_ms)
                            {
                                task.getSession.m_pi.a_msa_grand_prix[i.course] = i;
                            }
                            break;
                        }
                    case 32:    // [MailBox] New Email(s), Agora é a inicialização do Cache do Mail Box
                        {
                            var cmd_mbi2 = ((CmdMailBoxInfo2)(_pangya_db));

                            task.getSession.m_pi.m_mail_box.init(cmd_mbi2.getInfo(), task.getSession.m_pi.uid);

                            var v_mb = task.getSession.m_pi.m_mail_box.getAllUnreadEmail();

                            var p = packet_func.pacote210(v_mb);
                            packet_func.session_send(p, task.getSession, 1);
                            break;
                        }
                    case 33:    // Aviso Caddie Ferias
                        {
                            var v_cif = ((CmdCaddieInfo)(_pangya_db)).getInfo();    // cmd_cadi.getInfo();

                            if (v_cif.Any())
                            {

                                var p = packet_func.pacote0D4(v_cif);
                                packet_func.session_send(p, task.getSession, 1);            
                            }                                                                                                                                                           
                            break;
                        }
                    case 34:    // Msg Off Info
                        {
                            var v_moi = ((CmdMsgOffInfo)(_pangya_db)).GetInfo();    // cmd_moi.getInfo();

                            if (!v_moi.Any())
                            {

                                var p = packet_func.pacote0B2(v_moi);
                                packet_func.session_send(p, task.getSession, 1);

                            }

                            break;
                        }
                    case 35:    // YamEquipedInfo ItemBuff(item que da um efeito, por tempo)
                        {
                            task.getSession.m_pi.v_ib = ((CmdItemBuffInfo)(_pangya_db)).GetInfo();  // cmd_yei.getInfo();

                            //// Check Item Buff Times
                            player_manager.checkItemBuff(task.getSession);

                            break;
                        }
                    case 36:    // Chat Macro User
                        {
                            task.getSession.m_pi.cmu = ((CmdChatMacroUser)(_pangya_db)).getMacroUser();
                            break;
                        }
                    case 37:    // Last 5 Player Game Info
                        {
                            task.getSession.m_pi.l5pg = ((CmdLastPlayerGameInfo)(_pangya_db)).getInfo();
                            break;
                        }
                    case 38:    // Friend List
                        {
                            task.getSession.m_pi.mp_fi = ((CmdFriendInfo)(_pangya_db)).getInfo();
                            break;
                        }
                    case 39:    // Attendance Reward Info
                        {
                            task.getSession.m_pi.ari = ((CmdAttendanceRewardInfo)(_pangya_db)).getInfo();
                            break;
                        }
                    case 40:    // Register Player Logon ON DB
                        {
                            // Não usa por que é um UPDATE
                            break;
                        }
                    case 41:    // Register Logon of player on Server in DB
                        {
                            // Não usa por que é um UPDATE
                            break;
                        }
                    case 42:    // Grand Prix Clear
                        {
                            task.getSession.m_pi.v_gpc = ((CmdGrandPrixClear)(_pangya_db)).getInfo();

                            break;
                        }
                    case 43: // Grand Zodiac Pontos
                        {
                            task.getSession.m_pi.grand_zodiac_pontos = ((CmdGrandZodiacPontos)(_pangya_db)).getPontos();

                            break;
                        }
                    case 44: // Legacy Tiki Shop(PointShop)
                        {
                            task.getSession.m_pi.m_legacy_tiki_pts = ((CmdLegacyTikiShopInfo)(_pangya_db)).getInfo();

                            break;
                        }
                    default:
                        break;
                }

                // Incrementa o contador
                task.incremenetCount();

                if (task.getCount() == 39) // 44 - 5 (38 deixei o 1, 2, 3, 40 e 41 para o game server)
                {
                    task.sendCompleteData();
                    task.WorkFinish();
                }
                else if (task.getCount() > 0)
                {
                   task.sendReply((uint)_msg_id + 1);
                }
                if (task.getCount() > 39)
                    Console.WriteLine($"test {_msg_id} msg_code, {task.getCount()} m_count");
            }
            catch (Exception ex)
            {
                _smp.message_pool.push(new message(
              $"[LoginManager::SQLDBResponse][ErrorSystem] {ex.Message}\nStack Trace: {ex.StackTrace}",
              type_msg.CL_FILE_LOG_AND_CONSOLE));
                if (task.getSession != null && task.getSession.getConnected())
                    Program.gs.DisconnectSession(task.getSession);
            }
        }

        public bool canSameIDLogin() { return m_same_id_login; }
        public string getClientVersionSideServer() { return m_client_version; }

        void loadIni()
        {                                   
            IniHandle ini = new IniHandle("server.ini");

            try
            {
                m_same_id_login = ini.ReadBool("OPTION", "SAME_ID_LOGIN", false);
            }
            catch (exception ex)
            {
                _smp.message_pool.push(new message(
              $"[LoginManager::loadIni][ErrorSystem] {ex.Message}\nStack Trace: {ex.StackTrace}",
              type_msg.CL_FILE_LOG_AND_CONSOLE));
            }

            m_client_version = ini.ReadString("SERVERINFO", "CLIENTVERSION", "0.0");
        }                                                                                                 
        int getSize() { return v_task.Count; }       
    }
}
