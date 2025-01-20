using GameServer.Cmd;
using PangyaAPI.Network.Pangya_St;
using PangyaAPI.Utilities;
using PangyaAPI.Utilities.Log;
using System;                     
using System.Linq;               
using _smp = PangyaAPI.Utilities.Log;
using snmdb = PangyaAPI.SQL.Manager;
namespace GameServer.PangType
{
    public class PlayerInfo : PlayerInfoBase
    {        
        public override void addCookie(ulong _cookie)
        {
            if (_cookie <= 0)
                throw new exception("[PlayerInfo::addCookie][Error] _cookie valor invalido: " + _cookie);

            try
            {
                // Check alteration on cookie of DB 
                if (checkAlterationCookieOnDB())
                    throw new exception("[PlayerInfo::addCookie][Error] Player[UID=" + uid + "] cookie on db is different of server.");

                cookie += _cookie;

                //m_update_cookie_db.requestUpdateOnDB();

                //  snmdb::NormalManagerDB.add(2, new CmdUpdateCookie(uid, _cookie, CmdUpdateCookie::INCREASE), SQLDBResponse, this);


            }
            catch (exception e)
            {

                _smp::message_pool.push(new message("[PlayerInfo::addCookie][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                throw;
            }

            _smp::message_pool.push(new message("[PlayerInfo::addCookie][Log] Player: " + uid + ", ganhou " + _cookie + " e ficou com " + cookie + " Cookie(s).", type_msg.CL_FILE_LOG_AND_CONSOLE));
        }

        public override void addCookie(uint _uid, ulong _cookie)
        {
        }

        public override int addExp(uint _exp)
        {
            if (_exp == 0)
                throw new exception("[PlayerInfo::addExp][Error] _exp is invalid(zero)", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PLAYER_INFO, 21, 0));

            int ret = -1;

            var exp = (uint)~0u;

            try
            {
                     
                if ((exp = ExpByLevel[level]) == ~0u)
                    _smp::message_pool.push(new message("[AddExp][Log] player[UID=" + uid + "] ja eh infinit legend I, nao precisar mais add exp para ele.", type_msg.CL_FILE_LOG_AND_CONSOLE));
                else
                {
                    // Att Exp do player
                    ui.exp += _exp;

                    if (ui.exp >= exp)
                    { // LEVEL UP!
                        byte new_level = 0, ant_level = 0;

                        // Atualiza todos os levels das estruturas que o player tem
                        ant_level = (byte)level;

                        // Check if up n levels
                        do
                        {
                            new_level = (byte)++level;

                            mi.level = new_level;
                            ui.level = new_level;

                            // Att Exp do player
                            ui.exp -= exp;

                            // LEVEL UP!
                            ret = new_level - ant_level;

                        } while ((exp = ExpByLevel[level]) != ~0u && ui.exp > exp);

                        _smp::message_pool.push(new message("[AddExp][Log] player[UID=" + uid + "] Upou de Level[FROM=" + ant_level + ", TO="
                                + new_level + "]", type_msg.CL_FILE_LOG_AND_CONSOLE));

                    }
                    else // Update só a Exp
                        ret = 0;

                    _smp::message_pool.push(new message("[AddExp][Log] player[UID=" + uid + "] adicionou Experiencia[value=" + _exp + "] e ficou com [LEVEL="
                            + level + ", EXP=" + ui.exp + "]", type_msg.CL_FILE_LOG_AND_CONSOLE));

                    // UPDATE ON DB, LEVEL AND EXP
                    snmdb::NormalManagerDB.add(3, new Cmd.CmdUpdateLevelAndExp(uid, level, ui.exp), SQLDBResponse, this);
                }
                 
            }
            catch (exception e) {
                                       

                _smp::message_pool.push(new message("[PlayerInfo::addExp][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                throw;
            }

            return ret;
            }

        public override void addGrandZodiacPontos(ulong _pontos)
        {
            if (_pontos < 0)
                    throw new exception("[PlayerInfo::addGrandZodiacPontos][Error] invalid _pontos(" + _pontos + "), ele eh negativo.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PLAYER_INFO, 101, 0));

            grand_zodiac_pontos += _pontos;

            // Update no Banco de dados
            snmdb::NormalManagerDB.add(8, new CmdGrandZodiacPontos(uid, (uint)grand_zodiac_pontos, CmdGrandZodiacPontos.eCMD_GRAND_ZODIAC_TYPE.CGZT_UPDATE), SQLDBResponse, this);

            // Log
            _smp::message_pool.push(new message("[PlayerInfo::addGrandZodiacPontos][Log] Player[UID=" + uid
                    + "] add " + _pontos + " pontos do grand zodiac e ficou com " + grand_zodiac_pontos, type_msg.CL_FILE_LOG_AND_CONSOLE));

        }

        public override void addMoeda(ulong _pang, ulong _cookie)
        {
            if (_pang > 0)
                addPang(_pang);

            if (_cookie > 0)
                addCookie(_cookie);
        }

        public override void addPang(ulong _pang)
        {

            if (_pang <= 0)
                throw new exception("[PlayerInfo::addPang][Error] _pang valor invalido: " + _pang);

            try
            {

                // Check alteration on pang of DB 
                if (checkAlterationPangOnDB())
                {

                    // Pang é diferente atualiza o pang com o valor do banco de daos
                    _smp::message_pool.push(new message("[PlayerInfo::addPang][Error] Player[UID=" + uid + "] pang on db is different of server.", type_msg.CL_FILE_LOG_AND_CONSOLE));

                    var old_pang = ui.pang;

                    // Atualiza o valor do pang do server com o do banco de dados
                    updatePang();

                    // Log
                    _smp::message_pool.push(new message("[PlayerInfo::addPang][Log] Player[UID=" + uid
                            + "] o Pang[DB=" + ui.pang + ", GS=" + old_pang
                            + "] no banco de dados eh diferente do que esta no server, atualiza para o valor do banco de dados.", type_msg.CL_FILE_LOG_AND_CONSOLE));
                }

                // Add o pang para o player
                ui.pang += _pang;

                //m_update_pang_db.requestUpdateOnDB();

                //snmdb::NormalManagerDB.add(1, new CmdUpdatePang(uid, _pang, CmdUpdatePang::INCREASE), PlayerInfo::SQLDBResponse, this);


            }
            catch (exception e)
            {

                _smp::message_pool.push(new message("[PlayerInfo::addPang][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                throw;
            }
            _smp::message_pool.push(new message("[PlayerInfo::addPang][Log] Player: " + uid + ", ganhou " + _pang + " e ficou com " + ui.pang + " Pang(s).", type_msg.CL_FILE_LOG_AND_CONSOLE));

        }

        public override void addPang(uint _uid, ulong _pang)
        {
            if (_pang <= 0)
                throw new exception("[PlayerInfo::addPang][Error] _pang valor invalido: " + _pang, ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PLAYER_INFO, 21, 0));

            snmdb::NormalManagerDB.add(1, new CmdUpdatePang(_uid, _pang, CmdUpdatePang.T_UPDATE_PANG.INCREASE), SQLDBResponse, null);

            _smp::message_pool.push(new message("[PlayerInfo::addPang][Log] Player: " + _uid + ", ganhou " + _pang + " Pang(s).", type_msg.CL_FILE_LOG_AND_CONSOLE));
        }


        public override void addUserInfo(UserInfoEx _ui, ulong _total_pang_win_game = 0)
        {
            ui.add(_ui, (uint)_total_pang_win_game);

            // Update User Info ON DB
            updateUserInfo();
        }

        public override bool checkAlterationCookieOnDB()
        {
            var cmd_cp = new CmdCookie(uid);    // Waiter

            snmdb::NormalManagerDB.add(0, cmd_cp, null, null);
             

            if (cmd_cp.getException().getCodeError() != 0)
                throw cmd_cp.getException();

            return (cmd_cp.getCookie() != cookie);
        }

        public override bool checkAlterationPangOnDB()
        {
           var cmd_pang = new CmdPang(uid);    // Waiter

            snmdb::NormalManagerDB.add(0, cmd_pang, null, null);
                               
            if (cmd_pang.getException().getCodeError() != 0)
                throw cmd_pang.getException();

            return (cmd_pang.getPang() != ui.pang);
        }

        public override bool checkEquipedItem(int _typeid)
        {
            return false;
        }

        public override PlayerRoomInfo.uItemBoost checkEquipedItemBoost()
        {
            return null;
        }

        public override void consomeCookie(ulong _cookie)
        {
        }

        public override void consomeMoeda(ulong _pang, ulong _cookie)
        {
        }

        public override void consomePang(ulong _pang)
        {
        }

        public override CaddieInfoEx findCaddieById(int _id)
        {
            return null;
        }

        public override CaddieInfoEx findCaddieByTypeid(int _typeid)
        {
            return null;
        }

        public override CaddieInfoEx findCaddieByTypeidAndId(int _typeid, int _id)
        {
            return null;
        }

        public override CardInfo findCardById(int _id)
        {
            return null;
        }

        public override CardInfo findCardByTypeid(int _typeid)
        {
            return null;
        }

        public override CardEquipInfoEx findCardEquipedById(int _id, int _char_typeid, int _slot)
        {
            return null;
        }

        public override CardEquipInfoEx findCardEquipedByTypeid(int _typeid, int _char_typeid = 0, int _slot = 0, int _tipo = 0, int _efeito = 0)
        {
            return null;
        }

        public override CharacterInfo findCharacterById(int _id)
        {
            return null;
        }

        public override CharacterInfo findCharacterByTypeid(int _typeid)
        {
            return null;
        }

        public override CharacterInfo findCharacterByTypeidAndId(int _typeid, int _id)
        {
            return null;
        }

        public override FriendInfo findFriendInfoById(string _id)
        {
            return null;
        }

        public override FriendInfo findFriendInfoByNickname(string _nickname)
        {
            return null;
        }

        public override FriendInfo findFriendInfoByUID(int _uid)
        {
            if (_uid == 0u)
            {

                return null;
            }

            var it = mp_fi.Where(c => c.Key == _uid);

            return it.Any() ? it.First().Value : null;
        }

        public override GrandPrixClear findGrandPrixClear(int _typeid)
        {
            return null;
        }

        public override MascotInfoEx findMascotById(int _id)
        {
            return null;
        }

        public override MascotInfoEx findMascotByTypeid(int _typeid)
        {
            return null;
        }

        public override MascotInfoEx findMascotByTypeidAndId(int _typeid, int _id)
        {
            return null;
        }

        public override MyRoomItem findMyRoomItemById(int _id)
        {
            return null;
        }

        public override MyRoomItem findMyRoomItemByTypeid(int _typeid)
        {
            return new MyRoomItem();
        }

        public override TrofelEspecialInfo findTrofelEspecialById(int _id)
        {
            return new TrofelEspecialInfo();
        }

        public override TrofelEspecialInfo findTrofelEspecialByTypeid(int _typeid)
        {
            return new TrofelEspecialInfo();
        }

        public override TrofelEspecialInfo findTrofelEspecialByTypeidAndId(int _typeid, int _id)
        {
            return new TrofelEspecialInfo();
        }

        public override TrofelEspecialInfo findTrofelGrandPrixById(int _id)
        {
            return new TrofelEspecialInfo();
        }

        public override TrofelEspecialInfo findTrofelGrandPrixByTypeid(int _typeid)
        {
            return new TrofelEspecialInfo();
        }

        public override TrofelEspecialInfo findTrofelGrandPrixByTypeidAndId(int _typeid, int _id)
        {
            return new TrofelEspecialInfo();
        }

        public override WarehouseItemEx findWarehouseItemById(int _id)
        {
            return mp_wi.GetValues((uint)_id).First();
        }

        public override WarehouseItemEx findWarehouseItemByTypeid(int _typeid)
        {
            return new WarehouseItemEx();
        }

        public override WarehouseItemEx findWarehouseItemByTypeidAndId(int _typeid, int _id)
        {
            return new WarehouseItemEx();
        }

        public override int getCharacterMaxSlot(CharacterInfo.Stats _stats)
        {
            return 1;
        }

        public override int getClubSetMaxSlot(CharacterInfo.Stats _stats)
        {
            return 1;
        }

        public override int getSizeCupGrandZodiac()
        {
            int size_cup = 1;

            if (grand_zodiac_pontos < 300)
                size_cup = 9;
            else if (grand_zodiac_pontos < 600)
                size_cup = 8;
            else if (grand_zodiac_pontos < 1200)
                size_cup = 7;
            else if (grand_zodiac_pontos < 1800)
                size_cup = 6;
            else if (grand_zodiac_pontos < 4000)
                size_cup = 5;
            else if (grand_zodiac_pontos < 5200)
                size_cup = 4;
            else if (grand_zodiac_pontos < 7600)
                size_cup = 3;
            else if (grand_zodiac_pontos < 10000)
                size_cup = 2;

            return size_cup;
        }

        public override int getSlotPower()
        {
            return 1;
        }

        public override int getSumRecordGrandPrix()
        {
            return 1;
        }

        public override bool isAuxPartEquiped(int _typeid)
        {
            return false;
        }

        public override bool isFriend(int _uid)
        {
            return false;
        }

        public override bool isMasterCourse()
        {
            return false;
        }

        public override bool isPartEquiped(int _typeid, int _id)
        {
            return false;
        }

        public override bool ownerCaddieItem(int _typeid)
        {
            return false;
        }

        public override bool ownerHairStyle(int _typeid)
        {
            return false;
        }

        public override bool ownerItem(int _typeid, int option = 0)
        {
            return false;
        }

        public override bool ownerMailBoxItem(int _typeid)
        {
            return false;
        }

        public override bool ownerSetItem(int _typeid)
        {
            return false;
        }

        public override void updateCookie()
        {
            try
            {

                var cmd_cp = new CmdCookie(uid);    // Waiter

                snmdb::NormalManagerDB.add(0, cmd_cp, null, null);

                if (cmd_cp.getException().getCodeError() != 0)
                    throw cmd_cp.getException();

                cookie = cmd_cp.getCookie();

            }
            catch (exception e)
            {

                _smp::message_pool.push(new message("[PlayerInfo::updateCookie][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                // Relanção por que essa função não tem retorno para verifica, então a exception garante que o código não vai continua
                throw;
            }
        }

        public override bool updateGrandPrixClear(int _typeid, int _position)
        {
            return false;
        }

        public override void updateLocationDB()
        {
        }

        public override void updateMedal(uMedalWin _medal_win)
        {
            if (_medal_win.ucMedal == 0u)
                throw new exception("[PlayerInfo::updateMedal][Error] Player[UID=" + uid
                        + "] tentou atualizar medalhas, mas passou nenhuma medalha para atualizar. Hacker ou Bug.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PLAYER_INFO, 600, 0));

            // Update medal info player
            ui.medal.add(_medal_win);

            // Update Info do player na database
            updateUserInfo();
        }

        public override void updateMedal(uint _uid, uMedalWin _medal_win)
        {

            if (_medal_win.ucMedal == 0u)
                throw new exception("[PlayerInfo::updateMedal][Error] Player[UID=" + uid
                        + "] tentou atualizar medalhas, mas passou nenhuma medalha para atualizar. Hacker ou Bug.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PLAYER_INFO, 600, 0));

            // Update medal info player
            ui.medal.add(_medal_win);

            // Update Info do player na database
            updateUserInfo();
        }

        public override void updateMoeda()
        {
            // Update Cookie
            updateCookie();

            // Update Pang
            updatePang();
        }

        public override void updatePang()
        {
            try
            {

                var cmd_pang = new CmdPang(uid);    // Waiter

                snmdb::NormalManagerDB.add(0, cmd_pang, null, null);

                if (cmd_pang.getException().getCodeError() != 0)
                    throw cmd_pang.getException();

                ui.pang = cmd_pang.getPang();
            }
            catch (exception e)
            {

                _smp::message_pool.push(new message("[PlayerInfo::updatePang][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                // Relanção por que essa função não tem retorno para verifica, então a exception garante que o código não vai continua
                throw;
            }
        }

        public override void updateTrofelInfo(int _trofel_typeid, bool _trofel_rank)
        {
        }

        public override void updateTrofelInfo(uint _uid, int _trofel_typeid, bool _trofel_rank)
        {
        }

        public override void updateUserInfo()
        {
            snmdb::NormalManagerDB.add(3, new CmdUpdateUserInfo(uid, ui), SQLDBResponse, this);

            _smp::message_pool.push(new message("[PlayerInfo::updateUserInfo][Log] Atualizou info do player[UID=" + uid + "]", type_msg.CL_FILE_LOG_AND_CONSOLE));
        }

        public override void updateUserInfo(uint _uid, UserInfoEx _ui)
        {
            if (_uid == 0)
                throw new exception("[PlayerInfo::updateUserInfo][Error] _uid is invalid(zero)", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PLAYER_INFO, 300, 0));

            snmdb::NormalManagerDB.add(3, new CmdUpdateUserInfo(_uid, _ui), SQLDBResponse, null);

            _smp::message_pool.push(new message("[PlayerInfo::updateUserInfo][Log] Atualizou info do player[UID=" + _uid + "]", type_msg.CL_FILE_LOG_AND_CONSOLE));

        }

        public multimap<uint/*ID*/, WarehouseItemEx> findWarehouseItemItByTypeid(uint _typeid)
        {
            multimap<uint/*ID*/, WarehouseItemEx> HasSet = new multimap<uint, WarehouseItemEx>();
            foreach (var item in mp_wi)
            {
                var result = item.Value.FirstOrDefault(c => c._typeid == _typeid);
                if (result != null)
                {
                    HasSet.Add(result.id, result);
                }
            }
            return HasSet;
        }

        public multimap<uint/*ID*/, WarehouseItemEx> findWarehouseItemItByTypeid(uint _typeid, uint _id)
        {
            multimap<uint/*ID*/, WarehouseItemEx> HasSet = new multimap<uint, WarehouseItemEx>();

            var it = mp_wi.Find(_id);
            foreach (var item in it)
            {
                if (item._typeid != _typeid)
                {
                    HasSet.Add(item.id, item);
                }
            }
            return HasSet;
        }

        public static void SQLDBResponse(int _msg_id, PangyaAPI.SQL.Pangya_DB _pangya_db, object _arg)
        {
            if (_arg == null)
            {
                _smp.message_pool.push("[PlayerInfo::SQLDBResponse][WARNING] _arg is null na msg_id = " + (_msg_id));
                return;
            }

            try
            {
                var pi = (PlayerInfo)_arg;


                //// Por Hora só sai, depois faço outro tipo de tratamento se precisar
                //if (_pangya_db.getException().getCodeError() != 0)
                //{

                //    // Trata alguns tipo aqui, que são necessários
                //    switch (_msg_id)
                //    {
                //        case 1: // Update Pang
                //            {
                //                // Error at update on DB
                //                pi.m_update_pang_db.errorUpdateOnDB();

                //                break;
                //            }
                //        case 2: // Update Cookie
                //            {
                //                // Error at update on DB
                //                pi.m_update_cookie_db.errorUpdateOnDB();

                //                break;
                //            }
                //        case 5: // Update Location Player on DB
                //            {
                //                // Error at update on DB
                //                pi.m_pl.errorUpdateOnDB();

                //                break;
                //            }
                //    }

                //    _smp::message_pool.push("[PlayerInfo::SQLDBResponse][Error] " + _pangya_db.getException().getFullMessageError());

                //    return;
                //}

                //switch (_msg_id)
                //{
                //    case 1: // UPDATE pang
                //        {

                //            // Success update on DB
                //            pi.m_update_pang_db.confirmUpdadeOnDB();

                //            // Não tem retorno então não precisa reinterpretar o pangya_db
                //            //var cmd_up = ( CmdUpdatePang)(_pangya_db);
                //            break;
                //        }
                //    case 2: // UPDATE cookie
                //        {

                //            // Success update on DB
                //            pi.m_update_cookie_db.confirmUpdadeOnDB();

                //            // Não tem retorno então não precisa reinterpretar o pangya_db
                //            //var cmd_uc = ( CmdUpdateCookie)(_pangya_db);
                //            break;
                //        }
                //    case 3: // UPDATE USER INFO
                //        {
                //            // Não tem retorno então não precisa reinterpretar o pangya_db
                //            // var cmd_uui = ( CmdUpdateUserInfo)(_pangya_db);
                //            break;
                //        }
                //    case 4: // Update Normal Trofel Info
                //        {
                //            break;
                //        }
                //    case 5: // Update Location Player on DB
                //        {
                //            // Success update on DB
                //            pi.m_pl.confirmUpdadeOnDB();

                //            break;
                //        }
                //    case 6: // Insert Grand Prix Clear
                //        {

                //            var cmd_igpc = (CmdInsertGrandPrixClear)(_pangya_db);

                //            break;
                //        }
                //    case 7: // Update Grand Prix Clear
                //        {
                //            var cmd_ugpc = (CmdUpdateGrandPrixClear)(_pangya_db);

                //            break;
                //        }
                //    case 8: // Update Grand Zodiac Pontos
                //        {
                //            var cmd_gzp = (CmdGrandZodiacPontos)(_pangya_db);

                //            break;
                //        }
                //    case 0:
                //    default:
                //        break;
                //}

            }
            catch (Exception)
            {

                throw;
            }
        }
    }

}
